using System;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IEHSIRightKnobDepressedEventHandler:IInputEventHandlerEventHandler {}
	class EHSIRightKnobDepressedEventHandler : IEHSIRightKnobDepressedEventHandler
	{
		private readonly IEHSIStateTracker _ehsiStateTracker;

		public EHSIRightKnobDepressedEventHandler(IEHSIStateTracker ehsiStateTracker)
		{
			_ehsiStateTracker = ehsiStateTracker;
		}

		public void Handle()
		{
			_ehsiStateTracker.RightKnobDepressedTime = DateTime.Now;
			_ehsiStateTracker.RightKnobReleasedTime = null;
			_ehsiStateTracker.RightKnobLastActivityTime = DateTime.Now;
		}
	}
}
