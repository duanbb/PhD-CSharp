using System;
using System.Collections.Generic;
using System.Linq;

namespace Interoperability.Entity
{
    /// <summary>
    /// //应该分为TrainSentence（子类）和非TrainSentence（父类），
    /// 但是分的话，初始化就要分别写函数，造成不必要的麻烦。
    /// 所以暂时未分。
    /// </summary>
    class Sentence
    {
        public int Index;//观察用
        public string Speech;
        public IDictionary<SourceWorker, SourceAnnotation> SourceWorkerSourceAnnotationDic;
        public IDictionary<TargetWorker, TargetAnnotation> TargetWorkerTargetAnnotationDic;
        public IDictionary<TargetWorker, TargetAnnotation>[] TargetWorkerTargetAnnotationDicGroup;//只在TrainConstant.SentenceList里的Sentence才会用到

        public IDictionary<SourceAnnotation, double> SortedSourceAnnotationDic;//观察用

        public IDictionary<Label, double> SortedSourceLabelDic;//观察用

        public SourceAnnotation GoldSourceAnnotation;

        /// <summary>
        /// 记录此sentence被标注为annotation的次数，观察用
        /// </summary>
        public IDictionary<TargetAnnotation, double> SortedTargetAnnotationDic;

        public IDictionary<Label, double> SortedTargetLabelDic;//观察用

        /// <summary>
        /// Test对比用。
        /// </summary>
        public TargetAnnotation GoldTargetAnnotation;

        public Sentence(int index, string speech)
        {
            Index = index;
            Speech = speech;
            SourceWorkerSourceAnnotationDic = new Dictionary<SourceWorker, SourceAnnotation>();
            TargetWorkerTargetAnnotationDic = new Dictionary<TargetWorker, TargetAnnotation>();
        }

        public override string ToString()
        {
            return Index + "|" + Speech;
        }

        #region For Space
        public IDictionary<Label, double> SpaceRealTargetLabelDic;
        public TargetAnnotation SpaceTargetAnnotation()
        {
            TargetAnnotation result = new TargetAnnotation();
            foreach (KeyValuePair<Label, double> labelAndReal in this.SpaceRealTargetLabelDic)
            {
                result.LabelAndTruthDic[labelAndReal.Key] = labelAndReal.Value >= 0;
            }
            return result;
        }

        public double SpaceSimilarity()
        {
            return GeneralFunction.Similarity(this.GoldTargetAnnotation, this.SpaceTargetAnnotation());
        }
        #endregion

        #region For Probability
        /// <summary>
        /// 得到最优的TargetAnnotation。
        /// 排序只为观察，最后只输出具有最优值的Target Annotation。
        /// </summary>
        /// <returns></returns>
        public TargetAnnotation CascadedTargetAnnotation()
        {
            if (Cascaded.CascadedConstant.Pr_T_S[this.GoldSourceAnnotation].Count != 0)
                return Cascaded.CascadedConstant.Pr_T_S[this.GoldSourceAnnotation].First().Key;
            return new TargetAnnotation();
        }

        public double CascadedSimilarity()
        {
            return GeneralFunction.Similarity(this.GoldTargetAnnotation, this.CascadedTargetAnnotation());
        }
        #endregion

        #region For MLE
        public TargetAnnotation MLETargetAnnotation()
        {
            if (MLE.MLEConstant.Pr_T_S.ContainsKey(this.GoldSourceAnnotation))
            {
                IList<Label> trueLabels = new List<Label>();
                double maxNumber = MLE.MLEConstant.Pr_T_S[this.GoldSourceAnnotation].First().Value;
                foreach (KeyValuePair<TargetAnnotation, double> targetAnnotationAndNumber in MLE.MLEConstant.Pr_T_S[this.GoldSourceAnnotation])
                {
                    if (maxNumber == targetAnnotationAndNumber.Value)
                    {
                        foreach (KeyValuePair<Label, bool> labelAndTruth in targetAnnotationAndNumber.Key.LabelAndTruthDic)
                        {
                            if (labelAndTruth.Value && !trueLabels.Contains(labelAndTruth.Key))
                                trueLabels.Add(labelAndTruth.Key);
                        }
                    }
                    else break;
                }
                return new TargetAnnotation(trueLabels.ToArray());
            }
            else
                return new TargetAnnotation();
        }

        public double MLESimilarity()
        {
            return GeneralFunction.Similarity(this.GoldTargetAnnotation, this.MLETargetAnnotation());
        }
        #endregion


        //下列属性在TrainConstant.SentenceList里的Sentence才会用到
        public IDictionary<TargetWorker, double> TemporaryNonormalizeWeightDic;
        public IDictionary<TargetWorker, double> TemporaryNormalizeWeightDic;
        public IDictionary<TargetWorker, double> TemporaryOptimizeWeightDic;//废弃
        public IDictionary<TargetWorker, double> OtherNonormalizeWeightDic;
        public IDictionary<TargetWorker, double> OtherNormalizeWeightDic;
    }

    static class SentenceProperty
    {
        static public void GoldSourceAnnotation()
        {
            switch (Constant.Gold)
            {
                case Gold.Top:
                    foreach (Sentence sentence in Constant.SentenceList)
                    {
                        IList<Label> trueLabels = new List<Label>();
                        double maxNumber = sentence.SortedSourceAnnotationDic.First().Value;
                        foreach (KeyValuePair<SourceAnnotation, double> sourceAnnotationAndNumber in sentence.SortedSourceAnnotationDic)
                        {
                            if (maxNumber == sourceAnnotationAndNumber.Value)
                            {
                                foreach (KeyValuePair<Label, bool> labelAndTruth in sourceAnnotationAndNumber.Key.LabelAndTruthDic)
                                {
                                    if (labelAndTruth.Value && !trueLabels.Contains(labelAndTruth.Key))
                                        trueLabels.Add(labelAndTruth.Key);
                                }
                            }
                            else break;
                        }
                        sentence.GoldSourceAnnotation = new SourceAnnotation(trueLabels.ToArray());
                    }
                    break;
                case Gold.TopTwo:
                    foreach (Sentence sentence in Constant.SentenceList)
                    {
                        IList<Label> trueLabels = new List<Label>();
                        double maxNumber = sentence.SortedSourceAnnotationDic.First().Value;
                        int count = 0;
                        foreach (KeyValuePair<SourceAnnotation, double> sourceAnnotationAndNumber in sentence.SortedSourceAnnotationDic)
                        {
                            if (maxNumber == sourceAnnotationAndNumber.Value)
                            {
                                foreach (KeyValuePair<Label, bool> labelAndTruth in sourceAnnotationAndNumber.Key.LabelAndTruthDic)
                                {
                                    if (labelAndTruth.Value && !trueLabels.Contains(labelAndTruth.Key))
                                        trueLabels.Add(labelAndTruth.Key);
                                }
                                ++count;
                                continue;
                            }
                            else if (count < 2)
                            {
                                foreach (KeyValuePair<Label, bool> labelAndTruth in sourceAnnotationAndNumber.Key.LabelAndTruthDic)
                                {
                                    if (labelAndTruth.Value && !trueLabels.Contains(labelAndTruth.Key))
                                        trueLabels.Add(labelAndTruth.Key);
                                }
                                ++count;
                            }
                            else break;
                        }
                        sentence.GoldSourceAnnotation = new SourceAnnotation(trueLabels.ToArray());
                    }
                    break;
                case Gold.Halfmore:
                    foreach (Sentence sentence in Constant.SentenceList)
                    {
                        IList<Label> trueLabels = new List<Label>();
                        foreach (KeyValuePair<Label, double> labelAndCount in sentence.SortedSourceLabelDic)
                        {
                            if (labelAndCount.Value >= sentence.SourceWorkerSourceAnnotationDic.Count / 2.0)
                            {
                                if (labelAndCount.Key != Label.None)
                                    trueLabels.Add(labelAndCount.Key);
                            }
                            else break;
                        }
                        sentence.GoldSourceAnnotation = new SourceAnnotation(trueLabels.ToArray());
                    }
                    break;
                case Gold.TopAndHalfmore:
                    foreach (Sentence sentence in Constant.SentenceList)
                    {
                        IList<Label> trueLabels = new List<Label>();
                        double maxNumber = sentence.SortedSourceAnnotationDic.First().Value;
                        foreach (KeyValuePair<SourceAnnotation, double> sourceAnnotationAndNumber in sentence.SortedSourceAnnotationDic)
                        {
                            if (maxNumber == sourceAnnotationAndNumber.Value)
                            {
                                foreach (KeyValuePair<Label, bool> labelAndTruth in sourceAnnotationAndNumber.Key.LabelAndTruthDic)
                                {
                                    if (labelAndTruth.Value && !trueLabels.Contains(labelAndTruth.Key))
                                        trueLabels.Add(labelAndTruth.Key);
                                }
                            }
                            else break;
                        }
                        foreach (KeyValuePair<Label, double> labelAndCount in sentence.SortedSourceLabelDic)
                        {
                            if (labelAndCount.Value >= sentence.SourceWorkerSourceAnnotationDic.Count / 2.0)
                            {
                                if (!trueLabels.Contains(labelAndCount.Key))
                                    trueLabels.Add(labelAndCount.Key);
                            }
                            else break;
                        }
                        sentence.GoldSourceAnnotation = new SourceAnnotation(trueLabels.ToArray());
                    }
                    break;
            }
        }

        static public void GoldTargetAnnotation()
        {
            switch (Constant.Gold)
            {
                case Gold.Top:
                    foreach (Sentence sentence in Constant.SentenceList)
                    {
                        IList<Label> trueLabels = new List<Label>();
                        double maxNumber = sentence.SortedTargetAnnotationDic.First().Value;
                        foreach (KeyValuePair<TargetAnnotation, double> targetAnnotationAndNumber in sentence.SortedTargetAnnotationDic)
                        {
                            if (maxNumber == targetAnnotationAndNumber.Value)
                            {
                                foreach (KeyValuePair<Label, bool> labelAndTruth in targetAnnotationAndNumber.Key.LabelAndTruthDic)
                                {
                                    if (labelAndTruth.Value && !trueLabels.Contains(labelAndTruth.Key))
                                        trueLabels.Add(labelAndTruth.Key);
                                }
                            }
                            else break;
                        }
                        sentence.GoldTargetAnnotation = new TargetAnnotation(trueLabels.ToArray());
                    }
                    break;
                case Gold.TopTwo:
                    foreach (Sentence sentence in Constant.SentenceList)
                    {
                        IList<Label> trueLabels = new List<Label>();
                        double maxNumber = sentence.SortedTargetAnnotationDic.First().Value;
                        int count = 0;
                        foreach (KeyValuePair<TargetAnnotation, double> targetAnnotationAndNumber in sentence.SortedTargetAnnotationDic)
                        {
                            if (maxNumber == targetAnnotationAndNumber.Value)
                            {
                                foreach (KeyValuePair<Label, bool> labelAndTruth in targetAnnotationAndNumber.Key.LabelAndTruthDic)
                                {
                                    if (labelAndTruth.Value && !trueLabels.Contains(labelAndTruth.Key))
                                        trueLabels.Add(labelAndTruth.Key);
                                }
                                ++count;
                                continue;
                            }
                            else if (count < 2)
                            {
                                foreach (KeyValuePair<Label, bool> labelAndTruth in targetAnnotationAndNumber.Key.LabelAndTruthDic)
                                {
                                    if (labelAndTruth.Value && !trueLabels.Contains(labelAndTruth.Key))
                                        trueLabels.Add(labelAndTruth.Key);
                                }
                                ++count;
                            }
                            else break;
                        }
                        sentence.GoldTargetAnnotation = new TargetAnnotation(trueLabels.ToArray());
                    }
                    break;
                case Gold.Halfmore:
                    foreach (Sentence sentence in Constant.SentenceList)
                    {
                        IList<Label> trueLabels = new List<Label>();
                        foreach (KeyValuePair<Label, double> labelAndCount in sentence.SortedTargetLabelDic)
                        {
                            if (labelAndCount.Value >= sentence.TargetWorkerTargetAnnotationDic.Count / 2.0)
                                trueLabels.Add(labelAndCount.Key);
                            else break;
                        }
                        sentence.GoldTargetAnnotation = new TargetAnnotation(trueLabels.ToArray());
                    }
                    break;
                case Gold.TopAndHalfmore:
                    foreach (Sentence sentence in Constant.SentenceList)
                    {
                        IList<Label> trueLabels = new List<Label>();
                        double maxNumber = sentence.SortedTargetAnnotationDic.First().Value;
                        foreach (KeyValuePair<TargetAnnotation, double> targetAnnotationAndNumber in sentence.SortedTargetAnnotationDic)
                        {
                            if (maxNumber == targetAnnotationAndNumber.Value)
                            {
                                foreach (KeyValuePair<Label, bool> labelAndTruth in targetAnnotationAndNumber.Key.LabelAndTruthDic)
                                {
                                    if (labelAndTruth.Value && !trueLabels.Contains(labelAndTruth.Key))
                                        trueLabels.Add(labelAndTruth.Key);
                                }
                            }
                            else break;
                        }
                        foreach (KeyValuePair<Label, double> labelAndCount in sentence.SortedTargetLabelDic)
                        {
                            if (labelAndCount.Value >= sentence.TargetWorkerTargetAnnotationDic.Count / 2.0)
                            {
                                if (!trueLabels.Contains(labelAndCount.Key))
                                    trueLabels.Add(labelAndCount.Key);
                            }
                            else break;
                        }
                        sentence.GoldTargetAnnotation = new TargetAnnotation(trueLabels.ToArray());
                    }
                    break;
            }
        }

        static public void SortedSourceWorkerSourceAnnotationCountDic()
        {
            foreach (Sentence sentence in Constant.SentenceList)
            {
                sentence.SourceWorkerSourceAnnotationDic = sortDictionaryForSourceAnnotation(sentence.SourceWorkerSourceAnnotationDic);
            }
        }

        static public void SortedTargetWorkerTargetAnnotationCountDic()
        {
            foreach (Sentence sentence in Constant.SentenceList)
            {
                sentence.TargetWorkerTargetAnnotationDic = sortDictionaryForTargetAnnotation(sentence.TargetWorkerTargetAnnotationDic);
            }
        }

        static private IDictionary<T, SourceAnnotation> sortDictionaryForSourceAnnotation<T>(IDictionary<T, SourceAnnotation> dictionary)
        {
            List<KeyValuePair<T, SourceAnnotation>> sortedElements = new List<KeyValuePair<T, SourceAnnotation>>(dictionary);
            switch (Constant.Filter)
            {
                case Filter.More:
                    sortedElements.Sort(delegate(KeyValuePair<T, SourceAnnotation> s1, KeyValuePair<T, SourceAnnotation> s2)
                    {
                        return s2.Value.NumberOfTrueLabels.CompareTo(s1.Value.NumberOfTrueLabels);//大到小
                    });
                    break;
                case Filter.Less:
                    sortedElements.Sort(delegate(KeyValuePair<T, SourceAnnotation> s1, KeyValuePair<T, SourceAnnotation> s2)
                    {
                        return s1.Value.NumberOfTrueLabels.CompareTo(s2.Value.NumberOfTrueLabels);//小到大
                    });
                    break;
            }
            Dictionary<T, SourceAnnotation> result = new Dictionary<T, SourceAnnotation>();
            foreach (KeyValuePair<T, SourceAnnotation> element in sortedElements)
            {
                result.Add(element.Key, element.Value);
            }
            return result;
        }

        static private IDictionary<T, TargetAnnotation> sortDictionaryForTargetAnnotation<T>(IDictionary<T, TargetAnnotation> dictionary)
        {
            List<KeyValuePair<T, TargetAnnotation>> sortedElements = new List<KeyValuePair<T, TargetAnnotation>>(dictionary);
            switch (Constant.Filter)
            {
                case Filter.More:
                    sortedElements.Sort(delegate(KeyValuePair<T, TargetAnnotation> s1, KeyValuePair<T, TargetAnnotation> s2)
                    {
                        return s2.Value.NumberOfTrueLabels.CompareTo(s1.Value.NumberOfTrueLabels);//大到小
                    });
                    break;
                case Filter.Less:
                    sortedElements.Sort(delegate(KeyValuePair<T, TargetAnnotation> s1, KeyValuePair<T, TargetAnnotation> s2)
                    {
                        return s1.Value.NumberOfTrueLabels.CompareTo(s2.Value.NumberOfTrueLabels);//小到大
                    });
                    break;
            }
            Dictionary<T, TargetAnnotation> result = new Dictionary<T, TargetAnnotation>();
            foreach (KeyValuePair<T, TargetAnnotation> element in sortedElements)
            {
                result.Add(element.Key, element.Value);
            }
            return result;
        }

        static public void SortedSourceAnnotationCountDic()
        {
            foreach (Sentence sentence in Constant.SentenceList)
            {
                IDictionary<SourceAnnotation, double> count = new Dictionary<SourceAnnotation, double>();
                foreach (SourceAnnotation sourceAnnotation in sentence.SourceWorkerSourceAnnotationDic.Values)
                {
                    if (count.ContainsKey(sourceAnnotation))
                        ++count[sourceAnnotation];
                    else
                        count.Add(sourceAnnotation, 1);
                }
                sentence.SortedSourceAnnotationDic = GeneralFunction.SortDictionary(count);
            }
        }

        static public void SortedTargetAnnotationCountDic()
        {
            foreach (Sentence sentence in Constant.SentenceList)
            {
                IDictionary<TargetAnnotation, double> count = new Dictionary<TargetAnnotation, double>();
                foreach (TargetAnnotation targetAnnotation in sentence.TargetWorkerTargetAnnotationDic.Values)
                {
                    if (count.ContainsKey(targetAnnotation))
                        ++count[targetAnnotation];
                    else
                        count.Add(targetAnnotation, 1);
                }
                sentence.SortedTargetAnnotationDic = GeneralFunction.SortDictionary(count);
            }
        }

        static public void SortedSourceLabelDic()
        {
            foreach (Sentence sentence in Constant.SentenceList)
            {
                IDictionary<Label, double> count = new Dictionary<Label, double>();
                foreach (SourceAnnotation sourceAnnotation in sentence.SourceWorkerSourceAnnotationDic.Values)
                {
                    if (!sourceAnnotation.None)
                    {
                        foreach (KeyValuePair<Label, bool> labelAndTruth in sourceAnnotation.LabelAndTruthDic)
                        {
                            if (labelAndTruth.Value)
                            {
                                if (count.ContainsKey(labelAndTruth.Key))
                                    ++count[labelAndTruth.Key];
                                else
                                    count.Add(labelAndTruth.Key, 1);
                            }
                        }
                    }
                    else
                    {
                        if (count.ContainsKey(Label.None))
                            ++count[Label.None];
                        else
                            count.Add(Label.None, 1);
                    }
                }
                sentence.SortedSourceLabelDic = GeneralFunction.SortDictionary(count);
            }
        }

        static public void SortedTargetLabelDic()
        {
            foreach (Sentence sentence in Constant.SentenceList)
            {
                IDictionary<Label, double> count = new Dictionary<Label, double>();
                foreach (TargetAnnotation targetAnnotation in sentence.TargetWorkerTargetAnnotationDic.Values)
                {
                    if(!targetAnnotation.None)
                    {
                        foreach (KeyValuePair<Label, bool> labelAndTruth in targetAnnotation.LabelAndTruthDic)
                        {
                            if (labelAndTruth.Value)
                            {
                                if (count.ContainsKey(labelAndTruth.Key))
                                    ++count[labelAndTruth.Key];
                                else
                                    count.Add(labelAndTruth.Key, 1);
                            }
                        }
                    }
                    else
                    {
                        if (count.ContainsKey(Label.None))
                            ++count[Label.None];
                        else
                            count.Add(Label.None, 1);
                    }
                }
                sentence.SortedTargetLabelDic = GeneralFunction.SortDictionary(count);
            }
        }

        static public void OtherNonormalizeWeightDic()
        {
            foreach (Sentence sentence in Constant.SentenceList)
            {
                IDictionary<TargetWorker, double> result = new Dictionary<TargetWorker, double>();
                foreach (TargetWorker targetWorker in sentence.TargetWorkerTargetAnnotationDic.Keys)//标过此句的worker
                {
                    double weight = 0;

                    //计算此人和其他人的标注的差别
                    foreach (KeyValuePair<TargetWorker, TargetAnnotation> targetWorkerAndTargetAnnotation in sentence.TargetWorkerTargetAnnotationDic)//此sentence被标过的TargetAnnotation
                    {
                        if (!targetWorker.Equals(targetWorkerAndTargetAnnotation.Key))//排除此人的标注
                        {
                            weight += GeneralFunction.Similarity(targetWorkerAndTargetAnnotation.Value, sentence.TargetWorkerTargetAnnotationDic[targetWorker]);
                        }
                    }
                    
                    result.Add(targetWorker, weight / (sentence.TargetWorkerTargetAnnotationDic.Count -1));
                }
                sentence.OtherNonormalizeWeightDic = GeneralFunction.SortDictionary(result);
            }
        }

        static public void TemporaryNonormalizeWeightDic()
        {
            foreach (Sentence sentence in Constant.SentenceList)
            {
                IDictionary<TargetWorker, double> result = new Dictionary<TargetWorker, double>();
                foreach (TargetWorker targetWorker in sentence.TargetWorkerTargetAnnotationDic.Keys)//标过此句的worker
                {
                    double weight = 0;
                    IDictionary<Label, double> temporaryVector = new Dictionary<Label, double>();//临时标注
                    foreach (Label targetLabel in Constant.TargetTaxonomy.LabelArray)
                    {
                        temporaryVector.Add(targetLabel, 0);
                    }

                    //计算临时标注
                    foreach (KeyValuePair<TargetWorker, TargetAnnotation> targetWorkerAndTargetAnnotation in sentence.TargetWorkerTargetAnnotationDic)//此sentence被标过的TargetAnnotation
                    {
                        if (!targetWorker.Equals(targetWorkerAndTargetAnnotation.Key))//排除此人的标注
                        {
                            foreach (Label targetLabel in Constant.TargetTaxonomy.LabelArray)
                            {
                                temporaryVector[targetLabel] += targetWorkerAndTargetAnnotation.Value.LabelAndTruthDic[targetLabel] ? 1 : -1;
                            }
                        }
                    }
                    foreach (Label targetLabel in Constant.TargetTaxonomy.LabelArray)
                    {
                        temporaryVector[targetLabel] /= (sentence.TargetWorkerTargetAnnotationDic.Count() - 1);
                    }

                    //计算此worker的标注和临时标注的差别
                    foreach (Label targetLabel in Constant.TargetTaxonomy.LabelArray)
                    {
                        weight += 2 - Math.Abs(temporaryVector[targetLabel] - (sentence.TargetWorkerTargetAnnotationDic[targetWorker].LabelAndTruthDic[targetLabel] ? 1 : -1));
                    }

                    result.Add(targetWorker, weight / (2 * Constant.TargetTaxonomy.LabelArray.Length));

                }
                sentence.TemporaryNonormalizeWeightDic = GeneralFunction.SortDictionary(result);
            }
        }

        static public void TemporaryNormalizeWeightDic()
        {
            foreach (Sentence sentence in Constant.SentenceList)
            {
                sentence.TemporaryNormalizeWeightDic = new Dictionary<TargetWorker, double>();
                double sum = 0;
                foreach (TargetWorker targetWorker in sentence.TemporaryNonormalizeWeightDic.Keys)
                {
                    sum += sentence.TemporaryNonormalizeWeightDic[targetWorker];
                }
                foreach (TargetWorker targetWorker in sentence.TemporaryNonormalizeWeightDic.Keys)
                {
                    sentence.TemporaryNormalizeWeightDic.Add(targetWorker, sentence.TemporaryNonormalizeWeightDic[targetWorker] / sum);
                }
            }
        }

        static public void OtherNormalizeWeightDic()
        {
            foreach (Sentence sentence in Constant.SentenceList)
            {
                sentence.OtherNormalizeWeightDic = new Dictionary<TargetWorker, double>();
                double sum = 0;
                foreach (TargetWorker targetWorker in sentence.OtherNonormalizeWeightDic.Keys)
                {
                    sum += sentence.OtherNonormalizeWeightDic[targetWorker];
                }
                foreach (TargetWorker targetWorker in sentence.OtherNonormalizeWeightDic.Keys)
                {
                    sentence.OtherNormalizeWeightDic.Add(targetWorker, sentence.OtherNonormalizeWeightDic[targetWorker] / sum);
                }
            }
        }

        /// <summary>
        /// 用Max和Min调节，让最差的人权重为0，最好的人权重为1。
        /// 貌似不合理。
        /// 已废弃。
        /// </summary>
        static public void TemporaryOptimizeWeightDic()
        {
            foreach (Sentence sentence in Constant.SentenceList)
            {
                sentence.TemporaryOptimizeWeightDic = new Dictionary<TargetWorker, double>();
                double max = sentence.TemporaryNonormalizeWeightDic.Values.Max();
                double min = sentence.TemporaryNonormalizeWeightDic.Values.Min();
                if (max != min)//所有人标注一致时会出现此情况
                {
                    double sum = 0;
                    foreach (TargetWorker targetWorker in sentence.TemporaryNonormalizeWeightDic.Keys)
                    {
                        double optimizedWeight = (sentence.TemporaryNonormalizeWeightDic[targetWorker] - min) / (max - min);
                        sentence.TemporaryOptimizeWeightDic.Add(targetWorker, optimizedWeight);
                        sum += optimizedWeight;
                    }
                    foreach (TargetWorker targetWorker in sentence.TemporaryNonormalizeWeightDic.Keys)
                    {
                        sentence.TemporaryOptimizeWeightDic[targetWorker] = sentence.TemporaryOptimizeWeightDic[targetWorker] / sum;
                    }
                }
                else
                {
                    foreach (TargetWorker targetWorker in sentence.TemporaryNonormalizeWeightDic.Keys)
                    {
                        sentence.TemporaryOptimizeWeightDic.Add(targetWorker, 1.0 / sentence.TemporaryNonormalizeWeightDic.Count());
                    }
                }
            }
        }
    }
}