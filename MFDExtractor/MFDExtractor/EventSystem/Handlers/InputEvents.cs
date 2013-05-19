

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

		public InputEvents(IInstrumentRendererSet renderers, IEHSIStateTracker ehsiStateTracker, ExtractorState extractorState)
		{
			NightVisionModeToggled =  new NightVisionModeToggledEventHandler(extractorState);
			AirspeedIndexIncreasedByOne = new AirspeedIndexIncreasedByOneEventHandler(renderers.ASI);
			AirspeedIndexDecreasedByOne = new AirspeedIndexDecreasedByOneEventHandler(renderers.ASI);
			EHSILeftKnobDecreasedByOne = new EHSILeftKnobDecreasedByOneEventHandler();
			EHSILeftKnobIncreasedByOne = new EHSILeftKnobIncreasedByOneEventHandler();
			EHSIRightKnobDecreasedByOne = new EHSIRightKnobDecreasedByOneEventHandler(renderers.EHSI);
			EHSIRightKnobIncreasedByOne = new EHSIRightKnobIncreasedByOneEventHandler(ehsiStateTracker, renderers.EHSI);
			EHSIRightKnobDepressed = new EHSIRightKnobDepressedEventHandler(ehsiStateTracker);
			EHSIRightKnobReleased = new EHSIRightKnobReleasedEventHandler(ehsiStateTracker);
			EHSIMenuButtonDepressed = new EHSIMenuButtonDepressedEventHandler(renderers.EHSI);
			ISISBrightButtonDepressed = new ISISBrightButtonDepressedEventHandler(renderers.ISIS);
			ISISStandardButtonDepressed = new ISISStandardButtonDepressedEventHandler(renderers.ISIS);
			AzimuthIndicatorBrightnessIncreased = new AzimuthIndicatorBrightnessIncreasedEventHandler(renderers.RWR);
			AzimuthIndicatorBrightnessDecreased = new AzimuthIndicatorBrightnessDecreasedEventHandler(renderers.RWR);
			AccelerometerReset = new AccelerometerResetEventHandler(renderers.Accelerometer);
		}
	}
}
