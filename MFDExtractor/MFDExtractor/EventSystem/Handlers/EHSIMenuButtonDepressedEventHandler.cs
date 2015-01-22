using F4Utils.Process;
using LightningGauges.Renderers.F16.EHSI;

namespace MFDExtractor.EventSystem.Handlers
{
	internal interface IEHSIMenuButtonDepressedEventHandler:IInputEventHandlerEventHandler{}
	internal class EHSIMenuButtonDepressedEventHandler : IEHSIMenuButtonDepressedEventHandler
	{
		private readonly IEHSIStateTracker _ehsiStateTracker;

		public EHSIMenuButtonDepressedEventHandler(IEHSIStateTracker ehsiStateTracker)
		{
			_ehsiStateTracker = ehsiStateTracker;
		}

		public void Handle()
		{
		    var ehsi = _ehsiStateTracker.EHSI;
			var currentMode =ehsi.InstrumentState.InstrumentMode;
			InstrumentModes? newMode = null;
			switch (currentMode)
			{
				case InstrumentModes.Unknown:
					break;
				case InstrumentModes.PlsTacan:
					newMode = InstrumentModes.Nav;
					break;
				case InstrumentModes.Tacan:
					newMode = InstrumentModes.PlsTacan;
					break;
				case InstrumentModes.Nav:
					newMode = InstrumentModes.PlsNav;
					break;
				case InstrumentModes.PlsNav:
					newMode = InstrumentModes.Tacan;
					break;
			}
			if (newMode.HasValue)
			{
				ehsi.InstrumentState.InstrumentMode = newMode.Value;
			}
			KeyFileUtils.SendCallbackToFalcon("SimStepHSIMode");
		}
	}
}
