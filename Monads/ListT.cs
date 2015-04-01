using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperimentalMonads.Monads {
    public class ListT : Transformer<ListT> {
        public IMonad<MTM<ListT, M>, A> pure<M, A>(A a) where M : Monad<M>, new() {
            var m = new M();
            return new ListTMonad<M, A>(m.pure(List.empty<A>().Add(a)));
        }

        public IMonad<MTM<ListT, M>, A> lift<M, A>(IMonad<M, A> ma) where M : Monad<M>, new() {
            return ListT.emptyS<M, A>().Add(ma);
        }

        public IMonad<MTM<ListT, M>, A> empty<M, A>() 
            where M : Monad<M>, new() {
            var m = new M();

            return new ListTMonad<M, A>(m.pure(List.empty<A>()));
        }

        public static IMonad<MTM<ListT, M>, A> pureS<M, A>(A a) where M : Monad<M>, new() {
            var listT = new ListT();

            return listT.pure<M, A>(a);
        }

        public static IMonad<MTM<ListT, M>, A> emptyS<M, A>()
            where M : Monad<M>, new() {
            var m = new M();

            return new ListTMonad<M, A>(m.pure(List.empty<A>()));
        }

        public static ListTMonad<M, A> makeListTMonad<M, A>(
            IMonad<M, IMonad<List, A>> runListT) where M : Monad<M>, new() {
                return new ListTMonad<M, A>(runListT);
        }
    }

    public class ListTMonad<M, A> : IMonad<MTM<ListT, M>, A> where M : Monad<M>, new() {
        public readonly IMonad<M, IMonad<List, A>> runListT;

        public ListTMonad(IMonad<M, IMonad<List, A>> runListT) {
            this.runListT = runListT;
        }

        public IMonad<MTM<ListT, M>, B> map<B>(Func<A, B> f) {
            return new ListTMonad<M, B>(this.runListT.map(iList => iList.map(f)));
        }

        public IMonad<MTM<ListT, M>, Unit> map(Action<A> a) {
            return this.map(a.convertToFunc());
        }

        public ListTMonad<M, A> add(IMonad<M, A> ma) {
            var newRunListT = runListT.bind(list => ma.map(a => list.Add(a)));

            return new ListTMonad<M, A>(newRunListT);
        }

        public ListTMonad<M, A> add(A a) {
            var m = new M();
            var newRunListT = runListT.bind(list1 => m.pure(list1.Add(a)));

            return new ListTMonad<M, A>(newRunListT);
        }

        public ListTMonad<M, A> append(ListTMonad<M, A> that) { 
            var m = new M();
            var newRunListT = 
                runListT.bind(list1 => that.runListT.map(list2 => list1.Append(list2)));

            return new ListTMonad<M, A>(newRunListT);
        }

        public IMonad<M, int> count() {
            return this.runListT.map(l => l.Count());
        }

        public IMonad<M, bool> exists(Func<A, bool> predicate) {
            return runListT.map(list => list.Exists(predicate));
        }

        public IMonad<MTM<ListT, M>, A> filter(Func<A, bool> predicate) {
            return this.runListT.map(innerList => innerList.Filter(predicate)).MakeListT();
        }

        

        public IMonad<MTM<ListT, M>, B> bind<B>(Func<A, IMonad<MTM<ListT, M>, B>> f) {
            var m = new M();
            Func<IMonad<MTM<ListT, M>, B>,
                IMonad<MTM<ListT, M>, B>,
                IMonad<MTM<ListT, M>, B>> appender =
                (b, a) => b.Append(a);


            var newRunListT = runListT.bind(list => {
                if (list.Count() > 0) {
                    var mappedList = list.map(f);
                    return mappedList.FoldLeft(ListT.emptyS<M, B>(), appender).RunListT();
                } else { 
                    return m.pure(List.empty<B>());
                }
            });


            return new ListTMonad<M, B>(newRunListT);
        }

        public IMonad<MTM<ListT, M>, B> pure<B>(B b) {
            var m = new M();

            return new ListTMonad<M, B>(m.pure(List.empty<B>().Add(b)));
        }
    }

    public static class ListTExtensions {
        public static IMonad<MTM<ListT, M>, A> Append<M, A>(
            this IMonad<MTM<ListT, M>, A> l1,
            IMonad<MTM<ListT, M>, A> that) where M : Monad<M>, new() {
            var list1 = (ListTMonad<M, A>)l1;
            var that1 = (ListTMonad<M, A>)that;

            return list1.append(that1);
        }

        public static IMonad<MTM<ListT, M>, A> Add<M, A>(this IMonad<MTM<ListT, M>, A> listTMonad,
            A a) where M : Monad<M>, new() {
               return ((ListTMonad<M, A>)listTMonad).add(a);
        }

        public static IMonad<MTM<ListT, M>, A> Add<M, A>(this IMonad<MTM<ListT, M>, A> listTMonad,
            IMonad<M, A> ma) where M : Monad<M>, new() {
                return ((ListTMonad<M, A>)listTMonad).add(ma);
        }

        public static IMonad<M, int> Count<M, A>(this IMonad<MTM<ListT, M>, A> listTMonad) where M : Monad<M>, new() {
            return ((ListTMonad<M, A>)listTMonad).count();
        }

        public static IMonad<M, IMonad<List, A>> RunListT<M, A>(
            this IMonad<MTM<ListT, M>, A> this1) where M : Monad<M>, new() {
            return ((ListTMonad<M, A>)this1).runListT;
        }

        public static IMonad<MTM<ListT, M>, A> MakeListT<M, A>(
            this IMonad<M, IMonad<List, A>> runListT) where M : Monad<M>, new() {
            return new ListTMonad<M, A>(runListT);
        }

        public static IMonad<M, bool> Exists<M, A>(this IMonad<MTM<ListT, M>, A> listT,
            Func<A, bool> predicate) where M : Monad<M>, new() {
            return ((ListTMonad<M, A>)listT).exists(predicate);
        }

        public static IMonad<MTM<ListT, M>, A>  Filter<M, A>(this IMonad<MTM<ListT, M>, A> listT, 
            Func<A, bool> predicate) where M : Monad<M>, new() {
            return ((ListTMonad<M, A>)listT).filter(predicate);
        } 
    }
}
