using System;
using LightningGauges.Renderers;
using MFDExtractor.Properties;
using MFDExtractor.UI;

namespace MFDExtractor.RendererFactories
{
	internal interface IVVIRendererFactory
	{
		IF16VerticalVelocityIndicator Create();
	}

	class VVIRendererFactory : IVVIRendererFactory
	{
		public IF16VerticalVelocityIndicator Create()
		{
			var vviStyleString = Settings.Default.VVI_Style;
			var vviStyle = (VVIStyles)Enum.Parse(typeof(VVIStyles), vviStyleString);
			switch (vviStyle)
			{
				case VVIStyles.Tape:
					return new F16VerticalVelocityIndicatorUSA();
				case VVIStyles.Needle:
					return new F16VerticalVelocityIndicatorEU();
				default:
					return new F16VerticalVelocityIndicatorUSA();
			}
		}
	}
}
