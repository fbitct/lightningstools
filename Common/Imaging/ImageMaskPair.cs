using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Common.Imaging
{
    public class ImageMaskPair:IDisposable
    {
        private Bitmap _maskedImage = null;
        private bool _disposed = false;
        private bool _disposeImagesAtDisposalTime = false;
        private ImageMaskPair() : base() { }
        public ImageMaskPair(Bitmap image, Bitmap mask):this()
        {
            this.Image = image;
            this.Mask = mask;
        }
        private ImageMaskPair(Bitmap image, Bitmap mask, bool disposeImagesAtDisposalTime)
            : this(image, mask)
        {
            _disposeImagesAtDisposalTime = disposeImagesAtDisposalTime;
        }
        public Bitmap Image
        {
            get;
            set;
        }
        public Bitmap Mask
        {
            get;
            set;
        }
        public bool Use1BitAlpha
        {
            get;
            set;
        }
        public Bitmap MaskedImage
        {
            get
            {
                if (_maskedImage == null)
                {
                    int width=this.Image.Width;
                    int height=this.Image.Height;
                    
                    BitmapData imageLock = this.Image.LockBits(new Rectangle(0,0,width,height), ImageLockMode.ReadOnly, this.Image.PixelFormat);
                    byte[] imageContents = new byte[width * height * 4];
                    Marshal.Copy(imageLock.Scan0, imageContents, 0, imageContents.Length);
                    this.Image.UnlockBits(imageLock);

                    BitmapData maskLock = this.Mask.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, this.Mask.PixelFormat);
                    byte[] maskContents = new byte[width * height * 4];
                    Marshal.Copy(maskLock.Scan0, maskContents, 0, maskContents.Length);
                    this.Mask.UnlockBits(maskLock);

                    byte[] newMaskedImageContents = new byte[width * height * 4];
                    Array.Copy(imageContents, newMaskedImageContents, imageContents.Length);

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int thisBaseOffset= (y * width *4) + (x * 4);
                            byte alpha;
                            if (this.Use1BitAlpha)
                            {
                                alpha = (byte)(
                                    255 - (
                                    //(0.3f * (float)maskContents[thisBaseOffset + 2]) 
                                    //    + 
                                    //(0.59f * (float)maskContents[thisBaseOffset + 1])
                                    //    + 
                                    // (0.11f * (float)maskContents[thisBaseOffset])
                                        (0.333f * (float)maskContents[thisBaseOffset + 2])
                                            +
                                        (0.333f * (float)maskContents[thisBaseOffset + 1])
                                            +
                                         (0.333f * (float)maskContents[thisBaseOffset]))
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
                                alpha = (byte)(
                                    255 - (
                                    //(0.3f * (float)maskContents[thisBaseOffset + 2]) 
                                    //    + 
                                    //(0.59f * (float)maskContents[thisBaseOffset + 1])
                                    //    + 
                                    // (0.11f * (float)maskContents[thisBaseOffset])
                                        (0.333f * (float)maskContents[thisBaseOffset + 2])
                                            +
                                        (0.333f * (float)maskContents[thisBaseOffset + 1])
                                            +
                                         (0.333f * (float)maskContents[thisBaseOffset]))
                                );
                            }
                            newMaskedImageContents[thisBaseOffset + 3] = alpha;
                        }
                    }

                    Bitmap newMaskedImage = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                    BitmapData newMaskedImageLock = newMaskedImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, newMaskedImage.PixelFormat);
                    Marshal.Copy(newMaskedImageContents, 0, newMaskedImageLock.Scan0, newMaskedImageContents.Length);
                    newMaskedImage.UnlockBits(newMaskedImageLock);
                    _maskedImage = newMaskedImage;
                }

                return _maskedImage;
            }
        }
        public static ImageMaskPair CreateFromFiles(string imagePath, string maskPath)
        {
            Bitmap image = (Bitmap)Bitmap.FromFile(imagePath);
            Bitmap mask = (Bitmap)Bitmap.FromFile(maskPath);
            Common.Imaging.Util.ConvertPixelFormat(ref image, PixelFormat.Format32bppArgb);
            Common.Imaging.Util.ConvertPixelFormat(ref mask, PixelFormat.Format32bppArgb);
            return new ImageMaskPair(image, mask,true);
        }
        ~ImageMaskPair()
        {
            this.Dispose(false); 
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
                        Common.Util.DisposeObject(this.Image);
                        Common.Util.DisposeObject(this.Mask);
                        Common.Util.DisposeObject(this.MaskedImage);
                    }
                }
                //dispose of unmanaged resources here
                _disposed = true;
            }
        }
        public void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
