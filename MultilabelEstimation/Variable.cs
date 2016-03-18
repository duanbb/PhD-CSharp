using MultilabelEstimation.Consistency;
using MultilabelEstimation.Group;
using MultilabelEstimation.Algorithm.Personality;
using System;
using System.Collections.Generic;
using System.IO;

namespace MultilabelEstimation
{
    enum Label
    {
        //yorokobi, suki, yasu, ikari, aware, kowa, haji, iya, takaburi, odoroki,
        happiness, fondness, relief, anger, sadness, fear, shame, disgust, excitement, surprise,
        WantToGo, ConfirmToGo, HaveBeenThereBefore, NoIntention
    }
    enum Story
    {
        Masatyan, Bokutati, BokutatiSample, SnowFestival, TwoStories
    }
    enum Selector//Sentence和Annotator共享
    {
        Most, Least, Best, Worst
    }
    enum AlgorithmSelector
    {
        MajorityVote, IDS, PDS, JDDS, IDDS, NDDS
    }
    enum SimilaritySelector
    {
        NumericIndependentEuclidean,
        BinaryIndependentDice, Dice,
        BinaryIndependentJaccard, Jaccard,
        BinaryIndependentCompare, Compare,
        BinaryResultAndNumericGold,
        Same
    }
    enum GroupingMethod
    {
        Seperate, Overlap
    }
    enum PriorP
    {
        Pj, Mcj, Sij, ConditionalPj, ConditionalMcj, ConditionalSij
    }
    enum Smoothing//La: +1; Li: +1/numberOfIntlabel; JP: +1/2
    {
        None, Laplace, Lidstone, JeffreysPerks, Pow10minus10
    }
    enum RelationScheme//AllLower太慢，废弃
    {
        RenewOne, UpdateOne, RenewLower, UpdateLower, AllLower, AlwaysInitialization, IndependentRenewLower, IndependentRenewOne
    }
    enum GoldType
    {
        Joint, //N项同时被标最多次时，取N项之并
        JointOnlyOne, //N项同时被标最多次时，取一项（标的尽量少，频率尽量多）
        SeparateOverTrueLabelNumber, 
        SeparateOverHalf, 
        SeperateOverTrueLabelNumberAndHalf,//上两个结果的并
        SeperateOverTrueLabelNumberAndHalfAndJoint,//三个结果的并
    }
    static class Variable
    {
        static public string ConsoleOutput;
        static public int NumberOfAnnotationsPerSentence;
        static public int NumberOfAnnotationsPerSentenceAfterGrouping;//分组后一组中每句被标多少次
        static public IList<Sentence> Sentences;
        //static public IList<Sentence> OriginalSentences;
        static public IList<Annotator> Annotators;
        //static public Label[] OriginalLabelArray;//用于遍历情感，已经整体频率按频率降序排列
        static public Label[] LabelArray;//InitializeData()初始化
        static public string spliter;
        static public IDictionary<Annotator, IDictionary<Sentence, List<Annotation>>> Data;//全体标注数据（用Tuple<Annotator, Sentence> => List<Annotation>不方便观察）
        static public IDictionary<Label, string> LabelToString;
        static public StreamWriter OutputFile;
        static public double TotalSimilarity;//用于计算每人总相似度的百分比
        static public double TotalNumberOfAnnotatedTimes;//用于计算工作质量
        static public double AverageTrueLabelsPerAnnotation;
        static public bool PjDividSentenceCount;
        static public bool SijDividPDataOnI;
        static public bool OutputPdata;
        static public bool OutputResult;
        static public RelationScheme Relation;
        static public PriorP[] PriorP;
        static public Smoothing SmoothPajl;
        static public double ConvergeValueThreshold;
        static public int ConvergeTimeThreshold;
        static public GoldType GoldType;
        static public GoldType MVType;
        static Variable()
        {
            Sentences = new List<Sentence>();
            LabelToString = new Dictionary<Label, string>();
            Annotators = new List<Annotator>();
            Data = new Dictionary<Annotator, IDictionary<Sentence, List<Annotation>>>();
            //LabelArray = new Label[10] { Label.happiness, Label.fondness, Label.relief, Label.anger, Label.sadness, Label.fear, Label.shame, Label.disgust, Label.excitement, Label.surprise };//用于遍历情感，乱序
            spliter = "************************************************************************************************************************";
            //ResultFile.WriteLine("Top Sentence Number,Average Of Labels,Threshold of Gold,I-DS,D-DS,Experiemt AD-DS,Sentence AD-DS");
        }
    }

    struct Annotator : IEquatable<Annotator>
    {
        public string ID;
        public Similarity Similarity;//用于计算工作量
        public Annotator(string id)
        {
            this.ID = id;
            Similarity = new Similarity();
        }

        public bool Equals(Annotator other)
        {
            return this.ID == other.ID;
        }

        int NumberOfAnnotatedSentences
        {
            get
            {
                int number = 0;
                foreach (List<Annotation> annotation in Variable.Data[this].Values)
                {
                    if (annotation.Count > 0)
                    {
                        ++number;
                    }
                }
                return number;
            }
        }

        public override string ToString()
        {
            return this.ID;
        }
    }

    sealed class Sentence : IEquatable<Sentence>
    {
        public int ID;//句子原文可重复，所以要用ID做索引
        public string Speech;//句子原文
        public Character Character;//角色
        public Sentence(int id, string speech)
        {
            ID = id;
            Speech = speech;
        }
        public Sentence(int id, string speech, Character character)
        {
            ID = id;
            Speech = speech;
            Character = character;
        }

        public bool Equals(Sentence other)
        {
            return this.ID == other.ID;
        }
        public override bool Equals(object obj)//必须重写这个，Tuple<Labelset, Labelset> 才能用作Key来索引Dictionary
        {
            Sentence sentence = obj as Sentence;
            return sentence.Equals(this);
        }
        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public Result[] IndependentGold;//独立标准（某label的标注量大于某阈值）
        public Result[] DependentGoldStandard;//依赖标准（某label的标注量大于某阈值）
        public Result BinaryGold;//依赖多数黄金标准
        public NumericResult NumericGold;//某标签被标注的次数占总次数的比例
        public Result IndependentResult;//独立估计的结果
        public Result PreciseResult;//依赖估计的结果
        public Result TreeForAllResult;//近似估计的结果
        public Result TreeForSenResult;//近似估计的结果
        public Label[] LabelArray;//按频率降序，TTDDS使用
        public List<KeyValuePair<Label, int>> SortedLabels;//生成SnowFestival的GoldStandard时使用
        public Result INVandDNVasGold;
        public AnnotationGroup[] AnnotaitonGroups;

        public IList<KeyValuePair<LabelPair, double>> Tree;

        public Annotation IndResultAndDepResult
        {
            get
            {
                Annotation gold = new Annotation();
                foreach (Label label in Variable.LabelArray)
                {
                    if (IndependentResult.Labels[label] && PreciseResult.Labels[label])
                        gold.Labels[label] = true;
                }
                return gold;
            }
        }

        public Result[] INVandDNVasGolds
        {
            get
            {
                Result[] golds = new Result[Variable.NumberOfAnnotationsPerSentenceAfterGrouping + 2];
                for (int r = 0; r < Variable.NumberOfAnnotationsPerSentenceAfterGrouping + 2; ++r)
                {
                    golds[r] = new Result();
                    foreach (Label label in Variable.LabelArray)
                    {
                        if (IndependentGold[r].Labels[label] && DependentGoldStandard[r].Labels[label])
                        {
                            golds[r].Labels[label] = true;
                        }
                    }
                }
                return golds;
            }
        }

        public override string ToString()
        {
            return this.ID.ToString();
        }
    }

    //包括全部标签
    class Annotation : IEquatable<Annotation>
    {
        public IDictionary<Label, bool> Labels;
        public bool Mu
        {
            get
            {
                foreach (Label label in Variable.LabelArray)
                {
                    if (Labels[label])
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public int NumberOfTrueLabel
        {
            get
            {
                int number = 0;
                foreach (Label label in Variable.LabelArray)
                {
                    if (Labels[label])
                        ++number;
                }
                return number;
            }
        }
        public int IntLabel
        {
            get
            {
                int intLabel = 0;
                int i = 0;
                foreach (Label label in Variable.LabelArray)
                {
                    intLabel += Convert.ToInt16(Labels[label]) * Convert.ToInt16(Math.Pow(2, i));
                    ++i;
                }
                return intLabel;
            }
        }
        public Annotation()
        {
            Labels = new Dictionary<Label, bool>();
            foreach (Label label in Variable.LabelArray)
            {
                Labels.Add(label, false);
            }
        }
        public Annotation(int intLabel)
        {
            Labels = new Dictionary<Label, bool>();
            foreach (Label label in Variable.LabelArray)
            {
                Labels.Add(label, false);
            }
            for (int i = 0; i < Variable.LabelArray.Length && intLabel != 0; ++i)
            {
                if (intLabel % 2 == 1)
                    Labels[Variable.LabelArray[i]] = true;
                intLabel /= 2;
            }
        }
        public Labelset ToLabelset(Label[] labels)
        {
            Labelset labelset = new Labelset();
            foreach (Label label in labels)
            {
                labelset.Labels.Add(label, Labels[label]);
            }
            return labelset;
        }
        public bool IsAccordingToLabelset(Labelset labelset)
        {
            foreach (Label label in labelset.Labels.Keys)
            {
                if (Labels[label] != labelset.Labels[label]) return false;
            }
            return true;
        }
        public bool Equals(Annotation annotation)
        {
            foreach (KeyValuePair<Label, bool> label in Labels)
            {
                if (annotation.Labels[label.Key] == label.Value)
                    continue;
                else return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            return this.IntLabel;
        }
        public override string ToString()
        {
            if (this.Mu) return "Neutral";
            string result = string.Empty;
            foreach (Label label in Labels.Keys)
            {
                if (Labels[label])
                    result += Variable.LabelToString[label] + "|";
            }
            return result.Remove(result.Length - 1);
        }
    }

    sealed class Labelset : IEquatable<Labelset> //只含一部分情感
    {
        public IDictionary<Label, bool> Labels;
        public bool IsBeginning;//算转移概率时用
        public Labelset()
        {
            this.Labels = new Dictionary<Label, bool>();
        }
        public Labelset(bool isBeginning)
        {
            this.IsBeginning = isBeginning;
        }
        public Labelset(Label label, bool truth)//相当于生成一个SinglelabelAnnotation
        {
            this.Labels = new Dictionary<Label, bool>();
            this.Labels.Add(new KeyValuePair<Label, bool>(label, truth));
        }
        public Labelset(IList<Label> labels, int num)//相当于DecimalToAnnotation
        {
            this.Labels = new Dictionary<Label, bool>();
            foreach (Label label in labels)
            {
                this.Labels.Add(label, false);
            }
            for (int i = 0; i < labels.Count && num != 0; ++i)
            {
                if (num % 2 == 1)
                    this.Labels[labels[i]] = true;
                num /= 2;
            }
        }
        //父集缩为子集
        public Labelset(IList<Label> labels, Labelset labelset)
        {
            this.Labels = new Dictionary<Label, bool>();
            foreach (Label label in labels)
            {
                this.Labels.Add(label, labelset.Labels[label]);
            }
        }
        //复制构造函数
        public Labelset(Labelset otherLabelset)
        {
            this.Labels = new Dictionary<Label, bool>(otherLabelset.Labels);
        }
        public int NumberOfTypes
        {
            get
            {
                return Convert.ToInt16(Math.Pow(2, Labels.Count));
            }
        }
        public int IntLabel
        {
            get
            {
                if (IsBeginning) return -1;
                int intLabel = 0;
                int i = 0;
                foreach (Label label in this.Labels.Keys)
                {
                    intLabel += Convert.ToInt16(Labels[label]) * Convert.ToInt16(Math.Pow(2, i));
                    ++i;
                }
                return intLabel;
            }
        }
        public int NumberOfTrueLabels
        {
            get
            {
                if (IsBeginning) return -1;
                int number = 0;
                foreach (Label label in this.Labels.Keys)
                {
                    if (this.Labels[label])
                        ++number;
                }
                return number;
            }
        }
        public Labelset ToSingleLabelAnnotation(Label label)
        {
            return new Labelset(label, this.Labels[label]);
        }

        public bool Equals(Labelset labelset)
        {
            if (!this.IsBeginning && !labelset.IsBeginning)//都不是开头
            {
                if (labelset.Labels.Count != Labels.Count)
                {
                    return false;
                }
                else
                {
                    foreach (KeyValuePair<Label, bool> label in Labels)
                    {
                        if (labelset.Labels.ContainsKey(label.Key) && labelset.Labels[label.Key] == label.Value)
                            continue;
                        else return false;
                    }
                }
                return true;
            }
            else if (this.IsBeginning && labelset.IsBeginning)//都是开头
                return true;
            else return false;//一个是开头一个不是开头
        }

        public override bool Equals(object obj)//必须重写这个，Tuple<Labelset, Labelset> 才能用作Key来索引Dictionary
        {
            Labelset labelset = obj as Labelset;
            return labelset.Equals(this);
        }

        public override int GetHashCode()
        {
            //int hashCode = 1;
            //foreach (KeyValuePair<Label, bool> label in this.Labels)
            //{
            //    hashCode *= (label.Key.GetHashCode() + 1) * 10 + label.Value.GetHashCode() + 1;
            //}
            return this.IntLabel;
        }
        public override string ToString()
        {
            if (this.IsBeginning) return "Beginning";
            string result = string.Empty;
            foreach (Label label in Labels.Keys)
            {
                if (Labels[label])
                    result += Variable.LabelToString[label] + "|";
            }
            return result.Length != 0 ? result.Remove(result.Length - 1) : "Neutral";
        }

        public double NumberOfTrueStrongAffects
        {
            get 
            {
                double numberOfTrueStrongLabels = 0;
                foreach (Label label in this.Labels.Keys)
                {
                    if (this.Labels[label] && PersonalityVariable.StrongAffects.Contains(label))
                        ++numberOfTrueStrongLabels;
                }
                return numberOfTrueStrongLabels;
            }
        }

        public double NumberOfTrueWeakAffects
        {
            get
            {
                double numberOfTrueWeakLabels = 0;
                foreach (Label label in this.Labels.Keys)
                {
                    if (this.Labels[label] && PersonalityVariable.WeakAffects.Contains(label))
                        ++numberOfTrueWeakLabels;
                }
                return numberOfTrueWeakLabels;
            }
        }

        public double HowStrong
        {
            get
            {
                if (NumberOfTrueStrongAffects + NumberOfTrueWeakAffects != 0)
                    return NumberOfTrueStrongAffects / (NumberOfTrueStrongAffects + NumberOfTrueWeakAffects);
                else return 0.5;
            }
        }
        public double HowWeak
        {
            get
            {
                if (NumberOfTrueStrongAffects + NumberOfTrueWeakAffects != 0)
                    return NumberOfTrueWeakAffects / (NumberOfTrueStrongAffects + NumberOfTrueWeakAffects);
                else return 0.5;
            }
        }
    }

    sealed class LabelPair : IEquatable<LabelPair>
    {
        public Label First;//条件
        public Label Second;//结果
        public IDictionary<Sentence, double> Label1TrueLabel2TrueSentenceAndFreq;//句子和其被标注为此labelpair的次数（论文用）
        public IDictionary<Sentence, double> Label1TrueLabel2FalseSentenceAndFreq;
        public IDictionary<Sentence, double> Label1FalseLabel2TrueSentenceAndFreq;
        public IDictionary<Sentence, double> Label1FalseLabel2FalseSentenceAndFreq;
        public double Weight;//PDS时是JointEntropy

        public LabelPair(Label x, Label y)
        {
            this.First = x;
            this.Second = y;
            this.Label1TrueLabel2TrueSentenceAndFreq = new Dictionary<Sentence, double>();
            this.Label1TrueLabel2FalseSentenceAndFreq = new Dictionary<Sentence, double>();
            this.Label1FalseLabel2TrueSentenceAndFreq = new Dictionary<Sentence, double>();
            this.Label1FalseLabel2FalseSentenceAndFreq = new Dictionary<Sentence, double>();
            this.Weight = 0;
        }

        public bool Equals(LabelPair obj)
        {
            return obj.First == this.First && obj.Second == this.Second;
        }

        public override int GetHashCode()
        {
            return (this.First.GetHashCode() + 1) * 10 + this.Second.GetHashCode() + 1;
        }

        public Label[] ToArray()
        {
            return new Label[] { this.First, this.Second };
        }

        public IList<Label> ToList()
        {
            IList<Label> labels = new List<Label>();
            labels.Add(this.First);
            labels.Add(this.Second);
            return labels;
        }

        public List<KeyValuePair<Sentence, double>> SortedLabel1TrueLabel2TrueSentenceAndFreq
        {
            get
            {
                List<KeyValuePair<Sentence, double>> sortedSentenceAndFreq = new List<KeyValuePair<Sentence, double>>(Label1TrueLabel2TrueSentenceAndFreq);
                sortedSentenceAndFreq.Sort(delegate(KeyValuePair<Sentence, double> s1, KeyValuePair<Sentence, double> s2)
                {
                    return s2.Value.CompareTo(s1.Value);
                });
                return sortedSentenceAndFreq;
            }
        }
        public List<KeyValuePair<Sentence, double>> SortedLabel1TrueLabel2FalseSentenceAndFreq
        {
            get
            {
                List<KeyValuePair<Sentence, double>> sortedSentenceAndFreq = new List<KeyValuePair<Sentence, double>>(Label1TrueLabel2FalseSentenceAndFreq);
                sortedSentenceAndFreq.Sort(delegate(KeyValuePair<Sentence, double> s1, KeyValuePair<Sentence, double> s2)
                {
                    return s2.Value.CompareTo(s1.Value);
                });
                return sortedSentenceAndFreq;
            }
        }
        public List<KeyValuePair<Sentence, double>> SortedLabel1FalseLabel2TrueSentenceAndFreq
        {
            get
            {
                List<KeyValuePair<Sentence, double>> sortedSentenceAndFreq = new List<KeyValuePair<Sentence, double>>(Label1FalseLabel2TrueSentenceAndFreq);
                sortedSentenceAndFreq.Sort(delegate(KeyValuePair<Sentence, double> s1, KeyValuePair<Sentence, double> s2)
                {
                    return s2.Value.CompareTo(s1.Value);
                });
                return sortedSentenceAndFreq;
            }
        }
        public List<KeyValuePair<Sentence, double>> SortedLabel1FalseLabel2FalseSentenceAndFreq
        {
            get
            {
                List<KeyValuePair<Sentence, double>> sortedSentenceAndFreq = new List<KeyValuePair<Sentence, double>>(Label1FalseLabel2FalseSentenceAndFreq);
                sortedSentenceAndFreq.Sort(delegate(KeyValuePair<Sentence, double> s1, KeyValuePair<Sentence, double> s2)
                {
                    return s2.Value.CompareTo(s1.Value);
                });
                return sortedSentenceAndFreq;
            }
        }
        public double Label1TrueLabel2TrueFrequency
        {
            get
            {
                double n = 0;
                foreach (int i in Label1TrueLabel2TrueSentenceAndFreq.Values)
                {
                    n += i;
                }
                return n;
            }
        }
        public double Label1TrueLabel2FalseFrequency
        {
            get
            {
                double n = 0;
                foreach (int i in Label1TrueLabel2FalseSentenceAndFreq.Values)
                {
                    n += i;
                }
                return n;
            }
        }
        public double Label1FalseLabel2TrueFrequency
        {
            get
            {
                double n = 0;
                foreach (int i in Label1FalseLabel2TrueSentenceAndFreq.Values)
                {
                    n += i;
                }
                return n;
            }
        }
        public double Label1FalseLabel2FalseFrequency
        {
            get
            {
                double n = 0;
                foreach (int i in Label1FalseLabel2FalseSentenceAndFreq.Values)
                {
                    n += i;
                }
                return n;
            }
        }

        public LabelPair Reverse//Tree, BN用
        {
            get
            {
                return new LabelPair(Second, First);
            }
        }

        public bool Contains(Label label)
        {
            if (label == First || label == Second) return true;
            else return false;
        }

        public bool AreTwoLabelsInOneCut(IList<IList<Label>> labelsCuts)//Tree用
        {
            foreach (List<Label> labelsInOneCut in labelsCuts)
            {
                if (labelsInOneCut.Contains(First) && labelsInOneCut.Contains(Second))
                    return true;
            }
            return false;
        }
        public bool AreTwoLabelsInNoCut(IList<IList<Label>> labelsCuts)//Tree用
        {
            foreach (List<Label> labelsInOneCut in labelsCuts)
            {
                if (labelsInOneCut.Contains(First) || labelsInOneCut.Contains(Second))
                    return false;
            }
            return true;
        }
        public int IsOnlyOneLabelInOneCut(IList<IList<Label>> labelsCuts)//Tree用
        {
            for (int i = 0; i < labelsCuts.Count; ++i)
            {
                if (labelsCuts[i].Contains(First))
                {
                    for (int j = 0; j < labelsCuts.Count; ++j)
                    {
                        if (i != j && labelsCuts[j].Contains(Second))
                            return -1;
                    }
                    return i;
                }
                if (labelsCuts[i].Contains(Second))
                {
                    for (int j = 0; j < labelsCuts.Count; ++j)
                    {
                        if (i != j && labelsCuts[j].Contains(First))
                            return -1;
                    }
                    return i;
                }
            }
            return -1;
        }

        public bool IsAWitnessByMI(IList<Label> witness, int group, double threshold)//BN用，PCAlgorithm
        {
            double MI = 0;
            double m = 0;
            for (int i = 0; i < Math.Pow(2, witness.Count); ++i)
            {
                //初始化
                int numberOfXTrue = 0;
                int numberOfXFalse = 0;
                int numberOfYTrue = 0;
                int numberOfYFalse = 0;
                int numberOfXTrueYTrue = 0;
                int numberOfXTrueYFalse = 0;
                int numberOfXFalseYTrue = 0;
                int numberOfXFalseAYFalse = 0;
                int N = 0;
                int numberOfZ = 0;
                Labelset labelset = new Labelset(witness, i);
                //统计数量信息
                foreach (Sentence sentence in Variable.Sentences)
                {
                    foreach (Annotation annotation in sentence.AnnotaitonGroups[group].AnnotatorAnnotationDic.Values)
                    {
                        ++N;
                        if (annotation.IsAccordingToLabelset(labelset))
                        {
                            ++numberOfZ;
                            if (annotation.Labels[First])
                            {
                                ++numberOfXTrue;
                                if (annotation.Labels[Second])
                                {
                                    ++numberOfYTrue;
                                    ++numberOfXTrueYTrue;
                                }
                                else
                                {
                                    ++numberOfYFalse;
                                    ++numberOfXTrueYFalse;
                                }
                            }
                            else
                            {
                                ++numberOfXFalse;
                                if (annotation.Labels[Second])
                                {
                                    ++numberOfYTrue;
                                    ++numberOfXFalseYTrue;
                                }
                                else
                                {
                                    ++numberOfYFalse;
                                    ++numberOfXFalseAYFalse;
                                }
                            }
                        }
                    }
                }
                MI += getConditionalMI(numberOfXTrue, numberOfXFalse, numberOfYTrue, numberOfYFalse,
                    numberOfXTrueYTrue, numberOfXTrueYFalse, numberOfXFalseYTrue, numberOfXFalseAYFalse, numberOfZ, N);
                m = N;
            }
            return SpecialFunction.chisqc(Math.Pow(2, witness.Count), 2 * m * MI) >= threshold;//（右尾面积）越小越有联系（不独立），越大越独立
        }


        private double getConditionalMI(int numberOfXTrue, int numberOfXFalse, int numberOfYTrue, int numberOfYFalse,
            int numberOfXTrueYTrue, int numberOfXTrueYFalse, int numberOfXFalseYTrue, int numberOfXFalseYFalse, double numberOfZ, int N)
        {
            double mi = 0;
            if (numberOfXTrueYTrue != 0)
                mi += (numberOfXTrueYTrue / numberOfZ) * Math.Log((numberOfXTrueYTrue * numberOfZ) / (numberOfXTrue * numberOfYTrue), 2);
            if (numberOfXTrueYFalse != 0)
                mi += (numberOfXTrueYFalse / numberOfZ) * Math.Log((numberOfXTrueYFalse * numberOfZ) / (numberOfXTrue * numberOfYFalse), 2);
            if (numberOfXFalseYTrue != 0)
                mi += (numberOfXFalseYTrue / numberOfZ) * Math.Log((numberOfXFalseYTrue * numberOfZ) / (numberOfXFalse * numberOfYTrue), 2);
            if (numberOfXFalseYTrue != 0)
                mi += (numberOfXFalseYFalse / numberOfZ) * Math.Log((numberOfXFalseYFalse * numberOfZ) / (numberOfXFalse * numberOfYFalse), 2);
            mi *= numberOfZ / N;
            return mi;
        }

        public bool IsAWitnessByProbability(IList<Label> witness, int group, double threshold)//BN用
        {
            //被测证据为空集时（First，Second）无条件独立
            if (witness.Count == 0)
            {
                //初始化
                int numberOfLabel1True = 0;
                int numberOfLabel1False = 0;
                int numberOfLabel2True = 0;
                int numberOfLabel2False = 0;
                int numberOfLabel1TrueLabel2True = 0;
                int numberOfLabel1TrueLabel2False = 0;
                int numberOfLabel1FalseLabel2True = 0;
                int numberOfLabel1FalseLabel2False = 0;
                int N = 0;
                //统计数量信息
                foreach (Sentence sentence in Variable.Sentences)
                {
                    foreach (Annotation annotation in sentence.AnnotaitonGroups[group].AnnotatorAnnotationDic.Values)
                    {
                        ++N;
                        if (annotation.Labels[First])
                        {
                            ++numberOfLabel1True;
                            if (annotation.Labels[Second])
                            {
                                ++numberOfLabel2True;
                                ++numberOfLabel1TrueLabel2True;
                            }
                            else
                            {
                                ++numberOfLabel2False;
                                ++numberOfLabel1TrueLabel2False;
                            }
                        }
                        else
                        {
                            ++numberOfLabel1False;
                            if (annotation.Labels[Second])
                            {
                                ++numberOfLabel2True;
                                ++numberOfLabel1FalseLabel2True;
                            }
                            else
                            {
                                ++numberOfLabel2False;
                                ++numberOfLabel1FalseLabel2False;
                            }
                        }
                    }
                }
                //判断是否独立
                if (!TwoLabelsAreIndependent(numberOfLabel1True, numberOfLabel2True, numberOfLabel1False, numberOfLabel2False,
                    numberOfLabel1TrueLabel2True, numberOfLabel1TrueLabel2False, numberOfLabel1FalseLabel2True, numberOfLabel1FalseLabel2False, N, threshold))
                    return false;
                else return true;
            }
            //被测证据不为空集时
            for (int i = 0; i < Math.Pow(2, witness.Count); ++i)
            {
                //初始化
                int numberOfLabel1True = 0;
                int numberOfLabel1False = 0;
                int numberOfLabel2True = 0;
                int numberOfLabel2False = 0;
                int numberOfLabel1TrueLabel2True = 0;
                int numberOfLabel1TrueLabel2False = 0;
                int numberOfLabel1FalseLabel2True = 0;
                int numberOfLabel1FalseLabel2False = 0;
                int N = 0;
                Labelset labelset = new Labelset(witness, i);
                //统计数量信息
                foreach (Sentence sentence in Variable.Sentences)
                {
                    foreach (Annotation annotation in sentence.AnnotaitonGroups[group].AnnotatorAnnotationDic.Values)
                    {
                        if (annotation.IsAccordingToLabelset(labelset))
                        {
                            ++N;
                            if (annotation.Labels[First])
                            {
                                ++numberOfLabel1True;
                                if (annotation.Labels[Second])
                                {
                                    ++numberOfLabel2True;
                                    ++numberOfLabel1TrueLabel2True;
                                }
                                else
                                {
                                    ++numberOfLabel2False;
                                    ++numberOfLabel1TrueLabel2False;
                                }
                            }
                            else
                            {
                                ++numberOfLabel1False;
                                if (annotation.Labels[Second])
                                {
                                    ++numberOfLabel2True;
                                    ++numberOfLabel1FalseLabel2True;
                                }
                                else
                                {
                                    ++numberOfLabel2False;
                                    ++numberOfLabel1FalseLabel2False;
                                }
                            }
                        }
                    }
                }
                //判断是否独立
                if (!TwoLabelsAreIndependent(numberOfLabel1True, numberOfLabel2True, numberOfLabel1False, numberOfLabel2False,
                    numberOfLabel1TrueLabel2True, numberOfLabel1TrueLabel2False, numberOfLabel1FalseLabel2True, numberOfLabel1FalseLabel2False, N, threshold))
                    return false;
            }
            return true;
        }

        private bool TwoLabelsAreIndependent(int numberOfLabel1True, int numberOfLabel2True, int numberOfLabel1False, int numberOfLabel2False,
            int numberOfLabel1TrueLabel2True, int numberOfLabel1TrueLabel2False, int numberOfLabel1FalseLabel2True, int numberOfLabel1FalseLabel2False, double N, double threshold)
        {
            if (threshold == 0)
                return numberOfLabel1TrueLabel2True * N == numberOfLabel1True * numberOfLabel2True &&
                      numberOfLabel1TrueLabel2False * N == numberOfLabel1True * numberOfLabel2False &&
                      numberOfLabel1FalseLabel2True * N == numberOfLabel1False * numberOfLabel2True &&
                      numberOfLabel1FalseLabel2False * N == numberOfLabel1False * numberOfLabel2False;
            else
                return Math.Abs(numberOfLabel1TrueLabel2True / N - (numberOfLabel1True / N) * (numberOfLabel2True / N)) <= threshold &&
                    Math.Abs(numberOfLabel1TrueLabel2False / N - (numberOfLabel1True / N) * (numberOfLabel2False / N)) <= threshold &&
                    Math.Abs(numberOfLabel1FalseLabel2True / N - (numberOfLabel1False / N) * (numberOfLabel2True / N)) <= threshold &&
                    Math.Abs(numberOfLabel1FalseLabel2False / N - (numberOfLabel1False / N) * (numberOfLabel2False / N)) <= threshold;
        }

        public override string ToString()
        {
            return "(" + First + ", " + Second + ")";
        }
    }

    //用于计算annotator工作量
    sealed class Similarity
    {
        public double TotalSimilarity;
        public double AverageSimilarity
        {
            get
            {
                return TotalSimilarity / NumberOfAnnotatedSentences;
            }
        }
        public int NumberOfAnnotatedSentences;
        public double PercentOfTotalSimilarity
        {
            get
            {
                return 100 * TotalSimilarity / Variable.TotalSimilarity;
            }
        }
        public double PercentOfWorkload
        {
            get
            {
                return 100 * NumberOfAnnotatedSentences / Variable.TotalNumberOfAnnotatedTimes;
            }
        }
        public double differenceBetweenSimilarityAndWorkload
        {
            get
            {
                return PercentOfTotalSimilarity - PercentOfWorkload;
            }
        }

        public Similarity()
        {
            TotalSimilarity = 0;
            NumberOfAnnotatedSentences = 0;
        }
    }
}