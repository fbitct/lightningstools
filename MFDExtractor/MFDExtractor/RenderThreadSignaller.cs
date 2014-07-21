using System;
using System.Collections.Generic;
using System.Threading;
using MFDExtractor.Properties;

namespace MFDExtractor
{
    internal interface IRenderThreadSignaller
    {
        void Signal(
            ExtractorState extractorState,
            IInstrument instrument,
            bool stateIsStale);
    }

    class RenderThreadSignaller : IRenderThreadSignaller
    {
        public void Signal(
            ExtractorState extractorState,
            IInstrument instrument,
            bool stateIsStale
            )
        {
            if (!(extractorState.Running && extractorState.KeepRunning))
            {
                return;
            }

            var renderOnlyOnStateChanges = Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (instrument.Form != null && instrument.Form.Settings != null && instrument.Form.Settings.Enabled)
            {
                if (
                    //extractorState.TestMode || 
                    !renderOnlyOnStateChanges || stateIsStale || (instrument.Form != null && instrument.Form.RenderImmediately))
                {
                    if ((extractorState.RenderCycleNum % Math.Max(instrument.Form.Settings.RenderEveryN,1) != instrument.Form.Settings.RenderOnN - 1)
                        &&
                        (instrument.Form == null || !instrument.Form.RenderImmediately))
                    {
                        return;
                    }
                    if (instrument.Form != null)
                    {
                        instrument.Form.RenderImmediately = false;
                    }
                    if (instrument.StartSignal != null)
                    {
                        instrument.StartSignal.Set();
                    }
                }
            }
        }
    }
}
