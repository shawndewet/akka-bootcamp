using System;
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    /// Actor responsible for reading FROM the console. 
    /// Also responsible for calling <see cref="ActorSystem.Terminate"/>.
    /// </summary>
    class ConsoleReaderActor : UntypedActor
    {
        public const string ExitCommand = "exit";
        public const string StartCommand = "start";

        private IActorRef _validationActor;

        public ConsoleReaderActor(IActorRef validationActor)
        {
            _validationActor = validationActor;
        }

        protected override void OnReceive(object message)
        {
            if (message.Equals(StartCommand))
            {
                PrintInstructions();
            }
            //else if (message is Messages.InputError)
            //{
            //    _consoleWriterActor.Tell(message as Messages.InputError);
            //}

            GetAndValidateInput();

        }

        #region Internal methods

        private void PrintInstructions()
        {
            Console.WriteLine("Write whatever you want into the console!");
            Console.Write("Some lines will appear as");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(" red ");
            Console.ResetColor();
            Console.Write(" and others will appear as");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" green! ");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Type 'exit' to quit this application at any time.\n");
        }

        /// <summary>
        /// Reads input from console, validates it, then signals appropriate response
        /// </summary>
        private void GetAndValidateInput()
        {
            Console.WriteLine($"Thread {System.Threading.Thread.CurrentThread.ManagedThreadId} {Self.Path} {GetHashCode()} says Type Something:");
            var message = Console.ReadLine();
            
            if (!string.IsNullOrEmpty(message) && String.Equals(message, ExitCommand, StringComparison.OrdinalIgnoreCase))
            {
                //if user typed ExitCommand, shut down the entire actor system
                Context.System.Terminate();
                return;
            }

            //otherwise, just hand teh message off to validation actor
            _validationActor.Tell(message);
        }




        #endregion
    }
}