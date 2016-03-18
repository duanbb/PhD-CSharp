using System;
using NetSVMLight;
using System.Collections.Generic;

namespace SupportVectorMachine
{
    static class Program
    {
        static void Main(string[] args)
        {
            int trainNumber = 200;

            //三部分可分别运行
            #region 初始化
            //Function.MakeFileForCrowdscaleClassification(trainNumber);
            #endregion

            #region SVM
            Kernel kernel = Kernel.Linear;
            int ParamD = 1;//parameter d in polynomial kernel (default: 3)
            int ParamG = 1;//parameter gamma in rbf kernel (default: 1)
            int ParamS = 1;//parameter s in sigmoid/poly kernel (default: 1)
            int ParamC = 1;//Param C in sigmoid/poly kernel. (default: 1)16777208
            Function.SVMCrowdScale(trainNumber, kernel, ParamD, ParamG, ParamS, ParamC);
            #endregion

            #region single label decision
            SinglelabelEstimation.Function.Initialize(ref Variable.Workers, ref Variable.Sentences, ref Variable.GoldStandard, ref Variable.SentenceTexts);
            //double workersPerTweet = GetAnswersPerWorker();//report用
            //fliterSentences(0);//只算有Gold的后若干句，Report用
            double accuracy = Function.CalculateAccuracy(trainNumber);
            #endregion
            //SVMExample();
            Console.WriteLine("Press any key to exit..."); Console.Read();
        }

        static double GetAnswersPerTweet()
        {
            double numberOfAnswers = 0;
            foreach (string sentence in Variable.GoldStandard.Keys)
            {
                numberOfAnswers += Variable.Sentences[sentence].Count;
            }
            return numberOfAnswers / Variable.GoldStandard.Count;
        }

        static double GetAnswersPerWorker()
        {
            IList<string> workers = new List<string>();
            double numberOfAnswers = 0;
            foreach (string sentence in Variable.GoldStandard.Keys)
            {
                numberOfAnswers += Variable.Sentences[sentence].Count;
                foreach (string worker in Variable.Sentences[sentence].Keys)
                {
                    if (!workers.Contains(worker))
                    {
                        workers.Add(worker);
                    }
                }
            }
            return numberOfAnswers / workers.Count;
        }

        static void fliterSentences(int trainNumber)
        {
            IList<string> testSentences = new List<string>();
            int train = 0;
            foreach (string sentence in Variable.GoldStandard.Keys)
            {
                if (train >= trainNumber)
                {
                    testSentences.Add(sentence);
                }
                ++train;
            }

            IList<string> sentences = new List<string>(Variable.Sentences.Keys);
            foreach (string sentence in sentences)
            {
                if (!testSentences.Contains(sentence))
                {
                    Variable.Sentences.Remove(sentence);
                }
            }
        }
    }
}