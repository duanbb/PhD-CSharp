using System;
using System.Collections.Generic;
using System.IO;
using MultilabelEstimation.Supervised;
using MultilabelEstimation.Group;

namespace MultilabelEstimation.Algorithm.IDS
{
    static class SIDSFunction
    {
        static public void RunSIDS()
        {
            if (!SupervisedFunction.IsNumberOfTraningSentencesValid()) return;
            for (int g = 0; g < GroupVariable.AnnotatorGroups.Length; ++g)
            {
                foreach (Sentence sentence in Variable.Sentences)
                {
                    if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                    sentence.AnnotaitonGroups[g].SIDSNumResult = new NumericResult();
                }
            }
            foreach (Label label in Variable.LabelArray)
            {
                InitializeTrainingSijAndPj(label);
                for (int groupIndex = 0; groupIndex < GroupVariable.AnnotatorGroups.Length; ++groupIndex)
                {
                    //Function.ConsoleWriteLine("SIDS, " + g);
                    CalculatePij(label, groupIndex);
                    CalculatePj(label);
                    CalculatePajl(label, groupIndex);
                    CalculateSij(label, groupIndex);
                    ObtainLabelResult(label, groupIndex);
                }
            }
            double NumericIndependentEuclidean = 0;
            double BinaryIndependentDice = 0;
            double BinaryDependentDice = 0;
            double BinaryIndependentCompare = 0;
            double BinaryDependentCompare = 0;
            double BinaryDependentJaccard = 0;
            double BinaryAndNumeric = 0;
            for (int groupIndex = 0; groupIndex < GroupVariable.AnnotatorGroups.Length; ++groupIndex)
            {
                Function.WriteNumericResultFile("SIDS", groupIndex);
                Function.WriteBinaryResultFile("SIDS", groupIndex);//只输出，计算在前面
                double groupNumericIndependentEuclidean = 0;
                double groupBinaryIndependentDice = 0;
                double groupBinaryDependentDice = 0;
                double groupBinaryIndependentCompare = 0;
                double groupBinaryDependentCompare = 0;
                double groupBinaryDependentJaccard = 0;
                double groupBinaryAndNumeric = 0;
                foreach (Sentence sentence in Variable.Sentences)
                {
                    if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                    groupNumericIndependentEuclidean += SimilarityMeasure.Euclidean(sentence.AnnotaitonGroups[groupIndex].SIDSNumResult, sentence.NumericGold);
                    groupBinaryIndependentDice += SimilarityMeasure.DicePlusANumber(sentence.AnnotaitonGroups[groupIndex].SIDSNumResult.ToBinaryResult(), sentence.NumericGold.ToBinaryResult());
                    groupBinaryIndependentCompare += SimilarityMeasure.Compare(sentence.AnnotaitonGroups[groupIndex].SIDSNumResult.ToBinaryResult(), sentence.NumericGold.ToBinaryResult());
                    groupBinaryDependentCompare += SimilarityMeasure.Compare(sentence.AnnotaitonGroups[groupIndex].SIDSResult, sentence.BinaryGold);
                    groupBinaryDependentDice += SimilarityMeasure.DicePlusANumber(sentence.AnnotaitonGroups[groupIndex].SIDSResult, sentence.BinaryGold);
                    groupBinaryDependentJaccard += SimilarityMeasure.JaccardPlusANumber(sentence.AnnotaitonGroups[groupIndex].SIDSResult, sentence.BinaryGold);
                    groupBinaryAndNumeric += SimilarityMeasure.BinaryAndNumeric(sentence.AnnotaitonGroups[groupIndex].SIDSResult, sentence.NumericGold);
                }
                NumericIndependentEuclidean += groupNumericIndependentEuclidean / (Variable.Sentences.Count - SupervisedVariable.NumberOfTraningSentences);
                BinaryIndependentDice += groupBinaryIndependentDice / (Variable.Sentences.Count - SupervisedVariable.NumberOfTraningSentences);
                BinaryDependentDice += groupBinaryDependentDice / (Variable.Sentences.Count - SupervisedVariable.NumberOfTraningSentences);
                BinaryIndependentCompare += groupBinaryIndependentCompare / (Variable.Sentences.Count - SupervisedVariable.NumberOfTraningSentences);
                BinaryDependentCompare += groupBinaryDependentCompare / (Variable.Sentences.Count - SupervisedVariable.NumberOfTraningSentences);
                BinaryDependentJaccard += groupBinaryDependentJaccard / (Variable.Sentences.Count - SupervisedVariable.NumberOfTraningSentences);
                BinaryAndNumeric += groupBinaryAndNumeric / (Variable.Sentences.Count - SupervisedVariable.NumberOfTraningSentences);
            }
            NumericIndependentEuclidean /= GroupVariable.AnnotatorGroups.Length;
            BinaryIndependentDice /= GroupVariable.AnnotatorGroups.Length;
            BinaryDependentDice /= GroupVariable.AnnotatorGroups.Length;
            BinaryIndependentCompare /= GroupVariable.AnnotatorGroups.Length;
            BinaryDependentCompare /= GroupVariable.AnnotatorGroups.Length;
            BinaryDependentJaccard /= GroupVariable.AnnotatorGroups.Length;
            BinaryAndNumeric /= GroupVariable.AnnotatorGroups.Length;
        }

        static private void InitializeTrainingSijAndPj(Label label)
        {
            double[,] Nil = new double[SupervisedVariable.NumberOfTraningSentences, 2];
            //句子i被标的总次数，无论标签为何，用于计算Sij
            double[] Ni = new double[SupervisedVariable.NumberOfTraningSentences];
            foreach(Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID >= SupervisedVariable.NumberOfTraningSentences) break;
                foreach (Annotator annotator in Variable.Annotators)
                {
                    foreach (Annotation annotation in Variable.Data[annotator][sentence])
                    {
                        if (annotation.Labels[label]) ++Nil[sentence.ID, 1];
                        else ++Nil[sentence.ID, 0];
                        ++Ni[sentence.ID];
                    }
                }
            }
            foreach(Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID < SupervisedVariable.NumberOfTraningSentences)
                {
                    for (int j = 0; j < 2; ++j)
                    {
                        SIDSVariable.TrainingSij.Value[sentence][new Labelset(label, Convert.ToBoolean(j))] = Nil[sentence.ID, j] / Ni[sentence.ID];
                        SIDSVariable.TrainingPj.Value[new Labelset(label, Convert.ToBoolean(j))] += SIDSVariable.TrainingSij.Value[sentence][new Labelset(label, Convert.ToBoolean(j))];
                    }
                }
            }
        }

        static private void CalculatePajl(Label label, int group)
        {
            SIDSVariable.PAkjl = new PAkjl(0);
            IDictionary<string, double[,]> numerator = new Dictionary<string, double[,]>();//分子
            IDictionary<string, double[]> denominator = new Dictionary<string, double[]>();//分母
            //计算分子分母
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (KeyValuePair<Annotator, Annotation> kAndl in sentence.AnnotaitonGroups[group].AnnotatorAnnotationDic)
                {
                    if (!numerator.ContainsKey(kAndl.Key.ID))
                    {
                        numerator.Add(kAndl.Key.ID, new double[2, 2]);
                        denominator.Add(kAndl.Key.ID, new double[2]);
                    }
                    if (sentence.ID >= SupervisedVariable.NumberOfTraningSentences) continue;//没标过训练句的人的信息也应加入到分子分母中，所以此句应在循环内，并且是continue
                    for (int j = 0; j < 2; ++j)//正确标签
                    {
                        numerator[kAndl.Key.ID][j, kAndl.Value.IntLabel] += SIDSVariable.TrainingSij.Value[sentence][new Labelset(label, Convert.ToBoolean(j))];
                        denominator[kAndl.Key.ID][j] += SIDSVariable.TrainingSij.Value[sentence][new Labelset(label, Convert.ToBoolean(j))];
                    }
                }
            }
            //计算π
            foreach (Annotator annotator in GroupVariable.AnnotatorGroups[group])//人
            {
                for (int j = 0; j < 2; ++j)//正确标签
                {
                    if (denominator[annotator.ID][j] != 0)//某些结果就是在所有句子中都没出现过
                    {
                        for (int l = 0; l < 2; ++l)//人标的标签
                        {
                            SIDSVariable.PAkjl.Value[annotator][new Labelset(label, Convert.ToBoolean(j))][new Labelset(label, Convert.ToBoolean(j))] = numerator[annotator.ID][j, l] / denominator[annotator.ID][j];
                        }
                    }
                }
            }
        }

        static private void CalculatePj(Label label)
        {
            SIDSVariable.Pj = new Pj(0);
            foreach(Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int j = 0; j < 2; ++j)
                {
                    SIDSVariable.Pj.Value[new Labelset(label, Convert.ToBoolean(j))] += SIDSVariable.Sij.Value[sentence][new Labelset(label, Convert.ToBoolean(j))];
                }
            }
            for (int j = 0; j < 2; ++j)
            {
                SIDSVariable.Pj.Value[new Labelset(label, Convert.ToBoolean(j))] += SIDSVariable.TrainingPj.Value[new Labelset(label, Convert.ToBoolean(j))];
            }
            if (Variable.PjDividSentenceCount)
            {
                for (int j = 0; j < 2; ++j)
                {
                    SIDSVariable.Pj.Value[new Labelset(label, Convert.ToBoolean(j))] /= Variable.Sentences.Count;
                }
            }
        }

        static private void CalculatePij(Label label, int group)
        {
            double[,] Nil = new double[Variable.Sentences.Count, 2];
            //句子i被标的总次数，无论标签为何，用于计算Sij
            double[] Ni = new double[Variable.Sentences.Count];
            foreach (Sentence i in Variable.Sentences)
            {
                if (i.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                foreach (Annotation j in i.AnnotaitonGroups[group].AnnotatorAnnotationDic.Values)
                {
                    if (j.Labels[label]) ++Nil[i.ID, 1];
                    else ++Nil[i.ID, 0];
                    ++Ni[i.ID];
                }
            }
            SIDSVariable.Sij = new Sij(0);
            foreach(Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int j = 0; j < 2; ++j)
                {
                    SIDSVariable.Sij.Value[sentence][new Labelset(label, Convert.ToBoolean(j))] = Nil[sentence.ID, j] / Ni[sentence.ID];
                }
            }
        }

        static private void ObtainLabelResult(Label label, int group)
        {
            foreach (Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                sentence.AnnotaitonGroups[group].SIDSNumResult.Labels[label] = SIDSVariable.Sij.Value[sentence][new Labelset(label, true)];
            }
        }

        static private void CalculateSij(Label label, int group)
        {
            double[,] numerator = new double[Variable.Sentences.Count, 2];//sij的分子
            for (int i = 0; i < Variable.Sentences.Count; ++i)
            {
                if (i < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int j = 0; j < 2; ++j)
                    numerator[i, j] = 1;
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int j = 0; j < 2; ++j)//正确标签
                {
                    foreach (KeyValuePair<Annotator, Annotation> kAndl in sentence.AnnotaitonGroups[group].AnnotatorAnnotationDic)//这里面的l肯定只包括k标过的
                    {
                        numerator[sentence.ID, j] *= SIDSVariable.PAkjl.Value[kAndl.Key][new Labelset(label, Convert.ToBoolean(j))][new Labelset(label, Convert.ToBoolean(kAndl.Value.IntLabel))];
                    }
                    numerator[sentence.ID, j] *= SIDSVariable.Pj.Value[new Labelset(label, Convert.ToBoolean(j))];
                }
            }
            double[] denominator = new double[Variable.Sentences.Count];
            for (int i = 0; i < Variable.Sentences.Count; ++i)
            {
                if (i < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int q = 0; q < 2; ++q)
                {
                    denominator[i] += numerator[i, q];
                }
            }
            SIDSVariable.Sij = new Sij(++SIDSVariable.Sij.Time);
            foreach(Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int j = 0; j < 2; ++j)
                {
                    if (Variable.SijDividPDataOnI)
                        SIDSVariable.Sij.Value[sentence][new Labelset(label, Convert.ToBoolean(j))] = numerator[sentence.ID, j] / denominator[sentence.ID];
                    else
                        SIDSVariable.Sij.Value[sentence][new Labelset(label, Convert.ToBoolean(j))] = numerator[sentence.ID, j];
                }
            }
        }
    }
}