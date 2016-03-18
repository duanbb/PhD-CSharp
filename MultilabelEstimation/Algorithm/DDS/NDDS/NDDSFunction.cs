using MultilabelEstimation.Group;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultilabelEstimation.Algorithm.DDS.NDDS
{
    class NDDSFunction
    {
        static public void RunNDDS(double threshold, IndependenceEstimation independentEstimation)
        {
            for (int groupIndex = 0; groupIndex < GroupVariable.AnnotatorGroups.Length; ++groupIndex)
            {
                Sij sij = Initialize(groupIndex, threshold, independentEstimation);
                CoreFunction.Intgerate(Variable.LabelArray, groupIndex, ref sij);
                DDSFunction.ObtainBinaryResult(sij, "NDDS", groupIndex);
                Function.WriteBinaryResultFile("NDDS", groupIndex);
            }
        }

        static public Sij Initialize(int groupIndex, double threshold, IndependenceEstimation independentEstimation)
        {
            #region 初始化
            Sij sij = new Sij(1);
            #endregion
            Label[] labelArray = GroupFunction.DescendLabelsByNumber(groupIndex);
            Graph BN = NDDSFunction.BuildBN(groupIndex, labelArray, independentEstimation, threshold);
            #region 从BN中求每个情感（事件）的父节点（条件）
            IDictionary<Label, IList<Label>> LabelsAndPas = new Dictionary<Label, IList<Label>>();
            foreach (Label label in labelArray)
            {
                LabelsAndPas.Add(label, new List<Label>());
            }
            foreach (KeyValuePair<LabelPair, bool> hasRelationship in BN.AdjMatrix)
            {
                if (hasRelationship.Value)
                    LabelsAndPas[hasRelationship.Key.Second].Add(hasRelationship.Key.First);
            }
            #endregion
            #region 求计算联合概率的参数
            IDictionary<Sentence, IDictionary<LabelAndWitness, double>> Probability = new Dictionary<Sentence, IDictionary<LabelAndWitness, double>>();
            foreach (Sentence sentence in Variable.Sentences)
            {
                Probability.Add(sentence, new Dictionary<LabelAndWitness, double>());
            }
            IDictionary<Smoothing, double[]> smoothingNumber = Function.SmoothingNumber(2);
            foreach (KeyValuePair<Label, IList<Label>> labelAndPas in LabelsAndPas)
            {
                if (labelAndPas.Value.Count == 0)
                {
                    foreach (Sentence sentence in Variable.Sentences)
                    {
                        double numberOfLabelTrue = 0;
                        double numberOfLabelFalse = 0;
                        foreach (Annotation annotation in sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Values)
                        {
                            if (annotation.Labels[labelAndPas.Key])
                                ++numberOfLabelTrue;
                            else
                                ++numberOfLabelFalse;
                        }
                        if (NDDSVariable.SmoothBN != Smoothing.None)
                        {
                            Probability[sentence].Add(new LabelAndWitness(new Labelset(labelAndPas.Key, false), new Labelset()), (numberOfLabelFalse + smoothingNumber[NDDSVariable.SmoothBN][0]) / (Variable.NumberOfAnnotationsPerSentenceAfterGrouping + smoothingNumber[NDDSVariable.SmoothBN][1]));
                            Probability[sentence].Add(new LabelAndWitness(new Labelset(labelAndPas.Key, true), new Labelset()), (numberOfLabelTrue + (numberOfLabelTrue + smoothingNumber[NDDSVariable.SmoothBN][0]) / (Variable.NumberOfAnnotationsPerSentenceAfterGrouping + smoothingNumber[NDDSVariable.SmoothBN][1])));
                        }
                        else
                        {
                            Probability[sentence].Add(new LabelAndWitness(new Labelset(labelAndPas.Key, false), new Labelset()), numberOfLabelFalse / Variable.NumberOfAnnotationsPerSentenceAfterGrouping);
                            Probability[sentence].Add(new LabelAndWitness(new Labelset(labelAndPas.Key, true), new Labelset()),  numberOfLabelTrue / Variable.NumberOfAnnotationsPerSentenceAfterGrouping);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Math.Pow(2, labelAndPas.Value.Count); ++i)
                    {
                        Labelset Labelset = new Labelset(labelAndPas.Value, i);
                        foreach (Sentence sentence in Variable.Sentences)
                        {
                            double numberOfLabelTrue = 0;
                            double numberOfLabelFalse = 0;
                            foreach (Annotation annotation in sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Values)
                            {
                                if (annotation.IsAccordingToLabelset(Labelset))
                                {
                                    if (annotation.Labels[labelAndPas.Key])
                                        ++numberOfLabelTrue;
                                    else
                                        ++numberOfLabelFalse;
                                }
                            }
                            if (NDDSVariable.SmoothBN != Smoothing.None)
                            {
                                Probability[sentence].Add(new LabelAndWitness(new Labelset(labelAndPas.Key, false), Labelset), (numberOfLabelFalse + smoothingNumber[NDDSVariable.SmoothBN][0]) / (Variable.NumberOfAnnotationsPerSentenceAfterGrouping + smoothingNumber[NDDSVariable.SmoothBN][1]));
                                Probability[sentence].Add(new LabelAndWitness(new Labelset(labelAndPas.Key, true), Labelset), (numberOfLabelTrue + smoothingNumber[NDDSVariable.SmoothBN][0]) / (Variable.NumberOfAnnotationsPerSentenceAfterGrouping + smoothingNumber[NDDSVariable.SmoothBN][1]));
                            }
                            else
                            {
                                Probability[sentence].Add(new LabelAndWitness(new Labelset(labelAndPas.Key, false), Labelset), numberOfLabelFalse / Variable.NumberOfAnnotationsPerSentenceAfterGrouping);
                                Probability[sentence].Add(new LabelAndWitness(new Labelset(labelAndPas.Key, true), Labelset), numberOfLabelTrue / Variable.NumberOfAnnotationsPerSentenceAfterGrouping);
                            }
                        }
                    }
                }
            }
            #endregion
            #region 计算Sij
            IDictionary<Sentence, double> denominator = new Dictionary<Sentence, double>();//归一化参数
            foreach (Sentence sentence in Variable.Sentences)
            {
                sij.Value.Add(sentence, new Dictionary<Labelset, double>());
                denominator.Add(sentence, 0);
                for (int l = 0; l < Math.Pow(2, Variable.LabelArray.Length); ++l)
                {
                    Labelset Labelset = new Labelset(Variable.LabelArray, l);
                    double value = 1;
                    foreach (Label label in labelArray)
                    {
                        Labelset singleLabelAnnotation = new Labelset(label, Labelset.Labels[label]);
                        Labelset subLabelset = new Labelset(LabelsAndPas[label], Labelset);
                        value *= Probability[sentence][new LabelAndWitness(singleLabelAnnotation, subLabelset)];
                    }
                    if (value != 0)
                    {
                        sij.Value[sentence].Add(Labelset, value);
                        denominator[sentence] += value;
                    }
                }
            }
            #endregion
            #region 归一化
            foreach (Sentence sentence in Variable.Sentences.ToArray())
            {
                foreach (Labelset labelset in sij.Value[sentence].Keys.ToArray())
                {
                    sij.Value[sentence][labelset] /= denominator[sentence];
                }
            }
            #endregion
            return sij;
        }

        static private Graph BuildBN(int group, Label[] labelArray, IndependenceEstimation independentEstimation, double thresholdOfIndependentForNetwork)
        {
            #region Build-PMap-Skeleton
            Graph H = new Graph();
            int i = 0;
            IDictionary<LabelPair, IList<Label>> labelPairAndWitness = new Dictionary<LabelPair, IList<Label>>();
            while (i <= H.MaxDegree())
            {
                foreach (Label X in labelArray)
                {
                    foreach (Label Y in labelArray)
                    {
                        LabelPair labelPair = new LabelPair(X, Y);
                        if (X != Y && H.AdjMatrix[labelPair] && H.AdjMatrix[labelPair.Reverse])//相接的点
                        {
                            foreach (List<Label> witness in H.GetLatentWitnesses(labelPair, i))
                            {
                                bool isAWitness = false;
                                switch (independentEstimation)
                                {
                                    case IndependenceEstimation.Probability:
                                        isAWitness = labelPair.IsAWitnessByProbability(witness, group, thresholdOfIndependentForNetwork);
                                        break;
                                    case IndependenceEstimation.MutualInformation:
                                        isAWitness = labelPair.IsAWitnessByMI(witness, group, thresholdOfIndependentForNetwork);
                                        break;
                                }
                                if (isAWitness)
                                {
                                    labelPairAndWitness.Add(labelPair, witness);
                                    H.AdjMatrix[labelPair] = false;
                                    H.AdjMatrix[labelPair.Reverse] = false;
                                    break;
                                }
                            }
                        }
                    }
                }
                ++i;
            }
            #endregion
            #region Mark-Immoralities
            bool isChanged = true;
            while (isChanged)
            {
                isChanged = false;
                foreach (Label Xi in labelArray)
                {
                    foreach (Label Xj in labelArray)
                    {
                        foreach (Label Xk in labelArray)
                        {
                            if (Xi != Xj && Xj != Xk && Xi != Xk)//三个情感两两不等
                            {
                                if (H.AdjMatrix[new LabelPair(Xi, Xj)] && H.AdjMatrix[new LabelPair(Xj, Xi)] &&
                                    H.AdjMatrix[new LabelPair(Xj, Xk)] && H.AdjMatrix[new LabelPair(Xk, Xj)] &&
                                    !H.AdjMatrix[new LabelPair(Xi, Xk)] && !H.AdjMatrix[new LabelPair(Xk, Xi)])
                                {
                                    if ((labelPairAndWitness.ContainsKey(new LabelPair(Xi, Xk)) && !labelPairAndWitness[new LabelPair(Xi, Xk)].Contains(Xj)))
                                    {
                                        H.AdjMatrix[new LabelPair(Xj, Xi)] = false;
                                        H.AdjMatrix[new LabelPair(Xj, Xk)] = false;
                                        isChanged = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            #region Build-PDAG
            bool isConverged = false;
            while (!isConverged)
            {
                //R1
                bool foundR1 = false;
                foreach (Label X in labelArray)
                {
                    foreach (Label Y in labelArray)
                    {
                        foreach (Label Z in labelArray)
                        {
                            if (X != Y && X != Z && Y != Z)
                            {
                                if (H.AdjMatrix[new LabelPair(X, Y)] && !H.AdjMatrix[new LabelPair(Y, X)] &&
                                    H.AdjMatrix[new LabelPair(Y, Z)] && H.AdjMatrix[new LabelPair(Z, Y)] &&
                                    !H.AdjMatrix[new LabelPair(X, Z)] && !H.AdjMatrix[new LabelPair(Z, X)])
                                {
                                    H.AdjMatrix[new LabelPair(Z, Y)] = false;
                                    foundR1 = true;
                                }
                            }
                        }
                    }
                }
                //R2
                bool foundR2 = false;
                foreach (Label X in labelArray)
                {
                    foreach (Label Y in labelArray)
                    {
                        foreach (Label Z in labelArray)
                        {
                            if (X != Y && X != Z && Y != Z)
                            {
                                if (H.AdjMatrix[new LabelPair(X, Y)] && !H.AdjMatrix[new LabelPair(Y, X)] &&
                                    H.AdjMatrix[new LabelPair(Y, Z)] && !H.AdjMatrix[new LabelPair(Z, Y)] &&
                                    H.AdjMatrix[new LabelPair(X, Z)] && H.AdjMatrix[new LabelPair(Z, X)])
                                {
                                    H.AdjMatrix[new LabelPair(Z, X)] = false;
                                    foundR2 = true;
                                }
                            }
                        }
                    }
                }
                //R3
                bool foundR3 = false;
                foreach (Label X in labelArray)
                {
                    foreach (Label Y1 in labelArray)
                    {
                        foreach (Label Y2 in labelArray)
                        {
                            foreach (Label Z in labelArray)
                            {
                                if (X != Y1 && X != Y2 && X != Z && Y1 != Y2 && Y1 != Z && Y2 != Z)
                                {
                                    if (H.AdjMatrix[new LabelPair(X, Y1)] && H.AdjMatrix[new LabelPair(Y1, X)] &&
                                        H.AdjMatrix[new LabelPair(X, Y2)] && H.AdjMatrix[new LabelPair(Y2, X)] &&
                                        H.AdjMatrix[new LabelPair(X, Z)] && H.AdjMatrix[new LabelPair(Z, X)] &&
                                        H.AdjMatrix[new LabelPair(Y1, Z)] && !H.AdjMatrix[new LabelPair(Z, Y1)] &&
                                        H.AdjMatrix[new LabelPair(Y2, Z)] && !H.AdjMatrix[new LabelPair(Z, Y2)])
                                    {
                                        H.AdjMatrix[new LabelPair(Z, X)] = false;
                                        foundR3 = true;
                                    }
                                }
                            }
                        }
                    }
                }
                //是否Converged
                if (!foundR1 && !foundR2 && !foundR3)
                {
                    isConverged = true;
                }
            }
            #endregion
            #region Complete-BN
            foreach (Label X in labelArray)
            {
                foreach (Label Y in labelArray)
                {
                    if (X != Y)
                    {
                        if (H.AdjMatrix[new LabelPair(X, Y)] && H.AdjMatrix[new LabelPair(Y, X)])
                            H.AdjMatrix[new LabelPair(Y, X)] = false;
                    }
                }
            }
            #endregion
            return H;
        }

        static private Graph BuildBNOld(int group, double thresholdOfIndependentForNetwork)//废弃：证据不唯一
        {
            #region Build-PMap-Skeleton
            Graph H = new Graph();
            IList<LabelPair> labelPairs = new List<LabelPair>();
            IList<Label> traversedLabels = new List<Label>();
            foreach (Label label1 in Variable.LabelArray)
            {
                traversedLabels.Add(label1);
                foreach (Label label2 in Variable.LabelArray)
                {
                    if (!traversedLabels.Contains(label2))
                        labelPairs.Add(new LabelPair(label1, label2));
                }
            }
            IDictionary<LabelPair, IList<IList<Label>>> labelPairAndWitnesses = new Dictionary<LabelPair, IList<IList<Label>>>();
            foreach (LabelPair labelPair in labelPairs)
            {
                labelPairAndWitnesses.Add(labelPair, new List<IList<Label>>());
                foreach (List<Label> witness in H.GetWitnesses(labelPair))
                {
                    if (labelPair.IsAWitnessByProbability(witness, group, thresholdOfIndependentForNetwork))
                    {
                        labelPairAndWitnesses[labelPair].Add(witness);
                        H.AdjMatrix[labelPair] = false;
                        H.AdjMatrix[labelPair.Reverse] = false;
                    }
                }
            }
            #endregion
            #region Mark-Immoralities
            traversedLabels.Clear();
            foreach (Label Xi in Variable.LabelArray)
            {
                traversedLabels.Add(Xi);
                foreach (Label Xj in Variable.LabelArray)
                {
                    foreach (Label Xk in Variable.LabelArray)
                    {
                        if (Xi != Xj && Xj != Xk && !traversedLabels.Contains(Xk))//三个情感两两不等
                        {
                            if (H.AdjMatrix[new LabelPair(Xi, Xj)] && H.AdjMatrix[new LabelPair(Xj, Xi)] &&
                                H.AdjMatrix[new LabelPair(Xj, Xk)] && H.AdjMatrix[new LabelPair(Xk, Xj)] &&
                                !H.AdjMatrix[new LabelPair(Xi, Xk)] && !H.AdjMatrix[new LabelPair(Xk, Xi)])
                            {
                                bool witnessContainsXj = false;
                                foreach (List<Label> witness in labelPairAndWitnesses[new LabelPair(Xi, Xk)])
                                {
                                    if (witness.Contains(Xj))
                                    {
                                        witnessContainsXj = true;
                                        break;
                                    }
                                }
                                if (!witnessContainsXj)
                                {
                                    H.AdjMatrix[new LabelPair(Xj, Xi)] = false;
                                    H.AdjMatrix[new LabelPair(Xj, Xk)] = false;
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            #region Build-PDAG
            bool isConverged = false;//出现环：happiness, sadness, anger
            while (!isConverged)
            {
                //R1
                bool foundR1 = false;
                foreach (Label X in Variable.LabelArray)
                {
                    foreach (Label Y in Variable.LabelArray)
                    {
                        foreach (Label Z in Variable.LabelArray)
                        {
                            if (X != Y && X != Z && Y != Z)
                            {
                                if (H.AdjMatrix[new LabelPair(X, Y)] && !H.AdjMatrix[new LabelPair(Y, X)] &&
                                    H.AdjMatrix[new LabelPair(Y, Z)] && H.AdjMatrix[new LabelPair(Z, Y)] &&
                                    !H.AdjMatrix[new LabelPair(X, Z)] && !H.AdjMatrix[new LabelPair(Z, X)])
                                {
                                    H.AdjMatrix[new LabelPair(Z, Y)] = false;
                                    foundR1 = true;
                                }
                            }
                        }
                    }
                }
                //R2
                bool foundR2 = false;
                foreach (Label X in Variable.LabelArray)
                {
                    foreach (Label Y in Variable.LabelArray)
                    {
                        foreach (Label Z in Variable.LabelArray)
                        {
                            if (X != Y && X != Z && Y != Z)
                            {
                                if (H.AdjMatrix[new LabelPair(X, Y)] && !H.AdjMatrix[new LabelPair(Y, X)] &&
                                    H.AdjMatrix[new LabelPair(Y, Z)] && !H.AdjMatrix[new LabelPair(Z, Y)] &&
                                    H.AdjMatrix[new LabelPair(X, Z)] && H.AdjMatrix[new LabelPair(Z, X)])
                                {
                                    H.AdjMatrix[new LabelPair(Z, X)] = false;
                                    foundR2 = true;
                                }
                            }
                        }
                    }
                }
                //R3
                bool foundR3 = false;
                foreach (Label X in Variable.LabelArray)
                {
                    foreach (Label Y1 in Variable.LabelArray)
                    {
                        foreach (Label Y2 in Variable.LabelArray)
                        {
                            foreach (Label Z in Variable.LabelArray)
                            {
                                if (X != Y1 && X != Y2 && X != Z && Y1 != Y2 && Y1 != Z && Y2 != Z)
                                {
                                    if (H.AdjMatrix[new LabelPair(X, Y1)] && H.AdjMatrix[new LabelPair(Y1, X)] &&
                                        H.AdjMatrix[new LabelPair(X, Y2)] && H.AdjMatrix[new LabelPair(Y2, X)] &&
                                        H.AdjMatrix[new LabelPair(X, Z)] && H.AdjMatrix[new LabelPair(Z, X)] &&
                                        H.AdjMatrix[new LabelPair(Y1, Z)] && !H.AdjMatrix[new LabelPair(Z, Y1)] &&
                                        H.AdjMatrix[new LabelPair(Y2, Z)] && !H.AdjMatrix[new LabelPair(Z, Y2)])
                                    {
                                        H.AdjMatrix[new LabelPair(Z, X)] = false;
                                        foundR3 = true;
                                    }
                                }
                            }
                        }
                    }
                }
                //是否Converged
                if (!foundR1 && !foundR2 && !foundR3)
                {
                    isConverged = true;
                }
            }
            #endregion
            #region Complete-BN
            foreach (Label X in Variable.LabelArray)
            {
                foreach (Label Y in Variable.LabelArray)
                {
                    if (X != Y)
                    {
                        if (H.AdjMatrix[new LabelPair(X, Y)] && H.AdjMatrix[new LabelPair(Y, X)])
                        {
                            H.AdjMatrix[new LabelPair(Y, X)] = false;
                        }
                    }
                }
            }
            #endregion
            return H;
        }
    }
}
