namespace FQueue.Logging
{
    using System;
    using System.Diagnostics.Tracing;

    internal class ConsoleListener : EventListener
    {
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            Console.WriteLine($"[{DateTime.Now}-{eventData.EventName}] - {eventData.Message}");
        }
    }
}