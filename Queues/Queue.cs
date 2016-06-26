namespace fqueue.Queues
{
    using System;
    using System.Threading;

    using static FQueue.Logging.Logger;

    public class Queue
    {
        /// <summary>
        /// Number of elements in the queue
        /// </summary>
        private int count = 0;

        /// <summary>
        /// The current queue index
        /// </summary>
        private long currentIndex = -1L;

        /// <summary>
        /// The current tail index
        /// </summary>
        private long tail = -1L;

        private const int BufferLen = 1000;

        /// <summary>
        /// The buffer.
        /// </summary>
        private readonly QueueElement[] buffer = new QueueElement[BufferLen];

        /// <summary>
        /// The semaphore.
        /// </summary>
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(0);

        private string name;

        /// <summary>
        /// The queue
        /// </summary>
        /// <param name="name"></param>
        public Queue(string name)
        {
            this.name = name;
        }

        public int Count => this.count;

        public string Name => this.name;

        /// <summary>
        /// The enqueue.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        public void Enqueue(string item)
        {
            if (this.count == BufferLen)
            {
                Trace.Error("Queue is full. Dropping item.");
                throw new IndexOutOfRangeException("Queue is full.");
            }

            // Create a new queue item with a unique id
            var queueItem = new QueueElement(Interlocked.Increment(ref this.currentIndex), item);
            this.buffer[queueItem.Id % BufferLen] = queueItem;
            Interlocked.Increment(ref this.count);
            this.semaphore.Release();

            Trace.Info($"Item pushed to {this.name}, current count {this.count}");

            if (BufferLen - this.count < 100)
            {
                Trace.Warning($"{this.name} length {this.count} approaching maximum: {BufferLen}");
            }
        }

        /// <summary>
        /// The dequeue.
        /// </summary>
        /// <returns>
        /// The <see cref="QueueItem"/>.
        /// </returns>
        public QueueItem Dequeue()
        {
            if (false == this.semaphore.Wait(TimeSpan.FromSeconds(10)))
            {
                Trace.Warning($"Timed out waiting for queue: {this.name}.");
                return null;
            }

            var index = Interlocked.Increment(ref this.tail);
            var item = this.buffer[index];
            Interlocked.Decrement(ref this.count);
            Trace.Info($"Item popped from {this.name}, current count {this.count}");
            return item;
        }

        /// <summary>
        /// The queue element.
        /// </summary>
        private class QueueElement : QueueItem
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="QueueElement"/> class.
            /// </summary>
            /// <param name="id">The id.</param>
            /// <param name="payload">The payload.</param>
            /// <param name="next">The next item in the queue</param>
            public QueueElement(long id, string payload)
                : base(id, payload)
            {
            }
        }
    }
}
