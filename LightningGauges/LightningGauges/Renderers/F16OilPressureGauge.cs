using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public interface IF16OilPressureGauge : IInstrumentRenderer
    {
        F16OilPressureGauge.F16OilPressureGaugeInstrumentState InstrumentState { get; set; }
        F16OilPressureGauge.F16OilPressureGaugeOptions Options { get; set; }
    }

    public class F16OilPressureGauge : InstrumentRendererBase, IF16OilPressureGauge
    {
        #region Image Location Constants

        private const string OIL_BACKGROUND_IMAGE_FILENAME = "oil.bmp";
        private const string OIL_BACKGROUND2_IMAGE_FILENAME = "oil2.bmp";

        private const string OIL_NEEDLE_IMAGE_FILENAME = "arrow_rpm.bmp";
        private const string OIL_NEEDLE_MASK_FILENAME = "arrow_rpmmask.bmp";

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

        #endregion

        public F16OilPressureGauge()
        {
            InstrumentState = new F16OilPressureGaugeInstrumentState();
            Options = new F16OilPressureGaugeOptions();
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
                                                OIL_BACKGROUND_IMAGE_FILENAME);
                    _background.MakeTransparent(Color.Black);
                }
                if (_background2 == null)
                {
                    _background2 =
                        (Bitmap)
                        Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                                OIL_BACKGROUND2_IMAGE_FILENAME);
                    _background.MakeTransparent(Color.Black);
                }
                if (_needle == null)
                {
                    _needle = ImageMaskPair.CreateFromFiles(
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + OIL_NEEDLE_IMAGE_FILENAME,
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + OIL_NEEDLE_MASK_FILENAME
                        );
                    _needle.Use1BitAlpha = true;
                }
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16OilPressureGaugeInstrumentState InstrumentState { get; set; }
        public F16OilPressureGaugeOptions Options { get; set; }

        #region Instrument State

        [Serializable]
        public class F16OilPressureGaugeInstrumentState : InstrumentStateBase
        {
            private float _oilPressurePercent;

            public F16OilPressureGaugeInstrumentState()
            {
                OilPressurePercent = 0;
            }

            public float OilPressurePercent
            {
                get { return _oilPressurePercent; }
                set
                {
                    var pct = value;
                    if (pct < 0) pct = 0;
                    if (pct > 100) pct = 100;
                    _oilPressurePercent = pct;
                }
            }
        }

        #endregion

        #region Options Class

        public class F16OilPressureGaugeOptions
        {
            public F16OilPressureGaugeOptions()
            {
                IsSecondary = false;
            }

            public bool IsSecondary { get; set; }
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
                var width = 178;
                var height = 178;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                //set the initial scale transformation 
                g.TranslateTransform(-39, -39);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = g.Save();

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
                float angle = 0;
                if (InstrumentState.OilPressurePercent < 50.0f)
                {
                    angle = -86 + (3.23f*(InstrumentState.OilPressurePercent - 50.0f));
                }
                else
                {
                    angle = -86 + (3.16f*(InstrumentState.OilPressurePercent - 50.0f));
                }

                g.TranslateTransform(_background.Width/2.0f, _background.Width/2.0f);
                g.RotateTransform(angle);
                g.TranslateTransform(-(float) _background.Width/2.0f, -(float) _background.Width/2.0f);
                g.DrawImage(_needle.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }

    }
}