using System;
using F4KeyFile;
using F4Utils.Process;
using LightningGauges.Renderers.F16.EHSI;
using MFDExtractor.Properties;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IEHSIRightKnobDecreasedByOneEventHandler:IInputEventHandlerEventHandler{}
	public class EHSIRightKnobDecreasedByOneEventHandler : IEHSIRightKnobDecreasedByOneEventHandler
	{
	    private readonly IEHSIStateTracker _ehsiStateTracker;
        private readonly IEHSI _ehsi;

		public EHSIRightKnobDecreasedByOneEventHandler(IEHSIStateTracker ehsiStateTracker,IEHSI ehsi)
		{
		    _ehsiStateTracker = ehsiStateTracker;
			_ehsi = ehsi;
		}

		public void Handle()
		{
            _ehsiStateTracker.RightKnobLastActivityTime = DateTime.Now;

			if (_ehsi.InstrumentState.ShowBrightnessLabel)
			{
				var newBrightness = (int)Math.Floor((_ehsi.InstrumentState.Brightness - _ehsi.InstrumentState.MaxBrightness) * (1.0f / 32.0f));
				_ehsi.InstrumentState.Brightness = newBrightness;
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
