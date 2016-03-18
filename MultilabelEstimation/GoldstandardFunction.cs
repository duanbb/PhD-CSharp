using MultilabelEstimation.Group;
using System.Collections.Generic;

namespace MultilabelEstimation
{
    static class GoldstandardFunction
    {
        static private Result GenerateJointJointResult(ICollection<Annotation> annotations)
        {
            IDictionary<Annotation, int> dependentResultsAndTimes = new Dictionary<Annotation, int>();//<结果，次数>
            foreach (Annotation annotation in annotations)
            {
                if (dependentResultsAndTimes.ContainsKey(annotation))
                    ++dependentResultsAndTimes[annotation];
                else
                    dependentResultsAndTimes.Add(annotation, 1);
            }
            //选出被标最多项（N项同时被标最多次时，取N项之并）
            List<KeyValuePair<Annotation, int>> sortedResultAndTimes = new List<KeyValuePair<Annotation, int>>(dependentResultsAndTimes);
            sortedResultAndTimes.Sort(delegate(KeyValuePair<Annotation, int> s1, KeyValuePair<Annotation, int> s2)
            {
                return s2.Value.CompareTo(s1.Value);
            });
            int maxTimes = sortedResultAndTimes[0].Value;
            Result result = new Result(maxTimes / (double)annotations.Count);
            foreach (KeyValuePair<Annotation, int> annotationAndFrequency in sortedResultAndTimes)
            {
                if (annotationAndFrequency.Value == maxTimes)
                {
                    foreach (Label label in Variable.LabelArray)
                    {
                        if (annotationAndFrequency.Key.Labels[label])
                        {
                            result.Labels[label] = true;
                        }
                    }
                }
                else break;
            }
            return result;
        }

        static private Result GenerateJointOnlyOneResult(ICollection<Annotation> annotations)
        {
            IDictionary<Annotation, int> dependentResultsAndTimes = new Dictionary<Annotation, int>();//<结果，次数>
            foreach (Annotation annotation in annotations)
            {
                if (dependentResultsAndTimes.ContainsKey(annotation))
                    ++dependentResultsAndTimes[annotation];
                else
                    dependentResultsAndTimes.Add(annotation, 1);
            }
            //选出被标最多项（N项同时被标最多次时，取N项之并）
            List<KeyValuePair<Annotation, int>> sortedResultAndTimes = new List<KeyValuePair<Annotation, int>>(dependentResultsAndTimes);
            sortedResultAndTimes.Sort(delegate(KeyValuePair<Annotation, int> s1, KeyValuePair<Annotation, int> s2)
            {
                if (s1.Value != s2.Value)
                    return s2.Value.CompareTo(s1.Value);
                else if (s1.Key.NumberOfTrueLabel != s2.Key.NumberOfTrueLabel)
                    return s2.Key.NumberOfTrueLabel.CompareTo(s1.Key.NumberOfTrueLabel);
                else return s1.Key.IntLabel.CompareTo(s2.Key.IntLabel);
            });
            Result result = new Result(sortedResultAndTimes[0].Value / (double)annotations.Count);
            foreach (Label label in Variable.LabelArray)
            {
                if (sortedResultAndTimes[0].Key.Labels[label])
                    result.Labels[label] = true;
            }
            return result;
        }

        static private Result GenerateSeparateOverTrueLabelNumberResult(ICollection<Annotation> annotations)
        {
            double numberOfTrueLabel = 0;
            int numberOfAnnotation = 0;
            IDictionary<Label, int> labelAndFrequency = new Dictionary<Label, int>();
            foreach (Label label in Variable.LabelArray)
            {
                labelAndFrequency.Add(label, 0);
            }
            foreach (Annotation annotation in annotations)
            {
                numberOfTrueLabel += annotation.NumberOfTrueLabel;
                ++numberOfAnnotation;
                foreach (Label label in annotation.Labels.Keys)
                {
                    if (annotation.Labels[label])
                        ++labelAndFrequency[label];
                }
            }
            List<KeyValuePair<Label, int>> sortedLabelAndTimes = new List<KeyValuePair<Label, int>>(labelAndFrequency);
            sortedLabelAndTimes.Sort(delegate(KeyValuePair<Label, int> s1, KeyValuePair<Label, int> s2)
            {
                return s2.Value.CompareTo(s1.Value);
            });
            double averageNumberOfTrueLabel = numberOfTrueLabel / numberOfAnnotation;
            Result result = new Result(averageNumberOfTrueLabel);
            if (averageNumberOfTrueLabel == 0)//所有作为参数传入的annotation都为Neutral
                return result;
            int currentFrequency = 0;
            for (int i = 0; averageNumberOfTrueLabel - i > 0 || sortedLabelAndTimes[i].Value == currentFrequency; ++i)
            {
                result.Labels[sortedLabelAndTimes[i].Key] = true;
                currentFrequency = sortedLabelAndTimes[i].Value;
            }
            return result;
        }

        static private Result GenerateSeparateOverHalfResult(ICollection<Annotation> annotations)
        {
            IDictionary<Label, int> labelAndFrequency = new Dictionary<Label, int>();
            foreach (Label label in Variable.LabelArray)
            {
                labelAndFrequency.Add(label, 0);
            }
            foreach (Annotation annotation in annotations)
            {
                foreach (Label label in annotation.Labels.Keys)
                {
                    if (annotation.Labels[label])
                        ++labelAndFrequency[label];
                }
            }
            List<KeyValuePair<Label, int>> sortedLabelAndTimes = new List<KeyValuePair<Label, int>>(labelAndFrequency);
            sortedLabelAndTimes.Sort(delegate(KeyValuePair<Label, int> s1, KeyValuePair<Label, int> s2)
            {
                return s2.Value.CompareTo(s1.Value);
            });
            double half = annotations.Count / 2;
            Result result = new Result(half);
            for (int i = 0; i < sortedLabelAndTimes.Count; ++i)
            {
                if (sortedLabelAndTimes[i].Value >= half)
                    result.Labels[sortedLabelAndTimes[i].Key] = true;
                else
                    break;
            }
            return result;
        }

        static private Result JointTwoResults(Result result1, Result result2)
        {
            Result result = new Result();
            foreach (Label label in Variable.LabelArray)
            {
                if (result1.Labels[label] || result2.Labels[label])
                    result.Labels[label] = true;
            }
            return result;
        }

        static private Result JointThreeResults(Result result1, Result result2, Result result3)
        {
            Result result = new Result();
            foreach (Label label in Variable.LabelArray)
            {
                if (result1.Labels[label] || result2.Labels[label] || result3.Labels[label])
                    result.Labels[label] = true;
            }
            return result;
        }

        static public Result GetResult(ICollection<Annotation> annotations, GoldType goldType)
        {
            switch (goldType)
            {
                case GoldType.Joint:
                    return GenerateJointJointResult(annotations);
                case GoldType.JointOnlyOne:
                    return GenerateJointOnlyOneResult(annotations);
                case GoldType.SeparateOverTrueLabelNumber:
                    return GenerateSeparateOverTrueLabelNumberResult(annotations);
                case GoldType.SeparateOverHalf:
                    return GenerateSeparateOverHalfResult(annotations);
                case GoldType.SeperateOverTrueLabelNumberAndHalf:
                    return JointTwoResults(GoldstandardFunction.GenerateSeparateOverTrueLabelNumberResult(annotations), GoldstandardFunction.GenerateSeparateOverHalfResult(annotations));
                case GoldType.SeperateOverTrueLabelNumberAndHalfAndJoint:
                    return JointThreeResults(GoldstandardFunction.GenerateSeparateOverTrueLabelNumberResult(annotations), GoldstandardFunction.GenerateSeparateOverHalfResult(annotations), GoldstandardFunction.GenerateJointJointResult(annotations));
                default:
                    return new Result();
            }
        }
    }
}
