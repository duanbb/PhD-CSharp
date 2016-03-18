using MultilabelEstimation.Algorithm.Personality;
using MultilabelEstimation.Algorithm.Personality.PeTM;
using System.Collections.Generic;
using System.Linq;

namespace MultilabelEstimation.Group
{
    sealed class NumericResult
    {
        public IDictionary<Label, double> Labels;
        public bool Mu
        {
            get
            {
                foreach (Label label in Variable.LabelArray)
                {
                    if (Labels[label] > 0)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public NumericResult()
        {
            Labels = new Dictionary<Label, double>();
            foreach (Label label in Variable.LabelArray)
            {
                Labels.Add(label, 0);
            }
        }
        public Result ToBinaryResult()
        {
            Result result = new Result();
            foreach (Label label in Variable.LabelArray)
            {
                if (Labels[label] >= 0.5)
                {
                    result.Labels[label] = true;
                    result.Probability *= Labels[label];
                }
                else result.Probability *= 1 - Labels[label];
            }
            return result;
        }
    }

    sealed class Result : Annotation
    {
        public double Probability;

        public Result()
        {
            this.Probability = 1;
        }

        public Result(double probability)
        {
            this.Probability = probability;
        }

        public Result(Result otherResult)//复制构造函数
        {
            foreach (Label label in otherResult.Labels.Keys)
            {
                this.Labels[label] = otherResult.Labels[label];
            }
            this.Probability = otherResult.Probability;
        }

        public Result(KeyValuePair<Labelset, double> resultAndProbability)
        {
            this.Labels = resultAndProbability.Key.Labels;
            this.Probability = resultAndProbability.Value;
        }

        public void TransToPersonalityResult(Will will)
        {
            if (will == Will.strong)
            {
                foreach (Label label in this.Labels.Keys.ToArray())
                {
                    if (this.Labels[label] && PersonalityVariable.WeakAffects.Contains(label))
                    {
                        if (PersonalityVariable.ExchangeLabel == ExchangeLabel.Yes)
                        {
                            switch (label)
                            {
                                case Label.fondness:
                                    this.Labels[Label.happiness] = true;
                                    break;
                                case Label.sadness:
                                    this.Labels[Label.anger] = true;
                                    break;
                            }
                        }
                        this.Labels[label] = false;//无替换
                    }
                }
            }
            else
            {
                foreach (Label label in this.Labels.Keys.ToArray())
                {
                    if (this.Labels[label] && PersonalityVariable.StrongAffects.Contains(label))
                    {
                        switch (label)
                        {
                            case Label.happiness:
                                this.Labels[Label.fondness] = true;
                                break;
                            case Label.anger:
                                this.Labels[Label.sadness] = true;
                                break;
                        }
                        this.Labels[label] = false;//无替换
                    }
                }
            }
        }
    }

    sealed class AnnotationGroup
    {
        public IDictionary<Annotator, Annotation> AnnotatorAnnotationDic;
        public NumericResult IDSNumResult;
        public Result IDSResult;
        public NumericResult SIDSNumResult;
        public Result SIDSResult
        {
            get
            {
                return SIDSNumResult.ToBinaryResult();
            }
        }
        public NumericResult JDDSNumResult;
        public Result JDDSResult;
        public NumericResult SDDSNumResult;
        public Result IDDSResult;
        public Result DTDDSResult;
        public Result TDDSResult;
        public Result SDDSResult;
        public NumericResult DTDDSNumResult;
        public NumericResult TDDSNumResult;
        public NumericResult PDSNumResult;
        public NumericResult SPDSNumResult;
        public Result PDSResult;
        public Result SPDSResult;
        public Result NDDSResult;
        public Result PeTMResult;
        public Result PeMVResult;
        public Result PeTResult;

        public NumericResult MVNumResult//独立依赖结果一样
        {
            get
            {
                NumericResult mvNumResult = new NumericResult();
                foreach (Annotation annotation in AnnotatorAnnotationDic.Values)
                {
                    foreach (Label label in Variable.LabelArray)
                    {
                        if (annotation.Labels[label])
                        {
                            ++mvNumResult.Labels[label];
                        }
                    }
                }
                foreach (Label label in Variable.LabelArray)
                {
                    mvNumResult.Labels[label] /= AnnotatorAnnotationDic.Count;
                }
                return mvNumResult;
            }
        }
        public Result MVResult//Majority Vote
        {
            get
            {
                return GoldstandardFunction.GetResult(AnnotatorAnnotationDic.Values, Variable.MVType);
            }
        }
        public AnnotationGroup()
        {
            AnnotatorAnnotationDic = new Dictionary<Annotator, Annotation>();
        }
        public Result GetResultFromAlgorithmName(string name)
        {
            switch (name)
            {
                case "MV":
                    return MVResult;
                case "IDS":
                    return IDSResult;
                case "PDS":
                    return PDSResult;
                case "IDDS":
                    return IDDSResult;
                case "JDDS":
                    return JDDSResult;
                case "NDDS":
                    return NDDSResult;
                case "PeTM":
                    return PeTMResult;
                case "PeT":
                    return PeTResult;
                case "PeMV":
                    return PeMVResult;
                default:
                    return null;
            }
        }
        public NumericResult GetNumericResultFromName(string name)
        {
            switch (name)
            {
                case "MV":
                    return MVNumResult;
                case "IDS":
                    return IDSNumResult;
                case "PDS":
                    return PDSNumResult;
                case "JDDS":
                    return JDDSNumResult;
                default:
                    return null;
            }
        }
    }

    static class GroupVariable
    {
        static public IList<Annotator>[] AnnotatorGroups;
    }
}