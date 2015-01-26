using System.Collections.Generic;
using LightningGauges.Renderers.F16.AzimuthIndicator;
using MFDExtractor.Properties;

namespace MFDExtractor.EventSystem.Handlers
{
	internal interface IAzimuthIndicatorBrightnessIncreasedEventHandler:IInputEventHandlerEventHandler{}
	internal class AzimuthIndicatorBrightnessIncreasedEventHandler : IAzimuthIndicatorBrightnessIncreasedEventHandler
	{
		private readonly IDictionary<InstrumentType, IInstrument> _instruments;

        public AzimuthIndicatorBrightnessIncreasedEventHandler(IDictionary<InstrumentType, IInstrument> instruments)
		{
			_instruments = instruments;
		}

		public void Handle()
		{
		    var azimuthIndicator = _instruments[InstrumentType.RWR].Renderer as IAzimuthIndicator;
            var newBrightness = azimuthIndicator.InstrumentState.Brightness + ((int)(azimuthIndicator.InstrumentState.MaxBrightness * (1.0f / 32.0f)));
            if (newBrightness > azimuthIndicator.InstrumentState.MaxBrightness)
            {
                newBrightness = azimuthIndicator.InstrumentState.MaxBrightness;
            }
            azimuthIndicator.InstrumentState.Brightness = newBrightness;
			Settings.Default.AzimuthIndicatorBrightness = newBrightness;
		}
	}
}
