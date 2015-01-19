using LightningGauges.Renderers;
using LightningGauges.Renderers.F16;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IAccelerometerResetEventHandler:IInputEventHandlerEventHandler {}
	public class AccelerometerResetEventHandler : IAccelerometerResetEventHandler
	{
		private readonly IAccelerometer _accelerometer;
		public AccelerometerResetEventHandler(IAccelerometer accelerometer)
		{
			_accelerometer = accelerometer ?? new Accelerometer();
		}
		public void Handle()
		{
			_accelerometer.InstrumentState.ResetMinAndMaxGs();
		}
	}
}
