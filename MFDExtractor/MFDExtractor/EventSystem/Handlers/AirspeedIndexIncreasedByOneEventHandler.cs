using LightningGauges.Renderers;
using LightningGauges.Renderers.F16;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IAirspeedIndexIncreasedByOneEventHandler:IInputEventHandlerEventHandler {}
	public class AirspeedIndexIncreasedByOneEventHandler : IAirspeedIndexIncreasedByOneEventHandler
	{
		private readonly IAirspeedIndicator _airspeedIndicator;

		public AirspeedIndexIncreasedByOneEventHandler(IAirspeedIndicator airspeedIndicator)
		{
			_airspeedIndicator = airspeedIndicator;
		}

		public void Handle()
		{
			_airspeedIndicator.InstrumentState.AirspeedIndexKnots += 2.5F;
		}
	}
}
