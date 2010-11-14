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
    public class F16CabinPressureAltitudeIndicator : InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants
        private static string IMAGES_FOLDER_NAME = new DirectoryInfo (System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)).FullName + Path.DirectorySeparatorChar + "images";
        private const string CABINPRESS_BACKGROUND_IMAGE_FILENAME = "cabinpress.bmp";

        private const string CABINPRESS_NEEDLE_IMAGE_FILENAME = "cabprneed.bmp";
        private const string CABINPRESS_NEEDLE_MASK_FILENAME = "cabprneed_mask.bmp";

        #endregion

        #region Instance variables
        private static Bitmap _background;
        private static ImageMaskPair _needle;
        private static object _imagesLock = new object();
        private static bool _imagesLoaded = false;
        private bool _disposed = false;
        #endregion

        public F16CabinPressureAltitudeIndicator()
            : base()
        {
            this.InstrumentState = new F16CabinPressureAltitudeIndicatorInstrumentState();
            this.Options = new F16CabinPressureAltitudeIndicatorOptions();
        }
        #region Initialization Code
        private void LoadImageResources()
        {
            lock (_imagesLock)
            {
                if (_background == null)
                {
                    _background = (Bitmap)Common.Imaging.Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + CABINPRESS_BACKGROUND_IMAGE_FILENAME);
                }
                if (_needle == null)
                {
                    _needle = ImageMaskPair.CreateFromFiles(
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + CABINPRESS_NEEDLE_IMAGE_FILENAME,
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + CABINPRESS_NEEDLE_MASK_FILENAME
                    );
                    _needle.Use1BitAlpha = true;
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
                int width = _background.Width-12;
                int height = _background.Height-12;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform((float)bounds.Width / (float)width, (float)bounds.Height / (float)height); //set the initial scale transformation 
                g.TranslateTransform(-6, -5);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                GraphicsState basicState = g.Save();

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background, new Rectangle(0, 0, _background.Width, _background.Height), new Rectangle(0, 0, _background.Width, _background.Height), GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the needle 
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                float angle = 0;
                angle = ((this.InstrumentState.CabinPressureAltitudeFeet / 50000.0f) * 298.0f) - 3;
                g.TranslateTransform((float)_background.Width / 2.0f, (float)_background.Width / 2.0f);
                g.RotateTransform(angle);
                g.TranslateTransform(-(float)_background.Width / 2.0f, -(float)_background.Width / 2.0f);
                g.DrawImage(_needle.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }
        public F16CabinPressureAltitudeIndicatorInstrumentState InstrumentState
        {
            get;
            set;
        }
        public F16CabinPressureAltitudeIndicatorOptions Options
        {
            get;
            set;
        }
        #region Instrument State
        [Serializable]
        public class F16CabinPressureAltitudeIndicatorInstrumentState : InstrumentStateBase
        {
            private float _CabinPressureAltitudeFeet = 0;
            public F16CabinPressureAltitudeIndicatorInstrumentState()
            {
                this.CabinPressureAltitudeFeet = 0;
            }
            public float CabinPressureAltitudeFeet
            {
                get
                {
                    return _CabinPressureAltitudeFeet;
                }
                set
                {
                    float pressureAltitude = value;
                    if (pressureAltitude < 0) pressureAltitude = 0;
                    if (pressureAltitude > 50000) pressureAltitude = 50000;
                    _CabinPressureAltitudeFeet = pressureAltitude;
                }
            }
        }
        #endregion
        #region Options Class
        public class F16CabinPressureAltitudeIndicatorOptions
        {
            public F16CabinPressureAltitudeIndicatorOptions()
                : base()
            {
            }
        }
        #endregion
        ~F16CabinPressureAltitudeIndicator()
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
