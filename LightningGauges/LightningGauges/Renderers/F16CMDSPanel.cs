using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public interface IF16CMDSPanel : IInstrumentRenderer
    {
        F16CMDSPanel.F16CMDSPanelInstrumentState InstrumentState { get; set; }
    }

    public class F16CMDSPanel : InstrumentRendererBase, IF16CMDSPanel
    {
        #region Image Location Constants

        private const string CMDS_BACKGROUND_IMAGE_FILENAME = "cmds.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static readonly object _imagesLock = new object();
        private static Bitmap _background;
        private static bool _imagesLoaded;
        private readonly Brush _digitBrush;
        private readonly Color _digitColor = Color.FromArgb(255, 63, 250, 63);

        private readonly Font _digitFont;
        private readonly StringFormat _digitStringFormat;
        private readonly Font _legendFont;
        private readonly FontFamily dotMatrixFontFamily;
        private readonly PrivateFontCollection pfc = new PrivateFontCollection();

        #endregion

        public F16CMDSPanel()
        {
            pfc.AddFontFile("lcddot.ttf");
            dotMatrixFontFamily = pfc.Families[0];
            _digitFont = new Font(dotMatrixFontFamily, 12, FontStyle.Regular, GraphicsUnit.Point);
            _legendFont = new Font("Lucida Console", 10, FontStyle.Bold, GraphicsUnit.Point);

            _digitStringFormat = new StringFormat();
            _digitStringFormat.Alignment = StringAlignment.Far;
            _digitStringFormat.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
            _digitStringFormat.LineAlignment = StringAlignment.Center;
            _digitStringFormat.Trimming = StringTrimming.None;
            _digitBrush = new SolidBrush(_digitColor);
            InstrumentState = new F16CMDSPanelInstrumentState();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                            CMDS_BACKGROUND_IMAGE_FILENAME);
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16CMDSPanelInstrumentState InstrumentState { get; set; }

        #region Instrument State

        [Serializable]
        public class F16CMDSPanelInstrumentState : InstrumentStateBase
        {
            private const int MAX_CHAFF = 99;
            private const int MAX_FLARE = 99;
            private const int MAX_OTHER1 = 99;
            private const int MAX_OTHER2 = 99;

            private int _chaffCount;
            private int _flareCount;
            private int _other1Count;
            private int _other2Count;

            public int ChaffCount
            {
                get { return _chaffCount; }
                set
                {
                    var chaff = value;
                    if (chaff < 0) chaff = 0;
                    if (chaff > MAX_CHAFF) chaff = MAX_CHAFF;
                    _chaffCount = chaff;
                }
            }

            public int FlareCount
            {
                get { return _flareCount; }
                set
                {
                    var flare = value;
                    if (flare < 0) flare = 0;
                    if (flare > MAX_FLARE) flare = MAX_FLARE;
                    _flareCount = flare;
                }
            }

            public int Other1Count
            {
                get { return _other1Count; }
                set
                {
                    var other1 = value;
                    if (other1 < 0) other1 = 0;
                    if (other1 > MAX_OTHER1) other1 = MAX_OTHER1;
                    _other1Count = other1;
                }
            }

            public int Other2Count
            {
                get { return _other2Count; }
                set
                {
                    var other2 = value;
                    if (other2 < 0) other2 = 0;
                    if (other2 > MAX_OTHER2) other2 = MAX_OTHER2;
                    _other2Count = other2;
                }
            }

            public bool ChaffLow { get; set; }
            public bool FlareLow { get; set; }
            public bool Other1Low { get; set; }
            public bool Other2Low { get; set; }
            public bool Degraded { get; set; }
            public bool Go { get; set; }
            public bool NoGo { get; set; }
            public bool DispenseReady { get; set; }
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
                var width = 336;
                var height = 88;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                //set the initial scale transformation 
                g.TranslateTransform(-90, -29);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = g.Save();


                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background, new Rectangle(0, 0, _background.Width, _background.Height),
                            new Rectangle(0, 0, _background.Width, _background.Height), GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                float translateX = 162;
                float translateY = 72;

                float digitBoxWidth = 57;
                float digitBoxHeight = 35;
                //draw the Other1 count
                {
                    var countString = string.Format("{0:00}", InstrumentState.Other1Count);
                    if (InstrumentState.Other1Low) countString = "Lo" + countString;
                    if (InstrumentState.Degraded)
                    {
                        countString = "PGR";
                    }
                    var countLocation = new RectangleF(translateX, translateY, digitBoxWidth, digitBoxHeight);
                    g.DrawString(countString, _digitFont, _digitBrush, countLocation, _digitStringFormat);
                }

                //draw the Other2 count
                {
                    translateX += digitBoxWidth;
                    var countString = string.Format("{0:00}", InstrumentState.Other2Count);
                    if (InstrumentState.Other2Low) countString = "Lo" + countString;
                    if (InstrumentState.Degraded)
                    {
                        countString = "FAIL";
                    }
                    var countLocation = new RectangleF(translateX, translateY, digitBoxWidth, digitBoxHeight);
                    g.DrawString(countString, _digitFont, _digitBrush, countLocation, _digitStringFormat);
                }

                //draw the Chaff count
                {
                    translateX += digitBoxWidth;
                    var countString = string.Format("{0:00}", InstrumentState.ChaffCount);
                    if (InstrumentState.ChaffLow) countString = "Lo" + countString;
                    if (InstrumentState.Degraded)
                    {
                        countString = "GO";
                    }
                    var countLocation = new RectangleF(translateX, translateY, digitBoxWidth, digitBoxHeight);
                    g.DrawString(countString, _digitFont, _digitBrush, countLocation, _digitStringFormat);
                }

                //draw the Flare count
                {
                    translateX += digitBoxWidth;
                    var countString = string.Format("{0:00}", InstrumentState.Other2Count);
                    if (InstrumentState.FlareLow) countString = "Lo" + countString;
                    if (InstrumentState.Degraded)
                    {
                        countString = "BYP";
                    }
                    var countLocation = new RectangleF(translateX, translateY, digitBoxWidth, digitBoxHeight);
                    g.DrawString(countString, _digitFont, _digitBrush, countLocation, _digitStringFormat);
                }

                RectangleF dispenseReadyRectangle = new Rectangle(283, 36, 115, 21);
                g.FillRectangle(Brushes.Black, dispenseReadyRectangle);
                if (InstrumentState.DispenseReady)
                {
                    g.DrawString("DISPENSE RDY", _legendFont, _digitBrush, dispenseReadyRectangle, _digitStringFormat);
                }

                RectangleF goRectangle = new Rectangle(220, 37, 25, 18);
                g.FillRectangle(Brushes.Black, goRectangle);
                if (InstrumentState.Go)
                {
                    g.DrawString("GO", _legendFont, _digitBrush, goRectangle, _digitStringFormat);
                }

                RectangleF noGoRectangle = new Rectangle(166, 36, 45, 21);
                g.FillRectangle(Brushes.Black, noGoRectangle);
                if (InstrumentState.NoGo)
                {
                    g.DrawString("NO GO", _legendFont, _digitBrush, noGoRectangle, _digitStringFormat);
                }


                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }

    }
}