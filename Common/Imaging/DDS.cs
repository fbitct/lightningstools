using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using DdsFileTypePlugin;

namespace DdsFileTypePlugin
{
    public class DDSFile
    {
        public DDSHeader Header;

        public void Load(Stream stream)
        {
        }

        public byte[] GetPixelData()
        {
            return new byte[] {};
        }

        public int GetWidth()
        {
            return 0;
        }

        public int GetHeight()
        {
            return 0;
        }

        #region Nested type: DDSHeader

        public struct DDSHeader
        {
            public int PitchOrLinearSize;
        }

        #endregion
    }
}

namespace Common.Imaging
{
    public static class DDS
    {
        public static Bitmap Load(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");
            var fi = new FileInfo(filePath);
            if (!fi.Exists) throw new FileNotFoundException(filePath);
            var file = new DDSFile();
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                file.Load(fs);
            }
            return GetBitmapFromDDSFile(file);
        }

        public static Bitmap GetBitmapFromDDSFileBytes(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");
            var file = new DDSFile();
            using (var ms = new MemoryStream(bytes))
            {
                file.Load(ms);
            }
            return GetBitmapFromDDSFile(file);
        }

        public static Bitmap GetBitmapFromDDSFileStream(Stream s)
        {
            var file = new DDSFile();
            file.Load(s);
            return GetBitmapFromDDSFile(file);
        }

        public static unsafe Bitmap GetBitmapFromDDSFile(DDSFile file)
        {
            int width = file.GetWidth();
            int height = file.GetHeight();
            int stride = file.Header.PitchOrLinearSize;
            var toReturn = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            int curByte = 0;
            BitmapData data = toReturn.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
                                                toReturn.PixelFormat);
            IntPtr scan0 = data.Scan0;
            fixed (byte* pixels = file.GetPixelData())
            {
                unchecked
                {
                    var scan0ptr = (byte*) scan0.ToPointer();

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            *(scan0ptr + curByte) = *(pixels + curByte + 2);
                            *(scan0ptr + curByte + 1) = *(pixels + curByte + 1);
                            *(scan0ptr + curByte + 2) = *(pixels + curByte);
                            *(scan0ptr + curByte + 3) = *(pixels + curByte + 3);
                            curByte += 4;
                        }
                    }
                }
            }
            toReturn.UnlockBits(data);
            return toReturn;
        }
    }
}