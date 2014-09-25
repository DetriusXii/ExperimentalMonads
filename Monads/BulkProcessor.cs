using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperimentalMonads.Monads {

    /// <summary>
    ///     This is a stream processor that handles executing large data sets
    ///     Turns out it's useful for revision 2 of HodiaInCSharp
    /// </summary>
    /// <typeparam name="T">The type of data in the collection</typeparam>
    public class BulkProcessor<T> {
        private int maxQueueSize;

        private Queue<T> dataQueue = new Queue<T>();
        private Action<Queue<T>> bulkAction = null;
        private IEnumerable<T> stream;

        public BulkProcessor(int maxQueueSize, IEnumerable<T> stream,
            Action<Queue<T>> bulkAction) {
            this.maxQueueSize = maxQueueSize;
            this.bulkAction = bulkAction;
            this.stream = stream;
        }

        public void startReading() { 
            foreach(T item in stream) {
                lock (dataQueue) {
                    dataQueue.Enqueue(item);

                    if (dataQueue.Count.Equals(maxQueueSize)) {
                        bulkAction(dataQueue);
                        dataQueue.Clear();
                    }
                }
            }

            if (dataQueue.Count > 0) {
                publishRemaining();
            }
        }

        private void publishRemaining() {
            lock (dataQueue) {
                bulkAction(dataQueue);
                dataQueue.Clear();
            }
        }
    }

    public class BulkFunctionProcessor<T, A> {
        private int maxQueueSize;

        private Queue<T> dataQueue = new Queue<T>();
        private Func<Queue<T>, A> bulkFunction = null;
        private IEnumerable<T> stream;

        public BulkFunctionProcessor(int maxQueueSize, IEnumerable<T> stream,
            Func<Queue<T>, A> bulkFunction) {
            this.maxQueueSize = maxQueueSize;
            this.bulkFunction = bulkFunction;
            this.stream = stream;
        }

        public B startReading<B>(B initialValue, Func<B, A, B> foldingFunction) {
            B currentB = initialValue;

            foreach (T item in stream) {
                dataQueue.Enqueue(item);

                if (dataQueue.Count.Equals(maxQueueSize)) {
                    currentB = foldingFunction(currentB, bulkFunction(dataQueue));
                    dataQueue.Clear();
                }
            }

            if (dataQueue.Count > 0) {
                currentB = publishRemaining(currentB, foldingFunction);
            }

            return currentB;
        }

        private B publishRemaining<B>(B initialValue, Func<B, A, B> foldingFunction) {
            B currentB = initialValue;
            
            currentB = foldingFunction(currentB, bulkFunction(dataQueue));
            dataQueue.Clear();
        
            return currentB;
        }
    }
}
