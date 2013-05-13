using System;
using System.Text;
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
                var altbits = ((AltBits) flightData.altBits); //12-08-12 added by Falcas 

                UpdateISIS(renderers, useBMSAdvancedSharedmemValues, flightData, altbits, extensionData, hsibits);
                _flightDataAdapterSet.VVI.Adapt(renderers.VVI, flightData);
                _flightDataAdapterSet.Altimeter.Adapt(renderers.Altimeter, flightData, useBMSAdvancedSharedmemValues);
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
                UpdateCabinPress(renderers, flightData);
                UpdateRollTrim(renderers, flightData);
                UpdatePitchTrim(renderers, flightData);
                UpdateRWR(renderers, flightData);
                UpdateCautionPanel(renderers, flightData);
                _flightDataAdapterSet.CMDS.Adapt(renderers.CMDSPanel, flightData);
                _flightDataAdapterSet.DED.Adapt(renderers.DED, flightData);
                UpdatePFL(renderers, flightData);
                UpdateEPUFuel(renderers, flightData);
                UpdateFuelFlow(renderers, flightData);
                UpdateFuelQTY(renderers, flightData);
                UpdateLandingGearLights(renderers, flightData);
                _flightDataAdapterSet.NWS.Adapt(renderers.NWSIndexer, flightData);
                _flightDataAdapterSet.Speedbrake.Adapt(renderers.Speedbrake, flightData);
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

        private static void UpdateAccelerometer(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            float gs = fromFalcon.gs;
            if (gs == 0) //ignore exactly zero g's
            {
                gs = 1;
            }
            renderers.Accelerometer.InstrumentState.AccelerationInGs = gs;
        }

        private static void UpdateOIL2(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            renderers.OIL2.InstrumentState.OilPressurePercent = fromFalcon.oilPressure2;
        }

        private static void UpdateOIL1(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            renderers.OIL1.InstrumentState.OilPressurePercent = fromFalcon.oilPressure;
        }

        private static void UpdateNOZ1andNOZ2(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            if (fromFalcon.DataFormat == FalconDataFormats.OpenFalcon)
            {
                //NOZ is hosed in OF
                //** UPDATE NOZ1
                renderers.NOZ1.InstrumentState.NozzlePositionPercent = NonImplementedGaugeCalculations.NOZ(fromFalcon.rpm, fromFalcon.z, fromFalcon.fuelFlow);
                //******************
                //** UPDATE NOZ2
                renderers.NOZ2.InstrumentState.NozzlePositionPercent =NonImplementedGaugeCalculations.NOZ(fromFalcon.rpm2, fromFalcon.z, fromFalcon.fuelFlow);
                //******************
            }
            else if (fromFalcon.DataFormat == FalconDataFormats.BMS4)
            {
                //** UPDATE NOZ1
                renderers.NOZ1.InstrumentState.NozzlePositionPercent =fromFalcon.nozzlePos*100.0f;
                //******************
                //** UPDATE NOZ2
                renderers.NOZ2.InstrumentState.NozzlePositionPercent =fromFalcon.nozzlePos2*100.0f;
                //******************
            }
            else
            {
                //NOZ is OK in AF, RedViper, FF5
                //** UPDATE NOZ1
                renderers.NOZ1.InstrumentState.NozzlePositionPercent =fromFalcon.nozzlePos;
                //******************
                //** UPDATE NOZ2
                renderers.NOZ2.InstrumentState.NozzlePositionPercent =fromFalcon.nozzlePos2;
                //******************
            }
        }

        private static void UpdateFTIT1andFTIT2(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            if (fromFalcon.DataFormat == FalconDataFormats.BMS4)
            {
                //Only BMS4 has a valid FTIT value in sharedmem
                //** UPDATE FTIT1
                renderers.FTIT1.InstrumentState.InletTemperatureDegreesCelcius =fromFalcon.ftit*100.0f;
                //******************
                //** UPDATE FTIT2
                renderers.FTIT2.InstrumentState.InletTemperatureDegreesCelcius =fromFalcon.ftit2*100.0f;
                //******************
            }
            else
            {
                //FTIT is hosed in AF, RedViper, FF5, OF
                //** UPDATE FTIT1
                renderers.FTIT1.InstrumentState.InletTemperatureDegreesCelcius =NonImplementedGaugeCalculations.Ftit(renderers.FTIT1.InstrumentState.InletTemperatureDegreesCelcius,fromFalcon.rpm);
                //******************
                //** UPDATE FTIT2
                renderers.FTIT2.InstrumentState.InletTemperatureDegreesCelcius =NonImplementedGaugeCalculations.Ftit(renderers.FTIT2.InstrumentState.InletTemperatureDegreesCelcius,fromFalcon.rpm2);
                //******************
            }
        }

        private static void UpdateRPM2(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            renderers.RPM2.InstrumentState.RPMPercent = fromFalcon.rpm2;
        }

        private static void UpdateRPM1(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            renderers.RPM1.InstrumentState.RPMPercent = fromFalcon.rpm;
        }

       

        private static void UpdateLandingGearLights(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            if (fromFalcon.DataFormat == FalconDataFormats.OpenFalcon ||fromFalcon.DataFormat == FalconDataFormats.BMS4)
            {
                renderers.LandingGearLights.InstrumentState.LeftGearDown =((fromFalcon.lightBits3 & (int) LightBits3.LeftGearDown) == (int) LightBits3.LeftGearDown);
                renderers.LandingGearLights.InstrumentState.NoseGearDown =((fromFalcon.lightBits3 & (int) LightBits3.NoseGearDown) == (int) LightBits3.NoseGearDown);
                renderers.LandingGearLights.InstrumentState.RightGearDown =((fromFalcon.lightBits3 & (int) LightBits3.RightGearDown) == (int) LightBits3.RightGearDown);
            }
            else
            {
                renderers.LandingGearLights.InstrumentState.LeftGearDown =((fromFalcon.LeftGearPos == 1.0f));
                renderers.LandingGearLights.InstrumentState.NoseGearDown =((fromFalcon.NoseGearPos == 1.0f));
                renderers.LandingGearLights.InstrumentState.RightGearDown =((fromFalcon.RightGearPos == 1.0f));
            }
        }

        private static void UpdateFuelQTY(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            renderers.FuelQuantity.InstrumentState.AftLeftFuelQuantityPounds =fromFalcon.aft/10.0f;
            renderers.FuelQuantity.InstrumentState.ForeRightFuelQuantityPounds =fromFalcon.fwd/10.0f;
            renderers.FuelQuantity.InstrumentState.TotalFuelQuantityPounds =fromFalcon.total;
        }

        private static void UpdateFuelFlow(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            renderers.FuelFlow.InstrumentState.FuelFlowPoundsPerHour = fromFalcon.fuelFlow;
        }

        private static void UpdateEPUFuel(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            renderers.EPUFuel.InstrumentState.FuelRemainingPercent = fromFalcon.epuFuel;
        }

        private static void UpdatePFL(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            if (fromFalcon.PFLLines != null)
            {
                renderers.PFL.InstrumentState.Line1 =Encoding.Default.GetBytes(fromFalcon.PFLLines[0] ?? "");
                renderers.PFL.InstrumentState.Line2 =Encoding.Default.GetBytes(fromFalcon.PFLLines[1] ?? "");
                renderers.PFL.InstrumentState.Line3 =Encoding.Default.GetBytes(fromFalcon.PFLLines[2] ?? "");
                renderers.PFL.InstrumentState.Line4 =Encoding.Default.GetBytes(fromFalcon.PFLLines[3] ?? "");
                renderers.PFL.InstrumentState.Line5 =Encoding.Default.GetBytes(fromFalcon.PFLLines[4] ?? "");
            }
            if (fromFalcon.PFLInvert != null)
            {
                renderers.PFL.InstrumentState.Line1Invert =Encoding.Default.GetBytes(fromFalcon.PFLInvert[0] ?? "");
                renderers.PFL.InstrumentState.Line2Invert =Encoding.Default.GetBytes(fromFalcon.PFLInvert[1] ?? "");
                renderers.PFL.InstrumentState.Line3Invert =Encoding.Default.GetBytes(fromFalcon.PFLInvert[2] ?? "");
                renderers.PFL.InstrumentState.Line4Invert =Encoding.Default.GetBytes(fromFalcon.PFLInvert[3] ?? "");
                renderers.PFL.InstrumentState.Line5Invert =Encoding.Default.GetBytes(fromFalcon.PFLInvert[4] ?? "");
            }
        }


        private static void UpdateCautionPanel(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
//TODO: implement all-lights-on when test is detected
            renderers.CautionPanel.InstrumentState.AftFuelLow = ((fromFalcon.lightBits2 &(int) LightBits2.AftFuelLow) ==(int) LightBits2.AftFuelLow);
            renderers.CautionPanel.InstrumentState.AntiSkid = ((fromFalcon.lightBits2 &(int) LightBits2.ANTI_SKID) ==(int) LightBits2.ANTI_SKID);
            //((F16CautionPanel)CautionPanel).InstrumentState.ATFNotEngaged = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.TFR_ENGAGED) == (int)F4SharedMem.Headers.LightBits2.TFR_ENGAGED);
            renderers.CautionPanel.InstrumentState.AvionicsFault = ((fromFalcon.lightBits &(int) LightBits.Avionics) ==(int) LightBits.Avionics);
            renderers.CautionPanel.InstrumentState.BUC = ((fromFalcon.lightBits2 &(int) LightBits2.BUC) ==(int) LightBits2.BUC);
            renderers.CautionPanel.InstrumentState.CabinPress = ((fromFalcon.lightBits &(int) LightBits.CabinPress) ==(int) LightBits.CabinPress);
            renderers.CautionPanel.InstrumentState.CADC = ((fromFalcon.lightBits3 &(int) Bms4LightBits3.cadc) ==(int) Bms4LightBits3.cadc);
            renderers.CautionPanel.InstrumentState.ECM = ((fromFalcon.lightBits &(int) LightBits.ECM) ==(int) LightBits.ECM);
            //((F16CautionPanel)renderers.CautionPanel).InstrumentState.EEC = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.ee) == (int)F4SharedMem.Headers.LightBits.ECM);
            renderers.CautionPanel.InstrumentState.ElecSys = ((fromFalcon.lightBits3 &(int) LightBits3.Elec_Fault) ==(int) LightBits3.Elec_Fault);
            renderers.CautionPanel.InstrumentState.EngineFault = ((fromFalcon.lightBits &(int) LightBits.EngineFault) ==(int) LightBits.EngineFault);
            renderers.CautionPanel.InstrumentState.EquipHot = ((fromFalcon.lightBits &(int) LightBits.EQUIP_HOT) ==(int) LightBits.EQUIP_HOT);
            renderers.CautionPanel.InstrumentState.FLCSFault = ((fromFalcon.lightBits &(int) LightBits.FltControlSys) ==(int) LightBits.FltControlSys);
            renderers.CautionPanel.InstrumentState.FuelOilHot = ((fromFalcon.lightBits2 &(int) LightBits2.FUEL_OIL_HOT) ==(int) LightBits2.FUEL_OIL_HOT);
            renderers.CautionPanel.InstrumentState.FwdFuelLow = ((fromFalcon.lightBits2 &(int) LightBits2.FwdFuelLow) ==(int) LightBits2.FwdFuelLow);
            renderers.CautionPanel.InstrumentState.Hook = ((fromFalcon.lightBits &(int) LightBits.Hook) ==(int) LightBits.Hook);
            renderers.CautionPanel.InstrumentState.IFF = ((fromFalcon.lightBits &(int) LightBits.IFF) ==(int) LightBits.IFF);
            renderers.CautionPanel.InstrumentState.NWSFail = ((fromFalcon.lightBits &(int) LightBits.NWSFail) ==(int) LightBits.NWSFail);
            renderers.CautionPanel.InstrumentState.Overheat = ((fromFalcon.lightBits &(int) LightBits.Overheat) ==(int) LightBits.Overheat);
            renderers.CautionPanel.InstrumentState.OxyLow = ((fromFalcon.lightBits2 &(int) LightBits2.OXY_LOW) ==(int) LightBits2.OXY_LOW);
            renderers.CautionPanel.InstrumentState.ProbeHeat = ((fromFalcon.lightBits2 &(int) LightBits2.PROBEHEAT) ==(int) LightBits2.PROBEHEAT);
            renderers.CautionPanel.InstrumentState.RadarAlt = ((fromFalcon.lightBits &(int) LightBits.RadarAlt) ==(int) LightBits.RadarAlt);
            renderers.CautionPanel.InstrumentState.SeatNotArmed = ((fromFalcon.lightBits2 &(int) LightBits2.SEAT_ARM) ==(int) LightBits2.SEAT_ARM);
            renderers.CautionPanel.InstrumentState.SEC = ((fromFalcon.lightBits2 &(int) LightBits2.SEC) ==(int) LightBits2.SEC);
            renderers.CautionPanel.InstrumentState.StoresConfig = ((fromFalcon.lightBits &(int) LightBits.CONFIG) ==(int) LightBits.CONFIG);

            //TODO: implement MLU cautions
        }

        private static void UpdateRWR(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            renderers.RWR.InstrumentState.MagneticHeadingDegrees = (360 + (fromFalcon.yaw/Constants.RADIANS_PER_DEGREE))%360;
            renderers.RWR.InstrumentState.RollDegrees = ((fromFalcon.roll/Constants.RADIANS_PER_DEGREE));
            var rwrObjectCount = fromFalcon.RwrObjectCount;
            if (fromFalcon.RWRsymbol != null)
            {
                var blips = new F16AzimuthIndicator.F16AzimuthIndicatorInstrumentState.Blip[fromFalcon.RWRsymbol.Length];
                renderers.RWR.InstrumentState.Blips = blips;
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
            renderers.RWR.InstrumentState.Activity = ((fromFalcon.lightBits2 & (int) LightBits2.AuxAct) ==(int) LightBits2.AuxAct);
            renderers.RWR.InstrumentState.ChaffCount = (int) fromFalcon.ChaffCount;
            renderers.RWR.InstrumentState.ChaffLow = ((fromFalcon.lightBits2 & (int) LightBits2.ChaffLo) ==(int) LightBits2.ChaffLo);
            renderers.RWR.InstrumentState.EWSDegraded = ((fromFalcon.lightBits2 & (int) LightBits2.Degr) ==(int) LightBits2.Degr);
            renderers.RWR.InstrumentState.EWSDispenseReady = ((fromFalcon.lightBits2 & (int) LightBits2.Rdy) ==(int) LightBits2.Rdy);
            renderers.RWR.InstrumentState.EWSNoGo = (
                ((fromFalcon.lightBits2 & (int) LightBits2.NoGo) == (int) LightBits2.NoGo)
                    ||
                ((fromFalcon.lightBits2 & (int) LightBits2.Degr) == (int) LightBits2.Degr)
                );
            renderers.RWR.InstrumentState.EWSGo =
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


            renderers.RWR.InstrumentState.FlareCount = (int) fromFalcon.FlareCount;
            renderers.RWR.InstrumentState.FlareLow = ((fromFalcon.lightBits2 &(int) LightBits2.FlareLo) == (int) LightBits2.FlareLo);
            renderers.RWR.InstrumentState.Handoff = ((fromFalcon.lightBits2 &(int) LightBits2.HandOff) == (int) LightBits2.HandOff);
            renderers.RWR.InstrumentState.Launch = ((fromFalcon.lightBits2 &(int) LightBits2.Launch) == (int) LightBits2.Launch);
            renderers.RWR.InstrumentState.LowAltitudeMode = ((fromFalcon.lightBits2 &(int) LightBits2.AuxLow) == (int) LightBits2.AuxLow);
            renderers.RWR.InstrumentState.NavalMode = ((fromFalcon.lightBits2 &(int) LightBits2.Naval) == (int) LightBits2.Naval);
            renderers.RWR.InstrumentState.Other1Count = 0;
            renderers.RWR.InstrumentState.Other1Low = true;
            renderers.RWR.InstrumentState.Other2Count = 0;
            renderers.RWR.InstrumentState.Other2Low = true;
            renderers.RWR.InstrumentState.RWRPowerOn = ((fromFalcon.lightBits2 &(int) LightBits2.AuxPwr) == (int) LightBits2.AuxPwr);
            renderers.RWR.InstrumentState.PriorityMode = ((fromFalcon.lightBits2 &(int) LightBits2.PriMode) == (int) LightBits2.PriMode);
            renderers.RWR.InstrumentState.SearchMode = ((fromFalcon.lightBits2 &(int) LightBits2.AuxSrch) == (int) LightBits2.AuxSrch);
            renderers.RWR.InstrumentState.SeparateMode = ((fromFalcon.lightBits2 &(int) LightBits2.TgtSep) == (int) LightBits2.TgtSep);
            renderers.RWR.InstrumentState.UnknownThreatScanMode = ((fromFalcon.lightBits2 &(int) LightBits2.Unk) == (int) LightBits2.Unk);
        }

        private static void UpdatePitchTrim(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            float pitchTrim = fromFalcon.TrimPitch;
            renderers.PitchTrim.InstrumentState.PitchTrimPercent = pitchTrim*2.0f*100.0f;
        }

        private static void UpdateRollTrim(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            float rolltrim = fromFalcon.TrimRoll;
            renderers.RollTrim.InstrumentState.RollTrimPercent = rolltrim*2.0f*100.0f;
        }

        private static void UpdateCabinPress(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            float z = fromFalcon.z;
            float origCabinAlt = renderers.CabinPress.InstrumentState.CabinPressureAltitudeFeet;
            bool pressurization = ((fromFalcon.lightBits & (int) LightBits.CabinPress) == (int) LightBits.CabinPress);
            renderers.CabinPress.InstrumentState.CabinPressureAltitudeFeet =NonImplementedGaugeCalculations.CabinAlt(origCabinAlt, z, pressurization);
        }

        private static void UpdateHYDAandHYDB(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            float rpm = fromFalcon.rpm;
            bool mainGen = ((fromFalcon.lightBits3 & (int) LightBits3.MainGen) == (int) LightBits3.MainGen);
            bool stbyGen = ((fromFalcon.lightBits3 & (int) LightBits3.StbyGen) == (int) LightBits3.StbyGen);
            bool epuGen = ((fromFalcon.lightBits3 & (int) LightBits3.EpuGen) == (int) LightBits3.EpuGen);
            bool epuOn = ((fromFalcon.lightBits2 & (int) LightBits2.EPUOn) == (int) LightBits2.EPUOn);
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

        private static void UpdateAOAIndexer(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            renderers.AOAIndexer.InstrumentState.AoaBelow = ((fromFalcon.lightBits & (int) LightBits.AOABelow) ==(int) LightBits.AOABelow);
            renderers.AOAIndexer.InstrumentState.AoaOn = ((fromFalcon.lightBits & (int) LightBits.AOAOn) ==(int) LightBits.AOAOn);
            renderers.AOAIndexer.InstrumentState.AoaAbove = ((fromFalcon.lightBits & (int) LightBits.AOAAbove) ==(int) LightBits.AOAAbove);
        }

        private static void UpdateAOAIndicator(IInstrumentRendererSet renderers, HsiBits hsibits, FlightData fromFalcon)
        {
            if (((hsibits & HsiBits.AOA) == HsiBits.AOA))
            {
                renderers.AOAIndicator.InstrumentState.OffFlag = true;
                renderers.AOAIndicator.InstrumentState.AngleOfAttackDegrees = 0;
            }
            else
            {
                renderers.AOAIndicator.InstrumentState.OffFlag = false;
                renderers.AOAIndicator.InstrumentState.AngleOfAttackDegrees = fromFalcon.alpha;
            }
        }

        private static void UpdateCompass(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            renderers.Compass.InstrumentState.MagneticHeadingDegrees = (360 +(fromFalcon.yaw/Constants.RADIANS_PER_DEGREE))%360;
        }

        private static void UpdateAirspeedIndicator(IInstrumentRendererSet renderers, FlightData fromFalcon)
        {
            renderers.ASI.InstrumentState.AirspeedKnots = fromFalcon.kias;
            renderers.ASI.InstrumentState.MachNumber = fromFalcon.mach;
        }

        private static void UpdateISIS(IInstrumentRendererSet renderers, bool useBMSAdvancedSharedmemValues,
            FlightData fromFalcon, AltBits altbits, FlightDataExtension extensionData, HsiBits hsibits)
        {
            renderers.ISIS.InstrumentState.AirspeedKnots = fromFalcon.kias;

            if (fromFalcon.DataFormat == FalconDataFormats.BMS4 && useBMSAdvancedSharedmemValues)
            {
                renderers.ISIS.InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.aauz;
                //((F16ISIS)ISIS).InstrumentState.IndicatedAltitudeFeetMSL = GetIndicatedAltitude (-fromFalcon.z, ((F16ISIS)ISIS).InstrumentState.BarometricPressure, ((F16ISIS)ISIS).Options.PressureAltitudeUnits == F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury);

                if (fromFalcon.VersionNum >= 111)
                {
                    if (((altbits & AltBits.CalType) == AltBits.CalType)) //13-08-12 added by Falcas
                    {
                        renderers.ISIS.Options.PressureAltitudeUnits =F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury;
                    }
                    else
                    {
                        renderers.ISIS.Options.PressureAltitudeUnits =F16ISIS.F16ISISOptions.PressureUnits.Millibars;
                    }

                    renderers.ISIS.InstrumentState.BarometricPressure = fromFalcon.AltCalReading;
                    //13-08-12 added by Falcas
                }
                else
                {
                    renderers.ISIS.InstrumentState.BarometricPressure = 2992f;
                    //14-0-12 Falcas removed the point
                    renderers.ISIS.Options.PressureAltitudeUnits =F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury; //14-08-12 added by Falcas
                }
            }
            else
            {
                //((F16ISIS)ISIS).InstrumentState.IndicatedAltitudeFeetMSL = GetIndicatedAltitude(-fromFalcon.z, ((F16ISIS)ISIS).InstrumentState.BarometricPressure, ((F16ISIS)ISIS).Options.PressureAltitudeUnits == F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury);
                renderers.ISIS.InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.z;
                renderers.ISIS.InstrumentState.BarometricPressure = 2992f;
                //14-0-12 Falcas removed the point
                renderers.ISIS.Options.PressureAltitudeUnits =F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury; //14-08-12 added by Falcas
            }
            if (extensionData != null)
            {
                renderers.ISIS.InstrumentState.RadarAltitudeAGL = extensionData.RadarAltitudeFeetAGL;
            }
            renderers.ISIS.InstrumentState.MachNumber = fromFalcon.mach;
            renderers.ISIS.InstrumentState.MagneticHeadingDegrees = (360 +(fromFalcon.yaw/Constants.RADIANS_PER_DEGREE))%360;
            renderers.ISIS.InstrumentState.NeverExceedSpeedKnots = 850;

            renderers.ISIS.InstrumentState.PitchDegrees = ((fromFalcon.pitch/Constants.RADIANS_PER_DEGREE));
            renderers.ISIS.InstrumentState.RollDegrees = ((fromFalcon.roll/Constants.RADIANS_PER_DEGREE));
            renderers.ISIS.InstrumentState.VerticalVelocityFeetPerMinute = -fromFalcon.zDot*60.0f;
            renderers.ISIS.InstrumentState.OffFlag = ((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF);
            renderers.ISIS.InstrumentState.AuxFlag = ((hsibits & HsiBits.ADI_AUX) == HsiBits.ADI_AUX);
            renderers.ISIS.InstrumentState.GlideslopeFlag = ((hsibits & HsiBits.ADI_GS) == HsiBits.ADI_GS);
            renderers.ISIS.InstrumentState.LocalizerFlag = ((hsibits & HsiBits.ADI_LOC) ==HsiBits.ADI_LOC);
        }
    }
}