using MultilabelEstimation.Group;
using System;
using System.Collections.Generic;

namespace MultilabelEstimation.Algorithm.DDS.IDDS
{
    static class IDDSFunction
    {
        static public void RunIDDS()
        {
            for (int groupIndex = 0; groupIndex < GroupVariable.AnnotatorGroups.Length; ++groupIndex)
            {
                Sij sij = Initialize(groupIndex);
                CoreFunction.Intgerate(Variable.LabelArray, groupIndex, ref sij);
                DDSFunction.ObtainBinaryResult(sij, "IDDS", groupIndex);
                Function.WriteBinaryResultFile("IDDS", groupIndex);
            }
        }

        static private Sij Initialize(int groupIndex)
        {
            Sij sij = new Sij(1);
            IDictionary<Label, double> labelFloatDic = new Dictionary<Label, double>();
            foreach (Label label in Variable.LabelArray)
            {
                labelFloatDic.Add(label, 0);
            }
            IDictionary<Sentence, IDictionary<Label, double>> ProbabilityOfLabelTrue = new Dictionary<Sentence, IDictionary<Label, double>>();
            IDictionary<Sentence, IDictionary<Label, double>> ProbabilityOfLabelFalse = new Dictionary<Sentence, IDictionary<Label, double>>();
            foreach (Sentence sentence in Variable.Sentences)
            {
                ProbabilityOfLabelTrue.Add(sentence, new Dictionary<Label, double>(labelFloatDic));
                ProbabilityOfLabelFalse.Add(sentence, new Dictionary<Label, double>(labelFloatDic));
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (Annotation annotation in sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Values)
                {
                    foreach (Label label in Variable.LabelArray)
                    {
                        if (annotation.Labels[label])
                            ++ProbabilityOfLabelTrue[sentence][label];
                        else
                            ++ProbabilityOfLabelFalse[sentence][label];
                    }
                }
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (Label label in Variable.LabelArray)
                {
                    ProbabilityOfLabelTrue[sentence][label] /= Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                    ProbabilityOfLabelFalse[sentence][label] /= Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                }
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                sij.Value.Add(sentence, new Dictionary<Labelset, double>());
                for (int l = 0; l < Math.Pow(2, Variable.LabelArray.Length); ++l)
                {
                    Labelset Labelset = new Labelset(Variable.LabelArray, l);
                    double value = 1;
                    foreach (Label label in Labelset.Labels.Keys)
                    {
                        if (Labelset.Labels[label])
                            value *= ProbabilityOfLabelTrue[sentence][label];
                        else
                            value *= ProbabilityOfLabelFalse[sentence][label];
                    }
                    if (value != 0)
                        sij.Value[sentence].Add(Labelset, value);
                }
            }
            return sij;
        }
    }
}