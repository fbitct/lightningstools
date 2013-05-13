using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public interface IF16VerticalVelocityIndicatorEU : IInstrumentRenderer
    {
        F16VerticalVelocityIndicatorEU.F16VerticalVelocityIndicatorEUInstrumentState InstrumentState { get; set; }
    }

    public class F16VerticalVelocityIndicatorEU : InstrumentRendererBase, IF16VerticalVelocityIndicatorEU
    {
        #region Image Location Constants

        private const string VVI_BACKGROUND_IMAGE_FILENAME = "vvieuro.bmp";
        private const string VVI_OFF_FLAG_IMAGE_FILENAME = "vvieuroflag.bmp";
        private const string VVI_OFF_FLAG_MASK_FILENAME = "vvieuroflag_mask.bmp";
        private const string VVI_NEEDLE_IMAGE_FILENAME = "arrowrpm.bmp";
        private const string VVI_NEEDLE_MASK_FILENAME = "arrowrpmmask.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static readonly object _imagesLock = new object();
        private static Bitmap _background;
        private static ImageMaskPair _offFlag;
        private static ImageMaskPair _needle;
        private static bool _imagesLoaded;

        #endregion

        public F16VerticalVelocityIndicatorEU()
        {
            InstrumentState = new F16VerticalVelocityIndicatorEUInstrumentState();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                            VVI_BACKGROUND_IMAGE_FILENAME);
            }
            if (_offFlag == null)
            {
                _offFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + VVI_OFF_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + VVI_OFF_FLAG_MASK_FILENAME
                    );
            }
            if (_needle == null)
            {
                _needle = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + VVI_NEEDLE_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + VVI_NEEDLE_MASK_FILENAME
                    );
                _needle.Use1BitAlpha = true;
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16VerticalVelocityIndicatorEUInstrumentState InstrumentState { get; set; }

        #region Instrument State

        [Serializable]
        public class F16VerticalVelocityIndicatorEUInstrumentState : InstrumentStateBase
        {
            private const float MAX_VELOCITY = 6000;
            private const float MIN_VELOCITY = -6000;
            private float _verticalVelocityFeet;

            public F16VerticalVelocityIndicatorEUInstrumentState()
            {
                OffFlag = false;
                VerticalVelocityFeet = 0.0f;
            }

            public float VerticalVelocityFeet
            {
                get { return _verticalVelocityFeet; }
                set
                {
                    var vv = value;
                    if (vv < MIN_VELOCITY) vv = MIN_VELOCITY;
                    if (vv > MAX_VELOCITY) vv = MAX_VELOCITY;
                    _verticalVelocityFeet = vv;
                }
            }

            public bool OffFlag { get; set; }
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
                var width = _background.Width;
                var height = _background.Height - 29;
                width -= 154;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                //set the initial scale transformation 

                g.TranslateTransform(-76, 0);
                g.TranslateTransform(0, -15);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = g.Save();


                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background, new Rectangle(0, 0, _background.Width, _background.Height),
                            new Rectangle(0, 0, _background.Width, _background.Height), GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the OFF flag
                if (InstrumentState.OffFlag)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.DrawImage(_offFlag.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }


                //draw the needle
                var centerX = 156.0f;
                var centerY = 130.0f;
                var vv = InstrumentState.VerticalVelocityFeet;
                if (Math.Abs(vv) > 6000.0) vv = Math.Sign(vv)*6000.0f;
                var vviInThousands = vv/1000.0f;
                float angle;
                if (Math.Abs(vviInThousands) > 1.0f)
                {
                    angle = -45.0f + (Math.Sign(vviInThousands)*((Math.Abs(vviInThousands) - 1.0f)*(45.0f/5.0f)));
                    if (vviInThousands < 0) angle -= 90;
                }
                else
                {
                    angle = -90.0f + (vviInThousands*45.0f);
                }
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(angle);
                g.TranslateTransform(-centerX, -centerY);
                g.TranslateTransform(28, 0);
                g.DrawImage(_needle.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);


                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }

    }
}