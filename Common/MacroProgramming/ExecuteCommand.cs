using System;
using System.Diagnostics;

namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class ExecuteCommand : Chainable
    {
        private string _args;
        private string _command;
        private DigitalSignal _in;
        private DigitalSignal _out;

        public ExecuteCommand()
        {
            In = new DigitalSignal();
            Out = new DigitalSignal();
        }

        public string Command
        {
            get { return _command; }
            set { _command = value; }
        }

        public string Args
        {
            get { return _args; }
            set { _args = value; }
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

        private void Execute()
        {
            try
            {
                Process.Start(_command, _args);
            }
            catch (Exception)
            {
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