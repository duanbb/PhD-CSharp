using System;
using System.Linq;

namespace ProgrammingTasksStatistics
{
    static class RelevanceRatio
    {
        //数组的平均数
        static double Average(int[] array)
        {
            double sum = 0;
            foreach (int num in array)
            {
                sum += num;
            }
            return sum / array.Length;
        }

        //(value - average)^2，步骤1的一列
        static double VarianceSquare(int[] array)
        {
            double result = 0;
            double average = Average(array);
            foreach (int num in array)
            {
                result += Math.Pow(num - average, 2);
            }
            return result;
        }

        static double GetRelevanceRatio(int[] right, int[] wrong)
        {
            //组内变异
            double VarianceInnerGroup = 0;
            VarianceInnerGroup += VarianceSquare(right);
            VarianceInnerGroup += VarianceSquare(wrong);
            //VarianceInnerGroup += VarianceSquare(other);

            //组间变异
            double VarianceAcrossGroups = 0;
            //求整体平均值
            //int[] all = new int[right.Length + wrong.Length];
            //right.CopyTo(all, 0);
            //wrong.CopyTo(all, right.Length);
            //other.CopyTo(all, right.Length+wrong.Length);
            int[] all = right.Concat(wrong).ToArray();
            double averageForAll = Average(all);
            VarianceAcrossGroups = right.Length * Math.Pow(Average(right) - averageForAll, 2) + wrong.Length * Math.Pow(Average(wrong) - averageForAll, 2);


            return VarianceAcrossGroups / (VarianceInnerGroup + VarianceAcrossGroups);
        }

        public static void Run()
        {
            //书上的例子
            //int[] rightForDifficulty = new int[] { 23,26,27,28 };
            //int[] wrongForDifficulty = new int[] { 25,26,29,32,33 };
            //int[] other = new int[] { 15, 16, 18, 22, 26, 29 };

            //正解 & 難しさ
            int[] rightForDifficulty = new int[] { 0, 0, 0, -1, -1, -1, -1, -2 };
            int[] wrongForDifficulty = new int[] { 0, 0 };
            double forDifficulty = GetRelevanceRatio(rightForDifficulty, wrongForDifficulty);

            //正解 & 自信
            int[] rightForConfidence = new int[] { 1, 0, 0, 2, 0, 1, 2, 1 };
            int[] wrongForConfidence = new int[] { 0, 0 };
            double forConfidence = GetRelevanceRatio(rightForConfidence, wrongForConfidence);
        }
    }
}