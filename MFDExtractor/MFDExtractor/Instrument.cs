using System;
using System.Diagnostics;
using Common.SimSupport;
using MFDExtractor.Properties;
using MFDExtractor.UI;
using System.Threading;
using log4net;
using System.Windows.Forms;
using ThreadState = System.Threading.ThreadState;
using MFDExtractor.Renderer;

namespace MFDExtractor
{
    internal interface IInstrument:IDisposable
    {
        InstrumentType Type { get; }
        IInstrumentRenderer Renderer { get; }
        InstrumentForm Form { get; }
        void Start();
        void Stop();
        PerformanceCounter RenderedFramesCounter { get; }
        PerformanceCounter SkippedFramesCounter { get; }
        PerformanceCounter TimeoutFramesCounter { get; }
        PerformanceCounter TotalFramesCounter { get; }
    }

    internal class Instrument : IInstrument
    {
        private bool _disposed;
        private readonly IInstrumentRenderHelper _instrumentRenderHelper;
        private readonly IInstrumentStateSnapshotCache _instrumentStateSnapshotCache;
        private readonly IInstrumentFormFactory _instrumentFormFactory;
        private Thread _renderThread;
        private readonly ILog _log = LogManager.GetLogger(typeof (Instrument));
        private int _renderCycle;
        private readonly bool _renderOnlyOnStateChanges;
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(2);
        internal Instrument(
            IInstrumentStateSnapshotCache instrumentStateSnapshotCache = null, 
            IInstrumentRenderHelper instrumentRenderHelper = null,
            IInstrumentFormFactory instrumentFormFactory = null
            )
        {
            _instrumentStateSnapshotCache = instrumentStateSnapshotCache ?? new InstrumentStateSnapshotCache();
            _instrumentRenderHelper = instrumentRenderHelper ?? new InstrumentRenderHelper();
            _instrumentFormFactory = instrumentFormFactory ?? new InstrumentFormFactory();
            _renderOnlyOnStateChanges = Settings.Default.RenderInstrumentsOnlyOnStatechanges;
        }
        public InstrumentType Type { get; internal set; }
        public InstrumentForm Form { get; set; }
        public IInstrumentRenderer Renderer { get; internal set; }
        public void Start()
        {
            Form = _instrumentFormFactory.Create(
                Type,
                Renderer
            );

            if (Form != null && !Form.Visible)
            {
                Form.Show();
            }
            if (Form != null && (_renderThread == null || (_renderThread.ThreadState & ThreadState.Stopped) == ThreadState.Stopped))
            {
                Common.Util.DisposeObject(_renderThread);
                if (Form.Visible || (Renderer is IMfdRenderer && Extractor.State.NetworkMode != Common.Networking.NetworkMode.Client))
                {
                    _renderThread = new Thread(() => ThreadWork()) { IsBackground = true, Name = Renderer.GetType().FullName, Priority = Settings.Default.ThreadPriority };
                }
            }

            if (_renderThread != null && (_renderThread.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
            {
                _renderThread.Start();
            }
        }

        public void Stop()
        {
            while (_renderThread !=null && _renderThread.IsAlive)
            {
                Common.Threading.Util.AbortThread(ref _renderThread);
                Application.DoEvents();
                Thread.Sleep(1);
            }
            if (Form != null)
            {
                Form.Hide();
                Form.Close();
                Common.Util.DisposeObject(Form);
                Form = null;
            }
            Common.Util.DisposeObject(_renderThread);
            _renderThread = null;
        }

        private static bool HighlightingBorderShouldBeDisplayedOnTargetForm(InstrumentForm targetForm)
        {
            return targetForm != null && targetForm.SizingOrMovingCursorsAreDisplayed && Settings.Default.HighlightOutputWindows;
        }

        private void ThreadWork()
        {
            var pollingPeriod = Settings.Default.PollingDelay;
            while (Extractor.State.Running && Extractor.State.KeepRunning)
            {
                var startTime = DateTime.Now;
                _renderCycle++;
                if (_renderCycle > 999) _renderCycle = 0;
                if (ShouldRenderNow())
                {
                    try
                    {
                        Render(Extractor.State.NightMode, pollingPeriod);
                        if (Form != null)
                        {
                            Form.RenderImmediately = false;
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error(e.Message, e);
                    }
                }
                else
                {
                    IncrementPerformanceCounter(SkippedFramesCounter);
                    IncrementPerformanceCounter(Extractor.State.SkippedFramesCounter);
                }
                var endTime = DateTime.Now;
                var elapsed = endTime.Subtract(startTime).TotalMilliseconds;
                var toWait = pollingPeriod - elapsed;
                if (toWait < 1) toWait = 1;
                Thread.Sleep((int)toWait);
                IncrementPerformanceCounter(TotalFramesCounter);
                IncrementPerformanceCounter(Extractor.State.TotalFramesCounter);
            }
        }

        private bool ShouldRenderNow()
        {
            if (!(Extractor.State.Running && Extractor.State.KeepRunning))
            {
                return false;
            }

            var stateIsStale = _instrumentStateSnapshotCache.CaptureInstrumentStateSnapshotAndCheckIfStale(Renderer, Form);
            var renderScheduledThisCycle = (_renderCycle % Math.Max(Form.Settings.RenderEveryN, 1) == Form.Settings.RenderOnN - 1);
            var shouldRenderNow= (Form != null)
                   &&
                 (
                       Form.RenderImmediately
                            ||
                    (
                       ((_renderOnlyOnStateChanges && stateIsStale) || !_renderOnlyOnStateChanges)
                            &&
                       (
                            
                            (Form.Settings != null && Form.Settings.Enabled)
                                &&
                            renderScheduledThisCycle
                       )
                    )
                 );
            return shouldRenderNow;
        }
        private void Render(bool nightMode, int timeout)
        {
            var success = Semaphore.Wait(timeout);
            if (!success)
            {
                IncrementPerformanceCounter(TimeoutFramesCounter);
                IncrementPerformanceCounter(Extractor.State.TimeoutFramesCounter);
                return;
            };
            try
            {
                _instrumentRenderHelper.Render(Renderer, Form, Form.Rotation, Form.Monochrome,
                    HighlightingBorderShouldBeDisplayedOnTargetForm(Form), nightMode);
                IncrementPerformanceCounter(RenderedFramesCounter);
                IncrementPerformanceCounter(Extractor.State.RenderedFramesCounter);
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private void IncrementPerformanceCounter(PerformanceCounter performanceCounter)
        {
            if (performanceCounter == null) return;
            try
            {
                performanceCounter.Increment();
            }
            catch
            {
            }
        }

        public PerformanceCounter RenderedFramesCounter { get; set; }
        public PerformanceCounter SkippedFramesCounter { get; set; }
        public PerformanceCounter TotalFramesCounter { get; set; }
        public PerformanceCounter TimeoutFramesCounter { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();
                    Form.Close();
                    Common.Util.DisposeObject(Form);

                    Common.Util.DisposeObject(_renderThread);
                    _renderThread = null;

                    Common.Util.DisposeObject(Renderer);
                    Renderer = null;

                    Common.Util.DisposeObject(RenderedFramesCounter);
                    Common.Util.DisposeObject(SkippedFramesCounter);
                    Common.Util.DisposeObject(TotalFramesCounter);

                }
            }
            _disposed = true;
        }

        ~Instrument()
        {
            Dispose(false);
        }

    }
}
