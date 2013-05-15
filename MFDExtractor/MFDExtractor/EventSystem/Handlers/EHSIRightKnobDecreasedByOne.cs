using System;
using F4KeyFile;
using F4SharedMem;
using F4Utils.Process;
using LightningGauges.Renderers;
using MFDExtractor.Properties;

namespace MFDExtractor.EventSystem.Handlers
{
	class EHSIRightKnobDecreasedByOne : IInputEventHandler
	{
		private readonly IF16EHSI _ehsi;
		public EHSIRightKnobDecreasedByOne(IF16EHSI ehsi)
		{
			_ehsi = ehsi;
		}

		public void Raise()
		{
			if (_ehsi.InstrumentState.ShowBrightnessLabel)
			{
				var newBrightness = (int)Math.Floor((_ehsi.InstrumentState.Brightness - _ehsi.InstrumentState.MaxBrightness) * (1.0f / 32.0f));
				_ehsi.InstrumentState.Brightness = newBrightness;
				Settings.Default.EHSIBrightness = newBrightness;
			}
			else
			{
				var format = Util.DetectFalconFormat();
				var useDecrementByOne = false;
				if (format.HasValue && format.Value == FalconDataFormats.BMS4)
				{
					var decByOneCallback = KeyFileUtils.FindKeyBinding("SimHsiCrsDecBy1");
					if (decByOneCallback != null && decByOneCallback.Key.ScanCode != (int)ScanCodes.NotAssigned)
					{
						useDecrementByOne = true;
					}
				}
				KeyFileUtils.SendCallbackToFalcon(useDecrementByOne ? "SimHsiCrsDecBy1" : "SimHsiCourseDec");
			}
			
		}
	}

}
