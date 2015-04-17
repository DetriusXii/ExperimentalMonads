using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperimentalMonads.Monads {
    public class NETList : Monad<NETList> {
        public IMonad<NETList, A> pure<A>(A a) {
            var newList = new List<A>();
            newList.Add(a);

            return new NETListMonad<A>(newList);
        }

        public static IMonad<NETList, A> pureS<A>(A a) {
            return new NETList().pure(a);
        }
    }

    public class NETListMonad<A> : IMonad<NETList, A>, IList<A> {
        private readonly IList<A> iList;
        
        public NETListMonad(IList<A> iList) {
            this.iList = iList;
        }


        public int IndexOf(A item) {
            return iList.IndexOf(item);
        }

        public void Insert(int index, A item) {
            iList.Insert(index, item);
        }

        public void RemoveAt(int index) {
            iList.RemoveAt(index);
        }

        public A this[int index] {
            get {
                return iList[index];
            }
            set {
                iList[index] = value;
            }
        }

        public void Add(A item) {
            iList.Add(item);
        }

        public void Clear() {
            iList.Clear();
        }

        public bool Contains(A item) {
            return iList.Contains(item);
        }

        public void CopyTo(A[] array, int arrayIndex) {
            iList.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return iList.Count; }
        }

        public bool IsReadOnly {
            get { return iList.IsReadOnly; }
        }

        public bool Remove(A item) {
            return iList.Remove(item);
        }

        public IEnumerator<A> GetEnumerator() {
            return iList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return iList.GetEnumerator();
        }

        public IMonad<NETList, B> pure<B>(B b) {
            var newList = new List<B>();
            newList.Add(b);

            return new NETListMonad<B>(newList);
        }

        public IMonad<NETList, B> map<B>(Func<A, B> f) {
            return new NETListMonad<B>(this.iList.Select(f).ToList());
        }

        public IMonad<NETList, Unit> map(Action<A> a) {
            return this.map(a.convertToFunc());
        }

        public IMonad<NETList, B> bind<B>(Func<A, IMonad<NETList, B>> f) {
            throw new NotSupportedException();
        }
    }
}

