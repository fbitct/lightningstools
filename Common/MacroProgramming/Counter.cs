using System;

namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class Counter : Chainable
    {
        private long _currentValue;
        private DigitalSignal _in;
        private long _increment = 1;
        private long _initialValue = 0;
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
                value.SignalChanged += InSignalChanged;
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
                value.SignalChanged += ResetSignalChanged;
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

        private void ResetSignalChanged(object sender, DigitalSignalChangedEventArgs e)
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

        private void InSignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (!e.CurrentState) return;
            try
            {
                _currentValue += _increment;
            }
            catch (OverflowException)
            {
                _currentValue = _currentValue == long.MaxValue ? long.MinValue : long.MaxValue;
            }
            if (_out != null)
            {
                _out.State = _currentValue;
            }
        }
    }
}