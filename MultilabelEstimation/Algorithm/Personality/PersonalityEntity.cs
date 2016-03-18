using MultilabelEstimation.Consistency;
using System;
using System.Collections.Generic;

namespace MultilabelEstimation.Algorithm.Personality
{
    sealed class BEkef
    {
        public IDictionary<Annotator, IDictionary<Tuple<Will, Will>, double>> Value;
        public int Time;

        public BEkef(int time)
        {
            Value = new Dictionary<Annotator, IDictionary<Tuple<Will, Will>, double>>();
            Time = time;
        }
    }

    sealed class Mce//Character's will
    {
        public IDictionary<Character, IDictionary<Will, double>> Value;
        public int Time;
        public Mce(int time)
        {
            Value = new Dictionary<Character, IDictionary<Will, double>>();
            Time = time;
        }
        public IDictionary<Character, Tuple<Will, string>> EstimatedPersonality
        {
            get
            {
                IDictionary<Character, Tuple<Will, string>> optimalPersonality = new Dictionary<Character, Tuple<Will, string>>();
                foreach (Character character in this.Value.Keys)
                {
                    optimalPersonality.Add(character, Tuple.Create(this.Value[character][Will.strong] >= this.Value[character][Will.weak] ? Will.strong : Will.weak, "s:" + this.Value[character][Will.strong] + "; w:" + this.Value[character][Will.weak]));
                }
                return optimalPersonality;
            }
        }
    }

    sealed class Pje//是p(t|e)，不是联合概率p(t,x)
    {
        public IDictionary<Labelset, IDictionary<Will, double>> Value;
        public int Time;//记录迭代次数

        public Pje(int time)
        {
            Value = new Dictionary<Labelset, IDictionary<Will, double>>();
            Time = time;
        }
    }
}
