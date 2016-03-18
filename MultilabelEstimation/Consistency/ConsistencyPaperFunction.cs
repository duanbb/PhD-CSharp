using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;
using System.Linq;

namespace MultilabelEstimation.Consistency
{

    static class ConsistencyPaperFunction
    {
        //生成角色被标的label的频率
        static public void CalcuateCharacterConsistency()
        {
            IDictionary<Character, IDictionary<Label, int>> characterConsistencyOfAnnotations = new Dictionary<Character, IDictionary<Label, int>>();
            IDictionary<Character, IDictionary<Label, int>> characterConsistencyOfGoldstandards = new Dictionary<Character, IDictionary<Label, int>>();
            foreach (Character character in ConsistencyVariable.Characters)
            {
                characterConsistencyOfAnnotations.Add(character, new Dictionary<Label, int>());
                characterConsistencyOfGoldstandards.Add(character, new Dictionary<Label, int>());
                foreach (Label label in Variable.LabelArray)
                {
                    characterConsistencyOfAnnotations[character].Add(label, 0);
                    characterConsistencyOfGoldstandards[character].Add(label, 0);
                }
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (Annotator annotator in Variable.Annotators)
                {
                    if (Variable.Data[annotator].ContainsKey(sentence))//判断此annotator是否标了此sentence
                    {
                        foreach (Annotation annotation in Variable.Data[annotator][sentence])
                        {
                            foreach (Label label in annotation.Labels.Keys)
                            {
                                if (annotation.Labels[label])
                                {
                                    ++characterConsistencyOfAnnotations[sentence.Character][label];
                                }
                            }
                        }
                    }
                }
                foreach (Label label in sentence.BinaryGold.Labels.Keys)
                {
                    if (sentence.BinaryGold.Labels[label])
                    {
                        ++characterConsistencyOfGoldstandards[sentence.Character][label];
                    }
                }
            }
            //输出到文件
            StreamWriter file = new StreamWriter("consistency.csv", false, Encoding.Default);
            foreach (Character character in characterConsistencyOfAnnotations.Keys)
            {
                string sOfAnnotations = character.ID;
                foreach (Label label in characterConsistencyOfAnnotations[character].Keys)
                {
                    sOfAnnotations += "," + characterConsistencyOfAnnotations[character][label];
                }
                file.WriteLine(sOfAnnotations);
                string sOfGoldstandards = character.ID;
                foreach (Label label in characterConsistencyOfGoldstandards[character].Keys)
                {
                    sOfGoldstandards += "," + characterConsistencyOfGoldstandards[character][label];
                }
                file.WriteLine(sOfGoldstandards);
            }
            file.Close();
        }

        //将label分成两组（strong-willed, weak-willed）
        static public void GroupLabels()
        {
            #region 注释（5个最大为真）
            //IDictionary<string, Label[]> characterConsistencies = new Dictionary<string, Label[]>();
            //characterConsistencies.Add("勇", new Label[] { Label.excitement, Label.sadness, Label.anger, Label.disgust, Label.surprise });
            //characterConsistencies.Add("政", new Label[] { Label.excitement, Label.happiness, Label.sadness, Label.anger, Label.disgust });
            //characterConsistencies.Add("お母さん", new Label[] { Label.relief, Label.happiness, Label.sadness, Label.anger, Label.disgust });
            //characterConsistencies.Add("小野", new Label[] { Label.excitement, Label.fondness, Label.happiness, Label.anger, Label.surprise });
            //characterConsistencies.Add("山田", new Label[] { Label.excitement, Label.fondness, Label.happiness, Label.anger, Label.surprise });
            //characterConsistencies.Add("他の子", new Label[] { Label.excitement, Label.fondness, Label.happiness, Label.sadness, Label.fear });
            //characterConsistencies.Add("先生", new Label[] { Label.excitement, Label.relief, Label.fondness, Label.anger, Label.disgust });
            //characterConsistencies.Add("尾沢先生", new Label[] { Label.relief, Label.fondness, Label.sadness, Label.disgust, Label.fear });
            //characterConsistencies.Add("二郎", new Label[] { Label.excitement, Label.relief, Label.sadness, Label.anger, Label.disgust });
            //characterConsistencies.Add("誠", new Label[] { Label.excitement, Label.relief, Label.fondness, Label.happiness, Label.sadness });
            //characterConsistencies.Add("正", new Label[] { Label.excitement, Label.sadness, Label.anger, Label.disgust, Label.fear });
            //characterConsistencies.Add("新", new Label[] { Label.relief, Label.fondness, Label.happiness, Label.sadness, Label.disgust });
            //characterConsistencies.Add("年", new Label[] { Label.excitement, Label.relief, Label.fondness, Label.happiness, Label.sadness });
            //characterConsistencies.Add("声", new Label[] { Label.excitement, Label.relief, Label.fondness, Label.happiness, Label.surprise });
            //characterConsistencies.Add("きみ子", new Label[] { Label.excitement, Label.relief, Label.fondness, Label.happiness, Label.sadness });
            //characterConsistencies.Add("二人", new Label[] { Label.excitement, Label.relief, Label.fondness, Label.happiness, Label.sadness });
            //characterConsistencies.Add("おばさん", new Label[] { Label.excitement, Label.relief, Label.fondness, Label.happiness, Label.surprise });
            //characterConsistencies.Add("お姉さん", new Label[] { Label.excitement, Label.relief, Label.fondness, Label.happiness, Label.sadness });
            //characterConsistencies.Add("お母さん", new Label[] { Label.excitement, Label.relief, Label.anger, Label.disgust, Label.fear });
            //characterConsistencies.Add("お父さん", new Label[] { Label.excitement, Label.relief, Label.sadness, Label.anger, Label.disgust });
            //characterConsistencies.Add("米屋さん", new Label[] { Label.excitement, Label.relief, Label.fondness, Label.happiness, Label.surprise });
            //if (((IList)characterConsistencies["勇"]).Contains(Label.excitement)) { }
            #endregion

            IDictionary<Label, int[]> labelDistribution = new Dictionary<Label, int[]>();
            labelDistribution.Add(Label.sadness, new int[] { 76, 166, 93, 0, 5, 25, 4, 16, 97, 213, 16, 32, 22, 0, 56, 5, 0, 7, 1, 72, 1 });
            labelDistribution.Add(Label.relief, new int[] { 50, 84, 245, 8, 8, 12, 61, 14, 78, 211, 0, 29, 25, 1, 29, 42, 34, 10, 8, 32, 17 });
            labelDistribution.Add(Label.fondness, new int[] { 28, 59, 101, 23, 43, 31, 5, 2, 32, 132, 0, 51, 27, 8, 66, 14, 74, 18, 2, 18, 25 });
            labelDistribution.Add(Label.happiness, new int[] { 53, 141, 116, 15, 30, 41, 3, 0, 33, 127, 0, 30, 6, 17, 76, 46, 79, 8, 1, 16, 19 });
            labelDistribution.Add(Label.excitement, new int[] { 118, 134, 13, 13, 24, 14, 12, 1, 91, 141, 9, 6, 7, 16, 30, 20, 13, 3, 4, 30, 9 });
            labelDistribution.Add(Label.disgust, new int[] { 131, 123, 21, 9, 7, 12, 7, 2, 74, 58, 16, 8, 0, 0, 16, 0, 0, 0, 9, 97, 1 });
            labelDistribution.Add(Label.anger, new int[] { 184, 180, 114, 24, 30, 5, 126, 0, 91, 76, 23, 3, 0, 0, 7, 0, 0, 0, 3, 39, 0 });
            labelDistribution.Add(Label.surprise, new int[] { 122, 55, 74, 10, 17, 11, 2, 0, 32, 86, 3, 2, 0, 22, 10, 3, 9, 0, 1, 19, 3 });
            labelDistribution.Add(Label.fear, new int[] { 55, 58, 13, 0, 1, 17, 2, 2, 30, 77, 6, 1, 2, 0, 22, 1, 0, 2, 3, 19, 1 });
            labelDistribution.Add(Label.shame, new int[] { 33, 38, 16, 0, 1, 2, 1, 0, 26, 13, 0, 7, 0, 0, 25, 0, 0, 0, 0, 5, 8 });

            //简单差
            IDictionary<Label, int> varianceFromRelief = new Dictionary<Label, int>();
            foreach (Label label in Variable.LabelArray)
            {
                int variance = 0;
                for (int i = 0; i < 21; ++i)
                {
                    variance += Math.Abs(labelDistribution[Label.excitement][i] - labelDistribution[label][i]);
                }
                varianceFromRelief.Add(label, variance);
            }
            List<KeyValuePair<Label, int>> sortedVariances = new List<KeyValuePair<Label, int>>(varianceFromRelief);
            sortedVariances.Sort(delegate(KeyValuePair<Label, int> s1, KeyValuePair<Label, int> s2)
            {
                return s1.Value.CompareTo(s2.Value);
            });

            //欧几里得距离
            IDictionary<Label, double> distanceFromRelief = new Dictionary<Label, double>();
            foreach (Label label in Variable.LabelArray)
            {
                double distance = 0;
                for (int i = 0; i < 21; ++i)
                {
                    distance += Math.Pow(labelDistribution[Label.excitement][i] - labelDistribution[label][i], 2);
                }
                distanceFromRelief.Add(label, Math.Pow(distance, 0.5));
            }
            List<KeyValuePair<Label, double>> sortedDistance = new List<KeyValuePair<Label, double>>(distanceFromRelief);
            sortedDistance.Sort(delegate(KeyValuePair<Label, double> s1, KeyValuePair<Label, double> s2)
            {
                return s1.Value.CompareTo(s2.Value);
            });

            //先匹配，人工分组
            IList<LabelPair> labelPairList = new List<LabelPair>();//前后无序，45个，用于初始化
            IList<Label> traversedLabels = new List<Label>();
            foreach (Label label1 in Variable.LabelArray)
            {
                traversedLabels.Add(label1);
                foreach (Label label2 in Variable.LabelArray)
                {
                    if (!traversedLabels.Contains(label2))
                    {
                        LabelPair labelPair = new LabelPair(label1, label2);
                        double distance = 0;
                        for (int i = 0; i < 21; ++i)//两个故事，21个角色
                        {
                            //distance += Math.Abs(labelDistribution[label1][i] - labelDistribution[label2][i]);
                            distance += Math.Pow(labelDistribution[label1][i] - labelDistribution[label2][i], 2);
                        }
                        //labelPair.Weight = distance;
                        labelPair.Weight = Math.Pow(distance, 0.5);
                        labelPairList.Add(labelPair);
                    }
                }
            }
            LabelPair[] labelPairs = MultilabelEstimation.Algorithm.PDS.PDSFunction.MinimumWeightedPerfectMatching(labelPairList, true);
        }

        static public void LabelpairFrequency()
        {
            //先匹配，人工分组
            IDictionary<LabelPair, int> labelPairDic = new Dictionary<LabelPair, int>();//前后无序，45个，用于初始化
            IList<Label> traversedLabels = new List<Label>();
            foreach (Label label1 in Variable.LabelArray)
            {
                traversedLabels.Add(label1);
                foreach (Label label2 in Variable.LabelArray)
                {
                    if (!traversedLabels.Contains(label2))
                    {
                        labelPairDic.Add(new LabelPair(label1, label2), 0);
                    }
                }
            }

            foreach (Annotator annotator in Variable.Data.Keys)
            {
                foreach (Sentence sentence in Variable.Data[annotator].Keys)
                {
                    foreach (Annotation annotation in Variable.Data[annotator][sentence])
                    {
                        foreach (LabelPair labelpair in labelPairDic.Keys.ToArray())
                        {
                            if (annotation.Labels[labelpair.First] && annotation.Labels[labelpair.Second])
                                ++labelPairDic[labelpair];
                        }
                    }
                }
            }
            List<KeyValuePair<LabelPair, int>> sortedLabelPairs = new List<KeyValuePair<LabelPair, int>>(labelPairDic);
            sortedLabelPairs.Sort(delegate(KeyValuePair<LabelPair, int> s1, KeyValuePair<LabelPair, int> s2)
            {
                return s2.Value.CompareTo(s1.Value);
            });

            IList<LabelPair> labelPairList = new List<LabelPair>(labelPairDic.Keys);
            foreach (LabelPair labelpair in labelPairList)
            {
                labelpair.Weight = labelPairDic[labelpair];
            }
            LabelPair[] labelPairs = MultilabelEstimation.Algorithm.PDS.PDSFunction.MinimumWeightedPerfectMatching(labelPairList, false);
        }
    }
}