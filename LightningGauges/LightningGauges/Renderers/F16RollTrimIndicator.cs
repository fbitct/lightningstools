using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Common.SimSupport;
using System.IO;
using System.Drawing.Drawing2D;
using Common.Imaging;

namespace LightningGauges.Renderers
{
    public class F16RollTrimIndicator: InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants
        private static string IMAGES_FOLDER_NAME = new DirectoryInfo (System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)).FullName + Path.DirectorySeparatorChar + "images";
        private const string ROLLTRIM_BACKGROUND_IMAGE_FILENAME = "rolltrim.bmp";
        private const string ROLLTRIM_NEEDLE_IMAGE_FILENAME = "rolltrimneed.bmp";
        private const string ROLLTRIM_NEEDLE_MASK_FILENAME = "rolltrimneed_mask.bmp";
        #endregion

        #region Instance variables
        private static object _imagesLock = new object();
        private static Bitmap _background;
        private static Bitmap _needle;
        private static bool _imagesLoaded = false;

        private bool _disposed = false;
        #endregion

        public F16RollTrimIndicator()
            : base()
        {
            this.InstrumentState = new F16RollTrimIndicatorInstrumentState();
        }
        #region Initialization Code
        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background = (Bitmap)Common.Imaging.Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ROLLTRIM_BACKGROUND_IMAGE_FILENAME);
            }
            if (_needle == null)
            {
                using (ImageMaskPair needleWithMask = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ROLLTRIM_NEEDLE_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ROLLTRIM_NEEDLE_MASK_FILENAME
                    ))
                {
                    needleWithMask.Use1BitAlpha = true;
                    _needle = (Bitmap) Common.Imaging.Util.CropBitmap(needleWithMask.MaskedImage, new Rectangle(136,93,61,61));
                    _needle.MakeTransparent(Color.Black);
                  
                }
                
            }
            _imagesLoaded = true;

        }
        #endregion

        public override void Render(Graphics g, Rectangle bounds)
        {
            if (!_imagesLoaded)
            {
                LoadImageResources();
            }
            lock (_imagesLock)
            {
                //store the canvas's transform and clip settings so we can restore them later
                GraphicsState initialState = g.Save();

                //set up the canvas scale and clipping region
                int width = 108;
                int height = 108;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform((float)bounds.Width / (float)width, (float)bounds.Height / (float)height); //set the initial scale transformation 
                g.TranslateTransform(-64, -70);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                GraphicsState basicState = g.Save();

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background, new Rectangle(0, 0, _background.Width, _background.Height), new Rectangle(0, 0, _background.Width, _background.Height), GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the needle
                float rt = -this.InstrumentState.RollTrimPercent;
                float angle = (rt / 100.0f) * 90;
                
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(128, 93);
                g.TranslateTransform(8, 0);
                g.TranslateTransform(_needle.Width/2.0f, _needle.Height/2.0f);
                g.RotateTransform(angle);
                g.TranslateTransform(-_needle.Width/2.0f, -_needle.Height/2.0f);
                g.DrawImage(_needle, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }
        public F16RollTrimIndicatorInstrumentState InstrumentState
        {
            get;
            set;
        }
        #region Instrument State
        [Serializable]
        public class F16RollTrimIndicatorInstrumentState : InstrumentStateBase
        {
            private float _rollTrimPercent= 0;
            public F16RollTrimIndicatorInstrumentState()
            {
                this.RollTrimPercent=0;
            }
            public float RollTrimPercent
            {
                get
                {
                    return _rollTrimPercent;
                }
                set
                {
                    float pct = value;
                    if (Math.Abs(pct) >100.0f) pct = Math.Sign(pct) *100.0f;
                    _rollTrimPercent= pct;
                }
            }
        }
        #endregion
        ~F16RollTrimIndicator()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //Common.Util.DisposeObject(_background);
                    //Common.Util.DisposeObject(_needle);
                }
                _disposed = true;
            }

        }
    }
}
