using HodiaInCSharp.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperimentalMonads.Monads {
    /// <summary>
    ///     Some static class conversions of Action to make it more expressible
    /// </summary>
    public static class FuncExtensions {
        public static Func<Unit> convertToFunc(this Action action) {
            return () => {
                action();
                return Unit.Instance;
            };
        }

        public static Func<A, Unit> convertToFunc<A>(this Action<A> action) {
            return (A a) => {
                action(a);
                return Unit.Instance;
            };
        }

        public static Func<A, B, Unit> convertToFunc<A, B>(this Action<A, B> action) {
            return (A a, B b) => {
                action(a, b);
                return Unit.Instance;
            };
        }
    }
}
