using System;
using F4KeyFile;
using F4SharedMem;
using F4Utils.Process;
using LightningGauges.Renderers;
using MFDExtractor.Properties;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IEHSIRightKnobIncreasedByOneEventHandler:IInputEventHandlerEventHandler{}
	public class EHSIRightKnobIncreasedByOneEventHandler : IEHSIRightKnobIncreasedByOneEventHandler
	{
		private readonly IEHSIStateTracker _ehsiStateTracker;
		private readonly IF16EHSI _ehsi;
		public EHSIRightKnobIncreasedByOneEventHandler(IEHSIStateTracker ehsiStateTracker, IF16EHSI ehsi)
		{
			_ehsiStateTracker = ehsiStateTracker;
			_ehsi = ehsi;
		}

		public void Handle()
		{
			_ehsiStateTracker.RightKnobLastActivityTime = DateTime.Now;
			if (_ehsi.InstrumentState.ShowBrightnessLabel)
			{
				var newBrightness = (int)Math.Floor((_ehsi.InstrumentState.Brightness +_ehsi.InstrumentState.MaxBrightness) * (1.0f / 32.0f));
				_ehsi.InstrumentState.Brightness = newBrightness;
				Settings.Default.EHSIBrightness = newBrightness;
			}
			else
			{
				var useIncrementByOne = false;
				var incByOneCallback = KeyFileUtils.FindKeyBinding("SimHsiCrsIncBy1");
				if (incByOneCallback != null && incByOneCallback.Key.ScanCode != (int)ScanCodes.NotAssigned)
				{
					useIncrementByOne = true;
				}
				KeyFileUtils.SendCallbackToFalcon(useIncrementByOne ? "SimHsiCrsIncBy1" : "SimHsiCourseInc");
			}
		}
	}
}
