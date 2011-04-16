using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Common.Win32;
using log4net;

namespace Common.Imaging
{
    public static class Util
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (Util));

        /// <summary>
        /// Given an Image object, returns a greyscale equivalent Image object
        /// </summary>
        /// <param name="source">a Bitmap object to conver to greyscale</param>
        /// <returns>an Image object based on the input Bitmap, but converted to greyscale</returns>
        public static Image ConvertImageToGreyscale(Bitmap source)
        {
            var bitmap = new Bitmap(source.Width, source.Height);
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color c = source.GetPixel(x, y);
                    var luma = (int) (c.R*0.3 + c.G*0.59 + c.B*0.11);
                    bitmap.SetPixel(x, y, Color.FromArgb(luma, luma, luma));
                }
            }
            return bitmap;
        }

        public static Image LoadBitmapFromFile(string filename)
        {
            Image temp = Image.FromFile(filename);
            ConvertPixelFormat(ref temp, PixelFormat.Format32bppPArgb);
            return temp;
        }

        public static Icon IconFromBitmap(Bitmap bitmap)
        {
            if (bitmap == null) return null;

            Bitmap toIconify = null;
            toIconify = bitmap;
            Icon toReturn = null;
            IntPtr hIcon = IntPtr.Zero;
            hIcon = toIconify.GetHicon();
            Icon temp = Icon.FromHandle(hIcon);
            using (var ms = new MemoryStream())
            {
                temp.Save(ms);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                toReturn = new Icon(ms);
            }
            NativeMethods.DestroyIcon(hIcon);
            return toReturn;
        }

        /// <summary>
        /// Convert's a <see cref="Bitmap"/> to a different pixel format
        /// </summary>
        /// <param name="img">a <see cref="Bitmap"/> Bitmap to convert</param>
        public static void ConvertPixelFormat(ref Image img, PixelFormat format)
        {
            if (img == null) return;
            bool areSame = false;
            try
            {
                if (format == img.PixelFormat)
                {
                    areSame = true;
                }
            }
            catch (Exception e)
            {
                _log.Debug(e.Message, e);
            }
            if (areSame) return;

            Image originalImg = img;
            Image converted = null;
            Graphics graphics = null;
            bool success = false;
            try
            {
                converted = new Bitmap(img.Width, img.Height, format);
                using (graphics = Graphics.FromImage(converted))
                {
                    graphics.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, converted.Width,
                                       converted.Height, GraphicsUnit.Pixel, new ImageAttributes());
                }
                Interlocked.Exchange(ref img, converted);
                success = true;
            }
            catch (Exception e)
            {
                _log.Debug(e.Message, e);
                Common.Util.DisposeObject(converted);
                converted = null;
            }
            finally
            {
                if (success)
                {
                    Common.Util.DisposeObject(originalImg);
                    originalImg = null;
                }
            }
        }

        public static Image BitmapFromBytes(byte[] bitmapBytes)
        {
            Image toReturn = null;
            if (bitmapBytes != null && bitmapBytes.Length > 0)
            {
                using (var ms = new MemoryStream(bitmapBytes))
                {
                    toReturn = Image.FromStream(ms);
                }
            }
            return toReturn;
        }

        public static byte[] BytesFromBitmap(Image image, String compressionType, String imageFormat)
        {
            byte[] toReturn = null;
            if (image != null)
            {
                try
                {
                    int x = image.Width;
                }
                catch (Exception e)
                {
                    _log.Debug(e.Message, e);
                    return null;
                }
                using (var ms = new MemoryStream())
                {
                    int encoderValue = -1;
                    string codecMimeType = "image/tiff";
                    ImageFormat format = ImageFormat.Tiff;
                    switch (compressionType)
                    {
                        case "LZW":
                            encoderValue = (int) EncoderValue.CompressionLZW;
                            break;
                        case "RLE":
                            encoderValue = (int) EncoderValue.CompressionRle;
                            break;
                        default:
                            encoderValue = (int) EncoderValue.CompressionNone;
                            break;
                    }
                    switch (imageFormat)
                    {
                        case "BMP":
                            format = ImageFormat.Bmp;
                            codecMimeType = "image/bmp";
                            break;
                        case "GIF":
                            format = ImageFormat.Gif;
                            codecMimeType = "image/gif";
                            break;
                        case "JPEG":
                            format = ImageFormat.Jpeg;
                            codecMimeType = "image/jpeg";
                            break;
                        case "PNG":
                            format = ImageFormat.Png;
                            codecMimeType = "image/png";
                            break;
                        default:
                            format = ImageFormat.Tiff;
                            codecMimeType = "image/tiff";
                            break;
                    }

                    Encoder encoder = Encoder.Compression;
                    var codecParams = new EncoderParameters(1);
                    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo codecToUse = null;
                    codecParams.Param[0] = new EncoderParameter(encoder, encoderValue);
                    foreach (ImageCodecInfo codec in codecs)
                    {
                        if (codec.MimeType == codecMimeType)
                        {
                            codecToUse = codec;
                            break;
                        }
                    }
                    try
                    {
                        image.Save(ms, codecToUse, codecParams);
                        toReturn = ms.ToArray();
                    }
                    catch (Exception e)
                    {
                        _log.Debug(e.Message, e);
                    }
                }
            }
            return toReturn;
        }

        public static Image RotateBitmap(Image image, float angle)
        {
            angle = -angle;
            //create a new empty bitmap to hold rotated Bitmap
            Image returnBitmap = new Bitmap(System.Math.Max(image.Width, image.Height),
                                            System.Math.Max(image.Width, image.Height), image.PixelFormat);
            //make a graphics object from the empty bitmap
            Graphics g = Graphics.FromImage(returnBitmap);
            //move rotation point to center of Bitmap
            g.TranslateTransform((float) image.Width/2, (float) image.Height/2);
            //rotate
            g.RotateTransform(angle);
            //move Bitmap back
            g.TranslateTransform(-(float) image.Width/2, -(float) image.Height/2);
            //draw passed in Bitmap onto graphics object
            g.DrawImage(image, new Point(0, 0));
            return returnBitmap;
        }

        public static Image CropBitmap(Image img, Rectangle cropArea)
        {
            Image bmpCrop = ((Bitmap) img).Clone(cropArea,
                                                 img.PixelFormat);
            return bmpCrop;
        }

        public static Image ResizeBitmap(Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = (size.Width/(float) sourceWidth);
            nPercentH = (size.Height/(float) sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            var destWidth = (int) (sourceWidth*nPercent);
            var destHeight = (int) (sourceHeight*nPercent);

            var bitmap = new Bitmap(destWidth, destHeight, imgToResize.PixelFormat);
            Graphics g = Graphics.FromImage(bitmap);

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return bitmap;
        }

        public static Image CopyBitmap(Image toCopy)
        {
            if (toCopy == null) return null;
            Image toReturn = new Bitmap(toCopy.Width, toCopy.Height, toCopy.PixelFormat);
            using (Graphics g = Graphics.FromImage(toReturn))
            {
                g.DrawImageUnscaled(toCopy, 0, 0, toCopy.Width, toCopy.Height);
            }
            return toReturn;
        }

        public static Image CloneBitmap(Image image)
        {
            if (image == null)
            {
                return null;
            }
            else
            {
                Image toReturn = null;
                try
                {
                    toReturn = (Image) image.Clone();
                }
                catch (Exception e)
                {
                    _log.Debug(e.Message, e);
                    toReturn = null;
                }
                return toReturn;
            }
        }

        public static Image GetDimmerImage(Image toProcess, float percentLuminanceRetained)
        {
            if (toProcess == null) return null;
            Image toReturn = new Bitmap(toProcess.Width, toProcess.Height);
            var ia = new ImageAttributes();
            ColorMatrix cm = GetDimmingColorMatrix(percentLuminanceRetained);
            ia.SetColorMatrix(cm);

            using (Graphics g = Graphics.FromImage(toReturn))
            {
                g.DrawImage(toProcess, new Rectangle(0, 0, toProcess.Width, toProcess.Height), 0, 0, toProcess.Width,
                            toProcess.Height, GraphicsUnit.Pixel, ia);
            }
            return toReturn;
        }

        public static ColorMatrix GetDimmingColorMatrix(float percentLuminanceRetained)
        {
            var cm = new ColorMatrix();
            cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = percentLuminanceRetained;
            return cm;
        }

        public static Image GetDimmerImage(Image toProcess)
        {
            if (toProcess == null) return null;
            Image toReturn = new Bitmap(toProcess.Width, toProcess.Height);
            var ia = new ImageAttributes();
            ColorMatrix cm = GetDimmingColorMatrix(0.8f);
            ia.SetColorMatrix(cm);
            using (Graphics g = Graphics.FromImage(toReturn))
            {
                g.DrawImage(toProcess, new Rectangle(0, 0, toProcess.Width, toProcess.Height), 0, 0, toProcess.Width,
                            toProcess.Height, GraphicsUnit.Pixel, ia);
            }
            return toReturn;
        }

        public static Image GetNegativeImage(Image toProcess)
        {
            if (toProcess == null) return null;
            Image toReturn = new Bitmap(toProcess.Width, toProcess.Height);
            var ia = new ImageAttributes();
            var cm = new ColorMatrix();
            cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = -1;
            ia.SetColorMatrix(cm);
            using (Graphics g = Graphics.FromImage(toReturn))
            {
                g.DrawImage(toProcess, new Rectangle(0, 0, toProcess.Width, toProcess.Height), 0, 0, toProcess.Width,
                            toProcess.Height, GraphicsUnit.Pixel, ia);
            }
            return toReturn;
        }

        public static ColorMatrix GetGreyscaleColorMatrix()
        {
            //ColorMatrix cm = new ColorMatrix(new float[][]{   new float[]{0.3f,0.3f,0.3f,0,0},
            //                      new float[]{0.59f,0.59f,0.59f,0,0},
            //                      new float[]{0.11f,0.11f,0.11f,0,0},
            //                      new float[]{0,0,0,1,0,0},
            //                      new float[]{0,0,0,0,1,0},
            //                      new float[]{0,0,0,0,0,1}});
            var cm = new ColorMatrix(new[]
                                         {
                                             new[] {0.33f, 0.33f, 0.33f, 0, 0},
                                             new[] {0.33f, 0.33f, 0.33f, 0, 0},
                                             new[] {0.33f, 0.33f, 0.33f, 0, 0},
                                             new float[] {0, 0, 0, 1, 0, 0},
                                             new float[] {0, 0, 0, 0, 1, 0},
                                             new float[] {0, 0, 0, 0, 0, 1}
                                         });
            return cm;
        }

        public static ColorMatrix GetNVISColorMatrix(int brightnessLevel, int maxBrightnessLevel)
        {
            var cm = new ColorMatrix
                (
                new[]
                    {
                        new float[] {0, 0, 0, 0, 0}, //red %
                        new[]
                            {
                                0,
                                (brightnessLevel/(float) maxBrightnessLevel),
                                0, 0, 0
                            }, //green
                        new float[] {0, 0, 0, 0, 0}, //blue %
                        new float[] {0, 0, 0, 1, 0}, //alpha %
                        new float[] {-1, 0, -1, 0, 1}, //add
                    }
                );
            return cm;
        }

        public static void CropToContentAndDisposeOriginal(ref Bitmap image)
        {
            Bitmap croppped = CropToContent(image);
            Common.Util.DisposeObject(image);
            image = croppped;
        }

        public static Bitmap CropToContent(Bitmap bitmap)
        {
            Image asImage = bitmap;
            ConvertPixelFormat(ref asImage, PixelFormat.Format32bppPArgb);
            bitmap = (Bitmap) asImage;

            Rectangle cropRectangle;
            int minXNonBlackPixel = 0;
            int maxXNonBlackPixel = 0;
            int minYNonBlackPixel = 0;
            int maxYNonBlackPixel = 0;
            BitmapData sourceImageLock = null;
            try
            {
                sourceImageLock = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                                  ImageLockMode.ReadOnly, bitmap.PixelFormat);
                var bytes = new byte[bitmap.Width*bitmap.Height*4];
                Marshal.Copy(sourceImageLock.Scan0, bytes, 0, bytes.Length);
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        if
                            (
                            bytes[(y*bitmap.Width*4) + (x*4)] != 0
                            ||
                            bytes[(y*bitmap.Width*4) + (x*4) + 1] != 0
                            ||
                            bytes[(y*bitmap.Width*4) + (x*4) + 2] != 0
                            )
                        {
                            if (minXNonBlackPixel == 0) minXNonBlackPixel = x;
                            if (x > maxXNonBlackPixel) maxXNonBlackPixel = x;
                            if (minYNonBlackPixel == 0) minYNonBlackPixel = y;
                            if (y > maxYNonBlackPixel) maxYNonBlackPixel = y;
                        }
                    }
                }
            }
            finally
            {
                if (sourceImageLock != null)
                {
                    bitmap.UnlockBits(sourceImageLock);
                }
            }
            cropRectangle = new Rectangle(minXNonBlackPixel, minYNonBlackPixel, maxXNonBlackPixel - minXNonBlackPixel,
                                          maxYNonBlackPixel - minYNonBlackPixel);
            var toReturn = new Bitmap(cropRectangle.Width, cropRectangle.Height, bitmap.PixelFormat);
            using (Graphics g = Graphics.FromImage(toReturn))
            {
                g.DrawImage(bitmap, new Rectangle(0, 0, cropRectangle.Width, cropRectangle.Height), cropRectangle,
                            GraphicsUnit.Pixel);
            }
            return toReturn;
        }

        public static void InvertImage(ref Image bitmap)
        {
            Image inverted = GetNegativeImage(bitmap);
            Common.Util.DisposeObject(bitmap);
            bitmap = inverted;
        }

        // <summary>

        /// Copies a bitmap into a 1bpp/8bpp bitmap of the same dimensions, fast
        /// </summary>
        /// <param name="b">original bitmap</param>
        /// <param name="bpp">1 or 8, target bpp</param>
        /// <returns>a 1bpp copy of the bitmap</returns>
        public static Image CopyToBpp(Bitmap b, int bpp)
        {
            if (bpp != 1 && bpp != 8) throw new ArgumentException("1 or 8", "bpp");
            int w = b.Width, h = b.Height;
            IntPtr hbm = b.GetHbitmap();
            var bmi = new NativeMethods.BITMAPINFO();
            bmi.biSize = 40;
            bmi.biWidth = w;
            bmi.biHeight = h;
            bmi.biPlanes = 1;
            bmi.biBitCount = (short) bpp;
            bmi.biCompression = NativeMethods.BI_RGB;
            bmi.biSizeBitmap = (uint) (((w + 7) & 0xFFFFFFF8)*h/8);
            bmi.biXPelsPerMeter = 1000000;
            bmi.biYPelsPerMeter = 1000000;
            uint ncols = (uint) 1 << bpp;
            bmi.biClrUsed = ncols;
            bmi.biClrImportant = ncols;
            bmi.cols = new uint[256];
            if (bpp == 1)
            {
                bmi.cols[0] = MAKERGB(0, 0, 0);
                bmi.cols[1] = MAKERGB(255, 255, 255);
            }
            else
            {
                for (int i = 0; i < ncols; i++) bmi.cols[i] = MAKERGB(i, i, i);
            }
            IntPtr bits0;
            IntPtr hbm0 = NativeMethods.CreateDIBSection(IntPtr.Zero, ref bmi, NativeMethods.DIB_RGB_COLORS, out bits0,
                                                         IntPtr.Zero, 0);
            IntPtr sdc = NativeMethods.GetDC(IntPtr.Zero);
            IntPtr hdc = NativeMethods.CreateCompatibleDC(sdc);
            NativeMethods.SelectObject(hdc, hbm);
            IntPtr hdc0 = NativeMethods.CreateCompatibleDC(sdc);
            NativeMethods.SelectObject(hdc0, hbm0);
            NativeMethods.BitBlt(hdc0, 0, 0, w, h, hdc, 0, 0, NativeMethods.SRCCOPY);
            Bitmap b0 = Image.FromHbitmap(hbm0);
            NativeMethods.DeleteDC(hdc);
            NativeMethods.DeleteDC(hdc0);
            NativeMethods.ReleaseDC(IntPtr.Zero, sdc);
            NativeMethods.DeleteObject(hbm);
            NativeMethods.DeleteObject(hbm0);
            return b0;
        }

        private static uint MAKERGB(int r, int g, int b)
        {
            return ((uint) (b & 255)) | ((uint) ((g & 255) << 8)) | ((uint) ((r & 255) << 16));
        }
    }
}