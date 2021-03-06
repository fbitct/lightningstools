using System;
using System.Collections.Generic;

namespace Common.MacroProgramming
{
    [Serializable]
    public enum ToggleDirection
    {
        Forward,
        Reverse
    }

    [Serializable]
    public sealed class Toggle : Chainable
    {
        private ToggleDirection _direction = ToggleDirection.Forward;
        private DigitalSignal _in;
        private List<DigitalSignal> _outs = new List<DigitalSignal>();
        private DigitalSignal _reset;
        private DigitalSignal _reverse;
        private int _toggleIndex;

        public Toggle()
        {
            _in = new DigitalSignal();
            _reset = new DigitalSignal();
            _reverse = new DigitalSignal();
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

        public DigitalSignal Reverse
        {
            get { return _reverse; }
            set
            {
                if (value == null)
                {
                    value = new DigitalSignal();
                }
                value.SignalChanged += ReverseSignalChanged;
                _reverse = value;
            }
        }

        public DigitalSignal Reset
        {
            get { return _reset; }
            set
            {
                if (value == null)
                {
                    value = new DigitalSignal();
                }
                value.SignalChanged += ResetSignalChanged;
                _reset = value;
            }
        }

        public List<DigitalSignal> Outs
        {
            get { return _outs; }
            set
            {
                if (value == null)
                {
                    value = new List<DigitalSignal>();
                }
                _outs = value;
            }
        }

        public ToggleDirection Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        public int ToggleIndex
        {
            get { return _toggleIndex; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (_outs == null)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value < _outs.Count)
                {
                    _toggleIndex = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("value");
                }
            }
        }

        public void ResetToggleIndex()
        {
            _toggleIndex = 0;
        }

        public void ReverseToggleDirection()
        {
            if (_direction == ToggleDirection.Forward)
            {
                _direction = ToggleDirection.Reverse;
            }
            else if (_direction == ToggleDirection.Reverse)
            {
                _direction = ToggleDirection.Forward;
            }
        }

        private void ReverseSignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                ReverseToggleDirection();
            }
        }

        private void ResetSignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                ResetToggleIndex();
            }
        }

        private void _in_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                if (_outs != null)
                {
                    if (_outs != null)
                    {
                        foreach (var t in _outs)
                        {
                            if (t != null)
                            {
                                t.State = false;
                            }
                        }
                        var currentOut = _outs[_toggleIndex];
                        switch (_direction)
                        {
                            case ToggleDirection.Forward:
                                _toggleIndex++;
                                if (_toggleIndex >= _outs.Count)
                                {
                                    _toggleIndex = 0;
                                }
                                break;
                            case ToggleDirection.Reverse:
                                _toggleIndex--;
                                if (_toggleIndex < 0)
                                {
                                    _toggleIndex = _outs.Count - 1;
                                }
                                break;
                        }
                        if (currentOut != null)
                        {
                            currentOut.State = true;
                        }
                    }
                }
            }
            else
            {
                if (_outs != null)
                {
                    foreach (var t in _outs)
                    {
                        if (t != null)
                        {
                            t.State = false;
                        }
                    }
                }
            }
        }
    }
}