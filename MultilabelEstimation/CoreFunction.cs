using MultilabelEstimation.Consistency;
using MultilabelEstimation.Group;
using MultilabelEstimation.Relation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultilabelEstimation
{
    static class CoreFunction
    {
        //初始化
        static public Sij InitializeSij(Label[] labels, int groupIndex)
        {
            Sij sij = new Sij(1);
            foreach (Sentence sentence in Variable.Sentences)
            {
                sij.Value.Add(sentence, new Dictionary<Labelset, double>());
                //Sij: Dictionary<句子，Dictionary<标签，次数>> 句子i被标为j的次数
                foreach (Annotation annotation in sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Values)
                {
                    Labelset labelset = annotation.ToLabelset(labels);
                    if (sij.Value[sentence].ContainsKey(labelset))
                        ++sij.Value[sentence][labelset];
                    else
                        sij.Value[sentence].Add(labelset, 1);
                }
                //Sij: Dictionary<句子，Dictionary<标签，概率>> 句子i被标为j的概率
                foreach (Labelset labelset in sij.Value[sentence].Keys.ToArray())
                {
                    sij.Value[sentence][labelset] /= Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                }
            }
            return sij;
        }

        static public Pj CalculatePj(Sij sij, int time)//计算Pj
        {
            Pj pj = new Pj(time);

            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (Labelset labelset in sij.Value[sentence].Keys)
                {
                    if (pj.Value.ContainsKey(labelset))
                        pj.Value[labelset] += sij.Value[sentence][labelset];
                    else
                        pj.Value.Add(labelset, sij.Value[sentence][labelset]);
                }
            }
            if (Variable.PjDividSentenceCount)
            {
                foreach (Labelset labelset in pj.Value.Keys.ToArray())
                {
                    pj.Value[labelset] /= Variable.Sentences.Count;
                }
            }
            //Variable.OutputFile.WriteLine(pj.ToString(DependentVariable.NumberOfIntlabel));
            return pj;//对于所有j，pj的和为1
        }

        static public PAkjl CalculatePAkjl(Label[] labels, Sij sij, int time, int groupIndex)//计算π
        {
            PAkjl pakjl = new PAkjl(time);
            IDictionary<Annotator, IDictionary<Labelset, double>> denominator = new Dictionary<Annotator, IDictionary<Labelset, double>>();//Dictionary<人，Dictionary<标签j，值>>：𝑛𝑢𝑚𝑏𝑒𝑟 𝑜𝑓 𝑠𝑒𝑛𝑡𝑒𝑛𝑐𝑒𝑠 𝑎𝑛𝑛𝑜𝑡𝑎𝑡𝑜𝑟 k 𝑟𝑒𝑐𝑜𝑟𝑑𝑠 𝑤ℎ𝑒𝑛 j 𝑖𝑠 𝑐𝑜𝑟𝑟𝑒𝑐𝑡，分母
            foreach (Annotator annotator in GroupVariable.AnnotatorGroups[groupIndex])
            {
                pakjl.Value.Add(annotator, new Dictionary<Labelset, IDictionary<Labelset, double>>());//π本身充当分子
                denominator.Add(annotator, new Dictionary<Labelset, double>());
            }
            //计算分子分母
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (Annotator annotator in sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Keys)//只考虑这个人对于这句的标注，不考虑这个人对其他句的标注（如果这个人不在这个其他句的本组数据里）
                {
                    foreach (Labelset labelsetj in sij.Value[sentence].Keys)//正确标签
                    {
                        //计算分子
                        if (!pakjl.Value[annotator].ContainsKey(labelsetj))
                            pakjl.Value[annotator].Add(labelsetj, new Dictionary<Labelset, double>());
                        Labelset labelsetl = sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic[annotator].ToLabelset(labels);
                        if (pakjl.Value[annotator][labelsetj].ContainsKey(labelsetl))
                            pakjl.Value[annotator][labelsetj][labelsetl] += sij.Value[sentence][labelsetj];
                        else
                            pakjl.Value[annotator][labelsetj].Add(labelsetl, sij.Value[sentence][labelsetj]);
                        //计算分母
                        if (denominator[annotator].ContainsKey(labelsetj))
                            denominator[annotator][labelsetj] += sij.Value[sentence][labelsetj];
                        else
                            denominator[annotator].Add(labelsetj, sij.Value[sentence][labelsetj]);
                    }
                }
            }
            //计算π
            //要平滑：1.如果a > b，则让a/a > b/b（如不平滑，则都为一）；2.求sij时，多个π相乘会浮点溢出，使sij等于0
            //此处是改进，只平滑π里有的，不平滑π里未出现过的jl对。经验证，Masatyan会改善
            IDictionary<Smoothing, double[]> smoothingNumber = Function.SmoothingNumber(Math.Pow(2, labels.Length));
            foreach (Annotator annotator in pakjl.Value.Keys)//人
            {
                foreach (Labelset labelsetj in pakjl.Value[annotator].Keys)
                {
                    foreach (Labelset labelsetl in pakjl.Value[annotator][labelsetj].Keys.ToArray())
                    {
                        if (Variable.SmoothPajl != Smoothing.None)
                        {
                            if (denominator[annotator][labelsetj] != 0)
                                pakjl.Value[annotator][labelsetj][labelsetl] = (pakjl.Value[annotator][labelsetj][labelsetl] + smoothingNumber[Variable.SmoothPajl][0]) / (denominator[annotator][labelsetj] + smoothingNumber[Variable.SmoothPajl][1]);
                            else
                                pakjl.Value[annotator][labelsetj][labelsetj] = 1;//Dic赋值时，没有会直接添进去
                        }
                        else
                            pakjl.Value[annotator][labelsetj][labelsetl] /= denominator[annotator][labelsetj];

                        //if (denominator[annotator][labelsetj] != 0)
                        //    pakjl.Value[annotator][labelsetj][labelsetl] /= denominator[annotator][labelsetj];
                        //else
                        //    pakjl.Value[annotator][labelsetj][labelsetl] = 1 / Math.Pow(2, labels.Length);
                    }
                }
            }
            return pakjl;
        }

        //计算Sij和Pdata
        static public bool CalculatePdataAndSij(Label[] labels, ref Sij sij, Pj pj, PAkjl pakjl, Mcj mcj, ref Pdata pdata, int groupIndex, IList<double> pdatas,
            IDictionary<Tuple<Labelset, Labelset>, double> labelsetPairFrequencyForPj, IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> labelsetPairFrequencyForMcj,
            IDictionary<Tuple<Sentence, Sentence>, IDictionary<Tuple<Labelset, Labelset>, double>> labelsetPairFrequencyForSij)
        {
            bool isFinished = false;
            IDictionary<Tuple<Labelset, Labelset>, double> conditionalPj = null;//转移概率
            IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> conditionalMcj = null;
            if (Variable.PriorP.Contains(PriorP.ConditionalPj))
                conditionalPj = RelationFunction.CalculateConditionalPj(pj, labelsetPairFrequencyForPj);
            if (Variable.PriorP.Contains(PriorP.ConditionalMcj))
                conditionalMcj = RelationFunction.CalculateConditionalMcj(mcj, labelsetPairFrequencyForMcj);
            //sij的分子
            IDictionary<Sentence, IDictionary<Labelset, double>> numerator = new Dictionary<Sentence, IDictionary<Labelset, double>>();
            //sij的分母（P(data on i)）
            IDictionary<Sentence, double> denominator = new Dictionary<Sentence, double>();
            //计算分子
            foreach (Sentence sentence in Variable.Sentences)
            {
                numerator.Add(sentence, new Dictionary<Labelset, double>());
                #region 寻找需要遍历的j
                //应该用pj中的j，即当前分组中出现过的所有标注情况，不能用sij现有的。因为要用pajl和pj重新计算sij，已与sij现有值无关，虽然对Boku来说结果一样
                ICollection<Labelset> labelsets = null;
                if (Variable.PriorP.Contains(PriorP.Sij) || Variable.PriorP.Contains(PriorP.ConditionalSij))
                    labelsets = sij.Value[sentence].Keys;
                else if (Variable.PriorP.Contains(PriorP.Mcj) || Variable.PriorP.Contains(PriorP.ConditionalMcj))
                    labelsets = mcj.Value[sentence.Character].Keys;
                else if (Variable.PriorP.Contains(PriorP.Pj) || Variable.PriorP.Contains(PriorP.ConditionalPj))
                    labelsets = pj.Value.Keys;//pj里只包含所有句出现过的所有标注情况，所以最多遍历pj即可，不需要for(int j = 0; j < Math.Pow(2, labels.Length); ++j)
                else//全部的
                {
                    labelsets = new List<Labelset>();
                    for (int j = 0; j < Math.Pow(2, labels.Length); ++j)
                    {
                        labelsets.Add(new Labelset(labels, j));
                    }
                }
                #endregion

                //开始计算
                foreach (Labelset labelsetj in labelsets)//此时结果会好一些（masa: group3, group5）
                {
                    double valueOfNumerator = 1;
                    #region（公式(5)）
                    foreach (Annotator annotator in sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Keys)
                    {
                        Labelset labelsetl = sentence.AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic[annotator].ToLabelset(labels);
                        if (pakjl.Value[annotator].ContainsKey(labelsetj))
                        {
                            if (pakjl.Value[annotator][labelsetj].ContainsKey(labelsetl))
                                valueOfNumerator *= pakjl.Value[annotator][labelsetj][labelsetl];
                            else
                            {
                                valueOfNumerator = 0;
                                break;
                            }
                        }
                        //else valueOfNumerator *= 1 / Math.Pow(2, labels.Length);//如果annotator没标过正确为j的句子，则认为此annotator对此j来说，所有可能标的标签l概率相等（对masa有用，没有的话（相当于乘以1）结果很差；boku进不来）
                        else { valueOfNumerator = 0; break; }//相当于valueOfNumerator*0；boku进不来；此时结果会好一些（masa: group3）
                    }
                    if (valueOfNumerator == 0) continue;
                    #endregion
                    #region 公式(5)*(6)
                    foreach (PriorP p in Variable.PriorP)
                    {
                        switch (p)
                        {
                            case PriorP.Pj:
                                valueOfNumerator *= pj.Value[labelsetj];
                                break;
                            case PriorP.Mcj:
                                valueOfNumerator *= mcj.Value[sentence.Character][labelsetj];//Consistency的关键
                                break;
                            case PriorP.Sij:
                                valueOfNumerator *= sij.Value[sentence][labelsetj];
                                break;
                            case PriorP.ConditionalPj:
                                {
                                    if (sentence.ID != 0)
                                    {
                                        bool finded = false;
                                        double optimalPreLabelsetValue = 0;
                                        double conditonalPj = 0;
                                        int n = 1;
                                        foreach (KeyValuePair<Labelset, double> labelsetPre in sij.SortLabelsets(Variable.Sentences[sentence.ID - 1]))//找出前一句最优标注和其概率
                                        {
                                            Tuple<Labelset, Labelset> labelsetPair = Tuple.Create(labelsetPre.Key, labelsetj);
                                            if (!finded)
                                            {
                                                if (conditionalPj.ContainsKey(labelsetPair))
                                                {
                                                    finded = true;
                                                    conditonalPj = conditionalPj[labelsetPair];//找出前一句最优标注到这一句的转移概率
                                                    optimalPreLabelsetValue = labelsetPre.Value;
                                                }
                                                continue;
                                            }
                                            if (finded)
                                            {
                                                if (labelsetPre.Value == optimalPreLabelsetValue && conditionalPj.ContainsKey(labelsetPair))//最优标注可能不只一个（概率相同，同为最大），随意要继续遍历
                                                {
                                                    conditonalPj += conditionalPj[labelsetPair];
                                                    ++n;
                                                }
                                                else break;
                                            }
                                        }
                                        valueOfNumerator *= conditonalPj / n;//n：最优标注的个数
                                    }
                                    else
                                    {
                                        Tuple<Labelset, Labelset> labelsetPair = Tuple.Create(new Labelset(true), labelsetj);
                                        if (conditionalPj.ContainsKey(labelsetPair))
                                            valueOfNumerator *= conditionalPj[labelsetPair];
                                        else valueOfNumerator = 0;
                                    }
                                }
                                break;
                            #region Mcj
                            case PriorP.ConditionalMcj:
                                {
                                    if (sentence.ID != 0)
                                    {
                                        bool finded = false;
                                        double optimalPreLabelsetValue = 0;
                                        double maxConditonalMcj = 0;
                                        int n = 1;
                                        Sentence sentencePre = Variable.Sentences[sentence.ID - 1];
                                        Tuple<Character, Character> characterPair = Tuple.Create(sentencePre.Character, sentence.Character);
                                        foreach (KeyValuePair<Labelset, double> labelsetPre in sij.SortLabelsets(sentencePre))
                                        {
                                            Tuple<Labelset, Labelset> labelsetPair = Tuple.Create(labelsetPre.Key, labelsetj);
                                            if (!finded)
                                            {
                                                if (conditionalMcj[characterPair].ContainsKey(labelsetPair))
                                                {
                                                    finded = true;
                                                    maxConditonalMcj = conditionalMcj[characterPair][labelsetPair];
                                                    optimalPreLabelsetValue = labelsetPre.Value;
                                                }
                                                continue;
                                            }
                                            if (finded)
                                            {
                                                if (labelsetPre.Value == optimalPreLabelsetValue && conditionalMcj[characterPair].ContainsKey(labelsetPair))
                                                {
                                                    maxConditonalMcj += conditionalMcj[characterPair][labelsetPair];
                                                    ++n;
                                                }
                                                else break;
                                            }
                                        }
                                        valueOfNumerator *= maxConditonalMcj / n;
                                    }
                                    else
                                    {
                                        Tuple<Character, Character> characterPair = Tuple.Create(new Character("##"), sentence.Character);
                                        Tuple<Labelset, Labelset> labelsetPair = Tuple.Create(new Labelset(true), labelsetj);
                                        if (conditionalMcj[characterPair].ContainsKey(labelsetPair))
                                            valueOfNumerator *= conditionalMcj[characterPair][labelsetPair];
                                        else valueOfNumerator = 0;
                                    }
                                }
                                break;
                            #endregion
                            case PriorP.ConditionalSij:
                                {
                                    if (sentence.ID != 0)
                                    {
                                        bool finded = false;
                                        double optimalPreLabelsetValue = 0;
                                        double conditonalPj = 0;
                                        int n = 1;
                                        Sentence sentencePre = Variable.Sentences[sentence.ID - 1];
                                        Tuple<Sentence, Sentence> sentencePair = Tuple.Create(sentencePre, sentence);
                                        foreach (KeyValuePair<Labelset, double> labelsetPre in sij.SortLabelsets(sentencePre))
                                        {
                                            Tuple<Labelset, Labelset> labelsetPair = Tuple.Create(labelsetPre.Key, labelsetj);
                                            if (!finded)
                                            {
                                                if (labelsetPairFrequencyForSij[sentencePair].ContainsKey(labelsetPair))
                                                {
                                                    finded = true;
                                                    conditonalPj = labelsetPairFrequencyForSij[sentencePair][labelsetPair] / labelsetPre.Value;
                                                    optimalPreLabelsetValue = labelsetPre.Value;
                                                }
                                                continue;
                                            }
                                            if (finded)
                                            {
                                                if (labelsetPre.Value == optimalPreLabelsetValue && labelsetPairFrequencyForSij[sentencePair].ContainsKey(labelsetPair))
                                                {
                                                    conditonalPj += labelsetPairFrequencyForSij[sentencePair][labelsetPair] / labelsetPre.Value;
                                                    ++n;
                                                }
                                                else break;
                                            }
                                        }
                                        valueOfNumerator *= conditonalPj / n;
                                    }
                                    else
                                    {
                                        Tuple<Sentence, Sentence> sentencePair = Tuple.Create(new Sentence(-1, "##"), sentence);
                                        Tuple<Labelset, Labelset> labelsetPair = Tuple.Create(new Labelset(true), labelsetj);
                                        if (labelsetPairFrequencyForSij[sentencePair].ContainsKey(labelsetPair))
                                            valueOfNumerator *= labelsetPairFrequencyForSij[sentencePair][labelsetPair];
                                        else valueOfNumerator = 0;
                                    }
                                }
                                break;
                        }
                    }
                    #endregion
                    if (valueOfNumerator != 0)
                        numerator[sentence].Add(labelsetj, valueOfNumerator);
                }
                #region 计算分母 (公式(7))
                double valueOfDenominator = 0;
                foreach (Labelset Labelsetq in numerator[sentence].Keys)//因为是加，故只需遍历numerator里有的标注，不需遍历所有标注
                {
                    valueOfDenominator += numerator[sentence][Labelsetq];
                }
                denominator.Add(sentence, valueOfDenominator);
                #endregion
            }

            //计算Pdata和Sij
            pdata = pdata != null ? new Pdata(++pdata.Time, pdata.Value) : new Pdata(1, 0);
            sij = new Sij(++sij.Time);
            foreach (Sentence sentence in Variable.Sentences)
            {
                sij.Value.Add(sentence, new Dictionary<Labelset, double>());
                foreach (Labelset labelset in numerator[sentence].Keys)
                {
                    if (Variable.SijDividPDataOnI)//常规方法
                        sij.Value[sentence][labelset] = numerator[sentence][labelset] / denominator[sentence];//Dic赋值时没有的元素会自动加
                    else
                        sij.Value[sentence][labelset] = numerator[sentence][labelset];
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

        static public void Intgerate(Label[] labels, int groupIndex, ref Sij sij)
        {
            Pj pj = null;
            Pdata pdata = null;
            Mcj mcj = null;
            IDictionary<Tuple<Labelset, Labelset>, double> labelsetPairFrequencyForPj = null;//-1(开头)不能用来索引数组，故只能使用哈希表
            IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> labelsetPairFrequencyForMcj = null;
            IDictionary<Tuple<Sentence, Sentence>, IDictionary<Tuple<Labelset, Labelset>, double>> labelsetPairFrequencyForSij = null;
            IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>> independentLabelsetPairFrequencyForPj = null;
            IDictionary<Tuple<Character, Character>, IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>> independentLabelsetPairFrequencyForMcj = null;
            IDictionary<Tuple<Sentence, Sentence>, IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>> independentLabelsetPairFrequencyForSij = null;
            if (Variable.PriorP.Contains(PriorP.ConditionalPj))//计算在一篇（一次）标注中的转移频率
                ChoiceFunction.InitializationOfLabelsetPairFrequencyForPj(labels, groupIndex, ref labelsetPairFrequencyForPj, ref independentLabelsetPairFrequencyForPj);
            if (Variable.PriorP.Contains(PriorP.ConditionalMcj))
                ChoiceFunction.InitializationOfLabelsetPairFrequencyForMcj(labels, groupIndex, ref labelsetPairFrequencyForMcj, ref independentLabelsetPairFrequencyForMcj);
            if (Variable.PriorP.Contains(PriorP.ConditionalSij))
                ChoiceFunction.InitializationOfLabelsetPairFrequencyForSij(labels, groupIndex, ref labelsetPairFrequencyForSij, ref independentLabelsetPairFrequencyForSij);
            IList<double> Pdatas = new List<double>();

            for (int time = 1; time <= Variable.ConvergeTimeThreshold; ++time)
            {
                //计算Pj，mcj（Consistency：角色c有j标签的概率）
                ChoiceFunction.PriorPj(ref pj, ref mcj, sij, time);
                //计算π
                PAkjl pakjl = CalculatePAkjl(labels, sij, time, groupIndex);
                //计算Sij
                if (CalculatePdataAndSij(labels, ref sij, pj, pakjl, mcj, ref pdata, groupIndex, Pdatas, labelsetPairFrequencyForPj, labelsetPairFrequencyForMcj, labelsetPairFrequencyForSij))
                    break;
                if (Variable.PriorP.Contains(PriorP.ConditionalPj))
                    ChoiceFunction.UpdateLabelsetPairFrequencyForPj(sij, ref labelsetPairFrequencyForPj, labels, ref independentLabelsetPairFrequencyForPj);
                if (Variable.PriorP.Contains(PriorP.ConditionalMcj))
                    ChoiceFunction.UpdateLabelsetPairFrequencyForMcj(sij, ref labelsetPairFrequencyForMcj, labels, ref independentLabelsetPairFrequencyForMcj);
            }
        }
    }
}