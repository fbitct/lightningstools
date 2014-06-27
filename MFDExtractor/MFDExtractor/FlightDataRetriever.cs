using System;
using Common.Networking;
using F4SharedMem;
using F4Utils.Terrain;
using MFDExtractor.Networking;

namespace MFDExtractor
{
    internal interface IFlightDataRetriever
    {
        FlightData GetFlightData(ExtractorState extractorState);
    }

    class FlightDataRetriever : IFlightDataRetriever
    {
        private readonly ISharedmemReaderFactory _sharedmemReaderFactory;
        private readonly IExtractorClient _extractorClient;
        public FlightDataRetriever(
            IExtractorClient extractorClient,
            ISharedmemReaderFactory sharedmemReaderFactory= null
            )
        {
            _extractorClient = extractorClient;
           
            _sharedmemReaderFactory = sharedmemReaderFactory ?? new SharedmemReaderFactory();
        }

        public FlightData GetFlightData(ExtractorState extractorState)
        {
            FlightData toReturn = null;
	        if (extractorState.TestMode || (!extractorState.SimRunning && extractorState.NetworkMode != NetworkMode.Client))
	        {
		        return EmptyFlightData;
	        }
	        switch (extractorState.NetworkMode)
	        {
		        case NetworkMode.Standalone:
		        case NetworkMode.Server:
		        {
			        var sharedMemReader = _sharedmemReaderFactory.Create();
			        toReturn = sharedMemReader.GetCurrentData();
		        }
			        break;
		        case NetworkMode.Client:
			        toReturn = _extractorClient.GetFlightData();
			        break;
	        }
	        return toReturn ?? EmptyFlightData;
        }
		private FlightData EmptyFlightData { get { return new FlightData { hsiBits = Int32.MaxValue }; } }
    }
}
