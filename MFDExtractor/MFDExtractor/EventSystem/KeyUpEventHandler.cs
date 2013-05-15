using System.Windows.Forms;
using Common.InputSupport;
using MFDExtractor.Configuration;
using MFDExtractor.EventSystem.Handlers;

namespace MFDExtractor.EventSystem
{
	internal interface IKeyUpEventHandler
	{
		void ProcessKeyUpEvent(KeyEventArgs e);
	}

	class KeyUpEventHandler : IKeyUpEventHandler
	{
		private readonly KeySettings _keySettings;
		private readonly IKeyEventArgsAugmenter _keyEventArgsAugmenter;
		private readonly IEHSIStateTracker _ehsiStateTracker;
		private readonly IInputEventHandler _ehsiRightKnobReleased;
		public KeyUpEventHandler(
			KeySettings keySettings, 
			IEHSIStateTracker ehsiStateTracker,
			IInputEventHandler ehsiRightKnobReleased,
			IKeyEventArgsAugmenter keyEventArgsAugmenter = null
			)
		{
			_keySettings = keySettings;
			_ehsiStateTracker = ehsiStateTracker;
			_ehsiRightKnobReleased = ehsiRightKnobReleased;
			_keyEventArgsAugmenter = keyEventArgsAugmenter ?? new KeyEventArgsAugmenter();
		}

		public void ProcessKeyUpEvent(KeyEventArgs e)
		{
			if (_keySettings.EHSICourseDepressedKey  == null) return;
			var modifiersPressedRightNow = _keyEventArgsAugmenter.UpdateKeyEventArgsWithExtendedKeyInfo(Keys.None);
			var modifiersInHotkey = (_keySettings.EHSICourseDepressedKey.Keys & Keys.Modifiers);

			if (
				_ehsiStateTracker.RightKnobIsPressed
				&&
				(_keySettings.EHSICourseDepressedKey.ControlType == ControlType.Key)
				&&
				(
					(e.KeyData & Keys.KeyCode) == (_keySettings.EHSICourseDepressedKey.Keys & Keys.KeyCode)
					||
					((e.KeyData & Keys.KeyCode) & ~Keys.LControlKey & ~Keys.RControlKey & ~Keys.LShiftKey & ~Keys.LMenu & ~Keys.RMenu) == Keys.None
				)
				&&
				(
					(
						modifiersInHotkey == Keys.None
					)
					||
					(
						((modifiersInHotkey & Keys.Alt) == Keys.Alt && (modifiersPressedRightNow & Keys.Alt) != Keys.Alt)
						||
						((modifiersInHotkey & Keys.Control) == Keys.Control && (modifiersPressedRightNow & Keys.Control) != Keys.Control)
						||
						((modifiersInHotkey & Keys.Shift) == Keys.Shift && (modifiersPressedRightNow & Keys.Shift) != Keys.Shift)
					)
				)
				)
			{
				_ehsiRightKnobReleased.Raise();
			}
		}
	}
}
