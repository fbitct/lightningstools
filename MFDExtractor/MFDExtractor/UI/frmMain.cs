using System;
using System.Threading;
using System.Windows.Forms;
using Common;
using Common.Networking;
using log4net;
using MFDExtractor.Properties;
using MFDExtractor.Runtime;
using Microsoft.DirectX.DirectInput;
using MFDExtractor.UI.Options;

namespace MFDExtractor.UI
{
    /// <summary>
    /// Code-behind for the MFD Extractor's main form
    /// </summary>
    public partial class frmMain : Form
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (frmMain));

        /// <summary>
        /// Event that gets fired from DirectInput when there are keyboard inputs waiting to be processed
        /// </summary>
        private readonly WaitHandle _directInputEvent = new AutoResetEvent(false);

        private readonly object _keybLock = new object();

        /// <summary>
        /// Flag to indicate that the form is closing; allows cleanup methods to detect this fact when called
        /// in multiple contexts
        /// </summary>
        private bool _isClosing;

        /// <summary>
        /// DirectInput keyboard object for reading keyboard state during polling loop
        /// </summary>
        private Device _keyb;

        /// <summary>
        /// Thread on which keyboard polling occurs
        /// </summary>
        private Thread _keyboardWatcherThread;
                       //TODO: convert this to use a global keyboard hook instead of DirectInput?

        /// <summary>
        /// Reference to an instance of the Options form 
        /// </summary>
        private OptionsForm _optionsFormForm;

        /// <summary>
        /// DirectInput Key code representing hotkey which switches the MFD Extractor to 3D mode
        /// </summary>
        private Key _threeDKeyCode;

        /// <summary>
        /// Windows Forms Key code representing hotkey which switches the MFD Extractor to 3D mode
        /// </summary>
        private Keys _threeDkeys;

        /// <summary>
        /// Windows Forms Key code representing hotkey which switches the MFD Extractor to 2D primary view mode
        /// </summary>
        private Keys _twoDPrimaryKeys;

        /// <summary>
        /// DirectInput Key code representing hotkey which switches the MFD Extractor to 2D primary view mode
        /// </summary>
        private Key _twoDPrimaryViewKeyCode;

        /// <summary>
        /// Windows Forms Key code representing hotkey which switches the MFD Extractor to 2D primary view mode
        /// </summary>
        private Keys _twoDSecondaryKeys;

        /// <summary>
        /// DirectInput Key code representing hotkey which switches the MFD Extractor to 2D secondary view mode
        /// </summary>
        private Key _twoDSecondaryViewKeyCode;

        /// <summary>
        /// Default constructor for the form
        /// </summary>
        public frmMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Acquires the DirectInput keyboard device
        /// </summary>
        private void InitializeKeyboard()
        {
            lock (_keybLock)
            {
                try
                {
                    _keyb = new Device(SystemGuid.Keyboard);
                    _keyb.SetCooperativeLevel(null,
                                              CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
                    _keyb.SetEventNotification(_directInputEvent);
                    try
                    {
                        _keyb.Acquire();
                    }
                    catch
                    {
                        Util.DisposeObject(_keyb);
                        _keyb = null;
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message, e);
                }
            }
        }

        /// <summary>
        /// Reads the current keyboard state
        /// </summary>
        private void ReadKeyboard()
        {
            if (_keyb == null) InitializeKeyboard();
            KeyboardState keystate;

            try
            {
                //Get the current keyboard state
                keystate = _keyb.GetCurrentKeyboardState();

                if (keystate[_twoDPrimaryViewKeyCode]) //if the 2D mode (primary view) hotkey is pressed
                {
                    if (
                        (
                            //check if Control key is down [if control key is part of the required 2D-mode (primary view) hotkey]
                        (
                            (((_twoDPrimaryKeys & Keys.Modifiers) & Keys.Control) != 0) //control key required
                            && (keystate[Key.LeftControl] || keystate[Key.RightControl]) //control key pressed
                        )
                        || //OR
                        (
                            ((_twoDPrimaryKeys & Keys.Modifiers) & Keys.Control) == 0 //control key not required
                        )
                        ) // end of CTRL check 
                        && //AND
                        (
                            //check if Shift key is down [if Shift key is part of the required 2D-mode (primary view) hotkey]
                        (
                            ((_twoDPrimaryKeys & Keys.Modifiers) & Keys.Shift) != 0 //shift key required
                            && (keystate[Key.LeftShift] || keystate[Key.RightShift]) //shift key pressed
                        )
                        || //OR 
                        (
                            ((_twoDPrimaryKeys & Keys.Modifiers) & Keys.Shift) == 0 //shift key not required
                        )
                        ) //end of SHIFT check
                        && //AND
                        (
                            //check if Alt key is down [if Alt key is part of the required 2D-mode (primary view) hotkey]
                        (
                            (((_twoDPrimaryKeys & Keys.Modifiers) & Keys.Alt)) != 0 //ALT key required
                            && (keystate[Key.LeftAlt] || keystate[Key.RightAlt]) //ALT key pressed
                        )
                        || //OR 
                        (
                            ((_twoDPrimaryKeys & Keys.Modifiers) & Keys.Alt) == 0 //ALT key not required
                        )
                        ) //end of ALT check
                        )
                    {
                        //if 2D primary view mode hotkey is pressed, then set the global 3D-mode flag to FALSE (i.e., switch to 2D primary view mode)
                        Extractor.GetInstance().ThreeDeeMode = false;
                        //tell the Extractor to use the primary-view coordiantes
                        Extractor.GetInstance().TwoDeePrimaryView = true;
                    }
                }
                else if (keystate[_twoDSecondaryViewKeyCode]) //if the 2D mode (secondary view) hotkey is pressed
                {
                    if (
                        (
                            //check if Control key is down [if control key is part of the required 2D-mode (secondary view) hotkey]
                        (
                            (((_twoDSecondaryKeys & Keys.Modifiers) & Keys.Control) != 0) //control key required
                            && (keystate[Key.LeftControl] || keystate[Key.RightControl]) //control key pressed
                        )
                        || //OR
                        (
                            ((_twoDSecondaryKeys & Keys.Modifiers) & Keys.Control) == 0 //control key not required
                        )
                        ) // end of CTRL check 
                        && //AND
                        (
                            //check if Shift key is down [if Shift key is part of the required 2D-mode (secondary view) hotkey]
                        (
                            ((_twoDSecondaryKeys & Keys.Modifiers) & Keys.Shift) != 0 //shift key required
                            && (keystate[Key.LeftShift] || keystate[Key.RightShift]) //shift key pressed
                        )
                        || //OR 
                        (
                            ((_twoDSecondaryKeys & Keys.Modifiers) & Keys.Shift) == 0 //shift key not required
                        )
                        ) //end of SHIFT check
                        && //AND
                        (
                            //check if Alt key is down [if Alt key is part of the required 2D-mode (secondary view) hotkey]
                        (
                            (((_twoDSecondaryKeys & Keys.Modifiers) & Keys.Alt)) != 0 //ALT key required
                            && (keystate[Key.LeftAlt] || keystate[Key.RightAlt]) //ALT key pressed
                        )
                        || //OR 
                        (
                            ((_twoDSecondaryKeys & Keys.Modifiers) & Keys.Alt) == 0 //ALT key not required
                        )
                        ) //end of ALT check
                        )
                    {
                        //if 2D secondary view mode hotkey is pressed, then set the global 3D-mode flag to FALSE (i.e., switch to 2D secondary view mode)
                        Extractor.GetInstance().ThreeDeeMode = false;
                        //tell the Extractor to use the secondary-view coordiantes
                        Extractor.GetInstance().TwoDeePrimaryView = false;
                    }
                }
                else if (keystate[_threeDKeyCode]) //if the 3D hotkey is pressed
                {
                    if (
                        (
                            //check if Control key is down [if Control key is part of the required 3D-mode hotkey]
                        (
                            ((_threeDkeys & Keys.Modifiers) & Keys.Control) != 0 //control key required
                            && (keystate[Key.LeftControl] || keystate[Key.RightControl]) //control key pressed
                        )
                        || //OR
                        (
                            (((_threeDkeys & Keys.Modifiers) & Keys.Control)) == 0 //control key not required
                        )
                        ) // end of CTRL check 
                        && //AND
                        (
                            //check if Shift key is down [if Shift key is part of the required 3D-mode hotkey]
                        (
                            ((_threeDkeys & Keys.Modifiers) & Keys.Shift) != 0 //shift key required
                            && (keystate[Key.LeftShift] || keystate[Key.RightShift]) //shift key pressed
                        )
                        || //OR 
                        (
                            ((_threeDkeys & Keys.Modifiers) & Keys.Shift) == 0 //shift key not required
                        )
                        ) //end of SHIFT check
                        && //AND
                        (
                            //check if Alt key is down [if Alt key is part of the required 3D-mode hotkey]
                        (
                            ((_threeDkeys & Keys.Modifiers) & Keys.Alt) != 0 //ALT key required
                            && (keystate[Key.LeftAlt] || keystate[Key.RightAlt]) //ALT key pressed
                        )
                        || //OR 
                        (
                            ((_threeDkeys & Keys.Modifiers) & Keys.Alt) == 0 //ALT key not required
                        )
                        ) //end of ALT check
                        )
                    {
                        //switch the MFD Extractor to 3D mode
                        Extractor.GetInstance().ThreeDeeMode = true;
                    }
                }
            }
            catch (Exception)
            {
                //if any problems are detected while attempting to read the current keyboard state, then 
                //re-attempt to acquire the DirectInput keyboard device; ignore other problems
                InitializeKeyboard();
            }
        }

        /// <summary>
        /// Work method for the keyboard polling thread
        /// </summary>
        private void KeyboardWatcherThreadWork()
        {
            while (!_isClosing)
            {
                _directInputEvent.WaitOne(5000, false);
                try
                {
                    ReadKeyboard();
                        //read the current keyboard state and check if any mode-switching hotkeys have been pressed
                }
                catch (Exception)
                {
                    //prevent this thread from aborting on an unhandled exception
                }
            }
        }

        /// <summary>
        /// Event handler for the Form's Load event
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the Form's Load event</param>
        private void frmMain_Load(object sender, EventArgs e)
        {
            Visible = false; //hide the form, initially
            Width = 0;
            Height = 0;

            //set the form's titlebar equal to the app's name as defined in the app properties file
            Text = Application.ProductName + " v" + Application.ProductVersion;
            nfyTrayIcon.Text = Text;
            //disable checks for illegal cross-thread calls, as there are some code chunks that do
            //directly update the UI thread 
            //TODO: eliminate the need for this "crutch" by properly implementing non-UI thread interaction
            //with the UI thread via calls to .Invoke
            CheckForIllegalCrossThreadCalls = false;

            //set up initial menu item enabled/states for the context menu (tray menu) items
            mnuCtxStart.Enabled = true;
            mnuCtxStop.Enabled = false;

            //load the application's settings from the config file
            Settings settings = Settings.Default;
            if (settings.UpgradeNeeded)
            {
                settings.Upgrade();
                settings.UpgradeNeeded = false;
                settings.Save();
            }
            //configure an instance of the main Extractor engine 
            Extractor extractor = Extractor.GetInstance();
            FormManager.ApplicationForm = this; //register the main form with the Extractor engine so that
            //the Extractor can inform the main form of any updates to
            //the position of visible forms

            //register for Extractor events
            extractor.Stopped += extractor_Stopped;
            extractor.Starting += extractor_Starting;
            extractor.Started += extractor_Started;
            extractor.Stopping += extractor_Stopping;

            //setup the keyboard polling thread if running in server or standalone mode
            if ((NetworkMode) settings.NetworkingMode != NetworkMode.Client)
            {
                SetupHotkeys();
                _keyboardWatcherThread = new Thread(KeyboardWatcherThreadWork);
                _keyboardWatcherThread.SetApartmentState(ApartmentState.STA);
                _keyboardWatcherThread.Priority = ThreadPriority.BelowNormal;
                _keyboardWatcherThread.IsBackground = true;
                _keyboardWatcherThread.Name = "KeyboardWatcherThread";
                _keyboardWatcherThread.Start();
            }
            //if we're supposed to automatically start the Extractor (per the user-config settings), start it
            //now
            if (settings.StartOnLaunch)
            {
                extractor.Start();
            }
        }

        /// <summary>
        /// Read the user-defined mode-switching hotkeys from the user settings file
        /// </summary>
        internal void SetupHotkeys()
        {
            _twoDPrimaryViewKeyCode = KeyConverter.ConvertFrom((Settings.Default.TwoDPrimaryHotkey) & Keys.KeyCode);
            _twoDSecondaryViewKeyCode = KeyConverter.ConvertFrom((Settings.Default.TwoDSecondaryHotkey) & Keys.KeyCode);
            _threeDKeyCode = KeyConverter.ConvertFrom((Settings.Default.ThreeDHotkey) & Keys.KeyCode);
            _twoDPrimaryKeys = Settings.Default.TwoDPrimaryHotkey;
            _twoDSecondaryKeys = Settings.Default.TwoDSecondaryHotkey;
            _threeDkeys = Settings.Default.ThreeDHotkey;
        }

        /// <summary>
        /// Event handler for the Extractor engine's Stopping event
        /// </summary>
        /// <param name="sender">Object which raised this event</param>
        /// <param name="e">Event arguments for the Stopping event</param>
        private void extractor_Stopping(object sender, EventArgs e)
        {
            //while the Extractor is stopping, disable the ability to Start or Stop the Extractor
            //via the tray icon menu
            mnuCtxStart.Enabled = false;
            mnuCtxStop.Enabled = false;
        }

        /// <summary>
        /// Event handler for the Extractor engine's Started event
        /// </summary>
        /// <param name="sender">Object which raised this event</param>
        /// <param name="e">Event arguments for the Started event</param>
        private void extractor_Started(object sender, EventArgs e)
        {
            //when the Extractor engine has fully started, then enable the Stop action item on 
            //the MFD Extractor's system tray icon menu
            mnuCtxStart.Enabled = false;
            mnuCtxStop.Enabled = true;
        }

        /// <summary>
        /// Event handler for the Extractor engine's Starting event
        /// </summary>
        /// <param name="sender">Object which raised this event</param>
        /// <param name="e">Event arguments for the Starting event</param>
        private void extractor_Starting(object sender, EventArgs e)
        {
            //While the Extractor engine is Starting up, disable any Start or Stop actions on the
            //MFD Extractor's system tray icon menu
            mnuCtxStart.Enabled = false;
            mnuCtxStop.Enabled = false;
        }

        /// <summary>
        /// Event handler for the Extractor engine's Stopped event
        /// </summary>
        /// <param name="sender">Object which raised this event</param>
        /// <param name="e">Event arguments for the Stopped event</param>
        private void extractor_Stopped(object sender, EventArgs e)
        {
            //When the Extractor engine has fully stopped, re-enable the Start action menu item on 
            //the MFD Extractor's system tray icon menu
            mnuCtxStart.Enabled = true;
            mnuCtxStop.Enabled = false;
        }

        /// <summary>
        /// Event handler for the Form's Closed event
        /// </summary>
        /// <param name="sender">Object which raised this event</param>
        /// <param name="e">Event arguments for the Form's Closed event</param>
        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            //When the user closes the MFD Extractor's main form (i.e. clicks the Exit action menu item
            //on the system tray icon's menu); then we need to inform the keyboard polling thread to quit,
            //and then unload the main form.  When all active threads have quit, the program will be unloaded
            //from memory.
            _isClosing = true;
            Quit();
        }

        /// <summary>
        /// Perform an orderly shutdown of the MFD Extractor.
        /// </summary>
        private void Quit()
        {
            //Stop the Extractor engine
            Extractor instance = Extractor.GetInstance();
            instance.Stop();
            Common.Util.DisposeObject(instance);

            //remove the applicaton's tray icon from the tray
            nfyTrayIcon.Visible = false;
            //Close the application's main form
            if (!_isClosing)
            {
                Close();
            }
        }

        /// <summary>
        /// Starts the MFD Extractor engine
        /// </summary>
        private static void Start()
        {
            //Start the MFD Extractor engine
            Extractor.GetInstance().Start();
        }

        /// <summary>
        /// Stops the MFD Extractor engine
        /// </summary>
        private static void Stop()
        {
            //Stop the MFD Extractor engine
            Extractor.GetInstance().Stop();
        }

        /// <summary>
        /// Display the application's Options dialog box
        /// </summary>
        private void ShowOptionsDialog()
        {
            if (_optionsFormForm == null)
            {
                Extractor extractor = Extractor.GetInstance(); //get a reference to the main Extractor engine
                mnuCtxExit.Enabled = false; //disable the Exit action on the system tray icon's menu


                //display the application's Options form to the user
                _optionsFormForm = new OptionsForm();
                _optionsFormForm.ShowDialog(this);

                SetupHotkeys(); //reconfigure any global mode-switching hotkeys per any new user settings
                extractor.LoadSettings();
                    //ask the Extractor engine to refresh its settings from the application's user-config file

                //re-enable the Exit action on the system tray icon's menu
                mnuCtxExit.Enabled = true;

                //free the memory used by the reference to the Options form
                _optionsFormForm = null;
            }
            else
            {
                //if the Options form is already being displayed, just bring it to the front
                _optionsFormForm.BringToFront();
            }
        }

        /// <summary>
        /// Event handler for the system tray icon's Options menu item's Click event
        /// </summary>
        /// <param name="sender">Object that raised this event</param>
        /// <param name="e">Event handler for the menu item's Click event</param>
        private void mnuCtxOptions_Click(object sender, EventArgs e)
        {
            //show the Options form to the user
            ShowOptionsDialog();
        }

        /// <summary>
        /// Event handler for the system tray icon's Exit menu item's Click event
        /// </summary>
        /// <param name="sender">Object that raised this event</param>
        /// <param name="e">Event handler for the menu item's Click event</param>
        private void mnuCtxExit_Click(object sender, EventArgs e)
        {
            //Shut down the application
            Quit();
        }

        /// <summary>
        /// Event handler for the system tray icon's Start menu item's Click event
        /// </summary>
        /// <param name="sender">Object that raised this event</param>
        /// <param name="e">Event handler for the menu item's Click event</param>
        private void mnuCtxStart_Click(object sender, EventArgs e)
        {
            //Start the Extractor engine
            Start();
        }

        /// <summary>
        /// Event handler for the system tray icon's Stop menu item's Click event
        /// </summary>
        /// <param name="sender">Object that raised this event</param>
        /// <param name="e">Event arguments for the menu item's Click event</param>
        private void mnuCtxStop_Click(object sender, EventArgs e)
        {
            //Stop the Extractor engine
            Stop();
        }
    }
}