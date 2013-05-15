using LightningGauges.Renderers;

namespace MFDExtractor.EventSystem.Handlers
{
	public interface IAccelerometerResetEventHandler:IInputEventHandlerEventHandler {}
	public class AccelerometerResetEventHandler : IAccelerometerResetEventHandler
	{
		private readonly IF16Accelerometer _accelerometer;
		public AccelerometerResetEventHandler(IF16Accelerometer accelerometer)
		{
			_accelerometer = accelerometer ?? new F16Accelerometer();
		}
		public void Handle()
		{
			_accelerometer.InstrumentState.ResetMinAndMaxGs();
		}
	}
}
