using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public interface IF16PitchTrimIndicator : IInstrumentRenderer
    {
        F16PitchTrimIndicator.F16PitchTrimIndicatorInstrumentState InstrumentState { get; set; }
    }

    public class F16PitchTrimIndicator : InstrumentRendererBase, IF16PitchTrimIndicator
    {
        #region Image Location Constants

        private const string PITCHTRIM_BACKGROUND_IMAGE_FILENAME = "pitchtrim.bmp";
        private const string PITCHTRIM_NEEDLE_IMAGE_FILENAME = "pitchtrimneedle.bmp";
        private const string PITCHTRIM_NEEDLE_MASK_FILENAME = "pitchtrimneedle_mask.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static readonly object _imagesLock = new object();
        private static Bitmap _background;
        private static Bitmap _needle;
        private static bool _imagesLoaded;

        #endregion

        public F16PitchTrimIndicator()
        {
            InstrumentState = new F16PitchTrimIndicatorInstrumentState();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                            PITCHTRIM_BACKGROUND_IMAGE_FILENAME);
            }
            if (_needle == null)
            {
                using (var needleWithMask = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + PITCHTRIM_NEEDLE_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + PITCHTRIM_NEEDLE_MASK_FILENAME
                    ))
                {
                    needleWithMask.Use1BitAlpha = true;
                    _needle = (Bitmap) Util.CropBitmap(needleWithMask.MaskedImage, new Rectangle(97, 68, 60, 60));
                    _needle.MakeTransparent(Color.Black);
                }
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16PitchTrimIndicatorInstrumentState InstrumentState { get; set; }

        #region Instrument State

        [Serializable]
        public class F16PitchTrimIndicatorInstrumentState : InstrumentStateBase
        {
            private float _pitchTrimPercent;

            public F16PitchTrimIndicatorInstrumentState()
            {
                PitchTrimPercent = 0;
            }

            public float PitchTrimPercent
            {
                get { return _pitchTrimPercent; }
                set
                {
                    var pct = value;
                    if (float.IsInfinity(pct) || float.IsNaN(pct)) pct = 0;
                    if (Math.Abs(pct) > 100.0f) pct = Math.Sign(pct) * 100.0f;
                    _pitchTrimPercent = pct;
                }
            }
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
                var width = _background.Width - 148;
                var height = _background.Height - 148;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                //set the initial scale transformation 
                g.TranslateTransform(-75, -68);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = g.Save();

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background, new Rectangle(0, 0, _background.Width, _background.Height),
                            new Rectangle(0, 0, _background.Width, _background.Height), GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the needle
                var pt = InstrumentState.PitchTrimPercent;
                var angle = (pt/100.0f)*90;

                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(78, 95);
                g.TranslateTransform(20, -25);
                g.TranslateTransform(_needle.Width/2.0f, _needle.Height/2.0f);
                g.RotateTransform(angle);
                g.TranslateTransform(-_needle.Width/2.0f, -_needle.Height/2.0f);
                g.DrawImage(_needle, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }
   }
}