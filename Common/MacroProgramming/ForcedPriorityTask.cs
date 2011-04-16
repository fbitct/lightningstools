using System;

namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class ForcedPriorityTask : Chainable
    {
        private DigitalSignal _in;
        private Macro _macro = new Macro();
        private DigitalSignal _out;

        public ForcedPriorityTask()
        {
            In = new DigitalSignal();
            Out = new DigitalSignal();
        }

        public Macro Macro
        {
            get { return _macro; }
            set
            {
                if (value == null)
                {
                    value = new Macro();
                }
                _macro = value;
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
                if (_macro != null)
                {
                    _macro.In.State = true;
                }
                if (_out != null)
                {
                    _out.State = true;
                }
            }
            else
            {
                if (_macro != null)
                {
                    _macro.In.State = false;
                }
                if (_out != null)
                {
                    _out.State = false;
                }
            }
        }
    }
}