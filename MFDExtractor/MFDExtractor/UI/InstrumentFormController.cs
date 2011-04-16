using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Common;
using Common.Generic;
using Common.SimSupport;
using log4net;

namespace MFDExtractor.UI
{
    internal class InstrumentFormController
    {
        private const int MIN_RENDERER_PASS_TIME_MILLSECONDS = 0;
                          //minimum time each instrument render should take per cycle before trying to run again (introduced for throttling purposes)

        private const int MIN_DELAY_AT_END_OF_INSTRUMENT_RENDER = 0;
                          //minimum time after each individual instrument render that should be waited 

        private static int _renderCycleNum;
        private static readonly ILog _log = LogManager.GetLogger(typeof (InstrumentFormController));

        private static readonly Dictionary<string, InstrumentFormController> _instances =
            new Dictionary<string, InstrumentFormController>();

        private static readonly Dictionary<IInstrumentRenderer, InstrumentStateSnapshot> _instrumentStates =
            new Dictionary<IInstrumentRenderer, InstrumentStateSnapshot>();

        private readonly EventHandler _onDataChangedEventHandler;
        private readonly EventHandler _onDisposedEventHandler;

        private InstrumentFormController()
        {
            _onDataChangedEventHandler = new EventHandler((s, e) => { _instrumentForm_DataChanged(s, e); });
            _onDisposedEventHandler = new EventHandler((s, e) => { _instrumentForm_Disposed(s, e); });
        }

        private InstrumentFormController(string instrumentName, object settingsObject, string formTitle,
                                         Image initialImage, IInstrumentRenderer renderer)
            : this()
        {
            PropertyInvokers = CreateDefaultPropertyInvokers(instrumentName, settingsObject);
            FormTitle = FormTitle;
            InstrumentName = instrumentName;
            Renderer = renderer;
        }

        public string InstrumentName { get; private set; }
        public static bool HighlightOutputWindows { get; set; }
        public static bool NightMode { get; set; }
        public static bool TestMode { get; set; }

        public static Dictionary<string, InstrumentFormController> Instances
        {
            get { return _instances; }
        }

        public InstrumentForm InstrumentForm { get; private set; }

        public bool RenderOnStateChangesOnly { get; set; }
        public string FormTitle { get; set; }

        public Rectangle OutputRectangle
        {
            get
            {
                return new Rectangle(
                    PropertyInvokers.LocationULX.GetProperty(),
                    PropertyInvokers.LocationULY.GetProperty(),
                    PropertyInvokers.LocationLRX.GetProperty() - PropertyInvokers.LocationULX.GetProperty(),
                    PropertyInvokers.LocationLRY.GetProperty() - PropertyInvokers.LocationULY.GetProperty()
                    );
            }
        }

        public Screen OutputScreen
        {
            get { return Common.Screen.Util.FindScreen(PropertyInvokers.OutputDisplayName.GetProperty()); }
        }

        public Image InitialImage { get; set; }
        public PerformanceCounter PerfCounter { get; private set; }
        public IInstrumentRenderer Renderer { get; private set; }
        public int PollingDelay { get; private set; }
        private bool RenderImmediately { get; set; }
        private DateTime LastRenderedOn { get; set; }
        private InstrumentFormPropertyInvokers PropertyInvokers { get; set; }
        public event EventHandler StatePersisted;

        public static void DestroyAll()
        {
            lock (_instances)
            {
                foreach (string instanceKey in _instances.Keys.ToArray())
                {
                    InstrumentFormController instance = _instances[instanceKey];
                    if (instance.InstrumentForm != null)
                    {
                        instance.InstrumentForm.Close();
                        Util.DisposeObject(instance.InstrumentForm); //TODO: make the outer object implement IDisposable
                    }
                    _instances.Remove(instanceKey);
                }
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
                if (instrumentName == null)
                    throw new ArgumentNullException(string.Format("{0}",
                                                                  MethodBase.GetCurrentMethod().GetParameters()[0].Name));
                if (_instances.ContainsKey(instrumentName))
                    throw new ArgumentException(string.Format("Instrument controller {0} already exists.",
                                                              instrumentName));

                var controller = new InstrumentFormController(instrumentName, settingsObject, formTitle, initialImage,
                                                              renderer);

                if (controller.PropertyInvokers.IsEnabled.GetProperty())
                {
                    Point location;
                    var size = new Size();
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
                        Common.Screen.Util.OpenFormOnSpecificMonitor(controller.InstrumentForm, parentForm,
                                                                     controller.OutputScreen, location, size, true, true);

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
                    var creationDataList = new List<CounterCreationData>();
                    foreach (InstrumentFormController instance in _instances.Values)
                    {
                        try
                        {
                            creationDataList.Add(new CounterCreationData(
                                                     string.Format("{0} FPS", instance.InstrumentName),
                                                     string.Format("{0} Frames per Second", instance.FormTitle),
                                                     PerformanceCounterType.RateOfCountsPerSecond32));
                        }
                        catch
                        {
                        }
                    }

                    // Create a category that contains multiple counters
                    // define the CounterCreationData for the three counters
                    CounterCreationData[] ccds = creationDataList.ToArray();

                    // Create a CounterCreationDataCollection from the array
                    var counterCollection =
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
                        PerformanceCounterCategory.Create(Application.ProductName,
                                                          Application.ProductName + " performance counters",
                                                          PerformanceCounterCategoryType.SingleInstance,
                                                          counterCollection);

                    foreach (InstrumentFormController instance in _instances.Values)
                    {
                        try
                        {
                            instance.PerfCounter = new PerformanceCounter(Application.ProductName,
                                                                          string.Format("{0} FPS",
                                                                                        instance.InstrumentName));
                            instance.PerfCounter.ReadOnly = false;
                        }
                        catch
                        {
                        }
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
                foreach (InstrumentFormController instance in _instances.Values)
                {
                    if (string.Equals(instance.InstrumentName, instrumentName,
                                      StringComparison.InvariantCultureIgnoreCase))
                    {
                        instance.Recover(screen);
                        break;
                    }
                }
            }
        }

        public void Recover(Screen screen)
        {
            InstrumentForm.StretchToFill = false;
            InstrumentForm.Location = screen.Bounds.Location;
            InstrumentForm.BringToFront();
        }

        private static InstrumentFormPropertyInvokers CreateDefaultPropertyInvokers(string instrumentName,
                                                                                    object settingsObject)
        {
            return
                new InstrumentFormPropertyInvokers
                    {
                        AlwaysOnTop =
                            new PropertyInvoker<bool>(string.Format("{0}_AlwaysOnTop", instrumentName), settingsObject),
                        IsEnabled =
                            new PropertyInvoker<bool>(string.Format("Enable{0}Output", instrumentName), settingsObject),
                        LocationULX =
                            new PropertyInvoker<int>(string.Format("{0}_OutULX", instrumentName), settingsObject),
                        LocationULY =
                            new PropertyInvoker<int>(string.Format("{0}_OutULY", instrumentName), settingsObject),
                        LocationLRX =
                            new PropertyInvoker<int>(string.Format("{0}_OutLRX", instrumentName), settingsObject),
                        LocationLRY =
                            new PropertyInvoker<int>(string.Format("{0}_OutLRY", instrumentName), settingsObject),
                        Monochrome =
                            new PropertyInvoker<bool>(string.Format("{0}_Monochrome", instrumentName), settingsObject),
                        OutputDisplayName =
                            new PropertyInvoker<string>(string.Format("{0}_OutputDisplay", instrumentName),
                                                        settingsObject),
                        RotateFlipType =
                            new PropertyInvoker<RotateFlipType>(string.Format("{0}_RotateFlipType", instrumentName),
                                                                settingsObject),
                        StretchToFit =
                            new PropertyInvoker<bool>(string.Format("{0}_StretchToFit", instrumentName), settingsObject),
                        RenderEveryN =
                            new PropertyInvoker<int>(string.Format("{0}_RenderEveryN", instrumentName), settingsObject),
                        RenderOnN =
                            new PropertyInvoker<int>(string.Format("{0}_RenderOnN", instrumentName), settingsObject)
                    };
        }

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
            if (Renderer == null || InstrumentForm == null) return;
            if (DateTime.Now.Subtract(LastRenderedOn).TotalMilliseconds < PollingDelay) return;

            Bitmap image = null;
            if (InstrumentForm.ClientRectangle != Rectangle.Empty)
            {
                try
                {
                    if (InstrumentForm.Rotation.ToString().Contains("90") ||
                        InstrumentForm.Rotation.ToString().Contains("270"))
                    {
                        image = new Bitmap(InstrumentForm.ClientRectangle.Height, InstrumentForm.ClientRectangle.Width,
                                           PixelFormat.Format32bppPArgb);
                    }
                    else
                    {
                        image = new Bitmap(InstrumentForm.ClientRectangle.Width, InstrumentForm.ClientRectangle.Height,
                                           PixelFormat.Format32bppPArgb);
                    }
                    using (Graphics g = Graphics.FromImage(image))
                    {
                        try
                        {
                            Renderer.Render(g, new Rectangle(0, 0, image.Width, image.Height));
                            LastRenderedOn = DateTime.Now;
                            if (ShouldHighlightingBorderBeDisplayedOnTargetForm(InstrumentForm))
                            {
                                Color scopeGreenColor = Color.FromArgb(255, 63, 250, 63);
                                var scopeGreenPen = new Pen(scopeGreenColor);
                                scopeGreenPen.Width = 5;
                                g.DrawRectangle(scopeGreenPen, new Rectangle(new Point(0, 0), image.Size));
                                RenderImmediately = true;
                            }
                        }
                        catch (ThreadAbortException)
                        {
                        }
                        catch (ThreadInterruptedException)
                        {
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                _log.Error("An error occurred while rendering " + Renderer.GetType(), e);
                            }
                            catch (NullReferenceException)
                            {
                            }
                        }
                    }
                    if (InstrumentForm.Rotation != RotateFlipType.RotateNoneFlipNone)
                    {
                        image.RotateFlip(InstrumentForm.Rotation);
                    }
                    using (Graphics graphics = InstrumentForm.CreateGraphics())
                    {
                        if (NightMode)
                        {
                            var nvisImageAttribs = new ImageAttributes();
                            ColorMatrix cm = Common.Imaging.Util.GetNVISColorMatrix(255, 255);
                            nvisImageAttribs.SetColorMatrix(cm, ColorMatrixFlag.Default);
                            graphics.DrawImage(image, InstrumentForm.ClientRectangle, 0, 0, image.Width, image.Height,
                                               GraphicsUnit.Pixel, nvisImageAttribs);
                        }
                        else if (InstrumentForm.Monochrome)
                        {
                            var monochromeImageAttribs = new ImageAttributes();
                            ColorMatrix cm = Common.Imaging.Util.GetGreyscaleColorMatrix();
                            monochromeImageAttribs.SetColorMatrix(cm, ColorMatrixFlag.Default);
                            graphics.DrawImage(image, InstrumentForm.ClientRectangle, 0, 0, image.Width, image.Height,
                                               GraphicsUnit.Pixel, monochromeImageAttribs);
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
                    Util.DisposeObject(image);
                }
            }


            if (
                TestMode
                ||
                (!RenderOnStateChangesOnly)
                ||
                (RenderOnStateChangesOnly && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(Renderer))
                ||
                (RenderImmediately)
                )
            {
                int renderEveryN = PropertyInvokers.RenderEveryN.GetProperty();
                    //render every N times through the render loop (for example, once every 5 times)
                if (renderEveryN == 0) renderEveryN = 1; //can't be zero
                int renderOnN = PropertyInvokers.RenderOnN.GetProperty();
                    //specifically, on the Nth time (for example, on the 4th time through)

                if (
                    (_renderCycleNum%renderEveryN == (renderOnN - 1))
                    ||
                    RenderImmediately
                    )
                {
                    RenderImmediately = false;
                }
            }
            if (PerfCounter != null)
            {
                PerfCounter.Increment();
            }

            DateTime endTime = DateTime.Now;
            TimeSpan elapsed = endTime.Subtract(startTime);
            if (elapsed.TotalMilliseconds < MIN_RENDERER_PASS_TIME_MILLSECONDS)
            {
                var toWait = new TimeSpan(0, 0, 0, 0,
                                          (int) (MIN_RENDERER_PASS_TIME_MILLSECONDS - elapsed.TotalMilliseconds));
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
                foreach (InstrumentFormController instance in _instances.Values)
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
                    catch
                    {
                    }
                }
                return retVal;
            }
        }


        private bool IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(IInstrumentRenderer renderer)
        {
            int staleDataTimeout = 500; //Timeout.Infinite;
            var baseRenderer = renderer as InstrumentRendererBase;
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
                timeSinceHashChanged = (int) Math.Floor(DateTime.Now.Subtract(oldStateDateTime).TotalMilliseconds);
            }
            bool stateIsStaleOrChanged = (hashesAreDifferent ||
                                          (timeSinceHashChanged > staleDataTimeout &&
                                           staleDataTimeout != Timeout.Infinite));
            if (stateIsStaleOrChanged)
            {
                var toStore = new InstrumentStateSnapshot {DateTime = newStateDateTime, HashCode = newStateHash};
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
                foreach (InstrumentFormController instance in _instances.Values)
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

        protected virtual void PersistState()
        {
            Point location = InstrumentForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(InstrumentForm.DesktopBounds);
            PropertyInvokers.OutputDisplayName.SetProperty(Common.Screen.Util.CleanDeviceName(screen.DeviceName));
            if (InstrumentForm.StretchToFill)
            {
                PropertyInvokers.StretchToFit.SetProperty(true);
            }
            else
            {
                PropertyInvokers.StretchToFit.SetProperty(false);
                Size size = InstrumentForm.Size;
                PropertyInvokers.LocationULX.SetProperty(location.X - screen.Bounds.Location.X);
                PropertyInvokers.LocationULY.SetProperty(location.Y - screen.Bounds.Location.Y);
                PropertyInvokers.LocationLRX.SetProperty((location.X - screen.Bounds.Location.X) + size.Width);
                PropertyInvokers.LocationLRY.SetProperty((location.Y - screen.Bounds.Location.Y) + size.Height);
            }
            PropertyInvokers.IsEnabled.SetProperty(InstrumentForm.InstrumentEnabled);
            PropertyInvokers.RotateFlipType.SetProperty(InstrumentForm.Rotation);
            PropertyInvokers.AlwaysOnTop.SetProperty(InstrumentForm.AlwaysOnTop);
            PropertyInvokers.Monochrome.SetProperty(InstrumentForm.Monochrome);
        }

        protected virtual void RegisterForFormEvents()
        {
            InstrumentForm.DataChanged += _onDataChangedEventHandler;
            InstrumentForm.Disposed += _onDataChangedEventHandler;
        }

        protected virtual void UnregisterForFormEvents()
        {
            InstrumentForm.DataChanged -= _onDataChangedEventHandler;
            InstrumentForm.Disposed -= _onDisposedEventHandler;
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
            if (StatePersisted != null)
            {
                StatePersisted(sender, e);
            }
        }

        #region Nested type: InstrumentFormPropertyInvokers

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

        #endregion

        #region Nested type: InstrumentStateSnapshot

        private struct InstrumentStateSnapshot
        {
            public DateTime DateTime;
            public int HashCode;
        }

        #endregion
    }
}