using System;
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    /// Actor responsible for serializing message writes to the console.
    /// (write one message at a time, champ :)
    /// </summary>
    class ConsoleWriterActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            var msg = message as string;

            // make sure we got a message
            if (string.IsNullOrEmpty(msg))
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} Please provide an input.\n{Sender.Path}");
                Console.ResetColor();
                return;
            }

            // if message has even # characters, display in red; else, green
            var even = msg.Length % 2 == 0;
            var color = even ? ConsoleColor.Red : ConsoleColor.Green;
            var alert = even ? $"{System.Threading.Thread.CurrentThread.ManagedThreadId} Your string had an even # of characters.\n" : $"{System.Threading.Thread.CurrentThread.ManagedThreadId} Your string had an odd # of characters.\n";
            Console.ForegroundColor = color;
            Console.WriteLine(alert);
            Console.ResetColor();

        }
    }
}
