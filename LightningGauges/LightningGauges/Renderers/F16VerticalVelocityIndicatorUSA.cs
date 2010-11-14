using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Common.SimSupport;
using System.IO;
using System.Drawing.Drawing2D;
using Common.Imaging;

namespace LightningGauges.Renderers
{
    public class F16VerticalVelocityIndicatorUSA : InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants
        private static string IMAGES_FOLDER_NAME = new DirectoryInfo (System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)).FullName + Path.DirectorySeparatorChar + "images";
        private const string VVI_BACKGROUND_IMAGE_FILENAME = "vvi.bmp";
        private const string VVI_BACKGROUND_MASK_FILENAME = "vvi_mask.bmp";
        private const string VVI_OFF_FLAG_IMAGE_FILENAME = "vviflag.bmp";
        private const string VVI_OFF_FLAG_MASK_FILENAME = "vviflag_mask.bmp";
        private const string VVI_INDICATOR_LINE_IMAGE_FILENAME = "vvistrip.bmp";
        private const string VVI_INDICATOR_LINE_MASK_FILENAME = "vvistrip_mask.bmp";

        private const string VVI_NUMBER_TAPE_IMAGE_FILENAME = "vvinum.bmp";

        #endregion

        #region Instance variables
        private static object _imagesLock = new object();
        private static ImageMaskPair _background;
        private static ImageMaskPair _offFlag;
        private static ImageMaskPair _indicatorLine;
        private static Bitmap _numberTape;
        private static bool _imagesLoaded = false;
        private bool _disposed = false;
        #endregion

        public F16VerticalVelocityIndicatorUSA()
            : base()
        {
            this.InstrumentState = new F16VerticalVelocityIndicatorUSAInstrumentState();
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
                _numberTape = (Bitmap)Common.Imaging.Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + VVI_NUMBER_TAPE_IMAGE_FILENAME);
            }
            _imagesLoaded = true;
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
                int width = _background.Image.Width;
                width -= 154;
                int height = _background.Image.Height - 29;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform((float)bounds.Width / (float)width, (float)bounds.Height / (float)height); //set the initial scale transformation 
                g.TranslateTransform(-76, 0);
                g.TranslateTransform(0, -15);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                GraphicsState basicState = g.Save();

                if (!this.InstrumentState.OffFlag)
                {
                    //draw the number tape
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    float translateX = 110;
                    float translateY = 236;
                    float pixelsPerHundredFeet = 4.75f;
                    float vv = this.InstrumentState.VerticalVelocityFeetPerMinute;
                    if (Math.Abs(vv) > 6000.0) vv = (float)Math.Sign(vv) * 6000.0f;
                    float verticalVelocityThousands = vv / 1000.0f;
                    translateY -= (-pixelsPerHundredFeet * verticalVelocityThousands * 10.0f);
                    translateY -= (float)_numberTape.Height / 2.0f;
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
                if (this.InstrumentState.OffFlag)
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
        public F16VerticalVelocityIndicatorUSAInstrumentState InstrumentState
        {
            get;
            set;
        }
        #region Instrument State
        [Serializable]
        public class F16VerticalVelocityIndicatorUSAInstrumentState : InstrumentStateBase
        {
            private const float MAX_VELOCITY = 6000;
            private const float MIN_VELOCITY = -6000;
            private float _verticalVelocityFeetPerMinute=0;
            public F16VerticalVelocityIndicatorUSAInstrumentState():base()
            {
                this.OffFlag = false;
                this.VerticalVelocityFeetPerMinute = 0.0f;
            }
            public float VerticalVelocityFeetPerMinute
            {
                get
                {
                    return _verticalVelocityFeetPerMinute;
                }
                set
                {
                    float vv = value;
                    if (vv < MIN_VELOCITY) vv = MIN_VELOCITY;
                    if (vv > MAX_VELOCITY) vv = MAX_VELOCITY;
                    _verticalVelocityFeetPerMinute = vv;
                }
            }

            public bool OffFlag
            {
                get;
                set;
            }
        }
        #endregion
        ~F16VerticalVelocityIndicatorUSA()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
