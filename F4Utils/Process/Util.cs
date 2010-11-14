﻿using System;
using System.Collections.Generic;

using System.Text;
using Common.Win32;
using System.IO;
using Microsoft.Win32;
using F4SharedMem;
using F4KeyFile;
using System.Diagnostics;
using log4net;

namespace F4Utils.Process
{
    public static class Util
    {
        private static ILog _log = LogManager.GetLogger(typeof(F4Utils.Process.Util));
        private static KeyFile _keyFile = null;
        private static Dictionary<string, KeyBinding> _knownCallbacks = new Dictionary<string, KeyBinding>();
        private static object _keySenderLock = new object();
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
                KeyBinding binding = _keyFile.FindBindingForCallback(callback) as KeyBinding;
                if (binding != null)
                {
                    _knownCallbacks.Add(callback, binding);
                }
            }
            if (_knownCallbacks.ContainsKey(callback))
            {
                return _knownCallbacks[callback];
            }
            else
            {
                return null;
            }
        }   
        public static void SendCallbackToFalcon(string callback)
        {
            F4Utils.Process.Util.ActivateFalconWindow();
            KeyBinding binding = FindKeyBinding(callback);
            if (binding != null)
            {
                if (F4Utils.Process.Util.IsFalconWindowForeground())
                {
                    lock (_keySenderLock) 
                    {
                        KeyWithModifiers primaryKeyWithModifiers = binding.Key;
                        KeyWithModifiers comboKeyWithModifiers = binding.ComboKey;
                        
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
        private static void SendClearingKeystrokes()
        {
            Common.MacroProgramming.KeyAndMouseFunctions.SendKey((ushort)ScanCodes.LShift, false, true);
            Common.MacroProgramming.KeyAndMouseFunctions.SendKey((ushort)ScanCodes.LMenu, false, true);
            Common.MacroProgramming.KeyAndMouseFunctions.SendKey((ushort)ScanCodes.LControl, false, true);
            Common.MacroProgramming.KeyAndMouseFunctions.SendKey((ushort)ScanCodes.RShift, false, true);
            Common.MacroProgramming.KeyAndMouseFunctions.SendKey((ushort)ScanCodes.RMenu, false, true);
            Common.MacroProgramming.KeyAndMouseFunctions.SendKey((ushort)ScanCodes.RControl, false, true);
        }
        private static void SendDownKeystrokes(KeyWithModifiers keyWithModifiers)
        {
            //send down keystrokes
            if ((keyWithModifiers.Modifiers & KeyModifiers.Shift) == KeyModifiers.Shift)
            {
                Common.MacroProgramming.KeyAndMouseFunctions.SendKey((ushort)ScanCodes.LShift, true, false);
            }
            if ((keyWithModifiers.Modifiers & KeyModifiers.Alt) == KeyModifiers.Alt)
            {
                Common.MacroProgramming.KeyAndMouseFunctions.SendKey((ushort)ScanCodes.LMenu, true, false);
            }
            if ((keyWithModifiers.Modifiers & KeyModifiers.Ctrl) == KeyModifiers.Ctrl)
            {
                Common.MacroProgramming.KeyAndMouseFunctions.SendKey((ushort)ScanCodes.LControl, true, false);
            }
            Common.MacroProgramming.KeyAndMouseFunctions.SendKey((ushort)keyWithModifiers.ScanCode, true, false);
        }

        private static void SendUpKeystrokes(KeyWithModifiers keyWithModifiers)
        {
            //send up keystrokes
            Common.MacroProgramming.KeyAndMouseFunctions.SendKey((ushort)keyWithModifiers.ScanCode, false, true);
            if ((keyWithModifiers.Modifiers & KeyModifiers.Ctrl) == KeyModifiers.Ctrl)
            {
                Common.MacroProgramming.KeyAndMouseFunctions.SendKey((ushort)ScanCodes.LControl, false, true);
            }
            if ((keyWithModifiers.Modifiers & KeyModifiers.Alt) == KeyModifiers.Alt)
            {
                Common.MacroProgramming.KeyAndMouseFunctions.SendKey((ushort)ScanCodes.LMenu, false, true);
            }
            if ((keyWithModifiers.Modifiers & KeyModifiers.Shift) == KeyModifiers.Shift)
            {
                Common.MacroProgramming.KeyAndMouseFunctions.SendKey((ushort)ScanCodes.LShift, false, true);
            }
        }
        public static void ActivateFalconWindow()
        {
            if (!IsFalconWindowForeground())
            {
                IntPtr windowHandle = GetFalconWindowHandle();
                if (windowHandle != IntPtr.Zero)
                {
                    NativeMethods.SetForegroundWindow(windowHandle);
                    System.Threading.Thread.SpinWait(20);
                }
            }
        }

        public static bool IsFalconWindowForeground()
        {
            IntPtr windowHandle = GetFalconWindowHandle();
            if (windowHandle != IntPtr.Zero)
            {
                IntPtr foregroundWindowHandle = NativeMethods.GetForegroundWindow();
                return foregroundWindowHandle == windowHandle;
            }
            return false;

        }
        public static string GetFalconWindowTitle()
        {
            StringBuilder txt = new StringBuilder(NativeMethods.MAX_PATH);
            IntPtr windowHandle = GetFalconWindowHandle();
            string toReturn = null;
            if (windowHandle != IntPtr.Zero)
            {
                NativeMethods.GetWindowText(windowHandle, txt, NativeMethods.MAX_PATH);
                toReturn = txt.ToString();
            }
            return toReturn;
        }
        public static bool IsFalconRunning()
        {
            IntPtr windowHandle = GetFalconWindowHandle();
            return (windowHandle != IntPtr.Zero);
        }
        public static string GetFalconExePath()
        {
            IntPtr windowHandle = GetFalconWindowHandle();
            string toReturn = null;
            if (windowHandle != IntPtr.Zero)
            {
                int procId = 0;
                NativeMethods.GetWindowThreadProcessId(windowHandle, out procId);
                System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(procId);
                toReturn = process.MainModule.FileName;
            }
            return toReturn;
        }
        public static IntPtr GetFalconWindowHandle()
        {
            IntPtr hWnd = NativeMethods.FindWindow("FalconDisplay", null);
            return hWnd;
        }
        public static string DetectCurrentCallsign()
        {
            string callsign = null;
            FalconDataFormats? falconDataFormat = DetectFalconFormat();
            string exePath = GetFalconExePath();
            FileVersionInfo verInfo = null;
            if (exePath !=null) verInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath);

            if (falconDataFormat.HasValue && falconDataFormat.Value == FalconDataFormats.AlliedForce)
            {
                try
                {
                    string configFolder = null;
                    if (exePath != null)
                    {
                        exePath = Path.GetDirectoryName(exePath);
                        configFolder = exePath + Path.DirectorySeparatorChar + "config";
                        using (StreamReader reader = File.OpenText(configFolder + Path.DirectorySeparatorChar + "options.cfg"))
                        {
                            while (!reader.EndOfStream)
                            {
                                string line = reader.ReadLine();
                                if (line != null)
                                {
                                    line = line.Trim();
                                    if (line.StartsWith("gr_PilotCallsign"))
                                    {
                                        int equalsLoc = line.IndexOf('=');
                                        if (equalsLoc >= 16)
                                        {
                                            callsign = line.Substring(equalsLoc + 1, line.Length - equalsLoc - 1);
                                            if (callsign != null) callsign = callsign.Trim();
                                            if (!String.IsNullOrEmpty(callsign))
                                            {
                                                List<Byte> bytes = new List<byte>();
                                                List<string> byteStrings = Common.Strings.Util.Tokenize(callsign);
                                                bool firstByte = true;
                                                foreach (string byteString in byteStrings)
                                                {
                                                    if (firstByte)
                                                    {
                                                        firstByte = false;
                                                        continue;
                                                    }
                                                    byte thisByte = byte.Parse(byteString, System.Globalization.NumberStyles.HexNumber);
                                                    bytes.Add(thisByte);
                                                }
                                                byte[] allBytes = bytes.ToArray();
                                                string decoded = Encoding.ASCII.GetString(allBytes);
                                                if (!string.IsNullOrEmpty(decoded))
                                                {
                                                    decoded = decoded.Trim();
                                                 }
                                                int nullLoc = decoded.IndexOf('\0');
                                                if (nullLoc >= 0)
                                                {
                                                    decoded = decoded.Substring(0, nullLoc);
                                                    callsign = decoded;
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);
                    callsign = "Viper";
                }
            }
            else if (falconDataFormat.HasValue && falconDataFormat.Value == FalconDataFormats.BMS4 && verInfo !=null && ((verInfo.ProductMajorPart == 4 && verInfo.ProductMinorPart >=6826)|| (verInfo.ProductMajorPart > 4)))
            {
                try
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Benchmark Sims");
                    string[] subkeys =key.GetSubKeyNames();
                    if (subkeys != null && subkeys.Length > 0)
                    {
                        bool callsignFound = false; 
                        foreach (string subkey in subkeys)
                        {
                            RegistryKey toRead = key.OpenSubKey(subkey, false);
                            string baseDir = (string)toRead.GetValue("baseDir", null);
                            FileInfo exePathFI = new FileInfo(exePath);
                            string exeDir = exePathFI.Directory.FullName;
                            if (baseDir != null && string.Compare(baseDir, exeDir, true) == 0)
                            {
                                callsign = Encoding.ASCII.GetString((byte[])toRead.GetValue("PilotCallsign"));
                                int firstNull = callsign.IndexOf('\0');
                                callsign = callsign.Substring(0, firstNull);
                                callsignFound = true;
                                break;
                            }
                        }
                        if (!callsignFound)
                        {
                            callsign = "Viper";
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);
                    callsign = "Viper";
                }

            }
            else
            {
                try
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\MicroProse\Falcon\4.0");
                    callsign = Encoding.ASCII.GetString((byte[])key.GetValue("PilotCallsign"));
                    int firstNull = callsign.IndexOf('\0');
                    callsign = callsign.Substring(0, firstNull);
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);
                    callsign = "Viper";
                }
            }
            return callsign;
        }
        public static FalconDataFormats? DetectFalconFormat()
        {
            FalconDataFormats? toReturn = null;
            string exePath = GetFalconExePath();
            if (exePath != null)
            {
                if (exePath.ToLowerInvariant().Contains("FalconAF.exe".ToLowerInvariant()))
                {
                    toReturn = FalconDataFormats.AlliedForce;
                }
                else if (exePath.ToLowerInvariant().Contains("RedViper.exe".ToLowerInvariant()))
                {
                    toReturn = FalconDataFormats.BMS2;
                }
                else if (exePath.ToLowerInvariant().Contains("FFViper.exe".ToLowerInvariant()))
                {
                    toReturn = FalconDataFormats.FreeFalcon5;
                }
                else if (exePath.ToLowerInvariant().Contains("F4-BMS.exe".ToLowerInvariant()))
                {
                    toReturn = FalconDataFormats.BMS3;
                    try
                    {
                        FileVersionInfo verInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath);

                        if (verInfo.ProductMajorPart >= 4)
                        {
                            toReturn = FalconDataFormats.BMS4;
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error(e.Message, e);
                    }
                }
                else if (exePath.ToLowerInvariant().Contains("Falcon BMS.exe".ToLowerInvariant()))
                {
                    toReturn = FalconDataFormats.BMS4;
                }
            }
            return toReturn;
        }
        public static F4KeyFile.KeyFile GetCurrentKeyFile()
        {
            F4KeyFile.KeyFile toReturn = null;
            string exeFilePath = GetFalconExePath();
            F4SharedMem.FalconDataFormats? currentDataFormat = DetectFalconFormat();
            if (exeFilePath != null)
            {
                string callsign = DetectCurrentCallsign();
                string configFolder = Path.GetDirectoryName(exeFilePath) + Path.DirectorySeparatorChar + "config";
                string pilotOptionsPath = null;
                if (currentDataFormat.HasValue && currentDataFormat.Value == FalconDataFormats.AlliedForce)
                {
                    pilotOptionsPath = configFolder + Path.DirectorySeparatorChar + callsign + ".pop2";
                    if (!new FileInfo(pilotOptionsPath).Exists)
                    {
                        pilotOptionsPath = configFolder + Path.DirectorySeparatorChar + "viper.pop2";
                    }
                }
                else
                {
                    pilotOptionsPath = configFolder + Path.DirectorySeparatorChar + callsign + ".pop";
                    if (!new FileInfo(pilotOptionsPath).Exists)
                    {
                        pilotOptionsPath = configFolder + Path.DirectorySeparatorChar + "viper.pop";
                    }
                }


                string keyFileName = null;
                if (new FileInfo(pilotOptionsPath).Exists)
                {
                    try
                    {
                        byte[] optionsFileContents = File.ReadAllBytes(pilotOptionsPath);
                        int startLoc = 159;
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
                            int nullLoc = -1;
                            for (int x = startLoc; x < optionsFileContents.Length; x++)
                            {
                                if (optionsFileContents[x] == 0)
                                {
                                    nullLoc = x;
                                    break;
                                }
                            }
                            if (nullLoc >= 0)
                            {
                                keyFileName = Encoding.ASCII.GetString(optionsFileContents, startLoc+1, nullLoc - (startLoc+1));
                            }
                        }
                        if (keyFileName != null) keyFileName += ".key";
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                    }
                }
                if (keyFileName == null) keyFileName = "keystrokes.key";

                string falconKeyFilePath = configFolder + Path.DirectorySeparatorChar + keyFileName;
                FileInfo keyFileInfo = new System.IO.FileInfo(falconKeyFilePath);
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
    }
}
