﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public class F16FuelQuantityIndicator : InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants

        private const string FUEL_BACKGROUND_IMAGE_FILENAME = "fuel.bmp";
        private const string FUEL_BACKGROUND_MASK_FILENAME = "fuel_mask.bmp";

        private const string FUEL_FORE_RIGHT_C_MODEL_IMAGE_FILENAME = "fuelfrarrow.bmp";
        private const string FUEL_FORE_RIGHT_C_MODEL_MASK_FILENAME = "fuelfrarrow_mask.bmp";
        private const string FUEL_AFT_LEFT_C_MODEL_IMAGE_FILENAME = "fuelalarrow.bmp";
        private const string FUEL_AFT_LEFT_C_MODEL_MASK_FILENAME = "fuelalarrow_mask.bmp";

        private const string FUEL_FORE_RIGHT_D_MODEL_IMAGE_FILENAME = "fuelfrarrowd.bmp";
        private const string FUEL_FORE_RIGHT_D_MODEL_MASK_FILENAME = "fuelfrarrowd_mask.bmp";
        private const string FUEL_AFT_LEFT_D_MODEL_IMAGE_FILENAME = "fuelalarrowd.bmp";
        private const string FUEL_AFT_LEFT_D_MODEL_MASK_FILENAME = "fuelalarrowd_mask.bmp";

        private const string FUEL_DIGITS_FILENAME = "ffnum.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static readonly object _imagesLock = new object();
        private static ImageMaskPair _background;
        private static ImageMaskPair _foreRightArrowCModel;
        private static ImageMaskPair _foreRightArrowDModel;
        private static ImageMaskPair _aftLeftArrowCModel;
        private static ImageMaskPair _aftLeftArrowDModel;
        private static Bitmap _digits;
        private static bool _imagesLoaded;
        private bool _disposed;

        #endregion

        public F16FuelQuantityIndicator()
        {
            InstrumentState = new F16FuelQuantityIndicatorInstrumentState();
            Options = new F16FuelQuantityIndicatorOptions();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + FUEL_BACKGROUND_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + FUEL_BACKGROUND_MASK_FILENAME
                    );
            }
            if (_foreRightArrowCModel == null)
            {
                _foreRightArrowCModel = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + FUEL_FORE_RIGHT_C_MODEL_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + FUEL_FORE_RIGHT_C_MODEL_MASK_FILENAME
                    );
                _foreRightArrowCModel.Use1BitAlpha = true;
            }

            if (_foreRightArrowDModel == null)
            {
                _foreRightArrowDModel = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + FUEL_FORE_RIGHT_D_MODEL_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + FUEL_FORE_RIGHT_D_MODEL_MASK_FILENAME
                    );
                _foreRightArrowDModel.Use1BitAlpha = true;
            }

            if (_aftLeftArrowCModel == null)
            {
                _aftLeftArrowCModel = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + FUEL_AFT_LEFT_C_MODEL_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + FUEL_AFT_LEFT_C_MODEL_MASK_FILENAME
                    );
                _aftLeftArrowCModel.Use1BitAlpha = true;
            }

            if (_aftLeftArrowDModel == null)
            {
                _aftLeftArrowDModel = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + FUEL_AFT_LEFT_D_MODEL_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + FUEL_AFT_LEFT_D_MODEL_MASK_FILENAME
                    );
                _aftLeftArrowDModel.Use1BitAlpha = true;
            }

            if (_digits == null)
            {
                _digits =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + FUEL_DIGITS_FILENAME);
                _digits.MakeTransparent(Color.Black);
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16FuelQuantityIndicatorInstrumentState InstrumentState { get; set; }
        public F16FuelQuantityIndicatorOptions Options { get; set; }

        #region Instrument State

        [Serializable]
        public class F16FuelQuantityIndicatorInstrumentState : InstrumentStateBase
        {
            private const float MAX_FUEL = 99999;
            private float _aftLeftFuelQuantityPounds;
            private float _foreRightFuelQuantityPounds;
            private float _totalFuelQuantityPounds;

            public float ForeRightFuelQuantityPounds
            {
                get { return _foreRightFuelQuantityPounds; }
                set
                {
                    float qty = value;
                    if (qty < 0) qty = 0;
                    if (qty > MAX_FUEL) qty = MAX_FUEL;
                    _foreRightFuelQuantityPounds = qty;
                }
            }

            public float AftLeftFuelQuantityPounds
            {
                get { return _aftLeftFuelQuantityPounds; }
                set
                {
                    float qty = value;
                    if (qty < 0) qty = 0;
                    if (qty > MAX_FUEL) qty = MAX_FUEL;
                    _aftLeftFuelQuantityPounds = qty;
                }
            }

            public float TotalFuelQuantityPounds
            {
                get { return _totalFuelQuantityPounds; }
                set
                {
                    float qty = value;
                    if (qty < 0) qty = 0;
                    if (qty > MAX_FUEL) qty = MAX_FUEL;
                    _totalFuelQuantityPounds = qty;
                }
            }
        }

        #endregion

        #region Options Class

        public class F16FuelQuantityIndicatorOptions
        {
            #region F16FuelQuantityNeedleType enum

            public enum F16FuelQuantityNeedleType
            {
                CModel,
                DModel
            }

            #endregion

            public F16FuelQuantityNeedleType NeedleType { get; set; }
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
                GraphicsState initialState = g.Save();

                //set up the canvas scale and clipping region
                int width = 176;
                int height = 176;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                    //set the initial scale transformation 
                g.TranslateTransform(-40, -40);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                GraphicsState basicState = g.Save();

                float hundredsDigit = (InstrumentState.TotalFuelQuantityPounds/100.0f)%10.0f;
                float thousandsDigit = (float) Math.Truncate((InstrumentState.TotalFuelQuantityPounds/1000.0f))%10.0f;
                var tenThousandsDigit = (float) Math.Truncate((InstrumentState.TotalFuelQuantityPounds/10000.0f)%10.0f);

                float pixelsPerDigit = 29.40f;
                float yOffsetToZero = -234;
                float xOffset = -130;
                //draw the ones digit
                {
                    xOffset = -100;
                    float yOffsetToActual = yOffsetToZero;
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.TranslateTransform(xOffset, yOffsetToActual);
                    g.DrawImage(_digits, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the tens digit
                {
                    xOffset = -116;
                    float yOffsetToActual = yOffsetToZero;
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.TranslateTransform(xOffset, yOffsetToActual);
                    g.DrawImage(_digits, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the hundreds digit
                {
                    xOffset = -132;
                    float yOffsetToActual = yOffsetToZero + (pixelsPerDigit*hundredsDigit);
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.TranslateTransform(xOffset, yOffsetToActual);
                    g.DrawImage(_digits, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the thousands digit
                {
                    xOffset = -148;
                    float yOffsetToActual = yOffsetToZero + (pixelsPerDigit*thousandsDigit);
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.TranslateTransform(xOffset, yOffsetToActual);
                    g.DrawImage(_digits, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the ten-thousands digit
                {
                    xOffset = -164;
                    float yOffsetToActual = yOffsetToZero + (pixelsPerDigit*tenThousandsDigit);
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.TranslateTransform(xOffset, yOffsetToActual);
                    g.DrawImage(_digits, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background.MaskedImage,
                            new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height),
                            new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height),
                            GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                float baseAngle = 0.0f;
                //draw the aft/left needle
                float aftLeftNeedleAngle = baseAngle + GetAngle(InstrumentState.AftLeftFuelQuantityPounds);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(_background.MaskedImage.Width/2.0f, _background.MaskedImage.Height/2.0f);
                g.RotateTransform(aftLeftNeedleAngle);
                g.TranslateTransform(-_background.MaskedImage.Width/2.0f, -_background.MaskedImage.Height/2.0f);
                if (Options.NeedleType == F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.CModel)
                {
                    g.DrawImage(_aftLeftArrowCModel.MaskedImage, new Point(0, 0));
                }
                else if (Options.NeedleType == F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.DModel)
                {
                    g.DrawImage(_aftLeftArrowDModel.MaskedImage, new Point(0, 0));
                }
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the fore/right needle
                float foreRightNeedleAngle = baseAngle + GetAngle(InstrumentState.ForeRightFuelQuantityPounds);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(_background.MaskedImage.Width/2.0f, _background.MaskedImage.Height/2.0f);
                g.RotateTransform(foreRightNeedleAngle);
                g.TranslateTransform(-_background.MaskedImage.Width/2.0f, -_background.MaskedImage.Height/2.0f);
                if (Options.NeedleType == F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.CModel)
                {
                    g.DrawImage(_foreRightArrowCModel.MaskedImage, new Point(0, 0));
                }
                else if (Options.NeedleType == F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.DModel)
                {
                    g.DrawImage(_foreRightArrowDModel.MaskedImage, new Point(0, 0));
                }
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);


                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }

        private float GetAngle(float fuelQuantityPounds)
        {
            return (fuelQuantityPounds/100.00f)*5.48f;
        }

        ~F16FuelQuantityIndicator()
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
                    //Common.Util.DisposeObject(_foreRightArrowCModel);
                    //Common.Util.DisposeObject(_foreRightArrowDModel);
                    //Common.Util.DisposeObject(_aftLeftArrowCModel);
                    //Common.Util.DisposeObject(_aftLeftArrowDModel);
                    //Common.Util.DisposeObject(_digits);
                }
                _disposed = true;
            }
        }
    }
}