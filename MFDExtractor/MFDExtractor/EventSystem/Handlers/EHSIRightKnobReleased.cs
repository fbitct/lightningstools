using System;

namespace MFDExtractor.EventSystem.Handlers
{
	class EHSIRightKnobReleased : IInputEventHandler
	{
		private readonly IEHSIStateTracker _ehsiStateTracker;

		public EHSIRightKnobReleased(IEHSIStateTracker ehsiStateTracker)
		{
			_ehsiStateTracker = ehsiStateTracker;
		}

		public void Raise()
		{
			_ehsiStateTracker.RightKnobDepressedTime = null;
			_ehsiStateTracker.RightKnobReleasedTime = DateTime.Now;
			_ehsiStateTracker.RightKnobLastActivityTime = DateTime.Now;
		}
	}
}
