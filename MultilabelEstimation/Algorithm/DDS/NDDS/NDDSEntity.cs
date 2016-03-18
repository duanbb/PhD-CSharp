using System;
using System.Collections.Generic;

namespace MultilabelEstimation.Algorithm.DDS.NDDS
{
    static class NDDSVariable
    {
        static public Smoothing SmoothBN;
    }

    enum IndependenceEstimation
    {
        Probability, MutualInformation
    }

    sealed class Graph
    {
        public IDictionary<LabelPair, bool> AdjMatrix;//邻接矩阵

        public Graph()
        {
            AdjMatrix = new Dictionary<LabelPair, bool>();
            foreach (Label label1 in Variable.LabelArray)
            {
                foreach (Label label2 in Variable.LabelArray)
                {
                    if (label1 != label2)
                    {
                        AdjMatrix.Add(new LabelPair(label1, label2), true);//建立complete undirected graph
                    }
                }
            }
        }

        public IList<IList<Label>> GetWitnesses(LabelPair labelpair)
        {
            IList<IList<Label>> witnesses = new List<IList<Label>>();
            //既是未包含参数两个label的列表，也是包含8个元素的子集
            IList<Label> labelArrayWithoutTheTwoLabels = new List<Label>();
            foreach (Label label in Variable.LabelArray)
            {
                if (label != labelpair.First && label != labelpair.Second)
                {
                    labelArrayWithoutTheTwoLabels.Add(label);
                }
            }
            //0个元素的子集（空集）
            witnesses.Add(new List<Label>());
            //1个元素的子集
            foreach (Label label in labelArrayWithoutTheTwoLabels)
            {
                IList<Label> witness = new List<Label>();
                witness.Add(label);
                witnesses.Add(witness);
            }
            //2个元素的子集
            for (int a = 0; a < labelArrayWithoutTheTwoLabels.Count; ++a)
            {
                for (int b = a + 1; b < labelArrayWithoutTheTwoLabels.Count; ++b)
                {
                    IList<Label> witness = new List<Label>();
                    witness.Add(labelArrayWithoutTheTwoLabels[a]);
                    witness.Add(labelArrayWithoutTheTwoLabels[b]);
                    witnesses.Add(witness);
                }
            }
            //3个元素的子集
            for (int a = 0; a < labelArrayWithoutTheTwoLabels.Count; ++a)
            {
                for (int b = a + 1; b < labelArrayWithoutTheTwoLabels.Count; ++b)
                {
                    for (int c = b + 1; c < labelArrayWithoutTheTwoLabels.Count; ++c)
                    {
                        IList<Label> witness = new List<Label>();
                        witness.Add(labelArrayWithoutTheTwoLabels[a]);
                        witness.Add(labelArrayWithoutTheTwoLabels[b]);
                        witness.Add(labelArrayWithoutTheTwoLabels[c]);
                        witnesses.Add(witness);
                    }

                }
            }
            //4个元素的子集
            for (int a = 0; a < labelArrayWithoutTheTwoLabels.Count; ++a)
            {
                for (int b = a + 1; b < labelArrayWithoutTheTwoLabels.Count; ++b)
                {
                    for (int c = b + 1; c < labelArrayWithoutTheTwoLabels.Count; ++c)
                    {
                        for (int d = c + 1; d < labelArrayWithoutTheTwoLabels.Count; ++d)
                        {
                            IList<Label> witness = new List<Label>();
                            witness.Add(labelArrayWithoutTheTwoLabels[a]);
                            witness.Add(labelArrayWithoutTheTwoLabels[b]);
                            witness.Add(labelArrayWithoutTheTwoLabels[c]);
                            witness.Add(labelArrayWithoutTheTwoLabels[d]);
                            witnesses.Add(witness);
                        }
                    }
                }
            }
            //5个元素的子集
            for (int a = 0; a < labelArrayWithoutTheTwoLabels.Count; ++a)
            {
                for (int b = a + 1; b < labelArrayWithoutTheTwoLabels.Count; ++b)
                {
                    for (int c = b + 1; c < labelArrayWithoutTheTwoLabels.Count; ++c)
                    {
                        for (int d = c + 1; d < labelArrayWithoutTheTwoLabels.Count; ++d)
                        {
                            for (int e = d + 1; e < labelArrayWithoutTheTwoLabels.Count; ++e)
                            {
                                IList<Label> witness = new List<Label>();
                                witness.Add(labelArrayWithoutTheTwoLabels[a]);
                                witness.Add(labelArrayWithoutTheTwoLabels[b]);
                                witness.Add(labelArrayWithoutTheTwoLabels[c]);
                                witness.Add(labelArrayWithoutTheTwoLabels[d]);
                                witness.Add(labelArrayWithoutTheTwoLabels[e]);
                                witnesses.Add(witness);
                            }
                        }
                    }
                }
            }
            //6个元素的子集
            for (int a = 0; a < labelArrayWithoutTheTwoLabels.Count; ++a)
            {
                for (int b = a + 1; b < labelArrayWithoutTheTwoLabels.Count; ++b)
                {
                    for (int c = b + 1; c < labelArrayWithoutTheTwoLabels.Count; ++c)
                    {
                        for (int d = c + 1; d < labelArrayWithoutTheTwoLabels.Count; ++d)
                        {
                            for (int e = d + 1; e < labelArrayWithoutTheTwoLabels.Count; ++e)
                            {
                                for (int f = e + 1; f < labelArrayWithoutTheTwoLabels.Count; ++f)
                                {
                                    IList<Label> witness = new List<Label>();
                                    witness.Add(labelArrayWithoutTheTwoLabels[a]);
                                    witness.Add(labelArrayWithoutTheTwoLabels[b]);
                                    witness.Add(labelArrayWithoutTheTwoLabels[c]);
                                    witness.Add(labelArrayWithoutTheTwoLabels[d]);
                                    witness.Add(labelArrayWithoutTheTwoLabels[e]);
                                    witness.Add(labelArrayWithoutTheTwoLabels[f]);
                                    witnesses.Add(witness);
                                }
                            }
                        }
                    }
                }
            }
            //7个元素的子集
            for (int a = 0; a < labelArrayWithoutTheTwoLabels.Count; ++a)
            {
                for (int b = a + 1; b < labelArrayWithoutTheTwoLabels.Count; ++b)
                {
                    for (int c = b + 1; c < labelArrayWithoutTheTwoLabels.Count; ++c)
                    {
                        for (int d = c + 1; d < labelArrayWithoutTheTwoLabels.Count; ++d)
                        {
                            for (int e = d + 1; e < labelArrayWithoutTheTwoLabels.Count; ++e)
                            {
                                for (int f = e + 1; f < labelArrayWithoutTheTwoLabels.Count; ++f)
                                {
                                    for (int g = f + 1; g < labelArrayWithoutTheTwoLabels.Count; ++g)
                                    {
                                        IList<Label> witness = new List<Label>();
                                        witness.Add(labelArrayWithoutTheTwoLabels[a]);
                                        witness.Add(labelArrayWithoutTheTwoLabels[b]);
                                        witness.Add(labelArrayWithoutTheTwoLabels[c]);
                                        witness.Add(labelArrayWithoutTheTwoLabels[d]);
                                        witness.Add(labelArrayWithoutTheTwoLabels[e]);
                                        witness.Add(labelArrayWithoutTheTwoLabels[f]);
                                        witness.Add(labelArrayWithoutTheTwoLabels[g]);
                                        witnesses.Add(witness);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //8个元素的子集
            witnesses.Add(labelArrayWithoutTheTwoLabels);
            return witnesses;
        }//有错误，只留在Probability的代码里，不用

        public IList<IList<Label>> GetLatentWitnesses(LabelPair labelPair, int i)
        {
            IList<IList<Label>> latentWitnesses = new List<IList<Label>>();
            //既是未包含参数两个label的列表，也是包含8个元素的子集
            IList<Label> adjLabelsWithoutTheTwoLabels = new List<Label>();
            foreach (Label label in Variable.LabelArray)
            {
                if (!labelPair.Contains(label) && (AdjMatrix[new LabelPair(labelPair.First, label)] || AdjMatrix[new LabelPair(labelPair.Second, label)]))//此处跟书上不一样，书上错了
                {
                    adjLabelsWithoutTheTwoLabels.Add(label);
                }
            }
            switch (i)
            {
                case 0:
                    latentWitnesses.Add(new List<Label>());
                    break;
                case 1:
                    for (int a = 0; a < adjLabelsWithoutTheTwoLabels.Count; ++a)
                    {
                        IList<Label> witness = new List<Label>();
                        witness.Add(adjLabelsWithoutTheTwoLabels[a]);
                        latentWitnesses.Add(witness);
                    }
                    break;
                case 2:
                    for (int a = 0; a < adjLabelsWithoutTheTwoLabels.Count; ++a)
                    {
                        for (int b = a + 1; b < adjLabelsWithoutTheTwoLabels.Count; ++b)
                        {
                            IList<Label> witness = new List<Label>();
                            witness.Add(adjLabelsWithoutTheTwoLabels[a]);
                            witness.Add(adjLabelsWithoutTheTwoLabels[b]);
                            latentWitnesses.Add(witness);
                        }
                    }
                    break;
                case 3:
                    for (int a = 0; a < adjLabelsWithoutTheTwoLabels.Count; ++a)
                    {
                        for (int b = a + 1; b < adjLabelsWithoutTheTwoLabels.Count; ++b)
                        {
                            for (int c = b + 1; c < adjLabelsWithoutTheTwoLabels.Count; ++c)
                            {
                                IList<Label> witness = new List<Label>();
                                witness.Add(adjLabelsWithoutTheTwoLabels[a]);
                                witness.Add(adjLabelsWithoutTheTwoLabels[b]);
                                witness.Add(adjLabelsWithoutTheTwoLabels[c]);
                                latentWitnesses.Add(witness);
                            }
                        }
                    }
                    break;
                case 4:
                    for (int a = 0; a < adjLabelsWithoutTheTwoLabels.Count; ++a)
                    {
                        for (int b = a + 1; b < adjLabelsWithoutTheTwoLabels.Count; ++b)
                        {
                            for (int c = b + 1; c < adjLabelsWithoutTheTwoLabels.Count; ++c)
                            {
                                for (int d = c + 1; d < adjLabelsWithoutTheTwoLabels.Count; ++d)
                                {
                                    IList<Label> witness = new List<Label>();
                                    witness.Add(adjLabelsWithoutTheTwoLabels[a]);
                                    witness.Add(adjLabelsWithoutTheTwoLabels[b]);
                                    witness.Add(adjLabelsWithoutTheTwoLabels[c]);
                                    witness.Add(adjLabelsWithoutTheTwoLabels[d]);
                                    latentWitnesses.Add(witness);
                                }
                            }
                        }
                    }
                    break;
                case 5:
                    for (int a = 0; a < adjLabelsWithoutTheTwoLabels.Count; ++a)
                    {
                        for (int b = a + 1; b < adjLabelsWithoutTheTwoLabels.Count; ++b)
                        {
                            for (int c = b + 1; c < adjLabelsWithoutTheTwoLabels.Count; ++c)
                            {
                                for (int d = c + 1; d < adjLabelsWithoutTheTwoLabels.Count; ++d)
                                {
                                    for (int e = d + 1; e < adjLabelsWithoutTheTwoLabels.Count; ++e)
                                    {
                                        IList<Label> witness = new List<Label>();
                                        witness.Add(adjLabelsWithoutTheTwoLabels[a]);
                                        witness.Add(adjLabelsWithoutTheTwoLabels[b]);
                                        witness.Add(adjLabelsWithoutTheTwoLabels[c]);
                                        witness.Add(adjLabelsWithoutTheTwoLabels[d]);
                                        witness.Add(adjLabelsWithoutTheTwoLabels[e]);
                                        latentWitnesses.Add(witness);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case 6:
                    for (int a = 0; a < adjLabelsWithoutTheTwoLabels.Count; ++a)
                    {
                        for (int b = a + 1; b < adjLabelsWithoutTheTwoLabels.Count; ++b)
                        {
                            for (int c = b + 1; c < adjLabelsWithoutTheTwoLabels.Count; ++c)
                            {
                                for (int d = c + 1; d < adjLabelsWithoutTheTwoLabels.Count; ++d)
                                {
                                    for (int e = d + 1; e < adjLabelsWithoutTheTwoLabels.Count; ++e)
                                    {
                                        for (int f = e + 1; f < adjLabelsWithoutTheTwoLabels.Count; ++f)
                                        {
                                            IList<Label> witness = new List<Label>();
                                            witness.Add(adjLabelsWithoutTheTwoLabels[a]);
                                            witness.Add(adjLabelsWithoutTheTwoLabels[b]);
                                            witness.Add(adjLabelsWithoutTheTwoLabels[c]);
                                            witness.Add(adjLabelsWithoutTheTwoLabels[d]);
                                            witness.Add(adjLabelsWithoutTheTwoLabels[e]);
                                            witness.Add(adjLabelsWithoutTheTwoLabels[f]);
                                            latentWitnesses.Add(witness);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case 7:
                    for (int a = 0; a < adjLabelsWithoutTheTwoLabels.Count; ++a)
                    {
                        for (int b = a + 1; b < adjLabelsWithoutTheTwoLabels.Count; ++b)
                        {
                            for (int c = b + 1; c < adjLabelsWithoutTheTwoLabels.Count; ++c)
                            {
                                for (int d = c + 1; d < adjLabelsWithoutTheTwoLabels.Count; ++d)
                                {
                                    for (int e = d + 1; e < adjLabelsWithoutTheTwoLabels.Count; ++e)
                                    {
                                        for (int f = e + 1; f < adjLabelsWithoutTheTwoLabels.Count; ++f)
                                        {
                                            for (int g = f + 1; g < adjLabelsWithoutTheTwoLabels.Count; ++g)
                                            {
                                                IList<Label> witness = new List<Label>();
                                                witness.Add(adjLabelsWithoutTheTwoLabels[a]);
                                                witness.Add(adjLabelsWithoutTheTwoLabels[b]);
                                                witness.Add(adjLabelsWithoutTheTwoLabels[c]);
                                                witness.Add(adjLabelsWithoutTheTwoLabels[d]);
                                                witness.Add(adjLabelsWithoutTheTwoLabels[e]);
                                                witness.Add(adjLabelsWithoutTheTwoLabels[f]);
                                                witness.Add(adjLabelsWithoutTheTwoLabels[g]);
                                                latentWitnesses.Add(witness);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case 8:
                    for (int a = 0; a < adjLabelsWithoutTheTwoLabels.Count; ++a)
                    {
                        for (int b = a + 1; b < adjLabelsWithoutTheTwoLabels.Count; ++b)
                        {
                            for (int c = b + 1; c < adjLabelsWithoutTheTwoLabels.Count; ++c)
                            {
                                for (int d = c + 1; d < adjLabelsWithoutTheTwoLabels.Count; ++d)
                                {
                                    for (int e = d + 1; e < adjLabelsWithoutTheTwoLabels.Count; ++e)
                                    {
                                        for (int f = e + 1; f < adjLabelsWithoutTheTwoLabels.Count; ++f)
                                        {
                                            for (int g = f + 1; g < adjLabelsWithoutTheTwoLabels.Count; ++g)
                                            {
                                                for (int h = g + 1; h < adjLabelsWithoutTheTwoLabels.Count; ++h)
                                                {
                                                    IList<Label> witness = new List<Label>();
                                                    witness.Add(adjLabelsWithoutTheTwoLabels[a]);
                                                    witness.Add(adjLabelsWithoutTheTwoLabels[b]);
                                                    witness.Add(adjLabelsWithoutTheTwoLabels[c]);
                                                    witness.Add(adjLabelsWithoutTheTwoLabels[d]);
                                                    witness.Add(adjLabelsWithoutTheTwoLabels[e]);
                                                    witness.Add(adjLabelsWithoutTheTwoLabels[f]);
                                                    witness.Add(adjLabelsWithoutTheTwoLabels[g]);
                                                    witness.Add(adjLabelsWithoutTheTwoLabels[h]);
                                                    latentWitnesses.Add(witness);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
            return latentWitnesses;
        }

        public int NumberOfOrientingEdges//有向边的条数
        {
            get
            {
                int num = 0;
                foreach (Label label1 in Variable.LabelArray)
                {
                    foreach (Label label2 in Variable.LabelArray)
                    {
                        if (label1 != label2 && AdjMatrix[new LabelPair(label1, label2)])
                        {
                            ++num;
                        }
                    }
                }
                return num;
            }
        }

        public int MaxDegree()//此时矩阵关于对角线对称
        {
            int maxADJ = 0;
            IList<Label> traversedLabels = new List<Label>();
            foreach (Label label1 in Variable.LabelArray)
            {
                int adj = 0;
                traversedLabels.Add(label1);
                foreach (Label label2 in Variable.LabelArray)
                {
                    if (!traversedLabels.Contains(label2))
                    {
                        if (AdjMatrix[new LabelPair(label1, label2)])
                            ++adj;
                    }
                }
                if (adj > maxADJ)
                    maxADJ = adj;
            }
            return maxADJ;
        }

        public override string ToString()
        {
            string s = string.Empty;
            foreach (KeyValuePair<LabelPair, bool> kv in AdjMatrix)
            {
                if (kv.Value)
                    s += kv.Key.ToString() + "\r\n";
            }
            s += "sub graphs:\r\n";
            for (int i = 0; i < SubGraphs.Count; ++i)
            {
                s += i + 1 + ": ";
                foreach (Label label in SubGraphs[i])
                {
                    s += label + " ";
                }
                s += "\r\n";
            }
            return s;
        }
        //论文画图用
        public IList<IList<Label>> SubGraphs
        {
            get
            {
                IList<IList<Label>> subGraphs = new List<IList<Label>>();
                foreach (KeyValuePair<LabelPair, bool> labelPair in AdjMatrix)
                {
                    if (labelPair.Value)
                    {
                        bool areBelongToASubGraph = false;
                        for (int i = 0; i < subGraphs.Count; ++i)
                        {
                            if (subGraphs[i].Contains(labelPair.Key.First) && !subGraphs[i].Contains(labelPair.Key.Second))
                            {
                                bool isBelongToAnotherGraph = false;
                                for (int j = 0; j < subGraphs.Count; ++j)
                                {
                                    if (j == i) continue;
                                    if (subGraphs[j].Contains(labelPair.Key.Second))
                                    {
                                        foreach (Label label in subGraphs[j])
                                        {
                                            subGraphs[i].Add(label);
                                        }
                                        subGraphs.RemoveAt(j);
                                        isBelongToAnotherGraph = true;
                                        break;
                                    }
                                }
                                if (!isBelongToAnotherGraph)
                                    subGraphs[i].Add(labelPair.Key.Second);
                                areBelongToASubGraph = true;
                                break;
                            }
                            else if (!subGraphs[i].Contains(labelPair.Key.First) && subGraphs[i].Contains(labelPair.Key.Second))
                            {
                                bool isBelongToAnotherGraph = false;
                                for (int j = 0; j < subGraphs.Count; ++j)
                                {
                                    if (j == i) continue;
                                    if (subGraphs[j].Contains(labelPair.Key.First))
                                    {
                                        foreach (Label label in subGraphs[j])
                                        {
                                            subGraphs[i].Add(label);
                                        }
                                        subGraphs.RemoveAt(j);
                                        isBelongToAnotherGraph = true;
                                        break;
                                    }
                                }
                                if (!isBelongToAnotherGraph)
                                    subGraphs[i].Add(labelPair.Key.First);
                                areBelongToASubGraph = true;
                                break;
                            }
                            else if (subGraphs[i].Contains(labelPair.Key.First) && subGraphs[i].Contains(labelPair.Key.Second))
                            {
                                areBelongToASubGraph = true;
                                break;
                            }
                        }
                        if (!areBelongToASubGraph)
                        {
                            IList<Label> subGraph = new List<Label>();
                            subGraph.Add(labelPair.Key.First);
                            subGraph.Add(labelPair.Key.Second);
                            subGraphs.Add(subGraph);
                        }
                    }
                }
                foreach (Label label in Variable.LabelArray)
                {
                    bool isBelongToASubGraph = false;
                    foreach (IList<Label> subGraph in subGraphs)
                    {
                        if (subGraph.Contains(label))
                        {
                            isBelongToASubGraph = true;
                            break;
                        }
                    }
                    if (!isBelongToASubGraph)
                    {
                        IList<Label> subGraph = new List<Label>();
                        subGraph.Add(label);
                        subGraphs.Add(subGraph);
                    }
                }
                return subGraphs;
            }
        }

    }

    sealed class LabelAndWitness : IEquatable<LabelAndWitness>
    {
        Labelset SingleAnnotation;
        Labelset Labelset;
        public LabelAndWitness(Labelset singleAnnotation, Labelset labelset)
        {
            this.SingleAnnotation = singleAnnotation;
            this.Labelset = labelset;
        }
        public bool Equals(LabelAndWitness otherAW)
        {
            return otherAW.SingleAnnotation.Equals(SingleAnnotation) && otherAW.Labelset.Equals(Labelset);
        }
        public override int GetHashCode()
        {
            return (this.SingleAnnotation.GetHashCode() + 1) * 10 + this.Labelset.GetHashCode() + 1;
        }
    }
}