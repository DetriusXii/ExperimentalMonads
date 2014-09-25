using ExperimentalMonads.Monads;
using HodiaInCSharp.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HodiaInCSharp.Monads {
    public class Promise: Monad<Promise> {

        public IMonad<Promise, A> pure<A>(A a) {
            return new PromiseMonad<A>(a);
        }

        public static IMonad<Promise, A> pureS<A>(A a) {
            return new PromiseMonad<A>(a);
        }
    }

    public class PromiseMonad<A> : IMonad<Promise, A> {
        private readonly Task<A> task;
        
        public PromiseMonad(A a) {
            Task.Factory.StartNew(() => a);
        }

        public PromiseMonad(Func<A> f) {
            Func<Unit, A> f2 = (Unit unit) => f();
           
            task = Task.Factory.StartNew(f);
        }

        public PromiseMonad(Task<A> task) {
            this.task = task;
        }

        public IMonad<Promise, B> pure<B>(B b) {
            return new PromiseMonad<B>(b);
        }

        public IMonad<Promise, B> map<B>(Func<A, B> f) {
            return new PromiseMonad<B>(task.ContinueWith(completedTask => f(completedTask.Result)));
        }

        public IMonad<Promise, Unit> map(Action<A> action) {
            return this.map(action.convertToFunc());
        }

        public IMonad<Promise, B> bind<B>(Func<A, IMonad<Promise, B>> f) {
            return new PromiseMonad<B>(task.ContinueWith(completedTask =>
                ((PromiseMonad<B>)f(completedTask.Result)).getResult()));
        }

        public A getResult() {
            return task.Result;
        }

        public bool isCompleted() {
            return task.IsCompleted;
        }
    }
}
