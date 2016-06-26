
namespace FQueue
{
    using System;
    using System.Threading;
    using Logging;
    using static Logging.Logger;
    using System.Diagnostics.Tracing;
    using System.Net;

    using fqueue.Net;
    using fqueue.Queues;

    using Newtonsoft.Json.Linq;

    public class Program
    {
        private static Timer startupTimer = null;

        private static EventWaitHandle startupHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        private static QueueEndpoint endpoint;

        public static void Main(string[] args)
        {
            var listener = new ConsoleListener();
            listener.EnableEvents(Trace, EventLevel.Informational);
            Trace.Startup();


            Trace.MethodEnter();
            startupTimer = new Timer(Program.Run, null, 1000, Timeout.Infinite);

            startupHandle.WaitOne();
            var line = String.Empty;
            do
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("fqueue -> ");
                Console.ForegroundColor = color;

                line = Console.ReadLine();

                line = line.Trim().ToLowerInvariant();

                var pargs = line.Split(' ');
                switch (pargs[0])
                {
                    case "push":
                        {
                            if (pargs.Length < 3)
                            {
                                Console.WriteLine("Invalid args.");
                                break;
                            }
                            var queueName = pargs[1];
                            var value = pargs[2];
                            QueueManager.Intance[queueName].Enqueue(value);
                        }
                        break;
                    case "pop":
                        {
                            if (pargs.Length < 2)
                            {
                                Console.WriteLine("Invalid args.");
                                break;
                            }
                            var queueName = pargs[1];
                            var item = QueueManager.Intance[queueName].Dequeue()?.Payload;
                            if (null != item)
                            {
                                Console.WriteLine(item);
                            }
                            else
                            {
                                Console.WriteLine("Queue empty");
                            }
                        }
                        break;
                    case "list":
                        {
                            foreach (var queue in QueueManager.Intance.List)
                            {
                                Console.WriteLine($"Name: {queue.Name} Length: {queue.Count} Index: {queue.Index}");
                            }
                            break;
                        }
                    case "exit":
                        break;
                    case "":
                        break;
                    default:
                        Console.WriteLine("Invalid Argument");
                        break;
                }
            }
            while (false == String.Equals("exit", line, StringComparison.OrdinalIgnoreCase));

            Trace.MethodLeave();
            Program.Cleanup();
        }

        public static async void Run(object arg)
        {
            Trace.MethodEnter();
            startupTimer.Dispose();

            Trace.Info("Starting intialization.");
            QueueManager.Intance.Initialize();

            Program.endpoint = new QueueEndpoint(new IPEndPoint(IPAddress.Any, 1024));

            Trace.Info("Initialization completed.");
            startupHandle.Set();


            Trace.MethodLeave();
        }

        public static void Cleanup()
        {
            Program.endpoint.Dispose();
            Trace.Shutdown();
        }
    }
}
