using F4SharedMem;
using F4SharedMem.Headers;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IAltimeterFlightDataAdapter
    {
        void Adapt(IF16Altimeter altimeter, FlightData flightData, bool useBMSAdvancedSharedmemValues);
    }

    class AltimeterFlightDataAdapter : IAltimeterFlightDataAdapter
    {
        public void Adapt(IF16Altimeter altimeter, FlightData flightData, bool useBMSAdvancedSharedmemValues)
        {
            var altbits = (AltBits)flightData.altBits;
            if (flightData.DataFormat == FalconDataFormats.BMS4 && useBMSAdvancedSharedmemValues)
            {
                AdaptBMS4Altimeter(altimeter, flightData, altbits);
            }
            else
            {
                AdaptLegacyAltimeter(altimeter, flightData);
            }
        }

        private static void AdaptLegacyAltimeter(IF16Altimeter altimeter, FlightData fromFalcon)
        {
            altimeter.InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.z;
            altimeter.InstrumentState.BarometricPressure = 2992f;
            altimeter.InstrumentState.PneumaticModeFlag = false;
            altimeter.Options.PressureAltitudeUnits = F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury;
        }

        private static void AdaptBMS4Altimeter(IF16Altimeter altimeter, FlightData fromFalcon, AltBits altbits)
        {
            altimeter.Options.PressureAltitudeUnits = ((altbits & AltBits.CalType) == AltBits.CalType)
                ? F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury
                : F16Altimeter.F16AltimeterOptions.PressureUnits.Millibars;

            altimeter.InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.aauz;
            if (fromFalcon.VersionNum >= 111)
            {
                altimeter.InstrumentState.BarometricPressure = fromFalcon.AltCalReading;
                altimeter.InstrumentState.PneumaticModeFlag = ((altbits & AltBits.PneuFlag) == AltBits.PneuFlag);
            }
            else
            {
                altimeter.InstrumentState.BarometricPressure = 2992f;
                altimeter.InstrumentState.PneumaticModeFlag = false;
                altimeter.Options.PressureAltitudeUnits = F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury;
            }
        }
    }
}
