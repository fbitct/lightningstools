using System;
using LightningGauges.Renderers;
using MFDExtractor.Properties;

namespace MFDExtractor.EventSystem.Handlers
{
	class AzimuthIndicatorBrightnessIncreased : IInputEventHandler
	{
		private readonly IF16AzimuthIndicator _azimuthIndicator;

		public AzimuthIndicatorBrightnessIncreased(IF16AzimuthIndicator azimuthIndicator)
		{
			_azimuthIndicator = azimuthIndicator;
		}

		public void Raise()
		{
			var newBrightness = (int)Math.Floor((_azimuthIndicator.InstrumentState.Brightness +_azimuthIndicator.InstrumentState.MaxBrightness) * (1.0f / 32.0f));
			_azimuthIndicator.InstrumentState.Brightness = newBrightness;
			Settings.Default.AzimuthIndicatorBrightness = newBrightness;
		}
	}
}
