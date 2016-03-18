using System;
using System.Collections.Generic;

namespace MultilabelEstimation.Algorithm.PDS
{
    sealed class LabelPairMatching : IComparable<LabelPairMatching>
    {
        public IList<LabelPair> bilabels;

        public LabelPairMatching(LabelPair bilabel0, LabelPair bilabel1, LabelPair bilabel2, LabelPair bilabel3, LabelPair bilabel4)
        {
            bilabels = new List<LabelPair>();
            bilabels.Add(bilabel0);
            bilabels.Add(bilabel1);
            bilabels.Add(bilabel2);
            bilabels.Add(bilabel3);
            bilabels.Add(bilabel4);
        }

        public LabelPairMatching(LabelPair bilabel0, LabelPair bilabel1)
        {
            bilabels = new List<LabelPair>();
            bilabels.Add(bilabel0);
            bilabels.Add(bilabel1);
        }

        public double Weight
        {
            get
            {
                double value = 0;
                foreach (LabelPair v in bilabels)
                {
                    value += v.Weight;
                }
                return value;
            }
        }

        int IComparable<LabelPairMatching>.CompareTo(LabelPairMatching other)
        {
            if (Weight > other.Weight)
                return 1;
            else if (Weight == other.Weight)
                return 0;
            else
                return -1;
        }
    }
}
