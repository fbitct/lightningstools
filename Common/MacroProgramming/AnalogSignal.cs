using System;
using System.Xml.Serialization;
using Common.SimSupport;
using log4net;

namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class AnalogSignalChangedEventArgs : EventArgs
    {
        private readonly double _currentState;
        private readonly double _previousState;

        public AnalogSignalChangedEventArgs(double currentState, double previousState)
        {
            _currentState = currentState;
            _previousState = previousState;
        }

        public double CurrentState
        {
            get { return _currentState; }
        }

        public double PreviousState
        {
            get { return _previousState; }
        }
    }

    [Serializable]
    [XmlInclude(typeof (AnalogSimOutput))]
    public class AnalogSignal : Signal
    {
        #region Delegates

        public delegate void AnalogSignalChangedEventHandler(object sender, AnalogSignalChangedEventArgs args);

        #endregion

        [field: NonSerializedAttribute] private static ILog _log = LogManager.GetLogger(typeof (AnalogSignal));

        private int _precision = 5; //# decimal places to compare to determine if change is present 
        private double _previousState;
        private double _state;

        public int Precision
        {
            get { return _precision; }
            set { _precision = value; }
        }

        [XmlIgnore]
        public double State
        {
            get { return _state; }
            set
            {
                //if magnitude of change is at least in the minimum order of magnitude, forward it on
                if (System.Math.Abs(_state - value) > (System.Math.Pow(10, -_precision)))
                {
                    _previousState = _state;
                    _state = value;
                    UpdateEventListeners();
                }
            }
        }

        public override string SignalType
        {
            get { return "Analog / Numeric"; }
        }

        [field: NonSerializedAttribute]
        public event AnalogSignalChangedEventHandler SignalChanged;

        public void UpdateEventListeners()
        {
            if (SignalChanged != null)
            {
                SignalChanged(this, new AnalogSignalChangedEventArgs(_state, _previousState));
            }
        }
    }
}