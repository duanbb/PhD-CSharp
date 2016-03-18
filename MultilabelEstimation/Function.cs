using MultilabelEstimation.Group;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MultilabelEstimation
{
    static class Function
    {
        //初始化输出文件抬头
        static public void InitialResultFile(StreamWriter file)
        {
            string result = "sentence,";
            foreach (Label label in Variable.LabelArray)
            {
                result += Variable.LabelToString[label] + ",";
            }
            result += "neutral,probability";
            file.WriteLine(result);
        }

        static private void descendLabelsForEachSentence(List<KeyValuePair<Label, int>> sortedLabelAndTimes)
        {
            foreach (Sentence sentence in Variable.Sentences)
            {
                IDictionary<Label, int> numberOfEachLabel = new Dictionary<Label, int>();
                foreach (Label label in Variable.LabelArray)
                {
                    numberOfEachLabel.Add(label, 0);
                }
                foreach (Annotator annotator in Variable.Annotators)
                {
                    foreach (Annotation annotation in Variable.Data[annotator][sentence])
                    {
                        foreach (KeyValuePair<Label, int> labelAndTimes in sortedLabelAndTimes)
                        {
                            if (annotation.Labels[labelAndTimes.Key])
                            {
                                ++numberOfEachLabel[labelAndTimes.Key];
                            }
                        }
                    }
                }
                List<KeyValuePair<Label, int>> sortedLabels = new List<KeyValuePair<Label, int>>(numberOfEachLabel);
                sortedLabels.Sort(delegate(KeyValuePair<Label, int> s1, KeyValuePair<Label, int> s2)
                {
                    return s2.Value.CompareTo(s1.Value);
                });
                sentence.SortedLabels = sortedLabels;
            }
        }

        static public void WriteBinaryResultFile(string algorithm, int groupIndex)//只输出
        {
            if (Variable.OutputResult)
            {
                StreamWriter resultFile = new StreamWriter("Result/" + Variable.NumberOfAnnotationsPerSentenceAfterGrouping + algorithm + "Binary" + groupIndex + ".csv", false, Encoding.Default);
                InitialResultFile(resultFile);
                double averageTrueLabelsPerResult = 0;
                foreach (Sentence sentence in Variable.Sentences)
                {
                    //if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                    WriteBinaryResultOfASentence(sentence.ID, sentence.AnnotaitonGroups[groupIndex].GetResultFromAlgorithmName(algorithm), sentence.Character.ID, sentence.Speech, resultFile);
                    averageTrueLabelsPerResult += sentence.AnnotaitonGroups[groupIndex].GetResultFromAlgorithmName(algorithm).NumberOfTrueLabel;
                }
                resultFile.Write("Average true labels per annotatin," + averageTrueLabelsPerResult / Variable.Sentences.Count + ",");
                resultFile.Close();
            }
        }

        //将GoldStandard输出到文件
        static public void WriteGoldToFile(string allOrReminded)
        {
            //StreamWriter file = new StreamWriter("Result/DependentGoldOf" + allOrReminded + "Data.csv", false, Encoding.Default);//只有Boku才有All，因为要筛选人
            StreamWriter file = new StreamWriter("Result/DependentGoldOf" + allOrReminded + "Data.csv", false, Encoding.UTF8);
            InitialResultFile(file);
            double averageTrueLabelsPerResult = 0;//计算GS平均的真LABEL数
            foreach (Sentence sentence in Variable.Sentences)
            {
                WriteBinaryResultOfASentence(sentence.ID, sentence.BinaryGold, sentence.Character.ID, sentence.Speech, file);
                averageTrueLabelsPerResult += sentence.BinaryGold.NumberOfTrueLabel;
            }
            file.WriteLine("Average of true labels," + averageTrueLabelsPerResult / Variable.Sentences.Count);
            file.Close();
        }

        //将一句的binary标注的结果输出到.csv
        static public void WriteBinaryResultOfASentence(int sentenceIndex, Result result, string character, string speech, StreamWriter file)
        {
            string resultString = sentenceIndex + ",";
            foreach (Label label in Variable.LabelArray)
            {
                resultString += Convert.ToInt16(result.Labels[label]) + ",";
            }
            resultString += Convert.ToInt16(result.Mu) + "," + result.Probability + "," + character + "," + speech;
            file.WriteLine(resultString);
        }

        //以某个模型的结果为基础，强制只有一个true label
        static public void GenerateGoldStandardForSnow(int group)
        {
            descendLabelsForEachSentence(PaperFunction.NumberOfEachLabel());
            foreach (Sentence sentence in Variable.Sentences)
            {
                if (sentence.AnnotaitonGroups[group].PDSResult.NumberOfTrueLabel == 1)
                {
                    sentence.BinaryGold = sentence.AnnotaitonGroups[group].PDSResult;
                }
                else
                {
                    sentence.BinaryGold = new Result(-sentence.AnnotaitonGroups[group].PDSResult.Probability);
                    foreach (KeyValuePair<Label, int> sortedLabel in sentence.SortedLabels)
                    {
                        if (sentence.AnnotaitonGroups[group].PDSResult.Labels[sortedLabel.Key])
                        {
                            sentence.BinaryGold.Labels[sortedLabel.Key] = true;
                            break;
                        }
                    }
                }
            }
            WriteGoldToFile("Snow");
        }

        //结果与系列的标准对比
        static public void GenerateSimilarityWithGolds(string algorithm, string golds)
        {
            StreamWriter similarityFile = new StreamWriter("Result/" + algorithm + "SimilarityWith" + golds + "Gold.csv");
            double similarityPerGold = 0;
            similarityFile.WriteLine(golds + "," + algorithm);
            switch (golds)
            {
                case "IndependentGold":
                    for (int r = 0; r < Variable.NumberOfAnnotationsPerSentenceAfterGrouping + 2; ++r)
                    {
                        double similarityPerSen = 0;
                        switch (algorithm)
                        {
                            case "Independent":
                                foreach (Sentence sentence in Variable.Sentences)
                                {
                                    similarityPerSen += SimilarityMeasure.Compare(sentence.IndependentGold[r], sentence.IndependentResult);
                                }
                                break;
                            case "Precise":
                                foreach (Sentence sentence in Variable.Sentences)
                                {
                                    similarityPerSen += SimilarityMeasure.Compare(sentence.IndependentGold[r], sentence.PreciseResult);
                                }
                                break;
                            case "TreeForAll":
                                foreach (Sentence sentence in Variable.Sentences)
                                {
                                    similarityPerSen += SimilarityMeasure.Compare(sentence.IndependentGold[r], sentence.TreeForAllResult);
                                }
                                break;
                            case "TreeForSen":
                                foreach (Sentence sentence in Variable.Sentences)
                                {
                                    similarityPerSen += SimilarityMeasure.Compare(sentence.IndependentGold[r], sentence.TreeForSenResult);
                                }
                                break;
                        }
                        similarityPerSen /= Variable.Sentences.Count;
                        similarityFile.WriteLine(r + "," + similarityPerSen);
                        similarityPerGold += similarityPerSen;
                    }
                    break;
                case "DependentGold":
                    for (int r = 0; r < Variable.NumberOfAnnotationsPerSentenceAfterGrouping + 2; ++r)
                    {
                        double similarityPerSen = 0;
                        switch (algorithm)
                        {
                            case "Independent":
                                foreach (Sentence sentence in Variable.Sentences)
                                {
                                    similarityPerSen += SimilarityMeasure.Compare(sentence.DependentGoldStandard[r], sentence.IndependentResult);
                                }
                                break;
                            case "Precise":
                                foreach (Sentence sentence in Variable.Sentences)
                                {
                                    similarityPerSen += SimilarityMeasure.Compare(sentence.DependentGoldStandard[r], sentence.PreciseResult);
                                }
                                break;
                            case "TreeForAll":
                                foreach (Sentence sentence in Variable.Sentences)
                                {
                                    similarityPerSen += SimilarityMeasure.Compare(sentence.DependentGoldStandard[r], sentence.TreeForAllResult);
                                }
                                break;
                            case "TreeForSen":
                                foreach (Sentence sentence in Variable.Sentences)
                                {
                                    similarityPerSen += SimilarityMeasure.Compare(sentence.DependentGoldStandard[r], sentence.TreeForSenResult);
                                }
                                break;
                        }
                        similarityPerSen /= Variable.Sentences.Count;
                        similarityFile.WriteLine(r + "," + similarityPerSen);
                        similarityPerGold += similarityPerSen;
                    }
                    break;
            }
            similarityFile.WriteLine("similarity per gold:," + similarityPerGold / Variable.NumberOfAnnotationsPerSentenceAfterGrouping + 2);
            similarityFile.Close();
        }

        //计算工作质量
        static public void SimilarityOfAnnotator()
        {
            Variable.TotalSimilarity = 0;
            Variable.TotalNumberOfAnnotatedTimes = 0;
            foreach (Annotator annotator in Variable.Annotators)
            {
                foreach (Sentence sentence in Variable.Sentences)
                {
                    foreach (Annotation annotation in Variable.Data[annotator][sentence])
                    {
                        double similarity = SimilarityMeasure.Compare(sentence.PreciseResult, annotation);
                        annotator.Similarity.TotalSimilarity += similarity;
                        ++annotator.Similarity.NumberOfAnnotatedSentences;
                        Variable.TotalSimilarity += similarity;
                        ++Variable.TotalNumberOfAnnotatedTimes;
                    }
                }
            }
            List<Annotator> sortedByTotal = new List<Annotator>(Variable.Annotators);
            sortedByTotal.Sort(delegate(Annotator s1, Annotator s2)
            {
                return s2.Similarity.TotalSimilarity.CompareTo(s1.Similarity.TotalSimilarity);
            });
            List<Annotator> sortedByAverage = new List<Annotator>(Variable.Annotators);
            sortedByAverage.Sort(delegate(Annotator s1, Annotator s2)
            {
                return s2.Similarity.AverageSimilarity.CompareTo(s1.Similarity.AverageSimilarity);
            });
            List<Annotator> sortedByNumber = new List<Annotator>(Variable.Annotators);
            sortedByNumber.Sort(delegate(Annotator s1, Annotator s2)
            {
                return s2.Similarity.NumberOfAnnotatedSentences.CompareTo(s1.Similarity.NumberOfAnnotatedSentences);
            });
            List<Annotator> sortedByPercent = new List<Annotator>(Variable.Annotators);
            sortedByPercent.Sort(delegate(Annotator s1, Annotator s2)
            {
                return s2.Similarity.PercentOfTotalSimilarity.CompareTo(s1.Similarity.PercentOfTotalSimilarity);
            });
            //输出
            StreamWriter workLoad = new StreamWriter("Result/WorkLoad.csv");
            string result = "PercentOfTotalSimilarity:" + "\n" + "name" + "," + "average" + "," + "number" + "," + "total" + "," + "percentOfTotalSimilarity" + "," + "percentOfTotalWorkload" + "," + "differenceBetweenSimilarityAndWorkload" + "," + "\n";
            foreach (Annotator s in sortedByPercent)
            {
                result += s.ID + "," + s.Similarity.AverageSimilarity + "," + s.Similarity.NumberOfAnnotatedSentences + "," + s.Similarity.TotalSimilarity + "," + s.Similarity.PercentOfTotalSimilarity + "," + s.Similarity.PercentOfWorkload + "," + s.Similarity.differenceBetweenSimilarityAndWorkload;
                result += "\n";
            }
            workLoad.Write(result);
            workLoad.Close();
        }

        //计算各结果与最终一项黄金标准的相似度
        static public void GenerateEverySimilarityWithGold()
        {
            double independent = 0;
            double precise = 0;
            double treeForAll = 0;
            double treeForSen = 0;
            foreach (Sentence sentence in Variable.Sentences)
            {
                independent += SimilarityMeasure.JaccardPlusANumber(sentence.IndependentResult, sentence.INVandDNVasGold);
                //precise += SimilarityMeasure.JaccardPlusANumber(sentence.PreciseResult, sentence.INVandDNVasGold);
                //treeForAll += SimilarityMeasure.JaccardPlusANumber(sentence.TreeForAllResult, sentence.INVandDNVasGold);
                //treeForSen += SimilarityMeasure.JaccardPlusANumber(sentence.TreeForSenResult, sentence.INVandDNVasGold);
            }
            independent /= Variable.Sentences.Count;
            precise /= Variable.Sentences.Count;
            treeForAll /= Variable.Sentences.Count;
            treeForSen /= Variable.Sentences.Count;
            //Variable.ResultFile.WriteLine(independent + "," + precise + "," + treeForAll + "," + treeForSen);
        }

        //GoldStandard中与其他最接近的一项
        static public void GoldStandardSimilarity(string golds)
        {
            StreamWriter goldSimilarityFile = new StreamWriter("Result/" + golds + "Similarity.csv");
            goldSimilarityFile.WriteLine(golds + ",Similarity");
            double[] goldStandardSimilarity = new double[Variable.NumberOfAnnotationsPerSentenceAfterGrouping + 2];
            switch (golds)
            {
                case "IndependentGold":
                    for (int r1 = 0; r1 < Variable.NumberOfAnnotationsPerSentenceAfterGrouping + 2; ++r1)
                    {
                        int n = 0;
                        foreach (Sentence sentence in Variable.Sentences)
                        {
                            for (int r2 = 0; r2 < Variable.NumberOfAnnotationsPerSentenceAfterGrouping + 2; ++r2)
                            {
                                if (r1 != r2)
                                {
                                    ++n;
                                    goldStandardSimilarity[r1] += SimilarityMeasure.JaccardPlusANumber(sentence.IndependentGold[r1], sentence.IndependentGold[r2]);
                                }
                            }
                        }
                        goldStandardSimilarity[r1] /= n;
                        goldSimilarityFile.WriteLine(r1 + "," + goldStandardSimilarity[r1]);
                    }
                    break;
                case "DependentGold":
                    for (int r1 = 0; r1 < Variable.NumberOfAnnotationsPerSentenceAfterGrouping + 2; ++r1)
                    {
                        int n = 0;
                        foreach (Sentence sentence in Variable.Sentences)
                        {
                            for (int r2 = 0; r2 < Variable.NumberOfAnnotationsPerSentenceAfterGrouping + 2; ++r2)
                            {
                                if (r1 != r2)
                                {
                                    ++n;
                                    goldStandardSimilarity[r1] += SimilarityMeasure.JaccardPlusANumber(sentence.DependentGoldStandard[r1], sentence.DependentGoldStandard[r2]);
                                }
                            }
                        }
                        goldStandardSimilarity[r1] /= n;
                        goldSimilarityFile.WriteLine(r1 + "," + goldStandardSimilarity[r1]);
                    }
                    break;
            }
            goldSimilarityFile.Close();
        }

        static public void ConsoleWriteLine(string s)
        {
            Console.WriteLine(s);
            Variable.ConsoleOutput += s + "\r\n";
        }

        static public void WriteNumericResultFile(string algorithm, int groupIndex)
        {
            if (Variable.OutputResult)
            {
                StreamWriter resultFile = new StreamWriter("Result/" + Variable.NumberOfAnnotationsPerSentenceAfterGrouping + algorithm + "NumericIndependent" + groupIndex + ".csv");
                Function.InitialResultFile(resultFile);
                for (int i = 0; i < Variable.Sentences.Count; ++i)
                {
                    Function.WriteNumericResultOfASentence(i, Variable.Sentences[i].AnnotaitonGroups[groupIndex].GetNumericResultFromName(algorithm), resultFile);
                    resultFile.WriteLine();
                }
                resultFile.Close();
            }
        }

        static public IDictionary<Smoothing, double[]> SmoothingNumber(double smoothingCoefficient)
        {
            IDictionary<Smoothing, double[]> smoothingNumber = new Dictionary<Smoothing, double[]>();
            smoothingNumber.Add(Smoothing.None, new double[] { 0, 0 });
            smoothingNumber.Add(Smoothing.Laplace, new double[] { 1, smoothingCoefficient });
            smoothingNumber.Add(Smoothing.Lidstone, new double[] { 1 / smoothingCoefficient, 1 });
            smoothingNumber.Add(Smoothing.JeffreysPerks, new double[] { 0.5, 0.5 * smoothingCoefficient });
            smoothingNumber.Add(Smoothing.Pow10minus10, new double[] { 0, Math.Pow(10, -10) });
            return smoothingNumber;
        }

        static public void InitializeEmptyLabelDic(ref IDictionary<Label, double> labelFloatDic, ref IList<LabelPair> labelPairList, Label[] labelArray)
        {
            IList<Label> traversedLabels = new List<Label>();
            foreach (Label label1 in labelArray)
            {
                labelFloatDic.Add(label1, 0);
                traversedLabels.Add(label1);
                foreach (Label label2 in labelArray)
                {
                    if (!traversedLabels.Contains(label2))
                        labelPairList.Add(new LabelPair(label1, label2));
                }
            }
        }

        //将一句的numeric标注的结果输出到.csv
        static public void WriteNumericResultOfASentence(int sentenceIndex, NumericResult annotation, StreamWriter file)
        {
            string result = sentenceIndex + ",";
            foreach (Label label in Variable.LabelArray)
            {
                result += annotation.Labels[label] + ",";
            }
            result += Convert.ToInt16(annotation.Mu);
            file.Write(result);
        }
    }
}