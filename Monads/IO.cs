/*
 * Created by SharpDevelop.
 * User: AJELOVIC
 * Date: 27/Feb/2013
 * Time: 9:21 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using HodiaInCSharp.Types;

namespace ExperimentalMonads.Monads
{
	/// <summary>
	///     The IO monad that captures functions with side effecting operations
    ///     like writing/reading to a file, modifying mutable memory, and  
	/// </summary>
	public class IO: Monad<IO> {
        /// <summary>
        ///     Wraps a pure value into the IO monad
        /// </summary>
        /// <typeparam name="A">the type of the pure value</typeparam>
        /// <param name="a">the value of the pure value</param>
        /// <returns>The IO monad with the pure value of type A</returns>
        public IMonad<IO, A> pure<A>(A a) {
			return new IOMonad<A>(() => a);
		}

        public static IMonad<IO, A> pureS<A>(A a) {
            return new IOMonad<A>(() => a);
        }

        public static IMonad<IO, A> withDisposable<A, B>(Func<B> openHandle, Func<B, A> during) where B: IDisposable{
            using(var b = openHandle()) {
                return IO.pureS(during(b));
            }
        }

        public static IMonad<IO, Unit> withDisposable<B>(Func<B> openHandle, Action<B> during) where B : IDisposable {
            using (var b = openHandle()) {
                return IO.pureS(during.convertToFunc()(b));
            }
        }

        
	}

    public static class IOExtensions {
        public static A unsafePerformIO<A>(this IMonad<IO, A> ioMonad) {
            return ((IOMonad<A>)ioMonad).UnsafePerformIO();
        }
    }
	
	public class IOMonad<A>: IMonad<IO, A> {
		private readonly Func<A> unsafePerformIO;
		
		public IOMonad(Func<A> unsafePerformIO) {
			this.unsafePerformIO = unsafePerformIO;
		}
		
		public IMonad<IO, B> pure<B>(B b) {
			return new IOMonad<B>(() => b);
		}

        public A UnsafePerformIO() {
            return this.unsafePerformIO();
        }

		public IMonad<IO, B> map<B>(Func<A, B> f) {
			return new IOMonad<B>(() => f(this.unsafePerformIO()));
		}

        public IMonad<IO, Unit> map(Action<A> a) {
            return this.map(a.convertToFunc());
        }
		
		public IMonad<IO, B> bind<B>(Func<A, IMonad<IO, B>> f) {
            var execution = f(this.unsafePerformIO());
            IOMonad<B> evaluatedMonad = (IOMonad<B>)execution.map(b => (B)b);

			
			return new IOMonad<B>(() => evaluatedMonad.unsafePerformIO());
		}
	}
}
