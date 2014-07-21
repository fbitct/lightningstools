using System.Drawing;
using Common.Networking;
using MFDExtractor.Networking;
using MFDExtractor.Properties;
using MFDExtractor.Renderer;

namespace MFDExtractor.FlightDataAdapters
{
	namespace MFDExtractor.FlightDataAdapters
	{
		internal interface IMFDFlightDataAdapter
		{
			void Adapt(IMfdRenderer mfd, ExtractorState extractorState, F4TexSharedMem.IReader texSharedMemReader, Rectangle sourceRectangle, IExtractorClient client, InstrumentType instrumentType);
		}

		class MFDFlightDataAdapter : IMFDFlightDataAdapter
		{
			public void Adapt(IMfdRenderer mfd, ExtractorState extractorState, F4TexSharedMem.IReader texSharedMemReader, Rectangle sourceRectangle, IExtractorClient client, InstrumentType instrumentType)
			{

			    if ((NetworkMode) Settings.Default.NetworkingMode == NetworkMode.Client)
			    {
			        mfd.InstrumentState.SourceImage = client.GetInstrumentImage(instrumentType);
			    }
			    else
			    {
			        mfd.InstrumentState.SourceImage = sourceRectangle.IsEmpty
			            ? null
			            : texSharedMemReader.GetImage(sourceRectangle);
			    }
			    mfd.InstrumentState.SourceRectangle = mfd.InstrumentState.SourceImage != null ? new Rectangle(0, 0, mfd.InstrumentState.SourceImage.Width, mfd.InstrumentState.SourceImage.Height) : Rectangle.Empty;
				mfd.InstrumentState.TestMode = extractorState.TestMode;
				mfd.InstrumentState.Blank = !extractorState.SimRunning;
			    if ((NetworkMode)Settings.Default.NetworkingMode == NetworkMode.Server)
			    {
			        ExtractorServer.SetInstrumentImage( mfd.InstrumentState.SourceImage, instrumentType);
			    }
			}
		}
	}
}
