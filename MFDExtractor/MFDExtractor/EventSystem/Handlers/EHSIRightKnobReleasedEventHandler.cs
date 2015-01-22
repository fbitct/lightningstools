using System;

namespace MFDExtractor.EventSystem.Handlers
{
	internal interface IEHSIRightKnobReleasedEventHandler:IInputEventHandlerEventHandler {}
	internal class EHSIRightKnobReleasedEventHandler : IEHSIRightKnobReleasedEventHandler
	{
		private readonly IEHSIStateTracker _ehsiStateTracker;

		public EHSIRightKnobReleasedEventHandler(IEHSIStateTracker ehsiStateTracker)
		{
			_ehsiStateTracker = ehsiStateTracker;
		}

		public void Handle()
		{
		    if (_ehsiStateTracker.RightKnobIsPressed)
		    {
		        _ehsiStateTracker.RightKnobDepressedTime = null;
		        _ehsiStateTracker.RightKnobReleasedTime = DateTime.Now;
		        _ehsiStateTracker.RightKnobLastActivityTime = DateTime.Now;
		    }
		}
	}
}
