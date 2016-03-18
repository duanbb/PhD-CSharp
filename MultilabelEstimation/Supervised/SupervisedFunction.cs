using System;

namespace MultilabelEstimation.Supervised
{
    static class SupervisedFunction
    {
        static public bool IsNumberOfTraningSentencesValid()
        {
            if (SupervisedVariable.NumberOfTraningSentences > Variable.Sentences.Count)
            {
                Console.WriteLine("Number of training sentences is lower than number of sentences");
                return false;
            }
            return true;
        }
    }
}
