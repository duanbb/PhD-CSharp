using Interoperability.Cascaded;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Interoperability.Entity
{
    abstract class Worker : IEquatable<Worker>
    {
        public string ID;

        public Worker(string id)
        {
            this.ID = id;
        }

        public override string ToString()
        {
            return ID;
        }

        public bool Equals(Worker other)
        {
            return this.ID == other.ID;
        }
    }

    sealed class TargetWorker : Worker, IEquatable<TargetWorker>
    {
        public IDictionary<Sentence, TargetAnnotation> SentenceTargetAnnotationDic;

        //只在TrainConstant.TargetWorkerList里的Sentence才会用到
        //因为不像sentence那样每组的TargetWorker&TargetAnnotation数量固定
        //所以此属性各数组中的Dictionary容量不一定
        //另：此属性只有在Cascaded-DS中才用到，Cascaded-MLE用不到
        public IDictionary<Sentence, TargetAnnotation>[] SentenceTargetAnnotationDicGroup;

        public TargetWorker(string id)
            : base(id)
        {
            SentenceTargetAnnotationDic = new Dictionary<Sentence, TargetAnnotation>();
        }

        public bool Equals(TargetWorker other)
        {
            return this.ID == other.ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        #region For Probability
        /// <summary>
        /// 只针对Target Label。
        /// 暂时没用。
        /// </summary>
        /// <param name="j">真实的Target Labeltruth。</param>
        /// <param name="l">人标的Target Labeltruth。</param>
        /// <returns></returns>
        public double WorkerBias(TargetLabeltruth j, TargetLabeltruth l)
        {
            double numerator = 0;
            double denominator = 0;
            bool[] trueAndFalse = { true, false };
            foreach (Label sourceLabel in Constant.SourceTaxonomy.LabelArray)
            {
                foreach (bool trueOrFalse in trueAndFalse)
                {
                    SourceLabeltruth sourceLabeltruth = new SourceLabeltruth(sourceLabel, trueOrFalse);
                    //int num, den;//持怀疑态度
                    //sourceLabeltruth.Times(this, l, out num, out den);
                    //numerator += ProbabilityConstant.Pr_t_s[sourceLabeltruth][j] * num;
                    //denominator += den;
                }
            }
            return numerator / denominator;
        }
        #endregion

        #region For Space
        public double TemporaryNonormalizeWeight;
        public double TemporaryNormalizeWeight;
        public double OtherNonormalizeWeight;
        public double OtherNormalizeWeight;

        public List<double[][]> ExpertiseMatrixList;

        public IDictionary<Sentence, double> SentenceWeightDic;////观察用，已放到Sentence里。
        #endregion
    }

    sealed class SourceWorker : Worker, IEquatable<SourceWorker>
    {
        public IDictionary<Sentence, SourceAnnotation> SentenceSourceAnnotationDic;

        public SourceWorker(string id)
            : base(id)
        {
            SentenceSourceAnnotationDic = new Dictionary<Sentence, SourceAnnotation>();
        }

        public bool Equals(SourceWorker other)
        {
            return this.ID == other.ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }

    static class TargetWorkerProperty
    {
        /// <summary>
        /// 为每个worker生成ExpertiseMatrix序列。
        /// </summary>
        /// <param name="maxTime">便利次数。（每次遍历为每个worker生成一个ExpertMatrix）</param>
        static public void ExpertiseMatrix(int maxTime)
        {
            string target = "Output/" + Constant.Gold + "/" + TrainConstant.Corpus + "_" + Constant.SourceTaxonomy.Name + "/ExpertiseCombination/ExpertiseMatrix";
            foreach (string f in Directory.GetDirectories(target + "/."))//必须放在初始化静态类Variable之前
            {
                Directory.Delete(f, true);
            }

            IDictionary<TargetWorker, double[]> targetWorkerCoefficientsDic = new Dictionary<TargetWorker, double[]>();

            //计算每个worker的coefficient（因为在迭代中不变，所以单独提出计算一次即可）
            foreach (TargetWorker targetWorker in TrainConstant.TargetWorkerList)
            {
                //记录worker对其标注的所有句的标注
                double[] coefficients = new double[targetWorker.SentenceTargetAnnotationDic.Count * Constant.TargetTaxonomy.LabelArray.Length];
                int index = 0;
                foreach (Sentence sentence in targetWorker.SentenceTargetAnnotationDic.Keys)
                {
                    double[] targetArray = targetWorker.SentenceTargetAnnotationDic[sentence].ToDoubleArray;
                    for (int i = 0; i < Constant.TargetTaxonomy.LabelArray.Length; ++i)
                    {
                        coefficients[targetWorker.SentenceTargetAnnotationDic.Count * i + index] = targetArray[i];
                    }
                    ++index;
                }
                targetWorkerCoefficientsDic.Add(targetWorker, coefficients);

                targetWorker.ExpertiseMatrixList = new List<double[][]>();
            }

            for (int i = 0; i < maxTime; ++i)
            {
                int indexOfTargetWorker = 0;//输出文件时使用。
                foreach (TargetWorker targetWorker in TrainConstant.TargetWorkerList)
                {
                    targetWorker.ExpertiseMatrixList.Add(new double[Constant.TargetTaxonomy.LabelArray.Length][]);

                    int indexOfTargetLabel = 0; //记录计算到哪行了
                    foreach (Label targetLabel in Constant.TargetTaxonomy.LabelArray)
                    {
                        double[] constants = new double[targetWorker.SentenceTargetAnnotationDic.Count];
                        double sumOfConstants = 0;//observe,在逐渐趋于0
                        int index = 0;
                        foreach (Sentence sentence in targetWorker.SentenceTargetAnnotationDic.Keys)//只考虑这个人标注过的句子
                        {
                            double targetValue = 0;
                            foreach (KeyValuePair<TargetWorker, TargetAnnotation> targetWorkerAndTargetAnnotation in sentence.TargetWorkerTargetAnnotationDic)//此sentence被标过的TargetAnnotation
                            {
                                if (!targetWorkerAndTargetAnnotation.Key.Equals(targetWorker))//排除此人的标注
                                {
                                    if (i == 0)
                                        targetValue += targetWorkerAndTargetAnnotation.Value.LabelAndTruthDic[targetLabel] ? 1 : -1;
                                    else
                                    {
                                        targetValue += GeneralFunction.ProductOfTwoVectors(targetWorkerAndTargetAnnotation.Key.ExpertiseMatrixList.ElementAt(i - 1)[indexOfTargetLabel], targetWorkerAndTargetAnnotation.Value.ToDoubleArray);
                                    }
                                }
                            }
                            //constants[index] = targetValue / (sentence.TargetWorkerTargetAnnotationDic.Count - 1) >= 0 ? 1 : -1;
                            constants[index] = targetValue / (sentence.TargetWorkerTargetAnnotationDic.Count - 1);
                            sumOfConstants += Math.Abs(constants[index]);
                            ++index;
                        }
                        targetWorker.ExpertiseMatrixList.ElementAt(i)[indexOfTargetLabel] = Space.SpaceFunction.OrdinaryLeastSquares(targetWorker.SentenceTargetAnnotationDic.Count, Constant.TargetTaxonomy.LabelArray.Length, targetWorkerCoefficientsDic[targetWorker], constants);
                        ++indexOfTargetLabel;
                    }
                    outputExpertiseMatrixToFile(targetWorker, indexOfTargetWorker, i);
                    ++indexOfTargetWorker;
                }
            }
        }

        static private void outputExpertiseMatrixToFile(TargetWorker targetWorker, int indexOfTargetWorker, int i)
        {
            string path = "Output/" + Constant.Gold + "/" + TrainConstant.Corpus + "_" + Constant.SourceTaxonomy.Name + "/ExpertiseCombination/ExpertiseMatrix/time" + i;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            StreamWriter file = new StreamWriter(path + "/time" + i + "_" + Constant.TargetTaxonomy.Name + "_" + indexOfTargetWorker + "_" + targetWorker.ID + ".csv", false, Encoding.Default);
            file.Write(",");
            foreach (Label label in Constant.TargetTaxonomy.LabelArray)
            {
                file.Write(label + ",");
            }
            file.WriteLine();

            int indexOfRow = 0;
            foreach (double[] row in targetWorker.ExpertiseMatrixList.ElementAt(i))
            {
                file.Write(Constant.TargetTaxonomy.LabelArray[indexOfRow] + ",");
                foreach (double cell in row)
                {
                    file.Write(cell + ",");
                }
                file.WriteLine();
                ++indexOfRow;
            }
            file.Close();
        }

        static public void TemporaryNonormalizeWeight()
        {
            foreach (TargetWorker targetWorker in TrainConstant.TargetWorkerList)
            {
                double result = 0;
                foreach (Sentence sentence in targetWorker.SentenceTargetAnnotationDic.Keys)
                {
                    result += sentence.TemporaryNonormalizeWeightDic[targetWorker];
                }
                targetWorker.TemporaryNonormalizeWeight = result / targetWorker.SentenceTargetAnnotationDic.Count();
            }
        }

        static public void TemporaryNormalizeWeight()
        {
            double sum = 0;
            foreach (TargetWorker worker in TrainConstant.TargetWorkerList)
            {
                sum += worker.TemporaryNonormalizeWeight;
            }
            foreach (TargetWorker targetWorker in TrainConstant.TargetWorkerList)
            {
                targetWorker.TemporaryNormalizeWeight = targetWorker.TemporaryNonormalizeWeight / sum;
            }
        }

        static public void OtherNonormalizeWeight()
        {
            foreach (TargetWorker worker in TrainConstant.TargetWorkerList)
            {
                foreach (TargetWorker targetWorker in TrainConstant.TargetWorkerList)
                {
                    double result = 0;
                    foreach (Sentence sentence in targetWorker.SentenceTargetAnnotationDic.Keys)
                    {
                        result += sentence.OtherNonormalizeWeightDic[targetWorker];
                    }
                    targetWorker.OtherNonormalizeWeight = result / targetWorker.SentenceTargetAnnotationDic.Count();
                }
            }
        }

        static public void OtherGeneralWeight()
        {
            double sum = 0;
            foreach (TargetWorker worker in TrainConstant.TargetWorkerList)
            {
                sum += worker.OtherNonormalizeWeight;
            }
            foreach (TargetWorker targetWorker in TrainConstant.TargetWorkerList)
            {
                targetWorker.OtherNormalizeWeight = targetWorker.OtherNonormalizeWeight / sum;
            }
        }

        static public void SentenceWeightDic()//观察用
        {
            foreach (TargetWorker targetWorker in TrainConstant.TargetWorkerList)
            {
                IDictionary<Sentence, double> result = new Dictionary<Sentence, double>();

                foreach (Sentence sentence in targetWorker.SentenceTargetAnnotationDic.Keys)//此worker标过的sentence
                {
                    double weight = 0;
                    IDictionary<Label, double> temporaryVector = new Dictionary<Label, double>();//临时标注
                    foreach (Label targetLabel in Constant.TargetTaxonomy.LabelArray)
                    {
                        temporaryVector.Add(targetLabel, 0);
                    }

                    //计算临时标注
                    foreach (KeyValuePair<TargetWorker, TargetAnnotation> targetWorkerAndTargetAnnotation in sentence.TargetWorkerTargetAnnotationDic)//此sentence被标过的TargetAnnotation
                    {
                        if (!targetWorker.Equals(targetWorkerAndTargetAnnotation.Key))//排除此人的标注
                        {
                            foreach (Label targetLabel in Constant.TargetTaxonomy.LabelArray)
                            {
                                temporaryVector[targetLabel] += targetWorkerAndTargetAnnotation.Value.LabelAndTruthDic[targetLabel] ? 1 : -1;
                            }
                        }
                    }
                    foreach (Label targetLabel in Constant.TargetTaxonomy.LabelArray)
                    {
                        temporaryVector[targetLabel] /= (sentence.TargetWorkerTargetAnnotationDic.Count() - 1);
                    }

                    //计算此worker的标注和临时标注的差别
                    foreach (Label targetLabel in Constant.TargetTaxonomy.LabelArray)
                    {
                        weight += 2 - Math.Abs(temporaryVector[targetLabel] - (targetWorker.SentenceTargetAnnotationDic[sentence].LabelAndTruthDic[targetLabel] ? 1 : -1));
                    }

                    result.Add(sentence, weight / (2 * Constant.TargetTaxonomy.LabelArray.Length));
                }
                targetWorker.SentenceWeightDic = GeneralFunction.SortDictionary(result);
            }
        }
    }
}