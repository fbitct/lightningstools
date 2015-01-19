using System;
using Common.UI;
using LightningGauges.Renderers;
using LightningGauges.Renderers.F16;
using LightningGauges.Renderers.F16.AzimuthIndicator;
using MFDExtractor.Properties;

namespace MFDExtractor.RendererFactories
{
	internal interface IAzimuthIndicatorFactory
	{
		IAzimuthIndicator Create(GdiPlusOptions gdiPlusOptions);
	}

	class AzimuthIndicatorFactory : IAzimuthIndicatorFactory
	{
		public IAzimuthIndicator Create(GdiPlusOptions gdiPlusOptions)
		{
			return new AzimuthIndicator
			{
				Options = new Options
				{
					Style = (AzimuthIndicator.InstrumentStyle)
						Enum.Parse(typeof (AzimuthIndicator.InstrumentStyle),
							Settings.Default.AzimuthIndicatorType),
					HideBezel = !Settings.Default.AzimuthIndicator_ShowBezel,
					GDIPlusOptions = gdiPlusOptions
				}
			};
		}
	}
}
