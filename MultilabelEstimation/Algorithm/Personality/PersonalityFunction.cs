using MultilabelEstimation.Consistency;
using MultilabelEstimation.Group;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MultilabelEstimation.Algorithm.Personality
{
    static class PersonalityFunction
    {
        static public BEkef CalculateBExy(Mce mce, IDictionary<Annotator, IDictionary<Character, IDictionary<Will, double>>> okxc, Smoothing smoothingBE, int time, int groupIndex)
        {
            BEkef bexy = new BEkef(time);
            IDictionary<Annotator, IDictionary<Will, double>> denominator = new Dictionary<Annotator, IDictionary<Will, double>>();
            IDictionary<Smoothing, double[]> smoothingNumber = Function.SmoothingNumber(2);
            foreach (Annotator annotator in GroupVariable.AnnotatorGroups[groupIndex])
            {
                //初始化
                bexy.Value.Add(annotator, new Dictionary<Tuple<Will, Will>, double>());
                denominator.Add(annotator, new Dictionary<Will, double>());
                bexy.Value[annotator].Add(Tuple.Create(Will.strong, Will.strong), 0);
                bexy.Value[annotator].Add(Tuple.Create(Will.strong, Will.weak), 0);
                bexy.Value[annotator].Add(Tuple.Create(Will.weak, Will.strong), 0);
                bexy.Value[annotator].Add(Tuple.Create(Will.weak, Will.weak), 0);
                denominator[annotator].Add(Will.strong, 0);
                denominator[annotator].Add(Will.weak, 0);
                //计算分子分母
                foreach (Character character in mce.Value.Keys)
                {
                    if (okxc[annotator].ContainsKey(character))
                    {
                        double strongstrong = mce.Value[character][Will.strong] * okxc[annotator][character][Will.strong];//TODO调试观察是不是每次迭代都不变
                        double strongweak = mce.Value[character][Will.strong] * okxc[annotator][character][Will.weak];
                        bexy.Value[annotator][Tuple.Create(Will.strong, Will.strong)] += strongstrong;
                        bexy.Value[annotator][Tuple.Create(Will.strong, Will.weak)] += strongweak;
                        //denominator[annotator][Will.strong] += strongstrong + strongweak;
                        denominator[annotator][Will.strong] += mce.Value[character][Will.strong];
                        double weakstrong = mce.Value[character][Will.weak] * okxc[annotator][character][Will.strong];
                        double weakweak = mce.Value[character][Will.weak] * okxc[annotator][character][Will.weak];
                        bexy.Value[annotator][Tuple.Create(Will.weak, Will.strong)] += weakstrong;
                        bexy.Value[annotator][Tuple.Create(Will.weak, Will.weak)] += weakweak;
                        //denominator[annotator][Will.weak] += weakstrong + weakweak;
                        denominator[annotator][Will.weak] += mce.Value[character][Will.weak];
                    }
                }
                //计算最终结果
                if (denominator[annotator][Will.strong] != 0)
                {
                    if (smoothingBE != Smoothing.None)
                    {
                        bexy.Value[annotator][Tuple.Create(Will.strong, Will.strong)] = (bexy.Value[annotator][Tuple.Create(Will.strong, Will.strong)] + smoothingNumber[Variable.SmoothPajl][0]) / (denominator[annotator][Will.strong] + smoothingNumber[Variable.SmoothPajl][1]);
                        bexy.Value[annotator][Tuple.Create(Will.strong, Will.weak)] = (bexy.Value[annotator][Tuple.Create(Will.strong, Will.weak)] + smoothingNumber[Variable.SmoothPajl][0]) / (denominator[annotator][Will.strong] + smoothingNumber[Variable.SmoothPajl][1]);
                    }
                    else
                    {
                        bexy.Value[annotator][Tuple.Create(Will.strong, Will.strong)] /= denominator[annotator][Will.strong];
                        bexy.Value[annotator][Tuple.Create(Will.strong, Will.weak)] /= denominator[annotator][Will.strong];
                    }
                }
                else
                {
                    bexy.Value[annotator][Tuple.Create(Will.strong, Will.strong)] = 1;

                    //bexy.Value[annotator][Tuple.Create(Will.strong, Will.strong)] = 0.5;
                    //bexy.Value[annotator][Tuple.Create(Will.strong, Will.weak)] = 0.5;
                }
                if (denominator[annotator][Will.weak] != 0)
                {
                    if (smoothingBE != Smoothing.None)
                    {
                        bexy.Value[annotator][Tuple.Create(Will.weak, Will.strong)] = (bexy.Value[annotator][Tuple.Create(Will.weak, Will.strong)] + smoothingNumber[Variable.SmoothPajl][0]) / (denominator[annotator][Will.weak] + smoothingNumber[Variable.SmoothPajl][1]);
                        bexy.Value[annotator][Tuple.Create(Will.weak, Will.weak)] = (bexy.Value[annotator][Tuple.Create(Will.weak, Will.weak)] + smoothingNumber[Variable.SmoothPajl][0]) / (denominator[annotator][Will.weak] + smoothingNumber[Variable.SmoothPajl][1]);
                    }
                    else
                    {
                        bexy.Value[annotator][Tuple.Create(Will.weak, Will.strong)] /= denominator[annotator][Will.weak];
                        bexy.Value[annotator][Tuple.Create(Will.weak, Will.weak)] /= denominator[annotator][Will.weak];
                    }
                }
                else
                {
                    bexy.Value[annotator][Tuple.Create(Will.weak, Will.weak)] = 1;

                    //bexy.Value[annotator][Tuple.Create(Will.weak, Will.strong)] = 0.5;
                    //bexy.Value[annotator][Tuple.Create(Will.weak, Will.weak)] = 0.5;
                }
            }
            return bexy;
        }

        static public IDictionary<Annotator, IDictionary<Character, IDictionary<Will, double>>> CalculateOkcx(int groupIndex)
        {
            IDictionary<Annotator, IDictionary<Character, IDictionary<Will, double>>> Okcx = new Dictionary<Annotator, IDictionary<Character, IDictionary<Will, double>>>();
            foreach (Annotator annotator in GroupVariable.AnnotatorGroups[groupIndex])
            {
                Okcx.Add(annotator, new Dictionary<Character, IDictionary<Will, double>>());
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                Character character = sentence.Character;
                foreach (Annotator annotator in sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Keys)
                {
                    if (Okcx[annotator].ContainsKey(character))
                    {
                        Okcx[annotator][character][Will.strong] += sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic[annotator].ToLabelset(Variable.LabelArray).NumberOfTrueStrongAffects;
                        Okcx[annotator][character][Will.weak] += sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic[annotator].ToLabelset(Variable.LabelArray).NumberOfTrueWeakAffects;
                    }
                    else
                    {
                        Okcx[annotator].Add(character, new Dictionary<Will, double>());
                        Okcx[annotator][character].Add(Will.strong, sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic[annotator].ToLabelset(Variable.LabelArray).NumberOfTrueStrongAffects);
                        Okcx[annotator][character].Add(Will.weak, sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic[annotator].ToLabelset(Variable.LabelArray).NumberOfTrueWeakAffects);
                    }
                }
            }
            foreach (Annotator annotator in GroupVariable.AnnotatorGroups[groupIndex])
            {
                foreach (Character character in Okcx[annotator].Keys)
                {
                    double numberOfWillAffects = Okcx[annotator][character][Will.strong] + Okcx[annotator][character][Will.weak];
                    if (numberOfWillAffects != 0)
                    {
                        Okcx[annotator][character][Will.strong] /= numberOfWillAffects;
                        Okcx[annotator][character][Will.weak] /= numberOfWillAffects;
                    }
                    else
                    {
                        Okcx[annotator][character][Will.strong] = 0.5;
                        Okcx[annotator][character][Will.weak] = 0.5;
                    }
                }
            }
            return Okcx;
        }

        //转换gold
        static public void TransGoldstandardsForPersonality()
        {
            foreach (Sentence sentence in Variable.Sentences)
            {
                sentence.BinaryGold.TransToPersonalityResult(PersonalityVariable.TruePersonality[sentence.Character].Item1);
            }
            Function.WriteGoldToFile("Personality");
        }

        static public void WriteMVResultFile(Mce mce, int groupIndex)
        {
            foreach (Sentence sentence in Variable.Sentences)
            {
                sentence.AnnotaitonGroups[groupIndex].PeMVResult = new Result(sentence.AnnotaitonGroups[groupIndex].MVResult);
                sentence.AnnotaitonGroups[groupIndex].PeMVResult.TransToPersonalityResult(mce.EstimatedPersonality[sentence.Character].Item1);
            }
            if (Variable.OutputResult)
            {
                StreamWriter resultFile = new StreamWriter("Result/" + Variable.NumberOfAnnotationsPerSentenceAfterGrouping + "PeMV" + "Binary" + groupIndex + ".csv", false, Encoding.Default);
                Function.InitialResultFile(resultFile);
                double averageTrueLabelsPerResult = 0;
                foreach (Sentence sentence in Variable.Sentences)
                {
                    Function.WriteBinaryResultOfASentence(sentence.ID, sentence.AnnotaitonGroups[groupIndex].PeMVResult,
                        sentence.Character.ID + ","
                        + " strongInM:" + mce.Value[sentence.Character][Will.strong] + " weakInM:" + mce.Value[sentence.Character][Will.weak] + ",",
                        sentence.Speech, resultFile);
                    averageTrueLabelsPerResult += sentence.AnnotaitonGroups[groupIndex].PeMVResult.NumberOfTrueLabel;
                }
                resultFile.Write("Average true labels per annotatin," + averageTrueLabelsPerResult / Variable.Sentences.Count + ",");
                resultFile.Close();
            }
        }

        //计算角色个性
        static public IDictionary<Character, Tuple<Will, string>> GetGoldOfPersonality()
        {
            IDictionary<Character, Tuple<Will, string>> characterPersonalities = new Dictionary<Character, Tuple<Will, string>>();
            foreach (Character character in ConsistencyVariable.Characters)
            {
                double numberOfTrueStrongAffects = 0;
                double numberOfTrueWeakAffects = 0;
                foreach (Sentence sentence in character.Sentences)
                {
                    foreach (Annotator annotator in Variable.Annotators)
                    {
                        if (Variable.Data[annotator].ContainsKey(sentence))
                        {
                            foreach (Annotation annotation in Variable.Data[annotator][sentence])
                            {
                                Labelset labelset = annotation.ToLabelset(Variable.LabelArray);
                                numberOfTrueStrongAffects += labelset.NumberOfTrueStrongAffects;
                                numberOfTrueWeakAffects += labelset.NumberOfTrueWeakAffects;
                            }
                        }
                    }
                }
                characterPersonalities.Add(character, Tuple.Create(numberOfTrueStrongAffects >= numberOfTrueWeakAffects ? Will.strong : Will.weak, "s:" + numberOfTrueStrongAffects + "; w:" + numberOfTrueWeakAffects));
            }
            return characterPersonalities;
        }
    }
}