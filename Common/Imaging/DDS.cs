using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace Common.Imaging
{
    public static class DDS
    {
        public unsafe static Bitmap Load(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");
            FileInfo fi = new FileInfo(filePath);
            if (!fi.Exists) throw new FileNotFoundException(filePath);
            DdsFileTypePlugin.DdsFile file = new DdsFileTypePlugin.DdsFile();
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                file.Load(fs);
            }
            return GetBitmapFromDDSFile(file);
        }
        public unsafe static Bitmap GetBitmapFromDDSFileBytes(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");
            DdsFileTypePlugin.DdsFile file = new DdsFileTypePlugin.DdsFile();
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                file.Load(ms);
            }
            return GetBitmapFromDDSFile(file);
        }
        public unsafe static Bitmap GetBitmapFromDDSFileStream(Stream s)
        {
            DdsFileTypePlugin.DdsFile file = new DdsFileTypePlugin.DdsFile();
            file.Load(s);
            return GetBitmapFromDDSFile(file);
        }
        public unsafe static Bitmap GetBitmapFromDDSFile(DdsFileTypePlugin.DdsFile file)
        {
            int width = file.GetWidth();
            int height = file.GetHeight();
            int stride = (int)file.m_header.m_pitchOrLinearSize;
            Bitmap toReturn = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            int curByte = 0;
            BitmapData data = toReturn.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, toReturn.PixelFormat);
            IntPtr scan0 = data.Scan0;
            fixed (byte* pixels = file.GetPixelData())
            {
                unchecked
                {
                    byte* scan0ptr = (byte*)scan0.ToPointer();

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
