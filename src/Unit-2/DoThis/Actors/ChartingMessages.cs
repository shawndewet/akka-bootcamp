using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChartApp.Actors
{
    #region Reporting

    public class GatherMetrics { }

    public class Metric
    {
        public Metric(string series, float counterValue)
        {
            CounterValue = counterValue;
            Series = series;
        }

        public float CounterValue { get; }
        public string Series { get; }
    }

    #endregion

    #region PerfCounter Mngt

    public enum CounterType
    {
        Cpu,
        Memory,
        Disk
    }

    /// <summary>
    /// Enables a counter and begins publishing values to <see cref="Subscriber"/>
    /// </summary>
    public class SubscribeCounter
    {
        public SubscribeCounter(CounterType counter, IActorRef subscriber)
        {
            Counter = counter;
            Subscriber = subscriber;
        }

        public CounterType Counter { get; }
        public IActorRef Subscriber { get; }
    }

    public class UnsubscribeCounter
    {
        public UnsubscribeCounter(CounterType counter, IActorRef subscriber)
        {
            Counter = counter;
            Subscriber = subscriber;
        }

        public CounterType Counter { get; }
        public IActorRef Subscriber { get; }
    }



    #endregion
}
