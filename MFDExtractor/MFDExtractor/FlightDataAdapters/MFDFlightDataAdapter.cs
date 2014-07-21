using System.Drawing;
using MFDExtractor.Renderer;

namespace MFDExtractor.FlightDataAdapters
{
	namespace MFDExtractor.FlightDataAdapters
	{
		internal interface IMFDFlightDataAdapter
		{
			void Adapt(IMfdRenderer mfd, ExtractorState extractorState, Image image);
		}

		class MFDFlightDataAdapter : IMFDFlightDataAdapter
		{
			public void Adapt(IMfdRenderer mfd, ExtractorState extractorState, Image image)
			{
				mfd.InstrumentState.SourceImage = image;
                mfd.InstrumentState.SourceRectangle = image != null ? new Rectangle(0, 0, image.Width, image.Height) : Rectangle.Empty;
				mfd.InstrumentState.TestMode = extractorState.TestMode;
				mfd.InstrumentState.Blank = !extractorState.SimRunning;
			}
		}
	}
}
