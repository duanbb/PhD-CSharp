using MultilabelEstimation.Consistency;
using System;
using System.Collections.Generic;

namespace MultilabelEstimation.Algorithm.Personality
{
    enum Will { strong, weak }
    enum PorSForJointje { P, S }
    enum BnOrNot { Yes, No }
    enum ExchangeLabel { Yes, No }
    enum TransGoldStandard { Yes, No }

    static class PersonalityVariable
    {
        static public IList<Label> StrongAffects;
        static public IList<Label> WeakAffects;
        static public IList<Label> MediumAffects;
        static public IDictionary<Character, Tuple<Will, string>> TruePersonality;
        static public TransGoldStandard TransGoldStandard;
        static public ExchangeLabel ExchangeLabel;

        static PersonalityVariable()
        {
            StrongAffects = new List<Label>();
            //StrongAffects.Add(Label.happiness);
            //StrongAffects.Add(Label.anger);
            //StrongAffects.Add(Label.surprise);
            //StrongAffects.Add(Label.excitement);

            StrongAffects.Add(Label.happiness);
            StrongAffects.Add(Label.excitement);
            StrongAffects.Add(Label.surprise);
            StrongAffects.Add(Label.fondness);

            WeakAffects = new List<Label>();
            //WeakAffects.Add(Label.fondness);
            //WeakAffects.Add(Label.sadness);
            //WeakAffects.Add(Label.shame);
            //WeakAffects.Add(Label.relief);

            WeakAffects.Add(Label.relief);
            WeakAffects.Add(Label.fear);
            WeakAffects.Add(Label.shame);
            WeakAffects.Add(Label.disgust);

            MediumAffects = new List<Label>();
            //MediumAffects.Add(Label.disgust);
            //MediumAffects.Add(Label.fear);

            MediumAffects.Add(Label.sadness);
            MediumAffects.Add(Label.anger);
        }
    }
}