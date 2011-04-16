using System;

namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class MouseDoubleClick : Chainable
    {
        private DigitalSignal _in;
        private DigitalSignal _out;
        private MouseButton _toClick = MouseButton.Left;

        public MouseDoubleClick()
        {
            In = new DigitalSignal();
            Out = new DigitalSignal();
        }

        public MouseButton ToClick
        {
            get { return _toClick; }
            set { _toClick = value; }
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
    }
}