using System;
using System.IO;
using MultilabelEstimation.Group;

namespace MultilabelEstimation.Algorithm.MV
{
    static class MVFunction
    {
        static public void RunMV(GoldType goldType)
        {
            Variable.MVType = goldType;
            for (int groupIndex = 0; groupIndex < GroupVariable.AnnotatorGroups.Length; ++groupIndex)
            {
                Function.WriteBinaryResultFile("MV", groupIndex);
            }
        }
    }
}