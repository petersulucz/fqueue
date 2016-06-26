
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
        /// <summary>
        /// The startup timer
        /// </summary>
        private static Timer startupTimer = null;

        /// <summary>
        /// Set once startup is completed
        /// </summary>
        private static readonly EventWaitHandle startupHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        /// <summary>
        /// The queue endpoint
        /// </summary>
        private static QueueEndpoint endpoint;

        /// <summary>
        /// Main.
        /// </summary>
        /// <param name="args">Args to main...</param>
        public static void Main(string[] args)
        {
            var listener = new ConsoleListener();
            listener.EnableEvents(Trace, EventLevel.Informational);
            Trace.Startup();
            startupTimer = new Timer(Program.Run, null, 1000, Timeout.Infinite);

            startupHandle.WaitOne();
            string line;

            do
            {
                // Write the prompt
                Program.WritePrompt();


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

                            Console.WriteLine(item ?? "Queue empty");
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

            // Finish the program
            Program.Cleanup();
        }

        /// <summary>
        /// Write the console prompt
        /// </summary>
        private static void WritePrompt()
        {
            // Wanted to back up original color. But that made a mess
            var color = ConsoleColor.White;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("fqueue -> ");
            Console.ForegroundColor = color;
        }

        /// <summary>
        /// Run the program
        /// </summary>
        /// <param name="arg">Arg. totally ignored...</param>
        public static void Run(object arg)
        {
            Trace.MethodEnter();
            startupTimer.Dispose();

            Trace.Info("Starting intialization.");
            QueueManager.Intance.Initialize();

            Program.endpoint = new QueueEndpoint(new IPEndPoint(IPAddress.Any, 1024));

            Trace.Info("Initialization completed.");

            // Set the handle on completion
            startupHandle.Set();

            Trace.MethodLeave();
        }

        /// <summary>
        /// Clean pu the mess
        /// </summary>
        public static void Cleanup()
        {
            Program.endpoint.Dispose();
            Trace.Shutdown();
        }
    }
}
