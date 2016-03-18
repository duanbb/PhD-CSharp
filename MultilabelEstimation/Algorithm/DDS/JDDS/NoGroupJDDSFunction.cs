using System;
using System.Collections.Generic;
using System.Linq;

namespace MultilabelEstimation.Algorithm.DDS.JDDS
{
    static class NoGroupJDDSFunction
    {
        static public void GeneratePreciseResult()
        {
            Initialize();
            for (int time = 1; time <= Variable.ConvergeTimeThreshold; ++time)
            {
                //计算Pk
                NoGroupJDDSVariable.Pj = CoreFunction.CalculatePj(NoGroupJDDSVariable.Sij, time);
                //计算π
                NoGroupDDSFunction.CalculatePAkjl(NoGroupJDDSVariable.Sij, ref NoGroupJDDSVariable.PAkjl);
                //计算Sij
                if (NoGroupDDSFunction.CalculatePdataAndSij(ref NoGroupJDDSVariable.Sij, NoGroupJDDSVariable.Pj, NoGroupJDDSVariable.PAkjl, ref NoGroupJDDSVariable.Pdata))
                {
                    break;
                }
            }
            NoGroupDDSFunction.ObtainResult(NoGroupJDDSVariable.Sij, "Precise");
        }

        static private void Initialize()
        {
            NoGroupJDDSVariable.Sij = new Sij(1);
            NoGroupJDDSVariable.Pj = new Pj(0);
            NoGroupJDDSVariable.PAkjl = new PAkjl(0);
            NoGroupJDDSVariable.Pdata = new Pdata(0, 0);

            //Dictionary<句子，Dictionary<标签，次数>> 句子i被标为j的次数，用于计算Sij
            double[,] Nil = new double[Variable.Sentences.Count, (int)Math.Pow(2, Variable.LabelArray.Length)];
            //句子i被标的总次数，无论标签为何，用于计算Sij
            double[] Ni = new double[Variable.Sentences.Count];
            foreach (Annotator annotator in Variable.Annotators)
            {
                foreach (Sentence sentence in Variable.Sentences)
                {
                    foreach (Annotation annotation in Variable.Data[annotator][sentence])
                    {
                        ++Nil[sentence.ID, annotation.IntLabel];
                        ++Ni[sentence.ID];
                    }
                }
            }
            //初始化Sij
            foreach (Sentence sentence in Variable.Sentences)
            {
                for (int l = 0; l < Math.Pow(2, Variable.LabelArray.Length); ++l)
                {
                    NoGroupJDDSVariable.Sij.Value[sentence][new Labelset(Variable.LabelArray, l)] = Nil[sentence.ID, l] / Ni[sentence.ID];
                }
            }
            //Variable.OutputFile.WriteLine(PreciseVariable.Sij.ToString(DependentVariable.NumberOfIntlabel));
        }
    }
}