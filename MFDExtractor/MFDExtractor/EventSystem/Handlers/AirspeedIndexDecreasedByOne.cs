using LightningGauges.Renderers;

namespace MFDExtractor.EventSystem.Handlers
{
	class AirspeedIndexDecreasedByOne : IInputEventHandler
	{
		private readonly IF16AirspeedIndicator _airspeedIndicator;

		public AirspeedIndexDecreasedByOne(IF16AirspeedIndicator airspeedIndicator)
		{
			_airspeedIndicator = airspeedIndicator;
		}

		public void Raise()
		{
			_airspeedIndicator.InstrumentState.AirspeedIndexKnots -= 2.5F;
		}
	}
}
