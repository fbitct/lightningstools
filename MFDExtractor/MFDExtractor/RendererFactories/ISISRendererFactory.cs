using System;
using Common.UI;
using LightningGauges.Renderers;
using LightningGauges.Renderers.F16.ISIS;
using MFDExtractor.Properties;

namespace MFDExtractor.RendererFactories
{
	public interface IISISRendererFactory
	{
		IF16ISIS Create(GdiPlusOptions gdiPlusOptions); 
	}
	class ISISRendererFactory:IISISRendererFactory
	{
		public IF16ISIS Create(GdiPlusOptions gdiPlusOptions)
		{
			var toReturn = new F16ISIS
			{
				Options = new F16ISIS.F16ISISOptions
				{
					PressureAltitudeUnits = (F16ISIS.F16ISISOptions.PressureUnits)Enum.Parse(typeof(F16ISIS.F16ISISOptions.PressureUnits), Settings.Default.ISIS_PressureUnits),
					GDIPlusOptions = gdiPlusOptions
				}
			};
			return toReturn;
		}
	}
}
