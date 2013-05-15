using Common.UI;
using MFDExtractor.Properties;

namespace MFDExtractor.Configuration
{
	internal interface IGdiPlusOptionsReader
	{
		GdiPlusOptions Read();
	}

	class GdiPlusOptionsReader : IGdiPlusOptionsReader
	{
		public GdiPlusOptions Read()
		{
			return new GdiPlusOptions
			{
				CompositingQuality = Settings.Default.CompositingQuality,
				InterpolationMode = Settings.Default.InterpolationMode,
				PixelOffsetMode = Settings.Default.PixelOffsetMode,
				SmoothingMode = Settings.Default.SmoothingMode,
				TextRenderingHint = Settings.Default.TextRenderingHint
			};
		}
	}
}
