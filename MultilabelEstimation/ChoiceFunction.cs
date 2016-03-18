using MultilabelEstimation.Consistency;
using MultilabelEstimation.Relation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultilabelEstimation
{
    static class ChoiceFunction
    {
        static public void PriorPj(ref Pj pj, ref Mcj mcj, Sij sij, int time)
        {
            if(Variable.PriorP.Contains(PriorP.Pj) || Variable.PriorP.Contains(PriorP.ConditionalPj))
                pj = CoreFunction.CalculatePj(sij, time);
            if (Variable.PriorP.Contains(PriorP.Mcj) || Variable.PriorP.Contains(PriorP.ConditionalMcj))
                mcj = ConsistencyFunction.CalculateMcj(sij, time);
        }

        static public IDictionary<Tuple<Labelset, Labelset>, double> ConditionalPj(Pj pj, IDictionary<Tuple<Labelset, Labelset>, double> LabelsetPairFrequencyForSentence, IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>> independentLabelsetPairFrequencyForSentence)
        {
            IDictionary<Tuple<Labelset, Labelset>, double> conditionalPj = null;
            switch (Variable.Relation)
            {
                case RelationScheme.RenewLower:
                case RelationScheme.UpdateLower:
                case RelationScheme.RenewOne:
                case RelationScheme.UpdateOne:
                case RelationScheme.AllLower:
                    conditionalPj = RelationFunction.CalculateConditionalPj(pj, LabelsetPairFrequencyForSentence);
                    break;
                case RelationScheme.IndependentRenewLower:
                case RelationScheme.IndependentRenewOne:
                    conditionalPj = RelationFunction.CalculateIndependentConditionalPj(pj, independentLabelsetPairFrequencyForSentence);
                    break;
            }
            return conditionalPj;
        }

        static public IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> ConditionalMj(Mcj mcj, IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> LabelsetPairFrequencyForCharacter, IDictionary<Tuple<Character, Character>, IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>> independentLabelsetPairFrequencyForCharacter)
        {
            IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> conditionalMcj = null;
            switch (Variable.Relation)
            {
                case RelationScheme.RenewLower:
                case RelationScheme.UpdateLower:
                case RelationScheme.RenewOne:
                case RelationScheme.UpdateOne:
                case RelationScheme.AllLower:
                    conditionalMcj = RelationFunction.CalculateConditionalMcj(mcj, LabelsetPairFrequencyForCharacter);
                    break;
                case RelationScheme.IndependentRenewLower:
                case RelationScheme.IndependentRenewOne:
                    conditionalMcj = RelationFunction.CalculateIndependentConditionalMcj(mcj, independentLabelsetPairFrequencyForCharacter);
                    break;
            }
            return conditionalMcj;
        }

        //包括独立与非独立
        static public void InitializationOfLabelsetPairFrequencyForPj(Label[] labels, int groupIndex, ref IDictionary<Tuple<Labelset, Labelset>, double> LabelsetPairFrequencyForSentence, ref IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>> independentLabelsetPairFrequencyForSentence)
        {
            switch (Variable.Relation)
            {
                case RelationScheme.RenewLower:
                case RelationScheme.UpdateLower:
                case RelationScheme.RenewOne:
                case RelationScheme.UpdateOne:
                case RelationScheme.AllLower:
                case RelationScheme.AlwaysInitialization:
                    LabelsetPairFrequencyForSentence = RelationFunction.InitializeLabelsetPairFrequencyForPj(labels, groupIndex);
                    break;
                case RelationScheme.IndependentRenewLower:
                case RelationScheme.IndependentRenewOne:
                    independentLabelsetPairFrequencyForSentence = RelationFunction.InitializeIndependentBoolPairFrequencyForSentence(labels, groupIndex);
                    break;
            }
        }

        //包括独立与非独立
        static public void InitializationOfLabelsetPairFrequencyForMcj(Label[] labels, int groupIndex, ref IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> LabelsetPairFrequencyForCharacter, ref IDictionary<Tuple<Character, Character>, IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>> independentLabelsetPairFrequencyForCharacter)
        {
            switch (Variable.Relation)
            {
                case RelationScheme.RenewLower:
                case RelationScheme.UpdateLower:
                case RelationScheme.RenewOne:
                case RelationScheme.UpdateOne:
                case RelationScheme.AllLower:
                case RelationScheme.AlwaysInitialization:
                    LabelsetPairFrequencyForCharacter = RelationFunction.InitializeLabelsetPairFrequencyForMcj(labels, groupIndex);
                    break;
                case RelationScheme.IndependentRenewLower:
                case RelationScheme.IndependentRenewOne:
                    independentLabelsetPairFrequencyForCharacter = RelationFunction.InitializeIndependentLabelsetPairFrequencyForCharacter(labels, groupIndex);
                    break;
            }
        }

        static public void InitializationOfLabelsetPairFrequencyForSij(Label[] labels, int groupIndex, ref IDictionary<Tuple<Sentence, Sentence>, IDictionary<Tuple<Labelset, Labelset>, double>> LabelsetPairFrequencyForSij, ref IDictionary<Tuple<Sentence, Sentence>, IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>> independentLabelsetPairFrequencyForSij)
        {
            switch (Variable.Relation)
            {
                case RelationScheme.RenewLower:
                case RelationScheme.UpdateLower:
                case RelationScheme.RenewOne:
                case RelationScheme.UpdateOne:
                case RelationScheme.AllLower:
                case RelationScheme.AlwaysInitialization:
                    LabelsetPairFrequencyForSij = RelationFunction.InitializeLabelsetPairFrequencyForSij(labels, groupIndex);
                    break;
                case RelationScheme.IndependentRenewLower:
                case RelationScheme.IndependentRenewOne:
                    //independentLabelsetPairFrequencyForSij = RelationFunction.InitializeIndependentLabelsetPairFrequencyForSij(labels, groupIndex);
                    break;
            }
        }

        //包括独立与非独立
        static public void UpdateLabelsetPairFrequencyForPj(Sij sij, ref IDictionary<Tuple<Labelset, Labelset>, double> LabelsetPairFrequencyForSentence, Label[] labels, ref IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>> independentLabelsetPairFrequencyForSentence)
        {
            switch (Variable.Relation)
            {
                case RelationScheme.RenewLower:
                    #region 取相邻两者最小值作为联合概率
                    LabelsetPairFrequencyForSentence = RelationFunction.RenewLabelsetPairFrequencyForSentence(false, sij);
                    #endregion
                    break;
                case RelationScheme.UpdateLower:
                    #region 取相邻两者最小值作为联合概率，不重新计算
                    RelationFunction.UpdateLabelsetPairFrequencyForSentence(false, sij, ref LabelsetPairFrequencyForSentence);
                    #endregion
                    break;
                case RelationScheme.RenewOne:
                    #region 根据新结果重新计算labelPair的频率
                    LabelsetPairFrequencyForSentence = RelationFunction.RenewLabelsetPairFrequencyForSentence(true, sij);
                    #endregion
                    break;
                case RelationScheme.UpdateOne:
                    #region 根据新结果将labelPair的频率添加到原有频率中，不重新计算
                    RelationFunction.UpdateLabelsetPairFrequencyForSentence(true, sij, ref LabelsetPairFrequencyForSentence);
                    #endregion
                    break;
                case RelationScheme.AllLower:
                    LabelsetPairFrequencyForSentence = RelationFunction.AllLabelsetPairFrequencyForSentence(sij);
                    break;
                case RelationScheme.IndependentRenewLower:
                    independentLabelsetPairFrequencyForSentence = RelationFunction.RenewIndependentLabelsetPairFrequencyForSentence(false, labels, sij);
                    break;
                case RelationScheme.IndependentRenewOne:
                    independentLabelsetPairFrequencyForSentence = RelationFunction.RenewIndependentLabelsetPairFrequencyForSentence(true, labels, sij);
                    break;
            }
        }

        //包括独立与非独立
        static public void UpdateLabelsetPairFrequencyForMcj(Sij sij, ref IDictionary<Tuple<Character, Character>, IDictionary<Tuple<Labelset, Labelset>, double>> LabelsetPairFrequencyForCharacter, Label[] labels, ref IDictionary<Tuple<Character, Character>, IDictionary<Label, IDictionary<Tuple<Labelset, Labelset>, double>>> independentLabelsetPairFrequencyForCharacter)
        {
            switch (Variable.Relation)
            {
                case RelationScheme.RenewLower:
                    #region 取相邻两者最小值作为联合概率
                    LabelsetPairFrequencyForCharacter = RelationFunction.RenewLabelsetPairFrequencyForCharacter(false, sij);
                    #endregion
                    break;
                case RelationScheme.UpdateLower:
                    #region 取相邻两者最小值作为联合概率，不重新计算
                    RelationFunction.UpdateLabelsetPairFrequencyForCharacter(false, sij, ref LabelsetPairFrequencyForCharacter);
                    #endregion
                    break;
                case RelationScheme.RenewOne:
                    #region 根据新结果重新计算labelPair的频率
                    LabelsetPairFrequencyForCharacter = RelationFunction.RenewLabelsetPairFrequencyForCharacter(true, sij);
                    #endregion
                    break;
                case RelationScheme.UpdateOne:
                    #region 根据新结果将labelPair的频率添加到原有频率中，不重新计算
                    RelationFunction.UpdateLabelsetPairFrequencyForCharacter(true, sij, ref LabelsetPairFrequencyForCharacter);
                    #endregion
                    break;
                case RelationScheme.AllLower:
                    LabelsetPairFrequencyForCharacter = RelationFunction.AllLabelsetPairFrequencyForCharacter(sij);
                    break;
                case RelationScheme.IndependentRenewLower:
                    independentLabelsetPairFrequencyForCharacter = RelationFunction.RenewIndependentLabelPairFreuquencyForCharacter(false, labels, sij);
                    break;
                case RelationScheme.IndependentRenewOne:
                    independentLabelsetPairFrequencyForCharacter = RelationFunction.RenewIndependentLabelPairFreuquencyForCharacter(true, labels, sij);
                    break;
            }
        }
    }
}