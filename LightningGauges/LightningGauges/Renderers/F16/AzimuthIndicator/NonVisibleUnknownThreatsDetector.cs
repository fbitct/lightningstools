namespace LightningGauges.Renderers.F16.AzimuthIndicator
{
    internal static class NonVisibleUnknownThreatsDetector
    {
        internal static bool AreNonVisibleUnknownThreatsDetected(InstrumentState instrumentState)
        {
            if (instrumentState == null || instrumentState.Blips == null) return false;
            for (var i = 0; i < instrumentState.Blips.Length; i++)
            {
                var thisBlip = instrumentState.Blips[i];
                if (thisBlip == null || thisBlip.Visible || thisBlip.Lethality ==0) continue;
                var symbolId = thisBlip.SymbolID;
                if (symbolId < 0 || symbolId == 1 || symbolId == 27 || symbolId == 28 || symbolId == 29)
                {
                    return true;
                }
            }
            return false;
        }
    }
}