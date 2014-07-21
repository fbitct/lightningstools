using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public interface IF16VerticalVelocityIndicatorEU : IF16VerticalVelocityIndicator{}

    public class F16VerticalVelocityIndicatorEU : InstrumentRendererBase, IF16VerticalVelocityIndicator
    {
        #region Image Location Constants

        private const string VVI_BACKGROUND_IMAGE_FILENAME = "vvieuro.bmp";
        private const string VVI_OFF_FLAG_IMAGE_FILENAME = "vvieuroflag.bmp";
        private const string VVI_OFF_FLAG_MASK_FILENAME = "vvieuroflag_mask.bmp";
        private const string VVI_NEEDLE_IMAGE_FILENAME = "arrowrpm.bmp";
        private const string VVI_NEEDLE_MASK_FILENAME = "arrowrpmmask.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static readonly object _imagesLock = new object();
        private static Bitmap _background;
        private static ImageMaskPair _offFlag;
        private static ImageMaskPair _needle;
        private static bool _imagesLoaded;

        #endregion

        public F16VerticalVelocityIndicatorEU()
        {
            InstrumentState = new F16VerticalVelocityIndicatorInstrumentState();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                            VVI_BACKGROUND_IMAGE_FILENAME);
            }
            if (_offFlag == null)
            {
                _offFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + VVI_OFF_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + VVI_OFF_FLAG_MASK_FILENAME
                    );
            }
            if (_needle == null)
            {
                _needle = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + VVI_NEEDLE_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + VVI_NEEDLE_MASK_FILENAME
                    );
                _needle.Use1BitAlpha = true;
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16VerticalVelocityIndicatorInstrumentState InstrumentState { get; set; }



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
                var width = _background.Width;
                var height = _background.Height - 29;
                width -= 154;
                destinationGraphics.ResetTransform(); //clear any existing transforms
                destinationGraphics.SetClip(destinationRectangle); //set the clipping region on the graphics object to our render rectangle's boundaries
                destinationGraphics.FillRectangle(Brushes.Black, destinationRectangle);
                destinationGraphics.ScaleTransform(destinationRectangle.Width/(float) width, destinationRectangle.Height/(float) height);
                //set the initial scale transformation 

                destinationGraphics.TranslateTransform(-76, 0);
                destinationGraphics.TranslateTransform(0, -15);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = destinationGraphics.Save();


                //draw the background image
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                destinationGraphics.DrawImage(_background, new Rectangle(0, 0, _background.Width, _background.Height),
                            new Rectangle(0, 0, _background.Width, _background.Height), GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                //draw the OFF flag
                if (InstrumentState.OffFlag)
                {
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                    destinationGraphics.DrawImage(_offFlag.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                }


                //draw the needle
                var centerX = 156.0f;
                var centerY = 130.0f;
                var vv = InstrumentState.VerticalVelocityFeet;
                if (Math.Abs(vv) > 6000.0) vv = Math.Sign(vv)*6000.0f;
                var vviInThousands = vv/1000.0f;
                float angle;
                if (Math.Abs(vviInThousands) > 1.0f)
                {
                    angle = -45.0f + (Math.Sign(vviInThousands)*((Math.Abs(vviInThousands) - 1.0f)*(45.0f/5.0f)));
                    if (vviInThousands < 0) angle -= 90;
                }
                else
                {
                    angle = -90.0f + (vviInThousands*45.0f);
                }
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                destinationGraphics.TranslateTransform(centerX, centerY);
                destinationGraphics.RotateTransform(angle);
                destinationGraphics.TranslateTransform(-centerX, -centerY);
                destinationGraphics.TranslateTransform(28, 0);
                destinationGraphics.DrawImage(_needle.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);


                //restore the canvas's transform and clip settings to what they were when we entered this method
                destinationGraphics.Restore(initialState);
            }
        }

    }
}