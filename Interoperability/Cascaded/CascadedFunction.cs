using Interoperability.Entity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Interoperability.Cascaded
{
    static class CascadedFunction
    {
        /// <summary>
        /// s转移到t的概率矩阵。
        /// （s已知，求t的概率）。
        /// 验证和是1。
        /// </summary>
        /// <returns>s->t->概率</returns>
        static public void Pr_t_s(int groupindex)
        {
            //初始化
            CascadedConstant.Pr_t_s = new Dictionary<SourceLabeltruth, IDictionary<TargetLabeltruth, double>>();

            //开始直接取
            foreach (Label sourceLabel in Constant.SourceTaxonomy.LabelArray)
            {
                SourceLabeltruth sourceTrue = new SourceLabeltruth(sourceLabel, true);
                SourceLabeltruth sourceFalse = new SourceLabeltruth(sourceLabel, false);
                CascadedConstant.Pr_t_s.Add(sourceTrue, new Dictionary<TargetLabeltruth, double>());
                CascadedConstant.Pr_t_s.Add(sourceFalse, new Dictionary<TargetLabeltruth, double>());

                foreach (Label targetLabel in Constant.TargetTaxonomy.LabelArray)
                {
                    TargetLabeltruth targetTrue = new TargetLabeltruth(targetLabel, true);
                    TargetLabeltruth targetFalse = new TargetLabeltruth(targetLabel, false);
                    #region 在SourceLabeltruth内计算
                    CascadedConstant.Pr_t_s[sourceTrue].Add(targetTrue, sourceTrue.Pr_t_s(targetTrue, groupindex));
                    CascadedConstant.Pr_t_s[sourceTrue].Add(targetFalse, sourceTrue.Pr_t_s(targetFalse, groupindex));
                    CascadedConstant.Pr_t_s[sourceFalse].Add(targetTrue, sourceFalse.Pr_t_s(targetTrue, groupindex));
                    CascadedConstant.Pr_t_s[sourceFalse].Add(targetFalse, sourceFalse.Pr_t_s(targetFalse, groupindex));
                    #endregion

                    #region normalize(用Pt做指数的话需要，已废弃)
                    //double trueProbability = sourceTrue.Pr_t_s(targetTrue);
                    //double falseProbability = sourceTrue.Pr_t_s(targetFalse);
                    //double constant = trueProbability + falseProbability;
                    //ProbabilityConstant.Pr_t_s[sourceTrue].Add(targetTrue, trueProbability / constant);
                    //ProbabilityConstant.Pr_t_s[sourceTrue].Add(targetFalse, falseProbability / constant);
                    //trueProbability = sourceFalse.Pr_t_s(targetTrue);
                    //falseProbability = sourceFalse.Pr_t_s(targetFalse);
                    //constant = trueProbability + falseProbability;
                    //ProbabilityConstant.Pr_t_s[sourceFalse].Add(targetTrue, trueProbability / constant);
                    //ProbabilityConstant.Pr_t_s[sourceFalse].Add(targetFalse, falseProbability / constant);
                    #endregion
                }
            }
        }

        /// <summary>
        /// 做计算Pr_t_s时的指数。
        /// 看Conjugate Prior，Bernoulli，Beta Distribution。
        /// 训练。
        /// </summary>
        static public void Pr_t(int groupindex)
        {
            //初始化
            CascadedConstant.Pr_t = new Dictionary<TargetLabeltruth, double>();
            foreach (Label label in Constant.TargetTaxonomy.LabelArray)
            {
                CascadedConstant.Pr_t.Add(new TargetLabeltruth(label, true), 0);
                CascadedConstant.Pr_t.Add(new TargetLabeltruth(label, false), 0);
            }

            //开始计算
            int count = 0;
            foreach (Sentence sentence in TrainConstant.SentenceList)
            {
                foreach (TargetAnnotation targetAnnotation in sentence.TargetWorkerTargetAnnotationDicGroup[groupindex].Values)
                {
                    foreach (KeyValuePair<Label, bool> labelAndTruth in targetAnnotation.LabelAndTruthDic)
                    {
                        ++CascadedConstant.Pr_t[new TargetLabeltruth(labelAndTruth.Key, labelAndTruth.Value)];
                    }
                    ++count;
                }
            }
            foreach (TargetLabeltruth targetLabeltruth in CascadedConstant.Pr_t.Keys.ToArray())
            {
                CascadedConstant.Pr_t[targetLabeltruth] /= count;
            }
        }

        static public void OutputPr_t_s(int groupsize, int groupindex)
        {
            string path = "Output/" + Constant.Gold + "/" + TrainConstant.Corpus + "_" + Constant.SourceTaxonomy.Name + "/Cascaded";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            StreamWriter File = new StreamWriter(path + "/" + groupsize + "-" + groupindex + "Pr_t_s.csv", false, Encoding.Default);
            File.Write(",");
            foreach (Label label in Constant.TargetTaxonomy.LabelArray)
            {
                File.Write(label + "=1," + label + "=0,");
            }
            File.WriteLine();

            foreach (SourceLabeltruth sourceLabeltruth in CascadedConstant.Pr_t_s.Keys)
            {
                File.Write(sourceLabeltruth.ToString() + ",");
                foreach (double probability in CascadedConstant.Pr_t_s[sourceLabeltruth].Values)
                {
                    File.Write(probability + ",");
                }
                File.WriteLine();
            }
            File.Close();
        }

        static public Label[][] PowerSet(Label[] labelArray)
        {
            Label[][] powerSet = new Label[1 << labelArray.Length][];//"<<"：移位运算符
            powerSet[0] = new Label[0]; // starting only with empty set
            for (int i = 0; i < labelArray.Length; i++)
            {
                Label cur = labelArray[i];
                int count = 1 << i; // doubling list each time
                for (int j = 0; j < count; j++)
                {
                    Label[] source = powerSet[j];
                    Label[] destination = powerSet[count + j] = new Label[source.Length + 1];
                    for (int q = 0; q < source.Length; q++)
                        destination[q] = source[q];
                    destination[source.Length] = cur;
                }
            }
            return powerSet;
        }

        static public void OutputGoldRealTargetAnnotations()
        {
            StreamWriter File = new StreamWriter("Output/" + Constant.Gold + "/" + TrainConstant.Corpus + "_" + Constant.SourceTaxonomy.Name + "/GoldRealTargetAnnotationsForProbability.csv", false, Encoding.Default);
            File.Write("Sentence,");
            foreach (Label label in Constant.TargetTaxonomy.LabelArray)
            {
                File.Write(label + ",");
            }
            File.WriteLine();

            foreach (Sentence sentence in Constant.SentenceList)
            {
                File.Write(sentence.ToString() + ",");
                foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                {
                    if (sentence.SortedTargetLabelDic.ContainsKey(label))
                        File.Write((sentence.SortedTargetLabelDic[label] / sentence.TargetWorkerTargetAnnotationDic.Count) + ",");//被标注的比例
                    else
                        File.Write(0 + ",");
                }
                File.WriteLine();
            }
            File.Close();
        }

        /// <summary>
        /// PPT 20141202
        /// 为每个source annotation求出可能的target annotations，并按照概率排序。
        /// 存储到ProbabilityConstant.Pr_T_S里。
        /// 注意：本函数不是训练，而是计算结果，所以遍历的是全部的sentence，不是用于train的sentence。
        /// </summary>
        static public void Pr_T_S()
        {
            //初始化
            CascadedConstant.Pr_T_S = new Dictionary<SourceAnnotation, IDictionary<TargetAnnotation, double>>();
            CascadedConstant.Pr_T_t = new Dictionary<TargetLabeltruth, IDictionary<TargetAnnotation, double>>();
            foreach (Label label in Constant.TargetTaxonomy.LabelArray)
            {
                CascadedConstant.Pr_T_t.Add(new TargetLabeltruth(label, true), new Dictionary<TargetAnnotation, double>());
                CascadedConstant.Pr_T_t.Add(new TargetLabeltruth(label, false), new Dictionary<TargetAnnotation, double>());
            }

            //开始计算
            foreach (Sentence sentence in Constant.SentenceList)
            {
                if (!CascadedConstant.Pr_T_S.ContainsKey(sentence.GoldSourceAnnotation))
                {
                    IDictionary<TargetAnnotation, double> targetAnnotationProbabilityDic = new Dictionary<TargetAnnotation, double>();
                    //遍历所有可能的target annotation
                    foreach (Label[] targetLabelArray in CascadedFunction.PowerSet(Constant.TargetTaxonomy.LabelArray))
                    {
                        TargetAnnotation T = new TargetAnnotation(targetLabelArray);
                        double probability = sentence.GoldSourceAnnotation.Pr_T_S(T); //算值
                        if (probability != 0)//忽略等于0的
                            targetAnnotationProbabilityDic.Add(T, probability);
                    }
                    //排序（已查明为什么sadness true总是最高）
                    IDictionary<TargetAnnotation, double> sortedElements = GeneralFunction.SortDictionary(targetAnnotationProbabilityDic);
                    CascadedConstant.Pr_T_S.Add(sentence.GoldSourceAnnotation, sortedElements);
                }
            }
        }
    }
}