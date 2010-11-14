using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class FlipFlop:Chainable
    {
        private DigitalSignal _in = null;
        private DigitalSignal _reset = null;
        private DigitalSignal _out = null;
        private bool _currentValue = false;

        public FlipFlop()
            :base()
        {
            this.In = new DigitalSignal();
            this.Reset = new DigitalSignal();
            this.Out = new DigitalSignal();
        }

        public bool CurrentValue
        {
            get
            {
                return _currentValue;
            }
        }
        public DigitalSignal In
        {
            get
            {
                return _in;
            }
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
        public DigitalSignal Out
        {
            get
            {
                return _out;
            }
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
            get
            {
                return _reset;
            }
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
        private void _reset_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                _currentValue = false;
            }
        }
        private void _in_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
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
