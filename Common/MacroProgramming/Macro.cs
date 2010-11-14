using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class Macro:Chainable
    {
        private DigitalSignal _toStart = null;
        private DigitalSignal _waitFor = null;
        private DigitalSignal _in = null;
        private DigitalSignal _out = null;
        [NonSerialized]
        private Thread _workerThread=null;
        private volatile bool _keepWaiting = false;
        public Macro():base()
        {
            this.ToStart = new DigitalSignal();
            this.WaitFor= new DigitalSignal();
            this.In= new DigitalSignal();
            this.Out= new DigitalSignal();
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
        private void Work()
        {
            _waitFor.State = false;
            if (_toStart != null)
            {
                _toStart.State = true;
            }
            while (_keepWaiting)
            {
                Thread.Sleep(20);
            }
            _toStart.State = false;
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
            _keepWaiting = true;
            _workerThread.Start();
            _workerThread.Join();
            
        }
        public void Stop()
        {
            _keepWaiting = false;
            if (_workerThread != null && _workerThread.IsAlive)
            {
                Util.AbortThread(_workerThread);
            }
            _toStart.State = false;
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

        public DigitalSignal ToStart
        {
            get
            {
                return _toStart;
            }
            set
            {
                if (value == null)
                {
                    value = new DigitalSignal();
                }
                _toStart = value;
            }
        }
        public DigitalSignal WaitFor
        {
            get
            {
                return _waitFor;
            }
            set
            {
                if (value == null)
                {
                    value = new DigitalSignal();
                }
                value.SignalChanged += _waitFor_SignalChanged;
                _waitFor = value;
            }
        }
        private void _waitFor_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                _keepWaiting = false;
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
