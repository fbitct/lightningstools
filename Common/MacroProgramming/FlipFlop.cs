using System;

namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class FlipFlop : Chainable
    {
        private bool _currentValue;
        private DigitalSignal _in;
        private DigitalSignal _out;
        private DigitalSignal _reset;

        public FlipFlop()
        {
            In = new DigitalSignal();
            Reset = new DigitalSignal();
            Out = new DigitalSignal();
        }

        public bool CurrentValue
        {
            get { return _currentValue; }
        }

        public DigitalSignal In
        {
            get { return _in; }
            set
            {
                if (value == null)
                {
                    value = new DigitalSignal();
                }
                value.SignalChanged += InSignalChanged;
                _in = value;
            }
        }

        public DigitalSignal Out
        {
            get { return _out; }
            set
            {
                if (value == null)
                {
                    value = new DigitalSignal();
                }
                _out = value;
            }
        }

        public DigitalSignal Reset
        {
            get { return _reset; }
            set
            {
                if (value == null)
                {
                    value = new DigitalSignal();
                }
                value.SignalChanged += ResetSignalChanged;
                _reset = value;
            }
        }

        private void ResetSignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                _currentValue = false;
            }
        }

        private void InSignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                _currentValue = !_currentValue;
                if (_out != null)
                {
                    _out.State = _currentValue;
                }
            }
        }
    }
}