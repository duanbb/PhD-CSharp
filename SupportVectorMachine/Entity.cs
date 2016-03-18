using System;
using System.Collections.Generic;
using SinglelabelEstimation;

namespace SupportVectorMachine
{
    static class Variable
    {
        static public string[] Workers;
        static public IDictionary<string, IDictionary<string, IList<int>>> Sentences;//sentence包含worker包含label
        static public IDictionary<string, int> GoldStandard;
        static public IDictionary<string, string> SentenceTexts;
    }
}
