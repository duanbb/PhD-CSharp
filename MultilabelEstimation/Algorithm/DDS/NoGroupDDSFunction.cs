using MultilabelEstimation.Group;
using System;
using System.Collections.Generic;
using System.IO;

namespace MultilabelEstimation.Algorithm.DDS
{
    class NoGroupDDSFunction
    {
        static public void CalculatePAkjl(Sij sij, ref PAkjl pakjl)
        {
            pakjl = new PAkjl(++pakjl.Time);
            IDictionary<Annotator, double[,]> numerator = new Dictionary<Annotator, double[,]>();//分子
            IDictionary<Annotator, double[]> denominator = new Dictionary<Annotator, double[]>();//分母
            foreach (Annotator annotator in Variable.Annotators)//人
            {
                numerator.Add(annotator, new double[(int)Math.Pow(2, Variable.LabelArray.Length), (int)Math.Pow(2, Variable.LabelArray.Length)]);
                denominator.Add(annotator, new double[(int)Math.Pow(2, Variable.LabelArray.Length)]);
            }
            //计算分子分母
            foreach (Annotator annotator in Variable.Annotators)
            {
                for (int j = 0; j < Math.Pow(2, Variable.LabelArray.Length); ++j)//正确标签
                {
                    foreach (Sentence sentence in Variable.Sentences)
                    {
                        foreach (Annotation annotation in Variable.Data[annotator][sentence])
                        {
                            numerator[annotator][j, annotation.IntLabel] += sij.Value[sentence][new Labelset(Variable.LabelArray, j)];
                            denominator[annotator][j] += sij.Value[sentence][new Labelset(Variable.LabelArray, j)];
                        }
                    }
                }
            }
            //计算π
            foreach (Annotator annotator in Variable.Annotators)//人
            {
                for (int j = 0; j < (int)Math.Pow(2, Variable.LabelArray.Length); ++j)//正确标签
                {
                    if (denominator[annotator][j] != 0)//某些结果就是在所有句子中都没出现过
                    {
                        for (int l = 0; l < (int)Math.Pow(2, Variable.LabelArray.Length); ++l)//人标的标签
                        {
                            pakjl.Value[annotator][new Labelset(Variable.LabelArray, j)][new Labelset(Variable.LabelArray, l)] = numerator[annotator][j, l] / denominator[annotator][j];
                        }
                    }
                }
            }
            //Variable.OutputFile.WriteLine(pajl.ToString(DependentVariable.NumberOfIntlabel));
        }

        static public bool CalculatePdataAndSij(ref Sij sij, Pj pj, PAkjl pakjl, ref Pdata pdata)
        {
            bool isFinished = false;
            pdata = new Pdata(++pdata.Time, pdata.Value);
            sij = new Sij(++sij.Time);
            double[,] numerator = new double[Variable.Sentences.Count, (int)Math.Pow(2, Variable.LabelArray.Length)];
            for (int i = 0; i < Variable.Sentences.Count; ++i)
            {
                for (int j = 0; j < Math.Pow(2, Variable.LabelArray.Length); ++j)
                    numerator[i, j] = 1;
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                for (int j = 0; j < Math.Pow(2, Variable.LabelArray.Length); ++j)//正确标签
                {
                    foreach (Annotator annotator in Variable.Annotators)//人
                    {
                        foreach (Annotation annotation in Variable.Data[annotator][sentence])
                        {
                            numerator[sentence.ID, j] *= pakjl.Value[annotator][new Labelset(Variable.LabelArray, j)][new Labelset(Variable.LabelArray, annotation.IntLabel)];
                        }
                    }
                    numerator[sentence.ID, j] *= pj.Value[new Labelset(Variable.LabelArray, j)];
                }
            }
            double[] denominator = new double[Variable.Sentences.Count];
            for (int i = 0; i < Variable.Sentences.Count; ++i)
            {
                for (int q = 0; q < Math.Pow(2, Variable.LabelArray.Length); ++q)
                {
                    denominator[i] += numerator[i, q];
                }
            }
            //计算Pdata和Sij
            foreach (Sentence sentence in Variable.Sentences)
            {
                for (int j = 0; j < Math.Pow(2, Variable.LabelArray.Length); ++j)
                {
                    sij.Value[sentence][new Labelset(Variable.LabelArray, j)] = numerator[sentence.ID, j] / denominator[sentence.ID];
                }
                pdata.Value += -Math.Log10(denominator[sentence.ID]);
            }
            if (pdata.MondifiedValue == 0 || pdata.Time == 10)
            {
                isFinished = true;
            }
            //Variable.OutputFile.WriteLine(pdata.ToString());
            //Variable.OutputFile.WriteLine(sij.ToString(DependentVariable.NumberOfIntlabel));
            return isFinished;
        }

        static public void ObtainResult(Sij sij, string algorithm)
        {
            StreamWriter resultFile = new StreamWriter("Result/" + algorithm + "Result.csv");
            Function.InitialResultFile(resultFile);
            foreach (Sentence sentence in Variable.Sentences)
            {
                int bestResult = 0;
                double bestResultValue = 0;
                for (int j = 0; j < Math.Pow(2, Variable.LabelArray.Length); ++j)
                {
                    if (sij.Value[sentence][new Labelset(Variable.LabelArray, j)] > bestResultValue)
                    {
                        bestResult = j;
                        bestResultValue = sij.Value[sentence][new Labelset(Variable.LabelArray, j)];
                    }
                }
                switch (algorithm)
                {
                    case "JDDS":
                        sentence.PreciseResult = new Result(new KeyValuePair<Labelset, double>(new Labelset(Variable.LabelArray, bestResult), bestResultValue));
                        Function.WriteBinaryResultOfASentence(sentence.ID, sentence.PreciseResult, sentence.Character.ID, sentence.Speech, resultFile);
                        break;
                    case "TDDS":
                        sentence.TreeForAllResult = new Result(new KeyValuePair<Labelset, double>(new Labelset(Variable.LabelArray, bestResult), bestResultValue));
                        Function.WriteBinaryResultOfASentence(sentence.ID, sentence.TreeForAllResult, sentence.Character.ID, sentence.Speech, resultFile);
                        break;
                    case "DTDDS":
                        sentence.TreeForSenResult = new Result(new KeyValuePair<Labelset, double>(new Labelset(Variable.LabelArray, bestResult), bestResultValue));
                        Function.WriteBinaryResultOfASentence(sentence.ID, sentence.TreeForSenResult, sentence.Character.ID, sentence.Speech, resultFile);
                        break;
                }
                resultFile.WriteLine();
            }
            resultFile.Close();
        }
    }
}