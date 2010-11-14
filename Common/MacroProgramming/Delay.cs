using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class Delay:Chainable
    {
        private TimeSpan _delayTime = TimeSpan.MinValue;
        private DigitalSignal _in = null;
        private DigitalSignal _out = null;

        public Delay():base()
        {
            this.In = new DigitalSignal();
            this.Out = new DigitalSignal();
        }
        public TimeSpan DelayTime
        {
            get
            {
                return _delayTime;
            }
            set
            {
                _delayTime = value;
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
        private void _in_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                if (_out != null)
                {
                    _out.State = false;
                }
                Thread.Sleep(_delayTime);
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
