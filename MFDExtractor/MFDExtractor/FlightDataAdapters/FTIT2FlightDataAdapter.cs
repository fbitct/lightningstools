using F4SharedMem;
using F4Utils.SimSupport;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IFTIT2FlightDataAdapter
    {
        void Adapt(IF16FanTurbineInletTemperature ftit2, FlightData flightData);
    }

    class FTIT2FlightDataAdapter : IFTIT2FlightDataAdapter
    {
        public void Adapt(IF16FanTurbineInletTemperature ftit2, FlightData flightData)
        {
            if (flightData.DataFormat == FalconDataFormats.BMS4)
            {
                ftit2.InstrumentState.InletTemperatureDegreesCelcius = flightData.ftit2 * 100.0f;
            }
            else
            {
                ftit2.InstrumentState.InletTemperatureDegreesCelcius = NonImplementedGaugeCalculations.Ftit(ftit2.InstrumentState.InletTemperatureDegreesCelcius, flightData.rpm2);
            }
        }
    }
}
