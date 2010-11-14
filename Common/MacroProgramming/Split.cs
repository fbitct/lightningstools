using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class Split:Chainable
    {
        private DigitalSignal _in = null;
        private List<DigitalSignal> _outs= new List<DigitalSignal>();
        private struct SignalValue
        {
            public DigitalSignal signal;
            public bool value;
        }
        public Split():base()
        {
            _in = new DigitalSignal();
        }
        private void _in_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                if (_outs !=null) {
                    for (int i = 0; i < _outs.Count; i++)
                    {
                        if (_outs[i] != null)
                        {
                            //_outs[i].State = true;
                            SignalValue sigVal = new SignalValue();
                            sigVal.signal = _outs[i];
                            sigVal.value = true;
                            Thread t = new Thread(new ParameterizedThreadStart(SendSignal));
                            t.SetApartmentState(ApartmentState.STA);
                            t.IsBackground = true;
                            t.Start(sigVal);
                        }
                    }
                }
            }
            else
            {
                if (_outs != null)
                {
                    for (int i = 0; i < _outs.Count; i++)
                    {
                        if (_outs[i] != null)
                        {
                            //_outs[i].State = false;
                            SignalValue sigVal = new SignalValue();
                            sigVal.signal = _outs[i];
                            sigVal.value = false;
                            Thread t = new Thread(new ParameterizedThreadStart(SendSignal));
                            t.SetApartmentState(ApartmentState.STA);
                            t.IsBackground = true;
                            t.Start(sigVal);
                        }
                    }
                }
            }
        }
        public DigitalSignal CreateAdditionalOut()
        {
            DigitalSignal newSig = new DigitalSignal();
            _outs.Add(newSig);
            return newSig;
        }
        private void SendSignal(object s) {
            SignalValue sigVal = (SignalValue)s;
            sigVal.signal.State = sigVal.value;
        }

        public List<DigitalSignal> Outs
        {
            get
            {
                return _outs;
            }
            set
            {
                if (value == null)
                {
                    value = new List<DigitalSignal>();
                }
                _outs = value;
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
