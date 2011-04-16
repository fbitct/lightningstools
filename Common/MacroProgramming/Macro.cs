using System;
using System.Threading;

namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class Macro : Chainable
    {
        private DigitalSignal _in;
        private volatile bool _keepWaiting;
        private DigitalSignal _out;
        private DigitalSignal _toStart;
        private DigitalSignal _waitFor;
        [NonSerialized] private Thread _workerThread;

        public Macro()
        {
            ToStart = new DigitalSignal();
            WaitFor = new DigitalSignal();
            In = new DigitalSignal();
            Out = new DigitalSignal();
        }

        public DigitalSignal In
        {
            get { return _in; }
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
            get { return _out; }
            set
            {
                if (value == null)
                {
                    value = new DigitalSignal();
                }
                _out = value;
            }
        }

        public DigitalSignal ToStart
        {
            get { return _toStart; }
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
            get { return _waitFor; }
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