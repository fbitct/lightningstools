using System;
using System.Xml.Serialization;
using Common.SimSupport;

namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class TextSignalChangedEventArgs : EventArgs
    {
        private readonly string _currentState = string.Empty;
        private readonly string _previousState = string.Empty;

        public TextSignalChangedEventArgs(string currentState, string previousState)
        {
            _currentState = currentState;
            _previousState = previousState;
        }

        public string CurrentState
        {
            get { return _currentState; }
        }

        public string PreviousState
        {
            get { return _previousState; }
        }
    }

    [Serializable]
    [XmlInclude(typeof (TextSimOutput))]
    public class TextSignal : Signal
    {
        #region Delegates

        public delegate void TextSignalChangedEventHandler(object sender, TextSignalChangedEventArgs args);

        #endregion

        [NonSerialized] private string _state = string.Empty;

        [XmlIgnore]
        public string State
        {
            get { return _state; }
            set
            {
                if (_state == value) return;
                var previousState = _state;
                _state = value;
                if (SignalChanged != null)
                {
                    SignalChanged(this, new TextSignalChangedEventArgs(value, previousState));
                }
            }
        }

        public override string SignalType
        {
            get { return "Text / Characters"; }
        }

        [field: NonSerializedAttribute]
        public event TextSignalChangedEventHandler SignalChanged;
    }
}