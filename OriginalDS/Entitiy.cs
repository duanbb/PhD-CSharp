using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace OriginalDS
{
    //声明所有可能用到的变量
    static class Variables
    {
        static public int CountOfLabelKinds;
        //{
        //    get
        //    {
        //        return 4;
        //    }
        //}
        static public int CountOfAnnotators;
        //{
        //    get
        //    {
        //        return 5;
        //    }
        //}

        //sentence包含annotator包含label
        static public Instances Instances = new Instances();
        static public Eij Eij = new Eij(1);
        static public Pajl Pajl = new Pajl(0);
        static public Pj Pj = new Pj(0);
        //整个系统的似然函数的期望P(data)
        static public Pdata Pdata = new Pdata(0, 0);

        //空哈希表
        static public Dictionary<int, double> emptyLdouble
        {
            get
            {
                Dictionary<int, double> emptyLdouble = new Dictionary<int, double>();
                for (int l = 0; l < Variables.CountOfLabelKinds; ++l)
                {
                    emptyLdouble.Add(l, 0);
                }
                return emptyLdouble;
            }
        }
        static public Dictionary<int, int> emptyLabelDicictionary
        {
            get
            {
                Dictionary<int, int> emptyLDic = new Dictionary<int, int>();
                for (int l = 0; l < Variables.CountOfLabelKinds; ++l)
                {
                    emptyLDic.Add(l, 0);
                }
                return emptyLDic;
            }
        }
        static public Dictionary<int, int> emptyInstancesDictionary
        {
            get
            {
                Dictionary<int, int> emptySDic = new Dictionary<int, int>();
                for (int s = 0; s < Variables.Instances.Value.Count; ++s)
                {
                    emptySDic.Add(s, 0);
                }
                return emptySDic;
            }
        }
        static public Dictionary<int, int> emptySDic1
        {
            get
            {
                Dictionary<int, int> emptySDic1 = new Dictionary<int, int>();
                for (int s = 0; s < Variables.Instances.Value.Count; ++s)
                {
                    emptySDic1.Add(s, 1);
                }
                return emptySDic1;
            }
        }
        static public Dictionary<int, double> emptyLdoubleValues1
        {
            get
            {
                Dictionary<int, double> emptyLdoubleValues1 = new Dictionary<int, double>();
                for (int l = 0; l < Variables.CountOfLabelKinds; ++l)
                {
                    emptyLdoubleValues1.Add(l, 1);
                }
                return emptyLdoubleValues1;
            }
        }
        //创建标签对集合
        static public Dictionary<Pair, double> emptySiNldouble
        {
            get
            {
                Dictionary<Pair, double> emptySiNldouble = new Dictionary<Pair, double>();
                for (int j = 0; j < Variables.CountOfLabelKinds; ++j)
                {
                    for (int l = 0; l < Variables.CountOfLabelKinds; ++l)
                    {
                        emptySiNldouble.Add(new Pair(j, l), 0);
                    }
                }
                return emptySiNldouble;
            }
        }

        static public StreamWriter ResultFile = new StreamWriter("output.txt");
        static public string spliter = "************************************************************************************************************************";
    }

    sealed class Instances
    {
        public List<Instance> Value;
        public Instances()
        {
            Value = new List<Instance>();
            string[] data = File.ReadAllLines("input.txt");
            IList<int> labelKinds = new List<int>();
            int numberOfAnnotators = 0;
            foreach (string datum in data)//一行一个datum，也就是一个instance
            {
                Instance instance = new Instance();
                MatchCollection labels = new Regex(@"(\d+)").Matches(datum);//连续的若干个数字
                int numberOfAnnotatorsForThisInstance = 0;//count the number of annotators for this instance
                for (int i = 0; i < labels.Count; ++i)
                {
                    Annotator annotator = new Annotator();
                    foreach (char charLabel in labels[i].Groups[1].Value)
                    {
                        int intLabel = Convert.ToInt16(charLabel) - Convert.ToInt16('0');
                        annotator.labels.Add(intLabel);
                        //count the number of kinds of label
                        if (!labelKinds.Contains(intLabel))
                            labelKinds.Add(intLabel);
                    }
                    instance.annotators.Add(annotator);

                    ++numberOfAnnotatorsForThisInstance;
                }
                Value.Add(instance);
                //count the number of annotators
                if (numberOfAnnotators < numberOfAnnotatorsForThisInstance)
                    numberOfAnnotators = numberOfAnnotatorsForThisInstance;
            }
            Variables.CountOfLabelKinds = labelKinds.Count;
            Variables.CountOfAnnotators = numberOfAnnotators;
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
                return Math.Abs(PreValue - Value);
            }
        }
        public Pdata(int time, double preValue)
        {
            Time = time;
            PreValue = preValue;
        }
        public override string ToString()
        {
            return "Pdata's Time: " + Time + "\r\nValue:" + Value + "\r\nChange of Value:" + MondifiedValue + "\r\n" + Variables.spliter + "\r\n" + Variables.spliter;
        }
    }

    //每个人对一个句子有若干个标注
    sealed class Annotator
    {
        public List<int> labels = new List<int>();
    }

    sealed class Instance
    {
        public List<Annotator> annotators = new List<Annotator>();
    }

    sealed class Pajl
    {
        //Dictionary<人，<KeiValuePair<标签，标签>,值>>
        public Dictionary<int, Dictionary<Pair, double>> Value;
        public int Time;
        public Pajl(int time)
        {
            Value = new Dictionary<int, Dictionary<Pair, double>>();
            //初始化π
            for (int k = 0; k < Variables.CountOfAnnotators; ++k)//人
            {
                Value.Add(k, new Dictionary<Pair, double>(Variables.emptySiNldouble));
            }
            Time = time;
        }
        public override string ToString()
        {
            string result = "Pajl's Time: " + Time.ToString() + "\r\n";
            foreach (KeyValuePair<int, Dictionary<Pair, double>> k in Value)
            {
                result += "Annotator: " + k.Key + ", <(Label_j, Label_l), Value>:\r\n";
                int t = 0;
                foreach (KeyValuePair<Pair, double> pair in k.Value)
                {
                    ++t;
                    result += "<" + pair.Key.ToString() + ": " + pair.Value + "> ";
                    if (t % Variables.CountOfLabelKinds == 0)
                        result += "\r\n";
                }
            }
            return result + Variables.spliter;
        }
    }

    sealed class Eij
    {
        //Dictionary<句，Dictionary<标签，值>>
        public Dictionary<int, Dictionary<int, double>> Value;
        public int Time;//记录迭代次数

        public Eij(int time)
        {
            Value = new Dictionary<int, Dictionary<int, double>>();
            //初始化Eij
            for (int i = 0; i < Variables.Instances.Value.Count; ++i)
            {
                Value.Add(i, new Dictionary<int, double>(Variables.emptyLdouble));
            }

            Time = time;
        }
        public override string ToString()
        {
            string result = "Eij's Time: " + Time.ToString() + "\r\n";
            foreach (KeyValuePair<int, Dictionary<int, double>> i in Value)
            {
                result += "Instance: " + i.Key + "\r\n" + "<Label: Value>:\r\n";
                foreach (KeyValuePair<int, double> l in i.Value)
                {
                    //result += "<" + l.Key + ": " + string.Format("{0:0.00e0}", l.Value) + "> ";
                    result += "<" + l.Key + ": " + l.Value + "> ";
                }
                result += "\r\n";
            }
            return result + Variables.spliter;
        }
    }

    sealed class Pj
    {
        //Dictionary<标签，值>>
        public Dictionary<int, double> Value;
        public int Time;

        public Pj(int time)
        {
            Value = new Dictionary<int, double>();
            //初始化Pj
            for (int l = 0; l < Variables.CountOfLabelKinds; ++l)
            {
                Value.Add(l, 0);
            }
            Time = time;
        }
        public override string ToString()
        {
            string result = "Pj's Time: " + Time.ToString() + "\r\n" + "<Label: Value>:\r\n";
            foreach (KeyValuePair<int, double> l in Value)
            {
                result += "<" + l.Key + ": " + l.Value + "> ";
            }
            return result + "\r\n" + Variables.spliter;
        }
    }

    struct Pair
    {
        // Fields
        public int First;
        public int Second;

        public Pair(int x, int y)
        {
            this.First = x;
            this.Second = y;
        }

        public override string ToString()
        {
            return "(" + First + ", " + Second + ")";
        }
    }
}