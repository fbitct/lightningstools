﻿using System;
using Common.UI;
using LightningGauges.Renderers;
using LightningGauges.Renderers.F16.ISIS;
using MFDExtractor.Properties;

namespace MFDExtractor.RendererFactories
{
	public interface IISISRendererFactory
	{
		IISIS Create(GdiPlusOptions gdiPlusOptions); 
	}
	class ISISRendererFactory:IISISRendererFactory
	{
		public IISIS Create(GdiPlusOptions gdiPlusOptions)
		{
			var toReturn = new ISIS
			{
				Options = new ISIS.ISISOptions
				{
					PressureAltitudeUnits = (ISIS.ISISOptions.PressureUnits)Enum.Parse(typeof(ISIS.ISISOptions.PressureUnits), Settings.Default.ISIS_PressureUnits),
					GDIPlusOptions = gdiPlusOptions
				}
			};
			return toReturn;
		}
	}
}
