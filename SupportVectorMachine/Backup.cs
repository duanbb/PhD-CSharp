using NetSVMLight;
using System.Collections.Generic;
using System.IO;

namespace SupportVectorMachine
{
    static class Backup
    {
        static public void SVMCrowdScaleForRegression(int trainNumber)
        {
            string folderName = trainNumber + "Regression";

            SVMLearn svmLearn = new SVMLearn();
            svmLearn.mode = Mode.Regression;
            svmLearn.kernelType = Kernel.Polynomial;
            svmLearn.ExecuteLearner("svm_learn.exe", folderName + "CrowdScale/train.dat", folderName + "CrowdScale/model.txt", folderName + "CrowdScale/learnLog.txt", false);
            new SVMClassify().ExecuteClassifier("svm_classify.exe", folderName + "CrowdScale/test.dat", folderName + "CrowdScale/model.txt", folderName + "CrowdScale/output.txt", folderName + "CrowdScale/clasifylog.txt", false);
        }

        static public void MakeFileForCrowdscaleRegression(int trainNumber)
        {
            SinglelabelEstimation.Function.Initialize(ref Variable.Workers, ref Variable.Sentences, ref Variable.GoldStandard, ref Variable.SentenceTexts);
            List<string> allWorkers = Function.SortWorkers();

            IDictionary<string, string> sentences = new Dictionary<string, string>();
            foreach (string sentence in Variable.GoldStandard.Keys)
            {
                List<string> workers = Function.SortWorkers(Variable.Sentences[sentence].Keys);
                string line = string.Empty;
                foreach (string worker in workers)
                {
                    line += " " + (allWorkers.IndexOf(worker) + 1) + ":" + (Variable.Sentences[sentence][worker][0]);
                }
                sentences.Add(sentence, line);
            }

            StreamWriter trainFile = new StreamWriter(trainNumber + "RegressionCrowdScale/train.dat");
            StreamWriter testFile = new StreamWriter(trainNumber + "RegressionCrowdScale/test.dat");

            int train = 0;
            foreach (string sentence in Variable.GoldStandard.Keys)
            {
                if (train < trainNumber)
                {
                    trainFile.WriteLine(Variable.GoldStandard[sentence] + sentences[sentence]);
                }
                else
                {
                    testFile.WriteLine(Variable.GoldStandard[sentence] + sentences[sentence]);
                }
                ++train;
            }
            trainFile.Close();
            testFile.Close();
        }

        static public void MakeFileForCrowdscaleJoint(int trainNumber)//废弃
        {
            //Variable.Sentences[sentence][worker]不含没标的，大于1次的有86个；有Gold的句子里没有一人一句多标的情况，一标有1720个
            SinglelabelEstimation.Function.Initialize(ref Variable.Workers, ref Variable.Sentences, ref Variable.GoldStandard, ref Variable.SentenceTexts);
            List<string> allWorkers = Function.SortWorkers();

            IDictionary<string, string> sentences = new Dictionary<string, string>();
            foreach (string sentence in Variable.GoldStandard.Keys)
            {
                List<string> workers = Function.SortWorkers(Variable.Sentences[sentence].Keys);
                string line = string.Empty;
                foreach (string worker in workers)
                {
                    line += " " + (allWorkers.IndexOf(worker) + 1) + ":" + (Variable.Sentences[sentence][worker][0] + 7);//加到7就会变，所以此方法与标签值有关，应废弃
                }
                sentences.Add(sentence, line);
            }

            StreamWriter train0 = new StreamWriter(trainNumber + "JointCrowdScale/0train.dat");
            StreamWriter train1 = new StreamWriter(trainNumber + "JointCrowdScale/1train.dat");
            StreamWriter train2 = new StreamWriter(trainNumber + "JointCrowdScale/2train.dat");
            StreamWriter train3 = new StreamWriter(trainNumber + "JointCrowdScale/3train.dat");
            StreamWriter train4 = new StreamWriter(trainNumber + "JointCrowdScale/4train.dat");
            StreamWriter test0 = new StreamWriter(trainNumber + "JointCrowdScale/0test.dat");
            StreamWriter test1 = new StreamWriter(trainNumber + "JointCrowdScale/1test.dat");
            StreamWriter test2 = new StreamWriter(trainNumber + "JointCrowdScale/2test.dat");
            StreamWriter test3 = new StreamWriter(trainNumber + "JointCrowdScale/3test.dat");
            StreamWriter test4 = new StreamWriter(trainNumber + "JointCrowdScale/4test.dat");

            Function.WriteComment(new StreamWriter[] { train0, train1, train2, train3, train4 }, new StreamWriter[] { test0, test1, test2, test3, test4 }, allWorkers.Count, trainNumber);

            int train = 0;
            foreach (string sentence in Variable.GoldStandard.Keys)
            {
                if (train < trainNumber)
                {
                    switch (Variable.GoldStandard[sentence])
                    {
                        case 0:
                            train0.WriteLine(1 + sentences[sentence]);
                            train1.WriteLine(-1 + sentences[sentence]);
                            train2.WriteLine(-1 + sentences[sentence]);
                            train3.WriteLine(-1 + sentences[sentence]);
                            train4.WriteLine(-1 + sentences[sentence]);
                            break;
                        case 1:
                            train0.WriteLine(-1 + sentences[sentence]);
                            train1.WriteLine(1 + sentences[sentence]);
                            train2.WriteLine(-1 + sentences[sentence]);
                            train3.WriteLine(-1 + sentences[sentence]);
                            train4.WriteLine(-1 + sentences[sentence]);
                            break;
                        case 2:
                            train0.WriteLine(-1 + sentences[sentence]);
                            train1.WriteLine(-1 + sentences[sentence]);
                            train2.WriteLine(1 + sentences[sentence]);
                            train3.WriteLine(-1 + sentences[sentence]);
                            train4.WriteLine(-1 + sentences[sentence]);
                            break;
                        case 3:
                            train0.WriteLine(-1 + sentences[sentence]);
                            train1.WriteLine(-1 + sentences[sentence]);
                            train2.WriteLine(-1 + sentences[sentence]);
                            train3.WriteLine(1 + sentences[sentence]);
                            train4.WriteLine(-1 + sentences[sentence]);
                            break;
                        case 4:
                            train0.WriteLine(-1 + sentences[sentence]);
                            train1.WriteLine(-1 + sentences[sentence]);
                            train2.WriteLine(-1 + sentences[sentence]);
                            train3.WriteLine(-1 + sentences[sentence]);
                            train4.WriteLine(1 + sentences[sentence]);
                            break;
                    }
                }
                else
                {
                    switch (Variable.GoldStandard[sentence])
                    {
                        case 0:
                            test0.WriteLine(1 + sentences[sentence]);
                            test1.WriteLine(-1 + sentences[sentence]);
                            test2.WriteLine(-1 + sentences[sentence]);
                            test3.WriteLine(-1 + sentences[sentence]);
                            test4.WriteLine(-1 + sentences[sentence]);
                            break;
                        case 1:
                            test0.WriteLine(-1 + sentences[sentence]);
                            test1.WriteLine(1 + sentences[sentence]);
                            test2.WriteLine(-1 + sentences[sentence]);
                            test3.WriteLine(-1 + sentences[sentence]);
                            test4.WriteLine(-1 + sentences[sentence]);
                            break;
                        case 2:
                            test0.WriteLine(-1 + sentences[sentence]);
                            test1.WriteLine(-1 + sentences[sentence]);
                            test2.WriteLine(1 + sentences[sentence]);
                            test3.WriteLine(-1 + sentences[sentence]);
                            test4.WriteLine(-1 + sentences[sentence]);
                            break;
                        case 3:
                            test0.WriteLine(-1 + sentences[sentence]);
                            test1.WriteLine(-1 + sentences[sentence]);
                            test2.WriteLine(-1 + sentences[sentence]);
                            test3.WriteLine(1 + sentences[sentence]);
                            test4.WriteLine(-1 + sentences[sentence]);
                            break;
                        case 4:
                            test0.WriteLine(-1 + sentences[sentence]);
                            test1.WriteLine(-1 + sentences[sentence]);
                            test2.WriteLine(-1 + sentences[sentence]);
                            test3.WriteLine(-1 + sentences[sentence]);
                            test4.WriteLine(1 + sentences[sentence]);
                            break;
                    }
                }
                ++train;
            }
            train0.Close();
            train1.Close();
            train2.Close();
            train3.Close();
            train4.Close();
            test0.Close();
            test1.Close();
            test2.Close();
            test3.Close();
            test4.Close();
        }
    }
}
