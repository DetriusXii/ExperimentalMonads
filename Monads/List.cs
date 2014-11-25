using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperimentalMonads.Monads {
    public class List: Monad<List> {
        public IMonad<List, A> pure<A>(A a) {
            return new ListMonad<A>(a, new Nil<A>());
        }

        public static IMonad<List, A> empty<A>() {
            return new Nil<A>();
        }

        public static IMonad<List, A> pureS<A>(A a) {
            return new ListMonad<A>(a, new Nil<A>());
        }

        public static IMonad<List, A> convertFromIList<A>(IList<A> iList) {
            var emptyList = List.empty<A>();

            foreach (A a in iList) { emptyList = emptyList.Add(a); }

            return emptyList.Reverse();
        }
    }

    public class Nil<A> : ListMonad<A> {
        public Nil() : base(new None<A>(), new None<ListMonad<A>>(), 0) { }
    }

    public class ListMonad<A> : IMonad<List, A> {
        public readonly OptionMonad<A> head;
        public readonly OptionMonad<ListMonad<A>> tail;
        public readonly int size;

        protected ListMonad(OptionMonad<A> head,
            OptionMonad<ListMonad<A>> tail, int size) {
            this.head = head;
            this.tail = tail;
            this.size = size;
        }

        public ListMonad(A head, ListMonad<A> tail) :  
            this(new Some<A>(head), tail.convertFromClass(), tail.size + 1) {}

        public ListMonad<A> add(A a) {
            return new ListMonad<A>(a, this);
        }

        public ListMonad<A> getTail() {
            return this.tail.getValueOrDefault(() =>
                new Nil<A>());
        }

        public B foldLeft<B>(B startCondition, Func<B, A, B> f) {
            var pos = this;
            var newB = startCondition;

            while (pos.GetType() != typeof(Nil<A>)) { 
                var recastHead = pos.head as Some<A>;
                
                if (recastHead != null) {
                    newB = f(newB, recastHead.value);
                } else {
                    break;
                }

                pos = pos.getTail();
            }

            return newB;
        }

        public ListMonad<A> reverse() { 
            ListMonad<A> newList = new Nil<A>();

            var pos = this;
            while (pos.GetType() != typeof(Nil<A>)) {
                var recastHead = pos.head as Some<A>;
                if (recastHead != null) {
                    newList = newList.add(recastHead.value);
                } else {
                    break;
                }

                pos = pos.getTail();
            }

            return newList;
        }

        public ListMonad<A> append(ListMonad<A> otherList) {
            ListMonad<A> newList = otherList;
            var pos = this.reverse();

            while (pos.GetType() != typeof(Nil<A>)) {
                var recastHead = pos.head as Some<A>;
                if (recastHead != null) {
                    newList = otherList.add(recastHead.value);
                } else {
                    break;
                }

                pos = pos.getTail();
            }

            return newList;
        }

        public IMonad<List, B> pure<B>(B b) {
            return new ListMonad<B>(b, new Nil<B>());
        }

        public IMonad<List, B> map<B>(Func<A, B> f) {
            var pos = this.reverse();
            ListMonad<B> newList = new Nil<B>();

            while (pos.GetType() != typeof(Nil<A>)) {
                var recastHead = pos.head as Some<A>;
                if (recastHead != null) {
                    newList = newList.add(f(recastHead.value));
                } else {
                    break;
                }
                pos = pos.getTail();
            }

            return newList;
        }

        public IMonad<List, Unit> map(Action<A> action) {
            return this.map(action.convertToFunc());
        }

        public bool exists(Func<A, bool> predicate) {
            var pos = this;
            var found = false;

            while (pos.GetType() != typeof(Nil<A>)) {
                var recastHead = pos.head as Some<A>;
                if (recastHead != null) {
                    if (predicate(recastHead.value)) {
                        found = true;
                        break;
                    }
                } else {
                    break;
                }

                pos = pos.getTail();
            }

            return found;
        }

        public IMonad<List, B> bind<B>(Func<A, IMonad<List, B>> f) {
            var pos = this.reverse();
            ListMonad<B> newList = new Nil<B>();

            while (pos.GetType() != typeof(Nil<A>)) {
                var recastHead = pos.head as Some<A>;
                if (recastHead != null) {
                    var subList = (ListMonad<B>)f(recastHead.value);
                    newList.append(subList);
                } else {
                    break;
                }

                pos = pos.getTail();
            }

            return newList;
        }
    }

    public static class ListMonadExtensions {
        public static IMonad<List, A> Add<A>(this IMonad<List, A> list, A a) {
            return ((ListMonad<A>)list).add(a);
        }

        public static IMonad<List, A> Reverse<A>(this IMonad<List, A> list) {
            return ((ListMonad<A>)list).reverse();
        }

        public static IMonad<List, A> Append<A>(this IMonad<List, A> list, IMonad<List, A> other) {
            return ((ListMonad<A>)list).append((ListMonad<A>)other);
        }

        public static IMonad<M, IMonad<List, A>> Sequence<M, A>(this IMonad<List, IMonad<M, A>> l)
            where M : Monad<M>, new() {
            var m = new M();

            return l.FoldLeft(m.pure(List.empty<A>()), (builtMonadList, m2) =>
                m2.bind(a =>
                    builtMonadList.map(builtList =>
                        builtList.Add(a)))).map(formedListMonad =>
                            formedListMonad.Reverse());

        }

        public static IMonad<M, IList<A>> Sequence<M, A>(this IList<IMonad<M, A>> l) where M : Monad<M>, new() {
            var m = new M();

            return l.Aggregate(m.pure<IList<A>>(new List<A>()), (builtMonadList, m2) =>
                m2.bind(a => builtMonadList.map(builtList => {
                    builtList.Add(a);
                    return builtList;
                })));
        }
    }

    public static class ListExtensions {
        public static string toStringValues<A>(this IList<A> list) {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            for (int i = 0; i < list.Count; i++) {
                sb.Append(list[i].ToString());
                
                if (i < list.Count - 1) {
                    sb.Append(",");
                }
            }

            sb.Append("]");

            return sb.ToString();
        }

        public static SortedList<K, V> getSortedList<O, K, V>(this IList<O> unsortedList,
            IComparer<K> comparer, Func<O, K> fk, Func<O, V> fv) {
                var newSortedList = new SortedList<K, V>(comparer);
                foreach (var o in unsortedList) {
                    if (!newSortedList.ContainsKey(fk(o))) {
                        newSortedList.Add(fk(o), fv(o));
                    }
                }
                return newSortedList;
        }

        public static SortedList<K, IList<V>> getSortedListWithDuplicates<O, K, V>(
            this IList<O> unsortedList,
            IComparer<K> comparer, Func<O, K> fk, Func<O, V> fv) {
            var newSortedList = new SortedList<K, IList<V>>(comparer);
            foreach (var o in unsortedList) { 
                if(!newSortedList.ContainsKey(fk(o))) {
                    var newList = new List<V>();
                    newList.Add(fv(o));
                    newSortedList.Add(fk(o), newList);
                } else {
                    var list = newSortedList[fk(o)];
                    list.Add(fv(o));
                }
            }
            return newSortedList;
        }

        public static SortedList<K1, SortedList<K2, V>>
            getDoubleSortedList<O, K1, K2, V>(this IList<O> unsortedList,
            IComparer<K1> c1, IComparer<K2> c2, Func<O, K1> fk1, Func<O, K2> fk2,
            Func<O, V> fv) {
            var newSortedList = new SortedList<K1, SortedList<K2, V>>(c1);
            foreach (var o in unsortedList) { 
                if (!newSortedList.ContainsKey(fk1(o))) {
                    var secondSortedList = new SortedList<K2, V>(c2);
                    secondSortedList.Add(fk2(o), fv(o));
                    newSortedList.Add(fk1(o), secondSortedList);
                } else {
                    var secondSortedList = newSortedList[fk1(o)];
                    if (!secondSortedList.ContainsKey(fk2(o))) {
                        secondSortedList.Add(fk2(o), fv(o));
                    }
                }
            }

            return newSortedList;
        }

        public static int Count<A>(this IMonad<List, A> list) {
            return ((ListMonad<A>)list).size;
        }

        public static B FoldLeft<A, B>(this IMonad<List, A> list,
            B startCondition, Func<B, A, B> f) {
            var l1 = (ListMonad<A>)list;
            return l1.foldLeft(startCondition, f);
        }

        public static bool Exists<A>(this IMonad<List, A> list, Func<A, bool> predicate) {
            var l = (ListMonad<A>)list;
            return l.exists(predicate);
        }
    }

    public class ImmutableSortedList<K, V> : IDictionary<K, V> {


        private readonly SortedList<K, V> sortedList;
        public ImmutableSortedList(SortedList<K, V> sortedList) {
            this.sortedList = sortedList;
        }

        public void Add(K key, V value) {
           
        }

        public bool ContainsKey(K key) {
            return sortedList.ContainsKey(key);
        }

        public ICollection<K> Keys {
            get { return this.sortedList.Keys; }
        }

        public bool Remove(K key) {
            return false;
        }

        public bool TryGetValue(K key, out V value) {
            return sortedList.TryGetValue(key, out value);
        }

        public ICollection<V> Values {
            get { throw new NotImplementedException(); }
        }

        public V this[K key] {
            get {
                return sortedList[key];
            }
            set {
                
            }
        }

        public void Add(KeyValuePair<K, V> item) {
        }

        public void Clear() {
        }

        public bool Contains(KeyValuePair<K, V> item) {
            return sortedList.Contains(item);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) {
        }

        public int Count {
            get { return sortedList.Count; }
        }

        public bool IsReadOnly {
            get { return true; }
        }

        public bool Remove(KeyValuePair<K, V> item) {
            return false;
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() {
            return sortedList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return sortedList.GetEnumerator();
        }
    }
}
