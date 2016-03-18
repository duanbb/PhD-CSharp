using MultilabelEstimation.Consistency;
using System;
using System.Collections.Generic;

namespace MultilabelEstimation.Algorithm.DDS.TDDS.Distinct
{
    static class DTDDSVariable
    {
        static public Smoothing SmoothTree = Smoothing.None;
        static public Sij Sij;
        static public Pj Pj;
        static public PAkjl PAkjl;
        static public Pdata Pdata;
        static public Mcj Mcj;
        static public IDictionary<Tuple<Labelset, Labelset>, double> ConditionalPj = new Dictionary<Tuple<Labelset, Labelset>, double>();
        static public IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> ConditionalMcj = new Dictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>>();
    }
}
