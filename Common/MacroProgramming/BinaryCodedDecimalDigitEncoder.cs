using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class BinaryCodedDecimalDigitEncoder: Chainable
    {
        private DigitalSignal[] _out = null;
        private AnalogSignal _in = null;

        public BinaryCodedDecimalDigitEncoder()
            : base()
        {
            DigitalSignal[] newOut = new DigitalSignal[4];
            for (int i = 0; i < newOut.Length; i++)
            {
                newOut[i] = new DigitalSignal();
            }
            this.Out = newOut;
            this.In = new AnalogSignal();
            this.In.SignalChanged += new AnalogSignal.AnalogSignalChangedEventHandler(_in_SignalChanged);
        }

        private void _in_SignalChanged(object sender, AnalogSignalChangedEventArgs e)
        {
            Evaluate();
        }

        private void Evaluate()
        {
            if (_in == null) return;
            if (_out == null) return;
            long newVal = (int)_in.State;
            for (int i = 0; i < _out.Length; i++)
            {
                int thisMask =(int)(global::System.Math.Pow(2, i));
                if (_out[i] != null)
                {
                    _out[i].State = ((newVal & thisMask) == thisMask);
                }
            }
        }



        public DigitalSignal[] Out
        {
            get
            {
                return _out;
            }
            set
            {
                if (value == null)
                {
                    value = new DigitalSignal[4];
                    for (int i = 0; i < value.Length; i++)
                    {
                        value[i] = new DigitalSignal();
                    }
                }
                _out = value;
                Evaluate();
            }
        }

        public AnalogSignal In
        {
            get
            {
                return _in;
            }
            set
            {
                if (value == null)
                {
                    value = new AnalogSignal();
                }
                value.SignalChanged += _in_SignalChanged;
                _in = value;
                Evaluate();
            }
        }



    }
}
