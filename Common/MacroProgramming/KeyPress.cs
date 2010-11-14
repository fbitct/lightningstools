using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class KeyPress:Chainable
    {
        private Keys _keyCode = Keys.None;
        private bool _press = true;
        private bool _release = true;
        private DigitalSignal _in = null;
        private DigitalSignal _out = null;
        private bool _extendedKey = false;
        public KeyPress():base()
        {
            this.In = new DigitalSignal();
            this.Out= new DigitalSignal();
        }

        private void _in_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                Out.State = false;
                if (_press)
                {
                    KeyAndMouseFunctions.SendKey(_keyCode, _extendedKey, true, false);
                }
                System.Threading.Thread.Sleep(50);
                if (_release)
                {
                    KeyAndMouseFunctions.SendKey(_keyCode, _extendedKey, false, true);
                }
                Out.State = true;
            }
            else
            {
                Out.State = false;
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
        public bool ExtendedKey
        {
            get
            {
                return _extendedKey;
            }
            set
            {
                _extendedKey = value;
            }
        }
        public Keys KeyCode
        {
            get
            {
                return _keyCode;
            }
            set
            {
                _keyCode = value;
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
        
    }
}
