using System;
using System.Collections.Generic;
using System.Linq;

namespace MultilabelEstimation.Algorithm.DDS.TDDS
{
    static class TDDSFunction
    {
        //生成树
        static public IList<KeyValuePair<LabelPair, double>> GenerateIMTree(IDictionary<Label, double> numberOfLabelTrue, IDictionary<Label, double> numberOfLabelFalse,
            IDictionary<LabelPair, double> numberOfLabel1TrueLabel2True, IDictionary<LabelPair, double> numberOfLabel1TrueLabel2False, IDictionary<LabelPair, double> numberOfLabel1FalseLabel2True,
            IDictionary<LabelPair, double> numberOfLabel1FalseLabel2False, int n, Label[] labelArray)//Dictinary的顺序不可控，所以返回值的类型必须是List<KeyValuePair>
        {
            #region 求互信息
            IDictionary<LabelPair, double> labelPairFloat = new Dictionary<LabelPair, double>();
            IList<Label> traversedLabels = new List<Label>();
            foreach (Label label1 in labelArray)
            {
                traversedLabels.Add(label1);
                foreach (Label label2 in labelArray)
                {
                    if (!traversedLabels.Contains(label2))
                        labelPairFloat.Add(new LabelPair(label1, label2), 0);
                }
            }
            IDictionary<LabelPair, double> mutualInformation = new Dictionary<LabelPair, double>(labelPairFloat);
            traversedLabels.Clear();
            foreach (Label label1 in labelArray)
            {
                traversedLabels.Add(label1);
                foreach (Label label2 in labelArray)
                {
                    if (!traversedLabels.Contains(label2))
                    {
                        LabelPair labelPair12 = new LabelPair(label1, label2);
                        LabelPair labelPair21 = new LabelPair(label2, label1);//这么算对，因为labelPair12或labelPair21在同一时间段内只能存在一个
                        if (numberOfLabel1TrueLabel2True.ContainsKey(labelPair12))
                        {
                            if (numberOfLabel1TrueLabel2True[labelPair12] != 0)
                                mutualInformation[labelPair12] += (numberOfLabel1TrueLabel2True[labelPair12] / n) * Math.Log((numberOfLabel1TrueLabel2True[labelPair12] * n) / (numberOfLabelTrue[label1] * numberOfLabelTrue[label2]));
                        }
                        else if (numberOfLabel1TrueLabel2True[labelPair21] != 0)
                            mutualInformation[labelPair12] += (numberOfLabel1TrueLabel2True[labelPair21] / n) * Math.Log((numberOfLabel1TrueLabel2True[labelPair21] * n) / (numberOfLabelTrue[label1] * numberOfLabelTrue[label2]));
                        if (numberOfLabel1TrueLabel2False.ContainsKey(labelPair12))
                        {
                            if (numberOfLabel1TrueLabel2False[labelPair12] != 0)
                                mutualInformation[labelPair12] += (numberOfLabel1TrueLabel2False[labelPair12] / n) * Math.Log((numberOfLabel1TrueLabel2False[labelPair12] * n) / (numberOfLabelTrue[label1] * numberOfLabelFalse[label2]));
                        }
                        else if (numberOfLabel1FalseLabel2True[labelPair21] != 0)
                            mutualInformation[labelPair12] += (numberOfLabel1FalseLabel2True[labelPair21] / n) * Math.Log((numberOfLabel1FalseLabel2True[labelPair21] * n) / (numberOfLabelTrue[label1] * numberOfLabelFalse[label2]));
                        if (numberOfLabel1FalseLabel2True.ContainsKey(labelPair12))
                        {
                            if (numberOfLabel1FalseLabel2True[labelPair12] != 0)
                                mutualInformation[labelPair12] += (numberOfLabel1FalseLabel2True[labelPair12] / n) * Math.Log((numberOfLabel1FalseLabel2True[labelPair12] * n) / (numberOfLabelFalse[label1] * numberOfLabelTrue[label2]));
                        }
                        else if (numberOfLabel1TrueLabel2False[labelPair21] != 0)
                            mutualInformation[labelPair12] += (numberOfLabel1TrueLabel2False[labelPair21] / n) * Math.Log((numberOfLabel1TrueLabel2False[labelPair21] * n) / (numberOfLabelFalse[label1] * numberOfLabelTrue[label2]));
                        if (numberOfLabel1FalseLabel2False.ContainsKey(labelPair12))
                        {
                            if (numberOfLabel1FalseLabel2False[labelPair12] != 0)
                                mutualInformation[labelPair12] += (numberOfLabel1FalseLabel2False[labelPair12] / n) * Math.Log((numberOfLabel1FalseLabel2False[labelPair12] * n) / (numberOfLabelFalse[label1] * numberOfLabelFalse[label2]));
                        }
                        else if (numberOfLabel1FalseLabel2False[labelPair21] != 0)
                            mutualInformation[labelPair12] += (numberOfLabel1FalseLabel2False[labelPair21] / n) * Math.Log((numberOfLabel1FalseLabel2False[labelPair21] * n) / (numberOfLabelFalse[label1] * numberOfLabelFalse[label2]));
                    }
                }
            }
            List<KeyValuePair<LabelPair, double>> sortedMutualInformation = new List<KeyValuePair<LabelPair, double>>(mutualInformation);
            sortedMutualInformation.Sort(delegate(KeyValuePair<LabelPair, double> s1, KeyValuePair<LabelPair, double> s2)
            {
                return s2.Value.CompareTo(s1.Value);
            });
            #endregion
            #region 求最小生成树（无方向）
            IList<KeyValuePair<LabelPair, double>> minTree = new List<KeyValuePair<LabelPair, double>>();
            IList<IList<Label>> LabelsCuts = new List<IList<Label>>();
            minTree.Add(sortedMutualInformation.First());
            LabelsCuts.Add(new List<Label>());
            LabelsCuts.First().Add(sortedMutualInformation.First().Key.First);
            LabelsCuts.First().Add(sortedMutualInformation.First().Key.Second);
            sortedMutualInformation.Remove(sortedMutualInformation.First());
            //for (int i = 0; i < sortedMutualInformation.Count && tree.Count != labelArray.Length - 1 && sortedMutualInformation[i].Value != 0; ++i)
            for (int i = 0; i < sortedMutualInformation.Count && minTree.Count != labelArray.Length - 1; ++i)
            {
                if (!sortedMutualInformation[i].Key.AreTwoLabelsInOneCut(LabelsCuts))
                {
                    minTree.Add(sortedMutualInformation[i]);
                    if (sortedMutualInformation[i].Key.AreTwoLabelsInNoCut(LabelsCuts))
                    {
                        LabelsCuts.Add(new List<Label>());
                        LabelsCuts.Last().Add(sortedMutualInformation.First().Key.First);
                        LabelsCuts.Last().Add(sortedMutualInformation.First().Key.Second);
                    }
                    else
                    {
                        int cutIndex = sortedMutualInformation[i].Key.IsOnlyOneLabelInOneCut(LabelsCuts);
                        if (cutIndex != -1)
                        {
                            if ((LabelsCuts[cutIndex].Contains(sortedMutualInformation[i].Key.First) && !LabelsCuts[cutIndex].Contains(sortedMutualInformation[i].Key.Second)))
                                LabelsCuts[cutIndex].Add(sortedMutualInformation[i].Key.Second);
                            else
                                LabelsCuts[cutIndex].Add(sortedMutualInformation[i].Key.First);
                        }
                        else
                        {
                            for (int ci = 0; ci < LabelsCuts.Count; ++ci)
                            {
                                if (LabelsCuts[ci].Contains(sortedMutualInformation[i].Key.First) || LabelsCuts[ci].Contains(sortedMutualInformation[i].Key.Second))
                                {
                                    for (int cj = ci + 1; cj < LabelsCuts.Count; ++cj)
                                    {
                                        if (LabelsCuts[cj].Contains(sortedMutualInformation[i].Key.First) || LabelsCuts[cj].Contains(sortedMutualInformation[i].Key.Second))
                                        {
                                            foreach (Label label in LabelsCuts[cj])
                                            {
                                                LabelsCuts[ci].Add(label);
                                            }
                                            LabelsCuts.RemoveAt(cj);
                                            goto EndMerge;
                                        }
                                    }
                                }
                            }
                        EndMerge: ;
                        }
                    }
                }
                sortedMutualInformation.Remove(sortedMutualInformation[i]);
                --i;
            }
            #endregion
            #region 生成树（有方向，前节点是求概率的条件（父节点），后节点是求概率的对象（子节点））
            IList<KeyValuePair<LabelPair, double>> tree = new List<KeyValuePair<LabelPair, double>>();
            Label currentLabel = labelArray[0];
            IList<Label> waitedLabels = new List<Label>();
            for (int i = 0;i < minTree.Count; ++i)
            {
                if (minTree[i].Key.Contains(currentLabel))
                {
                    if (minTree[i].Key.First == currentLabel)
                    {
                        tree.Add(new KeyValuePair<LabelPair, double>(minTree[i].Key, minTree[i].Value));
                        waitedLabels.Add(minTree[i].Key.Second);
                    }
                    else
                    {
                        tree.Add(new KeyValuePair<LabelPair, double>(minTree[i].Key.Reverse, minTree[i].Value));
                        waitedLabels.Add(minTree[i].Key.First);
                    }
                    minTree.RemoveAt(i);
                    --i;
                }
                if (i == minTree.Count - 1 && waitedLabels.Count != 0)
                {
                    currentLabel = waitedLabels.First();
                    waitedLabels.RemoveAt(0);
                    i = -1;
                }
            }
            return tree;
            #endregion
        }
    }
}