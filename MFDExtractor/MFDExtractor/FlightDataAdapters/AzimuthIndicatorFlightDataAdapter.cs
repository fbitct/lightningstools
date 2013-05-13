using Common.Math;
using F4SharedMem;
using F4SharedMem.Headers;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IAzimuthIndicatorFlightDataAdapter
    {
        void Adapt(IF16AzimuthIndicator azimuthIndicator, FlightData flightData);
    }

    class AzimuthIndicatorFlightDataAdapter : IAzimuthIndicatorFlightDataAdapter
    {
        public void Adapt(IF16AzimuthIndicator azimuthIndicator, FlightData flightData)
        {
            azimuthIndicator.InstrumentState.MagneticHeadingDegrees = (360 + (flightData.yaw / Constants.RADIANS_PER_DEGREE)) % 360;
            azimuthIndicator.InstrumentState.RollDegrees = ((flightData.roll / Constants.RADIANS_PER_DEGREE));
            var rwrObjectCount = flightData.RwrObjectCount;
            if (flightData.RWRsymbol != null)
            {
                var blips = new F16AzimuthIndicator.F16AzimuthIndicatorInstrumentState.Blip[flightData.RWRsymbol.Length];
                azimuthIndicator.InstrumentState.Blips = blips;
                if (flightData.RWRsymbol != null)
                {
                    for (var i = 0; i < flightData.RWRsymbol.Length; i++)
                    {
                        var thisBlip = new F16AzimuthIndicator.F16AzimuthIndicatorInstrumentState.Blip();
                        if (i < rwrObjectCount) thisBlip.Visible = true;
                        if (flightData.bearing != null)
                        {
                            thisBlip.BearingDegrees = (flightData.bearing[i] / Constants.RADIANS_PER_DEGREE);
                        }
                        if (flightData.lethality != null)
                        {
                            thisBlip.Lethality = flightData.lethality[i];
                        }
                        if (flightData.missileActivity != null)
                        {
                            thisBlip.MissileActivity = flightData.missileActivity[i];
                        }
                        if (flightData.missileLaunch != null)
                        {
                            thisBlip.MissileLaunch = flightData.missileLaunch[i];
                        }
                        if (flightData.newDetection != null)
                        {
                            thisBlip.NewDetection = flightData.newDetection[i];
                        }
                        if (flightData.selected != null)
                        {
                            thisBlip.Selected = flightData.selected[i];
                        }
                        thisBlip.SymbolID = flightData.RWRsymbol[i];
                        blips[i] = thisBlip;
                    }
                }
            }
            azimuthIndicator.InstrumentState.Activity = ((flightData.lightBits2 & (int)LightBits2.AuxAct) == (int)LightBits2.AuxAct);
            azimuthIndicator.InstrumentState.ChaffCount = (int)flightData.ChaffCount;
            azimuthIndicator.InstrumentState.ChaffLow = ((flightData.lightBits2 & (int)LightBits2.ChaffLo) == (int)LightBits2.ChaffLo);
            azimuthIndicator.InstrumentState.EWSDegraded = ((flightData.lightBits2 & (int)LightBits2.Degr) == (int)LightBits2.Degr);
            azimuthIndicator.InstrumentState.EWSDispenseReady = ((flightData.lightBits2 & (int)LightBits2.Rdy) == (int)LightBits2.Rdy);
            azimuthIndicator.InstrumentState.EWSNoGo = (
                ((flightData.lightBits2 & (int)LightBits2.NoGo) == (int)LightBits2.NoGo)
                    ||
                ((flightData.lightBits2 & (int)LightBits2.Degr) == (int)LightBits2.Degr)
                );
            azimuthIndicator.InstrumentState.EWSGo =
                (
                    ((flightData.lightBits2 & (int)LightBits2.Go) == (int)LightBits2.Go)
                        &&
                    !(
                        ((flightData.lightBits2 & (int)LightBits2.NoGo) == (int)LightBits2.NoGo)
                            ||
                        ((flightData.lightBits2 & (int)LightBits2.Degr) == (int)LightBits2.Degr)
                            ||
                        ((flightData.lightBits2 & (int)LightBits2.Rdy) == (int)LightBits2.Rdy)
                        )
                    );


            azimuthIndicator.InstrumentState.FlareCount = (int)flightData.FlareCount;
            azimuthIndicator.InstrumentState.FlareLow = ((flightData.lightBits2 & (int)LightBits2.FlareLo) == (int)LightBits2.FlareLo);
            azimuthIndicator.InstrumentState.Handoff = ((flightData.lightBits2 & (int)LightBits2.HandOff) == (int)LightBits2.HandOff);
            azimuthIndicator.InstrumentState.Launch = ((flightData.lightBits2 & (int)LightBits2.Launch) == (int)LightBits2.Launch);
            azimuthIndicator.InstrumentState.LowAltitudeMode = ((flightData.lightBits2 & (int)LightBits2.AuxLow) == (int)LightBits2.AuxLow);
            azimuthIndicator.InstrumentState.NavalMode = ((flightData.lightBits2 & (int)LightBits2.Naval) == (int)LightBits2.Naval);
            azimuthIndicator.InstrumentState.Other1Count = 0;
            azimuthIndicator.InstrumentState.Other1Low = true;
            azimuthIndicator.InstrumentState.Other2Count = 0;
            azimuthIndicator.InstrumentState.Other2Low = true;
            azimuthIndicator.InstrumentState.RWRPowerOn = ((flightData.lightBits2 & (int)LightBits2.AuxPwr) == (int)LightBits2.AuxPwr);
            azimuthIndicator.InstrumentState.PriorityMode = ((flightData.lightBits2 & (int)LightBits2.PriMode) == (int)LightBits2.PriMode);
            azimuthIndicator.InstrumentState.SearchMode = ((flightData.lightBits2 & (int)LightBits2.AuxSrch) == (int)LightBits2.AuxSrch);
            azimuthIndicator.InstrumentState.SeparateMode = ((flightData.lightBits2 & (int)LightBits2.TgtSep) == (int)LightBits2.TgtSep);
            azimuthIndicator.InstrumentState.UnknownThreatScanMode = ((flightData.lightBits2 & (int)LightBits2.Unk) == (int)LightBits2.Unk);

        }
    }
}
