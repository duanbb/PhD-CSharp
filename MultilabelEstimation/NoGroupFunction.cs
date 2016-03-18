using MultilabelEstimation.Group;
using System;
using System.Collections.Generic;
using System.IO;

namespace MultilabelEstimation
{
    class NoGroupFunction
    {
        //得到依赖黄金标准，1.0
        static private void generateDependentGoldStandard()
        {
            foreach (Sentence sentence in Variable.Sentences)
            {
                sentence.DependentGoldStandard = new Result[Variable.NumberOfAnnotationsPerSentenceAfterGrouping + 2];
            }
            StreamWriter goldResultFile = new StreamWriter("Result/DependentGold.csv");
            Function.InitialResultFile(goldResultFile);
            for (int r = 0; r < Variable.NumberOfAnnotationsPerSentenceAfterGrouping + 2; ++r)
            {
                goldResultFile.WriteLine("DepGoldStandard >= " + r + ":");
                foreach (Sentence sentence in Variable.Sentences)
                {
                    sentence.DependentGoldStandard[r] = new Result();
                    IDictionary<int, int> resultsCount = new Dictionary<int, int>();
                    for (int j = 0; j < Math.Pow(2, Variable.LabelArray.Length); ++j)
                    {
                        resultsCount.Add(j, 0);
                    }
                    foreach (Annotator annotator in Variable.Annotators)
                    {
                        foreach (Annotation annotation in Variable.Data[annotator][sentence])
                        {
                            ++resultsCount[annotation.IntLabel];
                        }
                    }
                    foreach (KeyValuePair<int, int> resultCount in resultsCount)
                    {
                        if (resultCount.Value >= r)
                        {
                            Annotation annotation = new Annotation(resultCount.Key);
                            foreach (Label label in Variable.LabelArray)
                            {
                                if (annotation.Labels[label])
                                    sentence.DependentGoldStandard[r].Labels[label] = true;
                            }
                        }
                    }
                    Function.WriteBinaryResultOfASentence(sentence.ID, sentence.DependentGoldStandard[r], sentence.Character.ID, sentence.Speech, goldResultFile);
                }
            }
            /*
            #region 求各GoldStandard的平均情感数
            double[] goldStandardAverage = new double[Variable.KindsOfVote];
            for (int r = 0; r < Variable.KindsOfVote; ++r)
            {
                foreach (Sentence sentence in Variable.Sentences)
                {
                    foreach (Label label in Variable.LabelArray)
                    {
                        if (sentence.DependentGoldStandard[r].Labels[label])
                            ++goldStandardAverage[r];
                    }
                }
                goldStandardAverage[r] /= Variable.Sentences.Count;
            }
            #endregion
            #region 得到最佳结果
            double difference = int.MaxValue;
            int nearestResultIndex = 0;
            for (int r = 0; r < goldStandardAverage.Length; ++r)
            {
                if (Math.Abs(goldStandardAverage[r] - Variable.AverageTrueLabelsPerAnnotation) <= difference)
                {
                    difference = Math.Abs(goldStandardAverage[r] - Variable.AverageTrueLabelsPerAnnotation);
                    nearestResultIndex = r;
                }
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                sentence.NaiveDependentResult = sentence.DependentGoldStandard[nearestResultIndex];
            }
            #endregion
            goldResultFile.WriteLine("Best Threshold:," + nearestResultIndex);
            Variable.ResultFile.Write(nearestResultIndex + ",");
             */
            goldResultFile.Close();
        }

        //得到独立黄金标准
        static private void generateIndependentGoldStandard()
        {
            foreach (Sentence sentence in Variable.Sentences)
            {
                sentence.IndependentGold = new Result[Variable.NumberOfAnnotationsPerSentenceAfterGrouping + 2];
            }
            StreamWriter goldResultFile = new StreamWriter("Result/IndependentGold.csv");
            Function.InitialResultFile(goldResultFile);
            for (int r = 0; r < Variable.NumberOfAnnotationsPerSentenceAfterGrouping + 2; ++r)
            {
                goldResultFile.WriteLine("IndGoldStandard >= " + r + ":");
                foreach (Sentence sentence in Variable.Sentences)
                {
                    sentence.IndependentGold[r] = new Result();
                    IDictionary<Label, int> labelsCount = new Dictionary<Label, int>();
                    foreach (Annotator annotator in Variable.Annotators)
                    {
                        foreach (Annotation annotation in Variable.Data[annotator][sentence])
                        {
                            foreach (Label label in Variable.LabelArray)
                            {
                                if (annotation.Labels[label])
                                {
                                    if (labelsCount.ContainsKey(label))
                                        ++labelsCount[label];
                                    else
                                        labelsCount.Add(label, 1);
                                }
                            }
                        }
                    }
                    foreach (Label label in labelsCount.Keys)
                    {
                        if (labelsCount[label] >= r)
                        {
                            sentence.IndependentGold[r].Labels[label] = true;
                        }
                    }
                    Function.WriteBinaryResultOfASentence(sentence.ID, sentence.IndependentGold[r], sentence.Character.ID, sentence.Speech, goldResultFile);
                    goldResultFile.WriteLine();
                }
            }
            /*
            #region 求各GoldStandard的平均值
            double[] goldStandardAverage = new double[Variable.KindsOfVote];
            for (int r = 0; r < Variable.KindsOfVote; ++r)
            {
                foreach (Sentence sentence in Variable.Sentences)
                {
                    foreach (Label label in Variable.LabelArray)
                    {
                        if (sentence.IndependentGoldStandard[r].Labels[label])
                            ++goldStandardAverage[r];
                    }
                }
                goldStandardAverage[r] /= Variable.Sentences.Count;
            }
            #endregion
            #region 得到最佳结果
            double difference = int.MaxValue;
            int nearestResultIndex = 0;
            for (int r = 0; r < goldStandardAverage.Length; ++r)
            {
                if (Math.Abs(goldStandardAverage[r] - Variable.AverageTrueLabelsPerAnnotation) <= difference)
                {
                    difference = Math.Abs(goldStandardAverage[r] - Variable.AverageTrueLabelsPerAnnotation);
                    nearestResultIndex = r;
                }
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                sentence.NaiveIndependentResult = sentence.IndependentGoldStandard[nearestResultIndex];
            }
            #endregion
            goldResultFile.WriteLine("Best Threshold:," + nearestResultIndex);
            Variable.ResultFile.Write(nearestResultIndex + ",");
             */
            goldResultFile.Close();
        }

        //得到Naive Independent与Naive Dependent的并的结果作为黄金标准
        static public void INVandDNVasGold()
        {
            generateIndependentGoldStandard();
            generateDependentGoldStandard();
            StreamWriter goldResultFile = new StreamWriter("Result/INVandDNVasGolds.csv");
            Function.InitialResultFile(goldResultFile);
            #region 求各GoldStandard的平均值
            double[] goldStandardAverage = new double[Variable.NumberOfAnnotationsPerSentenceAfterGrouping + 2];
            for (int r = 0; r < Variable.NumberOfAnnotationsPerSentenceAfterGrouping + 2; ++r)
            {
                goldResultFile.WriteLine("INVandDNVoldStandard >= " + r + ":");
                foreach (Sentence sentence in Variable.Sentences)
                {
                    Function.WriteBinaryResultOfASentence(sentence.ID, sentence.INVandDNVasGolds[r], sentence.Character.ID, sentence.Speech, goldResultFile);
                    goldResultFile.WriteLine();
                    foreach (Label label in Variable.LabelArray)
                    {
                        if (sentence.INVandDNVasGolds[r].Labels[label])
                            ++goldStandardAverage[r];
                    }
                }
                goldStandardAverage[r] /= Variable.Sentences.Count;
            }
            goldResultFile.Close();
            #endregion
            #region 得到最佳结果
            double difference = int.MaxValue;
            int nearestResultIndex = 0;
            for (int r = 0; r < goldStandardAverage.Length; ++r)
            {
                if (Math.Abs(goldStandardAverage[r] - Variable.AverageTrueLabelsPerAnnotation) <= difference)
                {
                    difference = Math.Abs(goldStandardAverage[r] - Variable.AverageTrueLabelsPerAnnotation);
                    nearestResultIndex = r;
                }
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                sentence.INVandDNVasGold = sentence.INVandDNVasGolds[nearestResultIndex];
            }
            #endregion
            //Variable.ResultFile.Write(nearestResultIndex + ",");
        }

        //结果与单一的标准对比
        static public void GenerateSimilarityWithGold(string algorithm, string gold)
        {
            double similarityPerSen = 0;
            switch (gold)
            {
                case "IndAndDepGold":
                    switch (algorithm)
                    {
                        case "Independent":
                            foreach (Sentence sentence in Variable.Sentences)
                            {
                                similarityPerSen += SimilarityMeasure.Compare(sentence.IndResultAndDepResult, sentence.IndependentResult);
                            }
                            break;
                        case "Precise":
                            foreach (Sentence sentence in Variable.Sentences)
                            {
                                similarityPerSen += SimilarityMeasure.Compare(sentence.IndResultAndDepResult, sentence.PreciseResult);
                            }
                            break;
                        case "TreeForAll":
                            foreach (Sentence sentence in Variable.Sentences)
                            {
                                similarityPerSen += SimilarityMeasure.Compare(sentence.IndResultAndDepResult, sentence.TreeForAllResult);
                            }
                            break;
                        case "TreeForSen":
                            foreach (Sentence sentence in Variable.Sentences)
                            {
                                similarityPerSen += SimilarityMeasure.Compare(sentence.IndResultAndDepResult, sentence.TreeForSenResult);
                            }
                            break;
                    }
                    break;
                case "DepMVGold":
                    switch (algorithm)
                    {
                        case "Independent":
                            foreach (Sentence sentence in Variable.Sentences)
                            {
                                similarityPerSen += SimilarityMeasure.Compare(sentence.BinaryGold, sentence.IndependentResult);
                            }
                            break;
                        case "Precise":
                            foreach (Sentence sentence in Variable.Sentences)
                            {
                                similarityPerSen += SimilarityMeasure.Compare(sentence.BinaryGold, sentence.PreciseResult);
                            }
                            break;
                        case "TreeForAll":
                            foreach (Sentence sentence in Variable.Sentences)
                            {
                                similarityPerSen += SimilarityMeasure.Compare(sentence.BinaryGold, sentence.TreeForAllResult);
                            }
                            break;
                        case "TreeForSen":
                            foreach (Sentence sentence in Variable.Sentences)
                            {
                                similarityPerSen += SimilarityMeasure.Compare(sentence.BinaryGold, sentence.TreeForSenResult);
                            }
                            break;
                    }
                    break;
            }
            similarityPerSen /= Variable.Sentences.Count;
        }

        //计算某结果（或标准）中平均每句的情感数
        static public void AverageLabelsOfResultOrGold(string result)
        {
            double average = 0;
            int n = 0;
            foreach (Sentence sentence in Variable.Sentences)
            {
                ++n;
                foreach (Label label in Variable.LabelArray)
                {
                    switch (result)
                    {
                        case "DependentMajorityVoteGold":
                            if (sentence.BinaryGold.Labels[label])
                                ++average;
                            break;
                        case "IndResultAndDepResultGold":
                            if (sentence.IndResultAndDepResult.Labels[label])
                                ++average;
                            break;
                        case "IndependentResult":
                            if (sentence.IndependentResult.Labels[label])
                                ++average;
                            break;
                        case "PreciseResult":
                            if (sentence.PreciseResult.Labels[label])
                                ++average;
                            break;
                        case "TreeForAllResult":
                            if (sentence.TreeForAllResult.Labels[label])
                                ++average;
                            break;
                        case "TreeForSenResult":
                            if (sentence.TreeForSenResult.Labels[label])
                                ++average;
                            break;
                    }
                }
            }
            average /= n;
        }
    }
}