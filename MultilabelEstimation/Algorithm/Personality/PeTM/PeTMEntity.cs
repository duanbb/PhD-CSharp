using MultilabelEstimation.Consistency;
using System;
using System.Collections.Generic;

namespace MultilabelEstimation.Algorithm.Personality.PeTM
{
    sealed class Sije
    {
        //Dictionary<句，Dictionary<标注，值>>
        public IDictionary<Sentence, IDictionary<Labelset, IDictionary<Will, double>>> Value;
        public int Time;//记录迭代次数

        public Sije(int time)
        {
            Value = new Dictionary<Sentence, IDictionary<Labelset, IDictionary<Will, double>>>();
            Time = time;
        }

        public Sij ToSij//计算π时用
        {
            get
            {
                Sij sij = new Sij(this.Time);
                foreach (Sentence sentence in this.Value.Keys)
                {
                    sij.Value.Add(sentence, new Dictionary<Labelset, double>());
                    foreach (Labelset labelset in this.Value[sentence].Keys)
                    {
                        sij.Value[sentence].Add(labelset, this.Value[sentence][labelset][Will.strong] + this.Value[sentence][labelset][Will.weak]);
                    }
                }
                return sij;
            }
        }

        //指定will
        public KeyValuePair<Labelset, double> CalculateJointBestLabelset(Sentence sentence, Will will)
        {
            Labelset bestResult = new Labelset(Variable.LabelArray, 0);
            double bestResultValue = 0;
            foreach (Labelset subAnnotaton in this.Value[sentence].Keys)//此处决定了Sij.Value的Key不可以是Tuple<Sentence, Labelset>类型
            {
                if (this.Value[sentence][subAnnotaton][will] > bestResultValue)
                {
                    bestResult = new Labelset(subAnnotaton);
                    bestResultValue = this.Value[sentence][subAnnotaton][will];
                }
                else if (this.Value[sentence][subAnnotaton][will] == bestResultValue)
                {
                    foreach (Label label in subAnnotaton.Labels.Keys)
                    {
                        if (subAnnotaton.Labels[label])
                            bestResult.Labels[label] = true;
                    }
                }
            }
            return new KeyValuePair<Labelset, double>(bestResult, bestResultValue);
        }

        //不指定will，废弃
        public KeyValuePair<Labelset, double> CalculateJointBestLabelset(Sentence sentence, ref IDictionary<Will, double> willForResult)//同是最大值的j们取并
        {
            Labelset bestResult = null;
            double bestResultValue = 0;
            willForResult.Add(Will.strong, 0);
            willForResult.Add(Will.weak, 0);
            foreach (Labelset subAnnotaton in this.Value[sentence].Keys)//此处决定了Sij.Value的Key不可以是Tuple<Sentence, Labelset>类型
            {
                if (this.Value[sentence][subAnnotaton][Will.strong] != 0)
                {
                    if (this.Value[sentence][subAnnotaton][Will.strong] > bestResultValue)
                    {
                        bestResult = new Labelset(subAnnotaton);
                        bestResultValue = this.Value[sentence][subAnnotaton][Will.strong];
                        willForResult[Will.strong] = this.Value[sentence][subAnnotaton][Will.strong];
                        willForResult[Will.weak] = this.Value[sentence][subAnnotaton][Will.weak];
                    }
                    else if (this.Value[sentence][subAnnotaton][Will.strong] == bestResultValue)
                    {
                        foreach (Label label in subAnnotaton.Labels.Keys)
                        {
                            if (subAnnotaton.Labels[label])
                                bestResult.Labels[label] = true;
                        }
                        willForResult[Will.strong] = this.Value[sentence][subAnnotaton][Will.strong];
                        willForResult[Will.weak] = this.Value[sentence][subAnnotaton][Will.weak];
                    }
                }
                if (this.Value[sentence][subAnnotaton][Will.weak] != 0)
                {
                    if (this.Value[sentence][subAnnotaton][Will.weak] > bestResultValue)
                    {
                        bestResult = new Labelset(subAnnotaton);
                        bestResultValue = this.Value[sentence][subAnnotaton][Will.weak];
                        willForResult[Will.strong] = this.Value[sentence][subAnnotaton][Will.strong];
                        willForResult[Will.weak] = this.Value[sentence][subAnnotaton][Will.weak];
                    }
                    else if (this.Value[sentence][subAnnotaton][Will.weak] == bestResultValue)
                    {
                        foreach (Label label in subAnnotaton.Labels.Keys)
                        {
                            if (subAnnotaton.Labels[label])
                                bestResult.Labels[label] = true;
                        }
                        willForResult[Will.strong] = this.Value[sentence][subAnnotaton][Will.strong];
                        willForResult[Will.weak] = this.Value[sentence][subAnnotaton][Will.weak];
                    }
                }
            }
            double will = willForResult[Will.strong] + willForResult[Will.weak];
            willForResult[Will.strong] /= will;
            willForResult[Will.weak] /= will;
            return new KeyValuePair<Labelset, double>(bestResult, bestResultValue);
        }
    }
}