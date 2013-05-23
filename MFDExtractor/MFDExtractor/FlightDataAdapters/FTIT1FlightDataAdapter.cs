using F4SharedMem;
using F4Utils.SimSupport;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IFTIT1FlightDataAdapter
    {
        void Adapt(IF16FanTurbineInletTemperature ftit1, FlightData flightData);
    }

    class FTIT1FlightDataAdapter : IFTIT1FlightDataAdapter
    {
        public void Adapt(IF16FanTurbineInletTemperature ftit1, FlightData flightData)
        {
            ftit1.InstrumentState.InletTemperatureDegreesCelcius = flightData.ftit * 100.0f;
        }
    }
}
