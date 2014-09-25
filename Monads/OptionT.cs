using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExperimentalMonads.Monads;
using HodiaInCSharp.Types;

namespace ExperimentalMonads.Monads {
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="I"></typeparam>
    public class OptionT<I>: Monad<OptionT<I>> where I: Monad<I>, new() {
        public IMonad<OptionT<I>, A> pure<A>(A a) {
            I inner = new I();
            var innerPure = inner.pure(a);
            var mappedInner = innerPure.map((A secondA) => 
                (IMonad<Option, A>)Option.pureS(secondA));

            return new OptionTMonad<I, A>(mappedInner);
        }

        public static IMonad<OptionT<I>, A> pureS<A>(A a) {
            I inner = new I();
            var innerPure = inner.pure(a);
            var mappedInner = innerPure.map((A secondA) =>
                (IMonad<Option, A>)Option.pureS(secondA));

            return new OptionTMonad<I, A>(mappedInner);
        }
    }

    public class OptionTMonad<I, A> : IMonad<OptionT<I>, A> where I : Monad<I>, new() {
        public readonly IMonad<I, IMonad<Option, A>> runMaybeT;

        public OptionTMonad(IMonad<I, IMonad<Option, A>> runMaybeT) {
            this.runMaybeT = runMaybeT;
        }

        public IMonad<OptionT<I>, B> pure<B>(B b) {
            I inner = new I();
            var innerPure = inner.pure(b);
            var mappedInner = innerPure.map((B secondB) =>
                (IMonad<Option, B>)Option.pureS(secondB));

            return new OptionTMonad<I, B>(mappedInner);
        }

        public IMonad<OptionT<I>, B> map<B>(Func<A, B> f) {
            var newRunMaybeT = this.runMaybeT.map((IMonad<Option, A> optionA) => {
                var some = optionA as Some<A>;
                if (some != null) {
                    return Option.pureS(f(some.value)).map(b => b);
                } else {
                    return Option.none<B>();
                }
            });

            return new OptionTMonad<I, B>(newRunMaybeT);
        }

        public IMonad<OptionT<I>, Unit> map(Action<A> action) {
            return this.map(action.convertToFunc());
        }

        public IMonad<OptionT<I>, B> bind<B>(Func<A, IMonad<OptionT<I>, B>> f) {
            I inner = new I();

            var newRunMaybeT = this.runMaybeT.bind((IMonad<Option, A> option) => {
                var some = option as Some<A>;
                if (some != null) {
                    var evaluatedMonad = (OptionTMonad<I, B>)f(some.value);
                    return evaluatedMonad.runMaybeT;
                } else {
                    return inner.pure((IMonad<Option, B>)Option.none<B>());
                }
            });

            return new OptionTMonad<I, B>(newRunMaybeT);
        }
    }

    public static class OptionTExtensions {
        public static IMonad<M, A> GetValueOrDefault<M, A>(
            this IMonad<OptionT<M>, A> optionT, Func<A> defaultBlock) 
            where M : Monad<M>, new() {
            var properOptionT = (OptionTMonad<M, A>)optionT;
            return properOptionT.runMaybeT.map(option =>
                option.getValueOrDefault(defaultBlock));
        }

        public static IMonad<OptionT<M>, A> MakeOptionT<M, A>(this IMonad<M, IMonad<Option, A>> optionM) 
            where M : Monad<M>, new() {
                return new OptionTMonad<M, A>(optionM);
        }
    }
}
