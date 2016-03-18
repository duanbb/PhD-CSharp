using MultilabelEstimation.Algorithm.DDS.NDDS;
using MultilabelEstimation.Consistency;
using MultilabelEstimation.Group;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MultilabelEstimation.Algorithm.Personality.PeTM
{
    class PeTMFunction
    {
        static public void RunPeTM(PorSForJointje PorS, Smoothing SmoothingBE, BnOrNot bnOrNot)
        {
            double[] accuracyOfPersonalityForEachGroup = new double[GroupVariable.AnnotatorGroups.Length];
            for (int groupIndex = 0; groupIndex < GroupVariable.AnnotatorGroups.Length; ++groupIndex)
            {
                IDictionary<Annotator, IDictionary<Character, IDictionary<Will, double>>> okcx = PersonalityFunction.CalculateOkcx(groupIndex);//模拟人对角色个性的标注，计算一次就不变了
                Mce mce = InitializeMce(groupIndex);
                Sije sije = null;
                if (bnOrNot == BnOrNot.Yes) sije = InitializeSijeWithBN(mce, groupIndex);
                else sije = InitializeSije(mce, groupIndex);//old new
                Pje pje = null;//p(t|e)
                Pdata pdata = null;
                IList<double> Pdatas = new List<double>();
                for (int convergeTime = 1; convergeTime <= Variable.ConvergeTimeThreshold; ++convergeTime)
                {
                    mce = CalculateMce(sije, groupIndex);
                    PAkjl pakjl = CoreFunction.CalculatePAkjl(Variable.LabelArray, sije.ToSij, convergeTime, groupIndex);
                    BEkef bekef = PersonalityFunction.CalculateBExy(mce, okcx, SmoothingBE, convergeTime, groupIndex);
                    if (PorS == PorSForJointje.P) pje = CalculatePje(sije, convergeTime);
                    if (CalculatePdataAndSije(ref sije, pakjl, bekef, pje, mce, okcx, ref pdata, Pdatas, groupIndex))//old/new
                        break;
                }
                IDictionary<Sentence, IDictionary<Will, double>> sic = ObtainBinaryResult(sije, mce, groupIndex);
                WriteBinaryResultFile(sic, mce, groupIndex);
                PersonalityFunction.WriteMVResultFile(mce, groupIndex);
                accuracyOfPersonalityForEachGroup[groupIndex] = PersonalityPaperFunction.AccuracyOfPersonalityForEachGroup(PersonalityVariable.TruePersonality, mce.EstimatedPersonality);
            }
            Function.ConsoleWriteLine("Accuracy Of PeTM: " + PersonalityPaperFunction.AccuracyOfPersonality(accuracyOfPersonalityForEachGroup));
        }

        static private Mce InitializeMce(int groupIndex)//初始化Sije时使用
        {
            Mce mce = new Mce(0);
            foreach (Character character in ConsistencyVariable.Characters)
            {
                double numberOfTrueStrongAffects = 0;
                double numberOfTrueWeakAffects = 0;
                foreach (Sentence sentence in character.Sentences)
                {
                    foreach (Annotation annotation in sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Values)
                    {
                        Labelset labelset = annotation.ToLabelset(Variable.LabelArray);
                        numberOfTrueStrongAffects += labelset.NumberOfTrueStrongAffects;
                        numberOfTrueWeakAffects += labelset.NumberOfTrueWeakAffects;
                    }
                }
                IDictionary<Will, double> willAndValue = new Dictionary<Will, double>();
                double will = numberOfTrueStrongAffects + numberOfTrueWeakAffects;
                willAndValue.Add(Will.strong, will == 0 ? 0.5 : numberOfTrueStrongAffects / will);//此处不是平滑
                willAndValue.Add(Will.weak, will == 0 ? 0.5 : numberOfTrueWeakAffects / will);
                mce.Value.Add(character, willAndValue);
            }
            return mce;
        }

        static private Sije InitializeSijeWithBN(Mce mce, int groupIndex)
        {
            Sije sije = new Sije(1);
            Sij sij = NDDSFunction.Initialize(groupIndex, Math.Pow(10, -1), IndependenceEstimation.MutualInformation);
            foreach (Sentence sentence in Variable.Sentences)
            {
                sije.Value.Add(sentence, new Dictionary<Labelset, IDictionary<Will, double>>());
                IDictionary<Will, double> willDenominator = new Dictionary<Will, double>();
                willDenominator.Add(Will.strong, 0);
                willDenominator.Add(Will.weak, 0);
                foreach (Labelset labelset in sij.Value[sentence].Keys.ToArray())
                {
                    double valueOfStrong = sij.Value[sentence][labelset] * labelset.HowStrong;
                    double valueOfWeak = sij.Value[sentence][labelset] * labelset.HowWeak;
                    if (sije.Value[sentence].ContainsKey(labelset))
                    {
                        sije.Value[sentence][labelset][Will.strong] += valueOfStrong;
                        sije.Value[sentence][labelset][Will.weak] += valueOfWeak;
                    }
                    else
                    {
                        sije.Value[sentence].Add(labelset, new Dictionary<Will, double>());
                        sije.Value[sentence][labelset].Add(Will.strong, valueOfStrong);
                        sije.Value[sentence][labelset].Add(Will.weak, valueOfWeak);
                    }
                    willDenominator[Will.strong] += valueOfStrong;
                    willDenominator[Will.weak] += valueOfStrong;
                }
                //p(t|e)
                foreach (Labelset labelset in sije.Value[sentence].Keys.ToArray())
                {
                    if (willDenominator[Will.strong] != 0)
                    {
                        sije.Value[sentence][labelset][Will.strong] /= willDenominator[Will.strong];
                    }
                    if (willDenominator[Will.weak] != 0)
                    {
                        sije.Value[sentence][labelset][Will.weak] /= willDenominator[Will.weak];
                    }
                }
                //p(t|e)*p(x)
                if (willDenominator[Will.strong] != 0 && willDenominator[Will.weak] != 0)//有一个等于0就不用再算了
                {
                    foreach (Labelset labelset in sije.Value[sentence].Keys.ToArray())
                    {
                        sije.Value[sentence][labelset][Will.strong] *= mce.Value[sentence.Character][Will.strong];
                        sije.Value[sentence][labelset][Will.weak] *= mce.Value[sentence.Character][Will.weak];
                    }
                }
            }
            return sije;
        }

        //相当于P(t,x)
        static private Sije InitializeSije(Mce mce, int groupIndex)
        {
            Sije sije = new Sije(1);
            foreach (Sentence sentence in Variable.Sentences)
            {
                sije.Value.Add(sentence, new Dictionary<Labelset, IDictionary<Will, double>>());
                IDictionary<Will, double> willDenominator = new Dictionary<Will, double>();
                willDenominator.Add(Will.strong, 0);
                willDenominator.Add(Will.weak, 0);
                foreach (Annotation annotation in sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Values)
                {
                    Labelset labelset = annotation.ToLabelset(Variable.LabelArray);
                    if (sije.Value[sentence].ContainsKey(labelset))
                    {
                        sije.Value[sentence][labelset][Will.strong] += labelset.HowStrong;
                        sije.Value[sentence][labelset][Will.weak] += labelset.HowWeak;
                    }
                    else
                    {
                        sije.Value[sentence].Add(labelset, new Dictionary<Will, double>());
                        sije.Value[sentence][labelset].Add(Will.strong, labelset.HowStrong);
                        sije.Value[sentence][labelset].Add(Will.weak, labelset.HowWeak);
                    }
                    willDenominator[Will.strong] += labelset.HowStrong;
                    willDenominator[Will.weak] += labelset.HowWeak;
                }
                //p(t|e)
                foreach (Labelset labelset in sije.Value[sentence].Keys.ToArray())
                {
                    if (willDenominator[Will.strong] != 0)
                    {
                        sije.Value[sentence][labelset][Will.strong] /= willDenominator[Will.strong];
                    }
                    if (willDenominator[Will.weak] != 0)
                    {
                        sije.Value[sentence][labelset][Will.weak] /= willDenominator[Will.weak];
                    }
                }
                //p(t|e)*p(x)
                if (willDenominator[Will.strong] != 0 && willDenominator[Will.weak] != 0)//有一个等于0就不用再算了
                {
                    foreach (Labelset labelset in sije.Value[sentence].Keys.ToArray())
                    {
                        sije.Value[sentence][labelset][Will.strong] *= mce.Value[sentence.Character][Will.strong];
                        sije.Value[sentence][labelset][Will.weak] *= mce.Value[sentence.Character][Will.weak];
                    }
                }
            }
            return sije;
        }

        //废弃
        static private Sije InitializeSijeOld(Mce mce, int groupIndex)
        {
            Sije sije = new Sije(1);
            foreach (Sentence sentence in Variable.Sentences)
            {
                double will = 0;
                sije.Value.Add(sentence, new Dictionary<Labelset, IDictionary<Will, double>>());
                foreach (Annotation annotation in sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Values)
                {
                    Labelset labelset = annotation.ToLabelset(Variable.LabelArray);
                    double strong = mce.Value[sentence.Character][Will.strong] * Math.Pow(labelset.HowStrong, 1);
                    double weak = mce.Value[sentence.Character][Will.weak] * Math.Pow(labelset.HowWeak, 1);
                    will += strong + weak;
                    if (sije.Value[sentence].ContainsKey(labelset))
                    {
                        sije.Value[sentence][labelset][Will.strong] += strong;
                        sije.Value[sentence][labelset][Will.weak] += weak;
                    }
                    else
                    {
                        sije.Value[sentence].Add(labelset, new Dictionary<Will, double>());
                        sije.Value[sentence][labelset].Add(Will.strong, strong);
                        sije.Value[sentence][labelset].Add(Will.weak, weak);
                    }
                }
                foreach (Labelset labelset in sije.Value[sentence].Keys.ToArray())
                {
                    sije.Value[sentence][labelset][Will.strong] /= will;
                    sije.Value[sentence][labelset][Will.weak] /= will;
                }
            }
            return sije;
        }

        //求p(t|m)，不是联合概率p(t,m)
        static private Pje CalculatePje(Sije sije, int time)
        {
            Pje pje = new Pje(time);
            IDictionary<Will, double> willDenominator = new Dictionary<Will, double>();
            willDenominator.Add(Will.strong, 0);
            willDenominator.Add(Will.weak, 0);
            foreach (Sentence sentence in sije.Value.Keys)
            {
                foreach (Labelset labelset in sije.Value[sentence].Keys)
                {
                    double valueOfStrong = sije.Value[sentence][labelset][Will.strong] * labelset.HowStrong;
                    double valueOfWeak = sije.Value[sentence][labelset][Will.weak] * labelset.HowWeak;
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

        static private Mce CalculateMce(Sije sije, int time)
        {
            Mce mce = new Mce(time);
            foreach (Character character in ConsistencyVariable.Characters)
            {
                double numberOfTrueStrongAffects = 0;
                double numberOfTrueWeakAffects = 0;
                foreach (Sentence sentence in character.Sentences)
                {
                    foreach (Labelset labelset in sije.Value[sentence].Keys)
                    {
                        numberOfTrueStrongAffects += sije.Value[sentence][labelset][Will.strong] * labelset.NumberOfTrueStrongAffects;
                        numberOfTrueWeakAffects += sije.Value[sentence][labelset][Will.weak] * labelset.NumberOfTrueWeakAffects;
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

        static private bool CalculatePdataAndSije(ref Sije sije, PAkjl pakjl, BEkef bekef, Pje pje, Mce mce, IDictionary<Annotator, IDictionary<Character, IDictionary<Will, double>>> okxc, ref Pdata pdata, IList<double> pdatas, int groupIndex)
        {
            bool isFinished = false;
            //sije的分子
            IDictionary<Sentence, IDictionary<Labelset, IDictionary<Will, double>>> numerator = new Dictionary<Sentence, IDictionary<Labelset, IDictionary<Will, double>>>();
            //sij的分母（P(data on i)）
            IDictionary<Sentence, double> denominator = new Dictionary<Sentence, double>();
            //计算分子
            foreach (Sentence sentence in sije.Value.Keys)
            {
                numerator.Add(sentence, new Dictionary<Labelset, IDictionary<Will, double>>());

                #region 联合概率P(t,m)
                IDictionary<Labelset, IDictionary<Will, double>> jointje;
                if (pje == null)//PorS == PorSForJointje.S
                {
                    //求后验概率P(t|e)，新增
                    jointje = new Dictionary<Labelset, IDictionary<Will, double>>();
                    IDictionary<Will, double> willDenominator = new Dictionary<Will, double>();
                    willDenominator.Add(Will.strong, 0);
                    willDenominator.Add(Will.weak, 0);
                    foreach (Labelset labelset in sije.Value[sentence].Keys)
                    {
                        //double valueOfStrong = sije.Value[sentence][labelset][Will.strong] * labelset.HowStrong;//这两个不科学
                        //double valueOfWeak = sije.Value[sentence][labelset][Will.weak] * labelset.HowWeak;
                        double valueOfStrong = sije.Value[sentence][labelset][Will.strong];
                        double valueOfWeak = sije.Value[sentence][labelset][Will.weak];
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

                double valueOfDenominator = 0;
                foreach (Labelset Labelsetq in numerator[sentence].Keys)//因为是加，故只需遍历numerator里有的标注，不需遍历所有标注
                {
                    valueOfDenominator += numerator[sentence][Labelsetq][Will.strong];
                    valueOfDenominator += numerator[sentence][Labelsetq][Will.weak];
                }
                denominator.Add(sentence, valueOfDenominator);
            }

            //计算Pdata和Sij
            pdata = pdata != null ? new Pdata(++pdata.Time, pdata.Value) : new Pdata(1, 0);
            sije = new Sije(++sije.Time);
            foreach (Sentence sentence in Variable.Sentences)
            {
                sije.Value.Add(sentence, new Dictionary<Labelset, IDictionary<Will, double>>());
                foreach (Labelset labelset in numerator[sentence].Keys)
                {
                    sije.Value[sentence].Add(labelset, new Dictionary<Will, double>());
                    sije.Value[sentence][labelset][Will.strong] = numerator[sentence][labelset][Will.strong] / denominator[sentence];//Dic赋值时没有的元素会自动加
                    sije.Value[sentence][labelset][Will.weak] = numerator[sentence][labelset][Will.weak] / denominator[sentence];
                }
                pdata.Value += -Math.Log(denominator[sentence]);
            }
            if (pdatas.Contains(pdata.Value) || (Math.Abs(pdata.MondifiedValue) <= Variable.ConvergeValueThreshold))
                isFinished = true;
            else
            {
                pdatas.Add(pdata.Value);
            }
            if (Variable.OutputPdata)
                Variable.OutputFile.WriteLine(pdata.ToString());
            return isFinished;
        }

        //废弃
        static private bool CalculatePdataAndSijeOld(ref Sije sije, PAkjl pakjl, BEkef bekef, Pje pje, Mce mce, IDictionary<Annotator, IDictionary<Character, IDictionary<Will, double>>> okxc, ref Pdata pdata, IList<double> pdatas, int groupIndex)
        {
            bool isFinished = false;
            //sije的分子
            IDictionary<Sentence, IDictionary<Labelset, IDictionary<Will, double>>> numerator = new Dictionary<Sentence, IDictionary<Labelset, IDictionary<Will, double>>>();
            //sij的分母（P(data on i)）
            IDictionary<Sentence, double> denominator = new Dictionary<Sentence, double>();
            //计算分子
            foreach (Sentence sentence in sije.Value.Keys)
            {
                numerator.Add(sentence, new Dictionary<Labelset, IDictionary<Will, double>>());
                //开始计算
                foreach (Labelset labelsetj in sije.Value[sentence].Keys)//j: true label
                {
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
                    //if (valueOfNumerator == 0) continue;
                    //乘以P(t|c)*P(c)
                    valueOfNumeratorForStrong *= sije.Value[sentence][labelsetj][Will.strong] * mce.Value[sentence.Character][Will.strong] * Math.Pow(labelsetj.HowStrong, 1);
                    valueOfNumeratorForWeak *= sije.Value[sentence][labelsetj][Will.weak] * mce.Value[sentence.Character][Will.weak] * Math.Pow(labelsetj.HowWeak, 1);
                    if (valueOfNumeratorForStrong != 0 || valueOfNumeratorForWeak != 0)
                    {
                        numerator[sentence].Add(labelsetj, new Dictionary<Will, double>());
                        numerator[sentence][labelsetj].Add(Will.strong, valueOfNumeratorForStrong);
                        numerator[sentence][labelsetj].Add(Will.weak, valueOfNumeratorForWeak);
                    }
                }
                double valueOfDenominator = 0;
                foreach (Labelset Labelsetq in numerator[sentence].Keys)//因为是加，故只需遍历numerator里有的标注，不需遍历所有标注
                {
                    valueOfDenominator += numerator[sentence][Labelsetq][Will.strong];
                    valueOfDenominator += numerator[sentence][Labelsetq][Will.weak];
                }
                denominator.Add(sentence, valueOfDenominator);
            }

            //计算Pdata和Sij
            pdata = pdata != null ? new Pdata(++pdata.Time, pdata.Value) : new Pdata(1, 0);
            sije = new Sije(++sije.Time);
            foreach (Sentence sentence in Variable.Sentences)
            {
                sije.Value.Add(sentence, new Dictionary<Labelset, IDictionary<Will, double>>());
                foreach (Labelset labelset in numerator[sentence].Keys)
                {
                    sije.Value[sentence].Add(labelset, new Dictionary<Will, double>());
                    sije.Value[sentence][labelset][Will.strong] = numerator[sentence][labelset][Will.strong] / denominator[sentence];//Dic赋值时没有的元素会自动加
                    sije.Value[sentence][labelset][Will.weak] = numerator[sentence][labelset][Will.weak] / denominator[sentence];
                }
                pdata.Value += -Math.Log(denominator[sentence]);
            }
            if (pdatas.Contains(pdata.Value) || (Math.Abs(pdata.MondifiedValue) <= Variable.ConvergeValueThreshold))
                isFinished = true;
            else
            {
                pdatas.Add(pdata.Value);
            }
            if (Variable.OutputPdata)
                Variable.OutputFile.WriteLine(pdata.ToString());
            return isFinished;
        }

        static private IDictionary<Sentence, IDictionary<Will, double>> ObtainBinaryResult(Sije sije, Mce mce, int groupIndex)
        {
            IDictionary<Sentence, IDictionary<Will, double>> sic = new Dictionary<Sentence, IDictionary<Will, double>>();
            foreach (Sentence sentence in sije.Value.Keys)
            {
                IDictionary<Will, double> willForResult = new Dictionary<Will, double>();
                #region 不根据will
                //sentence.AnnotaitonGroups[groupIndex].PersonalityDSMaxResult = new Result(sije.CalculateJointBestLabelset(sentence, ref willForResult));
                #endregion
                #region 根据will（更好）
                Will willOfChar = mce.Value[sentence.Character][Will.strong] >= mce.Value[sentence.Character][Will.weak] ? Will.strong : Will.weak;
                sentence.AnnotaitonGroups[groupIndex].PeTMResult = new Result(sije.CalculateJointBestLabelset(sentence, willOfChar));
                if (willOfChar == Will.strong)
                {
                    willForResult.Add(Will.strong, sentence.AnnotaitonGroups[groupIndex].PeTMResult.Probability);
                    willForResult.Add(Will.weak, 1 - sentence.AnnotaitonGroups[groupIndex].PeTMResult.Probability);
                }
                else
                {
                    willForResult.Add(Will.strong, 1 - sentence.AnnotaitonGroups[groupIndex].PeTMResult.Probability);
                    willForResult.Add(Will.weak, sentence.AnnotaitonGroups[groupIndex].PeTMResult.Probability);
                }
                #endregion
                sic.Add(sentence, willForResult);
            }
            return sic;
        }

        static private void WriteBinaryResultFile(IDictionary<Sentence, IDictionary<Will, double>> sic, Mce mce, int groupIndex)//只输出
        {
            if (Variable.OutputResult)
            {
                StreamWriter resultFile = new StreamWriter("Result/" + Variable.NumberOfAnnotationsPerSentenceAfterGrouping + "PeTM" + "Binary" + groupIndex + ".csv", false, Encoding.Default);
                Function.InitialResultFile(resultFile);
                double averageTrueLabelsPerResult = 0;
                foreach (Sentence sentence in Variable.Sentences)
                {
                    Function.WriteBinaryResultOfASentence(sentence.ID, sentence.AnnotaitonGroups[groupIndex].PeTMResult,
                        sentence.Character.ID + ","
                        + " strongInM:" + mce.Value[sentence.Character][Will.strong] + " weakInM:" + mce.Value[sentence.Character][Will.weak] + ","
                        + " strongInS:" + sic[sentence][Will.strong] + " weakInS:" + sic[sentence][Will.weak] + ",",
                        sentence.Speech, resultFile);
                    averageTrueLabelsPerResult += sentence.AnnotaitonGroups[groupIndex].PeTMResult.NumberOfTrueLabel;
                }
                resultFile.Write("Average true labels per annotatin," + averageTrueLabelsPerResult / Variable.Sentences.Count + ",");
                resultFile.Close();
            }
        }
    }
}