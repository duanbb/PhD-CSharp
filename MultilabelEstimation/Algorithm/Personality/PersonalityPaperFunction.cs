using MultilabelEstimation.Consistency;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MultilabelEstimation.Algorithm.Personality
{
    enum CevioType
    {
        spirit, anger, sadness
    }

    static class PersonalityPaperFunction
    {
        static public double AccuracyOfPersonalityForEachGroup(IDictionary<Character, Tuple<Will, string>> truePersonality, IDictionary<Character, Tuple<Will, string>> estimatedPersonality)
        {
            double accuracy = 0;
            foreach (Character character in truePersonality.Keys)
            {
                if (truePersonality[character].Item1 == estimatedPersonality[character].Item1)
                    ++accuracy;
            }
            return accuracy / truePersonality.Count;
        }

        static public double AccuracyOfPersonality(double[] accuracies)
        {
            double accuracyForAllGroups = 0;
            foreach (double accuracy in accuracies)
            {
                accuracyForAllGroups += accuracy;
            }
            return accuracyForAllGroups /= accuracies.Length;
        }

        static public void CevioGold()
        {
            IDictionary<Sentence, IDictionary<CevioType, double>> CeivoGold = new Dictionary<Sentence, IDictionary<CevioType, double>>();
            //double spiritForAll = 0;
            //int count = 0;
            foreach (Sentence sentence in Variable.Sentences)
            {
                CeivoGold.Add(sentence, new Dictionary<CevioType, double>());
                double spirit = 4;
                double anger = 0;
                double sadness = 0;
                foreach (Annotator annotator in Variable.Annotators)
                {
                    if (Variable.Data[annotator].ContainsKey(sentence))
                    {
                        foreach (Annotation annotation in Variable.Data[annotator][sentence])
                        {
                            //++count;
                            foreach (Label label in annotation.Labels.Keys)
                            {
                                if (annotation.Labels[label])
                                {
                                    if (PersonalityVariable.StrongAffects.Contains(label))
                                    {
                                        ++spirit;
                                        //++spiritForAll;
                                    }
                                    else if (PersonalityVariable.WeakAffects.Contains(label))
                                    {
                                        --spirit;
                                        //--spiritForAll;
                                    }
                                    else if (label == Label.anger)
                                        anger += 1;
                                    else//(label == Label.sadness)
                                        sadness += 2;
                                }
                            }
                        }
                    }
                }
                double total = spirit + anger + sadness;
                CeivoGold[sentence].Add(CevioType.spirit, spirit /= total);
                CeivoGold[sentence].Add(CevioType.anger, anger /= total);
                CeivoGold[sentence].Add(CevioType.sadness, sadness /= total);
            }

            #region Output Numeric Gold
            StreamWriter resultFile = new StreamWriter("Result/CeivoGold.csv",false, Encoding.UTF8);
            resultFile.WriteLine("sentence, spirit, anger, sadness");
            foreach (Sentence sentence in Variable.Sentences)
            {
                string result = sentence.ID + "," + CeivoGold[sentence][CevioType.spirit] + "," + CeivoGold[sentence][CevioType.anger] + "," + CeivoGold[sentence][CevioType.sadness] + "," + sentence.Speech;
                resultFile.Write(result);
                resultFile.WriteLine();
            }
            resultFile.Close();
            #endregion
        }

        static public void TransGoldstandardsToCevio()//未完成
        {
            IDictionary<Sentence, IDictionary<CevioType, double>> CeivoGold = new Dictionary<Sentence, IDictionary<CevioType, double>>();
            foreach (Sentence sentence in Variable.Sentences)
            {
                CeivoGold.Add(sentence, new Dictionary<CevioType, double>());
                double spirit = 4;
                double anger = 0;
                double sadness = 0;
                foreach (Label label in Variable.LabelArray)
                {
                    if (sentence.BinaryGold.Labels[label])
                    {
                        if (PersonalityVariable.StrongAffects.Contains(label))
                            ++spirit;
                        else if (PersonalityVariable.WeakAffects.Contains(label))
                            --spirit;
                        else if (label == Label.anger)
                            anger = 4;
                        else if (label == Label.sadness)
                            sadness = 4;
                        else
                        { }
                    }
                }
                double total = spirit + anger + sadness;
                CeivoGold[sentence].Add(CevioType.spirit, spirit /= total);
                CeivoGold[sentence].Add(CevioType.anger, anger /= total);
                CeivoGold[sentence].Add(CevioType.sadness, sadness /= total);
            }
        }
    }
}