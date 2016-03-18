using Interoperability.Entity;
using System;
using System.Collections.Generic;

namespace Interoperability.Cascaded
{
    /// <summary>
    /// 既包含Label又包含Truth
    /// 因为要继承，所以是class不是struct。
    /// </summary>
    abstract class Labeltruth
    {
        public Label Label;
        public bool Truth;

        public Labeltruth(Label label, bool truth)
        {
            this.Label = label;
            this.Truth = truth;
        }

        public override string ToString()
        {
            return Label + "=" + (Truth ? 1 : 0);
        }
    }

    /// <summary>
    /// 为Pr_t_s而作
    /// </summary>
    sealed class SourceLabeltruth : Labeltruth, IEquatable<SourceLabeltruth>
    {
        public SourceLabeltruth(Label label, bool truth) : base(label, truth) { }

        /// <summary>
        /// 与本Source Labeltruth相对应的Sentence的List。
        /// 训练用，所以只遍历TrainConstant里的Sentence
        /// </summary>
        public IList<Sentence> AccordedSentenceList
        {
            get
            {
                IList<Sentence> result = new List<Sentence>();
                foreach (Sentence sentence in TrainConstant.SentenceList)
                {
                    if (sentence.GoldSourceAnnotation.LabelAndTruthDic[this.Label] == this.Truth)
                    {
                        result.Add(sentence);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 相当于原项目的Sij（s->i, t->j）。
        /// 暂时为最大似然估计。未考虑worker bias。
        /// 训练。
        /// </summary>
        /// <param name="t">t</param>
        /// <param name="groupindex">组号</param>
        /// <returns></returns>
        public double Pr_t_s(TargetLabeltruth t, int groupindex)
        {
            double numerator = 0;
            double denominator = 0;

            if (this.AccordedSentenceList.Count != 0)
            {
                //遍历与此Srouce Labeltruth对应的Sentence List（与分组无关）
                foreach (Sentence sentence in this.AccordedSentenceList)
                {
                    //此sentence被标过的target annotation（与分组有关）
                    foreach (TargetAnnotation targetAnnotation in sentence.TargetWorkerTargetAnnotationDicGroup[groupindex].Values)
                    {
                        //被标次数
                        ++denominator;
                        //此target annotation里含有要找的target label truth
                        if (targetAnnotation.AccordingWithTargetLabeltruth(t))
                            ++numerator;
                    }
                }
                return numerator / denominator;
                //return Math.Pow(numerator / denominator, ProbabilityConstant.Pr_t[t]);
            }
            else
                return 0;
        }

        public override int GetHashCode()
        {
            return this.Label.GetHashCode() * 10 + this.Truth.GetHashCode();
        }

        public bool Equals(SourceLabeltruth other)
        {
            return this.Label == other.Label && this.Truth == other.Truth;
        }
    }

    /// <summary>
    /// 为了Pr_T_t而作。
    /// </summary>
    sealed class TargetLabeltruth : Labeltruth, IEquatable<TargetLabeltruth>
    {
        public TargetLabeltruth(Label label, bool truth) : base(label, truth) { }

        /// <summary>
        /// 计算已知Target Label为true求Target Annotation的概率。
        /// </summary>
        /// <param name="targetAnnotation">Target Annotation</param>
        /// <returns>
        /// 已知Target Label的Truth求Target Annotation的概率。
        /// 如Target Label的Truth与其在Target Annotation的Truth不符，则返回0。
        /// </returns>
        /// （观察重复使用情况）
        public double Pr_T_t(TargetAnnotation targetAnnotation)
        {
            #region 不符时直接返回0
            if (targetAnnotation.LabelAndTruthDic[this.Label] != this.Truth)
                return 0;
            #endregion

            #region 符时求Pr_T和Pr_t
            //如已经计算过，直接取即可，不用重新计算。
            //因为符合t的T共有2^|T|个，所以对于每个t都有2^|T|个T对应。
            if (CascadedConstant.Pr_T_t[this].ContainsKey(targetAnnotation))
                return CascadedConstant.Pr_T_t[this][targetAnnotation];
            else
            {
                double nT = 0;//分子
                double nt = 0;//分母
                foreach (Sentence sentence in TrainConstant.SentenceList)
                {
                    foreach (TargetAnnotation targetannotation in sentence.TargetWorkerTargetAnnotationDic.Values)
                    {
                        if (targetannotation.LabelAndTruthDic[this.Label] == this.Truth)
                        {
                            ++nt; 
                            if (targetAnnotation.Equals(targetannotation))
                                ++nT;
                        }
                    }
                }
                double pribability = nT / nt;
                //存储，以备以后再用
                CascadedConstant.Pr_T_t[this].Add(targetAnnotation, pribability);
                return pribability;
            }
            #endregion
        }

        public override int GetHashCode()
        {
            return this.Label.GetHashCode() * 10 + this.Truth.GetHashCode();
        }

        public bool Equals(TargetLabeltruth other)
        {
            return this.Label == other.Label && this.Truth == other.Truth;
        }
    }
}