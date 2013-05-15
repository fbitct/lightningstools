using F4KeyFile;
using F4SharedMem;
using F4Utils.Process;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IEHSILeftKnobIncreasedByOneEventHandler:IInputEventHandlerEventHandler {}
	public class EHSILeftKnobIncreasedByOneEventHandler : IEHSILeftKnobIncreasedByOneEventHandler
	{
		public void Handle()
		{
			var falconDataFormat = Util.DetectFalconFormat();
			var useIncrementByOne = false;
			if (falconDataFormat.HasValue && falconDataFormat.Value == FalconDataFormats.BMS4)
			{
				var incByOneCallback = KeyFileUtils.FindKeyBinding("SimHsiHdgIncBy1");
				if (incByOneCallback != null && incByOneCallback.Key.ScanCode != (int)ScanCodes.NotAssigned)
				{
					useIncrementByOne = true;
				}
			}
			KeyFileUtils.SendCallbackToFalcon(useIncrementByOne ? "SimHsiHdgIncBy1" : "SimHsiHeadingInc");
		}
	}
}
