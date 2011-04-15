using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using F4SharedMem;
using F4SharedMem.Headers;
using LightningGauges.Renderers;
using F4Utils.SimSupport;
using MFDExtractor.Networking;
using Common.Networking;

namespace MFDExtractor.Runtime.SimSupport.Falcon4
{
    public static class FlightDataToRendererStateTranslator
    {
        public static void UpdateRendererStatesFromFlightData(FlightData flightData, NetworkMode networkMode, bool simRunning, InstrumentRenderers renderers, bool useBMSAdvancedSharedmemValues, Action updateEHSIBrightnessLabelVisibility)
        {
            if (flightData == null || (networkMode != NetworkMode.Client && !simRunning))
            {
                flightData = new FlightData();
                flightData.hsiBits = Int32.MaxValue;
            }

            FlightData fromFalcon = flightData;
            FlightDataExtension extensionData = ((FlightDataExtension)fromFalcon.ExtensionData);

            if (simRunning|| networkMode == NetworkMode.Client)
            {
                HsiBits hsibits = ((HsiBits)fromFalcon.hsiBits);
                bool commandBarsOn = false;

                UpdateISIS(renderers, useBMSAdvancedSharedmemValues, fromFalcon, extensionData, hsibits);
                UpdateVVI(renderers, fromFalcon, hsibits);
                UpdateAltimeter(renderers, useBMSAdvancedSharedmemValues, fromFalcon);
                UpdateASI(renderers, fromFalcon);
                UpdateCompass(renderers, fromFalcon);
                UpdateAOAIndicator(renderers, fromFalcon, hsibits);
                UpdateAOAIndexer(renderers, fromFalcon);
                UpdatePrimaryADI(renderers, hsibits);
                UpdateStandbyADI(renderers, fromFalcon, hsibits);
                UpdateHSI(renderers, fromFalcon, hsibits);

                


                //***** UPDATE SOME COMPLEX HSI/ADI VARIABLES
                if (((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF)) //if the ADI is off
                {
                    ZeroOutADIAndISIS(renderers);
                }
                else
                {
                    UpdateADIPitchAndRoll(renderers, fromFalcon);
                    UpdateISISPitchAndRoll(renderers, fromFalcon);

                    //The following floating data is also crossed up in the flightData.h File:
                    //float AdiIlsHorPos;       // Position of horizontal ILS bar ----Vertical
                    //float AdiIlsVerPos;       // Position of vertical ILS bar-----horizontal
                    commandBarsOn = ((float)(Math.Abs(Math.Round(fromFalcon.AdiIlsHorPos, 4))) != 0.1745f);
                    if (
                            (Math.Abs((fromFalcon.AdiIlsVerPos / Common.Math.Constants.RADIANS_PER_DEGREE)) > 1.0f)
                                ||
                            (Math.Abs((fromFalcon.AdiIlsHorPos / Common.Math.Constants.RADIANS_PER_DEGREE)) > 5.0f)
                        )
                    {
                        commandBarsOn = false;
                    }
                    ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.ShowToFromFlag = true;
                    ((F16EHSI)renderers.EHSIRenderer).InstrumentState.ShowToFromFlag = true;

                    //if the TOTALFLAGS flag is off, then we're most likely in NAV mode
                    if ((hsibits & F4SharedMem.Headers.HsiBits.TotalFlags) != F4SharedMem.Headers.HsiBits.TotalFlags)
                    {
                        ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.ShowToFromFlag = false;
                        ((F16EHSI)renderers.EHSIRenderer).InstrumentState.ShowToFromFlag = false;
                    }
                    //if the TO/FROM flag is showing in shared memory, then we are most likely in TACAN mode (except in F4AF which always has the bit turned on)
                    else if (
                                (
                                    ((hsibits & F4SharedMem.Headers.HsiBits.ToTrue) == F4SharedMem.Headers.HsiBits.ToTrue)
                                        ||
                                    ((hsibits & F4SharedMem.Headers.HsiBits.FromTrue) == F4SharedMem.Headers.HsiBits.FromTrue)
                                )
                        )
                    {
                        if (!commandBarsOn) //better make sure we're not in any ILS mode too though
                        {
                            ShowToFromFlag(renderers);
                        }
                    }

                    HideToFromFlagIfGSOrLOCFlagIsVisible(renderers, hsibits);
                    if (commandBarsOn)
                    {
                        ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.ShowToFromFlag = false;
                        ((F16EHSI)renderers.EHSIRenderer).InstrumentState.ShowToFromFlag = false;
                    }

                    UpdateADIAndISISGlideslopeAndDeviation(renderers, fromFalcon, commandBarsOn);
                }

                if (fromFalcon.DataFormat == FalconDataFormats.BMS4 && useBMSAdvancedSharedmemValues)
                {
                    UpdateNavModeUsingBMSAdvancedSharedmemValues(renderers, fromFalcon);
                }
                else
                {
                    ((F16EHSI)renderers.EHSIRenderer).InstrumentState.InstrumentMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.Unknown;
                }
                if (((hsibits & HsiBits.HSI_OFF) == HsiBits.HSI_OFF))
                {
                    TurnOffHSIAndEHSINavigation(renderers);
                }
                else
                {
                    SetHSINavigationInfo(renderers, fromFalcon, hsibits);
                    SetEHSINavigationInfo(renderers, fromFalcon, hsibits);
                }


                UpdateCourseDeviationAndToFromIndication(renderers);

                //**************************

                UpdateEHSI(updateEHSIBrightnessLabelVisibility);
                UpdateHYDAandHYDB(renderers, fromFalcon);
                UpdateCabinPressureAltimeter(renderers, fromFalcon);
                UpdateRollTrimIndicator(renderers, fromFalcon);
                UpdatePitchTrimIndicator(renderers, fromFalcon);
                UpdateRWR(renderers, fromFalcon);
                UpdateCautionPanel(renderers, fromFalcon);
                UpdateCMDS(renderers, fromFalcon);
                UpdateDED(renderers, fromFalcon);
                UpdatePFL(renderers, fromFalcon);
                UpdateEPUFuel(renderers, fromFalcon);
                UpdateFuelFlow(renderers, fromFalcon);
                UpdateFuelQty(renderers, fromFalcon);
                UpdateLandingLights(renderers, fromFalcon);
                UpdateNWS(renderers, fromFalcon);
                UpdateSpeedbrake(renderers, fromFalcon);
                UpdateRPM(renderers, fromFalcon);
                UpdateFTIT(renderers, fromFalcon);
                UpdateNOZ(renderers, fromFalcon);
                UpdateOIL(renderers, fromFalcon);
                UpdateAccelerometer(renderers, fromFalcon);
            }
            else //Falcon's not running
            {
                if (renderers.VVIRenderer is F16VerticalVelocityIndicatorEU)
                {
                    ((F16VerticalVelocityIndicatorEU)renderers.VVIRenderer).InstrumentState.OffFlag = true;
                }
                else if (renderers.VVIRenderer is F16VerticalVelocityIndicatorUSA)
                {
                    ((F16VerticalVelocityIndicatorUSA)renderers.VVIRenderer).InstrumentState.OffFlag = true;
                }
                ((F16AngleOfAttackIndicator)renderers.AOAIndicatorRenderer).InstrumentState.OffFlag = true;
                ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.OffFlag = true;
                ((F16EHSI)renderers.EHSIRenderer).InstrumentState.NoDataFlag = true;
                ((F16ADI)renderers.ADIRenderer).InstrumentState.OffFlag = true;
                ((F16StandbyADI)renderers.StandbyADIRenderer).InstrumentState.OffFlag = true;
                ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.RWRPowerOn = false;
                ((F16ISIS)renderers.ISISRenderer).InstrumentState.RadarAltitudeAGL = 0;
                ((F16ISIS)renderers.ISISRenderer).InstrumentState.OffFlag = true;
                updateEHSIBrightnessLabelVisibility();
            }

        }
        #region Basic Flight Data Updater Methods
        private static void ZeroOutADIAndISIS(InstrumentRenderers renderers)
        {
            ZeroOutADI(renderers);
            ZeroOutISIS(renderers);
        }

        private static void UpdateADIAndISISGlideslopeAndDeviation(InstrumentRenderers renderers, FlightData fromFalcon, bool commandBarsOn)
        {
            UpdateADIGlideslopeAndDeviation(renderers, fromFalcon, commandBarsOn);
            UpdateISISGlideslopeAndDeviation(renderers, fromFalcon, commandBarsOn);
        }

        private static void UpdateISISGlideslopeAndDeviation(InstrumentRenderers renderers, FlightData fromFalcon, bool commandBarsOn)
        {
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.ShowCommandBars = commandBarsOn;
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.GlideslopeDeviationDegrees = fromFalcon.AdiIlsVerPos / Common.Math.Constants.RADIANS_PER_DEGREE;
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.LocalizerDeviationDegrees = fromFalcon.AdiIlsHorPos / Common.Math.Constants.RADIANS_PER_DEGREE;
        }

        private static void UpdateADIGlideslopeAndDeviation(InstrumentRenderers renderers, FlightData fromFalcon, bool commandBarsOn)
        {
            ((F16ADI)renderers.ADIRenderer).InstrumentState.ShowCommandBars = commandBarsOn;
            ((F16ADI)renderers.ADIRenderer).InstrumentState.GlideslopeDeviationDegrees = fromFalcon.AdiIlsVerPos / Common.Math.Constants.RADIANS_PER_DEGREE;
            ((F16ADI)renderers.ADIRenderer).InstrumentState.LocalizerDeviationDegrees = fromFalcon.AdiIlsHorPos / Common.Math.Constants.RADIANS_PER_DEGREE;
        }

        private static void ShowToFromFlag(InstrumentRenderers renderers)
        {
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.ShowToFromFlag = true;
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.ShowToFromFlag = true;
        }

        private static void HideToFromFlagIfGSOrLOCFlagIsVisible(InstrumentRenderers renderers, HsiBits hsibits)
        {
            //if the glideslope or localizer flags on the ADI are turned on, then we must be in an ILS mode and therefore we 
            //know we don't need to show the HSI TO/FROM flags.
            if (
                ((hsibits & F4SharedMem.Headers.HsiBits.ADI_GS) == F4SharedMem.Headers.HsiBits.ADI_GS)
                    ||
                ((hsibits & F4SharedMem.Headers.HsiBits.ADI_LOC) == F4SharedMem.Headers.HsiBits.ADI_LOC)
                )
            {
                ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.ShowToFromFlag = false;
                ((F16EHSI)renderers.EHSIRenderer).InstrumentState.ShowToFromFlag = false;
            }
        }

        private static void SetEHSINavigationInfo(InstrumentRenderers renderers, FlightData fromFalcon, HsiBits hsibits)
        {
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.DmeInvalidFlag = ((hsibits & HsiBits.CourseWarning) == HsiBits.CourseWarning);
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.DeviationInvalidFlag = ((hsibits & HsiBits.IlsWarning) == HsiBits.IlsWarning);
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.CourseDeviationLimitDegrees = fromFalcon.deviationLimit;
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.CourseDeviationDegrees = fromFalcon.courseDeviation;
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.DesiredCourseDegrees = (int)fromFalcon.desiredCourse;
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.DesiredHeadingDegrees = (int)fromFalcon.desiredHeading;
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.BearingToBeaconDegrees = fromFalcon.bearingToBeacon;
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.DistanceToBeaconNauticalMiles = fromFalcon.distanceToBeacon;
        }

        private static void SetHSINavigationInfo(InstrumentRenderers renderers, FlightData fromFalcon, HsiBits hsibits)
        {
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.DmeInvalidFlag = ((hsibits & HsiBits.CourseWarning) == HsiBits.CourseWarning);
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.DeviationInvalidFlag = ((hsibits & HsiBits.IlsWarning) == HsiBits.IlsWarning);
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.CourseDeviationLimitDegrees = fromFalcon.deviationLimit;
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.CourseDeviationDegrees = fromFalcon.courseDeviation;
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.DesiredCourseDegrees = (int)fromFalcon.desiredCourse;
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.DesiredHeadingDegrees = (int)fromFalcon.desiredHeading;
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.BearingToBeaconDegrees = fromFalcon.bearingToBeacon;
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.DistanceToBeaconNauticalMiles = fromFalcon.distanceToBeacon;
        }

        private static void TurnOffHSIAndEHSINavigation(InstrumentRenderers renderers)
        {
            TurnOffHSINavigation(renderers);
            TurnOffEHSINavigation(renderers);
        }

        private static void TurnOffEHSINavigation(InstrumentRenderers renderers)
        {
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.DmeInvalidFlag = true;
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.DeviationInvalidFlag = false;
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.CourseDeviationLimitDegrees = 0;
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.CourseDeviationDegrees = 0;
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.BearingToBeaconDegrees = 0;
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.DistanceToBeaconNauticalMiles = 0;
        }

        private static void TurnOffHSINavigation(InstrumentRenderers renderers)
        {
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.DmeInvalidFlag = true;
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.DeviationInvalidFlag = false;
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.CourseDeviationLimitDegrees = 0;
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.CourseDeviationDegrees = 0;
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.BearingToBeaconDegrees = 0;
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.DistanceToBeaconNauticalMiles = 0;
        }

        private static void UpdateISISPitchAndRoll(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.PitchDegrees = (float)((fromFalcon.pitch / Common.Math.Constants.RADIANS_PER_DEGREE));
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.RollDegrees = (float)((fromFalcon.roll / Common.Math.Constants.RADIANS_PER_DEGREE));
        }

        private static void UpdateADIPitchAndRoll(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            ((F16ADI)renderers.ADIRenderer).InstrumentState.PitchDegrees = (float)((fromFalcon.pitch / Common.Math.Constants.RADIANS_PER_DEGREE));
            ((F16ADI)renderers.ADIRenderer).InstrumentState.RollDegrees = (float)((fromFalcon.roll / Common.Math.Constants.RADIANS_PER_DEGREE));
        }

        private static void ZeroOutISIS(InstrumentRenderers renderers)
        {
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.PitchDegrees = 0;
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.RollDegrees = 0;
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.GlideslopeDeviationDegrees = 0;
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.LocalizerDeviationDegrees = 0;
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.ShowCommandBars = false;
        }

        private static void ZeroOutADI(InstrumentRenderers renderers)
        {
            ((F16ADI)renderers.ADIRenderer).InstrumentState.PitchDegrees = 0;
            ((F16ADI)renderers.ADIRenderer).InstrumentState.RollDegrees = 0;
            ((F16ADI)renderers.ADIRenderer).InstrumentState.GlideslopeDeviationDegrees = 0;
            ((F16ADI)renderers.ADIRenderer).InstrumentState.LocalizerDeviationDegrees = 0;
            ((F16ADI)renderers.ADIRenderer).InstrumentState.ShowCommandBars = false;
        }

        private static void UpdateEHSI(Action updateEHSIBrightnessLabelVisibility)
        {
            //*** UPDATE EHSI **********
            updateEHSIBrightnessLabelVisibility();
            //**************************
        }

        private static void UpdateNavModeUsingBMSAdvancedSharedmemValues(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            /*
            This value is called navMode and is unsigned char type with 4 possible values: ILS_TACAN = 0, and TACAN = 1,
            NAV = 2, ILS_NAV = 3
            */

            byte bmsNavMode = fromFalcon.navMode;
            switch (bmsNavMode)
            {
                case 0: //NavModes.PlsTcn:
                    ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.ShowToFromFlag = false;
                    ((F16EHSI)renderers.EHSIRenderer).InstrumentState.ShowToFromFlag = false;
                    ((F16EHSI)renderers.EHSIRenderer).InstrumentState.InstrumentMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsTacan;
                    break;
                case 1: //NavModes.Tcn:
                    ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.ShowToFromFlag = true;
                    ((F16EHSI)renderers.EHSIRenderer).InstrumentState.ShowToFromFlag = true;
                    ((F16EHSI)renderers.EHSIRenderer).InstrumentState.InstrumentMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.Tacan;
                    ((F16ADI)renderers.ADIRenderer).InstrumentState.ShowCommandBars = false;
                    ((F16ISIS)renderers.ISISRenderer).InstrumentState.ShowCommandBars = false;
                    break;
                case 2: //NavModes.Nav:
                    ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.ShowToFromFlag = false;
                    ((F16EHSI)renderers.EHSIRenderer).InstrumentState.ShowToFromFlag = false;
                    ((F16EHSI)renderers.EHSIRenderer).InstrumentState.InstrumentMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.Nav;
                    ((F16ADI)renderers.ADIRenderer).InstrumentState.ShowCommandBars = false;
                    ((F16ISIS)renderers.ISISRenderer).InstrumentState.ShowCommandBars = false;
                    break;
                case 3: //NavModes.PlsNav:
                    ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.ShowToFromFlag = false;
                    ((F16EHSI)renderers.EHSIRenderer).InstrumentState.ShowToFromFlag = false;
                    ((F16EHSI)renderers.EHSIRenderer).InstrumentState.InstrumentMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsNav;
                    break;
                default:
                    break;
            }
        }

        private static void UpdateAccelerometer(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //** UPDATE ACCELEROMETER
            float gs = fromFalcon.gs;
            if (gs == 0) //ignore exactly zero g's
            {
                gs = 1;
            }
            ((F16Accelerometer)renderers.AccelerometerRenderer).InstrumentState.AccelerationInGs = gs;
            //******************
        }

        private static void UpdateOIL(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //** UPDATE OIL1
            ((F16OilPressureGauge)renderers.OIL1Renderer).InstrumentState.OilPressurePercent = fromFalcon.oilPressure;
            //******************

            //** UPDATE OIL2
            ((F16OilPressureGauge)renderers.OIL2Renderer).InstrumentState.OilPressurePercent = fromFalcon.oilPressure2;
            //******************
        }

        private static void UpdateNOZ(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            if (fromFalcon.DataFormat == FalconDataFormats.OpenFalcon)
            {
                //NOZ is hosed in OF
                //** UPDATE NOZ1
                ((F16NozzlePositionIndicator)renderers.NOZ1Renderer).InstrumentState.NozzlePositionPercent = NonImplementedGaugeCalculations.NOZ(fromFalcon.rpm, fromFalcon.z, fromFalcon.fuelFlow);
                //******************
                //** UPDATE NOZ2
                ((F16NozzlePositionIndicator)renderers.NOZ2Renderer).InstrumentState.NozzlePositionPercent = NonImplementedGaugeCalculations.NOZ(fromFalcon.rpm2, fromFalcon.z, fromFalcon.fuelFlow);
                //******************
            }
            else
            {
                //NOZ is OK in BMS4, AF, RedViper, FF5
                //** UPDATE NOZ1
                ((F16NozzlePositionIndicator)renderers.NOZ1Renderer).InstrumentState.NozzlePositionPercent = fromFalcon.nozzlePos;
                //******************
                //** UPDATE NOZ2
                ((F16NozzlePositionIndicator)renderers.NOZ2Renderer).InstrumentState.NozzlePositionPercent = fromFalcon.nozzlePos2;
                //******************
            }
        }

        private static void UpdateFTIT(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            if (fromFalcon.DataFormat == FalconDataFormats.BMS4)
            {
                //Only BMS4 has a valid FTIT value in sharedmem
                //** UPDATE FTIT1
                ((F16FanTurbineInletTemperature)renderers.FTIT1Renderer).InstrumentState.InletTemperatureDegreesCelcius = fromFalcon.ftit * 100.0f;
                //******************
                //** UPDATE FTIT2
                ((F16FanTurbineInletTemperature)renderers.FTIT2Renderer).InstrumentState.InletTemperatureDegreesCelcius = fromFalcon.ftit2 * 100.0f;
                //******************
            }
            else
            {
                //FTIT is hosed in AF, RedViper, FF5, OF
                //** UPDATE FTIT1
                ((F16FanTurbineInletTemperature)renderers.FTIT1Renderer).InstrumentState.InletTemperatureDegreesCelcius = NonImplementedGaugeCalculations.Ftit(((F16FanTurbineInletTemperature)renderers.FTIT1Renderer).InstrumentState.InletTemperatureDegreesCelcius, fromFalcon.rpm);
                //******************
                //** UPDATE FTIT2
                ((F16FanTurbineInletTemperature)renderers.FTIT2Renderer).InstrumentState.InletTemperatureDegreesCelcius = NonImplementedGaugeCalculations.Ftit(((F16FanTurbineInletTemperature)renderers.FTIT2Renderer).InstrumentState.InletTemperatureDegreesCelcius, fromFalcon.rpm2);
                //******************
            }
        }

        private static void UpdateRPM(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //** UPDATE RPM1
            ((F16Tachometer)renderers.RPM1Renderer).InstrumentState.RPMPercent = fromFalcon.rpm;
            //******************

            //** UPDATE RPM2
            ((F16Tachometer)renderers.RPM2Renderer).InstrumentState.RPMPercent = fromFalcon.rpm2;
            //******************
        }

        private static void UpdateSpeedbrake(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //** UPDATE SPEEDBRAKE
            ((F16SpeedbrakeIndicator)renderers.SpeedbrakeRenderer).InstrumentState.PercentOpen = fromFalcon.speedBrake * 100.0f;

            if (fromFalcon.DataFormat == FalconDataFormats.BMS4)
            {
                ((F16SpeedbrakeIndicator)renderers.SpeedbrakeRenderer).InstrumentState.PowerLoss = ((fromFalcon.lightBits3 & (int)F4SharedMem.Headers.Bms4LightBits3.Power_Off) == (int)F4SharedMem.Headers.Bms4LightBits3.Power_Off);
            }
            else
            {
                ((F16SpeedbrakeIndicator)renderers.SpeedbrakeRenderer).InstrumentState.PowerLoss = ((fromFalcon.lightBits3 & (int)F4SharedMem.Headers.LightBits3.Power_Off) == (int)F4SharedMem.Headers.LightBits3.Power_Off);
            }
            //******************
        }

        private static void UpdateNWS(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //** UPDATE NWS
            ((F16NosewheelSteeringIndexer)renderers.NWSIndexerRenderer).InstrumentState.DISC = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.RefuelDSC) == (int)F4SharedMem.Headers.LightBits.RefuelDSC);
            ((F16NosewheelSteeringIndexer)renderers.NWSIndexerRenderer).InstrumentState.AR_NWS = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.RefuelAR) == (int)F4SharedMem.Headers.LightBits.RefuelAR);
            ((F16NosewheelSteeringIndexer)renderers.NWSIndexerRenderer).InstrumentState.RDY = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.RefuelRDY) == (int)F4SharedMem.Headers.LightBits.RefuelRDY);
            //******************
        }

        private static void UpdateLandingLights(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //** UPDATE LANDING GEAR LIGHTS

            if (fromFalcon.DataFormat == FalconDataFormats.OpenFalcon || fromFalcon.DataFormat == FalconDataFormats.BMS4)
            {
                ((F16LandingGearWheelsLights)renderers.LandingGearLightsRenderer).InstrumentState.LeftGearDown = ((fromFalcon.lightBits3 & (int)F4SharedMem.Headers.LightBits3.LeftGearDown) == (int)F4SharedMem.Headers.LightBits3.LeftGearDown);
                ((F16LandingGearWheelsLights)renderers.LandingGearLightsRenderer).InstrumentState.NoseGearDown = ((fromFalcon.lightBits3 & (int)F4SharedMem.Headers.LightBits3.NoseGearDown) == (int)F4SharedMem.Headers.LightBits3.NoseGearDown);
                ((F16LandingGearWheelsLights)renderers.LandingGearLightsRenderer).InstrumentState.RightGearDown = ((fromFalcon.lightBits3 & (int)F4SharedMem.Headers.LightBits3.RightGearDown) == (int)F4SharedMem.Headers.LightBits3.RightGearDown);
            }
            else
            {
                ((F16LandingGearWheelsLights)renderers.LandingGearLightsRenderer).InstrumentState.LeftGearDown = ((fromFalcon.LeftGearPos == 1.0f));
                ((F16LandingGearWheelsLights)renderers.LandingGearLightsRenderer).InstrumentState.NoseGearDown = ((fromFalcon.NoseGearPos == 1.0f));
                ((F16LandingGearWheelsLights)renderers.LandingGearLightsRenderer).InstrumentState.RightGearDown = ((fromFalcon.RightGearPos == 1.0f));
            }


            //******************
        }

        private static void UpdateFuelQty(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //** UPDATE FUEL QTY
            ((F16FuelQuantityIndicator)renderers.FuelQuantityRenderer).InstrumentState.AftLeftFuelQuantityPounds = fromFalcon.aft / 10.0f;
            ((F16FuelQuantityIndicator)renderers.FuelQuantityRenderer).InstrumentState.ForeRightFuelQuantityPounds = fromFalcon.fwd / 10.0f;
            ((F16FuelQuantityIndicator)renderers.FuelQuantityRenderer).InstrumentState.TotalFuelQuantityPounds = fromFalcon.total;
            //******************
        }

        private static void UpdateFuelFlow(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //** UPDATE FUEL FLOW
            ((F16FuelFlow)renderers.FuelFlowRenderer).InstrumentState.FuelFlowPoundsPerHour = fromFalcon.fuelFlow;
            //******************
        }

        private static void UpdateEPUFuel(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //** UPDATE EPU FUEL
            ((F16EPUFuelGauge)renderers.EPUFuelRenderer).InstrumentState.FuelRemainingPercent = fromFalcon.epuFuel;
            //******************
        }

        private static void UpdatePFL(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //** UPDATE PFL
            if (fromFalcon.PFLLines != null)
            {
                ((F16DataEntryDisplayPilotFaultList)renderers.PFLRenderer).InstrumentState.Line1 = Encoding.Default.GetBytes(fromFalcon.PFLLines[0]);
                ((F16DataEntryDisplayPilotFaultList)renderers.PFLRenderer).InstrumentState.Line2 = Encoding.Default.GetBytes(fromFalcon.PFLLines[1]);
                ((F16DataEntryDisplayPilotFaultList)renderers.PFLRenderer).InstrumentState.Line3 = Encoding.Default.GetBytes(fromFalcon.PFLLines[2]);
                ((F16DataEntryDisplayPilotFaultList)renderers.PFLRenderer).InstrumentState.Line4 = Encoding.Default.GetBytes(fromFalcon.PFLLines[3]);
                ((F16DataEntryDisplayPilotFaultList)renderers.PFLRenderer).InstrumentState.Line5 = Encoding.Default.GetBytes(fromFalcon.PFLLines[4]);
            }
            if (fromFalcon.PFLInvert != null)
            {
                ((F16DataEntryDisplayPilotFaultList)renderers.PFLRenderer).InstrumentState.Line1Invert = Encoding.Default.GetBytes(fromFalcon.PFLInvert[0]);
                ((F16DataEntryDisplayPilotFaultList)renderers.PFLRenderer).InstrumentState.Line2Invert = Encoding.Default.GetBytes(fromFalcon.PFLInvert[1]);
                ((F16DataEntryDisplayPilotFaultList)renderers.PFLRenderer).InstrumentState.Line3Invert = Encoding.Default.GetBytes(fromFalcon.PFLInvert[2]);
                ((F16DataEntryDisplayPilotFaultList)renderers.PFLRenderer).InstrumentState.Line4Invert = Encoding.Default.GetBytes(fromFalcon.PFLInvert[3]);
                ((F16DataEntryDisplayPilotFaultList)renderers.PFLRenderer).InstrumentState.Line5Invert = Encoding.Default.GetBytes(fromFalcon.PFLInvert[4]);
            }
            //*************************
        }

        private static void UpdateDED(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //** UPDATE DED 
            if (fromFalcon.DEDLines != null)
            {
                ((F16DataEntryDisplayPilotFaultList)renderers.DEDRenderer).InstrumentState.Line1 = Encoding.Default.GetBytes(fromFalcon.DEDLines[0]);
                ((F16DataEntryDisplayPilotFaultList)renderers.DEDRenderer).InstrumentState.Line2 = Encoding.Default.GetBytes(fromFalcon.DEDLines[1]);
                ((F16DataEntryDisplayPilotFaultList)renderers.DEDRenderer).InstrumentState.Line3 = Encoding.Default.GetBytes(fromFalcon.DEDLines[2]);
                ((F16DataEntryDisplayPilotFaultList)renderers.DEDRenderer).InstrumentState.Line4 = Encoding.Default.GetBytes(fromFalcon.DEDLines[3]);
                ((F16DataEntryDisplayPilotFaultList)renderers.DEDRenderer).InstrumentState.Line5 = Encoding.Default.GetBytes(fromFalcon.DEDLines[4]);
            }
            if (fromFalcon.Invert != null)
            {
                ((F16DataEntryDisplayPilotFaultList)renderers.DEDRenderer).InstrumentState.Line1Invert = Encoding.Default.GetBytes(fromFalcon.Invert[0]);
                ((F16DataEntryDisplayPilotFaultList)renderers.DEDRenderer).InstrumentState.Line2Invert = Encoding.Default.GetBytes(fromFalcon.Invert[1]);
                ((F16DataEntryDisplayPilotFaultList)renderers.DEDRenderer).InstrumentState.Line3Invert = Encoding.Default.GetBytes(fromFalcon.Invert[2]);
                ((F16DataEntryDisplayPilotFaultList)renderers.DEDRenderer).InstrumentState.Line4Invert = Encoding.Default.GetBytes(fromFalcon.Invert[3]);
                ((F16DataEntryDisplayPilotFaultList)renderers.DEDRenderer).InstrumentState.Line5Invert = Encoding.Default.GetBytes(fromFalcon.Invert[4]);
            }
            //*************************
        }

        private static void UpdateCMDS(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //**  UPDATE CMDS PANEL
            ((F16CMDSPanel)renderers.CMDSPanelRenderer).InstrumentState.Degraded = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Degr) == (int)F4SharedMem.Headers.LightBits2.Degr);
            ((F16CMDSPanel)renderers.CMDSPanelRenderer).InstrumentState.ChaffCount = (int)fromFalcon.ChaffCount;
            ((F16CMDSPanel)renderers.CMDSPanelRenderer).InstrumentState.ChaffLow = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.ChaffLo) == (int)F4SharedMem.Headers.LightBits2.ChaffLo);
            ((F16CMDSPanel)renderers.CMDSPanelRenderer).InstrumentState.DispenseReady = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Rdy) == (int)F4SharedMem.Headers.LightBits2.Rdy);
            ((F16CMDSPanel)renderers.CMDSPanelRenderer).InstrumentState.FlareCount = (int)fromFalcon.FlareCount;
            ((F16CMDSPanel)renderers.CMDSPanelRenderer).InstrumentState.FlareLow = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.FlareLo) == (int)F4SharedMem.Headers.LightBits2.FlareLo);
            ((F16CMDSPanel)renderers.CMDSPanelRenderer).InstrumentState.Go =
                (
                    ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Go) == (int)F4SharedMem.Headers.LightBits2.Go)
                            &&
                        !(
                            ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.NoGo) == (int)F4SharedMem.Headers.LightBits2.NoGo)
                                     ||
                            ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Degr) == (int)F4SharedMem.Headers.LightBits2.Degr)
                                     ||
                            ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Rdy) == (int)F4SharedMem.Headers.LightBits2.Rdy)
                        )
                );

            ((F16CMDSPanel)renderers.CMDSPanelRenderer).InstrumentState.NoGo =
                (
                    ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.NoGo) == (int)F4SharedMem.Headers.LightBits2.NoGo)
                             ||
                    ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Degr) == (int)F4SharedMem.Headers.LightBits2.Degr)
                );
            ((F16CMDSPanel)renderers.CMDSPanelRenderer).InstrumentState.Other1Count = 0;
            ((F16CMDSPanel)renderers.CMDSPanelRenderer).InstrumentState.Other1Low = true;
            ((F16CMDSPanel)renderers.CMDSPanelRenderer).InstrumentState.Other2Count = 0;
            ((F16CMDSPanel)renderers.CMDSPanelRenderer).InstrumentState.Other2Low = true;
            //**********************
        }

        private static void UpdateCautionPanel(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //** UPDATE CAUTION PANEL
            //TODO: implement all-lights-on when test is detected
            F16CautionPanel.F16CautionPanelInstrumentState myState = ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState;
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.AftFuelLow = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.AftFuelLow) == (int)F4SharedMem.Headers.LightBits2.AftFuelLow);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.AntiSkid = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.ANTI_SKID) == (int)F4SharedMem.Headers.LightBits2.ANTI_SKID);
            //((F16CautionPanel)_cautionPanelRenderer).InstrumentState.ATFNotEngaged = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.TFR_ENGAGED) == (int)F4SharedMem.Headers.LightBits2.TFR_ENGAGED);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.AvionicsFault = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.Avionics) == (int)F4SharedMem.Headers.LightBits.Avionics);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.BUC = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.BUC) == (int)F4SharedMem.Headers.LightBits2.BUC);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.CabinPress = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.CabinPress) == (int)F4SharedMem.Headers.LightBits.CabinPress);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.CADC = ((fromFalcon.lightBits3 & (int)F4SharedMem.Headers.Bms4LightBits3.cadc) == (int)F4SharedMem.Headers.Bms4LightBits3.cadc);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.ECM = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.ECM) == (int)F4SharedMem.Headers.LightBits.ECM);
            //((F16CautionPanel)_cautionPanelRenderer).InstrumentState.EEC = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.ee) == (int)F4SharedMem.Headers.LightBits.ECM);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.ElecSys = ((fromFalcon.lightBits3 & (int)F4SharedMem.Headers.LightBits3.Elec_Fault) == (int)F4SharedMem.Headers.LightBits3.Elec_Fault);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.EngineFault = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.EngineFault) == (int)F4SharedMem.Headers.LightBits.EngineFault);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.EquipHot = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.EQUIP_HOT) == (int)F4SharedMem.Headers.LightBits.EQUIP_HOT);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.FLCSFault = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.FltControlSys) == (int)F4SharedMem.Headers.LightBits.FltControlSys);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.FuelOilHot = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.FUEL_OIL_HOT) == (int)F4SharedMem.Headers.LightBits2.FUEL_OIL_HOT);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.FwdFuelLow = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.FwdFuelLow) == (int)F4SharedMem.Headers.LightBits2.FwdFuelLow);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.Hook = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.Hook) == (int)F4SharedMem.Headers.LightBits.Hook);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.IFF = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.IFF) == (int)F4SharedMem.Headers.LightBits.IFF);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.NWSFail = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.NWSFail) == (int)F4SharedMem.Headers.LightBits.NWSFail);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.Overheat = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.Overheat) == (int)F4SharedMem.Headers.LightBits.Overheat);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.OxyLow = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.OXY_LOW) == (int)F4SharedMem.Headers.LightBits2.OXY_LOW);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.ProbeHeat = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.PROBEHEAT) == (int)F4SharedMem.Headers.LightBits2.PROBEHEAT);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.RadarAlt = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.RadarAlt) == (int)F4SharedMem.Headers.LightBits.RadarAlt);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.SeatNotArmed = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.SEAT_ARM) == (int)F4SharedMem.Headers.LightBits2.SEAT_ARM);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.SEC = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.SEC) == (int)F4SharedMem.Headers.LightBits2.SEC);
            ((F16CautionPanel)renderers.CautionPanelRenderer).InstrumentState.StoresConfig = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.CONFIG) == (int)F4SharedMem.Headers.LightBits.CONFIG);

            //TODO: implement MLU cautions

            //***********************
        }

        private static void UpdateRWR(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //**  UPDATE RWR ****
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.MagneticHeadingDegrees = (360 + (fromFalcon.yaw / Common.Math.Constants.RADIANS_PER_DEGREE)) % 360;
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.RollDegrees = (float)((fromFalcon.roll / Common.Math.Constants.RADIANS_PER_DEGREE));
            int rwrObjectCount = fromFalcon.RwrObjectCount;
            if (fromFalcon.RWRsymbol != null)
            {
                F16AzimuthIndicator.F16AzimuthIndicatorInstrumentState.Blip[] blips = new F16AzimuthIndicator.F16AzimuthIndicatorInstrumentState.Blip[fromFalcon.RWRsymbol.Length];
                ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.Blips = blips;
                //for (int i = 0; i < rwrObjectCount; i++)
                if (fromFalcon.RWRsymbol != null)
                {
                    for (int i = 0; i < fromFalcon.RWRsymbol.Length; i++)
                    {
                        F16AzimuthIndicator.F16AzimuthIndicatorInstrumentState.Blip thisBlip = new F16AzimuthIndicator.F16AzimuthIndicatorInstrumentState.Blip();
                        if (i < rwrObjectCount) thisBlip.Visible = true;
                        if (fromFalcon.bearing != null)
                        {
                            thisBlip.BearingDegrees = (fromFalcon.bearing[i] / Common.Math.Constants.RADIANS_PER_DEGREE);
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
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.Activity = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.AuxAct) == (int)F4SharedMem.Headers.LightBits2.AuxAct);
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.ChaffCount = (int)fromFalcon.ChaffCount;
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.ChaffLow = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.ChaffLo) == (int)F4SharedMem.Headers.LightBits2.ChaffLo);
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.EWSDegraded = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Degr) == (int)F4SharedMem.Headers.LightBits2.Degr);
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.EWSDispenseReady = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Rdy) == (int)F4SharedMem.Headers.LightBits2.Rdy);
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.EWSNoGo = (
                        ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.NoGo) == (int)F4SharedMem.Headers.LightBits2.NoGo)
                                 ||
                        ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Degr) == (int)F4SharedMem.Headers.LightBits2.Degr)
                    );
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.EWSGo =
                (
                    ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Go) == (int)F4SharedMem.Headers.LightBits2.Go)
                            &&
                        !(
                            ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.NoGo) == (int)F4SharedMem.Headers.LightBits2.NoGo)
                                     ||
                            ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Degr) == (int)F4SharedMem.Headers.LightBits2.Degr)
                                     ||
                            ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Rdy) == (int)F4SharedMem.Headers.LightBits2.Rdy)
                        )
                );


            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.FlareCount = (int)fromFalcon.FlareCount;
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.FlareLow = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.FlareLo) == (int)F4SharedMem.Headers.LightBits2.FlareLo);
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.Handoff = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.HandOff) == (int)F4SharedMem.Headers.LightBits2.HandOff);
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.Launch = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Launch) == (int)F4SharedMem.Headers.LightBits2.Launch);
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.LowAltitudeMode = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.AuxLow) == (int)F4SharedMem.Headers.LightBits2.AuxLow);
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.NavalMode = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Naval) == (int)F4SharedMem.Headers.LightBits2.Naval);
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.Other1Count = 0;
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.Other1Low = true;
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.Other2Count = 0;
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.Other2Low = true;
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.RWRPowerOn = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.AuxPwr) == (int)F4SharedMem.Headers.LightBits2.AuxPwr);
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.PriorityMode = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.PriMode) == (int)F4SharedMem.Headers.LightBits2.PriMode);
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.SearchMode = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.AuxSrch) == (int)F4SharedMem.Headers.LightBits2.AuxSrch);
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.SeparateMode = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.TgtSep) == (int)F4SharedMem.Headers.LightBits2.TgtSep);
            ((F16AzimuthIndicator)renderers.RWRRenderer).InstrumentState.UnknownThreatScanMode = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Unk) == (int)F4SharedMem.Headers.LightBits2.Unk);
            //********************
        }

        private static void UpdatePitchTrimIndicator(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //**  UPDATE PITCH TRIM INDICATOR****
            float pitchTrim = fromFalcon.TrimPitch;
            ((F16PitchTrimIndicator)renderers.PitchTrimRenderer).InstrumentState.PitchTrimPercent = pitchTrim * 2.0f * 100.0f;
            //**************************
        }

        private static void UpdateRollTrimIndicator(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //**  UPDATE ROLL TRIM INDICATOR****
            float rolltrim = fromFalcon.TrimRoll;
            ((F16RollTrimIndicator)renderers.RollTrimRenderer).InstrumentState.RollTrimPercent = rolltrim * 2.0f * 100.0f;
            //**************************
        }

        private static void UpdateCabinPressureAltimeter(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //**  UPDATE CABIN PRESSURE ALTITUDE INDICATOR****
            float z = fromFalcon.z;
            float origCabinAlt = ((F16CabinPressureAltitudeIndicator)renderers.CabinPressRenderer).InstrumentState.CabinPressureAltitudeFeet;
            bool pressurization = ((fromFalcon.lightBits & (int)LightBits.CabinPress) == (int)LightBits.CabinPress);
            ((F16CabinPressureAltitudeIndicator)renderers.CabinPressRenderer).InstrumentState.CabinPressureAltitudeFeet = NonImplementedGaugeCalculations.CabinAlt(origCabinAlt, z, pressurization);
            //**************************
        }

        private static void UpdateHYDAandHYDB(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //**  UPDATE HYDA/HYDB****
            float rpm = fromFalcon.rpm;
            bool mainGen = ((fromFalcon.lightBits3 & (int)LightBits3.MainGen) == (int)LightBits3.MainGen);
            bool stbyGen = ((fromFalcon.lightBits3 & (int)LightBits3.StbyGen) == (int)LightBits3.StbyGen);
            bool epuGen = ((fromFalcon.lightBits3 & (int)LightBits3.EpuGen) == (int)LightBits3.EpuGen);
            bool epuOn = ((fromFalcon.lightBits2 & (int)LightBits2.EPUOn) == (int)LightBits2.EPUOn);
            float epuFuel = fromFalcon.epuFuel;
            ((F16HydraulicPressureGauge)renderers.HYDARenderer).InstrumentState.HydraulicPressurePoundsPerSquareInch = NonImplementedGaugeCalculations.HydA(rpm, mainGen, stbyGen, epuGen, epuOn, epuFuel);
            ((F16HydraulicPressureGauge)renderers.HYDBRenderer).InstrumentState.HydraulicPressurePoundsPerSquareInch = NonImplementedGaugeCalculations.HydB(rpm, mainGen, stbyGen, epuGen, epuOn, epuFuel);
            //**************************
        }

        private static void UpdateCourseDeviationAndToFromIndication(InstrumentRenderers renderers)
        {
            //compute course deviation and TO/FROM
            float deviationLimitDecimalDegrees = ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.CourseDeviationLimitDegrees % 180;
            float desiredCourseInDegrees = ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.DesiredCourseDegrees;
            float courseDeviationDecimalDegrees = ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.CourseDeviationDegrees;
            float bearingToBeaconInDegrees = ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.BearingToBeaconDegrees;
            float myCourseDeviationDecimalDegrees = Common.Math.Util.AngleDelta(desiredCourseInDegrees, bearingToBeaconInDegrees);
            bool toFlag = false;
            bool fromFlag = false;
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
                courseDeviationDecimalDegrees = Common.Math.Util.AngleDelta(Math.Abs(courseDeviationDecimalDegrees), 180) % 180;
            }
            else if (courseDeviationDecimalDegrees > 90)
            {
                courseDeviationDecimalDegrees = -Common.Math.Util.AngleDelta(courseDeviationDecimalDegrees, 180) % 180;
            }
            else
            {
                courseDeviationDecimalDegrees = -courseDeviationDecimalDegrees;
            }
            if (Math.Abs(courseDeviationDecimalDegrees) > deviationLimitDecimalDegrees) courseDeviationDecimalDegrees = Math.Sign(courseDeviationDecimalDegrees) * deviationLimitDecimalDegrees;

            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.CourseDeviationDegrees = courseDeviationDecimalDegrees;
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.ToFlag = toFlag;
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.FromFlag = fromFlag;
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.CourseDeviationDegrees = courseDeviationDecimalDegrees;
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.ToFlag = toFlag;
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.FromFlag = fromFlag;
        }

        private static void UpdateHSI(InstrumentRenderers renderers, FlightData fromFalcon, HsiBits hsibits)
        {
            //***** UPDATE HSI ***** 
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.HSI_OFF) == HsiBits.HSI_OFF);
            ((F16HorizontalSituationIndicator)renderers.HSIRenderer).InstrumentState.MagneticHeadingDegrees = (360 + (fromFalcon.yaw / Common.Math.Constants.RADIANS_PER_DEGREE)) % 360;
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.NoDataFlag = ((hsibits & HsiBits.HSI_OFF) == HsiBits.HSI_OFF);
            ((F16EHSI)renderers.EHSIRenderer).InstrumentState.MagneticHeadingDegrees = (360 + (fromFalcon.yaw / Common.Math.Constants.RADIANS_PER_DEGREE)) % 360;
            //**********************
        }

        private static void UpdateStandbyADI(InstrumentRenderers renderers, FlightData fromFalcon, HsiBits hsibits)
        {

            //***** UPDATE BACKUP ADI *****
            ((F16StandbyADI)renderers.StandbyADIRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.BUP_ADI_OFF) == HsiBits.BUP_ADI_OFF);
            if (((hsibits & HsiBits.BUP_ADI_OFF) == HsiBits.BUP_ADI_OFF))
            {
                //if the standby ADI is off
                ((F16StandbyADI)renderers.StandbyADIRenderer).InstrumentState.PitchDegrees = 0;
                ((F16StandbyADI)renderers.StandbyADIRenderer).InstrumentState.RollDegrees = 0;
                ((F16StandbyADI)renderers.StandbyADIRenderer).InstrumentState.OffFlag = true;
            }
            else
            {
                ((F16StandbyADI)renderers.StandbyADIRenderer).InstrumentState.PitchDegrees = (float)((fromFalcon.pitch / Common.Math.Constants.RADIANS_PER_DEGREE));
                ((F16StandbyADI)renderers.StandbyADIRenderer).InstrumentState.RollDegrees = (float)((fromFalcon.roll / Common.Math.Constants.RADIANS_PER_DEGREE));
                ((F16StandbyADI)renderers.StandbyADIRenderer).InstrumentState.OffFlag = false;
            }
            //**********************
        }

        private static void UpdatePrimaryADI(InstrumentRenderers renderers, HsiBits hsibits)
        {
            //***** UPDATE ADI *****
            ((F16ADI)renderers.ADIRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF);
            ((F16ADI)renderers.ADIRenderer).InstrumentState.AuxFlag = ((hsibits & HsiBits.ADI_AUX) == HsiBits.ADI_AUX);
            ((F16ADI)renderers.ADIRenderer).InstrumentState.GlideslopeFlag = ((hsibits & HsiBits.ADI_GS) == HsiBits.ADI_GS);
            ((F16ADI)renderers.ADIRenderer).InstrumentState.LocalizerFlag = ((hsibits & HsiBits.ADI_LOC) == HsiBits.ADI_LOC);
            //**********************
        }

        private static void UpdateAOAIndexer(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //**** UPDATE AOA INDEXER***
            float aoa = ((F16AngleOfAttackIndicator)renderers.AOAIndicatorRenderer).InstrumentState.AngleOfAttackDegrees;
            ((F16AngleOfAttackIndexer)renderers.AOAIndexerRenderer).InstrumentState.AoaBelow = ((fromFalcon.lightBits & (int)LightBits.AOABelow) == (int)LightBits.AOABelow);
            ((F16AngleOfAttackIndexer)renderers.AOAIndexerRenderer).InstrumentState.AoaOn = ((fromFalcon.lightBits & (int)LightBits.AOAOn) == (int)LightBits.AOAOn);
            ((F16AngleOfAttackIndexer)renderers.AOAIndexerRenderer).InstrumentState.AoaAbove = ((fromFalcon.lightBits & (int)LightBits.AOAAbove) == (int)LightBits.AOAAbove);
            //**************************
        }

        private static void UpdateAOAIndicator(InstrumentRenderers renderers, FlightData fromFalcon, HsiBits hsibits)
        {
            //**** UPDATE AOA INDICATOR***
            if (((hsibits & HsiBits.AOA) == HsiBits.AOA))
            {
                ((F16AngleOfAttackIndicator)renderers.AOAIndicatorRenderer).InstrumentState.OffFlag = true;
                ((F16AngleOfAttackIndicator)renderers.AOAIndicatorRenderer).InstrumentState.AngleOfAttackDegrees = 0;
            }
            else
            {
                ((F16AngleOfAttackIndicator)renderers.AOAIndicatorRenderer).InstrumentState.OffFlag = false;
                ((F16AngleOfAttackIndicator)renderers.AOAIndicatorRenderer).InstrumentState.AngleOfAttackDegrees = fromFalcon.alpha;
            }
            //*******************
        }

        private static void UpdateCompass(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //**** UPDATE COMPASS
            ((F16Compass)renderers.CompassRenderer).InstrumentState.MagneticHeadingDegrees = (360 + (fromFalcon.yaw / Common.Math.Constants.RADIANS_PER_DEGREE)) % 360;
            //*******************
        }

        private static void UpdateASI(InstrumentRenderers renderers, FlightData fromFalcon)
        {
            //*** UPDATE ASI ***
            ((F16AirspeedIndicator)renderers.ASIRenderer).InstrumentState.AirspeedKnots = fromFalcon.kias;
            ((F16AirspeedIndicator)renderers.ASIRenderer).InstrumentState.MachNumber = fromFalcon.mach;
            //*************************
        }

        private static void UpdateAltimeter(InstrumentRenderers renderers, bool useBMSAdvancedSharedmemValues, FlightData fromFalcon)
        {
            // *** UPDATE ALTIMETER ***
            if (fromFalcon.DataFormat == FalconDataFormats.BMS4 && useBMSAdvancedSharedmemValues)
            {
                ((F16Altimeter)renderers.AltimeterRenderer).InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.aauz;
            }
            else
            {
                ((F16Altimeter)renderers.AltimeterRenderer).InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.z;
            }
            //*************************
        }

        private static void UpdateVVI(InstrumentRenderers renderers, FlightData fromFalcon, HsiBits hsibits)
        {
            // *** UPDATE VVI ***
            float verticalVelocity = 0;
            if (((hsibits & HsiBits.VVI) == HsiBits.VVI))
            {
                verticalVelocity = 0;
            }
            else
            {
                verticalVelocity = -fromFalcon.zDot * 60.0f;
            }

            if (renderers.VVIRenderer is F16VerticalVelocityIndicatorEU)
            {
                ((F16VerticalVelocityIndicatorEU)renderers.VVIRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.VVI) == HsiBits.VVI);
                ((F16VerticalVelocityIndicatorEU)renderers.VVIRenderer).InstrumentState.VerticalVelocityFeet = verticalVelocity;
            }
            else if (renderers.VVIRenderer is F16VerticalVelocityIndicatorUSA)
            {
                ((F16VerticalVelocityIndicatorUSA)renderers.VVIRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.VVI) == HsiBits.VVI);
                ((F16VerticalVelocityIndicatorUSA)renderers.VVIRenderer).InstrumentState.VerticalVelocityFeetPerMinute = verticalVelocity;
            }

            //*********************
        }

        private static void UpdateISIS(InstrumentRenderers renderers, bool useBMSAdvancedSharedmemValues, FlightData fromFalcon, FlightDataExtension extensionData, HsiBits hsibits)
        {
            //*** UPDATE ISIS ***
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.AirspeedKnots = fromFalcon.kias;
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.BarometricPressure = 29.92f;
            if (fromFalcon.DataFormat == FalconDataFormats.BMS4 && useBMSAdvancedSharedmemValues)
            {
                ((F16ISIS)renderers.ISISRenderer).InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.aauz;
            }
            else
            {
                ((F16ISIS)renderers.ISISRenderer).InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.z;
            }
            if (extensionData != null)
            {
                ((F16ISIS)renderers.ISISRenderer).InstrumentState.RadarAltitudeAGL = extensionData.RadarAltitudeFeetAGL;
            }
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.MachNumber = fromFalcon.mach;
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.MagneticHeadingDegrees = (360 + (fromFalcon.yaw / Common.Math.Constants.RADIANS_PER_DEGREE)) % 360;
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.NeverExceedSpeedKnots = 850;

            ((F16ISIS)renderers.ISISRenderer).InstrumentState.PitchDegrees = (float)((fromFalcon.pitch / Common.Math.Constants.RADIANS_PER_DEGREE));
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.RollDegrees = (float)((fromFalcon.roll / Common.Math.Constants.RADIANS_PER_DEGREE));
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.VerticalVelocityFeetPerMinute = -fromFalcon.zDot * 60.0f;
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF);
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.AuxFlag = ((hsibits & HsiBits.ADI_AUX) == HsiBits.ADI_AUX);
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.GlideslopeFlag = ((hsibits & HsiBits.ADI_GS) == HsiBits.ADI_GS);
            ((F16ISIS)renderers.ISISRenderer).InstrumentState.LocalizerFlag = ((hsibits & HsiBits.ADI_LOC) == HsiBits.ADI_LOC);
        }

        #endregion

    }
}
