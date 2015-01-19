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
                                (NonVisibleUnknownThreatsDetector.AreNonVisibleUnknownThreatsDetected(InstrumentState)
                                    && DateTime.Now.Millisecond % 500 < 250
                                )
                            )
                            ) //draw highlighted UNK legend 
                        {
                            gfx.FillRectangle(OSBLegendBrush, leftLegend1Rectangle);
                            StringRenderer.DrawString(gfx, "UNK", OSBLegendFont, Brushes.Black, leftLegend1Rectangle,
                                       verticalOsbLegendLHSFormat);
                        }
                        else //draw non-highlighted UNK legend
                        {
                            StringRenderer.DrawString(gfx, "UNK", OSBLegendFont, OSBLegendBrush, leftLegend1Rectangle,
                                       verticalOsbLegendLHSFormat);
                        }

                    HOFFLegendRenderer.DrawHOFFLegend(gfx, leftLegend2Rectangle, verticalOsbLegendLHSFormat, InstrumentState, OSBLegendFont, OSBLegendBrush);
                    SEPLegendRenderer.DrawSEPLegend(gfx, leftLegend3Rectangle, verticalOsbLegendLHSFormat, InstrumentState, OSBLegendFont, OSBLegendBrush);
                    PRILegendRenderer.DrawPRILegend(gfx, leftLegend4Rectangle, verticalOsbLegendLHSFormat, InstrumentState, OSBLegendBrush, OSBLegendFont);
                    NVLLegendRenderer.DrawNVLLegend(gfx, rightLegend1Rectangle, verticalOsbLegendRHSFormat, InstrumentState, OSBLegendBrush, OSBLegendFont);
                    SRCHLegendRenderer.DrawSRCHLegend(gfx, rightLegend2Rectangle, verticalOsbLegendRHSFormat, InstrumentState, OSBLegendFont, OSBLegendBrush);
                    ALTLegendRenderer.DrawALTLegend(gfx, rightLegend3Rectangle, verticalOsbLegendRHSFormat, InstrumentState, OSBLegendBrush, OSBLegendFont);

                    ChaffCountRenderer.DrawChaffCount(severeColor, warnColor, okColor, gfx, chaffCountRectangle, chaffFlareCountStringFormat, InstrumentState, ChaffFlareCountLegendFont);
                    FlareCountRenderer.DrawFlareCount(severeColor, warnColor, okColor, gfx, flareCountRectangle, chaffFlareCountStringFormat, InstrumentState, ChaffFlareCountLegendFont);
                    Other1CountRenderer.DrawOther1Count(severeColor, warnColor, okColor, gfx, other1CountRectangle, chaffFlareCountStringFormat, InstrumentState, ChaffFlareCountLegendFont);
                    Other2CountRenderer.DrawOther2Count(severeColor, warnColor, okColor, gfx, other2CountRectangle, chaffFlareCountStringFormat, InstrumentState, ChaffFlareCountLegendFont);

                    if (InstrumentState.EWSGo)
                        {
                            var legendColor = ScopeGreenColor;
                            Brush legendBrush = new SolidBrush(legendColor);
                            StringRenderer.DrawString(gfx, "GO", OtherLegendFont, legendBrush, goNogoRect, miscTextStringFormat);
                        }
                        else if (InstrumentState.EWSNoGo)
                        {
                            var legendColor = warnColor;
                            Brush legendBrush = new SolidBrush(legendColor);
                            StringRenderer.DrawString(gfx, "NOGO", OtherLegendFont, legendBrush, goNogoRect, miscTextStringFormat);
                        }
                        else if (InstrumentState.EWSDispenseReady)
                        {
                            var legendColor = ScopeGreenColor;
                            Brush legendBrush = new SolidBrush(legendColor);
                            StringRenderer.DrawString(gfx, "DISP", OtherLegendFont, legendBrush, goNogoRect, miscTextStringFormat);
                        }


                        if (InstrumentState.EWSDispenseReady)
                        {
                            var legendColor = okColor;
                            Brush legendBrush = new SolidBrush(legendColor);
                            StringRenderer.DrawString(gfx, "RDY", OtherLegendFont, legendBrush, rdyRectangle, miscTextStringFormat);
                        }

                        if (InstrumentState.EWSDegraded) //draw PFL legend 
                        {
                            var legendColor = warnColor;
                            Brush legendBrush = new SolidBrush(legendColor);
                            StringRenderer.DrawString(gfx, "PFL", OtherLegendFont, legendBrush, pflRectangle, miscTextStringFormat);
                        }

                    EWSModeRenderer.DrawEWSMode(severeColor, gfx, ewmsModeRectangle, miscTextStringFormat, warnColor, okColor, InstrumentState, OtherLegendFont);

                    PageLegendsRenderer.DrawPageLegends(backgroundHeight, gfx, InstrumentState, PageLegendFont);

                    gfx.Transform = initialTransform;

                        gfx.TranslateTransform(atdRingOffsetTranslateX, atdRingOffsetTranslateY);
                        gfx.ScaleTransform(atdRingScale, atdRingScale);
                        var grayPen = Pens.Gray;
                        const int lineLength = 10;
                        //draw the outer lethality ring
                        {
                            DrawOuterLethalityRing(gfx, outerRingLeft, outerRingTop, grayPen, atdOuterRingDiameter, lineLength);
                        }

                        //draw the middle lethality ring 
                        {
                            MiddleLethalityRingRenderer.DrawMiddleLethalityRing(gfx, middleRingLeft, middleRingTop, grayPen, atdMiddleRingDiameter, lineLength, InstrumentState);
                        }

                        // draw the inner lethality ring
                        gfx.DrawEllipse(grayPen, innerRingLeft, innerRingTop, atdInnerRingDiameter, atdInnerRingDiameter);

                        if (!InstrumentState.RWRPowerOn) //draw RWR POWER OFF flag
                        {
                            PowerOffFlagRenderer.DrawPowerOffFlag(atdInnerRingDiameter, backgroundWidth, backgroundHeight, severeColor, gfx);
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
                        CenterRWRSearchModeIndicationRenderer.DrawCenterRWRSearchModeIndication(gfx, TestTextFontLarge);
                    }
                }

                
                if (InstrumentState.RWRPowerOn)
                {
                    //Added Falcas 07-11-2012
                    //RWR test
                    if (InstrumentState.RWRTest1)
                    {
                        RWRTestPage1Renderer.DrawRWRTestPage1(gfx, TestTextFontLarge);
                    }
                    if (InstrumentState.RWRTest2)
                    {
                        RWRTestPage2Renderer.DrawRWRTestPage2(gfx, TestTextFontLarge);
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
                        DrawHeartbeatTick(gfx, backgroundWidth, backgroundHeight);
                    }
                    GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
                }

                //Added Falcas 07-11-2012, Do not draw the blips if in test mode.
                if (InstrumentState.Blips != null && InstrumentState.RWRPowerOn && !InstrumentState.RWRTest1 && !InstrumentState.RWRTest2)
                {
                    DrawBlips(gfx, basicState, initialTransform, atdRingOffsetTranslateX, atdRingOffsetTranslateY, atdRingScale, backgroundWidth, backgroundHeight, missileColor, outerRingTop, innerRingTop, airborneThreatColor, groundThreatColor, searchThreatColor, navalThreatColor, unknownThreatColor);
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

        private static void DrawOuterLethalityRing(Graphics gfx, float outerRingLeft, float outerRingTop, Pen grayPen,
            float atdOuterRingDiameter, int lineLength)
        {
            var toRestore = gfx.Transform;
            gfx.TranslateTransform(outerRingLeft, outerRingTop);
            gfx.DrawEllipse(grayPen, 0, 0, atdOuterRingDiameter, atdOuterRingDiameter);

            for (var i = 0; i <= 180; i += 30)
            {
                var previousTransform = gfx.Transform;
                gfx.TranslateTransform(atdOuterRingDiameter/2.0f, atdOuterRingDiameter/2.0f);
                gfx.RotateTransform(i);
                gfx.TranslateTransform(-atdOuterRingDiameter/2.0f, -atdOuterRingDiameter/2.0f);
                if (i%90 == 0)
                {
                    gfx.DrawLine(grayPen, atdOuterRingDiameter/2.0f, 0, atdOuterRingDiameter/2.0f,
                        lineLength*2);
                    gfx.DrawLine(grayPen, atdOuterRingDiameter/2.0f, atdOuterRingDiameter,
                        atdOuterRingDiameter/2.0f, atdOuterRingDiameter - (lineLength*2));
                }
                else
                {
                    gfx.DrawLine(grayPen, atdOuterRingDiameter/2.0f, 0, atdOuterRingDiameter/2.0f,
                        lineLength);
                    gfx.DrawLine(grayPen, atdOuterRingDiameter/2.0f, atdOuterRingDiameter,
                        atdOuterRingDiameter/2.0f, atdOuterRingDiameter - lineLength);
                }
                gfx.Transform = previousTransform;
            }
            gfx.Transform = toRestore;
        }

        private static void DrawHeartbeatTick(Graphics gfx, int backgroundWidth, int backgroundHeight)
        {
            if (DateTime.Now.Millisecond < 500)
            {
                gfx.DrawLine(
                    ScopeGreenPen,
                    new PointF((backgroundWidth/2.0f) + 10, (backgroundHeight/2.0f)),
                    new PointF((backgroundWidth/2.0f) + 10, (backgroundHeight/2.0f) + 5)
                    );
            }
            else
            {
                gfx.DrawLine(
                    ScopeGreenPen,
                    new PointF((backgroundWidth/2.0f) + 10, (backgroundHeight/2.0f)),
                    new PointF((backgroundWidth/2.0f) + 10, (backgroundHeight/2.0f) - 5)
                    );
            }
        }

        private void DrawBlips(Graphics gfx, GraphicsState basicState, Matrix initialTransform, float atdRingOffsetTranslateX,
            float atdRingOffsetTranslateY, float atdRingScale, int backgroundWidth, int backgroundHeight, Color missileColor,
            float outerRingTop, float innerRingTop, Color airborneThreatColor, Color groundThreatColor, Color searchThreatColor,
            Color navalThreatColor, Color unknownThreatColor)
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
                    translateY = -((2.0f - lethality)*100.0f)*0.95f;
                    ;
                }
                else
                {
                    translateY = -((1.0f - lethality)*100.0f)*0.95f;
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
                    var category = EmitterCategoryRetriever.GetEmitterCategory(blip.SymbolID);
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
                        StringRenderer.DrawString(g, "U", symbolFont, brush, bounds, sf);
                        break;
                    case 2:
                        g.DrawPolygon(pen, advancedInterceptorPoints);
                        break;
                    case 3:
                        g.DrawPolygon(pen, basicInterceptorPoints);
                        break;
                    case 4:
                        StringRenderer.DrawString(g, "M", symbolFont, brush, bounds, sf);
                        break;
                    case 5:
                        StringRenderer.DrawString(g, "H", symbolFont, brush, bounds, sf);
                        break;
                    case 6:
                        StringRenderer.DrawString(g, "P", symbolFont, brush, bounds, sf);
                        break;
                    case 7:
                        StringRenderer.DrawString(g, "2", symbolFont, brush, bounds, sf);
                        break;
                    case 8:
                        StringRenderer.DrawString(g, "3", symbolFont, brush, bounds, sf);
                        break;
                    case 9:
                        StringRenderer.DrawString(g, "4", symbolFont, brush, bounds, sf);
                        break;
                    case 10:
                        StringRenderer.DrawString(g, "5", symbolFont, brush, bounds, sf);
                        break;
                    case 11:
                        StringRenderer.DrawString(g, "6", symbolFont, brush, bounds, sf);
                        break;
                    case 12:
                        StringRenderer.DrawString(g, "8", symbolFont, brush, bounds, sf);
                        break;
                    case 13:
                        StringRenderer.DrawString(g, "9", symbolFont, brush, bounds, sf);
                        break;
                    case 14:
                        StringRenderer.DrawString(g, "10", symbolFont, brush, bounds, sf);
                        break;
                    case 15:
                        StringRenderer.DrawString(g, "13", symbolFont, brush, bounds, sf);
                        break;
                    case 16:
                        if (primarySymbol)
                        {
                            StringRenderer.DrawString(g, "A", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            StringRenderer.DrawString(g, "S", symbolFont, brush, bounds, sf);
                        }
                        break;
                    case 17:
                        StringRenderer.DrawString(g, "S", symbolFont, brush, bounds, sf);
                        break;
                    case 18:
                        g.DrawPolygon(pen, shipSymbolPoints);
                        break;
                    case 19:
                        StringRenderer.DrawString(g, "C", symbolFont, brush, bounds, sf);
                        break;
                    case 20:
                        if (primarySymbol)
                        {
                            StringRenderer.DrawString(g, "15", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            StringRenderer.DrawString(g, "M", symbolFont, brush, bounds, sf);
                        }
                        break;
                    case 21:
                        StringRenderer.DrawString(g, "N", symbolFont, brush, bounds, sf);
                        break;
                    case 22:
                        StringRenderer.DrawString(g, "A", symbolFont, brush, bounds, sf);
                        bounds.Offset(0, lineSpacing);
                        StringRenderer.DrawString(g, ".", symbolFont, brush, bounds, sf);
                        break;
                    case 23:
                        StringRenderer.DrawString(g, "A", symbolFont, brush, bounds, sf);
                        bounds.Offset(0, lineSpacing);
                        StringRenderer.DrawString(g, "..", symbolFont, brush, bounds, sf);
                        break;
                    case 24:
                        StringRenderer.DrawString(g, "A", symbolFont, brush, bounds, sf);
                        bounds.Offset(0, lineSpacing);
                        StringRenderer.DrawString(g, "...", symbolFont, brush, bounds, sf);
                        break;
                    case 25:
                        StringRenderer.DrawString(g, "P", symbolFont, brush, bounds, sf);
                        bounds.Offset(0, lineSpacing);
                        StringRenderer.DrawString(g, ".", symbolFont, brush, bounds, sf);
                        break;
                    case 26:
                        StringRenderer.DrawString(g, "P|", symbolFont, brush, bounds, sf);
                        break;
                    case 27:
                        StringRenderer.DrawString(g, "U", symbolFont, brush, bounds, sf);
                        bounds.Offset(0, lineSpacing);
                        StringRenderer.DrawString(g, ".", symbolFont, brush, bounds, sf);
                        break;
                    case 28:
                        StringRenderer.DrawString(g, "U", symbolFont, brush, bounds, sf);
                        bounds.Offset(0, lineSpacing);
                        StringRenderer.DrawString(g, "..", symbolFont, brush, bounds, sf);
                        break;
                    case 29:
                        StringRenderer.DrawString(g, "U", symbolFont, brush, bounds, sf);
                        bounds.Offset(0, lineSpacing);
                        StringRenderer.DrawString(g, "...", symbolFont, brush, bounds, sf);
                        break;
                    case 30:
                        StringRenderer.DrawString(g, "C", symbolFont, brush, bounds, sf);
                        break;
                    case 31:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "1", symbolFont, brush, bounds, sf);
                        break;
                    case 32:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "4", symbolFont, brush, bounds, sf);
                        break;
                    case 33:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "5", symbolFont, brush, bounds, sf);
                        break;
                    case 34:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "6", symbolFont, brush, bounds, sf);
                        break;
                    case 35:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "14", symbolFont, brush, bounds, sf);
                        break;
                    case 36:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "15", symbolFont, brush, bounds, sf);
                        break;
                    case 37:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "16", symbolFont, brush, bounds, sf);
                        break;
                    case 38:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "18", symbolFont, brush, bounds, sf);
                        break;
                    case 39:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "19", symbolFont, brush, bounds, sf);
                        break;
                    case 40:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "20", symbolFont, brush, bounds, sf);
                        break;
                    case 41:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "21", symbolFont, brush, bounds, sf);
                        break;
                    case 42:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "22", symbolFont, brush, bounds, sf);
                        break;
                    case 43:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "23", symbolFont, brush, bounds, sf);
                        break;
                    case 44:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "25", symbolFont, brush, bounds, sf);
                        break;
                    case 45:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "27", symbolFont, brush, bounds, sf);
                        break;
                    case 46:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "29", symbolFont, brush, bounds, sf);
                        break;
                    case 47:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "30", symbolFont, brush, bounds, sf);
                        break;
                    case 48:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "31", symbolFont, brush, bounds, sf);
                        break;
                    case 49:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "P", symbolFont, brush, bounds, sf);
                        break;
                    case 50:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "PD", symbolFont, brush, bounds, sf);
                        break;
                    case 51:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "A", symbolFont, brush, bounds, sf);
                        break;
                    case 52:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "B", symbolFont, brush, bounds, sf);
                        break;
                    case 53:
                        g.DrawLines(pen, airborneThreatSymbolPoints);
                        StringRenderer.DrawString(g, "S", symbolFont, brush, bounds, sf);
                        break;
                    case 54:
                        StringRenderer.DrawString(g, " A|", symbolFont, brush, bounds, sf);
                        break;
                    case 55:
                        StringRenderer.DrawString(g, "|A|", symbolFont, brush, bounds, sf);
                        break;
                    case 56:
                        StringRenderer.DrawString(g, "|||", symbolFont, brush, bounds, sf);
                        StringRenderer.DrawString(g, "A", symbolFont, brush, bounds, sf);
                        break;
                    case 57:
                        if (primarySymbol)
                        {
                            StringRenderer.DrawString(g, "F", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            StringRenderer.DrawString(g, "S", symbolFont, brush, bounds, sf);
                        }
                        break;
                    case 58:
                        if (primarySymbol)
                        {
                            StringRenderer.DrawString(g, "F", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            StringRenderer.DrawString(g, "A", symbolFont, brush, bounds, sf);
                        }
                        break;
                    case 59:
                        if (primarySymbol)
                        {
                            StringRenderer.DrawString(g, "F", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            StringRenderer.DrawString(g, "M", symbolFont, brush, bounds, sf);
                        }
                        break;
                    case 60:
                        if (primarySymbol)
                        {
                            StringRenderer.DrawString(g, "F", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            StringRenderer.DrawString(g, "U", symbolFont, brush, bounds, sf);
                        }
                        break;
                    case 61:
                        if (primarySymbol)
                        {
                            StringRenderer.DrawString(g, "F", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            g.DrawPolygon(pen, basicInterceptorPoints);
                        }
                        break;
                    case 62:
                        if (primarySymbol)
                        {
                            StringRenderer.DrawString(g, "S", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            g.DrawPolygon(pen, basicInterceptorPoints);
                        }
                        break;
                    case 63:
                        if (primarySymbol)
                        {
                            StringRenderer.DrawString(g, "A", symbolFont, brush, bounds, sf);
                        }
                        else
                        {
                            g.DrawPolygon(pen, basicInterceptorPoints);
                        }
                        break;
                    case 64:
                        if (primarySymbol)
                        {
                            StringRenderer.DrawString(g, "M", symbolFont, brush, bounds, sf);
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
                            StringRenderer.DrawString(g, symbolString, symbolFont, brush, bounds, sf);
                        }
                        else if (symbolId >= 65)
                        {
                            var symbolString = Encoding.ASCII.GetString(new[] {(byte) symbolId});
                            StringRenderer.DrawString(g, symbolString, symbolFont, brush, bounds, sf);
                        }
                        else if (symbolId < 0)
                        {
                            StringRenderer.DrawString(g, "U", symbolFont, brush, bounds, sf);
                        }
                        break;
                }
            }
            g.Transform = originalTransform;
        }


        public enum InstrumentStyle
        {
            IP1310ALR,
            HAF,
            AdvancedThreatDisplay
        }
    }
}