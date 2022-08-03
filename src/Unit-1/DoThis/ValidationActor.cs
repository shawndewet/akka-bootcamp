using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinTail
{
    public class ValidationActor : UntypedActor
    {
        private readonly IActorRef _consoleWriterActor;

        public ValidationActor(IActorRef consoleWriterActor)
        {
            _consoleWriterActor = consoleWriterActor;
        }

        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (string.IsNullOrEmpty(msg))
            {
                _consoleWriterActor.Tell(new Messages.NullInputError("No input received."));
            }
            //else if (String.Equals(message, ExitCommand, StringComparison.OrdinalIgnoreCase))
            //{
            //    // shut down the system (acquire handle to system via
            //    // this actors context)
            //    Context.System.Terminate();
            //    //return;
            //}
            else
            {
                var valid = IsValid(msg);
                if (valid)
                {
                    _consoleWriterActor.Tell(new Messages.InputSuccess("Thanks for valid message"));
                }
                else
                {
                    _consoleWriterActor.Tell(new Messages.ValidationError("Invalid: input was odd"));
                }
            }

            // tell sender to continue doing its thing 
            // (whatever that may be, this actor doesn't care)
            Sender.Tell(new Messages.ContinueProcessing());
        }


        private bool IsValid(string message)
        {
            return message.Length % 2 == 0;
        }
    }
}
