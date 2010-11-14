using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public enum MouseButton
    {
        Left,
        Middle,
        Right
    }
    [Serializable]
    public sealed class MouseClick:Chainable
    {
        private MouseButton _toClick = MouseButton.Left;
        private bool _press = true;
        private bool _release = true;
        private DigitalSignal _in = null;
        private DigitalSignal _out = null;

        public MouseClick():base()
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

        public bool Press
        {
            get
            {
                return _press;
            }
            set
            {
                _press = value;
            }
        }
        public bool Release
        {
            get
            {
                return _release;
            }
            set
            {
                _release = value;
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
                        if (_press && _release)
                        {
                            KeyAndMouseFunctions.LeftClick();
                        }
                        else if (_press)
                        {
                            KeyAndMouseFunctions.LeftDown();
                        }
                        else if (_release)
                        {
                            KeyAndMouseFunctions.LeftUp();
                        }
                        break;
                    case MouseButton.Middle:
                        if (_press && _release)
                        {
                            KeyAndMouseFunctions.MiddleClick();
                        }
                        else if (_press)
                        {
                            KeyAndMouseFunctions.MiddleDown();
                        }
                        else if (_release)
                        {
                            KeyAndMouseFunctions.MiddleUp();
                        }
                        break;
                    case MouseButton.Right:
                        if (_press && _release)
                        {
                            KeyAndMouseFunctions.RightClick();
                        }
                        else if (_press)
                        {
                            KeyAndMouseFunctions.RightDown();
                        }
                        else if (_release)
                        {
                            KeyAndMouseFunctions.RightUp();
                        }
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
