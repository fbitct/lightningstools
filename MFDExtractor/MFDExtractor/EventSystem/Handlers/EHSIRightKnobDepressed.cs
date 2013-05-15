using System;

namespace MFDExtractor.EventSystem.Handlers
{
	class EHSIRightKnobDepressed : IInputEventHandler
	{
		private readonly IEHSIStateTracker _ehsiStateTracker;

		public EHSIRightKnobDepressed(IEHSIStateTracker ehsiStateTracker)
		{
			_ehsiStateTracker = ehsiStateTracker;
		}

		public void Raise()
		{
			_ehsiStateTracker.RightKnobDepressedTime = DateTime.Now;
			_ehsiStateTracker.RightKnobReleasedTime = null;
			_ehsiStateTracker.RightKnobLastActivityTime = DateTime.Now;
		}
	}
}
