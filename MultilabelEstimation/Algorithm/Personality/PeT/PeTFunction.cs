using MultilabelEstimation.Algorithm.DDS;
using MultilabelEstimation.Consistency;
using MultilabelEstimation.Group;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MultilabelEstimation.Algorithm.DDS.NDDS;

namespace MultilabelEstimation.Algorithm.Personality.PeT
{
    static class PeTFunction
    {
        static public void RunPeT(PorSForJointje PorS, Smoothing SmoothingBE, BnOrNot bnOrNot)
        {
            double[] accuracyOfPersonalityForEachGroup = new double[GroupVariable.AnnotatorGroups.Length];
            for (int groupIndex = 0; groupIndex < GroupVariable.AnnotatorGroups.Length; ++groupIndex)
            {
                IDictionary<Annotator, IDictionary<Character, IDictionary<Will, double>>> okcx = PersonalityFunction.CalculateOkcx(groupIndex);//模拟人对角色个性的标注，计算一次就不变了
                Mce mce = null;
                Sij sij = null;
                if (bnOrNot == BnOrNot.Yes) sij = NDDSFunction.Initialize(groupIndex, Math.Pow(10, -1), IndependenceEstimation.MutualInformation);
                else sij = CoreFunction.InitializeSij(Variable.LabelArray, groupIndex);
                Pje pje = null;//p(t|e)
                Pdata pdata = null;
                IList<double> Pdatas = new List<double>();
                for (int convergeTime = 1; convergeTime <= Variable.ConvergeTimeThreshold; ++convergeTime)
                {
                    mce = CalculateMce(sij, groupIndex);
                    PersonalityFunction.WriteMVResultFile(mce, groupIndex);
                    PAkjl pakjl = CoreFunction.CalculatePAkjl(Variable.LabelArray, sij, convergeTime, groupIndex);
                    BEkef bekef = PersonalityFunction.CalculateBExy(mce, okcx, SmoothingBE, convergeTime, groupIndex);
                    if (PorS == PorSForJointje.P) pje = CalculatePje(sij, convergeTime);
                    if (CalculatePdataAndSij(ref sij, pakjl, bekef, pje, mce, okcx, ref pdata, Pdatas, groupIndex))//old/new
                        break;
                }
                DDSFunction.ObtainBinaryResult(sij, "PeT", groupIndex);
                Function.WriteBinaryResultFile("PeT", groupIndex);
                accuracyOfPersonalityForEachGroup[groupIndex] = PersonalityPaperFunction.AccuracyOfPersonalityForEachGroup(PersonalityVariable.TruePersonality, mce.EstimatedPersonality);
            }
            Function.ConsoleWriteLine("Accuracy Of PeT: " + PersonalityPaperFunction.AccuracyOfPersonality(accuracyOfPersonalityForEachGroup));
        }

        static private bool CalculatePdataAndSij(ref Sij sij, PAkjl pakjl, BEkef bekef, Pje pje, Mce mce, IDictionary<Annotator, IDictionary<Character, IDictionary<Will, double>>> okxc, ref Pdata pdata, IList<double> pdatas, int groupIndex)
        {
            bool isFinished = false;
            //sij的分子
            IDictionary<Sentence, IDictionary<Labelset, IDictionary<Will, double>>> numerator = new Dictionary<Sentence, IDictionary<Labelset, IDictionary<Will, double>>>();
            //sij的分母（P(data on i)）
            IDictionary<Sentence, IDictionary<Will, double>> denominator = new Dictionary<Sentence, IDictionary<Will, double>>();
            //计算分子
            foreach (Sentence sentence in Variable.Sentences)
            {
                numerator.Add(sentence, new Dictionary<Labelset, IDictionary<Will, double>>());

                #region 联合概率P(t,e)
                IDictionary<Labelset, IDictionary<Will, double>> jointje;
                if (pje == null)//PorS == PorSForJointje.S
                {
                    //求后验概率P(t|e)，新增
                    jointje = new Dictionary<Labelset, IDictionary<Will, double>>();
                    IDictionary<Will, double> willDenominator = new Dictionary<Will, double>();
                    willDenominator.Add(Will.strong, 0);
                    willDenominator.Add(Will.weak, 0);
                    foreach (Labelset labelset in sij.Value[sentence].Keys)
                    {
                        double valueOfStrong = sij.Value[sentence][labelset] * labelset.HowStrong;
                        double valueOfWeak = sij.Value[sentence][labelset] * labelset.HowWeak;
                        if (jointje.ContainsKey(labelset))
                        {
                            jointje[labelset][Will.strong] += valueOfStrong;
                            jointje[labelset][Will.weak] += valueOfWeak;
                        }
                        else
                        {
                            jointje.Add(labelset, new Dictionary<Will, double>());
                            jointje[labelset].Add(Will.strong, valueOfStrong);
                            jointje[labelset].Add(Will.weak, valueOfWeak);
                        }
                        willDenominator[Will.strong] += valueOfStrong;
                        willDenominator[Will.weak] += valueOfWeak;
                    }
                    //p(t|e)
                    foreach (Labelset labelset in jointje.Keys.ToArray())
                    {
                        if (willDenominator[Will.strong] != 0)
                        {
                            jointje[labelset][Will.strong] /= willDenominator[Will.strong];
                        }
                        if (willDenominator[Will.weak] != 0)
                        {
                            jointje[labelset][Will.weak] /= willDenominator[Will.weak];
                        }
                    }
                    //p(t|e)*p(x)
                    if (willDenominator[Will.strong] != 0 && willDenominator[Will.weak] != 0)//有一个等于0就不用再算了
                    {
                        foreach (Labelset labelset in jointje.Keys.ToArray())
                        {
                            jointje[labelset][Will.strong] *= mce.Value[sentence.Character][Will.strong];
                            jointje[labelset][Will.weak] *= mce.Value[sentence.Character][Will.weak];
                        }
                    }
                }
                else//PorS == PorSForJointje.P
                {
                    jointje = pje.Value;
                    double valueOfStrong = 0;
                    double valueOfWeak = 0;
                    foreach (Labelset labelset in jointje.Keys.ToArray())
                    {
                        valueOfStrong += jointje[labelset][Will.strong];
                        valueOfWeak += jointje[labelset][Will.weak];
                    }
                    if (valueOfStrong != 0 && valueOfWeak != 0)
                    {
                        foreach (Labelset labelset in jointje.Keys.ToArray())
                        {
                            jointje[labelset][Will.strong] *= mce.Value[sentence.Character][Will.strong];
                            jointje[labelset][Will.weak] *= mce.Value[sentence.Character][Will.weak];
                        }
                    }
                }
                #endregion

                foreach (Labelset labelsetj in jointje.Keys)//j: true label
                {
                    #region P({n}|t,e)
                    double valueOfNumeratorForStrong = 1;
                    double valueOfNumeratorForWeak = 1;
                    foreach (Annotator annotator in sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Keys)
                    {
                        Labelset labelsetl = sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic[annotator].ToLabelset(Variable.LabelArray);
                        if (pakjl.Value[annotator].ContainsKey(labelsetj))
                        {
                            if (pakjl.Value[annotator][labelsetj].ContainsKey(labelsetl))
                            {
                                valueOfNumeratorForStrong *= pakjl.Value[annotator][labelsetj][labelsetl];
                                valueOfNumeratorForWeak *= pakjl.Value[annotator][labelsetj][labelsetl];
                            }
                            else
                            {
                                valueOfNumeratorForStrong = 0;
                                valueOfNumeratorForWeak = 0;
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                        //β部分的值（Personality新增）
                        valueOfNumeratorForStrong *= Math.Pow(bekef.Value[annotator][Tuple.Create(Will.strong, Will.strong)], okxc[annotator][sentence.Character][Will.strong])
                            * Math.Pow(bekef.Value[annotator][Tuple.Create(Will.strong, Will.weak)], okxc[annotator][sentence.Character][Will.weak]);
                        valueOfNumeratorForWeak *= Math.Pow(bekef.Value[annotator][Tuple.Create(Will.weak, Will.strong)], okxc[annotator][sentence.Character][Will.strong])
                            * Math.Pow(bekef.Value[annotator][Tuple.Create(Will.weak, Will.weak)], okxc[annotator][sentence.Character][Will.weak]);
                    }
                    #endregion
                    //乘以(P(t|e)*P(e))
                    valueOfNumeratorForStrong *= jointje[labelsetj][Will.strong];
                    valueOfNumeratorForWeak *= jointje[labelsetj][Will.weak];
                    if (valueOfNumeratorForStrong != 0 || valueOfNumeratorForWeak != 0)
                    {
                        numerator[sentence].Add(labelsetj, new Dictionary<Will, double>());
                        numerator[sentence][labelsetj].Add(Will.strong, valueOfNumeratorForStrong);
                        numerator[sentence][labelsetj].Add(Will.weak, valueOfNumeratorForWeak);
                    }
                }

                denominator.Add(sentence, new Dictionary<Will, double>());
                denominator[sentence].Add(Will.strong, 0);
                denominator[sentence].Add(Will.weak, 0);
                foreach (Labelset Labelsetq in numerator[sentence].Keys)//因为是加，故只需遍历numerator里有的标注，不需遍历所有标注
                {
                    denominator[sentence][Will.strong] += numerator[sentence][Labelsetq][Will.strong];
                    denominator[sentence][Will.weak] += numerator[sentence][Labelsetq][Will.weak];
                }

            }

            //计算Pdata和Sij
            pdata = pdata != null ? new Pdata(++pdata.Time, pdata.Value) : new Pdata(1, 0);
            sij = new Sij(++sij.Time);
            foreach (Sentence sentence in Variable.Sentences)
            {
                sij.Value.Add(sentence, new Dictionary<Labelset, double>());
                double nocompletValue = 0;
                foreach (Labelset labelset in numerator[sentence].Keys)
                {
                    if (denominator[sentence][Will.strong] == 0)
                    {
                        sij.Value[sentence][labelset] = numerator[sentence][labelset][Will.weak] / denominator[sentence][Will.weak] * mce.Value[sentence.Character][Will.weak];
                        nocompletValue += sij.Value[sentence][labelset];
                    }
                    else if (denominator[sentence][Will.weak] == 0)
                    {
                        sij.Value[sentence][labelset] = numerator[sentence][labelset][Will.strong] / denominator[sentence][Will.strong] * mce.Value[sentence.Character][Will.strong];
                        nocompletValue += sij.Value[sentence][labelset];
                    }
                    else
                    {
                        sij.Value[sentence][labelset] = numerator[sentence][labelset][Will.strong] / denominator[sentence][Will.strong] * mce.Value[sentence.Character][Will.strong]
                            + numerator[sentence][labelset][Will.weak] / denominator[sentence][Will.weak] * mce.Value[sentence.Character][Will.weak];//全概率公式
                    }
                }
                if (nocompletValue != 0)
                {
                    foreach (Labelset labelset in numerator[sentence].Keys)
                    {
                        sij.Value[sentence][labelset] /= nocompletValue;
                    }
                }
                pdata.Value += -Math.Log(denominator[sentence][Will.strong] + denominator[sentence][Will.weak]);
            }

            return isFinished;
        }

        static private Pje CalculatePje(Sij sij, int time)
        {
            Pje pje = new Pje(time);
            IDictionary<Will, double> willDenominator = new Dictionary<Will, double>();
            willDenominator.Add(Will.strong, 0);
            willDenominator.Add(Will.weak, 0);
            foreach (Sentence sentence in sij.Value.Keys)
            {
                foreach (Labelset labelset in sij.Value[sentence].Keys)
                {
                    double valueOfStrong = sij.Value[sentence][labelset] * labelset.HowStrong;
                    double valueOfWeak = sij.Value[sentence][labelset] * labelset.HowWeak;
                    if (pje.Value.ContainsKey(labelset))
                    {
                        pje.Value[labelset][Will.strong] += valueOfStrong;
                        pje.Value[labelset][Will.weak] += valueOfWeak;
                    }
                    else
                    {
                        pje.Value.Add(labelset, new Dictionary<Will, double>());
                        pje.Value[labelset].Add(Will.strong, valueOfStrong);
                        pje.Value[labelset].Add(Will.weak, valueOfWeak);
                    }
                    willDenominator[Will.strong] += valueOfStrong;
                    willDenominator[Will.weak] += valueOfWeak;
                }
            }
            //p(t|e)
            foreach (Labelset labelset in pje.Value.Keys.ToArray())
            {
                if (willDenominator[Will.strong] != 0)
                {
                    pje.Value[labelset][Will.strong] /= willDenominator[Will.strong];
                }
                if (willDenominator[Will.weak] != 0)
                {
                    pje.Value[labelset][Will.weak] /= willDenominator[Will.weak];
                }
            }
            return pje;
        }

        static private Mce CalculateMce(Sij sij, int time)
        {
            Mce mce = new Mce(time);
            foreach (Character character in ConsistencyVariable.Characters)
            {
                double numberOfTrueStrongAffects = 0;
                double numberOfTrueWeakAffects = 0;
                foreach (Sentence sentence in character.Sentences)
                {
                    foreach (Labelset labelset in sij.Value[sentence].Keys)
                    {
                        numberOfTrueStrongAffects += sij.Value[sentence][labelset] * labelset.NumberOfTrueStrongAffects;
                        numberOfTrueWeakAffects += sij.Value[sentence][labelset] * labelset.NumberOfTrueWeakAffects;
                    }
                }
                IDictionary<Will, double> willAndValue = new Dictionary<Will, double>();
                double will = numberOfTrueStrongAffects + numberOfTrueWeakAffects;
                willAndValue.Add(Will.strong, will == 0 ? 0.5 : numberOfTrueStrongAffects / will);
                willAndValue.Add(Will.weak, will == 0 ? 0.5 : numberOfTrueWeakAffects / will);
                mce.Value.Add(character, willAndValue);
            }
            return mce;
        }
    }
}