namespace FQueue.Logging
{
    using System;
    using System.Diagnostics.Tracing;
    using System.Linq;

    internal class ConsoleListener : EventListener
    {
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            var message = String.Empty;
            if (false == String.IsNullOrWhiteSpace(eventData.Message))
            {
                message = String.Format(eventData.Message, eventData.Payload.ToArray());
            }

            Console.WriteLine($"[{DateTime.Now}-{eventData.EventName}] - {message}");
        }
    }
}