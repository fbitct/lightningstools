using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Common.SimSupport;
using MFDExtractor.Properties;
using MFDExtractor.UI;
using System.Threading;

namespace MFDExtractor
{
    internal interface IInstrument
    {
        InstrumentForm Form { get;}
        IInstrumentRenderer Renderer { get; }
        AutoResetEvent StartSignal { get; }
        AutoResetEvent EndSignal { get; }
        void Start(ExtractorState extractorState );
        void Signal(IList<WaitHandle> waitHandles, ExtractorState extractorState);
    }

    class Instrument : IInstrument
    {
        private readonly IInstrumentRenderHelper _instrumentRenderHelper;
        private readonly IRenderThreadFactory _renderThreadFactory;
        private readonly IRenderThreadSignaller _renderThreadSignaller;
        private readonly IInstrumentStateSnapshotCache _instrumentStateSnapshotCache;
        private Thread _renderThread;
        internal Instrument(
            IInstrumentStateSnapshotCache instrumentStateSnapshotCache = null, 
            IInstrumentRenderHelper instrumentRenderHelper = null, 
            IRenderThreadFactory renderThreadFactory = null, 
            IRenderThreadSignaller renderThreadSignaller = null
            )
        {
            _instrumentStateSnapshotCache = instrumentStateSnapshotCache ?? new InstrumentStateSnapshotCache();
            _instrumentRenderHelper = instrumentRenderHelper ?? new InstrumentRenderHelper();
            _renderThreadFactory = renderThreadFactory ?? new RenderThreadFactory();
            _renderThreadSignaller = renderThreadSignaller ?? new RenderThreadSignaller();

        }
        public InstrumentType Type { get; internal set; }
        public InstrumentForm Form { get; internal set; }
        public IInstrumentRenderer Renderer { get; internal set; }
        public AutoResetEvent StartSignal { get; internal set; }
        public AutoResetEvent EndSignal { get; internal set; }
        public void Start(ExtractorState extractorState )
        {
            _renderThreadFactory.CreateOrRecycle(ref _renderThread, 
                Settings.Default.ThreadPriority, null,
                () => Form !=null && Form.Settings !=null && Form.Settings.Enabled, 
                ()=>ThreadWork(extractorState));
            if (_renderThread != null)
            {
                _renderThread.Start();
            }
        }

        public void Signal(IList<WaitHandle> waitHandles, ExtractorState extractorState  )
        {
            _renderThreadSignaller.Signal(waitHandles, extractorState, this,IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted());
        }

        private bool IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted()
        {
            var stateIsStale = _instrumentStateSnapshotCache.CaptureInstrumentStateSnapshotAndCheckIfStale(Renderer, Form);
            return stateIsStale || HighlightingBorderShouldBeDisplayedOnTargetForm(Form);
        }

        private void ThreadWork(ExtractorState extractorState)
        {
            try
            {
                while (extractorState.KeepRunning)
                {
                    StartSignal.WaitOne();
                    Render(extractorState.NightMode);
                    EndSignal.Set();
                }
            }
            catch (ThreadAbortException) { }
            catch (ThreadInterruptedException) { }
        }

        private void Render(bool nightMode)
        {
            var startTime = DateTime.Now;
            if (DateTime.Now.Subtract(Form.LastRenderedOn).TotalMilliseconds < Settings.Default.PollingDelay)
            {
                return;
            }
            _instrumentRenderHelper.Render(Renderer, Form, Form.Rotation, Form.Monochrome, HighlightingBorderShouldBeDisplayedOnTargetForm(Form), nightMode);
            var endTime = DateTime.Now;
            var elapsed = endTime.Subtract(startTime);
            if (!(elapsed.TotalMilliseconds < 0)) return;
            var toWait = new TimeSpan(0, 0, 0, 0, (int)(0 - elapsed.TotalMilliseconds));
            if (toWait.TotalMilliseconds < 0)
            {
                toWait = new TimeSpan(0, 0, 0, 0, 0);
            }
            Thread.Sleep(toWait);
        }
        private void RecoverInstrumentForm(Screen screen)
        {
            Form.StretchToFill = false;
            Form.Location = screen.Bounds.Location;
            Form.BringToFront();
        }

        private static bool HighlightingBorderShouldBeDisplayedOnTargetForm(InstrumentForm targetForm)
        {
            return targetForm != null && targetForm.SizingOrMovingCursorsAreDisplayed && Settings.Default.HighlightOutputWindows;
        }
    }
}
