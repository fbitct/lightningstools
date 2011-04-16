using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public class F16NozzlePositionIndicator : InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants

        private const string NOZ_BACKGROUND_IMAGE_FILENAME = "noz.bmp";
        private const string NOZ_BACKGROUND2_IMAGE_FILENAME = "noz2.bmp";

        private const string NOZ_NEEDLE_IMAGE_FILENAME = "arrow_rpm.bmp";
        private const string NOZ_NEEDLE_MASK_FILENAME = "arrow_rpmmask.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static Bitmap _background;
        private static Bitmap _background2;
        private static ImageMaskPair _needle;
        private static readonly object _imagesLock = new object();
        private static bool _imagesLoaded;
        private bool _disposed;

        #endregion

        public F16NozzlePositionIndicator()
        {
            InstrumentState = new F16NozzlePositionIndicatorInstrumentState();
            Options = new F16NozzlePositionIndicatorOptions();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            lock (_imagesLock)
            {
                if (_background == null)
                {
                    _background =
                        (Bitmap)
                        Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                                NOZ_BACKGROUND_IMAGE_FILENAME);
                }
                if (_background2 == null)
                {
                    _background2 =
                        (Bitmap)
                        Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                                NOZ_BACKGROUND2_IMAGE_FILENAME);
                }

                if (_needle == null)
                {
                    _needle = ImageMaskPair.CreateFromFiles(
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + NOZ_NEEDLE_IMAGE_FILENAME,
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + NOZ_NEEDLE_MASK_FILENAME
                        );
                    _needle.Use1BitAlpha = true;
                }
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16NozzlePositionIndicatorOptions Options { get; set; }
        public F16NozzlePositionIndicatorInstrumentState InstrumentState { get; set; }

        #region Instrument State

        [Serializable]
        public class F16NozzlePositionIndicatorInstrumentState : InstrumentStateBase
        {
            private float _nozzlePositionPercent;

            public F16NozzlePositionIndicatorInstrumentState()
            {
                NozzlePositionPercent = 0;
            }

            public float NozzlePositionPercent
            {
                get { return _nozzlePositionPercent; }
                set
                {
                    float pct = value;
                    if (pct < 0) pct = 0;
                    if (pct > 102) pct = 102;
                    _nozzlePositionPercent = pct;
                }
            }
        }

        #endregion

        #region Options Class

        public class F16NozzlePositionIndicatorOptions
        {
            public bool IsSecondary { get; set; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
                g.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                    //set the initial scale transformation 
                g.TranslateTransform(-39, -39);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                GraphicsState basicState = g.Save();

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                if (Options.IsSecondary)
                {
                    g.DrawImage(_background2, new Rectangle(0, 0, _background2.Width, _background2.Height),
                                new Rectangle(0, 0, _background2.Width, _background2.Height), GraphicsUnit.Pixel);
                }
                else
                {
                    g.DrawImage(_background, new Rectangle(0, 0, _background.Width, _background.Height),
                                new Rectangle(0, 0, _background.Width, _background.Height), GraphicsUnit.Pixel);
                }
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the needle 
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                float angle = 55.0f + (2.5f*InstrumentState.NozzlePositionPercent);

                g.TranslateTransform(_background.Width/2.0f, _background.Width/2.0f);
                g.RotateTransform(angle);
                g.TranslateTransform(-(float) _background.Width/2.0f, -(float) _background.Width/2.0f);
                g.DrawImage(_needle.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }

        ~F16NozzlePositionIndicator()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //Common.Util.DisposeObject(_background);
                    //Common.Util.DisposeObject(_background2);
                    //Common.Util.DisposeObject(_needle);
                }
                _disposed = true;
            }
        }
    }
}