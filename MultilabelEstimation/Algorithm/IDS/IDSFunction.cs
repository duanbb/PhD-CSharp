using MultilabelEstimation.Group;
using System.Collections.Generic;

namespace MultilabelEstimation.Algorithm.IDS
{
    static class IDSFunction
    {
        static public void RunIDS()
        {
            for (int groupIndex = 0; groupIndex < GroupVariable.AnnotatorGroups.Length; ++groupIndex)
            {
                foreach (Sentence sentence in Variable.Sentences)
                {
                    sentence.AnnotaitonGroups[groupIndex].IDSNumResult = new NumericResult();
                    sentence.AnnotaitonGroups[groupIndex].IDSResult = new Result();
                }
                foreach (Label label in Variable.LabelArray)
                {
                    Sij sij = CoreFunction.InitializeSij(new Label[] { label }, groupIndex);
                    CoreFunction.Intgerate(new Label[] { label }, groupIndex, ref sij);
                    ObtainLabelResult(sij, groupIndex);
                }
                Function.WriteBinaryResultFile("IDS", groupIndex);
            }
        }

        //得到一种情感结果
        static private void ObtainLabelResult(Sij sij, int group)
        {
            foreach (Sentence sentence in sij.Value.Keys)
            {
                //得到numeric结果
                foreach (Labelset Labelset in sij.Value[sentence].Keys)
                {
                    foreach (Label label in Labelset.Labels.Keys)//其实就一个Label
                    {
                        if (Labelset.Labels[label])
                            sentence.AnnotaitonGroups[group].IDSNumResult.Labels[label] = sij.Value[sentence][Labelset];
                    }
                }
                //得到binary结果
                KeyValuePair<Labelset, double> resultAndProbability = sij.CalculateJointBestLabelset(sentence);
                foreach (Label label in resultAndProbability.Key.Labels.Keys)
                {
                    sentence.AnnotaitonGroups[group].IDSResult.Labels[label] = resultAndProbability.Key.Labels[label];
                }
                sentence.AnnotaitonGroups[group].IDSResult.Probability *= resultAndProbability.Value;
            }
        }
    }
}