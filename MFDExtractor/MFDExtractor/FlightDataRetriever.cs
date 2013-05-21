using System;
using Common.Networking;
using F4SharedMem;

namespace MFDExtractor
{
    internal interface IFlightDataRetriever
    {
        FlightData GetFlightData(ExtractorState extractorState);
    }

    class FlightDataRetriever : IFlightDataRetriever
    {
        private readonly ISharedmemReaderFactory _sharedmemReaderFactory;
        private readonly IRadarAltitudeCalculator _radarAltitudeCalculator;
        private readonly IExtractorClient _extractorClient;
        public FlightDataRetriever(
            IExtractorClient extractorClient,
            ISharedmemReaderFactory sharedmemReaderFactory= null, 
            IRadarAltitudeCalculator radarAltitudeCalculator = null
            )
        {
            _extractorClient = extractorClient;
            _sharedmemReaderFactory = sharedmemReaderFactory ?? new SharedmemReaderFactory();
            _radarAltitudeCalculator = radarAltitudeCalculator ?? new RadarAltitudeCalculator();
        }

        public FlightData GetFlightData(ExtractorState extractorState)
        {
            FlightData toReturn = null;
            if (!extractorState.TestMode)
            {
                if (extractorState.SimRunning || extractorState.NetworkMode == NetworkMode.Client)
                {
                    switch (extractorState.NetworkMode)
                    {
                        case NetworkMode.Standalone:
                        case NetworkMode.Server:
                        {
                                var sharedMemReader = _sharedmemReaderFactory.Create();
                                toReturn = sharedMemReader.GetCurrentData();
                                if (extractorState.NetworkMode == NetworkMode.Server)
                                {
                                    _radarAltitudeCalculator.ComputeRadarAltitude(toReturn);
                                }
                            }
                            break;
                        case NetworkMode.Client:
                            toReturn = _extractorClient.GetFlightData();
                            break;
                    }
                }
            }
            return toReturn ?? new FlightData { hsiBits = Int32.MaxValue };
        }
    }
}
