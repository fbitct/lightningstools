using System;
using Common.Networking;
using F4SharedMem;
using MFDExtractor.Networking;

namespace MFDExtractor
{
    internal interface IFlightDataRetriever
    {
        FlightData GetFlightData();
    }

    class FlightDataRetriever : IFlightDataRetriever
    {
        private readonly ISharedmemReaderFactory _sharedmemReaderFactory;
        private readonly IExtractorClient _extractorClient;
        public FlightDataRetriever(
            IExtractorClient extractorClient=null,
            ISharedmemReaderFactory sharedmemReaderFactory= null
            )
        {
            _extractorClient = extractorClient;
           
            _sharedmemReaderFactory = sharedmemReaderFactory ?? new SharedmemReaderFactory();
        }

        public FlightData GetFlightData()
        {
            FlightData toReturn = null;
            if ((Extractor.State.OptionsFormIsShowing && !Extractor.State.SimRunning) || (!Extractor.State.SimRunning && Extractor.State.NetworkMode != NetworkMode.Client))
	        {
		        return EmptyFlightData;
	        }
            switch (Extractor.State.NetworkMode)
	        {
		        case NetworkMode.Standalone:
		        case NetworkMode.Server:
		        {
			        var sharedMemReader = _sharedmemReaderFactory.Create();
			        toReturn = sharedMemReader.GetCurrentData();
		        }
			        break;
		        case NetworkMode.Client:
			        toReturn = _extractorClient !=null ? _extractorClient.GetFlightData(): EmptyFlightData;
			        break;
	        }
	        return toReturn ?? EmptyFlightData;
        }
		private FlightData EmptyFlightData { get { return new FlightData { hsiBits = Int32.MaxValue }; } }
    }
}
