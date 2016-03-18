namespace Statistic
{
    class Program
    {
        static void Main(string[] args)
        {
            Function.InitializeData();
            Function.Distribution();
            Function.LabelCountOfSentence();
            Function.Disagreement();
            Function.Mi3OfEachAnnotator();
            Function.Mi3OfAllAnnotators();
            Function.AverageAffectOfSentence();
            Function.DisagreementWithEntropy();
            Function.DistributionOfSentences();
            Variable.ResultFile.Close();
        }
    }
}
