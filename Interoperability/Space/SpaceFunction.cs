using Interoperability.Entity;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Interoperability.Space
{
    using CenterSpace.NMath.Core;
    using CenterSpace.NMath.Matrix;
    using Extreme.Mathematics;
    using Extreme.Mathematics.LinearAlgebra;

    static class SpaceFunction
    {
        static public void OutputEstimatedRealTargetAnnotations(Method method)
        {
            StreamWriter File = new StreamWriter("Output/" + Constant.Gold + "/" + TrainConstant.Corpus + "_" + Constant.SourceTaxonomy.Name + "/" + method + "/" + method + "_RealTargetAnnotations.csv", false, Encoding.Default);
            File.Write("Sentence,");
            foreach (Label label in Constant.TargetTaxonomy.LabelArray)
            {
                File.Write(label + ",");
            }
            File.WriteLine();

            foreach (Sentence sentence in Constant.SentenceList)
            {
                File.Write(sentence.ToString() + ",");
                foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                {
                    File.Write(sentence.SpaceRealTargetLabelDic[label] + ",");
                }
                File.WriteLine();
            }
            File.Close();
        }

        static public void OutputGoldRealTargetAnnotations()
        {
            StreamWriter File = new StreamWriter("Output/" + Constant.Gold + "/" + TrainConstant.Corpus + "_" + Constant.SourceTaxonomy.Name + "/GoldRealTargetAnnotationsForSpace.csv", false, Encoding.Default);
            File.Write("Sentence,");
            foreach (Label label in Constant.TargetTaxonomy.LabelArray)
            {
                File.Write(label + ",");
            }
            File.WriteLine();

            foreach (Sentence sentence in Constant.SentenceList)
            {
                File.Write(sentence.ToString() + ",");
                foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                {
                    if (sentence.SortedTargetLabelDic.ContainsKey(label))
                        File.Write(((2 * sentence.SortedTargetLabelDic[label] - sentence.TargetWorkerTargetAnnotationDic.Count) / sentence.TargetWorkerTargetAnnotationDic.Count) + ",");//被标注的比例
                    else
                        File.Write(-1 + ",");
                }
                File.WriteLine();
            }
            File.Close();
        }

        static double[] weightArray(Method method)
        {
            //建立weights
            double[] result = new double[TrainConstant.SentenceList.Count * SpaceConstant.TargetWorkerNumberPerSentence];
            int indexOfSentence = 0;
            foreach (Sentence sentence in TrainConstant.SentenceList)
            {
                int indexOfWorker = 0;
                foreach (TargetWorker targetWorker in sentence.TargetWorkerTargetAnnotationDic.Keys)
                {
                    switch (method)
                    {
                        case Method.WeightedCombination:
                            result[TrainConstant.SentenceList.Count * indexOfWorker + indexOfSentence] = sentence.OtherNonormalizeWeightDic[targetWorker];
                            break;
                        case Method.TemporaryNogeneralNonormalize:
                            result[TrainConstant.SentenceList.Count * indexOfWorker + indexOfSentence] = sentence.TemporaryNonormalizeWeightDic[targetWorker];
                            break;
                        case Method.TemporaryNogeneralNormalize:
                            result[TrainConstant.SentenceList.Count * indexOfWorker + indexOfSentence] = sentence.TemporaryNormalizeWeightDic[targetWorker];
                            break;
                        case Method.TemporaryGeneralNonormalize:
                            result[TrainConstant.SentenceList.Count * indexOfWorker + indexOfSentence] = targetWorker.TemporaryNonormalizeWeight;
                            break;
                        case Method.TemporaryGeneralNormalize:
                            result[TrainConstant.SentenceList.Count * indexOfWorker + indexOfSentence] = targetWorker.TemporaryNormalizeWeight;
                            break;
                        case Method.OtherNogeneralNormalize:
                            result[TrainConstant.SentenceList.Count * indexOfWorker + indexOfSentence] = sentence.OtherNormalizeWeightDic[targetWorker];
                            break;
                        case Method.OtherGeneralNonormalize:
                            result[TrainConstant.SentenceList.Count * indexOfWorker + indexOfSentence] = targetWorker.OtherNonormalizeWeight;
                            break;
                        case Method.OtherGeneralNormalize:
                            result[TrainConstant.SentenceList.Count * indexOfWorker + indexOfSentence] = targetWorker.OtherNormalizeWeight;
                            break;
                    }
                    ++indexOfSentence;
                }
                ++indexOfWorker;
            }

            #region 输出到文件
            //StreamWriter file = new StreamWriter("Output/"  + Constant.Gold + "/" + TrainConstant.Corpus + "_" + Constant.SourceTaxonomy.Name + "_" + method + "_Weights.csv", false, Encoding.Default);
            //file.Write(doubleArrayToString(result));
            //file.Close();
            #endregion

            return result;
        }

        static public void AggregatedMatrix()
        {
            SpaceConstant.Matrix = new double[Constant.TargetTaxonomy.LabelArray.Length][];

            //生成系数矩阵
            double[] coefficients = new double[TrainConstant.SentenceList.Count * Constant.SourceTaxonomy.LabelArray.Length];
            int indexOfSentence = 0;
            foreach (Sentence sentence in TrainConstant.SentenceList)
            {
                double[] goldSourceArray = sentence.GoldSourceAnnotation.ToDoubleArray;
                //建立coefficients
                for (int i = 0; i < Constant.SourceTaxonomy.LabelArray.Length; ++i )
                {
                    coefficients[TrainConstant.SentenceList.Count * i + indexOfSentence] = goldSourceArray[i];
                }
                ++indexOfSentence;
            }


            int indexOfTargetLabel = 0;
            foreach (Label targetLabel in Constant.TargetTaxonomy.LabelArray)
            {
                //生成常数项
                double[] constants = new double[TrainConstant.SentenceList.Count];
                indexOfSentence = 0;
                foreach (Sentence sentence in TrainConstant.SentenceList)
                {
                    //2014.11.11PPT11，一行一行建，同sentence聚。（PPT为同worker聚）
                    //建立constants
                    constants[indexOfSentence] = sentence.GoldTargetAnnotation.LabelAndTruthDic[targetLabel] ? 1 : -1;
                    ++indexOfSentence;
                }

                //计算最小二乘解
                SpaceConstant.Matrix[indexOfTargetLabel] = OrdinaryLeastSquares(TrainConstant.SentenceList.Count, Constant.SourceTaxonomy.LabelArray.Length, coefficients, constants);
                ++indexOfTargetLabel;
            }
        }

        /// <summary>
        /// 生成转移矩阵，同时计算各worker的ExpertiseVector。
        /// </summary>
        /// <returns>转移矩阵</returns>
        static public void ExpertiseTransformationMatrix()
        {
            SpaceConstant.Matrix = new double[Constant.TargetTaxonomy.LabelArray.Length][];
            double[] coefficients = coefficientArrayForCombination();

            int indexOfTargetLabel = 0;
            //indexOfTargetLabel表示正在算matrix的哪一行
            foreach (Label targetLabel in Constant.TargetTaxonomy.LabelArray)
            {
                double[] constants = new double[TrainConstant.SentenceList.Count * SpaceConstant.TargetWorkerNumberPerSentence];
                int index = 0;
                foreach (Sentence sentence in TrainConstant.SentenceList)
                {
                    foreach (TargetWorker worker in sentence.TargetWorkerTargetAnnotationDic.Keys)
                    {
                        constants[index] = GeneralFunction.ProductOfTwoVectors(sentence.TargetWorkerTargetAnnotationDic[worker].ToDoubleArray, worker.ExpertiseMatrixList.Last()[indexOfTargetLabel]);
                        ++index;
                    }
                }
                SpaceConstant.Matrix[indexOfTargetLabel] = OrdinaryLeastSquares(TrainConstant.SentenceList.Count * SpaceConstant.TargetWorkerNumberPerSentence, Constant.SourceTaxonomy.LabelArray.Length, coefficients, constants);

                ++indexOfTargetLabel;
            }
        }

        /// <summary>
        /// 生成常数项。（方程组等号右边）
        /// </summary>
        /// <param name="targetLabel">所针对的Traget Label。</param>
        /// <returns>方程组的常数项。</returns>
        static double[] constantArrayForCombination(Label targetLabel)
        {
            double[] result = new double[TrainConstant.SentenceList.Count * SpaceConstant.TargetWorkerNumberPerSentence];
            int index = 0;
            foreach (Sentence sentence in TrainConstant.SentenceList)
            {
                foreach (TargetWorker worker in sentence.TargetWorkerTargetAnnotationDic.Keys)
                {
                    //2014.11.11PPT11，一行一行建，同sentence聚。（PPT为同worker聚）
                    //建立constants
                    result[index] = sentence.TargetWorkerTargetAnnotationDic[worker].LabelAndTruthDic[targetLabel] ? 1 : -1;
                    ++index;
                }
            }

            #region 输出到文件
            //StreamWriter file = new StreamWriter("Output/" + Constant.Gold + "/"  + TrainConstant.Corpus + "_" + Constant.SourceTaxonomy.Name + "_" + method + "_" + targetLabel + "_Constants.csv", false, Encoding.Default);
            //file.Write(doubleArrayToString(result));
            //file.Close();
            #endregion

            return result;
        }
        
        /// <summary>
        /// 生成方程组的系数。
        /// </summary>
        /// <returns>方程组的系数。</returns>
        static double[] coefficientArrayForCombination()
        {
            double[] result = new double[TrainConstant.SentenceList.Count * SpaceConstant.TargetWorkerNumberPerSentence * Constant.SourceTaxonomy.LabelArray.Length];
            int index = 0;
            foreach (Sentence sentence in TrainConstant.SentenceList)
            {
                double[] goldSourceArray = sentence.GoldSourceAnnotation.ToDoubleArray;
                foreach (TargetWorker worker in sentence.TargetWorkerTargetAnnotationDic.Keys)
                {
                    //2014.11.11PPT11，一行一行建，同sentence聚。（PPT为同worker聚）
                    //建立coefficients
                    for (int i = 0; i < Constant.SourceTaxonomy.LabelArray.Length; ++i)
                    {
                        result[index + TrainConstant.SentenceList.Count * SpaceConstant.TargetWorkerNumberPerSentence * i] = goldSourceArray[i];
                    }
                    ++index;
                }
            }

            #region 输出到文件
            //StreamWriter file = new StreamWriter("Output/" + Constant.Gold + "/"  + TrainConstant.Corpus + "_" + Constant.SourceTaxonomy.Name + "_" + method + "_Coefficients.csv", false, Encoding.Default);
            //file.Write(doubleArrayToString(result));
            //file.Close();
            #endregion

            return result;
        }

        /// <summary>
        /// 生成转移矩阵。
        /// Ordinary方法（2014.11.11 PPT11页）。
        /// 只用到了TrainConstant里的成员。
        /// </summary>
        /// <returns>转移矩阵</returns>
        static public void OrdinaryMatrix()
        {
            SpaceConstant.Matrix = new double[Constant.TargetTaxonomy.LabelArray.Length][];
            double[] coefficients = coefficientArrayForCombination();
            int indexOfTargetLabel = 0;
            foreach (Label targetLabel in Constant.TargetTaxonomy.LabelArray)
            {
                double[] constants = constantArrayForCombination(targetLabel);

                SpaceConstant.Matrix[indexOfTargetLabel] = OrdinaryLeastSquares(TrainConstant.SentenceList.Count * SpaceConstant.TargetWorkerNumberPerSentence, Constant.SourceTaxonomy.LabelArray.Length, coefficients, constants);
                ++indexOfTargetLabel;
            }
        }

        static public void WeightedMatrix(Method method)
        {
            SpaceConstant.Matrix = new double[Constant.TargetTaxonomy.LabelArray.Length][];
            double[] coefficients = coefficientArrayForCombination();

            double[] weights = weightArray(method);
            int indexOfTargetLabel = 0;
            foreach (Label targetLabel in Constant.TargetTaxonomy.LabelArray)
            {
                double[] constants = constantArrayForCombination(targetLabel);

                SpaceConstant.Matrix[indexOfTargetLabel] = weightedLeastSquares(TrainConstant.SentenceList.Count * SpaceConstant.TargetWorkerNumberPerSentence, Constant.SourceTaxonomy.LabelArray.Length, coefficients, constants, weights);
                ++indexOfTargetLabel;
            }
        }

        static private double[] weightedLeastSquares(int rowCount, int columnCount, double[] coefficients, double[] constants, double[] weights)
        {
            DoubleMatrix a = new DoubleMatrix(rowCount, columnCount, coefficients, StorageType.ColumnMajor);
            DoubleVector b = new DoubleVector(constants);
            DoubleVector w = new DoubleVector(weights);

            DoubleCOWeightedLeastSq solver = new DoubleCOWeightedLeastSq(a, w);
            DoubleVector solution = solver.Solve(b);

            return solution.ToArray();
        }

        static public void OutputMatrix(Method method)
        {
            StreamWriter File = new StreamWriter("Output/" + Constant.Gold + "/" + TrainConstant.Corpus + "_" + Constant.SourceTaxonomy.Name + "/" + method + "/" + method + "_TransformationMatrix.csv", false, Encoding.Default);
            File.Write(",");
            foreach (Label label in Constant.SourceTaxonomy.LabelArray)
            {
                File.Write(label + ",");
            }
            File.WriteLine();

            int indexOfRow = 0;
            foreach (double[] row in SpaceConstant.Matrix)
            {
                File.Write(Constant.TargetTaxonomy.LabelArray[indexOfRow] + ",");
                foreach (double cell in row)
                {
                    File.Write(cell + ",");
                }
                File.WriteLine();
                ++indexOfRow;
            }
            File.Close();
        }

        static private string doubleArrayToString(double[] doubleArray)
        {
            string result = string.Empty;
            for (int i = 0; i < doubleArray.Length; ++i)
            {
                result += i + "|" + doubleArray[i] + ',';
                if (i % 20 == 19 && i != 0)
                    result += "\r\n";
            }
            return result;
        }

        /// <summary>
        /// 求无加权最小二乘解。
        /// </summary>
        /// <param name="rowCount">行数（方程数）。</param>
        /// <param name="columnCount">列数（未知数数）。</param>
        /// <param name="coefficients">系数矩阵（用向量表示）。</param>
        /// <param name="constants">常数项向量。</param>
        /// <returns>无加权最小二乘解。</returns>
        static public double[] OrdinaryLeastSquares(int rowCount, int columnCount, double[] coefficients, double[] constants)
        {
            DenseMatrix a = Matrix.Create(rowCount, columnCount, coefficients, MatrixElementOrder.ColumnMajor);
            DenseVector b = Vector.Create(constants);

            LeastSquaresSolver solver = new LeastSquaresSolver(a, b);
            try
            {
                solver.SolutionMethod = LeastSquaresSolutionMethod.NormalEquations;
                return solver.Solution.ToArray();
            }
            catch
            {
                solver.SolutionMethod = LeastSquaresSolutionMethod.SingularValueDecomposition;
                return solver.Solution.ToArray();
            }

            //#region 输出到文件
            //StreamWriter coefficientFile = new StreamWriter("Output/ + Constant.Gold + "/" + Coefficients.csv", false, Encoding.Default);
            //coefficientFile.Write(doubleArrayToString(coefficients));
            //coefficientFile.Close();

            //StreamWriter constantFile = new StreamWriter("Output/ + Constant.Gold + "/" + Constants.csv", false, Encoding.Default);
            //constantFile.Write(doubleArrayToString(constants));
            //constantFile.Close();
            //#endregion
        }

        static public void RealTargetAnnotations()
        {
            foreach (Sentence sentence in Constant.SentenceList)
            {
                sentence.SpaceRealTargetLabelDic = new Dictionary<Label, double>();
                double[] doubleArray = sentence.GoldSourceAnnotation.ToDoubleArray;//要乘的向量（由SourceAnnotation转换）
                int targetLabelIndex = 0;
                foreach (double[] row in SpaceConstant.Matrix)
                {
                    Label targetLabel = Constant.TargetTaxonomy.LabelArray[targetLabelIndex];

                    double product = 0;
                    for (int i = 0; i < row.Length; ++i)
                    {
                        product += row[i] * doubleArray[i];
                    }
                    sentence.SpaceRealTargetLabelDic.Add(targetLabel, product);
                    ++targetLabelIndex;
                }
            }
        }
    }
}