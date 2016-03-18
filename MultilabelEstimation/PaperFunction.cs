using System.Collections.Generic;
using System.IO;

namespace MultilabelEstimation
{
    class PaperFunction
    {
        //计算两篇故事共有多少人标过
        static public int CalculateWorkers()
        {
            //第一步：将一个故事的worker存到文件里
            //StreamWriter workerFile = new StreamWriter("workers.txt");
            //foreach (Annotator annotator in Variable.Annotators)
            //{
            //    workerFile.WriteLine(annotator.ID);
            //}
            //workerFile.Close();
            //第二步：读取文件，与第二个故事的人作比较
            string[] workers = File.ReadAllLines("workers.txt");
            int n = Variable.Annotators.Count;
            foreach (string worker in workers)
            {
                bool existedWorker = false;
                foreach (Annotator annotator in Variable.Annotators)
                {
                    if (worker == annotator.ID)
                    {
                        existedWorker = true;
                        break;
                    }
                }
                if (!existedWorker)
                {
                    ++n;
                }
            }
            return n;
        }

        static public List<KeyValuePair<Label, int>> NumberOfEachLabel()
        {
            IDictionary<Label, int> numberOfEachLabel = new Dictionary<Label, int>();
            int allNumber = 0;
            int neutralNumber = 0;
            int labelsNumber = 0;
            double n = 0;
            foreach (Label label in Variable.LabelArray)
            {
                numberOfEachLabel.Add(label, 0);
            }
            foreach (Annotator annotator in Variable.Annotators)
            {
                foreach (Sentence sentence in Variable.Sentences)
                {
                    if (Variable.Data[annotator].ContainsKey(sentence))//判断此annotator是否标了此sentence
                    {
                        foreach (Annotation annotation in Variable.Data[annotator][sentence])
                        {
                            ++n;
                            if (annotation.Mu)
                            {
                                ++neutralNumber;
                                ++allNumber;
                                continue;
                            }
                            foreach (Label label in Variable.LabelArray)
                            {
                                if (annotation.Labels[label])
                                {
                                    ++numberOfEachLabel[label];
                                    ++labelsNumber;
                                    ++allNumber;
                                }
                            }
                        }
                    }
                }
            }
            double labelsPersentence = labelsNumber / n;
            List<KeyValuePair<Label, int>> sortedLabelAndTimes = new List<KeyValuePair<Label, int>>(numberOfEachLabel);
            sortedLabelAndTimes.Sort(delegate(KeyValuePair<Label, int> s1, KeyValuePair<Label, int> s2)
            {
                return s2.Value.CompareTo(s1.Value);
            });
            return sortedLabelAndTimes;
        }

        //求出每项标注的平均值
        static public void AverageTrueLabelsPerAnnotation()
        {
            Variable.AverageTrueLabelsPerAnnotation = 0;
            int n = 0;
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (Annotator annotator in Variable.Annotators)
                {
                    if (Variable.Data[annotator].ContainsKey(sentence))
                    {
                        foreach (Annotation annotation in Variable.Data[annotator][sentence])
                        {
                            ++n;
                            Variable.AverageTrueLabelsPerAnnotation += annotation.NumberOfTrueLabel;
                        }
                    }
                }
            }
            Variable.AverageTrueLabelsPerAnnotation /= n;
            Variable.OutputFile.WriteLine("Average true labels per annotation: " + Variable.AverageTrueLabelsPerAnnotation);
        }
    }
}