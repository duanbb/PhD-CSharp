using System.Collections.Generic;
using System.Linq;

namespace MultilabelEstimation.Consistency
{
    class ConsistencyFunction
    {
        //计算mcj（consistency：角色c有j标签的概率）
        static public Mcj CalculateMcj(Sij sij, int time)
        {
            Mcj mcj = new Mcj(time);
            foreach (Character character in ConsistencyVariable.Characters)
            {
                mcj.Value.Add(character, new Dictionary<Labelset, double>());
                foreach (Sentence sentence in character.Sentences)
                {
                    foreach (Labelset labelset in sij.Value[sentence].Keys)
                    {
                        if (mcj.Value[character].ContainsKey(labelset))
                            mcj.Value[character][labelset] += sij.Value[sentence][labelset];
                        else
                            mcj.Value[character].Add(labelset, sij.Value[sentence][labelset]);
                    }
                }
            }
            foreach (Character character in ConsistencyVariable.Characters)
            {
                foreach (Labelset labelset in mcj.Value[character].Keys.ToArray())
                {
                    mcj.Value[character][labelset] /= character.Sentences.Count;
                }
            }
            return mcj;
        }
    }
}
