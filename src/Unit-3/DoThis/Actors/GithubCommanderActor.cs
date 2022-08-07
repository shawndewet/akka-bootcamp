using System;
using System.Linq;
using Akka.Actor;
using Akka.Routing;

namespace GithubActors.Actors
{
    /// <summary>
    /// Top-level actor responsible for coordinating and launching repo-processing jobs
    /// </summary>
    public class GithubCommanderActor : ReceiveActor, IWithUnboundedStash
    {
        #region Message classes

        public class CanAcceptJob
        {
            public CanAcceptJob(RepoKey repo)
            {
                Repo = repo;
            }

            public RepoKey Repo { get; private set; }
        }

        public class AbleToAcceptJob
        {
            public AbleToAcceptJob(RepoKey repo)
            {
                Repo = repo;
            }

            public RepoKey Repo { get; private set; }
        }

        public class UnableToAcceptJob
        {
            public UnableToAcceptJob(RepoKey repo)
            {
                Repo = repo;
            }

            public RepoKey Repo { get; private set; }
        }

        #endregion

        private IActorRef _coordinator;
        private IActorRef _canAcceptJobSender;
        private int pendingJobReplies;
        public IStash Stash { get; set; }

        //public GithubCommanderActor()
        //{
        //    Receive<CanAcceptJob>(job =>
        //    {
        //        _canAcceptJobSender = Sender;
        //        _coordinator.Tell(job);
        //    });

        //    Receive<UnableToAcceptJob>(job =>
        //    {
        //        _canAcceptJobSender.Tell(job);
        //    });

        //    Receive<AbleToAcceptJob>(job =>
        //    {
        //        _canAcceptJobSender.Tell(job);

        //        //start processing messages
        //        _coordinator.Tell(new GithubCoordinatorActor.BeginJob(job.Repo));

        //        //launch the new window to view results of the processing
        //        Context.ActorSelection(ActorPaths.MainFormActor.Path).Tell(new MainFormActor.LaunchRepoResultsWindow(job.Repo, Sender));
        //    });
        //}

        public GithubCommanderActor()
        {
            Ready();
        }

        private void Ready()
        {
            Receive<CanAcceptJob>(job =>
            {
                _coordinator.Tell(job);
                //BecomeAsking();
                _canAcceptJobSender = Sender;
                //pendingJobReplies = 3; //the number of routes
                pendingJobReplies = _coordinator.Ask<Routees>(new GetRoutees()).Result.Members.Count();
                System.Diagnostics.Debug.Assert(pendingJobReplies == 3);
                Become(Asking);
            });
        }

        //private void BecomeAsking()
        //{
        //    _canAcceptJobSender = Sender;
        //    pendingJobReplies = 3; //the number of routes
        //    Become(Asking);
        //}

        private void Asking()
        {
            Receive<UnableToAcceptJob>(job =>
            {
                pendingJobReplies--;
                if (pendingJobReplies == 0)
                {
                    _canAcceptJobSender.Tell(job);
                    Become(Ready);
                    Stash.UnstashAll();
                }
            });

            Receive<AbleToAcceptJob>(job =>
            {
                _canAcceptJobSender.Tell(job);

                //start processing messages
                Sender.Tell(new GithubCoordinatorActor.BeginJob(job.Repo));

                //launch the new window to view results of the processing
                Context.ActorSelection(ActorPaths.MainFormActor.Path).Tell(new MainFormActor.LaunchRepoResultsWindow(job.Repo, Sender));
                
                Become(Ready);
                Stash.UnstashAll();
            });
        }

        protected override void PreStart()
        {
            //_coordinator = Context.ActorOf(Props.Create(() => new GithubCoordinatorActor()), ActorPaths.GithubCoordinatorActor.Name);

            //// create three coordinatorActors
            //var c1 = Context.ActorOf(Props.Create(() => new GithubCoordinatorActor()), ActorPaths.GithubCoordinatorActor.Name + "1");
            //var c2 = Context.ActorOf(Props.Create(() => new GithubCoordinatorActor()), ActorPaths.GithubCoordinatorActor.Name + "2");
            //var c3 = Context.ActorOf(Props.Create(() => new GithubCoordinatorActor()), ActorPaths.GithubCoordinatorActor.Name + "3");

            ////create a broadcast router to ask all of them if they're available for work
            //_coordinator = Context.ActorOf(Props.Empty.WithRouter(
            //    new BroadcastGroup(ActorPaths.GithubCoordinatorActor.Path + "1",
            //    ActorPaths.GithubCoordinatorActor.Path + "2",
            //    ActorPaths.GithubCoordinatorActor.Path + "3"
            //    )));

            _coordinator = Context.ActorOf(Props.Create(() => new GithubCoordinatorActor())
                .WithRouter(FromConfig.Instance), 
                ActorPaths.GithubCoordinatorActor.Name);

            base.PreStart();
        }

        protected override void PreRestart(Exception reason, object message)
        {
            //kill off the old coordinator so we can recreate it from scratch
            _coordinator.Tell(PoisonPill.Instance);
            base.PreRestart(reason, message);
        }
    }
}
