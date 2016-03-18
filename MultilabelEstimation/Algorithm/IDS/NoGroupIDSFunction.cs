using MultilabelEstimation.Group;
using MultilabelEstimation.Consistency;
using System.Collections.Generic;
using System.IO;
using System;

namespace MultilabelEstimation.Algorithm.IDS
{
    static class NoGroupIDSFunction
    {
        static public void GenerateIndependentResult()
        {
            foreach (Sentence sentence in Variable.Sentences)
            {
                sentence.IndependentResult = new Result();
            }
            foreach (Label label in Variable.LabelArray)
            {
                //Variable.OutputFile.WriteLine("Start: " + Variable.LabelToString[label]);
                IList<double> Pdatas = new List<double>();
                Initialize(label);
                for (int time = 1; time <= Variable.ConvergeTimeThreshold; ++time)
                {
                    //计算Pk
                    NoGroupIDSVariable.Pj = CoreFunction.CalculatePj(NoGroupIDSVariable.Sij, time);
                    //计算π
                    NoGroupIDSVariable.PAkjl = CoreFunction.CalculatePAkjl(new Label[] { label }, NoGroupIDSVariable.Sij, time, -1);
                    //计算Sij
                    //if (CoreFunction.CalculatePdataAndSij(new Label[] { label }, ref NoGroupIDSVariable.Sij, NoGroupIDSVariable.Pj, NoGroupIDSVariable.Pajl, NoGroupIDSVariable.Mcj, ref NoGroupIDSVariable.Pdata, -1, Pdatas,
                    //    new Dictionary<Tuple<Labelset, Labelset>, double>(), new Dictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>>()))
                    //{
                    //    ObtainLabelResult(label);
                    //    break;
                    //}
                }
            }
            OutputResult();
        }
        //初始化
        static private void Initialize(Label label)
        {
            NoGroupIDSVariable.Sij = new Sij(1);
            NoGroupIDSVariable.Pj = new Pj(0);
            NoGroupIDSVariable.PAkjl = new PAkjl(0);
            NoGroupIDSVariable.Pdata = new Pdata(0, 0);
            NoGroupIDSVariable.Mcj = new Mcj(0);
            //Dictionary<句子，Dictionary<标签，次数>> 句子i被标为j的次数，用于计算Sij
            double[,] Nil = new double[Variable.Sentences.Count, 2];
            //初始化NilOfK
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (Annotator annotator in Variable.Annotators)
                {
                    foreach (Annotation annotation in Variable.Data[annotator][sentence])
                    {
                        if (annotation.Labels[label])
                            ++Nil[sentence.ID, 1];
                        else
                            ++Nil[sentence.ID, 0];
                    }
                }
            }
            //计算初始Sij
            foreach(Sentence sentence in Variable.Sentences)
            {
                for (int j = 0; j < 2; ++j)
                {
                    NoGroupIDSVariable.Sij.Value[sentence][new Labelset(label, Convert.ToBoolean(j))] = Nil[sentence.ID, j] / Variable.NumberOfAnnotationsPerSentenceAfterGrouping;
                }
            }
            //Variable.OutputFile.WriteLine(IndependentVariable.Sij.ToString(2));
        }
        
        //得到一种情感结果
        static private void ObtainLabelResult(Label label)
        {
            foreach(Sentence sentence in Variable.Sentences)
            {
                if (NoGroupIDSVariable.Sij.Value[sentence][new Labelset(label, true)] > NoGroupIDSVariable.Sij.Value[sentence][new Labelset(label, false)])
                {
                    sentence.IndependentResult.Labels[label] = true;
                }
            }
        }
        //得到最终结果
        static public void OutputResult()
        {
            StreamWriter resultFile = new StreamWriter("Result/IndependentResult.csv");
            Function.InitialResultFile(resultFile);
            for (int i = 0; i < Variable.Sentences.Count; ++i)
            {
                Function.WriteBinaryResultOfASentence(i, Variable.Sentences[i].IndependentResult, Variable.Sentences[i].Character.ID, Variable.Sentences[i].Speech, resultFile);
            }
            resultFile.Close();
        }
    }
}