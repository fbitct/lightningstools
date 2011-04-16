using System;

namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class Inverter : Chainable
    {
        private DigitalSignal _in;
        private DigitalSignal _out;

        public Inverter()
        {
            In = new DigitalSignal();
            Out = new DigitalSignal();
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

        private void _in_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                if (_out != null)
                {
                    _out.State = false; //inverse of normal
                }
            }
            else
            {
                if (_out != null)
                {
                    _out.State = true; //inverse of normal
                }
            }
        }
    }
}