using System;
using System.Collections.Generic;
using MultilabelEstimation.Supervised;
using MultilabelEstimation.Group;

namespace MultilabelEstimation.Algorithm.PDS
{
    class SPDSFunction
    {
        static public void RunSPDS()
        {
            if (!SupervisedFunction.IsNumberOfTraningSentencesValid()) return;
            double NumericIndependentEuclidean = 0;
            double BinaryIndependentDice = 0;
            double BinaryDependentDice = 0;
            double BinaryIndependentCompare = 0;
            double BinaryDependentCompare = 0;
            double BinaryDependentJaccard = 0;
            double BinaryAndNumeric = 0;
            for (int groupIndex = 0; groupIndex < GroupVariable.AnnotatorGroups.Length; ++groupIndex)
            {
                Label[] LabelArray = GroupFunction.DescendLabelsByNumber(groupIndex);
                foreach (Sentence sentence in Variable.Sentences)
                {
                    if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                    sentence.AnnotaitonGroups[groupIndex].SPDSNumResult = new NumericResult();
                    sentence.AnnotaitonGroups[groupIndex].SPDSResult = new Result();
                }
                IList<LabelPair> bilabels = PDSFunction.GenerateBilabels(groupIndex);
                foreach (LabelPair bilabel in bilabels)
                {
                    InitializeTrainingSijAndPj(bilabel);
                    CalculatePij(bilabel, groupIndex);
                    CalculatePj(bilabel);
                    CalculatePAkjl(bilabel, groupIndex);
                    CalculateSij(bilabel, groupIndex);
                    ObtainLabelResult(bilabel, groupIndex);
                }
                Function.WriteBinaryResultFile("SPDS", groupIndex);//只输出，计算在前面
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
                    groupNumericIndependentEuclidean += SimilarityMeasure.Euclidean(sentence.AnnotaitonGroups[groupIndex].SPDSNumResult, sentence.NumericGold);
                    groupBinaryIndependentDice += SimilarityMeasure.DicePlusANumber(sentence.AnnotaitonGroups[groupIndex].SPDSNumResult.ToBinaryResult(), sentence.NumericGold.ToBinaryResult());
                    groupBinaryIndependentCompare += SimilarityMeasure.Compare(sentence.AnnotaitonGroups[groupIndex].SPDSNumResult.ToBinaryResult(), sentence.NumericGold.ToBinaryResult());
                    groupBinaryDependentCompare += SimilarityMeasure.Compare(sentence.AnnotaitonGroups[groupIndex].SPDSResult, sentence.BinaryGold);
                    groupBinaryDependentDice += SimilarityMeasure.DicePlusANumber(sentence.AnnotaitonGroups[groupIndex].SPDSResult, sentence.BinaryGold);
                    groupBinaryDependentJaccard += SimilarityMeasure.JaccardPlusANumber(sentence.AnnotaitonGroups[groupIndex].SPDSResult, sentence.BinaryGold);
                    groupBinaryAndNumeric += SimilarityMeasure.BinaryAndNumeric(sentence.AnnotaitonGroups[groupIndex].SPDSResult, sentence.NumericGold);
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

        static private void ObtainLabelResult(LabelPair bilabel, int group)
        {
            foreach (Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                sentence.AnnotaitonGroups[group].SPDSNumResult.Labels[bilabel.First] += SPDSVariable.Sij.Value[sentence][new Labelset(bilabel.ToList(), 1)] + SPDSVariable.Sij.Value[sentence][new Labelset(bilabel.ToList(), 3)];//得到numeric结果
                sentence.AnnotaitonGroups[group].SPDSNumResult.Labels[bilabel.Second] += SPDSVariable.Sij.Value[sentence][new Labelset(bilabel.ToList(), 2)] + SPDSVariable.Sij.Value[sentence][new Labelset(bilabel.ToList(), 3)]; ;//得到numeric结果
                switch (SPDSVariable.Sij.CalculateJointBestLabelset(sentence).Key.IntLabel)
                {
                    case 1:
                        sentence.AnnotaitonGroups[group].SPDSResult.Labels[bilabel.Second] = true;
                        break;
                    case 2:
                        sentence.AnnotaitonGroups[group].SPDSResult.Labels[bilabel.First] = true;
                        break;
                    case 3:
                        sentence.AnnotaitonGroups[group].SPDSResult.Labels[bilabel.Second] = true;
                        sentence.AnnotaitonGroups[group].SPDSResult.Labels[bilabel.First] = true;
                        break;
                }
            }
        }

        static private void CalculateSij(LabelPair bilabel, int group)
        {
            double[,] numerator = new double[Variable.Sentences.Count, 4];//sij的分子
            for (int i = 0; i < Variable.Sentences.Count; ++i)
            {
                if (i < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int j = 0; j < 4; ++j)
                    numerator[i, j] = 1;
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int j = 0; j < 4; ++j)//正确标签
                {
                    foreach (KeyValuePair<Annotator, Annotation> kAndl in sentence.AnnotaitonGroups[group].AnnotatorAnnotationDic)//这里面的l肯定只包括k标过的
                    {
                        numerator[sentence.ID, j] *= SPDSVariable.PAkjl.Value[kAndl.Key][new Labelset(bilabel.ToArray(), j)][new Labelset(bilabel.ToArray(), kAndl.Value.IntLabel)];
                    }
                    numerator[sentence.ID, j] *= SPDSVariable.Pj.Value[new Labelset(bilabel.ToArray(), j)];
                }
            }
            double[] denominator = new double[Variable.Sentences.Count];
            for (int i = 0; i < Variable.Sentences.Count; ++i)
            {
                if (i < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int q = 0; q < 4; ++q)
                {
                    denominator[i] += numerator[i, q];
                }
            }
            SPDSVariable.Sij = new Sij(++SPDSVariable.Sij.Time);
            foreach (Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int j = 0; j < 4; ++j)
                {
                    if (Variable.SijDividPDataOnI)
                        SPDSVariable.Sij.Value[sentence][new Labelset(bilabel.ToArray(), j)] = numerator[sentence.ID, j] / denominator[sentence.ID];
                    else
                        SPDSVariable.Sij.Value[sentence][new Labelset(bilabel.ToArray(), j)] = numerator[sentence.ID, j];
                }
            }
        }

        static private void CalculatePAkjl(LabelPair bilabel, int group)
        {
            SPDSVariable.PAkjl = new PAkjl(0);
            IDictionary<string, double[,]> numerator = new Dictionary<string, double[,]>();//分子
            IDictionary<string, double[]> denominator = new Dictionary<string, double[]>();//分母
            //计算分子分母
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (KeyValuePair<Annotator, Annotation> kAndl in sentence.AnnotaitonGroups[group].AnnotatorAnnotationDic)
                {
                    if (!numerator.ContainsKey(kAndl.Key.ID))
                    {
                        numerator.Add(kAndl.Key.ID, new double[4, 4]);
                        denominator.Add(kAndl.Key.ID, new double[4]);
                    }
                    if (sentence.ID >= SupervisedVariable.NumberOfTraningSentences) continue;
                    for (int j = 0; j < 4; ++j)//正确标签
                    {
                        numerator[kAndl.Key.ID][j, kAndl.Value.IntLabel] += SPDSVariable.TrainingSij.Value[sentence][new Labelset(bilabel.ToArray(), j)];
                        denominator[kAndl.Key.ID][j] += SPDSVariable.TrainingSij.Value[sentence][new Labelset(bilabel.ToArray(), j)];
                    }
                }
            }
            //计算π
            foreach (Annotator annotator in GroupVariable.AnnotatorGroups[group])//人
            {
                for (int j = 0; j < 4; ++j)//正确标签
                {
                    if (denominator[annotator.ID][j] != 0)//某些结果就是在所有句子中都没出现过
                    {
                        for (int l = 0; l < 4; ++l)//人标的标签
                        {
                            SPDSVariable.PAkjl.Value[annotator][new Labelset(bilabel.ToArray(), j)][new Labelset(bilabel.ToArray(), j)] = numerator[annotator.ID][j, l] / denominator[annotator.ID][j];
                        }
                    }
                }
            }
        }

        static private void CalculatePj(LabelPair bilabel)
        {
            SPDSVariable.Pj = new Pj(0);
            foreach (Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int j = 0; j < 4; ++j)
                {
                    SPDSVariable.Pj.Value[new Labelset(bilabel.ToArray(), j)] += SPDSVariable.Sij.Value[sentence][new Labelset(bilabel.ToArray(), j)];
                }
            }
            for (int j = 0; j < 4; ++j)
            {
                SPDSVariable.Pj.Value[new Labelset(bilabel.ToArray(), j)] += SPDSVariable.TrainingPj.Value[new Labelset(bilabel.ToArray(), j)];
            }
            if (Variable.PjDividSentenceCount)
            {
                for (int j = 0; j < 4; ++j)
                {
                    SPDSVariable.Pj.Value[new Labelset(bilabel.ToArray(), j)] /= Variable.Sentences.Count;
                }
            }
        }

        static private void CalculatePij(LabelPair bilabel, int group)
        {
            double[,] Nil = new double[Variable.Sentences.Count, 4];
            //句子i被标的总次数，无论标签为何，用于计算Sij
            double[] Ni = new double[Variable.Sentences.Count];
            foreach (Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                foreach (Annotation j in sentence.AnnotaitonGroups[group].AnnotatorAnnotationDic.Values)
                {
                    ++Nil[sentence.ID, j.IntLabel];
                    ++Ni[sentence.ID];
                }
            }
            SPDSVariable.Sij = new Sij(0);
            foreach (Sentence sentence in Variable.Sentences)
            {
                if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                for (int j = 0; j < 4; ++j)
                {
                    SPDSVariable.Sij.Value[sentence][new Labelset(bilabel.ToArray(), j)] = Nil[sentence.ID, j] / Ni[sentence.ID];
                }
            }
        }

        static private void InitializeTrainingSijAndPj(LabelPair bilabel)
        {
            double[,] Nil = new double[SupervisedVariable.NumberOfTraningSentences, 4];
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
                    for (int j = 0; j < 4; ++j)
                    {
                        SPDSVariable.TrainingSij.Value[sentence][new Labelset(bilabel.ToArray(), j)] = Nil[sentence.ID, j] / Ni[sentence.ID];
                        SPDSVariable.TrainingPj.Value[new Labelset(bilabel.ToArray(), j)] += SPDSVariable.TrainingSij.Value[sentence][new Labelset(bilabel.ToArray(), j)];
                    }
                }
                else break;
            }
        }
    }
}