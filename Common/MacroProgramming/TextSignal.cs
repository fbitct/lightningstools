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
    public sealed class TextSignalChangedEventArgs : EventArgs
    {
        private string _currentState = string.Empty;
        private string _previousState = string.Empty;
        public TextSignalChangedEventArgs(string currentState, string previousState)
            : base()
        {
            _currentState = currentState;
            _previousState = previousState;
        }
        public string CurrentState
        {
            get
            {
                return _currentState;
            }
        }
        public string PreviousState
        {
            get
            {
                return _previousState;
            }
        }
    }
    [Serializable]
    [XmlInclude (typeof(TextSimOutput))]
    public class TextSignal : Signal
    {
        public delegate void TextSignalChangedEventHandler(object sender, TextSignalChangedEventArgs args);
        [field: NonSerializedAttribute()]
        public event TextSignalChangedEventHandler SignalChanged;
        [NonSerialized]
        private string _state = string.Empty;
        public TextSignal()
            : base()
        {
        }
        [XmlIgnore]
        public string State
        {
            get
            {
                return _state;
            }
            set
            {
                if (_state != value)
                {
                    string previousState = _state;
                    _state = value;
                    if (SignalChanged != null)
                    {
                        SignalChanged(this, new TextSignalChangedEventArgs(value, previousState));
                    }
                }
            }
        }
        public override string SignalType
        {
            get
            {
                return "Text / Characters";
            }
        }

    }
}
