using F4Utils.Process;
using LightningGauges.Renderers;
using LightningGauges.Renderers.F16;
using LightningGauges.Renderers.F16.EHSI;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IEHSIMenuButtonDepressedEventHandler:IInputEventHandlerEventHandler{}
	public class EHSIMenuButtonDepressedEventHandler : IEHSIMenuButtonDepressedEventHandler
	{
		private readonly IEHSI _ehsi;

		public EHSIMenuButtonDepressedEventHandler(IEHSI ehsi)
		{
			_ehsi = ehsi;
		}

		public void Handle()
		{
			var currentMode =_ehsi.InstrumentState.InstrumentMode;
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
				_ehsi.InstrumentState.InstrumentMode = newMode.Value;
			}
			KeyFileUtils.SendCallbackToFalcon("SimStepHSIMode");
		}
	}
}
