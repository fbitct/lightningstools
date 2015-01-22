﻿using System.Windows.Forms;
using MFDExtractor.Configuration;
using MFDExtractor.EventSystem.Handlers;

namespace MFDExtractor.EventSystem
{
	internal interface IKeyEventHandler
	{
		void ProcessKeyDownEvent(KeyEventArgs e);
	    void ProcessKeyUpEvent(KeyEventArgs e);
	}

	class KeyEventHandler : IKeyEventHandler
	{
		private readonly IKeyEventArgsAugmenter _keyEventArgsAugmenter;
		private readonly IInputEvents _inputEvents;
	    private readonly IKeyInputEventHotkeyFilter _keyInputEventHotkeyFilter;
	    private readonly IKeySettingsReader _keySettingsReader;
		public KeyEventHandler(
			IInputEvents inputEvents,
            IKeyInputEventHotkeyFilter keyInputEventHotkeyFilter = null,
			IKeyEventArgsAugmenter keyEventArgsAugmenter = null,
            IKeySettingsReader keySettingsReader=null
			)
		{
			_inputEvents = inputEvents;
		    _keyInputEventHotkeyFilter = keyInputEventHotkeyFilter ?? new KeyInputEventHotkeyFilter();
			_keyEventArgsAugmenter = keyEventArgsAugmenter ?? new KeyEventArgsAugmenter();
		    _keySettingsReader = keySettingsReader ?? new KeySettingsReader();
		}



		public void ProcessKeyDownEvent(KeyEventArgs e)
		{
		    var keySettings = _keySettingsReader.Read();
			var keys = _keyEventArgsAugmenter.UpdateKeyEventArgsWithExtendedKeyInfo(e.KeyData);

            if (_keyInputEventHotkeyFilter.CheckIfMatches(keySettings.NVISKey, keys))
			{
				_inputEvents.NightVisionModeToggled.Handle();
			}
            else if (_keyInputEventHotkeyFilter.CheckIfMatches(keySettings.AirspeedIndexIncreaseKey, keys))
			{
				_inputEvents.AirspeedIndexIncreasedByOne.Handle();
			}
            else if (_keyInputEventHotkeyFilter.CheckIfMatches(keySettings.AirspeedIndexDecreaseKey, keys))
			{
				_inputEvents.AirspeedIndexDecreasedByOne.Handle();
			}
            else if (_keyInputEventHotkeyFilter.CheckIfMatches(keySettings.EHSIHeadingDecreaseKey, keys))
			{
				_inputEvents.EHSILeftKnobDecreasedByOne.Handle();
			}
            else if (_keyInputEventHotkeyFilter.CheckIfMatches(keySettings.EHSIHeadingIncreaseKey, keys))
			{
				_inputEvents.EHSILeftKnobIncreasedByOne.Handle();
			}
            else if (_keyInputEventHotkeyFilter.CheckIfMatches(keySettings.EHSICourseDecreaseKey, keys))
			{
				_inputEvents.EHSIRightKnobDecreasedByOne.Handle();
			}
            else if (_keyInputEventHotkeyFilter.CheckIfMatches(keySettings.EHSICourseIncreaseKey, keys))
			{
				_inputEvents.EHSIRightKnobIncreasedByOne.Handle();
			}
            else if (_keyInputEventHotkeyFilter.CheckIfMatches(keySettings.EHSICourseDepressedKey, keys))
			{
				_inputEvents.EHSIRightKnobDepressed.Handle();
			}
            else if (_keyInputEventHotkeyFilter.CheckIfMatches(keySettings.EHSIMenuButtonDepressedKey, keys))
			{
				_inputEvents.EHSIMenuButtonDepressed.Handle();
			}
            else if (_keyInputEventHotkeyFilter.CheckIfMatches(keySettings.ISISBrightButtonKey, keys))
			{
				_inputEvents.ISISBrightButtonDepressed.Handle();
			}
            else if (_keyInputEventHotkeyFilter.CheckIfMatches(keySettings.ISISStandardButtonKey, keys))
			{
				_inputEvents.ISISStandardButtonDepressed.Handle();
			}
            else if (_keyInputEventHotkeyFilter.CheckIfMatches(keySettings.AzimuthIndicatorBrightnessIncreaseKey, keys))
			{
				_inputEvents.AzimuthIndicatorBrightnessIncreased.Handle();
			}
            else if (_keyInputEventHotkeyFilter.CheckIfMatches(keySettings.AzimuthIndicatorBrightnessDecreaseKey, keys))
			{
				_inputEvents.AzimuthIndicatorBrightnessDecreased.Handle();
			}
            else if (_keyInputEventHotkeyFilter.CheckIfMatches(keySettings.AccelerometerResetKey, keys))
			{
				_inputEvents.AccelerometerReset.Handle();
			}
		}
        public void ProcessKeyUpEvent(KeyEventArgs e)
        {
            var keySettings = _keySettingsReader.Read();

            var keys = _keyEventArgsAugmenter.UpdateKeyEventArgsWithExtendedKeyInfo(e.KeyData);

            if (_keyInputEventHotkeyFilter.CheckIfMatches(keySettings.EHSICourseDepressedKey, keys))
            {
                _inputEvents.EHSIRightKnobReleased.Handle();
            }
        }
	}
}
