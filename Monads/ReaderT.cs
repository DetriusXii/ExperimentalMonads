using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExperimentalMonads.Monads;
using HodiaInCSharp.Types;

namespace HodiaInCSharp.Monads {
    public class ReaderT<I, R> : Monad<ReaderT<I, R>>  where I:Monad<I>, new() {
        public IMonad<ReaderT<I, R>, A> pure<A>(A a) {
            I innerMonad = new I();

            return new ReaderTMonad<I, R, A>((R r) => innerMonad.pure(a));
        }

        public static IMonad<ReaderT<I, R>, A> pureS<A>(A a) {
            I innerMonad = new I();

            return new ReaderTMonad<I, R, A>((R r) => innerMonad.pure(a));
        }

        public static ReaderTMonad<I, R, A> lift<A>(IMonad<I, A> innerMonad) {
            return new ReaderTMonad<I, R, A>((R r) => innerMonad);
        }
    }

    public class ReaderTMonad<I, R, A> : IMonad<ReaderT<I, R>, A> where I : Monad<I>, new() {
        public readonly Func<R, IMonad<I, A>> runReaderT;

        public ReaderTMonad(Func<R, IMonad<I, A>> runReaderT) {
            this.runReaderT = runReaderT;
        }

        public IMonad<ReaderT<I, R>, B> pure<B>(B b) {
            I innerMonad = new I();

            return new ReaderTMonad<I, R, B>((R r) => innerMonad.pure(b));
        }

        public IMonad<ReaderT<I, R>, B> map<B>(Func<A, B> f) {
            I innerMonad = new I();

            return new ReaderTMonad<I, R, B>((R r) => 
                this.runReaderT(r).map(f));
        }

        public IMonad<ReaderT<I, R>, Unit> map(Action<A> action) {
            return this.map(action.convertToFunc());
        }

        public IMonad<ReaderT<I, R>, B> bind<B>(
            Func<A, IMonad<ReaderT<I, R>, B>> f) {

            return new ReaderTMonad<I, R, B>((R r) => {
                var innerMonad = this.runReaderT(r);
                return innerMonad.bind((A a) => {
                    var recomputedReaderT = (ReaderTMonad<I, R, B>)f(a);
                    return recomputedReaderT.runReaderT(r);
                });
            });
        }
    }
}
