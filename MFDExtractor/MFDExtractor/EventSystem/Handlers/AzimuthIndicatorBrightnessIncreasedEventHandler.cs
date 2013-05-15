using System;
using LightningGauges.Renderers;
using MFDExtractor.Properties;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IAzimuthIndicatorBrightnessIncreasedEventHandler:IInputEventHandlerEventHandler{}
	public class AzimuthIndicatorBrightnessIncreasedEventHandler : IAzimuthIndicatorBrightnessIncreasedEventHandler
	{
		private readonly IF16AzimuthIndicator _azimuthIndicator;

		public AzimuthIndicatorBrightnessIncreasedEventHandler(IF16AzimuthIndicator azimuthIndicator)
		{
			_azimuthIndicator = azimuthIndicator;
		}

		public void Handle()
		{
			var newBrightness = (int)Math.Floor((_azimuthIndicator.InstrumentState.Brightness +_azimuthIndicator.InstrumentState.MaxBrightness) * (1.0f / 32.0f));
			_azimuthIndicator.InstrumentState.Brightness = newBrightness;
			Settings.Default.AzimuthIndicatorBrightness = newBrightness;
		}
	}
}
