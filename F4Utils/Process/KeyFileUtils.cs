using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Common.Win32;
using F4KeyFile;
using log4net;

namespace F4Utils.Process
{
    public static class KeyFileUtils
    {
        private const string KEYSTROKE_FILE_NAME__DEFAULT = "BMS - Full.key";
        private const string PLAYER_OPTS_FILENAME__DEFAULT = "viper.pop";
        private const string PLAYER_OPTS_FILE_EXTENSION = ".pop";
        private const string KEYFILE_EXENSION_DEFAULT = ".key";
        private const string CONFIG_DIRECTORY_NAME = "config";
        private const string USEROPTS_DIRECTORY_NAME = "User";
        private const int KEY_DELAY_MILLISECONDS = 1;
        private static readonly ILog Log = LogManager.GetLogger(typeof (KeyFileUtils));
        private static KeyFile _keyFile;
        private static readonly object KeySenderLock = new object();

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
            Debug.WriteLine(string.Format("Sending {0} callback to falcon", callback));
            Util.ActivateFalconWindow();
            var binding = FindKeyBinding(callback);
            if (binding == null) return;
            lock (KeySenderLock)
            {
                var primaryKeyWithModifiers = binding.Key;
                var comboKeyWithModifiers = binding.ComboKey;

                SendClearingKeystrokes();
                WaitToSendNextKeystrokes();

                SendUpKeystrokes(primaryKeyWithModifiers);
                WaitToSendNextKeystrokes();
                SendDownKeystrokes(primaryKeyWithModifiers);
                WaitToSendNextKeystrokes();
                SendUpKeystrokes(primaryKeyWithModifiers);
                WaitToSendNextKeystrokes();

                SendUpKeystrokes(comboKeyWithModifiers);
                WaitToSendNextKeystrokes();
                SendDownKeystrokes(comboKeyWithModifiers);
                WaitToSendNextKeystrokes();
                SendUpKeystrokes(comboKeyWithModifiers);
                WaitToSendNextKeystrokes();

                SendClearingKeystrokes();
                WaitToSendNextKeystrokes();
            }
        }

        private static void WaitToSendNextKeystrokes()
        {
            Thread.Sleep(KEY_DELAY_MILLISECONDS);
        }

        public static KeyFile GetCurrentKeyFile()
        {
            KeyFile toReturn = null;
            var exeFilePath = Util.GetFalconExePath();

            if (exeFilePath == null) return null;
            var callsign = CallsignUtils.DetectCurrentCallsign();


            var configFolder = Path.GetDirectoryName(exeFilePath) + Path.DirectorySeparatorChar
                               + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + 
                               USEROPTS_DIRECTORY_NAME + Path.DirectorySeparatorChar + CONFIG_DIRECTORY_NAME;

            var pilotOptionsPath = configFolder + Path.DirectorySeparatorChar + callsign +
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
            if (!keyFileInfo.Exists) return null;
            try
            {
                toReturn = KeyFile.Load(falconKeyFilePath);
            }
            catch (IOException e)
            {
                Log.Error(e.Message, e);
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
                Log.Error(e.Message, e);
            }
            if (playerOptionsFile == null || playerOptionsFile.keyfile == null || playerOptionsFile.keyfile.Length <= 0)
                return null;
            var keyFileName = Encoding.ASCII.GetString(playerOptionsFile.keyfile, 0, playerOptionsFile.keyfile.Length);
            var firstNull = keyFileName.IndexOf('\0');
            if (firstNull <= 0) return null;
            keyFileName = keyFileName.Substring(0, firstNull);
            keyFileName += KEYFILE_EXENSION_DEFAULT;
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