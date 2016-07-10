using System;
using System.Xml.Serialization;
using log4net;
using Common.Statistics;

namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class AnalogSignalChangedEventArgs : EventArgs
    {
        private readonly double _currentState;
        private readonly double _previousState;
        private readonly double _currentCorrelatedState;

        public AnalogSignalChangedEventArgs(double currentState, double previousState, double currentCorrelatedState=0)
        {
            _currentState = currentState;
            _currentCorrelatedState = currentCorrelatedState;
            _previousState = previousState;

        }

        public double CurrentState
        {
            get { return _currentState; }
        }
        public double CurrentCorrelatedState
        {
            get { return _currentCorrelatedState; }
        }
        public double PreviousState
        {
            get { return _previousState; }
        }
    }

    [Serializable]
    public class AnalogSignal : Signal
    {
        #region Delegates

        public delegate void AnalogSignalChangedEventHandler(object sender, AnalogSignalChangedEventArgs args);

        #endregion

        [field: NonSerializedAttribute] private static ILog _log = LogManager.GetLogger(typeof (AnalogSignal));
        private int _precision = -1; //# decimal places to round values to
        private TimestampedDecimal _previousState = new TimestampedDecimal() { Timestamp = DateTime.UtcNow, Value = 0 };
        private TimestampedDecimal _state = new TimestampedDecimal() { Timestamp = DateTime.UtcNow, Value = 0 };
        private TimestampedDecimal _correlatedState = new TimestampedDecimal() { Timestamp = DateTime.UtcNow, Value = 0 };
        private bool _isSine;
        private bool _isCosine;
        
        public int Precision
        {
            get { return _precision; }
            set { _precision = value; }
        }
        public bool IsVoltage { get; set; }
        public bool IsSine { get { return _isSine; } set { _isSine = value; _isCosine = false; IsAngle = true; MinValue = -1; MaxValue = 1; } }
        public bool IsCosine { get { return _isCosine; } set { _isCosine = value; _isSine = false; IsAngle = true; MinValue = -1; MaxValue = 1; } }
        public bool IsAngle {get;set;}
        public bool IsPercentage { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public Nullable<double> TimeConstant { get; set; }
        [XmlIgnore]
        public virtual double State
        {
            get 
            {
                return _state.Value;
            }
            set
            {
                var newVal =
                    _precision != -1
                        ? System.Math.Round(value, _precision)
                        : value;
                if (double.IsInfinity(newVal) || double.IsNaN(newVal))
                {
                    newVal = 0;
                }
                if (newVal != _state.Value)
                {
                    _previousState = _state;
                    _state = new TimestampedDecimal { Timestamp = DateTime.UtcNow, Value = newVal };
                    UpdateEventListeners();
                }
            }
        }
        [XmlIgnore]
        public virtual double CorrelatedState
        {
            get
            {
                return _correlatedState.Value;
            }
            set
            {
                var newVal =
                    _precision != -1
                        ? System.Math.Round(value, _precision)
                        : value;
                if (double.IsInfinity(newVal) || double.IsNaN(newVal))
                {
                    newVal = 0;
                }
                if (newVal != _correlatedState.Value)
                {
                    _correlatedState = new TimestampedDecimal { Timestamp = DateTime.UtcNow, Value = newVal };
                }
            }
        }

        public virtual TimestampedDecimal TimestampedState { get { return _state; } }
        [XmlIgnore]
        public override string SignalType
        {
            get { return "Analog / Numeric"; }
        }

        [field: NonSerializedAttribute]
        public event AnalogSignalChangedEventHandler SignalChanged;

        protected virtual void UpdateEventListeners()
        {
            if (SignalChanged != null)
            {
                SignalChanged(this, new AnalogSignalChangedEventArgs(State, _previousState.Value, CorrelatedState));
            }
        }
    }
}