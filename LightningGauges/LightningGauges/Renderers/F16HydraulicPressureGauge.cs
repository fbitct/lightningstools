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
    public class F16HydraulicPressureGauge : InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants
        private static string IMAGES_FOLDER_NAME = new DirectoryInfo (System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)).FullName + Path.DirectorySeparatorChar + "images";
        private const string HYD_BACKGROUND_IMAGE_FILENAME = "hyd.bmp";

        private const string HYD_NEEDLE_IMAGE_FILENAME = "hydneedle.bmp";
        private const string HYD_NEEDLE_MASK_FILENAME = "hydneedle_mask.bmp";

        #endregion

        #region Instance variables
        private static Bitmap _background;
        private static ImageMaskPair _needle;
        private static object _imagesLock = new object();
        private static bool _imagesLoaded = false;
        private bool _disposed = false;
        #endregion

        public F16HydraulicPressureGauge()
            : base()
        {
            this.InstrumentState = new F16HydraulicPressureGaugeInstrumentState();
            this.Options = new F16HydraulicPressureGaugeOptions();
        }
        #region Initialization Code
        private void LoadImageResources()
        {
            lock (_imagesLock)
            {
                if (_background == null)
                {
                    _background = (Bitmap)Common.Imaging.Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HYD_BACKGROUND_IMAGE_FILENAME);
                }
                if (_needle == null)
                {
                    _needle = ImageMaskPair.CreateFromFiles(
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HYD_NEEDLE_IMAGE_FILENAME,
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HYD_NEEDLE_MASK_FILENAME
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
                int width = 178;
                int height = 178;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform((float)bounds.Width / (float)width, (float)bounds.Height / (float)height); //set the initial scale transformation 
                g.TranslateTransform(-39, -39);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                GraphicsState basicState = g.Save();

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background, new Rectangle(0, 0, _background.Width, _background.Height), new Rectangle(0, 0, _background.Width, _background.Height), GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the needle 
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                float angle = 0;
                angle = ((this.InstrumentState.HydraulicPressurePoundsPerSquareInch/4000.0f)*308.0f) + 117.0f;
                g.TranslateTransform((float)_background.Width / 2.0f, (float)_background.Width / 2.0f);
                g.RotateTransform(angle);
                g.TranslateTransform(-(float)_background.Width / 2.0f, -(float)_background.Width / 2.0f);
                g.DrawImage(_needle.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }
        public F16HydraulicPressureGaugeInstrumentState InstrumentState
        {
            get;
            set;
        }
        public F16HydraulicPressureGaugeOptions Options
        {
            get;
            set;
        }
        #region Instrument State
        [Serializable]
        public class F16HydraulicPressureGaugeInstrumentState : InstrumentStateBase
        {
            private float _HydraulicPressurePoundsPerSquareInch = 0;
            public F16HydraulicPressureGaugeInstrumentState()
            {
                this.HydraulicPressurePoundsPerSquareInch = 0;
            }
            public float HydraulicPressurePoundsPerSquareInch
            {
                get
                {
                    return _HydraulicPressurePoundsPerSquareInch;
                }
                set
                {
                    float psi = value;
                    if (psi < 0) psi = 0;
                    if (psi > 4000) psi = 4000;
                    _HydraulicPressurePoundsPerSquareInch = psi;
                }
            }
        }
        #endregion
        #region Options Class
        public class F16HydraulicPressureGaugeOptions
        {
            public F16HydraulicPressureGaugeOptions()
                : base()
            {
            }
        }
        #endregion
        ~F16HydraulicPressureGauge()
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
