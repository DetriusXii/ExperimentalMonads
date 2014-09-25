using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HodiaInCSharp.Tuples {
    public class Tuple {
        public static Tuple2<A, B> tuple<A, B>(A a, B b) {
            return new Tuple2<A, B>(a, b);
        }

        public static Tuple3<A, B, C> tuple<A, B, C>(A a, B b, C c) {
            return new Tuple3<A, B, C>(a, b, c);
        }

        public static Tuple4<A, B, C, D> tuple<A, B, C, D>(A a, B b, C c, D d) {
            return new Tuple4<A, B, C, D>(a, b, c, d);
        }
    }

    public class Tuple2<A, B> {
        public readonly A a;
        public readonly B b;

        public Tuple2(A a, B b) {
            this.a = a;
            this.b = b;
        }
    }

    public class Tuple3<A, B, C> {
        public readonly A a;
        public readonly B b;
        public readonly C c;

        public Tuple3(A a, B b, C c) {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }

    public class Tuple4<A, B, C, D> {
        public readonly A a;
        public readonly B b;
        public readonly C c;
        public readonly D d;

        public Tuple4(A a, B b, C c, D d) {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }
    }
}
