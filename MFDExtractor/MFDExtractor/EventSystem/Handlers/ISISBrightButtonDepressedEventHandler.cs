using LightningGauges.Renderers;
using LightningGauges.Renderers.F16.ISIS;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IISISBrightButtonDepressedEventHandler:IInputEventHandlerEventHandler{}
	public class ISISBrightButtonDepressedEventHandler : IISISBrightButtonDepressedEventHandler
	{
		private readonly IF16ISIS _isis;

		public ISISBrightButtonDepressedEventHandler(IF16ISIS isis)
		{
			_isis = isis;
		}

		public void Handle()
		{
			_isis.InstrumentState.Brightness = _isis.InstrumentState.MaxBrightness;
		}
	}
}
