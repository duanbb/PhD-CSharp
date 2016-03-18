using System;
using System.Collections.Generic;
using System.IO;

namespace SinglelabelEstimation
{
    static public class Function
    {
        static public int Initialize(ref string[] Workers, ref IDictionary<string, IDictionary<string, IList<int>>> Sentences, ref IDictionary<string, int> GoldStandard, ref IDictionary<string, string> SentenceTexts)
        {
            string directory = System.Environment.CurrentDirectory;
            string nameSpace = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            string[] folders = directory.Split(new char[]{'\\'});
            if (nameSpace != folders[folders.Length - 3])
            {
                directory = string.Empty;
                folders[folders.Length - 3] = nameSpace;
                foreach (string folder in folders)
                {
                    directory += folder + '/';
                }
            }
            else
            {
                directory += '/';
            }

            #region Workers and Sentences
            Workers = File.ReadAllLines(directory + "CrowdScale 2013/workers.csv");
            string[] dataFile = File.ReadAllLines(directory + "CrowdScale 2013/data.csv");
            string[] sentencesFile = File.ReadAllLines(directory + "CrowdScale 2013/sentences.csv");
            Sentences = new Dictionary<string, IDictionary<string, IList<int>>>();
            SentenceTexts = new Dictionary<string, string>();
            int NumberOfAnswers = 0;
            for (int i = 0; i < dataFile.Length; ++i)
            {
                IDictionary<string, IList<int>> workers = new Dictionary<string, IList<int>>();//先建后插，提高效率
                string[] workerLabels = dataFile[i].Split(',');
                for (int j = 0; j < workerLabels.Length; ++j)
                {
                    if (workerLabels[j].Length != 0)
                    {
                        IList<int> labels = new List<int>();
                        foreach (char label in workerLabels[j])
                        {
                            labels.Add(Convert.ToInt16(label) - Convert.ToInt16('0'));
                            ++NumberOfAnswers;
                        }
                        workers.Add(Workers[j], labels);
                    }
                }
                string[] idAndText = sentencesFile[i].Split(',');
                Sentences.Add(idAndText[0], workers);
                string text = idAndText[1];
                for (int j = 2; j < idAndText.Length; ++j)
                {
                    text += "," + idAndText[j];
                }
                SentenceTexts.Add(idAndText[0], text);
            }
            #endregion;

            #region GoldStandard
            GoldStandard = new Dictionary<string, int>();
            string[] rows = File.ReadAllLines(directory + "CrowdScale 2013/reference_30_.csv");
            foreach (string row in rows)
            {
                GoldStandard.Add(row.Split(',')[0], Convert.ToInt16(row.Split(',')[1]));
            }
            #endregion
            return NumberOfAnswers;
        }

        static public void Run()
        {
            Variable.Sij = new Sij(1);
            Variable.Pajl = new Pajl(0);
            Variable.Pj = new Pj(0);
            Variable.Pdata = new Pdata(0, 0);
            //Dictionary<句子，次数> 句子i被标的总次数，用于计算Sij
            Dictionary<string, double> Ni = new Dictionary<string, double>(Variable.emptySdouble);
            //Dictionary<句子，Dictionary<标签，次数>> 句子i被标为l的次数，用于计算Sij
            Dictionary<string, Dictionary<int, double>> Nil = new Dictionary<string, Dictionary<int, double>>();
            foreach (string sentence in Variable.Sentences.Keys)
            {
                Nil.Add(sentence, new Dictionary<int, double>(Variable.emptyLdouble));
            }

            foreach (string sentence in Variable.Sentences.Keys)
            {
                foreach (string worker in Variable.Sentences[sentence].Keys)
                {
                    foreach (int label in Variable.Sentences[sentence][worker])
                    {
                        ++Ni[sentence];
                        ++Nil[sentence][label];
                    }
                }
            }
            //计算初始Sij
            foreach (string sentence in Variable.Sentences.Keys)
            {
                for (int j = 0; j < Variable.CountOfLabelKinds; ++j)
                {
                    //未平滑
                    Variable.Sij.Value[sentence][j] = Nil[sentence][j] / Ni[sentence];
                    //已平滑
                    //Variable.Sij.Value[sentence.ID][j] = (Nil[sentence.ID][j] + 1.0 / Variable.CountOfLabelKinds) / (Ni[sentence.ID] + 1);
                }
            }
            if (Variable.OutputS)
                Output(Variable.Sij.ToString());
            if (Variable.OutputAccuracy)
                CalculateAccuracy();

            for (int t = 1; t <= int.MaxValue; ++t)
            {
                //计算Pj
                CalculatePj();
                //计算π
                CalculatePajl();
                //计算Sij
                CalculatePdataAndSij();
                if (Variable.Pdata.MondifiedValue == 0)
                    break;
            }

            Variable.ResultFile.Close();
        }

        static private void CalculateAccuracy()
        {
            double numberOfRightSentences = 0;
            foreach (string sentence in Variable.GoldStandard.Keys)
            {
                //第一个条件是过滤后某些sentence里有的句子Variable.Sentences里没有，Report用
                if (Variable.Sentences.ContainsKey(sentence) && GetOptimalLabel(Variable.Sij.Value[sentence]) == Variable.GoldStandard[sentence])
                {
                    ++numberOfRightSentences;
                }
            }
            Output("Number of right tweets: " + numberOfRightSentences + "\r\n" + "Accuracy: " + numberOfRightSentences / Variable.GoldStandard.Count);
        }

        static private void Output(string s)
        {
            Variable.ResultFile.WriteLine(s);
            Console.WriteLine(s);
        }

        //计算阶乘
        static private int Factorial(int i)
        {
            return ((i <= 1) ? 1 : (i * Factorial(i - 1)));
        }

        //计算π
        static private void CalculatePajl()
        {
            Variable.Pajl = new Pajl(++Variable.Pajl.Time);
            //Dictionary<人，Dictioary<Pair<标签j，标签l>，值>>：𝑛𝑢𝑚𝑏𝑒𝑟 𝑜𝑓 𝑠𝑒𝑛𝑡𝑒𝑛𝑐𝑒𝑠 𝑎𝑛𝑛𝑜𝑡𝑎𝑡𝑜𝑟 𝑘 𝑟𝑒𝑐𝑜𝑟𝑑𝑠 𝑙 𝑤ℎ𝑒𝑛 𝑗 𝑖𝑠 𝑐𝑜𝑟𝑟𝑒𝑐𝑡，分子
            Dictionary<string, Dictionary<Pair, double>> SjNlOfK = new Dictionary<string, Dictionary<Pair, double>>();
            //Dictionary<人，Dictionary<标签j，值>>：𝑛𝑢𝑚𝑏𝑒𝑟 𝑜𝑓 𝑠𝑒𝑛𝑡𝑒𝑛𝑐𝑒𝑠 𝑎𝑛𝑛𝑜𝑡𝑎𝑡𝑜𝑟 𝑘 𝑟𝑒𝑐𝑜𝑟𝑑𝑠 𝑤ℎ𝑒𝑛 𝑗 𝑖𝑠 𝑐𝑜𝑟𝑟𝑒𝑐𝑡，分母
            Dictionary<string, Dictionary<int, double>> SjNOfK = new Dictionary<string, Dictionary<int, double>>();
            //创建标签对集合

            //初始化分子分母
            foreach (string worker in Variable.Workers)
            {
                SjNlOfK.Add(worker, new Dictionary<Pair, double>(Variable.emptySiNldouble));
                SjNOfK.Add(worker, new Dictionary<int, double>(Variable.emptyLdouble));
            }
            //计算分子分母
            foreach (string sentence in Variable.Sentences.Keys)
            {
                foreach (string worker in Variable.Sentences[sentence].Keys)
                {
                    for (int j = 0; j < Variable.CountOfLabelKinds; ++j)//正确标签
                    {
                        foreach (int label in Variable.Sentences[sentence][worker])//人标的标签
                        {
                            SjNlOfK[worker][new Pair(j, label)] += Variable.Sij.Value[sentence][j];//要重写==操作符
                            SjNOfK[worker][j] += Variable.Sij.Value[sentence][j];
                        }
                    }
                }
            }
            //计算π
            foreach (string worker in Variable.Workers)//人
            {
                foreach (Pair pair in SjNlOfK[worker].Keys)
                {
                    if (SjNlOfK[worker][pair] != 0)
                        //未平滑
                        //Variable.Pajl.Value[worker][pair] += SjNlOfK[worker][pair] / SjNOfK[worker][pair.First];
                        //已平滑（效果更好）
                        Variable.Pajl.Value[worker][pair] = (SjNlOfK[worker][pair] + 1) / (SjNOfK[worker][pair.First] + Variable.CountOfLabelKinds);
                }
            }
            if (Variable.OutputPai)
                Output(Variable.Pajl.ToString());
        }
        //计算Pj
        static private void CalculatePj()
        {
            Variable.Pj = new Pj(++Variable.Pj.Time);
            foreach (Dictionary<int, double> Sj in Variable.Sij.Value.Values)
            {
                for (int j = 0; j < Variable.CountOfLabelKinds; ++j)
                {
                    Variable.Pj.Value[j] += Sj[j];
                }
            }
            for (int j = 0; j < Variable.CountOfLabelKinds; ++j)
            {
                Variable.Pj.Value[j] /= Variable.Sentences.Count;
            }
            if (Variable.OutputP)
                Output(Variable.Pj.ToString());
        }
        //计算Sij和Pdata
        static private void CalculatePdataAndSij()
        {
            Variable.Pdata = new Pdata(++Variable.Pdata.Time, Variable.Pdata.Value);
            //论文里公式2.5的分母的分子<句子，<标签，值>>
            Dictionary<string, Dictionary<int, double>> numeratorIJ = new Dictionary<string, Dictionary<int, double>>();
            foreach (string sentence in Variable.Sentences.Keys)
            {
                numeratorIJ.Add(sentence, new Dictionary<int, double>(Variable.emptyLdoubleValues1));
            }
            foreach (string sentence in Variable.Sentences.Keys)
            {
                for (int j = 0; j < Variable.CountOfLabelKinds; ++j)
                {
                    foreach (string worker in Variable.Sentences[sentence].Keys)
                    {
                        foreach (int label in Variable.Sentences[sentence][worker])
                        {
                            numeratorIJ[sentence][j] *= Variable.Pajl.Value[worker][new Pair(j, label)];
                        }
                    }
                    numeratorIJ[sentence][j] *= Variable.Pj.Value[j] * Variable.Sij.Value[sentence][j];//此时效果更好
                }
            }
            //论文里公式2.5的分母<句子，值>
            Dictionary<string, double> denominatorI = new Dictionary<string, double>();
            foreach (string sentence in Variable.Sentences.Keys)
            {
                denominatorI.Add(sentence, 0);
            }
            foreach (string sentence in Variable.Sentences.Keys)
            {
                for (int q = 0; q < Variable.CountOfLabelKinds; ++q)
                {
                    denominatorI[sentence] += numeratorIJ[sentence][q];
                }
            }
            //计算Sij和Pdata
            Variable.Sij = new Sij(++Variable.Sij.Time);
            foreach (string sentence in Variable.Sentences.Keys)
            {
                for (int j = 0; j < Variable.CountOfLabelKinds; ++j)
                {
                    //未平滑
                    Variable.Sij.Value[sentence][j] = numeratorIJ[sentence][j] / denominatorI[sentence];
                    //已平滑
                    //Variable.Sij.Value[sentence.ID][j] = (numeratorIJ[sentence.ID][j] +  1.0 / Variable.CountOfLabelKinds) / (denominatorI[sentence.ID] + 1);
                }
                //Variable.Pdata.Value *= denominatorI[sentence.ID];//因式过小，乘遍一次就等于0了
                Variable.Pdata.Value += -Math.Log10(denominatorI[sentence]);
            }
            if (Variable.OutputPdata)
                Output(Variable.Pdata.ToString());
            if (Variable.OutputAccuracy)
                CalculateAccuracy();
            if (Variable.OutputS)
                Output(Variable.Sij.ToString());
        }

        static private int GetOptimalLabel(Dictionary<int, double> labelsAndValues)
        {
            int bestLabel = 4;
            double bestValue = 0;
            foreach (KeyValuePair<int, double> labelAndValue in labelsAndValues)
            {
                if (labelAndValue.Value > bestValue)
                {
                    bestLabel = labelAndValue.Key;
                    bestValue = labelAndValue.Value;
                }
            }
            return bestLabel;
        }
    }
}