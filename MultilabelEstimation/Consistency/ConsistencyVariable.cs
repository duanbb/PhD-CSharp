using System;
using System.Collections.Generic;

namespace MultilabelEstimation.Consistency
{
    sealed class Character : IEquatable<Character>
    {
        public string ID;
        public IList<Sentence> Sentences;//角色及其台词
        public Character(string id)
        {
            this.ID = id;
            this.Sentences = new List<Sentence>();
        }

        public bool Equals(Character other)
        {
            return this.ID == other.ID;
        }

        public override bool Equals(object obj)//必须重写这个，Tuple<Character, Character> 才能用作Key来索引Dictionary
        {
            Character character = obj as Character;
            return character.Equals(this);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public override string ToString()
        {
            return this.ID;
        }
    }

    static class ConsistencyVariable
    {
        static public List<Character> Characters;

        static ConsistencyVariable()
        {
            Characters = new List<Character>();
        }
    }
}