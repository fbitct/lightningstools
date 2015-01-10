using System;
using System.Xml.Serialization;

namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class DigitalSignalChangedEventArgs : EventArgs
    {
        private readonly bool _currentState;
        private readonly bool _previousState;

        public DigitalSignalChangedEventArgs(bool currentState, bool previousState)
        {
            _currentState = currentState;
            _previousState = previousState;
        }

        public bool CurrentState
        {
            get { return _currentState; }
        }

        public bool PreviousState
        {
            get { return _previousState; }
        }
    }

    [Serializable]
    public class DigitalSignal : Signal
    {
        #region Delegates

        public delegate void SignalChangedEventHandler(object sender, DigitalSignalChangedEventArgs args);

        #endregion

        [NonSerialized] private Inverter _inverter;
        [NonSerialized] private bool _state;

        [XmlIgnore]
        public DigitalSignal Inverse
        {
            get
            {
                if (_inverter == null)
                    _inverter = new Inverter {In = this};
                return _inverter.Out;
            }
        }

        public override string SignalType
        {
            get { return "Digital / Boolean"; }
        }

        [XmlIgnore]
        public bool State
        {
            get { return _state; }
            set
            {
                if (_state == value) return;
                var previousState = _state;
                _state = value;
                if (SignalChanged != null)
                {
                    SignalChanged(this, new DigitalSignalChangedEventArgs(value, previousState));
                }
            }
        }

        [field: NonSerializedAttribute]
        public event SignalChangedEventHandler SignalChanged;
    }
}