using Common.Math;
using F4SharedMem;
using F4SharedMem.Headers;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IISISFlightDataAdapter
    {
        void Adapt(IF16ISIS isis, FlightData flightData, bool useBMSAdvancedSharedmemValues);
    }
    class ISISFlightDataAdapter:IISISFlightDataAdapter
    {
        public void Adapt(IF16ISIS isis, FlightData flightData, bool useBMSAdvancedSharedmemValues)
        {
            var altbits = (AltBits) flightData.altBits;
            var hsibits = (HsiBits) flightData.hsiBits;
            isis.InstrumentState.AirspeedKnots = flightData.kias;

            if (flightData.DataFormat == FalconDataFormats.BMS4 && useBMSAdvancedSharedmemValues)
            {
                isis.InstrumentState.IndicatedAltitudeFeetMSL = -flightData.aauz;
                if (flightData.VersionNum >= 111)
                {
                    if (((altbits & AltBits.CalType) == AltBits.CalType))
                    {
                        isis.Options.PressureAltitudeUnits =F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury;
                    }
                    else
                    {
                        isis.Options.PressureAltitudeUnits =F16ISIS.F16ISISOptions.PressureUnits.Millibars;
                    }

                    isis.InstrumentState.BarometricPressure = flightData.AltCalReading;
                }
                else
                {
                    isis.InstrumentState.BarometricPressure = 2992f;
                    isis.Options.PressureAltitudeUnits =F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury; 
                }
            }
            else
            {
                isis.InstrumentState.IndicatedAltitudeFeetMSL = -flightData.z;
                isis.InstrumentState.BarometricPressure = 2992f;
                isis.Options.PressureAltitudeUnits =F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury;
            }
            if (flightData.ExtensionData != null)
            {
                isis.InstrumentState.RadarAltitudeAGL = ((FlightDataExtension)flightData.ExtensionData).RadarAltitudeFeetAGL;
            }
            isis.InstrumentState.MachNumber = flightData.mach;
            isis.InstrumentState.MagneticHeadingDegrees = (360 +(flightData.yaw/Constants.RADIANS_PER_DEGREE))%360;
            isis.InstrumentState.NeverExceedSpeedKnots = 850;
            isis.InstrumentState.PitchDegrees = ((flightData.pitch/Constants.RADIANS_PER_DEGREE));
            isis.InstrumentState.RollDegrees = ((flightData.roll/Constants.RADIANS_PER_DEGREE));
            isis.InstrumentState.VerticalVelocityFeetPerMinute = -flightData.zDot*60.0f;
            isis.InstrumentState.OffFlag = ((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF);
            isis.InstrumentState.AuxFlag = ((hsibits & HsiBits.ADI_AUX) == HsiBits.ADI_AUX);
            isis.InstrumentState.GlideslopeFlag = ((hsibits & HsiBits.ADI_GS) == HsiBits.ADI_GS);
            isis.InstrumentState.LocalizerFlag = ((hsibits & HsiBits.ADI_LOC) ==HsiBits.ADI_LOC);
        }
    }
}
