using System;
using System.Text;
using Common.Math;
using Common.Networking;
using F4SharedMem;
using F4SharedMem.Headers;
using F4Utils.SimSupport;
using LightningGauges.Renderers;

namespace MFDExtractor
{
    internal interface IFlightDataUpdater
    {
        void UpdateRendererStatesFromFlightData(
            IInstrumentRendererSet renderers,
            FlightData flightData,
            bool simRunning,
            bool useBMSAdvancedSharedmemValues,
            Action UpdateEHSIBrightnessLabelVisibility,
            NetworkMode networkMode);
    }

    internal class FlightDataUpdater : IFlightDataUpdater
    {
        public void UpdateRendererStatesFromFlightData(
            IInstrumentRendererSet renderers, 
            FlightData flightData, 
            bool simRunning, 
            bool useBMSAdvancedSharedmemValues, 
            Action UpdateEHSIBrightnessLabelVisibility,
            NetworkMode networkMode)
        {
            if (flightData == null || (networkMode != NetworkMode.Client && !simRunning))
            {
                flightData = new FlightData {hsiBits = Int32.MaxValue};
            }

            var extensionData = ((FlightDataExtension)flightData.ExtensionData);

            if (simRunning || networkMode == NetworkMode.Client)
            {
                var hsibits = ((HsiBits)flightData.hsiBits);
                var altbits = ((AltBits)flightData.altBits); //12-08-12 added by Falcas 

                UpdateISIS(renderers, useBMSAdvancedSharedmemValues, flightData, altbits, extensionData, hsibits);
                UpdateVVI(renderers, hsibits, flightData);
                UpdateAltimeter(renderers, useBMSAdvancedSharedmemValues, flightData, altbits);
                UpdateAirspeedIndicator(renderers, flightData);
                UpdateCompass(renderers, flightData);
                UpdateAOAIndicator(renderers, hsibits, flightData);
                UpdateAOAIndexer(renderers, flightData);
                UpdateADI(renderers, hsibits);
                UpdateBackupADI(renderers, hsibits, flightData);
                UpdateHSI(renderers, hsibits, flightData);

               

                //***** UPDATE SOME COMPLEX HSI/ADI VARIABLES
                if (ADIIsTurnedOff(hsibits))
                {
                    SetADIToOffState(renderers);
                    SetISISToOffState(renderers);
                }
                else
                {
                    SetADIPitchAndRoll(renderers, flightData);
                    SetISISPitchAndRoll(renderers, flightData);

                    //The following floating data is also crossed up in the flightData.h File:
                    //float AdiIlsHorPos;       // Position of horizontal ILS bar ----Vertical
                    //float AdiIlsVerPos;       // Position of vertical ILS bar-----horizontal
                    var commandBarsOn = ((float)(Math.Abs(Math.Round(flightData.AdiIlsHorPos, 4))) != 0.1745f);
                    if ((Math.Abs((flightData.AdiIlsVerPos / Constants.RADIANS_PER_DEGREE)) > 1.0f)
                            ||
                        (Math.Abs((flightData.AdiIlsHorPos / Constants.RADIANS_PER_DEGREE)) > 5.0f))
                    {
                        commandBarsOn = false;
                    }
                    ((F16HorizontalSituationIndicator)renderers.HSI).InstrumentState.ShowToFromFlag = true;
                    ((F16EHSI)renderers.EHSI).InstrumentState.ShowToFromFlag = true;

                    //if the TOTALFLAGS flag is off, then we're most likely in NAV mode
                    if ((hsibits & HsiBits.TotalFlags) != HsiBits.TotalFlags)
                    {
                        ((F16HorizontalSituationIndicator)renderers.HSI).InstrumentState.ShowToFromFlag = false;
                        ((F16EHSI)renderers.EHSI).InstrumentState.ShowToFromFlag = false;
                    }
                    //if the TO/FROM flag is showing in shared memory, then we are most likely in TACAN mode (except in F4AF which always has the bit turned on)
                    else if ((((hsibits & HsiBits.ToTrue) == HsiBits.ToTrue)
                                ||
                            ((hsibits & HsiBits.FromTrue) == HsiBits.FromTrue)))
                    {
                        if (!commandBarsOn) //better make sure we're not in any ILS mode too though
                        {
                            ((F16HorizontalSituationIndicator)renderers.HSI).InstrumentState.ShowToFromFlag = true;
                            ((F16EHSI)renderers.EHSI).InstrumentState.ShowToFromFlag = true;
                        }
                    }

                    //if the glideslope or localizer flags on the ADI are turned on, then we must be in an ILS mode and therefore we 
                    //know we don't need to show the HSI TO/FROM flags.
                    if (((hsibits & HsiBits.ADI_GS) == HsiBits.ADI_GS)
                            ||
                        ((hsibits & HsiBits.ADI_LOC) == HsiBits.ADI_LOC))
                    {
                        ((F16HorizontalSituationIndicator)renderers.HSI).InstrumentState.ShowToFromFlag = false;
                        ((F16EHSI)renderers.EHSI).InstrumentState.ShowToFromFlag = false;
                    }
                    if (commandBarsOn)
                    {
                        ((F16HorizontalSituationIndicator)renderers.HSI).InstrumentState.ShowToFromFlag = false;
                        ((F16EHSI)renderers.EHSI).InstrumentState.ShowToFromFlag = false;
                    }

                    ((F16ADI)renderers.ADI).InstrumentState.ShowCommandBars = commandBarsOn;
                    ((F16ADI)renderers.ADI).InstrumentState.GlideslopeDeviationDegrees = flightData.AdiIlsVerPos /Constants.RADIANS_PER_DEGREE;
                    ((F16ADI)renderers.ADI).InstrumentState.LocalizerDeviationDegrees = flightData.AdiIlsHorPos /Constants.RADIANS_PER_DEGREE;

                    ((F16ISIS)renderers.ISIS).InstrumentState.ShowCommandBars = commandBarsOn;
                    ((F16ISIS)renderers.ISIS).InstrumentState.GlideslopeDeviationDegrees = flightData.AdiIlsVerPos /Constants.RADIANS_PER_DEGREE;
                    ((F16ISIS)renderers.ISIS).InstrumentState.LocalizerDeviationDegrees = flightData.AdiIlsHorPos /Constants.RADIANS_PER_DEGREE;
                }

                UpdateNavigationMode(renderers, flightData, useBMSAdvancedSharedmemValues);
                if (((hsibits & HsiBits.HSI_OFF) == HsiBits.HSI_OFF))
                {
                    TurnOffHSI(renderers);
                    TurnOffEHSI(renderers);
                }
                else
                {
                    UpdateHSIFlightData(renderers, flightData, hsibits);
                    UpdateEHSIFlightData(renderers, flightData, hsibits);
                }

                UpdateHSIAndEHSICourseDeviationAndToFromFlags(renderers);
                UpdateEHSI(UpdateEHSIBrightnessLabelVisibility);
                UpdateHYDAandHYDB(renderers, flightData);
                UpdateCabinPress(renderers, flightData);
                UpdateRollTrim(renderers, flightData);
                UpdatePitchTrim(renderers, flightData);
                UpdateRWR(renderers, flightData);
                UpdateCautionPanel(renderers, flightData);
                UpdateCMDS(renderers, flightData);
                UpdateDED(renderers, flightData);
                UpdatePFL(renderers, flightData);
                UpdateEPUFuel(renderers, flightData);
                UpdateFuelFlow(renderers, flightData);
                UpdateFuelQTY(renderers, flightData);
                UpdateLandingGearLights(renderers, flightData);
                UpdateNWS(renderers, flightData);
                UpdateSpeedbrake(renderers, flightData);
                UpdateRPM1(renderers, flightData);
                UpdateRPM2(renderers, flightData);
                UpdateFTIT1andFTIT2(renderers, flightData);
                UpdateNOZ1andNOZ2(renderers, flightData);
                UpdateOIL1(renderers, flightData);
                UpdateOIL2(renderers, flightData);
                UpdateAccelerometer(renderers, flightData);
            }
            else //Falcon's not running
            {
                if (renderers.VVI is F16VerticalVelocityIndicatorEU)
                {
                    ((F16VerticalVelocityIndicatorEU)renderers.VVI).InstrumentState.OffFlag = true;
                }
                else if (renderers.VVI is F16VerticalVelocityIndicatorUSA)
                {
                    ((F16VerticalVelocityIndicatorUSA)renderers.VVI).InstrumentState.OffFlag = true;
                }
                ((F16AngleOfAttackIndicator)renderers.AOAIndicator).InstrumentState.OffFlag = true;
                ((F16HorizontalSituationIndicator)renderers.HSI).InstrumentState.OffFlag = true;
                ((F16EHSI)renderers.EHSI).InstrumentState.NoDataFlag = true;
                ((F16ADI)renderers.ADI).InstrumentState.OffFlag = true;
                ((F16StandbyADI)renderers.BackupADI).InstrumentState.OffFlag = true;
                ((F16AzimuthIndicator)renderers.RWR).InstrumentState.RWRPowerOn = false;
                ((F16ISIS)renderers.ISIS).InstrumentState.RadarAltitudeAGL = 0;
                ((F16ISIS)renderers.ISIS).InstrumentState.OffFlag = true;
                UpdateEHSIBrightnessLabelVisibility();
            }
        }

        private static void SetISISPitchAndRoll(IInstrumentRendererSet renderers, FlightData flightData)
        {
            ((F16ISIS) renderers.ISIS).InstrumentState.PitchDegrees = ((flightData.pitch/Constants.RADIANS_PER_DEGREE));
            ((F16ISIS) renderers.ISIS).InstrumentState.RollDegrees = ((flightData.roll/Constants.RADIANS_PER_DEGREE));
        }

        private static void SetADIPitchAndRoll(IInstrumentRendererSet renderers, FlightData flightData)
        {
            ((F16ADI) renderers.ADI).InstrumentState.PitchDegrees = ((flightData.pitch/Constants.RADIANS_PER_DEGREE));
            ((F16ADI) renderers.ADI).InstrumentState.RollDegrees = ((flightData.roll/Constants.RADIANS_PER_DEGREE));
        }

        private static bool ADIIsTurnedOff(HsiBits hsibits)
        {
            return ((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF);
        }

        private static void SetISISToOffState(IInstrumentRendererSet renderers)
        {
            ((F16ISIS) renderers.ISIS).InstrumentState.PitchDegrees = 0;
            ((F16ISIS) renderers.ISIS).InstrumentState.RollDegrees = 0;
            ((F16ISIS) renderers.ISIS).InstrumentState.GlideslopeDeviationDegrees = 0;
            ((F16ISIS) renderers.ISIS).InstrumentState.LocalizerDeviationDegrees = 0;
            ((F16ISIS) renderers.ISIS).InstrumentState.ShowCommandBars = false;
        }

        private static void SetADIToOffState(IInstrumentRendererSet renderers)
        {
            ((F16ADI) renderers.ADI).InstrumentState.PitchDegrees = 0;
            ((F16ADI) renderers.ADI).InstrumentState.RollDegrees = 0;
            ((F16ADI) renderers.ADI).InstrumentState.GlideslopeDeviationDegrees = 0;
            ((F16ADI) renderers.ADI).InstrumentState.LocalizerDeviationDegrees = 0;
            ((F16ADI) renderers.ADI).InstrumentState.ShowCommandBars = false;
        }

        private static void UpdateNavigationMode(IInstrumentRendererSet renderers, FlightData flightData,
            bool useBMSAdvancedSharedmemValues)
        {
            if (flightData.DataFormat == FalconDataFormats.BMS4 && useBMSAdvancedSharedmemValues)
            {
                /*
                    This value is called navMode and is unsigned char type with 4 possible values: ILS_TACAN = 0, and TACAN = 1,
                    NAV = 2, ILS_NAV = 3
                    */

                byte bmsNavMode = flightData.navMode;
                switch (bmsNavMode)
                {
                    case 0: //NavModes.PlsTcn:
                        ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.ShowToFromFlag = false;
                        ((F16EHSI) renderers.EHSI).InstrumentState.ShowToFromFlag = false;
                        ((F16EHSI) renderers.EHSI).InstrumentState.InstrumentMode =
                            F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsTacan;
                        break;
                    case 1: //NavModes.Tcn:
                        ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.ShowToFromFlag = true;
                        ((F16EHSI) renderers.EHSI).InstrumentState.ShowToFromFlag = true;
                        ((F16EHSI) renderers.EHSI).InstrumentState.InstrumentMode =
                            F16EHSI.F16EHSIInstrumentState.InstrumentModes.Tacan;
                        ((F16ADI) renderers.ADI).InstrumentState.ShowCommandBars = false;
                        ((F16ISIS) renderers.ISIS).InstrumentState.ShowCommandBars = false;
                        break;
                    case 2: //NavModes.Nav:
                        ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.ShowToFromFlag = false;
                        ((F16EHSI) renderers.EHSI).InstrumentState.ShowToFromFlag = false;
                        ((F16EHSI) renderers.EHSI).InstrumentState.InstrumentMode =
                            F16EHSI.F16EHSIInstrumentState.InstrumentModes.Nav;
                        ((F16ADI) renderers.ADI).InstrumentState.ShowCommandBars = false;
                        ((F16ISIS) renderers.ISIS).InstrumentState.ShowCommandBars = false;
                        break;
                    case 3: //NavModes.PlsNav:
                        ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.ShowToFromFlag = false;
                        ((F16EHSI) renderers.EHSI).InstrumentState.ShowToFromFlag = false;
                        ((F16EHSI) renderers.EHSI).InstrumentState.InstrumentMode =
                            F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsNav;
                        break;
                }
            }
            else
            {
                ((F16EHSI) renderers.EHSI).InstrumentState.InstrumentMode =
                    F16EHSI.F16EHSIInstrumentState.InstrumentModes.Unknown;
            }
        }

        private static void UpdateHSIAndEHSICourseDeviationAndToFromFlags(IInstrumentRendererSet renderers)
        {
            var deviationLimitDecimalDegrees =
                ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.CourseDeviationLimitDegrees%180;
            var courseDeviationDecimalDegrees =
                ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.CourseDeviationDegrees;
            bool toFlag;
            bool fromFlag;
            if (Math.Abs(courseDeviationDecimalDegrees) <= 90)
            {
                toFlag = true;
                fromFlag = false;
            }
            else
            {
                toFlag = false;
                fromFlag = true;
            }

            if (courseDeviationDecimalDegrees < -90)
            {
                courseDeviationDecimalDegrees = Util.AngleDelta(Math.Abs(courseDeviationDecimalDegrees), 180)%180;
            }
            else if (courseDeviationDecimalDegrees > 90)
            {
                courseDeviationDecimalDegrees = -Util.AngleDelta(courseDeviationDecimalDegrees, 180)%180;
            }
            else
            {
                courseDeviationDecimalDegrees = -courseDeviationDecimalDegrees;
            }
            if (Math.Abs(courseDeviationDecimalDegrees) > deviationLimitDecimalDegrees)
            {
                courseDeviationDecimalDegrees = Math.Sign(courseDeviationDecimalDegrees)*deviationLimitDecimalDegrees;
            }

            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.CourseDeviationDegrees =
                courseDeviationDecimalDegrees;
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.ToFlag = toFlag;
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.FromFlag = fromFlag;
            ((F16EHSI) renderers.EHSI).InstrumentState.CourseDeviationDegrees = courseDeviationDecimalDegrees;
            ((F16EHSI) renderers.EHSI).InstrumentState.ToFlag = toFlag;
            ((F16EHSI) renderers.EHSI).InstrumentState.FromFlag = fromFlag;
        }

        private static void UpdateEHSIFlightData(IInstrumentRendererSet renderers, FlightData flightData, HsiBits hsibits)
        {
            ((F16EHSI) renderers.EHSI).InstrumentState.DmeInvalidFlag = ((hsibits & HsiBits.CourseWarning) ==
                HsiBits.CourseWarning);
            ((F16EHSI) renderers.EHSI).InstrumentState.DeviationInvalidFlag = ((hsibits & HsiBits.IlsWarning) ==
                HsiBits.IlsWarning);
            ((F16EHSI) renderers.EHSI).InstrumentState.CourseDeviationLimitDegrees = flightData.deviationLimit;
            ((F16EHSI) renderers.EHSI).InstrumentState.CourseDeviationDegrees = flightData.courseDeviation;
            ((F16EHSI) renderers.EHSI).InstrumentState.DesiredCourseDegrees = (int) flightData.desiredCourse;
            ((F16EHSI) renderers.EHSI).InstrumentState.DesiredHeadingDegrees = (int) flightData.desiredHeading;
            ((F16EHSI) renderers.EHSI).InstrumentState.BearingToBeaconDegrees = flightData.bearingToBeacon;
            ((F16EHSI) renderers.EHSI).InstrumentState.DistanceToBeaconNauticalMiles = flightData.distanceToBeacon;
        }

        private static void UpdateHSIFlightData(IInstrumentRendererSet renderers, FlightData flightData, HsiBits hsibits)
        {
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.DmeInvalidFlag = ((hsibits & HsiBits.CourseWarning) ==
                HsiBits.CourseWarning);
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.DeviationInvalidFlag = ((hsibits &
                HsiBits.IlsWarning) == HsiBits.IlsWarning);
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.CourseDeviationLimitDegrees =
                flightData.deviationLimit;
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.CourseDeviationDegrees =
                flightData.courseDeviation;
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.DesiredCourseDegrees =
                (int) flightData.desiredCourse;
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.DesiredHeadingDegrees =
                (int) flightData.desiredHeading;
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.BearingToBeaconDegrees =
                flightData.bearingToBeacon;
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.DistanceToBeaconNauticalMiles =
                flightData.distanceToBeacon;
        }

        private static void TurnOffEHSI(IInstrumentRendererSet renderers)
        {
            ((F16EHSI) renderers.EHSI).InstrumentState.DmeInvalidFlag = true;
            ((F16EHSI) renderers.EHSI).InstrumentState.DeviationInvalidFlag = false;
            ((F16EHSI) renderers.EHSI).InstrumentState.CourseDeviationLimitDegrees = 0;
            ((F16EHSI) renderers.EHSI).InstrumentState.CourseDeviationDegrees = 0;
            ((F16EHSI) renderers.EHSI).InstrumentState.BearingToBeaconDegrees = 0;
            ((F16EHSI) renderers.EHSI).InstrumentState.DistanceToBeaconNauticalMiles = 0;
        }

        private static void TurnOffHSI(IInstrumentRendererSet renderers)
        {
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.DmeInvalidFlag = true;
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.DeviationInvalidFlag = false;
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.CourseDeviationLimitDegrees = 0;
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.CourseDeviationDegrees = 0;
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.BearingToBeaconDegrees = 0;
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.DistanceToBeaconNauticalMiles = 0;
        }

        private static void UpdateAccelerometer(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            var gs = fromFalcon.gs;
            if (gs == 0) //ignore exactly zero g's
            {
                gs = 1;
            }
            ((F16Accelerometer) renderers.Accelerometer).InstrumentState.AccelerationInGs = gs;
        }

        private static void UpdateOIL2(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            ((F16OilPressureGauge) renderers.OIL2).InstrumentState.OilPressurePercent = fromFalcon.oilPressure2;
        }

        private static void UpdateOIL1(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            ((F16OilPressureGauge) renderers.OIL1).InstrumentState.OilPressurePercent = fromFalcon.oilPressure;
        }

        private static void UpdateNOZ1andNOZ2(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            if (fromFalcon.DataFormat == FalconDataFormats.OpenFalcon)
            {
                //NOZ is hosed in OF
                //** UPDATE NOZ1
                ((F16NozzlePositionIndicator) renderers.NOZ1).InstrumentState.NozzlePositionPercent =
                    NonImplementedGaugeCalculations.NOZ(fromFalcon.rpm, fromFalcon.z, fromFalcon.fuelFlow);
                //******************
                //** UPDATE NOZ2
                ((F16NozzlePositionIndicator) renderers.NOZ2).InstrumentState.NozzlePositionPercent =
                    NonImplementedGaugeCalculations.NOZ(fromFalcon.rpm2, fromFalcon.z, fromFalcon.fuelFlow);
                //******************
            }
            else if (fromFalcon.DataFormat == FalconDataFormats.BMS4)
            {
                //** UPDATE NOZ1
                ((F16NozzlePositionIndicator) renderers.NOZ1).InstrumentState.NozzlePositionPercent =
                    fromFalcon.nozzlePos*100.0f;
                //******************
                //** UPDATE NOZ2
                ((F16NozzlePositionIndicator) renderers.NOZ2).InstrumentState.NozzlePositionPercent =
                    fromFalcon.nozzlePos2*100.0f;
                //******************
            }
            else
            {
                //NOZ is OK in AF, RedViper, FF5
                //** UPDATE NOZ1
                ((F16NozzlePositionIndicator) renderers.NOZ1).InstrumentState.NozzlePositionPercent =
                    fromFalcon.nozzlePos;
                //******************
                //** UPDATE NOZ2
                ((F16NozzlePositionIndicator) renderers.NOZ2).InstrumentState.NozzlePositionPercent =
                    fromFalcon.nozzlePos2;
                //******************
            }
        }

        private static void UpdateFTIT1andFTIT2(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            if (fromFalcon.DataFormat == FalconDataFormats.BMS4)
            {
                //Only BMS4 has a valid FTIT value in sharedmem
                //** UPDATE FTIT1
                ((F16FanTurbineInletTemperature) renderers.FTIT1).InstrumentState.InletTemperatureDegreesCelcius =
                    fromFalcon.ftit*100.0f;
                //******************
                //** UPDATE FTIT2
                ((F16FanTurbineInletTemperature) renderers.FTIT2).InstrumentState.InletTemperatureDegreesCelcius =
                    fromFalcon.ftit2*100.0f;
                //******************
            }
            else
            {
                //FTIT is hosed in AF, RedViper, FF5, OF
                //** UPDATE FTIT1
                ((F16FanTurbineInletTemperature) renderers.FTIT1).InstrumentState.InletTemperatureDegreesCelcius =
                    NonImplementedGaugeCalculations.Ftit(
                        ((F16FanTurbineInletTemperature) renderers.FTIT1).InstrumentState
                            .InletTemperatureDegreesCelcius,
                        fromFalcon.rpm);
                //******************
                //** UPDATE FTIT2
                ((F16FanTurbineInletTemperature) renderers.FTIT2).InstrumentState.InletTemperatureDegreesCelcius =
                    NonImplementedGaugeCalculations.Ftit(
                        ((F16FanTurbineInletTemperature) renderers.FTIT2).InstrumentState
                            .InletTemperatureDegreesCelcius,
                        fromFalcon.rpm2);
                //******************
            }
        }

        private static void UpdateRPM2(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            ((F16Tachometer) renderers.RPM2).InstrumentState.RPMPercent = fromFalcon.rpm2;
        }

        private static void UpdateRPM1(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            ((F16Tachometer) renderers.RPM1).InstrumentState.RPMPercent = fromFalcon.rpm;
        }

        private static void UpdateSpeedbrake(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            ((F16SpeedbrakeIndicator) renderers.Speedbrake).InstrumentState.PercentOpen = fromFalcon.speedBrake*
                100.0f;

            if (fromFalcon.DataFormat == FalconDataFormats.BMS4)
            {
                ((F16SpeedbrakeIndicator) renderers.Speedbrake).InstrumentState.PowerLoss = ((fromFalcon.lightBits3 &
                    (int)Bms4LightBits3.Power_Off) == (int)Bms4LightBits3.Power_Off);
            }
            else
            {
                ((F16SpeedbrakeIndicator) renderers.Speedbrake).InstrumentState.PowerLoss = ((fromFalcon.lightBits3 &
                    (int) LightBits3.Power_Off) == (int) LightBits3.Power_Off);
            }
        }

        private static void UpdateNWS(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            ((F16NosewheelSteeringIndexer) renderers.NWSIndexer).InstrumentState.DISC = ((fromFalcon.lightBits &
                (int) LightBits.RefuelDSC) ==
                (int) LightBits.RefuelDSC);
            ((F16NosewheelSteeringIndexer) renderers.NWSIndexer).InstrumentState.AR_NWS = ((fromFalcon.lightBits &
                (int) LightBits.RefuelAR) ==
                (int) LightBits.RefuelAR);
            ((F16NosewheelSteeringIndexer) renderers.NWSIndexer).InstrumentState.RDY = ((fromFalcon.lightBits &
                (int) LightBits.RefuelRDY) ==
                (int) LightBits.RefuelRDY);
        }

        private static void UpdateLandingGearLights(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            if (fromFalcon.DataFormat == FalconDataFormats.OpenFalcon ||
                fromFalcon.DataFormat == FalconDataFormats.BMS4)
            {
                ((F16LandingGearWheelsLights) renderers.LandingGearLights).InstrumentState.LeftGearDown =
                    ((fromFalcon.lightBits3 & (int) LightBits3.LeftGearDown) == (int) LightBits3.LeftGearDown);
                ((F16LandingGearWheelsLights) renderers.LandingGearLights).InstrumentState.NoseGearDown =
                    ((fromFalcon.lightBits3 & (int) LightBits3.NoseGearDown) == (int) LightBits3.NoseGearDown);
                ((F16LandingGearWheelsLights) renderers.LandingGearLights).InstrumentState.RightGearDown =
                    ((fromFalcon.lightBits3 & (int) LightBits3.RightGearDown) == (int) LightBits3.RightGearDown);
            }
            else
            {
                ((F16LandingGearWheelsLights) renderers.LandingGearLights).InstrumentState.LeftGearDown =
                    ((fromFalcon.LeftGearPos == 1.0f));
                ((F16LandingGearWheelsLights) renderers.LandingGearLights).InstrumentState.NoseGearDown =
                    ((fromFalcon.NoseGearPos == 1.0f));
                ((F16LandingGearWheelsLights) renderers.LandingGearLights).InstrumentState.RightGearDown =
                    ((fromFalcon.RightGearPos == 1.0f));
            }
        }

        private static void UpdateFuelQTY(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            ((F16FuelQuantityIndicator) renderers.FuelQuantity).InstrumentState.AftLeftFuelQuantityPounds =
                fromFalcon.aft/10.0f;
            ((F16FuelQuantityIndicator) renderers.FuelQuantity).InstrumentState.ForeRightFuelQuantityPounds =
                fromFalcon.fwd/10.0f;
            ((F16FuelQuantityIndicator) renderers.FuelQuantity).InstrumentState.TotalFuelQuantityPounds =
                fromFalcon.total;
        }

        private static void UpdateFuelFlow(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            ((F16FuelFlow) renderers.FuelFlow).InstrumentState.FuelFlowPoundsPerHour = fromFalcon.fuelFlow;
        }

        private static void UpdateEPUFuel(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            ((F16EPUFuelGauge) renderers.EPUFuel).InstrumentState.FuelRemainingPercent = fromFalcon.epuFuel;
        }

        private static void UpdatePFL(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            if (fromFalcon.PFLLines != null)
            {
                ((F16DataEntryDisplayPilotFaultList) renderers.PFL).InstrumentState.Line1 =
                    Encoding.Default.GetBytes(fromFalcon.PFLLines[0] ?? "");
                ((F16DataEntryDisplayPilotFaultList) renderers.PFL).InstrumentState.Line2 =
                    Encoding.Default.GetBytes(fromFalcon.PFLLines[1] ?? "");
                ((F16DataEntryDisplayPilotFaultList) renderers.PFL).InstrumentState.Line3 =
                    Encoding.Default.GetBytes(fromFalcon.PFLLines[2] ?? "");
                ((F16DataEntryDisplayPilotFaultList) renderers.PFL).InstrumentState.Line4 =
                    Encoding.Default.GetBytes(fromFalcon.PFLLines[3] ?? "");
                ((F16DataEntryDisplayPilotFaultList) renderers.PFL).InstrumentState.Line5 =
                    Encoding.Default.GetBytes(fromFalcon.PFLLines[4] ?? "");
            }
            if (fromFalcon.PFLInvert != null)
            {
                ((F16DataEntryDisplayPilotFaultList) renderers.PFL).InstrumentState.Line1Invert =
                    Encoding.Default.GetBytes(fromFalcon.PFLInvert[0] ?? "");
                ((F16DataEntryDisplayPilotFaultList) renderers.PFL).InstrumentState.Line2Invert =
                    Encoding.Default.GetBytes(fromFalcon.PFLInvert[1] ?? "");
                ((F16DataEntryDisplayPilotFaultList) renderers.PFL).InstrumentState.Line3Invert =
                    Encoding.Default.GetBytes(fromFalcon.PFLInvert[2] ?? "");
                ((F16DataEntryDisplayPilotFaultList) renderers.PFL).InstrumentState.Line4Invert =
                    Encoding.Default.GetBytes(fromFalcon.PFLInvert[3] ?? "");
                ((F16DataEntryDisplayPilotFaultList) renderers.PFL).InstrumentState.Line5Invert =
                    Encoding.Default.GetBytes(fromFalcon.PFLInvert[4] ?? "");
            }
        }

        private static void UpdateDED(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            if (fromFalcon.DEDLines != null)
            {
                ((F16DataEntryDisplayPilotFaultList) renderers.DED).InstrumentState.Line1 =
                    Encoding.Default.GetBytes(fromFalcon.DEDLines[0] ?? "");
                ((F16DataEntryDisplayPilotFaultList) renderers.DED).InstrumentState.Line2 =
                    Encoding.Default.GetBytes(fromFalcon.DEDLines[1] ?? "");
                ((F16DataEntryDisplayPilotFaultList) renderers.DED).InstrumentState.Line3 =
                    Encoding.Default.GetBytes(fromFalcon.DEDLines[2] ?? "");
                ((F16DataEntryDisplayPilotFaultList) renderers.DED).InstrumentState.Line4 =
                    Encoding.Default.GetBytes(fromFalcon.DEDLines[3] ?? "");
                ((F16DataEntryDisplayPilotFaultList) renderers.DED).InstrumentState.Line5 =
                    Encoding.Default.GetBytes(fromFalcon.DEDLines[4] ?? "");
            }
            if (fromFalcon.Invert != null)
            {
                ((F16DataEntryDisplayPilotFaultList) renderers.DED).InstrumentState.Line1Invert =
                    Encoding.Default.GetBytes(fromFalcon.Invert[0] ?? "");
                ((F16DataEntryDisplayPilotFaultList) renderers.DED).InstrumentState.Line2Invert =
                    Encoding.Default.GetBytes(fromFalcon.Invert[1] ?? "");
                ((F16DataEntryDisplayPilotFaultList) renderers.DED).InstrumentState.Line3Invert =
                    Encoding.Default.GetBytes(fromFalcon.Invert[2] ?? "");
                ((F16DataEntryDisplayPilotFaultList) renderers.DED).InstrumentState.Line4Invert =
                    Encoding.Default.GetBytes(fromFalcon.Invert[3] ?? "");
                ((F16DataEntryDisplayPilotFaultList) renderers.DED).InstrumentState.Line5Invert =
                    Encoding.Default.GetBytes(fromFalcon.Invert[4] ?? "");
            }
        }

        private static void UpdateCMDS(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            ((F16CMDSPanel) renderers.CMDSPanel).InstrumentState.Degraded = ((fromFalcon.lightBits2 &
                (int) LightBits2.Degr) ==
                (int) LightBits2.Degr);
            ((F16CMDSPanel) renderers.CMDSPanel).InstrumentState.ChaffCount = (int) fromFalcon.ChaffCount;
            ((F16CMDSPanel) renderers.CMDSPanel).InstrumentState.ChaffLow = ((fromFalcon.lightBits2 &
                (int) LightBits2.ChaffLo) ==
                (int) LightBits2.ChaffLo);
            ((F16CMDSPanel) renderers.CMDSPanel).InstrumentState.DispenseReady = ((fromFalcon.lightBits2 &
                (int) LightBits2.Rdy) ==
                (int) LightBits2.Rdy);
            ((F16CMDSPanel) renderers.CMDSPanel).InstrumentState.FlareCount = (int) fromFalcon.FlareCount;
            ((F16CMDSPanel) renderers.CMDSPanel).InstrumentState.FlareLow = ((fromFalcon.lightBits2 &
                (int) LightBits2.FlareLo) ==
                (int) LightBits2.FlareLo);
            ((F16CMDSPanel) renderers.CMDSPanel).InstrumentState.Go =
                (
                    ((fromFalcon.lightBits2 & (int) LightBits2.Go) == (int) LightBits2.Go)
                    //Falcas 04/09/2012 to match what you see in BMS
                    //    &&
                    //!(
                    //    ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.NoGo) == (int)F4SharedMem.Headers.LightBits2.NoGo)
                    //             ||
                    //    ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Degr) == (int)F4SharedMem.Headers.LightBits2.Degr)
                    //             ||
                    //    ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Rdy) == (int)F4SharedMem.Headers.LightBits2.Rdy)
                    //)
                    );

            ((F16CMDSPanel) renderers.CMDSPanel).InstrumentState.NoGo =
                (
                    ((fromFalcon.lightBits2 & (int) LightBits2.NoGo) == (int) LightBits2.NoGo)
                    //Falcas 04/09/2012 to match what you see in BMS
                    //         ||
                    //((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Degr) == (int)F4SharedMem.Headers.LightBits2.Degr)
                    );
            ((F16CMDSPanel) renderers.CMDSPanel).InstrumentState.Other1Count = 0;
            ((F16CMDSPanel) renderers.CMDSPanel).InstrumentState.Other1Low = true;
            ((F16CMDSPanel) renderers.CMDSPanel).InstrumentState.Other2Count = 0;
            ((F16CMDSPanel) renderers.CMDSPanel).InstrumentState.Other2Low = true;
        }

        private static void UpdateCautionPanel(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
//TODO: implement all-lights-on when test is detected
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.AftFuelLow = ((fromFalcon.lightBits2 &
                (int) LightBits2.AftFuelLow) ==
                (int) LightBits2.AftFuelLow);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.AntiSkid = ((fromFalcon.lightBits2 &
                (int) LightBits2.ANTI_SKID) ==
                (int) LightBits2.ANTI_SKID);
            //((F16CautionPanel)CautionPanel).InstrumentState.ATFNotEngaged = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.TFR_ENGAGED) == (int)F4SharedMem.Headers.LightBits2.TFR_ENGAGED);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.AvionicsFault = ((fromFalcon.lightBits &
                (int) LightBits.Avionics) ==
                (int) LightBits.Avionics);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.BUC = ((fromFalcon.lightBits2 &
                (int) LightBits2.BUC) ==
                (int) LightBits2.BUC);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.CabinPress = ((fromFalcon.lightBits &
                (int) LightBits.CabinPress) ==
                (int) LightBits.CabinPress);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.CADC = ((fromFalcon.lightBits3 &
                (int) Bms4LightBits3.cadc) ==
                (int) Bms4LightBits3.cadc);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.ECM = ((fromFalcon.lightBits &
                (int) LightBits.ECM) ==
                (int) LightBits.ECM);
            //((F16CautionPanel)renderers.CautionPanel).InstrumentState.EEC = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.ee) == (int)F4SharedMem.Headers.LightBits.ECM);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.ElecSys = ((fromFalcon.lightBits3 &
                (int) LightBits3.Elec_Fault) ==
                (int) LightBits3.Elec_Fault);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.EngineFault = ((fromFalcon.lightBits &
                (int) LightBits.EngineFault) ==
                (int) LightBits.EngineFault);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.EquipHot = ((fromFalcon.lightBits &
                (int) LightBits.EQUIP_HOT) ==
                (int) LightBits.EQUIP_HOT);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.FLCSFault = ((fromFalcon.lightBits &
                (int) LightBits.FltControlSys) ==
                (int) LightBits.FltControlSys);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.FuelOilHot = ((fromFalcon.lightBits2 &
                (int) LightBits2.FUEL_OIL_HOT) ==
                (int) LightBits2.FUEL_OIL_HOT);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.FwdFuelLow = ((fromFalcon.lightBits2 &
                (int) LightBits2.FwdFuelLow) ==
                (int) LightBits2.FwdFuelLow);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.Hook = ((fromFalcon.lightBits &
                (int) LightBits.Hook) ==
                (int) LightBits.Hook);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.IFF = ((fromFalcon.lightBits &
                (int) LightBits.IFF) ==
                (int) LightBits.IFF);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.NWSFail = ((fromFalcon.lightBits &
                (int) LightBits.NWSFail) ==
                (int) LightBits.NWSFail);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.Overheat = ((fromFalcon.lightBits &
                (int) LightBits.Overheat) ==
                (int) LightBits.Overheat);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.OxyLow = ((fromFalcon.lightBits2 &
                (int) LightBits2.OXY_LOW) ==
                (int) LightBits2.OXY_LOW);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.ProbeHeat = ((fromFalcon.lightBits2 &
                (int) LightBits2.PROBEHEAT) ==
                (int) LightBits2.PROBEHEAT);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.RadarAlt = ((fromFalcon.lightBits &
                (int) LightBits.RadarAlt) ==
                (int) LightBits.RadarAlt);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.SeatNotArmed = ((fromFalcon.lightBits2 &
                (int) LightBits2.SEAT_ARM) ==
                (int) LightBits2.SEAT_ARM);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.SEC = ((fromFalcon.lightBits2 &
                (int) LightBits2.SEC) ==
                (int) LightBits2.SEC);
            ((F16CautionPanel) renderers.CautionPanel).InstrumentState.StoresConfig = ((fromFalcon.lightBits &
                (int) LightBits.CONFIG) ==
                (int) LightBits.CONFIG);

            //TODO: implement MLU cautions
        }

        private static void UpdateRWR(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.MagneticHeadingDegrees = (360 + (fromFalcon.yaw/ Constants.RADIANS_PER_DEGREE))%360;
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.RollDegrees = ((fromFalcon.roll/Constants.RADIANS_PER_DEGREE));
            int rwrObjectCount = fromFalcon.RwrObjectCount;
            if (fromFalcon.RWRsymbol != null)
            {
                var blips = new F16AzimuthIndicator.F16AzimuthIndicatorInstrumentState.Blip[fromFalcon.RWRsymbol.Length];
                ((F16AzimuthIndicator) renderers.RWR).InstrumentState.Blips = blips;
                //for (int i = 0; i < rwrObjectCount; i++)
                if (fromFalcon.RWRsymbol != null)
                {
                    for (var i = 0; i < fromFalcon.RWRsymbol.Length; i++)
                    {
                        var thisBlip = new F16AzimuthIndicator.F16AzimuthIndicatorInstrumentState.Blip();
                        if (i < rwrObjectCount) thisBlip.Visible = true;
                        if (fromFalcon.bearing != null)
                        {
                            thisBlip.BearingDegrees = (fromFalcon.bearing[i]/Constants.RADIANS_PER_DEGREE);
                        }
                        if (fromFalcon.lethality != null)
                        {
                            thisBlip.Lethality = fromFalcon.lethality[i];
                        }
                        if (fromFalcon.missileActivity != null)
                        {
                            thisBlip.MissileActivity = fromFalcon.missileActivity[i];
                        }
                        if (fromFalcon.missileLaunch != null)
                        {
                            thisBlip.MissileLaunch = fromFalcon.missileLaunch[i];
                        }
                        if (fromFalcon.newDetection != null)
                        {
                            thisBlip.NewDetection = fromFalcon.newDetection[i];
                        }
                        if (fromFalcon.selected != null)
                        {
                            thisBlip.Selected = fromFalcon.selected[i];
                        }
                        thisBlip.SymbolID = fromFalcon.RWRsymbol[i];
                        blips[i] = thisBlip;
                    }
                }
            }
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.Activity = ((fromFalcon.lightBits2 & (int) LightBits2.AuxAct) == (int) LightBits2.AuxAct);
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.ChaffCount = (int) fromFalcon.ChaffCount;
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.ChaffLow = ((fromFalcon.lightBits2 & (int) LightBits2.ChaffLo) == (int) LightBits2.ChaffLo);
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.EWSDegraded = ((fromFalcon.lightBits2 & (int) LightBits2.Degr) == (int) LightBits2.Degr);
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.EWSDispenseReady = ((fromFalcon.lightBits2 & (int) LightBits2.Rdy) == (int) LightBits2.Rdy);
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.EWSNoGo = (
                ((fromFalcon.lightBits2 & (int) LightBits2.NoGo) == (int) LightBits2.NoGo)
                    ||
                ((fromFalcon.lightBits2 & (int) LightBits2.Degr) == (int) LightBits2.Degr)
                );
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.EWSGo =
                (
                    ((fromFalcon.lightBits2 & (int) LightBits2.Go) == (int) LightBits2.Go)
                        &&
                    !(
                        ((fromFalcon.lightBits2 & (int) LightBits2.NoGo) == (int) LightBits2.NoGo)
                            ||
                        ((fromFalcon.lightBits2 & (int) LightBits2.Degr) == (int) LightBits2.Degr)
                            ||
                        ((fromFalcon.lightBits2 & (int) LightBits2.Rdy) == (int) LightBits2.Rdy)
                        )
                    );


            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.FlareCount = (int) fromFalcon.FlareCount;
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.FlareLow = ((fromFalcon.lightBits2 &
                (int) LightBits2.FlareLo) == (int) LightBits2.FlareLo);
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.Handoff = ((fromFalcon.lightBits2 &
                (int) LightBits2.HandOff) == (int) LightBits2.HandOff);
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.Launch = ((fromFalcon.lightBits2 &
                (int) LightBits2.Launch) == (int) LightBits2.Launch);
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.LowAltitudeMode = ((fromFalcon.lightBits2 &
                (int) LightBits2.AuxLow) == (int) LightBits2.AuxLow);
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.NavalMode = ((fromFalcon.lightBits2 &
                (int) LightBits2.Naval) == (int) LightBits2.Naval);
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.Other1Count = 0;
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.Other1Low = true;
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.Other2Count = 0;
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.Other2Low = true;
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.RWRPowerOn = ((fromFalcon.lightBits2 &
                (int) LightBits2.AuxPwr) == (int) LightBits2.AuxPwr);
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.PriorityMode = ((fromFalcon.lightBits2 &
                (int) LightBits2.PriMode) == (int) LightBits2.PriMode);
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.SearchMode = ((fromFalcon.lightBits2 &
                (int) LightBits2.AuxSrch) == (int) LightBits2.AuxSrch);
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.SeparateMode = ((fromFalcon.lightBits2 &
                (int) LightBits2.TgtSep) == (int) LightBits2.TgtSep);
            ((F16AzimuthIndicator) renderers.RWR).InstrumentState.UnknownThreatScanMode = ((fromFalcon.lightBits2 &
                (int) LightBits2.Unk) == (int) LightBits2.Unk);
        }

        private static void UpdatePitchTrim(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            var pitchTrim = fromFalcon.TrimPitch;
            ((F16PitchTrimIndicator) renderers.PitchTrim).InstrumentState.PitchTrimPercent = pitchTrim*2.0f*100.0f;
        }

        private static void UpdateRollTrim(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            var rolltrim = fromFalcon.TrimRoll;
            ((F16RollTrimIndicator) renderers.RollTrim).InstrumentState.RollTrimPercent = rolltrim*2.0f*100.0f;
        }

        private static void UpdateCabinPress(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            var z = fromFalcon.z;
            var origCabinAlt = ((F16CabinPressureAltitudeIndicator) renderers.CabinPress).InstrumentState.CabinPressureAltitudeFeet;
            var pressurization = ((fromFalcon.lightBits & (int) LightBits.CabinPress) == (int) LightBits.CabinPress);
            ((F16CabinPressureAltitudeIndicator) renderers.CabinPress).InstrumentState.CabinPressureAltitudeFeet = 
                NonImplementedGaugeCalculations.CabinAlt(origCabinAlt, z, pressurization);
        }

        private static void UpdateHYDAandHYDB(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            var rpm = fromFalcon.rpm;
            var mainGen = ((fromFalcon.lightBits3 & (int) LightBits3.MainGen) == (int) LightBits3.MainGen);
            var stbyGen = ((fromFalcon.lightBits3 & (int) LightBits3.StbyGen) == (int) LightBits3.StbyGen);
            var epuGen = ((fromFalcon.lightBits3 & (int) LightBits3.EpuGen) == (int) LightBits3.EpuGen);
            var epuOn = ((fromFalcon.lightBits2 & (int) LightBits2.EPUOn) == (int) LightBits2.EPUOn);
            var epuFuel = fromFalcon.epuFuel;
            ((F16HydraulicPressureGauge) renderers.HYDA).InstrumentState.HydraulicPressurePoundsPerSquareInch =
                NonImplementedGaugeCalculations.HydA(rpm, mainGen, stbyGen, epuGen, epuOn, epuFuel);
            ((F16HydraulicPressureGauge) renderers.HYDB).InstrumentState.HydraulicPressurePoundsPerSquareInch =
                NonImplementedGaugeCalculations.HydB(rpm, mainGen, stbyGen, epuGen, epuOn, epuFuel);
        }

        private static void UpdateEHSI(Action UpdateEHSIBrightnessLabelVisibility)
        {
            UpdateEHSIBrightnessLabelVisibility();
        }

        private static void UpdateHSI(IInstrumentRendererSet renderers, HsiBits hsibits, FlightData fromFalcon)
        {
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.OffFlag = ((hsibits & HsiBits.HSI_OFF) ==
                HsiBits.HSI_OFF);
            ((F16HorizontalSituationIndicator) renderers.HSI).InstrumentState.MagneticHeadingDegrees = (360 +
                (fromFalcon
                    .yaw/
                    Constants
                        .RADIANS_PER_DEGREE))%
                360;
            ((F16EHSI) renderers.EHSI).InstrumentState.NoDataFlag = ((hsibits & HsiBits.HSI_OFF) == HsiBits.HSI_OFF);
            ((F16EHSI) renderers.EHSI).InstrumentState.MagneticHeadingDegrees = (360 +
                (fromFalcon.yaw/
                    Constants.RADIANS_PER_DEGREE))%360;
        }

        private static void UpdateBackupADI(IInstrumentRendererSet renderers, HsiBits hsibits, FlightData fromFalcon)
        {
            ((F16StandbyADI) renderers.BackupADI).InstrumentState.OffFlag = ((hsibits & HsiBits.BUP_ADI_OFF) == HsiBits.BUP_ADI_OFF);
            if (((hsibits & HsiBits.BUP_ADI_OFF) == HsiBits.BUP_ADI_OFF))
            {
                //if the standby ADI is off
                ((F16StandbyADI)renderers.BackupADI).InstrumentState.PitchDegrees = 0;
                ((F16StandbyADI)renderers.BackupADI).InstrumentState.RollDegrees = 0;
                ((F16StandbyADI)renderers.BackupADI).InstrumentState.OffFlag = true;
            }
            else
            {
                ((F16StandbyADI)renderers.BackupADI).InstrumentState.PitchDegrees = ((fromFalcon.pitch /
                                                                                      Constants.RADIANS_PER_DEGREE));
                ((F16StandbyADI)renderers.BackupADI).InstrumentState.RollDegrees = ((fromFalcon.roll /
                                                                                     Constants.RADIANS_PER_DEGREE));
                ((F16StandbyADI)renderers.BackupADI).InstrumentState.OffFlag = false;
            }

        }

        private static void UpdateADI(IInstrumentRendererSet renderers, HsiBits hsibits)
        {
            ((F16ADI) renderers.ADI).InstrumentState.OffFlag = ((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF);
            ((F16ADI) renderers.ADI).InstrumentState.AuxFlag = ((hsibits & HsiBits.ADI_AUX) == HsiBits.ADI_AUX);
            ((F16ADI) renderers.ADI).InstrumentState.GlideslopeFlag = ((hsibits & HsiBits.ADI_GS) == HsiBits.ADI_GS);
            ((F16ADI) renderers.ADI).InstrumentState.LocalizerFlag = ((hsibits & HsiBits.ADI_LOC) == HsiBits.ADI_LOC);
        }

        private static void UpdateAOAIndexer(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            ((F16AngleOfAttackIndexer) renderers.AOAIndexer).InstrumentState.AoaBelow = ((fromFalcon.lightBits & (int) LightBits.AOABelow) == (int) LightBits.AOABelow);
            ((F16AngleOfAttackIndexer) renderers.AOAIndexer).InstrumentState.AoaOn = ((fromFalcon.lightBits & (int) LightBits.AOAOn) == (int) LightBits.AOAOn);
            ((F16AngleOfAttackIndexer) renderers.AOAIndexer).InstrumentState.AoaAbove = ((fromFalcon.lightBits & (int) LightBits.AOAAbove) == (int) LightBits.AOAAbove);
        }

        private static void UpdateAOAIndicator(IInstrumentRendererSet renderers, HsiBits hsibits, FlightData fromFalcon)
        {
            if (((hsibits & HsiBits.AOA) == HsiBits.AOA))
            {
                ((F16AngleOfAttackIndicator) renderers.AOAIndicator).InstrumentState.OffFlag = true;
                ((F16AngleOfAttackIndicator) renderers.AOAIndicator).InstrumentState.AngleOfAttackDegrees = 0;
            }
            else
            {
                ((F16AngleOfAttackIndicator) renderers.AOAIndicator).InstrumentState.OffFlag = false;
                ((F16AngleOfAttackIndicator) renderers.AOAIndicator).InstrumentState.AngleOfAttackDegrees = fromFalcon.alpha;
            }
        }

        private static void UpdateCompass(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            ((F16Compass) renderers.Compass).InstrumentState.MagneticHeadingDegrees = (360 + (fromFalcon.yaw/ Constants.RADIANS_PER_DEGREE))% 360;
        }

        private static void UpdateAirspeedIndicator(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            ((F16AirspeedIndicator) renderers.ASI).InstrumentState.AirspeedKnots = fromFalcon.kias;
            ((F16AirspeedIndicator) renderers.ASI).InstrumentState.MachNumber = fromFalcon.mach;
        }

        private static void UpdateAltimeter(IInstrumentRendererSet renderers, bool useBMSAdvancedSharedmemValues,
            FlightData fromFalcon, AltBits altbits)
        {
            if (fromFalcon.DataFormat == FalconDataFormats.BMS4 && useBMSAdvancedSharedmemValues)
            {
                if (((altbits & AltBits.CalType) == AltBits.CalType)) //13-08-12 added by Falcas
                {
                    ((F16Altimeter) renderers.Altimeter).Options.PressureAltitudeUnits =
                        F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury;
                }
                else
                {
                    ((F16Altimeter) renderers.Altimeter).Options.PressureAltitudeUnits =
                        F16Altimeter.F16AltimeterOptions.PressureUnits.Millibars;
                }
                //((F16Altimeter)Altimeter).InstrumentState.IndicatedAltitudeFeetMSL = GetIndicatedAltitude (- fromFalcon.z,((F16Altimeter)Altimeter).InstrumentState.BarometricPressure, ((F16Altimeter)Altimeter).Options.PressureAltitudeUnits == F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury) ;
                ((F16Altimeter) renderers.Altimeter).InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.aauz;
                if (fromFalcon.VersionNum >= 111)
                {
                    ((F16Altimeter) renderers.Altimeter).InstrumentState.BarometricPressure =
                        fromFalcon.AltCalReading; //12-08-12 added by Falcas
                    ((F16Altimeter) renderers.Altimeter).InstrumentState.PneumaticModeFlag = ((altbits &
                        AltBits.PneuFlag) ==
                        AltBits.PneuFlag);
                    //12-08-12 added by Falcas
                }
                else
                {
                    ((F16Altimeter) renderers.Altimeter).InstrumentState.BarometricPressure = 2992f;
                    //12-08-12 added by Falcas
                    ((F16Altimeter) renderers.Altimeter).InstrumentState.PneumaticModeFlag = false;
                    //12-08-12 added by Falcas
                    ((F16Altimeter) renderers.Altimeter).Options.PressureAltitudeUnits =
                        F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury; //12-08-12 added by Falcas
                }
            }
            else
            {
                //((F16Altimeter)Altimeter).InstrumentState.IndicatedAltitudeFeetMSL = GetIndicatedAltitude(-fromFalcon.z, ((F16Altimeter)Altimeter).InstrumentState.BarometricPressure, ((F16Altimeter)Altimeter).Options.PressureAltitudeUnits == F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury);
                ((F16Altimeter) renderers.Altimeter).InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.z;
                ((F16Altimeter) renderers.Altimeter).InstrumentState.BarometricPressure = 2992f;
                //12-08-12 added by Falcas
                ((F16Altimeter) renderers.Altimeter).InstrumentState.PneumaticModeFlag = false;
                //12-08-12 added by Falcas
                ((F16Altimeter) renderers.Altimeter).Options.PressureAltitudeUnits =
                    F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury; //12-08-12 added by Falcas
            }
        }

        private static void UpdateVVI(IInstrumentRendererSet renderers, HsiBits hsibits, FlightData fromFalcon)
        {
            float verticalVelocity = 0;
            if (((hsibits & HsiBits.VVI) == HsiBits.VVI))
            {
                verticalVelocity = 0;
            }
            else
            {
                verticalVelocity = -fromFalcon.zDot*60.0f;
            }

            if (renderers.VVI is F16VerticalVelocityIndicatorEU)
            {
                ((F16VerticalVelocityIndicatorEU) renderers.VVI).InstrumentState.OffFlag = ((hsibits & HsiBits.VVI) == HsiBits.VVI);
                ((F16VerticalVelocityIndicatorEU) renderers.VVI).InstrumentState.VerticalVelocityFeet = verticalVelocity;
            }
            else if (renderers.VVI is F16VerticalVelocityIndicatorUSA)
            {
                ((F16VerticalVelocityIndicatorUSA) renderers.VVI).InstrumentState.OffFlag = ((hsibits & HsiBits.VVI) == HsiBits.VVI);
                ((F16VerticalVelocityIndicatorUSA) renderers.VVI).InstrumentState.VerticalVelocityFeet = verticalVelocity;
            }
        }

        private static void UpdateISIS(IInstrumentRendererSet renderers, bool useBMSAdvancedSharedmemValues,
            FlightData fromFalcon, AltBits altbits, FlightDataExtension extensionData, HsiBits hsibits)
        {
            ((F16ISIS) renderers.ISIS).InstrumentState.AirspeedKnots = fromFalcon.kias;

            if (fromFalcon.DataFormat == FalconDataFormats.BMS4 && useBMSAdvancedSharedmemValues)
            {
                ((F16ISIS) renderers.ISIS).InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.aauz;
                //((F16ISIS)ISIS).InstrumentState.IndicatedAltitudeFeetMSL = GetIndicatedAltitude (-fromFalcon.z, ((F16ISIS)ISIS).InstrumentState.BarometricPressure, ((F16ISIS)ISIS).Options.PressureAltitudeUnits == F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury);

                if (fromFalcon.VersionNum >= 111)
                {
                    if (((altbits & AltBits.CalType) == AltBits.CalType)) //13-08-12 added by Falcas
                    {
                        ((F16ISIS) renderers.ISIS).Options.PressureAltitudeUnits =
                            F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury;
                    }
                    else
                    {
                        ((F16ISIS) renderers.ISIS).Options.PressureAltitudeUnits =
                            F16ISIS.F16ISISOptions.PressureUnits.Millibars;
                    }

                    ((F16ISIS) renderers.ISIS).InstrumentState.BarometricPressure = fromFalcon.AltCalReading;
                    //13-08-12 added by Falcas
                }
                else
                {
                    ((F16ISIS) renderers.ISIS).InstrumentState.BarometricPressure = 2992f;
                    //14-0-12 Falcas removed the point
                    ((F16ISIS) renderers.ISIS).Options.PressureAltitudeUnits =
                        F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury; //14-08-12 added by Falcas
                }
            }
            else
            {
                //((F16ISIS)ISIS).InstrumentState.IndicatedAltitudeFeetMSL = GetIndicatedAltitude(-fromFalcon.z, ((F16ISIS)ISIS).InstrumentState.BarometricPressure, ((F16ISIS)ISIS).Options.PressureAltitudeUnits == F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury);
                ((F16ISIS) renderers.ISIS).InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.z;
                ((F16ISIS) renderers.ISIS).InstrumentState.BarometricPressure = 2992f;
                //14-0-12 Falcas removed the point
                ((F16ISIS) renderers.ISIS).Options.PressureAltitudeUnits =
                    F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury; //14-08-12 added by Falcas
            }
            if (extensionData != null)
            {
                ((F16ISIS) renderers.ISIS).InstrumentState.RadarAltitudeAGL = extensionData.RadarAltitudeFeetAGL;
            }
            ((F16ISIS) renderers.ISIS).InstrumentState.MachNumber = fromFalcon.mach;
            ((F16ISIS) renderers.ISIS).InstrumentState.MagneticHeadingDegrees = (360 +
                (fromFalcon.yaw/
                    Constants.RADIANS_PER_DEGREE))%360;
            ((F16ISIS) renderers.ISIS).InstrumentState.NeverExceedSpeedKnots = 850;

            ((F16ISIS) renderers.ISIS).InstrumentState.PitchDegrees = ((fromFalcon.pitch/Constants.RADIANS_PER_DEGREE));
            ((F16ISIS) renderers.ISIS).InstrumentState.RollDegrees = ((fromFalcon.roll/Constants.RADIANS_PER_DEGREE));
            ((F16ISIS) renderers.ISIS).InstrumentState.VerticalVelocityFeetPerMinute = -fromFalcon.zDot*60.0f;
            ((F16ISIS) renderers.ISIS).InstrumentState.OffFlag = ((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF);
            ((F16ISIS) renderers.ISIS).InstrumentState.AuxFlag = ((hsibits & HsiBits.ADI_AUX) == HsiBits.ADI_AUX);
            ((F16ISIS) renderers.ISIS).InstrumentState.GlideslopeFlag = ((hsibits & HsiBits.ADI_GS) == HsiBits.ADI_GS);
            ((F16ISIS) renderers.ISIS).InstrumentState.LocalizerFlag = ((hsibits & HsiBits.ADI_LOC) ==
                HsiBits.ADI_LOC);
        }
    }
}
