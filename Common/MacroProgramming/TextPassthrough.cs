﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class TextPassthrough : Chainable
    {
        private TextSignal _in = null;
        private TextSignal _out = null;

        public TextPassthrough()
            : base()
        {
            this.In = new TextSignal();
            this.Out = new TextSignal();
        }

        public TextSignal In
        {
            get
            {
                return _in;
            }
            set
            {
                if (value == null)
                {
                    value = new TextSignal();
                }
                value.SignalChanged += _in_SignalChanged;
                _in = value;
            }
        }
        public TextSignal Out
        {
            get
            {
                return _out;
            }
            set
            {
                if (value == null)
                {
                    value = new TextSignal();
                }
                _out = value;
            }
        }
        public void Refresh()
        {
            UpdateOutputValue(_in.State);
        }
        private void _in_SignalChanged(object sender, TextSignalChangedEventArgs e)
        {
            UpdateOutputValue(e.CurrentState);
        }

        private void UpdateOutputValue(string outputValue)
        {
            if (_out != null)
            {
                _out.State = outputValue;
            }
        }
    }
}
