
namespace FQueue
{
    using System;
    using System.Threading;
    using Logging;
    using static Logging.Logger;
    using System.Diagnostics.Tracing;

    public class Program
    {
        private static Timer startupTimer = null;

        public static void Main(string[] args)
        {
            var listener = new ConsoleListener();
            listener.EnableEvents(Trace, EventLevel.Verbose);

            Trace.MethodEnter();
            startupTimer = new Timer(Program.Run, null, 1000, Timeout.Infinite);

            Trace.Info("Starting FQUEUE");

            Console.ReadLine();

            Trace.MethodLeave();
        }

        public static async void Run(object arg)
        {
            Trace.MethodEnter();
            startupTimer.Dispose();

            Trace.MethodLeave();
        }
    }
}
