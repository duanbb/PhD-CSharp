using NetSVMLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SupportVectorMachine
{
    static class Function
    {
        static public double CalculateAccuracy(int trainNumber)
        {
            double[][] distancesForLabels = new double[5][];
            for (int i = 0; i <= 4; ++i)
            {
                distancesForLabels[i] = readDistancesOfSentences(i, trainNumber);
            }

            int testNumber = Variable.GoldStandard.Count - trainNumber;
            double[][] distancesOfSentences = new double[testNumber][];
            for (int i = 0; i < testNumber; ++i)
            {
                distancesOfSentences[i] = new double[5];
                for (int j = 0; j <= 4; ++j)
                {
                    distancesOfSentences[i][j] = distancesForLabels[j][i];
                }
            }

            IDictionary<string, int> testingSentencesAndIndexes = new Dictionary<string, int>();
            int index = 0;
            foreach (string sentence in Variable.GoldStandard.Keys)
            {
                if (index >= trainNumber)
                {
                    testingSentencesAndIndexes.Add(sentence, index - trainNumber);
                }
                ++index;
            }
            StreamWriter optimalLabels = new StreamWriter("OptimalLabels.csv", false, Encoding.Default);
            IDictionary<string, KeyValuePair<int, double>> optimalLabelOfSentences = new Dictionary<string, KeyValuePair<int, double>>();
            double rightSentenceNumber = 0;
            foreach (string sentence in testingSentencesAndIndexes.Keys)
            {
                optimalLabelOfSentences.Add(sentence, ObtainOptimalLabelOfSentence(distancesOfSentences[testingSentencesAndIndexes[sentence]]));
                string rightOrWrong = "○";
                if (optimalLabelOfSentences[sentence].Key == Variable.GoldStandard[sentence])
                {
                    ++rightSentenceNumber;
                }
                else
                    rightOrWrong = "×(" + Variable.GoldStandard[sentence] + ")";
                optimalLabels.WriteLine(optimalLabelOfSentences[sentence].Value + "," + optimalLabelOfSentences[sentence].Key + "," + rightOrWrong + "," + Variable.SentenceTexts[sentence]);
            }
            optimalLabels.Close();
            return rightSentenceNumber / testingSentencesAndIndexes.Count;
        }

        static KeyValuePair<int, double> ObtainOptimalLabelOfSentence(double[] distancesForLabels)
        {
            int optimalLabel = 0;
            double optimalValue = double.MinValue;
            for (int i = 0; i <= 4; ++i)
            {
                if (distancesForLabels[i] > optimalValue)
                {
                    optimalValue = distancesForLabels[i];
                    optimalLabel = i;
                }
            }
            KeyValuePair<int, double> optimalLabelForSentence = new KeyValuePair<int, double>(optimalLabel, optimalValue);
            return optimalLabelForSentence;
        }

        static private double[] readDistancesOfSentences(int label, int trainNumber)
        {
            string[] stringDistances = File.ReadAllLines(trainNumber + "ClassificationCrowdScale/" + label + "output.txt");
            double[] distances = new double[stringDistances.Length];
            for(int i=0; i<stringDistances.Length;++i)
            {
                distances[i] = Convert.ToDouble(stringDistances[i]);
            }
            return distances;
        }

        static public void SVMCrowdScale(int numberOfTrain, Kernel kernel, int ParamD, double ParamG, double ParamS, double ParamC)
        {
            string folderName = numberOfTrain + "Classification";
            SVMLearn svmLearn0 = new SVMLearn();
            svmLearn0.kernelType = kernel;
            svmLearn0.ParamD = ParamD;
            svmLearn0.ParamG = ParamG;
            svmLearn0.ParamS = ParamS;
            svmLearn0.ParamC = ParamC;
            svmLearn0.ExecuteLearner("svm_learn.exe", folderName + "CrowdScale/0train.dat", folderName + "CrowdScale/0model.txt", folderName + "CrowdScale/0learnLog.txt", false);
            new SVMClassify().ExecuteClassifier("svm_classify.exe", folderName + "CrowdScale/0test.dat", folderName + "CrowdScale/0model.txt", folderName + "CrowdScale/0output.txt", folderName + "CrowdScale/0clasifylog.txt", false);

            SVMLearn svmLearn1 = new SVMLearn();
            svmLearn1.kernelType = kernel;
            svmLearn1.ParamD = ParamD;
            svmLearn1.ParamG = ParamG;
            svmLearn1.ParamS = ParamS;
            svmLearn1.ParamC = ParamC;
            svmLearn1.ExecuteLearner("svm_learn.exe", folderName + "CrowdScale/1train.dat", folderName + "CrowdScale/1model.txt", folderName + "CrowdScale/1learnLog.txt", false);
            new SVMClassify().ExecuteClassifier("svm_classify.exe", folderName + "CrowdScale/1test.dat", folderName + "CrowdScale/1model.txt", folderName + "CrowdScale/1output.txt", folderName + "CrowdScale/1clasifylog.txt", false);

            SVMLearn svmLearn2 = new SVMLearn();
            svmLearn2.kernelType = kernel;
            svmLearn2.ParamD = ParamD;
            svmLearn2.ParamG = ParamG;
            svmLearn2.ParamS = ParamS;
            svmLearn2.ParamC = ParamC;
            svmLearn2.ExecuteLearner("svm_learn.exe", folderName + "CrowdScale/2train.dat", folderName + "CrowdScale/2model.txt", folderName + "CrowdScale/2learnLog.txt", false);
            new SVMClassify().ExecuteClassifier("svm_classify.exe", folderName + "CrowdScale/2test.dat", folderName + "CrowdScale/2model.txt", folderName + "CrowdScale/2output.txt", folderName + "CrowdScale/2clasifylog.txt", false);

            SVMLearn svmLearn3 = new SVMLearn();
            svmLearn3.kernelType = kernel;
            svmLearn3.ParamD = ParamD;
            svmLearn3.ParamG = ParamG;
            svmLearn3.ParamS = ParamS;
            svmLearn3.ParamC = ParamC;
            svmLearn3.ExecuteLearner("svm_learn.exe", folderName + "CrowdScale/3train.dat", folderName + "CrowdScale/3model.txt", folderName + "CrowdScale/3learnLog.txt", false);
            new SVMClassify().ExecuteClassifier("svm_classify.exe", folderName + "CrowdScale/3test.dat", folderName + "CrowdScale/3model.txt", folderName + "CrowdScale/3output.txt", folderName + "CrowdScale/3clasifylog.txt", false);

            SVMLearn svmLearn4 = new SVMLearn();
            svmLearn4.kernelType = kernel;
            svmLearn4.ParamD = ParamD;
            svmLearn4.ParamG = ParamG;
            svmLearn4.ParamS = ParamS;
            svmLearn4.ParamC = ParamC;
            svmLearn4.ExecuteLearner("svm_learn.exe", folderName + "CrowdScale/4train.dat", folderName + "CrowdScale/4model.txt", folderName + "CrowdScale/4learnLog.txt", false);
            new SVMClassify().ExecuteClassifier("svm_classify.exe", folderName + "CrowdScale/4test.dat", folderName + "CrowdScale/4model.txt", folderName + "CrowdScale/4output.txt", folderName + "CrowdScale/4clasifylog.txt", false);
        }

        static private void SVMExample()
        {
            SVMLearn svmLearn = new SVMLearn();
            svmLearn.ExecuteLearner("svm_learn.exe", "example/train.dat", "example/model.txt", "example/learnLog.txt", false);

            SVMClassify svmClassify = new SVMClassify();
            svmClassify.ExecuteClassifier("svm_classify.exe", "example/test.dat", "example/model.txt", "example/output.txt", "example/clasifylog.txt", false);
        }

        static public List<string> SortWorkers()
        {
            List<string> workers = new List<string>();
            foreach (string sentence in Variable.GoldStandard.Keys)
            {
                foreach (string worker in Variable.Sentences[sentence].Keys)
                {
                    if (!workers.Contains(worker))
                        workers.Add(worker);
                }
            }
            workers.Sort();
            return workers;
        }

        static public List<string> SortWorkers(ICollection<string> workerCollection)
        {
            List<string> workers = new List<string>();
            foreach (string worker in workerCollection)
            {
                workers.Add(worker);
            }
            workers.Sort();
            return workers;
        }

        static public void MakeFileForCrowdscaleClassification(int trainNumber)
        {
            SinglelabelEstimation.Function.Initialize(ref Variable.Workers, ref Variable.Sentences, ref Variable.GoldStandard, ref Variable.SentenceTexts);
            //double n = workersPerTweet(Variable.Sentences, Variable.GoldStandard);
            List<string> allWorkers = SortWorkers();

            IDictionary<int, IDictionary<string, string>> sentencesForLabel = new Dictionary<int, IDictionary<string, string>>();
            for (int i = 0; i <= 4; ++i)
            {
                sentencesForLabel.Add(i, new Dictionary<string, string>());
            }

            foreach (string sentence in Variable.GoldStandard.Keys)
            {
                List<string> workers = SortWorkers(Variable.Sentences[sentence].Keys);

                for (int i = 0; i <= 4; ++i)
                {
                    string line = string.Empty;
                    foreach (string worker in workers)
                    {
                        line += " " + (allWorkers.IndexOf(worker) + 1) + ":" + (Variable.Sentences[sentence][worker][0] == i ? "1" : "-1");//只要是相反数，就不影响结果；否则会影响
                    }
                    sentencesForLabel[i].Add(sentence, line);
                }
            }

            StreamWriter train0 = new StreamWriter(trainNumber + "ClassificationCrowdScale/0train.dat");
            StreamWriter train1 = new StreamWriter(trainNumber + "ClassificationCrowdScale/1train.dat");
            StreamWriter train2 = new StreamWriter(trainNumber + "ClassificationCrowdScale/2train.dat");
            StreamWriter train3 = new StreamWriter(trainNumber + "ClassificationCrowdScale/3train.dat");
            StreamWriter train4 = new StreamWriter(trainNumber + "ClassificationCrowdScale/4train.dat");
            StreamWriter test0 = new StreamWriter(trainNumber + "ClassificationCrowdScale/0test.dat");
            StreamWriter test1 = new StreamWriter(trainNumber + "ClassificationCrowdScale/1test.dat");
            StreamWriter test2 = new StreamWriter(trainNumber + "ClassificationCrowdScale/2test.dat");
            StreamWriter test3 = new StreamWriter(trainNumber + "ClassificationCrowdScale/3test.dat");
            StreamWriter test4 = new StreamWriter(trainNumber + "ClassificationCrowdScale/4test.dat");

            WriteComment(new StreamWriter[] { train0, train1, train2, train3, train4 }, new StreamWriter[] { test0, test1, test2, test3, test4 }, allWorkers.Count, trainNumber);

            int train = 0;
            foreach (string sentence in Variable.GoldStandard.Keys)
            {
                if (train < trainNumber)
                {
                    switch (Variable.GoldStandard[sentence])
                    {
                        case 0:
                            train0.WriteLine(1 + sentencesForLabel[0][sentence]);
                            train1.WriteLine(-1 + sentencesForLabel[1][sentence]);
                            train2.WriteLine(-1 + sentencesForLabel[2][sentence]);
                            train3.WriteLine(-1 + sentencesForLabel[3][sentence]);
                            train4.WriteLine(-1 + sentencesForLabel[4][sentence]);
                            break;
                        case 1:
                            train0.WriteLine(-1 + sentencesForLabel[0][sentence]);
                            train1.WriteLine(1 + sentencesForLabel[1][sentence]);
                            train2.WriteLine(-1 + sentencesForLabel[2][sentence]);
                            train3.WriteLine(-1 + sentencesForLabel[3][sentence]);
                            train4.WriteLine(-1 + sentencesForLabel[4][sentence]);
                            break;
                        case 2:
                            train0.WriteLine(-1 + sentencesForLabel[0][sentence]);
                            train1.WriteLine(-1 + sentencesForLabel[1][sentence]);
                            train2.WriteLine(1 + sentencesForLabel[2][sentence]);
                            train3.WriteLine(-1 + sentencesForLabel[3][sentence]);
                            train4.WriteLine(-1 + sentencesForLabel[4][sentence]);
                            break;
                        case 3:
                            train0.WriteLine(-1 + sentencesForLabel[0][sentence]);
                            train1.WriteLine(-1 + sentencesForLabel[1][sentence]);
                            train2.WriteLine(-1 + sentencesForLabel[2][sentence]);
                            train3.WriteLine(1 + sentencesForLabel[3][sentence]);
                            train4.WriteLine(-1 + sentencesForLabel[4][sentence]);
                            break;
                        case 4:
                            train0.WriteLine(-1 + sentencesForLabel[0][sentence]);
                            train1.WriteLine(-1 + sentencesForLabel[1][sentence]);
                            train2.WriteLine(-1 + sentencesForLabel[2][sentence]);
                            train3.WriteLine(-1 + sentencesForLabel[3][sentence]);
                            train4.WriteLine(1 + sentencesForLabel[4][sentence]);
                            break;
                    }
                }
                else
                {
                    switch (Variable.GoldStandard[sentence])
                    {
                        case 0:
                            test0.WriteLine(1 + sentencesForLabel[0][sentence]);
                            test1.WriteLine(-1 + sentencesForLabel[1][sentence]);
                            test2.WriteLine(-1 + sentencesForLabel[2][sentence]);
                            test3.WriteLine(-1 + sentencesForLabel[3][sentence]);
                            test4.WriteLine(-1 + sentencesForLabel[4][sentence]);
                            break;
                        case 1:
                            test0.WriteLine(-1 + sentencesForLabel[0][sentence]);
                            test1.WriteLine(1 + sentencesForLabel[1][sentence]);
                            test2.WriteLine(-1 + sentencesForLabel[2][sentence]);
                            test3.WriteLine(-1 + sentencesForLabel[3][sentence]);
                            test4.WriteLine(-1 + sentencesForLabel[4][sentence]);
                            break;
                        case 2:
                            test0.WriteLine(-1 + sentencesForLabel[0][sentence]);
                            test1.WriteLine(-1 + sentencesForLabel[1][sentence]);
                            test2.WriteLine(1 + sentencesForLabel[2][sentence]);
                            test3.WriteLine(-1 + sentencesForLabel[3][sentence]);
                            test4.WriteLine(-1 + sentencesForLabel[4][sentence]);
                            break;
                        case 3:
                            test0.WriteLine(-1 + sentencesForLabel[0][sentence]);
                            test1.WriteLine(-1 + sentencesForLabel[1][sentence]);
                            test2.WriteLine(-1 + sentencesForLabel[2][sentence]);
                            test3.WriteLine(1 + sentencesForLabel[3][sentence]);
                            test4.WriteLine(-1 + sentencesForLabel[4][sentence]);
                            break;
                        case 4:
                            test0.WriteLine(-1 + sentencesForLabel[0][sentence]);
                            test1.WriteLine(-1 + sentencesForLabel[1][sentence]);
                            test2.WriteLine(-1 + sentencesForLabel[2][sentence]);
                            test3.WriteLine(-1 + sentencesForLabel[3][sentence]);
                            test4.WriteLine(1 + sentencesForLabel[4][sentence]);
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

        static public void WriteComment(StreamWriter[] trainFiles, StreamWriter[] testFiles, int workerNumber, int trainNumber)
        {
            //true: train; false: test
            IDictionary<bool, IDictionary<int, int>> positiveExampleNumber = new Dictionary<bool, IDictionary<int, int>>();
            positiveExampleNumber.Add(true, new Dictionary<int, int>());
            positiveExampleNumber.Add(false, new Dictionary<int, int>());
            for (int i = 0; i <= 4; ++i)
            {
                positiveExampleNumber[true].Add(i, 0);
                positiveExampleNumber[false].Add(i, 0);
            }
            int ii = 0;
            foreach (string sentence in Variable.GoldStandard.Keys)
            {
                ++positiveExampleNumber[ii < trainNumber][Variable.GoldStandard[sentence]];
                ++ii;
            }
            for (int i = 0; i < trainFiles.Length; ++i)
            {
                trainFiles[i].WriteLine("# train examples: " + positiveExampleNumber[true][i] + " positive/ " + (trainNumber - positiveExampleNumber[true][i]) + " negative; workers: " + workerNumber);
                testFiles[i].WriteLine("# test examples: " + positiveExampleNumber[false][i] + " positive/ " + (Variable.GoldStandard.Count - trainNumber - positiveExampleNumber[false][i]) + " negative; workers: " + workerNumber);
            }
        }
    }
}
