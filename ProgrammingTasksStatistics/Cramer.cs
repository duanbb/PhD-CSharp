using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgrammingTasksStatistics
{
    static class Cramer
    {
        public static void Run()
        {
            //书上的例子
            IList<int[]> observedArrays = new List<int[]>();
            //原例
            //observedArrays.Add(new int[] { 34, 61, 53 });
            //observedArrays.Add(new int[] { 38, 40, 74 });
            //行列互换
            //observedArrays.Add(new int[] { 34, 38 });
            //observedArrays.Add(new int[] { 61, 40 });
            //observedArrays.Add(new int[] { 53, 74 });

            //正しいか & 難しさ（3.1の表で）
            //observedArrays.Add(new int[] { 3, 1, 4 });
            //observedArrays.Add(new int[] { 2, 0, 0 });

            //observedArrays.Add(new int[] { 3, 2 });
            //observedArrays.Add(new int[] { 1, 0 });
            //observedArrays.Add(new int[] { 4, 0 });

            //正しいか & 自信（3.2の表で）
            //observedArrays.Add(new int[] { 2, 3, 3 });
            //observedArrays.Add(new int[] { 0, 0, 2 });

            //難しさ & 自信（3.3の表で）
            observedArrays.Add(new int[] { 0, 0, 2 });
            observedArrays.Add(new int[] { 1, 1, 1 });
            observedArrays.Add(new int[] { 4, 0, 1 });

            //计算总数
            double count = 0;
            foreach (int[] array in observedArrays)
            {
                foreach (int datum in array)
                {
                    count += datum;
                }
            }

            //计算各行与各列的合计
            double[] rowCounts = new double[observedArrays.Count];
            double[] columnCounts = new double[observedArrays.ElementAt(0).Length];
            for (int i = 0; i < rowCounts.Length; ++i)
            {
                int innerCount = 0;//组内合计
                for (int j = 0; j < columnCounts.Length; ++j)
                {
                    innerCount += observedArrays.ElementAt(i)[j];
                    columnCounts[j] += observedArrays.ElementAt(i)[j];
                }
                rowCounts[i] = innerCount;
            }

            //生成期望次数数组
            double[][] expectationArrays = new double[rowCounts.Length][];
            for (int i = 0; i < rowCounts.Length; ++i)
            {
                expectationArrays[i] = new double[columnCounts.Length];
                for (int j = 0; j < columnCounts.Length; ++j)
                {
                    expectationArrays[i][j] = rowCounts.ElementAt(i) * columnCounts[j] / count;
                }
            }

            //{(观测次数-期望次数)^2 / 期望次数}之和，为卡方
            double chi_square = 0;
            for (int i = 0; i < rowCounts.Length; ++i)
            {
                for (int j = 0; j < columnCounts.Length; ++j)
                {
                    chi_square += Math.Pow(observedArrays.ElementAt(i)[j] - expectationArrays[i][j], 2) / expectationArrays[i][j];
                }
            }

            double cramer = Math.Pow(chi_square / (count * ((rowCounts.Length < columnCounts.Length ? rowCounts.Length : columnCounts.Length) - 1)), 0.5);
        }
    }
}