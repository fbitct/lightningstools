using LightningGauges.Renderers;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IAirspeedIndexDecreasedByOneEventHandler:IInputEventHandlerEventHandler {}
	public class AirspeedIndexDecreasedByOneEventHandler : IAirspeedIndexDecreasedByOneEventHandler
	{
		private readonly IF16AirspeedIndicator _airspeedIndicator;

		public AirspeedIndexDecreasedByOneEventHandler(IF16AirspeedIndicator airspeedIndicator)
		{
			_airspeedIndicator = airspeedIndicator;
		}

		public void Handle()
		{
			_airspeedIndicator.InstrumentState.AirspeedIndexKnots -= 2.5F;
		}
	}
}
