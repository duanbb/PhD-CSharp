using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Extreme.Mathematics.LinearAlgebra;
using Extreme.Mathematics;
using CenterSpace.NMath.Core;
using CenterSpace.NMath.Matrix;

namespace Test
{
    enum e { a, b, c };
    class Program
    {
        static void Main(string[] args)
        {
            PresentValue();
        }

        static void SpotRate()
        {
            double P2 = 100, F2 = 100;
            double C2 = 20;
            double s1 = 0.1;
            double s2 = Math.Pow((C2 + F2) / (P2 - (C2 / (1 + s1))), 0.5) - 1;
            FutureValue(s2);
        }

        static void FutureValue(double lambda)
        {
            double P = 100;
            double F1 = P * (1 + lambda);
            double F2 = F1 * (1 + lambda);
        }

        static void SimplePresentValue()
        {
            double F = 121;
            double lambda = 0.1;
            double P2 = F / (1 + lambda);
            double P1 = P2 / (1 + lambda);
        }

        static void PresentValue()
        {
            double F = 100;
            double yield = 0.0092;
            double C = 0.92;

            double ValueOfFace = F / Math.Pow(1 + yield / 2, 8);

            double ValueOfYield = 0;
            for (int i = 1; i <= 8; ++i)
            {
                double value = (C / 2) / Math.Pow(1 + yield / 2, i);
                ValueOfYield += value;
            }

            double AccruedInterest = 59.0 / (59 + 122) * 0.92;



            double Value = ValueOfFace + ValueOfYield + AccruedInterest; 
        }

        static void ConditionalProbability()
        {
            double P000 = 0.1;
            double P001 = 0.1;
            double P010 = 0.1;
            double P011 = 0.1;
            double P100 = 0.1;
            double P101 = 0.2;
            double P110 = 0.1;
            double P111 = 0.2;

            double P0 = P000 * P001 * P010 * P011;
            double P1 = P100 * P101 * P110 * P111;
        }

        static void NMathLeastSquares()
        {
            DoubleMatrix a = new DoubleMatrix(3, 2, new double[]
                {
                    1, -1, -1,
                    1, 1, -1,
                }, StorageType.ColumnMajor);

            var cholLsq = new DoubleCholeskyLeastSq(a);

            DoubleVector b = new DoubleVector(new double[] { -1, -1, 1 });

            DoubleVector solution = cholLsq.Solve(b);

            double[] result = solution.ToArray();
        }
        
        static void ExtremeLeastSquares()
        {
            //DenseMatrix a = Matrix.Create(6, 2, new double[]
            //    {
            //        1, 2, 1,1, 2, 1,
            //        1, 1, 2,1, 1, 2,
            //    }, MatrixElementOrder.ColumnMajor);
            //DenseVector b = Vector.Create(1, -2, 4, 5, 8, 6);

            DenseMatrix a = Matrix.Create(3, 2, new double[]
                {
                    1, 2, 1,
                    1, 1, 1,
                }, MatrixElementOrder.ColumnMajor);
            DenseVector b = Vector.Create(1, 2, 3);

            LeastSquaresSolver solver = new LeastSquaresSolver(a, b);
            solver.SolutionMethod = LeastSquaresSolutionMethod.NormalEquations;
            //Vector x = solver.Solve();

            double[] result = solver.Solution.ToArray();

            //DenseMatrix m = Matrix.Create(2, 2, new double[]
            //    {
            //        1, 1, 
            //        1, 1, 
            //    }, MatrixElementOrder.ColumnMajor);
            //DenseVector b1 = Vector.Create(1, 3);
            //Vector x1 = m.Solve(b1, false);
        }

        static string enumtostring()
        {
            e ee = e.a;
            return ee.ToString();
        }

        static void Boxing()
        {
            object o;
            int[] s = { 1, 2, 3, 4, 5 };
            o = s;
            foreach (int i in (int[])o)
            {
                int j = i;
            }
        }

        static void StandardDeviation()
        {
            double[] array = { -0.7, -1.3, 1.3, -0.7, 0.7, 0.7, -1.3, 1.3 };
            //double even = 0;
            //foreach(double a in array)
            //{
            //    even += a;
            //}

            double deviation = 0;
            foreach (double a in array)
            {
                deviation += Math.Pow(a, 2);
            }
            deviation = Math.Pow(deviation / 9, 0.5);
        }

        static void TupleAsKey()
        {
            IDictionary<Tuple<int, int>, int> a = new Dictionary<Tuple<int, int>, int>();
            a.Add(new Tuple<int, int>(1, 1), 100);
            int i = ++a[new Tuple<int, int>(1, 1)];
            int j = --a[Tuple.Create(1, 1)];
        }

        static void BeepA()
        {
            Console.Beep(440, int.MaxValue);
        }

        static void DivideZero()
        { 
            double j = 0;
            double i = 0/j;
        }

        static void DicExistKey()
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            dic["s"] += 2;
        }

        static void ForeachChangeDic()
        {
            IDictionary<int, double> dic = new Dictionary<int, double>();
            dic.Add(1, 0.1);
            dic.Add(2, 0.2);
            foreach (int i in dic.Keys.ToArray())
            {
                dic[i] = 0.5;
            }
        }

        static void TupleTest()
        {
            //Tuple<string, string> a = new Tuple<string, string>("s", "s");
            //IDictionary<Tuple<string, string>, int> b = new Dictionary<Tuple<string, string>, int>();
            //b.Add(a, 11);
            //Tuple<string, string> c = new Tuple<string, string>("s", "s");
            //int m = b[c];

            Dictionary<Tuple<A, A>, int> dic = new Dictionary<Tuple<A, A>, int>();
            A a1 = new A(1);
            A a2 = new A(2);
            A a3 = new A(1);
            A a4 = new A(2);
            dic.Add(Tuple.Create(a1, a2), 5);
            dic.Add(Tuple.Create(a3, a4), 6);

            //Dictionary<Tuple<A>, int> dic = new Dictionary<Tuple<A>, int>();
            //for (int i = 0; i < 10; ++i)
            //{
            //    A a1 = new A(1);
            //    dic.Add(Tuple.Create(a1), 5);
            //}
        }

        static void HashCode()
        {
            string s = "dddddddddddddddddddddddddddddddddddddddddddddddddddddddddd";
            string a = "y";
            string b = "y";
            string c = "dddddddddddddddddddddddddddddddddddddddddddddddddddddddddd";
            int Hashc = c.GetHashCode();
            int Hasha = a.GetHashCode();
            int Hashb = b.GetHashCode();
            int Hashs = s.GetHashCode();
        }

        static void DataOfZhang()
        {
            string[] DS1File = File.ReadAllLines("DS1.csv");
            string[] DS2File = File.ReadAllLines("DS2.csv");
            string[] MV1File = File.ReadAllLines("MV1.csv");
            string[] MV2File = File.ReadAllLines("MV2.csv");
            Dictionary<string, string[]> DS1 = new Dictionary<string, string[]>();
            Dictionary<string, string[]> DS2 = new Dictionary<string, string[]>();
            Dictionary<string, string[]> MV1 = new Dictionary<string, string[]>();
            Dictionary<string, string[]> MV2 = new Dictionary<string, string[]>();
            GenerateData(DS1, DS1File, 0);
            GenerateData(DS2, DS2File, 0);
            GenerateData(MV1, MV1File, 0.6);
            GenerateData(MV2, MV2File, 0.6);
            CalculateAccuracy(MV1, DS1);
            CalculateAccuracy(MV2, DS2);

            Console.Write("Finished. Press any key..."); Console.Read();
        }

        static private void CalculateAccuracy(Dictionary<string, string[]> MV, Dictionary<string, string[]> DS)
        {
            double numberOfRightTweets = 0;
            foreach (KeyValuePair<string, string[]> mv in MV)
            {
                bool isDifferent = false;
                for (int i = 0; i < 4; ++i)
                {
                    if (mv.Value[i] != DS[mv.Key][i])
                    {
                        isDifferent = true;
                        break;
                    }
                    //if(i==3)
                    //    ++numberOfRightTweets;
                }
                if (!isDifferent)
                    ++numberOfRightTweets;
            }
            Console.WriteLine("Accuracy: " + numberOfRightTweets + "/" + MV.Count + " = " + numberOfRightTweets / MV.Count);
        }

        static private void GenerateData(Dictionary<string, string[]> data, string[] file, double threshold)
        {
            foreach (string datum in file)
            {
                if (Convert.ToDouble(datum.Split(',')[6]) >= threshold)
                {
                    string sentence = datum.Split(',')[0];
                    data.Add(sentence, new string[4]);
                    data[sentence][0] = datum.Split(',')[1];
                    data[sentence][1] = datum.Split(',')[2];
                    data[sentence][2] = datum.Split(',')[3];
                    data[sentence][3] = datum.Split(',')[4];
                }
            }
        }

        static void Assumption()
        {
            //int[] X1 = new int[] { 1, 1, 2, 2, 2, 2, 1, 2 };
            //int[] X2 = new int[] { 2, 1, 1, 2, 1, 1, 2, 2 };
            int[] X1 = new int[] { 1, 1, 1, 1, 2, 2, 2, 2 };
            int[] X2 = new int[] { 1, 1, 1, 1, 2, 2, 2, 2 };
            int numberOfX1Is1 = 0;
            int numberOfX1Is2 = 0;
            int numberOfX2Is1 = 0;
            int numberOfX2Is2 = 0;
            int numberOfX1Is1X2Is1 = 0;
            int numberOfX1Is2X2Is1 = 0;
            int numberOfX1Is1X2Is2 = 0;
            int numberOfX1Is2X2Is2 = 0;
            double N = Convert.ToDouble(X1.Length);
            for (int i = 0; i < X1.Length; ++i)
            {
                if (X1[i] == 1)
                {
                    ++numberOfX1Is1;
                    if (X2[i] == 1)
                    {
                        ++numberOfX2Is1;
                        ++numberOfX1Is1X2Is1;
                    }
                    else
                    {
                        ++numberOfX2Is2;
                        ++numberOfX1Is1X2Is2;
                    }
                }
                else
                {
                    ++numberOfX1Is2;
                    if (X2[i] == 1)
                    {
                        ++numberOfX2Is1;
                        ++numberOfX1Is2X2Is1;
                    }
                    else
                    {
                        ++numberOfX2Is2;
                        ++numberOfX1Is2X2Is2;
                    }
                }
            }
            double MI = (numberOfX1Is1X2Is1 / N) * Math.Log((numberOfX1Is1X2Is1 * N) / (numberOfX1Is1 * numberOfX2Is1), 2)
                //+ (numberOfX1Is1X2Is2 / N) * Math.Log((numberOfX1Is1X2Is2 * N) / (numberOfX1Is1 * numberOfX2Is2), 2)
                //+ (numberOfX1Is2X2Is1 / N) * Math.Log((numberOfX1Is2X2Is1 * N) / (numberOfX1Is2 * numberOfX2Is1), 2)
                + (numberOfX1Is2X2Is2 / N) * Math.Log((numberOfX1Is2X2Is2 * N) / (numberOfX1Is2 * numberOfX2Is2), 2);
            double X = SpecialFunction.chisqc(1, 2 * N * MI);//（右尾面积）越小越有联系（不独立），越大越独立
        }
    }
}