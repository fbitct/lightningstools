using System;
using System.Collections.Generic;
using LightningGauges.Renderers.F16.ISIS;

namespace MFDExtractor.EventSystem.Handlers
{
	internal interface IISISStandardButtonDepressedEventHandler:IInputEventHandlerEventHandler{}
	internal class ISISStandardButtonDepressedEventHandler : IISISStandardButtonDepressedEventHandler
	{
		private readonly IDictionary<InstrumentType, IInstrument> _instruments;

		public ISISStandardButtonDepressedEventHandler(IDictionary<InstrumentType, IInstrument> instruments)
		{
			_instruments = instruments;
		}

		public void Handle()
		{
		    var isis = _instruments[InstrumentType.ISIS].Renderer as IISIS;
			isis.InstrumentState.Brightness = (int)Math.Floor((isis.InstrumentState.MaxBrightness) * 0.5f);
		}
	}
}
