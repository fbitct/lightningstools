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
            if (flightData.DataFormat == FalconDataFormats.BMS4)
            {
                ftit1.InstrumentState.InletTemperatureDegreesCelcius = flightData.ftit * 100.0f;
            }
            else
            {
                ftit1.InstrumentState.InletTemperatureDegreesCelcius = NonImplementedGaugeCalculations.Ftit(ftit1.InstrumentState.InletTemperatureDegreesCelcius, flightData.rpm);
            }
        }
    }
}
