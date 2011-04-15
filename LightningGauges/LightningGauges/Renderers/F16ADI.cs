﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Common.SimSupport;
using System.IO;
using System.Drawing.Drawing2D;
using Common.Imaging;

namespace LightningGauges.Renderers
{
    public class F16ADI:InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants
        private static string IMAGES_FOLDER_NAME = new DirectoryInfo (Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)).FullName + Path.DirectorySeparatorChar + "images";
        private const string ADI_BACKGROUND_IMAGE_FILENAME = "adi.bmp";
        private const string ADI_BACKGROUND_MASK_FILENAME = "adi_mask.bmp";
        private const string ADI_BALL_IMAGE_FILENAME = "adiball.bmp";
        private const string ADI_ARROWS_IMAGE_FILENAME = "adiarrows.bmp";
        private const string ADI_ARROWS_MASK_FILENAME = "adiarrows_mask.bmp";
        private const string ADI_AUX_FLAG_IMAGE_FILENAME = "adiaux.bmp";
        private const string ADI_AUX_FLAG_MASK_FILENAME = "adiflags_mask.bmp";
        private const string ADI_GLIDESLOPE_FLAG_IMAGE_FILENAME = "adigsflag.bmp";
        private const string ADI_GLIDESLOPE_FLAG_MASK_FILENAME = "adiflags_mask.bmp";
        private const string ADI_LOCALIZER_FLAG_IMAGE_FILENAME = "adiloc.bmp";
        private const string ADI_LOCALIZER_FLAG_MASK_FILENAME = "adiflags_mask.bmp";
        private const string ADI_OFF_FLAG_IMAGE_FILENAME = "adioff.bmp";
        private const string ADI_OFF_FLAG_MASK_FILENAME = "adiflags_mask.bmp";
        private const string ADI_HORIZONTAL_BAR_IMAGE_FILENAME = "adihorbar.bmp";
        private const string ADI_HORIZONTAL_BAR_MASK_FILENAME = "adihorbar_mask.bmp";
        private const string ADI_VERTICAL_BAR_IMAGE_FILENAME = "adiverbar.bmp";
        private const string ADI_VERTICAL_BAR_MASK_FILENAME = "adiverbar_mask.bmp";
        private const string ADI_AIRPLANE_SYMBOL_IMAGE_FILENAME = "adiplane.bmp";
        private const string ADI_AIRPLANE_SYMBOL_MASK_FILENAME = "adiplane_mask.bmp";
        private const string ADI_SLIP_INDICATOR_BALL_IMAGE_FILENAME = "adislip.bmp";
        private const string ADI_SLIP_INDICATOR_BALL_MASK_FILENAME = "adislip_mask.bmp";
        #endregion

        #region Instance variables
        private static object _imagesLock = new object();
        private static ImageMaskPair _background;
        private static Image _ball;
        private static ImageMaskPair _arrows;
        private static ImageMaskPair _auxFlag;
        private static ImageMaskPair _glideslopeFlag;
        private static ImageMaskPair _localizerFlag;
        private static ImageMaskPair _offFlag;
        private static ImageMaskPair _horizontalBar;
        private static ImageMaskPair _verticalBar;
        private static ImageMaskPair _airplaneSymbol;
        private static ImageMaskPair _slipIndicatorBall;
        private static bool _imagesLoaded = false;
        private bool _disposed = false;
        #endregion

        public F16ADI()
            : base()
        {
            this.InstrumentState = new F16ADIInstrumentState();
        }
        #region Initialization Code
        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_BACKGROUND_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_BACKGROUND_MASK_FILENAME
                    );
            }

            if (_ball == null)
            {
                _ball = Common.Imaging.Util.LoadBitmapFromFile( IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_BALL_IMAGE_FILENAME);
            }

            if (_arrows == null)
            {
                _arrows = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_ARROWS_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_ARROWS_MASK_FILENAME
                    );
            }

            if (_auxFlag == null)
            {
                _auxFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_AUX_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_AUX_FLAG_MASK_FILENAME
                    );
            }

            if (_glideslopeFlag == null)
            {
                _glideslopeFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_GLIDESLOPE_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_GLIDESLOPE_FLAG_MASK_FILENAME
                    );
            }

            if (_localizerFlag == null)
            {
                _localizerFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_LOCALIZER_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_LOCALIZER_FLAG_MASK_FILENAME
                    );
            }

            if (_offFlag == null)
            {
                _offFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_OFF_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_OFF_FLAG_MASK_FILENAME
                    );
            }
            if (_horizontalBar == null)
            {
                _horizontalBar = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_HORIZONTAL_BAR_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_HORIZONTAL_BAR_MASK_FILENAME
                    );
            }
            if (_verticalBar == null)
            {
                _verticalBar = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_VERTICAL_BAR_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_VERTICAL_BAR_MASK_FILENAME
                    );
            }
            if (_airplaneSymbol == null)
            {
                _airplaneSymbol = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_AIRPLANE_SYMBOL_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_AIRPLANE_SYMBOL_MASK_FILENAME
                    );
            }

            if (_slipIndicatorBall == null)
            {
                _slipIndicatorBall = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_SLIP_INDICATOR_BALL_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ADI_SLIP_INDICATOR_BALL_MASK_FILENAME
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
                int width = _background.Image.Width - 66;
                int height = _background.Image.Height - 55;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform((float)bounds.Width / (float)width, (float)bounds.Height / (float)height); //set the initial scale transformation 
                g.TranslateTransform(-32, -31);

                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                GraphicsState basicState = g.Save();

                //draw the ball
                float pixelsPerDegreePitch = 2.0f;
                float pitchDegrees = this.InstrumentState.PitchDegrees;
                float rollDegrees = this.InstrumentState.RollDegrees;
                float centerPixelY = ((float)_ball.Height / 2.0f) - (pixelsPerDegreePitch * pitchDegrees);
                float topPixelY = centerPixelY - 80;
                float leftPixelX = ((float)_ball.Width / 2.0f) - 80;
                RectangleF sourceRect = new RectangleF(leftPixelX, topPixelY, 160, 160);
                RectangleF destRect = new RectangleF(48, 40, sourceRect.Width, sourceRect.Height);

                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(128, 118);
                g.RotateTransform(-rollDegrees);
                g.TranslateTransform(-128, -118);
                g.TranslateTransform(-2, 0);
                g.DrawImage(_ball, destRect, sourceRect, GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the arrows
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(128, 118);
                g.RotateTransform(-rollDegrees);
                g.TranslateTransform(-128, -118);
                g.TranslateTransform(0, -10);
                g.DrawImage(_arrows.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the airplane symbol
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(0, -5);
                g.DrawImage(_airplaneSymbol.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                if (this.InstrumentState.ShowCommandBars && !this.InstrumentState.OffFlag)
                {
                    //draw the localizer bar
                    {
                        float positionPct = this.InstrumentState.LocalizerDeviationDegrees / this.InstrumentState.LocalizerDeviationLimitDegrees;
                        if (Math.Abs(positionPct) <= 1.0f)
                        {
                            float canvasRange = 36.0f;
                            float pos = (canvasRange * positionPct);
                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                            g.TranslateTransform(pos, -10.0f);
                            g.DrawImage(_verticalBar.MaskedImage, new Point(0, 0));
                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                        }
                    }

                    //draw the glideslope bar
                    {
                        float positionPct = this.InstrumentState.GlideslopeDeviationDegrees / this.InstrumentState.GlideslopeDeviationLimitDegrees;
                        if (Math.Abs(positionPct) <= 1.0f)
                        {
                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                            float canvasRange = 46.0f;
                            float pos = (-canvasRange * positionPct);
                            g.TranslateTransform(0.0f, pos);
                            g.DrawImage(_horizontalBar.MaskedImage, new Point(0, 0));
                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                        }
                    }
                }

                //draw the localizer flag
                if (this.InstrumentState.LocalizerFlag || this.InstrumentState.OffFlag)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.RotateTransform(25);
                    g.TranslateTransform(85, -133);
                    g.DrawImage(_localizerFlag.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the glideslope flag
                if (this.InstrumentState.GlideslopeFlag || this.InstrumentState.OffFlag)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.RotateTransform(-25);
                    g.TranslateTransform(-113, -25);
                    g.DrawImage(_glideslopeFlag.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the aux flag
                if (this.InstrumentState.AuxFlag || this.InstrumentState.OffFlag)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.RotateTransform(-25);
                    g.TranslateTransform(-12, 102);
                    g.DrawImage(_auxFlag.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the off flag
                if (this.InstrumentState.OffFlag)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.RotateTransform(25);
                    g.TranslateTransform(-15, -8);
                    g.DrawImage(_offFlag.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the slip indicator ball
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_slipIndicatorBall.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);


                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }
        public F16ADIInstrumentState InstrumentState
        {
            get;
            set;
        }
        #region Instrument State
        [Serializable]
        public class F16ADIInstrumentState : InstrumentStateBase
        {
            private const float MIN_PITCH = -90;
            private const float MAX_PITCH = 90;
            private const float MIN_ROLL = -180;
            private const float MAX_ROLL = 180;
            private const float DEFAULT_GLIDESLOPE_DEVIATION_LIMIT_DEGREES=1.0F;
            private const float DEFAULT_LOCALIZER_DEVIATION_LIMIT_DEGREES=5.0F;
            private float _pitchDegrees = 0;
            private float _rollDegrees = 0;
            private float _glideslopeDeviationDegrees = 0;
            private float _glideslopeDeviationLimitDegrees = DEFAULT_GLIDESLOPE_DEVIATION_LIMIT_DEGREES;
            private float _localizerDeviationDegrees = 0;
            private float _localizerDeviationLimitDegrees = DEFAULT_LOCALIZER_DEVIATION_LIMIT_DEGREES;

            public F16ADIInstrumentState()
                : base()
            {
                this.PitchDegrees = 0;
                this.RollDegrees = 0;
                this.GlideslopeDeviationLimitDegrees = DEFAULT_GLIDESLOPE_DEVIATION_LIMIT_DEGREES;
                this.GlideslopeDeviationDegrees = 0;
                this.LocalizerDeviationLimitDegrees = DEFAULT_LOCALIZER_DEVIATION_LIMIT_DEGREES;
                this.LocalizerDeviationDegrees = 0;
                this.OffFlag = false;
                this.AuxFlag = false;
                this.GlideslopeFlag = false;
                this.LocalizerFlag = false;
                this.ShowCommandBars = false;
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
            public float LocalizerDeviationDegrees
            {
                get
                {
                    return _localizerDeviationDegrees;
                }
                set
                {
                    float degrees = value;
                    degrees %= 360.0f;
                    if (float.IsNaN(degrees) || float.IsInfinity(degrees))
                    {
                        degrees = 0;
                    }
                    _localizerDeviationDegrees = degrees;
                }
            }
            public float LocalizerDeviationLimitDegrees
            {
                get
                {
                    return _localizerDeviationLimitDegrees;
                }
                set
                {
                    float degrees = value;
                    degrees %= 360.0f;
                    if (float.IsInfinity(degrees) || float.IsNaN(degrees) || degrees ==0)
                    {
                        degrees = DEFAULT_LOCALIZER_DEVIATION_LIMIT_DEGREES;
                    }
                    _localizerDeviationLimitDegrees = degrees;
                }
            }
            public float GlideslopeDeviationLimitDegrees
            {
                get
                {
                    return _glideslopeDeviationLimitDegrees;
                }
                set
                {
                    float degrees = value;
                    degrees %= 360.0f;
                    if (float.IsNaN(degrees) || float.IsInfinity(degrees) || degrees ==0)
                    {
                        degrees = DEFAULT_GLIDESLOPE_DEVIATION_LIMIT_DEGREES;
                    }
                    _glideslopeDeviationLimitDegrees = degrees;
                }
            }
            public float GlideslopeDeviationDegrees
            {
                get
                {
                    return _glideslopeDeviationDegrees;
                }
                set
                {
                    float degrees = value;
                    degrees %= 360.0f;
                    if (float.IsInfinity(degrees) || float.IsNaN(degrees))
                    {
                        degrees = 0;
                    }
                    _glideslopeDeviationDegrees = degrees;
                }
            }
            public bool OffFlag
            {
                get;
                set;
            }
            public bool AuxFlag
            {
                get;
                set;
            }
            public bool GlideslopeFlag
            {
                get;
                set;
            }
            public bool LocalizerFlag
            {
                get;
                set;
            }
            public bool ShowCommandBars
            {
                get;
                set;
            }
        }
        #endregion
        ~F16ADI() 
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
                    //Common.Util.DisposeObject(_auxFlag);
                    //Common.Util.DisposeObject(_glideslopeFlag);
                    //Common.Util.DisposeObject(_localizerFlag);
                    //Common.Util.DisposeObject(_offFlag);
                    //Common.Util.DisposeObject(_horizontalBar);
                    //Common.Util.DisposeObject(_verticalBar);
                    //Common.Util.DisposeObject(_airplaneSymbol);
                    //Common.Util.DisposeObject(_slipIndicatorBall);

                }
                _disposed = true;
            }
        }
    }
}
