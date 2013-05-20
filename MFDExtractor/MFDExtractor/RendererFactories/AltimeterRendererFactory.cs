using System;
using LightningGauges.Renderers;
using MFDExtractor.Properties;

namespace MFDExtractor.RendererFactories
{
	internal interface IAltimeterRendererFactory
	{
		IF16Altimeter Create();
	}

	class AltimeterRendererFactory : IAltimeterRendererFactory
	{
		public IF16Altimeter Create()
		{
			return new F16Altimeter
			{
				Options = new F16Altimeter.F16AltimeterOptions
				{

					Style =
						(F16Altimeter.F16AltimeterOptions.F16AltimeterStyle)
							Enum.Parse(typeof (F16Altimeter.F16AltimeterOptions.F16AltimeterStyle), Settings.Default.Altimeter_Style),
					PressureAltitudeUnits =
						(F16Altimeter.F16AltimeterOptions.PressureUnits)
							Enum.Parse(typeof (F16Altimeter.F16AltimeterOptions.PressureUnits), Settings.Default.Altimeter_PressureUnits)
				}
			};
		}
	}
}
