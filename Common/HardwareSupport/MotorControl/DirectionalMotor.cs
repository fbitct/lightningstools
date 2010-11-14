using System;
using Common.MacroProgramming;

namespace Common.HardwareSupport.MotorControl
{
    [Serializable]
    public class DirectionalMotor:MotorControlBase
    {
        private AnalogSignal _speedAndDirection = null;
        public DirectionalMotor()
            : base()
        {
            this.SpeedAndDirection = new AnalogSignal();
        }
        public AnalogSignal SpeedAndDirection
        {
            get
            {
                return _speedAndDirection;
            }
            set
            {
                _speedAndDirection = value;
                if (_speedAndDirection != null)
                {
                    _speedAndDirection.SignalChanged += new AnalogSignal.AnalogSignalChangedEventHandler(OnSpeedAndDirectionSignalChanged);
                }
            }
        }

        protected virtual void OnSpeedAndDirectionSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            this.PhysicalOutput.State = args.CurrentState;
        }
    }
}
