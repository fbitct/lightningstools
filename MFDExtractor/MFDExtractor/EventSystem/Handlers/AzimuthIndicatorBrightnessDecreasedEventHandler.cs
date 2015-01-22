using System;
using System.Collections.Generic;
using LightningGauges.Renderers.F16.AzimuthIndicator;
using MFDExtractor.Properties;

namespace MFDExtractor.EventSystem.Handlers
{
	internal interface IAzimuthIndicatorBrightnessDecreasedEventHandler:IInputEventHandlerEventHandler {}
	internal class AzimuthIndicatorBrightnessDecreasedEventHandler : IAzimuthIndicatorBrightnessDecreasedEventHandler
	{
        private readonly IDictionary<InstrumentType, IInstrument> _instruments;

		public AzimuthIndicatorBrightnessDecreasedEventHandler(IDictionary<InstrumentType, IInstrument> instruments)
		{
			_instruments = instruments;
		}

		public void Handle()
		{
		    var azimuthIndicator = _instruments[InstrumentType.RWR].Renderer as IAzimuthIndicator;
			var newBrightness = (int)Math.Floor((azimuthIndicator.InstrumentState.Brightness -azimuthIndicator.InstrumentState.MaxBrightness) * (1.0f / 32.0f));
			azimuthIndicator.InstrumentState.Brightness = newBrightness;
			Settings.Default.AzimuthIndicatorBrightness = newBrightness;
		}
	}
}
