using System;

namespace F16CPD.Mfd.Controls
{
    public class RotaryEncoderPositionChangedEventArgs : EventArgs
    {
        protected RotationDirection _rotationDirection = RotationDirection.Empty;

        public RotaryEncoderPositionChangedEventArgs(RotationDirection direction)
        {
            _rotationDirection = direction;
        }
    }

    public class RotaryEncoderMfdInputControl : MfdInputControl
    {
        protected MomentaryButtonMfdInputControl _clockwiseMomentary;
        protected MomentaryButtonMfdInputControl _counterClockwiseMomentary;

        public MomentaryButtonMfdInputControl ClockwiseMomentaryInputControl
        {
            get { return _clockwiseMomentary; }
        }

        public MomentaryButtonMfdInputControl CounterclockwiseMomentaryInputControl
        {
            get { return _counterClockwiseMomentary; }
        }

        public event EventHandler<RotaryEncoderPositionChangedEventArgs> Rotated;

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