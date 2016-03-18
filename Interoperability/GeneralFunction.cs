using Interoperability.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Interoperability
{
    static class GeneralFunction
    {
        /// <summary>
        /// 输出每个setnence的最优target anntoation, i.e., 概率最大的target annotation。
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="groupsize">组容量</param>
        /// <param name="groupindex">组号</param>
        static public void OutputEstimatedBinaryTargetAnnotations(Method method, int groupsize, int groupindex)
        {
            string path = "Output/" + Constant.Gold + "/" + TrainConstant.Corpus + "_" + Constant.SourceTaxonomy.Name + "/" + method;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            StreamWriter File = new StreamWriter(path + "/" + groupsize + "-" + groupindex + method + "_BinaryTargetAnnotations.csv", false, Encoding.Default);
            File.Write("Sentence,");
            foreach (Label label in Constant.TargetTaxonomy.LabelArray)
            {
                File.Write(label + ",");
            }
            File.WriteLine("GashCode");

            switch (method)
            {
                case Method.MLE:
                    foreach (Sentence sentence in Constant.SentenceList)
                    {
                        File.Write(sentence.ToString() + ",");
                        TargetAnnotation targetAnnotation = sentence.MLETargetAnnotation();
                        foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                        {
                            File.Write((targetAnnotation.LabelAndTruthDic[label] ? 1 : 0) + ",");
                        }
                        File.WriteLine(targetAnnotation.GetHashCode());
                    }
                    break;
                case Method.Cascaded:
                    foreach (Sentence sentence in Constant.SentenceList)
                    {
                        File.Write(sentence.ToString() + ",");
                        TargetAnnotation targetAnnotation = sentence.CascadedTargetAnnotation();
                        foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                        {
                            File.Write((targetAnnotation.LabelAndTruthDic[label] ? 1 : 0) + ",");
                        }
                        File.WriteLine(targetAnnotation.GetHashCode());
                    }
                    break;
                case Method.Aggregation:
                case Method.OrdinaryCombination:
                case Method.WeightedCombination:
                case Method.ExpertiseCombination:
                case Method.TemporaryNogeneralNonormalize:
                case Method.TemporaryNogeneralNormalize:
                case Method.TemporaryGeneralNonormalize:
                case Method.TemporaryGeneralNormalize:
                case Method.OtherNogeneralNormalize:
                case Method.OtherGeneralNonormalize:
                case Method.OtherGeneralNormalize:
                    foreach (Sentence sentence in Constant.SentenceList)
                    {
                        File.Write(sentence.ToString() + ",");
                        TargetAnnotation targetAnnotation = sentence.SpaceTargetAnnotation();
                        foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                        {
                            File.Write((targetAnnotation.LabelAndTruthDic[label] ? 1 : 0) + ",");
                        }
                        File.WriteLine(targetAnnotation.GetHashCode());
                    }
                    break;
            }
            File.Close();
        }

        static public double Accuracy(Method method)
        {
            double result = 0;

            //观察用
            IDictionary<Sentence, double> sentenceAndAccuracyDic = new Dictionary<Sentence, double>();

            switch (method)
            {
                case Method.MLE:
                    foreach (Sentence sentence in Constant.SentenceList)
                    {
                        double similarity = sentence.MLESimilarity();
                        sentenceAndAccuracyDic.Add(sentence, similarity);
                        result += similarity;
                    }
                    break;
                case Method.Cascaded:
                    foreach (Sentence sentence in Constant.SentenceList)
                    {
                        double similarity = sentence.CascadedSimilarity();
                        sentenceAndAccuracyDic.Add(sentence, similarity);
                        result += similarity;
                    }
                    break;
                case Method.Aggregation:
                case Method.OrdinaryCombination:
                case Method.WeightedCombination:
                case Method.ExpertiseCombination:
                case Method.TemporaryNogeneralNonormalize:
                case Method.TemporaryNogeneralNormalize:
                case Method.TemporaryGeneralNonormalize:
                case Method.TemporaryGeneralNormalize:
                case Method.OtherNogeneralNormalize:
                case Method.OtherGeneralNonormalize:
                case Method.OtherGeneralNormalize:
                    foreach (Sentence sentence in Constant.SentenceList)
                    {
                        double similarity = sentence.SpaceSimilarity();
                        sentenceAndAccuracyDic.Add(sentence, similarity);
                        result += similarity;
                    }
                    break;
            }
            
            //观察用
            IDictionary<Sentence, double> sortedElements = GeneralFunction.SortDictionary(sentenceAndAccuracyDic);

            return result / Constant.SentenceList.Count;
        }

        static public void OutputGoldBinarySourceAnnotations()
        {
            string path = "Output/" + Constant.Gold + "/" + TrainConstant.Corpus + "_" + Constant.SourceTaxonomy.Name;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            StreamWriter File = new StreamWriter(path + "/GoldBinarySourceAnnotations.csv", false, Encoding.Default);
            File.Write("Sentence,");
            foreach (Label label in Constant.SourceTaxonomy.LabelArray)
            {
                File.Write(label + ",");
            }
            File.WriteLine();

            foreach (Sentence sentence in Constant.SentenceList)
            {
                File.Write(sentence.ToString() + ",");
                foreach (Label label in Constant.SourceTaxonomy.LabelArray)
                {
                    File.Write((sentence.GoldSourceAnnotation.LabelAndTruthDic[label] ? 1 : 0) + ",");
                }
                File.WriteLine();
            }
            File.Close();
        }

        static public void OutputGoldBinaryTargetAnnotations()
        {
            string path = "Output/" + Constant.Gold + "/" + TrainConstant.Corpus + "_" + Constant.SourceTaxonomy.Name;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            StreamWriter File = new StreamWriter(path + "/GoldBinaryTargetAnnotations.csv", false, Encoding.Default);
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
                    File.Write((sentence.GoldTargetAnnotation.LabelAndTruthDic[label] ? 1 : 0) + ",");
                }
                File.WriteLine();
            }
            File.Close();
        }

        static public double ProductOfTwoVectors(double[] d1, double[] d2)
        {
            double result = 0;
            for (int i = 0; i < d1.Length; ++i)
            {
                result += d1[i] * d2[i];
            }
            return result;
        }

        //同时输出accuracy到console和file
        static public void ConsoleAndFile(string s)
        {
            Console.WriteLine(s);
            Constant.Output += s + "\r\n";
        }

        /// <summary>
        /// 生成Corpus的原始Sentence List，并加入到总Sentence List里。
        /// </summary>
        /// <param name="corpus">所针对的Corpus。</param>
        /// <returns>SentenceList。</returns>
        static public IList<Sentence> SentenceList(Corpus corpus)
        {
            IList<Sentence> result = new List<Sentence>();
            foreach (string speech in File.ReadAllLines(corpus + "/sentences.txt"))
            {
                result.Add(new Sentence(result.Count, speech));
            }
            Constant.SentenceList.AddRange(result);//此处决定Sentence不能是Struct，只能是Class
            return result;
        }

        static public IDictionary<T, double> SortDictionary<T>(IDictionary<T, double> dictionary)
        {
            List<KeyValuePair<T, double>> sortedElements = new List<KeyValuePair<T, double>>(dictionary);
            sortedElements.Sort(delegate(KeyValuePair<T, double> s1, KeyValuePair<T, double> s2)
            {
                return s2.Value.CompareTo(s1.Value);
            });
            Dictionary<T, double> result = new Dictionary<T, double>();
            foreach (KeyValuePair<T, double> element in sortedElements)
            {
                result.Add(element.Key, element.Value);
            }
            return result;
        }

        static public double Similarity(TargetAnnotation a, TargetAnnotation b)
        {
            double result = 0;
            switch (Constant.Similarity)
            {
                case Interoperability.Similarity.SMC:
                    foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                    {
                        if (a.LabelAndTruthDic[label] == b.LabelAndTruthDic[label])
                            ++result;
                    }
                    result /= Constant.TargetTaxonomy.LabelArray.Length;
                    break;
                case Interoperability.Similarity.Jaccard:
                    double numerator = 0;
                    double denominator = 0;
                    foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                    {
                        if (a.LabelAndTruthDic[label] && b.LabelAndTruthDic[label])
                        {
                            ++numerator;
                            ++denominator;
                        }
                        else if (a.LabelAndTruthDic[label] || b.LabelAndTruthDic[label])
                        {
                            ++denominator;
                        }
                    }
                    result = numerator / denominator;
                    break;
                case Interoperability.Similarity.Dice:
                    numerator = 0;
                    denominator = 0;
                    foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                    {
                        if (a.LabelAndTruthDic[label] && b.LabelAndTruthDic[label])
                        {
                            numerator += 2;
                            denominator += 2;
                        }
                        else if (a.LabelAndTruthDic[label] || b.LabelAndTruthDic[label])
                        {
                            ++denominator;
                        }
                    }
                    result = (numerator + 1) / (denominator + 1);
                    break;
            }
            return result;
        }
    }
}