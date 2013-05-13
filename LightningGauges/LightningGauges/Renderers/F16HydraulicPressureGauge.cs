using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public interface IF16HydraulicPressureGauge : IInstrumentRenderer
    {
        F16HydraulicPressureGauge.F16HydraulicPressureGaugeInstrumentState InstrumentState { get; set; }
        F16HydraulicPressureGauge.F16HydraulicPressureGaugeOptions Options { get; set; }
    }

    public class F16HydraulicPressureGauge : InstrumentRendererBase, IDisposable, IF16HydraulicPressureGauge
    {
        #region Image Location Constants

        private const string HYD_BACKGROUND_IMAGE_FILENAME = "hyd.bmp";

        private const string HYD_NEEDLE_IMAGE_FILENAME = "hydneedle.bmp";
        private const string HYD_NEEDLE_MASK_FILENAME = "hydneedle_mask.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static Bitmap _background;
        private static ImageMaskPair _needle;
        private static readonly object _imagesLock = new object();
        private static bool _imagesLoaded;

        #endregion

        public F16HydraulicPressureGauge()
        {
            InstrumentState = new F16HydraulicPressureGaugeInstrumentState();
            Options = new F16HydraulicPressureGaugeOptions();
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
                                                HYD_BACKGROUND_IMAGE_FILENAME);
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

        public F16HydraulicPressureGaugeInstrumentState InstrumentState { get; set; }
        public F16HydraulicPressureGaugeOptions Options { get; set; }

        #region Instrument State

        [Serializable]
        public class F16HydraulicPressureGaugeInstrumentState : InstrumentStateBase
        {
            private float _HydraulicPressurePoundsPerSquareInch;

            public F16HydraulicPressureGaugeInstrumentState()
            {
                HydraulicPressurePoundsPerSquareInch = 0;
            }

            public float HydraulicPressurePoundsPerSquareInch
            {
                get { return _HydraulicPressurePoundsPerSquareInch; }
                set
                {
                    var psi = value;
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
                g.DrawImage(_background, new Rectangle(0, 0, _background.Width, _background.Height),
                            new Rectangle(0, 0, _background.Width, _background.Height), GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the needle 
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                float angle = 0;
                angle = ((InstrumentState.HydraulicPressurePoundsPerSquareInch/4000.0f)*308.0f) + 117.0f;
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