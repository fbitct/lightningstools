using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using Common.SimSupport;
using Util = Common.Imaging.Util;

namespace LightningGauges.Renderers.F16.AzimuthIndicator
{
    public interface IAzimuthIndicator : IInstrumentRenderer

    {
        InstrumentState InstrumentState { get; set; }
        Options Options { get; set; }
    }

    public class AzimuthIndicator : InstrumentRendererBase, IAzimuthIndicator
    {
        #region Image Location Constants

        private const string RWR_BACKGROUND_IP1310ALR_IMAGE_FILENAME = "rwr.bmp";
        private const string RWR_BACKGROUND_HAF_IMAGE_FILENAME = "rwr2.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Other Constants

        private const int RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS = 32;

        #endregion

        #region Instance variables

        private static readonly object ImagesLock = new object();
        private static bool _imagesLoaded;
        private static Bitmap _backgroundIP1310ALR;
        private static Bitmap _backgroundHAF;
        private static readonly Font OSBLegendFont = new Font("Lucida Console", 15, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font ChaffFlareCountLegendFont = new Font("Lucida Console", 15, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly Font OtherLegendFont = new Font("Lucida Console", 18, FontStyle.Regular,GraphicsUnit.Point);
        private static readonly Font PageLegendFont = new Font("Lucida Console", 15, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly Font MissileWarningFont = new Font("Lucida Console", 12, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly Font SymbolTextFontSmall = new Font("Lucida Console", 12, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly Font SymbolTextFontLarge = new Font("Lucida Console", 15, FontStyle.Bold,GraphicsUnit.Point);
        private static readonly Font TestTextFontLarge = new Font("Lucida Console", 20, FontStyle.Bold, GraphicsUnit.Point); //Added Falcas 07-11-2012, For RWR test.
        private static readonly Color ScopeGreenColor = Color.FromArgb(255, 63, 250, 63);
        private static readonly Pen ScopeGreenPen = new Pen(ScopeGreenColor);
        private static readonly Color OSBLegendColor = Color.White;
        private static readonly Brush OSBLegendBrush = new SolidBrush(OSBLegendColor);
        private static readonly GraphicsPath FontPath = new GraphicsPath();

        #endregion

        public AzimuthIndicator()
        {
            Options = new Options();
            InstrumentState = new InstrumentState();
        }

        #region Initialization Code

        private static void LoadImageResources()
        {
            if (_imagesLoaded) return;
            _backgroundIP1310ALR =
                (Bitmap)
                Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                        RWR_BACKGROUND_IP1310ALR_IMAGE_FILENAME);
            _backgroundHAF =
                (Bitmap)
                Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                        RWR_BACKGROUND_HAF_IMAGE_FILENAME);

            _imagesLoaded = true;
        }

        #endregion

        public InstrumentState InstrumentState { get; set; }
        public Options Options { get; set; }


        private static void DrawString(Graphics g, string s, Font font, Brush brush, RectangleF layoutRectangle,
                                StringFormat format)
        {
            FontPath.Reset();
            FontPath.AddString(s, font.FontFamily, (int) font.Style, font.SizeInPoints, layoutRectangle, format);
            g.FillPath(brush, FontPath);
        }

        public override void Render(Graphics destinationGraphics, Rectangle destinationRectangle)
        {
            if (!_imagesLoaded)
            {
                LoadImageResources();
            }
            var gfx = destinationGraphics;
            Bitmap fullBright = null;
            if (InstrumentState.Brightness != InstrumentState.MaxBrightness)
            {
                fullBright = new Bitmap(destinationRectangle.Size.Width, destinationRectangle.Size.Height, PixelFormat.Format32bppPArgb);
                gfx = Graphics.FromImage(fullBright);
            }
            lock (ImagesLock)
            {
                var initialTransform = gfx.Transform;
                gfx.InterpolationMode = Options.GDIPlusOptions.InterpolationMode;
                gfx.PixelOffsetMode = Options.GDIPlusOptions.PixelOffsetMode;
                gfx.SmoothingMode = Options.GDIPlusOptions.SmoothingMode;
                gfx.TextRenderingHint = Options.GDIPlusOptions.TextRenderingHint;
                //store the canvas's transform and clip settings so we can restore them later
                var initialState = gfx.Save();

                //set up the canvas scale and clipping region
                gfx.ResetTransform(); //clear any existing transforms
                gfx.SetClip(destinationRectangle);
                //set the clipping region on the graphics object to our render rectangle's boundaries
                gfx.FillRectangle(Brushes.Black, destinationRectangle);


                var okColor = ScopeGreenColor; //Color.FromArgb(255, 94, 184, 96);
                var warnColor = Color.FromArgb(255, 230, 238, 152);
                var severeColor = Color.FromArgb(196, 43, 48);

                var missileColor = severeColor;

                var navalThreatColor = ScopeGreenColor;
                var airborneThreatColor = ScopeGreenColor;
                var searchThreatColor = ScopeGreenColor;
                var unknownThreatColor = ScopeGreenColor;
                var groundThreatColor = ScopeGreenColor;


                Bitmap background = null;
                var width = 0;
                var height = 0;
                int backgroundWidth;
                int backgroundHeight;
                if (Options.Style == InstrumentStyle.IP1310ALR ||
                    Options.Style == InstrumentStyle.HAF)
                {
                    if (Options.Style == InstrumentStyle.IP1310ALR)
                    {
                        background = _backgroundIP1310ALR;
                    }
                    else if (Options.Style == InstrumentStyle.HAF)
                    {
                        background = _backgroundHAF;
                    }
                    width = background.Width - 28;
                    height = background.Height - 28;
                }
                else if (Options.Style == InstrumentStyle.AdvancedThreatDisplay)
                {
                    width = 256;
                    height = 256;
                }
                if (background != null)
                {
                    backgroundWidth = background.Width;
                    backgroundHeight = background.Height;
                }
                else
                {
                    backgroundWidth = 256;
                    backgroundHeight = 256;
                }
                var scaleX = destinationRectangle.Width/(float) width;
                var scaleY = destinationRectangle.Height/(float) height;
                gfx.ScaleTransform(scaleX, scaleY); //set the initial scale transformation 

                if (Options.Style == InstrumentStyle.IP1310ALR ||
                    Options.Style == InstrumentStyle.HAF)
                {
                    gfx.TranslateTransform(-14, -14);
                }
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = gfx.Save();


                float atdRingScale;
                if (destinationRectangle.Width < destinationRectangle.Height)
                {
                    atdRingScale = destinationRectangle.Width/(float) width;
                }
                else if (destinationRectangle.Width > destinationRectangle.Height)
                {
                    atdRingScale = destinationRectangle.Height/(float) height;
                }
                else
                {
                    atdRingScale = destinationRectangle.Width/(float) width;
                }


                var chaffFlareCountStringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Far,
                    LineAlignment = StringAlignment.Near,
                    Trimming = StringTrimming.None,
                    FormatFlags = StringFormatFlags.NoWrap
                };

                var miscTextStringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Far,
                    Trimming = StringTrimming.None,
                    FormatFlags = StringFormatFlags.NoWrap
                };

                const int countLabelHeight = 15;
                const int countLabelSpacing = 22;
                const float countLabelCharWidth = 7;
                var chaffCountRectangle = new RectangleF(45, 5, countLabelCharWidth*4, countLabelHeight*2.5F);
                var flareCountRectangle = new RectangleF(chaffCountRectangle.Right + countLabelSpacing,
                                                         chaffCountRectangle.Y, countLabelCharWidth*4,
                                                         countLabelHeight*2.5F);
                var other1CountRectangle = new RectangleF(flareCountRectangle.Right + countLabelSpacing,
                                                          chaffCountRectangle.Y, countLabelCharWidth*4,
                                                          countLabelHeight*2.5F);
                var other2CountRectangle = new RectangleF(other1CountRectangle.Right + countLabelSpacing,
                                                          chaffCountRectangle.Y, countLabelCharWidth*4,
                                                          countLabelHeight*2.5F);
                var goNogoRect = new RectangleF(184, 220, 60, countLabelHeight);
                var rdyRectangle = new RectangleF(goNogoRect.Left, goNogoRect.Bottom, goNogoRect.Width,
                                                  goNogoRect.Height);
                var pflRectangle = new RectangleF(30, chaffCountRectangle.Bottom, 25, countLabelHeight);
                var ewmsModeRectangle = new RectangleF(other2CountRectangle.Right - pflRectangle.Width - 3,
                                                       pflRectangle.Top, pflRectangle.Width, pflRectangle.Height);


                const float atdOuterRingDiameter = 200.0f;
                var outerRingLeft = ((width - atdOuterRingDiameter)/2.0f);
                var outerRingTop = ((height - atdOuterRingDiameter)/2.0f);
                var atdRingOffsetTranslateX = ((destinationRectangle.Width - (atdOuterRingDiameter*atdRingScale))/2.0f) -
                                              (outerRingLeft*atdRingScale);
                var atdRingOffsetTranslateY = ((destinationRectangle.Height - (atdOuterRingDiameter*atdRingScale))/2.0f) -
                                              (outerRingTop*atdRingScale) + (chaffCountRectangle.Y*atdRingScale);

                var atdMiddleRingDiameter = atdOuterRingDiameter/2.0f;
                var middleRingLeft = ((width - atdMiddleRingDiameter)/2.0f);
                var middleRingTop = ((height - atdMiddleRingDiameter)/2.0f);

                const float atdInnerRingDiameter = RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS;
                var innerRingLeft = ((width - atdInnerRingDiameter)/2.0f);
                var innerRingTop = ((height - atdInnerRingDiameter)/2.0f);

                //draw the background image
                if (
                    (
                        (
                            Options.Style == InstrumentStyle.IP1310ALR
                            ||
                            Options.Style == InstrumentStyle.HAF
                        )
                        &&
                        !Options.HideBezel
                    )
                    )
                {
                    GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
                    gfx.DrawImage(background, new Rectangle(0, 0, backgroundWidth, backgroundHeight),
                                  new Rectangle(0, 0, backgroundWidth, backgroundHeight), GraphicsUnit.Pixel);
                    GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
                }
                if (Options.Style == InstrumentStyle.AdvancedThreatDisplay && InstrumentState.RWRPowerOn)
                {
                     //Added Falcas 28-10-2012, If there is no power keep blank
                     //DRAW OSB LEGENDS
                    var verticalOsbLegendLHSFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Far,
                        LineAlignment = StringAlignment.Center
                    };

                    var verticalOsbLegendRHSFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center
                    };

                    const int verticalOsbLegendWidth = 15;
                        const int verticalOsbLegendHeight = 50;

                        var leftLegend1Rectangle = new Rectangle(0, 7, verticalOsbLegendWidth, verticalOsbLegendHeight);
                        var leftLegend2Rectangle = new Rectangle(0, 68, verticalOsbLegendWidth, verticalOsbLegendHeight + 10);
                        var leftLegend3Rectangle = new Rectangle(0, 135, verticalOsbLegendWidth, verticalOsbLegendHeight);
                        var leftLegend4Rectangle = new Rectangle(0, 200, verticalOsbLegendWidth, verticalOsbLegendHeight);
                        var rightLegend1Rectangle = new Rectangle(width - verticalOsbLegendWidth, leftLegend1Rectangle.Top,
                                                                  leftLegend1Rectangle.Width, verticalOsbLegendHeight);
                        var rightLegend2Rectangle = new Rectangle(rightLegend1Rectangle.Left, leftLegend2Rectangle.Top,
                                                                  leftLegend2Rectangle.Width, verticalOsbLegendHeight + 10);
                        var rightLegend3Rectangle = new Rectangle(rightLegend2Rectangle.Left, leftLegend3Rectangle.Top,
                                                                  rightLegend2Rectangle.Width, verticalOsbLegendHeight);
                        var rightLegend4Rectangle = new Rectangle(rightLegend3Rectangle.Left, leftLegend4Rectangle.Top,
                                                                  rightLegend3Rectangle.Width, verticalOsbLegendHeight);

                        //draw TTD-specific extra items
                        if (
                            InstrumentState.RWRPowerOn &&
                            (
                                InstrumentState.UnknownThreatScanMode
                                ||
                                (
                                    AreNonVisibleUnknownThreatsDetected()
                                    && DateTime.Now.Millisecond % 500 < 250
                                )
                            )
                            ) //draw highlighted UNK legend 
                        {
                            gfx.FillRectangle(OSBLegendBrush, leftLegend1Rectangle);
                            DrawString(gfx, "UNK", OSBLegendFont, Brushes.Black, leftLegend1Rectangle,
                                       verticalOsbLegendLHSFormat);
                        }
                        else //draw non-highlighted UNK legend
                        {
                            DrawString(gfx, "UNK", OSBLegendFont, OSBLegendBrush, leftLegend1Rectangle,
                                       verticalOsbLegendLHSFormat);
                        }
                        if (InstrumentState.RWRPowerOn && InstrumentState.Handoff) //draw highlighted HOFF legend 
                        {
                            gfx.FillRectangle(OSBLegendBrush, leftLegend2Rectangle);
                            DrawString(gfx, "HOFF", OSBLegendFont, Brushes.Black, leftLegend2Rectangle,
                                       verticalOsbLegendLHSFormat);
                        }
                        else //draw non-highlighted UNK legend
                        {
                            DrawString(gfx, "HOFF", OSBLegendFont, OSBLegendBrush, leftLegend2Rectangle,
                                       verticalOsbLegendLHSFormat);
                        }

                        if (InstrumentState.RWRPowerOn && InstrumentState.SeparateMode) //draw highlighted SEP legend 
                        {
                            gfx.FillRectangle(OSBLegendBrush, leftLegend3Rectangle);
                            DrawString(gfx, "SEP", OSBLegendFont, Brushes.Black, leftLegend3Rectangle,
                                       verticalOsbLegendLHSFormat);
                        }
                        else //draw non-highlighted SEP legend
                        {
                            DrawString(gfx, "SEP", OSBLegendFont, OSBLegendBrush, leftLegend3Rectangle,
                                       verticalOsbLegendLHSFormat);
                        }

                        if (InstrumentState.RWRPowerOn &&
                            ((InstrumentState.PriorityMode && !AreNonVisiblePriorityThreatsDetected()) ||
                             (InstrumentState.PriorityMode && AreNonVisiblePriorityThreatsDetected() &&
                              DateTime.Now.Millisecond % 500 < 250)))
                        {
                            gfx.FillRectangle(OSBLegendBrush, leftLegend4Rectangle);
                            DrawString(gfx, "PRI", OSBLegendFont, Brushes.Black, leftLegend4Rectangle,
                                       verticalOsbLegendLHSFormat);
                        }
                        else //draw non-highlighted PRI legend
                        {
                            DrawString(gfx, "PRI", OSBLegendFont, OSBLegendBrush, leftLegend4Rectangle,
                                       verticalOsbLegendLHSFormat);
                        }


                        if (InstrumentState.RWRPowerOn &&
                            (InstrumentState.NavalMode ||
                             (AreNonVisibleNavalThreatsDetected() && DateTime.Now.Millisecond % 500 < 250)))
                        //draw highlighted NVL legend 
                        {
                            gfx.FillRectangle(OSBLegendBrush, rightLegend1Rectangle);
                            DrawString(gfx, "NVL", OSBLegendFont, Brushes.Black, rightLegend1Rectangle,
                                       verticalOsbLegendRHSFormat);
                        }
                        else //draw non-highlighted NVL legend
                        {
                            DrawString(gfx, "NVL", OSBLegendFont, OSBLegendBrush, rightLegend1Rectangle,
                                       verticalOsbLegendRHSFormat);
                        }


                        if (InstrumentState.RWRPowerOn &&
                            (InstrumentState.SearchMode ||
                             (AreNonVisibleSearchThreatsDetected() && DateTime.Now.Millisecond % 500 < 250)))
                        //draw highlighted SRCH legend 
                        {
                            gfx.FillRectangle(OSBLegendBrush, rightLegend2Rectangle);
                            DrawString(gfx, "SRCH", OSBLegendFont, Brushes.Black, rightLegend2Rectangle,
                                       verticalOsbLegendRHSFormat);
                        }
                        else //draw non-highlighted SRCH legend
                        {
                            DrawString(gfx, "SRCH", OSBLegendFont, OSBLegendBrush, rightLegend2Rectangle,
                                       verticalOsbLegendRHSFormat);
                        }

                        if (InstrumentState.RWRPowerOn
                            && (
                                   InstrumentState.LowAltitudeMode
                            //|| (AreNonVisibleGroundThreatsDetected() && DateTime.Now.Millisecond % 500 < 250)
                               )) //draw highlighted ALT legend 
                        {
                            gfx.FillRectangle(OSBLegendBrush, rightLegend3Rectangle);
                            DrawString(gfx, "ALT", OSBLegendFont, Brushes.Black, rightLegend3Rectangle,
                                       verticalOsbLegendRHSFormat);
                        }
                        else //draw non-highlighted ALT legend
                        {
                            DrawString(gfx, "ALT", OSBLegendFont, OSBLegendBrush, rightLegend3Rectangle,
                                       verticalOsbLegendRHSFormat);
                        }

                        //draw chaff count


                        Color chaffCountColor;
                        if (InstrumentState.ChaffCount == 0)
                        {
                            chaffCountColor = severeColor;
                        }
                        else if (InstrumentState.ChaffLow)
                        {
                            chaffCountColor = warnColor;
                        }
                        else
                        {
                            chaffCountColor = okColor;
                        }
                        Brush chaffCountBrush = new SolidBrush(chaffCountColor);
                        DrawString(gfx, "CHAF", ChaffFlareCountLegendFont, chaffCountBrush, chaffCountRectangle,
                                   chaffFlareCountStringFormat);
                        chaffCountRectangle.Offset(0, 12);
                        DrawString(gfx, string.Format("{0:00}", InstrumentState.ChaffCount), ChaffFlareCountLegendFont,
                                   chaffCountBrush, chaffCountRectangle, chaffFlareCountStringFormat);

                        Color flareCountColor;
                        if (InstrumentState.FlareCount == 0)
                        {
                            flareCountColor = severeColor;
                        }
                        else if (InstrumentState.FlareLow)
                        {
                            flareCountColor = warnColor;
                        }
                        else
                        {
                            flareCountColor = okColor;
                        }
                        Brush flareCountBrush = new SolidBrush(flareCountColor);
                        DrawString(gfx, "FLAR", ChaffFlareCountLegendFont, flareCountBrush, flareCountRectangle,
                                   chaffFlareCountStringFormat);
                        flareCountRectangle.Offset(0, 12);
                        DrawString(gfx, string.Format("{0:00}", InstrumentState.FlareCount), ChaffFlareCountLegendFont,
                                   flareCountBrush, flareCountRectangle, chaffFlareCountStringFormat);


                        Color other1CountColor;
                        if (InstrumentState.Other1Count == 0)
                        {
                            other1CountColor = severeColor;
                        }
                        else if (InstrumentState.Other1Low)
                        {
                            other1CountColor = warnColor;
                        }
                        else
                        {
                            other1CountColor = okColor;
                        }
                        Brush other1CountBrush = new SolidBrush(other1CountColor);
                        DrawString(gfx, "OTR1", ChaffFlareCountLegendFont, other1CountBrush, other1CountRectangle,
                                   chaffFlareCountStringFormat);
                        other1CountRectangle.Offset(0, 12);
                        DrawString(gfx, string.Format("{0:00}", InstrumentState.Other1Count), ChaffFlareCountLegendFont,
                                   other1CountBrush, other1CountRectangle, chaffFlareCountStringFormat);


                        Color other2CountColor;
                        if (InstrumentState.Other2Count == 0)
                        {
                            other2CountColor = severeColor;
                        }
                        else if (InstrumentState.Other2Low)
                        {
                            other2CountColor = warnColor;
                        }
                        else
                        {
                            other2CountColor = okColor;
                        }
                        Brush other2CountBrush = new SolidBrush(other2CountColor);
                        DrawString(gfx, "OTR2", ChaffFlareCountLegendFont, other2CountBrush, other2CountRectangle,
                                   chaffFlareCountStringFormat);
                        other2CountRectangle.Offset(0, 12);
                        DrawString(gfx, String.Format("{0:00}", InstrumentState.Other2Count), ChaffFlareCountLegendFont,
                                   other2CountBrush, other2CountRectangle, chaffFlareCountStringFormat);

                        if (InstrumentState.EWSGo)
                        {
                            var legendColor = ScopeGreenColor;
                            Brush legendBrush = new SolidBrush(legendColor);
                            DrawString(gfx, "GO", OtherLegendFont, legendBrush, goNogoRect, miscTextStringFormat);
                        }
                        else if (InstrumentState.EWSNoGo)
                        {
                            var legendColor = warnColor;
                            Brush legendBrush = new SolidBrush(legendColor);
                            DrawString(gfx, "NOGO", OtherLegendFont, legendBrush, goNogoRect, miscTextStringFormat);
                        }
                        else if (InstrumentState.EWSDispenseReady)
                        {
                            var legendColor = ScopeGreenColor;
                            Brush legendBrush = new SolidBrush(legendColor);
                            DrawString(gfx, "DISP", OtherLegendFont, legendBrush, goNogoRect, miscTextStringFormat);
                        }


                        if (InstrumentState.EWSDispenseReady)
                        {
                            var legendColor = okColor;
                            Brush legendBrush = new SolidBrush(legendColor);
                            DrawString(gfx, "RDY", OtherLegendFont, legendBrush, rdyRectangle, miscTextStringFormat);
                        }

                        if (InstrumentState.EWSDegraded) //draw PFL legend 
                        {
                            var legendColor = warnColor;
                            Brush legendBrush = new SolidBrush(legendColor);
                            DrawString(gfx, "PFL", OtherLegendFont, legendBrush, pflRectangle, miscTextStringFormat);
                        }

                        //Added Falcas 10-11-2012.
                        InstrumentState.EWMSMode = GetEWMSMode(InstrumentState.cmdsMode);
                        switch (InstrumentState.EWMSMode)
                        {
                            case EWMSMode.Off:
                                {
                                    var legendColor = severeColor;
                                    Brush legendBrush = new SolidBrush(legendColor);
                                    DrawString(gfx, "OFF", OtherLegendFont, legendBrush, ewmsModeRectangle,
                                               miscTextStringFormat);
                                }
                                break;
                            case EWMSMode.Standby:
                                {
                                    var legendColor = warnColor;
                                    Brush legendBrush = new SolidBrush(legendColor);
                                    DrawString(gfx, "SBY", OtherLegendFont, legendBrush, ewmsModeRectangle,
                                               miscTextStringFormat);
                                }
                                break;
                            case EWMSMode.Manual:
                                {
                                    var legendColor = okColor;
                                    Brush legendBrush = new SolidBrush(legendColor);
                                    DrawString(gfx, "MAN", OtherLegendFont, legendBrush, ewmsModeRectangle,
                                               miscTextStringFormat);
                                }
                                break;
                            case EWMSMode.Semiautomatic:
                                {
                                    var legendColor = okColor;
                                    Brush legendBrush = new SolidBrush(legendColor);
                                    DrawString(gfx, "SEM", OtherLegendFont, legendBrush, ewmsModeRectangle,
                                               miscTextStringFormat);
                                }
                                break;
                            case EWMSMode.Automatic:
                                {
                                    var legendColor = okColor;
                                    Brush legendBrush = new SolidBrush(legendColor);
                                    DrawString(gfx, "AUT", OtherLegendFont, legendBrush, ewmsModeRectangle,
                                               miscTextStringFormat);
                                }
                                break;
                            case EWMSMode.Bypass:
                                {
                                    var legendColor = severeColor;
                                    Brush legendBrush = new SolidBrush(legendColor);
                                    DrawString(gfx, "BYP", OtherLegendFont, legendBrush, ewmsModeRectangle,
                                               miscTextStringFormat);
                                }
                                break;
                            default:
                                break;
                        }

                        //Draw the page legends
                        var pageLegendStringFormat = new StringFormat();
                        pageLegendStringFormat.Alignment = StringAlignment.Center;
                        pageLegendStringFormat.LineAlignment = StringAlignment.Near;
                        pageLegendStringFormat.Trimming = StringTrimming.None;
                        pageLegendStringFormat.FormatFlags = StringFormatFlags.NoWrap;

                        const int pageLegendHeight = 15;
                        const int pageLegendWidth = 35;
                        const int pageLegendSeparation = 15;
                        var tacLegendRectangle = new Rectangle(57, backgroundHeight - pageLegendHeight - 5, pageLegendWidth,
                                                               pageLegendHeight);
                        var sysLegendRectangle = new Rectangle(tacLegendRectangle.Right + pageLegendSeparation,
                                                               tacLegendRectangle.Y, pageLegendWidth, pageLegendHeight);
                        var tstLegendRectangle = new Rectangle(sysLegendRectangle.Right + pageLegendSeparation,
                                                               tacLegendRectangle.Y, pageLegendWidth, pageLegendHeight);

                        //draw highlighted TAC legend
                        gfx.FillRectangle(Brushes.White, tacLegendRectangle);
                        DrawString(gfx, "TAC", PageLegendFont, Brushes.Black, tacLegendRectangle,
                                    pageLegendStringFormat);
                    
                        //draw non-highlighted SYS legend
                        DrawString(gfx, "SYS", PageLegendFont, Brushes.White, sysLegendRectangle,
                                    pageLegendStringFormat);

                        if (InstrumentState.RWRTest1 || InstrumentState.RWRTest2) //Added Falcas 10-11-2012
                        {
                            //draw highlighted TST legend
                            gfx.FillRectangle(Brushes.White, tstLegendRectangle);
                            DrawString(gfx, "TST", PageLegendFont, Brushes.Black, tstLegendRectangle,
                                       pageLegendStringFormat);
                        }
                        else
                        {
                            //draw non-highlighted TST legend
                            DrawString(gfx, "TST", PageLegendFont, Brushes.White, tstLegendRectangle,
                                       pageLegendStringFormat);
                        }

                        gfx.Transform = initialTransform;
                        gfx.TranslateTransform(atdRingOffsetTranslateX, atdRingOffsetTranslateY);
                        gfx.ScaleTransform(atdRingScale, atdRingScale);
                        var grayPen = Pens.Gray;
                        const int lineLength = 10;
                        //draw the outer lethality ring
                        {
                            var toRestore = gfx.Transform;
                            gfx.TranslateTransform(outerRingLeft, outerRingTop);
                            gfx.DrawEllipse(grayPen, 0, 0, atdOuterRingDiameter, atdOuterRingDiameter);

                            for (var i = 0; i <= 180; i += 30)
                            {
                                var previousTransform = gfx.Transform;
                                gfx.TranslateTransform(atdOuterRingDiameter / 2.0f, atdOuterRingDiameter / 2.0f);
                                gfx.RotateTransform(i);
                                gfx.TranslateTransform(-atdOuterRingDiameter / 2.0f, -atdOuterRingDiameter / 2.0f);
                                if (i % 90 == 0)
                                {
                                    gfx.DrawLine(grayPen, atdOuterRingDiameter / 2.0f, 0, atdOuterRingDiameter / 2.0f,
                                                 lineLength * 2);
                                    gfx.DrawLine(grayPen, atdOuterRingDiameter / 2.0f, atdOuterRingDiameter,
                                                 atdOuterRingDiameter / 2.0f, atdOuterRingDiameter - (lineLength * 2));
                                }
                                else
                                {
                                    gfx.DrawLine(grayPen, atdOuterRingDiameter / 2.0f, 0, atdOuterRingDiameter / 2.0f,
                                                 lineLength);
                                    gfx.DrawLine(grayPen, atdOuterRingDiameter / 2.0f, atdOuterRingDiameter,
                                                 atdOuterRingDiameter / 2.0f, atdOuterRingDiameter - lineLength);
                                }
                                gfx.Transform = previousTransform;
                            }
                            gfx.Transform = toRestore;
                        }

                        //draw the middle lethality ring 
                        {
                            var toRestore = gfx.Transform;
                            gfx.TranslateTransform(middleRingLeft, middleRingTop);
                            gfx.DrawEllipse(grayPen, 0, 0, atdMiddleRingDiameter, atdMiddleRingDiameter);

                            gfx.TranslateTransform(atdMiddleRingDiameter / 2.0f, atdMiddleRingDiameter / 2.0f);
                            gfx.RotateTransform(-InstrumentState.MagneticHeadingDegrees);
                            gfx.TranslateTransform(-atdMiddleRingDiameter / 2.0f, -atdMiddleRingDiameter / 2.0f);

                            //draw north line
                            gfx.DrawLine(grayPen, atdMiddleRingDiameter / 2.0f, -(lineLength * 2), atdMiddleRingDiameter / 2.0f, 0);

                            //draw west line
                            gfx.DrawLine(grayPen, 0, (atdMiddleRingDiameter / 2.0f), lineLength, (atdMiddleRingDiameter / 2.0f));

                            //draw east line
                            gfx.DrawLine(grayPen, atdMiddleRingDiameter, (atdMiddleRingDiameter / 2.0f),
                                         atdMiddleRingDiameter - lineLength, (atdMiddleRingDiameter / 2.0f));

                            //draw south line
                            gfx.DrawLine(grayPen, (atdMiddleRingDiameter / 2.0f), atdMiddleRingDiameter - lineLength,
                                         (atdMiddleRingDiameter / 2.0f), atdMiddleRingDiameter + lineLength);

                            //draw north flag
                            gfx.DrawLine(grayPen, (atdMiddleRingDiameter / 2.0f), -(lineLength * 2),
                                         (atdMiddleRingDiameter / 2.0f) + 7, -(lineLength * 0.75f));
                            gfx.DrawLine(grayPen, (atdMiddleRingDiameter / 2.0f), -(lineLength * 0.75f),
                                         (atdMiddleRingDiameter / 2.0f) + 7, -(lineLength * 0.75f));

                            //destinationGraphics.Transform = previousTransform;
                            gfx.Transform = toRestore;
                        }

                        // draw the inner lethality ring
                        gfx.DrawEllipse(grayPen, innerRingLeft, innerRingTop, atdInnerRingDiameter, atdInnerRingDiameter);

                        if (!InstrumentState.RWRPowerOn) //draw RWR POWER OFF flag
                        {
                            var rwrOffTextHeight = atdInnerRingDiameter;
                            var rwrOffTextWidth = atdInnerRingDiameter;
                            var rwrRectangle = new RectangleF((backgroundWidth / 2.0f) - (rwrOffTextWidth / 2.0f),
                                                              ((backgroundHeight / 2.0f) - (rwrOffTextHeight / 2.0f)),
                                                              rwrOffTextWidth, rwrOffTextHeight);
                            rwrRectangle.Inflate(-5, -5);
                            var legendColor = severeColor;
                            var legendPen = new Pen(legendColor);
                            new SolidBrush(legendColor);
                            //destinationGraphics.FillEllipse(legendBrush, rwrRectangle);
                            //DrawString(destinationGraphics, "RWR", _otherLegendFont, Brushes.Black, rwrRectangle, rwrOffTextStringFormat);
                            gfx.DrawLine(legendPen, new PointF(rwrRectangle.Left, rwrRectangle.Top),
                                         new PointF(rwrRectangle.Right, rwrRectangle.Bottom));
                            gfx.DrawLine(legendPen, new PointF(rwrRectangle.Left, rwrRectangle.Bottom),
                                         new PointF(rwrRectangle.Right, rwrRectangle.Top));
                        }
                }

                if (
                    (
                        Options.Style == InstrumentStyle.HAF
                        ||
                        Options.Style == InstrumentStyle.IP1310ALR
                    )
                    &&
                    InstrumentState.RWRPowerOn && !InstrumentState.RWRTest1 && !InstrumentState.RWRTest2
                    ) //Added Falcas 07-11-2012, Do not draw when in RWR test.
                {
                    GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
                    if (Options.Style == InstrumentStyle.AdvancedThreatDisplay)
                    {
                        gfx.Transform = initialTransform;
                        gfx.TranslateTransform(atdRingOffsetTranslateX, atdRingOffsetTranslateY);
                        gfx.ScaleTransform(atdRingScale, atdRingScale);
                    }

                    //draw heartbeat cross
                    gfx.DrawLine(
                        ScopeGreenPen,
                        new PointF((backgroundWidth/2.0f) - 20, (backgroundHeight/2.0f)),
                        new PointF((backgroundWidth/2.0f) - 10, (backgroundHeight/2.0f))
                        );
                    gfx.DrawLine(
                        ScopeGreenPen,
                        new PointF((backgroundWidth/2.0f) + 20, (backgroundHeight/2.0f)),
                        new PointF((backgroundWidth/2.0f) + 10, (backgroundHeight/2.0f))
                        );
                    gfx.DrawLine(
                        ScopeGreenPen,
                        new PointF((backgroundWidth/2.0f), (backgroundHeight/2.0f) - 20),
                        new PointF((backgroundWidth/2.0f), (backgroundHeight/2.0f) - 10)
                        );
                    gfx.DrawLine(
                        ScopeGreenPen,
                        new PointF((backgroundWidth/2.0f), (backgroundHeight/2.0f) + 20),
                        new PointF((backgroundWidth/2.0f), (backgroundHeight/2.0f) + 10)
                        );
                    GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);

                    //Added Falcas 07-11-2012, Center of RWR indication "S"
                    if (InstrumentState.SearchMode)
                    {
                        var pageLegendStringFormat = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Near,
                            Trimming = StringTrimming.None,
                            FormatFlags = StringFormatFlags.NoWrap
                        };

                        var RwrLegendRectangle = new Rectangle(113, 119, 30, 30);
                        DrawString(gfx, "S", TestTextFontLarge, Brushes.Lime, RwrLegendRectangle,
                                           pageLegendStringFormat);
                    }
                }

                
                if (InstrumentState.RWRPowerOn)
                {
                    //Added Falcas 07-11-2012
                    //RWR test
                    if (InstrumentState.RWRTest1)
                    {
                        var pageLegendStringFormat = new StringFormat();
                        pageLegendStringFormat.Alignment = StringAlignment.Center;
                        pageLegendStringFormat.LineAlignment = StringAlignment.Near;
                        pageLegendStringFormat.Trimming = StringTrimming.None;
                        pageLegendStringFormat.FormatFlags = StringFormatFlags.NoWrap;

                        var RwrLegendRectangle = new Rectangle(114, 98, 30, 30);
                        DrawString(gfx, "RWR", TestTextFontLarge, Brushes.Lime, RwrLegendRectangle,
                                           pageLegendStringFormat);
                        var Rwr2LegendRectangle = new Rectangle(114, 118, 30, 30);
                        DrawString(gfx, "SYSTEM GO", TestTextFontLarge, Brushes.Lime, Rwr2LegendRectangle,
                                           pageLegendStringFormat);
                    }
                    if (InstrumentState.RWRTest2)
                    {
                        var pageLegendStringFormat = new StringFormat();
                        pageLegendStringFormat.Alignment = StringAlignment.Center;
                        pageLegendStringFormat.LineAlignment = StringAlignment.Near;
                        pageLegendStringFormat.Trimming = StringTrimming.None;
                        pageLegendStringFormat.FormatFlags = StringFormatFlags.NoWrap;

                        var RwrLegendRectangle = new Rectangle(86, 80, 30, 30);
                        DrawString(gfx, "F16C", TestTextFontLarge, Brushes.Lime, RwrLegendRectangle,
                                           pageLegendStringFormat);
                        var Rwr1LegendRectangle = new Rectangle(105, 100, 30, 30);
                        DrawString(gfx, "1*OFP  0020", TestTextFontLarge, Brushes.Lime, Rwr1LegendRectangle,
                                           pageLegendStringFormat);
                        var Rwr2LegendRectangle = new Rectangle(105, 120, 30, 30);
                        DrawString(gfx, "2*WO   0040", TestTextFontLarge, Brushes.Lime, Rwr2LegendRectangle,
                                           pageLegendStringFormat);
                        var Rwr3LegendRectangle = new Rectangle(119, 140, 30, 30);
                        DrawString(gfx, "PA   0050", TestTextFontLarge, Brushes.Lime, Rwr3LegendRectangle,
                                           pageLegendStringFormat);
                        var Rwr4LegendRectangle = new Rectangle(119, 160, 30, 30);
                        DrawString(gfx, "US   0060", TestTextFontLarge, Brushes.Lime, Rwr4LegendRectangle,
                                           pageLegendStringFormat);
                    }

                    GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
                    if (Options.Style == InstrumentStyle.AdvancedThreatDisplay)
                    {
                        gfx.Transform = initialTransform;
                        gfx.TranslateTransform(atdRingOffsetTranslateX, atdRingOffsetTranslateY);
                        gfx.ScaleTransform(atdRingScale, atdRingScale);
                    }
                    if (!InstrumentState.RWRTest1 && !InstrumentState.RWRTest2) //Added Falcas 07-11-2012, Do not draw when in Test.
                    {
                        if (DateTime.Now.Millisecond < 500)
                        {
                            gfx.DrawLine(
                                ScopeGreenPen,
                                new PointF((backgroundWidth / 2.0f) + 10, (backgroundHeight / 2.0f)),
                                new PointF((backgroundWidth / 2.0f) + 10, (backgroundHeight / 2.0f) + 5)
                                );
                        }
                        else
                        {
                            gfx.DrawLine(
                                ScopeGreenPen,
                                new PointF((backgroundWidth / 2.0f) + 10, (backgroundHeight / 2.0f)),
                                new PointF((backgroundWidth / 2.0f) + 10, (backgroundHeight / 2.0f) - 5)
                                );
                        }
                    }
                    GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
                }

                //Added Falcas 07-11-2012, Do not draw the blips if in test mode.
                if (InstrumentState.Blips != null && InstrumentState.RWRPowerOn && !InstrumentState.RWRTest1 && !InstrumentState.RWRTest2)
                {
                    foreach (var blip in InstrumentState.Blips)
                    {
                        if (blip == null) continue;
                        if (!blip.Visible) continue;

                        GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);

                        if (Options.Style == InstrumentStyle.AdvancedThreatDisplay)
                        {
                            gfx.Transform = initialTransform;
                            gfx.TranslateTransform(atdRingOffsetTranslateX, atdRingOffsetTranslateY);
                            gfx.ScaleTransform(atdRingScale, atdRingScale);
                        }
                        //calculate position of symbols
                        var lethality = blip.Lethality;
                        var translateY = 0.0f;
                        if (lethality > 1)
                        {
                            translateY = -((2.0f - lethality) * 100.0f) * 0.95f;
                            ;
                        }
                        else
                        {
                            translateY = -((1.0f - lethality) * 100.0f) * 0.95f;
                            ;
                        }
                        if (Options.Style == InstrumentStyle.AdvancedThreatDisplay)
                        {
                            translateY *= 0.92f;
                        }
                        //rotate and translate symbol into place
                        var angle = -InstrumentState.MagneticHeadingDegrees + blip.BearingDegrees;
                        if (InstrumentState.Inverted)
                        {
                            angle = -angle;
                        }
                        //rotate the background image so that this emitter's bearing line points toward the top
                        gfx.TranslateTransform(backgroundWidth/2.0f, backgroundHeight/2.0f);
                        gfx.RotateTransform(angle);
                        gfx.TranslateTransform(-(float) backgroundWidth/2.0f, -(float) backgroundHeight/2.0f);

                        Brush missileWarningBrush = new SolidBrush(missileColor);
                        var missileWarningFormat = new StringFormat
                        {
                            Alignment = StringAlignment.Far,
                            LineAlignment = StringAlignment.Center
                        };
                        var launchLinePen = new Pen(missileColor);
                        var center = new PointF(
                            (backgroundWidth/2.0f),
                            (backgroundHeight/2.0f)
                            );

                        //draw missile launch line
                        if (Options.Style == InstrumentStyle.AdvancedThreatDisplay)
                        {
                            var endPoint = new PointF(center.X, outerRingTop);
                            // + (RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS / 2.0f));
                            if ((blip.MissileActivity > 0 || blip.MissileLaunch > 0))
                            {
                                gfx.DrawLine(launchLinePen, center.X, innerRingTop, endPoint.X, endPoint.Y);
                            }

                            if ((blip.MissileActivity > 0 || blip.MissileLaunch > 0))
                            {
                                var angleToDisplay = angle%360;
                                if (angleToDisplay < 0) angleToDisplay = 360 - Math.Abs(angleToDisplay);
                                if (angleToDisplay == 0) angleToDisplay = 360;
                                var angleString = string.Format("{0:000}", angleToDisplay);

                                var textWidth = gfx.MeasureString(angleString, MissileWarningFont).Width;
                                var missileWarningTextLocation = new PointF(
                                    endPoint.X,
                                    endPoint.Y - ((endPoint.Y - innerRingTop)/2.0f) - (textWidth/2.0f)
                                    );
                                var oldTransform = gfx.Transform;
                                gfx.TranslateTransform(missileWarningTextLocation.X, missileWarningTextLocation.Y);
                                gfx.RotateTransform(-angle);
                                gfx.TranslateTransform(-missileWarningTextLocation.X, -missileWarningTextLocation.Y);

                                gfx.DrawString
                                    (
                                        angleString,
                                        MissileWarningFont,
                                        missileWarningBrush,
                                        missileWarningTextLocation
                                    );
                                gfx.Transform = oldTransform;
                            }
                        }
                        //position the emitter symbol at the correct distance from the center of the RWR, given its lethality
                        gfx.TranslateTransform(0, translateY);

                        //rotate the emitter symbol graphic so that it appears upright when background is rotated back and displayed to user
                        gfx.TranslateTransform(backgroundWidth/2.0f, backgroundHeight/2.0f);
                        gfx.RotateTransform(-angle);
                        gfx.TranslateTransform(-backgroundWidth/2.0f, -backgroundHeight/2.0f);

                        //draw the emitter symbol
                        var usePrimarySymbol = DateTime.Now.Millisecond < 500;
                        var useLargeSymbol = DateTime.Now.Millisecond < 500 && blip.NewDetection > 0;

                        var emitterColor = ScopeGreenColor;
                        if (Options.Style == InstrumentStyle.AdvancedThreatDisplay)
                        {
                            var category = GetEmitterCategory(blip.SymbolID);
                            switch (category)
                            {
                                case EmitterCategory.AirborneThreat:
                                    emitterColor = airborneThreatColor;
                                    break;
                                case EmitterCategory.GroundThreat:
                                    emitterColor = groundThreatColor;
                                    break;
                                case EmitterCategory.Search:
                                    emitterColor = searchThreatColor;
                                    break;
                                case EmitterCategory.Missile:
                                    emitterColor = missileColor;
                                    break;
                                case EmitterCategory.Naval:
                                    emitterColor = navalThreatColor;
                                    break;
                                case EmitterCategory.Unknown:
                                    emitterColor = unknownThreatColor;
                                    break;
                                default:
                                    break;
                            }
                        }
                        var emitterSymbolDestinationRectangle =
                            new RectangleF((int) ((backgroundWidth - RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS)/2.0f),
                                           (int) ((backgroundHeight - RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS)/2.0f),
                                           RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS, RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS);
                        DrawEmitterSymbol(blip.SymbolID, gfx, emitterSymbolDestinationRectangle, useLargeSymbol,
                                          usePrimarySymbol, emitterColor);

                        var emitterPen = new Pen(emitterColor);
                        gfx.TranslateTransform(-RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS/2.0f,
                                               -RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS/2.0f);
                        if (blip.Selected > 0)
                        {
                            //draw "selected threat " diamond
                            var points = new[]
                                             {
                                                 new PointF(
                                                     (backgroundWidth/2.0f) + (RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS/2.0f),
                                                     backgroundHeight/2.0f),
                                                 new PointF((backgroundWidth/2.0f),
                                                            (backgroundHeight/2.0f) +
                                                            (RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS/2.0f)),
                                                 new PointF(
                                                     (backgroundWidth/2.0f) + (RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS/2.0f),
                                                     (backgroundHeight/2.0f) + RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS),
                                                 new PointF((backgroundWidth/2.0f) + RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS,
                                                            (backgroundHeight/2.0f) +
                                                            RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS/2.0f)
                                             };
                            gfx.DrawPolygon(emitterPen, points);
                        }
                        if ((blip.MissileActivity > 0 && blip.MissileLaunch == 0))
                        {
                            if (DateTime.Now.Millisecond%250 < 125)
                                //flash the symbol by only drawing it some of the time
                            {
                                //draw missile activity symbol
                                emitterPen.DashStyle = DashStyle.Dash;
                                gfx.DrawEllipse(emitterPen,
                                                new RectangleF((backgroundWidth/2.0f) + 4, (backgroundHeight/2.0f) + 4,
                                                               RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS - 8,
                                                               RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS - 8));
                                emitterPen.DashStyle = DashStyle.Solid;
                            }
                        }
                        else if ((blip.MissileActivity > 0 && blip.MissileLaunch > 0))
                        {
                            //draw missile launch symbol 
                            gfx.DrawEllipse(emitterPen,
                                            new RectangleF(backgroundWidth/2.0f, backgroundHeight/2.0f,
                                                           RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS,
                                                           RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS));
                        }

                        GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
                    } //next blip
                } //end if this.InstrumentState.Blips !=null

                //restore the canvas's transform and clip settings to what they were when we entered this method
                gfx.Restore(initialState);
            }
            if (fullBright != null)
            {
                var ia = new ImageAttributes();
                var dimmingMatrix =
                    Util.GetDimmingColorMatrix(InstrumentState.Brightness/(float) InstrumentState.MaxBrightness);
                ia.SetColorMatrix(dimmingMatrix);
                destinationGraphics.DrawImage(fullBright, destinationRectangle, 0, 0, fullBright.Width, fullBright.Height, GraphicsUnit.Pixel, ia);
                Common.Util.DisposeObject(gfx);
                Common.Util.DisposeObject(fullBright);
            }
        }

        private bool AreNonVisiblePriorityThreatsDetected()
        {
            if (InstrumentState == null || InstrumentState.Blips == null) return false;

            if (InstrumentState.PriorityMode)
            {
                var trackingOwnshipCount = 0;
                var visibleCount = 0;
                for (var i = 0; i < InstrumentState.Blips.Length; i++)
                {
                    var thisBlip = InstrumentState.Blips[i];
                    if (thisBlip == null) continue;
                    if (thisBlip.Lethality == 0) continue;
                    trackingOwnshipCount++;
                    if (thisBlip.Visible)
                    {
                        visibleCount++;
                        continue;
                    }
                }
                if (visibleCount == 5 && trackingOwnshipCount > visibleCount) return true;
            }
            return false;
        }

        private bool AreNonVisibleNavalThreatsDetected()
        {
            if (InstrumentState == null || InstrumentState.Blips == null) return false;
            for (var i = 0; i < InstrumentState.Blips.Length; i++)
            {
                var thisBlip = InstrumentState.Blips[i];
                if (thisBlip == null) continue;
                if (thisBlip.Lethality == 0) continue;
                if (thisBlip.Visible) continue;
                var symbolId = thisBlip.SymbolID;
                if (symbolId == 18)
                {
                    return true;
                }
            }
            return false;
        }

        private bool AreNonVisibleSearchThreatsDetected()
        {
            if (InstrumentState == null || InstrumentState.Blips == null) return false;
            if (InstrumentState.SearchMode) return false;
            for (var i = 0; i < InstrumentState.Blips.Length; i++)
            {
                var thisBlip = InstrumentState.Blips[i];
                if (thisBlip == null) continue;
                if (thisBlip.Lethality == 0) continue;
                if (thisBlip.Visible) continue;
                var symbolId = thisBlip.SymbolID;
                if (
                    (
                        (symbolId >= 5 && symbolId <= 17)
                        ||
                        (symbolId >= 19 && symbolId <= 26)
                        ||
                        (symbolId == 30)
                        ||
                        (symbolId >= 54 && symbolId <= 56)
                    )
                    )
                {
                    return true;
                }
            }
            return false;
        }

        private bool AreNonVisibleUnknownThreatsDetected()
        {
            if (InstrumentState == null || InstrumentState.Blips == null) return false;
            for (var i = 0; i < InstrumentState.Blips.Length; i++)
            {
                var thisBlip = InstrumentState.Blips[i];
                if (thisBlip == null) continue;
                if (thisBlip.Visible) continue;
                if (thisBlip.Lethality == 0) continue;
                var symbolId = thisBlip.SymbolID;
                if (symbolId < 0 || symbolId == 1 || symbolId == 27 || symbolId == 28 || symbolId == 29)
                {
                    return true;
                }
            }
            return false;
        }

        private static void DrawEmitterSymbol(int symbolId, Graphics g, RectangleF bounds, bool largeSize, bool primarySymbol,
                                       Color color)
        {
            var originalTransform = g.Transform;
            var symbolFont = largeSize ? SymbolTextFontLarge : SymbolTextFontSmall;

            var x = bounds.X;
            var y = bounds.Y;

            var basicInterceptorPoints = new[]
                                             {
                                                 new PointF(x + 8, y + 2), //top center
                                                 new PointF(x - 1, y + 8), //left bottom
                                                 new PointF(x + 1, y + 8), //left of center on bottom
                                                 new PointF(x + 8, y + 6), //center point on bottom
                                                 new PointF(x + 15, y + 8), //right of center on bottom
                                                 new PointF(x + 17, y + 8) //right bottom
                                             };
            for (var i = 0; i < basicInterceptorPoints.Length; i++)
            {
                var p = basicInterceptorPoints[i];
                var p2 = new PointF(p.X + (bounds.Width/4.0f), p.Y + (bounds.Height/4.0f));
                basicInterceptorPoints[i] = p2;
            }
            var advancedInterceptorPoints = new[]
                                                {
                                                    new PointF(x + 8, y + 2), //top center
                                                    new PointF(x - 1, y + 8), //left bottom
                                                    new PointF(x + 5, y + 8), //left of center on bottom
                                                    new PointF(x + 8, y + 12), //center point on bottom
                                                    new PointF(x + 11, y + 8), //right of center on bottom
                                                    new PointF(x + 17, y + 8) //right bottom
                                                };
            for (var i = 0; i < advancedInterceptorPoints.Length; i++)
            {
                var p = advancedInterceptorPoints[i];
                var p2 = new PointF(p.X + (bounds.Width/4.0f), p.Y + (bounds.Height/4.0f));
                advancedInterceptorPoints[i] = p2;
            }
            var airborneThreatSymbolPoints = new[]
                                                 {new PointF(x + 4, y), new PointF(x + 8, y - 4), new PointF(x + 12, y)};
            for (var i = 0; i < airborneThreatSymbolPoints.Length; i++)
            {
                var p = airborneThreatSymbolPoints[i];
                var p2 = new PointF(p.X + (bounds.Width/4.0f), p.Y + (bounds.Height/4.0f));
                airborneThreatSymbolPoints[i] = p2;
            }
            var shipSymbolPoints = new[]
                                       {
                                           new PointF(x + 6.5f, -3 + y + (RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS/4.0f)),
                                           new PointF(x + 6.5f, -3 + y + 1.5f + (RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS/4.0f))
                                           ,
                                           new PointF(x, -3 + y + 1.5f + (RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS/4.0f)),
                                           new PointF(x + 3.5f, -3 + y + 4.5f + (RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS/4.0f))
                                           ,
                                           new PointF(x + 15, -3 + y + 4.5f + (RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS/4.0f)),
                                           new PointF(x + 15, -3 + y + 1.5f + (RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS/4.0f)),
                                           new PointF(x + 11, -3 + y + 1.5f + (RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS/4.0f)),
                                           new PointF(x + 11, -3 + y + (RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS/4.0f))
                                       };
            for (var i = 0; i < shipSymbolPoints.Length; i++)
            {
                var p = shipSymbolPoints[i];
                var p2 = new PointF(p.X + (bounds.Width/4.0f), p.Y + (bounds.Height/4.0f));
                shipSymbolPoints[i] = p2;
            }


            bounds.Offset(0, 2);
            var sf = new StringFormat(StringFormatFlags.NoWrap)
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            const int lineSpacing = 4;
            using (Brush brush = new SolidBrush(color))
            using (var pen = new Pen(color))
            {
                switch (symbolId)
                {
                    case 0:
                        break;
                    case 1:
                        DrawString(g, "U", symbolFont, brush, bounds, sf);
                        break;
                    case 2:
                        g.DrawPolygon(pen, advancedInterceptorPoints);
                        break;
                    case 3:
                        g.DrawPolygon(pen, basicInterceptorPoints);
                        break;
                    case 4:
                        DrawString(g, "M", symbolFont, brush, bounds, sf);
                        break;
                    case 5:
                        DrawString(g, "H", symbolFont, brush, bounds, sf);
                        break;
                    case 6:
                        DrawString(g, "P", symbolFont, brush, bounds, sf);
                        break;
                    case 7:
                        DrawString(g, "2", symbolFont, brush, bounds, sf);
                        break;
                    case 8:
                        DrawString(g, "3", symbolFont, brush, bounds, sf);
                        break;
                    case 9:
                        DrawString(g, "4", symbolFont, brush, bounds, sf);
                        break;
                    case 10:
                        DrawString(g, "5", symbolFont, brush, bounds, sf);
                        break;
                    case 11:
                        DrawString(g, "6", symbolFont, brush, bounds, sf);
                        break;
                    case 12:
                        DrawString(g, "8", symbolFont, brush, bounds, sf);
                        break;
                    case 13:
                        DrawString(g, "9", symbolFont, brush, bounds, sf);
                        break;
                    case 14:
                        DrawString(g, "10", symbolFont, brush, bounds, sf);
                        break;
                    case 15:
                        DrawString(g, "13", symbolFont, brush, bounds, sf);
                        break;
                    case 16:
                        if (primarySymbol)
                        {
                            DrawString(g, "A", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            DrawString(g, "S", symbolFont, brush, bounds, sf);
                        }
                        break;
                    case 17:
                        DrawString(g, "S", symbolFont, brush, bounds, sf);
                        break;
                    case 18:
                        g.DrawPolygon(pen, shipSymbolPoints);
                        break;
                    case 19:
                        DrawString(g, "C", symbolFont, brush, bounds, sf);
                        break;
                    case 20:
                        if (primarySymbol)
                        {
                            DrawString(g, "15", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            DrawString(g, "M", symbolFont, brush, bounds, sf);
                        }
                        break;
                    case 21:
                        DrawString(g, "N", symbolFont, brush, bounds, sf);
                        break;
                    case 22:
                        DrawString(g, "A", symbolFont, brush, bounds, sf);
                        bounds.Offset(0, lineSpacing);
                        DrawString(g, ".", symbolFont, brush, bounds, sf);
                        break;
                    case 23:
                        DrawString(g, "A", symbolFont, brush, bounds, sf);
                        bounds.Offset(0, lineSpacing);
                        DrawString(g, "..", symbolFont, brush, bounds, sf);
                        break;
                    case 24:
                        DrawString(g, "A", symbolFont, brush, bounds, sf);
                        bounds.Offset(0, lineSpacing);
                        DrawString(g, "...", symbolFont, brush, bounds, sf);
                        break;
                    case 25:
                        DrawString(g, "P", symbolFont, brush, bounds, sf);
                        bounds.Offset(0, lineSpacing);
                        DrawString(g, ".", symbolFont, brush, bounds, sf);
                        break;
                    case 26:
                        DrawString(g, "P|", symbolFont, brush, bounds, sf);
                        break;
                    case 27:
                        DrawString(g, "U", symbolFont, brush, bounds, sf);
                        bounds.Offset(0, lineSpacing);
                        DrawString(g, ".", symbolFont, brush, bounds, sf);
                        break;
                    case 28:
                        DrawString(g, "U", symbolFont, brush, bounds, sf);
                        bounds.Offset(0, lineSpacing);
                        DrawString(g, "..", symbolFont, brush, bounds, sf);
                        break;
                    case 29:
                        DrawString(g, "U", symbolFont, brush, bounds, sf);
                        bounds.Offset(0, lineSpacing);
                        DrawString(g, "...", symbolFont, brush, bounds, sf);
                        break;
                    case 30:
                        DrawString(g, "C", symbolFont, brush, bounds, sf);
                        break;
                    case 31:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "1", symbolFont, brush, bounds, sf);
                        break;
                    case 32:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "4", symbolFont, brush, bounds, sf);
                        break;
                    case 33:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "5", symbolFont, brush, bounds, sf);
                        break;
                    case 34:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "6", symbolFont, brush, bounds, sf);
                        break;
                    case 35:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "14", symbolFont, brush, bounds, sf);
                        break;
                    case 36:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "15", symbolFont, brush, bounds, sf);
                        break;
                    case 37:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "16", symbolFont, brush, bounds, sf);
                        break;
                    case 38:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "18", symbolFont, brush, bounds, sf);
                        break;
                    case 39:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "19", symbolFont, brush, bounds, sf);
                        break;
                    case 40:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "20", symbolFont, brush, bounds, sf);
                        break;
                    case 41:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "21", symbolFont, brush, bounds, sf);
                        break;
                    case 42:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "22", symbolFont, brush, bounds, sf);
                        break;
                    case 43:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "23", symbolFont, brush, bounds, sf);
                        break;
                    case 44:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "25", symbolFont, brush, bounds, sf);
                        break;
                    case 45:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "27", symbolFont, brush, bounds, sf);
                        break;
                    case 46:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "29", symbolFont, brush, bounds, sf);
                        break;
                    case 47:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "30", symbolFont, brush, bounds, sf);
                        break;
                    case 48:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "31", symbolFont, brush, bounds, sf);
                        break;
                    case 49:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "P", symbolFont, brush, bounds, sf);
                        break;
                    case 50:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "PD", symbolFont, brush, bounds, sf);
                        break;
                    case 51:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "A", symbolFont, brush, bounds, sf);
                        break;
                    case 52:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "B", symbolFont, brush, bounds, sf);
                        break;
                    case 53:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        DrawString(g, "S", symbolFont, brush, bounds, sf);
                        break;
                    case 54:
                        DrawString(g, " A|", symbolFont, brush, bounds, sf);
                        break;
                    case 55:
                        DrawString(g, "|A|", symbolFont, brush, bounds, sf);
                        break;
                    case 56:
                        DrawString(g, "|||", symbolFont, brush, bounds, sf);
                        DrawString(g, "A", symbolFont, brush, bounds, sf);
                        break;
                    case 57:
                        if (primarySymbol)
                        {
                            DrawString(g, "F", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            DrawString(g, "S", symbolFont, brush, bounds, sf);
                        }
                        break;
                    case 58:
                        if (primarySymbol)
                        {
                            DrawString(g, "F", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            DrawString(g, "A", symbolFont, brush, bounds, sf);
                        }
                        break;
                    case 59:
                        if (primarySymbol)
                        {
                            DrawString(g, "F", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            DrawString(g, "M", symbolFont, brush, bounds, sf);
                        }
                        break;
                    case 60:
                        if (primarySymbol)
                        {
                            DrawString(g, "F", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            DrawString(g, "U", symbolFont, brush, bounds, sf);
                        }
                        break;
                    case 61:
                        if (primarySymbol)
                        {
                            DrawString(g, "F", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            g.DrawPolygon(pen, basicInterceptorPoints);
                        }
                        break;
                    case 62:
                        if (primarySymbol)
                        {
                            DrawString(g, "S", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            g.DrawPolygon(pen, basicInterceptorPoints);
                        }
                        break;
                    case 63:
                        if (primarySymbol)
                        {
                            DrawString(g, "A", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            g.DrawPolygon(pen, basicInterceptorPoints);
                        }
                        break;
                    case 64:
                        if (primarySymbol)
                        {
                            DrawString(g, "M", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            g.DrawPolygon(pen, basicInterceptorPoints);
                        }
                        break;

                    default:
                        if (symbolId >= 100)
                        {
                            var symbolString = (symbolId - 100).ToString();
                            DrawString(g, symbolString, symbolFont, brush, bounds, sf);
                        }
                        else if (symbolId >= 65)
                        {
                            var symbolString = Encoding.ASCII.GetString(new[] {(byte) symbolId});
                            DrawString(g, symbolString, symbolFont, brush, bounds, sf);
                        }
                        else if (symbolId < 0)
                        {
                            DrawString(g, "U", symbolFont, brush, bounds, sf);
                        }
                        break;
                }
            }
            g.Transform = originalTransform;
        }

        private EmitterCategory GetEmitterCategory(int symbolId)
        {
            EmitterCategory category;
            switch (symbolId)
            {
                case (int) ThreatSymbols.RWRSYM_ADVANCED_INTERCEPTOR:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_BASIC_INTERCEPTOR:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_ACTIVE_MISSILE:
                    category = EmitterCategory.Missile;
                    break;
                case (int) ThreatSymbols.RWRSYM_HAWK:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_PATRIOT:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_SA2:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_SA3:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_SA4:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_SA5:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_SA6:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_SA8:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_SA9:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_SA10:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_SA13:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_AAA:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_SEARCH:
                    category = EmitterCategory.Search;
                    break;
                case (int) ThreatSymbols.RWRSYM_NAVAL:
                    category = EmitterCategory.Naval;
                    break;
                case (int) ThreatSymbols.RWRSYM_CHAPARAL:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_SA15:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_NIKE:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_A1:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_A2:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_A3:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_PDOT:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_PSLASH:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_UNK1:
                    category = EmitterCategory.Unknown;
                    break;
                case (int) ThreatSymbols.RWRSYM_UNK2:
                    category = EmitterCategory.Unknown;
                    break;
                case (int) ThreatSymbols.RWRSYM_UNK3:
                    category = EmitterCategory.Unknown;
                    break;
                case (int) ThreatSymbols.RWRSYM_KSAM:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V1:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V4:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V5:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V6:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V14:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V15:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V16:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V18:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V19:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V20:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V21:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V22:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V23:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V25:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V27:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V29:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V30:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_V31:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_VP:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_VPD:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_VA:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_VB:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_VS:
                    category = EmitterCategory.AirborneThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_Aa:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_Ab:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_Ac:
                    category = EmitterCategory.GroundThreat;
                    break;
                case (int) ThreatSymbols.RWRSYM_MIB_F_S:
                    category = EmitterCategory.Unknown;
                    break;
                case (int) ThreatSymbols.RWRSYM_MIB_F_A:
                    category = EmitterCategory.Unknown;
                    break;
                case (int) ThreatSymbols.RWRSYM_MIB_F_M:
                    category = EmitterCategory.Unknown;
                    break;
                case (int) ThreatSymbols.RWRSYM_MIB_F_U:
                    category = EmitterCategory.Unknown;
                    break;
                case (int) ThreatSymbols.RWRSYM_MIB_F_BW:
                    category = EmitterCategory.Unknown;
                    break;
                case (int) ThreatSymbols.RWRSYM_MIB_BW_S:
                    category = EmitterCategory.Unknown;
                    break;
                case (int) ThreatSymbols.RWRSYM_MIB_BW_A:
                    category = EmitterCategory.Unknown;
                    break;
                case (int) ThreatSymbols.RWRSYM_MIB_BW_M:
                    category = EmitterCategory.Unknown;
                    break;
                default:
                    category = EmitterCategory.Unknown;
                    break;
            }
            return category;
        }

        //Added Falcas 10-11-2012
        private static EWMSMode GetEWMSMode(int cmdsMode)
        {
            EWMSMode mode;
            switch (cmdsMode)
            {
                case (int)EWMSMode.Off:
                    mode = EWMSMode.Off;
                    break;
                case (int)EWMSMode.Standby:
                    mode = EWMSMode.Standby;
                    break;
                case (int)EWMSMode.Manual:
                    mode = EWMSMode.Manual;
                    break;
                case (int)EWMSMode.Semiautomatic:
                    mode = EWMSMode.Semiautomatic;
                    break;
                case (int)EWMSMode.Automatic:
                    mode = EWMSMode.Automatic;
                    break;
                case (int)EWMSMode.Bypass:
                    mode = EWMSMode.Bypass;
                    break;
                default:
                    mode = EWMSMode.Off;
                    break;
            }
            return mode;
        }


        public enum InstrumentStyle
        {
            IP1310ALR,
            HAF,
            AdvancedThreatDisplay
        }
    }
}