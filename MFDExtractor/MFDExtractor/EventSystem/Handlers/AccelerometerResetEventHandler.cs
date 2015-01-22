using System.Collections.Generic;
using LightningGauges.Renderers.F16;

namespace MFDExtractor.EventSystem.Handlers
{
	internal interface IAccelerometerResetEventHandler:IInputEventHandlerEventHandler {}
	internal class AccelerometerResetEventHandler : IAccelerometerResetEventHandler
	{
        private readonly IDictionary<InstrumentType, IInstrument> _instruments;
        public AccelerometerResetEventHandler(IDictionary<InstrumentType, IInstrument> instruments)
        {
            _instruments = instruments;
        }
		public void Handle()
		{
		    var accelerometer = _instruments[InstrumentType.Accelerometer].Renderer as IAccelerometer;
			accelerometer.InstrumentState.ResetMinAndMaxGs();
		}
	}
}
