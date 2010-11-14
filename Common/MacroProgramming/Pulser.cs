using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class Pulser:Chainable
    {
        private TimeSpan _timeHigh = TimeSpan.Zero;
        private TimeSpan _timeLow = TimeSpan.Zero;
        private DigitalSignal _in = null;
        private DigitalSignal _out = null;
        private Macro _macro = new Macro();
        [NonSerialized]
        private Thread _workerThread =null;
        private volatile bool _keepRunning = false;
        public Pulser():base()
        {
            this.In = new DigitalSignal();
            this.Out = new DigitalSignal();
        }
        private void Start()
        {
            if (_workerThread !=null && _workerThread.IsAlive)
            {
                Util.AbortThread(_workerThread);
            }
            _workerThread = new Thread(PulseWork);
            _workerThread.SetApartmentState(ApartmentState.STA);
            _workerThread.IsBackground = true;
            _keepRunning = true;
            _workerThread.Start();
        }
        private void Stop()
        {
            _keepRunning = false;
            if (_workerThread != null)
            {
                if (_workerThread.IsAlive)
                {
                    Util.AbortThread(_workerThread);
                }
            }
            if (_macro != null)
            {
                _macro.In.State = false;
            }
            if (_out != null)
            {
                _out.State = false;
            }
        }
        private void PulseWork()
        {
            if (_out != null)
            {
                _out.State = false;
            }
            do
            {
                if (_macro != null)
                {
                    _macro.In.State = true;
                }
                Thread.Sleep(_timeHigh);
                if (_macro != null)
                {
                    _macro.In.State = false;
                }
                Thread.Sleep(_timeLow);
            } while (_keepRunning);
            if (_out != null)
            {
                _out.State = true;
            }
        }
        private void _in_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }
        public TimeSpan TimeHigh
        {
            get
            {
                return _timeHigh;
            }
            set
            {
                _timeHigh = value;
            }
        }
        public TimeSpan TimeLow
        {
            get
            {
                return _timeLow;
            }
            set
            {
                _timeLow = value;
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
                _in = value;
                _in.SignalChanged += _in_SignalChanged;
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
        
    }
}
