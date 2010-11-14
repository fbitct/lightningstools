using Common.MacroProgramming;
namespace Common.HardwareSupport.MotorControl
{
    public static class MotorControlFactory
    {
        public static StepperMotor CreateStepperMotorControlWithPhysicalBoundary(
            AnalogSignal goToPositionSignal, 
            int numStepsInTravelRange, 
            AnalogSignal outputLine
        )
        {
            StepperMotor stepper = new StepperMotor();
            stepper.GoToPositionSignal = goToPositionSignal;
            stepper.NumStepsInRangeOfTravel = numStepsInTravelRange;
            stepper.PhysicalOutput = outputLine;
            return stepper;
        }

        public static StepperMotor CreateStepperMotorControlWithBoundaryPositionSensing
        (
            AnalogSignal goToPositionSignal,
            int numStepsInTravelRange,
            DigitalSignal minPositionReachedSignal,
            DigitalSignal maxPositionReachedSignal,
            AnalogSignal outputLine
        )
        {
            StepperMotor stepper = new StepperMotor();
            stepper.GoToPositionSignal = goToPositionSignal;
            stepper.NumStepsInRangeOfTravel = numStepsInTravelRange;
            stepper.MinPositionReachedSignal = minPositionReachedSignal;
            stepper.MaxPositionReachedSignal = maxPositionReachedSignal;
            stepper.PhysicalOutput = outputLine;
            return stepper;
        }

        public static StepperMotor CreateStepperMotorControlWithAbsolutePositionSensing
        (
            AnalogSignal goToPositionSignal,
            AnalogSignal absolutePositionSensingSignal,
            Range minPositionRange,
            Range maxPositionRange,
            int numStepsInTravelRange,
            AnalogSignal outputLine
        )
        {
            StepperMotor stepper = new StepperMotor();
            stepper.GoToPositionSignal = goToPositionSignal;
            stepper.NumStepsInRangeOfTravel = numStepsInTravelRange;
            RangeEvaluator minPositionReachedRE = new RangeEvaluator();
            minPositionReachedRE.Range = minPositionRange;
            minPositionReachedRE.In = goToPositionSignal;
            stepper.MinPositionReachedSignal = minPositionReachedRE.Out;
            RangeEvaluator maxPositionReachedRE = new RangeEvaluator();
            maxPositionReachedRE.In = goToPositionSignal;
            stepper.MaxPositionReachedSignal = maxPositionReachedRE.Out;
            stepper.PhysicalOutput = outputLine;
            return stepper;
        }

        public static PositionableMotorWithFeedback CreateServoMechanism
        (
            AnalogSignal goToPositionSignal,
            AnalogSignal absolutePositionSensingSignal,
            Range minPositionRange,
            Range maxPositionRange,
            AnalogSignal outputLine
        )
        {
            PositionableMotorWithFeedback motor = new PositionableMotorWithFeedback();
            motor.GoToPositionSignal = goToPositionSignal;
            RangeEvaluator minPositionReachedRE = new RangeEvaluator();
            minPositionReachedRE.Range = minPositionRange;
            minPositionReachedRE.In = goToPositionSignal;
            motor.MinPositionReachedSignal = minPositionReachedRE.Out;
            RangeEvaluator maxPositionReachedRE = new RangeEvaluator();
            maxPositionReachedRE.In = goToPositionSignal;
            motor.MaxPositionReachedSignal = maxPositionReachedRE.Out;
            motor.PhysicalOutput = outputLine;
            return motor;
        }
        public static DirectionalMotor CreateDirectionalMotor
        (
            AnalogSignal speedAndDirection, 
            AnalogSignal outputLine
        )
        {
            DirectionalMotor motor = new DirectionalMotor();
            motor.SpeedAndDirection = speedAndDirection;
            motor.PhysicalOutput =outputLine;
            return motor;
        }
        public static PositionableMotor CreatePositionableMotor
        (
            AnalogSignal goToPositionSignal,
            AnalogSignal outputLine
        )
        {
            PositionableMotor toReturn = new PositionableMotor();
            toReturn.GoToPositionSignal = goToPositionSignal;
            toReturn.PhysicalOutput = outputLine;
            return toReturn;
        }
    }
}
