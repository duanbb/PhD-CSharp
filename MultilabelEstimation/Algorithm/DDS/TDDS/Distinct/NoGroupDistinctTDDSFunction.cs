using System;
using System.Collections.Generic;

namespace MultilabelEstimation.Algorithm.DDS.TDDS.Distinct
{
    static class NoGroupDistinctTDDSFunction
    {
        static public void GenerateOneTreeForSenResult()
        {
            Initialize();
            for (int time = 1; time <= Variable.ConvergeTimeThreshold; ++time)//其中几个函数可以和Dependent公用
            {
                //计算Pk
                DTDDSVariable.Pj = CoreFunction.CalculatePj(DTDDSVariable.Sij, time);
                //计算π
                NoGroupDDSFunction.CalculatePAkjl(DTDDSVariable.Sij, ref DTDDSVariable.PAkjl);
                //计算Sij
                if (NoGroupDDSFunction.CalculatePdataAndSij(ref DTDDSVariable.Sij, DTDDSVariable.Pj, DTDDSVariable.PAkjl, ref DTDDSVariable.Pdata))
                {
                    break;
                }
            }
            NoGroupDDSFunction.ObtainResult(DTDDSVariable.Sij, "TreeForSen");
        }

        static private void Initialize()//运行过TreeForAll，就不用再计算每句的树了
        {
            #region 初始化
            DTDDSVariable.Sij = new Sij(1);
            //每句的信息，用于树的具体值（每个句子对应的树的值不同
            IDictionary<Label, double> labelFloatDic = new Dictionary<Label, double>();
            IDictionary<LabelPair, double> labelPairFloat = new Dictionary<LabelPair, double>();//前后无序，45个，用于初始化
            //Function.InitializeEmptyLabelDic(ref labelFloatDic, ref labelPairFloat, Variable.LabelArray);

            IDictionary<Sentence, IDictionary<Label, double>> NumberOfLabelTrue = new Dictionary<Sentence, IDictionary<Label, double>>();
            IDictionary<Sentence, IDictionary<Label, double>> NumberOfLabelFalse = new Dictionary<Sentence, IDictionary<Label, double>>();
            IDictionary<Sentence, IDictionary<LabelPair, double>> NumberOfLabel1TrueLabel2True = new Dictionary<Sentence, IDictionary<LabelPair, double>>();
            IDictionary<Sentence, IDictionary<LabelPair, double>> NumberOfLabel1TrueLabel2False = new Dictionary<Sentence, IDictionary<LabelPair, double>>();
            IDictionary<Sentence, IDictionary<LabelPair, double>> NumberOfLabel1FalseLabel2True = new Dictionary<Sentence, IDictionary<LabelPair, double>>();
            IDictionary<Sentence, IDictionary<LabelPair, double>> NumberOfLabel1FalseLabel2False = new Dictionary<Sentence, IDictionary<LabelPair, double>>();
            foreach (Sentence sentence in Variable.Sentences)
            {
                NumberOfLabelTrue.Add(sentence, new Dictionary<Label, double>(labelFloatDic));
                NumberOfLabelFalse.Add(sentence, new Dictionary<Label, double>(labelFloatDic));
                NumberOfLabel1TrueLabel2True.Add(sentence, new Dictionary<LabelPair, double>(labelPairFloat));
                NumberOfLabel1TrueLabel2False.Add(sentence, new Dictionary<LabelPair, double>(labelPairFloat));
                NumberOfLabel1FalseLabel2True.Add(sentence, new Dictionary<LabelPair, double>(labelPairFloat));
                NumberOfLabel1FalseLabel2False.Add(sentence, new Dictionary<LabelPair, double>(labelPairFloat));
            }
            #endregion
            #region 求互信息的参数
            foreach (Annotator annotator in Variable.Annotators)
            {
                foreach (Sentence sentence in Variable.Sentences)
                {
                    foreach (Annotation annotation in Variable.Data[annotator][sentence])
                    {
                        IList<Label> traversedLabels = new List<Label>();
                        foreach (Label label1 in Variable.LabelArray)
                        {
                            traversedLabels.Add(label1);
                            if (annotation.Labels[label1])
                            {
                                ++NumberOfLabelTrue[sentence][label1];
                                foreach (Label label2 in Variable.LabelArray)
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
                                foreach (Label label2 in Variable.LabelArray)
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
            }
            #endregion
            #region 为每个句子生成树
            foreach (Sentence sentence in Variable.Sentences)
            {
                IDictionary<Label, int> numberOfEachLabel = new Dictionary<Label, int>();
                foreach (Label label in Variable.LabelArray)
                {
                    numberOfEachLabel.Add(label, 0);
                }
                foreach (Annotator annotator in Variable.Annotators)
                {
                    foreach (Annotation annotation in Variable.Data[annotator][sentence])
                    {
                        foreach (Label label in Variable.LabelArray)
                        {
                            if (annotation.Labels[label])
                            {
                                ++numberOfEachLabel[label];
                            }
                        }
                    }
                }
                List<KeyValuePair<Label, int>> sortedLabel = new List<KeyValuePair<Label, int>>(numberOfEachLabel);
                sortedLabel.Sort(delegate(KeyValuePair<Label, int> s1, KeyValuePair<Label, int> s2)
                {
                    return s2.Value.CompareTo(s1.Value);
                });
                sentence.LabelArray = new Label[Variable.LabelArray.Length];
                for (int a = 0; a < Variable.LabelArray.Length; ++a)
                {
                    sentence.LabelArray[a] = sortedLabel[a].Key;
                }
                sentence.Tree = TDDSFunction.GenerateIMTree(NumberOfLabelTrue[sentence], NumberOfLabelFalse[sentence],
                    NumberOfLabel1TrueLabel2True[sentence], NumberOfLabel1TrueLabel2False[sentence],
                    NumberOfLabel1FalseLabel2True[sentence], NumberOfLabel1FalseLabel2False[sentence], Variable.NumberOfAnnotationsPerSentenceAfterGrouping, sentence.LabelArray);
            }
            #endregion
            //NumberOfIncompletedTreeSentence();
            #region 初始化Sij
            foreach (Sentence sentence in Variable.Sentences)
            {
                for (int l = 0; l < Math.Pow(2, Variable.LabelArray.Length); ++l)
                {
                    Annotation annotation = new Annotation(l);
                    if (annotation.Labels[sentence.Tree[0].Key.First])
                        DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] = NumberOfLabelTrue[sentence][sentence.Tree[0].Key.First] / Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                    else
                        DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] = NumberOfLabelFalse[sentence][sentence.Tree[0].Key.First] / Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                    foreach (KeyValuePair<LabelPair, double> ap in sentence.Tree)
                    {
                        LabelPair reverse = new LabelPair(ap.Key.Second, ap.Key.First);
                        if (annotation.Labels[ap.Key.First])
                        {
                            if (NumberOfLabelTrue[sentence][ap.Key.First] != 0)//考虑分母为0的情况
                            {
                                if (annotation.Labels[ap.Key.Second])
                                    if (NumberOfLabel1TrueLabel2True[sentence].ContainsKey(ap.Key))
                                        DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= NumberOfLabel1TrueLabel2True[sentence][ap.Key] / NumberOfLabelTrue[sentence][ap.Key.First];
                                    else
                                        DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= NumberOfLabel1TrueLabel2True[sentence][reverse] / NumberOfLabelTrue[sentence][ap.Key.First];
                                else if (NumberOfLabel1TrueLabel2False[sentence].ContainsKey(ap.Key))
                                    DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= NumberOfLabel1TrueLabel2False[sentence][ap.Key] / NumberOfLabelTrue[sentence][ap.Key.First];
                                else
                                    DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= NumberOfLabel1FalseLabel2True[sentence][reverse] / NumberOfLabelTrue[sentence][ap.Key.First];
                            }
                            else
                            {
                                DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= 0;
                                break;
                            }
                        }
                        else
                        {
                            if (NumberOfLabelFalse[sentence][ap.Key.First] != 0)//考虑分母为0的情况
                            {
                                if (annotation.Labels[ap.Key.Second])
                                    if (NumberOfLabel1FalseLabel2True[sentence].ContainsKey(ap.Key))
                                        DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= NumberOfLabel1FalseLabel2True[sentence][ap.Key] / NumberOfLabelFalse[sentence][ap.Key.First];
                                    else
                                        DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= NumberOfLabel1TrueLabel2False[sentence][reverse] / NumberOfLabelFalse[sentence][ap.Key.First];
                                else if (NumberOfLabel1FalseLabel2False[sentence].ContainsKey(ap.Key))
                                    DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= NumberOfLabel1FalseLabel2False[sentence][ap.Key] / NumberOfLabelFalse[sentence][ap.Key.First];
                                else
                                    DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= NumberOfLabel1FalseLabel2False[sentence][reverse] / NumberOfLabelFalse[sentence][ap.Key.First];
                            }
                            else
                            {
                                DTDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= 0;
                                break;
                            }
                        }
                    }
                }
            }
            #endregion
        }
        static public void NumberOfIncompletedTreeSentence()
        {
            int numberOfIncompletedTreeSentence = 0;//66
            int numberOfSentence = 0;//78
            foreach (Sentence sentence in Variable.Sentences)
            {
                ++numberOfSentence;
                foreach (KeyValuePair<LabelPair, double> ap in sentence.Tree)
                {
                    if (ap.Value == 0)
                    {
                        ++numberOfIncompletedTreeSentence;
                        break;
                    }
                }
            }
        }
    }
}