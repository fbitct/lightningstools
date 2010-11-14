using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class ExecuteCommand:Chainable
    {
        private string _command=null;
        private string _args=null;
        private DigitalSignal _in = null;
        private DigitalSignal _out = null;

        public ExecuteCommand()
            :base ()
        {
            this.In = new DigitalSignal();
            this.Out= new DigitalSignal();
        }

        private void Execute()
        {
            try
            {
                System.Diagnostics.Process.Start(_command, _args);
            }
            catch (Exception)
            {
            }
        }
        public string Command
        {
            get
            {
                return _command;
            }
            set
            {
                _command = value;
            }
        }
        public string Args
        {
            get
            {
                return _args;
            }
            set
            {
                _args = value;
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
        private void _in_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                if (_out != null)
                {
                    _out.State = false;
                }
                Execute();
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
