using System;
using System.Collections.Generic;
using System.Text;
using Common.MacroProgramming;
namespace Common.SimSupport
{
    [Serializable]
    public abstract class SimCommand:Chainable
    {
        private DigitalSignal _in = new DigitalSignal();
        private DigitalSignal _out = new DigitalSignal();
        public abstract void Execute();

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
                value.SignalChanged += new DigitalSignal.SignalChangedEventHandler(value_SignalChanged); ;
                _in = value;
            }
        }

        void value_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                if (_out != null)
                {
                    _out.State = false;
                }
                Execute();
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

        
    }
}
