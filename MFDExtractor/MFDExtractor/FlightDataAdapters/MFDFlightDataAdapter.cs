using System.Drawing;
using Common.Networking;
using MFDExtractor.Networking;
using MFDExtractor.Properties;
using MFDExtractor.Renderer;
using System;

namespace MFDExtractor.FlightDataAdapters
{
	namespace MFDExtractor.FlightDataAdapters
	{
		internal interface IMFDFlightDataAdapter
		{
			void Adapt(IMfdRenderer mfd, F4TexSharedMem.IReader texSharedMemReader, Rectangle sourceRectangle, InstrumentType instrumentType);
		}

		class MFDFlightDataAdapter : IMFDFlightDataAdapter
		{
			public void Adapt(IMfdRenderer mfd, F4TexSharedMem.IReader texSharedMemReader, Rectangle sourceRectangle, InstrumentType instrumentType)
			{

			    if (mfd == null || texSharedMemReader == null) return;
                try
                {
                    if ((NetworkMode)Settings.Default.NetworkingMode == NetworkMode.Client)
                    {
                        mfd.InstrumentState.SourceImage = ExtractorClient.GetInstrumentImage(instrumentType);
                    }
                    else
                    {
                        mfd.InstrumentState.SourceImage = sourceRectangle.IsEmpty
                            ? null
                            : texSharedMemReader.GetImage(sourceRectangle);
                    }
                    mfd.InstrumentState.SourceRectangle = mfd.InstrumentState.SourceImage != null ? new Rectangle(0, 0, mfd.InstrumentState.SourceImage.Width, mfd.InstrumentState.SourceImage.Height) : Rectangle.Empty;
                    mfd.InstrumentState.TestMode = Extractor.State.OptionsFormIsShowing && !Extractor.State.SimRunning;
                    mfd.InstrumentState.Blank = !Extractor.State.SimRunning;
                    if (!Extractor.State.Running || !Extractor.State.KeepRunning)
                    {
                        mfd.InstrumentState.SourceImage = null;
                    }
                    if ((NetworkMode)Settings.Default.NetworkingMode == NetworkMode.Server)
                    {
                        ExtractorServer.SetInstrumentImage(mfd.InstrumentState.SourceImage, instrumentType);
                    }
                }
                catch (InvalidOperationException) { }
                catch (AccessViolationException) { }
                
			}
		}
	}
}
