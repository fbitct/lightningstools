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
		private readonly IEHSIStateTracker _ehsiStateTracker;
		private readonly IInputEvents _inputEvents;

		public KeyDownEventHandler(
			IEHSIStateTracker ehsiStateTracker,
			IInputEvents inputEvents,
			KeySettings keySettings,
			IKeyEventArgsAugmenter keyEventArgsAugmenter = null
			)
		{
			_ehsiStateTracker = ehsiStateTracker;
			_inputEvents = inputEvents;
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
				_inputEvents.NightVisionModeToggled.Handle();
			}
			else if (KeyIsHotkey(_keySettings.AirspeedIndexIncreaseKey, keys))
			{
				_inputEvents.AirspeedIndexIncreasedByOne.Handle();
			}
			else if (KeyIsHotkey(_keySettings.AirspeedIndexDecreaseKey, keys))
			{
				_inputEvents.AirspeedIndexDecreasedByOne.Handle();
			}
			else if (KeyIsHotkey(_keySettings.EHSIHeadingDecreaseKey, keys))
			{
				_inputEvents.EHSILeftKnobDecreasedByOne.Handle();
			}
			else if (KeyIsHotkey(_keySettings.EHSIHeadingIncreaseKey, keys))
			{
				_inputEvents.EHSILeftKnobIncreasedByOne.Handle();
			}
			else if (KeyIsHotkey(_keySettings.EHSICourseDecreaseKey, keys))
			{
				_inputEvents.EHSIRightKnobDecreasedByOne.Handle();
			}
			else if (KeyIsHotkey(_keySettings.EHSICourseIncreaseKey, keys))
			{
				_inputEvents.EHSIRightKnobIncreasedByOne.Handle();
			}
			else if (KeyIsHotkey(_keySettings.EHSICourseDepressedKey, keys) && !_ehsiStateTracker.RightKnobIsPressed)
			{
				_inputEvents.EHSIRightKnobDepressed.Handle();
			}
			else if (KeyIsHotkey(_keySettings.EHSIMenuButtonDepressedKey, keys))
			{
				_inputEvents.EHSIMenuButtonDepressed.Handle();
			}
			else if (KeyIsHotkey(_keySettings.ISISBrightButtonKey, keys))
			{
				_inputEvents.ISISBrightButtonDepressed.Handle();
			}
			else if (KeyIsHotkey(_keySettings.ISISStandardButtonKey, keys))
			{
				_inputEvents.ISISStandardButtonDepressed.Handle();
			}
			else if (KeyIsHotkey(_keySettings.AzimuthIndicatorBrightnessIncreaseKey, keys))
			{
				_inputEvents.AzimuthIndicatorBrightnessIncreased.Handle();
			}
			else if (KeyIsHotkey(_keySettings.AzimuthIndicatorBrightnessDecreaseKey, keys))
			{
				_inputEvents.AzimuthIndicatorBrightnessDecreased.Handle();
			}
			else if (KeyIsHotkey(_keySettings.AccelerometerResetKey, keys))
			{
				_inputEvents.AccelerometerReset.Handle();
			}
		}
	}
}
