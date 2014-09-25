using HodiaInCSharp.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperimentalMonads.Monads {
    public class ValidationT<I, E>: Monad<ValidationT<I, E>> where I: Monad<I>, new() {
        
        public IMonad<ValidationT<I, E>, A> pure<A>(A a) {
            ValidationMonad<E, A> success = new Success<E, A>(a);

            I innerMonad = new I();
            return new ValidationTMonad<I, E, A>(innerMonad.pure(success));
        }

        public static IMonad<ValidationT<I, E>, A> pureS<A>(A a) {
            IMonad<Validation<E>, A> success = new Success<E, A>(a);

            I innerMonad = new I();
            return new ValidationTMonad<I, E, A>(innerMonad.pure(success));
        }

        public static IMonad<ValidationT<I, E>, A> pureFailure<A>(E e) {
            IMonad<Validation<E>, A> failure = new Failure<E, A>(e);

            I innerMonad = new I();
            return new ValidationTMonad<I, E, A>(innerMonad.pure(failure));
        }
    }

    public static class ValidationTExtensions {
        public static IMonad<I, IMonad<Validation<E>, A>>
            RunValidationT<I, E, A>(this IMonad<ValidationT<I, E>, A> v) where I: Monad<I>, new() {
                return ((ValidationTMonad<I, E, A>)v).runValidationT;
        }

        public static IMonad<I, A> GetValueOrDefault<I, E, A>(
            this IMonad<ValidationT<I, E>, A> v, Func<E, IMonad<I, A>> handleError) where
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

    public class ValidationTMonad<I, E, A> : IMonad<ValidationT<I, E>, A> where I: Monad<I>, new() {
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
        
        public IMonad<ValidationT<I, E>, B> pure<B>(B b) {
            ValidationMonad<E, B> success = new Success<E, B>(b);
            IMonad<I, 
                ValidationMonad<E, B>> innerMonad = runValidationT.pure(success);

            return new ValidationTMonad<I, E, B>(innerMonad);
        }

        public IMonad<ValidationT<I, E>, B> map<B>(Func<A, B> f) {
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

        public IMonad<ValidationT<I, E>, Unit> map(Action<A> action) {
            return this.map(action.convertToFunc());
        }

        public IMonad<ValidationT<I, E>, B> bind<B>(
            Func<A, IMonad<ValidationT<I, E>, B>> f) {
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
