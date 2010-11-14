using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Remoting.Contexts;
using System.Xml.Serialization;
using Common.SimSupport;
using log4net;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class AnalogSignalChangedEventArgs : EventArgs
    {
        private double _currentState = 0;
        private double _previousState = 0;
        public AnalogSignalChangedEventArgs(double currentState, double previousState)
            : base()
        {
            _currentState = currentState;
            _previousState = previousState;
        }
        public double CurrentState
        {
            get
            {
                return _currentState;
            }
        }
        public double PreviousState
        {
            get
            {
                return _previousState;
            }
        }
    }
    [Serializable]
    [XmlInclude(typeof(AnalogSimOutput))]
    public class AnalogSignal : Signal
    {
        [field: NonSerializedAttribute()]
        private static ILog _log = LogManager.GetLogger(typeof(AnalogSignal));

        public delegate void AnalogSignalChangedEventHandler(object sender, AnalogSignalChangedEventArgs args);
        [field: NonSerializedAttribute()]
        public event AnalogSignalChangedEventHandler SignalChanged;
        private double _state = 0;
        public AnalogSignal()
            : base()
        {
        }
        [XmlIgnore]
        public double State
        {
            get
            {
                return _state;
            }
            set
            {
                if (_state != value)
                {
                    double previousState = _state;
                    _state = value;
                    if (SignalChanged != null)
                    {
                        SignalChanged(this, new AnalogSignalChangedEventArgs(value, previousState));
                    }
                }
            }
        }
        public override string SignalType
        {
            get
            {
                return "Analog / Numeric";
            }
        }
    }
}
