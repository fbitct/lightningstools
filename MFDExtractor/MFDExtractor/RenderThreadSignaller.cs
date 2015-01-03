using System;
using MFDExtractor.Properties;
using System.Threading;
using System.Collections.Generic;
using MFDExtractor.Renderer;

namespace MFDExtractor
{
    internal interface IRenderThreadSignaller
    {
        void Signal(
            IList<WaitHandle> toWait,
            ExtractorState extractorState,
            IInstrument instrument,
            bool stateIsStale);
    }

    class RenderThreadSignaller : IRenderThreadSignaller
    {
        public void Signal(
            IList<WaitHandle> toWait,
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
                if (!renderOnlyOnStateChanges || stateIsStale || (instrument.Form != null && instrument.Form.RenderImmediately))
                {
                    if ((extractorState.RenderCycleNum % Math.Max(instrument.Form.Settings.RenderEveryN,1) != instrument.Form.Settings.RenderOnN - 1)
                        &&
                        (instrument.Form == null || !instrument.Form.RenderImmediately))
                    {
                        return;
                    }
                    if (instrument.Form != null)
                    {
                        instrument.Form.RenderImmediately = (instrument.Renderer is MfdRenderer);
                    }
                    if (instrument.StartSignal != null)
                    {
                        instrument.StartSignal.Set();
                    }
                    if (instrument.EndSignal != null && toWait !=null)
                    {
                        toWait.Add(instrument.EndSignal);
                    }
                }
            }
        }
    }
}
