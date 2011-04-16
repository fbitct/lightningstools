using System;
using System.Collections.Generic;
using System.Threading;

namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class Split : Chainable
    {
        private DigitalSignal _in;
        private List<DigitalSignal> _outs = new List<DigitalSignal>();

        public Split()
        {
            _in = new DigitalSignal();
        }

        public List<DigitalSignal> Outs
        {
            get { return _outs; }
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

        private void _in_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                if (_outs != null)
                {
                    for (int i = 0; i < _outs.Count; i++)
                    {
                        if (_outs[i] != null)
                        {
                            //_outs[i].State = true;
                            var sigVal = new SignalValue();
                            sigVal.signal = _outs[i];
                            sigVal.value = true;
                            var t = new Thread(SendSignal);
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
                            var sigVal = new SignalValue();
                            sigVal.signal = _outs[i];
                            sigVal.value = false;
                            var t = new Thread(SendSignal);
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
            var newSig = new DigitalSignal();
            _outs.Add(newSig);
            return newSig;
        }

        private void SendSignal(object s)
        {
            var sigVal = (SignalValue) s;
            sigVal.signal.State = sigVal.value;
        }

        #region Nested type: SignalValue

        private struct SignalValue
        {
            public DigitalSignal signal;
            public bool value;
        }

        #endregion
    }
}