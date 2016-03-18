using Interoperability.Entity;
using System.Collections.Generic;
using System.Linq;

namespace Interoperability.MLE
{
    static class MLEFunction
    {
        /// <summary>
        /// 论文公式2
        /// </summary>
        static public void Pr_T_S(int groupindex)
        {
            //因为每换一组就要清空一次，所以在这里而不是MLEConstant的构造函数里初始化。
            MLEConstant.Pr_T_S = new Dictionary<SourceAnnotation, IDictionary<TargetAnnotation, double>>();

            //开始计算
            foreach (Sentence sentence in TrainConstant.SentenceList)
            {
                SourceAnnotation sourceAnnotation = sentence.GoldSourceAnnotation;
                if (!MLEConstant.Pr_T_S.ContainsKey(sourceAnnotation))
                {
                    MLEConstant.Pr_T_S.Add(sourceAnnotation, new Dictionary<TargetAnnotation, double>());
                }
                foreach (TargetAnnotation targetAnnotation in sentence.TargetWorkerTargetAnnotationDicGroup[groupindex].Values)
                {
                    if (MLEConstant.Pr_T_S[sourceAnnotation].ContainsKey(targetAnnotation))
                    {
                        ++MLEConstant.Pr_T_S[sourceAnnotation][targetAnnotation];
                    }
                    else
                    {
                        MLEConstant.Pr_T_S[sourceAnnotation].Add(targetAnnotation, 1);
                    }
                }
            }

            //为次数排序就行，不用除Pr_S，因为对每个T，其对应的Pr_S都一样
            foreach (SourceAnnotation sourceAnnotation in MLEConstant.Pr_T_S.Keys.ToArray())
            {
                MLEConstant.Pr_T_S[sourceAnnotation] = GeneralFunction.SortDictionary(MLEConstant.Pr_T_S[sourceAnnotation]);
            }
        }
    }
}