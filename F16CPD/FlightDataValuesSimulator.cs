namespace F16CPD
{
    internal sealed class FlightDataValuesSimulator
    {
        private int _timesCalled;
        private bool _valuesIncreasing = true;

        public FlightData GetNextFlightData()
        {
            return GetNextFlightData(null);
        }

        public FlightData GetInitialFlightData()
        {
            var flightData = new FlightData
                                 {
                                     AltimeterMode = AltimeterMode.Electronic,
                                     AutomaticLowAltitudeWarningInFeet = 300,
                                     BarometricPressureInDecimalInchesOfMercury = 29.92f,
                                     HsiDesiredCourseInDegrees = 0,
                                     HsiDesiredHeadingInDegrees = 0,
                                     HsiDeviationInvalidFlag = false,
                                     TransitionAltitudeInFeet = 18000,
                                     RateOfTurnInDecimalDegreesPerSecond = 0,
                                     VviOffFlag = false,
                                     AoaOffFlag = false,
                                     HsiOffFlag = false,
                                     AdiOffFlag = false,
                                     PfdOffFlag = false,
                                     RadarAltimeterOffFlag = false,
                                     CpdPowerOnFlag = true,
                                     MarkerBeaconOuterMarkerFlag = false,
                                     MarkerBeaconMiddleMarkerFlag = false,
                                     AdiEnableCommandBars = false,
                                     TacanChannel = "106X"
                                 };
            return flightData;
        }

        /// <summary>
        ///   Takes a <see cref = "FlightData" /> object containing inital data values to use,
        ///   and returns a new <see cref = "FlightData" /> object containing new values overwriting the
        ///   values passed in last time
        /// </summary>
        /// <param name = "toUpdate">an existing <see cref = "FlightData" /> object to update with simulated parameters</param>
        /// <returns>a new <see cref = "FlightData" /> object based on the parameters passed in, but with
        ///   incremented values for most or all variables</returns>
        public FlightData GetNextFlightData(FlightData toUpdate)
        {
            unchecked
            {
                _timesCalled++;
            }
            if (_timesCalled < 0) _timesCalled = 0;

            if ((_timesCalled%1000) == 0)
            {
                _valuesIncreasing = !_valuesIncreasing;
            }

            var toReturn = Common.Serialization.Util.DeepClone(toUpdate);

            toReturn.CpdPowerOnFlag = true;

            /*
            toReturn.TransitionAltitudeInFeet = toUpdate.TransitionAltitudeInFeet ;
            toReturn.TacanChannel = toUpdate.TacanChannel;
            */

            if (_valuesIncreasing)
            {
                //position and heading
                toReturn.LatitudeInDecimalDegrees += 1.00f/60.00f;
                toReturn.LongitudeInDecimalDegrees += 1.00f/60.00f;
                toReturn.MagneticHeadingInDecimalDegrees += 0.1f;
                toReturn.MapCoordinateFeetNorth += 5000;
                toReturn.MapCoordinateFeetEast += 5000;

                //altitudes
                toReturn.TrueAltitudeAboveMeanSeaLevelInDecimalFeet += 100.1f;
                toReturn.AltitudeAboveGroundLevelInDecimalFeet += 100.1f;

                //basic speeds (airspeed/groundspeed/mach)
                toReturn.IndicatedAirspeedInDecimalFeetPerSecond += 10.1f;
                toReturn.TrueAirspeedInDecimalFeetPerSecond += 10.1f;
                toReturn.GroundSpeedInDecimalFeetPerSecond += 10.1f;
                toReturn.MachNumber += 0.12f;

                //basic rates
                toReturn.RateOfTurnInDecimalDegreesPerSecond += 0.1f;
                toReturn.VerticalVelocityInDecimalFeetPerSecond += 100.1f;

                //flight attitude/orientation/climb angles
                toReturn.PitchAngleInDecimalDegrees += 1.1f;
                toReturn.RollAngleInDecimalDegrees += 1.1f;
                toReturn.AngleOfAttackInDegrees += 0.1f;
                toReturn.BetaAngleInDecimalDegrees += 1.1f;
                toReturn.GammaAngleInDecimalDegrees += 1.1f;
                toReturn.WindOffsetToFlightPathMarkerInDecimalDegrees += 1.1f;

                //glideslope/localizer deviations
                toReturn.AdiIlsGlideslopeDeviationInDecimalDegrees += 0.1f;
                toReturn.AdiIlsLocalizerDeviationInDecimalDegrees += 0.1f;
                toReturn.HsiCourseDeviationInDecimalDegrees += 0.1f;
                toReturn.HsiLocalizerDeviationInDecimalDegrees += 0.1f;

                //HSI-specific
                toReturn.HsiDesiredCourseInDegrees += 1;
                toReturn.HsiDesiredHeadingInDegrees += 1;
                toReturn.HsiDistanceToBeaconInNauticalMiles += 0.1f;
                toReturn.HsiBearingToBeaconInDecimalDegrees += 0.1f;

                //radio channels
                toReturn.TacanChannel += 1;

                //indexes/ALOW/baro
                toReturn.AutomaticLowAltitudeWarningInFeet += 10;
                toReturn.BarometricPressureInDecimalInchesOfMercury += 0.01f;
                //toReturn.AltitudeIndexInFeet += 100;
            }
            else
            {
                //position and heading
                toReturn.LatitudeInDecimalDegrees -= 1.00f/60.00f;
                toReturn.LongitudeInDecimalDegrees -= 1.00f/60.00f;
                toReturn.MapCoordinateFeetNorth += 5000;
                toReturn.MapCoordinateFeetEast += 5000;
                toReturn.MagneticHeadingInDecimalDegrees -= 0.1f;

                //altitudes
                toReturn.TrueAltitudeAboveMeanSeaLevelInDecimalFeet -= 100.1f;
                toReturn.AltitudeAboveGroundLevelInDecimalFeet -= 100.1f;

                //basic speeds (airspeed/groundspeed/mach)
                toReturn.IndicatedAirspeedInDecimalFeetPerSecond -= 10.1f;
                toReturn.TrueAirspeedInDecimalFeetPerSecond -= 10.1f;
                toReturn.GroundSpeedInDecimalFeetPerSecond -= 10.1f;
                toReturn.MachNumber -= 0.12f;

                //basic rates
                toReturn.RateOfTurnInDecimalDegreesPerSecond -= 0.1f;
                toReturn.VerticalVelocityInDecimalFeetPerSecond -= 100.1f;

                //flight attitude/orientation/climb angles
                toReturn.PitchAngleInDecimalDegrees -= 1.1f;
                toReturn.RollAngleInDecimalDegrees -= 1.1f;
                toReturn.AngleOfAttackInDegrees -= 0.1f;
                toReturn.BetaAngleInDecimalDegrees -= 1.1f;
                toReturn.GammaAngleInDecimalDegrees -= 1.1f;
                toReturn.WindOffsetToFlightPathMarkerInDecimalDegrees -= 1.1f;

                //glideslope/localizer deviations
                toReturn.AdiIlsGlideslopeDeviationInDecimalDegrees -= 0.1f;
                toReturn.AdiIlsLocalizerDeviationInDecimalDegrees -= 0.1f;
                toReturn.HsiCourseDeviationInDecimalDegrees -= 0.1f;
                toReturn.HsiLocalizerDeviationInDecimalDegrees -= 0.1f;

                //HSI-specific
                toReturn.HsiDesiredCourseInDegrees -= 1;
                toReturn.HsiDesiredHeadingInDegrees -= 1;
                toReturn.HsiDistanceToBeaconInNauticalMiles -= 0.1f;
                toReturn.HsiBearingToBeaconInDecimalDegrees -= 0.1f;

                //indexes/ALOW/baro
                toReturn.AutomaticLowAltitudeWarningInFeet -= 10;
                toReturn.BarometricPressureInDecimalInchesOfMercury -= 0.01f;
                //toReturn.AltitudeIndexInFeet -= 100;
            }

            if (_timesCalled%50 == 0)
            {
                //dual-valued enumerations
                toReturn.AltimeterMode = toReturn.AltimeterMode == AltimeterMode.Electronic
                                             ? AltimeterMode.Pneumatic
                                             : AltimeterMode.Electronic;

                //boolean values/flags
                toReturn.HsiDisplayToFromFlag = !toUpdate.HsiDisplayToFromFlag;
                toReturn.AdiEnableCommandBars = !toUpdate.AdiEnableCommandBars;
                toReturn.HsiDeviationInvalidFlag = !toUpdate.HsiDeviationInvalidFlag;
                toReturn.AdiGlideslopeInvalidFlag = !toUpdate.AdiGlideslopeInvalidFlag;
                toReturn.AdiLocalizerInvalidFlag = !toUpdate.AdiLocalizerInvalidFlag;
                toReturn.AdiAuxFlag = !toUpdate.AdiAuxFlag;
                //toReturn.HsiOffFlag = !toUpdate.HsiOffFlag;
                toReturn.AdiOffFlag = !toUpdate.AdiOffFlag;
                toReturn.RadarAltimeterOffFlag = !toUpdate.RadarAltimeterOffFlag;
                toReturn.AoaOffFlag = !toUpdate.AoaOffFlag;
                toReturn.VviOffFlag = !toUpdate.VviOffFlag;
                //toReturn.PfdOffFlag = !toUpdate.PfdOffFlag;
                toReturn.MarkerBeaconOuterMarkerFlag = !toUpdate.MarkerBeaconOuterMarkerFlag;
                toReturn.MarkerBeaconMiddleMarkerFlag = !toUpdate.MarkerBeaconMiddleMarkerFlag;
            }

            return toReturn;
        }
    }
}