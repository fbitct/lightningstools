﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Common.Imaging
{
    public class ImageMaskPair : IDisposable
    {
        private readonly bool _disposeImagesAtDisposalTime;
        private bool _disposed;
        private Bitmap _maskedImage;

        private ImageMaskPair()
        {
        }

        public ImageMaskPair(Bitmap image, Bitmap mask) : this()
        {
            Image = image;
            Mask = mask;
        }

        private ImageMaskPair(Bitmap image, Bitmap mask, bool disposeImagesAtDisposalTime)
            : this(image, mask)
        {
            _disposeImagesAtDisposalTime = disposeImagesAtDisposalTime;
        }

        public Bitmap Image { get; set; }
        public Bitmap Mask { get; set; }
        public bool Use1BitAlpha { get; set; }

        public Bitmap MaskedImage
        {
            get
            {
                if (_maskedImage == null)
                {
                    int width = Image.Width;
                    int height = Image.Height;

                    BitmapData imageLock = Image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                                                          Image.PixelFormat);
                    var imageContents = new byte[width*height*4];
                    Marshal.Copy(imageLock.Scan0, imageContents, 0, imageContents.Length);
                    Image.UnlockBits(imageLock);

                    BitmapData maskLock = Mask.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                                                        Mask.PixelFormat);
                    var maskContents = new byte[width*height*4];
                    Marshal.Copy(maskLock.Scan0, maskContents, 0, maskContents.Length);
                    Mask.UnlockBits(maskLock);

                    var newMaskedImageContents = new byte[width*height*4];
                    Array.Copy(imageContents, newMaskedImageContents, imageContents.Length);

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int thisBaseOffset = (y*width*4) + (x*4);
                            byte alpha;
                            if (Use1BitAlpha)
                            {
                                alpha = (byte) (
                                                   255 - (
                                                             //(0.3f * (float)maskContents[thisBaseOffset + 2]) 
                                                         //    + 
                                                         //(0.59f * (float)maskContents[thisBaseOffset + 1])
                                                         //    + 
                                                         // (0.11f * (float)maskContents[thisBaseOffset])
                                                         (0.333f*maskContents[thisBaseOffset + 2])
                                                         +
                                                         (0.333f*maskContents[thisBaseOffset + 1])
                                                         +
                                                         (0.333f*maskContents[thisBaseOffset]))
                                               );
                                if (alpha > 127)
                                {
                                    alpha = 255;
                                }
                                else
                                {
                                    alpha = 0;
                                }
                            }
                            else
                            {
                                alpha = (byte) (
                                                   255 - (
                                                             //(0.3f * (float)maskContents[thisBaseOffset + 2]) 
                                                         //    + 
                                                         //(0.59f * (float)maskContents[thisBaseOffset + 1])
                                                         //    + 
                                                         // (0.11f * (float)maskContents[thisBaseOffset])
                                                         (0.333f*maskContents[thisBaseOffset + 2])
                                                         +
                                                         (0.333f*maskContents[thisBaseOffset + 1])
                                                         +
                                                         (0.333f*maskContents[thisBaseOffset]))
                                               );
                            }
                            newMaskedImageContents[thisBaseOffset + 3] = alpha;
                        }
                    }

                    var newMaskedImage = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                    BitmapData newMaskedImageLock = newMaskedImage.LockBits(new Rectangle(0, 0, width, height),
                                                                            ImageLockMode.WriteOnly,
                                                                            newMaskedImage.PixelFormat);
                    Marshal.Copy(newMaskedImageContents, 0, newMaskedImageLock.Scan0, newMaskedImageContents.Length);
                    newMaskedImage.UnlockBits(newMaskedImageLock);
                    _maskedImage = newMaskedImage;
                }

                return _maskedImage;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public static ImageMaskPair CreateFromFiles(string imagePath, string maskPath)
        {
            Image image = System.Drawing.Image.FromFile(imagePath);
            Image mask = System.Drawing.Image.FromFile(maskPath);
            Util.ConvertPixelFormat(ref image, PixelFormat.Format32bppArgb);
            Util.ConvertPixelFormat(ref mask, PixelFormat.Format32bppArgb);
            return new ImageMaskPair((Bitmap) image, (Bitmap) mask, true);
        }

        ~ImageMaskPair()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //dispose of managed resources here
                    if (_disposeImagesAtDisposalTime)
                    {
                        Common.Util.DisposeObject(Image);
                        Common.Util.DisposeObject(Mask);
                        Common.Util.DisposeObject(MaskedImage);
                    }
                }
                //dispose of unmanaged resources here
                _disposed = true;
            }
        }
    }
}