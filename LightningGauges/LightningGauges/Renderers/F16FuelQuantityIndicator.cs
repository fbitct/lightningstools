﻿using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public interface IF16FuelQuantityIndicator : IInstrumentRenderer
    {
        F16FuelQuantityIndicator.F16FuelQuantityIndicatorInstrumentState InstrumentState { get; set; }
        F16FuelQuantityIndicator.F16FuelQuantityIndicatorOptions Options { get; set; }
    }

    public class F16FuelQuantityIndicator : InstrumentRendererBase, IF16FuelQuantityIndicator
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
                    var qty = value;
                    if (float.IsNaN(qty) || float.IsInfinity(qty)) qty = 0;
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
                    var qty = value;
                    if (float.IsNaN(qty) || float.IsInfinity(qty)) qty = 0;
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
                    var qty = value;
                    if (float.IsNaN(qty) || float.IsInfinity(qty)) qty = 0;
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

        public override void Render(Graphics destinationGraphics, Rectangle destinationRectangle)
        {
            if (!_imagesLoaded)
            {
                LoadImageResources();
            }
            lock (_imagesLock)
            {
                //store the canvas's transform and clip settings so we can restore them later
                var initialState = destinationGraphics.Save();

                //set up the canvas scale and clipping region
                var width = 176;
                var height = 176;
                destinationGraphics.ResetTransform(); //clear any existing transforms
                destinationGraphics.SetClip(destinationRectangle); //set the clipping region on the graphics object to our render rectangle's boundaries
                destinationGraphics.FillRectangle(Brushes.Black, destinationRectangle);
                destinationGraphics.ScaleTransform(destinationRectangle.Width/(float) width, destinationRectangle.Height/(float) height);
                //set the initial scale transformation 
                destinationGraphics.TranslateTransform(-40, -40);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = destinationGraphics.Save();

                var hundredsDigit = (InstrumentState.TotalFuelQuantityPounds/100.0f)%10.0f;
                var thousandsDigit = (float) Math.Truncate((InstrumentState.TotalFuelQuantityPounds/1000.0f))%10.0f;
                var tenThousandsDigit = (float) Math.Truncate((InstrumentState.TotalFuelQuantityPounds/10000.0f)%10.0f);

                var pixelsPerDigit = 29.40f;
                float yOffsetToZero = -234;
                float xOffset = -130;
                //draw the ones digit
                {
                    xOffset = -100;
                    var yOffsetToActual = yOffsetToZero;
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                    destinationGraphics.TranslateTransform(xOffset, yOffsetToActual);
                    destinationGraphics.DrawImage(_digits, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                }

                //draw the tens digit
                {
                    xOffset = -116;
                    var yOffsetToActual = yOffsetToZero;
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                    destinationGraphics.TranslateTransform(xOffset, yOffsetToActual);
                    destinationGraphics.DrawImage(_digits, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                }

                //draw the hundreds digit
                {
                    xOffset = -132;
                    var yOffsetToActual = yOffsetToZero + (pixelsPerDigit*hundredsDigit);
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                    destinationGraphics.TranslateTransform(xOffset, yOffsetToActual);
                    destinationGraphics.DrawImage(_digits, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                }

                //draw the thousands digit
                {
                    xOffset = -148;
                    var yOffsetToActual = yOffsetToZero + (pixelsPerDigit*thousandsDigit);
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                    destinationGraphics.TranslateTransform(xOffset, yOffsetToActual);
                    destinationGraphics.DrawImage(_digits, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                }

                //draw the ten-thousands digit
                {
                    xOffset = -164;
                    var yOffsetToActual = yOffsetToZero + (pixelsPerDigit*tenThousandsDigit);
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                    destinationGraphics.TranslateTransform(xOffset, yOffsetToActual);
                    destinationGraphics.DrawImage(_digits, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                }

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                destinationGraphics.DrawImage(_background.MaskedImage,
                            new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height),
                            new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height),
                            GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                var baseAngle = 0.0f;
                //draw the aft/left needle
                var aftLeftNeedleAngle = baseAngle + GetAngle(InstrumentState.AftLeftFuelQuantityPounds);
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                destinationGraphics.TranslateTransform(_background.MaskedImage.Width/2.0f, _background.MaskedImage.Height/2.0f);
                destinationGraphics.RotateTransform(aftLeftNeedleAngle);
                destinationGraphics.TranslateTransform(-_background.MaskedImage.Width/2.0f, -_background.MaskedImage.Height/2.0f);
                if (Options.NeedleType == F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.CModel)
                {
                    destinationGraphics.DrawImage(_aftLeftArrowCModel.MaskedImage, new Point(0, 0));
                }
                else if (Options.NeedleType == F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.DModel)
                {
                    destinationGraphics.DrawImage(_aftLeftArrowDModel.MaskedImage, new Point(0, 0));
                }
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                //draw the fore/right needle
                var foreRightNeedleAngle = baseAngle + GetAngle(InstrumentState.ForeRightFuelQuantityPounds);
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                destinationGraphics.TranslateTransform(_background.MaskedImage.Width/2.0f, _background.MaskedImage.Height/2.0f);
                destinationGraphics.RotateTransform(foreRightNeedleAngle);
                destinationGraphics.TranslateTransform(-_background.MaskedImage.Width/2.0f, -_background.MaskedImage.Height/2.0f);
                if (Options.NeedleType == F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.CModel)
                {
                    destinationGraphics.DrawImage(_foreRightArrowCModel.MaskedImage, new Point(0, 0));
                }
                else if (Options.NeedleType == F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.DModel)
                {
                    destinationGraphics.DrawImage(_foreRightArrowDModel.MaskedImage, new Point(0, 0));
                }
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);


                //restore the canvas's transform and clip settings to what they were when we entered this method
                destinationGraphics.Restore(initialState);
            }
        }

        private float GetAngle(float fuelQuantityPounds)
        {
            return (fuelQuantityPounds/100.00f)*5.48f;
        }

    }
}