using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fqueue.Net
{
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    using fqueue.Queues;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using static FQueue.Logging.Logger;

    public class QueueEndpoint : IDisposable
    {
        private readonly TcpListener listener;

        private readonly CancellationTokenSource tokenSource;

        private int activeConnections = 0;

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
            Trace.Info($"Opening lister on {((IPEndPoint)this.listener.LocalEndpoint).Address}:{((IPEndPoint)this.listener.LocalEndpoint).Port}");
            var token = (CancellationToken)obj;
            this.listener.Start();

            while (false == token.IsCancellationRequested)
            {
                var client = await this.listener.AcceptTcpClientAsync();

                Interlocked.Increment(ref this.activeConnections);
                // Fire off into nothingness
                var tsk = Task.Run(() => this.HandleConnection(client), token);
            }
        }

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
                                    await this.HandlePush(jobject, stream);
                                    break;
                                case "pop":
                                    await this.HandlePop(jobject, stream);
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
                Interlocked.Decrement(ref this.activeConnections);
            }
        }

        private async Task HandlePop(JObject reader, Stream client)
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

        private async Task HandlePush(JObject reader, Stream client)
        {
            var queueName = reader["queue"].ToString();

            var payload = reader["data"].ToString();

            byte[] response = null;
            if (true == QueueManager.Intance[queueName].Enqueue(payload))
            {
                response = Encoding.Unicode.GetBytes("OK");
            }
            else
            {
                response = Encoding.Unicode.GetBytes("Failed");
            }

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
