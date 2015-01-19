using LightningGauges.Renderers;
using LightningGauges.Renderers.F16;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IAirspeedIndexDecreasedByOneEventHandler:IInputEventHandlerEventHandler {}
	public class AirspeedIndexDecreasedByOneEventHandler : IAirspeedIndexDecreasedByOneEventHandler
	{
		private readonly IAirspeedIndicator _airspeedIndicator;

		public AirspeedIndexDecreasedByOneEventHandler(IAirspeedIndicator airspeedIndicator)
		{
			_airspeedIndicator = airspeedIndicator;
		}

		public void Handle()
		{
			_airspeedIndicator.InstrumentState.AirspeedIndexKnots -= 2.5F;
		}
	}
}
