using System;
using System.IO;
using System.Text;
using Common.Win32;
using F4KeyFile;
using log4net;

namespace F4Utils.Process
{
    public class KeyFileUtils
    {
        private const string KEYSTROKE_FILE_NAME__DEFAULT = "BMS.key";
        private const string PLAYER_OPTS_FILENAME__DEFAULT = "viper.pop";
        private const string PLAYER_OPTS_FILE_EXTENSION = ".pop";
        private const string KEYFILE_EXENSION_DEFAULT = ".key";
        private const string CONFIG_DIRECTORY_NAME = "config";
        private const string USEROPTS_DIRECTORY_NAME = "User";

        private static readonly ILog _log = LogManager.GetLogger(typeof (KeyFileUtils));
        private static KeyFile _keyFile;
        private static readonly object _keySenderLock = new object();

        public static void ResetCurrentKeyFile()
        {
            _keyFile = null;
        }

        public static KeyBinding FindKeyBinding(string callback)
        {
            if (_keyFile == null) _keyFile = GetCurrentKeyFile();
            if (_keyFile == null) return null;
           return _keyFile.GetBindingForCallback(callback) as KeyBinding;
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
            
            if (exeFilePath != null)
            {
                var callsign = CallsignUtils.DetectCurrentCallsign();
                var configFolder = string.Empty;

               
                configFolder = Path.GetDirectoryName(exeFilePath) + Path.DirectorySeparatorChar
                    + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + 
                                USEROPTS_DIRECTORY_NAME + Path.DirectorySeparatorChar + CONFIG_DIRECTORY_NAME;
                
                string pilotOptionsPath;
                
                pilotOptionsPath = configFolder + Path.DirectorySeparatorChar + callsign +
                                    PLAYER_OPTS_FILE_EXTENSION;
                if (!new FileInfo(pilotOptionsPath).Exists)
                {
                    pilotOptionsPath = configFolder + Path.DirectorySeparatorChar + PLAYER_OPTS_FILENAME__DEFAULT;
                }
                

                string keyFileName = null;
                if (new FileInfo(pilotOptionsPath).Exists)
                {
                    keyFileName = GetKeyFileNameFromPlayerOpts(pilotOptionsPath);
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
        private static string GetKeyFileNameFromPlayerOpts(string playerOptionsFilePath)
        {
            PlayerOp.PlayerOp playerOptionsFile = null;
            try
            {
                using (var fs = new FileStream(playerOptionsFilePath, FileMode.Open, FileAccess.Read))
                {
                      playerOptionsFile= new PlayerOp.PlayerOp(fs);
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
            if (playerOptionsFile != null && playerOptionsFile.keyfile !=null && playerOptionsFile.keyfile.Length > 0)
            {
                var keyFileName = Encoding.ASCII.GetString(playerOptionsFile.keyfile, 0, playerOptionsFile.keyfile.Length);
                var firstNull = keyFileName.IndexOf('\0');
                if (firstNull > 0)
                {
                    keyFileName = keyFileName.Substring(0, firstNull);
                    keyFileName += KEYFILE_EXENSION_DEFAULT;
                    return keyFileName;
                }
            }
            return null;
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