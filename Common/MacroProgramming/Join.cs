using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class Join:Chainable
    {
        private List<DigitalSignal> _ins = new List<DigitalSignal>();
        private DigitalSignal _out = null;

        public Join():base()
        {
            this.Out = new DigitalSignal();
        }
        private void _in_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                bool bPulse = true;
                if (_ins != null)
                {
                    for (int i = 0; i < _ins.Count; i++)
                    {
                        if (!_ins[i].State)
                        {
                            bPulse = false;
                            break;
                        }
                    }
                }
                if (_out != null)
                {
                    if (bPulse)
                    {
                        _out.State = true;
                    }
                    else
                    {
                        _out.State = false;
                    }
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
        public DigitalSignal CreateAdditionalIn()
        {
            DigitalSignal newSig = new DigitalSignal();
            _ins.Add(newSig);
            return newSig;
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
        public List<DigitalSignal> Ins
        {
            get
            {
                return _ins;
            }
            set
            {
                if (value == null)
                {
                    value = new List<DigitalSignal>();
                }
                for (int i = 0; i < value.Count; i++)
                {
                    if (value[i] != null)
                    {
                        value[i].SignalChanged += _in_SignalChanged;
                    }
                }
                _ins = value;
            }
        }
        
    }
}
