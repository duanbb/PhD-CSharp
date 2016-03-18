using Interoperability.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Interoperability
{
    static class Test
    {
        static public void TestExpertise(int MaxTime)
        {
            //遍历每次的ExpertiseMatrix
            for (int i = 0; i < MaxTime; ++i)
            {
                IDictionary<Sentence, IDictionary<Label, double>> SentenceAndRealTargetAnnotationDic = new Dictionary<Sentence, IDictionary<Label, double>>();
                IDictionary<Sentence, IDictionary<Label, bool>> SentenceAndBinaryTargetAnnotationDic = new Dictionary<Sentence, IDictionary<Label, bool>>();
                foreach (Sentence sentence in TrainConstant.SentenceList)//只考虑train里的句子，因为test句子的worker没有expertise matrix
                {
                    SentenceAndRealTargetAnnotationDic.Add(sentence, new Dictionary<Label, double>());
                    SentenceAndBinaryTargetAnnotationDic.Add(sentence, new Dictionary<Label, bool>());
                    int LabelIndex = 0;
                    foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                    {
                        double value = 0;
                        foreach (KeyValuePair<TargetWorker, TargetAnnotation> WorkerAnnotation in sentence.TargetWorkerTargetAnnotationDic)
                        {
                            value += GeneralFunction.ProductOfTwoVectors(WorkerAnnotation.Key.ExpertiseMatrixList.ElementAt(i)[LabelIndex], WorkerAnnotation.Value.ToDoubleArray);
                        }
                        SentenceAndRealTargetAnnotationDic[sentence].Add(label, value / sentence.TargetWorkerTargetAnnotationDic.Count);
                        SentenceAndBinaryTargetAnnotationDic[sentence].Add(label, value >= 0);
                        ++LabelIndex;
                    }
                }

                #region 计算Accuracy
                double accuracy = 0;
                //观察用
                IDictionary<Sentence, double> sentenceAndAccuracyDic = new Dictionary<Sentence, double>();
                foreach (Sentence sentence in TrainConstant.SentenceList)
                {
                    double similarity = 0;
                    
                    foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                    {
                        if (sentence.GoldTargetAnnotation.LabelAndTruthDic[label] == SentenceAndBinaryTargetAnnotationDic[sentence][label])
                            ++similarity;
                    }
                    similarity /= Constant.TargetTaxonomy.LabelArray.Length;
                    sentenceAndAccuracyDic.Add(sentence, similarity);
                    accuracy += similarity;
                }
                //观察用
                IDictionary<Sentence, double> sortedElements = GeneralFunction.SortDictionary(sentenceAndAccuracyDic);

                accuracy /= TrainConstant.SentenceList.Count;

                Console.WriteLine("time" + i + "," + accuracy);
                #endregion

                #region 输出RealAnnotations
                string path = "Output/" + Constant.Gold + "/" + TrainConstant.Corpus + "_" + Constant.SourceTaxonomy.Name + "/ExpertiseCombination/Test/time" + i;
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                StreamWriter RealFile = new StreamWriter(path + "/time" + i + "TestRealTargetAnnotations.csv", false, Encoding.Default);
                RealFile.Write("Sentence,");
                foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                {
                    RealFile.Write(label + ",");
                }
                RealFile.WriteLine();

                foreach (Sentence sentence in TrainConstant.SentenceList)
                {
                    RealFile.Write(sentence.ToString() + ",");
                    foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                    {
                        RealFile.Write(SentenceAndRealTargetAnnotationDic[sentence][label] + ",");
                    }
                    RealFile.WriteLine();
                }
                RealFile.Close();
                #endregion

                #region 输出BinaryAnnotations
                StreamWriter BinaryFile = new StreamWriter(path + "/time" + i + "TestBinaryTargetAnnotations.csv", false, Encoding.Default);
                BinaryFile.Write("Sentence,");
                foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                {
                    BinaryFile.Write(label + ",");
                }
                BinaryFile.WriteLine();

                foreach (Sentence sentence in TrainConstant.SentenceList)
                {
                    BinaryFile.Write(sentence.ToString() + ",");
                    foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                    {
                        BinaryFile.Write((SentenceAndBinaryTargetAnnotationDic[sentence][label] ? 1 : 0) + ",");
                    }
                    BinaryFile.WriteLine();
                }
                BinaryFile.Close();
                #endregion
            }
        }
    }
}