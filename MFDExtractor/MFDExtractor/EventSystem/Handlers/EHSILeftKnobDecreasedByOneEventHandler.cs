using F4KeyFile;
using F4Utils.Process;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IEHSILeftKnobDecreasedByOneEventHandler:IInputEventHandlerEventHandler{}
	public class EHSILeftKnobDecreasedByOneEventHandler : IEHSILeftKnobDecreasedByOneEventHandler
	{
		public void Handle()
		{
			var useDecrementByOne = false;
			var decByOneCallback = KeyFileUtils.FindKeyBinding("SimHsiHdgDecBy1");
			if (decByOneCallback != null && decByOneCallback.Key.ScanCode != (int)ScanCodes.NotAssigned)
			{
				useDecrementByOne = true;
			}
			KeyFileUtils.SendCallbackToFalcon(useDecrementByOne ? "SimHsiHdgDecBy1" : "SimHsiHeadingDec");
		}
	}
}
