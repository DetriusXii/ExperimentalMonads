using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperimentalMonads.Monads {
    public class Validation<E> : Monad<Validation<E>> {
        public IMonad<Validation<E>, A> pure<A>(A a) {
            return new Success<E, A>(a);
        }

        public static IMonad<Validation<E>, A> pureS<A>(A a) {
            return new Success<E, A>(a);
        }

        public static IMonad<Validation<E>, A> failure<A>(E e) {
            return new Failure<E, A>(e);
        }
    }

    public static class Validation {
        public static bool isSuccess<E, A>(this IMonad<Validation<E>, A> validation) {
            var success = validation as Success<E, A>;
            return success != null;
        }

        public static OptionMonad<A> convertToOption<E, A>(
            this IMonad<Validation<E>, A> validation, Func<E, Unit> handleError) {
            if (validation.isSuccess()) {
                var success = (Success<E, A>)validation;
                return new Some<A>(success.value);
            } else {
                var failure = (Failure<E, A>)validation;
                handleError(failure.failureValue);
                return new None<A>();
            }
        }

        public static ValidationMonad<E, B> pure<E, A, B>(
            this IMonad<Validation<E>, A> validation, B b) {
            return (ValidationMonad<E, B>)Validation<E>.pureS(b);
        }

        public static ValidationMonad<E, B> map<E, A, B>(
            this IMonad<Validation<E>, A> validation, Func<A, B> f) {
            return (ValidationMonad<E, B>)validation.map(f);
        }

        public static ValidationMonad<E, B> bind<E, A, B>(
            this IMonad<Validation<E>, A> validation,
            Func<A, IMonad<Validation<E>, B>> f) {
            return (ValidationMonad<E, B>)validation.bind(f);
        }

        public static OptionMonad<A> getSuccessValue<E, A>(this IMonad<Validation<E>, A> validation) {
            var success = validation as Success<E, A>;
            if (success != null) {
                return new Some<A>(success.value);
            } else { 
                return new None<A>();
            }
        }

        public static OptionMonad<E> getFailureValue<E, A>(
            this IMonad<Validation<E>, A> validation) {
            var failure = validation as Failure<E, A>;
            if (failure != null) {
                return new Some<E>(failure.failureValue);
            } else {
                return new None<E>();
            }
        }

        public static ValidationMonad<E, A> convertToValidationMonad<E, A>(
            this IMonad<Option, A> option, E failureValue) {
            var some = option as Some<A>;
            if (some != null) {
                return new Success<E, A>(some.value);
            } else {
                return new Failure<E, A>(failureValue);
            }
        }

        public static ValidationMonad<E2, A> mapFailure<E1, E2, A>(this IMonad<Validation<E1>, A> validation, 
            Func<E1, E2> f) {
                var failure = validation as Failure<E1, A>;
                var success = validation as Success<E1, A>;
                if (failure != null) {
                    return new Failure<E2, A>(f(failure.failureValue));
                } else if (success != null) {
                    return new Success<E2, A>(success.value);
                } else {
                    throw new UnknownValidationMonadTypeException();
                }
        }

        public static A getValueOrDefault<E, A>(this IMonad<Validation<E>, A> v,
            Func<E, A> f) {
            var success = v as Success<E, A>;
            var failure = v as Failure<E, A>;
            if (success != null) {
                return success.value;
            } else if (failure != null) {
                return f(failure.failureValue);
            } else {
                throw new UnknownValidationMonadTypeException();
            }
        }

        public static IMonad<M, IMonad<Validation<E>, A>> sequence<M, E, A>(this IMonad<Validation<E>, IMonad<M, A>> validation)
            where M : Monad<M>, new() {
            var m = new M();

            return validation.map(innerMonad => innerMonad.map(value => Validation<E>.pureS(value))).
                getValueOrDefault(errorMessage => m.pure(Validation<E>.failure<A>(errorMessage)));
        }
    }

    public class UnknownValidationMonadTypeException : Exception { 
    }

    public class ValidationS {
        public static ValidationMonad<E, A> tryCatch<E, A>(Func<A> tryBlock)
            where E : Exception {
            
            try {
                var result = tryBlock();
                return new Success<E, A>(result);
            } catch (Exception ex) {
                var matchedException = ex as E;
                if (matchedException != null) {
                    return new Failure<E, A>(matchedException);
                } else {
                    throw;
                }
            }
        }


    }

    public class ValidationMonad<E, A> : IMonad<Validation<E>, A> {
        public IMonad<Validation<E>, B> pure<B>(B b) {
            return new Success<E, B>(b);
        }

        public IMonad<Validation<E>, B> map<B>(Func<A, B> f) {
            Success<E, A> success = this as Success<E, A>;

            if (success != null) {
                return new Success<E, B>(f(success.value));
            } else {
                Failure<E, A> oldFailure = (Failure<E, A>)this;
                return new Failure<E, B>(oldFailure.failureValue);
            }
        }

        public IMonad<Validation<E>, Unit> map(Action<A> action) {
            return this.map(action.convertToFunc());
        }

        public IMonad<Validation<E>, B> bind<B>(
            Func<A, IMonad<Validation<E>, B>> f) {
            Success<E, A> success = this as Success<E, A>;

            if (success != null) {
                return f(success.value);
            } else {
                Failure<E, A> oldFailure = (Failure<E, A>)this;
                return new Failure<E, B>(oldFailure.failureValue);
            }
        }
    }

    public class Success<E, A> : ValidationMonad<E, A> {
        public readonly A value;

        public Success(A a) {
            this.value = a;
        }
    }

    public class Failure<E, A> : ValidationMonad<E, A> { 
        public readonly E failureValue;

        public Failure(E e) {
            this.failureValue = e;
        }
    }
}
