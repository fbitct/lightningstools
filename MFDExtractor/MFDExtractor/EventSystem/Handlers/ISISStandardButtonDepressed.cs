using System;
using LightningGauges.Renderers;

namespace MFDExtractor.EventSystem.Handlers
{
	class ISISStandardButtonDepressed : IInputEventHandler
	{
		private readonly IF16ISIS _isis;

		public ISISStandardButtonDepressed(IF16ISIS isis)
		{
			_isis = isis;
		}

		public void Raise()
		{
			_isis.InstrumentState.Brightness = (int)Math.Floor((_isis.InstrumentState.MaxBrightness) * 0.5f);
		}
	}
}
