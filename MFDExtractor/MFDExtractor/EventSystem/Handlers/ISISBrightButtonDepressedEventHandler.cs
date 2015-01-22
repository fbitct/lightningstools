using System.Collections.Generic;
using LightningGauges.Renderers.F16.ISIS;

namespace MFDExtractor.EventSystem.Handlers
{
	internal interface IISISBrightButtonDepressedEventHandler:IInputEventHandlerEventHandler{}
	internal class ISISBrightButtonDepressedEventHandler : IISISBrightButtonDepressedEventHandler
	{
		private readonly IDictionary<InstrumentType, IInstrument> _instruments;

		public ISISBrightButtonDepressedEventHandler(IDictionary<InstrumentType, IInstrument> instruments)
		{
		    _instruments = instruments;
		}

		public void Handle()
		{
		    var isis = _instruments[InstrumentType.ISIS].Renderer as IISIS;
			isis.InstrumentState.Brightness = isis.InstrumentState.MaxBrightness;
		}
	}
}
