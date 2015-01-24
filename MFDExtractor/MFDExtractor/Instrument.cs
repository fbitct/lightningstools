using System;
using Common.SimSupport;
using MFDExtractor.Properties;
using MFDExtractor.UI;
using System.Threading;
using log4net;
using System.Windows.Forms;

namespace MFDExtractor
{
    internal interface IInstrument:IDisposable
    {
        InstrumentType Type { get; }
        IInstrumentRenderer Renderer { get; }
        void Start(ExtractorState extractorState );
        void Stop();
    }

    internal class Instrument : IInstrument
    {
        private bool _disposed;
        private readonly IInstrumentRenderHelper _instrumentRenderHelper;
        private readonly IInstrumentStateSnapshotCache _instrumentStateSnapshotCache;
        private readonly IInstrumentFormFactory _instrumentFormFactory;
        private Thread _renderThread;
        private readonly ILog _log = LogManager.GetLogger(typeof (Instrument));
        private ExtractorState _extractorState;
        private int _renderCycle;
        private readonly bool _renderOnlyOnStateChanges;
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);
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
        private InstrumentForm Form { get; set; }
        public IInstrumentRenderer Renderer { get; internal set; }
        public void Start(ExtractorState extractorState )
        {
            _extractorState = extractorState;
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
                _renderThread = new Thread(()=>ThreadWork(extractorState)) { IsBackground = true, Name = Renderer.GetType().FullName, Priority= Settings.Default.ThreadPriority };
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
                Application.DoEvents();
                Thread.Sleep(1);
            }
            if (Form != null)
            {
                Form.Hide();
                Form.Close();
                Common.Util.DisposeObject(Form);
            }
            Common.Util.DisposeObject(_renderThread);
        }

        private static bool HighlightingBorderShouldBeDisplayedOnTargetForm(InstrumentForm targetForm)
        {
            return targetForm != null && targetForm.SizingOrMovingCursorsAreDisplayed && Settings.Default.HighlightOutputWindows;
        }

        private void ThreadWork(ExtractorState extractorState)
        {
            while (_extractorState.Running && _extractorState.KeepRunning)
            {
                var startTime = DateTime.Now;
                _renderCycle++;
                if (_renderCycle > 999) _renderCycle = 0;
                if (ShouldRenderNow())
                {
                    try
                    {
                        Render(extractorState.NightMode);
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
                var endTime = DateTime.Now;
                var elapsed = endTime.Subtract(startTime).TotalMilliseconds;
                var toWait = Settings.Default.PollingDelay - elapsed;
                if (toWait < 1) toWait = 1;
                Thread.Sleep((int)toWait);
            }
        }

        private bool ShouldRenderNow()
        {
            if (!(_extractorState.Running && _extractorState.KeepRunning))
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
        private void Render(bool nightMode)
        {
            try
            {
                Semaphore.Wait();
                _instrumentRenderHelper.Render(Renderer, Form, Form.Rotation, Form.Monochrome, HighlightingBorderShouldBeDisplayedOnTargetForm(Form), nightMode);
            }
            catch (Exception e) { _log.Error(e.Message, e); }
            finally
            {
                Semaphore.Release();
            }
        }

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
