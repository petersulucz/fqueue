namespace fqueue.Queues
{
    using System.Collections.Generic;

    using fqueue.Configuration;

    using FQueue.Logging;

    /// <summary>
    /// The queue manager.
    /// </summary>
    internal class QueueManager
    {

        private static QueueManager manager;

        public static QueueManager Intance => manager ?? (manager = new QueueManager());

        private static Dictionary<string, Queue> Queues = new Dictionary<string, Queue>();

        private QueueManager()
        {
            
        }

        public void Initialize()
        {
            foreach (var queue in ConfigurationManager.Instance.Queues)
            {
                Logger.Trace.Info($"Initializing queue {queue.Name}");
                QueueManager.Queues[queue.Name] = new Queue(queue.Name);
            }
        }

        public Queue this[string key] => QueueManager.Queues[key];

        public IEnumerable<Queue> List => Queues.Values;
    }
}
