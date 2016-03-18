using Interoperability.Cascaded;
using Interoperability.Entity;
using Interoperability.MLE;
using Interoperability.Space;
using System;
using System.IO;

namespace Interoperability
{
    static class Program
    {
        static void Main()
        {
            #region 参数
            Constant.Gold = Gold.Top;
            Constant.Similarity = Similarity.SMC;
            Constant.Methods = new Method[] { Method.MLE, Method.Cascaded, };// Method.Aggregation, Method.OrdinaryCombination, Method.WeightedCombination, Method.ExpertiseCombination, Method.Probability
            int[] groupsizes = new int[] { 3, 5, 10, 15, 30 };
            Constant.Filter = Filter.Ordinary;

            //TrainConstant.Corpus = Corpus.Love; NotTrainConstant.Corpus = Corpus.Apple;
            TrainConstant.Corpus = Corpus.Apple; NotTrainConstant.Corpus = Corpus.Love;
            //Constant.SourceTaxonomy = new Taxonomy(TaxonomyType.Ekman, Constant.EkmanLabelArray); Constant.TargetTaxonomy = new Taxonomy(TaxonomyType.Nakamura, Constant.NakaLabelArray);
            Constant.SourceTaxonomy = new Taxonomy(TaxonomyType.Nakamura, Constant.NakaLabelArray); Constant.TargetTaxonomy = new Taxonomy(TaxonomyType.Ekman, Constant.EkmanLabelArray);
            #endregion

            #region Initialization (Train)
            TrainConstant.SentenceList = GeneralFunction.SentenceList(TrainConstant.Corpus);
            switch (TrainConstant.Corpus)
            {
                case Corpus.LoveSample:
                    InitializationFunction._InitializeLoveSample();
                    break;
                case Corpus.Love:
                    InitializationFunction.InitializeLove(TrainConstant.SentenceList, ref TrainConstant.SourceWorkerList, ref TrainConstant.TargetWorkerList);
                    break;
                case Corpus.AppleSample:
                    InitializationFunction._InitializeAppleSample(TrainConstant.SentenceList, ref TrainConstant.SourceWorkerList, ref TrainConstant.TargetWorkerList);
                    break;
                case Corpus.Apple:
                    InitializationFunction.InitializeApple(TrainConstant.SentenceList, ref TrainConstant.SourceWorkerList, ref TrainConstant.TargetWorkerList);
                    break;
            }
            #endregion

            #region Initialization (Other)
            NotTrainConstant.SentenceList = GeneralFunction.SentenceList(NotTrainConstant.Corpus);
            switch (NotTrainConstant.Corpus)
            {
                case Corpus.Love:
                    InitializationFunction.InitializeLove(NotTrainConstant.SentenceList, ref NotTrainConstant.SourceWorkerList, ref NotTrainConstant.TargetWorkerList);
                    break;
                case Corpus.Apple:
                    InitializationFunction.InitializeApple(NotTrainConstant.SentenceList, ref NotTrainConstant.SourceWorkerList, ref NotTrainConstant.TargetWorkerList);
                    break;
            }
            #endregion

            #region 过滤 （为了分组）
            if (Constant.Filter != Filter.Ordinary)
            {
                SentenceProperty.SortedSourceWorkerSourceAnnotationCountDic();//如果按照true label个数过滤worker的话，需要
                SentenceProperty.SortedTargetWorkerTargetAnnotationCountDic();//如果按照true label个数过滤worker的话，需要
            }
            InitializationFunction.FilterTargetWorker();
            #endregion

            #region PropertyGeneration （整体属性，与各组无关）
            SentenceProperty.SortedSourceAnnotationCountDic();
            SentenceProperty.SortedSourceLabelDic();
            SentenceProperty.GoldSourceAnnotation();
            GeneralFunction.OutputGoldBinarySourceAnnotations();
            SentenceProperty.SortedTargetAnnotationCountDic();
            SentenceProperty.SortedTargetLabelDic();
            SentenceProperty.GoldTargetAnnotation();
            GeneralFunction.OutputGoldBinaryTargetAnnotations();
            SpaceFunction.OutputGoldRealTargetAnnotations();
            CascadedFunction.OutputGoldRealTargetAnnotations();
            #endregion

            #region Statistic (For Paper)
            //IDictionary<Label, double> numberOfEachLabel = PaperFunction.NumberOfEachLabel(SourceOrTarget.Source, Constant.SentenceList);
            //int numberOfWorkers = PaperFunction.NumberOfWorkers(SourceOrTarget.Source, Constant.SentenceList);
            //IDictionary<Annotation, double> numberOfEachAnnotation = PaperFunction.NumberOfEachAnnotation(SourceOrTarget.Target, TrainConstant.SentenceList);
            #endregion

            #region Algorithm
            foreach (int groupsize in groupsizes)
            {
                InitializationFunction.Group(groupsize);
                int numberOfGroups = 30 / groupsize;

                foreach (Method method in Constant.Methods)
                {
                    switch (method)
                    {
                        case Method.MLE:
                            double accuracy = 0;
                            for (int i = 0; i < numberOfGroups; ++i)
                            {
                                MLEFunction.Pr_T_S(i);//训练用（因为测试时不用计算，所以没有测试步骤）

                                accuracy += GeneralFunction.Accuracy(method);

                                GeneralFunction.OutputEstimatedBinaryTargetAnnotations(method, groupsize, i);
                            }
                            GeneralFunction.ConsoleAndFile(method + "," + accuracy / numberOfGroups);
                            break;

                        case Method.Cascaded:
                            accuracy = 0;
                            for (int i = 0; i < numberOfGroups; ++i)
                            {
                                CascadedFunction.Pr_t(i);
                                CascadedFunction.Pr_t_s(i);
                                CascadedFunction.OutputPr_t_s(groupsize, i);
                                CascadedFunction.Pr_T_S();//测试用（训练在S内完成，并且只需要前面计算好的Pr_t和Pr_t_s，所以与分组无关）

                                accuracy += GeneralFunction.Accuracy(method);

                                GeneralFunction.OutputEstimatedBinaryTargetAnnotations(method, groupsize, i);
                            }
                            GeneralFunction.ConsoleAndFile(method + "," + accuracy / numberOfGroups);

                            #region Test Pr_T_t
                            //TargetAnnotation ta = new TargetAnnotation();
                            //ta.LabelAndTruthDic[Label.喜Joy] = true;
                            //TargetLabeltruth tl = new TargetLabeltruth(Label.喜Joy, true);
                            //double Pr_T_t = tl.Pr_T_t(ta);

                            // 验证和是1
                            //TargetLabeltruth tl = new TargetLabeltruth(Label.喜Joy, true);
                            //double Pr_T_t_ForAllT = 0;
                            //foreach (Label[] targetLabelArray in ProbabilityFunction.PowerSet(Constant.TargetTaxonomy.LabelArray))
                            //{
                            //    TargetAnnotation targetAnnotation = new TargetAnnotation(targetLabelArray);
                            //    Pr_T_t_ForAllT += tl.Pr_T_t(targetAnnotation);
                            //}
                            #endregion

                            #region Test Pr_t_s（验证和是1）
                            //TargetLabeltruth tltrue = new TargetLabeltruth(Label.哀Sadness, true);
                            //TargetLabeltruth tlflase = new TargetLabeltruth(Label.哀Sadness, false);
                            //SourceLabeltruth sltrue = new SourceLabeltruth(Label.Sadness, true);
                            //SourceLabeltruth slfalse = new SourceLabeltruth(Label.Sadness, false);
                            //double Pr_t_srue_t_true = ProbabilityConstant.Matrix[sltrue][tltrue];
                            //double Pr_t_srue_t_false = ProbabilityConstant.Matrix[sltrue][tlflase];
                            //double Pr_s_false_t_true = ProbabilityConstant.Matrix[slfalse][tltrue];
                            //double Pr_s_false_t_false = ProbabilityConstant.Matrix[slfalse][tlflase];
                            #endregion

                            #region Test MLE
                            //SourceAnnotation sa = new SourceAnnotation();
                            ////sa.LabelAndTruthDic[Label.Anger] = true;
                            ////sa.LabelAndTruthDic[Label.Sadness] = true;
                            //sa.LabelAndTruthDic[Label.Joy] = true;
                            ////sa.LabelAndTruthDic[Label.Surprise] = true;
                            ////sa.LabelAndTruthDic[Label.Fear] = true;
                            ////sa.LabelAndTruthDic[Label.Disgust] = true;
                            //IDictionary<TargetAnnotation, double> targetAnnotationProbabilityDic = new Dictionary<TargetAnnotation, double>();
                            //foreach (Label[] targetLabelArray in ProbabilityFunction.PowerSet(Constant.TargetTaxonomy.LabelArray))
                            //{
                            //    TargetAnnotation targetAnnotation = new TargetAnnotation(targetLabelArray);
                            //    double probability = sa.Pr_T_S(targetAnnotation);
                            //    if (probability != 0)
                            //        targetAnnotationProbabilityDic.Add(targetAnnotation, probability);
                            //}
                            //IDictionary<TargetAnnotation, double> sortedElements = GeneralFunction.SortDictionary(targetAnnotationProbabilityDic);
                            #endregion
                            break;

                        #region Space
                        case Method.Aggregation:
                            SpaceFunction.AggregatedMatrix();

                            SpaceFunction.OutputMatrix(Method.Aggregation);
                            SpaceFunction.RealTargetAnnotations();

                            SpaceFunction.OutputEstimatedRealTargetAnnotations(method);
                            GeneralFunction.OutputEstimatedBinaryTargetAnnotations(method, groupsize, int.MaxValue);

                            GeneralFunction.ConsoleAndFile(method + "," + GeneralFunction.Accuracy(method));

                            break;

                        case Method.OrdinaryCombination://0论文中OC
                            SpaceFunction.OrdinaryMatrix();

                            SpaceFunction.OutputMatrix(method);
                            SpaceFunction.RealTargetAnnotations();

                            SpaceFunction.OutputEstimatedRealTargetAnnotations(method);
                            GeneralFunction.OutputEstimatedBinaryTargetAnnotations(method, groupsize, int.MaxValue);

                            GeneralFunction.ConsoleAndFile(method + "," + GeneralFunction.Accuracy(method));
                            break;

                        case Method.WeightedCombination://5（IMECS中WC）
                            SentenceProperty.OtherNonormalizeWeightDic();

                            SpaceFunction.WeightedMatrix(method);
                            SpaceFunction.OutputMatrix(method);
                            SpaceFunction.RealTargetAnnotations();

                            SpaceFunction.OutputEstimatedRealTargetAnnotations(method);
                            GeneralFunction.OutputEstimatedBinaryTargetAnnotations(method, groupsize, int.MaxValue);

                            GeneralFunction.ConsoleAndFile(method + "," + GeneralFunction.Accuracy(method));
                            break;

                        case Method.ExpertiseCombination:
                            TargetWorkerProperty.ExpertiseMatrix(1);

                            SpaceFunction.ExpertiseTransformationMatrix();
                            SpaceFunction.OutputMatrix(method);
                            SpaceFunction.RealTargetAnnotations();

                            SpaceFunction.OutputEstimatedRealTargetAnnotations(method);
                            GeneralFunction.OutputEstimatedBinaryTargetAnnotations(method, groupsize, int.MaxValue);

                            GeneralFunction.ConsoleAndFile(method + "," + GeneralFunction.Accuracy(method));
                            break;


                        case Method.TestExpertise:
                            TargetWorkerProperty.ExpertiseMatrix(10);
                            Test.TestExpertise(10);
                            break;
                        #endregion

                        #region 废弃方法
                        case Method.TemporaryNogeneralNonormalize://1
                            SentenceProperty.TemporaryNonormalizeWeightDic();

                            SpaceFunction.WeightedMatrix(method);
                            SpaceFunction.OutputMatrix(method);
                            GeneralFunction.ConsoleAndFile(method + "," + GeneralFunction.Accuracy(method));
                            break;

                        //一个sentence内所有worker的影响和为1。
                        //结果与未Normalized不相同（理论上是相同的，可能是计算精度问题）
                        case Method.TemporaryNogeneralNormalize://2(需要1)
                            SentenceProperty.TemporaryNormalizeWeightDic();

                            SpaceFunction.WeightedMatrix(method);
                            SpaceFunction.OutputMatrix(method);
                            GeneralFunction.ConsoleAndFile(method + "," + GeneralFunction.Accuracy(method));
                            break;

                        case Method.TemporaryGeneralNonormalize://3(需要1)
                            TargetWorkerProperty.TemporaryNonormalizeWeight();

                            SpaceFunction.WeightedMatrix(method);
                            SpaceFunction.OutputMatrix(method);
                            GeneralFunction.ConsoleAndFile(method + "," + GeneralFunction.Accuracy(method));
                            break;

                        case Method.TemporaryGeneralNormalize://4(需要3)
                            TargetWorkerProperty.TemporaryNormalizeWeight();

                            SpaceFunction.WeightedMatrix(method);
                            SpaceFunction.OutputMatrix(method);
                            GeneralFunction.ConsoleAndFile(method + "," + GeneralFunction.Accuracy(method));
                            break;


                        case Method.OtherNogeneralNormalize://6（需要5）
                            SentenceProperty.OtherNormalizeWeightDic();

                            SpaceFunction.WeightedMatrix(method);
                            SpaceFunction.OutputMatrix(method);
                            GeneralFunction.ConsoleAndFile(method + "," + GeneralFunction.Accuracy(method));
                            break;

                        case Method.OtherGeneralNonormalize://7（需要5）
                            TargetWorkerProperty.OtherNonormalizeWeight();

                            SpaceFunction.WeightedMatrix(method);
                            SpaceFunction.OutputMatrix(method);
                            GeneralFunction.ConsoleAndFile(method + "," + GeneralFunction.Accuracy(method));
                            break;

                        case Method.OtherGeneralNormalize:
                            TargetWorkerProperty.OtherGeneralWeight();

                            SpaceFunction.WeightedMatrix(method);
                            SpaceFunction.OutputMatrix(method);
                            GeneralFunction.ConsoleAndFile(method + "," + GeneralFunction.Accuracy(method));
                            break;
                        #endregion
                    }
                }
            }
            #endregion

            #region Output
            StreamWriter Output = new StreamWriter("Output/" + Constant.Gold + "/" + TrainConstant.Corpus + "_" + Constant.SourceTaxonomy.Name + "/Accuracy.csv");
            Output.Write(Constant.Output);
            Output.Close();
            Console.Write("Press any key...");
            Console.Read();
            #endregion
        }
    }
}