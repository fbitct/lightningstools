using System;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IEHSIRightKnobReleasedEventHandler:IInputEventHandlerEventHandler {}
	public class EHSIRightKnobReleasedEventHandler : IEHSIRightKnobReleasedEventHandler
	{
		private readonly IEHSIStateTracker _ehsiStateTracker;

		public EHSIRightKnobReleasedEventHandler(IEHSIStateTracker ehsiStateTracker)
		{
			_ehsiStateTracker = ehsiStateTracker;
		}

		public void Handle()
		{
			_ehsiStateTracker.RightKnobDepressedTime = null;
			_ehsiStateTracker.RightKnobReleasedTime = DateTime.Now;
			_ehsiStateTracker.RightKnobLastActivityTime = DateTime.Now;
		}
	}
}
