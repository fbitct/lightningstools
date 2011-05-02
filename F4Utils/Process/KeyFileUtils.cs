using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common.MacroProgramming;
using F4KeyFile;
using F4SharedMem;
using log4net;

namespace F4Utils.Process
{
    public class KeyFileUtils
    {
        private const string KEYSTROKE_FILE_NAME__DEFAULT = "keystrokes.key";
        private const string PLAYER_OPTS_FILENAME__DEFAULT = "viper.pop";
        private const string PLAYER_OPTS_FILE_EXTENSION = ".pop";
        private const string PLAYER_OPTS_V2_FILENAME_DEFAULT = "viper.pop2";
        private const string PLAYER_OPTS_V2_FILE_EXTENSION = ".pop2";
        private const string KEYFILE_EXENSION_DEFAULT = ".key";
        private const string CONFIG_DIRECTORY_NAME = "config";
        private const string USEROPTS_DIRECTORY_NAME = "User";

        private static readonly ILog _log = LogManager.GetLogger(typeof (KeyFileUtils));
        private static Dictionary<string, KeyBinding> _knownCallbacks = new Dictionary<string, KeyBinding>();
        private static KeyFile _keyFile;
        private static readonly object _keySenderLock = new object();

        public static void ResetCurrentKeyFile()
        {
            _keyFile = null;
            _knownCallbacks = new Dictionary<string, KeyBinding>();
        }

        public static KeyBinding FindKeyBinding(string callback)
        {
            if (_keyFile == null) _keyFile = GetCurrentKeyFile();
            if (_keyFile == null) return null;
            if (!_knownCallbacks.ContainsKey(callback))
            {
                var binding = _keyFile.FindBindingForCallback(callback) as KeyBinding;
                if (binding != null)
                {
                    _knownCallbacks.Add(callback, binding);
                }
            }
            if (_knownCallbacks.ContainsKey(callback))
            {
                return _knownCallbacks[callback];
            }
            return null;
        }

        public static void SendCallbackToFalcon(string callback)
        {
            Util.ActivateFalconWindow();
            var binding = FindKeyBinding(callback);
            if (binding != null)
            {
                if (Util.IsFalconWindowForeground())
                {
                    lock (_keySenderLock)
                    {
                        var primaryKeyWithModifiers = binding.Key;
                        var comboKeyWithModifiers = binding.ComboKey;

                        SendClearingKeystrokes();

                        SendUpKeystrokes(primaryKeyWithModifiers);
                        SendDownKeystrokes(primaryKeyWithModifiers);
                        SendUpKeystrokes(primaryKeyWithModifiers);

                        SendUpKeystrokes(comboKeyWithModifiers);
                        SendDownKeystrokes(comboKeyWithModifiers);
                        SendUpKeystrokes(comboKeyWithModifiers);

                        SendClearingKeystrokes();
                    }
                }
            }
        }

        public static KeyFile GetCurrentKeyFile()
        {
            KeyFile toReturn = null;
            var exeFilePath = Util.GetFalconExePath();
            var currentDataFormat = Util.DetectFalconFormat();
            if (exeFilePath != null)
            {
                var callsign = CallsignUtils.DetectCurrentCallsign();
                var configFolder = string.Empty;

                if (currentDataFormat.HasValue && currentDataFormat.Value == FalconDataFormats.BMS4)
                {
                    configFolder = Path.GetDirectoryName(exeFilePath) + Path.DirectorySeparatorChar + ".." +
                                   Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar +
                                   USEROPTS_DIRECTORY_NAME + Path.DirectorySeparatorChar + CONFIG_DIRECTORY_NAME;
                }
                else
                {
                    configFolder = Path.GetDirectoryName(exeFilePath) + Path.DirectorySeparatorChar +
                                   CONFIG_DIRECTORY_NAME;
                }
                string pilotOptionsPath;
                if (currentDataFormat.HasValue && currentDataFormat.Value == FalconDataFormats.AlliedForce)
                {
                    pilotOptionsPath = configFolder + Path.DirectorySeparatorChar + callsign +
                                       PLAYER_OPTS_V2_FILE_EXTENSION;
                    if (!new FileInfo(pilotOptionsPath).Exists)
                    {
                        pilotOptionsPath = configFolder + Path.DirectorySeparatorChar + PLAYER_OPTS_V2_FILENAME_DEFAULT;
                    }
                }
                else
                {
                    pilotOptionsPath = configFolder + Path.DirectorySeparatorChar + callsign +
                                       PLAYER_OPTS_FILE_EXTENSION;
                    if (!new FileInfo(pilotOptionsPath).Exists)
                    {
                        pilotOptionsPath = configFolder + Path.DirectorySeparatorChar + PLAYER_OPTS_FILENAME__DEFAULT;
                    }
                }


                string keyFileName = null;
                if (new FileInfo(pilotOptionsPath).Exists)
                {
                    keyFileName = GetKeyFileNameFromPlayerOptsRaw(currentDataFormat, pilotOptionsPath);
                }
                if (keyFileName == null) keyFileName = KEYSTROKE_FILE_NAME__DEFAULT;

                var falconKeyFilePath = configFolder + Path.DirectorySeparatorChar + keyFileName;
                var keyFileInfo = new FileInfo(falconKeyFilePath);
                if (keyFileInfo.Exists)
                {
                    toReturn = new KeyFile(keyFileInfo);
                    try
                    {
                        toReturn.Load();
                    }
                    catch (IOException e)
                    {
                        _log.Error(e.Message, e);
                    }
                }
            }
            return toReturn;
        }

        private static string GetKeyFileNameFromPlayerOptsRaw(FalconDataFormats? currentDataFormat,
                                                              string playerOptionsFilePath)
        {
            string keyFileName = null;
            try
            {
                var optionsFileContents = File.ReadAllBytes(playerOptionsFilePath);
                var startLoc = 159;
                if (currentDataFormat.HasValue && (currentDataFormat.Value == FalconDataFormats.BMS4))
                {
                    startLoc = 167;
                }
                else if (currentDataFormat.HasValue && currentDataFormat.Value == FalconDataFormats.FreeFalcon5)
                {
                    startLoc = 163;
                }
                if (optionsFileContents.Length > startLoc)
                {
                    var nullLoc = -1;
                    for (var x = startLoc; x < optionsFileContents.Length; x++)
                    {
                        if (optionsFileContents[x] == 0)
                        {
                            nullLoc = x;
                            break;
                        }
                    }
                    if (nullLoc >= 0)
                    {
                        keyFileName = Encoding.ASCII.GetString(optionsFileContents, startLoc + 1,
                                                               nullLoc - (startLoc + 1));
                    }
                }
                if (keyFileName != null) keyFileName += KEYFILE_EXENSION_DEFAULT;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
            return keyFileName;
        }

        private static void SendClearingKeystrokes()
        {
            KeyAndMouseFunctions.SendKey((ushort) ScanCodes.LShift, false, true);
            KeyAndMouseFunctions.SendKey((ushort) ScanCodes.LMenu, false, true);
            KeyAndMouseFunctions.SendKey((ushort) ScanCodes.LControl, false, true);
            KeyAndMouseFunctions.SendKey((ushort) ScanCodes.RShift, false, true);
            KeyAndMouseFunctions.SendKey((ushort) ScanCodes.RMenu, false, true);
            KeyAndMouseFunctions.SendKey((ushort) ScanCodes.RControl, false, true);
        }

        private static void SendDownKeystrokes(KeyWithModifiers keyWithModifiers)
        {
            //send down keystrokes
            if ((keyWithModifiers.Modifiers & KeyModifiers.Shift) == KeyModifiers.Shift)
            {
                KeyAndMouseFunctions.SendKey((ushort) ScanCodes.LShift, true, false);
            }
            if ((keyWithModifiers.Modifiers & KeyModifiers.Alt) == KeyModifiers.Alt)
            {
                KeyAndMouseFunctions.SendKey((ushort) ScanCodes.LMenu, true, false);
            }
            if ((keyWithModifiers.Modifiers & KeyModifiers.Ctrl) == KeyModifiers.Ctrl)
            {
                KeyAndMouseFunctions.SendKey((ushort) ScanCodes.LControl, true, false);
            }
            KeyAndMouseFunctions.SendKey((ushort) keyWithModifiers.ScanCode, true, false);
        }

        private static void SendUpKeystrokes(KeyWithModifiers keyWithModifiers)
        {
            //send up keystrokes
            KeyAndMouseFunctions.SendKey((ushort) keyWithModifiers.ScanCode, false, true);
            if ((keyWithModifiers.Modifiers & KeyModifiers.Ctrl) == KeyModifiers.Ctrl)
            {
                KeyAndMouseFunctions.SendKey((ushort) ScanCodes.LControl, false, true);
            }
            if ((keyWithModifiers.Modifiers & KeyModifiers.Alt) == KeyModifiers.Alt)
            {
                KeyAndMouseFunctions.SendKey((ushort) ScanCodes.LMenu, false, true);
            }
            if ((keyWithModifiers.Modifiers & KeyModifiers.Shift) == KeyModifiers.Shift)
            {
                KeyAndMouseFunctions.SendKey((ushort) ScanCodes.LShift, false, true);
            }
        }
    }
}