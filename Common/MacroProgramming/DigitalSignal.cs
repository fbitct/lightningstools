using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Remoting.Contexts;
using System.Xml.Serialization;
using Common.SimSupport;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class DigitalSignalChangedEventArgs : EventArgs
    {
        private bool _currentState = false;
        private bool _previousState = false;
        public DigitalSignalChangedEventArgs(bool currentState, bool previousState)
            : base()
        {
            _currentState = currentState;
            _previousState = previousState;
        }
        public bool CurrentState
        {
            get
            {
                return _currentState;
            }
        }
        public bool PreviousState
        {
            get
            {
                return _previousState;
            }
        }
       

    }
    [Serializable]
    [XmlInclude(typeof(DigitalSimOutput))]
    public class DigitalSignal:Signal
    {
        public delegate void SignalChangedEventHandler(object sender, DigitalSignalChangedEventArgs args);
        [field: NonSerializedAttribute()]
        public event SignalChangedEventHandler SignalChanged;
        [NonSerialized]
        private bool _state = false;
        [NonSerialized]
        private Inverter _inverter = null;
        public DigitalSignal()
            : base()
        {
        }
        [XmlIgnore]
        public DigitalSignal Inverse
        {
            get
            {
                if (_inverter == null)
                {
                    _inverter = new Inverter();
                    _inverter.In = this;
                }
                return _inverter.Out;
            }
        }
        public override string SignalType
        {
            get
            {
                return "Digital / Boolean";
            }
        }
        [XmlIgnore]
        public bool State
        {
            get
            {
                return _state;
            }
            set
            {
                if (_state != value)
                {
                    bool previousState = _state;
                    _state = value;
                    if (SignalChanged != null)
                    {
                        SignalChanged(this, new DigitalSignalChangedEventArgs(value, previousState));
                    }
                }
            }
        }


    }
}
