﻿using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers.F16
{
    public interface ITachometer : IInstrumentRenderer
    {
        Tachometer.TachometerInstrumentState InstrumentState { get; set; }
        Tachometer.TachometerOptions Options { get; set; }
    }

    public class Tachometer : InstrumentRendererBase, ITachometer
    {
        #region Image Location Constants

        private const string RPM_BACKGROUND_IMAGE_FILENAME = "rpm.bmp";
        private const string RPM_BACKGROUND2_IMAGE_FILENAME = "rpm2.bmp";

        private const string RPM_NEEDLE_IMAGE_FILENAME = "arrow_rpm.bmp";
        private const string RPM_NEEDLE_MASK_FILENAME = "arrow_rpmmask.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static Bitmap _background;
        private static Bitmap _background2;
        private static ImageMaskPair _needle;
        private static readonly object _imagesLock = new object();
        private static bool _imagesLoaded;

        #endregion

        public Tachometer()
        {
            InstrumentState = new TachometerInstrumentState();
            Options = new TachometerOptions();
            LoadImageResources();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            lock (_imagesLock)
            {
                if (_background == null)
                {
                    _background =
                        (Bitmap)
                        Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                                RPM_BACKGROUND_IMAGE_FILENAME);
                }
                if (_background2 == null)
                {
                    _background2 =
                        (Bitmap)
                        Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                                RPM_BACKGROUND2_IMAGE_FILENAME);
                }
                if (_needle == null)
                {
                    _needle = ImageMaskPair.CreateFromFiles(
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + RPM_NEEDLE_IMAGE_FILENAME,
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + RPM_NEEDLE_MASK_FILENAME
                        );
                    _needle.Use1BitAlpha = true;
                }
            }
            _imagesLoaded = true;
        }

        #endregion

        public TachometerInstrumentState InstrumentState { get; set; }

        public TachometerOptions Options { get; set; }

        #region Instrument State

        [Serializable]
        public class TachometerInstrumentState : InstrumentStateBase
        {
            private float _rpmPercent;

            public float RPMPercent
            {
                get { return _rpmPercent; }
                set
                {
                    var pct = value;
                    if (float.IsInfinity(pct) || float.IsNaN(pct)) pct = 0;
                    if (pct < 0) pct = 0;
                    if (pct > 110) pct = 110;
                    _rpmPercent = pct;
                }
            }
        }

        #endregion

        #region Options class

        public class TachometerOptions
        {
            public bool IsSecondary { get; set; }
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
                var width = 178;
                var height = 178;
                destinationGraphics.ResetTransform(); //clear any existing transforms
                destinationGraphics.SetClip(destinationRectangle); //set the clipping region on the graphics object to our render rectangle's boundaries
                destinationGraphics.FillRectangle(Brushes.Black, destinationRectangle);
                destinationGraphics.ScaleTransform(destinationRectangle.Width/(float) width, destinationRectangle.Height/(float) height);
                //set the initial scale transformation 
                destinationGraphics.TranslateTransform(-39, -39);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = destinationGraphics.Save();

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                if (Options.IsSecondary)
                {
                    destinationGraphics.DrawImageFast(_background2, new Rectangle(0, 0, _background.Width, _background.Height),
                                new Rectangle(0, 0, _background2.Width, _background2.Height), GraphicsUnit.Pixel);
                }
                else
                {
                    destinationGraphics.DrawImageFast(_background, new Rectangle(0, 0, _background.Width, _background.Height),
                                new Rectangle(0, 0, _background.Width, _background.Height), GraphicsUnit.Pixel);
                }
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                //draw the needle 
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                var angle = GetAngle(InstrumentState.RPMPercent);

                destinationGraphics.TranslateTransform(_background.Width/2.0f, _background.Width/2.0f);
                destinationGraphics.RotateTransform(angle);
                destinationGraphics.TranslateTransform(-(float) _background.Width/2.0f, -(float) _background.Width/2.0f);
                destinationGraphics.DrawImageFast(_needle.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                destinationGraphics.Restore(initialState);
            }
        }

        private float GetAngle(float RPMPercent)
        {
            float angle = 0;
            if (RPMPercent >= 0.0f && RPMPercent <= 65.0f)
            {
                angle = 1.7f*RPMPercent; //1.7 degrees per of space per 1 percent of readout
            }
            else if (RPMPercent >= 65.0f)
            {
                angle = 110.5f + ((RPMPercent - 65.0f)*4.7f); //4.7 degrees of space for 1 percent of readout
            }
            return angle;
        }

    }
}