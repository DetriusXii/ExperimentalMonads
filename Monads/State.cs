/*
 * Created by SharpDevelop.
 * User: AJELOVIC
 * Date: 26/Feb/2013
 * Time: 3:24 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace ExperimentalMonads.Monads
{
	/// <summary>
	/// Description of State.
	/// </summary>
	public class State<S>: Monad<State<S>>
	{
		public IMonad<State<S>, A> pure<A>(A a) {
			return new StateMonad<S, A>((S s) => new Tuple<S, A>(s, a));
		}

        public static IMonad<State<S>, A> pureS<A>(A a) {
            return new StateMonad<S, A>((S s) => new Tuple<S, A>(s, a));
        }
	}
	
	public class StateMonad<S, A>: IMonad<State<S>, A> {
		public readonly Func<S, Tuple<S, A>> runState;
		
		public StateMonad(Func<S, Tuple<S, A>> runState) {
			this.runState = runState;
		}

        public IMonad<State<S>, S> get() { 
            Func<S, Tuple<S, S>> newRunState = (S s) => {
                var tuple = this.runState(s);
                var newTuple = new Tuple<S, S>(tuple.Item1, tuple.Item1);

                return newTuple;
            };

            return new StateMonad<S, S>(newRunState);
        }

        public IMonad<State<S>, Unit> put(S newS) {
            return new StateMonad<S, Unit>((S s) => new Tuple<S, Unit>(newS, Unit.Instance));
        }

        public IMonad<State<S>, Unit> modify(Func<S, S> modifyFunction) { 
            return this.get().bind(oldState => this.put(modifyFunction(oldState)));
        }

		public IMonad<State<S>, B> pure<B>(B b) {
			return new StateMonad<S, B>((S s) => new Tuple<S, B>(s, b));
		}
		
		public IMonad<State<S>, B> map<B>(Func<A, B> f) {
			Func<S, Tuple<S, B>> newRunState = (S s) => {
				Tuple<S, A> tuple = this.runState(s);
			
				var newTuple = new Tuple<S, B>(tuple.Item1, f(tuple.Item2));
				
				return newTuple;
			};
			

			return new StateMonad<S, B>(newRunState);
		}

        public IMonad<State<S>, Unit> map(Action<A> action) {
            return this.map(action.convertToFunc());
        }
		
		public IMonad<State<S>, B> bind<B>(Func<A, IMonad<State<S>, B>> f) {
			Func<S, Tuple<S, B>> newRunState = ((S s) => {
				var tuple = this.runState(s);
				var evaluationMonad = (StateMonad<S, B>)f(tuple.Item2);
				
				return evaluationMonad.runState(tuple.Item1);
			});
			
			return new StateMonad<S, B>(newRunState);
		}
	}

    public static class StateExtensions {
        public static IMonad<State<S>, S> Get<S, A>(this IMonad<State<S>, A> stateMonad) {
            return ((StateMonad<S, A>)stateMonad).get();
        }

        public static IMonad<State<S>, Unit> Put<S, A>(this IMonad<State<S>, A> stateMonad, S newS) {
            return ((StateMonad<S, A>)stateMonad).put(newS);
        }

        public static IMonad<State<S>, Unit> Modify<S, A>(this IMonad<State<S>, A> stateMonad, 
            Func<S, S> modifyFunction) {
                return ((StateMonad<S, A>)stateMonad).modify(modifyFunction);
        }

        public static Tuple<S, A> RunState<S, A>(this IMonad<State<S>, A> stateMonad, S s) {
            return ((StateMonad<S, A>)stateMonad).runState(s);
        }
    } 
}
