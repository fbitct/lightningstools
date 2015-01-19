using System;
using Common.UI;
using LightningGauges.Renderers;
using LightningGauges.Renderers.F16;
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
				Options = new AzimuthIndicator.AzimuthIndicatorOptions
				{
					Style = (AzimuthIndicator.AzimuthIndicatorOptions.InstrumentStyle)
						Enum.Parse(typeof (AzimuthIndicator.AzimuthIndicatorOptions.InstrumentStyle),
							Settings.Default.AzimuthIndicatorType),
					HideBezel = !Settings.Default.AzimuthIndicator_ShowBezel,
					GDIPlusOptions = gdiPlusOptions
				}
			};
		}
	}
}
