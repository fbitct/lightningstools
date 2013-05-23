using F4SharedMem;
using F4SharedMem.Headers;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IAltimeterFlightDataAdapter
    {
        void Adapt(IF16Altimeter altimeter, FlightData flightData);
    }

    class AltimeterFlightDataAdapter : IAltimeterFlightDataAdapter
    {
        public void Adapt(IF16Altimeter altimeter, FlightData flightData)
        {
            var altbits = (AltBits)flightData.altBits;
            AdaptAltimeter(altimeter, flightData, altbits);
        }

       
        private static void AdaptAltimeter(IF16Altimeter altimeter, FlightData fromFalcon, AltBits altbits)
        {
            altimeter.Options.PressureAltitudeUnits = ((altbits & AltBits.CalType) == AltBits.CalType)
                ? F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury
                : F16Altimeter.F16AltimeterOptions.PressureUnits.Millibars;

            altimeter.InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.aauz;
            altimeter.InstrumentState.BarometricPressure = fromFalcon.AltCalReading;
            altimeter.InstrumentState.PneumaticModeFlag = ((altbits & AltBits.PneuFlag) == AltBits.PneuFlag);
            altimeter.InstrumentState.StandbyModeFlag = ((altbits & AltBits.PneuFlag) == AltBits.PneuFlag);
        }
    }
}
