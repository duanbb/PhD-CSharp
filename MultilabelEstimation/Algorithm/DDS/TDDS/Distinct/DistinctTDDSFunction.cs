using MultilabelEstimation.Group;
using MultilabelEstimation.Consistency;
using System;
using System.Collections.Generic;

namespace MultilabelEstimation.Algorithm.DDS.TDDS.Distinct
{
    class DistinctTDDSFunction
    {
        static public void RunDTDDS()
        {
            for (int groupIndex = 0; groupIndex < GroupVariable.AnnotatorGroups.Length; ++groupIndex)
            {
                Initialize(groupIndex);
                IList<double> Pdatas = new List<double>();
                for (int time = 1; time <= Variable.ConvergeTimeThreshold; ++time)
                {
                    //计算Pk，mcj（consistent：角色c有j标签的概率）
                    ChoiceFunction.PriorPj(ref DTDDSVariable.Pj, ref DTDDSVariable.Mcj, DTDDSVariable.Sij, time);
                    //计算π
                    //DTDDSVariable.Pajl = CoreFunction.CalculatePajl(Variable.LabelArray, DTDDSVariable.Sij, time, groupIndex);
                    //计算Sij
                    //if (CoreFunction.CalculatePdataAndSij(Variable.LabelArray, ref DTDDSVariable.Sij, DTDDSVariable.Pj, DTDDSVariable.Pajl, DTDDSVariable.Mcj, ref DTDDSVariable.Pdata, groupIndex, Pdatas,
                    //    DTDDSVariable.ConditionalPj, DTDDSVariable.ConditionalMcj))
                        //break;
                }
                DDSFunction.ObtainBinaryResult(DTDDSVariable.Sij, "DTDDS", groupIndex);
                DDSFunction.ObtainNumericResult(DTDDSVariable.Sij, "DTDDS", groupIndex);
                Function.WriteNumericResultFile("DTDDS", groupIndex);
                Function.WriteBinaryResultFile("DTDDS", groupIndex);
            }
        }

        static private void Initialize(int group)
        {
            #region 初始化
            DTDDSVariable.Sij = new Sij(1);
            //每句的信息，用于树的具体值（每个句子对应的树的值不同)
            IDictionary<Sentence, IDictionary<Label, double>> NumberOfLabelTrue = new Dictionary<Sentence, IDictionary<Label, double>>();
            IDictionary<Sentence, IDictionary<Label, double>> NumberOfLabelFalse = new Dictionary<Sentence, IDictionary<Label, double>>();
            IDictionary<Sentence, IDictionary<LabelPair, double>> NumberOfLabel1TrueLabel2True = new Dictionary<Sentence, IDictionary<LabelPair, double>>();
            IDictionary<Sentence, IDictionary<LabelPair, double>> NumberOfLabel1TrueLabel2False = new Dictionary<Sentence, IDictionary<LabelPair, double>>();
            IDictionary<Sentence, IDictionary<LabelPair, double>> NumberOfLabel1FalseLabel2True = new Dictionary<Sentence, IDictionary<LabelPair, double>>();
            IDictionary<Sentence, IDictionary<LabelPair, double>> NumberOfLabel1FalseLabel2False = new Dictionary<Sentence, IDictionary<LabelPair, double>>();
            foreach (Sentence sentence in Variable.Sentences)
            {
                IDictionary<Label, int> numberOfEachLabel = new Dictionary<Label, int>();
                foreach (Label label in Variable.LabelArray)
                {
                    numberOfEachLabel.Add(label, 0);
                }
                foreach (Annotation annotation in sentence.AnnotaitonGroups[group].AnnotatorAnnotationDic.Values)
                {
                    foreach (Label label in Variable.LabelArray)
                    {
                        if (annotation.Labels[label])
                        {
                            ++numberOfEachLabel[label];
                        }
                    }
                }
                List<KeyValuePair<Label, int>> sortedLabel = new List<KeyValuePair<Label, int>>(numberOfEachLabel);
                sortedLabel.Sort(delegate(KeyValuePair<Label, int> s1, KeyValuePair<Label, int> s2)
                {
                    return s2.Value.CompareTo(s1.Value);
                });
                sentence.LabelArray = new Label[10];
                for (int a = 0; a < Variable.LabelArray.Length; ++a)
                {
                    sentence.LabelArray[a] = sortedLabel[a].Key;
                }
                IDictionary<Label, double> labelFloatDic = new Dictionary<Label, double>();
                IDictionary<LabelPair, double> labelPairFloat = new Dictionary<LabelPair, double>();//前后无序，45个，用于初始化
                //Function.InitializeEmptyLabelDic(ref labelFloatDic, ref labelPairFloat, sentence.LabelArray);
                NumberOfLabelTrue.Add(sentence, new Dictionary<Label, double>(labelFloatDic));
                NumberOfLabelFalse.Add(sentence, new Dictionary<Label, double>(labelFloatDic));
                NumberOfLabel1TrueLabel2True.Add(sentence, new Dictionary<LabelPair, double>(labelPairFloat));
                NumberOfLabel1TrueLabel2False.Add(sentence, new Dictionary<LabelPair, double>(labelPairFloat));
                NumberOfLabel1FalseLabel2True.Add(sentence, new Dictionary<LabelPair, double>(labelPairFloat));
                NumberOfLabel1FalseLabel2False.Add(sentence, new Dictionary<LabelPair, double>(labelPairFloat));
            }
            #endregion
            #region 求互信息的参数
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (Annotation annotation in sentence.AnnotaitonGroups[group].AnnotatorAnnotationDic.Values)
                {
                    IList<Label> traversedLabels = new List<Label>();
                    foreach (Label label1 in sentence.LabelArray)
                    {
                        traversedLabels.Add(label1);
                        if (annotation.Labels[label1])
                        {
                            ++NumberOfLabelTrue[sentence][label1];
                            foreach (Label label2 in sentence.LabelArray)
                            {
                                if (!traversedLabels.Contains(label2))
                                {
                                    if (annotation.Labels[label2])
                                    {
                                        ++NumberOfLabel1TrueLabel2True[sentence][new LabelPair(label1, label2)];
                                    }
                                    else
                                    {
                                        ++NumberOfLabel1TrueLabel2False[sentence][new LabelPair(label1, label2)];
                                    }
                                }
                            }
                        }
                        else
                        {
                            ++NumberOfLabelFalse[sentence][label1];
                            foreach (Label label2 in sentence.LabelArray)
                            {
                                if (!traversedLabels.Contains(label2))
                                {
                                    if (annotation.Labels[label2])
                                    {
                                        ++NumberOfLabel1FalseLabel2True[sentence][new LabelPair(label1, label2)];
                                    }
                                    else
                                    {
                                        ++NumberOfLabel1FalseLabel2False[sentence][new LabelPair(label1, label2)];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            #region 为每个句子生成树
            foreach (Sentence sentence in Variable.Sentences)
            {
                sentence.Tree = TDDSFunction.GenerateIMTree(NumberOfLabelTrue[sentence], NumberOfLabelFalse[sentence],
                    NumberOfLabel1TrueLabel2True[sentence], NumberOfLabel1TrueLabel2False[sentence],
                    NumberOfLabel1FalseLabel2True[sentence], NumberOfLabel1FalseLabel2False[sentence], Variable.NumberOfAnnotationsPerSentenceAfterGrouping, sentence.LabelArray);
            }
            #endregion
            //Dependent.Tree.Distinct.FunctionOfDistinct.NumberOfIncompletedTreeSentence();
            IDictionary<Smoothing, double[]> smoothingNumber = Function.SmoothingNumber(Variable.LabelArray.Length);
            #region 初始化Sij
            foreach (Sentence sentence in Variable.Sentences)
            {
                for (int j = 0; j < Math.Pow(2, Variable.LabelArray.Length); ++j)
                {
                    Annotation annotation = new Annotation(j);
                    if (annotation.Labels[sentence.Tree[0].Key.First])
                        DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, j)] = NumberOfLabelTrue[sentence][sentence.Tree[0].Key.First] / Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                    else
                        DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, j)] = NumberOfLabelFalse[sentence][sentence.Tree[0].Key.First] / Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                    foreach (KeyValuePair<LabelPair, double> labelPairAndValue in sentence.Tree)
                    {
                        LabelPair ap;
                        if (NumberOfLabel1TrueLabel2True[sentence].ContainsKey(labelPairAndValue.Key))
                            ap = labelPairAndValue.Key;
                        else
                            ap = labelPairAndValue.Key.Reverse;
                        if (annotation.Labels[ap.First])
                        {
                            if (DTDDSVariable.SmoothTree == Smoothing.None)
                            {
                                if (NumberOfLabelTrue[sentence][ap.First] != 0)//考虑分母为0的情况
                                {
                                    if (annotation.Labels[ap.Second])
                                        DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, j)] *= NumberOfLabel1TrueLabel2True[sentence][ap] / NumberOfLabelTrue[sentence][ap.First];
                                    else
                                        DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, j)] *= NumberOfLabel1TrueLabel2False[sentence][ap] / NumberOfLabelTrue[sentence][ap.First];
                                }
                                else
                                {
                                    DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, j)] *= 0;
                                    break;
                                }
                            }
                            else
                            {
                                if (annotation.Labels[ap.Second])
                                    DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, j)] *= (NumberOfLabel1TrueLabel2True[sentence][ap] + smoothingNumber[DTDDSVariable.SmoothTree][0]) / (NumberOfLabelTrue[sentence][ap.First] + smoothingNumber[DTDDSVariable.SmoothTree][1]);
                                else
                                    DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, j)] *= (NumberOfLabel1TrueLabel2False[sentence][ap] + smoothingNumber[DTDDSVariable.SmoothTree][0]) / (NumberOfLabelTrue[sentence][ap.First] + smoothingNumber[DTDDSVariable.SmoothTree][1]);
                            }
                        }
                        else
                        {
                            if (DTDDSVariable.SmoothTree == Smoothing.None)
                            {
                                if (NumberOfLabelFalse[sentence][ap.First] != 0)//考虑分母为0的情况
                                {
                                    if (annotation.Labels[ap.Second])
                                        DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, j)] *= NumberOfLabel1FalseLabel2True[sentence][ap] / NumberOfLabelFalse[sentence][ap.First];
                                    else
                                        DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, j)] *= NumberOfLabel1FalseLabel2False[sentence][ap] / NumberOfLabelFalse[sentence][ap.First];

                                }
                                else
                                {
                                    DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, j)] *= 0;
                                    break;
                                }
                            }
                            else
                            {
                                if (annotation.Labels[ap.Second])
                                    DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, j)] *= (NumberOfLabel1FalseLabel2True[sentence][ap] + smoothingNumber[DTDDSVariable.SmoothTree][0]) / (NumberOfLabelFalse[sentence][ap.First] + smoothingNumber[DTDDSVariable.SmoothTree][1]);
                                else
                                    DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, j)] *= (NumberOfLabel1FalseLabel2False[sentence][ap] + smoothingNumber[DTDDSVariable.SmoothTree][0]) / (NumberOfLabelFalse[sentence][ap.First] + smoothingNumber[DTDDSVariable.SmoothTree][1]);
                            }
                        }
                    }
                }
            }
            //for (int i = 0; i < Variable.Sentences.Count; ++i)
            //{
            //    double all = 0;
            //    for (int j = 0; j < DependentVariable.NumberOfIntlabel; ++j)
            //    {
            //        all += DTDDSVariable.Sij.Value[sentence][new Labelset(bilabel.ToArray(), j)];
            //    }
            //    for (int j = 0; j < DependentVariable.NumberOfIntlabel; ++j)
            //    {
            //        DTDDSVariable.Sij.Value[sentence][new Labelset(bilabel.ToArray(), j)] /= all;
            //    }
            //}
            #endregion
            //Variable.OutputFile.WriteLine(DTDDSVariable.Sij.ToString(DependentVariable.NumberOfIntlabel));
            //Variable.OutputFile.Close();
        }
    }
}