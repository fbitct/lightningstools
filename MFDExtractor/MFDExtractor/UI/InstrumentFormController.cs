using System;
using System.Collections.Generic;
using System.Linq;
using Common.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using Common.SimSupport;
using System.Drawing.Imaging;
using System.Threading;
using log4net;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace MFDExtractor.UI
{
    class InstrumentFormController
    {
        private const int MIN_RENDERER_PASS_TIME_MILLSECONDS = 0;//minimum time each instrument render should take per cycle before trying to run again (introduced for throttling purposes)
        private const int MIN_DELAY_AT_END_OF_INSTRUMENT_RENDER = 0;//minimum time after each individual instrument render that should be waited 
        private static int _renderCycleNum = 0;
        private static ILog _log = LogManager.GetLogger(typeof(InstrumentFormController));

        private static Dictionary<string, InstrumentFormController> _instances = new Dictionary<string, InstrumentFormController>();
        private EventHandler _onDataChangedEventHandler = null;
        private EventHandler _onDisposedEventHandler = null;
        public event EventHandler StatePersisted;

        private InstrumentFormController()
        {
            _onDataChangedEventHandler = new EventHandler((s, e) => { _instrumentForm_DataChanged(s, e); });
            _onDisposedEventHandler = new EventHandler((s, e) => { _instrumentForm_Disposed(s, e); });
        }
        private InstrumentFormController(string instrumentName, object settingsObject, string formTitle, Image initialImage, IInstrumentRenderer renderer)
            : this()
        {
            this.PropertyInvokers = CreateDefaultPropertyInvokers(instrumentName, settingsObject);
            this.FormTitle = FormTitle;
            this.InstrumentName = instrumentName;
            this.Renderer = renderer;
        }
        public string InstrumentName { get; private set; }
        public static bool HighlightOutputWindows { get; set; }
        public static bool NightMode { get; set; }
        public static bool TestMode { get; set; }
        public static void DestroyAll()
        {
            lock (_instances)
            {
                foreach (var instanceKey in _instances.Keys.ToArray())
                {
                    InstrumentFormController instance = _instances[instanceKey];
                    if (instance.InstrumentForm != null)
                    {
                        instance.InstrumentForm.Close();
                        Common.Util.DisposeObject(instance.InstrumentForm);//TODO: make the outer object implement IDisposable
                    }
                    _instances.Remove(instanceKey);
                }
            }
        }
        public static Dictionary<string, InstrumentFormController> Instances
        {
            get
            {
                return _instances;
            }
        }
        public static void Create
        (
            string instrumentName,
            string formTitle,
            Form parentForm,
            Image initialImage,
            EventHandler statePersistedEventHandler,
            object settingsObject,
            IInstrumentRenderer renderer
        )
        {
            lock (_instances)
            {

                if (instrumentName == null) throw new ArgumentNullException(string.Format("{0}", MethodInfo.GetCurrentMethod().GetParameters()[0].Name));
                if (_instances.ContainsKey(instrumentName)) throw new ArgumentException(string.Format("Instrument controller {0} already exists.", instrumentName));

                InstrumentFormController controller = new InstrumentFormController(instrumentName, settingsObject, formTitle, initialImage, renderer);

                if (controller.PropertyInvokers.IsEnabled.GetProperty())
                {
                    Point location;
                    Size size = new Size();
                    controller.InstrumentForm = new InstrumentForm();
                    controller.InstrumentForm.ShowInTaskbar = false;
                    controller.InstrumentForm.ShowIcon = false;
                    controller.InstrumentForm.Text = formTitle;
                    if (controller.PropertyInvokers.StretchToFit.GetProperty())
                    {
                        controller.InstrumentForm.Size = controller.OutputRectangle.Size;
                        controller.InstrumentForm.Location = controller.OutputRectangle.Location;
                        controller.InstrumentForm.StretchToFill = true;
                        location = new Point(0, 0);
                        size = controller.OutputScreen.Bounds.Size;
                    }
                    else
                    {
                        location = controller.OutputRectangle.Location;
                        size = controller.OutputRectangle.Size;
                    }
                    controller.InstrumentForm.InstrumentEnabled = controller.PropertyInvokers.IsEnabled.GetProperty();
                    controller.InstrumentForm.AlwaysOnTop = controller.PropertyInvokers.AlwaysOnTop.GetProperty();
                    controller.InstrumentForm.Monochrome = controller.PropertyInvokers.Monochrome.GetProperty();
                    controller.InstrumentForm.Rotation = controller.PropertyInvokers.RotateFlipType.GetProperty();
                    controller.InstrumentForm.WindowState = FormWindowState.Normal;
                    if (controller.PropertyInvokers.IsEnabled.GetProperty())
                    {
                        Common.Screen.Util.OpenFormOnSpecificMonitor(controller.InstrumentForm, parentForm, controller.OutputScreen, location, size, true, true);

                        if (initialImage != null)
                        {
                            using (Graphics graphics = controller.InstrumentForm.CreateGraphics())
                            {
                                graphics.DrawImage(initialImage, controller.InstrumentForm.ClientRectangle);
                            }
                        }
                        controller.RegisterForFormEvents();
                    }
                    controller.StatePersisted += statePersistedEventHandler;
                    _instances.Add(instrumentName, controller);
                }
            }
        }
        private static void CreatePerformanceCounters()
        {
            lock (_instances)
            {
                try
                {
                    List<CounterCreationData> creationDataList = new List<CounterCreationData>();
                    foreach (var instance in _instances.Values)
                    {
                        try
                        {
                            creationDataList.Add(new CounterCreationData(
                                string.Format("{0} FPS", instance.InstrumentName),
                                string.Format("{0} Frames per Second", instance.FormTitle),
                                PerformanceCounterType.RateOfCountsPerSecond32));
                        }
                        catch { }
                    }

                    // Create a category that contains multiple counters
                    // define the CounterCreationData for the three counters
                    CounterCreationData[] ccds = creationDataList.ToArray();

                    // Create a CounterCreationDataCollection from the array
                    CounterCreationDataCollection counterCollection =
                      new CounterCreationDataCollection(ccds);

                    //delete existing counters
                    try
                    {
                        PerformanceCounterCategory.Delete(Application.ProductName);
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    // Create the category with the counters
                    PerformanceCounterCategory category =
                      PerformanceCounterCategory.Create(Application.ProductName, Application.ProductName + " performance counters", PerformanceCounterCategoryType.SingleInstance,
                        counterCollection);

                    foreach (var instance in _instances.Values)
                    {
                        try
                        {
                            instance.PerfCounter = new PerformanceCounter(Application.ProductName, string.Format("{0} FPS", instance.InstrumentName));
                            instance.PerfCounter.ReadOnly = false;
                        }
                        catch { }
                    }

                }
                catch (Exception)
                {
                }
            }
        }
        public static void RecoverInstrumentForm(string instrumentName, Screen screen)
        {
            lock (_instances)
            {
                foreach (var instance in _instances.Values)
                {
                    if (string.Equals(instance.InstrumentName, instrumentName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        instance.Recover(screen);
                        break;
                    }
                }
            }
        }
        public void Recover(Screen screen)
        {
            this.InstrumentForm.StretchToFill = false;
            this.InstrumentForm.Location = screen.Bounds.Location;
            this.InstrumentForm.BringToFront();
        }
        private static InstrumentFormPropertyInvokers CreateDefaultPropertyInvokers(string instrumentName, object settingsObject)
        {
            return
            new InstrumentFormPropertyInvokers()
            {
                AlwaysOnTop = new PropertyInvoker<bool>(string.Format("{0}_AlwaysOnTop", instrumentName), settingsObject),
                IsEnabled = new PropertyInvoker<bool>(string.Format("Enable{0}Output", instrumentName), settingsObject),
                LocationULX = new PropertyInvoker<int>(string.Format("{0}_OutULX", instrumentName), settingsObject),
                LocationULY = new PropertyInvoker<int>(string.Format("{0}_OutULY", instrumentName), settingsObject),
                LocationLRX = new PropertyInvoker<int>(string.Format("{0}_OutLRX", instrumentName), settingsObject),
                LocationLRY = new PropertyInvoker<int>(string.Format("{0}_OutLRY", instrumentName), settingsObject),
                Monochrome = new PropertyInvoker<bool>(string.Format("{0}_Monochrome", instrumentName), settingsObject),
                OutputDisplayName = new PropertyInvoker<string>(string.Format("{0}_OutputDisplay", instrumentName), settingsObject),
                RotateFlipType = new PropertyInvoker<RotateFlipType>(string.Format("{0}_RotateFlipType", instrumentName), settingsObject),
                StretchToFit = new PropertyInvoker<bool>(string.Format("{0}_StretchToFit", instrumentName), settingsObject),
                RenderEveryN = new PropertyInvoker<int>(string.Format("{0}_RenderEveryN", instrumentName), settingsObject),
                RenderOnN = new PropertyInvoker<int>(string.Format("{0}_RenderOnN", instrumentName), settingsObject)
            };
        }
        private InstrumentForm _instrumentForm = null;
        public InstrumentForm InstrumentForm { get { return _instrumentForm; } private set { _instrumentForm = value; } }

        public bool RenderOnStateChangesOnly { get; set; }
        public string FormTitle { get; set; }

        public Rectangle OutputRectangle
        {
            get
            {
                return new Rectangle(
                    this.PropertyInvokers.LocationULX.GetProperty(),
                    this.PropertyInvokers.LocationULY.GetProperty(),
                    this.PropertyInvokers.LocationLRX.GetProperty() - this.PropertyInvokers.LocationULX.GetProperty(),
                    this.PropertyInvokers.LocationLRY.GetProperty() - this.PropertyInvokers.LocationULY.GetProperty()
                );
            }
        }
        public Screen OutputScreen { get { return Common.Screen.Util.FindScreen(this.PropertyInvokers.OutputDisplayName.GetProperty()); } }
        public Image InitialImage { get; set; }
        public PerformanceCounter PerfCounter { get; private set; }
        public IInstrumentRenderer Renderer { get; private set; }
        public int PollingDelay { get; private set; }
        private bool RenderImmediately { get; set; }
        private DateTime LastRenderedOn { get; set; }
        public static void RenderAll()
        {
            lock (_instances)
            {
                _renderCycleNum++;
                foreach (var instance in _instances)
                {
                    instance.Value.Render();
                }
            }
        }
        private void Render()
        {
            DateTime startTime = DateTime.Now;
            if (this.Renderer == null || this.InstrumentForm == null) return;
            if (DateTime.Now.Subtract(this.LastRenderedOn).TotalMilliseconds < this.PollingDelay) return;

            Bitmap image = null;
            if (this.InstrumentForm.ClientRectangle != Rectangle.Empty)
            {
                try
                {
                    if (this.InstrumentForm.Rotation.ToString().Contains("90") || this.InstrumentForm.Rotation.ToString().Contains("270"))
                    {
                        image = new Bitmap(this.InstrumentForm.ClientRectangle.Height, this.InstrumentForm.ClientRectangle.Width, PixelFormat.Format32bppPArgb);
                    }
                    else
                    {
                        image = new Bitmap(this.InstrumentForm.ClientRectangle.Width, this.InstrumentForm.ClientRectangle.Height, PixelFormat.Format32bppPArgb);
                    }
                    using (Graphics g = Graphics.FromImage(image))
                    {
                        try
                        {
                            this.Renderer.Render(g, new Rectangle(0, 0, image.Width, image.Height));
                            this.LastRenderedOn = DateTime.Now;
                            if (ShouldHighlightingBorderBeDisplayedOnTargetForm(this.InstrumentForm))
                            {
                                Color scopeGreenColor = Color.FromArgb(255, 63, 250, 63);
                                Pen scopeGreenPen = new Pen(scopeGreenColor);
                                scopeGreenPen.Width = 5;
                                g.DrawRectangle(scopeGreenPen, new Rectangle(new Point(0, 0), image.Size));
                                this.RenderImmediately = true;
                            }
                        }
                        catch (ThreadAbortException) { }
                        catch (ThreadInterruptedException) { }
                        catch (Exception e)
                        {
                            try
                            {
                                _log.Error("An error occurred while rendering " + this.Renderer.GetType().ToString(), e);
                            }
                            catch (NullReferenceException) { }
                        }
                    }
                    if (this.InstrumentForm.Rotation != RotateFlipType.RotateNoneFlipNone)
                    {
                        image.RotateFlip(this.InstrumentForm.Rotation);
                    }
                    using (Graphics graphics = this.InstrumentForm.CreateGraphics())
                    {
                        if (InstrumentFormController.NightMode)
                        {
                            ImageAttributes nvisImageAttribs = new ImageAttributes();
                            ColorMatrix cm = Common.Imaging.Util.GetNVISColorMatrix(255, 255);
                            nvisImageAttribs.SetColorMatrix(cm, ColorMatrixFlag.Default);
                            graphics.DrawImage(image, this.InstrumentForm.ClientRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, nvisImageAttribs);
                        }
                        else if (this.InstrumentForm.Monochrome)
                        {
                            ImageAttributes monochromeImageAttribs = new ImageAttributes();
                            ColorMatrix cm = Common.Imaging.Util.GetGreyscaleColorMatrix();
                            monochromeImageAttribs.SetColorMatrix(cm, ColorMatrixFlag.Default);
                            graphics.DrawImage(image, this.InstrumentForm.ClientRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, monochromeImageAttribs);
                        }
                        else
                        {
                            graphics.DrawImageUnscaled(image, 0, 0, image.Width, image.Height);
                        }
                    }

                }
                catch (ExternalException)
                {
                    //GDI+ error message we don't care about
                }
                catch (ObjectDisposedException)
                {
                    //GDI+ error message thrown on operations on disposed images -- can happen when one thread disposes while shutting-down thread tries to render
                }
                catch (ArgumentException)
                {
                    //GDI+ error message we don't care about
                }
                catch (OutOfMemoryException)
                {
                    //bullshit OOM messages from GDI+
                }
                catch (InvalidOperationException)
                {
                    //GDI+ error message we don't care about
                }
                finally
                {
                    Common.Util.DisposeObject(image);
                }
            }


            if (
                InstrumentFormController.TestMode
                    ||
                (!this.RenderOnStateChangesOnly)
                    ||
                (this.RenderOnStateChangesOnly && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(this.Renderer))
                    ||
                (this.RenderImmediately)
            )
            {
                int renderEveryN = this.PropertyInvokers.RenderEveryN.GetProperty(); //render every N times through the render loop (for example, once every 5 times)
                if (renderEveryN == 0) renderEveryN = 1; //can't be zero
                int renderOnN = this.PropertyInvokers.RenderOnN.GetProperty(); //specifically, on the Nth time (for example, on the 4th time through)

                if (
                        (_renderCycleNum % renderEveryN == (renderOnN - 1))
                            ||
                        this.RenderImmediately
                )
                {
                    this.RenderImmediately = false;
                }
            }
            if (this.PerfCounter != null)
            {
                this.PerfCounter.Increment();
            }

            DateTime endTime = DateTime.Now;
            TimeSpan elapsed = endTime.Subtract(startTime);
            if (elapsed.TotalMilliseconds < MIN_RENDERER_PASS_TIME_MILLSECONDS)
            {
                TimeSpan toWait = new TimeSpan(0, 0, 0, 0, (int)(MIN_RENDERER_PASS_TIME_MILLSECONDS - elapsed.TotalMilliseconds));
                if (toWait.TotalMilliseconds < MIN_DELAY_AT_END_OF_INSTRUMENT_RENDER)
                {
                    toWait = new TimeSpan(0, 0, 0, 0, MIN_DELAY_AT_END_OF_INSTRUMENT_RENDER);
                }
                Thread.Sleep(toWait);
            }
        }
        public static bool IsWindowSizingOrMovingBeingAttemptedOnAnyOutputWindow()
        {
            lock (_instances)
            {
                bool retVal = false;
                foreach (var instance in _instances.Values)
                {
                    try
                    {
                        InstrumentForm iForm = instance.InstrumentForm;
                        if (
                            iForm != null &&
                            iForm.Visible && iForm.SizingOrMovingCursorsAreDisplayed
                                &&
                            (
                                ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left)
                                    ||
                                ((Control.MouseButtons & MouseButtons.Right) == MouseButtons.Right)
                            )
                        )
                        {
                            retVal = true;
                            break;
                        }
                    }
                    catch { }
                }
                return retVal;
            }
        }

        private struct InstrumentStateSnapshot
        {
            public int HashCode;
            public DateTime DateTime;
        }
        private static Dictionary<IInstrumentRenderer, InstrumentStateSnapshot> _instrumentStates = new Dictionary<IInstrumentRenderer, InstrumentStateSnapshot>();


        private bool IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(IInstrumentRenderer renderer)
        {
            int staleDataTimeout = 500;//Timeout.Infinite;
            InstrumentRendererBase baseRenderer = renderer as InstrumentRendererBase;
            if (baseRenderer == null) return true;
            int oldStateHash = 0;
            DateTime oldStateDateTime = DateTime.MinValue;
            bool oldHashWasFound = false;
            if (_instrumentStates.ContainsKey(baseRenderer))
            {
                oldStateHash = _instrumentStates[baseRenderer].HashCode;
                oldStateDateTime = _instrumentStates[baseRenderer].DateTime;
                oldHashWasFound = true;
            }
            InstrumentStateBase newState = baseRenderer.GetState();
            int newStateHash = newState != null ? newState.GetHashCode() : 0;
            DateTime newStateDateTime = DateTime.Now;

            bool hashesAreDifferent = !oldHashWasFound || (oldStateHash != newStateHash);

            int timeSinceHashChanged = Int32.MaxValue;
            if (oldStateDateTime != DateTime.MinValue)
            {
                timeSinceHashChanged = (int)Math.Floor(DateTime.Now.Subtract(oldStateDateTime).TotalMilliseconds);
            }
            bool stateIsStaleOrChanged = (hashesAreDifferent || (timeSinceHashChanged > staleDataTimeout && staleDataTimeout != Timeout.Infinite));
            if (stateIsStaleOrChanged)
            {
                InstrumentStateSnapshot toStore = new InstrumentStateSnapshot() { DateTime = newStateDateTime, HashCode = newStateHash };
                if (_instrumentStates.ContainsKey(baseRenderer))
                {
                    _instrumentStates[baseRenderer] = toStore;
                }
                else
                {
                    _instrumentStates.Add(baseRenderer, toStore);
                }
            }
            InstrumentForm form = GetFormForRenderer(renderer);
            if (ShouldHighlightingBorderBeDisplayedOnTargetForm(form)) return true;
            return stateIsStaleOrChanged;
        }
        private InstrumentForm GetFormForRenderer(IInstrumentRenderer renderer)
        {
            lock (_instances)
            {
                if (renderer == null) return null;
                foreach (var instance in _instances.Values)
                {
                    if (instance.Renderer == renderer) return instance.InstrumentForm;
                }
            }
            return null;
        }

        private static bool ShouldHighlightingBorderBeDisplayedOnTargetForm(InstrumentForm targetForm)
        {
            return targetForm.SizingOrMovingCursorsAreDisplayed
                    &&
                HighlightOutputWindows
            ;
        }

        private InstrumentFormPropertyInvokers PropertyInvokers { get; set; }

        protected virtual void PersistState()
        {
            Point location = this.InstrumentForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(this.InstrumentForm.DesktopBounds);
            this.PropertyInvokers.OutputDisplayName.SetProperty(Common.Screen.Util.CleanDeviceName(screen.DeviceName));
            if (this.InstrumentForm.StretchToFill)
            {
                this.PropertyInvokers.StretchToFit.SetProperty(true);
            }
            else
            {
                this.PropertyInvokers.StretchToFit.SetProperty(false);
                Size size = this.InstrumentForm.Size;
                this.PropertyInvokers.LocationULX.SetProperty(location.X - screen.Bounds.Location.X);
                this.PropertyInvokers.LocationULY.SetProperty(location.Y - screen.Bounds.Location.Y);
                this.PropertyInvokers.LocationLRX.SetProperty((location.X - screen.Bounds.Location.X) + size.Width);
                this.PropertyInvokers.LocationLRY.SetProperty((location.Y - screen.Bounds.Location.Y) + size.Height);
            }
            this.PropertyInvokers.IsEnabled.SetProperty(this.InstrumentForm.InstrumentEnabled);
            this.PropertyInvokers.RotateFlipType.SetProperty(this.InstrumentForm.Rotation);
            this.PropertyInvokers.AlwaysOnTop.SetProperty(this.InstrumentForm.AlwaysOnTop);
            this.PropertyInvokers.Monochrome.SetProperty(this.InstrumentForm.Monochrome);
        }
        protected virtual void RegisterForFormEvents()
        {
            this.InstrumentForm.DataChanged += _onDataChangedEventHandler;
            this.InstrumentForm.Disposed += _onDataChangedEventHandler;
        }

        protected virtual void UnregisterForFormEvents()
        {
            this.InstrumentForm.DataChanged -= _onDataChangedEventHandler;
            this.InstrumentForm.Disposed -= _onDisposedEventHandler;
        }

        protected virtual void _instrumentForm_DataChanged(object sender, EventArgs e)
        {
            PersistState();
        }
        protected virtual void _instrumentForm_Disposed(object sender, EventArgs e)
        {
            UnregisterForFormEvents();
        }
        protected virtual void OnStatePersisted(object sender, EventArgs e)
        {
            if (this.StatePersisted != null)
            {
                StatePersisted(sender, e);
            }
        }
        private class InstrumentFormPropertyInvokers
        {
            public PropertyInvoker<bool> AlwaysOnTop { get; set; }
            public PropertyInvoker<bool> IsEnabled { get; set; }
            public PropertyInvoker<int> LocationULX { get; set; }
            public PropertyInvoker<int> LocationULY { get; set; }
            public PropertyInvoker<int> LocationLRX { get; set; }
            public PropertyInvoker<int> LocationLRY { get; set; }
            public PropertyInvoker<bool> Monochrome { get; set; }
            public PropertyInvoker<string> OutputDisplayName { get; set; }
            public PropertyInvoker<bool> StretchToFit { get; set; }
            public PropertyInvoker<RotateFlipType> RotateFlipType { get; set; }
            public PropertyInvoker<int> RenderEveryN { get; set; }
            public PropertyInvoker<int> RenderOnN { get; set; }
        }
    }

}
