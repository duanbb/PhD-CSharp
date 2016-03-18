using System;
using System.Collections.Generic;
using System.IO;

namespace Intelligence
{
    class Program
    {
        static void Main(string[] args)
        {
            double[] x = new double[1000];
            for (int i = 0; i < x.Length; ++i)
            {
                x[i] = i / (double)x.Length;
            }
            double ThetaStar = Math.Log(2, Math.E);
            double Lambda = -0.01;
            double[] Theta = new double[x.Length];
            Theta[0] = Math.Log(2, 10);
            StreamWriter FileTheta = new StreamWriter("Theta.csv");
            FileTheta.WriteLine(Theta[0]);
            StreamWriter FileE = new StreamWriter("E.csv");
            #region(1)
            //for (int t = 1; t < x.Length; ++t)
            //{
            //    int DeltaT = Math.Sign(ThetaStar - x[t]);
            //    int DeltaS = Math.Sign(Theta[t - 1] - x[t]);
            //    double newTheta = Theta[t - 1] - Lambda * DeltaT * (-DeltaT * DeltaS > 0 ? -1 : 0);
            //    Theta[t] = newTheta;
            //    FileTheta.WriteLine(newTheta);
            //    FileE.WriteLine(Math.Pow(ThetaStar - newTheta, 2));
            //}
            #endregion
            #region(2)
            double LambdaZero = 0.1;
            double a = 0.001;
            for (int t = 1; t < x.Length; ++t)
            {
                Lambda = LambdaZero * Math.Pow(Math.E, -a * t);
                int DeltaT = Math.Sign(ThetaStar - x[t]);
                int DeltaS = Math.Sign(Theta[t - 1] - x[t]);
                double newTheta = Theta[t - 1] - Lambda * DeltaT * (-DeltaT * DeltaS > 0 ? -1 : 0);
                Theta[t] = newTheta;
                FileTheta.WriteLine(newTheta);
                FileE.WriteLine(Math.Pow(ThetaStar - newTheta, 2));
            }
            #endregion
            FileTheta.Close();
            FileE.Close();
        }
    }
}