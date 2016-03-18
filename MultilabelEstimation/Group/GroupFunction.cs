using System;
using System.Collections.Generic;

namespace MultilabelEstimation.Group
{
    static class GroupFunction
    {
        //各情感总数降序排列
        static public Label[] DescendLabelsByNumber(int group)
        {
            IDictionary<Label, int> numberOfEachLabel = new Dictionary<Label, int>();
            foreach (Label label in Variable.LabelArray)
            {
                numberOfEachLabel.Add(label, 0);
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (Annotation annotation in sentence.AnnotaitonGroups[group].AnnotatorAnnotationDic.Values)
                {
                    foreach (Label label in Variable.LabelArray)
                    {
                        if (annotation.Labels[label])
                        {
                            ++numberOfEachLabel[label];
                        }
                    }
                }
            }
            List<KeyValuePair<Label, int>> sortedLabel = new List<KeyValuePair<Label, int>>(numberOfEachLabel);
            sortedLabel.Sort(delegate(KeyValuePair<Label, int> s1, KeyValuePair<Label, int> s2)
            {
                return s2.Value.CompareTo(s1.Value);
            });
            Label[] labelArray = new Label[Variable.LabelArray.Length];
            for (int a = 0; a < sortedLabel.Count; ++a)
            {
                labelArray[a] = sortedLabel[a].Key;
            }
            return labelArray;
        }
    }
}