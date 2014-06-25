using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public interface IF16AngleOfAttackIndicator:IInstrumentRenderer

    {
        F16AngleOfAttackIndicator.F16AngleOfAttackIndicatorInstrumentState InstrumentState { get; set; }
    }

    public class F16AngleOfAttackIndicator : InstrumentRendererBase, IF16AngleOfAttackIndicator
    {
        #region Image Location Constants

        private const string AOA_BACKGROUND_IMAGE_FILENAME = "aoa.bmp";
        private const string AOA_BACKGROUND_MASK_FILENAME = "aoa_mask.bmp";
        private const string AOA_OFF_FLAG_IMAGE_FILENAME = "aoaflag.bmp";
        private const string AOA_OFF_FLAG_MASK_FILENAME = "aoaflag_mask.bmp";
        private const string AOA_INDICATOR_LINE_IMAGE_FILENAME = "aoastrip.bmp";
        private const string AOA_INDICATOR_LINE_MASK_FILENAME = "aoastrip_mask.bmp";

        private const string AOA_NUMBER_TAPE_IMAGE_FILENAME = "aoanum.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static readonly object _imagesLock = new object();
        private static ImageMaskPair _background;
        private static ImageMaskPair _offFlag;
        private static ImageMaskPair _indicatorLine;
        private static Bitmap _numberTape;
        private static bool _imagesLoaded;

        #endregion

        public F16AngleOfAttackIndicator()
        {
            InstrumentState = new F16AngleOfAttackIndicatorInstrumentState();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + AOA_BACKGROUND_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + AOA_BACKGROUND_MASK_FILENAME
                    );
            }
            if (_offFlag == null)
            {
                _offFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + AOA_OFF_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + AOA_OFF_FLAG_MASK_FILENAME
                    );
            }
            if (_indicatorLine == null)
            {
                _indicatorLine = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + AOA_INDICATOR_LINE_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + AOA_INDICATOR_LINE_MASK_FILENAME
                    );
            }
            if (_numberTape == null)
            {
                _numberTape =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                            AOA_NUMBER_TAPE_IMAGE_FILENAME);
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16AngleOfAttackIndicatorInstrumentState InstrumentState { get; set; }

        #region Instrument State

        [Serializable]
        public class F16AngleOfAttackIndicatorInstrumentState : InstrumentStateBase
        {
            private const float MIN_AOA = -35.0F;
            private const float MAX_AOA = 35.0f;
            private float _angleOfAttackDegrees;

            public F16AngleOfAttackIndicatorInstrumentState()
            {
                OffFlag = false;
                AngleOfAttackDegrees = 0.0f;
            }

            public float AngleOfAttackDegrees
            {
                get { return _angleOfAttackDegrees; }
                set
                {
                    var degrees = value;
                    if (float.IsNaN(degrees) || float.IsInfinity(degrees)) degrees = 0;
                    degrees %= 360.0F;
                    if (degrees < MIN_AOA) degrees = MIN_AOA;
                    if (degrees > MAX_AOA) degrees = MAX_AOA;
                    _angleOfAttackDegrees = degrees;
                }
            }

            public bool OffFlag { get; set; }
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
                var width = _background.Image.Width;
                width -= 153;
                var height = _background.Image.Height - 28;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                //set the initial scale transformation 

                g.TranslateTransform(0, -12);
                g.TranslateTransform(-75, 0);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = g.Save();

                if (!InstrumentState.OffFlag)
                {
                    //draw the number tape
                    var aoaDegrees = InstrumentState.AngleOfAttackDegrees;
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    float translateX = 105;
                    float translateY = 253;
                    var pixelsPerDegreeAoa = 10f;
                    translateY -= (-pixelsPerDegreeAoa*aoaDegrees);
                    translateY -= _numberTape.Height/2.0f;
                    g.TranslateTransform(translateX, translateY);
                    g.ScaleTransform(0.75f, 0.75f);
                    g.DrawImage(_numberTape, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }
                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);


                //draw the indicator line
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(0, 0);
                g.DrawImage(_indicatorLine.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);


                //draw the OFF flag
                if (InstrumentState.OffFlag)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.TranslateTransform(0, 0);
                    g.DrawImage(_offFlag.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }


                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }

    }
}