

using LightningGauges.Renderers.F16;
using LightningGauges.Renderers.F16.AzimuthIndicator;
using LightningGauges.Renderers.F16.EHSI;
using LightningGauges.Renderers.F16.ISIS;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IInputEvents
	{
		INightVisionModeToggledEventHandler NightVisionModeToggled { get;  }
		IAirspeedIndexDecreasedByOneEventHandler AirspeedIndexDecreasedByOne { get;  }
		IAirspeedIndexIncreasedByOneEventHandler AirspeedIndexIncreasedByOne { get;  }
		IEHSILeftKnobDecreasedByOneEventHandler EHSILeftKnobDecreasedByOne { get;  }
		IEHSILeftKnobIncreasedByOneEventHandler EHSILeftKnobIncreasedByOne { get;  }
		IEHSIRightKnobDecreasedByOneEventHandler EHSIRightKnobDecreasedByOne { get;  }
		IEHSIRightKnobIncreasedByOneEventHandler EHSIRightKnobIncreasedByOne { get;  }
		IEHSIRightKnobDepressedEventHandler EHSIRightKnobDepressed { get;  }
		IEHSIRightKnobReleasedEventHandler EHSIRightKnobReleased { get;  }
		IEHSIMenuButtonDepressedEventHandler EHSIMenuButtonDepressed { get;  }
		IISISBrightButtonDepressedEventHandler ISISBrightButtonDepressed { get;  }
		IISISStandardButtonDepressedEventHandler ISISStandardButtonDepressed { get;  }
		IAzimuthIndicatorBrightnessIncreasedEventHandler AzimuthIndicatorBrightnessIncreased { get;  }
		IAzimuthIndicatorBrightnessDecreasedEventHandler AzimuthIndicatorBrightnessDecreased { get;  }
		IAccelerometerResetEventHandler AccelerometerReset { get;  }

	}
	public class InputEvents : IInputEvents
	{
			public INightVisionModeToggledEventHandler NightVisionModeToggled { get; private set; }
			public IAirspeedIndexDecreasedByOneEventHandler AirspeedIndexDecreasedByOne { get; private set; }
			public IAirspeedIndexIncreasedByOneEventHandler AirspeedIndexIncreasedByOne { get; private set; }
			public IEHSILeftKnobDecreasedByOneEventHandler EHSILeftKnobDecreasedByOne { get; private set; }
			public IEHSILeftKnobIncreasedByOneEventHandler EHSILeftKnobIncreasedByOne { get; private set; }
			public IEHSIRightKnobDecreasedByOneEventHandler EHSIRightKnobDecreasedByOne { get; private set; }
			public IEHSIRightKnobIncreasedByOneEventHandler EHSIRightKnobIncreasedByOne { get; private set; }
			public IEHSIRightKnobDepressedEventHandler EHSIRightKnobDepressed { get; private set; }
			public IEHSIRightKnobReleasedEventHandler EHSIRightKnobReleased { get; private set; }
			public IEHSIMenuButtonDepressedEventHandler EHSIMenuButtonDepressed { get; private set; }
			public IISISBrightButtonDepressedEventHandler ISISBrightButtonDepressed { get; private set; }
			public IISISStandardButtonDepressedEventHandler ISISStandardButtonDepressed { get; private set; }
			public IAzimuthIndicatorBrightnessIncreasedEventHandler AzimuthIndicatorBrightnessIncreased { get; private set; }
			public IAzimuthIndicatorBrightnessDecreasedEventHandler AzimuthIndicatorBrightnessDecreased { get; private set; }
			public IAccelerometerResetEventHandler AccelerometerReset { get; private set; }

		public InputEvents(
            IAirspeedIndicator asi, 
            IEHSI ehsi, 
            IISIS isis, 
            IAzimuthIndicator rwr, 
            IAccelerometer accelerometer, 
            IEHSIStateTracker ehsiStateTracker,
            ExtractorState extractorState)
		{
			NightVisionModeToggled =  new NightVisionModeToggledEventHandler(extractorState);
			AirspeedIndexIncreasedByOne = new AirspeedIndexIncreasedByOneEventHandler(asi);
			AirspeedIndexDecreasedByOne = new AirspeedIndexDecreasedByOneEventHandler(asi);
			EHSILeftKnobDecreasedByOne = new EHSILeftKnobDecreasedByOneEventHandler();
			EHSILeftKnobIncreasedByOne = new EHSILeftKnobIncreasedByOneEventHandler();
			EHSIRightKnobDecreasedByOne = new EHSIRightKnobDecreasedByOneEventHandler(ehsiStateTracker,ehsi);
			EHSIRightKnobIncreasedByOne = new EHSIRightKnobIncreasedByOneEventHandler(ehsiStateTracker, ehsi);
			EHSIRightKnobDepressed = new EHSIRightKnobDepressedEventHandler(ehsiStateTracker);
			EHSIRightKnobReleased = new EHSIRightKnobReleasedEventHandler(ehsiStateTracker);
			EHSIMenuButtonDepressed = new EHSIMenuButtonDepressedEventHandler(ehsi);
			ISISBrightButtonDepressed = new ISISBrightButtonDepressedEventHandler(isis);
			ISISStandardButtonDepressed = new ISISStandardButtonDepressedEventHandler(isis);
			AzimuthIndicatorBrightnessIncreased = new AzimuthIndicatorBrightnessIncreasedEventHandler(rwr);
			AzimuthIndicatorBrightnessDecreased = new AzimuthIndicatorBrightnessDecreasedEventHandler(rwr);
			AccelerometerReset = new AccelerometerResetEventHandler(accelerometer);
		}
	}
}
