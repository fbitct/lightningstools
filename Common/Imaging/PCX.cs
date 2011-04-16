﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Common.Imaging.Internal;

namespace Common.Imaging
{
    public static class PCX
    {
        public static Bitmap LoadFromFile(string filepath)
        {
            var fi = new FileInfo(filepath);
            if (!fi.Exists) throw new FileNotFoundException(filepath);
            byte[] bytes = null;
            using (var fs = new FileStream(filepath, FileMode.Open))
            {
                bytes = new byte[fi.Length];
                fs.Read(bytes, 0, (int) fi.Length);
                fs.Close();
            }
            return LoadFromBytes(bytes);
        }

        public static unsafe Bitmap LoadFromBytes(byte[] pcxBytes)
        {
            var header = new PCXHead();

            long fileSize = pcxBytes.Length;

            // Get the header
            byte curByte = 0;

            header.ID = pcxBytes[curByte];
            curByte++;
            header.Version = pcxBytes[curByte];
            curByte++;
            header.Encoding = pcxBytes[curByte];
            curByte++;
            header.BitPerPixel = pcxBytes[curByte];
            curByte++;
            header.X1 = BitConverter.ToInt16(pcxBytes, curByte);
            curByte += 2;
            header.Y1 = BitConverter.ToInt16(pcxBytes, curByte);
            curByte += 2;
            header.X2 = BitConverter.ToInt16(pcxBytes, curByte);
            curByte += 2;
            header.Y2 = BitConverter.ToInt16(pcxBytes, curByte);
            curByte += 2;
            header.HRes = BitConverter.ToInt16(pcxBytes, curByte);
            curByte += 2;
            header.VRes = BitConverter.ToInt16(pcxBytes, curByte);
            curByte += 2;
            header.ClrMap = new byte[16*3];
            for (int i = 0; i < (16*3); i++)
            {
                header.ClrMap[i] = pcxBytes[curByte];
                curByte++;
            }
            header.Reserved1 = pcxBytes[curByte];
            curByte++;
            header.NumPlanes = pcxBytes[curByte];
            curByte++;
            header.BPL = BitConverter.ToInt16(pcxBytes, curByte);
            curByte += 2;
            header.Pal_t = BitConverter.ToInt16(pcxBytes, curByte);
            curByte += 2;
            header.Filler = new byte[58];
            for (int i = 0; i < 58; i++)
            {
                header.Filler[i] = pcxBytes[curByte];
                curByte++;
            }

            // Each scan line MUST have a size that can be divided by a 'long' data type
            int scanLineSize = header.NumPlanes*header.BPL;
            int divResult = scanLineSize%4;
            if (divResult > 0) scanLineSize = ((scanLineSize/4) + 1)*4;

            // Set the bitmap size data member
            var BitmapSize = new Size(header.X2 - header.X1 + 1, header.Y2 - header.Y1 + 1);
            long imageSize = scanLineSize*BitmapSize.Height;

            // Prepare a buffer large enough to hold the image
            var rawBitmap = new byte[imageSize];

            // Get the compressed image
            long dataPos = 0;
            long pos = 128; // That's where the data begins

            for (int y = 0; y < BitmapSize.Height; y++)
            {
                int x = 0;
                // Decompress the scan line
                for (; x < header.BPL;)
                {
                    uint value = pcxBytes[pos++];
                    if (value > 192)
                    {
                        // Two high bits are set = Repeat
                        value -= 192; // Repeat how many times?
                        byte Color = pcxBytes[pos++]; // What color?

                        if (x <= BitmapSize.Width)
                        {
                            // Image data.  Place in the raw bitmap.
                            for (byte bRepeat = 0; bRepeat < value; bRepeat++)
                            {
                                rawBitmap[dataPos++] = Color;
                                x++;
                            }
                        }
                        else
                            x += (int) value; // Outside the image.  Skip.
                    }
                    else
                    {
                        if (x <= BitmapSize.Width)
                            rawBitmap[dataPos++] = (byte) value;
                        x++;
                    }
                }

                // Pad the rest with zeros
                if (x < scanLineSize)
                {
                    for (; x < scanLineSize; x++)
                        rawBitmap[dataPos++] = 0;
                }
            }

            var toReturn = new Bitmap(BitmapSize.Width, BitmapSize.Height, PixelFormat.Format8bppIndexed);

            if (header.Version == 5 && pcxBytes.Length > 769)
            {
                pos = pcxBytes.Length - 769;
                // Get the palette
                ColorPalette toReturnPalette = toReturn.Palette;
                if (pcxBytes[pos++] == 12)
                {
                    for (int index = 0; index < 256; index++)
                    {
                        int r = pcxBytes[pos++];
                        int g = pcxBytes[pos++];
                        int b = pcxBytes[pos++];
                        Color thisEntry = Color.FromArgb(r, g, b);
                        toReturnPalette.Entries[index] = thisEntry;
                    }
                }
                else
                {
                    //TODO: do something here
                    throw new InvalidDataException();
                }
                toReturn.Palette = toReturnPalette;
            }

            BitmapData lockData = toReturn.LockBits(new Rectangle(0, 0, toReturn.Width, toReturn.Height),
                                                    ImageLockMode.WriteOnly, toReturn.PixelFormat);
            IntPtr start = lockData.Scan0;
            void* startPointer = start.ToPointer();
            for (int i = 0; i < rawBitmap.Length; i++)
            {
                byte thisPixelPalleteEntry = rawBitmap[i];
                *((byte*) startPointer + i) = thisPixelPalleteEntry;
            }
            toReturn.UnlockBits(lockData);
            return toReturn;
        }
    }
}