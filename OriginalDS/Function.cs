using System;
using System.Collections.Generic;

namespace OriginalDS
{
    static class Function
    {
        static public void Run()
        {
            //Dictionary<句子，次数> 句子i被标的总次数，用于计算Eij
            Dictionary<int, int> Ni = new Dictionary<int, int>(Variables.emptyInstancesDictionary);
            //Dictionary<句子，Dictionary<标签，次数>> 句子i被标为l的次数，用于计算Eij
            Dictionary<int, Dictionary<int, int>> Nil = new Dictionary<int, Dictionary<int, int>>();
            for (int i = 0; i < Variables.Instances.Value.Count; ++i)
            {
                Nil.Add(i, new Dictionary<int, int>(Variables.emptyLabelDicictionary));
            }
            //句子i被k标的总次数，无论标签为何<人，<句，值>>
            Dictionary<int, Dictionary<int, Dictionary<int, int>>> NilOfK = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
            for (int k = 0; k < Variables.CountOfAnnotators; ++k)
            {
                Dictionary<int, Dictionary<int, int>> n = new Dictionary<int, Dictionary<int, int>>();//分配内存
                for (int i = 0; i < Variables.Instances.Value.Count; ++i)
                {
                    n.Add(i, new Dictionary<int, int>(Variables.emptyLabelDicictionary));
                }
                NilOfK.Add(k, new Dictionary<int, Dictionary<int, int>>(n));//重新开辟内存，新内存区域仍与原内存区域保持数据统一
            }

            for (int i = 0; i < Variables.Instances.Value.Count; ++i)
            {
                for (int k = 0; k < Variables.CountOfAnnotators; ++k)
                {
                    foreach (int label in Variables.Instances.Value[i].annotators[k].labels)
                    {
                        ++Ni[i];
                        ++Nil[i][label];
                        ++NilOfK[k][i][label];
                    }
                }
            }
            //计算系数的分子<人，<句，值>>
            Dictionary<int, Dictionary<int, int>> numeratorOfCoefficient = new Dictionary<int, Dictionary<int, int>>();
            for (int k = 0; k < Variables.CountOfAnnotators; ++k)
            {
                numeratorOfCoefficient.Add(k, new Dictionary<int, int>(Variables.emptyInstancesDictionary));
            }
            for (int k = 0; k < Variables.CountOfAnnotators; ++k)
            {
                for (int i = 0; i < Variables.Instances.Value.Count; ++i)
                {
                    for (int l = 0; l < Variables.CountOfLabelKinds; ++l)
                    {
                        numeratorOfCoefficient[k][i] += NilOfK[k][i][l];
                    }
                }
            }
            for (int k = 0; k < Variables.CountOfAnnotators; ++k)
            {
                for (int i = 0; i < Variables.Instances.Value.Count; ++i)
                {
                    numeratorOfCoefficient[k][i] = Factorial(numeratorOfCoefficient[k][i]);
                }
            }
            //计算系数的分母<人，<句，值>>
            Dictionary<int, Dictionary<int, int>> denominatorOfCoefficient = new Dictionary<int, Dictionary<int, int>>();
            for (int k = 0; k < Variables.CountOfAnnotators; ++k)
            {
                denominatorOfCoefficient.Add(k, new Dictionary<int, int>(Variables.emptySDic1));
            }
            for (int k = 0; k < Variables.CountOfAnnotators; ++k)
            {
                for (int i = 0; i < Variables.Instances.Value.Count; ++i)
                {
                    for (int l = 0; l < Variables.CountOfLabelKinds; ++l)
                    {
                        denominatorOfCoefficient[k][i] *= Factorial(NilOfK[k][i][l]);
                    }
                }
            }
            //计算系数<人，<句，值>>
            Dictionary<int, Dictionary<int, int>> Coefficient = new Dictionary<int, Dictionary<int, int>>();//系数在求Eij时，分子分母都有，被约掉了；对于Pdata来说是个常数，无意义
            for (int k = 0; k < Variables.CountOfAnnotators; ++k)
            {
                Coefficient.Add(k, new Dictionary<int, int>(Variables.emptyInstancesDictionary));
            }
            for (int k = 0; k < Variables.CountOfAnnotators; ++k)
            {
                for (int i = 0; i < Variables.Instances.Value.Count; ++i)
                {
                    Coefficient[k][i] = numeratorOfCoefficient[k][i] / denominatorOfCoefficient[k][i];
                }
            }
            //计算初始Eij
            for (int i = 0; i < Variables.Instances.Value.Count; ++i)
            {
                for (int j = 0; j < Variables.CountOfLabelKinds; ++j)
                {
                    Variables.Eij.Value[i][j] = (double)Nil[i][j] / (double)Ni[i];
                    //Variables.Eij.Value[i][j] = 1 / (double)Variables.CountOfLabelKinds;//初始取平均的话，之后的计算就一直是平均值了
                }
            }

            Variables.ResultFile.WriteLine(Variables.Eij.ToString());

            for (int t = 1; t <= int.MaxValue; ++t)
            {
                //计算Pj
                CalculatePj();
                //计算πkjl
                CalculatePajl(NilOfK);
                //计算Eij
                CalculatePdataAndEij(NilOfK);
                if (Variables.Pdata.MondifiedValue == 0)
                    break;
            }

            Variables.ResultFile.Close();
        }

        //计算阶乘
        static private int Factorial(int i)
        {
            return ((i <= 1) ? 1 : (i * Factorial(i - 1)));
        }

        //计算π
        static private void CalculatePajl(Dictionary<int, Dictionary<int, Dictionary<int, int>>> NilOfK)
        {
            Variables.Pajl = new Pajl(++Variables.Pajl.Time);
            //Dictionary<人，Dictioary<Pair<标签j，标签l>，值>>：𝑛𝑢𝑚𝑏𝑒𝑟 𝑜𝑓 𝑠𝑒𝑛𝑡𝑒𝑛𝑐𝑒𝑠 𝑎𝑛𝑛𝑜𝑡𝑎𝑡𝑜𝑟 𝑘 𝑟𝑒𝑐𝑜𝑟𝑑𝑠 𝑙 𝑤ℎ𝑒𝑛 𝑗 𝑖𝑠 𝑐𝑜𝑟𝑟𝑒𝑐𝑡，分子
            Dictionary<int, Dictionary<Pair, double>> SjNlOfK = new Dictionary<int, Dictionary<Pair, double>>();
            //Dictionary<人，Dictionary<标签j，值>>：𝑛𝑢𝑚𝑏𝑒𝑟 𝑜𝑓 𝑠𝑒𝑛𝑡𝑒𝑛𝑐𝑒𝑠 𝑎𝑛𝑛𝑜𝑡𝑎𝑡𝑜𝑟 𝑘 𝑟𝑒𝑐𝑜𝑟𝑑𝑠 𝑤ℎ𝑒𝑛 𝑗 𝑖𝑠 𝑐𝑜𝑟𝑟𝑒𝑐𝑡，分母
            Dictionary<int, Dictionary<int, double>> SjNOfK = new Dictionary<int, Dictionary<int, double>>();
            //创建标签对集合

            //初始化分子分母
            for (int k = 0; k < Variables.CountOfAnnotators; ++k)
            {
                SjNlOfK.Add(k, new Dictionary<Pair, double>(Variables.emptySiNldouble));
                SjNOfK.Add(k, new Dictionary<int, double>(Variables.emptyLdouble));
            }
            //计算分子分母
            for (int k = 0; k < Variables.CountOfAnnotators; ++k)//人
            {
                for (int j = 0; j < Variables.CountOfLabelKinds; ++j)//正确标签
                {
                    for (int l = 0; l < Variables.CountOfLabelKinds; ++l)//人标的标签
                    {
                        for (int i = 0; i < Variables.Instances.Value.Count; ++i)//句
                        {
                            SjNlOfK[k][new Pair(j, l)] += Variables.Eij.Value[i][j] * NilOfK[k][i][l];//要重写==操作符
                            SjNOfK[k][j] += Variables.Eij.Value[i][j] * NilOfK[k][i][l];
                        }
                    }
                }
            }
            //计算π
            for (int k = 0; k < Variables.CountOfAnnotators; ++k)//人
            {
                for (int j = 0; j < Variables.CountOfLabelKinds; ++j)//正确标签
                {
                    for (int l = 0; l < Variables.CountOfLabelKinds; ++l)//人标的标签
                    {
                        Variables.Pajl.Value[k][new Pair(j, l)] += SjNlOfK[k][new Pair(j, l)] / SjNOfK[k][j];
                    }
                }
            }
            Variables.ResultFile.WriteLine(Variables.Pajl.ToString());
        }
        //计算Pj
        static private void CalculatePj()
        {
            Variables.Pj = new Pj(++Variables.Pj.Time);
            foreach (Dictionary<int, double> Sj in Variables.Eij.Value.Values)
            {
                for (int j = 0; j < Variables.CountOfLabelKinds; ++j)
                {
                    Variables.Pj.Value[j] += Sj[j];
                }
            }
            for (int j = 0; j < Variables.CountOfLabelKinds; ++j)
            {
                Variables.Pj.Value[j] /= Variables.Instances.Value.Count;
            }
            Variables.ResultFile.WriteLine(Variables.Pj.ToString());
        }
        //计算Eij和Pdata
        static private void CalculatePdataAndEij(Dictionary<int, Dictionary<int, Dictionary<int, int>>> NilOfK)
        {
            Variables.Pdata = new Pdata(++Variables.Pdata.Time, Variables.Pdata.Value);
            Variables.Eij = new Eij(++Variables.Eij.Time);
            //论文里公式2.5的分母的分子<句子，<标签，值>>
            Dictionary<int, Dictionary<int, double>> numeratorIJ = new Dictionary<int, Dictionary<int, double>>();
            for (int i = 0; i < Variables.Instances.Value.Count; ++i)
            {
                numeratorIJ.Add(i, new Dictionary<int, double>(Variables.emptyLdoubleValues1));
            }
            for (int i = 0; i < Variables.Instances.Value.Count; ++i)
            {
                for (int j = 0; j < Variables.CountOfLabelKinds; ++j)
                {
                    for (int k = 0; k < Variables.CountOfAnnotators; ++k)
                    {
                        for (int l = 0; l < Variables.CountOfLabelKinds; ++l)
                        {
                            //nominatorIOfJ[i][j] *= Math.Pow(Variables.Pajl.Value[k][new Pair(j, l)], NilOfK[k][i][l]) * Coefficient[k][i];
                            numeratorIJ[i][j] *= Math.Pow(Variables.Pajl.Value[k][new Pair(j, l)], NilOfK[k][i][l]);
                        }
                    }
                    numeratorIJ[i][j] *= Variables.Pj.Value[j];
                }
            }
            //论文里公式2.5的分母<句子，值>
            Dictionary<int, double> denominatorI = new Dictionary<int, double>();
            for (int i = 0; i < Variables.Instances.Value.Count; ++i)
            {
                denominatorI.Add(i, 0);
            }
            for (int i = 0; i < Variables.Instances.Value.Count; ++i)
            {
                for (int q = 0; q < Variables.CountOfLabelKinds; ++q)
                {
                    denominatorI[i] += numeratorIJ[i][q];
                }
            }
            //计算Pdata
            for (int i = 0; i < Variables.Instances.Value.Count; ++i)
            {
                for (int j = 0; j < Variables.CountOfLabelKinds; ++j)
                {
                    Variables.Eij.Value[i][j] = numeratorIJ[i][j] / denominatorI[i];
                }
                //Variables.Pdata.Value *= denominatorI[i];//因式过小，乘遍一次就等于0了
                Variables.Pdata.Value += -Math.Log10(denominatorI[i]);
            }
            Variables.ResultFile.WriteLine(Variables.Pdata.ToString());
            Variables.ResultFile.WriteLine(Variables.Eij.ToString());
        }
    }
}