using System;

namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class AnalogPassthrough : Chainable
    {
        private AnalogSignal _in;
        private AnalogSignal _out;

        public AnalogPassthrough()
        {
            In = new AnalogSignal();
            Out = new AnalogSignal();
        }

        public AnalogSignal In
        {
            get { return _in; }
            set
            {
                if (value == null)
                {
                    value = new AnalogSignal();
                }
                value.SignalChanged += _in_SignalChanged;
                _in = value;
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

        public void Refresh()
        {
            UpdateOutputValue(_in.State);
        }

        private void _in_SignalChanged(object sender, AnalogSignalChangedEventArgs e)
        {
            UpdateOutputValue(e.CurrentState);
        }

        private void UpdateOutputValue(double outputState)
        {
            if (_out != null)
            {
                _out.State = outputState;
            }
        }
    }
}