using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using HodiaInCSharp.Types;

namespace ExperimentalMonads.Monads {
    /// <summary>
    ///     The compile time nullable type.
    ///     A life saver and very useful.
    /// </summary>
    public class Option: Monad<Option> {
        public IMonad<Option, A> pure<A>(A a) {
            return new Some<A>(a);
        }

        public static IMonad<Option, A> pureS<A>(A a) {
            return new Some<A>(a);
        }


        public static IMonad<Option, A> none<A>() {
            return new None<A>();
        }
    }

    public static class OptionExtensions {
        public static Boolean isSome<A>(this IMonad<Option, A> option) {
            var some = option as Some<A>;

            return some != null;
        }

        public static Boolean isSome<A>(this OptionMonad<A> option) {
            var some = option as Some<A>;

            return some != null;
        }

        public static OptionMonad<A> convertFromNullable<A>(this Nullable<A> nullable) 
            where A: struct {
            if (nullable.HasValue) {
                return new Some<A>(nullable.Value);
            } else {
                return new None<A>();
            }
        }

        public static OptionMonad<A> FirstOption<A>(this IQueryable<A> iQueryable,
            Expression<Func<A, bool>> whereClause) {

            try {
                A a = iQueryable.First<A>(whereClause);
                return new Some<A>(a);
            } catch (InvalidOperationException) {
                return new None<A>();
            } catch (ArgumentNullException) {
                return new None<A>();
            }
        }

        public static OptionMonad<A> FirstOption<A>(this IQueryable<A> iQueryable) {
            try {
                if (iQueryable.Count() > 0) {
                    return new Some<A>(iQueryable.First<A>());
                } else {
                    return new None<A>();
                }
                //return iQueryable.Count() > 0 ? (OptionMonad<A>)new Some<A>(iQueryable.First<A>()) : new None<A>();
            } catch (InvalidOperationException) {
                return new None<A>();
            } catch (ArgumentNullException) {
                return new None<A>();
            }
        }

        public static OptionMonad<A> FirstOption<A>(this IList<A> iList,
            Func<A, bool> whereClause) {
                try {
                    A a = iList.First(whereClause);
                    return new Some<A>(a);
                } catch {
                    return new None<A>();
                }
        }

        public static OptionMonad<B> GetValue<A, B>(this IDictionary<A, B> iDictionary, A key) {
            try {
                return iDictionary.ContainsKey(key) ?
                    (OptionMonad<B>)new Some<B>(iDictionary[key]) : new None<B>();
            } catch (ArgumentNullException) {
                return new None<B>();
            } catch (KeyNotFoundException) {
                return new None<B>();
            }
        }

        public static OptionMonad<A> asInstanceOf<A>(this object a) where A: class {
            if (a != null) {
                var conversion = a as A;
                if (conversion != null) {
                    return new Some<A>(conversion);
                } else {
                    return new None<A>();
                }
            } else {
                return new None<A>();
            }
        }

        public static OptionMonad<A> convertFromClass<A>(this A a) where A : class {
            if (a != null) {
                return new Some<A>(a);
            } else {
                return new None<A>();
            }
        }

        public static A getValueOrDefault<A>(this IMonad<Option, A> optionM, 
            Func<A> defaultBlock) {
            var some = optionM as Some<A>;
            if (some != null) {
                return some.value;
            } else {
                return defaultBlock();
            }
        }

        public static Unit getValueOrDefault(this IMonad<Option, Unit> option,
            Action defaultBlock) {
            return option.getValueOrDefault(defaultBlock.convertToFunc());
        }
    }

    public class OptionMonad<A> : IMonad<Option, A> {
        public IMonad<Option, B> pure<B>(B b) {
            return new Some<B>(b);
        }

        public IMonad<Option, B> map<B>(Func<A, B> f) {
            Some<A> some = this as Some<A>;

            if (some != null) {
                return new Some<B>(f(some.value));
            } else {
                return new None<B>();
            }
        }

        public IMonad<Option, Unit> map(Action<A> action) {
            return this.map(action.convertToFunc());
        }

        public IMonad<Option, B> bind<B>(Func<A, IMonad<Option, B>> f) {
            Some<A> some = this as Some<A>;

            if (some != null) {
                return f(some.value);
            } else {
                return new None<B>();
            }
        }
    }

    public class Some<A> : OptionMonad<A> {
        public readonly A value;

        public Some(A a) {
            this.value = a;
        }
    }

    public class None<A> : OptionMonad<A> {
        public None() { }
    }
}
