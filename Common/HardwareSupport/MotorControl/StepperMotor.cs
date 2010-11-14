using System;

namespace Common.HardwareSupport.MotorControl
{
    [Serializable]
    public class StepperMotor:PositionableMotorWithFeedback
    {
        public StepperMotor()
            : base()
        {
            this.NumStepsInRangeOfTravel = 0;

        }
        public int NumStepsInRangeOfTravel { get; set; }
        protected override void OnGoToPositionSignalChanged(object sender, Common.MacroProgramming.AnalogSignalChangedEventArgs args)
        {
            if (this.MinPositionReachedSignal != null && this.MinPositionReachedSignal.State == true && args.CurrentState <= args.PreviousState)
            {
                return;
            }
            else if (this.MaxPositionReachedSignal != null && this.MaxPositionReachedSignal.State == true && args.CurrentState >= args.PreviousState)
            {
                return;
            }
            double changeInValue = args.CurrentState - args.PreviousState;
            this.PhysicalOutput.State = changeInValue * this.NumStepsInRangeOfTravel;
            base.OnGoToPositionSignalChanged(sender, args);
        }
    }
}
