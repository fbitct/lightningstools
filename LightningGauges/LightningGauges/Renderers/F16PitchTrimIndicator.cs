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
    public class F16PitchTrimIndicator : InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants
        private static string IMAGES_FOLDER_NAME = new DirectoryInfo (System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)).FullName + Path.DirectorySeparatorChar + "images";
        private const string PITCHTRIM_BACKGROUND_IMAGE_FILENAME = "pitchtrim.bmp";
        private const string PITCHTRIM_NEEDLE_IMAGE_FILENAME = "pitchtrimneedle.bmp";
        private const string PITCHTRIM_NEEDLE_MASK_FILENAME = "pitchtrimneedle_mask.bmp";
        #endregion

        #region Instance variables
        private static object _imagesLock = new object();
        private static Bitmap _background;
        private static Bitmap _needle;
        private static bool _imagesLoaded = false;
        private bool _disposed = false;
        #endregion

        public F16PitchTrimIndicator()
            : base()
        {
            this.InstrumentState = new F16PitchTrimIndicatorInstrumentState();
        }
        #region Initialization Code
        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background = (Bitmap)Common.Imaging.Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + PITCHTRIM_BACKGROUND_IMAGE_FILENAME);
            }
            if (_needle == null)
            {
                using (ImageMaskPair needleWithMask = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + PITCHTRIM_NEEDLE_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + PITCHTRIM_NEEDLE_MASK_FILENAME
                    ))
                {
                    needleWithMask.Use1BitAlpha = true;
                    _needle = Common.Imaging.Util.CropBitmap(needleWithMask.MaskedImage, new Rectangle(97, 68, 60, 60));
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
                int width = _background.Width -148;
                int height = _background.Height-148;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform((float)bounds.Width / (float)width, (float)bounds.Height / (float)height); //set the initial scale transformation 
                g.TranslateTransform(-75, -68);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                GraphicsState basicState = g.Save();

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background, new Rectangle(0, 0, _background.Width, _background.Height), new Rectangle(0, 0, _background.Width, _background.Height), GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the needle
                float pt = this.InstrumentState.PitchTrimPercent;
                float angle = (pt / 100.0f) * 90;

                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(78, 95);
                g.TranslateTransform(20, -25);
                g.TranslateTransform(_needle.Width / 2.0f, _needle.Height / 2.0f);
                g.RotateTransform(angle);
                g.TranslateTransform(-_needle.Width / 2.0f, -_needle.Height / 2.0f);
                g.DrawImage(_needle, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }
        public F16PitchTrimIndicatorInstrumentState InstrumentState
        {
            get;
            set;
        }
        #region Instrument State
        [Serializable]
        public class F16PitchTrimIndicatorInstrumentState : InstrumentStateBase
        {
            private float _pitchTrimPercent = 0;
            public F16PitchTrimIndicatorInstrumentState()
            {
                this.PitchTrimPercent = 0;
            }
            public float PitchTrimPercent
            {
                get
                {
                    return _pitchTrimPercent;
                }
                set
                {
                    float pct = value;
                    if (Math.Abs(pct) > 100.0f) pct = Math.Sign(pct) * 100.0f;
                    _pitchTrimPercent = pct;
                }
            }
        }
        #endregion
        ~F16PitchTrimIndicator()
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
