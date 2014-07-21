﻿using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public interface IF16RollTrimIndicator : IInstrumentRenderer
    {
        F16RollTrimIndicator.F16RollTrimIndicatorInstrumentState InstrumentState { get; set; }
    }

    public class F16RollTrimIndicator : InstrumentRendererBase, IF16RollTrimIndicator
    {
        #region Image Location Constants

        private const string ROLLTRIM_BACKGROUND_IMAGE_FILENAME = "rolltrim.bmp";
        private const string ROLLTRIM_NEEDLE_IMAGE_FILENAME = "rolltrimneed.bmp";
        private const string ROLLTRIM_NEEDLE_MASK_FILENAME = "rolltrimneed_mask.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static readonly object _imagesLock = new object();
        private static Bitmap _background;
        private static Bitmap _needle;
        private static bool _imagesLoaded;

        #endregion

        public F16RollTrimIndicator()
        {
            InstrumentState = new F16RollTrimIndicatorInstrumentState();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                            ROLLTRIM_BACKGROUND_IMAGE_FILENAME);
            }
            if (_needle == null)
            {
                using (var needleWithMask = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ROLLTRIM_NEEDLE_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + ROLLTRIM_NEEDLE_MASK_FILENAME
                    ))
                {
                    needleWithMask.Use1BitAlpha = true;
                    _needle = (Bitmap) Util.CropBitmap(needleWithMask.MaskedImage, new Rectangle(136, 93, 61, 61));
                    _needle.MakeTransparent(Color.Black);
                }
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16RollTrimIndicatorInstrumentState InstrumentState { get; set; }

        #region Instrument State

        [Serializable]
        public class F16RollTrimIndicatorInstrumentState : InstrumentStateBase
        {
            private float _rollTrimPercent;

            public F16RollTrimIndicatorInstrumentState()
            {
                RollTrimPercent = 0;
            }

            public float RollTrimPercent
            {
                get { return _rollTrimPercent; }
                set
                {
                    var pct = value;
                    if (float.IsInfinity(pct) || float.IsNaN(pct)) pct = 0;
                    if (Math.Abs(pct) > 100.0f) pct = Math.Sign(pct) * 100.0f;
                    _rollTrimPercent = pct;
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
                var width = 108;
                var height = 108;
                destinationGraphics.ResetTransform(); //clear any existing transforms
                destinationGraphics.SetClip(destinationRectangle); //set the clipping region on the graphics object to our render rectangle's boundaries
                destinationGraphics.FillRectangle(Brushes.Black, destinationRectangle);
                destinationGraphics.ScaleTransform(destinationRectangle.Width/(float) width, destinationRectangle.Height/(float) height);
                //set the initial scale transformation 
                destinationGraphics.TranslateTransform(-64, -70);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = destinationGraphics.Save();

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                destinationGraphics.DrawImage(_background, new Rectangle(0, 0, _background.Width, _background.Height),
                            new Rectangle(0, 0, _background.Width, _background.Height), GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                //draw the needle
                var rt = -InstrumentState.RollTrimPercent;
                var angle = (rt/100.0f)*90;

                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                destinationGraphics.TranslateTransform(128, 93);
                destinationGraphics.TranslateTransform(8, 0);
                destinationGraphics.TranslateTransform(_needle.Width/2.0f, _needle.Height/2.0f);
                destinationGraphics.RotateTransform(angle);
                destinationGraphics.TranslateTransform(-_needle.Width/2.0f, -_needle.Height/2.0f);
                destinationGraphics.DrawImage(_needle, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                destinationGraphics.Restore(initialState);
            }
        }
   }
}