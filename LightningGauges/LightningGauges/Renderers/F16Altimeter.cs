using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public class F16Altimeter : InstrumentRendererBase, IDisposable
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
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
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
        private readonly PrivateFontCollection _fonts = new PrivateFontCollection();
        private bool _disposed;
        private bool _imagesLoaded;

        #endregion

        public F16Altimeter()
        {
            InstrumentState = new F16AltimeterInstrumentState();
            Options = new F16AltimeterOptions
                          {
                              Style = F16AltimeterOptions.F16AltimeterStyle.Electromechanical
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

        private F16AltimeterInstrumentState _instrumentState;
        public F16AltimeterInstrumentState InstrumentState { 
            get
            {
                return _instrumentState;

            } 
            set
            {
                _instrumentState = value;
            } 
        }
        public F16AltimeterOptions Options { get; set; }

        #region Options Class

        public class F16AltimeterOptions
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

            public F16AltimeterOptions()
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
        public class F16AltimeterInstrumentState : InstrumentStateBase
        {
            public F16AltimeterInstrumentState()
            {
                PneumaticModeFlag = true;
                ElectricModeFlag = false;
                IndicatedAltitudeFeetMSL = 0.0f;
                BarometricPressure = 2992f;
            }

            public float BarometricPressure { get; set; }
            public float IndicatedAltitudeFeetMSL { get; set; }
            public bool StandbyModeFlag { get; set; }
            public bool NormalModeFlag { get; set; }
            public bool PneumaticModeFlag { get; set; }
            public bool ElectricModeFlag { get; set; }
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
            lock (ImagesLock)
            {
                //store the canvas's transform and clip settings so we can restore them later
                var initialState = g.Save();
                var absIndicatedAltitude = Math.Abs(InstrumentState.IndicatedAltitudeFeetMSL);
                //set up the canvas scale and clipping region
                var width = 0;
                var height = 0;
                switch (Options.Style)
                {
                    case F16AltimeterOptions.F16AltimeterStyle.Electromechanical:
                        width = _backgroundElectroMechanical.Image.Width - 40;
                        height = _backgroundElectroMechanical.Image.Height - 40;
                        if (InstrumentState.IndicatedAltitudeFeetMSL < 0)
                            absIndicatedAltitude = 99999.99f - absIndicatedAltitude;
                        break;
                    case F16AltimeterOptions.F16AltimeterStyle.Electronic:
                        width = _backgroundElectronic.Image.Width - 40;
                        height = _backgroundElectronic.Image.Height - 40;
                        break;
                }

                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                //set the initial scale transformation 
                g.TranslateTransform(-20, -8);

                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = g.Save();

                //calculate digits
                float tenThousands = (int) Math.Floor((Math.Abs(absIndicatedAltitude)/10000.0f)%10);
                float thousands = (int) Math.Floor((Math.Abs(absIndicatedAltitude)/1000.0f)%10);
                var hundreds = (Math.Abs(absIndicatedAltitude)/100.0f)%10;

                switch (Options.Style)
                {
                    case F16AltimeterOptions.F16AltimeterStyle.Electromechanical:
                        {
                            //draw the altitude digits
                            const float digitHeights = 26.5f;
                            const float translateX = -130;
                            const float translateY = -272;

                            //draw ten-thousands digit
                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                            g.TranslateTransform(translateX, translateY);
                            g.TranslateTransform(0, digitHeights*tenThousands);
                            g.DrawImage(_tenThousandsDigitsElectroMechanical, new Point(0, 0));
                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                            //draw thousands digit
                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                            g.TranslateTransform(translateX, translateY);
                            g.TranslateTransform(0, digitHeights*thousands);
                            g.DrawImage(_thousandsDigitsElectroMechanical, new Point(0, 0));
                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                            //draw hundreds digit
                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                            g.TranslateTransform(translateX, translateY);
                            g.TranslateTransform(0, digitHeights * hundreds);
                            g.DrawImage(_hundredsDigitsElectroMechanical, new Point(0, 0));
                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                            //draw the barometric pressure area
                            //draw top rectangle
                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                            var topRectangle = new RectangleF(0, 0, width, 42);
                            g.FillRectangle(Brushes.Black, topRectangle);
                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                            float barometricPressureAreaWidth = 65;
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
                            var barometricPressureBrush = Brushes.White;

                            string baroString = null;
                            if (Options.PressureAltitudeUnits == F16AltimeterOptions.PressureUnits.InchesOfMercury)
                            {
                                baroString = string.Format("{0:#0.00}", pressure / 100);
                                barometricPressureRectangle = new RectangleF(topRectangle.Width - barometricPressureAreaWidth - 18,
                                                                             136, barometricPressureAreaWidth,
                                                                             topRectangle.Height - 20);
                            }
                            else if (Options.PressureAltitudeUnits == F16AltimeterOptions.PressureUnits.Millibars)
                            {
                                baroString = string.Format("{0:###0}", pressure);
                                barometricPressureRectangle = new RectangleF(topRectangle.Width - barometricPressureAreaWidth - 22,
                                                                             136, barometricPressureAreaWidth,
                                                                             topRectangle.Height - 20);
                            }
                            g.DrawString(baroString, new Font(_fonts.Families[0], 14, FontStyle.Bold, GraphicsUnit.Point),
                                           barometricPressureBrush, barometricPressureRectangle, barometricPressureStringFormat);
                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                        }
                        break;
                    case F16AltimeterOptions.F16AltimeterStyle.Electronic:
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
                            var bigDigitsFont = new Font(_fonts.Families[1], 20, FontStyle.Regular, GraphicsUnit.Point);
                            var littleDigitsFont = new Font(_fonts.Families[1], 18, FontStyle.Regular, GraphicsUnit.Point);
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
                            if (absIndicatedAltitude < 0) thousandsString = "-" + thousandsString;
                            g.DrawString(thousandsString, bigDigitsFont, digitsBrush, bigDigitsRect, digitsFormat);
                            var allHundredsString = string.Format("{0:00000}", Math.Abs(absIndicatedAltitude)).Substring(2, 3);
                            var hundredsString = allHundredsString.Substring(0, 1);

                            var tensString = string.Format("{0:0}",
                                                           (int)Math.Floor(((Math.Abs(absIndicatedAltitude) / 10.0f) % 10.0f)));
                            const string onesString = "0";

                            g.DrawString(hundredsString, littleDigitsFont, digitsBrush, hundredsRect, digitsFormat);
                            g.DrawString(tensString, littleDigitsFont, digitsBrush, tensRect, digitsFormat);
                            g.DrawString(onesString, littleDigitsFont, digitsBrush, onesRect, digitsFormat);

                            //draw the barometric pressure area
                            //draw top rectangle
                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                            var topRectangle = new RectangleF(0, 0, width, 42);
                            g.FillRectangle(Brushes.Black, topRectangle);
                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                            float barometricPressureAreaWidth = 65;
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
                            
                            string baroString = null;
                            if (Options.PressureAltitudeUnits == F16AltimeterOptions.PressureUnits.InchesOfMercury)
                            {
                                baroString = string.Format("{0:#0.00}", pressure / 100);
                                barometricPressureRectangle = new RectangleF(topRectangle.Width - barometricPressureAreaWidth - 35,
                                                                             139, barometricPressureAreaWidth,
                                                                             topRectangle.Height - 20);
                            }
                            else if (Options.PressureAltitudeUnits == F16AltimeterOptions.PressureUnits.Millibars)
                            {
                                baroString = string.Format("{0:###0}", pressure);
                                barometricPressureRectangle = new RectangleF(topRectangle.Width - barometricPressureAreaWidth - 35,
                                                                             139, barometricPressureAreaWidth,
                                                                             topRectangle.Height - 20);
                            }
                            g.DrawString(baroString, new Font(_fonts.Families[1], 12, FontStyle.Regular, GraphicsUnit.Point),
                                           digitsBrush, barometricPressureRectangle, barometricPressureStringFormat);
                            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                        }
                        break;
                }
                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                switch (Options.Style)
                {
                    case F16AltimeterOptions.F16AltimeterStyle.Electromechanical:
                        g.TranslateTransform(0, -11);
                        g.DrawImage(
                            InstrumentState.PneumaticModeFlag
                                ? _backgroundElectroMechanical.MaskedImage
                                : _backgroundElectroMechanicalNoFlag.MaskedImage, new Point(0, 0));
                        break;
                    case F16AltimeterOptions.F16AltimeterStyle.Electronic:
                        g.TranslateTransform(0, -11);
                        g.DrawImage(
                            InstrumentState.StandbyModeFlag
                                ? _backgroundElectronic.MaskedImage
                                : _backgroundElectronicNoFlag.MaskedImage, new Point(0, 0));
                        break;
                }
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the altitude hand
                var degrees = hundreds*36;
                const float centerX = 128;
                const float centerY = 117;
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(degrees);
                g.TranslateTransform(-centerX, -centerY);
                g.DrawImage(_needle.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }

        ~F16Altimeter()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                //Common.Util.DisposeObject(_backgroundElectromechanical);
                //Common.Util.DisposeObject(_backgroundElectromechanicalNoFlag);
                //Common.Util.DisposeObject(_backgroundElectronic);
                //Common.Util.DisposeObject(_backgroundElectronicNoFlag);
                //Common.Util.DisposeObject(_needle);
                //Common.Util.DisposeObject(_tenThousandsDigitsElectroMechanical );
                //Common.Util.DisposeObject(_thousandsDigitsElectroMechanical);
                //Common.Util.DisposeObject(_hundredsDigitsElectroMechanical);
            }
            _disposed = true;
        }
    }
}