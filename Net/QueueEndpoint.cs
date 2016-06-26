namespace fqueue.Net
{
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    using fqueue.Queues;
    using System;
    using System.Threading.Tasks;

    using Newtonsoft.Json.Linq;

    using static FQueue.Logging.Logger;

    /// <summary>
    /// The queue endpoint
    /// </summary>
    public class QueueEndpoint : IDisposable
    {
        /// <summary>
        /// The tcp listener
        /// </summary>
        private readonly TcpListener listener;

        /// <summary>
        /// The cancellation source
        /// </summary>
        private readonly CancellationTokenSource tokenSource;

        /// <summary>
        /// The number of current active connections
        /// </summary>
        private int activeConnections = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueEndpoint"/> class.
        /// </summary>
        /// <param name="endpoint">The ip endpoint to bind to</param>
        public QueueEndpoint(IPEndPoint endpoint)
        {
            Trace.MethodEnter();
            this.listener = new TcpListener(endpoint);
            this.tokenSource = new CancellationTokenSource();

            var thread = new Thread(this.DistributeTasks);
            thread.Start(this.tokenSource.Token);

            Trace.MethodLeave();
        }

        /// <summary>
        /// The distribute tasks.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        public async void DistributeTasks(object obj)
        {
            Trace.Info($"Opening listener on {((IPEndPoint)this.listener.LocalEndpoint).Address}:{((IPEndPoint)this.listener.LocalEndpoint).Port}");
            var token = (CancellationToken)obj;
            this.listener.Start();

            while (false == token.IsCancellationRequested)
            {
                var client = await this.listener.AcceptTcpClientAsync();

                // Add to active
                Interlocked.Increment(ref this.activeConnections);

                // Fire off into nothingness
                var tsk = Task.Run(() => this.HandleConnection(client), token);
            }
        }

        /// <summary>
        /// Handle an incoming connection
        /// </summary>
        /// <param name="client">The client socket</param>
        /// <returns>A task</returns>
        private async Task HandleConnection(TcpClient client)
        {
            try
            {
                using (client)
                {
                    Trace.Info($"Connection received. [Active={this.activeConnections}]");
                    using (var stream = client.GetStream())
                    {
                        using (var textreader = new StreamReader(stream, Encoding.Unicode))
                        {
                            var text = String.Empty;

                            // Read everything upto the \r\r
                            var line = String.Empty;
                            while (false == String.IsNullOrWhiteSpace(line = await textreader.ReadLineAsync()))
                            {
                                text += line;
                            }

                            var jobject = JObject.Parse(text);

                            var dtype = jobject["type"].ToObject<string>();
                            switch (dtype.Trim().ToLowerInvariant())
                            {
                                case "push":
                                    await HandlePush(jobject, stream);
                                    break;
                                case "pop":
                                    await HandlePop(jobject, stream);
                                    return;
                                default:
                                    return;
                            }
                        }
                    }
                }
            }
            finally
            {
                // Dont want leaks so do in finally
                Interlocked.Decrement(ref this.activeConnections);
            }
        }

        /// <summary>
        /// Handle a pop from the queue
        /// </summary>
        /// <param name="reader">The json object</param>
        /// <param name="client">The stream client</param>
        /// <returns>A task</returns>
        private static async Task HandlePop(JObject reader, Stream client)
        {
            var queueName = reader["queue"].ToString();

            var payload = QueueManager.Intance[queueName].Dequeue()?.Payload;

            if (String.IsNullOrWhiteSpace(payload))
            {
                payload = "ERROR";
            }

            byte[] response = null;
            response = Encoding.Unicode.GetBytes(payload);

            await client.WriteAsync(response, 0, response.Length);
        }

        /// <summary>
        /// Handle a push onto the queue
        /// </summary>
        /// <param name="reader">The json object</param>
        /// <param name="client">The client stream.</param>
        /// <returns>A task</returns>
        private static async Task HandlePush(JObject reader, Stream client)
        {
            var queueName = reader["queue"].ToString();

            var payload = reader["data"].ToString();
           
            // Get the response bytes. These should be constants.
            var response = Encoding.Unicode.GetBytes(true == QueueManager.Intance[queueName].Enqueue(payload) ? "OK" : "Failed");

            await client.WriteAsync(response, 0, response.Length);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            this.tokenSource.Cancel();
            this.tokenSource.Dispose();
        }
    }
}
