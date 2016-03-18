using System;
using System.Collections.Generic;
using MultilabelEstimation.Supervised;
using MultilabelEstimation.Group;

namespace MultilabelEstimation.Algorithm.DDS
{
    static class DDSFunction
    {
        static public void ObtainBinaryResult(Sij sij, string algorithm, int groupIndex)
        {
            switch (algorithm)
            {
                case "JDDS":
                    foreach (Sentence sentence in Variable.Sentences)
                        sentence.AnnotaitonGroups[groupIndex].JDDSResult = new Result(sij.CalculateJointBestLabelset(sentence));
                    break;
                case "SDDS":
                    foreach (Sentence sentence in Variable.Sentences)
                    {
                        if (sentence.ID < SupervisedVariable.NumberOfTraningSentences) continue;
                        sentence.AnnotaitonGroups[groupIndex].SDDSResult = new Result(sij.CalculateJointBestLabelset(sentence));
                    }
                    break;
                case "IDDS":
                    foreach (Sentence sentence in Variable.Sentences)
                        sentence.AnnotaitonGroups[groupIndex].IDDSResult = new Result(sij.CalculateJointBestLabelset(sentence));
                    break;
                case "DTDDS":
                    foreach (Sentence sentence in Variable.Sentences)
                        sentence.AnnotaitonGroups[groupIndex].DTDDSResult = new Result(sij.CalculateJointBestLabelset(sentence));
                    break;
                case "TDDS":
                    foreach (Sentence sentence in Variable.Sentences)
                        sentence.AnnotaitonGroups[groupIndex].TDDSResult = new Result(sij.CalculateJointBestLabelset(sentence));
                    break;
                case "NDDS":
                    foreach (Sentence sentence in Variable.Sentences)
                        sentence.AnnotaitonGroups[groupIndex].NDDSResult = new Result(sij.CalculateJointBestLabelset(sentence));
                    break;
                case "PeT":
                    foreach (Sentence sentence in Variable.Sentences)
                        sentence.AnnotaitonGroups[groupIndex].PeTResult = new Result(sij.CalculateJointBestLabelset(sentence));
                    break;
            }
        }

        static public void ObtainNumericResult(Sij sij, string algorithm, int group)
        {
            switch (algorithm)
            {
                case "JDDS":
                    foreach (Sentence sentence in sij.Value.Keys)
                    {
                        sentence.AnnotaitonGroups[group].JDDSNumResult = new NumericResult();
                        foreach (Labelset Labelset in sij.Value[sentence].Keys)
                        {
                            foreach (Label label in Variable.LabelArray)
                            {
                                if (Labelset.Labels[label])
                                    sentence.AnnotaitonGroups[group].JDDSNumResult.Labels[label] += sij.Value[sentence][Labelset];
                            }
                        }
                    }
                    break;
                case "DTDDS":
                    foreach (Sentence sentence in sij.Value.Keys)
                    {
                        sentence.AnnotaitonGroups[group].DTDDSNumResult = new NumericResult();
                        foreach (Labelset Labelset in sij.Value[sentence].Keys)
                        {
                            foreach (Label label in Variable.LabelArray)
                            {
                                if (Labelset.Labels[label])
                                    sentence.AnnotaitonGroups[group].DTDDSNumResult.Labels[label] += sij.Value[sentence][Labelset];
                            }
                        }
                    }
                    break;
                case "TDDS":
                    foreach (Sentence sentence in sij.Value.Keys)
                    {
                        sentence.AnnotaitonGroups[group].TDDSNumResult = new NumericResult();
                        foreach (Labelset Labelset in sij.Value[sentence].Keys)
                        {
                            foreach (Label label in Variable.LabelArray)
                            {
                                if (Labelset.Labels[label])
                                    sentence.AnnotaitonGroups[group].TDDSNumResult.Labels[label] += sij.Value[sentence][Labelset];
                            }
                        }
                    }
                    break;
                case "SDDS":
                    foreach (Sentence sentence in sij.Value.Keys)
                    {
                        sentence.AnnotaitonGroups[group].SDDSNumResult = new NumericResult();
                        foreach (Labelset Labelset in sij.Value[sentence].Keys)
                        {
                            foreach (Label label in Variable.LabelArray)
                            {
                                if (Labelset.Labels[label])
                                    sentence.AnnotaitonGroups[group].SDDSNumResult.Labels[label] += sij.Value[sentence][Labelset];
                            }
                        }
                    }
                    break;
            }
        }
    }
}