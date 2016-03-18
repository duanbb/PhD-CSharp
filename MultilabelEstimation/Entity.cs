using System.Collections.Generic;

namespace MultilabelEstimation
{
    sealed class Sij
    {
        //Dictionary<句，Dictionary<标注，值>>
        public IDictionary<Sentence, IDictionary<Labelset, double>> Value;
        public int Time;//记录迭代次数

        public Sij(int time)
        {
            Value = new Dictionary<Sentence, IDictionary<Labelset, double>>();
            Time = time;
        }

        public Sij(Sij sij)//复制初始化值，用于PjOrPij.PijNotChange时
        {
            Value = sij.Value;
            Time = sij.Time;
        }

        public KeyValuePair<Labelset, double> CalculateJointBestLabelset(Sentence sentence)//同是最大值的j们取并
        {
            Labelset bestResult = null;
            double bestResultValue = 0;
            //double totalProbability = 0;
            foreach (Labelset subAnnotaton in this.Value[sentence].Keys)//此处决定了Sij.Value的Key不可以是Tuple<Sentence, Labelset>类型
            {
                if (this.Value[sentence][subAnnotaton] > bestResultValue)
                {
                    bestResult = new Labelset(subAnnotaton);
                    bestResultValue = this.Value[sentence][subAnnotaton];
                }
                else if (this.Value[sentence][subAnnotaton] == bestResultValue)
                {
                    foreach (Label label in subAnnotaton.Labels.Keys)
                    {
                        if (subAnnotaton.Labels[label])
                            bestResult.Labels[label] = true;
                    }
                }
                //totalProbability += this.Value[sentence][subAnnotaton];
            }
            return new KeyValuePair<Labelset, double>(bestResult, bestResultValue);
        }

        public List<KeyValuePair<Labelset, double>> SortLabelsets(Sentence sentence)//得到MaxBinary结果（一系列J排序）
        {
            List<KeyValuePair<Labelset, double>> sortedlabelset = new List<KeyValuePair<Labelset, double>>(this.Value[sentence]);
            sortedlabelset.Sort(delegate(KeyValuePair<Labelset, double> s1, KeyValuePair<Labelset, double> s2)
            {
                if (s1.Value != s2.Value)
                    return s2.Value.CompareTo(s1.Value);
                else if (s1.Key.NumberOfTrueLabels != s2.Key.NumberOfTrueLabels)
                    return s1.Key.NumberOfTrueLabels.CompareTo(s2.Key.NumberOfTrueLabels);
                else
                    return s1.Key.IntLabel.CompareTo(s2.Key.IntLabel);
            });
            return sortedlabelset;
        }

        //public KeyValuePair<Labelset, double> CalculateBestLabelset(Sentence sentence, IDictionary<Labelset, double> probability)
        //{
        //    IList<Labelset> bestResults = new List<Labelset>();
        //    double bestResultValue = 0;
        //    foreach (Labelset subAnnotaton in this.Value[sentence].Keys)
        //    {
        //        if (this.Value[sentence][subAnnotaton] > bestResultValue)
        //        {
        //            bestResults.Clear();
        //            bestResults.Add(subAnnotaton);
        //            bestResultValue = this.Value[sentence][subAnnotaton];
        //        }
        //        else if (this.Value[sentence][subAnnotaton] == bestResultValue)
        //        {
        //            bestResults.Add(subAnnotaton);
        //        }
        //    }
        //    if (bestResults.Count == 1)
        //        return new KeyValuePair<Labelset, double>(bestResults[0], bestResultValue);
        //    else
        //    {
        //        Labelset bestResult = null;
        //        double bestResultValueInPj = 0;
        //        foreach (Labelset substitutbestResult in bestResults)
        //        {
        //            if (probability[substitutbestResult] > bestResultValueInPj)
        //            {
        //                bestResult = new Labelset(substitutbestResult);
        //                bestResultValueInPj = probability[substitutbestResult];
        //            }
        //            else if (probability[substitutbestResult] == bestResultValueInPj)//测试用
        //            {

        //            }
        //        }
        //        return new KeyValuePair<Labelset, double>(bestResult, bestResultValue);
        //    }
        //}

        //public KeyValuePair<Labelset, double> CalculateBestLabelset(Sentence sentence, IDictionary<Labelset, double> probability1, IDictionary<Labelset, double> probability2)
        //{
        //    IList<Labelset> bestResults = new List<Labelset>();
        //    double bestResultValue = 0;
        //    foreach (Labelset subAnnotaton in this.Value[sentence].Keys)
        //    {
        //        if (this.Value[sentence][subAnnotaton] > bestResultValue)
        //        {
        //            bestResults.Clear();
        //            bestResults.Add(subAnnotaton);
        //            bestResultValue = this.Value[sentence][subAnnotaton];
        //        }
        //        else if (this.Value[sentence][subAnnotaton] == bestResultValue)
        //        {
        //            bestResults.Add(subAnnotaton);
        //        }
        //    }
        //    if (bestResults.Count == 1)
        //        return new KeyValuePair<Labelset, double>(bestResults[0], bestResultValue);
        //    else
        //    {
        //        IList<Labelset> bestResults1 = new List<Labelset>();
        //        double bestResultValue1 = 0;
        //        foreach (Labelset substitutbestResult in bestResults)
        //        {
        //            if (probability1[substitutbestResult] > bestResultValue1)
        //            {
        //                bestResults1.Clear();
        //                bestResults1.Add(substitutbestResult);
        //                bestResultValue1 = probability1[substitutbestResult];
        //            }
        //            else if (probability1[substitutbestResult] == bestResultValue1)
        //            {
        //                bestResults1.Add(substitutbestResult);
        //            }
        //        }
        //        if (bestResults1.Count == 1)
        //            return new KeyValuePair<Labelset, double>(bestResults1[0], bestResultValue);
        //        else
        //        {
        //            Labelset bestResult = null;
        //            double bestResultValue2 = 0;
        //            foreach (Labelset substitutbestResult in bestResults)
        //            {
        //                if (probability2[substitutbestResult] > bestResultValue2)
        //                {
        //                    bestResult = new Labelset(substitutbestResult);
        //                    bestResultValue2 = probability2[substitutbestResult];
        //                }
        //                else if (probability2[substitutbestResult] == bestResultValue2)//测试用
        //                {

        //                }
        //            }
        //            return new KeyValuePair<Labelset, double>(bestResult, bestResultValue);
        //        }
        //    }
        //}

        public override string ToString()
        {
            string result = "Sij's Time: " + Time.ToString() + "\r\n";
            foreach (Sentence sentence in Value.Keys)
            {
                result += "Sentence: " + sentence.ID + "\r\n" + "<Result: Value>:\r\n";
                foreach (Labelset labelset in Value[sentence].Keys)
                {
                    if (Value[sentence][labelset] != 0)
                        result += "<" + labelset.ToString() + ": " + Value[sentence][labelset] + "> ";
                }
                result += "\r\n";
            }
            return result + Variable.spliter;
        }
    }

    sealed class Pj
    {
        //Dictionary<结果，值>
        public IDictionary<Labelset, double> Value;
        public int Time;

        public Pj(int time)
        {
            Value = new Dictionary<Labelset, double>();
            Time = time;
        }
        public Pj(Pj pj)
        {
            Value = pj.Value;
            Time = pj.Time;
        }
        public override string ToString()
        {
            string result = "Pj's Time: " + Time.ToString() + " " + "<Result: Value>:\r\n";
            foreach (Labelset labelset in Value.Keys)
            {
                if (Value[labelset] != 0)
                    result += "<" + labelset.ToString() + ": " + Value[labelset] + "> ";
            }
            return result + "\r\n" + Variable.spliter;
        }
    }

    sealed class PAkjl
    {
        //Dictionary<worker>->[标签，标签]->值
        public IDictionary<Annotator, IDictionary<Labelset, IDictionary<Labelset, double>>> Value;
        public int Time;
        public PAkjl(int time)
        {
            Value = new Dictionary<Annotator, IDictionary<Labelset, IDictionary<Labelset, double>>>();
            Time = time;
        }
        public override string ToString()
        {
            string result = "Pajl's Time: " + Time.ToString() + "\r\n";
            foreach (Annotator annotator in Value.Keys)
            {
                result += "Annotator: " + annotator.ID + ", <(Label_j, Label_l): Value>:\r\n";
                bool allZero = true;
                foreach (Labelset labelsetj in this.Value[annotator].Keys)
                {
                    foreach (Labelset labelsetl in this.Value[annotator][labelsetj].Keys)
                    {
                        if (this.Value[annotator][labelsetj][labelsetl] != 0)
                        {
                            result += "<(" + labelsetj.ToString() + ": " + labelsetl.ToString() + "): " + this.Value[annotator][labelsetj][labelsetl] + "> ";
                            allZero = false;
                        }
                        if (!allZero)
                        {
                            result += "\r\n";
                            allZero = true;
                        }
                    }
                }
            }
            return result + Variable.spliter;
        }
    }

    sealed class Pdata
    {
        public double Value;
        public double PreValue;
        public int Time;
        public double MondifiedValue
        {
            get
            {
                return Value - PreValue;
                //return Math.Abs(PreValue - Value);
            }
        }
        public Pdata(int time, double preValue)
        {
            Time = time;
            PreValue = preValue;
        }
        public override string ToString()
        {
            return "Pdata's Time: " + Time + "\r\nValue:" + Value + "\r\nChange of Value:" + MondifiedValue + "\r\n" + Variable.spliter + "\r\n" + Variable.spliter;
        }
    }
}
