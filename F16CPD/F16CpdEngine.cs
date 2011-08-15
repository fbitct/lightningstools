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

        private SerializableDictionary<CpdInputControls, ControlBinding> _controlBindings =
            new SerializableDictionary<CpdInputControls, ControlBinding>();

        private bool _disposing;
        private double _heightFactor = 4;
        private bool _isDisposed;

        private bool _keepRunning;
        private Thread _keyboardWatcherThread;
        private F16CpdMfdManager _manager;
        private Mediator _mediator;
        private Mediator.PhysicalControlStateChangedEventHandler _mediatorHandler;
        private bool _mouseDown;
        private DateTime? _mouseDownTime;
        private Bitmap _renderTarget;
        private ISimSupportModule _simSupportModule;
        private bool _testCalledOnce;
        private double _widthFactor = 3;

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

        [DebuggerStepThrough]
        private void InitializeInternal()
        {
            if (_mediator != null)
            {
                _mediator.PhysicalControlStateChanged -= _mediatorHandler;
                Common.Util.DisposeObject(_mediator);
            }
            _mediator = new Mediator(this) {RaiseEvents = true};
            LoadControlBindings();
            _mediatorHandler =
                new Mediator.PhysicalControlStateChangedEventHandler(MediatorPhysicalControlStateChanged);
            _mediator.PhysicalControlStateChanged += _mediatorHandler;
            foreach (var deviceMonitor in _mediator.DeviceMonitors.Values)
            {
                deviceMonitor.Poll();
            }
        }

        private void SetupKeyboardWatcherThread()
        {
            AbortThread(ref _keyboardWatcherThread);
            _keyboardWatcherThread = new Thread(KeyboardWatcherThreadWork);
            _keyboardWatcherThread.SetApartmentState(ApartmentState.STA);
            _keyboardWatcherThread.Priority = ThreadPriority.Highest;
            _keyboardWatcherThread.IsBackground = true;
            _keyboardWatcherThread.Name = "KeyboardWatcherThread";
            _keyboardWatcherThread.Start();
        }

        private static void AbortThread(ref Thread t)
        {
            if (t == null) return;
            try
            {
                t.Abort();
            }
            catch
            {
            }
            Common.Util.DisposeObject(t);
            t = null;
        }

        private Keys UpdateKeyEventArgsWithExtendedKeyInfo(Keys keys)
        {
            if ((NativeMethods.GetKeyState(NativeMethods.VK_SHIFT) & 0x8000) != 0)
            {
                keys |= Keys.Shift;
                //SHIFT is pressed
            }
            if ((NativeMethods.GetKeyState(NativeMethods.VK_CONTROL) & 0x8000) != 0)
            {
                keys |= Keys.Control;
                //CONTROL is pressed
            }
            if ((NativeMethods.GetKeyState(NativeMethods.VK_MENU) & 0x8000) != 0)
            {
                keys |= Keys.Alt;
                //ALT is pressed
            }
            return keys;
        }

        private void KeyboardWatcherThreadWork()
        {
            AutoResetEvent resetEvent;
            Device device = null;
            try
            {
                resetEvent = new AutoResetEvent(false);
                device = new Device(SystemGuid.Keyboard);
                device.SetCooperativeLevel(null, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
                device.SetEventNotification(resetEvent);
                device.Properties.BufferSize = 255;
                device.Acquire();
                var lastKeyboardState = new bool[Enum.GetValues(typeof (Key)).Length];
                var currentKeyboardState = new bool[Enum.GetValues(typeof (Key)).Length];
                while (_keepRunning)
                {
                    resetEvent.WaitOne();
                    try
                    {
                        var curState = device.GetCurrentKeyboardState();
                        var possibleKeys = Enum.GetValues(typeof (Key));

                        var i = 0;
                        foreach (Key thisKey in possibleKeys)
                        {
                            currentKeyboardState[i] = curState[thisKey];
                            i++;
                        }

                        i = 0;
                        foreach (Key thisKey in possibleKeys)
                        {
                            var isPressedNow = currentKeyboardState[i];
                            var wasPressedBefore = lastKeyboardState[i];
                            if (isPressedNow && !wasPressedBefore)
                            {
                                var winFormsKey =
                                    (Keys) NativeMethods.MapVirtualKey((uint) thisKey, NativeMethods.MAPVK_VSC_TO_VK_EX);

                                KeyDownHandler(this, new KeyEventArgs(winFormsKey));
                            }
                            i++;
                        }
                        Array.Copy(currentKeyboardState, lastKeyboardState, currentKeyboardState.Length);
                    }
                    catch (Exception e)
                    {
                        _log.Debug(e.Message, e);
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
            finally
            {
                if (device != null)
                {
                    device.Unacquire();
                }
                Common.Util.DisposeObject(device);
                device = null;
            }
        }

        protected void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (_manager == null || _manager.SimSupportModule == null || !_manager.SimSupportModule.IsSimRunning ||
                _manager.SimSupportModule.IsSendingInput)
            {
                return;
            }
            var keyDown = e.KeyCode;


            if ((NativeMethods.GetKeyState(NativeMethods.VK_SHIFT) & 0x8000) != 0)
            {
                keyDown |= Keys.Shift;
                //SHIFT is pressed
            }
            if ((NativeMethods.GetKeyState(NativeMethods.VK_CONTROL) & 0x8000) != 0)
            {
                keyDown |= Keys.Control;
                //CONTROL is pressed
            }
            if ((NativeMethods.GetKeyState(NativeMethods.VK_MENU) & 0x8000) != 0)
            {
                keyDown |= Keys.Alt;
                //ALT is pressed
            }

            foreach (var binding in _controlBindings.Values)
            {
                if (binding != null && binding.BindingType == BindingType.Keybinding)
                {
                    if ((binding.Keys & Keys.KeyCode) != Keys.None)
                    {
                        if (
                            ((keyDown & Keys.KeyCode) == (binding.Keys & Keys.KeyCode))
                            &&
                            ((keyDown & Keys.Modifiers) == (binding.Keys & Keys.Modifiers))
                            )
                        {
                            var controlType = binding.CpdInputControl;
                            if (controlType != CpdInputControls.Unknown)
                            {
                                {
                                    _manager.FireHandler(controlType);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void MediatorPhysicalControlStateChanged(object sender, PhysicalControlStateChangedEventArgs e)
        {
            if (e == null || e.Control == null || e.Control.Parent == null) return;
            if (e.Control.ControlType == ControlType.Axis || e.Control.ControlType == ControlType.Unknown) return;
            if (_controlBindings != null)
            {
                foreach (var binding in _controlBindings.Values)
                {
                    if (binding.BindingType == BindingType.Unknown || binding.BindingType == BindingType.Keybinding)
                        continue;
                    var control = (DIPhysicalControlInfo) e.Control;
                    var device = (DIPhysicalDeviceInfo) e.Control.Parent;
                    if (binding.DirectInputDevice != null && device.Guid != Guid.Empty &&
                        binding.DirectInputDevice.Guid != Guid.Empty && device.Guid == binding.DirectInputDevice.Guid)
                    {
                        //something on a device we're interested in just changed
                        if (control.Equals(binding.DirectInputControl))
                        {
                            if (binding.BindingType == BindingType.DirectInputButtonBinding &&
                                control.ControlType == ControlType.Button)
                            {
                                if (e.CurrentState == 1 && e.PreviousState != 1)
                                {
                                    //this button was just pressed, so fire the attached event
                                    if (_manager != null) _manager.FireHandler(binding.CpdInputControl);
                                }
                                break;
                            }
                            if (binding.BindingType == BindingType.DirectInputPovBinding &&
                                control.ControlType == ControlType.Pov)
                            {
                                var currentDegrees = e.CurrentState/100.0f;
                                if (e.CurrentState == -1) currentDegrees = -1;
                                /*  POV directions in degrees
                                          0
                                    337.5  22.5   
                                   315         45
                                 292.5           67.5
                                270                90
                                 247.5           112.5
                                  225          135
                                    202.5  157.5
                                        180
                                 */
                                PovDirections? direction = null;
                                if ((currentDegrees > 337.5 && currentDegrees <= 360) ||
                                    (currentDegrees >= 0 && currentDegrees <= 22.5))
                                {
                                    direction = PovDirections.Up;
                                }
                                else if (currentDegrees > 22.5 && currentDegrees <= 67.5)
                                {
                                    direction = PovDirections.UpRight;
                                }
                                else if (currentDegrees > 67.5 && currentDegrees <= 112.5)
                                {
                                    direction = PovDirections.Right;
                                }
                                else if (currentDegrees > 112.5 && currentDegrees <= 157.5)
                                {
                                    direction = PovDirections.DownRight;
                                }
                                else if (currentDegrees > 157.5 && currentDegrees <= 202.5)
                                {
                                    direction = PovDirections.Down;
                                }
                                else if (currentDegrees > 202.5 && currentDegrees <= 247.5)
                                {
                                    direction = PovDirections.DownLeft;
                                }
                                else if (currentDegrees > 247.5 && currentDegrees <= 292.5)
                                {
                                    direction = PovDirections.Left;
                                }
                                else if (currentDegrees > 292.5 && currentDegrees <= 337.5)
                                {
                                    direction = PovDirections.UpLeft;
                                }

                                if (direction != null && direction == binding.PovDirection)
                                {
                                    if (_manager != null) _manager.FireHandler(binding.CpdInputControl);
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }

        protected void LoadControlBindings()
        {
            if (_controlBindings == null)
                _controlBindings = new SerializableDictionary<CpdInputControls, ControlBinding>();
            _controlBindings.Clear();
            foreach (CpdInputControls val in Enum.GetValues(typeof (CpdInputControls)))
            {
                _controlBindings.Add(val, new ControlBinding());
            }

            var bindings = Settings.Default.ControlBindings;
            if (!String.IsNullOrEmpty(bindings))
            {
                var deserialized = Common.Serialization.Util.DeserializeFromXml(bindings, _controlBindings.GetType());
                if (deserialized != null)
                {
                    var asBindings = (SerializableDictionary<CpdInputControls, ControlBinding>) deserialized;
                    foreach (var entry in asBindings)
                    {
                        var thisEntry = entry.Value;
                        if (thisEntry.BindingType == BindingType.DirectInputAxisBinding ||
                            thisEntry.BindingType == BindingType.DirectInputButtonBinding ||
                            thisEntry.BindingType == BindingType.DirectInputPovBinding)
                        {
                            if (_mediator.DeviceMonitors.ContainsKey(thisEntry.DirectInputDevice.Guid))
                            {
                                var thisBinding = _controlBindings[entry.Key];
                                thisBinding.BindingType = thisEntry.BindingType;
                                thisBinding.ControlName = thisEntry.ControlName;
                                thisBinding.Keys = thisEntry.Keys;
                                thisBinding.PovDirection = thisEntry.PovDirection;
                                thisBinding.CpdInputControl = thisEntry.CpdInputControl;
                                thisBinding.DirectInputDevice =
                                    _mediator.DeviceMonitors[thisEntry.DirectInputDevice.Guid].DeviceInfo;
                                foreach (var control in thisBinding.DirectInputDevice.Controls)
                                {
                                    if (control.ControlNum == thisEntry.DirectInputControl.ControlNum &&
                                        control.ControlType == thisEntry.DirectInputControl.ControlType)
                                    {
                                        thisBinding.DirectInputControl = (DIPhysicalControlInfo) control;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (thisEntry.BindingType == BindingType.Keybinding)
                        {
                            var thisBinding = _controlBindings[entry.Key];
                            thisBinding.CpdInputControl = thisEntry.CpdInputControl;
                            thisBinding.BindingType = thisEntry.BindingType;
                            thisBinding.ControlName = thisEntry.ControlName;
                            thisBinding.Keys = thisEntry.Keys;
                        }
                    }
                }
            }
        }

        public void Start()
        {
            GC.Collect();
            InitializeInternal();
            SetupKeyboardWatcherThread();
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
                UpdateManagerSize();
                UpdateManagerSize();
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

        private void RenderOnce(int pollingFrequencyMillis)
        {
            if (_isDisposed || !_keepRunning) return;
            var loopStartTime = DateTime.Now;
            UpdateManagerSize();
            Render();
            Application.DoEvents();
            //System.GC.Collect();
            var loopEndTime = DateTime.Now;
            var elapsed = loopEndTime.Subtract(loopStartTime);
            var wait = (int) (pollingFrequencyMillis - elapsed.TotalMilliseconds);
            if (wait < 1) wait = 1;
            Thread.Sleep(wait);
        }

        protected void UpdateRotationSpecificSettings()
        {
            var rotation = Settings.Default.Rotation;
            if (rotation == RotateFlipType.Rotate180FlipNone || rotation == RotateFlipType.RotateNoneFlipNone)
            {
                _widthFactor = 3;
                _heightFactor = 4;
            }
            else
            {
                _widthFactor = 4;
                _heightFactor = 3;
            }
        }

        protected void UpdateManagerSize()
        {
            UpdateRotationSpecificSettings();
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
                if (rotation == RotateFlipType.Rotate180FlipNone || rotation == RotateFlipType.RotateNoneFlipNone)
                {
                    _manager = new F16CpdMfdManager(new Size(DesktopBounds.Width, DesktopBounds.Height));
                }
                else
                {
                    _manager = new F16CpdMfdManager(new Size(DesktopBounds.Height, DesktopBounds.Width));
                }
            }
            else
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
            if (_simSupportModule == null)
            {
                _simSupportModule = new Falcon4Support(_manager, _mediator);
                _manager.SimSupportModule = _simSupportModule;
            }
            return;
        }

        public void Stop()
        {
            _keepRunning = false;
            Application.DoEvents();
            Thread.Sleep(1000);
            AbortThread(ref _keyboardWatcherThread);
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

        protected void HandleMouseClick(MouseEventArgs e)
        {
            //if there's no MFD manager set up yet, ignore this event
            if (_manager == null) return;

            //if the pressed mouse button was the left mouse button
            if (e.Button == MouseButtons.Left)
            {
                //if the mouse was single-clicked
                if (e.Clicks == 1)
                {
                    //get the current menu page
                    var currentPage = _manager.ActiveMenuPage;
                    //ask the current page which button was clicked
                    var x = e.Location.X - Location.X;
                    var y = e.Location.Y - Location.Y;

                    var xPrime = x;
                    var yPrime = y;

                    var rotation = Settings.Default.Rotation;
                    if (rotation == RotateFlipType.Rotate270FlipNone)
                    {
                        xPrime = DisplayRectangle.Height - y;
                        yPrime = x;
                    }
                    else if (rotation == RotateFlipType.Rotate90FlipNone)
                    {
                        xPrime = y;
                        yPrime = DisplayRectangle.Width - x;
                    }
                    else if (rotation == RotateFlipType.Rotate180FlipNone)
                    {
                        xPrime = DisplayRectangle.Width - x;
                        yPrime = DisplayRectangle.Height - y;
                    }

                    //OptionSelectButton clickedButton = currentPage.GetOptionSelectButtonByLocation(x, y);
                    var clickedButton = currentPage.GetOptionSelectButtonByLocation(xPrime, yPrime);
                    if (clickedButton != null) //if a button was clicked
                    {
                        var whenPressed = _mouseDownTime.HasValue ? _mouseDownTime.Value : DateTime.Now;
                        clickedButton.Press(whenPressed); //fire the button's press  event
                    }
                }
            }
        }

        private void F16CpdEngine_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseDown = true;
            _mouseDownTime = DateTime.Now;
            var pollingFrequencyMillis = Settings.Default.PollingFrequencyMillis;
            while (_mouseDown && ((MouseButtons & MouseButtons.Left) == MouseButtons.Left))
            {
                HandleMouseClick(new MouseEventArgs(MouseButtons.Left, 1, Cursor.Position.X, Cursor.Position.Y, 0));
                RenderOnce(pollingFrequencyMillis);
            }
            _mouseDown = false;
            _mouseDownTime = null;
        }

        private void F16CpdEngine_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseDown = false;
        }

        private void F16CpdEngine_LocationChanged(object sender, EventArgs e)
        {
            //if (!_loading)
            //{

            Settings.Default.CpdWindowX = Location.X;
            Settings.Default.CpdWindowY = Location.Y;
            Util.SaveCurrentProperties();
            //}
        }

        private void F16CpdEngine_SizeChanged(object sender, EventArgs e)
        {
            //if (!_loading)
            //{
            UpdateManagerSize();
            Settings.Default.CpdWindowWidth = DesktopBounds.Width;
            Settings.Default.CpdWindowHeight = DesktopBounds.Height;
            Util.SaveCurrentProperties();

            Common.Util.DisposeObject(_renderTarget);
            _renderTarget = CreateRenderTarget();

            //}
        }

        #region Destructors

        /// <summary>
        ///   Public implementation of IDisposable.Dispose().  Cleans up managed
        ///   and unmanaged resources used by this object before allowing garbage collection
        /// </summary>
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

        /// <summary>
        ///   Standard finalizer, which will call Dispose() if this object is not
        ///   manually disposed.  Ordinarily called only by the garbage collector.
        /// </summary>
        ~F16CpdEngine()
        {
            Dispose();
        }

        /// <summary>
        ///   Private implementation of Dispose()
        /// </summary>
        /// <param name = "disposing">flag to indicate if we should actually perform disposal.  Distinguishes the private method signature from the public signature.</param>
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

                    AbortThread(ref _keyboardWatcherThread);
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