using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class Inverter:Chainable
    {
        private DigitalSignal _in = null;
        private DigitalSignal _out = null;

        public Inverter():base()
        {
            this.In = new DigitalSignal();
            this.Out = new DigitalSignal();
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
        

    }
}
