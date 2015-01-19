using LightningGauges.Renderers;
using LightningGauges.Renderers.F16.ISIS;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IISISBrightButtonDepressedEventHandler:IInputEventHandlerEventHandler{}
	public class ISISBrightButtonDepressedEventHandler : IISISBrightButtonDepressedEventHandler
	{
		private readonly IISIS _isis;

		public ISISBrightButtonDepressedEventHandler(IISIS isis)
		{
			_isis = isis;
		}

		public void Handle()
		{
			_isis.InstrumentState.Brightness = _isis.InstrumentState.MaxBrightness;
		}
	}
}
