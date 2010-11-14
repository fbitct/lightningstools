using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class AnalogSignalNormalizer : Chainable
    {
        private AnalogSignal _in = null;
        private AnalogSignal _out = null;
        private Range _inputRange = new Range();
        private Range _outputRange = new Range();

        public AnalogSignalNormalizer()
            : base()
        {
            this.In = new AnalogSignal();
            this.InputRange = new Range();
            this.InputRange.LowerInclusiveBound = 0.0;
            this.InputRange.UpperInclusiveBound = 1.0;
            this.OutputRange = new Range();
            this.OutputRange.LowerInclusiveBound = 0.0;
            this.OutputRange.UpperInclusiveBound = 1.0;
            this.Out = new AnalogSignal();
        }
        private void _in_SignalChanged(object sender, AnalogSignalChangedEventArgs e)
        {
            Evaluate(e.CurrentState);
        }

        private void Evaluate(double newVal)
        {
            if (newVal > _inputRange.LowerInclusiveBound && newVal  < _inputRange.UpperInclusiveBound)
            {
                if (_out != null)
                {
                    double newValAsPercentageOfInputRange =((newVal-_inputRange.LowerInclusiveBound) / _inputRange.Width) ;
                    _out.State = (newValAsPercentageOfInputRange * (_outputRange.Width)) + _outputRange.LowerInclusiveBound;
                }
            }
            else if (newVal <= _inputRange.LowerInclusiveBound)
            {
                if (_out != null)
                {
                    _out.State = _outputRange.LowerInclusiveBound;
                }
            }
            else if (newVal >= _inputRange.UpperInclusiveBound)
            {
                if (_out != null)
                {
                    _out.State = _outputRange.UpperInclusiveBound;
                }
            }
            else
            {
                if (_out != null)
                {
                    _out.State = _outputRange.LowerInclusiveBound;
                }
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
                if (_in != null)
                {
                    Evaluate(_in.State);
                }
            }
        }
        public Range InputRange
        {
            get
            {
                return _inputRange;
            }
            set
            {
                if (value == null)
                {
                    value = new Range();
                }
                _inputRange = value;
            }
        }
        public Range OutputRange
        {
            get
            {
                return _outputRange;
            }
            set
            {
                if (value == null)
                {
                    value = new Range();
                }
                _outputRange = value;
            }
        }
        public AnalogSignal Out
        {
            get
            {
                return _out;
            }
            set
            {
                if (value == null)
                {
                    value = new AnalogSignal();
                }
                _out = value;
                if (_in != null)
                {
                    Evaluate(_in.State);
                }
            }
        }



    }
}
