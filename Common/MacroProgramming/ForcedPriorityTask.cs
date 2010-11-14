using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class ForcedPriorityTask:Chainable
    {
        private DigitalSignal _in = null;
        private DigitalSignal _out = null;
        private Macro _macro = new Macro();

        public ForcedPriorityTask():base()
        {
            this.In = new DigitalSignal();
            this.Out= new DigitalSignal();
        }

        private void _in_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                if (_out != null)
                {
                    _out.State = false;
                }
                if (_macro != null)
                {
                    _macro.In.State = true;
                }
                if (_out != null)
                {
                    _out.State = true;
                }

            }
            else
            {
                if (_macro != null)
                {
                    _macro.In.State = false;
                }
                if (_out != null)
                {
                    _out.State = false;
                }
            }
        }

        public Macro Macro
        {
            get
            {
                return _macro;
            }
            set
            {
                if (value == null)
                {
                    value = new Macro();
                }
                _macro = value;
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
