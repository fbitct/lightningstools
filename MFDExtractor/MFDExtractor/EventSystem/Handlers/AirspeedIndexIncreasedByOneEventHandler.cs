using System.Collections.Generic;
using LightningGauges.Renderers.F16;

namespace MFDExtractor.EventSystem.Handlers
{
	internal interface IAirspeedIndexIncreasedByOneEventHandler:IInputEventHandlerEventHandler {}
	internal class AirspeedIndexIncreasedByOneEventHandler : IAirspeedIndexIncreasedByOneEventHandler
	{
	    private readonly IDictionary<InstrumentType, IInstrument> _instruments; 
        
		public AirspeedIndexIncreasedByOneEventHandler(IDictionary<InstrumentType, IInstrument> instruments)
		{
		    _instruments = instruments;
		}

		public void Handle()
		{
			(_instruments[InstrumentType.ASI].Renderer as IAirspeedIndicator).InstrumentState.AirspeedIndexKnots += 2.5F;
		}
	}
}
