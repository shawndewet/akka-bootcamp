using Akka.Actor;
using System;

namespace WinTail
{
    public class TailCoordinatorActor : UntypedActor
    {
        #region Message Types

        public class StartTail
        {
            public StartTail(string filePath, IActorRef reporterActor)
            {
                FilePath = filePath;
                ReporterActor = reporterActor;
            }

            public string FilePath { get; }
            public IActorRef ReporterActor { get; }
        }

        public class StopTail
        {
            public StopTail(string filePath)
            {
                FilePath = filePath;

            }

            public string FilePath { get; }
        }

        #endregion

        protected override void OnReceive(object message)
        {
            if (message is StartTail)
            {
                var msg = message as StartTail;

                //here we are creating our first parent/child relationship.
                // the TailActor instance created here is a chle of this instance of TailCoordinatorActor

                Context.ActorOf(Props.Create(() => new TailActor(msg.FilePath, msg.ReporterActor)));

            }
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                10, //maxRetries
                TimeSpan.FromSeconds(30), //withinTimerange
                x => //localOnlyDecider
                {
                    if (x is ArithmeticException) return Directive.Resume;

                    else if (x is NotSupportedException) return Directive.Stop;

                    else return Directive.Restart;
                });
        }
    }
}
