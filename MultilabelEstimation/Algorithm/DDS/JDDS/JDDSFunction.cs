using MultilabelEstimation.Group;

namespace MultilabelEstimation.Algorithm.DDS.JDDS
{
    static class JDDSFunction
    {
        static public void RunJDDS()
        {
            //遍历在某个group size分组下的第几组
            for (int groupIndex = 0; groupIndex < GroupVariable.AnnotatorGroups.Length; ++groupIndex)
            {
                Sij sij = CoreFunction.InitializeSij(Variable.LabelArray, groupIndex);
                CoreFunction.Intgerate(Variable.LabelArray, groupIndex, ref sij);//迭代在此
                DDSFunction.ObtainBinaryResult(sij, "JDDS", groupIndex);
                Function.WriteBinaryResultFile("JDDS", groupIndex);
            }
        }
    }
}