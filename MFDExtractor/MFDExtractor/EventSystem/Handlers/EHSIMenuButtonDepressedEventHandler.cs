using F4Utils.Process;
using LightningGauges.Renderers;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IEHSIMenuButtonDepressedEventHandler:IInputEventHandlerEventHandler{}
	public class EHSIMenuButtonDepressedEventHandler : IEHSIMenuButtonDepressedEventHandler
	{
		private readonly IF16EHSI _ehsi;

		public EHSIMenuButtonDepressedEventHandler(IF16EHSI ehsi)
		{
			_ehsi = ehsi;
		}

		public void Handle()
		{
			var currentMode =_ehsi.InstrumentState.InstrumentMode;
			F16EHSI.F16EHSIInstrumentState.InstrumentModes? newMode = null;
			switch (currentMode)
			{
				case F16EHSI.F16EHSIInstrumentState.InstrumentModes.Unknown:
					break;
				case F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsTacan:
					newMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.Nav;
					break;
				case F16EHSI.F16EHSIInstrumentState.InstrumentModes.Tacan:
					newMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsTacan;
					break;
				case F16EHSI.F16EHSIInstrumentState.InstrumentModes.Nav:
					newMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsNav;
					break;
				case F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsNav:
					newMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.Tacan;
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
