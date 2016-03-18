using System;
using MultilabelEstimation.Group;

namespace MultilabelEstimation
{
    class SimilarityMeasure
    {
        static public double Manhattan(NumericResult ann1, NumericResult ann2)
        {
            double result = 0;
            foreach (Label label in Variable.LabelArray)
            {
                result += Math.Abs(ann1.Labels[label] - ann2.Labels[label]);
            }
            return result / Variable.LabelArray.Length;
        }

        static public double Euclidean(NumericResult ann1, NumericResult ann2)
        {
            double result = 0;
            foreach (Label label in Variable.LabelArray)
            {
                result += Math.Pow(ann1.Labels[label] - ann2.Labels[label], 2);
            }
            return Math.Pow(result, 0.5);
        }

        //用余弦计算每个句子的相似度(两者都为MU时视为完全相似)不合理
        static public double Cosine(Annotation ann1, Annotation ann2)
        {
            if (ann1.Mu || ann2.Mu)
            {
                if (ann1.Mu && ann2.Mu)
                    return 1;
                return 0;
            }
            double numerator = 0;
            double denominator1 = 0;
            double denominator2 = 0;
            foreach (Label label in Variable.LabelArray)
            {
                numerator += Convert.ToInt16(ann1.Labels[label]) * Convert.ToInt16(ann2.Labels[label]);
                denominator1 += Math.Pow(Convert.ToInt16(ann1.Labels[label]), 2);
                denominator2 += Math.Pow(Convert.ToInt16(ann2.Labels[label]), 2);
            }
            denominator1 = Math.Pow(denominator1, 0.5);
            denominator2 = Math.Pow(denominator2, 0.5);
            return numerator / (denominator1 * denominator2);
        }

        //用简单对比计算每个句子的相似度
        static public double Compare(Annotation ann1, Annotation ann2)
        {
            double result = 0;
            foreach (Label label in Variable.LabelArray)
            {
                if (ann1.Labels[label] == ann2.Labels[label])
                    ++result;
            }
            return result / Variable.LabelArray.Length;
        }

        static public double BinaryAndNumeric(Annotation ann1, NumericResult ann2)
        {
            double result = 0;
            foreach (Label label in Variable.LabelArray)
            {
                result += 1 - Math.Abs(Convert.ToDouble(ann1.Labels[label]) - ann2.Labels[label]);
            }
            return result / Variable.LabelArray.Length;
        }

        static public double JaccardPlusANumber(Annotation ann1, Annotation ann2)
        {
            double numerator = Math.Pow(10, -10);
            double denominator = Math.Pow(10, -10);
            foreach (Label label in Variable.LabelArray)
            {
                if (ann1.Labels[label] || ann2.Labels[label])
                    ++denominator;
                if (ann1.Labels[label] && ann2.Labels[label])
                    ++numerator;
            }
            return numerator / denominator;
        }

        //(两者都为MU时视为完全相似)不合理
        static public double Jaccard(Annotation ann1, Annotation ann2)
        {
            double numerator = 0;
            double denominator = 0;
            foreach (Label label in Variable.LabelArray)
            {
                if (ann1.Labels[label] || ann2.Labels[label])
                    ++denominator;
                if (ann1.Labels[label] && ann2.Labels[label])
                    ++numerator;
            }
            return denominator == 0 ? 1 : numerator / denominator;
        }

        static public double DicePlusANumber(Annotation ann1, Annotation ann2)
        {
            double numerator = Math.Pow(10, -10);
            double denominator = Math.Pow(10, -10) * 2;
            foreach (Label label in Variable.LabelArray)
            {
                if (ann1.Labels[label])
                    ++denominator;
                if (ann2.Labels[label])
                    ++denominator;
                if (ann1.Labels[label] && ann2.Labels[label])
                    ++numerator;
            }
            return 2 * numerator / denominator;
        }

        //(两者都为MU时视为完全相似)不合理
        static public double Dice(Annotation ann1, Annotation ann2)
        {
            double numerator = 0;
            double denominator = 0;
            foreach (Label label in Variable.LabelArray)
            {
                if (ann1.Labels[label])
                    ++denominator;
                if (ann2.Labels[label])
                    ++denominator;
                if (ann1.Labels[label] && ann2.Labels[label])
                    ++numerator;
            }
            return denominator == 0 ? 1 : 2 * numerator / denominator;
        }
    }
}
