using Common.InputSupport;
using MFDExtractor.Configuration;
using MFDExtractor.EventSystem.Handlers;

namespace MFDExtractor.EventSystem
{
	internal interface IMediatorStateChangeHandler
	{
		void HandleStateChange(object sender, PhysicalControlStateChangedEventArgs e);
	}

	class MediatorStateChangeHandler : IMediatorStateChangeHandler
	{
		private readonly IDirectInputEventHotkeyFilter _directInputEventHotkeyFilter;
		private readonly IDIHotkeyDetection _diHotkeyDetection;
		private readonly IEHSIStateTracker _ehsiStateTracker;
		private readonly KeySettings _keySettings;
		private readonly IInputEventHandler 
			_nightVisionModeIsToggled,
			_airspeedIndexDecreasedByOne,_airspeedIndexIncreasedByOne,
			_ehsiLeftKnobDecreasedByOne,_ehsiLeftKnobIncreasedByOne,
			_ehsiRightKnobDecreasedByOne,_ehsiRightKnobIncreasedByOne,
			_ehsiRightKnobDepressed,_ehsiRightKnobReleased,
			_ehsiMenuButtonDepressed, 
			_isisBrightButtonDepressed, _isisStandardButtonDepressed,
			_azimuthIndicatorBrightnessIncreased, _azimuthIndicatorBrightnessDecreased,
			_accelerometerIsReset;
		
		public MediatorStateChangeHandler(
			KeySettings keySettings,
			IDirectInputEventHotkeyFilter directInputEventHotkeyFilter,
			IDIHotkeyDetection diHotkeyDetection,
			IEHSIStateTracker ehsiStateTracker,
			IInputEventHandler nightVisionModeIsToggled, 
			IInputEventHandler airspeedIndexDecreasedByOne,IInputEventHandler airspeedIndexIncreasedByOne,
			IInputEventHandler ehsiLeftKnobDecreasedByOne, IInputEventHandler ehsiLeftKnobIncreasedByOne, 
			IInputEventHandler ehsiRightKnobDecreasedByOne, IInputEventHandler ehsiRightKnobIncreasedByOne, 
			IInputEventHandler ehsiRightKnobDepressed, IInputEventHandler ehsiRightKnobReleased,
			IInputEventHandler ehsiMenuButtonDepressed, 
			IInputEventHandler isisBrightButtonDepressed, IInputEventHandler isisStandardButtonDepressed, 
			IInputEventHandler azimuthIndicatorBrightnessIncreased, IInputEventHandler azimuthIndicatorBrightnessDecreased, 
			IInputEventHandler accelerometerIsReset)
		{
			_keySettings = keySettings;
			_directInputEventHotkeyFilter = directInputEventHotkeyFilter;
			_diHotkeyDetection = diHotkeyDetection;
			_nightVisionModeIsToggled = nightVisionModeIsToggled;
			_airspeedIndexIncreasedByOne = airspeedIndexIncreasedByOne;
			_airspeedIndexDecreasedByOne = airspeedIndexDecreasedByOne;
			_ehsiLeftKnobDecreasedByOne = ehsiLeftKnobDecreasedByOne;
			_ehsiLeftKnobIncreasedByOne = ehsiLeftKnobIncreasedByOne;
			_ehsiRightKnobDecreasedByOne = ehsiRightKnobDecreasedByOne;
			_ehsiRightKnobIncreasedByOne = ehsiRightKnobIncreasedByOne;
			_ehsiRightKnobDepressed = ehsiRightKnobDepressed;
			_ehsiRightKnobReleased = ehsiRightKnobReleased;
			_ehsiStateTracker = ehsiStateTracker;
			_ehsiMenuButtonDepressed = ehsiMenuButtonDepressed;
			_isisBrightButtonDepressed = isisBrightButtonDepressed;
			_isisStandardButtonDepressed = isisStandardButtonDepressed;
			_azimuthIndicatorBrightnessIncreased = azimuthIndicatorBrightnessIncreased;
			_azimuthIndicatorBrightnessDecreased = azimuthIndicatorBrightnessDecreased;
			_accelerometerIsReset = accelerometerIsReset;
		}
		public void HandleStateChange(object sender, PhysicalControlStateChangedEventArgs e)
		{
			if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.NVISKey))
			{
				_nightVisionModeIsToggled.Raise();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.AirspeedIndexIncreaseKey))
			{
				_airspeedIndexIncreasedByOne.Raise();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.AirspeedIndexDecreaseKey))
			{
				_airspeedIndexDecreasedByOne.Raise();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.EHSIHeadingDecreaseKey))
			{
				_ehsiLeftKnobDecreasedByOne.Raise();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.EHSIHeadingIncreaseKey))
			{
				_ehsiLeftKnobIncreasedByOne.Raise();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.EHSICourseDecreaseKey))
			{
				_ehsiRightKnobDecreasedByOne.Raise();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.EHSICourseIncreaseKey))
			{
				_ehsiRightKnobIncreasedByOne.Raise();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.EHSICourseDepressedKey))
			{
				_ehsiRightKnobDepressed.Raise();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.EHSIMenuButtonDepressedKey))
			{
				_ehsiMenuButtonDepressed.Raise();
			}
			else if (
				!_diHotkeyDetection.DirectInputHotkeyIsTriggering(_keySettings.EHSICourseDepressedKey)
				&&
				_ehsiStateTracker.RightKnobIsPressed
				)
			{
				_ehsiRightKnobReleased.Raise();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.ISISBrightButtonKey))
			{
				_isisBrightButtonDepressed.Raise();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.ISISStandardButtonKey))
			{
				_isisStandardButtonDepressed.Raise();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.AzimuthIndicatorBrightnessIncreaseKey))
			{
				_azimuthIndicatorBrightnessIncreased.Raise();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.AzimuthIndicatorBrightnessDecreaseKey))
			{
				_azimuthIndicatorBrightnessDecreased.Raise();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.AccelerometerResetKey))
			{
				_accelerometerIsReset.Raise();
			}
		}
	}
}
