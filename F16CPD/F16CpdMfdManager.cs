using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Net;
using System.IO;
using System.Drawing.Imaging;
using System.ComponentModel;
using F16CPD.Mfd.Controls;
using F16CPD.SimSupport;
using F16CPD.Mfd;
using F16CPD.Mfd.Menus;
using F16CPD.FlightInstruments;
using F16CPD.Networking;
using System.Windows.Forms;
using System.Text.RegularExpressions;
namespace F16CPD
{
    //TODO: fix nautical miles scale on map screen (mostly fixed, need to test bounds -- how high should this go?)
    //TODO: add map centering options on map screen
    //TODO: add track options on map screen (track up, north up, 
    //TODO: add orientation feature to map screen
    //TODO: implement split screen mode in map screen
    //TODO: trim label highlighting to boundary of text
    //TODO: implement built-in test mode
    //TODO: implement other MFD pages
    //TODO: PRIO create way to save input assignments
    //TODO: PRIO prevent output window resizing below a certain threshold
    //TODO: re-enable "recover output window" button on main form
    //TODO: convert from SingleInstanceApplication??
    //TODO: retest networking now that output rotation is enabled.
    public sealed class F16CpdMfdManager : MfdManager, IDisposable
    {
        private bool _isDisposed = false;
        private ToggleSwitchMfdInputControl _hsiModeSelectorSwitch = null;
        private ToggleSwitchMfdInputControl _fuelSelectControl = null;
        private ToggleSwitchMfdInputControl _extFuelTransSwitch = null;
        private RotaryEncoderMfdInputControl _paramAdjustKnob = null;
        private float _mapScale = 500000.0f;
        private bool _nightMode = false;
        private int _brightness = 255;
        private const int _maxBrightness = 255;
        private const int _numBrightnessSteps = 30;
        private ISimSupportModule _simSupportModule = null;
        private F16CPDClient _client = null;
        private Bitmap _lastMapFromServer = null;
        private BackgroundWorker _mapFetchingBackgroundWorker = null;
        private object _mapImageLock = new object();
        private int _mapRangeRingsDiameterInNauticalMiles = 40;
        private int _airspeedIndexInKnots = 0;
        private int _altitudeIndexInFeet = 0;
        private FileInfo _currentChecklistFile = null;
        private FileInfo _lastRenderedChecklistFile= null;
        private Bitmap _lastRenderedChecklistPdfPage = null;
        private int _lastRenderedChecklistPageNum = 1;
        private int _currentChecklistPageNum = 1;
        private int _currentChecklistPagesTotal = 0;

        private FileInfo _currentChartFile = null;
        private FileInfo _lastRenderedChartFile = null;
        private Bitmap _lastRenderedChartPdfPage = null;
        private int _lastRenderedChartPageNum = 1;
        private int _currentChartPageNum = 1;
        private int _currentChartPagesTotal = 0;

        public int AltitudeIndexInFeet
        {
            get
            {
                return _altitudeIndexInFeet;
            }
            set
            {
                if (value < 0) value = 0;
                if (value > 99980) value = 99980;
                _altitudeIndexInFeet = value;
            }
        }

        public int AirspeedIndexInKnots
        {
            get
            {
                return _airspeedIndexInKnots;
            }
            set
            {
                if (value < 0) value = 0;
                if (value > 1500) value = 1500;
                _airspeedIndexInKnots = value;
            }
        }

        public int MapRangeRingsDiameterInNauticalMiles
        {
            get { return _mapRangeRingsDiameterInNauticalMiles; }
            set { _mapRangeRingsDiameterInNauticalMiles = value; }
        }

        public IF16CPDClient Client
        {
            get
            {
                return _client;
            }
        }
        public ISimSupportModule SimSupportModule
        {
            get
            {
                return _simSupportModule;
            }
            set
            {
                _simSupportModule = value;
            }
        }
        public Pfd Pfd
        {
            get;
            set;
        }
        public Hsi Hsi
        {
            get;
            set;
        }
        public FlightData FlightData
        {
            get;
            set;
        }
        public int Brightness
        {
            get
            {
                return _brightness;
            }
        }
        public int MaxBrightness
        {
            get
            {
                return _maxBrightness;
            }
        }
        public bool NightMode
        {
            get
            {
                return _nightMode;
            }
            set
            {
                _nightMode = value;
            }
        }

        private void SetNightMode(bool newValue)
        {
            _nightMode = newValue;
            if (this.FlightData != null) NightMode = newValue;
        }

        public F16CpdMfdManager(Size screenBoundsPixels):base(screenBoundsPixels)
        {
            SetupNetworking();
            BuildMfdPages();
            BuildNonOsbInputControls();
            InitializeFlightInstruments();
            _brightness = Properties.Settings.Default.Brightness;
            SetupMapFetchingBackgroundWorker();

        }
        private void SetupMapFetchingBackgroundWorker()
        {
            if (Properties.Settings.Default.RunAsClient)
            {
                _mapFetchingBackgroundWorker = new BackgroundWorker();
                _mapFetchingBackgroundWorker.DoWork += new DoWorkEventHandler(mapFetchingBackgroundWorker_DoWork);
            }
        }
        private void TeardownService()
        {
            if (Properties.Settings.Default.RunAsServer)
            {
                string portNumber = Properties.Settings.Default.ServerPortNum;
                int port = 21153;
                Int32.TryParse(portNumber, out port);
                F16CPDServer.TearDownService(port);
            }

        }
        private void SetupNetworking()
        {
            if (Properties.Settings.Default.RunAsServer)
            {
                string portNumber = Properties.Settings.Default.ServerPortNum;
                int port = 21153;
                Int32.TryParse(portNumber, out port);
                F16CPDServer.CreateService("F16CPDService", port);
            }
            else if (Properties.Settings.Default.RunAsClient)
            {
                string serverIPAddress = Properties.Settings.Default.ServerIPAddress;
                string portNumber = Properties.Settings.Default.ServerPortNum;
                int port = 21153;
                Int32.TryParse(portNumber, out port);
                IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
                IPAddress.TryParse(serverIPAddress, out ipAddress);
                IPEndPoint endpoint = new IPEndPoint(ipAddress, port);
                _client = new F16CPDClient(endpoint, "F16CPDService");
                _client.ClearPendingClientMessages();
            }
        }
        private void ProcessPendingMessagesToServerFromClient()
        {
            if (!Properties.Settings.Default.RunAsServer) return;
            Networking.Message pendingMessage = F16CPDServer.GetNextPendingServerMessage();
            if (pendingMessage !=null)
            {
                bool processed = false;
                if (_simSupportModule != null)
                {
                    processed = _simSupportModule.ProcessPendingMessageToServerFromClient(pendingMessage);
                }
                if (!processed)
                {
                    switch (pendingMessage.MessageType)
                    {
                        case "RequestNewMapImage":
                            //any other "New Map Image Requested" messages in the queue will be removed at this time
                            F16CPDServer.ClearPendingServerMessagesOfType("RequestNewMapImage");
                            Dictionary<string, object> payload = (Dictionary<string, object>)pendingMessage.Payload;
                            Size renderSize = (Size)payload["RenderSize"];
                            float mapScale= (float)payload["MapScale"];
                            int mapRangeDiameter = (int)payload["RangeRingsDiameter"];
                            Bitmap renderedMap = RenderMapOnBehalfOfRemoteClient(renderSize, mapScale, mapRangeDiameter);
                            if (renderedMap != null)
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    renderedMap.Save(ms, ImageFormat.Png);
                                    ms.Flush();
                                    ms.Seek(0, SeekOrigin.Begin);
                                    byte[] rawBytes = new byte[ms.Length];
                                    ms.Read(rawBytes, 0, (int)ms.Length);
                                    F16CPDServer.SetSimProperty("CurrentMapImage", rawBytes);
                                }
                                Common.Util.DisposeObject(renderedMap);
                            }
                            processed = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        private void ProcessPendingMessagesToClientFromServer()
        {
            if (!Properties.Settings.Default.RunAsClient) return;
            Networking.Message pendingMessage = _client.GetNextPendingClientMessage();
            if (pendingMessage !=null) {
                bool processed = false;
                if (_simSupportModule != null)
                {
                    processed = _simSupportModule.ProcessPendingMessageToClientFromServer(pendingMessage);
                }
                if (!processed)
                {
                    switch (pendingMessage.MessageType)
                    {
                        case "CpdInputControlChangedEvent":
                            CpdInputControls controlThatChanged = (CpdInputControls)pendingMessage.Payload;
                            FireHandler(controlThatChanged);
                            processed = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        private void BuildNonOsbInputControls()
        {
            _hsiModeSelectorSwitch = new ToggleSwitchMfdInputControl();
            _hsiModeSelectorSwitch.PositionChanged += new EventHandler<ToggleSwitchPositionChangedEventArgs>(_hsiModeSelectorSwitch_PositionChanged);
            _hsiModeSelectorSwitch.AddPosition(@"ILS/TCN");
            _hsiModeSelectorSwitch.AddPosition("TCN");
            _hsiModeSelectorSwitch.AddPosition("NAV");
            _hsiModeSelectorSwitch.AddPosition(@"ILS/NAV");

            _fuelSelectControl = new ToggleSwitchMfdInputControl();
            _fuelSelectControl.PositionChanged += new EventHandler<ToggleSwitchPositionChangedEventArgs>(_fuelSelectControl_PositionChanged);
            _fuelSelectControl.AddPosition("TEST");
            _fuelSelectControl.AddPosition("NORM");
            _fuelSelectControl.AddPosition("RSVR");
            _fuelSelectControl.AddPosition("INT WING");
            _fuelSelectControl.AddPosition("EXT WING");
            _fuelSelectControl.AddPosition("EXT CTR");

            _extFuelTransSwitch = new ToggleSwitchMfdInputControl();
            _extFuelTransSwitch.PositionChanged += new EventHandler<ToggleSwitchPositionChangedEventArgs>(_extFuelTransSwitch_PositionChanged);
            _extFuelTransSwitch.AddPosition("NORM");
            _extFuelTransSwitch.AddPosition("WING FIRST");

            _paramAdjustKnob = new RotaryEncoderMfdInputControl();
        }

        private void _extFuelTransSwitch_PositionChanged(object sender, ToggleSwitchPositionChangedEventArgs e)
        {
            if (e == null) return;
            switch (e.NewPosition.PositionName)
            {
                case "NORM":
                    if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.ExtFuelSwitchTransNorm, e.NewPosition);
                    break;
                case "WING FIRST":
                    if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.ExtFuelSwitchTransWingFirst, e.NewPosition);
                    break;
            }
        }

        private void _fuelSelectControl_PositionChanged(object sender, ToggleSwitchPositionChangedEventArgs e)
        {
            if (e == null) return;
            switch (e.NewPosition.PositionName)
            {
                case "TEST":
                    if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.FuelSelectTest, e.NewPosition);
                    break;
                case "NORM":
                    if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.FuelSelectNorm, e.NewPosition);
                    break;
                case "RSVR":
                    if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.FuelSelectRsvr, e.NewPosition);
                    break;
                case "INT WING":
                    if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.FuelSelectIntWing, e.NewPosition);
                    break;
                case "EXT WING":
                    if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.FuelSelectExtWing, e.NewPosition);
                    break;
                case "EXT CTR":
                    if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.FuelSelectExtCtr, e.NewPosition);
                    break;
            }
        }

        private void _hsiModeSelectorSwitch_PositionChanged(object sender, ToggleSwitchPositionChangedEventArgs e)
        {
            if (e ==null) return;
            switch(e.NewPosition.PositionName) 
            {
                case "ILS/TCN":
                    if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.HsiModeIlsTcn, e.NewPosition);
                    break;
                case "TCN":
                    if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.HsiModeTcn, e.NewPosition);
                    break;
                case "NAV":
                    if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.HsiModeNav, e.NewPosition);
                    break;
                case "ILS/NAV":
                    if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.HsiModeIlsNav, e.NewPosition);
                    break;
            }
        }


        private void InitializeFlightInstruments()
        {
            this.Pfd = new Pfd();
            this.Hsi = new Hsi();
            this.FlightData = new FlightData();
        }
        private void BuildMfdPages()
        {
            MfdMenuPage primaryPage = BuildPrimaryMenuPage();
            MfdMenuPage instrumentsDisplayPage = BuildInstrumentsDisplayMenuPage();
            MfdMenuPage testPage = BuildTestPage();
            MfdMenuPage tgpPage = BuildTargetingPodMenuPage();
            MfdMenuPage messagePage = BuildMessageMenuPage();
            MfdMenuPage tadPage = BuildTADMenuPage();
            MfdMenuPage checklistsPage = BuildChecklistMenuPage();
            MfdMenuPage chartsPage = BuildChartsMenuPage();
            MfdMenuPage controlMapPage = BuildControlMapMenuPage();
            MfdMenuPage controlOverlayPage = BuildControlOverlayMenuPage();
            MfdMenuPage BitmapAnnotationPage = BuildBitmapAnnotationMenuPage();
            base.MenuPages = new MfdMenuPage[] { primaryPage, instrumentsDisplayPage, testPage, tgpPage, messagePage, tadPage, controlMapPage, controlOverlayPage, BitmapAnnotationPage, checklistsPage, chartsPage};
            foreach (MfdMenuPage thisPage in base.MenuPages)
            {
                OptionSelectButton nightModeButton = CreateOptionSelectButton(thisPage, 6, "NGT", false);
                nightModeButton.FunctionName = "NightMode";
                nightModeButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(nightModeButton_Pressed);
                nightModeButton.LabelLocation = new Point(-10000, -10000);
                nightModeButton.LabelSize = new Size(0, 0);
                thisPage.OptionSelectButtons.Add(nightModeButton);

                OptionSelectButton dayModeButton = CreateOptionSelectButton(thisPage, 26, "DAY", false);
                dayModeButton.FunctionName = "DayMode";
                dayModeButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(dayModeButton_Pressed);
                dayModeButton.LabelSize = new Size(0, 0);
                dayModeButton.LabelLocation = new Point(-10000, -10000);
                thisPage.OptionSelectButtons.Add(dayModeButton);

                OptionSelectButton brightnessIncreaseButton = CreateOptionSelectButton(thisPage, 13, "BRT", false);
                brightnessIncreaseButton.FunctionName = "IncreaseBrightness";
                brightnessIncreaseButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(brightnessIncreaseButton_Pressed);
                brightnessIncreaseButton.LabelSize = new Size(0, 0);
                brightnessIncreaseButton.LabelLocation = new Point(-10000, -10000);
                thisPage.OptionSelectButtons.Add(brightnessIncreaseButton);

                OptionSelectButton brightnessDecreaseButton = CreateOptionSelectButton(thisPage, 19, "DIM", false);
                brightnessDecreaseButton.FunctionName = "DecreaseBrightness";
                brightnessDecreaseButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(brightnessDecreaseButton_Pressed);
                brightnessDecreaseButton.LabelSize = new Size(0, 0);
                brightnessDecreaseButton.LabelLocation = new Point(-10000, -10000);
                thisPage.OptionSelectButtons.Add(brightnessDecreaseButton);

            }
            base.ActiveMenuPage = instrumentsDisplayPage;
        }

        private void brightnessDecreaseButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            unchecked
            {
                int brightnessStep = (int)((float)_maxBrightness / (float)_numBrightnessSteps);
                _brightness -= brightnessStep;
                if (_brightness < 0) _brightness = 0;
                Properties.Settings.Default.Brightness = _brightness;
                Util.SaveCurrentProperties();
            }
        }

        private void brightnessIncreaseButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            unchecked
            {
                int brightnessStep = (int)((float)_maxBrightness / (float)_numBrightnessSteps);
                _brightness += brightnessStep;
                if (_brightness > _maxBrightness) _brightness = _maxBrightness;
                Properties.Settings.Default.Brightness = _brightness;
                Util.SaveCurrentProperties();
            }
        }

        private void dayModeButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            SetNightMode(false);
        }
        private MfdMenuPage BuildTestPage()
        {
            MfdMenuPage thisPage = new MfdMenuPage(this);
            List<OptionSelectButton> buttons = new List<OptionSelectButton>();
            buttons.Add(CreateOptionSelectButton(thisPage, 3, "2PRV", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 4, "PRV", false));

            OptionSelectButton testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", true);
            testPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(testPageSelectButton_Press);
            buttons.Add(testPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 8, "CLR", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 9, "MOUSE", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 10, "OFP ID", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 11, "CAL MSS", false));
            OptionSelectButton imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(imagingPageSelectButton_Press);
            buttons.Add(imagingPageSelectButton);

            OptionSelectButton messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(messagingPageSelectButton_Press);
            buttons.Add(messagingPageSelectButton);
            
            OptionSelectButton tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD", false);
            tacticalAwarenessDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(tacticalAwarenessDisplayPageSelectButton_Press);
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            OptionSelectButton targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(targetingPodPageSelectButton_Press);
            buttons.Add(targetingPodPageSelectButton);

            OptionSelectButton headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(headDownDisplayPageSelectButton_Press);
            buttons.Add(headDownDisplayPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 22, "MFCD\r\nBIT", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 24, @"\/", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 24.5f, "PAGE\r\n1", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 25, @"^", false));
            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Test Page";
            return thisPage;
        }

        private void nightModeButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            SetNightMode(true);
        }
        private MfdMenuPage BuildPrimaryMenuPage()
        {
            MfdMenuPage thisPage = new MfdMenuPage(this);
            List<OptionSelectButton> buttons = new List<OptionSelectButton>();
            
            OptionSelectButton testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(testPageSelectButton_Press);
            buttons.Add(testPageSelectButton);

            OptionSelectButton imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(imagingPageSelectButton_Press);
            buttons.Add(imagingPageSelectButton);

            OptionSelectButton messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(messagingPageSelectButton_Press);
            buttons.Add(messagingPageSelectButton);

            OptionSelectButton tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD", false);
            tacticalAwarenessDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(tacticalAwarenessDisplayPageSelectButton_Press);
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            OptionSelectButton targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(targetingPodPageSelectButton_Press);
            buttons.Add(targetingPodPageSelectButton);

            OptionSelectButton headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(headDownDisplayPageSelectButton_Press);
            buttons.Add(headDownDisplayPageSelectButton);

            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Primary Page";
            return thisPage;
        }


        private MfdMenuPage BuildInstrumentsDisplayMenuPage()
        {
            MfdMenuPage thisPage = new MfdMenuPage(this);
            List<OptionSelectButton> buttons = new List<OptionSelectButton>();
            int triangleLegLengthPixels = 25;
            OptionSelectButton pneuButton = CreateOptionSelectButton(thisPage, 3, "PNEU", false);
            pneuButton.FunctionName = "ToggleAltimeterModeElecPneu";
            pneuButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(pneuButton_Press);
            OptionSelectButton ackButton = CreateOptionSelectButton(thisPage, 4, "ACK", false);
            ackButton.FunctionName = "AcknowledgeMessage";
            ackButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(ackButton_Pressed);
            OptionSelectButton testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.FunctionName = "TestHdd";
            testPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(testPageSelectButton_Press);
            OptionSelectButton altitudeIndexUpButton = CreateOptionSelectButton(thisPage, 7, "^", false, triangleLegLengthPixels);
            altitudeIndexUpButton.FunctionName = "AltitudeIndexIncrease";
            altitudeIndexUpButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(altitudeIndexUpButton_Press);
            OptionSelectButton altitudeIndexLabel = CreateOptionSelectButton(thisPage, 7.5f, "ALT", false);
            OptionSelectButton altitudeIndexDownButton = CreateOptionSelectButton(thisPage, 8, @"\/", false, triangleLegLengthPixels);
            altitudeIndexDownButton.FunctionName = "AltitudeIndexDecrease";
            altitudeIndexDownButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(altitudeIndexDownButton_Press);
            OptionSelectButton barometricPressureUpButton = CreateOptionSelectButton(thisPage, 9, "^", false, triangleLegLengthPixels);
            barometricPressureUpButton.FunctionName = "BarometricPressureSettingIncrease";
            barometricPressureUpButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(barometricPressureUpButton_Press);
            OptionSelectButton barometricPressureLabel = CreateOptionSelectButton(thisPage, 9.5f, "BARO", false);
            OptionSelectButton barometricPressureDownButton = CreateOptionSelectButton(thisPage, 10, @"\/", false, triangleLegLengthPixels);
            barometricPressureDownButton.FunctionName = "BarometricPressureSettingDecrease";
            barometricPressureDownButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(barometricPressureDownButton_Press);
            OptionSelectButton courseSelectUpButton = CreateOptionSelectButton(thisPage, 11, "^", false, triangleLegLengthPixels);
            courseSelectUpButton.FunctionName = "CourseSelectIncrease";
            courseSelectUpButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(courseSelectUpButton_Press);
            OptionSelectButton courseSelectLabel = CreateOptionSelectButton(thisPage, 11.5f, "CRS", false);
            OptionSelectButton courseSelectDownButton = CreateOptionSelectButton(thisPage, 12, @"\/", false, triangleLegLengthPixels);
            courseSelectDownButton.FunctionName = "CourseSelectDecrease";
            courseSelectDownButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(courseSelectDownButton_Press);
            OptionSelectButton imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(imagingPageSelectButton_Press);
            OptionSelectButton messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(messagingPageSelectButton_Press);
            OptionSelectButton tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD", false);
            tacticalAwarenessDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(tacticalAwarenessDisplayPageSelectButton_Press);
            OptionSelectButton targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(targetingPodPageSelectButton_Press);
            OptionSelectButton headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", true);
            headDownDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(headDownDisplayPageSelectButton_Press);
            OptionSelectButton headingSelectDownButton = CreateOptionSelectButton(thisPage, 20, @"\/", false, triangleLegLengthPixels);
            headingSelectDownButton.FunctionName = "HeadingSelectDecrease";
            headingSelectDownButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(headingSelectDownButton_Press);
            OptionSelectButton headingSelectLabel = CreateOptionSelectButton(thisPage, 20.5f, "HDG", false);
            OptionSelectButton headingSelectUpButton = CreateOptionSelectButton(thisPage, 21, "^", false, triangleLegLengthPixels);
            headingSelectUpButton.FunctionName = "HeadingSelectIncrease";
            headingSelectUpButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(headingSelectUpButton_Press);
            OptionSelectButton lowAltitudeThresholdSelectDownButton = CreateOptionSelectButton(thisPage, 22, @"\/", false, triangleLegLengthPixels);
            lowAltitudeThresholdSelectDownButton.FunctionName = "LowAltitudeWarningThresholdDecrease";
            lowAltitudeThresholdSelectDownButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(lowAltitudeThresholdSelectDownButton_Press);
            OptionSelectButton lowAltitudeThresholdLabel = CreateOptionSelectButton(thisPage, 22.5f, "ALOW", false);
            OptionSelectButton lowAltitudeThresholdSelectUpButton = CreateOptionSelectButton(thisPage, 23, "^", false, triangleLegLengthPixels);
            lowAltitudeThresholdSelectUpButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(lowAltitudeThresholdSelectUpButton_Press);
            lowAltitudeThresholdSelectUpButton.FunctionName = "LowAltitudeWarningThresholdIncrease";
            OptionSelectButton airspeedIndexSelectDownButton = CreateOptionSelectButton(thisPage, 24, @"\/", false, triangleLegLengthPixels);
            airspeedIndexSelectDownButton.FunctionName = "AirspeedIndexDecrease";
            airspeedIndexSelectDownButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(airspeedIndexSelectDownButton_Press);
            OptionSelectButton airspeedIndexLabel = CreateOptionSelectButton(thisPage, 24.5f, "ASPD", false);
            OptionSelectButton airspeedIndexSelectUpButton = CreateOptionSelectButton(thisPage, 25, @"^", false, triangleLegLengthPixels);
            airspeedIndexSelectUpButton.FunctionName = "AirspeedIndexIncrease";
            airspeedIndexSelectUpButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(airspeedIndexSelectUpButton_Press);

            buttons.Add(pneuButton);
            buttons.Add(ackButton);
            buttons.Add(testPageSelectButton);
            buttons.Add(altitudeIndexUpButton);
            buttons.Add(altitudeIndexLabel);
            buttons.Add(altitudeIndexDownButton);
            buttons.Add(barometricPressureUpButton);
            buttons.Add(barometricPressureLabel);
            buttons.Add(barometricPressureDownButton);
            buttons.Add(courseSelectUpButton);
            buttons.Add(courseSelectLabel);
            buttons.Add(courseSelectDownButton);
            buttons.Add(imagingPageSelectButton);
            buttons.Add(messagingPageSelectButton);
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);
            buttons.Add(targetingPodPageSelectButton);
            buttons.Add(headDownDisplayPageSelectButton);
            buttons.Add(headingSelectDownButton);
            buttons.Add(headingSelectLabel);
            buttons.Add(headingSelectUpButton);
            buttons.Add(lowAltitudeThresholdSelectDownButton);
            buttons.Add(lowAltitudeThresholdLabel);
            buttons.Add(lowAltitudeThresholdSelectUpButton);
            buttons.Add(airspeedIndexSelectDownButton);
            buttons.Add(airspeedIndexLabel);
            buttons.Add(airspeedIndexSelectUpButton);
            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Instruments Display Page";
            return thisPage;
        }

        private void ackButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton4, (OptionSelectButton)sender);
        }

        private void testPageSelectButton_Press(object sender, EventArgs e)
        {
            MfdMenuPage newPage = FindMenuPageByName("Test Page");
            this.ActiveMenuPage = newPage;
        }

        private void airspeedIndexSelectUpButton_Press(object sender, EventArgs e)
        {
            AirspeedIndexInKnots += 20;
        }

        private void airspeedIndexSelectDownButton_Press(object sender, EventArgs e)
        {
            AirspeedIndexInKnots -= 20;
        }

        private void lowAltitudeThresholdSelectUpButton_Press(object sender, EventArgs e)
        {
            OptionSelectButton button = (OptionSelectButton)sender;
            if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton23, button);
        }

        private void lowAltitudeThresholdSelectDownButton_Press(object sender, EventArgs e)
        {
            OptionSelectButton button = (OptionSelectButton)sender;
            if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton22, button);
        }

        private void headingSelectUpButton_Press(object sender, MomentaryButtonPressedEventArgs e)
        {
            OptionSelectButton button = (OptionSelectButton)sender;
            DateTime whenPressed = e.WhenPressed;
            int howLongPressed = (int)DateTime.Now.Subtract(whenPressed).TotalMilliseconds;
            float secondsPressed = (int)(howLongPressed / 1000);
            int numTimes = 1;
            if (howLongPressed >300) numTimes = Properties.Settings.Default.FastCourseAndHeadingAdjustSpeed;

            for (int i = 0; i < numTimes; i++)
            {
                FlightData.HsiDesiredHeadingInDegrees += 1;
                if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton21, (OptionSelectButton)sender);
            }
        }

        private void headingSelectDownButton_Press(object sender, MomentaryButtonPressedEventArgs e)
        {
            OptionSelectButton button = (OptionSelectButton)sender;
            DateTime whenPressed = e.WhenPressed;
            int howLongPressed = (int)DateTime.Now.Subtract(whenPressed).TotalMilliseconds;
            float secondsPressed = (int)(howLongPressed / 1000);
            int numTimes = 1;
            if (howLongPressed > 300) numTimes = Properties.Settings.Default.FastCourseAndHeadingAdjustSpeed;

            for (int i = 0; i < numTimes; i++)
            {
                FlightData.HsiDesiredHeadingInDegrees -= 1;
                if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton20, (OptionSelectButton)sender);
            }
        }

        private void headDownDisplayPageSelectButton_Press(object sender, EventArgs e)
        {
            MfdMenuPage newPage = FindMenuPageByName("Instruments Display Page");
            this.ActiveMenuPage = newPage;
        }

        private void targetingPodPageSelectButton_Press(object sender, EventArgs e)
        {
            MfdMenuPage newPage = FindMenuPageByName("Targeting Pod Page");
            this.ActiveMenuPage = newPage;
        }

        private void tacticalAwarenessDisplayPageSelectButton_Press(object sender, EventArgs e)
        {
            MfdMenuPage newPage = FindMenuPageByName("TAD Page");
            this.ActiveMenuPage = newPage;
        }

        private void messagingPageSelectButton_Press(object sender, EventArgs e)
        {
            MfdMenuPage newPage = FindMenuPageByName("Message Page");
            this.ActiveMenuPage = newPage;
        }

        private void imagingPageSelectButton_Press(object sender, EventArgs e)
        {
            MfdMenuPage newPage = FindMenuPageByName("Bitmap Annotation Page");
            this.ActiveMenuPage = newPage;
        }

        private void courseSelectDownButton_Press(object sender, MomentaryButtonPressedEventArgs e)
        {
            OptionSelectButton button = (OptionSelectButton)sender;

            DateTime whenPressed = e.WhenPressed;
            int howLongPressed = (int)DateTime.Now.Subtract(whenPressed).TotalMilliseconds;
            float secondsPressed = (int)(howLongPressed / 1000);
            int numTimes = 1;
            if (howLongPressed > 300) numTimes = Properties.Settings.Default.FastCourseAndHeadingAdjustSpeed;

            for (int i = 0; i < numTimes; i++)
            {
                FlightData.HsiDesiredCourseInDegrees -= 1;
                if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton12, (OptionSelectButton)sender);
            }
        }

        private void courseSelectUpButton_Press(object sender, MomentaryButtonPressedEventArgs e)
        {
            OptionSelectButton button = (OptionSelectButton)sender;
            DateTime whenPressed = e.WhenPressed;
            int howLongPressed = (int)DateTime.Now.Subtract(whenPressed).TotalMilliseconds;
            float secondsPressed = (int)(howLongPressed / 1000);
            int numTimes = 1;
            if (howLongPressed > 300) numTimes = Properties.Settings.Default.FastCourseAndHeadingAdjustSpeed;

            for (int i = 0; i < numTimes; i++)
            {
                FlightData.HsiDesiredCourseInDegrees +=1 ;
                if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton11, (OptionSelectButton)sender);
            }
        }

        private void barometricPressureDownButton_Press(object sender, EventArgs e)
        {
            OptionSelectButton button = (OptionSelectButton)sender;
            FlightData.BarometricPressureInDecimalInchesOfMercury -= 0.01f;
            if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton10, (OptionSelectButton)sender);
        }

        private void barometricPressureUpButton_Press(object sender, EventArgs e)
        {
            OptionSelectButton button = (OptionSelectButton)sender;
            FlightData.BarometricPressureInDecimalInchesOfMercury += 0.01f;
            if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton9, (OptionSelectButton)sender);
        }

        private void altitudeIndexDownButton_Press(object sender, MomentaryButtonPressedEventArgs e)
        {
            OptionSelectButton button = (OptionSelectButton)sender;

            DateTime whenPressed = e.WhenPressed;
            int howLongPressed = (int)DateTime.Now.Subtract(whenPressed).TotalMilliseconds;
            float secondsPressed = (int)(howLongPressed / 1000);
            int valueDelta = 20;

            if (howLongPressed>= 200) valueDelta = 100;
            if (secondsPressed >= 1) valueDelta = 500;
            if (secondsPressed >= 2) valueDelta = 1000;

            int diff = valueDelta - ((((int)(AltitudeIndexInFeet / valueDelta)) * valueDelta) - AltitudeIndexInFeet);
            AltitudeIndexInFeet -= diff;
        }

        private void altitudeIndexUpButton_Press(object sender, MomentaryButtonPressedEventArgs e)
        {
            OptionSelectButton button = (OptionSelectButton)sender;

            DateTime whenPressed = e.WhenPressed;
            int howLongPressed = (int)DateTime.Now.Subtract(whenPressed).TotalMilliseconds;
            float secondsPressed = (int)(howLongPressed / 1000);
            int valueDelta = 20;

            if (howLongPressed >= 200) valueDelta = 100;
            if (secondsPressed >= 1) valueDelta = 500;
            if (secondsPressed >= 2) valueDelta = 1000;

            int diff = valueDelta + ((((int)(AltitudeIndexInFeet / valueDelta)) * valueDelta) - AltitudeIndexInFeet);
            AltitudeIndexInFeet += diff;
        }

        private void pneuButton_Press(object sender, EventArgs e)
        {
            OptionSelectButton button = (OptionSelectButton)sender;
            if (FlightData.AltimeterMode == AltimeterMode.Electronic) {
                FlightData.AltimeterMode = AltimeterMode.Pneumatic;
                button.LabelText = "PNEU";
            }
            else 
            {
                button.LabelText = "ELEC";
                FlightData.AltimeterMode = AltimeterMode.Electronic;
            }
            if (_simSupportModule != null) _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton3, (OptionSelectButton)sender);

        }
        private MfdMenuPage FindMenuPageByName(string name)
        {
            foreach (MfdMenuPage page in this.MenuPages)
            {
                if (name.ToLowerInvariant().Trim() == page.Name.ToLowerInvariant().Trim())
                {
                    return page;
                }
            }
            return null;
        }
        private MfdMenuPage BuildTargetingPodMenuPage()
        {
            MfdMenuPage thisPage = new MfdMenuPage(this);
            List<OptionSelectButton> buttons = new List<OptionSelectButton>();

            OptionSelectButton testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(testPageSelectButton_Press);
            buttons.Add(testPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 10, "CLR MSG", false));

            OptionSelectButton imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(imagingPageSelectButton_Press);
            buttons.Add(imagingPageSelectButton);

            OptionSelectButton messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(messagingPageSelectButton_Press);
            buttons.Add(messagingPageSelectButton);

            OptionSelectButton tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD", false);
            tacticalAwarenessDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(tacticalAwarenessDisplayPageSelectButton_Press);
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            OptionSelectButton targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", true);
            targetingPodPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(targetingPodPageSelectButton_Press);
            buttons.Add(targetingPodPageSelectButton);

            OptionSelectButton headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(headDownDisplayPageSelectButton_Press);
            buttons.Add(headDownDisplayPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 22, "CAP", false));
            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Targeting Pod Page";
            return thisPage;
        }

        private MfdMenuPage BuildMessageMenuPage()
        {
            MfdMenuPage thisPage = new MfdMenuPage(this);
            List<OptionSelectButton> buttons = new List<OptionSelectButton>();
            buttons.Add(CreateOptionSelectButton(thisPage, 2, "TO USB", false));

            OptionSelectButton testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(testPageSelectButton_Press);
            buttons.Add(testPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 6, "SADL", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 11, "DEL", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 12, "DEL ALL", false));

            OptionSelectButton imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(imagingPageSelectButton_Press);
            buttons.Add(imagingPageSelectButton);

            OptionSelectButton messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", true);
            messagingPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(messagingPageSelectButton_Press);
            buttons.Add(messagingPageSelectButton);

            OptionSelectButton tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD", false);
            tacticalAwarenessDisplayPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(tacticalAwarenessDisplayPageSelectButton_Press);
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);
            
            OptionSelectButton targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(targetingPodPageSelectButton_Press);
            buttons.Add(targetingPodPageSelectButton);

            OptionSelectButton headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(headDownDisplayPageSelectButton_Press);
            buttons.Add(headDownDisplayPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 24, @"\/", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 24.5f, "NO MSG\n\r", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 25, @"^", false));
            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Message Page";
            return thisPage;
        }
        private string GetCADRGScaleTextForMapScale(float mapScale)
        {
            string toReturn = "1:";

            int millions = (int)Math.Round(mapScale / (1000.0f * 1000.0f), 0);
            int thousands = (int)Math.Round(mapScale / 1000.0f, 0);
            int hundreds = (int)Math.Round(mapScale / 100.0f, 0);
            int tens = (int)Math.Round(mapScale / 10.0f, 0);
            int ones = (int)Math.Round(mapScale, 0);
            if (millions > 0)
            {
                toReturn += millions.ToString() + " M";
            }
            else if (thousands > 0)
            {
                toReturn += thousands.ToString() + " K";
            }
            else if (hundreds > 0)
            {
                toReturn += hundreds.ToString();
            }
            else
            {
                toReturn += ones.ToString();
            }
            return toReturn;
        }

        private MfdMenuPage BuildChecklistMenuPage()
        {
            MfdMenuPage thisPage = new MfdMenuPage(this);
            List<OptionSelectButton> buttons = new List<OptionSelectButton>();

            OptionSelectButton controlMapPageSelectButton = CreateOptionSelectButton(thisPage, 1, "CNTL", false);
            controlMapPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(controlMapPageSelectButton_Press);
            buttons.Add(controlMapPageSelectButton);

            OptionSelectButton chartPageSelectButton = CreateOptionSelectButton(thisPage, 2, "CHARTS", false);
            chartPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(chartPageSelectButton_Pressed);
            buttons.Add(chartPageSelectButton);

            OptionSelectButton checklistPageSelectButton= CreateOptionSelectButton(thisPage, 3, "CHKLST", true);
            checklistPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(checklistPageSelectButton_Pressed);
            buttons.Add(checklistPageSelectButton);

            OptionSelectButton mapPageSelectButton = CreateOptionSelectButton(thisPage, 4, "MAP", false);
            mapPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(mapPageSelectButton_Pressed);
            buttons.Add(mapPageSelectButton);

            OptionSelectButton testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(testPageSelectButton_Press);
            buttons.Add(testPageSelectButton);

            OptionSelectButton previousChecklistFileSelectButton= CreateOptionSelectButton(thisPage, 7, "^", false);
            previousChecklistFileSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(previousChecklistFileSelectButton_Pressed);
            buttons.Add(previousChecklistFileSelectButton);

            OptionSelectButton currentChecklistFileLabel = CreateOptionSelectButton(thisPage, 8, "NO CHKLST\nFILES", false);
            currentChecklistFileLabel.FunctionName = "CurrentChecklistFileLabel";
            buttons.Add(currentChecklistFileLabel);

            OptionSelectButton nextChecklistFileSelectButton= CreateOptionSelectButton(thisPage, 9, @"\/", false);
            nextChecklistFileSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(nextChecklistFileSelectButton_Pressed);
            buttons.Add(nextChecklistFileSelectButton);

            OptionSelectButton imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(imagingPageSelectButton_Press);
            buttons.Add(imagingPageSelectButton);

            OptionSelectButton messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(messagingPageSelectButton_Press);
            buttons.Add(messagingPageSelectButton);

            OptionSelectButton tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD", true);
            tacticalAwarenessDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(tacticalAwarenessDisplayPageSelectButton_Press);
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            OptionSelectButton targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(targetingPodPageSelectButton_Press);
            buttons.Add(targetingPodPageSelectButton);

            OptionSelectButton headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(headDownDisplayPageSelectButton_Press);
            buttons.Add(headDownDisplayPageSelectButton);






            OptionSelectButton nextChecklistPageSelectButton = CreateOptionSelectButton(thisPage, 23, @"\/", false);
            nextChecklistPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(nextChecklistPageSelectButton_Pressed);
            buttons.Add(nextChecklistPageSelectButton);

            OptionSelectButton currentChecklistPageNumLabel = CreateOptionSelectButton(thisPage, 24, "page", false);
            currentChecklistPageNumLabel.FunctionName = "CurrentChecklistPageNumLabel";
            buttons.Add(currentChecklistPageNumLabel);

            OptionSelectButton prevChecklistPageSelectButton = CreateOptionSelectButton(thisPage, 25, @"^", false);
            prevChecklistPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(prevChecklistPageSelectButton_Pressed);
            buttons.Add(prevChecklistPageSelectButton);

            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Checklists Page";
            return thisPage;
        }

        private MfdMenuPage BuildChartsMenuPage()
        {
            MfdMenuPage thisPage = new MfdMenuPage(this);
            List<OptionSelectButton> buttons = new List<OptionSelectButton>();

            OptionSelectButton controlMapPageSelectButton = CreateOptionSelectButton(thisPage, 1, "CNTL", false);
            controlMapPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(controlMapPageSelectButton_Press);
            buttons.Add(controlMapPageSelectButton);

            OptionSelectButton chartPageSelectButton = CreateOptionSelectButton(thisPage, 2, "CHARTS", true);
            chartPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(chartPageSelectButton_Pressed);
            buttons.Add(chartPageSelectButton);

            OptionSelectButton checklistPageSelectButton = CreateOptionSelectButton(thisPage, 3, "CHKLST", false);
            checklistPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(checklistPageSelectButton_Pressed);
            buttons.Add(checklistPageSelectButton);

            OptionSelectButton mapPageSelectButton = CreateOptionSelectButton(thisPage, 4, "MAP", false);
            mapPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(mapPageSelectButton_Pressed);
            buttons.Add(mapPageSelectButton);

            OptionSelectButton testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(testPageSelectButton_Press);
            buttons.Add(testPageSelectButton);

            OptionSelectButton previousChartFileSelectButton = CreateOptionSelectButton(thisPage, 7, "^", false);
            previousChartFileSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(previousChartFileSelectButton_Pressed);
            buttons.Add(previousChartFileSelectButton);

            OptionSelectButton currentChartFileLabel = CreateOptionSelectButton(thisPage, 8, "NO CHART\nFILES", false);
            currentChartFileLabel.FunctionName = "CurrentChartFileLabel";
            buttons.Add(currentChartFileLabel);

            OptionSelectButton nextChartFileSelectButton = CreateOptionSelectButton(thisPage, 9, @"\/", false);
            nextChartFileSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(nextChartFileSelectButton_Pressed);
            buttons.Add(nextChartFileSelectButton);

            OptionSelectButton imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(imagingPageSelectButton_Press);
            buttons.Add(imagingPageSelectButton);

            OptionSelectButton messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(messagingPageSelectButton_Press);
            buttons.Add(messagingPageSelectButton);

            OptionSelectButton tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD", true);
            tacticalAwarenessDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(tacticalAwarenessDisplayPageSelectButton_Press);
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            OptionSelectButton targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(targetingPodPageSelectButton_Press);
            buttons.Add(targetingPodPageSelectButton);

            OptionSelectButton headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(headDownDisplayPageSelectButton_Press);
            buttons.Add(headDownDisplayPageSelectButton);






            OptionSelectButton nextChartPageSelectButton = CreateOptionSelectButton(thisPage, 23, @"\/", false);
            nextChartPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(nextChartPageSelectButton_Pressed);
            buttons.Add(nextChartPageSelectButton);

            OptionSelectButton currentChartPageNumLabel = CreateOptionSelectButton(thisPage, 24, "page", false);
            currentChartPageNumLabel.FunctionName = "CurrentChartPageNumLabel";
            buttons.Add(currentChartPageNumLabel);

            OptionSelectButton prevChartPageSelectButton = CreateOptionSelectButton(thisPage, 25, @"^", false);
            prevChartPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(prevChartPageSelectButton_Pressed);
            buttons.Add(prevChartPageSelectButton);

            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Charts Page";
            return thisPage;
        }

        void checklistPageSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            SwitchToChecklistsPage();
        }
        void chartPageSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            SwitchToChartsPage();
        }

        void mapPageSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            SwitchToMapPage();
        }

        void prevChartPageSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            if (_currentChartPageNum > 1) _currentChartPageNum--;
        }

        void nextChartPageSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            if (_currentChartPageNum != _currentChartPagesTotal && _currentChartPagesTotal > 0)
            {
                _currentChartPageNum++;
            }
        }

        void nextChartFileSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            NextChartFile();
        }

        void previousChartFileSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            PrevChartFile();
        }


        void prevChecklistPageSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            if (_currentChecklistPageNum > 1) _currentChecklistPageNum--;
        }

        void nextChecklistPageSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            if (_currentChecklistPageNum != _currentChecklistPagesTotal && _currentChecklistPagesTotal >0)
            {
                _currentChecklistPageNum++;
            }
        }

        void nextChecklistFileSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            NextChecklistFile();
        }

        void previousChecklistFileSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            PrevChecklistFile();
        }
        private MfdMenuPage BuildTADMenuPage()
        {
            MfdMenuPage thisPage = new MfdMenuPage(this);
            List<OptionSelectButton> buttons = new List<OptionSelectButton>();

            OptionSelectButton controlMapPageSelectButton = CreateOptionSelectButton(thisPage, 1, "CNTL", false);
            controlMapPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(controlMapPageSelectButton_Press);
            buttons.Add(controlMapPageSelectButton);

            OptionSelectButton chartPageSelectButton = CreateOptionSelectButton(thisPage, 2, "CHARTS", false);
            chartPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(chartPageSelectButton_Pressed);
            buttons.Add(chartPageSelectButton);

            OptionSelectButton checklistPageSelectButton= CreateOptionSelectButton(thisPage, 3, "CHKLST", false);
            checklistPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(checklistPageSelectButton_Pressed);
            buttons.Add(checklistPageSelectButton);

            OptionSelectButton mapOnOffButton= CreateOptionSelectButton(thisPage, 4, "MAP", true);
            mapOnOffButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(mapOnOffButton_Pressed);
            buttons.Add(mapOnOffButton);

            OptionSelectButton testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(testPageSelectButton_Press);
            buttons.Add(testPageSelectButton);

            OptionSelectButton scaleIncreaseButton = CreateOptionSelectButton(thisPage, 7, "^", false);
            scaleIncreaseButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(scaleIncreaseButton_Pressed);
            buttons.Add(scaleIncreaseButton);

            OptionSelectButton scaleLabel = CreateOptionSelectButton(thisPage, 7.5f, "CADRG\r\n" + GetCADRGScaleTextForMapScale(_mapScale), false);
            scaleLabel.FunctionName = "MapScaleLabel";
            buttons.Add(scaleLabel);

            OptionSelectButton scaleDecreaseButton = CreateOptionSelectButton(thisPage, 8, @"\/", false);
            scaleDecreaseButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(scaleDecreaseButton_Pressed);
            buttons.Add(scaleDecreaseButton);
            buttons.Add(CreateOptionSelectButton(thisPage, 9, "CNTR\r\nOWN", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 10, "CLR MSG", false));

            OptionSelectButton imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(imagingPageSelectButton_Press);
            buttons.Add(imagingPageSelectButton);

            OptionSelectButton messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(messagingPageSelectButton_Press);
            buttons.Add(messagingPageSelectButton);

            OptionSelectButton tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD", true);
            tacticalAwarenessDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(tacticalAwarenessDisplayPageSelectButton_Press);
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            OptionSelectButton targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(targetingPodPageSelectButton_Press);
            buttons.Add(targetingPodPageSelectButton);

            OptionSelectButton headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(headDownDisplayPageSelectButton_Press);
            buttons.Add(headDownDisplayPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 22, "CAP", false));

            OptionSelectButton mapRangeIncrease = CreateOptionSelectButton(thisPage, 25, @"^", false);
            mapRangeIncrease.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(mapRangeIncrease_Pressed);
            buttons.Add(mapRangeIncrease);

            OptionSelectButton mapRangeDecrease= CreateOptionSelectButton(thisPage, 24, @"\/", false);
            mapRangeDecrease.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(mapRangeDecrease_Pressed);
            buttons.Add(mapRangeDecrease);

            OptionSelectButton mapRangeLabel = CreateOptionSelectButton(thisPage, 24.5f, _mapRangeRingsDiameterInNauticalMiles.ToString(), false);
            mapRangeLabel.FunctionName = "MapRangeLabel";
            buttons.Add(mapRangeLabel);

            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "TAD Page";
            return thisPage;
        }

        void mapOnOffButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            SwitchToMapPage();
        }
        private void SwitchToMapPage()
        {
            MfdMenuPage newPage = FindMenuPageByName("TAD Page");
            if (newPage != null)
            {
                this.ActiveMenuPage = newPage;
            }
        }

        private void SwitchToChecklistsPage()
        {
            MfdMenuPage newPage = FindMenuPageByName("Checklists Page");
            if (newPage != null)
            {
                this.ActiveMenuPage = newPage;
            }
        }
        private void SwitchToChartsPage()
        {
            MfdMenuPage newPage = FindMenuPageByName("Charts Page");
            if (newPage != null)
            {
                this.ActiveMenuPage = newPage;
            }
        }

        private void UpdateCurrentChecklistPageCount() 
        {
            if (_currentChecklistFile != null)
            {
                int numPages = PdfRenderEngine.NumPagesInPdf(_currentChecklistFile.FullName);
                _currentChecklistPagesTotal = numPages;
                _currentChecklistPageNum = 1;
            }
            else
            {
                _currentChecklistPagesTotal = 0;
                _currentChecklistPageNum = 0;
            }
        }
        private void NextChecklistFile()
        {
            FileInfo[] files = GetChecklistsFiles();
            _currentChecklistFile = GetNextFile(_currentChecklistFile, files);
            UpdateCurrentChecklistPageCount();
        }
        private void PrevChecklistFile()
        {
            FileInfo[] files = GetChecklistsFiles();
            _currentChecklistFile = GetPrevFile(_currentChecklistFile, files);
            UpdateCurrentChecklistPageCount();
        }



        private void UpdateCurrentChartPageCount()
        {
            if (_currentChartFile != null)
            {
                int numPages = PdfRenderEngine.NumPagesInPdf(_currentChartFile.FullName);
                _currentChartPagesTotal = numPages;
                _currentChartPageNum = 1;
            }
            else
            {
                _currentChartPagesTotal = 0;
                _currentChartPageNum = 0;
            }
        }
        private void NextChartFile()
        {
            FileInfo[] files = GetChartFiles();
            _currentChartFile = GetNextFile(_currentChartFile, files);
            UpdateCurrentChartPageCount();
        }
        private void PrevChartFile()
        {
            FileInfo[] files = GetChartFiles();
            _currentChartFile = GetPrevFile(_currentChartFile, files);
            UpdateCurrentChartPageCount();
        }



        private FileInfo GetPrevFile(FileInfo currentFile, FileInfo[] files)
        {
            if (files == null || files.Length ==0) return null;
            if (currentFile == null) return files[0];
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].FullName == currentFile.FullName)
                {
                    if (i > 0)
                    {
                        return files[i - 1];
                    }
                    else
                    {
                        return files[files.Length - 1];
                    }
                }
            }
            return files[files.Length-1];
        }
        private FileInfo GetNextFile(FileInfo currentFile, FileInfo[] files)
        {
            if (files == null || files.Length == 0) return null;
            if (currentFile == null) return files[0];
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].FullName == currentFile.FullName)
                {
                    if (files.Length - 1 > i)
                    {
                        return files[i + 1];
                    }
                    else
                    {
                        return files[0];
                    }
                }
            }
            return files[0];
        }
        private FileInfo[] GetChecklistsFiles()
        {
            string searchPattern = "*.pdf";
            DirectoryInfo di = new DirectoryInfo(Application.ExecutablePath);
            string folderToSearch = di.Parent.FullName + Path.DirectorySeparatorChar + "checklists";
            return GetFilesOfType(searchPattern, folderToSearch);
        }
        private FileInfo[] GetChartFiles()
        {
            string searchPattern = "*.pdf";
            DirectoryInfo di = new DirectoryInfo(Application.ExecutablePath);
            string folderToSearch = di.Parent.FullName + Path.DirectorySeparatorChar + "charts";
            return GetFilesOfType(searchPattern, folderToSearch);
        }
        private FileInfo[] GetFilesOfType(string searchPattern, string folderToSearch)
        {
            if (String.IsNullOrEmpty(searchPattern) || String.IsNullOrEmpty(folderToSearch))
            {
                return null;
            }
            DirectoryInfo di = new DirectoryInfo(folderToSearch);
            FileInfo[] files = null;
            if (di.Exists)
            {
                files = di.GetFiles(searchPattern);
            }
            return files;

        }
        private void mapRangeDecrease_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            if (_mapRangeRingsDiameterInNauticalMiles > 25)
            {
                _mapRangeRingsDiameterInNauticalMiles -= 5;
            }
            else
            {
                _mapRangeRingsDiameterInNauticalMiles -= 1;
            }
            if (_mapRangeRingsDiameterInNauticalMiles < 0) _mapRangeRingsDiameterInNauticalMiles = 0;
        }

        private void mapRangeIncrease_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            if (_mapRangeRingsDiameterInNauticalMiles >= 20)
            {
                _mapRangeRingsDiameterInNauticalMiles += 5;
            }
            else
            {
                _mapRangeRingsDiameterInNauticalMiles += 1;
            }
            if (_mapRangeRingsDiameterInNauticalMiles > 5000) _mapRangeRingsDiameterInNauticalMiles = 5000;
        }
        private float GetMapScaleForCADRGScaleText(string CADRGScaletext)
        {
            float toReturn = float.NaN;
            switch (CADRGScaletext)
            {
                case "1:250 M":
                    toReturn = 250 * 1000 * 1000;
                    break;
                case "1:100 M":
                    toReturn = 100* 1000* 1000;
                    break;
                case "1:50 M":
                    toReturn = 50* 1000* 1000;
                    break;
                case "1:25 M":
                    toReturn = 25* 1000* 1000;
                    break;
                case "1:10 M":
                    toReturn = 10* 1000* 1000;
                    break;
                case "1:5 M":
                    toReturn = 5* 1000* 1000;
                    break;
                case "1:2 M":
                    toReturn = 2* 1000* 1000;
                    break;
                case "1:1 M":
                    toReturn = 1000* 1000;
                    break;
                case "1:500 K":
                    toReturn = 500* 1000;
                    break;
                case "1:250 K":
                    toReturn = 250* 1000;
                    break;
                case "1:100 K":
                    toReturn = 100* 1000;
                    break;
                case "1:50 K":
                    toReturn = 50* 1000;
                    break;
                case "1:25 K":
                    toReturn = 25* 1000;
                    break;
                case "1:10 K":
                    toReturn = 10* 1000;
                    break;
                case "1:5 K":
                    toReturn = 5* 1000;
                    break;
                default:
                    break;
            }
            return toReturn;
        }
        private float GetNextLowerMapScale(float mapScale)
        {
            float toReturn = mapScale;
            string mapScaleText = GetCADRGScaleTextForMapScale(mapScale);
            switch (mapScaleText)
            {
                case "1:250 M":
                    toReturn = GetMapScaleForCADRGScaleText("1:250 M");
                    break;
                case "1:100 M":
                    toReturn = GetMapScaleForCADRGScaleText("1:250 M");
                    break;
                case "1:50 M":
                    toReturn = GetMapScaleForCADRGScaleText("1:100 M");
                    break;
                case "1:25 M":
                    toReturn = GetMapScaleForCADRGScaleText("1:50 M");
                    break;
                case "1:10 M":
                    toReturn = GetMapScaleForCADRGScaleText("1:25 M");
                    break;
                case "1:5 M":
                    toReturn = GetMapScaleForCADRGScaleText("1:10 M");
                    break;
                case "1:2 M":
                    toReturn = GetMapScaleForCADRGScaleText("1:5 M");
                    break;
                case "1:1 M":
                    toReturn = GetMapScaleForCADRGScaleText("1:2 M");
                    break;
                case "1:500 K":
                    toReturn = GetMapScaleForCADRGScaleText("1:1 M");
                    break;
                case "1:250 K":
                    toReturn = GetMapScaleForCADRGScaleText("1:500 K");
                    break;
                case "1:100 K":
                    toReturn = GetMapScaleForCADRGScaleText("1:250 K");
                    break;
                case "1:50 K":
                    toReturn = GetMapScaleForCADRGScaleText("1:100 K");
                    break;
                case "1:25 K":
                    toReturn = GetMapScaleForCADRGScaleText("1:50 K");
                    break;
                case "1:10 K":
                    toReturn = GetMapScaleForCADRGScaleText("1:25 K");
                    break;
                case "1:5 K":
                    toReturn = GetMapScaleForCADRGScaleText("1:10 K");
                    break;
                default:
                    break;
            }
            return toReturn;
        }
        private float GetNextHigherMapScale(float mapScale)
        {
            float toReturn = mapScale;
            string mapScaleText = GetCADRGScaleTextForMapScale(mapScale);
            switch (mapScaleText)
            {
                case "1:250 M":
                    toReturn = GetMapScaleForCADRGScaleText("1:100 M");
                    break;
                case "1:100 M":
                    toReturn = GetMapScaleForCADRGScaleText("1:50 M");
                    break;
                case "1:50 M":
                    toReturn = GetMapScaleForCADRGScaleText("1:25 M");
                    break;
                case "1:25 M":
                    toReturn = GetMapScaleForCADRGScaleText("1:10 M");
                    break;
                case "1:10 M":
                    toReturn = GetMapScaleForCADRGScaleText("1:5 M");
                    break;
                case "1:5 M":
                    toReturn = GetMapScaleForCADRGScaleText("1:2 M");
                    break;
                case "1:2 M":
                    toReturn = GetMapScaleForCADRGScaleText("1:1 M");
                    break;
                case "1:1 M":
                    toReturn = GetMapScaleForCADRGScaleText("1:500 K");
                    break;
                case "1:500 K":
                    toReturn = GetMapScaleForCADRGScaleText("1:250 K");
                    break;
                case "1:250 K":
                    toReturn = GetMapScaleForCADRGScaleText("1:100 K");
                    break;
                case "1:100 K":
                    toReturn = GetMapScaleForCADRGScaleText("1:50 K");
                    break;
                case "1:50 K":
                    toReturn = GetMapScaleForCADRGScaleText("1:25 K");
                    break;
                case "1:25 K":
                    toReturn = GetMapScaleForCADRGScaleText("1:10 K");
                    break;
                case "1:10 K":
                    toReturn = GetMapScaleForCADRGScaleText("1:5 K");
                    break;
                case "1:5 K":
                    toReturn = GetMapScaleForCADRGScaleText("1:5 K");
                    break;
                default:
                    break;
            }
            return toReturn;
        }
        private void scaleDecreaseButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            _mapScale = GetNextLowerMapScale(_mapScale);
        }

        private void scaleIncreaseButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            _mapScale = GetNextHigherMapScale(_mapScale);
        }

        private MfdMenuPage BuildControlMapMenuPage()
        {
            MfdMenuPage thisPage = new MfdMenuPage(this);
            List<OptionSelectButton> buttons = new List<OptionSelectButton>();
            buttons.Add(CreateOptionSelectButton(thisPage, 1, "CNTL", true));
            buttons.Add(CreateOptionSelectButton(thisPage, 2, "PROF", false));
            
            OptionSelectButton controlMapPageSelectButton = CreateOptionSelectButton(thisPage, 3, "MAP", true);
            controlMapPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(controlMapPageSelectButton_Press);
            buttons.Add(controlMapPageSelectButton);

            OptionSelectButton controlOverlayPageSelectButton = CreateOptionSelectButton(thisPage, 4, "OVR", false);
            controlOverlayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(controlOverlayPageSelectButton_Press);
            buttons.Add(controlOverlayPageSelectButton);

            OptionSelectButton testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(testPageSelectButton_Press);
            buttons.Add(testPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 7, "DEL\n\rCIB", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 8, "DEL\n\rMAP", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 9, "DEL\n\rDRW", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 10, "DEL\n\rSHP", false));

            OptionSelectButton imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(imagingPageSelectButton_Press);
            buttons.Add(imagingPageSelectButton);

            OptionSelectButton messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(messagingPageSelectButton_Press);
            buttons.Add(messagingPageSelectButton);

            OptionSelectButton tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD", false);
            tacticalAwarenessDisplayPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(tacticalAwarenessDisplayPageSelectButton_Press);
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            OptionSelectButton targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(targetingPodPageSelectButton_Press);
            buttons.Add(targetingPodPageSelectButton);

            OptionSelectButton headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false); 
            headDownDisplayPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(headDownDisplayPageSelectButton_Press);
            buttons.Add(headDownDisplayPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 21, "LOAD\n\rECHM", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 22, "LOAD\n\rSHP", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 23, "LOAD\n\rDRW", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 24, "LOAD\n\rMAP", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 25, "LOAD\n\rCIB", false));
            
            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Control Map Page";
            return thisPage;
        }

        private void controlOverlayPageSelectButton_Press(object sender, EventArgs e)
        {
            MfdMenuPage newPage = FindMenuPageByName("Control Overlay Page");
            this.ActiveMenuPage = newPage;
        }

        private void controlMapPageSelectButton_Press(object sender, EventArgs e)
        {
            MfdMenuPage newPage = FindMenuPageByName("Control Map Page");
            this.ActiveMenuPage = newPage;
        }

        private MfdMenuPage BuildControlOverlayMenuPage()
        {
            MfdMenuPage thisPage = new MfdMenuPage(this);
            List<OptionSelectButton> buttons = new List<OptionSelectButton>();
            buttons.Add(CreateOptionSelectButton(thisPage, 1, "CNTL", true));
            buttons.Add(CreateOptionSelectButton(thisPage, 2, "PROF", false));

            OptionSelectButton controlMapPageSelectButton = CreateOptionSelectButton(thisPage, 3, "MAP", false);
            controlMapPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(controlMapPageSelectButton_Press);
            buttons.Add(controlMapPageSelectButton);

            OptionSelectButton controlOverlayPageSelectButton = CreateOptionSelectButton(thisPage, 4, "OVR", true);
            controlOverlayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(controlOverlayPageSelectButton_Press);
            buttons.Add(controlOverlayPageSelectButton);

            OptionSelectButton testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(testPageSelectButton_Press);
            buttons.Add(testPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 7, "^", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 8, "NO DRW FILES", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 9, @"\/", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 10, "ECHUM", false));

            OptionSelectButton imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(imagingPageSelectButton_Press);
            buttons.Add(imagingPageSelectButton);

            OptionSelectButton messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(messagingPageSelectButton_Press);
            buttons.Add(messagingPageSelectButton);

            OptionSelectButton tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD", false);
            tacticalAwarenessDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(tacticalAwarenessDisplayPageSelectButton_Press);
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            OptionSelectButton targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(targetingPodPageSelectButton_Press);
            buttons.Add(targetingPodPageSelectButton);

            OptionSelectButton headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(headDownDisplayPageSelectButton_Press);
            buttons.Add(headDownDisplayPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 23, @"\/", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 24, "NO SHP FILES", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 25, "^", false));
            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Control Overlay Page";
            return thisPage;
        }
        private MfdMenuPage BuildBitmapAnnotationMenuPage()
        {
            MfdMenuPage thisPage = new MfdMenuPage(this);
            List<OptionSelectButton> buttons = new List<OptionSelectButton>();
            buttons.Add(CreateOptionSelectButton(thisPage, 2, "TO\n\rINTEL", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 4, "OWN\n\rFRISCO", false));

            OptionSelectButton testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed +=new EventHandler<MomentaryButtonPressedEventArgs>(testPageSelectButton_Press);
            buttons.Add(testPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 7, "^", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 7.5f, "16102F\n\r16UKWN0", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 8, @"\/", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 9, "SEND", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 10, "DATA\n\rMODE", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 11, "DEL", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 12, "DEL ALL", false));


            OptionSelectButton imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", true);
            imagingPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(imagingPageSelectButton_Press);
            buttons.Add(imagingPageSelectButton);

            OptionSelectButton messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(messagingPageSelectButton_Press);
            buttons.Add(messagingPageSelectButton);

            OptionSelectButton tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD", false);
            tacticalAwarenessDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(tacticalAwarenessDisplayPageSelectButton_Press);
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            OptionSelectButton targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(targetingPodPageSelectButton_Press);
            buttons.Add(targetingPodPageSelectButton);

            OptionSelectButton headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += new EventHandler<MomentaryButtonPressedEventArgs>(headDownDisplayPageSelectButton_Press);
            buttons.Add(headDownDisplayPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 22, "CAP", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 23, "X1", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 24, "TO USB", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 25, "TO MFCD", false));
            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Bitmap Annotation Page";
            return thisPage;
        }
        private OptionSelectButton CreateOptionSelectButton(MfdMenuPage page, float positionNum, string labelText, bool invertLabelText)
        {
            return CreateOptionSelectButton(page, positionNum, labelText, invertLabelText, null);
        }
        private OptionSelectButton CreateOptionSelectButton(MfdMenuPage page, float positionNum, string labelText, bool invertLabelText, int? triangleLegLengthPixels)
        {
            OptionSelectButton button = new OptionSelectButton(page);
            button.PositionNumber = positionNum;
            button.LabelText = labelText;
            button.InvertLabelText = invertLabelText;
            Rectangle boundingRectangle = CalculateOSBLabelBitmapRectangle(positionNum);
            button.LabelLocation = boundingRectangle.Location;
            button.LabelSize = boundingRectangle.Size;
            if (triangleLegLengthPixels.HasValue)
            {
                button.TriangleLegLength = triangleLegLengthPixels.Value;
            }
            if (positionNum >= 1 && positionNum <= 5)
            {
                //TOP 
                button.TextVAlignment = VAlignment.Top;
                button.TextHAlignment = HAlignment.Center;
            }
            else if (positionNum >= 6 && positionNum <=13)
            {
                //RIGHT
                button.TextVAlignment = VAlignment.Center;
                button.TextHAlignment = HAlignment.Right;
            }
            else if (positionNum >= 14 && positionNum <= 18)
            {
                //BOTTOM
                button.TextVAlignment = VAlignment.Bottom;
                button.TextHAlignment = HAlignment.Center;
            }
            else if (positionNum >= 19 && positionNum <= 26)
            {
                //LEFT
                button.TextVAlignment = VAlignment.Center;
                button.TextHAlignment = HAlignment.Left;
            }

            if (labelText.Trim() == "^")
            {
                button.TextVAlignment = VAlignment.Center;
            }
            else if (labelText.Trim() == @"\/")
            {
                button.TextVAlignment = VAlignment.Center;
            }
            return button;
        }
        private Rectangle CalculateOSBLabelBitmapRectangle(float positionNum)
        {
            float pixelsPerInch = Constants.F_NATIVE_RES_HEIGHT/8.32f;
            float bezelButtonRevealWidthInches = 0.83376676384839650145772594752187f;
            int bezelButtonRevealWidthPixels = (int)Math.Floor((bezelButtonRevealWidthInches * pixelsPerInch));

            float bezelButtonRevealHeightInches = 0.83695842450765864332603938730853f;
            int bezelButtonRevealHeightPixels = (int)Math.Floor((bezelButtonRevealHeightInches * pixelsPerInch));

            int maxTextWidthPixels = (int)(bezelButtonRevealWidthPixels * 1.5f);
            float bezelButtonSeparatorWidthInches = 0.14500291545189504373177842565598f;
            int bezelButtonSeparatorWidthPixels = (int)(Math.Ceiling(bezelButtonSeparatorWidthInches * pixelsPerInch));
            float bezelButtonSeparatorHeightInches = bezelButtonSeparatorWidthInches;//0.14555798687089715536105032822757f;
            int bezelButtonSeparatorHeightPixels = (int)(Math.Ceiling(bezelButtonSeparatorHeightInches * pixelsPerInch));
            int leftMarginPixels = (int)(((Constants.I_NATIVE_RES_WIDTH - ((5 * bezelButtonRevealWidthPixels) + (4* bezelButtonSeparatorWidthPixels))) / 2.0f));
            int topMarginPixels = (int)(((Constants.I_NATIVE_RES_HEIGHT - ((8 * bezelButtonRevealHeightPixels) + (7 * bezelButtonSeparatorHeightPixels))) / 2.0f));
            Rectangle boundingRectangle = new Rectangle();
            if (positionNum >= 1 && positionNum <=5)
            {
                //TOP ROW OF BUTTONS
                int x = (int)(((positionNum - 1) * (bezelButtonRevealWidthPixels + bezelButtonSeparatorWidthPixels))) + leftMarginPixels;
                boundingRectangle.Location = new Point(x, 0);
                int width = bezelButtonRevealWidthPixels;
                int height = bezelButtonRevealHeightPixels;
                boundingRectangle.Size = new Size(width, height);
            }
            else if (positionNum >= 6 && positionNum <=13)
            {
                //RIGHT HAND SIDE BUTTONS
                int y = (int)(((positionNum - 6) * (bezelButtonRevealHeightPixels + bezelButtonSeparatorHeightPixels))) + topMarginPixels;
                int width = maxTextWidthPixels;
                int height = bezelButtonRevealHeightPixels;
                boundingRectangle.Size = new Size(width, height);
                boundingRectangle.Location = new Point(Constants.I_NATIVE_RES_WIDTH- width, y);
            }
            else if (positionNum >= 14 && positionNum <= 18)
            {
                //BOTTOM ROW OF BUTTONS
                int x = (int)(((18 - positionNum) * (bezelButtonRevealWidthPixels + bezelButtonSeparatorWidthPixels))) + leftMarginPixels;
                int y = Constants.I_NATIVE_RES_HEIGHT - bezelButtonRevealHeightPixels;
                boundingRectangle.Location = new Point(x, y);
                int width = bezelButtonRevealWidthPixels;
                int height = bezelButtonRevealHeightPixels;
                boundingRectangle.Size = new Size(width, height);
            }
            else if (positionNum >= 19 && positionNum <= 26)
            {
                //LEFT HAND SIDE BUTTONS
                int y = (int)(((26 - positionNum) * (bezelButtonRevealHeightPixels + bezelButtonSeparatorHeightPixels))) + topMarginPixels;
                int width = maxTextWidthPixels;
                int height = bezelButtonRevealHeightPixels;
                boundingRectangle.Size = new Size(width, height);
                boundingRectangle.Location = new Point(0, y);
            }
            return boundingRectangle;
        }
        public void ProcessPendingMessages()
        {
            if (Properties.Settings.Default.RunAsServer)
            {
                ProcessPendingMessagesToServerFromClient();
            }
            else if (Properties.Settings.Default.RunAsClient)
            {
                ProcessPendingMessagesToClientFromServer();
            }
        }
        public override void Render(Graphics g)
        {
            Brush greenBrush = new SolidBrush(Color.FromArgb(0, 255, 0));

            DateTime startTime = DateTime.Now;
            //Debug.WriteLine("here 1 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
            //g.Clear(Color.Transparent);
            int labelWidth = (int)(35* ((float)this.ScreenBoundsPixels.Width / Constants.F_NATIVE_RES_WIDTH));
            int labelHeight = (int)(20 * ((float)this.ScreenBoundsPixels.Height / Constants.F_NATIVE_RES_HEIGHT));
            Rectangle overallRenderRectangle = new Rectangle(0,0, this.ScreenBoundsPixels.Width, this.ScreenBoundsPixels.Height);
            OptionSelectButton button = null;
            Matrix origTransform = g.Transform;

            //Debug.WriteLine("here 2 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
            if (this.ActiveMenuPage.Name == "Instruments Display Page")
            {
                button = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("AcknowledgeMessage");
                button.Visible = !this.FlightData.PfdOffFlag & this.FlightData.CpdPowerOnFlag;

                button = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("TestHdd");
                button.Visible = !this.FlightData.PfdOffFlag & this.FlightData.CpdPowerOnFlag;

                button = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("AltitudeIndexIncrease");
                button.Visible = !this.FlightData.PfdOffFlag & this.FlightData.CpdPowerOnFlag;
                button = this.ActiveMenuPage.FindOptionSelectButtonByLabelText("ALT");
                button.Visible = !this.FlightData.PfdOffFlag & this.FlightData.CpdPowerOnFlag;
                button = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("AltitudeIndexDecrease");
                button.Visible = !this.FlightData.PfdOffFlag & this.FlightData.CpdPowerOnFlag;

                button = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("BarometricPressureSettingIncrease");
                button.Visible = !this.FlightData.PfdOffFlag & this.FlightData.CpdPowerOnFlag;
                button = this.ActiveMenuPage.FindOptionSelectButtonByLabelText("BARO");
                button.Visible = !this.FlightData.PfdOffFlag & this.FlightData.CpdPowerOnFlag;
                button = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("BarometricPressureSettingDecrease");
                button.Visible = !this.FlightData.PfdOffFlag & this.FlightData.CpdPowerOnFlag;

                button = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("ToggleAltimeterModeElecPneu");
                button.Visible = !this.FlightData.PfdOffFlag & this.FlightData.CpdPowerOnFlag;

                button = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("LowAltitudeWarningThresholdIncrease");
                button.Visible = !this.FlightData.PfdOffFlag & this.FlightData.CpdPowerOnFlag;
                button = this.ActiveMenuPage.FindOptionSelectButtonByLabelText("ALOW");
                button.Visible = !this.FlightData.PfdOffFlag & this.FlightData.CpdPowerOnFlag;
                button = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("LowAltitudeWarningThresholdDecrease");
                button.Visible = !this.FlightData.PfdOffFlag & this.FlightData.CpdPowerOnFlag;

                button = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("AirspeedIndexIncrease");
                button.Visible = !this.FlightData.PfdOffFlag & this.FlightData.CpdPowerOnFlag;
                button = this.ActiveMenuPage.FindOptionSelectButtonByLabelText("ASPD");
                button.Visible = !this.FlightData.PfdOffFlag & this.FlightData.CpdPowerOnFlag;
                button = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("AirspeedIndexDecrease");
                button.Visible = !this.FlightData.PfdOffFlag & this.FlightData.CpdPowerOnFlag;

                button = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("CourseSelectIncrease");
                button.Visible = !this.FlightData.HsiOffFlag & this.FlightData.CpdPowerOnFlag;
                button = this.ActiveMenuPage.FindOptionSelectButtonByLabelText("CRS");
                button.Visible = !this.FlightData.HsiOffFlag & this.FlightData.CpdPowerOnFlag;
                button = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("CourseSelectDecrease");
                button.Visible = !this.FlightData.HsiOffFlag & this.FlightData.CpdPowerOnFlag;

                button = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("HeadingSelectIncrease");
                button.Visible = !this.FlightData.HsiOffFlag & this.FlightData.CpdPowerOnFlag;
                button = this.ActiveMenuPage.FindOptionSelectButtonByLabelText("HDG");
                button.Visible = !this.FlightData.HsiOffFlag & this.FlightData.CpdPowerOnFlag;
                button = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("HeadingSelectDecrease");
                button.Visible = !this.FlightData.HsiOffFlag & this.FlightData.CpdPowerOnFlag;
            }
            else if (this.ActiveMenuPage.Name == "TAD Page")
            {
                //Debug.WriteLine("here 3 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
                OptionSelectButton scaleLabel = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("MapScaleLabel");
                string scaleString = "";
                scaleString = "CADRG\r\n" + GetCADRGScaleTextForMapScale(_mapScale);
                scaleLabel.LabelText = scaleString;

                OptionSelectButton mapRangeLabel = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("MapRangeLabel");
                string mapRangeString = _mapRangeRingsDiameterInNauticalMiles.ToString();
                mapRangeLabel.LabelText = mapRangeString;
                //Debug.WriteLine("here 4 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
            }
            else if (this.ActiveMenuPage.Name == "Checklists Page")
            {
                if (_currentChecklistFile == null) NextChecklistFile();
                OptionSelectButton currentChecklistFileLabel = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("CurrentChecklistFileLabel");
                string shortName =null;
                if (_currentChecklistFile !=null) 
                {
                    shortName = Common.Win32.Paths.Util.GetShortPathName(_currentChecklistFile.FullName);
                    FileInfo fi = new FileInfo(shortName);
                    shortName = fi.Name;
                    shortName = Common.Win32.Paths.Util.Compact(_currentChecklistFile.Name, 64) ;
                    shortName = BreakStringIntoLines(shortName, 9);

                }
                string labelText = _currentChecklistFile !=null? shortName.ToUpperInvariant(): "NO CHKLST\nFILES";
                currentChecklistFileLabel.LabelText = labelText;

                OptionSelectButton currentChecklistPageNumLabel = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("CurrentChecklistPageNumLabel");
                currentChecklistPageNumLabel.LabelText = _currentChecklistPageNum.ToString() + "/" + _currentChecklistPagesTotal.ToString();
            }
            else if (this.ActiveMenuPage.Name == "Charts Page")
            {
                if (_currentChartFile == null) NextChartFile();
                OptionSelectButton currentChartFileLabel = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("CurrentChartFileLabel");
                string shortName = null;
                if (_currentChartFile != null)
                {
                    shortName = Common.Win32.Paths.Util.GetShortPathName(_currentChartFile.FullName);
                    FileInfo fi = new FileInfo(shortName);
                    shortName = fi.Name;
                    shortName = Common.Win32.Paths.Util.Compact(_currentChartFile.Name, 64);
                    shortName = BreakStringIntoLines(shortName, 9);

                }
                string labelText = _currentChartFile != null ? shortName.ToUpperInvariant() : "NO CHART\nFILES";
                currentChartFileLabel.LabelText = labelText;

                OptionSelectButton currentChartPageNumLabel = this.ActiveMenuPage.FindOptionSelectButtonByFunctionName("CurrentChartPageNumLabel");
                currentChartPageNumLabel.LabelText = _currentChartPageNum.ToString() + "/" + _currentChartPagesTotal.ToString();
            }
            //Debug.WriteLine("here 5 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
            g.SetClip(overallRenderRectangle);
            g.TranslateTransform(overallRenderRectangle.X, overallRenderRectangle.Y);
            Size originalSize = new Size(Constants.I_NATIVE_RES_WIDTH, Constants.I_NATIVE_RES_HEIGHT);
            g.ScaleTransform(((float)overallRenderRectangle.Width / (float)originalSize.Width), ((float)overallRenderRectangle.Height / (float)originalSize.Height));
            //Debug.WriteLine("here 6 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);

            if (!this.FlightData.CpdPowerOnFlag)
            {
                string toDisplay = "OFF";
                GraphicsPath path = new GraphicsPath();
                StringFormat sf = new StringFormat(StringFormatFlags.NoWrap);
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                Font f = new Font (FontFamily.GenericMonospace, 20,System.Drawing.FontStyle.Bold);
                SizeF textSize = g.MeasureString(toDisplay, f, overallRenderRectangle.Size, sf);
                int leftX = (((Constants.I_NATIVE_RES_WIDTH - ((int)textSize.Width)) /2));
                int topY = (((Constants.I_NATIVE_RES_HEIGHT- ((int)textSize.Height)) / 2));
                Rectangle target = new Rectangle(leftX, topY, (int)textSize.Width, (int)textSize.Height);
                path.AddString(toDisplay, f.FontFamily, (int)f.Style, f.SizeInPoints, target, sf);
                /*
                g.FillRectangle(greenBrush, target);
                g.FillPath(Brushes.Black, path); 
                 */
                g.FillPath(greenBrush, path);
                return;

            }

            //Debug.WriteLine("here 7 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);

            if (this.FlightData.CpdPowerOnFlag)
            {
                g.Transform = origTransform;

                if (this.ActiveMenuPage.Name == "Instruments Display Page")
                {
                    DateTime pfdRenderStart = DateTime.Now;
                    Pfd pfd = this.Pfd;
                    pfd.Manager = this;
                    Rectangle pfdRenderRectangle = new Rectangle(labelWidth + 1, labelHeight + 1, (this.ScreenBoundsPixels.Width - ((labelWidth + 1) * 2)), ((this.ScreenBoundsPixels.Height - ((labelHeight + 1) * 2)) / 2)+10);
                    pfdRenderRectangle = new Rectangle(pfdRenderRectangle.Left, pfdRenderRectangle.Top, (int)(pfdRenderRectangle.Width), (int)(pfdRenderRectangle.Height));
                    Size pfdRenderSize = new Size(610, 495);
                    g.SetClip(pfdRenderRectangle);
                    g.TranslateTransform(pfdRenderRectangle.X, pfdRenderRectangle.Y);
                    g.ScaleTransform(((float)pfdRenderRectangle.Width / (float)pfdRenderSize.Width), ((float)pfdRenderRectangle.Height / (float)pfdRenderSize.Height));
                    pfd.Render(g, pfdRenderSize);
                    g.Transform = origTransform;

                    DateTime pfdRenderEnd = DateTime.Now;
                    TimeSpan pfdRenderTime = pfdRenderEnd.Subtract(pfdRenderStart);
                    //Debug.WriteLine("PFD render time:" + pfdRenderTime.TotalMilliseconds);


                    DateTime hsiRenderStart = DateTime.Now;
                    Hsi hsi = this.Hsi;
                    hsi.Manager = this;
                    Rectangle hsiRenderBounds = new Rectangle(pfdRenderRectangle.Left, pfdRenderRectangle.Bottom + 5, pfdRenderRectangle.Width, pfdRenderRectangle.Height-40);
                    Size hsiRenderSize = new Size(596, 391);
                    origTransform = g.Transform;
                    g.SetClip(hsiRenderBounds);
                    g.TranslateTransform(hsiRenderBounds.X, hsiRenderBounds.Y);
                    g.ScaleTransform(((float)hsiRenderBounds.Width / (float)hsiRenderSize.Width), ((float)hsiRenderBounds.Height / (float)hsiRenderSize.Height));
                    hsi.Render(g, hsiRenderSize);
                    g.Transform = origTransform;
                    DateTime hsiRenderEnd = DateTime.Now;
                    TimeSpan hsiRenderTime = hsiRenderEnd.Subtract(hsiRenderStart);
                    //Debug.WriteLine("HSI render time:" + hsiRenderTime.TotalMilliseconds);

                }
                else if (this.ActiveMenuPage.Name == "TAD Page")
                {
                    //Debug.WriteLine("here 8 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);

                    if (Properties.Settings.Default.RunAsClient)
                    {
                        //Debug.WriteLine("here 9 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
                        //render last map image we obtained from the server
                        lock (_mapImageLock)
                        {
                            if (_lastMapFromServer != null)
                            {
                                g.DrawImage(_lastMapFromServer, new Point(0, 0));
                            }
                        }
                        //invoke an async operation that will retrieve any pending map image available at the server
                        GetLatestMapImageFromServerAsync();

                        //send new request to server to generate a new map image 
                        Dictionary<string, object> payload = new Dictionary<string, object>();
                        payload.Add("RenderSize", this.ScreenBoundsPixels);
                        payload.Add("MapScale", _mapScale);
                        payload.Add("RangeRingsDiameter", _mapRangeRingsDiameterInNauticalMiles); 
                        Networking.Message message = new Networking.Message("RequestNewMapImage", payload);
                        this.Client.SendMessageToServer(message);
                        //Debug.WriteLine("here 10 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
                    }
                    else
                    {
                        //Debug.WriteLine("here 11 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
                        RenderMapLocally(g, _mapScale, _mapRangeRingsDiameterInNauticalMiles);
                        //Debug.WriteLine("here 12 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);

                    }
                }
                else if (this.ActiveMenuPage.Name == "Checklists Page")
                {
                    if (_currentChecklistFile != null)
                    {
                        Rectangle checklistRenderRectangle = new Rectangle(labelWidth + 1, labelHeight + 1, (this.ScreenBoundsPixels.Width - ((labelWidth + 1) * 2)), ((this.ScreenBoundsPixels.Height - ((labelHeight + 1) * 2)) ));
                        RenderCurrentChecklist(g, checklistRenderRectangle);
                    }
                }
                else if (this.ActiveMenuPage.Name == "Charts Page")
                {
                    if (_currentChartFile != null)
                    {
                        Rectangle chartRenderRectangle = new Rectangle(labelWidth + 1, labelHeight + 1, (this.ScreenBoundsPixels.Width - ((labelWidth + 1) * 2)), ((this.ScreenBoundsPixels.Height - ((labelHeight + 1) * 2))));
                        RenderCurrentChart(g, chartRenderRectangle);
                    }
                }

                //Debug.WriteLine("here 13 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);

                g.Transform = origTransform;
                g.SetClip(overallRenderRectangle);
                g.TranslateTransform(overallRenderRectangle.X, overallRenderRectangle.Y);
                g.ScaleTransform(((float)overallRenderRectangle.Width / (float)originalSize.Width), ((float)overallRenderRectangle.Height / (float)originalSize.Height));
                //Debug.WriteLine("here 14 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);

                foreach (OptionSelectButton thisButton in this.ActiveMenuPage.OptionSelectButtons)
                {
                    if (thisButton.Visible)
                    {
                        thisButton.DrawLabel(g);
                    }
                }
                //Debug.WriteLine("here 15 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
                g.Transform = origTransform;

                DateTime finishTime = DateTime.Now;
                TimeSpan elapsed = finishTime.Subtract(startTime);
                //Debug.WriteLine("Overall CPD render time:" + elapsed.TotalMilliseconds);
            }
        }
        private string BreakStringIntoLines(string toBreak, int maxLineLength)
        {
            if (toBreak == null) return null;
            if (maxLineLength <= 0) return "";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < toBreak.Length; i++)
            {
                sb.Append(toBreak[i]);
                if ((i+1) % maxLineLength == 0)
                {
                    sb.Append("\n");
                }
            }
            return sb.ToString();
        }
        private void RenderCurrentChecklist(Graphics target, Rectangle targetRect)
        {
            CompositingQuality origCompositQuality = target.CompositingQuality;
            SmoothingMode origSmoothingMode = target.SmoothingMode;
            InterpolationMode origInterpolationMode = target.InterpolationMode;

            target.InterpolationMode = InterpolationMode.HighQualityBicubic;
            target.SmoothingMode = SmoothingMode.HighQuality;
            target.CompositingQuality = CompositingQuality.HighQuality;
            if (_currentChecklistFile != null)
            {
                if (_lastRenderedChecklistFile ==null || _currentChecklistFile.FullName != _lastRenderedChecklistFile.FullName || _lastRenderedChecklistPageNum != _currentChecklistPageNum)
                {
                    Common.Util.DisposeObject(_lastRenderedChecklistPdfPage);
                    _lastRenderedChecklistPdfPage = PdfRenderEngine.GeneratePageBitmap(_currentChecklistFile.FullName, _currentChecklistPageNum, 
                        new Size(150,150));
                    _lastRenderedChecklistPageNum = _currentChecklistPageNum;
                    _lastRenderedChecklistFile = _currentChecklistFile;
                }
                if (_lastRenderedChecklistPdfPage != null)
                {
                    if (_nightMode)
                    {
                        using (Bitmap copy = (Bitmap)Common.Imaging.Util.CopyBitmap(_lastRenderedChecklistPdfPage)) 
                        using (Bitmap reverseVideo = (Bitmap)Common.Imaging.Util.GetDimmerImage(copy, 0.4f))
                        {
                            target.DrawImage(reverseVideo, targetRect, 0, 0, reverseVideo.Width, reverseVideo.Height, GraphicsUnit.Pixel);
                        }
                    }
                    else
                    {
                        target.DrawImage(_lastRenderedChecklistPdfPage, targetRect, 0, 0, _lastRenderedChecklistPdfPage.Width, _lastRenderedChecklistPdfPage.Height, GraphicsUnit.Pixel);
                    }
                }
            }
            target.InterpolationMode = origInterpolationMode;
            target.SmoothingMode = origSmoothingMode;
            target.CompositingQuality = origCompositQuality;
        }

        private void RenderCurrentChart(Graphics target, Rectangle targetRect)
        {
            CompositingQuality origCompositQuality = target.CompositingQuality;
            SmoothingMode origSmoothingMode = target.SmoothingMode;
            InterpolationMode origInterpolationMode = target.InterpolationMode;

            target.InterpolationMode = InterpolationMode.HighQualityBicubic;
            target.SmoothingMode = SmoothingMode.HighQuality;
            target.CompositingQuality = CompositingQuality.HighQuality;
            if (_currentChartFile != null)
            {
                if (_lastRenderedChartFile == null || _currentChartFile.FullName != _lastRenderedChartFile.FullName || _lastRenderedChartPageNum != _currentChartPageNum)
                {
                    Common.Util.DisposeObject(_lastRenderedChartPdfPage);
                    _lastRenderedChartPdfPage = PdfRenderEngine.GeneratePageBitmap(_currentChartFile.FullName, _currentChartPageNum,
                        new Size(150, 150));
                    _lastRenderedChartPageNum = _currentChartPageNum;
                    _lastRenderedChartFile = _currentChartFile;
                }
                if (_lastRenderedChartPdfPage != null)
                {
                    if (_nightMode)
                    {
                        using (Bitmap copy = (Bitmap)Common.Imaging.Util.CopyBitmap(_lastRenderedChartPdfPage))
                        using (Bitmap reverseVideo = (Bitmap)Common.Imaging.Util.GetDimmerImage(copy, 0.4f))
                        {
                            target.DrawImage(reverseVideo, targetRect, 0, 0, reverseVideo.Width, reverseVideo.Height, GraphicsUnit.Pixel);
                        }
                    }
                    else
                    {
                        target.DrawImage(_lastRenderedChartPdfPage, targetRect, 0, 0, _lastRenderedChartPdfPage.Width, _lastRenderedChartPdfPage.Height, GraphicsUnit.Pixel);
                    }
                }
            }
            target.InterpolationMode = origInterpolationMode;
            target.SmoothingMode = origSmoothingMode;
            target.CompositingQuality = origCompositQuality;
        }
        private void GetLatestMapImageFromServerAsync()
        {
            if (_mapFetchingBackgroundWorker != null)
            {
                if (!_mapFetchingBackgroundWorker.IsBusy)
                {
                    _mapFetchingBackgroundWorker.RunWorkerAsync();
                }
            }
        }

        private void mapFetchingBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            GetLatestMapImageFromServer();
        }
        private void GetLatestMapImageFromServer()
        {
            //get any pending map image from server
            byte[] mapBytes = (byte[])this.Client.GetSimProperty("CurrentMapImage"); //causes a method invoke on the server to occur
            Bitmap mapFromServer = null;
            if (mapBytes != null && mapBytes.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(mapBytes, 0, mapBytes.Length);
                    ms.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    mapFromServer = (Bitmap)Bitmap.FromStream(ms);
                }
            }
            lock (_mapImageLock)
            {
                Common.Util.DisposeObject(_lastMapFromServer);
                _lastMapFromServer = mapFromServer;
            }
        }
        private Bitmap RenderMapOnBehalfOfRemoteClient(Size renderSize, float mapScale, int rangeRingDiameterInNauticalMiles)
        {
            Bitmap rendered = new Bitmap(renderSize.Width, renderSize.Height, PixelFormat.Format16bppRgb565);
            using (Graphics g = Graphics.FromImage(rendered))
            {
                Rectangle renderRectangle = new Rectangle(new Point(0, 0), renderSize);
                RenderMapLocally(g, renderRectangle, mapScale, rangeRingDiameterInNauticalMiles);
            }
            return rendered;
        }
        private void RenderMapLocally(Graphics g, float mapScale, int rangeRingDiameterInNauticalMiles)
        {
            Rectangle renderRectangle = new Rectangle(0, 0, (this.ScreenBoundsPixels.Width), (this.ScreenBoundsPixels.Height));
            RenderMapLocally(g, renderRectangle, mapScale, rangeRingDiameterInNauticalMiles);
        }
        private void RenderMapLocally(Graphics g, Rectangle renderRectangle, float mapScale, int rangeRingDiameterInNauticalMiles)
        {
            if (Properties.Settings.Default.RunAsClient) return;
            Brush greenBrush = Brushes.Green;
            DateTime tadRenderStart = DateTime.Now;
            Rectangle tadRenderRectangle = renderRectangle;
            g.SetClip(tadRenderRectangle);
            this.SimSupportModule.RenderMap(g, tadRenderRectangle, mapScale,rangeRingDiameterInNauticalMiles, MapRotationMode.CurrentHeadingOnTop);

            float scaleX = ((float)tadRenderRectangle.Width)/ Constants.F_NATIVE_RES_WIDTH;
            float scaleY = ((float)tadRenderRectangle.Height)/ Constants.F_NATIVE_RES_HEIGHT;

            g.ScaleTransform(scaleX, scaleY);

            Rectangle latLongRect = new Rectangle(192, 734, (406 - 192), (768 - 734));
            g.FillRectangle(Brushes.Black, latLongRect);
            float latitudeDecDeg = this.FlightData.LatitudeInDecimalDegrees;
            float longitudeDecDeg = this.FlightData.LongitudeInDecimalDegrees;

            float latitudeWholeDeg = (int)(latitudeDecDeg);
            float longitudeWholeDeg = (int)(longitudeDecDeg);

            float latitudeMinutes = (latitudeDecDeg - latitudeWholeDeg) * 60.0f;
            float longitudeMinutes = (longitudeDecDeg - longitudeWholeDeg) * 60.0f;

            string latitudeQualifier = latitudeDecDeg >= 0.0f ? "N" : "S";
            string longitudeQualifier = longitudeDecDeg >= 0.0f ? "E" : "W";

            string latString = latitudeQualifier + latitudeWholeDeg.ToString() + " " + string.Format("{0:00.000}", latitudeMinutes);
            string longString = longitudeQualifier + longitudeWholeDeg.ToString() + " " + string.Format("{0:00.000}", longitudeMinutes);
            string latLongString = latString + "  " + longString;
            Font latLongFont = new Font(FontFamily.GenericMonospace, 10, System.Drawing.FontStyle.Bold);

            g.DrawString(latLongString, latLongFont, greenBrush, latLongRect);

            DateTime tadRenderEnd = DateTime.Now;
            TimeSpan tadRenderTime = tadRenderEnd.Subtract(tadRenderStart);
            //Debug.WriteLine("TAD render time:" + tadRenderTime.TotalMilliseconds);
        }
        private void InformClientOfCpdInputControlChangedEvent(CpdInputControls control)
        {
            if (!Properties.Settings.Default.RunAsServer) return;
            Networking.Message message = new Networking.Message("CpdInputControlChangedEvent", control);
            F16CPDServer.SubmitMessageToClient(message);
        }

        public void FireHandler(CpdInputControls control)
        {
            if (Properties.Settings.Default.RunAsServer)
            {
                InformClientOfCpdInputControlChangedEvent(control);
            }
            else
            {
                MfdInputControl inputControl = GetControl(control);
                if (inputControl != null)
                {
                    if (inputControl is MomentaryButtonMfdInputControl)
                    {
                        ((MomentaryButtonMfdInputControl)inputControl).Press(DateTime.Now);
                    }
                    else if (inputControl is ToggleSwitchMfdInputControl)
                    {
                        ((ToggleSwitchMfdInputControl)inputControl).Toggle();
                    }
                    else if (inputControl is ToggleSwitchPositionMfdInputControl)
                    {
                        ((ToggleSwitchPositionMfdInputControl)inputControl).Activate();
                    }
                    else if (inputControl is RotaryEncoderMfdInputControl)
                    {
                        ((RotaryEncoderMfdInputControl)inputControl).RotateClockwise();
                    }
                }
            }
        }

        public MfdInputControl GetControl(CpdInputControls control)
        {
            MfdInputControl toReturn = null;
            switch (control)
            {
                case CpdInputControls.Unknown:
                    break;
                case CpdInputControls.OsbButton1:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(1);
                    }
                    break;
                case CpdInputControls.OsbButton2:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(2);
                    }
                    break;
                case CpdInputControls.OsbButton3:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(3);
                    }
                    break;
                case CpdInputControls.OsbButton4:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(4);
                    }
                    break;
                case CpdInputControls.OsbButton5:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(5);
                    }
                    break;
                case CpdInputControls.OsbButton6:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(6);
                    }
                    break;
                case CpdInputControls.OsbButton7:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(7);
                    }
                    break;
                case CpdInputControls.OsbButton8:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(8);
                    }
                    break;
                case CpdInputControls.OsbButton9:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(9);
                    }
                    break;
                case CpdInputControls.OsbButton10:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(10);
                    }
                    break;
                case CpdInputControls.OsbButton11:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(11);
                    }
                    break;
                case CpdInputControls.OsbButton12:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(12);
                    }
                    break;
                case CpdInputControls.OsbButton13:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(13);
                    }
                    break;
                case CpdInputControls.OsbButton14:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(14);
                    }
                    break;
                case CpdInputControls.OsbButton15:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(15);
                    }
                    break;
                case CpdInputControls.OsbButton16:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(16);
                    }
                    break;
                case CpdInputControls.OsbButton17:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(17);
                    }
                    break;
                case CpdInputControls.OsbButton18:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(18);
                    }
                    break;
                case CpdInputControls.OsbButton19:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(19);
                    }
                    break;
                case CpdInputControls.OsbButton20:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(20);
                    }
                    break;
                case CpdInputControls.OsbButton21:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(21);
                    }
                    break;
                case CpdInputControls.OsbButton22:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(22);
                    }
                    break;
                case CpdInputControls.OsbButton23:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(23);
                    }
                    break;
                case CpdInputControls.OsbButton24:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(24);
                    }
                    break;
                case CpdInputControls.OsbButton25:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(25);
                    }
                    break;
                case CpdInputControls.OsbButton26:
                    if (this.ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(26);
                    }
                    break;
                case CpdInputControls.HsiModeControl:
                    toReturn = _hsiModeSelectorSwitch;
                    break;
                case CpdInputControls.HsiModeTcn:
                    toReturn = _hsiModeSelectorSwitch.GetPositionByName("TCN");
                    break;
                case CpdInputControls.HsiModeIlsTcn:
                    toReturn = _hsiModeSelectorSwitch.GetPositionByName("ILS/TCN");
                    break;
                case CpdInputControls.HsiModeNav:
                    toReturn = _hsiModeSelectorSwitch.GetPositionByName("NAV");
                    break;
                case CpdInputControls.HsiModeIlsNav:
                    toReturn = _hsiModeSelectorSwitch.GetPositionByName("ILS/NAV");
                    break;
                case CpdInputControls.ParameterAdjustKnob:
                    toReturn = _paramAdjustKnob;
                    break;
                case CpdInputControls.ParameterAdjustKnobIncrease:
                    toReturn = _paramAdjustKnob.ClockwiseMomentaryInputControl;
                    break;
                case CpdInputControls.ParameterAdjustKnobDecrease:
                    toReturn = _paramAdjustKnob.CounterclockwiseMomentaryInputControl;
                    break;
                case CpdInputControls.FuelSelectControl:
                    toReturn = _fuelSelectControl;
                    break;
                case CpdInputControls.FuelSelectTest:
                    toReturn = _fuelSelectControl.GetPositionByName("TEST");
                    break;
                case CpdInputControls.FuelSelectNorm:
                    toReturn = _fuelSelectControl.GetPositionByName("NORM");
                    break;
                case CpdInputControls.FuelSelectRsvr:
                    toReturn = _fuelSelectControl.GetPositionByName("RSVR");
                    break;
                case CpdInputControls.FuelSelectIntWing:
                    toReturn = _fuelSelectControl.GetPositionByName("INT WING");
                    break;
                case CpdInputControls.FuelSelectExtWing:
                    toReturn = _fuelSelectControl.GetPositionByName("EXT WING");
                    break;
                case CpdInputControls.FuelSelectExtCtr:
                    toReturn = _fuelSelectControl.GetPositionByName("EXT CTR");
                    break;
                case CpdInputControls.ExtFuelTransSwitch:
                    toReturn = _extFuelTransSwitch;
                    break;
                case CpdInputControls.ExtFuelSwitchTransNorm:
                    toReturn = _extFuelTransSwitch.GetPositionByName("NORM");
                    break;
                case CpdInputControls.ExtFuelSwitchTransWingFirst:
                    toReturn = _extFuelTransSwitch.GetPositionByName("WING FIRST");
                    break;
                default:
                    break;
            }
            return toReturn;
        }
        #region Destructors
        /// <summary>
        /// Standard finalizer, which will call Dispose() if this object is not
        /// manually disposed.  Ordinarily called only by the garbage collector.
        /// </summary>
        ~F16CpdMfdManager()
        {
            Dispose();
        }
        /// <summary>
        /// Private implementation of Dispose()
        /// </summary>
        /// <param name="disposing">flag to indicate if we should actually perform disposal.  Distinguishes the private method signature from the public signature.</param>
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    //dispose of managed resources here
                    Common.Util.DisposeObject(this.Pfd);
                    Common.Util.DisposeObject(this.Hsi);
                    Common.Util.DisposeObject(_client);
                    TeardownService();
                }
            }
            // Code to dispose the un-managed resources of the class
            _isDisposed = true;

        }
        /// <summary>
        /// Public implementation of IDisposable.Dispose().  Cleans up managed
        /// and unmanaged resources used by this object before allowing garbage collection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
