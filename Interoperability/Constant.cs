using Interoperability.Entity;
using System.Collections.Generic;

namespace Interoperability
{
    enum TaxonomyType
    {
        Ekman, Nakamura
    }

    enum SourceOrTarget
    {
        Source, Target
    }

    enum Corpus
    {
        Love, LoveSample, Apple, AppleSample
    }

    enum Label
    {
        喜Joy, 好Fondness, 安Relief, 怒Anger, 哀Sadness, 怖Fear, 恥Shame, 厭Disgust, 昂Excitement, 驚Surprise,
        Anger, Disgust, Fear, Joy, Sadness, Surprise,
        None
    }

    enum Gold
    {
        Top, TopTwo, Halfmore, TopAndHalfmore
    }

    enum Similarity
    {
        SMC, Jaccard, Dice
    }

    /// <summary>
    /// CompareWith: Temporary, Other.
    /// GeneralAmongSentence: Nogeneral, General.
    /// Normalize: Nonormalize, Normalize.
    /// 
    /// Nonormalize和Normalize没区别
    /// Nogeneral和General有区别
    /// Temporary和Other没区别
    /// </summary>
    enum Method
    {
        MLE,
        Aggregation,
        OrdinaryCombination,
        WeightedCombination, //OtherNogeneralNonormalize, 
        ExpertiseCombination,
        #region 废弃方法
        TemporaryNogeneralNonormalize, 
        TemporaryNogeneralNormalize,
        TemporaryGeneralNonormalize, 
        TemporaryGeneralNormalize,
        OtherNogeneralNormalize,
        OtherGeneralNonormalize,
        OtherGeneralNormalize,
        #endregion

        //TODO
        Cascaded,

        TestExpertise
    }

    enum Filter
    {
        More, Less, Ordinary
    }

    static class Constant
    {
        static public Label[] NakaLabelArray;
        static public Label[] EkmanLabelArray;
        static public Taxonomy SourceTaxonomy;
        static public Taxonomy TargetTaxonomy;
        static public Gold Gold;
        static public Similarity Similarity;
        static public List<Sentence> SentenceList;
        static public Method[] Methods;
        static public Filter Filter;
        static public string Output;

        static Constant()
        {
            NakaLabelArray = new Label[] { Label.喜Joy, Label.好Fondness, Label.安Relief, Label.怒Anger, Label.哀Sadness, Label.怖Fear, Label.恥Shame, Label.厭Disgust, Label.昂Excitement, Label.驚Surprise };
            EkmanLabelArray = new Label[] { Label.Anger, Label.Disgust, Label.Fear, Label.Joy, Label.Sadness, Label.Surprise };
            SentenceList = new List<Sentence>();
        }
    }

    static class TrainConstant
    {
        static public Corpus Corpus;
        static public IList<Sentence> SentenceList;
        static public IList<TargetWorker> TargetWorkerList;
        static public IList<SourceWorker> SourceWorkerList;
        static TrainConstant()
        {
            //SentenceList = new List<Sentence>();
            TargetWorkerList = new List<TargetWorker>();
            SourceWorkerList = new List<SourceWorker>();
        }
    }

    static class NotTrainConstant
    {
        static public Corpus Corpus;
        static public IList<Sentence> SentenceList;//其实没用
        static public IList<TargetWorker> TargetWorkerList;//其实没用
        static public IList<SourceWorker> SourceWorkerList;//其实没用
        static NotTrainConstant()
        {
            //SentenceList = new List<Sentence>();
            TargetWorkerList = new List<TargetWorker>();
            SourceWorkerList = new List<SourceWorker>();
        }
    }

    struct Taxonomy
    {
        public TaxonomyType Name;
        public Label[] LabelArray;
        public Taxonomy(TaxonomyType name, Label[] labelArray)
        {
            this.Name = name;
            this.LabelArray = labelArray;
        }
    }
}