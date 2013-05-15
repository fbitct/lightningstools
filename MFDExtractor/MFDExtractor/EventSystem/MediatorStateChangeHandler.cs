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
		private readonly IInputEvents _inputEvents;
		
		public MediatorStateChangeHandler(
			KeySettings keySettings,
			IDirectInputEventHotkeyFilter directInputEventHotkeyFilter,
			IDIHotkeyDetection diHotkeyDetection,
			IEHSIStateTracker ehsiStateTracker,
			IInputEvents inputEvents)
		{
			_keySettings = keySettings;
			_directInputEventHotkeyFilter = directInputEventHotkeyFilter;
			_diHotkeyDetection = diHotkeyDetection;
			_ehsiStateTracker = ehsiStateTracker;
			_inputEvents = inputEvents;
		}
		public void HandleStateChange(object sender, PhysicalControlStateChangedEventArgs e)
		{
			if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.NVISKey))
			{
				_inputEvents.NightVisionModeToggled.Handle();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.AirspeedIndexIncreaseKey))
			{
				_inputEvents.AirspeedIndexIncreasedByOne.Handle();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.AirspeedIndexDecreaseKey))
			{
				_inputEvents.AirspeedIndexDecreasedByOne.Handle();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.EHSIHeadingDecreaseKey))
			{
				_inputEvents.EHSILeftKnobDecreasedByOne.Handle();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.EHSIHeadingIncreaseKey))
			{
				_inputEvents.EHSILeftKnobIncreasedByOne.Handle();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.EHSICourseDecreaseKey))
			{
				_inputEvents.EHSIRightKnobDecreasedByOne.Handle();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.EHSICourseIncreaseKey))
			{
				_inputEvents.EHSIRightKnobIncreasedByOne.Handle();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.EHSICourseDepressedKey))
			{
				_inputEvents.EHSIRightKnobDepressed.Handle();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.EHSIMenuButtonDepressedKey))
			{
				_inputEvents.EHSIMenuButtonDepressed.Handle();
			}
			else if (
				!_diHotkeyDetection.DirectInputHotkeyIsTriggering(_keySettings.EHSICourseDepressedKey)
				&&
				_ehsiStateTracker.RightKnobIsPressed
				)
			{
				_inputEvents.EHSIRightKnobReleased.Handle();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.ISISBrightButtonKey))
			{
				_inputEvents.ISISBrightButtonDepressed.Handle();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.ISISStandardButtonKey))
			{
				_inputEvents.ISISStandardButtonDepressed.Handle();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.AzimuthIndicatorBrightnessIncreaseKey))
			{
				_inputEvents.AzimuthIndicatorBrightnessIncreased.Handle();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.AzimuthIndicatorBrightnessDecreaseKey))
			{
				_inputEvents.AzimuthIndicatorBrightnessDecreased.Handle();
			}
			else if (_directInputEventHotkeyFilter.CheckIfMatches(e, _keySettings.AccelerometerResetKey))
			{
				_inputEvents.AccelerometerReset.Handle();
			}
		}
	}
}
