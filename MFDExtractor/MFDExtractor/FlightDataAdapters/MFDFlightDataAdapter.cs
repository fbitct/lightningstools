using System.Drawing;
using MFDExtractor.Renderer;

namespace MFDExtractor.FlightDataAdapters
{
	namespace MFDExtractor.FlightDataAdapters
	{
		internal interface IMFDFlightDataAdapter
		{
			void Adapt(IMfdRenderer mfd, ExtractorState extractorState, F4TexSharedMem.IReader texSharedMemReader, Rectangle sourceRectangle);
		}

		class MFDFlightDataAdapter : IMFDFlightDataAdapter
		{
			public void Adapt(IMfdRenderer mfd, ExtractorState extractorState, F4TexSharedMem.IReader texSharedMemReader, Rectangle sourceRectangle)
			{
				mfd.InstrumentState.SourceImage = sourceRectangle.IsEmpty ? null: texSharedMemReader.GetImage(sourceRectangle);
                mfd.InstrumentState.SourceRectangle = mfd.InstrumentState.SourceImage != null ? new Rectangle(0, 0, mfd.InstrumentState.SourceImage.Width, mfd.InstrumentState.SourceImage.Height) : Rectangle.Empty;
				mfd.InstrumentState.TestMode = extractorState.TestMode;
				mfd.InstrumentState.Blank = !extractorState.SimRunning;
			}
		}
	}
}
