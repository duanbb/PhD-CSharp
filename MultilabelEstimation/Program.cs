using MultilabelEstimation.Algorithm.Personality;
using MultilabelEstimation.Consistency;
using MultilabelEstimation.Group;
using MultilabelEstimation.Supervised;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MultilabelEstimation.Algorithm.DDS.NDDS;

namespace MultilabelEstimation
{
    static class Program
    {
        static void Main(string[] args)
        {
            #region 初始化
            Story CurrentStory = Story.Masatyan;
            //int[] groupings = new int[] { 30 };
            int[] groupsizes = new int[] { 3, 5, 10, 15, 30 };
            SimilaritySelector[] SimilaritySelectors = new SimilaritySelector[] { SimilaritySelector.Same, SimilaritySelector.Compare, SimilaritySelector.BinaryResultAndNumericGold };
            Variable.GoldType = GoldType.Joint;
            bool BeepOrTextWhenFinished = false;

            Variable.Relation = RelationScheme.RenewLower;//与初始化无关，此处控制的是第一次以后的迭代（理论上最合理：RenewLower）
            Variable.PjDividSentenceCount = true;
            Variable.SijDividPDataOnI = true;
            Variable.OutputPdata = false;
            Variable.OutputResult = true;
            //Multi-emotion Estimation Model Considering Emotion \textit{Consistency}
            //Variable.PriorP = new PriorP[] { PriorP.Pj, PriorP.Mcj, PriorP.Sij };//同一个属性，conditional和非conditional不能同时出现
            //Multi-emotion Estimation Model Considering Emotion \textit{Consistency} and \textit{Contextual Relationships}
            Variable.PriorP = new PriorP[] { PriorP.ConditionalPj, PriorP.ConditionalMcj, PriorP.ConditionalSij };//P:alpha, M:beta, S:gamma
            Algorithm.DDS.NDDS.NDDSVariable.SmoothBN = Smoothing.None;
            Variable.SmoothPajl = Smoothing.Laplace;//要平滑(对Personality来说，Laplace更好)
            Variable.ConvergeValueThreshold = 0;
            Variable.ConvergeTimeThreshold = 10000;//Boku1次最好，Masa2次最好
            SupervisedVariable.NumberOfTraningSentences = 0;

            #region Personality
            PersonalityVariable.TransGoldStandard = TransGoldStandard.No;
            PersonalityVariable.ExchangeLabel = ExchangeLabel.No;
            #endregion

            InitializeData(CurrentStory);
            #endregion

            #region GoldStandard
            switch (CurrentStory)
            {
                case Story.Bokutati:
                case Story.BokutatiSample:
                    AnnotatorSelectionForBoku(30, Selector.Most);//筛选人，Variable.Data不变，只变Variable.Annotators
                    break;
                case Story.Masatyan:
                    AnnotationSelectionForMasa(30, Selector.Most);
                    break;
                case Story.TwoStories:
                    ConsistencyPaperFunction.LabelpairFrequency();
                    return;//两个故事时只为统计数据，不计算
            }
            DescendLabelsByNumber();//label降序排列，生成Variable.LabelArray
            NumericGold();//各label被标注的概率作为GS
            MajorityVoteGold("Reminded");//被标最多次的多选情况作为GS
            PersonalityPaperFunction.CevioGold();

            if (PersonalityVariable.TransGoldStandard == TransGoldStandard.Yes)
            {
                PersonalityVariable.TruePersonality = PersonalityFunction.GetGoldOfPersonality();
                PersonalityFunction.TransGoldstandardsForPersonality();
                //PersonalityPaperFunction.TransGoldstandardsToCevio();
            }
            #endregion

            #region 统计
            //PaperFunction.CalculateWorkers();//论文用，计算两篇故事共有多少人标过
            //PaperFunction.NumberOfEachLabel();//论文用，统计每个label频率
            PaperFunction.AverageTrueLabelsPerAnnotation();//论文用，计算平均每次被标了几个label，有输出
            //ConsistencyPaperFunction.CalcuateCharacterConsistency();
            //ConsistencyPaperFunction.GroupLabels();
            #endregion

            #region Algorithm
            foreach (int groupsize in groupsizes)
            {
                Grouping(GroupingMethod.Seperate, groupsize);
                IList<Thread> threads = new List<Thread>();
                Thread MVThread = new Thread(new ThreadStart(delegate() { Algorithm.MV.MVFunction.RunMV(GoldType.Joint); })); MVThread.Start(); threads.Add(MVThread); MVThread.Name = "MV";
                Thread IDSThread = new Thread(new ThreadStart(Algorithm.IDS.IDSFunction.RunIDS)); IDSThread.Start(); threads.Add(IDSThread); IDSThread.Name = "IDS";
                Thread PDSThread = new Thread(new ThreadStart(Algorithm.PDS.PDSFunction.RunPDS)); PDSThread.Start(); threads.Add(PDSThread); PDSThread.Name = "PDS";
                Thread JDDSThread = new Thread(new ThreadStart(Algorithm.DDS.JDDS.JDDSFunction.RunJDDS)); JDDSThread.Start(); threads.Add(JDDSThread); JDDSThread.Name = "JDDS";
                Thread IDDSThread = new Thread(new ThreadStart(Algorithm.DDS.IDDS.IDDSFunction.RunIDDS)); IDDSThread.Start(); threads.Add(IDDSThread); IDDSThread.Name = "IDDS";
                Thread NDDSThread = new Thread(delegate() { Algorithm.DDS.NDDS.NDDSFunction.RunNDDS(Math.Pow(10, -1), IndependenceEstimation.MutualInformation); }); NDDSThread.Start(); threads.Add(NDDSThread); NDDSThread.Name = "NDDS";
                Thread PeTMThread = new Thread(delegate() { Algorithm.Personality.PeTM.PeTMFunction.RunPeTM(PorSForJointje.S, Smoothing.Laplace, BnOrNot.No); }); PeTMThread.Start(); threads.Add(PeTMThread); PeTMThread.Name = "PeTM";
                Thread PeTThread = new Thread(delegate() { Algorithm.Personality.PeT.PeTFunction.RunPeT(PorSForJointje.S, Smoothing.Laplace, BnOrNot.No); }); PeTThread.Start(); threads.Add(PeTThread); PeTThread.Name = "PeT";
                foreach (Thread t in threads)
                    t.Join();
                ObtainAccuracy(threads, SimilaritySelectors);//最后统一计算准确率
            }
            Variable.OutputFile.WriteLine(Variable.ConsoleOutput);
            Variable.OutputFile.Close();
            if (BeepOrTextWhenFinished) Console.Beep(440, 10000); else Console.WriteLine("Press any key to exit..."); Console.Read();
            #endregion
        }

        static private void InitializeData(Story currentStory)
        {
            foreach (string file in Directory.GetFiles("Result/."))//必须放在初始化静态类Variable之前
            {
                File.Delete(file);
            }
            string[] speeches;
            string storyName = string.Empty;
            if (currentStory != Story.SnowFestival)
            {
                Variable.LabelArray = new Label[10] { Label.anger, Label.relief, Label.happiness, Label.sadness, Label.excitement, Label.disgust, Label.surprise, Label.fondness, Label.fear, Label.shame };
                Variable.LabelToString.Add(Label.happiness, "happiness");
                Variable.LabelToString.Add(Label.fondness, "fondness");
                Variable.LabelToString.Add(Label.relief, "relief");
                Variable.LabelToString.Add(Label.anger, "anger");
                Variable.LabelToString.Add(Label.sadness, "sadness");
                Variable.LabelToString.Add(Label.fear, "fear");
                Variable.LabelToString.Add(Label.shame, "shame");
                Variable.LabelToString.Add(Label.disgust, "disgust");
                Variable.LabelToString.Add(Label.excitement, "excitement");
                Variable.LabelToString.Add(Label.surprise, "surprise");

                switch (currentStory)
                {
                    case Story.Masatyan:
                        Variable.NumberOfAnnotationsPerSentence = 40;
                        speeches = File.ReadAllLines("政ちゃんと赤いりんご/sentences.txt");
                        initializeCharacterAndSentence(speeches);
                        initializeAnnotation(0, 24, Story.Masatyan);
                        initializeAnnotation(25, 51, Story.Masatyan);
                        initializeAnnotation(52, 77, Story.Masatyan);
                        storyName = "政ちゃん";
                        break;
                    case Story.Bokutati:
                        Variable.NumberOfAnnotationsPerSentence = 41;
                        speeches = File.ReadAllLines("僕たちは愛するけれど/sentences.txt");
                        initializeCharacterAndSentence(speeches);
                        initializeAnnotation(0, Variable.Sentences.Count - 1, Story.Bokutati);
                        storyName = "僕たち";
                        MajorityVoteGold("All");//被标最多次的多选情况作为GS，写在这里为了筛选数据(Best, Worst)
                        break;
                    case Story.BokutatiSample:
                        Variable.NumberOfAnnotationsPerSentence = 41;
                        speeches = File.ReadAllLines("僕たちは愛するけれど/sentences-sample.txt");
                        initializeCharacterAndSentence(speeches);
                        initializeAnnotation(0, Variable.Sentences.Count - 1, Story.Bokutati);
                        storyName = "僕たち";
                        MajorityVoteGold("All");//被标最多次的多选情况作为GS，写在这里为了筛选数据(Best, Worst)
                        break;
                    case Story.TwoStories:
                        Variable.NumberOfAnnotationsPerSentence = 40;
                        speeches = File.ReadAllLines("政ちゃんと赤いりんご/sentences.txt");
                        initializeCharacterAndSentence(speeches);
                        initializeAnnotation(0, 24, Story.Masatyan);
                        initializeAnnotation(25, 51, Story.Masatyan);
                        initializeAnnotation(52, 77, Story.Masatyan);
                        Variable.NumberOfAnnotationsPerSentence = 41;
                        speeches = File.ReadAllLines("僕たちは愛するけれど/sentences.txt");
                        initializeCharacterAndSentence(speeches);
                        initializeAnnotation(0, 62, Story.Bokutati);
                        break;
                }
            }
            else
            {
                List<PriorP> priors = Variable.PriorP.ToList();
                if (priors.Remove(PriorP.Mcj) || priors.Remove(PriorP.ConditionalPj) || priors.Remove(PriorP.ConditionalMcj))
                    Variable.PriorP = priors.ToArray();

                Variable.NumberOfAnnotationsPerSentence = 5;
                Variable.LabelArray = new Label[4] { Label.WantToGo, Label.ConfirmToGo, Label.HaveBeenThereBefore, Label.NoIntention };
                Variable.LabelToString.Add(Label.WantToGo, "want to go");
                Variable.LabelToString.Add(Label.ConfirmToGo, "confirm to go");
                Variable.LabelToString.Add(Label.HaveBeenThereBefore, "have been there before");
                Variable.LabelToString.Add(Label.NoIntention, "no intention");
                //Variable.LabelToString.Add(Label.cannot, "行きたがっている");
                //Variable.LabelToString.Add(Label.plan, "行く予定がある");
                //Variable.LabelToString.Add(Label.done, "行ったことがある");
                //Variable.LabelToString.Add(Label.notwant, "行く気はない");

                speeches = File.ReadAllLines("雪祭り1/sentences.txt");
                for (int i = 0; i < speeches.Length; ++i)
                {
                    Variable.Sentences.Add(new Sentence(i, speeches[i]));
                }
                initializeAnnotation(0, speeches.Length - 1, currentStory);
                storyName = "雪祭り1";
            }
            Variable.OutputFile = new StreamWriter("Result/" + storyName + ".txt");
        }

        static private void initializeCharacterAndSentence(string[] speeches)
        {
            for (int i = 0; i < speeches.Length; ++i)
            {
                string[] characterAndSentence = speeches[i].Split('：');
                Character character = ConsistencyVariable.Characters.Find(delegate(Character c) { return c.ID.Equals(characterAndSentence[0]); });
                if (character == null)
                {
                    character = new Character(characterAndSentence[0]);
                    ConsistencyVariable.Characters.Add(character);
                }
                Sentence sentence = new Sentence(i, characterAndSentence[1], character);
                character.Sentences.Add(sentence);
                Variable.Sentences.Add(sentence);
            }
        }

        static private void initializeAnnotation(int startIndex, int endIndex, Story currentStory)
        {
            string[] data = null;
            switch (currentStory)
            {
                case Story.Masatyan:
                    data = File.ReadAllLines("政ちゃんと赤いりんご/" + Variable.NumberOfAnnotationsPerSentence + "人数据/data" + startIndex + "-" + endIndex + ".csv");
                    break;
                case Story.Bokutati:
                    data = File.ReadAllLines("僕たちは愛するけれど/data.csv");
                    break;
                case Story.BokutatiSample:
                    data = File.ReadAllLines("僕たちは愛するけれど/data-sample.csv");
                    break;
                case Story.SnowFestival:
                    data = File.ReadAllLines("雪祭り1/data.csv");
                    break;
            }

            foreach (string datum in data)
            {
                string[] labels = datum.Split(',');//labels[0]是用户名
                Annotator annotator = new Annotator(labels[0]);
                if (!Variable.Annotators.Contains(annotator))//重复的人不再添加
                {
                    Variable.Annotators.Add(annotator);
                    Variable.Data.Add(annotator, new Dictionary<Sentence, List<Annotation>>());
                }
                Annotation annotation = new Annotation();
                if (currentStory != Story.SnowFestival)
                {
                    for (int i = 1; i <= (endIndex - startIndex + 1) * (Variable.LabelArray.Length + 1); ++i)
                    {
                        switch (labels[i])
                        {
                            case "happiness":
                                annotation.Labels[Label.happiness] = true;
                                break;
                            case "fondness":
                                annotation.Labels[Label.fondness] = true;
                                break;
                            case "relief":
                                annotation.Labels[Label.relief] = true;
                                break;
                            case "anger":
                                annotation.Labels[Label.anger] = true;
                                break;
                            case "sadness":
                                annotation.Labels[Label.sadness] = true;
                                break;
                            case "fear":
                                annotation.Labels[Label.fear] = true;
                                break;
                            case "shame":
                                annotation.Labels[Label.shame] = true;
                                break;
                            case "disgust":
                                annotation.Labels[Label.disgust] = true;
                                break;
                            case "excitement":
                                annotation.Labels[Label.excitement] = true;
                                break;
                            case "surprise":
                                annotation.Labels[Label.surprise] = true;
                                break;
                        }
                        if (i % (Variable.LabelArray.Length + 1) == 0)
                        {
                            Sentence sentence = Variable.Sentences[startIndex + (i - 1) / (Variable.LabelArray.Length + 1)];
                            if (!Variable.Data[annotator].ContainsKey(sentence))
                                Variable.Data[annotator].Add(sentence, new List<Annotation>());
                            Variable.Data[annotator][sentence].Add(annotation);
                            annotation = new Annotation();
                        }
                    }
                }
                else
                {
                    for (int i = 1; i <= (endIndex - startIndex + 1) * Variable.LabelArray.Length; ++i)
                    {
                        if (labels[i] == "1")
                        {
                            switch (i % 4)
                            {
                                case 1:
                                    annotation.Labels[Label.WantToGo] = true;
                                    break;
                                case 2:
                                    annotation.Labels[Label.ConfirmToGo] = true;
                                    break;
                                case 3:
                                    annotation.Labels[Label.HaveBeenThereBefore] = true;
                                    break;
                                case 0:
                                    annotation.Labels[Label.NoIntention] = true;
                                    break;
                            }
                        }
                        if (i % Variable.LabelArray.Length == 0)
                        {
                            //多页面时（不同批人完成所有任务）：没有Mu的可以放在一个data.csv里（SnowFestival）,此时该人对该句没数据就表示没标；有Mu的必须分开放（Masa）
                            if (annotation.Mu != true)
                            {
                                Variable.Data[annotator][Variable.Sentences[startIndex + (i - 1) / Variable.LabelArray.Length]].Add(annotation);
                                annotation = new Annotation();
                            }
                        }
                    }
                }
            }
        }

        //得到numeric黄金标准 2.0 (独立依赖结果一样)
        static private void NumericGold()
        {
            foreach (Sentence sentence in Variable.Sentences)
            {
                sentence.NumericGold = new NumericResult();
                int NumberOfAnnotationsPreSentence = 0;
                foreach (Annotator annotator in Variable.Annotators)
                {
                    if (Variable.Data[annotator].ContainsKey(sentence))
                    {
                        foreach (Annotation annotation in Variable.Data[annotator][sentence])
                        {
                            ++NumberOfAnnotationsPreSentence;
                            foreach (Label label in Variable.LabelArray)
                            {
                                if (annotation.Labels[label])
                                {
                                    ++sentence.NumericGold.Labels[label];
                                }
                            }
                        }
                    }
                }
                foreach (Label label in Variable.LabelArray)
                {
                    sentence.NumericGold.Labels[label] /= NumberOfAnnotationsPreSentence;
                }
            }

            #region Output Numeric Gold
            StreamWriter resultFile = new StreamWriter("Result/NumericGoldStandard.csv");
            Function.InitialResultFile(resultFile);
            for (int i = 0; i < Variable.Sentences.Count; ++i)
            {
                Function.WriteNumericResultOfASentence(i, Variable.Sentences[i].NumericGold, resultFile);
                resultFile.WriteLine();
            }
            resultFile.Close();
            #endregion
        }

        //筛选句（会删除某些句，故不科学）
        static private void SentenceSelectionForBoku(int r, Selector selector)//Variable.Data不变，只变Variable.Sentences
        {
            if (r >= Variable.Sentences.Count)
            {
                Console.WriteLine("Wrong Sentence Number: " + r);
                return;
            }
            IDictionary<Sentence, double> parameter = new Dictionary<Sentence, double>();
            foreach (Sentence sentence in Variable.Sentences)
            {
                parameter.Add(sentence, 0);
                foreach (Annotator annotator in Variable.Annotators)
                {
                    foreach (Annotation annotation in Variable.Data[annotator][sentence])
                    {
                        switch (selector)
                        {
                            case Selector.Most:
                            case Selector.Least:
                                parameter[sentence] += annotation.NumberOfTrueLabel;
                                break;
                            case Selector.Best:
                            case Selector.Worst:
                                parameter[sentence] += SimilarityMeasure.Compare(annotation, sentence.BinaryGold);
                                break;
                        }
                    }
                }
            }
            List<KeyValuePair<Sentence, double>> sortedElements = new List<KeyValuePair<Sentence, double>>(parameter);
            switch (selector)
            {
                case Selector.Most:
                case Selector.Best:
                    sortedElements.Sort(delegate(KeyValuePair<Sentence, double> s1, KeyValuePair<Sentence, double> s2)
                    {
                        return s2.Value.CompareTo(s1.Value);
                    });
                    break;
                case Selector.Least:
                case Selector.Worst:
                    sortedElements.Sort(delegate(KeyValuePair<Sentence, double> s1, KeyValuePair<Sentence, double> s2)
                    {
                        return s1.Value.CompareTo(s2.Value);
                    });
                    break;
            }
            Variable.Sentences.Clear();
            for (int i = 0; i < r; ++i)
            {
                sortedElements[i].Key.ID = i;
                Variable.Sentences.Add(sortedElements[i].Key);
            }

        }

        static private void AnnotatorSelectionForBoku(int r, Selector selector)//Variable.Data不变，只变Variable.Annotators
        {
            if (r >= Variable.Data.Count)
            {
                Console.WriteLine("Wrong Annotator Number: " + r);
                return;
            }
            Variable.NumberOfAnnotationsPerSentence = r;
            IDictionary<Annotator, double> annotatorAndValue = new Dictionary<Annotator, double>();
            foreach (Annotator annotator in Variable.Annotators)
            {
                annotatorAndValue.Add(annotator, 0);
                foreach (Sentence sentence in Variable.Sentences)
                {
                    foreach (Annotation annotation in Variable.Data[annotator][sentence])
                    {
                        switch (selector)
                        {
                            case Selector.Most:
                            case Selector.Least:
                                annotatorAndValue[annotator] += annotation.NumberOfTrueLabel;
                                break;
                            case Selector.Best:
                            case Selector.Worst:
                                annotatorAndValue[annotator] += SimilarityMeasure.Compare(annotation, sentence.BinaryGold);
                                break;
                        }
                    }
                }
            }
            List<KeyValuePair<Annotator, double>> sortedElements = new List<KeyValuePair<Annotator, double>>(annotatorAndValue);
            switch (selector)
            {
                case Selector.Most:
                case Selector.Best:
                    sortedElements.Sort(delegate(KeyValuePair<Annotator, double> s1, KeyValuePair<Annotator, double> s2)
                    {
                        return s2.Value.CompareTo(s1.Value);
                    });
                    break;
                case Selector.Least:
                case Selector.Worst:
                    sortedElements.Sort(delegate(KeyValuePair<Annotator, double> s1, KeyValuePair<Annotator, double> s2)
                    {
                        return s1.Value.CompareTo(s2.Value);
                    });
                    break;
            }
            //for (int i = r; i < sortedElements.Count; ++i)
            //{
            //    Variable.Annotators.Remove(sortedElements[i].Key);
            //}
            Variable.Annotators.Clear();
            for (int i = 0; i < r; ++i)
            {
                Variable.Annotators.Add(sortedElements[i].Key);
            }
        }

        static private void AnnotationSelectionForMasa(int r, Selector selector)
        {
            Variable.NumberOfAnnotationsPerSentence = r;
            foreach (Sentence sentence in Variable.Sentences)
            {
                IDictionary<Annotator, double> annotator_value = new Dictionary<Annotator, double>();//因为强制让每人标同一句不超过一次，故对sentence来说，筛选annotation就相当于筛选annotator
                foreach (Annotator annotator in Variable.Annotators)
                {
                    if (Variable.Data[annotator].ContainsKey(sentence))
                    {
                        annotator_value.Add(annotator, 0);
                        foreach (Annotation annotation in Variable.Data[annotator][sentence])
                        {
                            switch (selector)
                            {
                                case Selector.Most:
                                case Selector.Least:
                                    annotator_value[annotator] += annotation.NumberOfTrueLabel;
                                    break;
                                case Selector.Best:
                                case Selector.Worst:
                                    annotator_value[annotator] += SimilarityMeasure.Compare(annotation, sentence.BinaryGold);
                                    break;
                            }
                        }
                    }
                }
                List<KeyValuePair<Annotator, double>> sortedElements = new List<KeyValuePair<Annotator, double>>(annotator_value);
                switch (selector)
                {
                    case Selector.Most:
                    case Selector.Best:
                        sortedElements.Sort(delegate(KeyValuePair<Annotator, double> s1, KeyValuePair<Annotator, double> s2)
                        {
                            return s2.Value.CompareTo(s1.Value);
                        });
                        break;
                    case Selector.Least:
                    case Selector.Worst:
                        sortedElements.Sort(delegate(KeyValuePair<Annotator, double> s1, KeyValuePair<Annotator, double> s2)
                        {
                            return s1.Value.CompareTo(s2.Value);
                        });
                        break;
                }
                for (int i = r; i < sortedElements.Count; ++i)
                {
                    Variable.Data[sortedElements[i].Key].Remove(sentence);//从人的数据中删除sentence,修改Variable.Data了（Boku没修改）
                }
                foreach (Annotator annotator in Variable.Annotators.ToArray())
                {
                    if (Variable.Data[annotator].Count == 0)
                    {
                        Variable.Data.Remove(annotator);
                        Variable.Annotators.Remove(annotator);
                    }
                }
            }
        }

        //各情感总数降序排列
        static private void DescendLabelsByNumber()
        {
            IDictionary<Label, int> numberOfEachLabel = new Dictionary<Label, int>();
            foreach (Label label in Variable.LabelArray)
            {
                numberOfEachLabel.Add(label, 0);
            }
            foreach (Annotator annotator in Variable.Annotators)
            {
                foreach (Sentence sentence in Variable.Sentences)
                {
                    if (Variable.Data[annotator].ContainsKey(sentence))
                    {
                        foreach (Annotation annotation in Variable.Data[annotator][sentence])
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
                }
            }
            List<KeyValuePair<Label, int>> sortedLabel = new List<KeyValuePair<Label, int>>(numberOfEachLabel);
            sortedLabel.Sort(delegate(KeyValuePair<Label, int> s1, KeyValuePair<Label, int> s2)
            {
                return s2.Value.CompareTo(s1.Value);
            });
            for (int a = 0; a < sortedLabel.Count; ++a)
            {
                Variable.LabelArray[a] = sortedLabel[a].Key;
            }
        }

        //得到依赖黄金标准（最多一项，Majority Vote）
        static private void MajorityVoteGold(string allOrReminded)
        {
            foreach (Sentence sentence in Variable.Sentences)
            {
                ICollection<Annotation> annotations = new List<Annotation>();
                foreach (Annotator annotator in Variable.Annotators)
                {
                    if (Variable.Data[annotator].ContainsKey(sentence))
                    {
                        foreach (Annotation annotation in Variable.Data[annotator][sentence])
                        {
                            annotations.Add(annotation);//取到当前句子的所有标注
                        }
                    }
                }
                sentence.BinaryGold = GoldstandardFunction.GetResult(annotations, Variable.GoldType);
            }
            Function.WriteGoldToFile(allOrReminded);
        }

        static private void Grouping(GroupingMethod groupingMethod, int groupsize)
        {
            Function.ConsoleWriteLine("Grouping:" + groupsize);
            switch (groupingMethod)
            {
                case GroupingMethod.Seperate:
                    //组数
                    GroupVariable.AnnotatorGroups = new IList<Annotator>[Variable.NumberOfAnnotationsPerSentence / groupsize];
                    for (int i = 0; i < GroupVariable.AnnotatorGroups.Length; ++i)
                    {
                        GroupVariable.AnnotatorGroups[i] = new List<Annotator>();
                    }
                    //每组人数
                    Variable.NumberOfAnnotationsPerSentenceAfterGrouping = groupsize;
                    foreach (Sentence sentence in Variable.Sentences)
                    {
                        //这个sentence在每组中的标注
                        sentence.AnnotaitonGroups = new AnnotationGroup[GroupVariable.AnnotatorGroups.Length];
                        for (int i = 0; i < GroupVariable.AnnotatorGroups.Length; ++i)
                        {
                            sentence.AnnotaitonGroups[i] = new AnnotationGroup();
                        }
                        int n = 0;
                        foreach (Annotator annotator in Variable.Annotators)
                        {
                            if (Variable.Data[annotator].ContainsKey(sentence))
                            {
                                foreach (Annotation annotation in Variable.Data[annotator][sentence])
                                {
                                    sentence.AnnotaitonGroups[n / groupsize].AnnotatorAnnotationDic.Add(annotator, annotation);//每人只有一个标注(手动修改标注文件，去掉一人同时标几句的情况)
                                    if (!GroupVariable.AnnotatorGroups[n / groupsize].Contains(annotator))
                                        GroupVariable.AnnotatorGroups[n / groupsize].Add(annotator);
                                    ++n;
                                }
                            }
                        }
                    }
                    break;
                case GroupingMethod.Overlap://太慢，废弃
                    int[] temp = new int[groupsize];
                    IList<Annotator[]> list = new List<Annotator[]>();
                    getCombination(ref list, Variable.Annotators, Variable.Annotators.Count, groupsize, temp, groupsize);//全匹配
                    GroupVariable.AnnotatorGroups = new IList<Annotator>[list.Count];
                    for (int i = 0; i < list.Count; ++i)
                    {
                        GroupVariable.AnnotatorGroups[i] = new List<Annotator>();
                        foreach (Annotator annotator in list[i])
                        {
                            GroupVariable.AnnotatorGroups[i].Add(annotator);
                        }
                    }
                    Variable.NumberOfAnnotationsPerSentenceAfterGrouping = groupsize;
                    foreach (Sentence sentence in Variable.Sentences)
                    {
                        sentence.AnnotaitonGroups = new AnnotationGroup[GroupVariable.AnnotatorGroups.Length];
                        for (int i = 0; i < GroupVariable.AnnotatorGroups.Length; ++i)
                        {
                            sentence.AnnotaitonGroups[i] = new AnnotationGroup();
                            foreach (Annotator annotator in GroupVariable.AnnotatorGroups[i])
                            {
                                foreach (Annotation annotation in Variable.Data[annotator][sentence])
                                {
                                    sentence.AnnotaitonGroups[i].AnnotatorAnnotationDic.Add(annotator, annotation);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        static private void getCombination(ref IList<Annotator[]> list, IList<Annotator> t, int n, int m, int[] b, int M)
        {
            for (int i = n; i >= m; i--)
            {
                b[m - 1] = i - 1;
                if (m > 1)
                {
                    getCombination(ref list, t, i - 1, m - 1, b, M);
                }
                else
                {
                    if (list == null)
                    {
                        list = new List<Annotator[]>();
                    }
                    Annotator[] temp = new Annotator[M];
                    for (int j = 0; j < b.Length; j++)
                    {
                        temp[j] = t[b[j]];
                    }
                    list.Add(temp);
                }
            }
        }

        static private void ObtainAccuracy(IList<Thread> threads, SimilaritySelector[] SimilaritySelectors)
        {
            foreach(Thread t in threads.ToArray())
            {
                if (t.Name == "PeTM" || t.Name == "PeT")
                {
                    Thread personalityMVThread = new Thread(new ThreadStart(delegate() {}));
                    personalityMVThread.Name = "PeMV";
                    threads.Add(personalityMVThread);//只为计算结果PersonalityMV的准确率
                    break;
                }
            }
            foreach (Thread t in threads)
            {
                IDictionary<SimilaritySelector, double> AverageAccuracy = new Dictionary<SimilaritySelector, double>();
                foreach (SimilaritySelector ss in SimilaritySelectors)
                {
                    AverageAccuracy.Add(ss, 0);
                }
                for (int groupIndex = 0; groupIndex < GroupVariable.AnnotatorGroups.Length; ++groupIndex)
                {
                    IDictionary<SimilaritySelector, double> GroupAccuracy = new Dictionary<SimilaritySelector, double>();
                    foreach (SimilaritySelector ss in SimilaritySelectors)
                    {
                        GroupAccuracy.Add(ss, 0);
                    }
                    foreach (Sentence sentence in Variable.Sentences)
                    {
                        Result result = sentence.AnnotaitonGroups[groupIndex].GetResultFromAlgorithmName(t.Name);
                        foreach (SimilaritySelector ss in SimilaritySelectors)
                        {
                            switch (ss)
                            {
                                case SimilaritySelector.Same:
                                    GroupAccuracy[ss] += Convert.ToDouble(result.Equals(sentence.BinaryGold));
                                    break;
                                case SimilaritySelector.Compare:
                                    GroupAccuracy[ss] += SimilarityMeasure.Compare(result, sentence.BinaryGold);
                                    break;
                                case SimilaritySelector.BinaryResultAndNumericGold:
                                    GroupAccuracy[ss] += SimilarityMeasure.BinaryAndNumeric(result, sentence.NumericGold);
                                    break;
                                case SimilaritySelector.Dice:
                                    GroupAccuracy[ss] += SimilarityMeasure.DicePlusANumber(result, sentence.BinaryGold);
                                    break;
                                case SimilaritySelector.Jaccard:
                                    GroupAccuracy[ss] += SimilarityMeasure.JaccardPlusANumber(result, sentence.BinaryGold);
                                    break;
                            }
                        }
                    }
                    foreach (SimilaritySelector ss in SimilaritySelectors)
                    {
                        AverageAccuracy[ss] += GroupAccuracy[ss] / Variable.Sentences.Count;
                    }
                }
                foreach (SimilaritySelector ss in SimilaritySelectors)
                {
                    AverageAccuracy[ss] /= GroupVariable.AnnotatorGroups.Length;
                    switch (ss)
                    {
                        case SimilaritySelector.Same:
                            Function.ConsoleWriteLine(t.Name + "Same: " + AverageAccuracy[ss]);
                            break;
                        case SimilaritySelector.Compare:
                            Function.ConsoleWriteLine(t.Name + "Compare: " + AverageAccuracy[ss]);
                            break;
                        case SimilaritySelector.Jaccard:
                            Function.ConsoleWriteLine(t.Name + "Jaccard: " + AverageAccuracy[ss]);
                            break;
                        case SimilaritySelector.Dice:
                            Function.ConsoleWriteLine(t.Name + "Dice: " + AverageAccuracy[ss]);
                            break;
                        case SimilaritySelector.BinaryResultAndNumericGold:
                            Function.ConsoleWriteLine(t.Name + "Binary&Numeric: " + AverageAccuracy[ss]);
                            break;
                    }
                }
            }
        }

        #region 1.0（注释）
        //static void Main(string[] args) //考察每句情感数，每种方法的结果放到不同的文件中
        //{
        //    Function.InitializeData();
        //    for (int i = 0; i <= Variable.OriginalNumberOfSentence; ++i)
        //    {
        //        Function.ConsoleWriteLine(i);
        //        Variable.ResultFile.Write(i + ",");
        //        Function.NumberOfLabelTopR(i);

        //        //Function.GenerateDependentMajorityVoteGold();
        //        Function.ConsoleWriteLine("INV and DNV as Gold");
        //        Function.INVandDNVasGold();

        //        Function.ConsoleWriteLine("Independent");
        //        Independent.FunctionOfInd.GenerateIndependentResult();
        //        Function.ConsoleWriteLine("Precise");
        //        Dependent.Precise.FunctionOfPre.GeneratePreciseResult();
        //        Function.ConsoleWriteLine("Tree for All");
        //        Dependent.Tree.Common.FunctionOfCommon.GenerateOneTreeForAllResult();
        //        Function.ConsoleWriteLine("Tree for Sentence");
        //        Dependent.Tree.Distinct.FunctionOfDistinct.GenerateOneTreeForSenResult();

        //        Function.GenerateSimilarityWithGolds("Independent", "DependentGold");
        //        Function.GenerateSimilarityWithGolds("Precise", "DependentGold");
        //        Function.GenerateSimilarityWithGolds("TreeForAll", "DependentGold");
        //        Function.GenerateSimilarityWithGolds("TreeForSen", "DependentGold");

        //        Function.GoldStandardSimilarity("DependentGold");
        //        Function.GenerateEverySimilarityWithGold();
        //        Function.AverageLabelsOfResultOrGold("DependentMajorityVoteGold");
        //        Function.AverageLabelsOfResultOrGold("IndResultAndDepResultGold");
        //        Function.AverageLabelsOfResultOrGold("IndependentResult");
        //        Function.AverageLabelsOfResultOrGold("PreciseResult");
        //        Function.AverageLabelsOfResultOrGold("TreeForAllResult");
        //        Function.AverageLabelsOfResultOrGold("TreeForSenResult");

        //        Function.NumberOfEachLabel();
        //    }
        //    Function.SimilarityOfAnnotator();//统计工作量，暂时没用
        //    Variable.OutputFile.Close();
        //    Variable.ResultFile.Close();
        //}
        #endregion
    }
}