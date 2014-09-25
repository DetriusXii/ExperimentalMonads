/*
 * Created by SharpDevelop.
 * User: AJELOVIC
 * Date: 26/Feb/2013
 * Time: 2:17 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace ExperimentalMonads.Monads
{
	/// <summary>
	/// Description of Monoid.
	/// </summary>
	public abstract class IMonoid<W> {
		public readonly W value;
		
		public IMonoid(W w) {
			this.value = w;
		}
		
		public abstract IMonoid<W> pure(W w);
		public abstract IMonoid<W> empty();
		public abstract IMonoid<W> append(IMonoid<W> other);
	}
	
	public class StringMonoid: IMonoid<String> {
		public StringMonoid(String str): base(str) { }
		
		public override IMonoid<String> pure(String str) {
			return new StringMonoid(str);
		}
		
		public override IMonoid<String> empty() {
			return new StringMonoid(String.Empty);
		}
		
		public override IMonoid<String> append(IMonoid<String> other) {
			return new StringMonoid(this.value + other.value);
		}
	}
}
