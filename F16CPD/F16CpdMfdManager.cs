using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using F16CPD.FlightInstruments;
using F16CPD.Mfd;
using F16CPD.Mfd.Controls;
using F16CPD.Mfd.Menus;
using F16CPD.Networking;
using F16CPD.Properties;
using F16CPD.SimSupport;
using Message = F16CPD.Networking.Message;

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
        private const int MAX_BRIGHTNESS = 255;
        private const int NUM_BRIGHTNESS_STEPS = 30;
        private readonly object _mapImageLock = new object();
        private int _airspeedIndexInKnots;
        private int _altitudeIndexInFeet;
        private int _brightness = 255;
        private F16CPDClient _client;
        private FileInfo _currentChartFile;
        private int _currentChartPageNum = 1;
        private int _currentChartPagesTotal;
        private FileInfo _currentChecklistFile;
        private int _currentChecklistPageNum = 1;
        private int _currentChecklistPagesTotal;
        private ToggleSwitchMfdInputControl _extFuelTransSwitch;
        private ToggleSwitchMfdInputControl _fuelSelectControl;
        private ToggleSwitchMfdInputControl _hsiModeSelectorSwitch;
        private bool _isDisposed;
        private Bitmap _lastMapFromServer;

        private FileInfo _lastRenderedChartFile;
        private int _lastRenderedChartPageNum = 1;
        private Bitmap _lastRenderedChartPdfPage;
        private FileInfo _lastRenderedChecklistFile;
        private int _lastRenderedChecklistPageNum = 1;
        private Bitmap _lastRenderedChecklistPdfPage;
        private BackgroundWorker _mapFetchingBackgroundWorker;
        private int _mapRangeRingsDiameterInNauticalMiles = 40;
        private float _mapScale = 500000.0f;
        private bool _nightMode;
        private RotaryEncoderMfdInputControl _paramAdjustKnob;
        private ISimSupportModule _simSupportModule;

        public F16CpdMfdManager(Size screenBoundsPixels) : base(screenBoundsPixels)
        {
            SetupNetworking();
            BuildMfdPages();
            BuildNonOsbInputControls();
            InitializeFlightInstruments();
            _brightness = Settings.Default.Brightness;
            SetupMapFetchingBackgroundWorker();
        }

        public int AltitudeIndexInFeet
        {
            get { return _altitudeIndexInFeet; }
            set
            {
                if (value < 0) value = 0;
                if (value > 99980) value = 99980;
                _altitudeIndexInFeet = value;
            }
        }

        public int AirspeedIndexInKnots
        {
            get { return _airspeedIndexInKnots; }
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
            get { return _client; }
        }

        public ISimSupportModule SimSupportModule
        {
            get { return _simSupportModule; }
            set { _simSupportModule = value; }
        }

        public Pfd Pfd { get; set; }
        public Hsi Hsi { get; set; }
        public FlightData FlightData { get; set; }

        public int Brightness
        {
            get { return _brightness; }
        }

        public int MaxBrightness
        {
            get { return MAX_BRIGHTNESS; }
        }

        public bool NightMode
        {
            get { return _nightMode; }
            set { _nightMode = value; }
        }

        private void SetNightMode(bool newValue)
        {
            _nightMode = newValue;
            if (FlightData != null) NightMode = newValue;
        }

        private void SetupMapFetchingBackgroundWorker()
        {
            if (Settings.Default.RunAsClient)
            {
                _mapFetchingBackgroundWorker = new BackgroundWorker();
                _mapFetchingBackgroundWorker.DoWork += mapFetchingBackgroundWorker_DoWork;
            }
        }

        private static void TeardownService()
        {
            if (Settings.Default.RunAsServer)
            {
                var portNumber = Settings.Default.ServerPortNum;
                var port = 21153;
                Int32.TryParse(portNumber, out port);
                F16CPDServer.TearDownService(port);
            }
        }

        private void SetupNetworking()
        {
            if (Settings.Default.RunAsServer)
            {
                var portNumber = Settings.Default.ServerPortNum;
                var port = 21153;
                Int32.TryParse(portNumber, out port);
                F16CPDServer.CreateService("F16CPDService", port);
            }
            else if (Settings.Default.RunAsClient)
            {
                var serverIPAddress = Settings.Default.ServerIPAddress;
                var portNumber = Settings.Default.ServerPortNum;
                var port = 21153;
                Int32.TryParse(portNumber, out port);
                var ipAddress = new IPAddress(new byte[] {127, 0, 0, 1});
                IPAddress.TryParse(serverIPAddress, out ipAddress);
                var endpoint = new IPEndPoint(ipAddress, port);
                _client = new F16CPDClient(endpoint, "F16CPDService");
                _client.ClearPendingClientMessages();
            }
        }

        private void ProcessPendingMessagesToServerFromClient()
        {
            if (!Settings.Default.RunAsServer) return;
            var pendingMessage = F16CPDServer.GetNextPendingServerMessage();
            if (pendingMessage != null)
            {
                var processed = false;
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
                            var payload = (Dictionary<string, object>) pendingMessage.Payload;
                            var renderSize = (Size) payload["RenderSize"];
                            var mapScale = (float) payload["MapScale"];
                            var mapRangeDiameter = (int) payload["RangeRingsDiameter"];
                            var renderedMap = RenderMapOnBehalfOfRemoteClient(renderSize, mapScale, mapRangeDiameter);
                            if (renderedMap != null)
                            {
                                using (var ms = new MemoryStream())
                                {
                                    renderedMap.Save(ms, ImageFormat.Png);
                                    ms.Flush();
                                    ms.Seek(0, SeekOrigin.Begin);
                                    var rawBytes = new byte[ms.Length];
                                    ms.Read(rawBytes, 0, (int) ms.Length);
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
            if (!Settings.Default.RunAsClient) return;
            var pendingMessage = _client.GetNextPendingClientMessage();
            if (pendingMessage != null)
            {
                var processed = false;
                if (_simSupportModule != null)
                {
                    processed = _simSupportModule.ProcessPendingMessageToClientFromServer(pendingMessage);
                }
                if (!processed)
                {
                    switch (pendingMessage.MessageType)
                    {
                        case "CpdInputControlChangedEvent":
                            var controlThatChanged = (CpdInputControls) pendingMessage.Payload;
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
            _hsiModeSelectorSwitch.PositionChanged += _hsiModeSelectorSwitch_PositionChanged;
            _hsiModeSelectorSwitch.AddPosition(@"ILS/TCN");
            _hsiModeSelectorSwitch.AddPosition("TCN");
            _hsiModeSelectorSwitch.AddPosition("NAV");
            _hsiModeSelectorSwitch.AddPosition(@"ILS/NAV");

            _fuelSelectControl = new ToggleSwitchMfdInputControl();
            _fuelSelectControl.PositionChanged += _fuelSelectControl_PositionChanged;
            _fuelSelectControl.AddPosition("TEST");
            _fuelSelectControl.AddPosition("NORM");
            _fuelSelectControl.AddPosition("RSVR");
            _fuelSelectControl.AddPosition("INT WING");
            _fuelSelectControl.AddPosition("EXT WING");
            _fuelSelectControl.AddPosition("EXT CTR");

            _extFuelTransSwitch = new ToggleSwitchMfdInputControl();
            _extFuelTransSwitch.PositionChanged += _extFuelTransSwitch_PositionChanged;
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
                    if (_simSupportModule != null)
                        _simSupportModule.HandleInputControlEvent(CpdInputControls.ExtFuelSwitchTransNorm, e.NewPosition);
                    break;
                case "WING FIRST":
                    if (_simSupportModule != null)
                        _simSupportModule.HandleInputControlEvent(CpdInputControls.ExtFuelSwitchTransWingFirst,
                                                                  e.NewPosition);
                    break;
            }
        }

        private void _fuelSelectControl_PositionChanged(object sender, ToggleSwitchPositionChangedEventArgs e)
        {
            if (e == null) return;
            switch (e.NewPosition.PositionName)
            {
                case "TEST":
                    if (_simSupportModule != null)
                        _simSupportModule.HandleInputControlEvent(CpdInputControls.FuelSelectTest, e.NewPosition);
                    break;
                case "NORM":
                    if (_simSupportModule != null)
                        _simSupportModule.HandleInputControlEvent(CpdInputControls.FuelSelectNorm, e.NewPosition);
                    break;
                case "RSVR":
                    if (_simSupportModule != null)
                        _simSupportModule.HandleInputControlEvent(CpdInputControls.FuelSelectRsvr, e.NewPosition);
                    break;
                case "INT WING":
                    if (_simSupportModule != null)
                        _simSupportModule.HandleInputControlEvent(CpdInputControls.FuelSelectIntWing, e.NewPosition);
                    break;
                case "EXT WING":
                    if (_simSupportModule != null)
                        _simSupportModule.HandleInputControlEvent(CpdInputControls.FuelSelectExtWing, e.NewPosition);
                    break;
                case "EXT CTR":
                    if (_simSupportModule != null)
                        _simSupportModule.HandleInputControlEvent(CpdInputControls.FuelSelectExtCtr, e.NewPosition);
                    break;
            }
        }

        private void _hsiModeSelectorSwitch_PositionChanged(object sender, ToggleSwitchPositionChangedEventArgs e)
        {
            if (e == null) return;
            switch (e.NewPosition.PositionName)
            {
                case "ILS/TCN":
                    if (_simSupportModule != null)
                        _simSupportModule.HandleInputControlEvent(CpdInputControls.HsiModeIlsTcn, e.NewPosition);
                    break;
                case "TCN":
                    if (_simSupportModule != null)
                        _simSupportModule.HandleInputControlEvent(CpdInputControls.HsiModeTcn, e.NewPosition);
                    break;
                case "NAV":
                    if (_simSupportModule != null)
                        _simSupportModule.HandleInputControlEvent(CpdInputControls.HsiModeNav, e.NewPosition);
                    break;
                case "ILS/NAV":
                    if (_simSupportModule != null)
                        _simSupportModule.HandleInputControlEvent(CpdInputControls.HsiModeIlsNav, e.NewPosition);
                    break;
            }
        }


        private void InitializeFlightInstruments()
        {
            Pfd = new Pfd();
            Hsi = new Hsi();
            FlightData = new FlightData();
        }

        private void BuildMfdPages()
        {
            var primaryPage = BuildPrimaryMenuPage();
            var instrumentsDisplayPage = BuildInstrumentsDisplayMenuPage();
            var testPage = BuildTestPage();
            var tgpPage = BuildTargetingPodMenuPage();
            var messagePage = BuildMessageMenuPage();
            var tadPage = BuildTADMenuPage();
            var checklistsPage = BuildChecklistMenuPage();
            var chartsPage = BuildChartsMenuPage();
            var controlMapPage = BuildControlMapMenuPage();
            var controlOverlayPage = BuildControlOverlayMenuPage();
            var bitmapAnnotationPage = BuildBitmapAnnotationMenuPage();
            MenuPages = new[]
                            {
                                primaryPage, instrumentsDisplayPage, testPage, tgpPage, messagePage, tadPage,
                                controlMapPage, controlOverlayPage, bitmapAnnotationPage, checklistsPage, chartsPage
                            };
            foreach (var thisPage in MenuPages)
            {
                var nightModeButton = CreateOptionSelectButton(thisPage, 6, "NGT", false);
                nightModeButton.FunctionName = "NightMode";
                nightModeButton.Pressed += nightModeButton_Pressed;
                nightModeButton.LabelLocation = new Point(-10000, -10000);
                nightModeButton.LabelSize = new Size(0, 0);
                thisPage.OptionSelectButtons.Add(nightModeButton);

                var dayModeButton = CreateOptionSelectButton(thisPage, 26, "DAY", false);
                dayModeButton.FunctionName = "DayMode";
                dayModeButton.Pressed += dayModeButton_Pressed;
                dayModeButton.LabelSize = new Size(0, 0);
                dayModeButton.LabelLocation = new Point(-10000, -10000);
                thisPage.OptionSelectButtons.Add(dayModeButton);

                var brightnessIncreaseButton = CreateOptionSelectButton(thisPage, 13, "BRT", false);
                brightnessIncreaseButton.FunctionName = "IncreaseBrightness";
                brightnessIncreaseButton.Pressed += brightnessIncreaseButton_Pressed;
                brightnessIncreaseButton.LabelSize = new Size(0, 0);
                brightnessIncreaseButton.LabelLocation = new Point(-10000, -10000);
                thisPage.OptionSelectButtons.Add(brightnessIncreaseButton);

                var brightnessDecreaseButton = CreateOptionSelectButton(thisPage, 19, "DIM", false);
                brightnessDecreaseButton.FunctionName = "DecreaseBrightness";
                brightnessDecreaseButton.Pressed += brightnessDecreaseButton_Pressed;
                brightnessDecreaseButton.LabelSize = new Size(0, 0);
                brightnessDecreaseButton.LabelLocation = new Point(-10000, -10000);
                thisPage.OptionSelectButtons.Add(brightnessDecreaseButton);
            }
            ActiveMenuPage = instrumentsDisplayPage;
        }

        private void brightnessDecreaseButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            unchecked
            {
                const int brightnessStep = (int) (MAX_BRIGHTNESS/(float) NUM_BRIGHTNESS_STEPS);
                _brightness -= brightnessStep;
                if (_brightness < 0) _brightness = 0;
                Settings.Default.Brightness = _brightness;
                Util.SaveCurrentProperties();
            }
        }

        private void brightnessIncreaseButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            unchecked
            {
                const int brightnessStep = (int) (MAX_BRIGHTNESS/(float) NUM_BRIGHTNESS_STEPS);
                _brightness += brightnessStep;
                if (_brightness > MAX_BRIGHTNESS) _brightness = MAX_BRIGHTNESS;
                Settings.Default.Brightness = _brightness;
                Util.SaveCurrentProperties();
            }
        }

        private void dayModeButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            SetNightMode(false);
        }

        private MfdMenuPage BuildTestPage()
        {
            var thisPage = new MfdMenuPage(this);
            var buttons = new List<OptionSelectButton>
                              {
                                  CreateOptionSelectButton(thisPage, 3, "2PRV", false),
                                  CreateOptionSelectButton(thisPage, 4, "PRV", false)
                              };

            var testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", true);
            testPageSelectButton.Pressed += testPageSelectButton_Press;
            buttons.Add(testPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 8, "CLR", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 9, "MOUSE", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 10, "OFP ID", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 11, "CAL MSS", false));
            var imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += imagingPageSelectButton_Press;
            buttons.Add(imagingPageSelectButton);

            var messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += messagingPageSelectButton_Press;
            buttons.Add(messagingPageSelectButton);

            var tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD",
                                                                                    false);
            tacticalAwarenessDisplayPageSelectButton.Pressed += tacticalAwarenessDisplayPageSelectButton_Press;
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            var targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += targetingPodPageSelectButton_Press;
            buttons.Add(targetingPodPageSelectButton);

            var headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += headDownDisplayPageSelectButton_Press;
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
            var thisPage = new MfdMenuPage(this);
            var buttons = new List<OptionSelectButton>();

            var testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed += testPageSelectButton_Press;
            buttons.Add(testPageSelectButton);

            var imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += imagingPageSelectButton_Press;
            buttons.Add(imagingPageSelectButton);

            var messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += messagingPageSelectButton_Press;
            buttons.Add(messagingPageSelectButton);

            var tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD",
                                                                                    false);
            tacticalAwarenessDisplayPageSelectButton.Pressed += tacticalAwarenessDisplayPageSelectButton_Press;
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            var targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += targetingPodPageSelectButton_Press;
            buttons.Add(targetingPodPageSelectButton);

            var headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += headDownDisplayPageSelectButton_Press;
            buttons.Add(headDownDisplayPageSelectButton);

            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Primary Page";
            return thisPage;
        }


        private MfdMenuPage BuildInstrumentsDisplayMenuPage()
        {
            var thisPage = new MfdMenuPage(this);
            var buttons = new List<OptionSelectButton>();
            const int triangleLegLengthPixels = 25;
            var pneuButton = CreateOptionSelectButton(thisPage, 3, "PNEU", false);
            pneuButton.FunctionName = "ToggleAltimeterModeElecPneu";
            pneuButton.Pressed += pneuButton_Press;
            var ackButton = CreateOptionSelectButton(thisPage, 4, "ACK", false);
            ackButton.FunctionName = "AcknowledgeMessage";
            ackButton.Pressed += ackButton_Pressed;
            var testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.FunctionName = "TestHdd";
            testPageSelectButton.Pressed += testPageSelectButton_Press;
            var altitudeIndexUpButton = CreateOptionSelectButton(thisPage, 7, "^", false,
                                                                 triangleLegLengthPixels);
            altitudeIndexUpButton.FunctionName = "AltitudeIndexIncrease";
            altitudeIndexUpButton.Pressed += altitudeIndexUpButton_Press;
            var altitudeIndexLabel = CreateOptionSelectButton(thisPage, 7.5f, "ALT", false);
            var altitudeIndexDownButton = CreateOptionSelectButton(thisPage, 8, @"\/", false,
                                                                   triangleLegLengthPixels);
            altitudeIndexDownButton.FunctionName = "AltitudeIndexDecrease";
            altitudeIndexDownButton.Pressed += altitudeIndexDownButton_Press;
            var barometricPressureUpButton = CreateOptionSelectButton(thisPage, 9, "^", false,
                                                                      triangleLegLengthPixels);
            barometricPressureUpButton.FunctionName = "BarometricPressureSettingIncrease";
            barometricPressureUpButton.Pressed += barometricPressureUpButton_Press;
            var barometricPressureLabel = CreateOptionSelectButton(thisPage, 9.5f, "BARO", false);
            var barometricPressureDownButton = CreateOptionSelectButton(thisPage, 10, @"\/", false,
                                                                        triangleLegLengthPixels);
            barometricPressureDownButton.FunctionName = "BarometricPressureSettingDecrease";
            barometricPressureDownButton.Pressed += barometricPressureDownButton_Press;
            var courseSelectUpButton = CreateOptionSelectButton(thisPage, 11, "^", false,
                                                                triangleLegLengthPixels);
            courseSelectUpButton.FunctionName = "CourseSelectIncrease";
            courseSelectUpButton.Pressed += courseSelectUpButton_Press;
            var courseSelectLabel = CreateOptionSelectButton(thisPage, 11.5f, "CRS", false);
            var courseSelectDownButton = CreateOptionSelectButton(thisPage, 12, @"\/", false,
                                                                  triangleLegLengthPixels);
            courseSelectDownButton.FunctionName = "CourseSelectDecrease";
            courseSelectDownButton.Pressed += courseSelectDownButton_Press;
            var imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += imagingPageSelectButton_Press;
            var messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += messagingPageSelectButton_Press;
            var tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD",
                                                                                    false);
            tacticalAwarenessDisplayPageSelectButton.Pressed += tacticalAwarenessDisplayPageSelectButton_Press;
            var targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += targetingPodPageSelectButton_Press;
            var headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", true);
            headDownDisplayPageSelectButton.Pressed += headDownDisplayPageSelectButton_Press;
            var headingSelectDownButton = CreateOptionSelectButton(thisPage, 20, @"\/", false,
                                                                   triangleLegLengthPixels);
            headingSelectDownButton.FunctionName = "HeadingSelectDecrease";
            headingSelectDownButton.Pressed += headingSelectDownButton_Press;
            var headingSelectLabel = CreateOptionSelectButton(thisPage, 20.5f, "HDG", false);
            var headingSelectUpButton = CreateOptionSelectButton(thisPage, 21, "^", false,
                                                                 triangleLegLengthPixels);
            headingSelectUpButton.FunctionName = "HeadingSelectIncrease";
            headingSelectUpButton.Pressed += headingSelectUpButton_Press;
            var lowAltitudeThresholdSelectDownButton = CreateOptionSelectButton(thisPage, 22, @"\/",
                                                                                false,
                                                                                triangleLegLengthPixels);
            lowAltitudeThresholdSelectDownButton.FunctionName = "LowAltitudeWarningThresholdDecrease";
            lowAltitudeThresholdSelectDownButton.Pressed += lowAltitudeThresholdSelectDownButton_Press;
            var lowAltitudeThresholdLabel = CreateOptionSelectButton(thisPage, 22.5f, "ALOW", false);
            var lowAltitudeThresholdSelectUpButton = CreateOptionSelectButton(thisPage, 23, "^", false,
                                                                              triangleLegLengthPixels);
            lowAltitudeThresholdSelectUpButton.Pressed += lowAltitudeThresholdSelectUpButton_Press;
            lowAltitudeThresholdSelectUpButton.FunctionName = "LowAltitudeWarningThresholdIncrease";
            var airspeedIndexSelectDownButton = CreateOptionSelectButton(thisPage, 24, @"\/", false,
                                                                         triangleLegLengthPixels);
            airspeedIndexSelectDownButton.FunctionName = "AirspeedIndexDecrease";
            airspeedIndexSelectDownButton.Pressed += airspeedIndexSelectDownButton_Press;
            var airspeedIndexLabel = CreateOptionSelectButton(thisPage, 24.5f, "ASPD", false);
            var airspeedIndexSelectUpButton = CreateOptionSelectButton(thisPage, 25, @"^", false,
                                                                       triangleLegLengthPixels);
            airspeedIndexSelectUpButton.FunctionName = "AirspeedIndexIncrease";
            airspeedIndexSelectUpButton.Pressed += airspeedIndexSelectUpButton_Press;

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
            if (_simSupportModule != null)
                _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton4, (OptionSelectButton) sender);
        }

        private void testPageSelectButton_Press(object sender, EventArgs e)
        {
            var newPage = FindMenuPageByName("Test Page");
            ActiveMenuPage = newPage;
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
            var button = (OptionSelectButton) sender;
            if (_simSupportModule != null)
                _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton23, button);
        }

        private void lowAltitudeThresholdSelectDownButton_Press(object sender, EventArgs e)
        {
            var button = (OptionSelectButton) sender;
            if (_simSupportModule != null)
                _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton22, button);
        }

        private void headingSelectUpButton_Press(object sender, MomentaryButtonPressedEventArgs e)
        {
            var whenPressed = e.WhenPressed;
            var howLongPressed = (int) DateTime.Now.Subtract(whenPressed).TotalMilliseconds;
            var numTimes = 1;
            if (howLongPressed > 300) numTimes = Settings.Default.FastCourseAndHeadingAdjustSpeed;

            for (var i = 0; i < numTimes; i++)
            {
                FlightData.HsiDesiredHeadingInDegrees += 1;
                if (_simSupportModule != null)
                    _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton21, (OptionSelectButton) sender);
            }
        }

        private void headingSelectDownButton_Press(object sender, MomentaryButtonPressedEventArgs e)
        {
            var whenPressed = e.WhenPressed;
            var howLongPressed = (int) DateTime.Now.Subtract(whenPressed).TotalMilliseconds;
            var numTimes = 1;
            if (howLongPressed > 300) numTimes = Settings.Default.FastCourseAndHeadingAdjustSpeed;

            for (var i = 0; i < numTimes; i++)
            {
                FlightData.HsiDesiredHeadingInDegrees -= 1;
                if (_simSupportModule != null)
                    _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton20, (OptionSelectButton) sender);
            }
        }

        private void headDownDisplayPageSelectButton_Press(object sender, EventArgs e)
        {
            var newPage = FindMenuPageByName("Instruments Display Page");
            ActiveMenuPage = newPage;
        }

        private void targetingPodPageSelectButton_Press(object sender, EventArgs e)
        {
            var newPage = FindMenuPageByName("Targeting Pod Page");
            ActiveMenuPage = newPage;
        }

        private void tacticalAwarenessDisplayPageSelectButton_Press(object sender, EventArgs e)
        {
            var newPage = FindMenuPageByName("TAD Page");
            ActiveMenuPage = newPage;
        }

        private void messagingPageSelectButton_Press(object sender, EventArgs e)
        {
            var newPage = FindMenuPageByName("Message Page");
            ActiveMenuPage = newPage;
        }

        private void imagingPageSelectButton_Press(object sender, EventArgs e)
        {
            var newPage = FindMenuPageByName("Bitmap Annotation Page");
            ActiveMenuPage = newPage;
        }

        private void courseSelectDownButton_Press(object sender, MomentaryButtonPressedEventArgs e)
        {
            var whenPressed = e.WhenPressed;
            var howLongPressed = (int) DateTime.Now.Subtract(whenPressed).TotalMilliseconds;
            var numTimes = 1;
            if (howLongPressed > 300) numTimes = Settings.Default.FastCourseAndHeadingAdjustSpeed;

            for (var i = 0; i < numTimes; i++)
            {
                FlightData.HsiDesiredCourseInDegrees -= 1;
                if (_simSupportModule != null)
                    _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton12, (OptionSelectButton) sender);
            }
        }

        private void courseSelectUpButton_Press(object sender, MomentaryButtonPressedEventArgs e)
        {
            var whenPressed = e.WhenPressed;
            var howLongPressed = (int) DateTime.Now.Subtract(whenPressed).TotalMilliseconds;
            var numTimes = 1;
            if (howLongPressed > 300) numTimes = Settings.Default.FastCourseAndHeadingAdjustSpeed;

            for (var i = 0; i < numTimes; i++)
            {
                FlightData.HsiDesiredCourseInDegrees += 1;
                if (_simSupportModule != null)
                    _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton11, (OptionSelectButton) sender);
            }
        }

        private void barometricPressureDownButton_Press(object sender, EventArgs e)
        {
            FlightData.BarometricPressureInDecimalInchesOfMercury -= 0.01f;
            if (_simSupportModule != null)
                _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton10, (OptionSelectButton) sender);
        }

        private void barometricPressureUpButton_Press(object sender, EventArgs e)
        {
            FlightData.BarometricPressureInDecimalInchesOfMercury += 0.01f;
            if (_simSupportModule != null)
                _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton9, (OptionSelectButton) sender);
        }

        private void altitudeIndexDownButton_Press(object sender, MomentaryButtonPressedEventArgs e)
        {
            var whenPressed = e.WhenPressed;
            var howLongPressed = (int) DateTime.Now.Subtract(whenPressed).TotalMilliseconds;
            var secondsPressed = (howLongPressed/1000.0f);
            var valueDelta = 20;

            if (howLongPressed >= 200) valueDelta = 100;
            if (secondsPressed >= 1) valueDelta = 500;
            if (secondsPressed >= 2) valueDelta = 1000;

            var diff = valueDelta - ((((AltitudeIndexInFeet/valueDelta))*valueDelta) - AltitudeIndexInFeet);
            AltitudeIndexInFeet -= diff;
        }

        private void altitudeIndexUpButton_Press(object sender, MomentaryButtonPressedEventArgs e)
        {
            var whenPressed = e.WhenPressed;
            var howLongPressed = (int) DateTime.Now.Subtract(whenPressed).TotalMilliseconds;
            var secondsPressed = (howLongPressed/1000.0f);
            var valueDelta = 20;

            if (howLongPressed >= 200) valueDelta = 100;
            if (secondsPressed >= 1) valueDelta = 500;
            if (secondsPressed >= 2) valueDelta = 1000;

            var diff = valueDelta + ((((AltitudeIndexInFeet/valueDelta))*valueDelta) - AltitudeIndexInFeet);
            AltitudeIndexInFeet += diff;
        }

        private void pneuButton_Press(object sender, EventArgs e)
        {
            var button = (OptionSelectButton) sender;
            if (FlightData.AltimeterMode == AltimeterMode.Electronic)
            {
                FlightData.AltimeterMode = AltimeterMode.Pneumatic;
                button.LabelText = "PNEU";
            }
            else
            {
                button.LabelText = "ELEC";
                FlightData.AltimeterMode = AltimeterMode.Electronic;
            }
            if (_simSupportModule != null)
                _simSupportModule.HandleInputControlEvent(CpdInputControls.OsbButton3, (OptionSelectButton) sender);
        }

        private MfdMenuPage FindMenuPageByName(string name)
        {
            return
                MenuPages.FirstOrDefault(page => name.ToLowerInvariant().Trim() == page.Name.ToLowerInvariant().Trim());
        }

        private MfdMenuPage BuildTargetingPodMenuPage()
        {
            var thisPage = new MfdMenuPage(this);
            var buttons = new List<OptionSelectButton>();

            var testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed += testPageSelectButton_Press;
            buttons.Add(testPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 10, "CLR MSG", false));

            var imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += imagingPageSelectButton_Press;
            buttons.Add(imagingPageSelectButton);

            var messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += messagingPageSelectButton_Press;
            buttons.Add(messagingPageSelectButton);

            var tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD",
                                                                                    false);
            tacticalAwarenessDisplayPageSelectButton.Pressed += tacticalAwarenessDisplayPageSelectButton_Press;
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            var targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", true);
            targetingPodPageSelectButton.Pressed += targetingPodPageSelectButton_Press;
            buttons.Add(targetingPodPageSelectButton);

            var headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += headDownDisplayPageSelectButton_Press;
            buttons.Add(headDownDisplayPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 22, "CAP", false));
            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Targeting Pod Page";
            return thisPage;
        }

        private MfdMenuPage BuildMessageMenuPage()
        {
            var thisPage = new MfdMenuPage(this);
            var buttons = new List<OptionSelectButton> {CreateOptionSelectButton(thisPage, 2, "TO USB", false)};

            var testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed += testPageSelectButton_Press;
            buttons.Add(testPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 6, "SADL", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 11, "DEL", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 12, "DEL ALL", false));

            var imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += imagingPageSelectButton_Press;
            buttons.Add(imagingPageSelectButton);

            var messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", true);
            messagingPageSelectButton.Pressed += messagingPageSelectButton_Press;
            buttons.Add(messagingPageSelectButton);

            var tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD",
                                                                                    false);
            tacticalAwarenessDisplayPageSelectButton.Pressed += tacticalAwarenessDisplayPageSelectButton_Press;
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            var targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += targetingPodPageSelectButton_Press;
            buttons.Add(targetingPodPageSelectButton);

            var headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += headDownDisplayPageSelectButton_Press;
            buttons.Add(headDownDisplayPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 24, @"\/", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 24.5f, "NO MSG\n\r", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 25, @"^", false));
            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Message Page";
            return thisPage;
        }

        private static string GetCADRGScaleTextForMapScale(float mapScale)
        {
            var toReturn = "1:";

            var millions = (int) Math.Round(mapScale/(1000.0f*1000.0f), 0);
            var thousands = (int) Math.Round(mapScale/1000.0f, 0);
            var hundreds = (int) Math.Round(mapScale/100.0f, 0);
            var ones = (int) Math.Round(mapScale, 0);
            if (millions > 0)
            {
                toReturn += millions + " M";
            }
            else if (thousands > 0)
            {
                toReturn += thousands + " K";
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
            var thisPage = new MfdMenuPage(this);
            var buttons = new List<OptionSelectButton>();

            var controlMapPageSelectButton = CreateOptionSelectButton(thisPage, 1, "CNTL", false);
            controlMapPageSelectButton.Pressed += controlMapPageSelectButton_Press;
            buttons.Add(controlMapPageSelectButton);

            var chartPageSelectButton = CreateOptionSelectButton(thisPage, 2, "CHARTS", false);
            chartPageSelectButton.Pressed += chartPageSelectButton_Pressed;
            buttons.Add(chartPageSelectButton);

            var checklistPageSelectButton = CreateOptionSelectButton(thisPage, 3, "CHKLST", true);
            checklistPageSelectButton.Pressed += checklistPageSelectButton_Pressed;
            buttons.Add(checklistPageSelectButton);

            var mapPageSelectButton = CreateOptionSelectButton(thisPage, 4, "MAP", false);
            mapPageSelectButton.Pressed += mapPageSelectButton_Pressed;
            buttons.Add(mapPageSelectButton);

            var testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed += testPageSelectButton_Press;
            buttons.Add(testPageSelectButton);

            var previousChecklistFileSelectButton = CreateOptionSelectButton(thisPage, 7, "^", false);
            previousChecklistFileSelectButton.Pressed += previousChecklistFileSelectButton_Pressed;
            buttons.Add(previousChecklistFileSelectButton);

            var currentChecklistFileLabel = CreateOptionSelectButton(thisPage, 8, "NO CHKLST\nFILES",
                                                                     false);
            currentChecklistFileLabel.FunctionName = "CurrentChecklistFileLabel";
            buttons.Add(currentChecklistFileLabel);

            var nextChecklistFileSelectButton = CreateOptionSelectButton(thisPage, 9, @"\/", false);
            nextChecklistFileSelectButton.Pressed += nextChecklistFileSelectButton_Pressed;
            buttons.Add(nextChecklistFileSelectButton);

            var imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += imagingPageSelectButton_Press;
            buttons.Add(imagingPageSelectButton);

            var messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += messagingPageSelectButton_Press;
            buttons.Add(messagingPageSelectButton);

            var tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD",
                                                                                    true);
            tacticalAwarenessDisplayPageSelectButton.Pressed += tacticalAwarenessDisplayPageSelectButton_Press;
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            var targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += targetingPodPageSelectButton_Press;
            buttons.Add(targetingPodPageSelectButton);

            var headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += headDownDisplayPageSelectButton_Press;
            buttons.Add(headDownDisplayPageSelectButton);


            var nextChecklistPageSelectButton = CreateOptionSelectButton(thisPage, 23, @"\/", false);
            nextChecklistPageSelectButton.Pressed += nextChecklistPageSelectButton_Pressed;
            buttons.Add(nextChecklistPageSelectButton);

            var currentChecklistPageNumLabel = CreateOptionSelectButton(thisPage, 24, "page", false);
            currentChecklistPageNumLabel.FunctionName = "CurrentChecklistPageNumLabel";
            buttons.Add(currentChecklistPageNumLabel);

            var prevChecklistPageSelectButton = CreateOptionSelectButton(thisPage, 25, @"^", false);
            prevChecklistPageSelectButton.Pressed += prevChecklistPageSelectButton_Pressed;
            buttons.Add(prevChecklistPageSelectButton);

            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Checklists Page";
            return thisPage;
        }

        private MfdMenuPage BuildChartsMenuPage()
        {
            var thisPage = new MfdMenuPage(this);
            var buttons = new List<OptionSelectButton>();

            var controlMapPageSelectButton = CreateOptionSelectButton(thisPage, 1, "CNTL", false);
            controlMapPageSelectButton.Pressed += controlMapPageSelectButton_Press;
            buttons.Add(controlMapPageSelectButton);

            var chartPageSelectButton = CreateOptionSelectButton(thisPage, 2, "CHARTS", true);
            chartPageSelectButton.Pressed += chartPageSelectButton_Pressed;
            buttons.Add(chartPageSelectButton);

            var checklistPageSelectButton = CreateOptionSelectButton(thisPage, 3, "CHKLST", false);
            checklistPageSelectButton.Pressed += checklistPageSelectButton_Pressed;
            buttons.Add(checklistPageSelectButton);

            var mapPageSelectButton = CreateOptionSelectButton(thisPage, 4, "MAP", false);
            mapPageSelectButton.Pressed += mapPageSelectButton_Pressed;
            buttons.Add(mapPageSelectButton);

            var testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed += testPageSelectButton_Press;
            buttons.Add(testPageSelectButton);

            var previousChartFileSelectButton = CreateOptionSelectButton(thisPage, 7, "^", false);
            previousChartFileSelectButton.Pressed += previousChartFileSelectButton_Pressed;
            buttons.Add(previousChartFileSelectButton);

            var currentChartFileLabel = CreateOptionSelectButton(thisPage, 8, "NO CHART\nFILES", false);
            currentChartFileLabel.FunctionName = "CurrentChartFileLabel";
            buttons.Add(currentChartFileLabel);

            var nextChartFileSelectButton = CreateOptionSelectButton(thisPage, 9, @"\/", false);
            nextChartFileSelectButton.Pressed += nextChartFileSelectButton_Pressed;
            buttons.Add(nextChartFileSelectButton);

            var imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += imagingPageSelectButton_Press;
            buttons.Add(imagingPageSelectButton);

            var messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += messagingPageSelectButton_Press;
            buttons.Add(messagingPageSelectButton);

            var tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD",
                                                                                    true);
            tacticalAwarenessDisplayPageSelectButton.Pressed += tacticalAwarenessDisplayPageSelectButton_Press;
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            var targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += targetingPodPageSelectButton_Press;
            buttons.Add(targetingPodPageSelectButton);

            var headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += headDownDisplayPageSelectButton_Press;
            buttons.Add(headDownDisplayPageSelectButton);


            var nextChartPageSelectButton = CreateOptionSelectButton(thisPage, 23, @"\/", false);
            nextChartPageSelectButton.Pressed += nextChartPageSelectButton_Pressed;
            buttons.Add(nextChartPageSelectButton);

            var currentChartPageNumLabel = CreateOptionSelectButton(thisPage, 24, "page", false);
            currentChartPageNumLabel.FunctionName = "CurrentChartPageNumLabel";
            buttons.Add(currentChartPageNumLabel);

            var prevChartPageSelectButton = CreateOptionSelectButton(thisPage, 25, @"^", false);
            prevChartPageSelectButton.Pressed += prevChartPageSelectButton_Pressed;
            buttons.Add(prevChartPageSelectButton);

            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Charts Page";
            return thisPage;
        }

        private void checklistPageSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            SwitchToChecklistsPage();
        }

        private void chartPageSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            SwitchToChartsPage();
        }

        private void mapPageSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            SwitchToMapPage();
        }

        private void prevChartPageSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            if (_currentChartPageNum > 1) _currentChartPageNum--;
        }

        private void nextChartPageSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            if (_currentChartPageNum != _currentChartPagesTotal && _currentChartPagesTotal > 0)
            {
                _currentChartPageNum++;
            }
        }

        private void nextChartFileSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            NextChartFile();
        }

        private void previousChartFileSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            PrevChartFile();
        }


        private void prevChecklistPageSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            if (_currentChecklistPageNum > 1) _currentChecklistPageNum--;
        }

        private void nextChecklistPageSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            if (_currentChecklistPageNum != _currentChecklistPagesTotal && _currentChecklistPagesTotal > 0)
            {
                _currentChecklistPageNum++;
            }
        }

        private void nextChecklistFileSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            NextChecklistFile();
        }

        private void previousChecklistFileSelectButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            PrevChecklistFile();
        }

        private MfdMenuPage BuildTADMenuPage()
        {
            var thisPage = new MfdMenuPage(this);
            var buttons = new List<OptionSelectButton>();

            var controlMapPageSelectButton = CreateOptionSelectButton(thisPage, 1, "CNTL", false);
            controlMapPageSelectButton.Pressed += controlMapPageSelectButton_Press;
            buttons.Add(controlMapPageSelectButton);

            var chartPageSelectButton = CreateOptionSelectButton(thisPage, 2, "CHARTS", false);
            chartPageSelectButton.Pressed += chartPageSelectButton_Pressed;
            buttons.Add(chartPageSelectButton);

            var checklistPageSelectButton = CreateOptionSelectButton(thisPage, 3, "CHKLST", false);
            checklistPageSelectButton.Pressed += checklistPageSelectButton_Pressed;
            buttons.Add(checklistPageSelectButton);

            var mapOnOffButton = CreateOptionSelectButton(thisPage, 4, "MAP", true);
            mapOnOffButton.Pressed += mapOnOffButton_Pressed;
            buttons.Add(mapOnOffButton);

            var testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed += testPageSelectButton_Press;
            buttons.Add(testPageSelectButton);

            var scaleIncreaseButton = CreateOptionSelectButton(thisPage, 7, "^", false);
            scaleIncreaseButton.Pressed += scaleIncreaseButton_Pressed;
            buttons.Add(scaleIncreaseButton);

            var scaleLabel = CreateOptionSelectButton(thisPage, 7.5f,
                                                      "CADRG\r\n" +
                                                      GetCADRGScaleTextForMapScale(_mapScale), false);
            scaleLabel.FunctionName = "MapScaleLabel";
            buttons.Add(scaleLabel);

            var scaleDecreaseButton = CreateOptionSelectButton(thisPage, 8, @"\/", false);
            scaleDecreaseButton.Pressed += scaleDecreaseButton_Pressed;
            buttons.Add(scaleDecreaseButton);
            buttons.Add(CreateOptionSelectButton(thisPage, 9, "CNTR\r\nOWN", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 10, "CLR MSG", false));

            var imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += imagingPageSelectButton_Press;
            buttons.Add(imagingPageSelectButton);

            var messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += messagingPageSelectButton_Press;
            buttons.Add(messagingPageSelectButton);

            var tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD",
                                                                                    true);
            tacticalAwarenessDisplayPageSelectButton.Pressed += tacticalAwarenessDisplayPageSelectButton_Press;
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            var targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += targetingPodPageSelectButton_Press;
            buttons.Add(targetingPodPageSelectButton);

            var headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += headDownDisplayPageSelectButton_Press;
            buttons.Add(headDownDisplayPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 22, "CAP", false));

            var mapRangeIncrease = CreateOptionSelectButton(thisPage, 25, @"^", false);
            mapRangeIncrease.Pressed += mapRangeIncrease_Pressed;
            buttons.Add(mapRangeIncrease);

            var mapRangeDecrease = CreateOptionSelectButton(thisPage, 24, @"\/", false);
            mapRangeDecrease.Pressed += mapRangeDecrease_Pressed;
            buttons.Add(mapRangeDecrease);

            var mapRangeLabel = CreateOptionSelectButton(thisPage, 24.5f,
                                                         _mapRangeRingsDiameterInNauticalMiles.ToString(),
                                                         false);
            mapRangeLabel.FunctionName = "MapRangeLabel";
            buttons.Add(mapRangeLabel);

            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "TAD Page";
            return thisPage;
        }

        private void mapOnOffButton_Pressed(object sender, MomentaryButtonPressedEventArgs e)
        {
            SwitchToMapPage();
        }

        private void SwitchToMapPage()
        {
            var newPage = FindMenuPageByName("TAD Page");
            if (newPage != null)
            {
                ActiveMenuPage = newPage;
            }
        }

        private void SwitchToChecklistsPage()
        {
            var newPage = FindMenuPageByName("Checklists Page");
            if (newPage != null)
            {
                ActiveMenuPage = newPage;
            }
        }

        private void SwitchToChartsPage()
        {
            var newPage = FindMenuPageByName("Charts Page");
            if (newPage != null)
            {
                ActiveMenuPage = newPage;
            }
        }

        private void UpdateCurrentChecklistPageCount()
        {
            if (_currentChecklistFile != null)
            {
                var numPages = PdfRenderEngine.NumPagesInPdf(_currentChecklistFile.FullName);
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
            var files = GetChecklistsFiles();
            _currentChecklistFile = GetNextFile(_currentChecklistFile, files);
            UpdateCurrentChecklistPageCount();
        }

        private void PrevChecklistFile()
        {
            var files = GetChecklistsFiles();
            _currentChecklistFile = GetPrevFile(_currentChecklistFile, files);
            UpdateCurrentChecklistPageCount();
        }


        private void UpdateCurrentChartPageCount()
        {
            if (_currentChartFile != null)
            {
                var numPages = PdfRenderEngine.NumPagesInPdf(_currentChartFile.FullName);
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
            var files = GetChartFiles();
            _currentChartFile = GetNextFile(_currentChartFile, files);
            UpdateCurrentChartPageCount();
        }

        private void PrevChartFile()
        {
            var files = GetChartFiles();
            _currentChartFile = GetPrevFile(_currentChartFile, files);
            UpdateCurrentChartPageCount();
        }


        private static FileInfo GetPrevFile(FileInfo currentFile, FileInfo[] files)
        {
            if (files == null || files.Length == 0) return null;
            if (currentFile == null) return files[0];
            for (var i = 0; i < files.Length; i++)
            {
                if (files[i].FullName == currentFile.FullName)
                {
                    if (i > 0)
                    {
                        return files[i - 1];
                    }
                    return files[files.Length - 1];
                }
            }
            return files[files.Length - 1];
        }

        private static FileInfo GetNextFile(FileInfo currentFile, FileInfo[] files)
        {
            if (files == null || files.Length == 0) return null;
            if (currentFile == null) return files[0];
            for (var i = 0; i < files.Length; i++)
            {
                if (files[i].FullName == currentFile.FullName)
                {
                    if (files.Length - 1 > i)
                    {
                        return files[i + 1];
                    }
                    return files[0];
                }
            }
            return files[0];
        }

        private static FileInfo[] GetChecklistsFiles()
        {
            const string searchPattern = "*.pdf";
            var di = new DirectoryInfo(Application.ExecutablePath);
            if (di.Parent != null)
            {
                var folderToSearch = di.Parent.FullName + Path.DirectorySeparatorChar + "checklists";
                return GetFilesOfType(searchPattern, folderToSearch);
            }
            return null;
        }

        private static FileInfo[] GetChartFiles()
        {
            const string searchPattern = "*.pdf";
            var di = new DirectoryInfo(Application.ExecutablePath);
            if (di.Parent != null)
            {
                var folderToSearch = di.Parent.FullName + Path.DirectorySeparatorChar + "charts";
                return GetFilesOfType(searchPattern, folderToSearch);
            }
            return null;
        }

        private static FileInfo[] GetFilesOfType(string searchPattern, string folderToSearch)
        {
            if (String.IsNullOrEmpty(searchPattern) || String.IsNullOrEmpty(folderToSearch))
            {
                return null;
            }
            var di = new DirectoryInfo(folderToSearch);
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

        private static float GetMapScaleForCADRGScaleText(string CADRGScaletext)
        {
            var toReturn = float.NaN;
            switch (CADRGScaletext)
            {
                case "1:250 M":
                    toReturn = 250*1000*1000;
                    break;
                case "1:100 M":
                    toReturn = 100*1000*1000;
                    break;
                case "1:50 M":
                    toReturn = 50*1000*1000;
                    break;
                case "1:25 M":
                    toReturn = 25*1000*1000;
                    break;
                case "1:10 M":
                    toReturn = 10*1000*1000;
                    break;
                case "1:5 M":
                    toReturn = 5*1000*1000;
                    break;
                case "1:2 M":
                    toReturn = 2*1000*1000;
                    break;
                case "1:1 M":
                    toReturn = 1000*1000;
                    break;
                case "1:500 K":
                    toReturn = 500*1000;
                    break;
                case "1:250 K":
                    toReturn = 250*1000;
                    break;
                case "1:100 K":
                    toReturn = 100*1000;
                    break;
                case "1:50 K":
                    toReturn = 50*1000;
                    break;
                case "1:25 K":
                    toReturn = 25*1000;
                    break;
                case "1:10 K":
                    toReturn = 10*1000;
                    break;
                case "1:5 K":
                    toReturn = 5*1000;
                    break;
                default:
                    break;
            }
            return toReturn;
        }

        private static float GetNextLowerMapScale(float mapScale)
        {
            var toReturn = mapScale;
            var mapScaleText = GetCADRGScaleTextForMapScale(mapScale);
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

        private static float GetNextHigherMapScale(float mapScale)
        {
            var toReturn = mapScale;
            var mapScaleText = GetCADRGScaleTextForMapScale(mapScale);
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
            var thisPage = new MfdMenuPage(this);
            var buttons = new List<OptionSelectButton>
                              {
                                  CreateOptionSelectButton(thisPage, 1, "CNTL", true),
                                  CreateOptionSelectButton(thisPage, 2, "PROF", false)
                              };

            var controlMapPageSelectButton = CreateOptionSelectButton(thisPage, 3, "MAP", true);
            controlMapPageSelectButton.Pressed += controlMapPageSelectButton_Press;
            buttons.Add(controlMapPageSelectButton);

            var controlOverlayPageSelectButton = CreateOptionSelectButton(thisPage, 4, "OVR", false);
            controlOverlayPageSelectButton.Pressed += controlOverlayPageSelectButton_Press;
            buttons.Add(controlOverlayPageSelectButton);

            var testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed += testPageSelectButton_Press;
            buttons.Add(testPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 7, "DEL\n\rCIB", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 8, "DEL\n\rMAP", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 9, "DEL\n\rDRW", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 10, "DEL\n\rSHP", false));

            var imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += imagingPageSelectButton_Press;
            buttons.Add(imagingPageSelectButton);

            var messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += messagingPageSelectButton_Press;
            buttons.Add(messagingPageSelectButton);

            var tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD",
                                                                                    false);
            tacticalAwarenessDisplayPageSelectButton.Pressed += tacticalAwarenessDisplayPageSelectButton_Press;
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            var targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += targetingPodPageSelectButton_Press;
            buttons.Add(targetingPodPageSelectButton);

            var headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += headDownDisplayPageSelectButton_Press;
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
            var newPage = FindMenuPageByName("Control Overlay Page");
            ActiveMenuPage = newPage;
        }

        private void controlMapPageSelectButton_Press(object sender, EventArgs e)
        {
            var newPage = FindMenuPageByName("Control Map Page");
            ActiveMenuPage = newPage;
        }

        private MfdMenuPage BuildControlOverlayMenuPage()
        {
            var thisPage = new MfdMenuPage(this);
            var buttons = new List<OptionSelectButton>
                              {
                                  CreateOptionSelectButton(thisPage, 1, "CNTL", true),
                                  CreateOptionSelectButton(thisPage, 2, "PROF", false)
                              };

            var controlMapPageSelectButton = CreateOptionSelectButton(thisPage, 3, "MAP", false);
            controlMapPageSelectButton.Pressed += controlMapPageSelectButton_Press;
            buttons.Add(controlMapPageSelectButton);

            var controlOverlayPageSelectButton = CreateOptionSelectButton(thisPage, 4, "OVR", true);
            controlOverlayPageSelectButton.Pressed += controlOverlayPageSelectButton_Press;
            buttons.Add(controlOverlayPageSelectButton);

            var testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed += testPageSelectButton_Press;
            buttons.Add(testPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 7, "^", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 8, "NO DRW FILES", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 9, @"\/", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 10, "ECHUM", false));

            var imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += imagingPageSelectButton_Press;
            buttons.Add(imagingPageSelectButton);

            var messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += messagingPageSelectButton_Press;
            buttons.Add(messagingPageSelectButton);

            var tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD",
                                                                                    false);
            tacticalAwarenessDisplayPageSelectButton.Pressed += tacticalAwarenessDisplayPageSelectButton_Press;
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            var targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += targetingPodPageSelectButton_Press;
            buttons.Add(targetingPodPageSelectButton);

            var headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += headDownDisplayPageSelectButton_Press;
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
            var thisPage = new MfdMenuPage(this);
            var buttons = new List<OptionSelectButton>
                              {
                                  CreateOptionSelectButton(thisPage, 2, "TO\n\rINTEL", false),
                                  CreateOptionSelectButton(thisPage, 4, "OWN\n\rFRISCO", false)
                              };

            var testPageSelectButton = CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed += testPageSelectButton_Press;
            buttons.Add(testPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 7, "^", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 7.5f, "16102F\n\r16UKWN0", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 8, @"\/", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 9, "SEND", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 10, "DATA\n\rMODE", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 11, "DEL", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 12, "DEL ALL", false));


            var imagingPageSelectButton = CreateOptionSelectButton(thisPage, 14, "IMG", true);
            imagingPageSelectButton.Pressed += imagingPageSelectButton_Press;
            buttons.Add(imagingPageSelectButton);

            var messagingPageSelectButton = CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += messagingPageSelectButton_Press;
            buttons.Add(messagingPageSelectButton);

            var tacticalAwarenessDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 16, "TAD",
                                                                                    false);
            tacticalAwarenessDisplayPageSelectButton.Pressed += tacticalAwarenessDisplayPageSelectButton_Press;
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            var targetingPodPageSelectButton = CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += targetingPodPageSelectButton_Press;
            buttons.Add(targetingPodPageSelectButton);

            var headDownDisplayPageSelectButton = CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += headDownDisplayPageSelectButton_Press;
            buttons.Add(headDownDisplayPageSelectButton);

            buttons.Add(CreateOptionSelectButton(thisPage, 22, "CAP", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 23, "X1", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 24, "TO USB", false));
            buttons.Add(CreateOptionSelectButton(thisPage, 25, "TO MFCD", false));
            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Bitmap Annotation Page";
            return thisPage;
        }

        private static OptionSelectButton CreateOptionSelectButton(MfdMenuPage page, float positionNum, string labelText,
                                                                   bool invertLabelText)
        {
            return CreateOptionSelectButton(page, positionNum, labelText, invertLabelText, null);
        }

        private static OptionSelectButton CreateOptionSelectButton(MfdMenuPage page, float positionNum, string labelText,
                                                                   bool invertLabelText, int? triangleLegLengthPixels)
        {
            var button = new OptionSelectButton(page)
                             {
                                 PositionNumber = positionNum,
                                 LabelText = labelText,
                                 InvertLabelText = invertLabelText
                             };
            var boundingRectangle = CalculateOSBLabelBitmapRectangle(positionNum);
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
            else if (positionNum >= 6 && positionNum <= 13)
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

        private static Rectangle CalculateOSBLabelBitmapRectangle(float positionNum)
        {
            const float pixelsPerInch = Constants.F_NATIVE_RES_HEIGHT/8.32f;
            const float bezelButtonRevealWidthInches = 0.83376676384839650145772594752187f;
            var bezelButtonRevealWidthPixels = (int) Math.Floor((bezelButtonRevealWidthInches*pixelsPerInch));

            const float bezelButtonRevealHeightInches = 0.83695842450765864332603938730853f;
            var bezelButtonRevealHeightPixels = (int) Math.Floor((bezelButtonRevealHeightInches*pixelsPerInch));

            var maxTextWidthPixels = (int) (bezelButtonRevealWidthPixels*1.5f);
            const float bezelButtonSeparatorWidthInches = 0.14500291545189504373177842565598f;
            var bezelButtonSeparatorWidthPixels = (int) (Math.Ceiling(bezelButtonSeparatorWidthInches*pixelsPerInch));
            const float bezelButtonSeparatorHeightInches = bezelButtonSeparatorWidthInches;
            //0.14555798687089715536105032822757f;
            var bezelButtonSeparatorHeightPixels = (int) (Math.Ceiling(bezelButtonSeparatorHeightInches*pixelsPerInch));
            var leftMarginPixels =
                (int)
                (((Constants.I_NATIVE_RES_WIDTH -
                   ((5*bezelButtonRevealWidthPixels) + (4*bezelButtonSeparatorWidthPixels)))/2.0f));
            var topMarginPixels =
                (int)
                (((Constants.I_NATIVE_RES_HEIGHT -
                   ((8*bezelButtonRevealHeightPixels) + (7*bezelButtonSeparatorHeightPixels)))/2.0f));
            var boundingRectangle = new Rectangle();
            if (positionNum >= 1 && positionNum <= 5)
            {
                //TOP ROW OF BUTTONS
                var x = (int) (((positionNum - 1)*(bezelButtonRevealWidthPixels + bezelButtonSeparatorWidthPixels))) +
                        leftMarginPixels;
                boundingRectangle.Location = new Point(x, 0);
                var width = bezelButtonRevealWidthPixels;
                var height = bezelButtonRevealHeightPixels;
                boundingRectangle.Size = new Size(width, height);
            }
            else if (positionNum >= 6 && positionNum <= 13)
            {
                //RIGHT HAND SIDE BUTTONS
                var y = (int) (((positionNum - 6)*(bezelButtonRevealHeightPixels + bezelButtonSeparatorHeightPixels))) +
                        topMarginPixels;
                var width = maxTextWidthPixels;
                var height = bezelButtonRevealHeightPixels;
                boundingRectangle.Size = new Size(width, height);
                boundingRectangle.Location = new Point(Constants.I_NATIVE_RES_WIDTH - width, y);
            }
            else if (positionNum >= 14 && positionNum <= 18)
            {
                //BOTTOM ROW OF BUTTONS
                var x = (int) (((18 - positionNum)*(bezelButtonRevealWidthPixels + bezelButtonSeparatorWidthPixels))) +
                        leftMarginPixels;
                var y = Constants.I_NATIVE_RES_HEIGHT - bezelButtonRevealHeightPixels;
                boundingRectangle.Location = new Point(x, y);
                var width = bezelButtonRevealWidthPixels;
                var height = bezelButtonRevealHeightPixels;
                boundingRectangle.Size = new Size(width, height);
            }
            else if (positionNum >= 19 && positionNum <= 26)
            {
                //LEFT HAND SIDE BUTTONS
                var y =
                    (int) (((26 - positionNum)*(bezelButtonRevealHeightPixels + bezelButtonSeparatorHeightPixels))) +
                    topMarginPixels;
                var width = maxTextWidthPixels;
                var height = bezelButtonRevealHeightPixels;
                boundingRectangle.Size = new Size(width, height);
                boundingRectangle.Location = new Point(0, y);
            }
            return boundingRectangle;
        }

        public void ProcessPendingMessages()
        {
            if (Settings.Default.RunAsServer)
            {
                ProcessPendingMessagesToServerFromClient();
            }
            else if (Settings.Default.RunAsClient)
            {
                ProcessPendingMessagesToClientFromServer();
            }
        }

        public override void Render(Graphics g)
        {
            Brush greenBrush = new SolidBrush(Color.FromArgb(0, 255, 0));

            var startTime = DateTime.Now;
            //Debug.WriteLine("here 1 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
            //g.Clear(Color.Transparent);
            var labelWidth = (int) (35*(ScreenBoundsPixels.Width/Constants.F_NATIVE_RES_WIDTH));
            var labelHeight = (int) (20*(ScreenBoundsPixels.Height/Constants.F_NATIVE_RES_HEIGHT));
            var overallRenderRectangle = new Rectangle(0, 0, ScreenBoundsPixels.Width, ScreenBoundsPixels.Height);
            OptionSelectButton button;
            var origTransform = g.Transform;

            //Debug.WriteLine("here 2 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
            if (ActiveMenuPage.Name == "Instruments Display Page")
            {
                button = ActiveMenuPage.FindOptionSelectButtonByFunctionName("AcknowledgeMessage");
                button.Visible = !FlightData.PfdOffFlag & FlightData.CpdPowerOnFlag;

                button = ActiveMenuPage.FindOptionSelectButtonByFunctionName("TestHdd");
                button.Visible = !FlightData.PfdOffFlag & FlightData.CpdPowerOnFlag;

                button = ActiveMenuPage.FindOptionSelectButtonByFunctionName("AltitudeIndexIncrease");
                button.Visible = !FlightData.PfdOffFlag & FlightData.CpdPowerOnFlag;
                button = ActiveMenuPage.FindOptionSelectButtonByLabelText("ALT");
                button.Visible = !FlightData.PfdOffFlag & FlightData.CpdPowerOnFlag;
                button = ActiveMenuPage.FindOptionSelectButtonByFunctionName("AltitudeIndexDecrease");
                button.Visible = !FlightData.PfdOffFlag & FlightData.CpdPowerOnFlag;

                button = ActiveMenuPage.FindOptionSelectButtonByFunctionName("BarometricPressureSettingIncrease");
                button.Visible = !FlightData.PfdOffFlag & FlightData.CpdPowerOnFlag;
                button = ActiveMenuPage.FindOptionSelectButtonByLabelText("BARO");
                button.Visible = !FlightData.PfdOffFlag & FlightData.CpdPowerOnFlag;
                button = ActiveMenuPage.FindOptionSelectButtonByFunctionName("BarometricPressureSettingDecrease");
                button.Visible = !FlightData.PfdOffFlag & FlightData.CpdPowerOnFlag;

                button = ActiveMenuPage.FindOptionSelectButtonByFunctionName("ToggleAltimeterModeElecPneu");
                button.Visible = !FlightData.PfdOffFlag & FlightData.CpdPowerOnFlag;

                button = ActiveMenuPage.FindOptionSelectButtonByFunctionName("LowAltitudeWarningThresholdIncrease");
                button.Visible = !FlightData.PfdOffFlag & FlightData.CpdPowerOnFlag;
                button = ActiveMenuPage.FindOptionSelectButtonByLabelText("ALOW");
                button.Visible = !FlightData.PfdOffFlag & FlightData.CpdPowerOnFlag;
                button = ActiveMenuPage.FindOptionSelectButtonByFunctionName("LowAltitudeWarningThresholdDecrease");
                button.Visible = !FlightData.PfdOffFlag & FlightData.CpdPowerOnFlag;

                button = ActiveMenuPage.FindOptionSelectButtonByFunctionName("AirspeedIndexIncrease");
                button.Visible = !FlightData.PfdOffFlag & FlightData.CpdPowerOnFlag;
                button = ActiveMenuPage.FindOptionSelectButtonByLabelText("ASPD");
                button.Visible = !FlightData.PfdOffFlag & FlightData.CpdPowerOnFlag;
                button = ActiveMenuPage.FindOptionSelectButtonByFunctionName("AirspeedIndexDecrease");
                button.Visible = !FlightData.PfdOffFlag & FlightData.CpdPowerOnFlag;

                button = ActiveMenuPage.FindOptionSelectButtonByFunctionName("CourseSelectIncrease");
                button.Visible = !FlightData.HsiOffFlag & FlightData.CpdPowerOnFlag;
                button = ActiveMenuPage.FindOptionSelectButtonByLabelText("CRS");
                button.Visible = !FlightData.HsiOffFlag & FlightData.CpdPowerOnFlag;
                button = ActiveMenuPage.FindOptionSelectButtonByFunctionName("CourseSelectDecrease");
                button.Visible = !FlightData.HsiOffFlag & FlightData.CpdPowerOnFlag;

                button = ActiveMenuPage.FindOptionSelectButtonByFunctionName("HeadingSelectIncrease");
                button.Visible = !FlightData.HsiOffFlag & FlightData.CpdPowerOnFlag;
                button = ActiveMenuPage.FindOptionSelectButtonByLabelText("HDG");
                button.Visible = !FlightData.HsiOffFlag & FlightData.CpdPowerOnFlag;
                button = ActiveMenuPage.FindOptionSelectButtonByFunctionName("HeadingSelectDecrease");
                button.Visible = !FlightData.HsiOffFlag & FlightData.CpdPowerOnFlag;
            }
            else if (ActiveMenuPage.Name == "TAD Page")
            {
                //Debug.WriteLine("here 3 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
                var scaleLabel = ActiveMenuPage.FindOptionSelectButtonByFunctionName("MapScaleLabel");
                var scaleString = "CADRG\r\n" + GetCADRGScaleTextForMapScale(_mapScale);
                scaleLabel.LabelText = scaleString;

                var mapRangeLabel = ActiveMenuPage.FindOptionSelectButtonByFunctionName("MapRangeLabel");
                var mapRangeString = _mapRangeRingsDiameterInNauticalMiles.ToString();
                mapRangeLabel.LabelText = mapRangeString;
                //Debug.WriteLine("here 4 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
            }
            else if (ActiveMenuPage.Name == "Checklists Page")
            {
                if (_currentChecklistFile == null) NextChecklistFile();
                var currentChecklistFileLabel =
                    ActiveMenuPage.FindOptionSelectButtonByFunctionName("CurrentChecklistFileLabel");
                string shortName = null;
                if (_currentChecklistFile != null)
                {
                    shortName = Common.Win32.Paths.Util.GetShortPathName(_currentChecklistFile.FullName);
                    shortName = Common.Win32.Paths.Util.Compact(_currentChecklistFile.Name, 64);
                    shortName = BreakStringIntoLines(shortName, 9);
                }
                if (shortName != null)
                {
                    var labelText = shortName.ToUpperInvariant();
                    currentChecklistFileLabel.LabelText = labelText;
                }

                var currentChecklistPageNumLabel =
                    ActiveMenuPage.FindOptionSelectButtonByFunctionName("CurrentChecklistPageNumLabel");
                currentChecklistPageNumLabel.LabelText = _currentChecklistPageNum + "/" + _currentChecklistPagesTotal;
            }
            else if (ActiveMenuPage.Name == "Charts Page")
            {
                if (_currentChartFile == null) NextChartFile();
                var currentChartFileLabel =
                    ActiveMenuPage.FindOptionSelectButtonByFunctionName("CurrentChartFileLabel");
                string shortName = null;
                if (_currentChartFile != null)
                {
                    shortName = Common.Win32.Paths.Util.GetShortPathName(_currentChartFile.FullName);
                    shortName = Common.Win32.Paths.Util.Compact(_currentChartFile.Name, 64);
                    shortName = BreakStringIntoLines(shortName, 9);
                }
                if (shortName != null)
                {
                    var labelText = shortName.ToUpperInvariant();
                    currentChartFileLabel.LabelText = labelText;
                }

                var currentChartPageNumLabel =
                    ActiveMenuPage.FindOptionSelectButtonByFunctionName("CurrentChartPageNumLabel");
                currentChartPageNumLabel.LabelText = _currentChartPageNum + "/" + _currentChartPagesTotal;
            }
            //Debug.WriteLine("here 5 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
            g.SetClip(overallRenderRectangle);
            g.TranslateTransform(overallRenderRectangle.X, overallRenderRectangle.Y);
            var originalSize = new Size(Constants.I_NATIVE_RES_WIDTH, Constants.I_NATIVE_RES_HEIGHT);
            g.ScaleTransform((overallRenderRectangle.Width/(float) originalSize.Width),
                             (overallRenderRectangle.Height/(float) originalSize.Height));
            //Debug.WriteLine("here 6 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);

            if (!FlightData.CpdPowerOnFlag)
            {
                const string toDisplay = "OFF";
                var path = new GraphicsPath();
                var sf = new StringFormat(StringFormatFlags.NoWrap)
                             {
                                 Alignment = StringAlignment.Center,
                                 LineAlignment = StringAlignment.Center
                             };
                var f = new Font(FontFamily.GenericMonospace, 20, FontStyle.Bold);
                var textSize = g.MeasureString(toDisplay, f, overallRenderRectangle.Size, sf);
                var leftX = (((Constants.I_NATIVE_RES_WIDTH - ((int) textSize.Width))/2));
                var topY = (((Constants.I_NATIVE_RES_HEIGHT - ((int) textSize.Height))/2));
                var target = new Rectangle(leftX, topY, (int) textSize.Width, (int) textSize.Height);
                path.AddString(toDisplay, f.FontFamily, (int) f.Style, f.SizeInPoints, target, sf);
                /*
                g.FillRectangle(greenBrush, target);
                g.FillPath(Brushes.Black, path); 
                 */
                g.FillPath(greenBrush, path);
                return;
            }

            //Debug.WriteLine("here 7 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);

            if (FlightData.CpdPowerOnFlag)
            {
                g.Transform = origTransform;

                if (ActiveMenuPage.Name == "Instruments Display Page")
                {
                    var pfdRenderStart = DateTime.Now;
                    var pfd = Pfd;
                    pfd.Manager = this;
                    var pfdRenderRectangle = new Rectangle(labelWidth + 1, labelHeight + 1,
                                                           (ScreenBoundsPixels.Width - ((labelWidth + 1)*2)),
                                                           ((ScreenBoundsPixels.Height - ((labelHeight + 1)*2))/2) + 10);
                    pfdRenderRectangle = new Rectangle(pfdRenderRectangle.Left, pfdRenderRectangle.Top,
                                                       (pfdRenderRectangle.Width), (pfdRenderRectangle.Height));
                    var pfdRenderSize = new Size(610, 495);
                    g.SetClip(pfdRenderRectangle);
                    g.TranslateTransform(pfdRenderRectangle.X, pfdRenderRectangle.Y);
                    g.ScaleTransform((pfdRenderRectangle.Width/(float) pfdRenderSize.Width),
                                     (pfdRenderRectangle.Height/(float) pfdRenderSize.Height));
                    pfd.Render(g, pfdRenderSize);
                    g.Transform = origTransform;

                    var pfdRenderEnd = DateTime.Now;
                    var pfdRenderTime = pfdRenderEnd.Subtract(pfdRenderStart);
                    //Debug.WriteLine("PFD render time:" + pfdRenderTime.TotalMilliseconds);


                    var hsiRenderStart = DateTime.Now;
                    var hsi = Hsi;
                    hsi.Manager = this;
                    var hsiRenderBounds = new Rectangle(pfdRenderRectangle.Left, pfdRenderRectangle.Bottom + 5,
                                                        pfdRenderRectangle.Width, pfdRenderRectangle.Height - 40);
                    var hsiRenderSize = new Size(596, 391);
                    origTransform = g.Transform;
                    g.SetClip(hsiRenderBounds);
                    g.TranslateTransform(hsiRenderBounds.X, hsiRenderBounds.Y);
                    g.ScaleTransform((hsiRenderBounds.Width/(float) hsiRenderSize.Width),
                                     (hsiRenderBounds.Height/(float) hsiRenderSize.Height));
                    hsi.Render(g, hsiRenderSize);
                    g.Transform = origTransform;
                    var hsiRenderEnd = DateTime.Now;
                    var hsiRenderTime = hsiRenderEnd.Subtract(hsiRenderStart);
                    //Debug.WriteLine("HSI render time:" + hsiRenderTime.TotalMilliseconds);
                }
                else if (ActiveMenuPage.Name == "TAD Page")
                {
                    //Debug.WriteLine("here 8 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);

                    if (Settings.Default.RunAsClient)
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
                        var payload = new Dictionary<string, object>
                                          {
                                              {"RenderSize", ScreenBoundsPixels},
                                              {"MapScale", _mapScale},
                                              {"RangeRingsDiameter", _mapRangeRingsDiameterInNauticalMiles}
                                          };
                        var message = new Message("RequestNewMapImage", payload);
                        Client.SendMessageToServer(message);
                        //Debug.WriteLine("here 10 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
                    }
                    else
                    {
                        //Debug.WriteLine("here 11 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
                        RenderMapLocally(g, _mapScale, _mapRangeRingsDiameterInNauticalMiles);
                        //Debug.WriteLine("here 12 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
                    }
                }
                else if (ActiveMenuPage.Name == "Checklists Page")
                {
                    if (_currentChecklistFile != null)
                    {
                        var checklistRenderRectangle = new Rectangle(labelWidth + 1, labelHeight + 1,
                                                                     (ScreenBoundsPixels.Width - ((labelWidth + 1)*2)),
                                                                     ((ScreenBoundsPixels.Height - ((labelHeight + 1)*2))));
                        RenderCurrentChecklist(g, checklistRenderRectangle);
                    }
                }
                else if (ActiveMenuPage.Name == "Charts Page")
                {
                    if (_currentChartFile != null)
                    {
                        var chartRenderRectangle = new Rectangle(labelWidth + 1, labelHeight + 1,
                                                                 (ScreenBoundsPixels.Width - ((labelWidth + 1)*2)),
                                                                 ((ScreenBoundsPixels.Height - ((labelHeight + 1)*2))));
                        RenderCurrentChart(g, chartRenderRectangle);
                    }
                }

                //Debug.WriteLine("here 13 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);

                g.Transform = origTransform;
                g.SetClip(overallRenderRectangle);
                g.TranslateTransform(overallRenderRectangle.X, overallRenderRectangle.Y);
                g.ScaleTransform((overallRenderRectangle.Width/(float) originalSize.Width),
                                 (overallRenderRectangle.Height/(float) originalSize.Height));
                //Debug.WriteLine("here 14 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);

                foreach (var thisButton in ActiveMenuPage.OptionSelectButtons)
                {
                    if (thisButton.Visible)
                    {
                        thisButton.DrawLabel(g);
                    }
                }
                //Debug.WriteLine("here 15 at " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
                g.Transform = origTransform;

                var finishTime = DateTime.Now;
                var elapsed = finishTime.Subtract(startTime);
                //Debug.WriteLine("Overall CPD render time:" + elapsed.TotalMilliseconds);
            }
        }

        private static string BreakStringIntoLines(string toBreak, int maxLineLength)
        {
            if (toBreak == null) return null;
            if (maxLineLength <= 0) return "";
            var sb = new StringBuilder();
            for (var i = 0; i < toBreak.Length; i++)
            {
                sb.Append(toBreak[i]);
                if ((i + 1)%maxLineLength == 0)
                {
                    sb.Append("\n");
                }
            }
            return sb.ToString();
        }

        private void RenderCurrentChecklist(Graphics target, Rectangle targetRect)
        {
            var origCompositQuality = target.CompositingQuality;
            var origSmoothingMode = target.SmoothingMode;
            var origInterpolationMode = target.InterpolationMode;

            target.InterpolationMode = InterpolationMode.HighQualityBicubic;
            target.SmoothingMode = SmoothingMode.HighQuality;
            target.CompositingQuality = CompositingQuality.HighQuality;
            if (_currentChecklistFile != null)
            {
                if (_lastRenderedChecklistFile == null ||
                    _currentChecklistFile.FullName != _lastRenderedChecklistFile.FullName ||
                    _lastRenderedChecklistPageNum != _currentChecklistPageNum)
                {
                    Common.Util.DisposeObject(_lastRenderedChecklistPdfPage);
                    _lastRenderedChecklistPdfPage = PdfRenderEngine.GeneratePageBitmap(_currentChecklistFile.FullName,
                                                                                       _currentChecklistPageNum,
                                                                                       new Size(150, 150));
                    _lastRenderedChecklistPageNum = _currentChecklistPageNum;
                    _lastRenderedChecklistFile = _currentChecklistFile;
                }
                if (_lastRenderedChecklistPdfPage != null)
                {
                    if (_nightMode)
                    {
                        using (var copy = (Bitmap) Common.Imaging.Util.CopyBitmap(_lastRenderedChecklistPdfPage))
                        using (var reverseVideo = (Bitmap) Common.Imaging.Util.GetDimmerImage(copy, 0.4f))
                        {
                            target.DrawImage(reverseVideo, targetRect, 0, 0, reverseVideo.Width, reverseVideo.Height,
                                             GraphicsUnit.Pixel);
                        }
                    }
                    else
                    {
                        target.DrawImage(_lastRenderedChecklistPdfPage, targetRect, 0, 0,
                                         _lastRenderedChecklistPdfPage.Width, _lastRenderedChecklistPdfPage.Height,
                                         GraphicsUnit.Pixel);
                    }
                }
            }
            target.InterpolationMode = origInterpolationMode;
            target.SmoothingMode = origSmoothingMode;
            target.CompositingQuality = origCompositQuality;
        }

        private void RenderCurrentChart(Graphics target, Rectangle targetRect)
        {
            var origCompositQuality = target.CompositingQuality;
            var origSmoothingMode = target.SmoothingMode;
            var origInterpolationMode = target.InterpolationMode;

            target.InterpolationMode = InterpolationMode.HighQualityBicubic;
            target.SmoothingMode = SmoothingMode.HighQuality;
            target.CompositingQuality = CompositingQuality.HighQuality;
            if (_currentChartFile != null)
            {
                if (_lastRenderedChartFile == null || _currentChartFile.FullName != _lastRenderedChartFile.FullName ||
                    _lastRenderedChartPageNum != _currentChartPageNum)
                {
                    Common.Util.DisposeObject(_lastRenderedChartPdfPage);
                    _lastRenderedChartPdfPage = PdfRenderEngine.GeneratePageBitmap(_currentChartFile.FullName,
                                                                                   _currentChartPageNum,
                                                                                   new Size(150, 150));
                    _lastRenderedChartPageNum = _currentChartPageNum;
                    _lastRenderedChartFile = _currentChartFile;
                }
                if (_lastRenderedChartPdfPage != null)
                {
                    if (_nightMode)
                    {
                        using (var copy = (Bitmap) Common.Imaging.Util.CopyBitmap(_lastRenderedChartPdfPage))
                        using (var reverseVideo = (Bitmap) Common.Imaging.Util.GetDimmerImage(copy, 0.4f))
                        {
                            target.DrawImage(reverseVideo, targetRect, 0, 0, reverseVideo.Width, reverseVideo.Height,
                                             GraphicsUnit.Pixel);
                        }
                    }
                    else
                    {
                        target.DrawImage(_lastRenderedChartPdfPage, targetRect, 0, 0, _lastRenderedChartPdfPage.Width,
                                         _lastRenderedChartPdfPage.Height, GraphicsUnit.Pixel);
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
            var mapBytes = (byte[]) Client.GetSimProperty("CurrentMapImage");
            //causes a method invoke on the server to occur
            Bitmap mapFromServer = null;
            if (mapBytes != null && mapBytes.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    ms.Write(mapBytes, 0, mapBytes.Length);
                    ms.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    mapFromServer = (Bitmap) Image.FromStream(ms);
                }
            }
            lock (_mapImageLock)
            {
                Common.Util.DisposeObject(_lastMapFromServer);
                _lastMapFromServer = mapFromServer;
            }
        }

        private Bitmap RenderMapOnBehalfOfRemoteClient(Size renderSize, float mapScale,
                                                       int rangeRingDiameterInNauticalMiles)
        {
            var rendered = new Bitmap(renderSize.Width, renderSize.Height, PixelFormat.Format16bppRgb565);
            using (var g = Graphics.FromImage(rendered))
            {
                var renderRectangle = new Rectangle(new Point(0, 0), renderSize);
                RenderMapLocally(g, renderRectangle, mapScale, rangeRingDiameterInNauticalMiles);
            }
            return rendered;
        }

        private void RenderMapLocally(Graphics g, float mapScale, int rangeRingDiameterInNauticalMiles)
        {
            var renderRectangle = new Rectangle(0, 0, (ScreenBoundsPixels.Width), (ScreenBoundsPixels.Height));
            RenderMapLocally(g, renderRectangle, mapScale, rangeRingDiameterInNauticalMiles);
        }

        private void RenderMapLocally(Graphics g, Rectangle renderRectangle, float mapScale,
                                      int rangeRingDiameterInNauticalMiles)
        {
            if (Settings.Default.RunAsClient) return;
            var greenBrush = Brushes.Green;
            var tadRenderStart = DateTime.Now;
            var tadRenderRectangle = renderRectangle;
            g.SetClip(tadRenderRectangle);
            SimSupportModule.RenderMap(g, tadRenderRectangle, mapScale, rangeRingDiameterInNauticalMiles,
                                       MapRotationMode.CurrentHeadingOnTop);

            var scaleX = (tadRenderRectangle.Width)/Constants.F_NATIVE_RES_WIDTH;
            var scaleY = (tadRenderRectangle.Height)/Constants.F_NATIVE_RES_HEIGHT;

            g.ScaleTransform(scaleX, scaleY);

            var latLongRect = new Rectangle(192, 734, (406 - 192), (768 - 734));
            g.FillRectangle(Brushes.Black, latLongRect);
            var latitudeDecDeg = FlightData.LatitudeInDecimalDegrees;
            var longitudeDecDeg = FlightData.LongitudeInDecimalDegrees;

            float latitudeWholeDeg = (int) (latitudeDecDeg);
            float longitudeWholeDeg = (int) (longitudeDecDeg);

            var latitudeMinutes = (latitudeDecDeg - latitudeWholeDeg)*60.0f;
            var longitudeMinutes = (longitudeDecDeg - longitudeWholeDeg)*60.0f;

            var latitudeQualifier = latitudeDecDeg >= 0.0f ? "N" : "S";
            var longitudeQualifier = longitudeDecDeg >= 0.0f ? "E" : "W";

            var latString = latitudeQualifier + latitudeWholeDeg + " " + string.Format("{0:00.000}", latitudeMinutes);
            var longString = longitudeQualifier + longitudeWholeDeg + " " +
                             string.Format("{0:00.000}", longitudeMinutes);
            var latLongString = latString + "  " + longString;
            var latLongFont = new Font(FontFamily.GenericMonospace, 10, FontStyle.Bold);

            g.DrawString(latLongString, latLongFont, greenBrush, latLongRect);

            var tadRenderEnd = DateTime.Now;
            var tadRenderTime = tadRenderEnd.Subtract(tadRenderStart);
            //Debug.WriteLine("TAD render time:" + tadRenderTime.TotalMilliseconds);
        }

        private static void InformClientOfCpdInputControlChangedEvent(CpdInputControls control)
        {
            if (!Settings.Default.RunAsServer) return;
            var message = new Message("CpdInputControlChangedEvent", control);
            F16CPDServer.SubmitMessageToClient(message);
        }

        public void FireHandler(CpdInputControls control)
        {
            if (Settings.Default.RunAsServer)
            {
                InformClientOfCpdInputControlChangedEvent(control);
            }
            else
            {
                var inputControl = GetControl(control);
                if (inputControl != null)
                {
                    if (inputControl is MomentaryButtonMfdInputControl)
                    {
                        ((MomentaryButtonMfdInputControl) inputControl).Press(DateTime.Now);
                    }
                    else if (inputControl is ToggleSwitchMfdInputControl)
                    {
                        ((ToggleSwitchMfdInputControl) inputControl).Toggle();
                    }
                    else if (inputControl is ToggleSwitchPositionMfdInputControl)
                    {
                        ((ToggleSwitchPositionMfdInputControl) inputControl).Activate();
                    }
                    else if (inputControl is RotaryEncoderMfdInputControl)
                    {
                        ((RotaryEncoderMfdInputControl) inputControl).RotateClockwise();
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
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(1);
                    }
                    break;
                case CpdInputControls.OsbButton2:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(2);
                    }
                    break;
                case CpdInputControls.OsbButton3:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(3);
                    }
                    break;
                case CpdInputControls.OsbButton4:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(4);
                    }
                    break;
                case CpdInputControls.OsbButton5:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(5);
                    }
                    break;
                case CpdInputControls.OsbButton6:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(6);
                    }
                    break;
                case CpdInputControls.OsbButton7:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(7);
                    }
                    break;
                case CpdInputControls.OsbButton8:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(8);
                    }
                    break;
                case CpdInputControls.OsbButton9:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(9);
                    }
                    break;
                case CpdInputControls.OsbButton10:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(10);
                    }
                    break;
                case CpdInputControls.OsbButton11:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(11);
                    }
                    break;
                case CpdInputControls.OsbButton12:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(12);
                    }
                    break;
                case CpdInputControls.OsbButton13:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(13);
                    }
                    break;
                case CpdInputControls.OsbButton14:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(14);
                    }
                    break;
                case CpdInputControls.OsbButton15:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(15);
                    }
                    break;
                case CpdInputControls.OsbButton16:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(16);
                    }
                    break;
                case CpdInputControls.OsbButton17:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(17);
                    }
                    break;
                case CpdInputControls.OsbButton18:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(18);
                    }
                    break;
                case CpdInputControls.OsbButton19:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(19);
                    }
                    break;
                case CpdInputControls.OsbButton20:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(20);
                    }
                    break;
                case CpdInputControls.OsbButton21:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(21);
                    }
                    break;
                case CpdInputControls.OsbButton22:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(22);
                    }
                    break;
                case CpdInputControls.OsbButton23:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(23);
                    }
                    break;
                case CpdInputControls.OsbButton24:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(24);
                    }
                    break;
                case CpdInputControls.OsbButton25:
                    if (ActiveMenuPage != null)
                    {
                        toReturn = ActiveMenuPage.FindOptionSelectButtonByPositionNumber(25);
                    }
                    break;
                case CpdInputControls.OsbButton26:
                    if (ActiveMenuPage != null)
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
        ///   Public implementation of IDisposable.Dispose().  Cleans up managed
        ///   and unmanaged resources used by this object before allowing garbage collection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   Standard finalizer, which will call Dispose() if this object is not
        ///   manually disposed.  Ordinarily called only by the garbage collector.
        /// </summary>
        ~F16CpdMfdManager()
        {
            Dispose();
        }

        /// <summary>
        ///   Private implementation of Dispose()
        /// </summary>
        /// <param name = "disposing">flag to indicate if we should actually perform disposal.  Distinguishes the private method signature from the public signature.</param>
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    //dispose of managed resources here
                    Common.Util.DisposeObject(Pfd);
                    Common.Util.DisposeObject(Hsi);
                    Common.Util.DisposeObject(_client);
                    TeardownService();
                }
            }
            // Code to dispose the un-managed resources of the class
            _isDisposed = true;
        }

        #endregion
    }
}