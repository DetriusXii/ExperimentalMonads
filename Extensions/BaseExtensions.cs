using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExperimentalMonads.Monads;
using HodiaInCSharp.Types;

namespace HodiaInCSharp.Extensions {
    public static class BaseExtensions {
        public static A FoldUpTo<T, A>(this T[] array,
            A initialValue,
            Func<T, A, A> foldF, int count) {
            A accumulatedValue =  initialValue;

            for (int i = 0; i < count && i < array.Length; i++) {
                T item = array[i];
                accumulatedValue = foldF(item, accumulatedValue);

            }

            return accumulatedValue;
        }

        public static OptionMonad<A> FindFirstOption<A>(this List<A> list,
            Func<A, bool> f) {
            OptionMonad<A> foundOption = new None<A>();
            
            foreach (A a in list) {
                if (f(a)) {
                    foundOption = new Some<A>(a);
                    break;
                }
            }

            return foundOption;
        }

        public static Unit Discard<A>(this A a) {
            return Unit.Instance;
        }
    }
}
