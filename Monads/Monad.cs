using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperimentalMonads.Monads {
    /// <summary>
    ///     Interface that monadic computations need to handle
    /// </summary>
    /// <typeparam name="T">
    ///     This is the type of the higher kinded monad generic
    /// </typeparam>
    /// <typeparam name="A">
    ///     This is the type of the free value that the monad applies
    /// </typeparam>
    public interface IMonad<T, out A> where T : Monad<T> {
        /// <summary>
        ///     Wraps a value in a specific monad
        /// </summary>
        /// <typeparam name="B">The type of the free value</typeparam>
        /// <param name="b">The value of the free value</param>
        /// <returns>The monad with the free value wrapped in it</returns>
        IMonad<T, B> pure<B>(B b);
        /// <summary>
        ///     Transforms the free value
        /// </summary>
        /// <typeparam name="B">The result when f is applied to the map</typeparam>
        /// <param name="f">The function to transform the value from A to B</param>
        /// <returns>The monad with transformed value B</returns>
        IMonad<T, B> map<B>(Func<A, B> f);
        IMonad<T, Unit> map(Action<A> a);
        /// <summary>
        ///     Transforms the free value
        /// </summary>
        /// <typeparam name="B">
        ///     The type of the new free value
        /// </typeparam>
        /// <param name="f">The sequencing operation that the original 
        ///     monad applies to
        /// </param>
        /// <returns>The new monad with type B</returns>
        IMonad<T, B> bind<B>(Func<A, IMonad<T, B>> f);
    }


    /// <summary>
    ///     The higher kind of monad that marks the class.  Does think that
    ///     should be standard requirement for the monad.
    /// </summary>
    /// <typeparam name="M">The monad class M</typeparam>
    public interface Monad<M> where M : Monad<M> {
        IMonad<M, A> pure<A>(A a);
    }


    /// <summary>
    ///     The transformer type that marks the transformer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface Transformer<T> where T : Transformer<T>, new() {
        IMonad<MTM<T, M>, A> pure<M, A>(A a) where M : Monad<M>, new();
        IMonad<MTM<T, M>, A> lift<M, A>(IMonad<M, A> ma) where M : Monad<M>, new();
    }

    public class Transformer2<T1, T2> 
        where T1 : Transformer<T1>, new() 
        where T2 : Transformer<T2>, new() {
        public IMonad<MTM<T1, MTM<T2, M>>, A> pure<M, A>(A a) where M : Monad<M>, new() {
            var m = new M();
            var t1 = new T1();
            var t2 = new T2();

            return t1.lift(t2.lift(m.pure(a)));
        }

        public IMonad<MTM<T1, MTM<T2, M>>, A> lift<M, A>(IMonad<M, A> ma) where M : Monad<M>, new() {
            var t1 = new T1();
            var t2 = new T2();

            return t1.lift(t2.lift(ma));
        }
    }

    /// <summary>
    ///     Forms the pair for the transformer and the monad.
    ///     The transformer by itself is not a monad.  Only when coupled with another
    ///     monad does it become a monad
    /// </summary>
    /// <typeparam name="T">The type of the transformer</typeparam>
    /// <typeparam name="M">The type of the monad</typeparam>
    public class MTM<T, M> : Monad<MTM<T, M>> where T : Transformer<T>, new()
        where M : Monad<M>, new () {

        public IMonad<MTM<T, M>, A> pure<A>(A a) {
            M m = new M();
            T t = new T();

            var im = m.pure(a);
            var itm = t.lift(im);
            return itm;
        }
    }

    public interface LiftIO<M> where M : Monad<M> {
        IMonad<M, A> liftIO<A>(IMonad<IO, A> ioa);
    }

    /// <summary>
    ///     Extension methods for IMOnad. For a monad that shares the same type,
    ///     join makes monad less dense.  Some(Some(x)) becomes Some(x)
    ///     [[4, 5, 6], [2,3,4]] becomes [4,5,6,2,3,4]
    /// </summary>
    public static class IMonadExtensions { 
        public static IMonad<T, A> join<T, A>(this IMonad<T, IMonad<T, A>> unjoined) where T : Monad<T> {
            return unjoined.bind(inner => inner);
        }
    }
}
