namespace MFDExtractor.EventSystem.Handlers
{
	public interface INightVisionModeToggledEventHandler:IInputEventHandlerEventHandler {}
	public class NightVisionModeToggledEventHandler : INightVisionModeToggledEventHandler
	{
		private readonly ExtractorState _extractorState;

		public NightVisionModeToggledEventHandler(ExtractorState extractorState)
		{
			_extractorState = extractorState;
		}

		public void Handle()
		{
			_extractorState.NightMode = !_extractorState.NightMode;
		}
	}
}
