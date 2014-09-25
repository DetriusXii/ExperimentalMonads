using HodiaInCSharp.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperimentalMonads.Monads {
    /// <summary>
    ///     The identity Monad is one of the basic monads that satisfy the monad laws.
    ///     There is no need to use it.
    /// </summary>
    public class Identity: Monad<Identity> {
        public IMonad<Identity, A> pure<A>(A a) {
            return new IdentityMonad<A>(a);
        }
    }

    public class IdentityMonad<A> : IMonad<Identity, A> {
        public readonly A value;

        public IdentityMonad(A a) {
            this.value = a;
        }

        public IMonad<Identity, B> pure<B>(B b) {
            return new IdentityMonad<B>(b);
        }

        public IMonad<Identity, B> map<B>(Func<A, B> f) {
            return new IdentityMonad<B>(f(this.value));
        }

        public IMonad<Identity, Unit> map(Action<A> a) {
            return this.map(a.convertToFunc());
        }

        public IMonad<Identity, B> bind<B>(Func<A, IMonad<Identity, B>> f) {
            return f(this.value);
        }
    }
}
