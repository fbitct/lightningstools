using System;
using Common.MacroProgramming;

namespace Common.HardwareSupport.MotorControl
{
    [Serializable]
    public class PositionableMotor:MotorControlBase
    {
        private AnalogSignal _goToPositionSignal=null;
        public PositionableMotor():base()
        {
            this.GoToPositionSignal = new AnalogSignal();
        }
        public AnalogSignal GoToPositionSignal
        {
            get
            {
                return _goToPositionSignal;
            }
            set
            {
                _goToPositionSignal = value;
                if (_goToPositionSignal != null)
                {
                    if (this.PhysicalOutput != null)
                    {
                        this.PhysicalOutput.State = 1.0;
                    }
                    _goToPositionSignal.SignalChanged += new AnalogSignal.AnalogSignalChangedEventHandler(OnGoToPositionSignalChanged);
                }
            }
        }

        protected virtual void OnGoToPositionSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            this.PhysicalOutput.State = args.CurrentState;
        }

    }
}
