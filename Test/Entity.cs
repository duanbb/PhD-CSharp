using System;

namespace Test
{
    abstract class C
    {
        //int c;
        public C()
        {
            //c = 1;
        }
    }

    class A : IEquatable<A>
    {
        int Value;
        public A(int value)
        {
            Value = value;
        }

        public bool Equals(A a)
        {
            return this.Value == a.Value;
        }

        public override bool Equals(object obj)
        {
            A a = obj as A;
            return this.Value == a.Value;
        }

        public override int GetHashCode()
        {
            return this.Value;
        }
    }

    class B
    {
        string ID;
        double Value;
        public B(string id, double value)
        {
            this.ID = id;
            this.Value = value;
        }
    }
}