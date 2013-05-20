using System;
using Common.UI;
using LightningGauges.Renderers;
using MFDExtractor.Properties;

namespace MFDExtractor.RendererFactories
{
	internal interface IAzimuthIndicatorFactory
	{
		IF16AzimuthIndicator Create(GdiPlusOptions gdiPlusOptions);
	}

	class AzimuthIndicatorFactory : IAzimuthIndicatorFactory
	{
		public IF16AzimuthIndicator Create(GdiPlusOptions gdiPlusOptions)
		{
			return new F16AzimuthIndicator
			{
				Options = new F16AzimuthIndicator.F16AzimuthIndicatorOptions
				{
					Style = (F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle)
						Enum.Parse(typeof (F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle),
							Settings.Default.AzimuthIndicatorType),
					HideBezel = !Settings.Default.AzimuthIndicator_ShowBezel,
					GDIPlusOptions = gdiPlusOptions
				}
			};
		}
	}
}
