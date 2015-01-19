﻿using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers.F16
{
    public interface INozzlePositionIndicator : IInstrumentRenderer
    {
        NozzlePositionIndicator.NozzlePositionIndicatorOptions Options { get; set; }
        NozzlePositionIndicator.NozzlePositionIndicatorInstrumentState InstrumentState { get; set; }
    }

    public class NozzlePositionIndicator : InstrumentRendererBase, INozzlePositionIndicator
    {
        #region Image Location Constants

        private const string NOZ_BACKGROUND_IMAGE_FILENAME = "noz.bmp";
        private const string NOZ_BACKGROUND2_IMAGE_FILENAME = "noz2.bmp";

        private const string NOZ_NEEDLE_IMAGE_FILENAME = "arrow_rpm.bmp";
        private const string NOZ_NEEDLE_MASK_FILENAME = "arrow_rpmmask.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static Bitmap _background;
        private static Bitmap _background2;
        private static ImageMaskPair _needle;
        private static readonly object _imagesLock = new object();
        private static bool _imagesLoaded;

        #endregion

        public NozzlePositionIndicator()
        {
            InstrumentState = new NozzlePositionIndicatorInstrumentState();
            Options = new NozzlePositionIndicatorOptions();
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
                                                NOZ_BACKGROUND_IMAGE_FILENAME);
                }
                if (_background2 == null)
                {
                    _background2 =
                        (Bitmap)
                        Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                                NOZ_BACKGROUND2_IMAGE_FILENAME);
                }

                if (_needle == null)
                {
                    _needle = ImageMaskPair.CreateFromFiles(
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + NOZ_NEEDLE_IMAGE_FILENAME,
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + NOZ_NEEDLE_MASK_FILENAME
                        );
                    _needle.Use1BitAlpha = true;
                }
            }
            _imagesLoaded = true;
        }

        #endregion

        public NozzlePositionIndicatorOptions Options { get; set; }
        public NozzlePositionIndicatorInstrumentState InstrumentState { get; set; }

        #region Instrument State

        [Serializable]
        public class NozzlePositionIndicatorInstrumentState : InstrumentStateBase
        {
            private float _nozzlePositionPercent;

            public NozzlePositionIndicatorInstrumentState()
            {
                NozzlePositionPercent = 0;
            }

            public float NozzlePositionPercent
            {
                get { return _nozzlePositionPercent; }
                set
                {
                    var pct = value;
                    if (float.IsInfinity(pct) || float.IsNaN(pct)) pct = 0;
                    if (pct < 0) pct = 0;
                    if (pct > 102) pct = 102;
                    _nozzlePositionPercent = pct;
                }
            }
        }

        #endregion

        #region Options Class

        public class NozzlePositionIndicatorOptions
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
                    destinationGraphics.DrawImage(_background2, new Rectangle(0, 0, _background2.Width, _background2.Height),
                                new Rectangle(0, 0, _background2.Width, _background2.Height), GraphicsUnit.Pixel);
                }
                else
                {
                    destinationGraphics.DrawImage(_background, new Rectangle(0, 0, _background.Width, _background.Height),
                                new Rectangle(0, 0, _background.Width, _background.Height), GraphicsUnit.Pixel);
                }
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                //draw the needle 
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                var angle = 55.0f + (2.5f*InstrumentState.NozzlePositionPercent);

                destinationGraphics.TranslateTransform(_background.Width/2.0f, _background.Width/2.0f);
                destinationGraphics.RotateTransform(angle);
                destinationGraphics.TranslateTransform(-(float) _background.Width/2.0f, -(float) _background.Width/2.0f);
                destinationGraphics.DrawImage(_needle.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                destinationGraphics.Restore(initialState);
            }
        }

    }
}