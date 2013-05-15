using System;
using LightningGauges.Renderers;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IISISStandardButtonDepressedEventHandler:IInputEventHandlerEventHandler{}
	public class ISISStandardButtonDepressedEventHandler : IISISStandardButtonDepressedEventHandler
	{
		private readonly IF16ISIS _isis;

		public ISISStandardButtonDepressedEventHandler(IF16ISIS isis)
		{
			_isis = isis;
		}

		public void Handle()
		{
			_isis.InstrumentState.Brightness = (int)Math.Floor((_isis.InstrumentState.MaxBrightness) * 0.5f);
		}
	}
}
