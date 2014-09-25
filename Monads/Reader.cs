using HodiaInCSharp.Types;
/*
 * Created by SharpDevelop.
 * User: AJELOVIC
 * Date: 26/Feb/2013
 * Time: 10:14 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
namespace ExperimentalMonads.Monads
{
	/// <summary>
	/// Description of Reader.
	/// </summary>
	public class Reader<R>: Monad<Reader<R>> {
		public IMonad<Reader<R>, A> pure<A>(A a) {
			Func<R, A> f = ((R r) => a);
			
			return new ReaderMonad<R, A>(f);
		}

        public static IMonad<Reader<R>, A> pureS<A>(A a) {
            Func<R, A> f = ((R r) => a);

            return new ReaderMonad<R, A>(f);
        }
	}

    public class Reader {
        public static IMonad<Reader<R>, R> ask<R>() {
            Func<R, R> id = ((R r) => r);

            return new ReaderMonad<R, R>(id);
        }
    }

    public static class ReaderExtensions {
        public static Func<R, A> runReader<R, A>(this IMonad<Reader<R>, A> reader) {
            return ((ReaderMonad<R, A>)reader).runReader;
        }
    }
	
	public class ReaderMonad<R, A>: IMonad<Reader<R>, A> {
		public readonly Func<R, A> runReader;
		
		public ReaderMonad(Func<R, A> runReader) {
			this.runReader = runReader;
		}
		
		public IMonad<Reader<R>, B> pure<B>(B b) {
			Func<R, B> f = ((R r) => b);
			
			return new ReaderMonad<R, B>(f);
		}
		
		public IMonad<Reader<R>, B> map<B>(Func<A, B> f) {
            Func<R, B> newRunReader = ((R r) => {
                var a = this.runReader(r);
			    return f(a);
			});
			
			return new ReaderMonad<R, B>(newRunReader);
		}

        public IMonad<Reader<R>, Unit> map(Action<A> action) {
            return this.map(action.convertToFunc());
        }
		
		public IMonad<Reader<R>, B> bind<B>(Func<A, IMonad<Reader<R>, B>> f) {
			Func<R, B> newRunReader = ((R r) => {
				ReaderMonad<R, B> insideReaderMonad = 
					(ReaderMonad<R, B>)f(this.runReader(r));
				return insideReaderMonad.runReader(r);
			});
			
			return new ReaderMonad<R, B>(newRunReader);
		}
	}
}
