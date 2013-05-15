using LightningGauges.Renderers;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IAirspeedIndexIncreasedByOneEventHandler:IInputEventHandlerEventHandler {}
	public class AirspeedIndexIncreasedByOneEventHandler : IAirspeedIndexIncreasedByOneEventHandler
	{
		private readonly IF16AirspeedIndicator _airspeedIndicator;

		public AirspeedIndexIncreasedByOneEventHandler(IF16AirspeedIndicator airspeedIndicator)
		{
			_airspeedIndicator = airspeedIndicator;
		}

		public void Handle()
		{
			_airspeedIndicator.InstrumentState.AirspeedIndexKnots += 2.5F;
		}
	}
}
