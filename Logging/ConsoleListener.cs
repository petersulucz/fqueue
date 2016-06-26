namespace FQueue.Logging
{
    using System;
    using System.Diagnostics.Tracing;
    using System.Linq;

    internal class ConsoleListener : EventListener
    {
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            var oldColor = Console.ForegroundColor;

            switch (eventData.Level)
            {
                case EventLevel.Critical:
                case EventLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case EventLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case EventLevel.LogAlways: 
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                default:
                    break;
            }
            var message = String.Empty;
            if (false == String.IsNullOrWhiteSpace(eventData.Message))
            {
                message = String.Format(eventData.Message, eventData.Payload.ToArray());
            }
            Console.WriteLine($"[{DateTime.Now}-{eventData.EventName}] - {message}");

            Console.ForegroundColor = oldColor;
        }
    }
}