﻿using System;
using Common.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers.F16
{
    public interface ICompass : IInstrumentRenderer
    {
        Compass.CompassInstrumentState InstrumentState { get; set; }
    }

    public class Compass : InstrumentRendererBase, ICompass
    {
        #region Image Location Constants

        private const string COMPASS_BACKGROUND_IMAGE_FILENAME = "compass.bmp";
        private const string COMPASS_BACKGROUND_MASK_FILENAME = "compass_mask.bmp";
        private const string COMPASS_TAPE_IMAGE_FILENAME = "compasstape.bmp";
        private const string COMPASS_NEEDLE_IMAGE_FILENAME = "compneedle.bmp";
        private const string COMPASS_NEEDLE_MASK_FILENAME = "compneedle_mask.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static readonly object _imagesLock = new object();
        private static ImageMaskPair _background;
        private static Bitmap _tape;
        private static ImageMaskPair _needle;
        private static bool _imagesLoaded;

        #endregion

        public Compass()
        {
            InstrumentState = new CompassInstrumentState();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + COMPASS_BACKGROUND_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + COMPASS_BACKGROUND_MASK_FILENAME
                    );
                _background.Use1BitAlpha = true;
            }
            if (_needle == null)
            {
                _needle = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + COMPASS_NEEDLE_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + COMPASS_NEEDLE_MASK_FILENAME
                    );
            }
            if (_tape == null)
            {
                _tape =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                            COMPASS_TAPE_IMAGE_FILENAME);
            }
            _imagesLoaded = true;
        }

        #endregion

        public CompassInstrumentState InstrumentState { get; set; }

        #region Instrument State

        [Serializable]
        public class CompassInstrumentState : InstrumentStateBase
        {
            private float _magneticHeadingDegrees;

            public float MagneticHeadingDegrees
            {
                get { return _magneticHeadingDegrees; }
                set
                {
                    if (float.IsNaN(value) || float.IsInfinity(value)) value = 0;
                    var heading = value;
                    heading %= 360.0f;
                    _magneticHeadingDegrees = heading;
                }
            }
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
                var width = 205;
                var height = 211;
                destinationGraphics.ResetTransform(); //clear any existing transforms
                destinationGraphics.SetClip(destinationRectangle); //set the clipping region on the graphics object to our render rectangle's boundaries
                destinationGraphics.FillRectangle(Brushes.Black, destinationRectangle);
                destinationGraphics.ScaleTransform(destinationRectangle.Width/(float) width, destinationRectangle.Height/(float) height);
                //set the initial scale transformation 
                destinationGraphics.TranslateTransform(-25, -23);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = destinationGraphics.Save();
                //draw the tape 
                var pixelsPerDegree = 2.802f;
                var heading = InstrumentState.MagneticHeadingDegrees;
                var offset = (pixelsPerDegree*heading);
                float translateX = -1327;
                float translateY = 90;
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                destinationGraphics.TranslateTransform(translateX, translateY);
                destinationGraphics.TranslateTransform(offset, 0);
                destinationGraphics.ScaleTransform(0.80f, 0.80f);
                destinationGraphics.DrawImageFast(_tape, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                //draw the needle 
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                destinationGraphics.DrawImageFast(_needle.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);


                //draw the background image
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                destinationGraphics.DrawImageFast(_background.MaskedImage,
                            new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height),
                            new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height),
                            GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                destinationGraphics.Restore(initialState);
            }
        }

    }
}