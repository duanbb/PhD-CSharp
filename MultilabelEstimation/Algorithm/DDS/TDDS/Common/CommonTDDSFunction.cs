using MultilabelEstimation.Group;
using MultilabelEstimation.Consistency;
using System;
using System.Collections.Generic;

namespace MultilabelEstimation.Algorithm.DDS.TDDS.Common
{
    class CommonTDDSFunction
    {
        static public void RunTDDS()
        {
            for (int groupIndex = 0; groupIndex < GroupVariable.AnnotatorGroups.Length; ++groupIndex)
            {
                Initialize(groupIndex);
                IList<double> Pdatas = new List<double>();
                for (int time = 1; time <= Variable.ConvergeTimeThreshold; ++time)
                {
                    //计算Pk
                    //计算Pk，mcj（consistent：角色c有j标签的概率）
                    ChoiceFunction.PriorPj(ref TDDSVariable.Pj, ref TDDSVariable.Mcj, TDDSVariable.Sij, time);
                    //计算π
                    TDDSVariable.PAkjl = CoreFunction.CalculatePAkjl(Variable.LabelArray, TDDSVariable.Sij, time, groupIndex);
                    //计算Sij
                    //if (CoreFunction.CalculatePdataAndSij(Variable.LabelArray, ref TDDSVariable.Sij, TDDSVariable.Pj, TDDSVariable.Pajl, TDDSVariable.Mcj, ref TDDSVariable.Pdata, groupIndex, Pdatas, 
                    //    TDDSVariable.ConditionalPj, TDDSVariable.ConditionalMcj))
                        //break;
                }
                DDSFunction.ObtainBinaryResult(TDDSVariable.Sij, "TDDS", groupIndex);
                DDSFunction.ObtainNumericResult(TDDSVariable.Sij, "TDDS", groupIndex);
                Function.WriteBinaryResultFile("TDDS", groupIndex);
            }
        }

        static private void Initialize(int group)
        {
            Label[] labelArray = GroupFunction.DescendLabelsByNumber(group);
            #region 初始化
            TDDSVariable.Sij = new Sij(1);
            //整体的信息，用于构造互信息参数，求树（所有句子一棵树）
            IDictionary<Label, double> labelFloatDic = new Dictionary<Label, double>();
            IDictionary<LabelPair, double> labelPairFloat = new Dictionary<LabelPair, double>();//前后无序，45个，用于初始化
            //Function.InitializeEmptyLabelDic(ref labelFloatDic, ref labelPairFloat, labelArray);
            IDictionary<Label, double> numberOfLabelTrue = new Dictionary<Label, double>(labelFloatDic);
            IDictionary<Label, double> numberOfLabelFalse = new Dictionary<Label, double>(labelFloatDic);
            IDictionary<LabelPair, double> numberOfLabel1TrueLabel2True = new Dictionary<LabelPair, double>(labelPairFloat);
            IDictionary<LabelPair, double> numberOfLabel1TrueLabel2False = new Dictionary<LabelPair, double>(labelPairFloat);
            IDictionary<LabelPair, double> numberOfLabel1FalseLabel2True = new Dictionary<LabelPair, double>(labelPairFloat);
            IDictionary<LabelPair, double> numberOfLabel1FalseLabel2False = new Dictionary<LabelPair, double>(labelPairFloat);

            IDictionary<Sentence, IDictionary<Label, double>> ProbabilityOfLabelTrue = new Dictionary<Sentence, IDictionary<Label, double>>();
            IDictionary<Sentence, IDictionary<Label, double>> ProbabilityOfLabelFalse = new Dictionary<Sentence, IDictionary<Label, double>>();
            IDictionary<Sentence, IDictionary<LabelPair, double>> ProbabilityOfLabel1TrueLabel2True = new Dictionary<Sentence, IDictionary<LabelPair, double>>();
            IDictionary<Sentence, IDictionary<LabelPair, double>> ProbabilityOfLabel1TrueLabel2False = new Dictionary<Sentence, IDictionary<LabelPair, double>>();
            IDictionary<Sentence, IDictionary<LabelPair, double>> ProbabilityOfLabel1FalseLabel2True = new Dictionary<Sentence, IDictionary<LabelPair, double>>();
            IDictionary<Sentence, IDictionary<LabelPair, double>> ProbabilityOfLabel1FalseLabel2False = new Dictionary<Sentence, IDictionary<LabelPair, double>>();

            //每句的信息，用于树的具体值（每个句子对应的树的值不同）
            foreach (Sentence sentence in Variable.Sentences)
            {
                ProbabilityOfLabelTrue.Add(sentence, new Dictionary<Label, double>(labelFloatDic));
                ProbabilityOfLabelFalse.Add(sentence, new Dictionary<Label, double>(labelFloatDic));
                ProbabilityOfLabel1TrueLabel2True.Add(sentence, new Dictionary<LabelPair, double>(labelPairFloat));
                ProbabilityOfLabel1TrueLabel2False.Add(sentence, new Dictionary<LabelPair, double>(labelPairFloat));
                ProbabilityOfLabel1FalseLabel2True.Add(sentence, new Dictionary<LabelPair, double>(labelPairFloat));
                ProbabilityOfLabel1FalseLabel2False.Add(sentence, new Dictionary<LabelPair, double>(labelPairFloat));
            }
            #endregion
            #region 求互信息的参数
            int N = 0;
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (Annotation annotation in sentence.AnnotaitonGroups[group].AnnotatorAnnotationDic.Values)
                {
                    ++N;
                    IList<Label> traversedLabels = new List<Label>();
                    foreach (Label label1 in labelArray)
                    {
                        traversedLabels.Add(label1);
                        if (annotation.Labels[label1])
                        {
                            ++numberOfLabelTrue[label1];
                            ++ProbabilityOfLabelTrue[sentence][label1];
                            foreach (Label label2 in labelArray)
                            {
                                if (!traversedLabels.Contains(label2))
                                {
                                    if (annotation.Labels[label2])
                                    {
                                        ++numberOfLabel1TrueLabel2True[new LabelPair(label1, label2)];
                                        ++ProbabilityOfLabel1TrueLabel2True[sentence][new LabelPair(label1, label2)];
                                    }
                                    else
                                    {
                                        ++numberOfLabel1TrueLabel2False[new LabelPair(label1, label2)];
                                        ++ProbabilityOfLabel1TrueLabel2False[sentence][new LabelPair(label1, label2)];
                                    }
                                }
                            }
                        }
                        else
                        {
                            ++numberOfLabelFalse[label1];
                            ++ProbabilityOfLabelFalse[sentence][label1];
                            foreach (Label label2 in labelArray)
                            {
                                if (!traversedLabels.Contains(label2))
                                {
                                    if (annotation.Labels[label2])
                                    {
                                        ++numberOfLabel1FalseLabel2True[new LabelPair(label1, label2)];
                                        ++ProbabilityOfLabel1FalseLabel2True[sentence][new LabelPair(label1, label2)];
                                    }
                                    else
                                    {
                                        ++numberOfLabel1FalseLabel2False[new LabelPair(label1, label2)];
                                        ++ProbabilityOfLabel1FalseLabel2False[sentence][new LabelPair(label1, label2)];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            #region 求树（全部一棵树）
            IList<KeyValuePair<LabelPair, double>> tree = TDDSFunction.GenerateIMTree(numberOfLabelTrue, numberOfLabelFalse,
                numberOfLabel1TrueLabel2True, numberOfLabel1TrueLabel2False,
                numberOfLabel1FalseLabel2True, numberOfLabel1FalseLabel2False, N, labelArray);//此处是导致多线程结果不同的原因：虽然换组时Variable.LabelArray不会变化，但原先sentence中用于CommonTree和DistinctTree计算的成员属性没有做区分。
            #endregion
            #region 初始化Sij
            if (TDDSVariable.SmoothTree == Smoothing.None)
            {
                foreach (Sentence sentence in Variable.Sentences)
                {
                    foreach (Label label in labelArray)
                    {
                        ProbabilityOfLabelTrue[sentence][label] /= Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                        ProbabilityOfLabelFalse[sentence][label] /= Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                    }
                    foreach (LabelPair labelPair in labelPairFloat.Keys)
                    {
                        ProbabilityOfLabel1TrueLabel2True[sentence][labelPair] /= Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                        ProbabilityOfLabel1TrueLabel2False[sentence][labelPair] /= Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                        ProbabilityOfLabel1FalseLabel2True[sentence][labelPair] /= Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                        ProbabilityOfLabel1FalseLabel2False[sentence][labelPair] /= Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                    }
                }
            }
            else
            {
                IDictionary<Smoothing, double[]> smoothingNumber = Function.SmoothingNumber(Variable.NumberOfAnnotationsPerSentenceAfterGrouping);
                foreach (Sentence sentence in Variable.Sentences)
                {
                    foreach (Label label in labelArray)
                    {
                        ProbabilityOfLabelTrue[sentence][label] = (ProbabilityOfLabelTrue[sentence][label] + smoothingNumber[TDDSVariable.SmoothTree][0]) / (Variable.NumberOfAnnotationsPerSentenceAfterGrouping + smoothingNumber[TDDSVariable.SmoothTree][1]);
                        ProbabilityOfLabelFalse[sentence][label] = (ProbabilityOfLabelFalse[sentence][label] + smoothingNumber[TDDSVariable.SmoothTree][0]) / (Variable.NumberOfAnnotationsPerSentenceAfterGrouping + smoothingNumber[TDDSVariable.SmoothTree][1]); ;
                    }
                    foreach (LabelPair labelPair in labelPairFloat.Keys)
                    {
                        ProbabilityOfLabel1TrueLabel2True[sentence][labelPair] = (ProbabilityOfLabel1TrueLabel2True[sentence][labelPair] + smoothingNumber[TDDSVariable.SmoothTree][0]) / (Variable.NumberOfAnnotationsPerSentenceAfterGrouping + smoothingNumber[TDDSVariable.SmoothTree][1]);
                        ProbabilityOfLabel1TrueLabel2False[sentence][labelPair] = (ProbabilityOfLabel1TrueLabel2False[sentence][labelPair] + smoothingNumber[TDDSVariable.SmoothTree][0]) / (Variable.NumberOfAnnotationsPerSentenceAfterGrouping + smoothingNumber[TDDSVariable.SmoothTree][1]);
                        ProbabilityOfLabel1FalseLabel2True[sentence][labelPair] = (ProbabilityOfLabel1FalseLabel2True[sentence][labelPair] + smoothingNumber[TDDSVariable.SmoothTree][0]) / (Variable.NumberOfAnnotationsPerSentenceAfterGrouping + smoothingNumber[TDDSVariable.SmoothTree][1]);
                        ProbabilityOfLabel1FalseLabel2False[sentence][labelPair] = (ProbabilityOfLabel1FalseLabel2False[sentence][labelPair] + smoothingNumber[TDDSVariable.SmoothTree][0]) / (Variable.NumberOfAnnotationsPerSentenceAfterGrouping + smoothingNumber[TDDSVariable.SmoothTree][1]);
                    }
                }
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                for (int l = 0; l < Math.Pow(2, Variable.LabelArray.Length); ++l)
                {
                    Labelset Labelset = new Labelset(Variable.LabelArray, l);
                    TDDSVariable.Sij.Value[sentence].Add(Labelset, 1);
                    if (TDDSVariable.SmoothTree == Smoothing.None)
                    {
                        if (Labelset.Labels[tree[0].Key.First])
                        {
                            if (ProbabilityOfLabelTrue[sentence][tree[0].Key.First] != 0)
                                TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabelTrue[sentence][tree[0].Key.First];//应该除，除后准确率更高，原因未知
                        }
                        else
                        {
                            if (ProbabilityOfLabelFalse[sentence][tree[0].Key.First] != 0)
                                TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabelFalse[sentence][tree[0].Key.First];
                        }
                    }
                    else
                    {
                        if (Labelset.Labels[tree[0].Key.First])
                            TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabelTrue[sentence][tree[0].Key.First];
                        else
                            TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabelFalse[sentence][tree[0].Key.First];
                    }
                    foreach (KeyValuePair<LabelPair, double> labelPairAndValue in tree)
                    {
                        LabelPair labelPair = labelPairAndValue.Key;
                        if (TDDSVariable.SmoothTree == Smoothing.None)
                        {
                            if (Labelset.Labels[labelPair.First])
                            {
                                if (ProbabilityOfLabelTrue[sentence][labelPair.First] != 0)//考虑分母为0的情况
                                {
                                    if (Labelset.Labels[labelPair.Second])
                                    {
                                        if (ProbabilityOfLabel1TrueLabel2True[sentence].ContainsKey(labelPair))
                                            TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabel1TrueLabel2True[sentence][labelPair] / ProbabilityOfLabelTrue[sentence][labelPair.First];
                                        else
                                            TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabel1TrueLabel2True[sentence][labelPair.Reverse] / ProbabilityOfLabelTrue[sentence][labelPair.First];
                                    }
                                    else
                                    {
                                        if (ProbabilityOfLabel1TrueLabel2False[sentence].ContainsKey(labelPair))
                                            TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabel1TrueLabel2False[sentence][labelPair] / ProbabilityOfLabelTrue[sentence][labelPair.First];
                                        else
                                            TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabel1FalseLabel2True[sentence][labelPair.Reverse] / ProbabilityOfLabelTrue[sentence][labelPair.First];
                                    }
                                }
                                else
                                {
                                    TDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= 0;//此处应该是0，不是1
                                    break;
                                }
                            }
                            else
                            {
                                if (ProbabilityOfLabelFalse[sentence][labelPair.First] != 0)//考虑分母为0的情况
                                {
                                    if (Labelset.Labels[labelPair.Second])
                                    {
                                        if (ProbabilityOfLabel1FalseLabel2True[sentence].ContainsKey(labelPair))
                                            TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabel1FalseLabel2True[sentence][labelPair] / ProbabilityOfLabelFalse[sentence][labelPair.First];
                                        else
                                            TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabel1TrueLabel2False[sentence][labelPair.Reverse] / ProbabilityOfLabelFalse[sentence][labelPair.First];
                                    }
                                    else
                                    {
                                        if (ProbabilityOfLabel1FalseLabel2False[sentence].ContainsKey(labelPair))
                                            TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabel1FalseLabel2False[sentence][labelPair] / ProbabilityOfLabelFalse[sentence][labelPair.First];
                                        else
                                            TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabel1FalseLabel2False[sentence][labelPair.Reverse] / ProbabilityOfLabelFalse[sentence][labelPair.First];
                                    }
                                }
                                else
                                {
                                    TDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= 0;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (Labelset.Labels[labelPair.First])
                            {
                                if (Labelset.Labels[labelPair.Second])
                                {
                                    if (ProbabilityOfLabel1TrueLabel2True[sentence].ContainsKey(labelPair))
                                        TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabel1TrueLabel2True[sentence][labelPair] / ProbabilityOfLabelTrue[sentence][labelPair.First];
                                    else
                                        TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabel1TrueLabel2True[sentence][labelPair.Reverse] / ProbabilityOfLabelTrue[sentence][labelPair.First];
                                }
                                else
                                {
                                    if (ProbabilityOfLabel1TrueLabel2False[sentence].ContainsKey(labelPair))
                                        TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabel1TrueLabel2False[sentence][labelPair] / ProbabilityOfLabelTrue[sentence][labelPair.First];
                                    else
                                        TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabel1FalseLabel2True[sentence][labelPair.Reverse] / ProbabilityOfLabelTrue[sentence][labelPair.First];
                                }
                            }
                            else
                            {
                                if (Labelset.Labels[labelPair.Second])
                                {
                                    if (ProbabilityOfLabel1FalseLabel2True[sentence].ContainsKey(labelPair))
                                        TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabel1FalseLabel2True[sentence][labelPair] / ProbabilityOfLabelFalse[sentence][labelPair.First];
                                    else
                                        TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabel1TrueLabel2False[sentence][labelPair.Reverse] / ProbabilityOfLabelFalse[sentence][labelPair.First];
                                }
                                else
                                {
                                    if (ProbabilityOfLabel1FalseLabel2False[sentence].ContainsKey(labelPair))
                                        TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabel1FalseLabel2False[sentence][labelPair] / ProbabilityOfLabelFalse[sentence][labelPair.First];
                                    else
                                        TDDSVariable.Sij.Value[sentence][Labelset] *= ProbabilityOfLabel1FalseLabel2False[sentence][labelPair.Reverse] / ProbabilityOfLabelFalse[sentence][labelPair.First];
                                }
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
            //        all += TDDSVariable.Sij.Value[sentence][new Labelset(bilabel.ToArray(), j)];
            //    }
            //    for (int j = 0; j < DependentVariable.NumberOfIntlabel; ++j)
            //    {
            //        TDDSVariable.Sij.Value[sentence][new Labelset(bilabel.ToArray(), j)] /= all;
            //    }
            //}
            #endregion
            //Variable.OutputFile.WriteLine(TDDSVariable.Sij.ToString(DependentVariable.NumberOfIntlabel));
            //Variable.OutputFile.Close();
            //double[] ii = new double[Variable.Sentences.Count];
            //foreach (Sentence Sentence in Variable.Sentences)
            //{
            //    for (int l = 0; l < DependentVariable.NumberOfIntlabel; ++l)
            //    {
            //        ii[Sentence.ID] += TDDSVariable.Sij.Value[Sentence.ID, l];
            //    }
            //}
        }
    }
}