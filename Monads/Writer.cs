/*
 * Created by SharpDevelop.
 * User: AJELOVIC
 * Date: 26/Feb/2013
 * Time: 2:15 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace ExperimentalMonads.Monads
{
	/// <summary>
	/// Description of Writer.
	/// </summary>
	public class Writer<W, S> : Monad<Writer<W, S>> where W: IMonoid<S> {
		public readonly W monoid;
		
		public Writer(W monoid) {
			this.monoid = monoid;
		}
		
		public IMonad<Writer<W, S>, A> pure<A>(A a) {
			var tuple = Tuple.Create((W)this.monoid.empty(), a);
			
			return new WriterMonad<W, S, A>(tuple);
		}
	}
	
	public class WriterMonad<W, S, A>: IMonad<Writer<W, S>, A> where W: IMonoid<S> {
		public readonly Tuple<W, A> runWriter;
		
		public WriterMonad(Tuple<W, A> runWriter) {
			this.runWriter = runWriter;
		}
		
		public IMonad<Writer<W, S>, B> pure<B>(B b) {
			return new WriterMonad<W, S, B>(new Tuple<W, B>((W)runWriter.Item1.empty(), b));
		}
		
		public IMonad<Writer<W, S>, B> map<B>(Func<A, B> f) {

			return new WriterMonad<W, S, B>(new Tuple<W, B>(this.runWriter.Item1,
			                                                f(this.runWriter.Item2)));
		}

        public IMonad<Writer<W, S>, Unit> map(Action<A> action) {
            return this.map(action.convertToFunc());
        }

		public IMonad<Writer<W, S>, B> bind<B>(Func<A, IMonad<Writer<W, S>, B>> f) {
			WriterMonad<W, S, B> evaluationMonad = 
				(WriterMonad<W, S, B>)f(this.runWriter.Item2);
			
			W appendedLog = 
				(W)this.runWriter.Item1.append(evaluationMonad.runWriter.Item1);
			
			return new WriterMonad<W, S, B>(
				new Tuple<W, B>(appendedLog, evaluationMonad.runWriter.Item2));
		}
	}
}
