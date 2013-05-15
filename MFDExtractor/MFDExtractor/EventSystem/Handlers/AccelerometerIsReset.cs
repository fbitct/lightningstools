using LightningGauges.Renderers;

namespace MFDExtractor.EventSystem.Handlers
{
	class AccelerometerIsReset : IInputEventHandler
	{
		private readonly IF16Accelerometer _accelerometer;
		public AccelerometerIsReset(IF16Accelerometer accelerometer)
		{
			_accelerometer = accelerometer ?? new F16Accelerometer();
		}
		public void Raise()
		{
			_accelerometer.InstrumentState.ResetMinAndMaxGs();
		}
	}
}
