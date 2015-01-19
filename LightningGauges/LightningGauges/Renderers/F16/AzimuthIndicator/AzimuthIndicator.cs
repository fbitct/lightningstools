﻿using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
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
                    UNKLegendRenderer.DrawUNKLegend(gfx, leftLegend1Rectangle, verticalOsbLegendLHSFormat, InstrumentState, OSBLegendFont, OSBLegendBrush);
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
                    EWSDispenserStatusRenderer.DrawEWSDispenserStatus(gfx, goNogoRect, miscTextStringFormat, warnColor, okColor, rdyRectangle, pflRectangle, ScopeGreenColor, OtherLegendFont, InstrumentState);
                    EWSModeRenderer.DrawEWSMode(severeColor, gfx, ewmsModeRectangle, miscTextStringFormat, warnColor, okColor, InstrumentState, OtherLegendFont);
                    PageLegendsRenderer.DrawPageLegends(backgroundHeight, gfx, InstrumentState, PageLegendFont);

                    gfx.Transform = initialTransform;

                        gfx.TranslateTransform(atdRingOffsetTranslateX, atdRingOffsetTranslateY);
                        gfx.ScaleTransform(atdRingScale, atdRingScale);
                        var grayPen = Pens.Gray;
                        const int lineLength = 10;
                        OuterLethalityRingRenderer.DrawOuterLethalityRing(gfx, outerRingLeft, outerRingTop, grayPen, atdOuterRingDiameter, lineLength);
                        MiddleLethalityRingRenderer.DrawMiddleLethalityRing(gfx, middleRingLeft, middleRingTop, grayPen, atdMiddleRingDiameter, lineLength, InstrumentState);

                    InnerLethalityRingRenderer.DrawInnerLethalityRing(gfx, grayPen, innerRingLeft, innerRingTop, atdInnerRingDiameter);

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
                    HeartbeatCrossRenderer.DrawHeartbeatCross(gfx, ref basicState, backgroundWidth, backgroundHeight, ScopeGreenPen);

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
                        HeartbeatTickRenderer.DrawHeartbeatTick(gfx, backgroundWidth, backgroundHeight, ScopeGreenPen);
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

        private void DrawBlips(Graphics gfx, GraphicsState basicState, Matrix initialTransform, float atdRingOffsetTranslateX,
            float atdRingOffsetTranslateY, float atdRingScale, int backgroundWidth, int backgroundHeight, Color missileColor,
            float outerRingTop, float innerRingTop, Color airborneThreatColor, Color groundThreatColor, Color searchThreatColor,
            Color navalThreatColor, Color unknownThreatColor)
        {
            foreach (var blip in InstrumentState.Blips)
            {
                if (blip == null) continue;
                if (!blip.Visible) continue;

                BlipRenderer.DrawBlip(gfx, ref basicState, initialTransform, atdRingOffsetTranslateX, atdRingOffsetTranslateY, atdRingScale, backgroundWidth, backgroundHeight, missileColor, outerRingTop, innerRingTop, ScopeGreenColor, airborneThreatColor, groundThreatColor, searchThreatColor, navalThreatColor, unknownThreatColor, blip, InstrumentState, Options, MissileWarningFont, RWR_SYMBOL_SUBIMAGE_WIDTH_PIXELS, SymbolTextFontLarge, SymbolTextFontSmall);
            } //next blip
        }


        public enum InstrumentStyle
        {
            IP1310ALR,
            HAF,
            AdvancedThreatDisplay
        }
    }
}