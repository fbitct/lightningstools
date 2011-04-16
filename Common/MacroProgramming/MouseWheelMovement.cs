using System;

namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class MouseWheelMovement : Chainable
    {
        public const int WheelDelta = 120;

        private int _amount;

        private DigitalSignal _in;
        private DigitalSignal _out;

        public MouseWheelMovement()
        {
            In = new DigitalSignal();
            Out = new DigitalSignal();
        }

        public int Amount
        {
            get { return _amount; }
            set { _amount = value; }
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
                    _out.State = false;
                }
                KeyAndMouseFunctions.MouseWheelMove(_amount);
                if (_out != null)
                {
                    _out.State = true;
                }
            }
            else
            {
                if (_out != null)
                {
                    _out.State = false;
                }
            }
        }
    }
}