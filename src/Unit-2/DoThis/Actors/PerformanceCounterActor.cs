using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Akka.Actor;

namespace ChartApp.Actors
{
    /// <summary>
    /// Actor responsible for monitoring a specific <see cref="PerformanceCounter"/>
    /// </summary>
    public class PerformanceCounterActor : UntypedActor
    {
        private readonly string _seriesName;
        private readonly Func<PerformanceCounter> _performanceCounterGenerator;
        private PerformanceCounter _counter;

        private readonly HashSet<IActorRef> _subscribers;
        private ICancelable _cancelPublishing;

        public PerformanceCounterActor(string seriesName, Func<PerformanceCounter> performanceCounterGenerator)
        {
            _seriesName = seriesName;
            _performanceCounterGenerator = performanceCounterGenerator;
            _subscribers = new HashSet<IActorRef>();
        }

        #region Actor lifecycle methods

        protected override void PreStart()
        {
            _counter = _performanceCounterGenerator();
            _cancelPublishing = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
                TimeSpan.FromMilliseconds(250),
                TimeSpan.FromMilliseconds(250),
                Self,
                new GatherMetrics(),
                Self);
        }

        protected override void PostStop()
        {
            try
            {
                _cancelPublishing.Cancel(false);
                _counter.Dispose();
            }
            catch (Exception)
            {
                //gulp
            }
            finally
            {
                base.PostStop();
            }
        }



        #endregion

        protected override void OnReceive(object message)
        {
            
            
            if (message is GatherMetrics)
            {
                var metric = new Metric(_seriesName, _counter.NextValue());
                foreach (var sub in _subscribers)
                    sub.Tell(metric);
            }
            else if (message is SubscribeCounter)
            {
                var sc = message as SubscribeCounter;
                _subscribers.Add(sc.Subscriber);
            }
            else if (message is UnsubscribeCounter)
            {
                var uc = message as UnsubscribeCounter;
                _subscribers.Remove(uc.Subscriber);
            }
        }
    }
}
