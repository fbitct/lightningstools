using F4Utils.Process;
using LightningGauges.Renderers;
using LightningGauges.Renderers.F16;

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
			EHSI.EHSIInstrumentState.InstrumentModes? newMode = null;
			switch (currentMode)
			{
				case EHSI.EHSIInstrumentState.InstrumentModes.Unknown:
					break;
				case EHSI.EHSIInstrumentState.InstrumentModes.PlsTacan:
					newMode = EHSI.EHSIInstrumentState.InstrumentModes.Nav;
					break;
				case EHSI.EHSIInstrumentState.InstrumentModes.Tacan:
					newMode = EHSI.EHSIInstrumentState.InstrumentModes.PlsTacan;
					break;
				case EHSI.EHSIInstrumentState.InstrumentModes.Nav:
					newMode = EHSI.EHSIInstrumentState.InstrumentModes.PlsNav;
					break;
				case EHSI.EHSIInstrumentState.InstrumentModes.PlsNav:
					newMode = EHSI.EHSIInstrumentState.InstrumentModes.Tacan;
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
