using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        private static readonly object _imagesLock = new object();
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
            Options = new F16AltimeterOptions();
            Options.Style = F16AltimeterOptions.F16AltimeterStyle.Electromechanical;
            LoadFonts();
        }

        #region Initialization Code

        private void LoadFonts()
        {
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

        public F16AltimeterInstrumentState InstrumentState { get; set; }
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
                BarometricPressure = 29.92f;
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
            lock (_imagesLock)
            {
                //store the canvas's transform and clip settings so we can restore them later
                GraphicsState initialState = g.Save();
                float absIndicatedAltitude = Math.Abs(InstrumentState.IndicatedAltitudeFeetMSL);
                //set up the canvas scale and clipping region
                int width = 0;
                int height = 0;
                if (Options.Style == F16AltimeterOptions.F16AltimeterStyle.Electromechanical)
                {
                    width = _backgroundElectroMechanical.Image.Width - 40;
                    height = _backgroundElectroMechanical.Image.Height - 40;
                    if (InstrumentState.IndicatedAltitudeFeetMSL < 0)
                        absIndicatedAltitude = 99999.99f - absIndicatedAltitude;
                }
                else if (Options.Style == F16AltimeterOptions.F16AltimeterStyle.Electronic)
                {
                    width = _backgroundElectronic.Image.Width - 40;
                    height = _backgroundElectronic.Image.Height - 40;
                }

                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                    //set the initial scale transformation 
                g.TranslateTransform(-20, -8);

                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                GraphicsState basicState = g.Save();

                //calculate digits
                float tenThousands = (int) Math.Floor((Math.Abs(absIndicatedAltitude)/10000.0f)%10);
                float thousands = (int) Math.Floor((Math.Abs(absIndicatedAltitude)/1000.0f)%10);
                float hundreds = (Math.Abs(absIndicatedAltitude)/100.0f)%10;

                if (Options.Style == F16AltimeterOptions.F16AltimeterStyle.Electromechanical)
                {
                    //draw the altitude digits
                    float digitHeights = 26.5f; //pixels
                    float translateX = -130;
                    float translateY = -272;

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
                    g.TranslateTransform(0, digitHeights*hundreds);
                    g.DrawImage(_hundredsDigitsElectroMechanical, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }
                else if (Options.Style == F16AltimeterOptions.F16AltimeterStyle.Electronic)
                {
                    float bigDigitsX = 80;
                    float bigDigitsY = 103;
                    float bigDigitsWidth = 50;
                    float bigDigitsHeight = 35;
                    float littleDigitWidth = 50;
                    float littleDigitHeight = 30;
                    var bigDigitsRect = new RectangleF(bigDigitsX, bigDigitsY, bigDigitsWidth, bigDigitsHeight);
                    var littleDigitsRect = new RectangleF(bigDigitsRect.Right + 7, bigDigitsRect.Y + 4, littleDigitWidth,
                                                          littleDigitHeight);
                    var onesRect =
                        new RectangleF(littleDigitsRect.X + littleDigitsRect.Width - (littleDigitsRect.Width/3.0f),
                                       littleDigitsRect.Y, (littleDigitsRect.Width/3.0f), littleDigitsRect.Height);
                    var tensRect =
                        new RectangleF(
                            littleDigitsRect.X + littleDigitsRect.Width - ((littleDigitsRect.Width/3.0f)*2.0f),
                            littleDigitsRect.Y, (littleDigitsRect.Width/3.0f), littleDigitsRect.Height);
                    var hundredsRect =
                        new RectangleF(
                            littleDigitsRect.X + littleDigitsRect.Width - ((littleDigitsRect.Width/3.0f)*3.0f),
                            littleDigitsRect.Y, (littleDigitsRect.Width/3.0f), littleDigitsRect.Height);
                    var bigDigitsFont = new Font(_fonts.Families[0], 20, FontStyle.Regular, GraphicsUnit.Point);
                    var littleDigitsFont = new Font(_fonts.Families[0], 18, FontStyle.Regular, GraphicsUnit.Point);
                    var digitsFormat = new StringFormat();
                    digitsFormat.Alignment = StringAlignment.Far;
                    digitsFormat.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
                    digitsFormat.LineAlignment = StringAlignment.Far;
                    digitsFormat.Trimming = StringTrimming.None;
                    Color digitColor = Color.FromArgb(255, 252, 205);
                    Brush digitsBrush = new SolidBrush(digitColor);
                    float thousandscombined = (int) Math.Floor((Math.Abs(absIndicatedAltitude)/1000.0f));
                    string thousandsString = string.Format("{0:#0}", thousandscombined);
                    if (absIndicatedAltitude < 0) thousandsString = "-" + thousandsString;
                    g.DrawString(thousandsString, bigDigitsFont, digitsBrush, bigDigitsRect, digitsFormat);
                    string allHundredsString = string.Format("{0:00000}", Math.Abs(absIndicatedAltitude)).Substring(2, 3);
                    string hundredsString = allHundredsString.Substring(0, 1);

                    string tensString = string.Format("{0:0}",
                                                      (int) Math.Floor(((Math.Abs(absIndicatedAltitude)/10.0f)%10.0f)));
                    string onesString = "0";

                    g.DrawString(hundredsString, littleDigitsFont, digitsBrush, hundredsRect, digitsFormat);
                    g.DrawString(tensString, littleDigitsFont, digitsBrush, tensRect, digitsFormat);
                    g.DrawString(onesString, littleDigitsFont, digitsBrush, onesRect, digitsFormat);
                }
                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                if (Options.Style == F16AltimeterOptions.F16AltimeterStyle.Electromechanical)
                {
                    g.TranslateTransform(0, -11);
                    if (InstrumentState.PneumaticModeFlag)
                    {
                        g.DrawImage(_backgroundElectroMechanical.MaskedImage, new Point(0, 0));
                    }
                    else
                    {
                        g.DrawImage(_backgroundElectroMechanicalNoFlag.MaskedImage, new Point(0, 0));
                    }
                }
                else if (Options.Style == F16AltimeterOptions.F16AltimeterStyle.Electronic)
                {
                    g.TranslateTransform(0, -11);
                    if (InstrumentState.StandbyModeFlag)
                    {
                        g.DrawImage(_backgroundElectronic.MaskedImage, new Point(0, 0));
                    }
                    else
                    {
                        g.DrawImage(_backgroundElectronicNoFlag.MaskedImage, new Point(0, 0));
                    }
                }
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the altitude hand
                float degrees = hundreds*36;
                float centerX = 128;
                float centerY = 117;
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
            if (!_disposed)
            {
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
}