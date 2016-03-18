using System;
using System.Collections.Generic;
using System.IO;

namespace SinglelabelEstimation
{
    enum TextType
    {
        Doctor, CrowdScale
    }
    //声明所有可能用到的变量
    static class Variable
    {
        static public StreamWriter ResultFile = new StreamWriter("Result.txt");
        static public string[] Workers;
        static public IDictionary<string, IDictionary<string, IList<int>>> Sentences;//sentence包含worker包含label，与multiple label estimation相反
        static public IDictionary<string, int> GoldStandard;
        static public IDictionary<string, string> SentenceTexts;//句子和正文
        static public string spliter = "**************************************************";
        static public bool OutputS = false;
        static public bool OutputP = false;
        static public bool OutputPai = false;
        static public bool OutputPdata = true;
        static public bool OutputAccuracy = true;
        static public Sij Sij;
        static public Pajl Pajl;
        static public Pj Pj;
        static public Pdata Pdata;

        static public int CountOfLabelKinds
        {
            get
            {
                return 5;
            }
        }
        static public int CountOfWorkers
        {
            get
            {
                return 1960;
            }
        }

        //空哈希表
        static public Dictionary<int, double> emptyLdouble
        {
            get
            {
                Dictionary<int, double> emptyLdouble = new Dictionary<int, double>();
                for (int l = 0; l < Variable.CountOfLabelKinds; ++l)
                {
                    emptyLdouble.Add(l, 0);
                }
                return emptyLdouble;
            }
        }
        static public Dictionary<string, double> emptySdouble
        {
            get
            {
                Dictionary<string, double> emptySDic = new Dictionary<string, double>();
                foreach (string sentence in Variable.Sentences.Keys)
                {
                    emptySDic.Add(sentence, 0);
                }
                return emptySDic;
            }
        }
        static public Dictionary<int, double> emptyLdoubleValues1
        {
            get
            {
                Dictionary<int, double> emptyLdoubleValues1 = new Dictionary<int, double>();
                for (int l = 0; l < Variable.CountOfLabelKinds; ++l)
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
                for (int j = 0; j < Variable.CountOfLabelKinds; ++j)
                {
                    for (int l = 0; l < Variable.CountOfLabelKinds; ++l)
                    {
                        emptySiNldouble.Add(new Pair(j, l), 0);
                    }
                }
                return emptySiNldouble;
            }
        }
    }

    struct Sentence
    {
        public string ID ;
        public string Text ;

        public Sentence(string id, string text)
        {
            ID = id;
            Text = text;
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
            return "Pdata's Time: " + Time + "\r\nValue:" + Value + "\r\nChange of Value:" + MondifiedValue + "\r\n" + Variable.spliter + "\r\n" + Variable.spliter;
        }
    }

    sealed class Pajl
    {
        //Dictionary<人，<KeiValuePair<标签，标签>,值>>
        public Dictionary<string, Dictionary<Pair, double>> Value;
        public int Time;
        public Pajl(int time)
        {
            Value = new Dictionary<string, Dictionary<Pair, double>>();
            //初始化π
            foreach (string worker in Variable.Workers)//人
            {
                Value.Add(worker, new Dictionary<Pair, double>(Variable.emptySiNldouble));
            }
            Time = time;
        }
        public override string ToString()
        {
            string result = "Pajl's Time: " + Time.ToString() + "\r\n";
            foreach (KeyValuePair<string, Dictionary<Pair, double>> worker in Value)
            {
                result += "Worker: " + worker.Key + ", <(Label_j, Label_l), Value>:\r\n";
                int t = 0;
                foreach (KeyValuePair<Pair, double> pair in worker.Value)
                {
                    ++t;
                    result += "<" + pair.Key.ToString() + ": " + pair.Value + "> ";
                    if (t % Variable.CountOfLabelKinds == 0)
                        result += "\r\n";
                }
            }
            return result + Variable.spliter;
        }
    }

    sealed class Sij
    {
        //Dictionary<句，Dictionary<标签，值>>
        public Dictionary<string, Dictionary<int, double>> Value;
        public int Time;//记录迭代次数

        public Sij(int time)
        {
            Value = new Dictionary<string, Dictionary<int, double>>();
            //初始化Sij
            foreach (string sentence in Variable.Sentences.Keys)
            {
                Value.Add(sentence, new Dictionary<int, double>(Variable.emptyLdouble));
            }

            Time = time;
        }
        public override string ToString()
        {
            string result = "Sij's Time: " + Time.ToString() + "\r\n";
            foreach (KeyValuePair<string, Dictionary<int, double>> i in Value)
            {
                result += "Sentence: " + i.Key + "\r\n" + "<Label: Value>:\r\n";
                foreach (KeyValuePair<int, double> l in i.Value)
                {
                    result += "<" + l.Key + ": " + l.Value + "> ";
                }
                result += "\r\n";
            }
            return result + Variable.spliter;
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
            for (int l = 0; l < Variable.CountOfLabelKinds; ++l)
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
            return result + "\r\n" + Variable.spliter;
        }
    }

    struct Pair : IEquatable<Pair>
    {
        // Fields
        public int First;
        public int Second;

        public Pair(int x, int y)
        {
            this.First = x;
            this.Second = y;
        }

        public bool Equals(Pair other)
        {
            return First == other.First && Second == other.Second;
        }

        public override string ToString()
        {
            return "(" + First + ", " + Second + ")";
        }
    }
}