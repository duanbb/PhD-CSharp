using System;
using System.Collections.Generic;

namespace SinglelabelEstimation
{
    static class Program
    {
        static void Main(string[] args)
        {
            //PreprocessingForCrowdScale.MakeDataAndWorkerFileForCrowdSclae();//生成CrowdScale数据文件
            //PreprocessingForCrowdScale.MakeSentenceFileForCrowdScale();//生成原文文件

            Function.Initialize(ref Variable.Workers, ref Variable.Sentences, ref Variable.GoldStandard, ref Variable.SentenceTexts);
            Function.Run();
            Console.Write("Finished. Press any key..."); Console.Read();
        }
    }
}