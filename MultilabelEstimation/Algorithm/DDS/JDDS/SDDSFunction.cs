using MultilabelEstimation.Group;
using MultilabelEstimation.Supervised;
using System;
using System.Collections.Generic;

namespace MultilabelEstimation.Algorithm.DDS.JDDS
{
    static class SDDSFunction
    {
        static public void RunSDDS()
        {
            if (!SupervisedFunction.IsNumberOfTraningSentencesValid()) return;
            InitializeTrainingSijAndTrainingPj();
            for (int groupIndex = 0; groupIndex < GroupVariable.AnnotatorGroups.Length; ++groupIndex)
            {
                CalculatePij(groupIndex);
                CalculatePj();
                CalculatePAkjl(groupIndex);
                CalculateSij(groupIndex);
                DDSFunction.ObtainNumericResult(SDDSVariable.Sij, "SDDS", groupIndex);
                DDSFunction.ObtainBinaryResult(SDDSVariable.Sij, "SDDS", groupIndex);
                Function.WriteNumericResultFile("SDDS", groupIndex);
                Function.WriteBinaryResultFile("SDDS", groupIndex);//连计算再输出
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
                    groupNumericIndependentEuclidean += SimilarityMeasure.Euclidean(sentence.AnnotaitonGroups[groupIndex].SDDSNumResult, sentence.NumericGold);
                    groupBinaryIndependentDice += SimilarityMeasure.DicePlusANumber(sentence.AnnotaitonGroups[groupIndex].SDDSNumResult.ToBinaryResult(), sentence.NumericGold.ToBinaryResult());
                    groupBinaryIndependentCompare += SimilarityMeasure.Compare(sentence.AnnotaitonGroups[groupIndex].SDDSNumResult.ToBinaryResult(), sentence.NumericGold.ToBinaryResult());
                    groupBinaryDependentCompare += SimilarityMeasure.Compare(sentence.AnnotaitonGroups[groupIndex].SDDSResult, sentence.BinaryGold);
                    groupBinaryDependentDice += SimilarityMeasure.DicePlusANumber(sentence.AnnotaitonGroups[groupIndex].SDDSResult, sentence.BinaryGold);
                    groupBinaryDependentJaccard += SimilarityMeasure.JaccardPlusANumber(sentence.AnnotaitonGroups[groupIndex].SDDSResult, sentence.BinaryGold);
                    groupBinaryAndNumeric += SimilarityMeasure.BinaryAndNumeric(sentence.AnnotaitonGroups[groupIndex].SDDSResult, sentence.NumericGold);
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

        static private void CalculateSij(int group)
        {
            double[,] numerator = new double[Variable.Sentences.Count, (int)Math.Pow(2, Variable.LabelArray.Length)];//sij的分子
            for (int i = 0; i < Variable.Sentences.Count; ++i)
            {
                if (i < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int j = 0; j < Math.Pow(2, Variable.LabelArray.Length); ++j)
                    numerator[i, j] = 1;
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int j = 0; j < Math.Pow(2, Variable.LabelArray.Length); ++j)//正确标签
                {
                    foreach (KeyValuePair<Annotator, Annotation> kAndl in sentence.AnnotaitonGroups[group].AnnotatorAnnotationDic)//这里面的l肯定只包括k标过的
                    {
                        numerator[sentence.ID, j] *= SDDSVariable.PAkjl.Value[kAndl.Key][new Labelset(Variable.LabelArray, j)][new Labelset(Variable.LabelArray, kAndl.Value.IntLabel)];
                    }
                    numerator[sentence.ID, j] *= SDDSVariable.Pj.Value[new Labelset(Variable.LabelArray, j)];
                }
            }
            double[] denominator = new double[Variable.Sentences.Count];
            for (int i = 0; i < Variable.Sentences.Count; ++i)
            {
                if (i < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int q = 0; q < Math.Pow(2, Variable.LabelArray.Length); ++q)
                {
                    denominator[i] += numerator[i, q];
                }
            }
            SDDSVariable.Sij = new Sij(++SDDSVariable.Sij.Time);
            foreach (Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int j = 0; j < Math.Pow(2, Variable.LabelArray.Length); ++j)
                {
                    if (Variable.SijDividPDataOnI)
                        SDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, j)] = numerator[sentence.ID, j] / denominator[sentence.ID];
                    else
                        SDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, j)] = numerator[sentence.ID, j];
                }
            }
        }
        static private void CalculatePAkjl(int group)
        {
            SDDSVariable.PAkjl = new PAkjl(0);
            IDictionary<string, double[,]> numerator = new Dictionary<string, double[,]>();//分子
            IDictionary<string, double[]> denominator = new Dictionary<string, double[]>();//分母
            //计算分子分母
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (KeyValuePair<Annotator, Annotation> kAndl in sentence.AnnotaitonGroups[group].AnnotatorAnnotationDic)
                {
                    if (!numerator.ContainsKey(kAndl.Key.ID))
                    {
                        numerator.Add(kAndl.Key.ID, new double[(int)Math.Pow(2, Variable.LabelArray.Length), (int)Math.Pow(2, Variable.LabelArray.Length)]);
                        denominator.Add(kAndl.Key.ID, new double[(int)Math.Pow(2, Variable.LabelArray.Length)]);
                    }
                    if (sentence.ID >= SupervisedVariable.NumberOfTraningSentences) continue;
                    for (int j = 0; j < Math.Pow(2, Variable.LabelArray.Length); ++j)//正确标签
                    {
                        numerator[kAndl.Key.ID][j, kAndl.Value.IntLabel] += SDDSVariable.TrainingSij.Value[sentence][new Labelset(Variable.LabelArray, j)];
                        denominator[kAndl.Key.ID][j] += SDDSVariable.TrainingSij.Value[sentence][new Labelset(Variable.LabelArray, j)];
                    }
                }
            }
            //计算π
            foreach (Annotator annotator in GroupVariable.AnnotatorGroups[group])//人
            {
                for (int j = 0; j < SupervisedVariable.NumberOfTraningSentences; ++j)//正确标签
                {
                    if (denominator[annotator.ID][j] != 0)//某些结果就是在所有句子中都没出现过
                    {
                        for (int l = 0; l < SupervisedVariable.NumberOfTraningSentences; ++l)//人标的标签
                        {
                            SDDSVariable.PAkjl.Value[annotator][new Labelset(Variable.LabelArray, j)][new Labelset(Variable.LabelArray, j)] = numerator[annotator.ID][j, l] / denominator[annotator.ID][j];
                        }
                    }
                }
            }
        }

        static private void CalculatePj()
        {
            SDDSVariable.Pj = new Pj(0);
            foreach (Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int j = 0; j < Math.Pow(2, Variable.LabelArray.Length); ++j)
                {
                    SDDSVariable.Pj.Value[new Labelset(Variable.LabelArray, j)] += SDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, j)];
                }
            }
            for (int j = 0; j < Math.Pow(2, Variable.LabelArray.Length); ++j)
            {
                SDDSVariable.Pj.Value[new Labelset(Variable.LabelArray, j)] += SDDSVariable.TrainingPj.Value[new Labelset(Variable.LabelArray, j)];
            }
            if (Variable.PjDividSentenceCount)
            {
                for (int j = 0; j < Math.Pow(2, Variable.LabelArray.Length); ++j)
                {
                    SDDSVariable.Pj.Value[new Labelset(Variable.LabelArray, j)] /= Variable.Sentences.Count;
                }
            }
        }

        static private void CalculatePij(int group)
        {
            double[,] Nil = new double[Variable.Sentences.Count, (int)Math.Pow(2, Variable.LabelArray.Length)];
            //句子i被标的总次数，无论标签为何，用于计算Sij
            double[] Ni = new double[Variable.Sentences.Count];
            foreach (Sentence i in Variable.Sentences)
            {
                if (i.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                foreach (Annotation j in i.AnnotaitonGroups[group].AnnotatorAnnotationDic.Values)
                {
                    ++Nil[i.ID, j.IntLabel];
                    ++Ni[i.ID];
                }
            }
            SDDSVariable.Sij = new Sij(0);
            foreach (Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int j = 0; j < Math.Pow(2, Variable.LabelArray.Length); ++j)
                {
                    SDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, j)] = Nil[sentence.ID, j] / Ni[sentence.ID];
                }
            }
        }

        static private void InitializeTrainingSijAndTrainingPj()
        {
            double[,] Nil = new double[SupervisedVariable.NumberOfTraningSentences, (int)Math.Pow(2, Variable.LabelArray.Length)];
            //句子i被标的总次数，无论标签为何，用于计算Sij
            double[] Ni = new double[SupervisedVariable.NumberOfTraningSentences];
            foreach (Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID >= SupervisedVariable.NumberOfTraningSentences) break;
                foreach (Annotator annotator in Variable.Annotators)
                {
                    foreach (Annotation annotation in Variable.Data[annotator][sentence])
                    {
                        ++Nil[sentence.ID, annotation.IntLabel];
                        ++Ni[sentence.ID];
                    }
                }
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID < SupervisedVariable.NumberOfTraningSentences)
                {
                    for (int j = 0; j < Math.Pow(2, Variable.LabelArray.Length); ++j)
                    {
                        SDDSVariable.TrainingSij.Value[sentence][new Labelset(Variable.LabelArray, j)] = Nil[sentence.ID, j] / Ni[sentence.ID];
                        SDDSVariable.TrainingPj.Value[new Labelset(Variable.LabelArray, j)] += SDDSVariable.TrainingSij.Value[sentence][new Labelset(Variable.LabelArray, j)];
                    }
                }
                else break;
            }
        }
    }
}