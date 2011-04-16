using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public class F16FanTurbineInletTemperature : InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants

        private const string FTIT_BACKGROUND_IMAGE_FILENAME = "ftit.bmp";
        private const string FTIT_BACKGROUND2_IMAGE_FILENAME = "ftit2.bmp";

        private const string FTIT_NEEDLE_IMAGE_FILENAME = "arrow_rpm.bmp";
        private const string FTIT_NEEDLE_MASK_FILENAME = "arrow_rpmmask.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static Bitmap _background;
        private static Bitmap _background2;
        private static ImageMaskPair _needle;
        private static bool _imagesLoaded;
        private static readonly object _imagesLock = new object();
        private bool _disposed;

        #endregion

        public F16FanTurbineInletTemperature()
        {
            Options = new F16FanTurbineInletTemperatureOptions();
            InstrumentState = new F16FanTurbineInletTemperatureInstrumentState();
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
                                                FTIT_BACKGROUND_IMAGE_FILENAME);
                }
                if (_background2 == null)
                {
                    _background2 =
                        (Bitmap)
                        Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                                FTIT_BACKGROUND2_IMAGE_FILENAME);
                }
                if (_needle == null)
                {
                    _needle = ImageMaskPair.CreateFromFiles(
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + FTIT_NEEDLE_IMAGE_FILENAME,
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + FTIT_NEEDLE_MASK_FILENAME
                        );
                }
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16FanTurbineInletTemperatureInstrumentState InstrumentState { get; set; }
        public F16FanTurbineInletTemperatureOptions Options { get; set; }

        #region Instrument State

        [Serializable]
        public class F16FanTurbineInletTemperatureInstrumentState : InstrumentStateBase
        {
            private const float MIN_TEMP_CELCIUS = 0.0f;
            private float _inletTemperatureDegreesCelcius = 1200.0f;

            public float InletTemperatureDegreesCelcius
            {
                get { return _inletTemperatureDegreesCelcius; }
                set
                {
                    float temp = value;
                    if (temp < MIN_TEMP_CELCIUS) temp = MIN_TEMP_CELCIUS;
                    _inletTemperatureDegreesCelcius = temp;
                }
            }
        }

        #endregion

        #region Options Class

        public class F16FanTurbineInletTemperatureOptions
        {
            public F16FanTurbineInletTemperatureOptions()
            {
                IsSecondary = false;
            }

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
                float angle = 116f + GetAngle(InstrumentState.InletTemperatureDegreesCelcius);

                g.TranslateTransform(_background.Width/2.0f, _background.Width/2.0f);
                g.RotateTransform(angle);
                g.TranslateTransform(-(float) _background.Width/2.0f, -(float) _background.Width/2.0f);
                g.DrawImage(_needle.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }

        private float GetAngle(float inletTemperature)
        {
            float angle = 0;
            if (inletTemperature <= 200.0f)
            {
                angle = 0;
            }
            else if (inletTemperature > 200.0f && inletTemperature <= 700.0f)
            {
                angle = ((inletTemperature - 200.0f)/50.0f)*10.5f;
                    //10.5 degrees of space for 50 degrees Celcius of readout
            }
            else if (inletTemperature > 700.0f && inletTemperature <= 1000.0f)
            {
                angle = 105 + ((inletTemperature - 700.0f)/10.0f)*5.4f;
                    //5.5 degrees of space for 10 degrees Celcius of readout
            }
            else if (inletTemperature > 1000.0f && inletTemperature <= 1200.0f)
            {
                angle = 266 + ((inletTemperature - 1000.0f)/50.0f)*10.5f;
                    //10.5 degrees of space for 50 degrees Celcius of readout
            }

            return angle;
        }

        ~F16FanTurbineInletTemperature()
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