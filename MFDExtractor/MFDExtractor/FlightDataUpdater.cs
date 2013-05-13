using System;
using Common.Math;
using Common.Networking;
using F4SharedMem;
using F4SharedMem.Headers;
using F4Utils.SimSupport;
using LightningGauges.Renderers;
using MFDExtractor.FlightDataAdapters;

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
        private readonly IFlightDataAdapterSet _flightDataAdapterSet;
        public FlightDataUpdater(IFlightDataAdapterSet flightDataAdapterSet = null)
        {
            _flightDataAdapterSet = flightDataAdapterSet ?? new FlightDataAdapterSet();
        }
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

            var extensionData = ((FlightDataExtension) flightData.ExtensionData);

            if (simRunning || networkMode == NetworkMode.Client)
            {
                var hsibits = ((HsiBits) flightData.hsiBits);

                _flightDataAdapterSet.ISIS.Adapt(renderers.ISIS,flightData, useBMSAdvancedSharedmemValues);
                _flightDataAdapterSet.VVI.Adapt(renderers.VVI, flightData);
                _flightDataAdapterSet.Altimeter.Adapt(renderers.Altimeter, flightData, useBMSAdvancedSharedmemValues);
                _flightDataAdapterSet.AirspeedIndicator.Adapt(renderers.ASI, flightData);
                _flightDataAdapterSet.Compass.Adapt(renderers.Compass, flightData);
                _flightDataAdapterSet.AOAIndicator.Adapt(renderers.AOAIndicator, flightData);
                _flightDataAdapterSet.AOAIndexer.Adapt(renderers.AOAIndexer, flightData);
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
                    var commandBarsOn = ((float) (Math.Abs(Math.Round(flightData.AdiIlsHorPos, 4))) != 0.1745f);
                    if ((Math.Abs((flightData.AdiIlsVerPos/Constants.RADIANS_PER_DEGREE)) > 1.0f)
                        ||
                        (Math.Abs((flightData.AdiIlsHorPos/Constants.RADIANS_PER_DEGREE)) > 5.0f))
                    {
                        commandBarsOn = false;
                    }
                    renderers.HSI.InstrumentState.ShowToFromFlag = true;
                    renderers.EHSI.InstrumentState.ShowToFromFlag = true;

                    //if the TOTALFLAGS flag is off, then we're most likely in NAV mode
                    if ((hsibits & HsiBits.TotalFlags) != HsiBits.TotalFlags)
                    {
                        renderers.HSI.InstrumentState.ShowToFromFlag = false;
                        renderers.EHSI.InstrumentState.ShowToFromFlag = false;
                    }
                        //if the TO/FROM flag is showing in shared memory, then we are most likely in TACAN mode (except in F4AF which always has the bit turned on)
                    else if ((((hsibits & HsiBits.ToTrue) == HsiBits.ToTrue)
                        ||
                        ((hsibits & HsiBits.FromTrue) == HsiBits.FromTrue)))
                    {
                        if (!commandBarsOn) //better make sure we're not in any ILS mode too though
                        {
                            renderers.HSI.InstrumentState.ShowToFromFlag = true;
                            renderers.EHSI.InstrumentState.ShowToFromFlag = true;
                        }
                    }

                    //if the glideslope or localizer flags on the ADI are turned on, then we must be in an ILS mode and therefore we 
                    //know we don't need to show the HSI TO/FROM flags.
                    if (((hsibits & HsiBits.ADI_GS) == HsiBits.ADI_GS)
                        ||
                        ((hsibits & HsiBits.ADI_LOC) == HsiBits.ADI_LOC))
                    {
                        renderers.HSI.InstrumentState.ShowToFromFlag = false;
                        renderers.EHSI.InstrumentState.ShowToFromFlag = false;
                    }
                    if (commandBarsOn)
                    {
                        renderers.HSI.InstrumentState.ShowToFromFlag = false;
                        renderers.EHSI.InstrumentState.ShowToFromFlag = false;
                    }

                    renderers.ADI.InstrumentState.ShowCommandBars = commandBarsOn;
                    renderers.ADI.InstrumentState.GlideslopeDeviationDegrees = flightData.AdiIlsVerPos/Constants.RADIANS_PER_DEGREE;
                    renderers.ADI.InstrumentState.LocalizerDeviationDegrees = flightData.AdiIlsHorPos/Constants.RADIANS_PER_DEGREE;

                    renderers.ISIS.InstrumentState.ShowCommandBars = commandBarsOn;
                    renderers.ISIS.InstrumentState.GlideslopeDeviationDegrees = flightData.AdiIlsVerPos/Constants.RADIANS_PER_DEGREE;
                    renderers.ISIS.InstrumentState.LocalizerDeviationDegrees = flightData.AdiIlsHorPos/Constants.RADIANS_PER_DEGREE;
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
                _flightDataAdapterSet.CabinPress.Adapt(renderers.CabinPress, flightData);
                _flightDataAdapterSet.RollTrim.Adapt(renderers.RollTrim, flightData);
                _flightDataAdapterSet.PitchTrim.Adapt(renderers.PitchTrim, flightData);
                _flightDataAdapterSet.AzimuthIndicator.Adapt(renderers.RWR, flightData);
                _flightDataAdapterSet.CautionPanel.Adapt(renderers.CautionPanel, flightData);
                _flightDataAdapterSet.CMDS.Adapt(renderers.CMDSPanel, flightData);
                _flightDataAdapterSet.DED.Adapt(renderers.DED, flightData);
                _flightDataAdapterSet.PFL.Adapt(renderers.PFL, flightData);
                _flightDataAdapterSet.EPUFuel.Adapt(renderers.EPUFuel, flightData);
                _flightDataAdapterSet.FuelFlow.Adapt(renderers.FuelFlow, flightData);
                UpdateFuelQTY(renderers, flightData);
                _flightDataAdapterSet.LandingGearLights.Adapt(renderers.LandingGearLights, flightData);
                _flightDataAdapterSet.NWS.Adapt(renderers.NWSIndexer, flightData);
                _flightDataAdapterSet.Speedbrake.Adapt(renderers.Speedbrake, flightData);
                _flightDataAdapterSet.RPM1.Adapt(renderers.RPM1, flightData);
                _flightDataAdapterSet.RPM2.Adapt(renderers.RPM2, flightData);
                UpdateFTIT1andFTIT2(renderers, flightData);
                UpdateNOZ1andNOZ2(renderers, flightData);
                _flightDataAdapterSet.OIL1.Adapt(renderers.OIL1, flightData);
                _flightDataAdapterSet.OIL2.Adapt(renderers.OIL2, flightData);
                _flightDataAdapterSet.Accelerometer.Adapt(renderers.Accelerometer, flightData);
            }
            else //Falcon's not running
            {
                if (renderers.VVI is F16VerticalVelocityIndicatorEU)
                {
                    ((F16VerticalVelocityIndicatorEU) renderers.VVI).InstrumentState.OffFlag = true;
                }
                else if (renderers.VVI is F16VerticalVelocityIndicatorUSA)
                {
                    ((F16VerticalVelocityIndicatorUSA) renderers.VVI).InstrumentState.OffFlag = true;
                }
                renderers.AOAIndicator.InstrumentState.OffFlag = true;
                renderers.HSI.InstrumentState.OffFlag = true;
                renderers.EHSI.InstrumentState.NoDataFlag = true;
                renderers.ADI.InstrumentState.OffFlag = true;
                renderers.BackupADI.InstrumentState.OffFlag = true;
                renderers.RWR.InstrumentState.RWRPowerOn = false;
                renderers.ISIS.InstrumentState.RadarAltitudeAGL = 0;
                renderers.ISIS.InstrumentState.OffFlag = true;
                UpdateEHSIBrightnessLabelVisibility();
            }
        }

        private static void SetISISPitchAndRoll(IInstrumentRendererSet renderers, FlightData flightData)
        {
            renderers.ISIS.InstrumentState.PitchDegrees = ((flightData.pitch/Constants.RADIANS_PER_DEGREE));
            renderers.ISIS.InstrumentState.RollDegrees = ((flightData.roll/Constants.RADIANS_PER_DEGREE));
        }

        private static void SetADIPitchAndRoll(IInstrumentRendererSet renderers, FlightData flightData)
        {
            renderers.ADI.InstrumentState.PitchDegrees = ((flightData.pitch/Constants.RADIANS_PER_DEGREE));
            renderers.ADI.InstrumentState.RollDegrees = ((flightData.roll/Constants.RADIANS_PER_DEGREE));
        }

        private static bool ADIIsTurnedOff(HsiBits hsibits)
        {
            return ((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF);
        }

        private static void SetISISToOffState(IInstrumentRendererSet renderers)
        {
            renderers.ISIS.InstrumentState.PitchDegrees = 0;
            renderers.ISIS.InstrumentState.RollDegrees = 0;
            renderers.ISIS.InstrumentState.GlideslopeDeviationDegrees = 0;
            renderers.ISIS.InstrumentState.LocalizerDeviationDegrees = 0;
            renderers.ISIS.InstrumentState.ShowCommandBars = false;
        }

        private static void SetADIToOffState(IInstrumentRendererSet renderers)
        {
            renderers.ADI.InstrumentState.PitchDegrees = 0;
            renderers.ADI.InstrumentState.RollDegrees = 0;
            renderers.ADI.InstrumentState.GlideslopeDeviationDegrees = 0;
            renderers.ADI.InstrumentState.LocalizerDeviationDegrees = 0;
            renderers.ADI.InstrumentState.ShowCommandBars = false;
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
                        renderers.HSI.InstrumentState.ShowToFromFlag = false;
                        renderers.EHSI.InstrumentState.ShowToFromFlag = false;
                        renderers.EHSI.InstrumentState.InstrumentMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsTacan;
                        break;
                    case 1: //NavModes.Tcn:
                        renderers.HSI.InstrumentState.ShowToFromFlag = true;
                        renderers.EHSI.InstrumentState.ShowToFromFlag = true;
                        renderers.EHSI.InstrumentState.InstrumentMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.Tacan;
                        renderers.ADI.InstrumentState.ShowCommandBars = false;
                        renderers.ISIS.InstrumentState.ShowCommandBars = false;
                        break;
                    case 2: //NavModes.Nav:
                        renderers.HSI.InstrumentState.ShowToFromFlag = false;
                        renderers.EHSI.InstrumentState.ShowToFromFlag = false;
                        renderers.EHSI.InstrumentState.InstrumentMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.Nav;
                        renderers.ADI.InstrumentState.ShowCommandBars = false;
                        renderers.ISIS.InstrumentState.ShowCommandBars = false;
                        break;
                    case 3: //NavModes.PlsNav:
                        renderers.HSI.InstrumentState.ShowToFromFlag = false;
                        renderers.EHSI.InstrumentState.ShowToFromFlag = false;
                        renderers.EHSI.InstrumentState.InstrumentMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsNav;
                        break;
                }
            }
            else
            {
                renderers.EHSI.InstrumentState.InstrumentMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.Unknown;
            }
        }

        private static void UpdateHSIAndEHSICourseDeviationAndToFromFlags(IInstrumentRendererSet renderers)
        {
            var deviationLimitDecimalDegrees = renderers.HSI.InstrumentState.CourseDeviationLimitDegrees%180;
            var courseDeviationDecimalDegrees = renderers.HSI.InstrumentState.CourseDeviationDegrees;
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

            renderers.HSI.InstrumentState.CourseDeviationDegrees = courseDeviationDecimalDegrees;
            renderers.HSI.InstrumentState.ToFlag = toFlag;
            renderers.HSI.InstrumentState.FromFlag = fromFlag;
            renderers.EHSI.InstrumentState.CourseDeviationDegrees = courseDeviationDecimalDegrees;
            renderers.EHSI.InstrumentState.ToFlag = toFlag;
            renderers.EHSI.InstrumentState.FromFlag = fromFlag;
        }

        private static void UpdateEHSIFlightData(IInstrumentRendererSet renderers, FlightData flightData,
            HsiBits hsibits)
        {
            renderers.EHSI.InstrumentState.DmeInvalidFlag = ((hsibits & HsiBits.CourseWarning) == HsiBits.CourseWarning);
            renderers.EHSI.InstrumentState.DeviationInvalidFlag = ((hsibits & HsiBits.IlsWarning) == HsiBits.IlsWarning);
            renderers.EHSI.InstrumentState.CourseDeviationLimitDegrees = flightData.deviationLimit;
            renderers.EHSI.InstrumentState.CourseDeviationDegrees = flightData.courseDeviation;
            renderers.EHSI.InstrumentState.DesiredCourseDegrees = (int) flightData.desiredCourse;
            renderers.EHSI.InstrumentState.DesiredHeadingDegrees = (int) flightData.desiredHeading;
            renderers.EHSI.InstrumentState.BearingToBeaconDegrees = flightData.bearingToBeacon;
            renderers.EHSI.InstrumentState.DistanceToBeaconNauticalMiles = flightData.distanceToBeacon;
        }

        private static void UpdateHSIFlightData(IInstrumentRendererSet renderers, FlightData flightData, HsiBits hsibits)
        {
            renderers.HSI.InstrumentState.DmeInvalidFlag = ((hsibits & HsiBits.CourseWarning) ==HsiBits.CourseWarning);
            renderers.HSI.InstrumentState.DeviationInvalidFlag = ((hsibits &HsiBits.IlsWarning) == HsiBits.IlsWarning);
            renderers.HSI.InstrumentState.CourseDeviationLimitDegrees =flightData.deviationLimit;
            renderers.HSI.InstrumentState.CourseDeviationDegrees =flightData.courseDeviation;
            renderers.HSI.InstrumentState.DesiredCourseDegrees =(int) flightData.desiredCourse;
            renderers.HSI.InstrumentState.DesiredHeadingDegrees =(int) flightData.desiredHeading;
            renderers.HSI.InstrumentState.BearingToBeaconDegrees =flightData.bearingToBeacon;
            renderers.HSI.InstrumentState.DistanceToBeaconNauticalMiles =flightData.distanceToBeacon;
        }

        private static void TurnOffEHSI(IInstrumentRendererSet renderers)
        {
            renderers.EHSI.InstrumentState.DmeInvalidFlag = true;
            renderers.EHSI.InstrumentState.DeviationInvalidFlag = false;
            renderers.EHSI.InstrumentState.CourseDeviationLimitDegrees = 0;
            renderers.EHSI.InstrumentState.CourseDeviationDegrees = 0;
            renderers.EHSI.InstrumentState.BearingToBeaconDegrees = 0;
            renderers.EHSI.InstrumentState.DistanceToBeaconNauticalMiles = 0;
        }

        private static void TurnOffHSI(IInstrumentRendererSet renderers)
        {
            renderers.HSI.InstrumentState.DmeInvalidFlag = true;
            renderers.HSI.InstrumentState.DeviationInvalidFlag = false;
            renderers.HSI.InstrumentState.CourseDeviationLimitDegrees = 0;
            renderers.HSI.InstrumentState.CourseDeviationDegrees = 0;
            renderers.HSI.InstrumentState.BearingToBeaconDegrees = 0;
            renderers.HSI.InstrumentState.DistanceToBeaconNauticalMiles = 0;
        }

        private static void UpdateNOZ1andNOZ2(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            if (fromFalcon.DataFormat == FalconDataFormats.OpenFalcon)
            {
                renderers.NOZ1.InstrumentState.NozzlePositionPercent = NonImplementedGaugeCalculations.NOZ(fromFalcon.rpm, fromFalcon.z, fromFalcon.fuelFlow);
                renderers.NOZ2.InstrumentState.NozzlePositionPercent =NonImplementedGaugeCalculations.NOZ(fromFalcon.rpm2, fromFalcon.z, fromFalcon.fuelFlow);
            }
            else if (fromFalcon.DataFormat == FalconDataFormats.BMS4)
            {
                renderers.NOZ1.InstrumentState.NozzlePositionPercent =fromFalcon.nozzlePos*100.0f;
                renderers.NOZ2.InstrumentState.NozzlePositionPercent =fromFalcon.nozzlePos2*100.0f;
            }
            else
            {
                renderers.NOZ1.InstrumentState.NozzlePositionPercent =fromFalcon.nozzlePos;
                renderers.NOZ2.InstrumentState.NozzlePositionPercent =fromFalcon.nozzlePos2;
            }
        }

        private static void UpdateFTIT1andFTIT2(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            if (fromFalcon.DataFormat == FalconDataFormats.BMS4)
            {
                renderers.FTIT1.InstrumentState.InletTemperatureDegreesCelcius =fromFalcon.ftit*100.0f;
                renderers.FTIT2.InstrumentState.InletTemperatureDegreesCelcius =fromFalcon.ftit2*100.0f;
            }
            else
            {
                renderers.FTIT1.InstrumentState.InletTemperatureDegreesCelcius =NonImplementedGaugeCalculations.Ftit(renderers.FTIT1.InstrumentState.InletTemperatureDegreesCelcius,fromFalcon.rpm);
                renderers.FTIT2.InstrumentState.InletTemperatureDegreesCelcius =NonImplementedGaugeCalculations.Ftit(renderers.FTIT2.InstrumentState.InletTemperatureDegreesCelcius,fromFalcon.rpm2);
            }
        }

        private static void UpdateFuelQTY(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            renderers.FuelQuantity.InstrumentState.AftLeftFuelQuantityPounds =fromFalcon.aft/10.0f;
            renderers.FuelQuantity.InstrumentState.ForeRightFuelQuantityPounds =fromFalcon.fwd/10.0f;
            renderers.FuelQuantity.InstrumentState.TotalFuelQuantityPounds =fromFalcon.total;
        }

        private static void UpdateHYDAandHYDB(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            var rpm = fromFalcon.rpm;
            var mainGen = ((fromFalcon.lightBits3 & (int) LightBits3.MainGen) == (int) LightBits3.MainGen);
            var stbyGen = ((fromFalcon.lightBits3 & (int) LightBits3.StbyGen) == (int) LightBits3.StbyGen);
            var epuGen = ((fromFalcon.lightBits3 & (int) LightBits3.EpuGen) == (int) LightBits3.EpuGen);
            var epuOn = ((fromFalcon.lightBits2 & (int) LightBits2.EPUOn) == (int) LightBits2.EPUOn);
            float epuFuel = fromFalcon.epuFuel;
            renderers.HYDA.InstrumentState.HydraulicPressurePoundsPerSquareInch =NonImplementedGaugeCalculations.HydA(rpm, mainGen, stbyGen, epuGen, epuOn, epuFuel);
            renderers.HYDB.InstrumentState.HydraulicPressurePoundsPerSquareInch =NonImplementedGaugeCalculations.HydB(rpm, mainGen, stbyGen, epuGen, epuOn, epuFuel);
        }

        private static void UpdateEHSI(Action UpdateEHSIBrightnessLabelVisibility)
        {
            UpdateEHSIBrightnessLabelVisibility();
        }

        private static void UpdateHSI(IInstrumentRendererSet renderers, HsiBits hsibits, FlightData fromFalcon)
        {
            renderers.HSI.InstrumentState.OffFlag = ((hsibits & HsiBits.HSI_OFF) ==HsiBits.HSI_OFF);
            renderers.HSI.InstrumentState.MagneticHeadingDegrees = (360 +(fromFalcon.yaw/Constants.RADIANS_PER_DEGREE))%360;
            renderers.EHSI.InstrumentState.NoDataFlag = ((hsibits & HsiBits.HSI_OFF) == HsiBits.HSI_OFF);
            renderers.EHSI.InstrumentState.MagneticHeadingDegrees = (360 +(fromFalcon.yaw/Constants.RADIANS_PER_DEGREE))%360;
        }

        private static void UpdateBackupADI(IInstrumentRendererSet renderers, HsiBits hsibits, FlightData fromFalcon)
        {
            renderers.BackupADI.InstrumentState.OffFlag = ((hsibits & HsiBits.BUP_ADI_OFF) == HsiBits.BUP_ADI_OFF);
            if (((hsibits & HsiBits.BUP_ADI_OFF) == HsiBits.BUP_ADI_OFF))
            {
                //if the standby ADI is off
                renderers.BackupADI.InstrumentState.PitchDegrees = 0;
                renderers.BackupADI.InstrumentState.RollDegrees = 0;
                renderers.BackupADI.InstrumentState.OffFlag = true;
            }
            else
            {
                renderers.BackupADI.InstrumentState.PitchDegrees = ((fromFalcon.pitch/Constants.RADIANS_PER_DEGREE));
                renderers.BackupADI.InstrumentState.RollDegrees = ((fromFalcon.roll/Constants.RADIANS_PER_DEGREE));
                renderers.BackupADI.InstrumentState.OffFlag = false;
            }
        }

        private static void UpdateADI(IInstrumentRendererSet renderers, HsiBits hsibits)
        {
            renderers.ADI.InstrumentState.OffFlag = ((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF);
            renderers.ADI.InstrumentState.AuxFlag = ((hsibits & HsiBits.ADI_AUX) == HsiBits.ADI_AUX);
            renderers.ADI.InstrumentState.GlideslopeFlag = ((hsibits & HsiBits.ADI_GS) == HsiBits.ADI_GS);
            renderers.ADI.InstrumentState.LocalizerFlag = ((hsibits & HsiBits.ADI_LOC) == HsiBits.ADI_LOC);
        }

       
    }
}