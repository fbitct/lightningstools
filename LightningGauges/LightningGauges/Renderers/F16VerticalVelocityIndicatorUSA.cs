using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public class F16VerticalVelocityIndicatorUSA : InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants

        private const string VVI_BACKGROUND_IMAGE_FILENAME = "vvi.bmp";
        private const string VVI_BACKGROUND_MASK_FILENAME = "vvi_mask.bmp";
        private const string VVI_OFF_FLAG_IMAGE_FILENAME = "vviflag.bmp";
        private const string VVI_OFF_FLAG_MASK_FILENAME = "vviflag_mask.bmp";
        private const string VVI_INDICATOR_LINE_IMAGE_FILENAME = "vvistrip.bmp";
        private const string VVI_INDICATOR_LINE_MASK_FILENAME = "vvistrip_mask.bmp";

        private const string VVI_NUMBER_TAPE_IMAGE_FILENAME = "vvinum.bmp";

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
        private bool _disposed;

        #endregion

        public F16VerticalVelocityIndicatorUSA()
        {
            InstrumentState = new F16VerticalVelocityIndicatorUSAInstrumentState();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + VVI_BACKGROUND_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + VVI_BACKGROUND_MASK_FILENAME
                    );
            }
            if (_offFlag == null)
            {
                _offFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + VVI_OFF_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + VVI_OFF_FLAG_MASK_FILENAME
                    );
            }
            if (_indicatorLine == null)
            {
                _indicatorLine = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + VVI_INDICATOR_LINE_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + VVI_INDICATOR_LINE_MASK_FILENAME
                    );
            }
            if (_numberTape == null)
            {
                _numberTape =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                            VVI_NUMBER_TAPE_IMAGE_FILENAME);
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16VerticalVelocityIndicatorUSAInstrumentState InstrumentState { get; set; }

        #region Instrument State

        [Serializable]
        public class F16VerticalVelocityIndicatorUSAInstrumentState : InstrumentStateBase
        {
            private const float MAX_VELOCITY = 6000;
            private const float MIN_VELOCITY = -6000;
            private float _verticalVelocityFeetPerMinute;

            public F16VerticalVelocityIndicatorUSAInstrumentState()
            {
                OffFlag = false;
                VerticalVelocityFeetPerMinute = 0.0f;
            }

            public float VerticalVelocityFeetPerMinute
            {
                get { return _verticalVelocityFeetPerMinute; }
                set
                {
                    var vv = value;
                    if (vv < MIN_VELOCITY) vv = MIN_VELOCITY;
                    if (vv > MAX_VELOCITY) vv = MAX_VELOCITY;
                    _verticalVelocityFeetPerMinute = vv;
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
                var width = _background.Image.Width;
                width -= 154;
                var height = _background.Image.Height - 29;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                //set the initial scale transformation 
                g.TranslateTransform(-76, 0);
                g.TranslateTransform(0, -15);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = g.Save();

                if (!InstrumentState.OffFlag)
                {
                    //draw the number tape
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    float translateX = 110;
                    float translateY = 236;
                    var pixelsPerHundredFeet = 4.75f;
                    var vv = InstrumentState.VerticalVelocityFeetPerMinute;
                    if (Math.Abs(vv) > 6000.0) vv = Math.Sign(vv)*6000.0f;
                    var verticalVelocityThousands = vv/1000.0f;
                    translateY -= (-pixelsPerHundredFeet*verticalVelocityThousands*10.0f);
                    translateY -= _numberTape.Height/2.0f;
                    g.TranslateTransform(translateX, translateY);
                    g.ScaleTransform(0.79f, 0.79f);
                    g.DrawImage(_numberTape, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }
                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);


                //draw the indicator line
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(0, 1);
                g.DrawImage(_indicatorLine.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);


                //draw the OFF flag
                if (InstrumentState.OffFlag)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.TranslateTransform(0, -3);
                    g.DrawImage(_offFlag.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }


                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }

        ~F16VerticalVelocityIndicatorUSA()
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
                    //Common.Util.DisposeObject(_offFlag);
                    //Common.Util.DisposeObject(_indicatorLine);
                    //Common.Util.DisposeObject(_numberTape);
                }
                _disposed = true;
            }
        }
    }
}