﻿using System;
using Common.Drawing;
using Common.Drawing.Text;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers.F16
{
    public interface IAltimeter:IInstrumentRenderer
    {
        Altimeter.AltimeterInstrumentState InstrumentState { get; set; }
        Altimeter.AltimeterOptions Options { get; set; }
    }

    public class Altimeter : InstrumentRendererBase, IAltimeter
    {
        #region Image Location Constants

        private const string ALT_BACKGROUND_ELECTROMECHANICAL_IMAGE_FILENAME = "alt.bmp";
        private const string ALT_BACKGROUND_ELECTROMECHANICAL_MASK_FILENAME = "alt_mask.bmp";
        private const string ALT_BACKGROUND_ELECTROMECHANICAL_NOFLAG_IMAGE_FILENAME = "altnoflag.bmp";
        private const string ALT_BACKGROUND_ELECTRONIC_IMAGE_FILENAME = "alt2.bmp";
        private const string ALT_BACKGROUND_ELECTRONIC_NOFLAG_IMAGE_FILENAME = "alt2noflag.bmp";
        private const string ALT_BACKGROUND_ELECTRONIC_MASK_FILENAME = "alt2_mask.bmp";
        private const string ALT_NEEDLE_IMAGE_FILENAME = "altarrow.bmp";
        private const string ALT_NEEDLE_MASK_FILENAME = "altarrow_mask.bmp";
        private const string ALT_TEN_THOUSANDS_IMAGE_FILENAME = "alttenthou.bmp";
        private const string ALT_THOUSANDS_IMAGE_FILENAME = "altthou.bmp";
        private const string ALT_HUNDREDS_IMAGE_FILENAME = "altten.bmp";
        private const string ALT_DIGIT_FONT_FILENAME = "lcddot.ttf";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static readonly object ImagesLock = new object();
        private static ImageMaskPair _backgroundElectroMechanical;
        private static ImageMaskPair _backgroundElectroMechanicalNoFlag;
        private static ImageMaskPair _backgroundElectronic;
        private static ImageMaskPair _backgroundElectronicNoFlag;
        private static ImageMaskPair _needle;
        private static Bitmap _tenThousandsDigitsElectroMechanical;
        private static Bitmap _thousandsDigitsElectroMechanical;
        private static Bitmap _hundredsDigitsElectroMechanical;
        private static Font _baroPressureFont;
        private static Font _unitsFont;
        private static Font _baroPressureFont2;
        private static Font _unitsFont2;
        private readonly PrivateFontCollection _fonts = new PrivateFontCollection();
        private static Font _bigDigitsFont;
        private static Font _littleDigitsFont;
        private bool _imagesLoaded;

        #endregion

        public Altimeter()
        {
            InstrumentState = new AltimeterInstrumentState();
            Options = new AltimeterOptions
                          {
                              Style = AltimeterOptions.F16AltimeterStyle.Electromechanical
                          };
            LoadFonts();
        }

        #region Initialization Code

        private void LoadFonts()
        {
            _fonts.AddFontFile("ISISDigits.ttf");
            _fonts.AddFontFile(ALT_DIGIT_FONT_FILENAME);
        }

        private void LoadImageResources()
        {
            if (_backgroundElectroMechanical == null)
            {
                _backgroundElectroMechanical = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ALT_BACKGROUND_ELECTROMECHANICAL_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ALT_BACKGROUND_ELECTROMECHANICAL_MASK_FILENAME
                    );
            }
            if (_backgroundElectroMechanicalNoFlag == null)
            {
                _backgroundElectroMechanicalNoFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                    ALT_BACKGROUND_ELECTROMECHANICAL_NOFLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ALT_BACKGROUND_ELECTROMECHANICAL_MASK_FILENAME
                    );
            }
            if (_backgroundElectronic == null)
            {
                _backgroundElectronic = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ALT_BACKGROUND_ELECTRONIC_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ALT_BACKGROUND_ELECTRONIC_MASK_FILENAME
                    );
                _backgroundElectronic.Use1BitAlpha = true;
            }
            if (_backgroundElectronicNoFlag == null)
            {
                _backgroundElectronicNoFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ALT_BACKGROUND_ELECTRONIC_NOFLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ALT_BACKGROUND_ELECTRONIC_MASK_FILENAME
                    );
                _backgroundElectronicNoFlag.Use1BitAlpha = true;
            }
            if (_needle == null)
            {
                _needle = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ALT_NEEDLE_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ALT_NEEDLE_MASK_FILENAME
                    );
            }
            if (_tenThousandsDigitsElectroMechanical == null)
            {
                _tenThousandsDigitsElectroMechanical =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                            ALT_TEN_THOUSANDS_IMAGE_FILENAME);
                _tenThousandsDigitsElectroMechanical.MakeTransparent(Color.Black);
            }

            if (_thousandsDigitsElectroMechanical == null)
            {
                _thousandsDigitsElectroMechanical =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                            ALT_THOUSANDS_IMAGE_FILENAME);
                _thousandsDigitsElectroMechanical.MakeTransparent(Color.Black);
            }
            if (_hundredsDigitsElectroMechanical == null)
            {
                _hundredsDigitsElectroMechanical =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                            ALT_HUNDREDS_IMAGE_FILENAME);
                _hundredsDigitsElectroMechanical.MakeTransparent(Color.Black);
            }
            _imagesLoaded = true;
        }

        #endregion

        private AltimeterInstrumentState _instrumentState;
        public AltimeterInstrumentState InstrumentState { 
            get
            {
                return _instrumentState;

            } 
            set
            {
                _instrumentState = value;
            } 
        }
        public AltimeterOptions Options { get; set; }

        #region Options Class

        public class AltimeterOptions
        {
            #region F16AltimeterStyle enum

            public enum F16AltimeterStyle
            {
                Electromechanical,
                Electronic
            }

            #endregion

            #region PressureUnits enum

            public enum PressureUnits
            {
                InchesOfMercury,
                Millibars
            }

            #endregion

            public AltimeterOptions()
            {
                Style = F16AltimeterStyle.Electromechanical;
                PressureAltitudeUnits = PressureUnits.InchesOfMercury;
            }

            public F16AltimeterStyle Style { get; set; }
            public PressureUnits PressureAltitudeUnits { get; set; }
        }

        #endregion

        #region Instrument State

        [Serializable]
        public class AltimeterInstrumentState : InstrumentStateBase
        {
            public AltimeterInstrumentState()
            {
                PneumaticModeFlag = false; //Falcas 04/09/12 Set to false so that default is no flag.
                StandbyModeFlag = false; //Falcas 04/09/12 Set to false so that default is no flag.
                ElectricModeFlag = false;
                IndicatedAltitudeFeetMSL = 0.0f;
                BarometricPressure = 2992f;
            }

            private float _barometricPressure;
            public float BarometricPressure
            {
                get { return _barometricPressure; }
                set 
                { 
                    if (float.IsNaN(value) || float.IsInfinity(value)) value = 0;
                    _barometricPressure = value;
                }
            }
            private float _indicatedAltitudeFeetMSL;
            public float IndicatedAltitudeFeetMSL
            {
                get { return _indicatedAltitudeFeetMSL; }
                set
                {
                    if (float.IsNaN(value) || float.IsInfinity(value)) value = 0;
                    if (value > MAX_ALT) value = MAX_ALT;
                    _indicatedAltitudeFeetMSL = value;
                }
            }
            private const int MAX_ALT = 1000*1000;
            public bool StandbyModeFlag { get; set; }
            public bool NormalModeFlag { get; set; }
            public bool PneumaticModeFlag { get; set; }
            public bool ElectricModeFlag { get; set; }
        }

        #endregion


        public override void Render(Graphics destinationGraphics, Rectangle destinationRectangle)
        {
            if (!_imagesLoaded)
            {
                LoadImageResources();
            }
            lock (ImagesLock)
            {
                //store the canvas's transform and clip settings so we can restore them later
                var initialState = destinationGraphics.Save();
                var absIndicatedAltitude = Math.Abs(InstrumentState.IndicatedAltitudeFeetMSL);
                //set up the canvas scale and clipping region
                var width = 0;
                var height = 0;
                switch (Options.Style)
                {
                    case AltimeterOptions.F16AltimeterStyle.Electromechanical:
                        width = _backgroundElectroMechanical.Image.Width - 40;
                        height = _backgroundElectroMechanical.Image.Height - 40;
                        if (InstrumentState.IndicatedAltitudeFeetMSL < 0)
                            absIndicatedAltitude = 99999.99f - absIndicatedAltitude;
                        break;
                    case AltimeterOptions.F16AltimeterStyle.Electronic:
                        width = _backgroundElectronic.Image.Width - 40;
                        height = _backgroundElectronic.Image.Height - 40;
                        break;
                }

                destinationGraphics.ResetTransform(); //clear any existing transforms
                destinationGraphics.SetClip(destinationRectangle); //set the clipping region on the graphics object to our render rectangle's boundaries
                destinationGraphics.FillRectangle(Brushes.Black, destinationRectangle);
                destinationGraphics.ScaleTransform(destinationRectangle.Width/(float) width, destinationRectangle.Height/(float) height);
                //set the initial scale transformation 
                destinationGraphics.TranslateTransform(-20, -8);

                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = destinationGraphics.Save();

                //calculate digits
                float tenThousands = (int) Math.Floor((Math.Abs(absIndicatedAltitude)/10000.0f)%10);
                float thousands = (int) Math.Floor((Math.Abs(absIndicatedAltitude)/1000.0f)%10);
                var hundreds = (Math.Abs(absIndicatedAltitude)/100.0f)%10;

                switch (Options.Style)
                {
                    case AltimeterOptions.F16AltimeterStyle.Electromechanical:
                        {
                            //draw the altitude digits
                            const float digitHeights = 26.5f;
                            const float translateX = -130;
                            const float translateY = -272;

                            //draw ten-thousands digit
                            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                            destinationGraphics.TranslateTransform(translateX, translateY);
                            destinationGraphics.TranslateTransform(0, digitHeights*tenThousands);
                            destinationGraphics.DrawImageFast(_tenThousandsDigitsElectroMechanical, new Point(0, 0));
                            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                            //draw thousands digit
                            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                            destinationGraphics.TranslateTransform(translateX, translateY);
                            destinationGraphics.TranslateTransform(0, digitHeights*thousands);
                            destinationGraphics.DrawImageFast(_thousandsDigitsElectroMechanical, new Point(0, 0));
                            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                            //draw hundreds digit
                            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                            destinationGraphics.TranslateTransform(translateX, translateY);
                            destinationGraphics.TranslateTransform(0, digitHeights * hundreds);
                            destinationGraphics.DrawImageFast(_hundredsDigitsElectroMechanical, new Point(0, 0));
                            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                            //draw the barometric pressure area
                            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                            var topRectangle = new RectangleF(0, 0, width, 42);
                            destinationGraphics.FillRectangle(Brushes.Black, topRectangle);
                            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                            float barometricPressureAreaWidth = 65;
                            float unitAreaWidth = 65;
                            var barometricPressureStringFormat = new StringFormat();
                            barometricPressureStringFormat.Alignment = StringAlignment.Far;
                            barometricPressureStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                                                         StringFormatFlags.NoWrap;
                            barometricPressureStringFormat.LineAlignment = StringAlignment.Far;
                            barometricPressureStringFormat.Trimming = StringTrimming.None;

                            var pressure = InstrumentState.BarometricPressure;
                            var barometricPressureRectangle = new RectangleF(topRectangle.Width - barometricPressureAreaWidth - 15,
                                                                             20, barometricPressureAreaWidth,
                                                                             topRectangle.Height - 20);
                            var unitsRectangle = new RectangleF(topRectangle.Width - unitAreaWidth - 15,
                                                                             20, unitAreaWidth,
                                                                             topRectangle.Height - 20);
                            var barometricPressureBrush = Brushes.White;

                            string baroString = null;
                            string unitsString = null;
                            if (Options.PressureAltitudeUnits == AltimeterOptions.PressureUnits.InchesOfMercury)
                            {
                                baroString = string.Format("{0:#0.00}", pressure / 100);
                                barometricPressureRectangle = new RectangleF(topRectangle.Width - barometricPressureAreaWidth - 18,
                                                                             136, barometricPressureAreaWidth,
                                                                             topRectangle.Height - 20);
                                unitsString = "IN. HG";
                                unitsRectangle = new RectangleF(topRectangle.Width - unitAreaWidth - 34,
                                                                             136, unitAreaWidth,
                                                                             topRectangle.Height - 40);
                            }
                            else if (Options.PressureAltitudeUnits == AltimeterOptions.PressureUnits.Millibars)
                            {
                                baroString = string.Format("{0:###0}", pressure);
                                barometricPressureRectangle = new RectangleF(topRectangle.Width - barometricPressureAreaWidth - 22,
                                                                             136, barometricPressureAreaWidth,
                                                                             topRectangle.Height - 20);
                                unitsString = "mb";
                                unitsRectangle = new RectangleF(topRectangle.Width - unitAreaWidth - 38,
                                                                             136, unitAreaWidth,
                                                                             topRectangle.Height - 40);
                            }
                            if (_baroPressureFont ==null) 
                            {
                                _baroPressureFont = new Font(_fonts.Families[0], 14, FontStyle.Bold, GraphicsUnit.Point);
                            }
                            destinationGraphics.DrawStringFast(baroString, _baroPressureFont,
                                           barometricPressureBrush, barometricPressureRectangle, barometricPressureStringFormat);
                            
                            if (_unitsFont ==null) 
                            {
                                _unitsFont = new Font(_fonts.Families[0], 4, FontStyle.Bold, GraphicsUnit.Point);
                            }
                            destinationGraphics.DrawStringFast(unitsString, _unitsFont,
                                           barometricPressureBrush, unitsRectangle, barometricPressureStringFormat);
                            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);    
                        }
                        break;
                    case AltimeterOptions.F16AltimeterStyle.Electronic:
                        {
                            const float bigDigitsX = 80;
                            const float bigDigitsY = 103;
                            const float bigDigitsWidth = 50;
                            const float bigDigitsHeight = 35;
                            const float littleDigitWidth = 50;
                            const float littleDigitHeight = 30;
                            var bigDigitsRect = new RectangleF(bigDigitsX, bigDigitsY, bigDigitsWidth, bigDigitsHeight);
                            var littleDigitsRect = new RectangleF(bigDigitsRect.Right + 7, bigDigitsRect.Y + 4, littleDigitWidth,
                                                                  littleDigitHeight);
                            var onesRect =
                                new RectangleF(littleDigitsRect.X + littleDigitsRect.Width - (littleDigitsRect.Width / 3.0f),
                                               littleDigitsRect.Y, (littleDigitsRect.Width / 3.0f), littleDigitsRect.Height);
                            var tensRect =
                                new RectangleF(
                                    littleDigitsRect.X + littleDigitsRect.Width - ((littleDigitsRect.Width / 3.0f) * 2.0f),
                                    littleDigitsRect.Y, (littleDigitsRect.Width / 3.0f), littleDigitsRect.Height);
                            var hundredsRect =
                                new RectangleF(
                                    littleDigitsRect.X + littleDigitsRect.Width - ((littleDigitsRect.Width / 3.0f) * 3.0f),
                                    littleDigitsRect.Y, (littleDigitsRect.Width / 3.0f), littleDigitsRect.Height);
                            if (_bigDigitsFont ==null) 
                            {
                                _bigDigitsFont = new Font(_fonts.Families[1], 20, FontStyle.Regular, GraphicsUnit.Point);
                            }
                            if (_littleDigitsFont == null)
                            {
                                _littleDigitsFont = new Font(_fonts.Families[1], 18, FontStyle.Regular, GraphicsUnit.Point);
                            }
                            var digitsFormat = new StringFormat
                                                   {
                                                       Alignment = StringAlignment.Far,
                                                       FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap,
                                                       LineAlignment = StringAlignment.Far,
                                                       Trimming = StringTrimming.None
                                                   };
                            var digitColor = Color.FromArgb(255, 252, 205);
                            Brush digitsBrush = new SolidBrush(digitColor);
                            float thousandscombined = (int)Math.Floor((Math.Abs(absIndicatedAltitude) / 1000.0f));
                            var thousandsString = string.Format("{0:#0}", thousandscombined);
                            if (InstrumentState.IndicatedAltitudeFeetMSL < 0) thousandsString = "-" + thousandsString; //Falcas 16-08-12
                            destinationGraphics.DrawStringFast(thousandsString, _bigDigitsFont, digitsBrush, bigDigitsRect, digitsFormat);
                            var allHundredsString = string.Format("{0:00000}", Math.Abs(absIndicatedAltitude)).Substring(2, 3);
                            var hundredsString = allHundredsString.Substring(0, 1);

                            var tensString = string.Format("{0:0}",
                                                           (int)Math.Floor(((Math.Abs(absIndicatedAltitude) / 10.0f) % 10.0f)));
                            const string onesString = "0";

                            destinationGraphics.DrawStringFast(hundredsString, _littleDigitsFont, digitsBrush, hundredsRect, digitsFormat);
                            destinationGraphics.DrawStringFast(tensString, _littleDigitsFont, digitsBrush, tensRect, digitsFormat);
                            destinationGraphics.DrawStringFast(onesString, _littleDigitsFont, digitsBrush, onesRect, digitsFormat);

                            //draw the barometric pressure area
                            //draw top rectangle
                            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                            var topRectangle = new RectangleF(0, 0, width, 42);
                            destinationGraphics.FillRectangle(Brushes.Black, topRectangle);
                            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                            float barometricPressureAreaWidth = 65;
                            float unitAreaWidth = 65;
                            var barometricPressureStringFormat = new StringFormat();
                            barometricPressureStringFormat.Alignment = StringAlignment.Far;
                            barometricPressureStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                                                         StringFormatFlags.NoWrap;
                            barometricPressureStringFormat.LineAlignment = StringAlignment.Far;
                            barometricPressureStringFormat.Trimming = StringTrimming.None;

                            var pressure = InstrumentState.BarometricPressure;

                            var barometricPressureRectangle = new RectangleF(topRectangle.Width - barometricPressureAreaWidth - 15,
                                                                             20, barometricPressureAreaWidth,
                                                                             topRectangle.Height - 20);
                            var unitsRectangle = new RectangleF(topRectangle.Width - unitAreaWidth - 15,
                                                                             20, unitAreaWidth,
                                                                             topRectangle.Height - 20);
                            
                            string baroString = null;
                            string unitsString = null;
                            if (Options.PressureAltitudeUnits == AltimeterOptions.PressureUnits.InchesOfMercury)
                            {
                                baroString = string.Format("{0:#0.00}", pressure / 100);
                                barometricPressureRectangle = new RectangleF(topRectangle.Width - barometricPressureAreaWidth - 35,
                                                                             139, barometricPressureAreaWidth,
                                                                             topRectangle.Height - 18);
                                unitsString = "IN. HG";
                                unitsRectangle = new RectangleF(topRectangle.Width - unitAreaWidth - 48,
                                                                             136, unitAreaWidth,
                                                                             topRectangle.Height - 35);
                            }
                            else if (Options.PressureAltitudeUnits == AltimeterOptions.PressureUnits.Millibars)
                            {
                                baroString = string.Format("{0:###0}", pressure);
                                barometricPressureRectangle = new RectangleF(topRectangle.Width - barometricPressureAreaWidth - 35,
                                                                             139, barometricPressureAreaWidth,
                                                                             topRectangle.Height - 18);
                                unitsString = "MB";
                                unitsRectangle = new RectangleF(topRectangle.Width - unitAreaWidth - 50,
                                                                             136, unitAreaWidth,
                                                                             topRectangle.Height - 35);
                            }
                            if (_baroPressureFont2 == null)
                            {
                                _baroPressureFont2 = new Font(_fonts.Families[1], 12, FontStyle.Regular, GraphicsUnit.Point);
                            }
                            destinationGraphics.DrawStringFast(baroString, _baroPressureFont2,
                                           digitsBrush, barometricPressureRectangle, barometricPressureStringFormat);
                            if (_unitsFont2 == null) 
                            {
                                _unitsFont2 = new Font(_fonts.Families[1], 6, FontStyle.Bold, GraphicsUnit.Point);
                            }
                            destinationGraphics.DrawStringFast(unitsString, _unitsFont2,
                                           digitsBrush, unitsRectangle, barometricPressureStringFormat);
                            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                        }
                        break;
                }
                //draw the background image
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                switch (Options.Style)
                {
                    case AltimeterOptions.F16AltimeterStyle.Electromechanical:
                        destinationGraphics.TranslateTransform(0, -11);
                        destinationGraphics.DrawImageFast(
                            InstrumentState.PneumaticModeFlag
                                ? _backgroundElectroMechanical.MaskedImage
                                : _backgroundElectroMechanicalNoFlag.MaskedImage, new Point(0, 0));
                        break;
                    case AltimeterOptions.F16AltimeterStyle.Electronic:
                        destinationGraphics.TranslateTransform(0, -11);
                        destinationGraphics.DrawImageFast(
                            InstrumentState.StandbyModeFlag
                                ? _backgroundElectronic.MaskedImage
                                : _backgroundElectronicNoFlag.MaskedImage, new Point(0, 0));
                        break;
                }
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                //draw the altitude hand
                var degrees = hundreds*36;
                const float centerX = 128;
                const float centerY = 117;
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                destinationGraphics.TranslateTransform(centerX, centerY);
                destinationGraphics.RotateTransform(degrees);
                destinationGraphics.TranslateTransform(-centerX, -centerY);
                destinationGraphics.DrawImageFast(_needle.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                destinationGraphics.Restore(initialState);
            }
        }

    }
}