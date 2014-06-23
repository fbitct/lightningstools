using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Windows.Forms;
using Common.Collections;
using Common.InputSupport;
using Common.InputSupport.DirectInput;
using Common.Win32;
using F16CPD.Mfd.Controls;
using F16CPD.Properties;
using F16CPD.SimSupport;
using F16CPD.SimSupport.Falcon4;
using F16CPD.UI.Forms;
using log4net;
using Microsoft.DirectX.DirectInput;

namespace F16CPD
{
    [Synchronization]
    internal partial class F16CpdEngine : MfdForm, IDisposable
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (F16CpdEngine));
        private readonly Bitmap _bezel = Resources.cpdbezel;
        private readonly FlightDataValuesSimulator _flightDataValuesSimulator = new FlightDataValuesSimulator();
        private readonly IControlBindingsLoader _controlBindingsLoader = new ControlBindingsLoader();
        private SerializableDictionary<CpdInputControls, ControlBinding> _controlBindings =
            new SerializableDictionary<CpdInputControls, ControlBinding>();
        private IDirectInputEventHandler _directInputEventHandler;
        private IKeyboardWatcher _keyboardWatcher = new KeyboardWatcher();
        private IMouseClickHandler _mouseClickHandler = new MouseClickHandler();

        private bool _disposing;
        private bool _isDisposed;

        private bool _keepRunning;
        private F16CpdMfdManager _manager;
        private Mediator _mediator;
        private Mediator.PhysicalControlStateChangedEventHandler _mediatorHandler;
        private Bitmap _renderTarget;
        private ISimSupportModule _simSupportModule;
        private bool _testCalledOnce;

        public F16CpdEngine()
        {
            InitializeComponent();
        }

        public bool TestMode { get; set; }

        private Bitmap CreateRenderTarget()
        {
            var rotation = Settings.Default.Rotation;
            if (rotation == RotateFlipType.Rotate180FlipNone || rotation == RotateFlipType.RotateNoneFlipNone)
            {
                return new Bitmap(ClientRectangle.Width, ClientRectangle.Height, PixelFormat.Format16bppRgb565);
            }
            return new Bitmap(ClientRectangle.Height, ClientRectangle.Width, PixelFormat.Format16bppRgb565);
        }

        private void InitializeInternal()
        {
            if (_mediator != null)
            {
                _mediator.PhysicalControlStateChanged -= _mediatorHandler;
                Common.Util.DisposeObject(_mediator);
            }
            _mediator = new Mediator(this) {RaiseEvents = true};
            _controlBindingsLoader.LoadControlBindings(_mediator);
            UpdateMfdManagerSize();
            _mediatorHandler =
                new Mediator.PhysicalControlStateChangedEventHandler(_directInputEventHandler.MediatorPhysicalControlStateChanged);
            _mediator.PhysicalControlStateChanged += _mediatorHandler;
            foreach (var deviceMonitor in _mediator.DeviceMonitors.Values)
            {
                deviceMonitor.Poll();
            }
            _keyboardWatcher.Start(new KeyDownEventHandler(_controlBindings, _manager));
            _mouseClickHandler.Start(_manager, this, delegate() { RenderOnce(Settings.Default.PollingFrequencyMillis); });

        }

        public void Start()
        {
            GC.Collect();
            InitializeInternal();
            _renderTarget = CreateRenderTarget();
            _keepRunning = true;
            if (Settings.Default.RunAsClient)
            {
                if (_manager != null && _manager.Client != null)
                {
                    _manager.Client.ClearPendingClientMessages();
                }
            }

            if (!Settings.Default.RunAsServer)
            {
                //if in client or standalone mode
                Show();
                var size = new Size(Settings.Default.CpdWindowWidth, Settings.Default.CpdWindowHeight);
                var location = new Point(Settings.Default.CpdWindowX, Settings.Default.CpdWindowY);
                SetClientSizeCore(size.Width, size.Height);
                Draggable = true;
                BackColor = Color.Green;
                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.None;
                TopMost = true;
                Location = location;
                SetClientSizeCore(size.Width, size.Height);
                UpdateMfdManagerSize();
                UpdateMfdManagerSize();
                TransparencyKey = Color.AntiqueWhite;
            }
            else
            {
                Hide();
            }
            var pollingFrequencyMillis = Settings.Default.PollingFrequencyMillis;
            if (pollingFrequencyMillis < 15) pollingFrequencyMillis = 15;
            while (_keepRunning)
            {
                try
                {
                    RenderOnce(pollingFrequencyMillis);
                }
                catch (Exception e)
                {
                    _log.Error(e.Message, e);
                }
            }
        }

        private int _cycle;
        private void RenderOnce(int pollingFrequencyMillis)
        {
            if (_isDisposed || !_keepRunning) return;
            var loopStartTime = DateTime.Now;
            UpdateMfdManagerSize();
            Render();
            if (_cycle %10==0) Application.DoEvents();
            _cycle++;
            var loopEndTime = DateTime.Now;
            var elapsed = loopEndTime.Subtract(loopStartTime);
            var wait = (int) (pollingFrequencyMillis - elapsed.TotalMilliseconds);
            if (wait < 1) wait = 1;
            if (!Settings.Default.RunAsClient && !_simSupportModule.IsSimRunning) wait = 350;
            Thread.Sleep(wait);
        }

        protected void UpdateMfdManagerSize()
        {
            var rotation = Settings.Default.Rotation;
            var oldWidth = DesktopBounds.Width;
            var oldHeight = DesktopBounds.Height;

            int newWidth;
            int newHeight;
            if (rotation == RotateFlipType.RotateNoneFlipNone || rotation == RotateFlipType.Rotate180FlipNone)
            {
                newWidth = Math.Min(oldHeight, oldWidth);
                newHeight = Math.Max(oldHeight, oldWidth);
            }
            else
            {
                newWidth = Math.Max(oldHeight, oldWidth);
                newHeight = Math.Min(oldHeight, oldWidth);
            }
            Width = newWidth;
            Height = newHeight;

            if (_manager == null)
            {
                CreateMfdManager(rotation);
            }
            else
            {
                UpdateMfdManagerScreenBounds(rotation);
            }
            LoadSimSupportModule();

            return;
        }

        private void LoadSimSupportModule()
        {
            if (_simSupportModule == null)
            {
                _simSupportModule = new Falcon4Support(_manager, _mediator);
                _manager.SimSupportModule = _simSupportModule;
            }
        }

        private void UpdateMfdManagerScreenBounds(RotateFlipType rotation)
        {
            if (rotation == RotateFlipType.Rotate180FlipNone || rotation == RotateFlipType.RotateNoneFlipNone)
            {
                _manager.ScreenBoundsPixels = new Size(DesktopBounds.Width, DesktopBounds.Height);
            }
            else
            {
                _manager.ScreenBoundsPixels = new Size(DesktopBounds.Height, DesktopBounds.Width);
            }
        }

        private void CreateMfdManager(RotateFlipType rotation)
        {
            if (rotation == RotateFlipType.Rotate180FlipNone || rotation == RotateFlipType.RotateNoneFlipNone)
            {
                _manager = new F16CpdMfdManager(new Size(DesktopBounds.Width, DesktopBounds.Height));
            }
            else
            {
                _manager = new F16CpdMfdManager(new Size(DesktopBounds.Height, DesktopBounds.Width));
            }
            _directInputEventHandler = new DirectInputEventHandler(_controlBindings, _manager);
            _keyboardWatcher.Start(new KeyDownEventHandler(_controlBindings, _manager));
        }

        public void Stop()
        {
            _keepRunning = false;
            Application.DoEvents();
            Thread.Sleep(1000);
            _keyboardWatcher.Stop();
            Close();
            Dispose();
            GC.Collect();
        }

        protected void Render()
        {
            if (_isDisposed || !_keepRunning || _disposing) return;

            try
            {
                if (TestMode)
                {
                    if (!_testCalledOnce)
                    {
                        _simSupportModule.InitializeTestMode();
                        _manager.FlightData = _flightDataValuesSimulator.GetInitialFlightData();

                        _testCalledOnce = true;
                    }
                    else
                    {
                        _manager.FlightData = _flightDataValuesSimulator.GetNextFlightData(_manager.FlightData);
                    }
                }
                else
                {
                    _simSupportModule.UpdateManagerFlightData();
                }
                _manager.ProcessPendingMessages();
                if (Settings.Default.RunAsServer)
                {
                    return; //no rendering needed in server mode
                }
                if (_disposing) return;
                using (var h = Graphics.FromImage(_renderTarget))
                {
                    h.Clear(Color.Black);
                    //h.CompositingQuality = CompositingQuality.HighQuality;
                    h.SmoothingMode = SmoothingMode.AntiAlias;
                    h.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    h.TextContrast = 10;
                    var origTransform = h.Transform;
                    //h.SetClip(this.DisplayRectangle);
                    _manager.Render(h);
                    h.Transform = origTransform;

                    var outerBounds = DesktopBounds;
                    var innerBounds = DesktopBounds;
                    innerBounds.Inflate(-7, -7);
                    var curPos = Cursor.Position;

                    if ((outerBounds.Contains(curPos) && !innerBounds.Contains(curPos)) ||
                        (Cursor == Cursors.SizeAll && Drag))
                    {
                        var greenPen = new Pen(Color.Green) {Width = 7};
                        var renderTargetBounds = new Rectangle(0, 0, _renderTarget.Width, _renderTarget.Height);
                        h.DrawRectangle(greenPen, renderTargetBounds);
                    }


                    var attrs = new ImageAttributes();
                    ColorMatrix cm;
                    if (_manager.NightMode)
                    {
                        cm = new ColorMatrix
                            (
                            new[]
                                {
                                    new float[] {0, 0, 0, 0, 0}, //red %
                                    new[]
                                        {
                                            0,
                                            (_manager.Brightness/(float) _manager.MaxBrightness),
                                            0, 0, 0
                                        }, //green
                                    new float[] {0, 0, 0, 0, 0}, //blue %
                                    new float[] {0, 0, 0, 1, 0}, //alpha %
                                    new float[] {-1, 0, -1, 0, 1}, //add
                                }
                            );
                    }
                    else
                    {
                        cm = new ColorMatrix
                            (
                            new[]
                                {
                                    new[] {_manager.Brightness/(float) _manager.MaxBrightness, 0, 0, 0, 0}, //red %
                                    new[] {0, _manager.Brightness/(float) _manager.MaxBrightness, 0, 0, 0}, //green %
                                    new[] {0, 0, _manager.Brightness/(float) _manager.MaxBrightness, 0, 0}, //blue %
                                    new float[] {0, 0, 0, 1, 0}, //alpha %
                                    new float[] {0, 0, 0, 0, 1}, //add
                                }
                            );
                    }
                    attrs.SetColorMatrix(cm, ColorMatrixFlag.Default);

                    using (var formGraphics = CreateGraphics())
                    {
                        formGraphics.CompositingQuality = CompositingQuality.HighQuality;
                        formGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        formGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        formGraphics.SmoothingMode = SmoothingMode.HighQuality;
                        formGraphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                        var rotation = Settings.Default.Rotation;
                        if (rotation == RotateFlipType.RotateNoneFlipNone)
                        {
                            formGraphics.DrawImage(_renderTarget, DisplayRectangle, 0, 0, _renderTarget.Width,
                                                   _renderTarget.Height, GraphicsUnit.Pixel, attrs);
                        }
                        else
                        {
                            using (var rotated = (Bitmap) _renderTarget.Clone())
                            {
                                rotated.RotateFlip(rotation);
                                formGraphics.DrawImage(rotated, DisplayRectangle, 0, 0, rotated.Width, rotated.Height,
                                                       GraphicsUnit.Pixel, attrs);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
        }


        

        private void F16CpdEngine_LocationChanged(object sender, EventArgs e)
        {
            Settings.Default.CpdWindowX = Location.X;
            Settings.Default.CpdWindowY = Location.Y;
            Util.SaveCurrentProperties();
        }

        private void F16CpdEngine_SizeChanged(object sender, EventArgs e)
        {
            UpdateMfdManagerSize();
            Settings.Default.CpdWindowWidth = DesktopBounds.Width;
            Settings.Default.CpdWindowHeight = DesktopBounds.Height;
            Util.SaveCurrentProperties();

            Common.Util.DisposeObject(_renderTarget);
            _renderTarget = CreateRenderTarget();
        }

        #region Destructors

        public new void Dispose()
        {
            Dispose(true);
            try
            {
                base.Dispose(true);
            }
            catch (ObjectDisposedException e)
            {
                _log.Debug(e.Message, e);
            }
            GC.SuppressFinalize(this);
        }

        ~F16CpdEngine()
        {
            Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _disposing = true;
                    if (components != null) components.Dispose();
                    try
                    {
                        if (_mediator != null)
                        {
                            _mediator.PhysicalControlStateChanged -= _mediatorHandler;
                        }
                        _mouseClickHandler.Stop();
                    }
                    catch (Exception e)
                    {
                        _log.Debug(e.Message, e);
                    }
                    //dispose of managed resources here
                    Common.Util.DisposeObject(_simSupportModule);
                    Common.Util.DisposeObject(_bezel);
                    Common.Util.DisposeObject(_mediator);
                    Common.Util.DisposeObject(_manager);
                    Common.Util.DisposeObject(_controlBindings);
                    Common.Util.DisposeObject(_renderTarget);
                    _keyboardWatcher.Stop();
                }
                base.Dispose(disposing);
            }
            // Code to dispose the un-managed resources of the class
            _isDisposed = true;
        }

        #endregion

        #region Nested type: RECT

        public struct RECT
        {
            public int Bottom;
            public int Left;
            public int Right;
            public int Top;
        }

        #endregion
    }
}