using Common.Math;
using F4SharedMem;
using F4SharedMem.Headers;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IStandbyADIFlightDataAdapter
    {
        void Adapt(IF16StandbyADI standbyADI, FlightData flightData);
    }

    class StandbyADIFlightDataAdapter : IStandbyADIFlightDataAdapter
    {
        public void Adapt(IF16StandbyADI standbyADI, FlightData flightData)
        {
            var hsibits = (HsiBits)flightData.hsiBits;
           standbyADI.InstrumentState.OffFlag = ((hsibits & HsiBits.BUP_ADI_OFF) == HsiBits.BUP_ADI_OFF);
            if (((hsibits & HsiBits.BUP_ADI_OFF) == HsiBits.BUP_ADI_OFF))
            {
                //if the standby ADI is off
                standbyADI.InstrumentState.PitchDegrees = 0;
                standbyADI.InstrumentState.RollDegrees = 0;
                standbyADI.InstrumentState.OffFlag = true;
            }
            else
            {
                standbyADI.InstrumentState.PitchDegrees = ((flightData.pitch / Constants.RADIANS_PER_DEGREE));
                standbyADI.InstrumentState.RollDegrees = ((flightData.roll / Constants.RADIANS_PER_DEGREE));
                standbyADI.InstrumentState.OffFlag = false;
            }
        }
    }
}
