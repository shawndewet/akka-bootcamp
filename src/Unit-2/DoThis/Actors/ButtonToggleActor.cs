using Akka.Actor;
using System;
using System.Windows.Forms;

namespace ChartApp.Actors
{
    public class ButtonToggleActor : UntypedActor
    {
        #region Message Types 

        /// <summary>
        /// Toggles this button on or off and sends and appropriate message to the <see cref="PerformanceCounterCoordinatorActor"/>
        /// </summary>
        public class Toggle { }

        #endregion

        private readonly IActorRef _coordinatorActor;
        private readonly Button _myButton;
        private readonly CounterType _myCounterType;
        private bool _isToggledOn;
        
        
        public ButtonToggleActor(IActorRef coordinatorActor, Button myButton, CounterType myCounterType, bool isToggledOn = false)
        {
            _coordinatorActor = coordinatorActor;
            _myButton = myButton;
            _myCounterType = myCounterType;
            _isToggledOn = isToggledOn;
        }

        protected override void OnReceive(object message)
        {
            if (message is Toggle)
            {
                if (_isToggledOn)
                    _coordinatorActor.Tell(new PerformanceCounterCoordinatorActor.Unwatch(_myCounterType));
                else
                    _coordinatorActor.Tell(new PerformanceCounterCoordinatorActor.Watch(_myCounterType));
                FlipToggle();
            }
            else
                Unhandled(message);
        }

        private void FlipToggle()
        {
            _isToggledOn = !_isToggledOn;

            _myButton.Text = $"{_myCounterType.ToString().ToUpperInvariant()} ({(_isToggledOn ? "ON" : "OFF")})";

        }
    }
}
