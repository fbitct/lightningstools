using System;
using System.Collections.Generic;
using System.Text;

namespace F16CPD.Mfd.Controls
{
    
    public class RotaryEncoderPositionChangedEventArgs:EventArgs
    {
        protected RotationDirection _rotationDirection = RotationDirection.Empty;
        public RotaryEncoderPositionChangedEventArgs(RotationDirection direction)
            : base()
        {
            _rotationDirection = direction;
        }
    }
    public class RotaryEncoderMfdInputControl:MfdInputControl
    {
        public event EventHandler<RotaryEncoderPositionChangedEventArgs> Rotated;
        protected MomentaryButtonMfdInputControl _clockwiseMomentary = null;
        protected MomentaryButtonMfdInputControl _counterClockwiseMomentary = null;
        public RotaryEncoderMfdInputControl()
            : base()
        {
        }
        public MomentaryButtonMfdInputControl ClockwiseMomentaryInputControl
        {
            get
            {
                return _clockwiseMomentary; 
            }
        }
        public MomentaryButtonMfdInputControl CounterclockwiseMomentaryInputControl
        {
            get
            {
                return _counterClockwiseMomentary;
            }
        }
        public MomentaryButtonMfdInputControl GetMomentaryInputControl(RotationDirection direction)
        {
            MomentaryButtonMfdInputControl toReturn = null;
            switch (direction)
            {
                case RotationDirection.Empty:
                    break;
                case RotationDirection.Counterclockwise:
                    toReturn = _counterClockwiseMomentary;
                    break;
                case RotationDirection.Clockwise:
                    toReturn = _clockwiseMomentary;
                    break;
                default:
                    break;
            }
            return toReturn;
        }
        public void RotateClockwise()
        {
            OnRotated(RotationDirection.Clockwise);
        }
        public void RotateCounterclockwise()
        {
            OnRotated(RotationDirection.Counterclockwise);
        }
        protected virtual void OnRotated(RotationDirection direction)
        {
            if (Rotated != null)
            {
                Rotated(this, new RotaryEncoderPositionChangedEventArgs(direction));
            }
        }
    }
}
