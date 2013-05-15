using System.Windows.Forms;
using Common.InputSupport;
using Common.InputSupport.UI;
using MFDExtractor.Configuration;
using MFDExtractor.EventSystem.Handlers;

namespace MFDExtractor.EventSystem
{
	internal interface IKeyDownEventHandler
	{
		void ProcessKeyDownEvent(KeyEventArgs e);
	}

	class KeyDownEventHandler : IKeyDownEventHandler
	{
		private readonly IKeyEventArgsAugmenter _keyEventArgsAugmenter;
		private readonly KeySettings _keySettings;
		private readonly IInputEventHandler
			_nightVisionModeIsToggled,
			_airspeedIndexDecreasedByOne, _airspeedIndexIncreasedByOne,
			_ehsiLeftKnobDecreasedByOne, _ehsiLeftKnobIncreasedByOne,
			_ehsiRightKnobDecreasedByOne, _ehsiRightKnobIncreasedByOne,
			_ehsiRightKnobDepressed, _ehsiRightKnobReleased,
			_ehsiMenuButtonDepressed,
			_isisBrightButtonDepressed, _isisStandardButtonDepressed,
			_azimuthIndicatorBrightnessIncreased, _azimuthIndicatorBrightnessDecreased,
			_accelerometerIsReset;

		private readonly IEHSIStateTracker _ehsiStateTracker;

		public KeyDownEventHandler(
			IEHSIStateTracker ehsiStateTracker,
			IInputEventHandler nightVisionModeIsToggled, 
			IInputEventHandler airspeedIndexDecreasedByOne, IInputEventHandler airspeedIndexIncreasedByOne, 
			IInputEventHandler ehsiLeftKnobDecreasedByOne, IInputEventHandler ehsiLeftKnobIncreasedByOne, 
			IInputEventHandler ehsiRightKnobDecreasedByOne, IInputEventHandler ehsiRightKnobIncreasedByOne, 
			IInputEventHandler ehsiRightKnobDepressed, IInputEventHandler ehsiRightKnobReleased, 
			IInputEventHandler ehsiMenuButtonDepressed, IInputEventHandler isisBrightButtonDepressed, 
			IInputEventHandler isisStandardButtonDepressed, 
			IInputEventHandler azimuthIndicatorBrightnessIncreased, IInputEventHandler azimuthIndicatorBrightnessDecreased, 
			IInputEventHandler accelerometerIsReset,
			KeySettings keySettings,
			IKeyEventArgsAugmenter keyEventArgsAugmenter = null
			)
		{
			_ehsiStateTracker = ehsiStateTracker;
			_nightVisionModeIsToggled = nightVisionModeIsToggled;
			_airspeedIndexDecreasedByOne = airspeedIndexDecreasedByOne;
			_airspeedIndexIncreasedByOne = airspeedIndexIncreasedByOne;
			_ehsiLeftKnobDecreasedByOne = ehsiLeftKnobDecreasedByOne;
			_ehsiLeftKnobIncreasedByOne = ehsiLeftKnobIncreasedByOne;
			_ehsiRightKnobDecreasedByOne = ehsiRightKnobDecreasedByOne;
			_ehsiRightKnobIncreasedByOne = ehsiRightKnobIncreasedByOne;
			_ehsiRightKnobDepressed = ehsiRightKnobDepressed;
			_ehsiRightKnobReleased = ehsiRightKnobReleased;
			_ehsiMenuButtonDepressed = ehsiMenuButtonDepressed;
			_isisBrightButtonDepressed = isisBrightButtonDepressed;
			_isisStandardButtonDepressed = isisStandardButtonDepressed;
			_azimuthIndicatorBrightnessIncreased = azimuthIndicatorBrightnessIncreased;
			_azimuthIndicatorBrightnessDecreased = azimuthIndicatorBrightnessDecreased;
			_accelerometerIsReset = accelerometerIsReset;
			_keySettings = keySettings;
			_keyEventArgsAugmenter = keyEventArgsAugmenter ?? new KeyEventArgsAugmenter();
		}

		private bool KeyIsHotkey(InputControlSelection hotkey, Keys keyPressed)
		{
			if (hotkey == null) return false;
			if (hotkey.ControlType == ControlType.Key)
			{
				if (
					(hotkey.Keys & Keys.KeyCode) == (keyPressed & Keys.KeyCode)
					&&
					(hotkey.Keys & Keys.Modifiers) == (keyPressed & Keys.Modifiers)
					)
				{
					return true;
				}
			}
			return false;
		}

		public void ProcessKeyDownEvent(KeyEventArgs e)
		{

			var keys = _keyEventArgsAugmenter.UpdateKeyEventArgsWithExtendedKeyInfo(e.KeyData);

			if (KeyIsHotkey(_keySettings.NVISKey, keys))
			{
				_nightVisionModeIsToggled.Raise();
			}
			else if (KeyIsHotkey(_keySettings.AirspeedIndexIncreaseKey, keys))
			{
				_airspeedIndexIncreasedByOne.Raise();
			}
			else if (KeyIsHotkey(_keySettings.AirspeedIndexDecreaseKey, keys))
			{
				_airspeedIndexDecreasedByOne.Raise();
			}
			else if (KeyIsHotkey(_keySettings.EHSIHeadingDecreaseKey, keys))
			{
				_ehsiLeftKnobDecreasedByOne.Raise();
			}
			else if (KeyIsHotkey(_keySettings.EHSIHeadingIncreaseKey, keys))
			{
				_ehsiLeftKnobIncreasedByOne.Raise();
			}
			else if (KeyIsHotkey(_keySettings.EHSICourseDecreaseKey, keys))
			{
				_ehsiRightKnobDecreasedByOne.Raise();
			}
			else if (KeyIsHotkey(_keySettings.EHSICourseIncreaseKey, keys))
			{
				_ehsiRightKnobIncreasedByOne.Raise();
			}
			else if (KeyIsHotkey(_keySettings.EHSICourseDepressedKey, keys) && !_ehsiStateTracker.RightKnobIsPressed)
			{
				_ehsiRightKnobDepressed.Raise();
			}
			else if (KeyIsHotkey(_keySettings.EHSIMenuButtonDepressedKey, keys))
			{
				_ehsiMenuButtonDepressed.Raise();
			}
			else if (KeyIsHotkey(_keySettings.ISISBrightButtonKey, keys))
			{
				_isisBrightButtonDepressed.Raise();
			}
			else if (KeyIsHotkey(_keySettings.ISISStandardButtonKey, keys))
			{
				_isisStandardButtonDepressed.Raise();
			}
			else if (KeyIsHotkey(_keySettings.AzimuthIndicatorBrightnessIncreaseKey, keys))
			{
				_azimuthIndicatorBrightnessIncreased.Raise();
			}
			else if (KeyIsHotkey(_keySettings.AzimuthIndicatorBrightnessDecreaseKey, keys))
			{
				_azimuthIndicatorBrightnessDecreased.Raise();
			}
			else if (KeyIsHotkey(_keySettings.AccelerometerResetKey, keys))
			{
				_accelerometerIsReset.Raise();
			}
		}
	}
}
