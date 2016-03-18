using MultilabelEstimation.Consistency;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultilabelEstimation.Relation
{
    static class RelationFunction //Frequency指的是在一次标注全文里的频率
    {
        static private double smallerValue(double d1, double d2)
        {
            return d1 < d2 ? d1 : d2;
        }

        static public IDictionary<Tuple<Sentence, Sentence>, IDictionary<Tuple<Labelset, Labelset>, double>> InitializeLabelsetPairFrequencyForSij(Label[] labels, int groupIndex)
        {
            IDictionary<Tuple<Sentence, Sentence>, IDictionary<Tuple<Labelset, Labelset>, double>> labelPairFrequencyForSij = new Dictionary<Tuple<Sentence, Sentence>, IDictionary<Tuple<Labelset, Labelset>, double>>();
            //开头
            Tuple<Sentence, Sentence> sentencePair = Tuple.Create(new Sentence(-1, "##"), Variable.Sentences.First());
            labelPairFrequencyForSij.Add(sentencePair, new Dictionary<Tuple<Labelset, Labelset>, double>());
            foreach (Annotation j2 in Variable.Sentences.First().AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Values)
            {
                Tuple<Labelset, Labelset> Labelset = Tuple.Create(new Labelset(true), j2.ToLabelset(labels));
                if (labelPairFrequencyForSij[sentencePair].ContainsKey(Labelset))//角色
                    labelPairFrequencyForSij[sentencePair][Labelset] += Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                else
                    labelPairFrequencyForSij[sentencePair].Add(Labelset, Variable.NumberOfAnnotationsPerSentenceAfterGrouping);
            }
            //中间（站在当前往前看）
            for (int i = 1; i < Variable.Sentences.Count; ++i)
            {
                sentencePair = Tuple.Create(Variable.Sentences[i - 1], Variable.Sentences[i]);//角色（换另一个角色，不换角色，都算）
                labelPairFrequencyForSij.Add(sentencePair, new Dictionary<Tuple<Labelset, Labelset>, double>());
                foreach (Annotation j1 in Variable.Sentences[i - 1].AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Values)
                {
                    foreach (Annotation j2 in Variable.Sentences[i].AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Values)
                    {
                        Tuple<Labelset, Labelset> Labelset = Tuple.Create(j1.ToLabelset(labels), j2.ToLabelset(labels));
                        if (labelPairFrequencyForSij[sentencePair].ContainsKey(Labelset))
                            ++labelPairFrequencyForSij[sentencePair][Labelset];
                        else
                            labelPairFrequencyForSij[sentencePair].Add(Labelset, 1);
                    }
                }
            }
            //计算次数（因为多算了，要除掉，也就是说全故事只算一次）
            foreach (Tuple<Sentence, Sentence> sp in labelPairFrequencyForSij.Keys)
            {
                foreach (Tuple<Labelset, Labelset> LabelsetPair in labelPairFrequencyForSij[sp].Keys.ToArray())
                {
                    labelPairFrequencyForSij[sp][LabelsetPair] /= Math.Pow(Variable.NumberOfAnnotationsPerSentenceAfterGrouping, 2);
                }
            }
            return labelPairFrequencyForSij;
        }

        static public IDictionary<Tuple<Labelset, Labelset>, double> InitializeLabelsetPairFrequencyForPj(Label[] labels, int groupIndex)
        {
            IDictionary<Tuple<Labelset, Labelset>, double> LabelsetPairFrequencyForSentence = new Dictionary<Tuple<Labelset, Labelset>, double>();
            //开头
            foreach (Annotation j2 in Variable.Sentences.First().AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Values)
            {
                Tuple<Labelset, Labelset> LabelsetPair = Tuple.Create(new Labelset(true), j2.ToLabelset(labels));
                if (LabelsetPairFrequencyForSentence.ContainsKey(LabelsetPair))//句
                    LabelsetPairFrequencyForSentence[LabelsetPair] += Variable.NumberOfAnnotationsPerSentenceAfterGrouping;//因为是开头，所以不是加1，而是加标注的数量(cross strategy)
                else
                    LabelsetPairFrequencyForSentence.Add(LabelsetPair, Variable.NumberOfAnnotationsPerSentenceAfterGrouping);
            }
            //中间（站在当前往前看）
            for (int i = 1; i < Variable.Sentences.Count; ++i)
            {
                foreach (Annotation j1 in Variable.Sentences[i - 1].AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Values)//句
                {
                    foreach (Annotation j2 in Variable.Sentences[i].AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Values)
                    {
                        Tuple<Labelset, Labelset> LabelsetPair = Tuple.Create(j1.ToLabelset(labels), j2.ToLabelset(labels));
                        if (LabelsetPairFrequencyForSentence.ContainsKey(LabelsetPair))
                            ++LabelsetPairFrequencyForSentence[LabelsetPair];
                        else
                            LabelsetPairFrequencyForSentence.Add(LabelsetPair, 1);
                    }
                }
            }
            //计算次数（因为多算了，要除掉）
            foreach (Tuple<Labelset, Labelset> LabelsetPair in LabelsetPairFrequencyForSentence.Keys.ToArray())//重要编程技巧
            {
                LabelsetPairFrequencyForSentence[LabelsetPair] /= Math.Pow(Variable.NumberOfAnnotationsPerSentenceAfterGrouping, 2);
            }
            return LabelsetPairFrequencyForSentence;
        }

        static public IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> InitializeLabelsetPairFrequencyForMcj(Label[] labels, int groupIndex)
        {
            IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> labelPairFrequencyForCharacter = new Dictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>>();
            //开头
            Tuple<Character, Character> characterPair = Tuple.Create(new Character("##"), Variable.Sentences.First().Character);
            labelPairFrequencyForCharacter.Add(characterPair, new Dictionary<Tuple<Labelset, Labelset>, double>());
            foreach (Annotation j2 in Variable.Sentences.First().AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Values)
            {
                Tuple<Labelset, Labelset> Labelset = Tuple.Create(new Labelset(true), j2.ToLabelset(labels));
                if (labelPairFrequencyForCharacter[characterPair].ContainsKey(Labelset))//角色
                    labelPairFrequencyForCharacter[characterPair][Labelset] += Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                else
                    labelPairFrequencyForCharacter[characterPair].Add(Labelset, Variable.NumberOfAnnotationsPerSentenceAfterGrouping);
            }
            //中间（站在当前往前看）
            for (int i = 1; i < Variable.Sentences.Count; ++i)
            {
                characterPair = Tuple.Create(Variable.Sentences[i - 1].Character, Variable.Sentences[i].Character);//角色（换另一个角色，不换角色，都算）
                if (!labelPairFrequencyForCharacter.ContainsKey(characterPair))
                {
                    labelPairFrequencyForCharacter.Add(characterPair, new Dictionary<Tuple<Labelset, Labelset>, double>());
                }
                foreach (Annotation j1 in Variable.Sentences[i - 1].AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Values)
                {
                    foreach (Annotation j2 in Variable.Sentences[i].AnnotaitonGroups[groupIndex].AnnotatorAnnotationDic.Values)
                    {
                        Tuple<Labelset, Labelset> Labelset = Tuple.Create(j1.ToLabelset(labels), j2.ToLabelset(labels));
                        if (labelPairFrequencyForCharacter[characterPair].ContainsKey(Labelset))
                            ++labelPairFrequencyForCharacter[characterPair][Labelset];
                        else
                            labelPairFrequencyForCharacter[characterPair].Add(Labelset, 1);
                    }
                }
            }
            //计算次数（因为多算了，要除掉，也就是说全故事只算一次）
            foreach (Tuple<Character, Character> cp in labelPairFrequencyForCharacter.Keys)
            {
                foreach (Tuple<Labelset, Labelset> LabelsetPair in labelPairFrequencyForCharacter[cp].Keys.ToArray())
                {
                    labelPairFrequencyForCharacter[cp][LabelsetPair] /= Math.Pow(Variable.NumberOfAnnotationsPerSentenceAfterGrouping, 2);
                }
            }
            return labelPairFrequencyForCharacter;
        }

        //Independent里，Labelset只包含一个label，就是Key中的label
        static public IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>> InitializeIndependentBoolPairFrequencyForSentence(Label[] labels, int groupIndex)
        {
            IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>> independentLabelsetPairFrequencyForSentence = new Dictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>();
            foreach (Label label in labels)
            {
                independentLabelsetPairFrequencyForSentence.Add(label, InitializeLabelsetPairFrequencyForPj(new Label[] { label }, groupIndex));
            }
            return independentLabelsetPairFrequencyForSentence;
        }

        static public IDictionary<Tuple<Character, Character>, IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>> InitializeIndependentLabelsetPairFrequencyForCharacter(Label[] labels, int groupIndex)
        {
            IDictionary<Tuple<Character, Character>, IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>> initializeIndependentLabelsetPairFrequencyForCharacter = new Dictionary<Tuple<Character, Character>, IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>>();
            foreach (Label label in labels)
            {
                IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> LabelsetFrequencyForCharacter = InitializeLabelsetPairFrequencyForMcj(new Label[] { label }, groupIndex);
                foreach (Tuple<Character, Character> characterPair in LabelsetFrequencyForCharacter.Keys)
                {
                    if (!initializeIndependentLabelsetPairFrequencyForCharacter.ContainsKey(characterPair))
                        initializeIndependentLabelsetPairFrequencyForCharacter.Add(characterPair, new Dictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>());
                    initializeIndependentLabelsetPairFrequencyForCharacter[characterPair].Add(label, LabelsetFrequencyForCharacter[characterPair]);
                }
            }
            return initializeIndependentLabelsetPairFrequencyForCharacter;
        }

        static public IDictionary<Tuple<Labelset, Labelset>, double> RenewLabelsetPairFrequencyForSentence(bool addOne, Sij sij)
        {
            IDictionary<Tuple<Labelset, Labelset>, double> labelPairFrequencyForSentence = new Dictionary<Tuple<Labelset, Labelset>, double>();
            //开头
            KeyValuePair<Labelset, double> laterBestLabelset = sij.SortLabelsets(Variable.Sentences[0])[0];
            double addend = addOne ? 1 : laterBestLabelset.Value;
            labelPairFrequencyForSentence.Add(Tuple.Create(new Labelset(true), laterBestLabelset.Key), addend);

            //中间
            KeyValuePair<Labelset, double> formerBestLabelset;
            for (int i = 1; i < Variable.Sentences.Count; ++i)
            {
                formerBestLabelset = sij.SortLabelsets(Variable.Sentences[i - 1])[0];
                laterBestLabelset = sij.SortLabelsets(Variable.Sentences[i])[0];
                Tuple<Labelset, Labelset> Labelset = Tuple.Create(formerBestLabelset.Key, laterBestLabelset.Key);
                addend = addOne ? 1 : smallerValue(formerBestLabelset.Value, laterBestLabelset.Value);
                if (labelPairFrequencyForSentence.ContainsKey(Labelset))
                    labelPairFrequencyForSentence[Labelset] += addend;
                else
                    labelPairFrequencyForSentence.Add(Labelset, addend);
            }
            return labelPairFrequencyForSentence;
        }

        static public IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> RenewLabelsetPairFrequencyForCharacter(bool addOne, Sij sij)
        {
            IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> labelPairFrequencyForCharacter = new Dictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>>();
            //开头
            Tuple<Character, Character> characterPair = Tuple.Create(new Character("##"), Variable.Sentences.First().Character);
            labelPairFrequencyForCharacter.Add(characterPair, new Dictionary<Tuple<Labelset, Labelset>, double>());
            KeyValuePair<Labelset, double> laterBestLabelset = sij.SortLabelsets(Variable.Sentences[0])[0];
            double addend = addOne ? 1 : laterBestLabelset.Value;
            labelPairFrequencyForCharacter[characterPair].Add(Tuple.Create(new Labelset(true), laterBestLabelset.Key), addend);
            //中间
            KeyValuePair<Labelset, double> formerBestLabelset;
            for (int i = 1; i < Variable.Sentences.Count; ++i)
            {
                characterPair = Tuple.Create(Variable.Sentences[i - 1].Character, Variable.Sentences[i].Character);//角色（换另一个角色，不换角色，都算）
                formerBestLabelset = sij.SortLabelsets(Variable.Sentences[i - 1])[0];
                laterBestLabelset = sij.SortLabelsets(Variable.Sentences[i])[0];
                Tuple<Labelset, Labelset> Labelset = Tuple.Create(formerBestLabelset.Key, laterBestLabelset.Key);
                addend = addOne ? 1 : smallerValue(formerBestLabelset.Value, laterBestLabelset.Value);
                if (labelPairFrequencyForCharacter.ContainsKey(characterPair))
                {
                    if (labelPairFrequencyForCharacter[characterPair].ContainsKey(Labelset))
                        labelPairFrequencyForCharacter[characterPair][Labelset] += addend;
                    else
                        labelPairFrequencyForCharacter[characterPair].Add(Labelset, addend);
                }
                else
                {
                    labelPairFrequencyForCharacter.Add(characterPair, new Dictionary<Tuple<Labelset, Labelset>, double>());
                    labelPairFrequencyForCharacter[characterPair].Add(Labelset, addend);
                }
            }
            return labelPairFrequencyForCharacter;
        }

        static public void UpdateLabelsetPairFrequencyForSentence(bool addOne, Sij sij, ref IDictionary<Tuple<Labelset, Labelset>, double> LabelsetFrequency)//Update需要用ref
        {
            //开头
            KeyValuePair<Labelset, double> laterBestLabelset = sij.SortLabelsets(Variable.Sentences[0])[0];
            Tuple<Labelset, Labelset> LabelsetPair = Tuple.Create(new Labelset(true), laterBestLabelset.Key);
            double addend = addOne ? 1 : laterBestLabelset.Value;
            if (LabelsetFrequency.ContainsKey(LabelsetPair))
                LabelsetFrequency[LabelsetPair] += addend;
            else
                LabelsetFrequency.Add(LabelsetPair, addend);

            //中间
            KeyValuePair<Labelset, double> formerBestLabelset;
            for (int i = 1; i < Variable.Sentences.Count; ++i)
            {
                formerBestLabelset = sij.SortLabelsets(Variable.Sentences[i - 1])[0];
                laterBestLabelset = sij.SortLabelsets(Variable.Sentences[i])[0];
                LabelsetPair = Tuple.Create(formerBestLabelset.Key, laterBestLabelset.Key);
                addend = addOne ? 1 : smallerValue(formerBestLabelset.Value, laterBestLabelset.Value);
                if (LabelsetFrequency.ContainsKey(LabelsetPair))
                    LabelsetFrequency[LabelsetPair] += addend;
                else
                    LabelsetFrequency.Add(LabelsetPair, addend);
            }
            //计算次数（因为多算了，要除掉）
            foreach (Tuple<Labelset, Labelset> i in LabelsetFrequency.Keys.ToArray())
            {
                LabelsetFrequency[i] /= 2;
            }
        }

        static public void UpdateLabelsetPairFrequencyForCharacter(bool addOne, Sij sij, ref IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> LabelsetFrequency)
        {
            //开头
            Tuple<Character, Character> characterPair = Tuple.Create(new Character("##"), Variable.Sentences.First().Character);
            KeyValuePair<Labelset, double> laterBestLabelset = sij.SortLabelsets(Variable.Sentences[0])[0];
            Tuple<Labelset, Labelset> LabelsetPair = Tuple.Create(new Labelset(true), laterBestLabelset.Key);
            double addend = addOne ? 1 : laterBestLabelset.Value;
            if (LabelsetFrequency[characterPair].ContainsKey(LabelsetPair))
                LabelsetFrequency[characterPair][LabelsetPair] += addend;
            else
                LabelsetFrequency[characterPair].Add(LabelsetPair, addend);
            //中间
            KeyValuePair<Labelset, double> formerBestLabelset;
            for (int i = 1; i < Variable.Sentences.Count; ++i)
            {
                characterPair = Tuple.Create(Variable.Sentences[i - 1].Character, Variable.Sentences[i].Character);//角色（换另一个角色，不换角色，都算）
                formerBestLabelset = sij.SortLabelsets(Variable.Sentences[i - 1])[0];
                laterBestLabelset = sij.SortLabelsets(Variable.Sentences[i])[0];
                LabelsetPair = Tuple.Create(formerBestLabelset.Key, laterBestLabelset.Key);
                addend = addOne ? 1 : smallerValue(formerBestLabelset.Value, laterBestLabelset.Value);
                if (LabelsetFrequency[characterPair].ContainsKey(LabelsetPair))
                    LabelsetFrequency[characterPair][LabelsetPair] += addend;
                else
                    LabelsetFrequency[characterPair].Add(LabelsetPair, addend);
            }

            //计算次数（因为多算了，要除掉，也就是说全故事只算一次）
            foreach (Tuple<Character, Character> cp in LabelsetFrequency.Keys)
            {
                foreach (Tuple<Labelset, Labelset> i in LabelsetFrequency[cp].Keys.ToArray())
                {
                    LabelsetFrequency[cp][i] /= 2;
                }
            }
        }

        static public IDictionary<Tuple<Labelset, Labelset>, double> AllLabelsetPairFrequencyForSentence(Sij sij)//将所有标注综合到一次从头到尾的标注中（太慢，废弃）
        {
            IDictionary<Tuple<Labelset, Labelset>, double> labelPairFrequencyForSentence = new Dictionary<Tuple<Labelset, Labelset>, double>();
            Tuple<Labelset, Labelset> LabelsetPair;
            {
                foreach (Labelset j2 in sij.Value[Variable.Sentences[0]].Keys)//开头
                {
                    LabelsetPair = Tuple.Create(new Labelset(true), j2);
                    if (labelPairFrequencyForSentence.ContainsKey(LabelsetPair))//句
                        labelPairFrequencyForSentence[LabelsetPair] += sij.Value[Variable.Sentences[0]][j2] * j2.NumberOfTypes;
                    else
                        labelPairFrequencyForSentence.Add(LabelsetPair, sij.Value[Variable.Sentences[0]][j2] * j2.NumberOfTypes);
                }
            }
            //中间（站在当前往前看）
            for (int i = 1; i < Variable.Sentences.Count; ++i)
            {
                foreach (Labelset j1 in sij.Value[Variable.Sentences[i - 1]].Keys)
                {
                    foreach (Labelset j2 in sij.Value[Variable.Sentences[i]].Keys)
                    {
                        LabelsetPair = Tuple.Create(j1, j2);
                        double value = smallerValue(sij.Value[Variable.Sentences[i - 1]][j1], sij.Value[Variable.Sentences[i]][j2]);
                        if (labelPairFrequencyForSentence.ContainsKey(LabelsetPair))
                            labelPairFrequencyForSentence[LabelsetPair] += value;
                        else
                            labelPairFrequencyForSentence.Add(LabelsetPair, value);
                    }
                }
            }
            //计算次数（因为多算了，要除掉）
            foreach (Tuple<Labelset, Labelset> sp in labelPairFrequencyForSentence.Keys.ToArray())
            {
                labelPairFrequencyForSentence[sp] /= sp.Item1.NumberOfTypes;
            }
            return labelPairFrequencyForSentence;
        }

        static public IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> AllLabelsetPairFrequencyForCharacter(Sij sij)
        {
            IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> labelPairFrequencyForCharacter = new Dictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>>();
            //开头
            Tuple<Character, Character> characterPair = Tuple.Create(new Character("##"), Variable.Sentences.First().Character);
            labelPairFrequencyForCharacter.Add(characterPair, new Dictionary<Tuple<Labelset, Labelset>, double>());
            foreach (Labelset j2 in sij.Value[Variable.Sentences[0]].Keys)
            {
                Tuple<Labelset, Labelset> LabelsetPair = Tuple.Create(new Labelset(true), j2);
                if (labelPairFrequencyForCharacter[characterPair].ContainsKey(LabelsetPair))//角色
                    labelPairFrequencyForCharacter[characterPair][LabelsetPair] += sij.Value[Variable.Sentences[0]][j2] * j2.NumberOfTypes;
                else
                    labelPairFrequencyForCharacter[characterPair].Add(LabelsetPair, sij.Value[Variable.Sentences[0]][j2] * j2.NumberOfTypes);
            }
            //中间（站在当前往前看）
            for (int i = 1; i < Variable.Sentences.Count; ++i)
            {
                characterPair = Tuple.Create(Variable.Sentences[i - 1].Character, Variable.Sentences[i].Character);//角色（换另一个角色，不换角色，都算）
                foreach (Labelset j1 in sij.Value[Variable.Sentences[i - 1]].Keys)
                {
                    foreach (Labelset j2 in sij.Value[Variable.Sentences[i]].Keys)
                    {
                        Tuple<Labelset, Labelset> Labelset = Tuple.Create(j1, j2);
                        double value = smallerValue(sij.Value[Variable.Sentences[i - 1]][j1], sij.Value[Variable.Sentences[i]][j2]);
                        if (labelPairFrequencyForCharacter.ContainsKey(characterPair))
                        {
                            if (labelPairFrequencyForCharacter[characterPair].ContainsKey(Labelset))
                                labelPairFrequencyForCharacter[characterPair][Labelset] += value;
                            else
                                labelPairFrequencyForCharacter[characterPair].Add(Labelset, value);
                        }
                        else
                        {
                            labelPairFrequencyForCharacter.Add(characterPair, new Dictionary<Tuple<Labelset, Labelset>, double>());
                            labelPairFrequencyForCharacter[characterPair].Add(Labelset, value);
                        }
                    }
                }
            }
            //计算次数（因为多算了，要除掉，也就是说全故事只算一次）
            foreach (Tuple<Character, Character> cp in labelPairFrequencyForCharacter.Keys)
            {
                foreach (Tuple<Labelset, Labelset> sp in labelPairFrequencyForCharacter[cp].Keys)
                {
                    labelPairFrequencyForCharacter[cp][sp] /= sp.Item1.NumberOfTypes;
                }
            }
            return labelPairFrequencyForCharacter;
        }

        static public IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>> RenewIndependentLabelsetPairFrequencyForSentence(bool addOne, Label[] labels, Sij sij)
        {
            //初始化
            IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>> independentLabelsetPairFrequencyForSentence = new Dictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>();
            foreach (Label label in labels)
            {
                independentLabelsetPairFrequencyForSentence.Add(label, new Dictionary<Tuple<Labelset, Labelset>, double>());
            }
            //开头
            KeyValuePair<Labelset, double> laterBestLabelset = sij.SortLabelsets(Variable.Sentences[0])[0];//开头
            double addend = addOne ? 1 : laterBestLabelset.Value;
            foreach (Label label in labels)
            {
                Tuple<Labelset, Labelset> Labelset = new Tuple<Labelset, Labelset>(new Labelset(true), laterBestLabelset.Key.ToSingleLabelAnnotation(label));
                if (independentLabelsetPairFrequencyForSentence[label].ContainsKey(Labelset))
                    independentLabelsetPairFrequencyForSentence[label][Labelset] += addend;
                else
                    independentLabelsetPairFrequencyForSentence[label].Add(Labelset, addend);
            }
            //中间
            KeyValuePair<Labelset, double> formerBestLabelset;
            for (int i = 1; i < Variable.Sentences.Count; ++i)
            {
                formerBestLabelset = sij.SortLabelsets(Variable.Sentences[i - 1])[0];
                laterBestLabelset = sij.SortLabelsets(Variable.Sentences[i])[0];
                addend = addOne ? 1 : smallerValue(formerBestLabelset.Value, laterBestLabelset.Value);
                foreach (Label label in labels)
                {
                    Tuple<Labelset, Labelset> Labelset = Tuple.Create(formerBestLabelset.Key.ToSingleLabelAnnotation(label), laterBestLabelset.Key.ToSingleLabelAnnotation(label));
                    if (independentLabelsetPairFrequencyForSentence[label].ContainsKey(Labelset))
                        independentLabelsetPairFrequencyForSentence[label][Labelset] += addend;
                    else
                        independentLabelsetPairFrequencyForSentence[label].Add(Labelset, addend);
                }
            }
            return independentLabelsetPairFrequencyForSentence;
        }

        static public IDictionary<Tuple<Character, Character>, IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>> RenewIndependentLabelPairFreuquencyForCharacter(bool addOne, Label[] labels, Sij sij)
        {
            //初始化
            IDictionary<Tuple<Character, Character>, IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>> independentLabelPairFreuquencyForCharacter = new Dictionary<Tuple<Character, Character>, IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>>();
            Tuple<Character, Character> characterPair = new Tuple<Character, Character>(new Character("##"), Variable.Sentences.First().Character);
            independentLabelPairFreuquencyForCharacter.Add(characterPair, new Dictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>());
            foreach (Label label in labels)
            {
                independentLabelPairFreuquencyForCharacter[characterPair].Add(label, new Dictionary<Tuple<Labelset, Labelset>, double>());
            }

            //开头
            KeyValuePair<Labelset, double> laterBestLabelset = sij.SortLabelsets(Variable.Sentences[0])[0];
            double addend = addOne ? 1 : laterBestLabelset.Value;
            foreach (Label label in labels)
            {
                Tuple<Labelset, Labelset> Labelset = Tuple.Create(new Labelset(true), laterBestLabelset.Key.ToSingleLabelAnnotation(label));
                if (independentLabelPairFreuquencyForCharacter[characterPair][label].ContainsKey(Labelset))
                    independentLabelPairFreuquencyForCharacter[characterPair][label][Labelset] += addend;
                else
                    independentLabelPairFreuquencyForCharacter[characterPair][label].Add(Labelset, addend);
            }

            //中间
            KeyValuePair<Labelset, double> formerBestLabelset;
            for (int i = 1; i < Variable.Sentences.Count; ++i)
            {
                characterPair = Tuple.Create(Variable.Sentences[i - 1].Character, Variable.Sentences[i].Character);//角色（换另一个角色，不换角色，都算）
                formerBestLabelset = sij.SortLabelsets(Variable.Sentences[i - 1])[0];
                laterBestLabelset = sij.SortLabelsets(Variable.Sentences[i])[0];
                addend = addOne ? 1 : smallerValue(formerBestLabelset.Value, laterBestLabelset.Value);
                foreach (Label label in labels)
                {
                    Tuple<Labelset, Labelset> Labelset = Tuple.Create(formerBestLabelset.Key.ToSingleLabelAnnotation(label), laterBestLabelset.Key.ToSingleLabelAnnotation(label));
                    if (independentLabelPairFreuquencyForCharacter.ContainsKey(characterPair))
                    {
                        if (independentLabelPairFreuquencyForCharacter[characterPair][label].ContainsKey(Labelset))
                            independentLabelPairFreuquencyForCharacter[characterPair][label][Labelset] += addend;
                        else
                            independentLabelPairFreuquencyForCharacter[characterPair][label].Add(Labelset, addend);
                    }
                    else
                    {
                        independentLabelPairFreuquencyForCharacter.Add(characterPair, new Dictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>());
                        foreach (Label l in labels)
                        {
                            independentLabelPairFreuquencyForCharacter[characterPair].Add(l, new Dictionary<Tuple<Labelset, Labelset>, double>());
                            independentLabelPairFreuquencyForCharacter[characterPair][l] = new Dictionary<Tuple<Labelset, Labelset>, double>();
                        }
                        independentLabelPairFreuquencyForCharacter[characterPair][label].Add(Labelset, addend);
                    }
                }
            }
            return independentLabelPairFreuquencyForCharacter;
        }

        //计算Pj的后验概率
        static public IDictionary<Tuple<Labelset, Labelset>, double> CalculateConditionalPj(Pj pj, IDictionary<Tuple<Labelset, Labelset>, double> LabelsetFrequency)
        {
            return calculateConditional(LabelsetFrequency, pj.Value, Variable.Sentences.Count);
        }

        //计算Mcj的后验概率
        static public IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> CalculateConditionalMcj(Mcj mcj, IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> LabelsetFrequencyForCharacter)
        {
            IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> conditionalMcj = new Dictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>>();
            foreach (Tuple<Character, Character> characterPair in LabelsetFrequencyForCharacter.Keys)
            {
                if (characterPair.Item1.ID != "##")
                    conditionalMcj.Add(characterPair, calculateConditional(LabelsetFrequencyForCharacter[characterPair], mcj.Value[characterPair.Item1], characterPair.Item1.Sentences.Count));
                else
                    conditionalMcj.Add(characterPair, LabelsetFrequencyForCharacter[characterPair]);
            }
            return conditionalMcj;
        }

        static private IDictionary<Tuple<Labelset, Labelset>, double> calculateConditional(IDictionary<Tuple<Labelset, Labelset>, double> LabelsetPairFrequency, IDictionary<Labelset, double> pribability, int count)
        {
            foreach (Tuple<Labelset, Labelset> LabelsetPair in LabelsetPairFrequency.Keys.ToArray())//因为参数中LabelsetPair是frequency，而labelset是probability，所以要在分母乘以count（相当于整体除以count）
            {
                if (!LabelsetPair.Item1.IsBeginning)//不是开头
                    LabelsetPairFrequency[LabelsetPair] /= (pribability[LabelsetPair.Item1] * count);
            }
            //排序观察
            //List<KeyValuePair<Tuple<Labelset, Labelset>, double>> sortedConditionalProbability = new List<KeyValuePair<Tuple<Labelset, Labelset>, double>>(LabelsetPairFrequency);
            //sortedConditionalProbability.Sort(delegate(KeyValuePair<Tuple<Labelset, Labelset>, double> s1, KeyValuePair<Tuple<Labelset, Labelset>, double> s2)
            //{
            //    return s2.Value.CompareTo(s1.Value);
            //});
            return LabelsetPairFrequency;//结尾忽略了，否则相同item1相加应该等于1
        }

        static public IDictionary<Tuple<Labelset, Labelset>, double> CalculateIndependentConditionalPj(Pj pj, IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>> independentLabelsetPairFrequency)
        {
            return calculateIndependentConditional(independentLabelsetPairFrequency, pj.Value, Variable.Sentences.Count); ;
        }

        static public IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> CalculateIndependentConditionalMcj(Mcj mcj, IDictionary<Tuple<Character, Character>, IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>> independentLabelsetPairFrequencyForCharacter)
        {
            IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> independentConditionalMcj = new Dictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>>();
            foreach (Tuple<Character, Character> characterPair in independentLabelsetPairFrequencyForCharacter.Keys)
            {
                if (characterPair.Item1.ID != "##")
                    independentConditionalMcj.Add(characterPair, calculateIndependentConditional(independentLabelsetPairFrequencyForCharacter[characterPair], mcj.Value[characterPair.Item1], characterPair.Item1.Sentences.Count));
                else
                    independentConditionalMcj.Add(characterPair, calculateIndependentConditional(independentLabelsetPairFrequencyForCharacter[characterPair], new Dictionary<Labelset, double>(), 0));
            }
            return independentConditionalMcj;
        }

        static private IDictionary<Tuple<Labelset, Labelset>, double> calculateIndependentConditional(IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>> independentLabelsetPairFrequency, IDictionary<Labelset, double> probablity, int count)
        {
            //初始化
            IDictionary<Label, IDictionary<bool, double>> labelFrequency = new Dictionary<Label, IDictionary<bool, double>>();
            if (count != 0)//不是character的开头
            {
                foreach (Label label in independentLabelsetPairFrequency.Keys)//初始化
                {
                    labelFrequency.Add(label, new Dictionary<bool, double>());
                    labelFrequency[label].Add(true, 0);
                    labelFrequency[label].Add(false, 0);
                }
                //计算每个label独立的pj
                foreach (Labelset Labelset in probablity.Keys)
                {
                    foreach (Label label in independentLabelsetPairFrequency.Keys)
                    {
                        labelFrequency[label][Labelset.Labels[label]] += probablity[Labelset];
                    }
                }
            }

            //计算独立的概率
            IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>> independentConditional = new Dictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>();
            foreach (Label label in independentLabelsetPairFrequency.Keys)
            {
                independentConditional.Add(label, new Dictionary<Tuple<Labelset, Labelset>, double>());
                foreach (Tuple<Labelset, Labelset> LabelsetPair in independentLabelsetPairFrequency[label].Keys)
                {
                    if (!LabelsetPair.Item1.IsBeginning)//不是开头)
                        independentConditional[label].Add(LabelsetPair, independentLabelsetPairFrequency[label][LabelsetPair] / (labelFrequency[label][LabelsetPair.Item1.Labels[label]] * count));
                    else
                        independentConditional[label].Add(LabelsetPair, independentLabelsetPairFrequency[label][LabelsetPair]);
                }
            }

            //整合成联合概率
            IDictionary<Tuple<Labelset, Labelset>, double> conditional = new Dictionary<Tuple<Labelset, Labelset>, double>();
            //开头
            foreach (Labelset j2 in probablity.Keys)
            {
                double value = 1;
                foreach (Label label in independentLabelsetPairFrequency.Keys)
                {
                    Tuple<Labelset, Labelset> independentDecimalPair = Tuple.Create(new Labelset(true), j2.ToSingleLabelAnnotation(label));
                    if (independentConditional[label].ContainsKey(independentDecimalPair))
                        value *= independentConditional[label][independentDecimalPair];
                    else
                    {
                        value = 0;
                        break;
                    }
                }
                if (value != 0)
                    conditional.Add(Tuple.Create(new Labelset(true), j2), value);
            }
            //开头中间
            foreach (Labelset j1 in probablity.Keys)
            {
                foreach (Labelset j2 in probablity.Keys)
                {
                    double value = 1;
                    foreach (Label label in independentLabelsetPairFrequency.Keys)
                    {
                        Tuple<Labelset, Labelset> independentLabelsetlPair = Tuple.Create(j1.ToSingleLabelAnnotation(label), j2.ToSingleLabelAnnotation(label));
                        if (independentConditional[label].ContainsKey(independentLabelsetlPair))
                            value *= independentConditional[label][independentLabelsetlPair];
                        else
                        {
                            value = 0;
                            break;
                        }
                    }
                    if (value != 0)
                        conditional.Add(Tuple.Create(j1, j2), value);
                }
            }
            return conditional;
        }
    }
}