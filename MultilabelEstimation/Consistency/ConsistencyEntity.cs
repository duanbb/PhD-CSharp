using System.Collections.Generic;

namespace MultilabelEstimation.Consistency
{
    sealed class Mcj
    {
        //Dictionary<角色>->[标签]->值
        public IDictionary<Character, IDictionary<Labelset, double>> Value;
        public int Time;
        public Mcj(int time)
        {
            Value = new Dictionary<Character, IDictionary<Labelset, double>>();
            Time = time;
        }
    }
}
