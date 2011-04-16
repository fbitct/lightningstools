using System;

namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class Counter : Chainable
    {
        private long _currentValue;
        private DigitalSignal _in;
        private long _increment = 1;
        private long _initialValue;
        private AnalogSignal _out;
        private DigitalSignal _reset;

        public Counter()
        {
            In = new DigitalSignal();
            Reset = new DigitalSignal();
            Out = new AnalogSignal();
        }

        public long CurrentValue
        {
            get { return _currentValue; }
        }

        public long Increment
        {
            get { return _increment; }
            set { _increment = value; }
        }

        public long InitialValue
        {
            get { return _initialValue; }
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
                value.SignalChanged += _in_SignalChanged;
                _in = value;
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
                value.SignalChanged += _reset_SignalChanged;
                _reset = value;
            }
        }

        public AnalogSignal Out
        {
            get { return _out; }
            set
            {
                if (value == null)
                {
                    value = new AnalogSignal();
                }
                _out = value;
            }
        }

        private void _reset_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                ResetCounter();
            }
        }

        private void ResetCounter()
        {
            _currentValue = _initialValue;
        }

        private void _in_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                try
                {
                    _currentValue += _increment;
                }
                catch (OverflowException)
                {
                    if (_currentValue == long.MaxValue)
                    {
                        _currentValue = long.MinValue;
                    }
                    else
                    {
                        _currentValue = long.MaxValue;
                    }
                }
                if (_out != null)
                {
                    _out.State = _currentValue;
                }
            }
        }
    }
}