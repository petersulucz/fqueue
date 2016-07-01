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

        private const int BufferLen = 100000;

        /// <summary>
        /// The buffer.
        /// </summary>
        private readonly QueueElement[] buffer = new QueueElement[BufferLen];

        /// <summary>
        /// The semaphore.
        /// </summary>
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(0);

        /// <summary>
        /// The name of the queue
        /// </summary>
        private readonly string name;

        /// <summary>
        /// Whether the queue is currently enabled
        /// </summary>
        private bool enabled;

        /// <summary>
        /// The queue
        /// </summary>
        /// <param name="name"></param>
        public Queue(string name)
        {
            this.name = name;
            this.enabled = true;
        }

        /// <summary>
        /// Gets the number of items in this queue.
        /// </summary>
        public int Count => this.count;

        /// <summary>
        /// Gets the name of this queue.
        /// </summary>
        public string Name => this.name;

        /// <summary>
        /// Gets or sets the enabled status
        /// </summary>
        public bool IsEnabled {
            get
            {
                return this.enabled;
            }
            set
            {
                this.enabled = value;
            }
        }

        /// <summary>
        /// Gets the current count of items that have been in the queue.
        /// </summary>
        public long Index => this.currentIndex;

        /// <summary>
        /// The enqueue.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        public Status Enqueue(string item)
        {
            if (this.count == BufferLen)
            {
                Trace.Error("Queue is full. Dropping item.");
                return Status.Full;
            }

            if (false == this.enabled)
            {
                Trace.Error("Queue in purge. Dropping item.");
                return Status.Purge;
   ;         }

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

            return Status.Success;
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
            var item = this.buffer[index % BufferLen];
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
