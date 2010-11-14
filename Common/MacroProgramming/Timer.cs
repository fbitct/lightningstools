using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Runtime.Remoting.Contexts;
using Common.MacroProgramming;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class Timer:Chainable
    {
        private TimeSpan _runningTime= TimeSpan.Zero;
        private DigitalSignal _in = null;
        private DigitalSignal _out = null;
        private Macro _macro=new Macro();
        [NonSerialized]
        private Thread _workerThread= null;
        public Timer():base()
        {
            _in = new DigitalSignal();
            _out = new DigitalSignal();
        }
        private void Work()
        {
            if (_out != null)
            {
                _out.State = false;
            }
            if (_macro != null)
            {
                _macro.In.State = true;
            }
            Thread.Sleep(System.Threading.Timeout.Infinite); //sleep till aborted
        }
        private void Start()
        {
            if (_workerThread != null && _workerThread.IsAlive)
            {
                Util.AbortThread(_workerThread);
            }
            _workerThread = new Thread(Work);
            _workerThread.SetApartmentState(ApartmentState.STA);
            _workerThread.IsBackground = true;
            _workerThread.Start();
            _workerThread.Join(_runningTime); //don't run for longer than _runningTime
            Util.AbortThread(_workerThread);
            if (_macro != null)
            {
                _macro.In.State = false;
            }
            if (_out != null)
            {
                _out.State = true;
            }
        }
        private void Stop()
        {
            if (_workerThread != null && _workerThread.IsAlive)
            {
                Util.AbortThread(_workerThread);
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

        public TimeSpan RunningTime
        {
            get
            {
                return _runningTime;
            }
            set
            {
                _runningTime = value;
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
