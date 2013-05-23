using System.Drawing;
using MFDExtractor.Renderer;

namespace MFDExtractor.FlightDataAdapters
{
	namespace MFDExtractor.FlightDataAdapters
	{
		internal interface IMFDFlightDataAdapter
		{
			void Adapt(IMfdRenderer mfd, ExtractorState extractorState, Image instrumentImagesSprite, Rectangle imageSourceRectangle);
		}

		class MFDFlightDataAdapter : IMFDFlightDataAdapter
		{
			public void Adapt(IMfdRenderer mfd, ExtractorState extractorState, Image instrumentImagesSprite, Rectangle imageSourceRectangle)
			{
				mfd.Options.SourceImage = instrumentImagesSprite;
				mfd.Options.SourceRectangle = imageSourceRectangle;
				mfd.InstrumentState.TestMode = extractorState.TestMode;
				mfd.InstrumentState.Blank = extractorState.SimRunning;
			}
		}
	}
}
