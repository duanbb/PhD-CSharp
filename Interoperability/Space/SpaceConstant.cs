
namespace Interoperability.Space
{
    static class SpaceConstant
    {
        static public int TargetWorkerNumberPerSentence;
        static public double[][] Matrix;
        static SpaceConstant()
        {
            if (TrainConstant.Corpus == Corpus.Love)
            {
                if (Constant.TargetTaxonomy.Name == TaxonomyType.Nakamura)
                    SpaceConstant.TargetWorkerNumberPerSentence = 41;
                else if(Constant.TargetTaxonomy.Name == TaxonomyType.Ekman)
                    SpaceConstant.TargetWorkerNumberPerSentence = 30;
            }
            else if (TrainConstant.Corpus == Corpus.Apple)
            {
                if (Constant.TargetTaxonomy.Name == TaxonomyType.Nakamura)
                    SpaceConstant.TargetWorkerNumberPerSentence = 40;
                else if (Constant.TargetTaxonomy.Name == TaxonomyType.Ekman)
                    SpaceConstant.TargetWorkerNumberPerSentence = 30;
            }
        }
    }
}
