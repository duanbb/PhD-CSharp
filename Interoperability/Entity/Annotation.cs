using Interoperability.Cascaded;
using System;
using System.Collections.Generic;

namespace Interoperability.Entity
{
    abstract class Annotation : IEquatable<Annotation>
    {
        public IDictionary<Label, bool> LabelAndTruthDic;

        public bool None
        {
            get//OK
            {
                foreach (Label label in LabelAndTruthDic.Keys)
                {
                    if (LabelAndTruthDic[label])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public override string ToString()
        {
            if (this.None) return "None";
            string result = string.Empty;
            foreach (Label label in LabelAndTruthDic.Keys)
            {
                if (LabelAndTruthDic[label])
                    result += label + "|";
            }
            return result.Remove(result.Length - 1);
        }

        public bool Equals(Annotation other)
        {
            foreach (KeyValuePair<Label, bool> labelAndValue in LabelAndTruthDic)
            {
                if (other.LabelAndTruthDic[labelAndValue.Key] == labelAndValue.Value)
                    continue;
                else return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int intLabel = 0;
            int i = 0;
            foreach (bool truth in this.LabelAndTruthDic.Values)
            {
                intLabel += Convert.ToInt16(truth) * Convert.ToInt16(Math.Pow(2, i));
                ++i;
            }
            return intLabel;
        }

        public int NumberOfTrueLabels
        {
            get
            {
                int result = 0;
                foreach (bool truth in this.LabelAndTruthDic.Values)
                {
                    if (truth)
                        ++result;
                }
                return result;
            }
        }
    }

    sealed class SourceAnnotation : Annotation, IEquatable<SourceAnnotation>
    {
        public SourceAnnotation()
        {
            LabelAndTruthDic = new Dictionary<Label, bool>();
            foreach (Label label in Constant.SourceTaxonomy.LabelArray)
            {
                LabelAndTruthDic.Add(label, false);
            }
        }

        public SourceAnnotation(Label[] trueSourceLabelArray)
        {
            LabelAndTruthDic = new Dictionary<Label, bool>();
            foreach (Label label in Constant.SourceTaxonomy.LabelArray)
            {
                LabelAndTruthDic.Add(label, false);
            }
            foreach (Label trueLabel in trueSourceLabelArray)
            {
                LabelAndTruthDic[trueLabel] = true;
            }
        }

        public double[] ToDoubleArray
        {
            get//OK
            {
                double[] result = new double[Constant.SourceTaxonomy.LabelArray.Length];
                int index = 0;
                foreach (bool truth in this.LabelAndTruthDic.Values)
                {
                    result[index] = truth ? 1 : -1;
                    ++index;
                }
                return result;
            }
        }

        //dictionary判断key是否相等时，先判断GetHashCode是否相等，再判断Equals是否true。
        public bool Equals(SourceAnnotation other)
        {
            foreach (KeyValuePair<Label, bool> labelAndValue in LabelAndTruthDic)
            {
                if (other.LabelAndTruthDic[labelAndValue.Key] == labelAndValue.Value)
                    continue;
                else return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int intLabel = 0;
            int i = 0;
            foreach (Label label in Constant.SourceTaxonomy.LabelArray)
            {
                intLabel += Convert.ToInt16(LabelAndTruthDic[label]) * Convert.ToInt16(Math.Pow(2, i));
                ++i;
            }
            return intLabel;
        }

        #region For Cascaded
        ///// <summary>
        ///// 计算已知Source Annotation求Source Label为true的概率。
        ///// </summary>
        ///// <param name="sourceLabeltruth">Source Label和其Truth</param>
        ///// <returns>
        ///// Label的Truth与其在Annotation里的Truth相同：1。
        ///// Label的Truth与其在Annotation里的Truth不同：0。
        ///// </returns>
        ///// 废弃
        //public double Pr_s_S(SourceLabeltruth sourceLabeltruth)
        //{
        //    return this.LabelAndTruthDic[sourceLabeltruth.Label] == sourceLabeltruth.Truth ? 1 : 0;
        //}

        /// <summary>
        /// 计算Pr(T|S)。
        /// 两种遍历结果一致。
        /// 论文公式10。
        /// </summary>
        /// <param name="T">作为条件的Target Annotation</param>
        /// <returns>Pr(T|S)</returns>
        public double Pr_T_S(TargetAnnotation T)
        {
            double result = 1;

            #region T在里S在外（20141202公式第一行）。
            /////先求P(T|s)，再求P(T|S)。
            /////经验证，连加时（如PPT），谁在里谁在外都一样。根据乘法结合律。
            /////连乘时又不一样了，每遍历一个s，P(t|T)就被乘一次。
            //foreach (KeyValuePair<Label, bool> sourceLabelAndTruth in this.LabelAndTruthDic)//遍历s
            //{
            //    SourceLabeltruth s = new SourceLabeltruth(sourceLabelAndTruth.Key, sourceLabelAndTruth.Value);
            //    double Pr_T_s = 1;//观察用，所以单独写出来。

            //    #region 非测试
            //    foreach (KeyValuePair<Label, bool> targetLabelAndTruth in T.LabelAndTruthDic)//遍历t
            //    {
            //        TargetLabeltruth t = new TargetLabeltruth(targetLabelAndTruth.Key, targetLabelAndTruth.Value);
            //        Pr_T_s *= t.Pr_T_t(T) * ProbabilityConstant.Pr_t_s[s][t];
            //        //Pr_T_s *= ProbabilityConstant.Pr_t_s[s][t];;
            //    }
            //    #endregion

            //    #region 测试
            //    //foreach(Label label in Constant.TargetTaxonomy.LabelArray)
            //    //{
            //    //TargetLabeltruth tTrue = new TargetLabeltruth(label, true);
            //    //double trueProbability = tTrue.Pr_T_t(T) * ProbabilityConstant.Pr_t_s[s][tTrue];
            //    //TargetLabeltruth tFalse = new TargetLabeltruth(label, false);
            //    //double falseProbability = tFalse.Pr_T_t(T) * ProbabilityConstant.Pr_t_s[s][tFalse];
            //    //}
            //    #endregion

            //    result *= Pr_T_s;
            //}
            #endregion

            #region s在里t在外（20141202公式第二行）。先求P(t|S)，再求P(T|S)。
            foreach (KeyValuePair<Label, bool> targetLabelAndTruth in T.LabelAndTruthDic)//先遍历t
            {
                #region P(t|S)未normalize
                TargetLabeltruth t = new TargetLabeltruth(targetLabelAndTruth.Key, targetLabelAndTruth.Value);
                double Pr_t_S = 1;//观察用，所以单独写出来。
                foreach (KeyValuePair<Label, bool> sourceLabelAndTruth in this.LabelAndTruthDic)//遍历s
                {
                    SourceLabeltruth s = new SourceLabeltruth(sourceLabelAndTruth.Key, sourceLabelAndTruth.Value);
                    Pr_t_S *= CascadedConstant.Pr_t_s[s][t];
                }
                //result *= t.Pr_T_t(T) * Pr_t_S;
                result *= (Pr_t_S / Math.Pow(CascadedConstant.Pr_t[t], Constant.SourceTaxonomy.LabelArray.Length - 1));
                #endregion

                #region 测试,P(t|S)，normalize t
                //TargetLabeltruth tTrue = new TargetLabeltruth(targetLabelAndTruth.Key, true);
                //TargetLabeltruth tFalse = new TargetLabeltruth(targetLabelAndTruth.Key, false);
                //double trueProbability = 1;
                //double falseProbability = 1;
                //foreach (KeyValuePair<Label, bool> sourceLabelAndTruth in this.LabelAndTruthDic)//遍历s
                //{
                //    SourceLabeltruth s = new SourceLabeltruth(sourceLabelAndTruth.Key, sourceLabelAndTruth.Value);
                //    trueProbability *= ProbabilityConstant.Pr_t_s[s][tTrue];
                //    falseProbability *= ProbabilityConstant.Pr_t_s[s][tFalse];
                //}
                //if (targetLabelAndTruth.Value)
                //    result *= tTrue.Pr_T_t(T) * (trueProbability) / (trueProbability + falseProbability);
                //else
                //    result *= tFalse.Pr_T_t(T) * (falseProbability) / (trueProbability + falseProbability);
                #endregion
            }
            #endregion

            return result;
        }
        #endregion
    }

    sealed class TargetAnnotation : Annotation, IEquatable<TargetAnnotation>
    {
        /// <summary>
        /// 默认建立neutral
        /// </summary>
        public TargetAnnotation()
        {
            LabelAndTruthDic = new Dictionary<Label, bool>();
            foreach (Label label in Constant.TargetTaxonomy.LabelArray)
            {
                LabelAndTruthDic.Add(label, false);
            }
        }

        public TargetAnnotation(Label[] trueTargetLabelArray)
        {
            LabelAndTruthDic = new Dictionary<Label, bool>();
            foreach (Label label in Constant.TargetTaxonomy.LabelArray)
            {
                LabelAndTruthDic.Add(label, false);
            }
            foreach (Label trueLabel in trueTargetLabelArray)
            {
                LabelAndTruthDic[trueLabel] = true;
            }
        }

        public double[] ToDoubleArray
        {
            get//OK
            {
                double[] result = new double[Constant.TargetTaxonomy.LabelArray.Length];
                int index = 0;
                foreach (bool truth in this.LabelAndTruthDic.Values)
                {
                    result[index] = truth ? 1 : -1;
                    ++index;
                }
                return result;
            }
        }

        //dictionary判断key是否相等时，先判断GetHashCode是否相等，再判断Equals是否true。
        public bool Equals(TargetAnnotation other)
        {
            foreach (KeyValuePair<Label, bool> labelAndValue in LabelAndTruthDic)
            {
                if (other.LabelAndTruthDic[labelAndValue.Key] == labelAndValue.Value)
                    continue;
                else return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int intLabel = 0;
            int i = 0;
            foreach (Label label in Constant.TargetTaxonomy.LabelArray)
            {
                intLabel += Convert.ToInt16(LabelAndTruthDic[label]) * Convert.ToInt16(Math.Pow(2, i));
                ++i;
            }
            return intLabel;
        }

        #region For Probability
        public bool AccordingWithTargetLabeltruth(TargetLabeltruth targetLabeltruth)
        {
            return this.LabelAndTruthDic[targetLabeltruth.Label] == targetLabeltruth.Truth;
        }
        #endregion
    }
}