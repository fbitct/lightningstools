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
    public class F16StandbyADI : InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants
        private static string IMAGES_FOLDER_NAME = new DirectoryInfo (System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)).FullName + Path.DirectorySeparatorChar + "images";
        private const string BUADI_BACKGROUND_IMAGE_FILENAME = "buadi.bmp";
        private const string BUADI_BACKGROUND_MASK_FILENAME = "buadi_mask.bmp";
        private const string BUADI_BALL_IMAGE_FILENAME = "buadiball.bmp";
        private const string BUADI_ARROWS_IMAGE_FILENAME = "buadislip.bmp";
        private const string BUADI_ARROWS_MASK_FILENAME = "buadislip_mask.bmp";
        private const string BUADI_OFF_FLAG_IMAGE_FILENAME = "buadiflag.bmp";
        private const string BUADI_OFF_FLAG_MASK_FILENAME = "buadiflag_mask.bmp";
        private const string BUADI_AIRPLANE_SYMBOL_IMAGE_FILENAME = "buadiplane.bmp";
        private const string BUADI_AIRPLANE_SYMBOL_MASK_FILENAME = "buadiplane_mask.bmp";
        #endregion

        #region Instance variables
        private static object _imagesLock = new object();
        private static ImageMaskPair _background;
        private static Image _ball;
        private static ImageMaskPair _arrows;
        private static ImageMaskPair _offFlag;
        private static ImageMaskPair _airplaneSymbol;
        private static bool _imagesLoaded = false;
        private bool _disposed = false;
        #endregion

        public F16StandbyADI()
            : base()
        {
            this.InstrumentState = new F16StandbyADIInstrumentState();
        }
        #region Initialization Code
        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + BUADI_BACKGROUND_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + BUADI_BACKGROUND_MASK_FILENAME
                    );
            }

            if (_ball == null)
            {
                _ball = Common.Imaging.Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + BUADI_BALL_IMAGE_FILENAME);
            }

            if (_arrows == null)
            {
                _arrows = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + BUADI_ARROWS_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + BUADI_ARROWS_MASK_FILENAME
                    );
            }

            if (_offFlag == null)
            {
                _offFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + BUADI_OFF_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + BUADI_OFF_FLAG_MASK_FILENAME
                    );
            }
            if (_airplaneSymbol == null)
            {
                _airplaneSymbol = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + BUADI_AIRPLANE_SYMBOL_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + BUADI_AIRPLANE_SYMBOL_MASK_FILENAME
                    );
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
                int width = _background.Image.Width - 84;
                int height = _background.Image.Height - 84;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform((float)bounds.Width / (float)width, (float)bounds.Height / (float)height); //set the initial scale transformation 
                g.TranslateTransform(-42, -42);

                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                GraphicsState basicState = g.Save();

                //draw the ball
                float pixelsPerDegreePitch = 2.0f;
                float pitch = this.InstrumentState.PitchDegrees;
                float roll = this.InstrumentState.RollDegrees;
                float centerPixelY = ((float)_ball.Height / 2.0f) - (pixelsPerDegreePitch * pitch);
                float topPixelY = centerPixelY - 80;
                float leftPixelX = ((float)_ball.Width / 2.0f) - 73;
                RectangleF sourceRect = new RectangleF(leftPixelX, topPixelY, 160, 160);
                RectangleF destRect = new RectangleF(48, 48, sourceRect.Width, sourceRect.Height);

                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                float translateX = 127;
                float translateY = 125;

                g.TranslateTransform(0, 0);
                g.TranslateTransform(translateX, translateY);
                g.RotateTransform(-roll);
                g.TranslateTransform(-translateX, -translateY);
                g.DrawImage(_ball, destRect, sourceRect, GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the arrows
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(0, 0);
                g.TranslateTransform(translateX, translateY);
                g.RotateTransform(-roll);
                g.TranslateTransform(-translateX, -translateY);
                g.DrawImage(_arrows.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the airplane symbol
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(0, 3);
                g.DrawImage(_airplaneSymbol.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the off flag
                if (this.InstrumentState.OffFlag)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.DrawImage(_offFlag.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }


                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }
        public F16StandbyADIInstrumentState InstrumentState
        {
            get;
            set;
        }
        #region Instrument State
        [Serializable]
        public class F16StandbyADIInstrumentState : InstrumentStateBase
        {
            private const float MIN_PITCH = -90;
            private const float MAX_PITCH = 90;
            private const float MIN_ROLL = -180;
            private const float MAX_ROLL = 180;
            private float _pitchDegrees = 0;
            private float _rollDegrees = 0;

            public F16StandbyADIInstrumentState()
                : base()
            {
                this.PitchDegrees = 0;
                this.RollDegrees = 0;
                this.OffFlag = false;
            }
            public float PitchDegrees
            {
                get
                {
                    return _pitchDegrees;
                }
                set
                {
                    float pitch = value;
                    if (pitch < MIN_PITCH) pitch = MIN_PITCH;
                    if (pitch > MAX_PITCH) pitch = MAX_PITCH;
                    if (float.IsNaN(pitch) || float.IsInfinity(pitch))
                    {
                        pitch = 0;
                    }
                    _pitchDegrees = pitch;
                }
            }
            public float RollDegrees
            {
                get
                {
                    return _rollDegrees;
                }
                set
                {
                    float roll = value;
                    if (roll < MIN_ROLL) roll = MIN_ROLL;
                    if (roll > MAX_ROLL) roll = MAX_ROLL;
                    if (float.IsInfinity(roll) || float.IsNaN(roll))
                    {
                        roll = 0;
                    }
                    _rollDegrees = roll;
                }
            }
            public bool OffFlag
            {
                get;
                set;
            }
        }
        #endregion
        ~F16StandbyADI()
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
                    //Common.Util.DisposeObject(_ball);
                    //Common.Util.DisposeObject(_arrows);
                    //Common.Util.DisposeObject(_offFlag);
                    //Common.Util.DisposeObject(_airplaneSymbol);

                }
                _disposed = true;
            }
        }
    }
}
