using System;
using System.Collections.Generic;

namespace F16CPD.Mfd.Controls
{
    public class ToggleSwitchPositionChangedEventArgs : EventArgs
    {
        protected ToggleSwitchPositionMfdInputControl _newPosition;
        protected ToggleSwitchPositionMfdInputControl _previousPosition;

        public ToggleSwitchPositionChangedEventArgs()
        {
        }

        public ToggleSwitchPositionChangedEventArgs(ToggleSwitchPositionMfdInputControl previousPosition,
                                                    ToggleSwitchPositionMfdInputControl newPosition)
        {
            _previousPosition = previousPosition;
            _newPosition = newPosition;
        }

        public ToggleSwitchPositionMfdInputControl PreviousPosition
        {
            get { return _previousPosition; }
            set { _previousPosition = value; }
        }

        public ToggleSwitchPositionMfdInputControl NewPosition
        {
            get { return _newPosition; }
            set { _newPosition = value; }
        }
    }

    public class ToggleSwitchMfdInputControl : MfdInputControl
    {
        protected int _curPosition = -1;
        protected List<ToggleSwitchPositionMfdInputControl> _positions = new List<ToggleSwitchPositionMfdInputControl>();
        protected int _prevPosition = -1;

        public List<ToggleSwitchPositionMfdInputControl> Positions
        {
            get { return _positions; }
            set { _positions = value; }
        }

        public int CurrentPositionIndex
        {
            get { return _curPosition; }
            set
            {
                int curPosition = _curPosition;
                if (value < 0) throw new ArgumentOutOfRangeException();
                if (value <= _positions.Count - 1)
                {
                    _curPosition = value;
                }
                else
                {
                    _curPosition = 0;
                }
                if (curPosition != _curPosition)
                {
                    _prevPosition = curPosition;
                }
                OnPositionChanged();
            }
        }

        public ToggleSwitchPositionMfdInputControl CurrentPosition
        {
            get { return _positions[_curPosition]; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                int curPosition = _curPosition;
                int i = 0;
                foreach (ToggleSwitchPositionMfdInputControl position in _positions)
                {
                    if (position.PositionName == value.PositionName)
                    {
                        _curPosition = i;
                    }
                    i++;
                }
                if (curPosition != _curPosition)
                {
                    _prevPosition = curPosition;
                }
                OnPositionChanged();
            }
        }

        public event EventHandler<ToggleSwitchPositionChangedEventArgs> PositionChanged;

        public int AddPosition(string positionName)
        {
            _positions.Add(new ToggleSwitchPositionMfdInputControl(positionName, this));
            return _positions.Count - 1;
        }

        public ToggleSwitchPositionMfdInputControl GetPositionByName(string positionName)
        {
            ToggleSwitchPositionMfdInputControl toReturn = null;
            foreach (ToggleSwitchPositionMfdInputControl position in _positions)
            {
                if (position.PositionName == positionName)
                {
                    toReturn = position;
                    break;
                }
            }
            return toReturn;
        }

        public void Toggle()
        {
            int curPosition = _curPosition;
            _curPosition++;
            if (_curPosition > _positions.Count - 1)
            {
                _curPosition = 0;
            }
            if (curPosition != _curPosition)
            {
                OnPositionChanged();
            }
        }

        protected virtual void OnPositionChanged()
        {
            if (PositionChanged != null)
            {
                ToggleSwitchPositionMfdInputControl prevPosition = null;
                ToggleSwitchPositionMfdInputControl curPosition = null;
                if (_prevPosition >= 0)
                {
                    prevPosition = Positions[_prevPosition];
                }
                if (_curPosition >= 0)
                {
                    curPosition = Positions[_curPosition];
                }
                PositionChanged(this, new ToggleSwitchPositionChangedEventArgs(prevPosition, curPosition));
            }
        }
    }
}