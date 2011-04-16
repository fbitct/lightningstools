using System;

namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class BinaryCodedDecimalDigitDecoder : Chainable
    {
        private DigitalSignal[] _in;
        private AnalogSignal _out;

        public BinaryCodedDecimalDigitDecoder()
        {
            var newIn = new DigitalSignal[4];
            for (int i = 0; i < newIn.Length; i++)
            {
                newIn[i] = new DigitalSignal();
            }
            In = newIn;
            Out = new AnalogSignal();
        }


        public DigitalSignal[] In
        {
            get { return _in; }
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
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] != null)
                    {
                        value[i].SignalChanged += _in_SignalChanged;
                    }
                }
                _in = value;
                Evaluate();
            }
        }

        public AnalogSignal Out
        {
            get { return _out; }
            set
            {
                if (value == null)
                {
                    value = new AnalogSignal();
                }
                _out = value;
                Evaluate();
            }
        }

        private void _in_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            Evaluate();
        }

        private void Evaluate()
        {
            if (_in == null) return;
            if (_out == null) return;
            int newVal = 0;

            for (int i = 0; i < _in.Length; i++)
            {
                if (_in[i].State)
                {
                    newVal |= (int) (System.Math.Pow(2, i));
                }
            }
            _out.State = newVal;
        }
    }
}