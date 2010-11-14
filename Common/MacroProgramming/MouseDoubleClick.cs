using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class MouseDoubleClick:Chainable
    {
        private MouseButton _toClick = MouseButton.Left;
        private DigitalSignal _in = null;
        private DigitalSignal _out = null;

        public MouseDoubleClick():base()
        {
            this.In = new DigitalSignal();
            this.Out= new DigitalSignal();
        }
        public MouseButton ToClick
        {
            get
            {
                return _toClick;
            }
            set
            {
                _toClick = value;
            }
        }

        private void _in_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                if (_out != null)
                {
                    _out.State = false;
                }
                switch (_toClick)
                {
                    case MouseButton.Left:
                        KeyAndMouseFunctions.LeftDoubleClick();
                        break;
                    case MouseButton.Middle:
                        KeyAndMouseFunctions.MiddleDoubleClick();
                        break;
                    case MouseButton.Right:
                        KeyAndMouseFunctions.RightDoubleClick();
                        break;
                    default:
                        break;
                }
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
