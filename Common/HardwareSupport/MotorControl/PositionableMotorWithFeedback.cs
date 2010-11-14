using System;
using Common.MacroProgramming;

namespace Common.HardwareSupport.MotorControl
{
    [Serializable]
    public class PositionableMotorWithFeedback:PositionableMotor
    {
        private DigitalSignal _minPositionReachedSignal = null;
        private DigitalSignal _maxPositionReachedSignal = null;
        public PositionableMotorWithFeedback()
            : base()
        {
            this.MinPositionReachedSignal = new DigitalSignal();
            this.MaxPositionReachedSignal = new DigitalSignal();
        }
        public DigitalSignal MinPositionReachedSignal
        {
            get
            {
                return _minPositionReachedSignal;
            }
            set
            {
                _minPositionReachedSignal = value;
            }
        }

        public DigitalSignal MaxPositionReachedSignal
        {
            get
            {
                return _maxPositionReachedSignal;
            }
            set
            {
                _maxPositionReachedSignal = value;
            }
        }
        protected override void OnGoToPositionSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            if (_minPositionReachedSignal != null && _minPositionReachedSignal.State == true && args.CurrentState <=args.PreviousState)
            {
                return;
            }
            else if (_maxPositionReachedSignal != null && _maxPositionReachedSignal.State == true && args.CurrentState >= args.PreviousState)
            {
                return;
            }
            base.OnGoToPositionSignalChanged(sender, args);
        }
    }
}
