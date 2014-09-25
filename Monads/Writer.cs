/*
 * Created by SharpDevelop.
 * User: AJELOVIC
 * Date: 26/Feb/2013
 * Time: 2:15 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using HodiaInCSharp.Tuples;
using HodiaInCSharp.Types;

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
			Tuple2<W, A> tuple = new Tuple2<W, A>((W)this.monoid.empty(), a);
			
			return new WriterMonad<W, S, A>(tuple);
		}
	}
	
	public class WriterMonad<W, S, A>: IMonad<Writer<W, S>, A> where W: IMonoid<S> {
		public readonly Tuple2<W, A> runWriter;
		
		public WriterMonad(Tuple2<W, A> runWriter) {
			this.runWriter = runWriter;
		}
		
		public IMonad<Writer<W, S>, B> pure<B>(B b) {
			return new WriterMonad<W, S, B>(new Tuple2<W, B>((W)runWriter.a.empty(), b));
		}
		
		public IMonad<Writer<W, S>, B> map<B>(Func<A, B> f) {

			return new WriterMonad<W, S, B>(new Tuple2<W, B>(this.runWriter.a,
			                                                f(this.runWriter.b)));
		}

        public IMonad<Writer<W, S>, Unit> map(Action<A> action) {
            return this.map(action.convertToFunc());
        }

		public IMonad<Writer<W, S>, B> bind<B>(Func<A, IMonad<Writer<W, S>, B>> f) {
			WriterMonad<W, S, B> evaluationMonad = 
				(WriterMonad<W, S, B>)f(this.runWriter.b);
			
			W appendedLog = 
				(W)this.runWriter.a.append(evaluationMonad.runWriter.a);
			
			return new WriterMonad<W, S, B>(
				new Tuple2<W, B>(appendedLog, evaluationMonad.runWriter.b));
		}
	}
}
