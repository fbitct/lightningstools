using System;
using F4KeyFile;
using F4SharedMem;
using F4Utils.Process;
using LightningGauges.Renderers;
using LightningGauges.Renderers.F16;
using LightningGauges.Renderers.F16.EHSI;
using MFDExtractor.Properties;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IEHSIRightKnobDecreasedByOneEventHandler:IInputEventHandlerEventHandler{}
	public class EHSIRightKnobDecreasedByOneEventHandler : IEHSIRightKnobDecreasedByOneEventHandler
	{
		private readonly IEHSI _ehsi;
		public EHSIRightKnobDecreasedByOneEventHandler(IEHSI ehsi)
		{
			_ehsi = ehsi;
		}

		public void Handle()
		{
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
