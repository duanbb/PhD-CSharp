using System.Collections.Generic;
using System;

namespace MultilabelEstimation.Algorithm.DDS.TDDS.Common
{
    static class NoGroupCommonTDDSFunction
    {
        static public void GenerateOneTreeForAllResult()
        {
            Initialize();
            for (int time = 1; time <= Variable.ConvergeTimeThreshold; ++time)
            {
                //计算Pk
                TDDSVariable.Pj = CoreFunction.CalculatePj(TDDSVariable.Sij, time);
                //计算π
                NoGroupDDSFunction.CalculatePAkjl(TDDSVariable.Sij, ref TDDSVariable.PAkjl);
                //计算Sij
                if (NoGroupDDSFunction.CalculatePdataAndSij(ref TDDSVariable.Sij, TDDSVariable.Pj, TDDSVariable.PAkjl, ref TDDSVariable.Pdata))
                    break;
            }
            NoGroupDDSFunction.ObtainResult(TDDSVariable.Sij, "TreeForAll");
        }

        static private void Initialize()
        {
            #region 初始化
            TDDSVariable.Sij = new Sij(1);
            //整体的信息，用于求树（所有句子一棵树）
            // 用于构造互信息参数
            IDictionary<Label, double> labelFloatDic = new Dictionary<Label, double>();
            IDictionary<LabelPair, double> labelPairFloat = new Dictionary<LabelPair, double>();//前后无序，45个，用于初始化
            //Function.InitializeEmptyLabelDic(ref labelFloatDic, ref labelPairFloat, Variable.LabelArray);
            
            IDictionary<Label, double> numberOfLabelTrue = new Dictionary<Label, double>(labelFloatDic);
            IDictionary<Label, double> numberOfLabelFalse = new Dictionary<Label, double>(labelFloatDic);
            IDictionary<LabelPair, double> numberOfLabel1TrueLabel2True = new Dictionary<LabelPair, double>(labelPairFloat);
            IDictionary<LabelPair, double> numberOfLabel1TrueLabel2False = new Dictionary<LabelPair, double>(labelPairFloat);
            IDictionary<LabelPair, double> numberOfLabel1FalseLabel2True = new Dictionary<LabelPair, double>(labelPairFloat);
            IDictionary<LabelPair, double> numberOfLabel1FalseLabel2False = new Dictionary<LabelPair, double>(labelPairFloat);

            IDictionary<Sentence,IDictionary<Label, double>> ProbabilityOfLabelTrue = new Dictionary<Sentence, IDictionary<Label, double>>();
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
            foreach (Annotator annotator in Variable.Annotators)
            {
                foreach (Sentence sentence in Variable.Sentences)
                {
                    foreach (Annotation annotation in Variable.Data[annotator][sentence])
                    {
                        ++N;
                        IList<Label> traversedLabels = new List<Label>();
                        foreach (Label label1 in Variable.LabelArray)
                        {
                            traversedLabels.Add(label1);
                            if (annotation.Labels[label1])
                            {
                                ++numberOfLabelTrue[label1];
                                ++ProbabilityOfLabelTrue[sentence][label1];
                                foreach (Label label2 in Variable.LabelArray)
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
                                foreach (Label label2 in Variable.LabelArray)
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
            }
            #endregion
            #region 求树（全部一棵树）
            IList<KeyValuePair<LabelPair, double>> tree = TDDSFunction.GenerateIMTree(numberOfLabelTrue, numberOfLabelFalse,
                numberOfLabel1TrueLabel2True, numberOfLabel1TrueLabel2False,
                numberOfLabel1FalseLabel2True, numberOfLabel1FalseLabel2False, N, Variable.LabelArray);
            #endregion
            #region 初始化Sij
            foreach (Sentence sentence in Variable.Sentences)
            {
                for (int l = 0; l < Math.Pow(2, Variable.LabelArray.Length); ++l)
                {
                    Annotation annotation = new Annotation(l);
                    if (annotation.Labels[tree[1].Key.First])
                        TDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] = ProbabilityOfLabelTrue[sentence][tree[0].Key.First] / Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                    else
                        TDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] = ProbabilityOfLabelFalse[sentence][tree[0].Key.First] / Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                    foreach (KeyValuePair<LabelPair, double> ap in tree)
                    {
                        if (annotation.Labels[ap.Key.First])
                        {
                            if (ProbabilityOfLabelTrue[sentence][ap.Key.First] != 0)//考虑分母为0的情况
                            {
                                if (annotation.Labels[ap.Key.Second])
                                    TDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= ProbabilityOfLabel1TrueLabel2True[sentence][ap.Key] / ProbabilityOfLabelTrue[sentence][ap.Key.First];
                                else
                                    TDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= ProbabilityOfLabel1TrueLabel2False[sentence][ap.Key] / ProbabilityOfLabelTrue[sentence][ap.Key.First];
                            }
                            else
                            {
                                TDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= 0;//此处应该是0，不是1
                                break;
                            }
                        }
                        else
                        {
                            if (ProbabilityOfLabelFalse[sentence][ap.Key.First] != 0)//考虑分母为0的情况
                            {
                                if (annotation.Labels[ap.Key.Second])
                                    TDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= ProbabilityOfLabel1FalseLabel2True[sentence][ap.Key] / ProbabilityOfLabelFalse[sentence][ap.Key.First];
                                else
                                    TDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= ProbabilityOfLabel1FalseLabel2False[sentence][ap.Key] / ProbabilityOfLabelFalse[sentence][ap.Key.First];
                            }
                            else
                            {
                                TDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] *= 0;
                                break;
                            }
                        }
                    }
                }
            }
            #endregion
        }
   }
}