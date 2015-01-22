using System;
using F4KeyFile;
using F4Utils.Process;
using MFDExtractor.Properties;

namespace MFDExtractor.EventSystem.Handlers
{
	internal interface IEHSIRightKnobDecreasedByOneEventHandler:IInputEventHandlerEventHandler{}
	internal class EHSIRightKnobDecreasedByOneEventHandler : IEHSIRightKnobDecreasedByOneEventHandler
	{
	    private readonly IEHSIStateTracker _ehsiStateTracker;

		public EHSIRightKnobDecreasedByOneEventHandler(IEHSIStateTracker ehsiStateTracker)
		{
		    _ehsiStateTracker = ehsiStateTracker;
		}

		public void Handle()
		{
            _ehsiStateTracker.RightKnobLastActivityTime = DateTime.Now;
		    var ehsi = _ehsiStateTracker.EHSI;
            if (ehsi.InstrumentState.ShowBrightnessLabel)
			{
                var newBrightness = (int)Math.Floor((ehsi.InstrumentState.Brightness - ehsi.InstrumentState.MaxBrightness) * (1.0f / 32.0f));
                ehsi.InstrumentState.Brightness = newBrightness;
				Settings.Default.EHSIBrightness = newBrightness;
			}
			else
			{
				var useDecrementByOne = false;
				var decByOneCallback = KeyFileUtils.FindKeyBinding("SimHsiCrsDecBy1");
				if (decByOneCallback != null && decByOneCallback.Key.ScanCode != (int)ScanCodes.NotAssigned)
				{
					useDecrementByOne = true;
				}
				KeyFileUtils.SendCallbackToFalcon(useDecrementByOne ? "SimHsiCrsDecBy1" : "SimHsiCourseDec");
			}
			
		}
	}

}
