using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public enum LogicOperation
    {
        Not,
        And,
        Or,
        Nand,
        Nor,
        Xor,
        XNor,
        False,
        True
    }
    [Serializable]
    public sealed class LogicGate:Chainable
    {
        private LogicOperation _operation = LogicOperation.False;
        private DigitalSignal _in1 = null;
        private DigitalSignal _in2 = null;
        private DigitalSignal _out = null;
        public LogicGate():base()
        {
            this.In1 = new DigitalSignal();
            this.In2= new DigitalSignal();
            this.Out = new DigitalSignal();
        }
        private void _in_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (_out == null)
                return;
            switch (_operation) {
                case LogicOperation.And:
                    _out.State = (_in1.State & _in2.State);
                    break;
                case LogicOperation.False:
                    _out.State = (_in1.State == false && _in2.State ==false);
                    break;
                case LogicOperation.Nand:
                    _out.State = !(_in1.State & _in2.State);
                    break;
                case LogicOperation.Nor:
                    _out.State = !(_in1.State | _in2.State);
                    break;
                case LogicOperation.Not:
                    _out.State = !_in1.State;
                    break;
                case LogicOperation.Or:
                    _out.State = (_in1.State | _in2.State);
                    break;
                case LogicOperation.True:
                    _out.State = (_in1.State == true && _in2.State == true);
                    break;
                case LogicOperation.XNor:
                    _out.State = !(_in1.State ^ _in2.State);
                    break;
                case LogicOperation.Xor:
                    _out.State = (_in1.State ^ _in2.State);
                    break;
                default:
                    break;
            }
        }

        public LogicOperation Operation
        {
            get
            {
                return _operation;
            }
            set
            {
                _operation = value;
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
        public DigitalSignal In1
        {
            get
            {
                return _in1;
            }
            set
            {
                if (value == null)
                {
                    value = new DigitalSignal();
                }
                value.SignalChanged += _in_SignalChanged;
                _in1 = value;
            }
        }
        public DigitalSignal In2
        {
            get
            {
                return _in2;
            }
            set
            {
                if (value == null)
                {
                    value = new DigitalSignal();
                }
                value.SignalChanged += _in_SignalChanged;
                _in2 = value;
            }
        }
        

    }
}
