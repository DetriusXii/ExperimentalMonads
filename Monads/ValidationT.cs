using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperimentalMonads.Monads {
    public class ValidationT<E> : Transformer<ValidationT<E>> {

        public IMonad<MTM<ValidationT<E>, M>, A> pure<M, A>(A a) where M : Monad<M>, new() {
            var m = new M();
            return new ValidationTMonad<M, E, A>(m.pure(Validation<E>.pureS(a)));
        }

        public IMonad<MTM<ValidationT<E>, M>, A> lift<M, A>(IMonad<M, A> ma) where M : Monad<M>, new() {
            return new ValidationTMonad<M, E, A>(ma.map(a => Validation<E>.pureS(a)));
        }
    }

    public static class ValidationTExtensions {
        public static IMonad<I, IMonad<Validation<E>, A>>
            RunValidationT<I, E, A>(this IMonad<MTM<ValidationT<E>, I>, A> v) where I: Monad<I>, new() {
                return ((ValidationTMonad<I, E, A>)v).runValidationT;
        }

        public static IMonad<I, A> GetValueOrDefault<I, E, A>(
            this IMonad<MTM<ValidationT<E>, I>, A> v, Func<E, IMonad<I, A>> handleError) where
            I : Monad<I>, new() { 
            I i = new I();

            return v.RunValidationT().bind(validation =>
                validation.map(t => i.pure(t)).getValueOrDefault(handleError)); 
        }

        public static ValidationTMonad<I, E, A> MakeValidationT<I, E, A>(this IMonad<I, IMonad<Validation<E>, A>> monad) 
            where I: Monad<I>, new() {
            return new ValidationTMonad<I, E, A>(monad);
        }
    }

    public class ValidationTMonad<I, E, A> : IMonad<MTM<ValidationT<E>, I>, A> where I: Monad<I>, new() {
        public readonly IMonad<I, IMonad<Validation<E>, A>> runValidationT;
        
        public ValidationTMonad(IMonad<I, ValidationMonad<E, A>> runValidationT) {
        	this.runValidationT = runValidationT.map((originalForm) => {
        		IMonad<Validation<E>, A> newForm = originalForm;
				return newForm;        		
        	});
        }

        public ValidationTMonad(IMonad<I, IMonad<Validation<E>, A>> runValidationT) {
            this.runValidationT = runValidationT;
        }
        
        public IMonad<MTM<ValidationT<E>, I>, B> pure<B>(B b) {
            ValidationMonad<E, B> success = new Success<E, B>(b);
            IMonad<I, 
                ValidationMonad<E, B>> innerMonad = runValidationT.pure(success);

            return new ValidationTMonad<I, E, B>(innerMonad);
        }

        public IMonad<MTM<ValidationT<E>, I>, B> map<B>(Func<A, B> f) {
            var newRunValidationT = runValidationT.bind<IMonad<Validation<E>, B>>(
                (IMonad<Validation<E>, A> validation) => {
                    var success = validation as Success<E, A>;

                    if (success != null) {
                        IMonad<Validation<E>, B> newSuccess =
                            success.pure(f(success.value));
                        return runValidationT.pure(newSuccess);
                    } else {
                        var failure = (Failure<E, A>)validation;
                        IMonad<Validation<E>, B> newFailure = 
                            new Failure<E, B>(failure.failureValue);

                        return runValidationT.pure(newFailure);
                    }
                });

            return new ValidationTMonad<I, E, B>(newRunValidationT);
        }

        public IMonad<MTM<ValidationT<E>, I>, Unit> map(Action<A> action) {
            return this.map(action.convertToFunc());
        }

        public IMonad<MTM<ValidationT<E>, I>, B> bind<B>(
            Func<A, IMonad<MTM<ValidationT<E>, I>, B>> f) {
            var newRunValidationT = runValidationT.bind<IMonad<Validation<E>, B>>(
                (IMonad<Validation<E>, A> validation) => {
                    var success = validation as Success<E, A>;

                    if (success != null) {
                        var newValidationT =
                            (ValidationTMonad<I, E, B>)f(success.value);
                        return newValidationT.runValidationT;
                    } else {
                        var failure = (Failure<E, A>)validation;
                        IMonad<Validation<E>, B> newFailure =
                            new Failure<E, B>(failure.failureValue);

                        return runValidationT.pure(newFailure);
                    }
            });

            return new ValidationTMonad<I, E, B>(newRunValidationT);
        }
    }
}
