namespace MFDExtractor.EventSystem.Handlers
{
	public interface INightVisionModeToggledEventHandler:IInputEventHandlerEventHandler {}
	public class NightVisionModeToggledEventHandler : INightVisionModeToggledEventHandler
	{
		private readonly Extractor _extractor;

		public NightVisionModeToggledEventHandler(Extractor extractor)
		{
			_extractor = extractor;
		}

		public void Handle()
		{
			_extractor.State.NightMode = !_extractor.State.NightMode;
		}
	}
}
