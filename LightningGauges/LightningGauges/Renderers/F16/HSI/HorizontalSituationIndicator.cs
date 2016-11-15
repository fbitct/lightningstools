﻿using Common.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;
using System;

namespace LightningGauges.Renderers.F16.HSI
{
    public interface IHorizontalSituationIndicator : IInstrumentRenderer
    {
        InstrumentState InstrumentState { get; set; }
    }

    public class HorizontalSituationIndicator : InstrumentRendererBase, IHorizontalSituationIndicator
    {
        #region Image Location Constants

        private const string HSI_BACKGROUND_IMAGE_FILENAME = "hsi.bmp";
        private const string HSI_BACKGROUND_MASK_FILENAME = "hsi_mask.bmp";
        private const string HSI_BEARING_TO_BEACON_NEEDLE_IMAGE_FILENAME = "hsibeac.bmp";
        private const string HSI_BEARING_TO_BEACON_NEEDLE_MASK_FILENAME = "hsibeac_mask.bmp";
        private const string HSI_CDI_FLAG_IMAGE_FILENAME = "hsicdflag.bmp";
        private const string HSI_CDI_FLAG_MASK_FILENAME = "hsicdflag_mask.bmp";
        private const string HSI_COMPASS_ROSE_IMAGE_FILENAME = "hsicomp.bmp";
        private const string HSI_COMPASS_ROSE_MASK_FILENAME = "hsicomp_mask.bmp";
        private const string HSI_COURSE_DEVIATION_INDICATOR_IMAGE_FILENAME = "hsicorsdev.bmp";
        private const string HSI_COURSE_DEVIATION_INDICATOR_MASK_FILENAME = "hsicorsdev_mask.bmp";
        private const string HSI_HEADING_BUG_IMAGE_FILENAME = "hsiheadref.bmp";
        private const string HSI_HEADING_BUG_MASK_FILENAME = "hsiheadref_mask.bmp";
        private const string HSI_INNER_WHEEL_IMAGE_FILENAME = "hsiinner.bmp";
        private const string HSI_INNER_WHEEL_MASK_FILENAME = "hsiinner_mask.bmp";
        private const string HSI_AIRPLANE_SYMBOL_IMAGE_FILENAME = "hsiplane.bmp";
        private const string HSI_AIRPLANE_SYMBOL_MASK_FILENAME = "hsiplane_mask.bmp";
        private const string HSI_RANGE_FLAG_IMAGE_FILENAME = "hsirangeflag.bmp";
        private const string HSI_RANGE_FLAG_MASK_FILENAME = "hsirangeflag_mask.bmp";
        private const string HSI_TO_FLAG_IMAGE_FILENAME = "hsitotrue.bmp";
        private const string HSI_TO_FLAG_MASK_FILENAME = "hsitotrue_mask.bmp";
        private const string HSI_FROM_FLAG_IMAGE_FILENAME = "hsitofalse.bmp";
        private const string HSI_FROM_FLAG_MASK_FILENAME = "hsitofalse_mask.bmp";
        private const string HSI_OFF_FLAG_IMAGE_FILENAME = "adioff.bmp";
        private const string HSI_OFF_FLAG_MASK_FILENAME = "adiflags_mask.bmp";

        private const string HSI_RANGE_FONT_FILENAME = "font1.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static readonly object _imagesLock = new object();
        private static ImageMaskPair _hsiBackground;
        private static ImageMaskPair _hsiBearingToBeaconNeedle;
        private static ImageMaskPair _hsiCDIFlag;
        private static ImageMaskPair _compassRose;
        private static ImageMaskPair _hsiCourseDeviationIndicator;
        private static ImageMaskPair _hsiHeadingBug;
        private static ImageMaskPair _hsiInnerWheel;
        private static ImageMaskPair _airplaneSymbol;
        private static ImageMaskPair _hsiRangeFlag;
        private static ImageMaskPair _toFlag;
        private static ImageMaskPair _fromFlag;
        private static ImageMaskPair _hsiOffFlag;

        private static FontGraphic _rangeFont;
        private static bool _imagesLoaded;

        #endregion

        public HorizontalSituationIndicator()
        {
            InstrumentState = new InstrumentState();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            if (_hsiBackground == null)
            {
                _hsiBackground = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_BACKGROUND_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_BACKGROUND_MASK_FILENAME
                    );
            }
            if (_hsiBearingToBeaconNeedle == null)
            {
                _hsiBearingToBeaconNeedle = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_BEARING_TO_BEACON_NEEDLE_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_BEARING_TO_BEACON_NEEDLE_MASK_FILENAME
                    );
                _hsiBearingToBeaconNeedle.Use1BitAlpha = true;
            }
            if (_hsiCDIFlag == null)
            {
                _hsiCDIFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_CDI_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_CDI_FLAG_MASK_FILENAME
                    );
                _hsiCDIFlag.Use1BitAlpha = true;
            }
            if (_compassRose == null)
            {
                _compassRose = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_COMPASS_ROSE_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_COMPASS_ROSE_MASK_FILENAME
                    );
            }
            if (_hsiCourseDeviationIndicator == null)
            {
                _hsiCourseDeviationIndicator = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_COURSE_DEVIATION_INDICATOR_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_COURSE_DEVIATION_INDICATOR_MASK_FILENAME
                    );
                _hsiCourseDeviationIndicator.Use1BitAlpha = true;
            }
            if (_hsiHeadingBug == null)
            {
                _hsiHeadingBug = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_HEADING_BUG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_HEADING_BUG_MASK_FILENAME
                    );
                _hsiHeadingBug.Use1BitAlpha = true;
            }
            if (_hsiInnerWheel == null)
            {
                _hsiInnerWheel = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_INNER_WHEEL_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_INNER_WHEEL_MASK_FILENAME
                    );
            }
            if (_airplaneSymbol == null)
            {
                _airplaneSymbol = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_AIRPLANE_SYMBOL_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_AIRPLANE_SYMBOL_MASK_FILENAME
                    );
                _airplaneSymbol.Use1BitAlpha = true;
            }
            if (_hsiRangeFlag == null)
            {
                _hsiRangeFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_RANGE_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_RANGE_FLAG_MASK_FILENAME
                    );
                _hsiRangeFlag.Use1BitAlpha = true;
            }
            if (_toFlag == null)
            {
                _toFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_TO_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_TO_FLAG_MASK_FILENAME
                    );
                _toFlag.Use1BitAlpha = true;
            }
            if (_fromFlag == null)
            {
                _fromFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_FROM_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_FROM_FLAG_MASK_FILENAME
                    );
                _fromFlag.Use1BitAlpha = true;
            }
            if (_hsiOffFlag == null)
            {
                _hsiOffFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_OFF_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_OFF_FLAG_MASK_FILENAME
                    );
            }

            if (_rangeFont == null)
            {
                _rangeFont = new FontGraphic(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_RANGE_FONT_FILENAME);
            }
            _imagesLoaded = true;
        }

        #endregion

        public InstrumentState InstrumentState { get; set; }

        #region Instrument State

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
                var width = _hsiBackground.Image.Width - 47;
                var height = _hsiBackground.Image.Height - 49;

                destinationGraphics.ResetTransform(); //clear any existing transforms
                destinationGraphics.SetClip(destinationRectangle); //set the clipping region on the graphics object to our render rectangle's boundaries
                destinationGraphics.FillRectangle(Brushes.Black, destinationRectangle);
                destinationGraphics.ScaleTransform(destinationRectangle.Width/(float) width, destinationRectangle.Height/(float) height);

                //set the initial scale transformation 
                destinationGraphics.TranslateTransform(-24, -14);

                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = destinationGraphics.Save();
                const float centerX = 128;
                const float centerY = 128;

                BackgroundRenderer.DrawBackground(destinationGraphics, ref basicState, _hsiBackground.MaskedImage);
                CompassRoseRenderer.DrawCompassRose(destinationGraphics, ref basicState, centerX, centerY, InstrumentState, _compassRose.MaskedImage);
                InnerWheelRenderer.DrawInnerWheel(destinationGraphics, ref basicState, centerX, centerY, InstrumentState, _hsiInnerWheel.MaskedImage);
                HeadingBugRenderer.DrawHeadingBug(destinationGraphics, ref basicState, centerX, centerY, InstrumentState, _hsiHeadingBug.MaskedImage );
                BearingToBeaconIndicatorRenderer.DrawBearingToBeaconIndicator(destinationGraphics, ref basicState, centerX, centerY, InstrumentState, _hsiBearingToBeaconNeedle.MaskedImage);
                CourseDeviationInvalidFlagRenderer.DrawCourseDeviationInvalidFlag(destinationGraphics, ref basicState, centerX, centerY, InstrumentState, _hsiCDIFlag.MaskedImage);
                RangeToBeaconRenderer.DrawRangeToBeacon(destinationGraphics, ref basicState, InstrumentState, _rangeFont);
                DesiredCourseRenderer.DrawDesiredCourse(destinationGraphics, ref basicState, InstrumentState, _rangeFont);
                ToFlagRenderer.DrawToFlag(destinationGraphics, ref basicState, centerX, centerY, InstrumentState, _toFlag.MaskedImage);
                FromFlagRenderer.DrawFromFlag(destinationGraphics, ref basicState, centerX, centerY, InstrumentState, _fromFlag.MaskedImage);
                RangeFlagRenderer.DrawRangeFlag(destinationGraphics, ref basicState, InstrumentState, _hsiRangeFlag.MaskedImage);
                CourseDeviationIndicatorRenderer.DrawCourseDeviationIndicator(destinationGraphics, ref basicState, centerX, centerY, InstrumentState, _hsiCourseDeviationIndicator.MaskedImage);
                AirplaneSymbolRenderer.DrawAirplaneSymbol(destinationGraphics, ref basicState, _airplaneSymbol.MaskedImage);
                OffFlagRenderer.DrawOffFlag(destinationGraphics, ref basicState, InstrumentState, _hsiOffFlag.MaskedImage);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                destinationGraphics.Restore(initialState);
            }
        }
    }
}