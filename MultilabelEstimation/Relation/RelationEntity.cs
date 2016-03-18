using System;
using System.Collections.Generic;
using MultilabelEstimation.Consistency;

namespace MultilabelEstimation.Relation
{
    static class RelationVariable
    {
        static public IList<Character> Characters;
        static RelationVariable()
        {
            Characters = new List<Character>(ConsistencyVariable.Characters);
            Characters.Insert(0, (new Character("##")));//加开头结尾
        }
    }
}