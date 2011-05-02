using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public class F16FuelFlow : InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants

        private const string FF_BACKGROUND_IMAGE_FILENAME = "fuelflow.bmp";
        private const string FF_BACKGROUND_MASK_FILENAME = "fuelflow_mask.bmp";

        private const string FF_HUNDREDS_DIGIT_IMAGE_FILENAME = "ffnum.bmp";
        private const string FF_THOUSANDS_DIGIT_IMAGE_FILENAME = "ffnumk.bmp";
        private const string FF_TEN_THOUSANDS_DIGIT_IMAGE_FILENAME = "ffnumkk.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static readonly object _imagesLock = new object();
        private static ImageMaskPair _background;
        private static Bitmap _hundredsDigits;
        private static Bitmap _thousandsDigits;
        private static Bitmap _tenThousandsDigits;
        private static bool _imagesLoaded;
        private bool _disposed;

        #endregion

        public F16FuelFlow()
        {
            InstrumentState = new F16FuelFlowInstrumentState();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + FF_BACKGROUND_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + FF_BACKGROUND_MASK_FILENAME
                    );
            }
            if (_hundredsDigits == null)
            {
                _hundredsDigits =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                            FF_HUNDREDS_DIGIT_IMAGE_FILENAME);
                _hundredsDigits.MakeTransparent(Color.Black);
            }
            if (_thousandsDigits == null)
            {
                _thousandsDigits =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                            FF_THOUSANDS_DIGIT_IMAGE_FILENAME);
                _thousandsDigits.MakeTransparent(Color.Black);
            }
            if (_tenThousandsDigits == null)
            {
                _tenThousandsDigits =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                            FF_TEN_THOUSANDS_DIGIT_IMAGE_FILENAME);
                _tenThousandsDigits.MakeTransparent(Color.Black);
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16FuelFlowInstrumentState InstrumentState { get; set; }

        #region Instrument State

        [Serializable]
        public class F16FuelFlowInstrumentState : InstrumentStateBase
        {
            private const float MAX_FLOW = 99999f;
            private float _fuelFlowPoundsPerHour;

            public F16FuelFlowInstrumentState()
            {
                FuelFlowPoundsPerHour = 0;
            }

            public float FuelFlowPoundsPerHour
            {
                get { return _fuelFlowPoundsPerHour; }
                set
                {
                    var flow = value;
                    if (flow < 0) flow = 0;
                    if (flow > MAX_FLOW) flow = MAX_FLOW;
                    _fuelFlowPoundsPerHour = flow;
                }
            }
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
                var initialState = g.Save();

                //set up the canvas scale and clipping region
                var width = 174;
                var height = 145;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                //set the initial scale transformation 
                g.TranslateTransform(-50, -60);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = g.Save();

                var hundredsDigit = (InstrumentState.FuelFlowPoundsPerHour/100.0f)%10.0f;
                var thousandsDigit = (float) Math.Truncate((InstrumentState.FuelFlowPoundsPerHour/1000.0f)%10.0f);
                var tenThousandsDigit = (float) Math.Truncate((InstrumentState.FuelFlowPoundsPerHour/10000.0f)%10.0f);

                if (thousandsDigit > 9) tenThousandsDigit += (thousandsDigit - 9);
                if (hundredsDigit > 9) thousandsDigit += (hundredsDigit - 9);

                var pixelsPerDigit = 29.5f;
                float xOffset = -130;
                float yOffsetToZero = -270;

                //draw the hundreds digit
                {
                    var yOffsetToActual = yOffsetToZero + (pixelsPerDigit*hundredsDigit);
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.TranslateTransform(xOffset, yOffsetToActual);
                    g.DrawImage(_hundredsDigits, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the thousands digit
                {
                    var yOffsetToActual = yOffsetToZero + (pixelsPerDigit*thousandsDigit);
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.TranslateTransform(xOffset, yOffsetToActual);
                    g.DrawImage(_thousandsDigits, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the ten-thousands digit
                {
                    var yOffsetToActual = yOffsetToZero + (pixelsPerDigit*tenThousandsDigit);
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.TranslateTransform(xOffset, yOffsetToActual);
                    g.DrawImage(_tenThousandsDigits, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background.MaskedImage,
                            new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height),
                            new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height),
                            GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }

        ~F16FuelFlow()
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
                    //Common.Util.DisposeObject(_hundredsDigits);
                    //Common.Util.DisposeObject(_thousandsDigits);
                    //Common.Util.DisposeObject(_tenThousandsDigits);
                }
                _disposed = true;
            }
        }
    }
}