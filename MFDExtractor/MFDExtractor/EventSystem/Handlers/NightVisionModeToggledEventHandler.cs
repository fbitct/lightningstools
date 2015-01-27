namespace MFDExtractor.EventSystem.Handlers
{
	public interface INightVisionModeToggledEventHandler:IInputEventHandlerEventHandler {}
	public class NightVisionModeToggledEventHandler : INightVisionModeToggledEventHandler
	{
		public void Handle()
		{
			Extractor.State.NightMode = !Extractor.State.NightMode;
		}
	}
}
