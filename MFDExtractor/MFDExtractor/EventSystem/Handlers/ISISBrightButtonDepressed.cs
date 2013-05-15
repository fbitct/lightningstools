using LightningGauges.Renderers;

namespace MFDExtractor.EventSystem.Handlers
{
	class ISISBrightButtonDepressed : IInputEventHandler
	{
		private readonly IF16ISIS _isis;

		public ISISBrightButtonDepressed(IF16ISIS isis)
		{
			_isis = isis;
		}

		public void Raise()
		{
			_isis.InstrumentState.Brightness = _isis.InstrumentState.MaxBrightness;
		}
	}
}
