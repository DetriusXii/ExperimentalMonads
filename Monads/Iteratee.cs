using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperimentalMonads.Monads { 
    public class InputInstances {
        public class Empty<E> : Input<E> {
            public override Z fold<Z>(Func<Z> empty, Func<E, Z> el, Func<Z> eof) {
                return empty();
            }
        }

        public class Element<E> : Input<E> {
            public readonly E e;
            
            public Element(E e) {
                this.e = e;
            }
            
            public override Z fold<Z>(Func<Z> empty, Func<E, Z> el, Func<Z> eof) {
                return el(e);
            }
        }

        public class Eof<E> : Input<E> {
            public override Z fold<Z>(Func<Z> empty, Func<E,Z> el, Func<Z> eof) {
 	            return eof();
            }
        }

        public static Input<E> empty<E>() {
            return new Empty<E>();
        }

        public static Input<E> elInput<E>(E e) {
            return new Element<E>(e);
        }

        public static Input<E> eofInput<E>() {
            return new Eof<E>();
        }
    }

    public abstract class Input<E> {
        public abstract Z fold<Z>(Func<Z> empty, Func<E, Z> el, Func<Z> eof);
 
        public Z apply<Z>(Func<Z> empty, Func<E, Z> el, Func<Z> eof) {
            return fold(empty, el, eof);
        }

        public bool isEmpty() {
            return apply(() => true, e => false, () => false);
        }

        public bool isEl() {
            return apply(() => false, e => true, () => false);
        }

        public bool isEof() {
            return apply(() => false, e => false, () => true);
        }

        public Input<B> map<B>(Func<E, B> f) {
            return fold(InputInstances.empty<B>, 
                e => InputInstances.elInput(f(e)), InputInstances.eofInput<B>);
        }

        public Input<B> bind<B>(Func<E, Input<B>> f) {
            return fold(InputInstances.empty<B>, e => f(e), InputInstances.eofInput<B>);
        }

        public Input<E> filter(Func<E, bool> f) {
            return fold(InputInstances.empty<E>, e => f(e) ? this : InputInstances.empty<E>(), InputInstances.eofInput<E>);
        }
            
        public bool exists(Func<E, bool> f) {
            return fold(() => false, f, () => false);
        }
    }

    public abstract class Step<E, A> {
        public abstract Z fold<Z>(Func<Func<Input<E>, IMonad<Iteratee<E>, A>>, Z> cont, Func<A, Func<Input<E>, Z>> done);

        public Z apply<Z>(Func<Func<Input<E>, IMonad<Iteratee<E>, A>>, Z> cont, Func<A, Func<Input<E>, Z>> done) {
            return fold(cont, done);
        }

        public IMonad<Iteratee<E>, A> pointI() {
            return new IterateeMonad<E, A>(this);
        }
    }

    public class StepInstances {
        public class Done<E, A> : Step<E, A> {
            public readonly A a;
            public readonly Input<E> r;
            
            public Done(A a, Input<E> r) {
                this.a = a;
                this.r = r;
            }

            public override Z fold<Z>(Func<Func<Input<E>,IMonad<Iteratee<E>,A>>,Z> cont, Func<A,Func<Input<E>,Z>> done) {
 	            return done(a)(r);
            }
        }

        public class Cont<E, A> : Step<E, A> {
            public readonly Func<Input<E>, IMonad<Iteratee<E>, A>> c;

            public Cont(Func<Input<E>, IMonad<Iteratee<E>, A>> c) {
                this.c = c;
            }

            public override Z fold<Z>(Func<Func<Input<E>,IMonad<Iteratee<E>,A>>,Z> cont, Func<A,Func<Input<E>,Z>> done) {
                return cont(c);
            }
        }

        public static Step<E, A> done<E, A>(A a, Input<E> r) {
            return new Done<E, A>(a, r);
        }

        public static Step<E, A> cont<E, A>(Func<Input<E>, IMonad<Iteratee<E>, A>> c) {
            return new Cont<E, A>(c);
        }
    }

    
    public class Iteratee<E> : Monad<Iteratee<E>> {
        
        public IMonad<Iteratee<E>, A> pure<A>(A a) {
 	        throw new NotImplementedException();
        }
    }

    public class IterateeMonad<E, A> : IMonad<Iteratee<E>, A> {
        public readonly Step<E, A> step;

        public IterateeMonad(Step<E, A> step) {
            this.step = step;
        }

        public Z fold<Z>(Func<Func<Input<E>, IMonad<Iteratee<E>, A>>, Z> cont, Func<A, Func<Input<E>, Z>> done) {
            return step.apply(cont, done);
        } 

        public IMonad<Iteratee<E>, B> pure<B>(B b) {
            throw new NotImplementedException();
        }

        public IMonad<Iteratee<E>, B> map<B>(Func<A, B> f) {
            return this.bind(a => StepInstances.done<E, B>(f(a), InputInstances.empty<E>()).pointI());
        }

        public IMonad<Iteratee<E>, Unit> map(Action<A> a) {
            throw new NotImplementedException();
        }

        public IMonad<Iteratee<E>, B> bind<B>(Func<A, IMonad<Iteratee<E>, B>> f) {
            Func<IMonad<Iteratee<E>, A>, IMonad<Iteratee<E>, B>> through = null;
            through = (IMonad<Iteratee<E>, A> x) => { 
                IterateeMonad<E, A> xIteratee = (IterateeMonad<E, A>)x;

                Func<Func<Input<E>,IMonad<Iteratee<E>,A>>,Step<E, B>> cont = (Func<Input<E>, IMonad<Iteratee<E>, A>> k) => 
                    StepInstances.cont<E, B>(input => through(k(input)));
                Func<A,Func<Input<E>, Step<E, B>>> done = (A a) => (Input<E> inputE) => {
                    if (inputE.isEmpty()) {
                        return ((IterateeMonad<E, B>)f(a)).step;
                    } else {
                        Func<Func<Input<E>, IMonad<Iteratee<E>,B>>, Step<E, B>> secondCont = kk => ((IterateeMonad<E, B>)kk(inputE)).step;
                        Func<B, Func<Input<E>, Step<E, B>>> secondDone = (B bb) => (Input<E> dontCare) => StepInstances.done<E, B>(bb, inputE);
                        

                        return ((IterateeMonad<E, B>)f(a)).step.fold<Step<E, B>>(secondCont, secondDone);
                    }
                };

                return new IterateeMonad<E, B>(xIteratee.step.fold(cont, done));
            };

            return through(this);
        }
    }
}