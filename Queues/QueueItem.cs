using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fqueue.Queues
{
    /// <summary>
    /// The queue item.
    /// </summary>
    public class QueueItem
    {
        /// <summary>
        /// The payload.
        /// </summary>
        public readonly string Payload;

        /// <summary>
        /// The id.
        /// </summary>
        public readonly long Id;

        /// <summary>
        /// The insert.
        /// </summary>
        public readonly DateTime Insert;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueItem"/> class. 
        /// </summary>
        /// <param name="id">
        /// The queue item id
        /// </param>
        /// <param name="payload">
        /// The payload of the item
        /// </param>
        public QueueItem(long id, string payload)
        {
            this.Id = id;
            this.Payload = payload;
            this.Insert = DateTime.UtcNow;
        }
    }
}
