using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using log4net;

namespace MFDExtractor
{
    public sealed class BMSSupport
    {
        private static ILog _log = LogManager.GetLogger(typeof(BMSSupport));

        public static void Read3DCoordinatesFromCurrentBmsDatFile(ref Rectangle mfd4_3DImageSourceRectangle,
                                                                   ref Rectangle mfd3_3DImageSourceRectangle,
                                                                   ref Rectangle leftMfd3DImageSourceRectangle,
                                                                   ref Rectangle rightMfd3DImageSourceRectangle,
                                                                   ref Rectangle hud3DImageSourceRectangle)
        {
            FileInfo file = FindBms3DCockpitFile();
            if (file == null)
            {
                return;
            }
            bool isDoubleResolution = IsDoubleResolutionRtt();
            mfd4_3DImageSourceRectangle = new Rectangle();
            mfd3_3DImageSourceRectangle = new Rectangle();
            leftMfd3DImageSourceRectangle = new Rectangle();
            rightMfd3DImageSourceRectangle = new Rectangle();
            hud3DImageSourceRectangle = new Rectangle();

            using (FileStream stream = file.OpenRead())
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string currentLine = reader.ReadLine();
                    if (currentLine.ToLowerInvariant().StartsWith("hud"))
                    {
                        List<string> tokens = Common.Strings.Util.Tokenize(currentLine);
                        if (tokens.Count > 12)
                        {
                            try
                            {
                                hud3DImageSourceRectangle.X = Convert.ToInt32(tokens[10]);
                                hud3DImageSourceRectangle.Y = Convert.ToInt32(tokens[11]);
                                hud3DImageSourceRectangle.Width =
                                    Math.Abs(Convert.ToInt32(tokens[12]) - hud3DImageSourceRectangle.X);

                                hud3DImageSourceRectangle.Height =
                                    Math.Abs(Convert.ToInt32(tokens[13]) - hud3DImageSourceRectangle.Y);
                            }
                            catch (Exception e)
                            {
                                _log.Error(e.Message, e);
                            }
                        }
                    }
                    else if (currentLine.ToLowerInvariant().StartsWith("mfd4"))
                    {
                        List<string> tokens = Common.Strings.Util.Tokenize(currentLine);
                        if (tokens.Count > 12)
                        {
                            try
                            {
                                mfd4_3DImageSourceRectangle.X = Convert.ToInt32(tokens[10]);
                                mfd4_3DImageSourceRectangle.Y = Convert.ToInt32(tokens[11]);
                                mfd4_3DImageSourceRectangle.Width =
                                    Math.Abs(Convert.ToInt32(tokens[12]) - mfd4_3DImageSourceRectangle.X);
                                mfd4_3DImageSourceRectangle.Height =
                                    Math.Abs(Convert.ToInt32(tokens[13]) - mfd4_3DImageSourceRectangle.Y);
                            }
                            catch (Exception e)
                            {
                                _log.Error(e.Message, e);
                            }
                        }
                    }
                    else if (currentLine.ToLowerInvariant().StartsWith("mfd3"))
                    {
                        List<string> tokens = Common.Strings.Util.Tokenize(currentLine);
                        if (tokens.Count > 12)
                        {
                            try
                            {
                                mfd3_3DImageSourceRectangle.X = Convert.ToInt32(tokens[10]);
                                mfd3_3DImageSourceRectangle.Y = Convert.ToInt32(tokens[11]);
                                mfd3_3DImageSourceRectangle.Width =
                                    Math.Abs(Convert.ToInt32(tokens[12]) - mfd3_3DImageSourceRectangle.X);
                                mfd3_3DImageSourceRectangle.Height =
                                    Math.Abs(Convert.ToInt32(tokens[13]) - mfd3_3DImageSourceRectangle.Y);
                            }
                            catch (Exception e)
                            {
                                _log.Error(e.Message, e);
                            }
                        }
                    }
                    else if (currentLine.ToLowerInvariant().StartsWith("mfdleft"))
                    {
                        List<string> tokens = Common.Strings.Util.Tokenize(currentLine);
                        if (tokens.Count > 12)
                        {
                            try
                            {
                                leftMfd3DImageSourceRectangle.X = Convert.ToInt32(tokens[10]);
                                leftMfd3DImageSourceRectangle.Y = Convert.ToInt32(tokens[11]);
                                leftMfd3DImageSourceRectangle.Width =
                                    Math.Abs(Convert.ToInt32(tokens[12]) - leftMfd3DImageSourceRectangle.X);
                                leftMfd3DImageSourceRectangle.Height =
                                    Math.Abs(Convert.ToInt32(tokens[13]) - leftMfd3DImageSourceRectangle.Y);
                            }
                            catch (Exception e)
                            {
                                _log.Error(e.Message, e);
                            }
                        }
                    }
                    else if (currentLine.ToLowerInvariant().StartsWith("mfdright"))
                    {
                        List<string> tokens = Common.Strings.Util.Tokenize(currentLine);
                        if (tokens.Count > 12)
                        {
                            try
                            {
                                rightMfd3DImageSourceRectangle.X = Convert.ToInt32(tokens[10]);
                                rightMfd3DImageSourceRectangle.Y = Convert.ToInt32(tokens[11]);
                                rightMfd3DImageSourceRectangle.Width =
                                    Math.Abs(Convert.ToInt32(tokens[12]) - rightMfd3DImageSourceRectangle.X);
                                rightMfd3DImageSourceRectangle.Height =
                                    Math.Abs(Convert.ToInt32(tokens[13]) - rightMfd3DImageSourceRectangle.Y);
                            }
                            catch (Exception e)
                            {
                                _log.Error(e.Message, e);
                            }
                        }
                    }
                }
            }
            if (isDoubleResolution)
            {
                leftMfd3DImageSourceRectangle = MultiplyRectangle(leftMfd3DImageSourceRectangle, 2);
                rightMfd3DImageSourceRectangle = MultiplyRectangle(rightMfd3DImageSourceRectangle, 2);
                mfd3_3DImageSourceRectangle = MultiplyRectangle(mfd3_3DImageSourceRectangle, 2);
                mfd4_3DImageSourceRectangle = MultiplyRectangle(mfd4_3DImageSourceRectangle, 2);
                hud3DImageSourceRectangle = MultiplyRectangle(hud3DImageSourceRectangle, 2);
            }
        }

        private static Rectangle MultiplyRectangle(Rectangle rect, int factor)
        {
            return new Rectangle(rect.X*factor, rect.Y*factor, rect.Width*factor,
                                 rect.Height*factor);
        }

        private static string RunningBmsInstanceBasePath()
        {
            string toReturn = null;
            string exePath = F4Utils.Process.Util.GetFalconExePath();
            if (!string.IsNullOrEmpty(exePath))
            {
                toReturn = new FileInfo(exePath).Directory.FullName;
            }
            return toReturn;
        }

        private static bool IsDoubleResolutionRtt()
        {
            string bmsPath = RunningBmsInstanceBasePath();
            if (string.IsNullOrEmpty(bmsPath)) return false;
            var file = new FileInfo(Path.Combine(bmsPath, "FalconBMS.cfg"));
            if (!file.Exists)
            {
                file = new FileInfo(Path.Combine(Path.Combine(bmsPath, "config"), "Falcon BMS.cfg"));
                if (!file.Exists)
                {
                    file = new FileInfo(Path.Combine(Path.Combine(bmsPath, @"..\..\User\config"), "Falcon BMS.cfg"));
                }
            }

            if (file.Exists)
            {
                var allLines = new List<string>();
                using (var reader = new StreamReader(file.FullName))
                {
                    while (!reader.EndOfStream)
                    {
                        allLines.Add(reader.ReadLine());
                    }
                    reader.Close();
                }

                for (int i = 0; i < allLines.Count; i++)
                {
                    string currentLine = allLines[i];
                    List<String> tokens = Common.Strings.Util.Tokenize(currentLine);
                    if (tokens.Count > 2)
                    {
                        if (tokens[0].ToLowerInvariant() == "set" &&
                            tokens[1].ToLowerInvariant() == "g_bDoubleRTTResolution".ToLowerInvariant() &&
                            (tokens[2].ToLowerInvariant() == "1".ToLowerInvariant()))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static FileInfo FindBms3DCockpitFile()
        {
            string basePath = RunningBmsInstanceBasePath();
            string path = null;
            if (basePath != null)
            {
                path = basePath + @"\art\ckptartn";
                var dir = new DirectoryInfo(path);
                if (dir.Exists)
                {
                    DirectoryInfo[] subDirs = dir.GetDirectories();
                    FileInfo file = null;
                    foreach (DirectoryInfo thisDir in subDirs)
                    {
                        file = new FileInfo(thisDir.FullName + @"\3dckpit.dat");
                        if (file.Exists)
                        {
                            try
                            {
                                using (
                                    FileStream fs = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite,
                                                              FileShare.None))
                                {
                                    fs.Close();
                                }
                            }
                            catch (IOException)
                            {
                                return file;
                            }
                        }
                    }

                    file = new FileInfo(dir.FullName + @"\3dckpit.dat");
                    if (file.Exists)
                    {
                        try
                        {
                            using (
                                FileStream fs = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite,
                                                          FileShare.None))
                            {
                                fs.Close();
                            }
                        }
                        catch (IOException)
                        {
                            return file;
                        }
                    }
                }

                path = basePath + @"\art\ckptart";
                dir = new DirectoryInfo(path);
                if (dir.Exists)
                {
                    DirectoryInfo[] subDirs = dir.GetDirectories();
                    FileInfo file = null;
                    foreach (DirectoryInfo thisDir in subDirs)
                    {
                        file = new FileInfo(thisDir.FullName + @"\3dckpit.dat");
                        if (file.Exists)
                        {
                            try
                            {
                                using (
                                    FileStream fs = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite,
                                                              FileShare.None))
                                {
                                    fs.Close();
                                }
                            }
                            catch (IOException)
                            {
                                return file;
                            }
                        }
                    }

                    file = new FileInfo(dir.FullName + @"\3dckpit.dat");
                    if (file.Exists)
                    {
                        return file;
                    }
                }


                path = basePath + @"\..\..\Data\art\ckptartn";
                dir = new DirectoryInfo(path);
                if (dir.Exists)
                {
                    DirectoryInfo[] subDirs = dir.GetDirectories();
                    FileInfo file = null;
                    foreach (DirectoryInfo thisDir in subDirs)
                    {
                        file = new FileInfo(thisDir.FullName + @"\3dckpit.dat");
                        if (file.Exists)
                        {
                            try
                            {
                                using (
                                    FileStream fs = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite,
                                                              FileShare.None))
                                {
                                    fs.Close();
                                }
                            }
                            catch (IOException)
                            {
                                return file;
                            }
                        }
                    }

                    file = new FileInfo(dir.FullName + @"\3dckpit.dat");
                    if (file.Exists)
                    {
                        return file;
                    }
                }

                path = basePath + @"\..\..\Data\art\ckptart";
                dir = new DirectoryInfo(path);
                if (dir.Exists)
                {
                    DirectoryInfo[] subDirs = dir.GetDirectories();
                    FileInfo file = null;
                    foreach (DirectoryInfo thisDir in subDirs)
                    {
                        file = new FileInfo(thisDir.FullName + @"\3dckpit.dat");
                        if (file.Exists)
                        {
                            try
                            {
                                using (
                                    FileStream fs = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite,
                                                              FileShare.None))
                                {
                                    fs.Close();
                                }
                            }
                            catch (IOException)
                            {
                                return file;
                            }
                        }
                    }

                    file = new FileInfo(dir.FullName + @"\3dckpit.dat");
                    if (file.Exists)
                    {
                        return file;
                    }
                }
            }
            return null;
        }
    }
}