using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Common.SimSupport;
using System.IO;
using System.Drawing.Drawing2D;
using Common.Imaging;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Diagnostics;
using Common.UI;
using log4net;

namespace LightningGauges.Renderers
{
    public class F16EHSI : InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants
        private static string IMAGES_FOLDER_NAME = new DirectoryInfo (System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)).FullName + Path.DirectorySeparatorChar + "images";
        #endregion

        #region Instance variables
        private static ILog _log = LogManager.GetLogger(typeof(F16EHSI));
        private static object _imagesLock = new object();
        private static bool _imagesLoaded = false;
        private const string EHSI_NO_DATA_IMAGE_FILENAME = "ehsioff.bmp";
        private const string EHSI_NO_DATA_MASK_FILENAME = "ehsioff_mask.bmp";
        private bool _disposed = false;
        private static ImageMaskPair _noData = null;
        private PrivateFontCollection _fonts = new PrivateFontCollection();
        #endregion

        public F16EHSI()
            : base()
        {
            _fonts.AddFontFile("isisdigits.ttf");
            _fonts.AddFontFile("ehsidigits.ttf");
            this.InstrumentState = new F16EHSIInstrumentState();
            this.Options = new F16EHSIOptions();
        }
        #region Initialization Code
        private void LoadImageResources()
        {
            if (_noData == null)
            {
                _noData= ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + EHSI_NO_DATA_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + EHSI_NO_DATA_MASK_FILENAME
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
            Graphics gfx = g;
            Bitmap fullBright = null;
            if (this.InstrumentState.Brightness != this.InstrumentState.MaxBrightness)
            {
                fullBright = new Bitmap(bounds.Size.Width, bounds.Size.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                gfx = Graphics.FromImage(fullBright);
            }
            lock (_imagesLock)
            {
                //store the canvas's transform and clip settings so we can restore them later
                GraphicsState initialState = gfx.Save();

                //set up the canvas scale and clipping region
                int width = 512;
                int height = 512;

                int scaleWidth = bounds.Width;
                int scaleHeight = bounds.Height;

                if (scaleHeight > scaleWidth) scaleHeight = scaleWidth;
                if (scaleWidth > scaleHeight) scaleWidth = scaleHeight;


                gfx.ResetTransform(); //clear any existing transforms
                gfx.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                gfx.FillRectangle(Brushes.Black, bounds);

                float translateX = 0;
                float translateY = 0;
                if (scaleWidth < bounds.Width)
                {
                    translateX = (bounds.Width - scaleWidth) / 2.0f;
                }
                if (scaleHeight < bounds.Height)
                {
                    translateY = (bounds.Height - scaleHeight) / 2.0f;
                }
                gfx.TranslateTransform(translateX, translateY);
                gfx.ScaleTransform((float)scaleWidth/ (float)width, (float)scaleHeight/ (float)height); //set the initial scale transformation 

                gfx.InterpolationMode = this.Options.GDIPlusOptions.InterpolationMode;
                gfx.PixelOffsetMode = this.Options.GDIPlusOptions.PixelOffsetMode;
                gfx.SmoothingMode = this.Options.GDIPlusOptions.SmoothingMode;
                gfx.TextRenderingHint = this.Options.GDIPlusOptions.TextRenderingHint;

                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                GraphicsState basicState = gfx.Save();

                RectangleF outerRect = new RectangleF(0, 0, width, height);

                if (this.InstrumentState.NoDataFlag)
                {
                    DrawNoDataFlag(gfx, outerRect);
                }
                else
                {
                    
                    //draw the compass
                    DrawCompassRose(gfx, outerRect);

                    //draw the desired heading bug
                    DrawDesiredHeadingBug(gfx, outerRect);

                    //draw course deviation needles
                    DrawCourseDeviationNeedles(gfx, outerRect);

                    //draw distance to beacon digits
                    DrawDistanceToBeacon(gfx, outerRect);

                    //draw desired course digits
                    DrawDesiredCourse(gfx, outerRect);

                    //draw heading and course select knob labels
                    DrawHeadingAndCourseAdjustLabels(gfx, outerRect);

                    //draw instrument mode label
                    DrawInstrumentMode(gfx, outerRect);

                    //draw bearing to beacon indicator
                    DrawBearingToBeaconIndicator(gfx, outerRect);

                    //draw INU invalid flag if needed
                    if (this.InstrumentState.INUInvalidFlag)
                    {
                        Color redFlagColor = Color.FromArgb(224, 43, 48);
                        DrawTextWarningFlag(gfx, outerRect, "I\nN\nU", redFlagColor, Color.White);
                    }

                    //draw ATT invalid flag
                    if (this.InstrumentState.AttitudeFailureFlag)
                    {
                        Color yellowFlagColor = Color.FromArgb(244, 240, 55);
                        DrawTextWarningFlag(gfx, outerRect, "A\nT\nT", yellowFlagColor, Color.Black);
                    }

                    //draw airplane symbol
                    DrawAirplaneSymbol(gfx, outerRect);

                }
                if (this.InstrumentState.ShowBrightnessLabel)
                {
                    DrawBrightnessLabel(gfx, outerRect);
                }

                //restore the canvas's transform and clip settings to what they were when we entered this method
                gfx.Restore(initialState);
            }
            if (fullBright != null)
            {
                ImageAttributes ia = new ImageAttributes();
                ColorMatrix dimmingMatrix = Common.Imaging.Util.GetDimmingColorMatrix((float)this.InstrumentState.Brightness / (float)this.InstrumentState.MaxBrightness);
                ia.SetColorMatrix(dimmingMatrix);
                g.DrawImage(fullBright, bounds, 0, 0, fullBright.Width, fullBright.Height,GraphicsUnit.Pixel, ia);
                Common.Util.DisposeObject(gfx);
                Common.Util.DisposeObject(fullBright);
            }
        }
        private void DrawBrightnessLabel(Graphics g, RectangleF outerBounds)
        {
            DrawCenterLabel(g, outerBounds, "BRT");
        }
        private void DrawCenterLabel(Graphics g, RectangleF outerBounds, string label)
        {
            FontFamily fontFamily = _fonts.Families[0];
            Font labelFont = new Font(fontFamily, 25, FontStyle.Bold, GraphicsUnit.Point);

            StringFormat labelStringFormat = new StringFormat();
            labelStringFormat.Alignment = StringAlignment.Center;
            labelStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap ;
            labelStringFormat.LineAlignment = StringAlignment.Center;
            labelStringFormat.Trimming = StringTrimming.None;
            SizeF charSize = g.MeasureString("A", labelFont);
            RectangleF labelRect = new RectangleF(
                new PointF(
                    0,
                    (((outerBounds.Y + outerBounds.Height) - charSize.Height) / 2.0f)
                    ),
                new SizeF(
                        outerBounds.Width, 
                        charSize.Height
                    )
                );
            labelRect.Offset(0,-40);

            g.DrawString(label, labelFont, Brushes.White, labelRect, labelStringFormat);
        }
        private void DrawAirplaneSymbol(Graphics g, RectangleF outerBounds)
        {
            PointF middle = new PointF(outerBounds.X +  (outerBounds.Width / 2.0f), outerBounds.Y +  (outerBounds.Height / 2.0f));
            float airplaneFuselageLength = 45;
            float airplaneTailWidth = 23;
            float airplaneWingWidth = 45;
            float gap = airplaneFuselageLength / 2.0f;

            Pen symbolPen = new Pen(Color.White);
            symbolPen.Width = 3;

            //draw wings
            PointF wingLeft = new PointF(middle.X - (airplaneWingWidth / 2.0f), middle.Y - (gap / 2.0f));
            PointF wingRight = new PointF(middle.X + (airplaneWingWidth / 2.0f), middle.Y - (gap / 2.0f));
            g.DrawLine(symbolPen, wingLeft, wingRight);

            //draw tail
            PointF tailLeft = new PointF(middle.X - (airplaneTailWidth / 2.0f), middle.Y + (gap / 2.0f));
            PointF tailRight = new PointF(middle.X + (airplaneTailWidth / 2.0f), middle.Y + (gap / 2.0f));
            g.DrawLine(symbolPen, tailLeft, tailRight);

            //draw fuselage
            PointF fuselageTop = new PointF(middle.X, middle.Y - (airplaneFuselageLength / 2.0f));
            PointF fuselageBottom = new PointF(middle.X, middle.Y + (airplaneFuselageLength / 2.0f));
            g.DrawLine(symbolPen, fuselageTop, fuselageBottom);



        }
        private void DrawBearingToBeaconIndicator(Graphics g, RectangleF outerBounds)
        {
            Color needleColor = Color.FromArgb(102, 190, 157);
            Brush needleBrush = new SolidBrush(needleColor);
            Pen needlePen = new Pen(needleColor);
            needlePen.Width = 4;
            GraphicsState basicState = g.Save();
            
            g.TranslateTransform(outerBounds.Width / 2.0f, outerBounds.Height / 2.0f);
            g.RotateTransform(-this.InstrumentState.MagneticHeadingDegrees);
            g.RotateTransform(this.InstrumentState.BearingToBeaconDegrees);
            g.TranslateTransform(-outerBounds.Width / 2.0f, -outerBounds.Height / 2.0f);

            
            g.TranslateTransform(outerBounds.Width / 2.0f,0);

            float bearingTriangleWidth = 23;
            float bearingTriangleHeight = 25;

            PointF bearingTriangleTop = new PointF(0, 5);
            PointF bearingTriangleLeft = new PointF(-bearingTriangleWidth / 2.0f, bearingTriangleTop.Y + bearingTriangleHeight);
            PointF bearingTriangleRight = new PointF(bearingTriangleWidth / 2.0f, bearingTriangleTop.Y + bearingTriangleHeight);
            g.FillPolygon(needleBrush, new PointF[] { bearingTriangleTop, bearingTriangleLeft, bearingTriangleRight });

            float bearingLineTopHeight = 23;
            PointF bearingLineTopTop = new PointF(bearingTriangleTop.X, bearingTriangleLeft.Y);
            PointF bearingLineTopBottom = new PointF(bearingLineTopTop.X, bearingLineTopTop.Y + bearingLineTopHeight);
            g.DrawLine(needlePen, bearingLineTopTop, bearingLineTopBottom);

            float bearingLineBottomHeight=37;
            PointF bearingLineBottomTop = new PointF(bearingTriangleTop.X, 455);
            PointF bearingLineBottomBottom = new PointF(bearingTriangleTop.X, bearingLineBottomTop.Y + bearingLineBottomHeight);
            g.DrawLine(needlePen, bearingLineBottomTop, bearingLineBottomBottom);


            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
        }
        private void DrawTextWarningFlag(Graphics g, RectangleF outerBounds, string flagText, Color flagColor, Color textColor)
        {
            Brush flagBrush = new SolidBrush(flagColor);
            FontFamily fontFamily = _fonts.Families[0];
            Font inuFlagFont = new Font(fontFamily, 20, FontStyle.Bold, GraphicsUnit.Point);

            StringFormat inuFlagStringFormat = new StringFormat();
            inuFlagStringFormat.Alignment = StringAlignment.Center;
            inuFlagStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
            inuFlagStringFormat.LineAlignment = StringAlignment.Center;
            inuFlagStringFormat.Trimming = StringTrimming.None;

            PointF flagLocation = new PointF(12, 75);
            SizeF flagSize = new SizeF(25, 75);
            RectangleF flagRect = new RectangleF(flagLocation, flagSize);
            g.FillRectangle(flagBrush, flagRect);
            Brush textBrush = new SolidBrush(textColor);
            g.DrawString(flagText, inuFlagFont, textBrush, flagRect, inuFlagStringFormat);                 
        }
        private void DrawCourseDeviationNeedles(Graphics g, RectangleF outerBounds)
        {
            GraphicsState initialState = g.Save();
            Color redFlagColor = Color.FromArgb(224, 43, 48);
            Color needleColor = Color.FromArgb(102, 190, 157);
            Brush redFlagBrush = new SolidBrush(redFlagColor);
            Brush needleBrush = new SolidBrush(needleColor);
            Pen needlePen = new Pen(needleColor);

            float pointerNeedleThickWidth = 8;
            float pointerNeedleThinWidth = 3;
            float maxDeviationX = 110;
            float dotHeight = 15;
            float dotY = (outerBounds.Height - dotHeight) / 2.0f;
            float leftInnerDotX = (-maxDeviationX / 2.0f) - (dotHeight / 2.0f);
            float leftOuterDotX = -maxDeviationX - (dotHeight / 2.0f);
            float rightInnerDotX = (maxDeviationX / 2.0f) - (dotHeight / 2.0f);
            float rightOuterDotX = maxDeviationX - (dotHeight / 2.0f);
            float topIndicatorLineHeight = 50;
            float coursePointerTriangleWidth = 30;
            float coursePointerTriangleHeight = 20;
            float pointerNeedleThickHeightTop = 40;
            float cdiNeedleHeight = 198;
            float deviationTranslateX = maxDeviationX * Math.Sign(this.InstrumentState.CourseDeviationDegrees) * (Math.Abs(this.InstrumentState.CourseDeviationDegrees) / Math.Abs(this.InstrumentState.CourseDeviationLimitDegrees));
            float pointerNeedleThickHeightBottom = 60;
            float bottomIndicatorLineHeight = 50;
            PointF courseNeedleTopLineTop = new PointF(0, 43);
            PointF courseNeedleTopLineBottom = new PointF(0, courseNeedleTopLineTop.Y + topIndicatorLineHeight);
            PointF coursePointerTriangleLeft = new PointF(courseNeedleTopLineBottom.X - (coursePointerTriangleWidth / 2.0f), courseNeedleTopLineBottom.Y + coursePointerTriangleHeight);
            PointF coursePointerTriangleRight = new PointF(courseNeedleTopLineBottom.X + (coursePointerTriangleWidth / 2.0f), coursePointerTriangleLeft.Y);
            PointF coursePointerTriangleTop = new PointF(0, courseNeedleTopLineBottom.Y - 2);
            PointF pointerNeedleThickTopTop = new PointF(0, coursePointerTriangleLeft.Y);
            PointF pointerNeedleThickTopBottom = new PointF(0, pointerNeedleThickTopTop.Y + pointerNeedleThickHeightTop);
            RectangleF deviationInvalidFlagRect = new RectangleF(new PointF(-80, pointerNeedleThickTopBottom.Y), new SizeF(60, 30));
            float toFromFlagHeight = deviationInvalidFlagRect.Height;
            float toFromFlagWidth = deviationInvalidFlagRect.Height;
            PointF cdiLineTop = new PointF(0, pointerNeedleThickTopBottom.Y+2);
            PointF cdiLineBottom = new PointF(0, cdiLineTop.Y + cdiNeedleHeight);
            PointF pointerNeedleThickBottomTop = new PointF(0, cdiLineBottom.Y + 2);
            PointF pointerNeedleThickBottomBottom = new PointF(0, pointerNeedleThickBottomTop.Y + pointerNeedleThickHeightBottom);
            PointF courseNeedleBottomLineTop = new PointF(0, pointerNeedleThickBottomBottom.Y);
            PointF courseNeedleBottomLineBottom = new PointF(0, courseNeedleBottomLineTop.Y + bottomIndicatorLineHeight);
            PointF toFlagTop = new PointF((maxDeviationX / 2.0f), pointerNeedleThickTopBottom.Y);
            PointF toFlagLeft = new PointF(toFlagTop.X - (toFromFlagWidth / 2.0f), toFlagTop.Y + toFromFlagHeight);
            PointF toFlagRight = new PointF(toFlagTop.X + (toFromFlagWidth / 2.0f), toFlagTop.Y + toFromFlagHeight);
            PointF fromFlagBottom = new PointF((maxDeviationX / 2.0f), pointerNeedleThickBottomTop.Y);
            PointF fromFlagLeft = new PointF(fromFlagBottom.X - (toFromFlagWidth / 2.0f), fromFlagBottom.Y - toFromFlagHeight);
            PointF fromFlagRight = new PointF(fromFlagBottom.X + (toFromFlagWidth / 2.0f), fromFlagBottom.Y - toFromFlagHeight);

            g.TranslateTransform(outerBounds.Width/2.0f, outerBounds.Height/2.0f);
            g.RotateTransform(-this.InstrumentState.MagneticHeadingDegrees);
            g.RotateTransform(this.InstrumentState.DesiredCourseDegrees);
            g.TranslateTransform(-outerBounds.Width / 2.0f, -outerBounds.Height / 2.0f);

            g.TranslateTransform(outerBounds.Width / 2.0f, 0);

            //draw deviation dots
            g.FillEllipse(Brushes.White, new RectangleF(leftInnerDotX, dotY, dotHeight,dotHeight));
            g.FillEllipse(Brushes.White, new RectangleF(leftOuterDotX, dotY, dotHeight, dotHeight));
            g.FillEllipse(Brushes.White, new RectangleF(rightInnerDotX, dotY, dotHeight, dotHeight));
            g.FillEllipse(Brushes.White, new RectangleF(rightOuterDotX, dotY, dotHeight, dotHeight));

            //draw thin line on top of pointer arrow
            needlePen.Width = pointerNeedleThinWidth;
            g.DrawLine(needlePen, courseNeedleTopLineTop, courseNeedleTopLineBottom);

            //draw pointer arrow
            g.FillPolygon(needleBrush, new PointF[] { coursePointerTriangleTop, coursePointerTriangleLeft, coursePointerTriangleRight});

            //draw thick line just below pointer arrow
            needlePen.Width = pointerNeedleThickWidth;
            g.DrawLine(needlePen, pointerNeedleThickTopTop, pointerNeedleThickTopBottom);


            
            if (this.InstrumentState.DeviationInvalidFlag)
            {
                //draw deviation invalid flag
                g.FillRectangle(redFlagBrush, deviationInvalidFlagRect);
            }
            //draw CDI needle
            needlePen.Width = pointerNeedleThickWidth;
            if (this.InstrumentState.DeviationInvalidFlag)
            {
                needlePen.DashPattern = new float[] {2,1.75f};
            }
            g.TranslateTransform(deviationTranslateX,0);
            g.DrawLine(needlePen, cdiLineTop, cdiLineBottom);
            g.TranslateTransform(-deviationTranslateX,0);

            needlePen.DashStyle = DashStyle.Solid;

            //draw thick line just below CDI needle  
            needlePen.Width = pointerNeedleThickWidth;
            g.DrawLine(needlePen, pointerNeedleThickBottomTop, pointerNeedleThickBottomBottom);

            //draw thin line indicating reciprocal-of-course
            needlePen.Width = pointerNeedleThinWidth;
            g.DrawLine(needlePen, courseNeedleBottomLineTop, courseNeedleBottomLineBottom);

            if (this.InstrumentState.ShowToFromFlag)
            {
                if (this.InstrumentState.ToFlag)
                {
                    g.FillPolygon(Brushes.White, new PointF[] { toFlagTop, toFlagLeft, toFlagRight });
                }
                if (this.InstrumentState.FromFlag)
                {
                    g.FillPolygon(Brushes.White, new PointF[] { fromFlagBottom, fromFlagLeft, fromFlagRight });
                }
            }

            
            GraphicsUtil.RestoreGraphicsState(g, ref initialState);

        }
        private void DrawDesiredHeadingBug(Graphics g, RectangleF outerBounds)
        {
            GraphicsState basicState = g.Save();
            g.TranslateTransform(outerBounds.Width / 2.0f, outerBounds.Height / 2.0f);
            g.RotateTransform(-this.InstrumentState.MagneticHeadingDegrees);
            g.RotateTransform(this.InstrumentState.DesiredHeadingDegrees);
            g.TranslateTransform(-outerBounds.Width / 2.0f, -outerBounds.Height / 2.0f);

            float headingBugSquareSize=20;
            float headingBugGapBetweenSquares=5;
            float headingBugSquareTop = 18;
            float centerX = outerBounds.X + ( outerBounds.Width / 2.0f);
            PointF leftHeadingBugSquareLocation = new PointF(centerX - headingBugSquareSize - (headingBugGapBetweenSquares/2.0f), headingBugSquareTop);
            PointF rightHeadingBugSquareLocation = new PointF(centerX + (headingBugGapBetweenSquares / 2.0f), headingBugSquareTop);
            RectangleF headingBugLeftSquare = new RectangleF(leftHeadingBugSquareLocation, new SizeF(headingBugSquareSize, headingBugSquareSize));
            RectangleF headingBugRightSquare = new RectangleF(rightHeadingBugSquareLocation, new SizeF(headingBugSquareSize, headingBugSquareSize));
            Color headingBugColor = Color.FromArgb(248, 238, 153);
            Brush headingBugBrush = new SolidBrush(headingBugColor);
            g.FillRectangle(headingBugBrush, headingBugLeftSquare);
            g.FillRectangle(headingBugBrush, headingBugRightSquare);
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
        }
        private void DrawNoDataFlag(Graphics g, RectangleF outerBounds)
        {
            if (this.InstrumentState.NoDataFlag)
            {
                g.FillRectangle(Brushes.Black, outerBounds);
                if (_noData.MaskedImage != null)
                {
                    g.DrawImage(_noData.MaskedImage, outerBounds, new RectangleF(new Point(0, 0), _noData.MaskedImage.Size), GraphicsUnit.Pixel);
                }
                else
                {
                    _log.Debug("_noData.MaskedImage was null in DrawNoDataFlag");   
                }
            }

        }
        private void DrawInstrumentMode(Graphics g, RectangleF outerBounds)
        {
            FontFamily fontFamily = _fonts.Families[0];
            Font labelFont = new Font(fontFamily, 25, FontStyle.Bold, GraphicsUnit.Point);

            StringFormat labelStringFormat = new StringFormat();
            labelStringFormat.Alignment = StringAlignment.Center;
            labelStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
            labelStringFormat.LineAlignment = StringAlignment.Center;
            labelStringFormat.Trimming = StringTrimming.None;

            float letterHeight = 20;
            float margin = 8;
            float labelWidth=50;

            TimeSpan howLongSinceInstrumentModeChanged = DateTime.Now.Subtract(this.InstrumentState.WhenInstrumentModeLastChanged);
            if (howLongSinceInstrumentModeChanged.TotalMilliseconds<= 2000)
            {
                string toDisplay = string.Empty;
                switch (this.InstrumentState.InstrumentMode)
                {
                    case F16EHSIInstrumentState.InstrumentModes.Unknown:
                        break;
                    case F16EHSIInstrumentState.InstrumentModes.PlsTacan:
                        toDisplay = "PLS/TACAN";
                        break;
                    case F16EHSIInstrumentState.InstrumentModes.Tacan:
                        toDisplay = "TACAN";
                        break;
                    case F16EHSIInstrumentState.InstrumentModes.Nav:
                        toDisplay = "NAV";
                        break;
                    case F16EHSIInstrumentState.InstrumentModes.PlsNav:
                        toDisplay = "PLS/NAV";
                        break;
                    default:
                        break;
                }
                if (!this.InstrumentState.ShowBrightnessLabel)
                {
                    DrawCenterLabel(g, outerBounds, toDisplay);
                }
            }

            //draw PLS label
            if (
                this.InstrumentState.InstrumentMode == F16EHSIInstrumentState.InstrumentModes.PlsNav
                    ||
                this.InstrumentState.InstrumentMode == F16EHSIInstrumentState.InstrumentModes.PlsTacan
            )
            {
                RectangleF plsLabelRect = new RectangleF(outerBounds.Width * 0.25f, outerBounds.Height - letterHeight - margin, labelWidth, letterHeight);
                g.DrawString("PLS", labelFont, Brushes.White, plsLabelRect, labelStringFormat);
            }

            if (
                this.InstrumentState.InstrumentMode == F16EHSIInstrumentState.InstrumentModes.PlsNav
                    ||
                this.InstrumentState.InstrumentMode == F16EHSIInstrumentState.InstrumentModes.Nav
            )
            {
                RectangleF navLabelRect = new RectangleF(outerBounds.Width * 0.7f, outerBounds.Height - letterHeight - margin, labelWidth, letterHeight);
                g.DrawString("NAV", labelFont, Brushes.White, navLabelRect, labelStringFormat);
            }

            if (
                this.InstrumentState.InstrumentMode == F16EHSIInstrumentState.InstrumentModes.PlsTacan
                    ||
                this.InstrumentState.InstrumentMode == F16EHSIInstrumentState.InstrumentModes.Tacan
            )
            {
                RectangleF tacanLabelRect = new RectangleF(outerBounds.Width * 0.7f, outerBounds.Height - letterHeight - margin, labelWidth, letterHeight);
                g.DrawString("TCN", labelFont, Brushes.White, tacanLabelRect, labelStringFormat);
            }

        }
        private void DrawHeadingAndCourseAdjustLabels(Graphics g, RectangleF outerBounds)
        {
            FontFamily fontFamily = _fonts.Families[0];
            Font labelFont = new Font(fontFamily, 20, FontStyle.Bold, GraphicsUnit.Point);

            StringFormat labelStringFormat = new StringFormat();
            labelStringFormat.Alignment = StringAlignment.Center;
            labelStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
            labelStringFormat.LineAlignment = StringAlignment.Center;
            labelStringFormat.Trimming = StringTrimming.None;

            float letterHeight = 20;
            float letterWidth = 20;
            float toAddX = -5;
            float toAddY = -5;
            float margin = 40;
            //draw HDG label
            RectangleF hdgHRect = new RectangleF(margin, outerBounds.Height * 0.82f, letterWidth, letterHeight);
            g.DrawString("H", labelFont, Brushes.White, hdgHRect, labelStringFormat);

            RectangleF hdgDRect = new RectangleF(hdgHRect.X + hdgHRect.Width + toAddX, hdgHRect.Y + hdgHRect.Height + toAddY, letterWidth, letterHeight);
            g.DrawString("D", labelFont, Brushes.White, hdgDRect, labelStringFormat);

            RectangleF hdgGRect = new RectangleF(hdgDRect.X + hdgDRect.Width + toAddX, hdgDRect.Y + hdgDRect.Height + toAddY, letterWidth, letterHeight);
            g.DrawString("G", labelFont, Brushes.White, hdgGRect, labelStringFormat);


            //draw CRS label
            RectangleF crsCRect= new RectangleF(outerBounds.Width - ((letterWidth + toAddX)*3)- hdgHRect.X, hdgGRect.Y, letterWidth, letterHeight);
            g.DrawString("C", labelFont, Brushes.White, crsCRect, labelStringFormat);

            RectangleF crsRRect= new RectangleF(crsCRect.X + crsCRect.Width + toAddX, hdgDRect.Y, letterWidth, letterHeight);
            g.DrawString("R", labelFont, Brushes.White, crsRRect, labelStringFormat);

            RectangleF crsSRect = new RectangleF(crsRRect.X + crsRRect.Width + toAddX, hdgHRect.Y, letterWidth, letterHeight);
            g.DrawString("S", labelFont, Brushes.White, crsSRect, labelStringFormat);


        }
        private void DrawDesiredCourse(Graphics g, RectangleF outerBounds)
        {
            FontFamily fontFamily = _fonts.Families[0];
            Font digitsFont = new Font(fontFamily, 27.5f, FontStyle.Bold, GraphicsUnit.Point);
            Font crsFont = new Font(fontFamily, 20, FontStyle.Bold, GraphicsUnit.Point);

            StringFormat desiredCourseDigitStringFormat = new StringFormat();
            desiredCourseDigitStringFormat.Alignment = StringAlignment.Center;
            desiredCourseDigitStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
            desiredCourseDigitStringFormat.LineAlignment = StringAlignment.Center;
            desiredCourseDigitStringFormat.Trimming = StringTrimming.None;

            StringFormat crsStringFormat = new StringFormat();
            crsStringFormat.Alignment = StringAlignment.Far;
            crsStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
            crsStringFormat.LineAlignment = StringAlignment.Center;
            crsStringFormat.Trimming = StringTrimming.None;

            
            GraphicsState initialState = g.Save();
            g.InterpolationMode = this.Options.GDIPlusOptions.InterpolationMode;
            g.PixelOffsetMode = this.Options.GDIPlusOptions.PixelOffsetMode;
            g.SmoothingMode = this.Options.GDIPlusOptions.SmoothingMode;
            g.TextRenderingHint = this.Options.GDIPlusOptions.TextRenderingHint;

            GraphicsState basicState = g.Save();

            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            string desiredCourse = string.Format("{0:000.0}", this.InstrumentState.DesiredCourseDegrees);
            string hundredsDigit = desiredCourse.Substring(0, 1);
            string tensDigit = desiredCourse.Substring(1, 1);
            string onesDigit = desiredCourse.Substring(2, 1);

            float digitWidth = 22;
            float digitHeight = 32;
            float digitSeparationPixels = -2;
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            float margin = 8;
            RectangleF hundredsRect = new RectangleF(outerBounds.Width -margin-((digitWidth+digitSeparationPixels)*3), margin, digitWidth, digitHeight);
            RectangleF tensRect = new RectangleF(hundredsRect.X + digitWidth + digitSeparationPixels, hundredsRect.Y, digitWidth, digitHeight);
            RectangleF onesRect = new RectangleF(tensRect.X + digitWidth + digitSeparationPixels, tensRect.Y, digitWidth, digitHeight);

            g.DrawString(hundredsDigit, digitsFont, Brushes.White, hundredsRect, desiredCourseDigitStringFormat);
            g.DrawString(tensDigit, digitsFont, Brushes.White, tensRect, desiredCourseDigitStringFormat);
            g.DrawString(onesDigit, digitsFont, Brushes.White, onesRect, desiredCourseDigitStringFormat);

            RectangleF crsRect = new RectangleF(hundredsRect.X, 45, (digitWidth+digitSeparationPixels)*3, 20);
            g.DrawString("CRS", crsFont, Brushes.White, crsRect, crsStringFormat);

            GraphicsUtil.RestoreGraphicsState(g, ref initialState);
        }
       
        private void DrawDistanceToBeacon(Graphics g, RectangleF outerBounds)
        {
            FontFamily fontFamily = _fonts.Families[0];
            Font digitsFont = new Font(fontFamily, 27.5f, FontStyle.Bold, GraphicsUnit.Point);
            Font nmFont= new Font(fontFamily, 20, FontStyle.Bold, GraphicsUnit.Point);

            StringFormat distanceDigitStringFormat = new StringFormat();
            distanceDigitStringFormat.Alignment = StringAlignment.Center;
            distanceDigitStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
            distanceDigitStringFormat.LineAlignment = StringAlignment.Center;
            distanceDigitStringFormat.Trimming = StringTrimming.None;

            StringFormat nmStringFormat = new StringFormat();
            nmStringFormat.Alignment = StringAlignment.Center;
            nmStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
            nmStringFormat.LineAlignment = StringAlignment.Center;
            nmStringFormat.Trimming = StringTrimming.None;

            GraphicsState initialState = g.Save();
            g.InterpolationMode = this.Options.GDIPlusOptions.InterpolationMode;
            g.PixelOffsetMode = this.Options.GDIPlusOptions.PixelOffsetMode;
            g.SmoothingMode = this.Options.GDIPlusOptions.SmoothingMode;
            g.TextRenderingHint = this.Options.GDIPlusOptions.TextRenderingHint;
            GraphicsState basicState = g.Save();

            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            string distanceToBeaconString = string.Format("{0:000.0}", this.InstrumentState.DistanceToBeaconNauticalMiles);
            string hundredsDigit = distanceToBeaconString.Substring(0, 1);
            string tensDigit = distanceToBeaconString.Substring(1, 1);
            string onesDigit = distanceToBeaconString.Substring(2, 1);
            string tenthsDigit= distanceToBeaconString.Substring(4, 1);

            float digitWidth=22;
            float digitHeight=32;
            float digitSeparationPixels = -4;
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            RectangleF hundredsRect = new RectangleF(12, 8, digitWidth, digitHeight);
            RectangleF tensRect = new RectangleF(hundredsRect.X + digitWidth + digitSeparationPixels, hundredsRect.Y, digitWidth, digitHeight);
            RectangleF onesRect = new RectangleF(tensRect.X + digitWidth + digitSeparationPixels, tensRect.Y, digitWidth, digitHeight);
            RectangleF tenthsRect = new RectangleF(onesRect.X + digitWidth + 4, onesRect.Y, digitWidth, digitHeight);

            g.DrawString(hundredsDigit, digitsFont, Brushes.White, hundredsRect, distanceDigitStringFormat);
            g.DrawString(tensDigit, digitsFont, Brushes.White, tensRect, distanceDigitStringFormat);
            g.DrawString(onesDigit, digitsFont, Brushes.White, onesRect, distanceDigitStringFormat);

            g.FillRectangle(Brushes.White, tenthsRect);
            g.DrawString(tenthsDigit, digitsFont, Brushes.Black, tenthsRect, distanceDigitStringFormat);

            if (this.InstrumentState.DmeInvalidFlag)
            {
                PointF dmeInvalidFlagUpperLeft = new PointF(hundredsRect.X, hundredsRect.Y + 8);
                SizeF dmeInvalidFlagSize = new SizeF((tenthsRect.X + tenthsRect.Width) - hundredsRect.X, 16);
                RectangleF dmeInvalidFlagRect = new RectangleF(dmeInvalidFlagUpperLeft, dmeInvalidFlagSize);
                Color redFlagColor = Color.FromArgb(224, 43, 48);
                Brush redFlagBrush = new SolidBrush(redFlagColor);
                g.FillRectangle(redFlagBrush, dmeInvalidFlagRect);
            }

            RectangleF nmRect = new RectangleF(hundredsRect.X, 45, 30, 20);
            g.DrawString("NM", nmFont, Brushes.White, nmRect, nmStringFormat);

            GraphicsUtil.RestoreGraphicsState(g, ref initialState);
        }
        private void DrawCompassRose(Graphics g, RectangleF outerBounds)
        {
            GraphicsState initialState = g.Save();
            g.InterpolationMode = this.Options.GDIPlusOptions.InterpolationMode;
            g.PixelOffsetMode = this.Options.GDIPlusOptions.PixelOffsetMode;
            g.SmoothingMode = this.Options.GDIPlusOptions.SmoothingMode;
            g.TextRenderingHint = this.Options.GDIPlusOptions.TextRenderingHint;
            GraphicsState basicState = g.Save();

            StringFormat majorHeadingDigitStringFormat = new StringFormat();
            majorHeadingDigitStringFormat.Alignment = StringAlignment.Center;
            majorHeadingDigitStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
            majorHeadingDigitStringFormat.LineAlignment = StringAlignment.Center;
            majorHeadingDigitStringFormat.Trimming = StringTrimming.None;

            FontFamily fontFamily = _fonts.Families[1];

            Font majorHeadingDigitFont = new Font(fontFamily, 27.5f, FontStyle.Bold, GraphicsUnit.Point);

            Pen linePen = new Pen(Color.White);
            linePen.Width = 3;
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            float majorHeadingLineLength=28;
            float minorHeadingLineLength=majorHeadingLineLength/2.0f;
            float majorHeadingLegendLayoutRectangleHeight = 30;
            float majorHeadingLegendLayoutRectangleWidth = 30;
            Brush majorHeadingBrush = new SolidBrush(Color.White);

            RectangleF innerBounds = new RectangleF(outerBounds.X, outerBounds.Y, outerBounds.Width, outerBounds.Height);
            float marginWidth = 30f;
            innerBounds.Inflate(-marginWidth, -marginWidth);

            for (int i = 0; i < 360;i+=45)
            {
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(marginWidth, marginWidth);
                g.TranslateTransform(innerBounds.Width / 2.0f, innerBounds.Height / 2.0f);
                g.RotateTransform(i);
                g.TranslateTransform(-innerBounds.Width / 2.0f, -innerBounds.Height / 2.0f);

                float separationPixels = 2;
                //draw 45-degree outer ticks
                if (i % 90 == 0)
                {
                    g.DrawLine(linePen, new PointF(innerBounds.Width / 2.0f, -separationPixels), new PointF(innerBounds.Width / 2.0f, -((minorHeadingLineLength * 1.5f) + separationPixels)));
                }
                else
                {
                    g.DrawLine(linePen, new PointF(innerBounds.Width / 2.0f, -separationPixels), new PointF(innerBounds.Width / 2.0f, -((majorHeadingLineLength) + separationPixels)));
                }
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            }


            for (int i = 0; i < 360; i++)
            {
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                g.TranslateTransform(outerBounds.Width / 2.0f, outerBounds.Height / 2.0f);
                g.RotateTransform(-this.InstrumentState.MagneticHeadingDegrees);
                g.TranslateTransform(-outerBounds.Width / 2.0f, -outerBounds.Height / 2.0f);

                g.TranslateTransform(marginWidth, marginWidth);
                g.TranslateTransform(innerBounds.Width / 2.0f, innerBounds.Height / 2.0f);
                g.RotateTransform(i);
                g.TranslateTransform (-innerBounds.Width/2.0f, -innerBounds.Height/2.0f);
                if (i % 10 == 0)
                {
                    g.DrawLine(linePen, new PointF(innerBounds.Width / 2.0f, 0), new PointF(innerBounds.Width / 2.0f, majorHeadingLineLength));
                }
                else if (i % 5 == 0)
                {
                    g.DrawLine(linePen, new PointF(innerBounds.Width / 2.0f, 0), new PointF(innerBounds.Width / 2.0f, minorHeadingLineLength));
                }
                if (i % 30 == 0)
                {
                    string majorHeadingLegendText = string.Format("{0:##}", i/10);
                    if (i == 90)
                    {
                        majorHeadingLegendText = "E";
                    }
                    else if (i == 180)
                    {
                        majorHeadingLegendText = "S";
                    }
                    else if (i == 270)
                    {
                        majorHeadingLegendText = "W";
                    }
                    else if (i == 0)
                    {
                        majorHeadingLegendText = "N";
                    }

                    RectangleF majorHeadingLegendLayoutRectangle = new RectangleF(((innerBounds.Width/2.0f)-majorHeadingLegendLayoutRectangleWidth/2.0f), (majorHeadingLegendLayoutRectangleHeight/2.0f), majorHeadingLegendLayoutRectangleWidth, majorHeadingLegendLayoutRectangleHeight);
                    majorHeadingLegendLayoutRectangle.Offset(0, 18);
                    g.DrawString(majorHeadingLegendText, majorHeadingDigitFont, majorHeadingBrush, majorHeadingLegendLayoutRectangle, majorHeadingDigitStringFormat);
                }
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            }
            GraphicsUtil.RestoreGraphicsState(g, ref initialState);

        }
        public F16EHSIInstrumentState InstrumentState
        {
            get;
            set;
        }
        public F16EHSIOptions Options
        {
            get;
            set;
        }
        #region Options Class
        public class F16EHSIOptions
        {
            public F16EHSIOptions()
                : base()
            {
            }
            public GDIPlusOptions GDIPlusOptions
            {
                get;
                set;
            }
        }

        #endregion
        #region Instrument State
        [Serializable]
        public class F16EHSIInstrumentState : InstrumentStateBase
        {
            public enum InstrumentModes
            {
                Unknown,
                PlsTacan,
                Tacan,
                Nav,
                PlsNav
            }
            private const float DEFAULT_COURSE_DEVIATION_LIMIT_DEGREES = 5.0F;
            private const float MAX_RANGE = 999.9F;
            private const int MAX_BRIGHTNESS = 255;
            private float _magneticHeadingDegrees = 0;
            private float _bearingToBeaconDegrees = 0;
            private float _courseDeviationDegrees = 0;
            private float _courseDeviationLimitDegrees = DEFAULT_COURSE_DEVIATION_LIMIT_DEGREES;
            private int _desiredHeadingDegrees = 0;
            private int _desiredCourseDegrees = 0;
            private float _distanceToBeaconNauticalMiles = 0;
            private DateTime _whenInstrumentModeLastChanged = DateTime.Now;
            private int _brightness = MAX_BRIGHTNESS;
            private InstrumentModes _instrumentMode = InstrumentModes.Unknown;
            public F16EHSIInstrumentState()
                : base()
            {
                this.MagneticHeadingDegrees = 0.0f;
                this.BearingToBeaconDegrees = 0.0f;
                this.DeviationInvalidFlag = false;
                this.CourseDeviationDegrees = 0.0f;
                this.CourseDeviationLimitDegrees = DEFAULT_COURSE_DEVIATION_LIMIT_DEGREES;
                this.DesiredHeadingDegrees = 0;
                this.DesiredCourseDegrees = 0;
                this.DistanceToBeaconNauticalMiles = 0.0f;
                this.ToFlag = false;
                this.FromFlag = false;
                this.ShowToFromFlag = true;
                this.NoDataFlag = false;
            }
            public bool NoDataFlag
            {
                get;
                set;
            }
            public bool ShowToFromFlag
            {
                get;
                set;
            }
            public bool ToFlag
            {
                get;
                set;
            }
            public bool FromFlag
            {
                get;
                set;
            }
            public float DistanceToBeaconNauticalMiles
            {
                get
                {
                    return _distanceToBeaconNauticalMiles;
                }
                set
                {
                    float distance = value;
                    if (distance < 0) distance = 0;
                    if (distance > MAX_RANGE) distance = MAX_RANGE;
                    if (float.IsNaN(distance) || float.IsNegativeInfinity(distance))
                    {
                        distance = 0;
                    }
                    if (float.IsPositiveInfinity(distance))
                    {
                        distance = MAX_RANGE;
                    }
                    _distanceToBeaconNauticalMiles = distance;
                }
            }
            public bool DmeInvalidFlag
            {
                get;
                set;
            }
            public int DesiredCourseDegrees
            {
                get
                {
                    return _desiredCourseDegrees;
                }
                set
                {
                    int desiredCourse = value;
                    if (desiredCourse > 360) desiredCourse %= 360;
                    _desiredCourseDegrees = desiredCourse;

                }
            }
            public int DesiredHeadingDegrees
            {
                get
                {
                    return _desiredHeadingDegrees;
                }
                set
                {
                    int desiredHeading = value;
                    desiredHeading %= 360;
                    _desiredHeadingDegrees = desiredHeading;
                }
            }
            public float CourseDeviationDegrees
            {
                get
                {
                    return _courseDeviationDegrees;
                }
                set
                {
                    float courseDeviation = value;
                    courseDeviation %= 360.0f;
                    if (float.IsInfinity(courseDeviation) || float.IsNaN(courseDeviation))
                    {
                        courseDeviation = 0;
                    }
                    _courseDeviationDegrees = courseDeviation;
                }
            }
            public float CourseDeviationLimitDegrees
            {
                get
                {
                    return _courseDeviationLimitDegrees;
                }
                set
                {
                    float courseDeviationLimit = value;
                    courseDeviationLimit %= 360.0f;
                    if (float.IsInfinity(courseDeviationLimit) || float.IsNaN(courseDeviationLimit) || courseDeviationLimit == 0)
                    {
                        courseDeviationLimit = DEFAULT_COURSE_DEVIATION_LIMIT_DEGREES;
                    }
                    _courseDeviationLimitDegrees = courseDeviationLimit;
                }
            }
            public float MagneticHeadingDegrees
            {
                get
                {
                    return _magneticHeadingDegrees;
                }
                set
                {
                    float heading = value;
                    heading %= 360.0f;
                    if (float.IsNaN(heading) || float.IsInfinity(heading))
                    {
                        heading = 0;
                    }
                    _magneticHeadingDegrees = heading;
                }
            }
            public float BearingToBeaconDegrees
            {
                get
                {
                    return _bearingToBeaconDegrees;
                }
                set
                {
                    float bearingToBeacon = value;
                    bearingToBeacon %= 360.0f;
                    if (float.IsInfinity(bearingToBeacon) || float.IsNaN(bearingToBeacon))
                    {
                        bearingToBeacon = 0;
                    }
                    _bearingToBeaconDegrees = bearingToBeacon;
                }
            }
            public bool DeviationInvalidFlag
            {
                get;
                set;
            }
            public bool INUInvalidFlag
            {
                get;
                set;
            }
            public bool AttitudeFailureFlag
            {
                get;
                set;
            }
            public InstrumentModes InstrumentMode
            {
                get
                {
                    return _instrumentMode;
                }
                set
                {
                    InstrumentModes currentMode = _instrumentMode;
                    if (currentMode != value)
                    {
                        _instrumentMode = value;
                        _whenInstrumentModeLastChanged = DateTime.Now;
                    }
                }
            }
            public DateTime WhenInstrumentModeLastChanged
            {
                get
                {
                    return _whenInstrumentModeLastChanged;
                }
            }
            public int Brightness
            {
                get
                {
                    return _brightness;
                }
                set
                {
                    int brightness = value;
                    if (brightness < 0) brightness = 0;
                    if (brightness > MAX_BRIGHTNESS) brightness = MAX_BRIGHTNESS;
                    _brightness = brightness;
                }
            }
            public int MaxBrightness
            {
                get { return MAX_BRIGHTNESS;}
            }
            public bool ShowBrightnessLabel
            {
                get;
                set;
            }
        }
        #endregion

        ~F16EHSI()
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
//                    Common.Util.DisposeObject(_noData);
                }
                _disposed = true;
            }

        }


    }
}
