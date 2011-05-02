using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using F4SharedMem;
using log4net;
using Microsoft.Win32;

namespace F4Utils.Process
{
    public class CallsignUtils
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (CallsignUtils));

        public static string DetectCurrentCallsign()
        {
            string callsign = null;
            var falconDataFormat = Util.DetectFalconFormat();
            var exePath = Util.GetFalconExePath();

            if (falconDataFormat.HasValue && falconDataFormat.Value == FalconDataFormats.AlliedForce)
            {
                callsign = DetectF4AFCallsign(exePath, callsign);
            }
            else if (falconDataFormat.HasValue && falconDataFormat.Value == FalconDataFormats.BMS4)
            {
                callsign = DetectBMSCallsign(exePath, callsign);
            }
            else
            {
                callsign = DetectF4ClassicCallsign(callsign);
            }
            return callsign;
        }

        private static string DetectF4ClassicCallsign(string callsign)
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\MicroProse\Falcon\4.0");
                if (key != null) callsign = Encoding.ASCII.GetString((byte[]) key.GetValue("PilotCallsign"));
                if (callsign != null)
                {
                    var firstNull = callsign.IndexOf('\0');
                    callsign = callsign.Substring(0, firstNull);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                callsign = "Viper";
            }
            return callsign;
        }

        private static string DetectBMSCallsign(string exePath, string callsign)
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Benchmark Sims");
                if (key != null)
                {
                    var subkeys = key.GetSubKeyNames();
                    if (subkeys != null && subkeys.Length > 0)
                    {
                        var callsignFound = false;
                        foreach (var subkey in subkeys)
                        {
                            var toRead = key.OpenSubKey(subkey, false);
                            if (toRead != null)
                            {
                                var baseDir = (string) toRead.GetValue("baseDir", null);
                                var exePathFI = new FileInfo(exePath);
                                var exeDir = exePathFI.Directory.FullName;

                                if (baseDir != null && exeDir != null && (string.Compare(baseDir, exeDir, true) == 0) ||
                                    exeDir.StartsWith(baseDir))
                                {
                                    callsign = Encoding.ASCII.GetString((byte[]) toRead.GetValue("PilotCallsign"));
                                    var firstNull = callsign.IndexOf('\0');
                                    callsign = callsign.Substring(0, firstNull);
                                    callsignFound = true;
                                    break;
                                }
                            }
                        }
                        if (!callsignFound)
                        {
                            callsign = "Viper";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                callsign = "Viper";
            }
            return callsign;
        }

        private static string DetectF4AFCallsign(string exePath, string callsign)
        {
            try
            {
                if (exePath != null)
                {
                    exePath = Path.GetDirectoryName(exePath);
                    var configFolder = exePath + Path.DirectorySeparatorChar + "config";
                    using (
                        var reader =
                            File.OpenText(configFolder + Path.DirectorySeparatorChar + "options.cfg"))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            if (line != null)
                            {
                                line = line.Trim();
                                if (line.StartsWith("gr_PilotCallsign"))
                                {
                                    var equalsLoc = line.IndexOf('=');
                                    if (equalsLoc >= 16)
                                    {
                                        callsign = line.Substring(equalsLoc + 1, line.Length - equalsLoc - 1);
                                        callsign = callsign.Trim();
                                        if (!String.IsNullOrEmpty(callsign))
                                        {
                                            var bytes = new List<byte>();
                                            var byteStrings = Common.Strings.Util.Tokenize(callsign);
                                            var firstByte = true;
                                            foreach (var byteString in byteStrings)
                                            {
                                                if (firstByte)
                                                {
                                                    firstByte = false;
                                                    continue;
                                                }
                                                var thisByte = byte.Parse(byteString, NumberStyles.HexNumber);
                                                bytes.Add(thisByte);
                                            }
                                            var allBytes = bytes.ToArray();
                                            var decoded = Encoding.ASCII.GetString(allBytes);
                                            if (!string.IsNullOrEmpty(decoded))
                                            {
                                                decoded = decoded.Trim();
                                            }
                                            var nullLoc = decoded.IndexOf('\0');
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
            return callsign;
        }
    }
}