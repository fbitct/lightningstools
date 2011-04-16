﻿using Common.MacroProgramming;

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
            var stepper = new StepperMotor();
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
            var stepper = new StepperMotor();
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
            var stepper = new StepperMotor();
            stepper.GoToPositionSignal = goToPositionSignal;
            stepper.NumStepsInRangeOfTravel = numStepsInTravelRange;
            var minPositionReachedRE = new RangeEvaluator();
            minPositionReachedRE.Range = minPositionRange;
            minPositionReachedRE.In = goToPositionSignal;
            stepper.MinPositionReachedSignal = minPositionReachedRE.Out;
            var maxPositionReachedRE = new RangeEvaluator();
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
            var motor = new PositionableMotorWithFeedback();
            motor.GoToPositionSignal = goToPositionSignal;
            var minPositionReachedRE = new RangeEvaluator();
            minPositionReachedRE.Range = minPositionRange;
            minPositionReachedRE.In = goToPositionSignal;
            motor.MinPositionReachedSignal = minPositionReachedRE.Out;
            var maxPositionReachedRE = new RangeEvaluator();
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
            var motor = new DirectionalMotor();
            motor.SpeedAndDirection = speedAndDirection;
            motor.PhysicalOutput = outputLine;
            return motor;
        }

        public static PositionableMotor CreatePositionableMotor
            (
            AnalogSignal goToPositionSignal,
            AnalogSignal outputLine
            )
        {
            var toReturn = new PositionableMotor();
            toReturn.GoToPositionSignal = goToPositionSignal;
            toReturn.PhysicalOutput = outputLine;
            return toReturn;
        }
    }
}