using System.Collections.Generic;
using System.Threading;
using MFDExtractor.Properties;
using MFDExtractor.UI;

namespace MFDExtractor
{
    internal interface IRenderStartHelper
    {
        void Start(
            List<WaitHandle> toWait,
            bool isRunning,
            bool keepRunning,
            bool enabled,
            long renderEveryN,
            long renderOnN,
            InstrumentForm instrumentForm,
            EventWaitHandle threadStartSignal,
            WaitHandle threadEndSignal,
            bool testMode,
            long renderCycleNumber,
            bool stateIsStale
            );
    }

    class RenderStartHelper : IRenderStartHelper
    {
        public void Start(
            List<WaitHandle> toWait,
            bool isRunning,
            bool keepRunning,
            bool enabled,
            long renderEveryN,
            long renderOnN,
            InstrumentForm instrumentForm,
            EventWaitHandle threadStartSignal,
            WaitHandle threadEndSignal,
            bool testMode,
            long renderCycleNumber,
            bool stateIsStale
            )
        {
            if (!(isRunning && keepRunning))
            {
                return;
            }

            var renderOnlyOnStateChanges = Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (enabled)
            {
                if (testMode || !renderOnlyOnStateChanges || stateIsStale || (instrumentForm != null && instrumentForm.RenderImmediately))
                {
                    if ((renderCycleNumber% renderEveryN != renderOnN - 1)
                        &&
                        (instrumentForm == null || !instrumentForm.RenderImmediately))
                    {
                        return;
                    }
                    if (instrumentForm != null)
                    {
                        instrumentForm.RenderImmediately = false;
                    }
                    if (threadStartSignal != null)
                    {
                        threadStartSignal.Set();
                    }
                    if (threadEndSignal != null)
                    {
                        toWait.Add(threadEndSignal);
                    }
                }
            }
        }
    }
}
