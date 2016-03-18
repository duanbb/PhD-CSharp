using System.Collections.Generic;
using System.IO;

namespace Statistic
{
    enum Affect
    {
        yorokobi,
        suki,
        yasu,
        ikari,
        aware,
        kowa,
        haji,
        iya,
        takaburi,
        odoroki,
        mu,
    }
    static class Variable
    {
        static public int NumberOfSentence = 78;
        static public int KindsOfLabel = 11;
        static public StreamWriter ResultFile = new StreamWriter("Result.csv");
        static public Sentence[] Sentences = new Sentence[NumberOfSentence];
        static public int NumberOfLabel = NumberOfSentence * KindsOfLabel;
        static public int NumberOfAnnotator = 10;
        //用于遍历情感
        static public Affect[] AffectArray = new Affect[10] { Affect.yorokobi, Affect.suki, Affect.yasu, Affect.ikari, Affect.aware, Affect.kowa, Affect.haji, Affect.iya, Affect.takaburi, Affect.odoroki };
        static public Affect[] AffectAndMuArray = new Affect[11] { Affect.yorokobi, Affect.suki, Affect.yasu, Affect.ikari, Affect.aware, Affect.kowa, Affect.haji, Affect.iya, Affect.takaburi, Affect.odoroki, Affect.mu };
    }

    sealed class Sentence
    {
        public int ID = 0;
        public Annotator[] Annotators;
        public string Speech;//句子原文
        public int LabelOfCount = 0;//平均每人为每句标的情感数
        public Annotator SynthesizedResult;//计算三次互信息时使用
        public double Entropy;//计算一致性时使用
        public Sentence(int id, string speech)
        {
            ID = id;
            Speech = speech;
            Annotators = new Annotator[Variable.NumberOfAnnotator];
            for (int i = 0; i < 10; ++i)
            {
                Annotators[i] = new Annotator();
            }
            SynthesizedResult = new Annotator();
        }
    }

    sealed class Annotator
    {
        public Dictionary<Affect, bool> Affects = new Dictionary<Affect, bool>();

        public Annotator()
        {
            Affects.Add(Affect.yorokobi, false);
            Affects.Add(Affect.suki, false);
            Affects.Add(Affect.yasu, false);
            Affects.Add(Affect.ikari, false);
            Affects.Add(Affect.aware, false);
            Affects.Add(Affect.kowa, false);
            Affects.Add(Affect.haji, false);
            Affects.Add(Affect.iya, false);
            Affects.Add(Affect.takaburi, false);
            Affects.Add(Affect.odoroki, false);
            Affects.Add(Affect.mu, false);
        }
    }

    struct BiAffect
    {
        public Affect Affect1;
        public Affect Affect2;

        public BiAffect(Affect affect1, Affect affect2)
        {
            Affect1 = affect1;
            Affect2 = affect2;
        }
    }
}
