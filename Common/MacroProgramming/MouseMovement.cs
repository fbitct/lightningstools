using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public enum MouseMoveMode
    {
        Relative,
        Absolute
    }
    [Serializable]
    public sealed class MouseMovement:Chainable
    {
        private int _x = 0;
        private int _y = 0;
        private DigitalSignal _in = null;
        private DigitalSignal _out = null;
        private MouseMoveMode _mode = MouseMoveMode.Relative;

        public MouseMovement():base()
        {
            this.In = new DigitalSignal();
            this.Out = new DigitalSignal();
        }
        private void _in_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                if (_out != null)
                {
                    _out.State = false;
                }
                if (_mode == MouseMoveMode.Relative)
                {
                    KeyAndMouseFunctions.MouseMoveRelative(_x, _y);
                }
                else if (_mode == MouseMoveMode.Absolute)
                {
                    KeyAndMouseFunctions.MouseMoveAbsolute(_x, _y);
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

        public int X
        {
            get
            {
                return _x;
            }
            set
            {
                if (_mode == MouseMoveMode.Absolute)
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                }
                _x = value;
            }
        }
        public int Y
        {
            get
            {
                return _y;
            }
            set
            {
                if (_mode == MouseMoveMode.Absolute)
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                }
                _y = value;
            }
        }
        public MouseMoveMode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
            }
        }
        
    }
}
