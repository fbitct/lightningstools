using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public class F16Accelerometer : InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants

        private const string ACCELEROMETER_BACKGROUND_IMAGE_FILENAME = "accelerometerface.bmp";

        private const string ACCELEROMETER_NEEDLE_IMAGE_FILENAME = "accelerometerneed.bmp";
        private const string ACCELEROMETER_NEEDLE_MASK_FILENAME = "accelerometerneed_mask.bmp";
        private const string ACCELEROMETER_NEEDLE2_IMAGE_FILENAME = "accelerometerneed2.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static Bitmap _background;
        private static ImageMaskPair _needle;
        private static ImageMaskPair _needle2;
        private static readonly object _imagesLock = new object();
        private bool _disposed;

        #endregion

        public F16Accelerometer()
        {
            InstrumentState = new F16AccelerometerInstrumentState();
            Options = new F16AccelerometerOptions();
            LoadImageResources();
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
                                                ACCELEROMETER_BACKGROUND_IMAGE_FILENAME);
                }
                if (_needle == null)
                {
                    _needle = ImageMaskPair.CreateFromFiles(
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ACCELEROMETER_NEEDLE_IMAGE_FILENAME,
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ACCELEROMETER_NEEDLE_MASK_FILENAME
                        );
                    _needle.Use1BitAlpha = true;
                }
                if (_needle2 == null)
                {
                    _needle2 = ImageMaskPair.CreateFromFiles(
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ACCELEROMETER_NEEDLE2_IMAGE_FILENAME,
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ACCELEROMETER_NEEDLE_MASK_FILENAME
                        );
                    _needle2.Use1BitAlpha = true;
                }
            }
        }

        #endregion

        public F16AccelerometerInstrumentState InstrumentState { get; set; }
        public F16AccelerometerOptions Options { get; set; }

        #region Instrument State

        [Serializable]
        public class F16AccelerometerInstrumentState : InstrumentStateBase
        {
            private const float MIN_INDICATABLE_GS = -5.0f;
            private const float MAX_INDICATABLE_GS = 10.0f;
            private float _accelerationInGs = 1;
            private float _maxGs = 1;
            private float _minGs = 1;

            public float AccelerationInGs
            {
                get { return _accelerationInGs; }
                set
                {
                    float acceleration = value;
                    if (acceleration < MIN_INDICATABLE_GS) acceleration = MIN_INDICATABLE_GS;
                    if (acceleration > MAX_INDICATABLE_GS) acceleration = MAX_INDICATABLE_GS;
                    if (acceleration > _maxGs) _maxGs = acceleration;
                    if (acceleration < _minGs) _minGs = acceleration;
                    _accelerationInGs = acceleration;
                }
            }

            public float MinGs
            {
                get { return _minGs; }
            }

            public float MaxGs
            {
                get { return _maxGs; }
            }

            public void ResetMinAndMaxGs()
            {
                _minGs = 1;
                _maxGs = 1;
            }
        }

        #endregion

        #region Options Class

        public class F16AccelerometerOptions
        {
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
            lock (_imagesLock)
            {
                //store the canvas's transform and clip settings so we can restore them later
                GraphicsState initialState = g.Save();

                //set up the canvas scale and clipping region
                int width = _background.Width;
                int height = _background.Height;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                    //set the initial scale transformation 
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                GraphicsState basicState = g.Save();

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background, new Rectangle(0, 0, _background.Width, _background.Height),
                            new Rectangle(0, 0, _background.Width, _background.Height), GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the min G needle
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                float angle = -43 + (((InstrumentState.MinGs + 5)/15.0f)*340);
                g.TranslateTransform(_background.Width/2.0f, _background.Width/2.0f);
                g.RotateTransform(angle);
                g.TranslateTransform(-(float) _background.Width/2.0f, -(float) _background.Width/2.0f);
                g.DrawImage(_needle2.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the max G needle
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                angle = -43 + (((InstrumentState.MaxGs + 5)/15.0f)*340);
                g.TranslateTransform(_background.Width/2.0f, _background.Width/2.0f);
                g.RotateTransform(angle);
                g.TranslateTransform(-(float) _background.Width/2.0f, -(float) _background.Width/2.0f);
                g.DrawImage(_needle2.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);


                //draw the actual G needle
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                angle = -43 + (((InstrumentState.AccelerationInGs + 5)/15.0f)*340);
                g.TranslateTransform(_background.Width/2.0f, _background.Width/2.0f);
                g.RotateTransform(angle);
                g.TranslateTransform(-(float) _background.Width/2.0f, -(float) _background.Width/2.0f);
                g.DrawImage(_needle.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);


                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }

        ~F16Accelerometer()
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
                    //Common.Util.DisposeObject(_needle);
                }
                _disposed = true;
            }
        }
    }
}