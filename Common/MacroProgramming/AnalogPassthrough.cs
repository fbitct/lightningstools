using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class AnalogPassthrough : Chainable
    {
        private AnalogSignal _in = null;
        private AnalogSignal _out = null;

        public AnalogPassthrough()
            : base()
        {
            this.In = new AnalogSignal();
            this.Out= new AnalogSignal();
        }

        public AnalogSignal In
        {
            get
            {
                return _in;
            }
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
            get
            {
                return _out;
            }
            set
            {
                if (value == null)
                {
                    value = new AnalogSignal();
                }
                _out = value;
            }
        }
        private void _in_SignalChanged(object sender, AnalogSignalChangedEventArgs e)
        {
            if (_out != null)
            {
                _out.State = e.CurrentState;
            }
        }
    }
}
