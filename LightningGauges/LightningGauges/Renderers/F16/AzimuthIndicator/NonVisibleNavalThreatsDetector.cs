namespace LightningGauges.Renderers.F16.AzimuthIndicator
{
    internal static class NonVisibleNavalThreatsDetector
    {
        internal static bool AreNonVisibleNavalThreatsDetected(InstrumentState instrumentState)
        {
            if (instrumentState == null || instrumentState.Blips == null) return false;
            for (var i = 0; i < instrumentState.Blips.Length; i++)
            {
                var thisBlip = instrumentState.Blips[i];
                if (thisBlip == null || thisBlip.Lethality ==0 || thisBlip.Visible) continue;
                var symbolId = thisBlip.SymbolID;
                if (symbolId == 18)
                {
                    return true;
                }
            }
            return false;
        }
    }
}