﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using Akka.Actor;

namespace ChartApp.Actors
{
    /// <summary>
    /// Actor responsible for translating UI calls into ActorSystem messages
    /// </summary>
    public class PerformanceCounterCoordinatorActor : ReceiveActor
    {
        #region Message Types

        /// <summary>
        /// Subscribe the <see cref="ChartingActor"/> to updates for <see cref="Counter"/>
        /// </summary>
        public class Watch
        {
            public Watch(CounterType counter)
            {
                Counter = counter;
            }

            public CounterType Counter { get; }
        }

        public class Unwatch
        {
            public Unwatch(CounterType counter)
            {
                Counter = counter;
            }

            public CounterType Counter { get; }
        }

        #endregion

        private static readonly Dictionary<CounterType, Func<PerformanceCounter>> CounterGenerators = new Dictionary<CounterType, Func<PerformanceCounter>>(){
            { CounterType.Cpu, ()=> new PerformanceCounter("Processor", "% processor time", "_Total", true)},
            { CounterType.Memory, ()=> new PerformanceCounter("Memory", "% committed bytes in use", true)},
            { CounterType.Disk, ()=> new PerformanceCounter("LogicalDisk", "% Disk Time", "_Total", true)}
        };

        private static readonly Dictionary<CounterType, Func<Series>> SeriesGenerators = new Dictionary<CounterType, Func<Series>>(){
            { 
                CounterType.Cpu, () => new Series(CounterType.Cpu.ToString()){
                    ChartType = SeriesChartType.SplineArea,
                    Color = Color.DarkGreen
                }
            },
            {
                CounterType.Memory, () => new Series(CounterType.Memory.ToString()){
                    ChartType = SeriesChartType.FastLine,
                    Color = Color.MediumBlue
                }
            },
            {
                CounterType.Disk, () => new Series(CounterType.Disk.ToString()){
                    ChartType = SeriesChartType.SplineArea,
                    Color = Color.DarkRed
                }
            }
        };

        private Dictionary<CounterType, IActorRef> _counterActors;
        private IActorRef _chartingActor;


        public PerformanceCounterCoordinatorActor(IActorRef chartingActor):
            this(chartingActor, new Dictionary<CounterType, IActorRef>())
        {

        }

        public PerformanceCounterCoordinatorActor(IActorRef chartingActor, Dictionary<CounterType, IActorRef> counterActors)
        {
            _chartingActor = chartingActor;
            _counterActors = counterActors;

            Receive<Watch>(watch =>
            {
                if (!counterActors.ContainsKey(watch.Counter))
                {
                    var counterActor = Context.ActorOf(Props.Create(() => new PerformanceCounterActor(watch.Counter.ToString(), CounterGenerators[watch.Counter])));

                    //add this counter actor to our index
                    _counterActors[watch.Counter] = counterActor;
                }


                _chartingActor.Tell(new ChartingActor.AddSeries(SeriesGenerators[watch.Counter]()));

                _counterActors[watch.Counter].Tell(new SubscribeCounter(watch.Counter, _chartingActor));
            });


            Receive<Unwatch>(unwatch =>
            {
                if (!_counterActors.ContainsKey(unwatch.Counter))
                {
                    return; //nothing to do
                }

                _counterActors[unwatch.Counter].Tell(new UnsubscribeCounter(unwatch.Counter, _chartingActor));

                _chartingActor.Tell(new ChartingActor.RemoveSeries(unwatch.Counter.ToString()));

            });


        }
    }
}
