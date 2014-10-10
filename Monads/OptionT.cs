using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExperimentalMonads.Monads;

namespace ExperimentalMonads.Monads {
    public class OptionT : Transformer<OptionT> {
        public IMonad<MTM<OptionT, M>, A> pure<M, A>(A a) where M : Monad<M>, new() {
            var m = new M();
            return new OptionTMonad<M, A>(m.pure(Option.pureS(a)));
        }

        public IMonad<MTM<OptionT, M>, A> lift<M, A>(IMonad<M, A> ma) where M : Monad<M>, new() {
            return new OptionTMonad<M, A>(ma.map(a => Option.pureS(a)));
        }
    }

    public class OptionTMonad<I, A> : IMonad<MTM<OptionT, I>, A> where I : Monad<I>, new() {
        public readonly IMonad<I, IMonad<Option, A>> runMaybeT;

        public OptionTMonad(IMonad<I, IMonad<Option, A>> runMaybeT) {
            this.runMaybeT = runMaybeT;
        }

        public IMonad<MTM<OptionT, I>, B> pure<B>(B b) {
            I inner = new I();
            var innerPure = inner.pure(b);
            var mappedInner = innerPure.map((B secondB) =>
                (IMonad<Option, B>)Option.pureS(secondB));

            return new OptionTMonad<I, B>(mappedInner);
        }

        public IMonad<MTM<OptionT, I>, B> map<B>(Func<A, B> f) {
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

        public IMonad<MTM<OptionT, I>, Unit> map(Action<A> action) {
            return this.map(action.convertToFunc());
        }

        public IMonad<MTM<OptionT, I>, B> bind<B>(Func<A, IMonad<MTM<OptionT, I>, B>> f) {
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
            this IMonad<MTM<OptionT, M>, A> optionT, Func<IMonad<M, A>> defaultBlock) 
            where M : Monad<M>, new() {
            var m = new M();

            var properOptionT = (OptionTMonad<M, A>)optionT;
            return properOptionT.runMaybeT.bind(option =>
                option.map(a => m.pure(a)).getValueOrDefault(defaultBlock));
        }

        public static IMonad<MTM<OptionT, M>, A> MakeOptionT<M, A>(this IMonad<M, IMonad<Option, A>> optionM) 
            where M : Monad<M>, new() {
                return new OptionTMonad<M, A>(optionM);
        }
    }
}
