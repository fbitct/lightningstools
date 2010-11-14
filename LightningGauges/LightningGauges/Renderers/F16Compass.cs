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
    public class F16Compass : InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants
        private static string IMAGES_FOLDER_NAME = new DirectoryInfo (System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)).FullName + Path.DirectorySeparatorChar + "images";
        private const string COMPASS_BACKGROUND_IMAGE_FILENAME = "compass.bmp";
        private const string COMPASS_BACKGROUND_MASK_FILENAME = "compass_mask.bmp";
        private const string COMPASS_TAPE_IMAGE_FILENAME = "compasstape.bmp";
        private const string COMPASS_NEEDLE_IMAGE_FILENAME = "compneedle.bmp";
        private const string COMPASS_NEEDLE_MASK_FILENAME = "compneedle_mask.bmp";
        #endregion

        #region Instance variables
        private static object _imagesLock = new object();
        private static ImageMaskPair _background;
        private static Bitmap _tape;
        private static ImageMaskPair _needle;
        private bool _disposed = false;
        private static bool _imagesLoaded = false;
        #endregion

        public F16Compass()
            : base()
        {
            this.InstrumentState = new F16CompassInstrumentState();
        }
        #region Initialization Code
        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + COMPASS_BACKGROUND_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + COMPASS_BACKGROUND_MASK_FILENAME
                    );
                _background.Use1BitAlpha=true;
            }
            if (_needle == null)
            {
                _needle = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + COMPASS_NEEDLE_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + COMPASS_NEEDLE_MASK_FILENAME
                    );

            }
            if (_tape == null)
            {
                _tape = (Bitmap)Common.Imaging.Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + COMPASS_TAPE_IMAGE_FILENAME);
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
                int width = 205;
                int height = 211;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform((float)bounds.Width / (float)width, (float)bounds.Height / (float)height); //set the initial scale transformation 
                g.TranslateTransform(-25, -23);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                GraphicsState basicState = g.Save();
                //draw the tape 
                float pixelsPerDegree = 2.802f;
                float heading = this.InstrumentState.MagneticHeadingDegrees;
                float offset = (pixelsPerDegree * heading);
                float translateX = -1327;
                float translateY = 90;
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(translateX, translateY);
                g.TranslateTransform(offset, 0);
                g.ScaleTransform(0.80f, 0.80f);
                g.DrawImage(_tape, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the needle 
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_needle.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);


                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background.MaskedImage, new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height), new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height), GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }
        public F16CompassInstrumentState InstrumentState
        {
            get;
            set;
        }
        #region Instrument State
        [Serializable]
        public class F16CompassInstrumentState : InstrumentStateBase
        {
            private float _magneticHeadingDegrees = 0.0f;
            public F16CompassInstrumentState():base()
            {
            }
            public float MagneticHeadingDegrees
            {
                get
                {
                    return _magneticHeadingDegrees;
                }
                set
                {
                    float heading = value;
                    heading %= 360.0f;
                    _magneticHeadingDegrees = heading;
                }
            }
        }
        #endregion
        ~F16Compass()
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
                    //Common.Util.DisposeObject(_tape);
                    //Common.Util.DisposeObject(_needle);
                }
                _disposed = true;
            }

        }

    }
}
