using MultilabelEstimation.Group;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultilabelEstimation.Algorithm.PDS
{
    static class PDSFunction
    {
        static public void RunPDS()
        {
            for (int groupIndex = 0; groupIndex < GroupVariable.AnnotatorGroups.Length; ++groupIndex)
            {
                foreach (Sentence sentence in Variable.Sentences)
                {
                    sentence.AnnotaitonGroups[groupIndex].PDSNumResult = new NumericResult();
                    sentence.AnnotaitonGroups[groupIndex].PDSResult = new Result();
                }
                LabelPair[] bilabels = GenerateBilabels(groupIndex);
                foreach (LabelPair bilabel in bilabels)
                {
                    Sij sij = CoreFunction.InitializeSij(bilabel.ToArray(), groupIndex);
                    CoreFunction.Intgerate(bilabel.ToArray(), groupIndex, ref sij);
                    ObtainLabelResult(bilabel, groupIndex, sij);
                }
                Function.WriteBinaryResultFile("PDS", groupIndex);
            }
        }

        static public LabelPair[] GenerateBilabels(int group)
        {
            Label[] labelArray = GroupFunction.DescendLabelsByNumber(group);
            #region 初始化，用于构造互信息参数
            IDictionary<Label, double> labelFloatDic = new Dictionary<Label, double>();
            IList<LabelPair> labelPairList = new List<LabelPair>();//前后无序，45个，用于初始化
            Function.InitializeEmptyLabelDic(ref labelFloatDic, ref labelPairList, labelArray);
            IDictionary<Label, double> numberOfLabelTrue = new Dictionary<Label, double>(labelFloatDic);
            IDictionary<Label, double> numberOfLabelFalse = new Dictionary<Label, double>(labelFloatDic);
            #endregion
            #region 求互信息的参数
            IList<Label> traversedLabels = new List<Label>();
            int N = 0;
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (Annotation annotation in sentence.AnnotaitonGroups[group].AnnotatorAnnotationDic.Values)
                {
                    ++N;
                    traversedLabels.Clear();
                    foreach (Label label1 in labelArray)
                    {
                        traversedLabels.Add(label1);
                        if (annotation.Labels[label1])
                        {
                            ++numberOfLabelTrue[label1];
                            foreach (Label label2 in labelArray)
                            {
                                if (!traversedLabels.Contains(label2))
                                {
                                    LabelPair labelPair = labelPairList.First(lp => lp.First == label1 && lp.Second == label2);
                                    if (annotation.Labels[label2])
                                    {
                                        if (labelPair.Label1TrueLabel2TrueSentenceAndFreq.ContainsKey(sentence))
                                            ++labelPair.Label1TrueLabel2TrueSentenceAndFreq[sentence];
                                        else
                                            labelPair.Label1TrueLabel2TrueSentenceAndFreq.Add(sentence, 1);
                                    }
                                    else
                                    {
                                        if (labelPair.Label1TrueLabel2FalseSentenceAndFreq.ContainsKey(sentence))
                                            ++labelPair.Label1TrueLabel2FalseSentenceAndFreq[sentence];
                                        else
                                            labelPair.Label1TrueLabel2FalseSentenceAndFreq.Add(sentence, 1);
                                    }
                                }
                            }
                        }
                        else
                        {
                            ++numberOfLabelFalse[label1];
                            foreach (Label label2 in labelArray)
                            {
                                if (!traversedLabels.Contains(label2))
                                {
                                    LabelPair labelPair = labelPairList.First(lp => lp.First == label1 && lp.Second == label2);
                                    if (annotation.Labels[label2])
                                    {
                                        if (labelPair.Label1FalseLabel2TrueSentenceAndFreq.ContainsKey(sentence))
                                            ++labelPair.Label1FalseLabel2TrueSentenceAndFreq[sentence];
                                        else
                                            labelPair.Label1FalseLabel2TrueSentenceAndFreq.Add(sentence, 1);
                                    }
                                    else
                                    {
                                        if (labelPair.Label1FalseLabel2FalseSentenceAndFreq.ContainsKey(sentence))
                                            ++labelPair.Label1FalseLabel2FalseSentenceAndFreq[sentence];
                                        else
                                            labelPair.Label1FalseLabel2FalseSentenceAndFreq.Add(sentence, 1);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            #region 情感对排序，论文用
            //List<LabelPair> sortednumberOfLabel1TrueLabel2True = new List<LabelPair>(labelPairList);//变量名为MI，其实值为joint entropy
            //sortednumberOfLabel1TrueLabel2True.Sort(delegate(LabelPair s1, LabelPair s2)
            //{
            //    return s2.Label1TrueLabel2TrueFrequency.CompareTo(s1.Label1TrueLabel2TrueFrequency);
            //});
            //List<LabelPair> sortednumberOfLabel1TrueLabel2False = new List<LabelPair>(labelPairList);//变量名为MI，其实值为joint entropy
            //sortednumberOfLabel1TrueLabel2False.Sort(delegate(LabelPair s1, LabelPair s2)
            //{
            //    return s2.Label1TrueLabel2FalseFrequency.CompareTo(s1.Label1TrueLabel2FalseFrequency);
            //});
            //List<LabelPair> sortednumberOfLabel1FalseLabel2True = new List<LabelPair>(labelPairList);//变量名为MI，其实值为joint entropy
            //sortednumberOfLabel1FalseLabel2True.Sort(delegate(LabelPair s1, LabelPair s2)
            //{
            //    return s2.Label1FalseLabel2TrueFrequency.CompareTo(s1.Label1FalseLabel2TrueFrequency);
            //});
            //List<LabelPair> sortednumberOfLabel1FalseLabel2False = new List<LabelPair>(labelPairList);//变量名为MI，其实值为joint entropy
            //sortednumberOfLabel1FalseLabel2False.Sort(delegate(LabelPair s1, LabelPair s2)
            //{
            //    return s2.Label1FalseLabel2FalseFrequency.CompareTo(s1.Label1FalseLabel2FalseFrequency);
            //});
            #endregion
            #region 求联合熵
            for (int i = 0; i < labelPairList.Count; ++i)
            {
                if (labelPairList[i].Label1TrueLabel2TrueFrequency != 0)
                    labelPairList[i].Weight += -(labelPairList[i].Label1TrueLabel2TrueFrequency / N) * Math.Log((labelPairList[i].Label1TrueLabel2TrueFrequency / N), 2);
                if (labelPairList[i].Label1TrueLabel2FalseFrequency != 0)
                    labelPairList[i].Weight += -(labelPairList[i].Label1TrueLabel2FalseFrequency / N) * Math.Log((labelPairList[i].Label1TrueLabel2FalseFrequency / N), 2);
                if (labelPairList[i].Label1FalseLabel2TrueFrequency != 0)
                    labelPairList[i].Weight += -(labelPairList[i].Label1FalseLabel2TrueFrequency / N) * Math.Log((labelPairList[i].Label1FalseLabel2TrueFrequency / N), 2);
                if (labelPairList[i].Label1FalseLabel2FalseFrequency != 0)
                    labelPairList[i].Weight += -(labelPairList[i].Label1FalseLabel2FalseFrequency / N) * Math.Log((labelPairList[i].Label1FalseLabel2FalseFrequency / N), 2);
            }
            List<LabelPair> sortedMutualInformation = new List<LabelPair>(labelPairList);//变量名为MI，其实值为joint entropy；排序仅为观察用
            sortedMutualInformation.Sort(delegate(LabelPair s1, LabelPair s2)
            {
                return s1.Weight.CompareTo(s2.Weight);
            });
            #endregion
            return MinimumWeightedPerfectMatching(labelPairList, true);
        }

        //求最佳匹配
        static public LabelPair[] MinimumWeightedPerfectMatching(IList<LabelPair> labelPairList, bool isMinimum)
        {
            #region minimum weighted perfect matching
            IList<LabelPairMatching> bilabelMatchings = new List<LabelPairMatching>();
            IList<Label> traversedLabels = new List<Label>();
            for (int a = 0; a < labelPairList.Count; ++a)
            {
                traversedLabels.Clear();
                traversedLabels.Add(labelPairList[a].First);
                traversedLabels.Add(labelPairList[a].Second);
                for (int b = a + 1; b < labelPairList.Count; ++b)
                {
                    if (!traversedLabels.Contains(labelPairList[b].First) && !traversedLabels.Contains(labelPairList[b].Second))
                    {
                        if (Variable.LabelArray.Length == 4)//SnowFestival
                        {
                            bilabelMatchings.Add(new LabelPairMatching(labelPairList[a], labelPairList[b]));
                            break;
                        }
                        traversedLabels.Add(labelPairList[b].First);
                        traversedLabels.Add(labelPairList[b].Second);
                        for (int c = b + 1; c < labelPairList.Count; ++c)
                        {
                            if (!traversedLabels.Contains(labelPairList[c].First) && !traversedLabels.Contains(labelPairList[c].Second))
                            {
                                traversedLabels.Add(labelPairList[c].First);
                                traversedLabels.Add(labelPairList[c].Second);
                                for (int d = c + 1; d < labelPairList.Count; ++d)
                                {
                                    if (!traversedLabels.Contains(labelPairList[d].First) && !traversedLabels.Contains(labelPairList[d].Second))
                                    {
                                        traversedLabels.Add(labelPairList[d].First);
                                        traversedLabels.Add(labelPairList[d].Second);
                                        for (int e = d + 1; e < labelPairList.Count; ++e)
                                        {
                                            if (!traversedLabels.Contains(labelPairList[e].First) && !traversedLabels.Contains(labelPairList[e].Second))
                                            {
                                                bilabelMatchings.Add(new LabelPairMatching(labelPairList[a], labelPairList[b], labelPairList[c], labelPairList[d], labelPairList[e]));
                                                break;
                                            }
                                        }
                                        traversedLabels.Remove(labelPairList[d].First);
                                        traversedLabels.Remove(labelPairList[d].Second);
                                    }
                                }
                                traversedLabels.Remove(labelPairList[c].First);
                                traversedLabels.Remove(labelPairList[c].Second);
                            }
                        }
                        traversedLabels.Remove(labelPairList[b].First);
                        traversedLabels.Remove(labelPairList[b].Second);
                    }
                }
            }
            //List<LabelPairMatching> sortedBilabelMatchings = new List<LabelPairMatching>(bilabelMatchings);
            //sortedBilabelMatchings.Sort(delegate(LabelPairMatching s1, LabelPairMatching s2)
            //{
            //    return s2.Weight.CompareTo(s1.Weight);
            //});
            #endregion
            return isMinimum ? bilabelMatchings.Min().bilabels.ToArray(): bilabelMatchings.Max().bilabels.ToArray();
        }

        static private void ObtainLabelResult(LabelPair bilabel, int group, Sij sij)
        {
            foreach (Sentence sentence in Variable.Sentences)
            {
                //得到numeric结果
                foreach (Labelset labelset in sij.Value[sentence].Keys)
                {
                    foreach (Label label in labelset.Labels.Keys)//其实就一个Label
                    {
                        if (labelset.Labels[label])
                            sentence.AnnotaitonGroups[group].PDSNumResult.Labels[label] = sij.Value[sentence][labelset];
                    }
                }
                //得到binary结果
                KeyValuePair<Labelset, double> resultAndProbability = sij.CalculateJointBestLabelset(sentence);
                foreach (Label label in resultAndProbability.Key.Labels.Keys)
                {
                    sentence.AnnotaitonGroups[group].PDSResult.Labels[label] = resultAndProbability.Key.Labels[label];
                }
                sentence.AnnotaitonGroups[group].PDSResult.Probability *= resultAndProbability.Value;
            }
        }
    }
}