using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public interface IF16AngleOfAttackIndexer:IInstrumentRenderer
    {
        F16AngleOfAttackIndexer.F16AngleOfAttackIndexerInstrumentState InstrumentState { get; set; }
    }

    public class F16AngleOfAttackIndexer : InstrumentRendererBase, IF16AngleOfAttackIndexer
    {
        #region Image Location Constants

        private const string AOAI_BACKGROUND_IMAGE_FILENAME = "index.bmp";
        private const string AOAI_BACKGROUND_MASK_FILENAME = "index_mask.bmp";
        private const string AOAI_AOA_LOW_IMAGE_FILENAME = "aoadn.bmp";
        private const string AOAI_AOA_LOW_MASK_FILENAME = "aoadn_mask.bmp";
        private const string AOAI_AOA_ON_IMAGE_FILENAME = "aoaon.bmp";
        private const string AOAI_AOA_ON_RANGE_MASK_FILENAME = "aoaon_mask.bmp";
        private const string AOAI_AOA_HIGH_IMAGE_FILENAME = "aoaup.bmp";
        private const string AOAI_AOA_HIGH_MASK_FILENAME = "aoaup_mask.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static readonly object _imagesLock = new object();
        private static ImageMaskPair _background;
        private static ImageMaskPair _aoaLow;
        private static ImageMaskPair _aoaOn;
        private static ImageMaskPair _aoaHigh;
        private static bool _imagesLoaded;

        #endregion

        public F16AngleOfAttackIndexer()
        {
            InstrumentState = new F16AngleOfAttackIndexerInstrumentState();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + AOAI_BACKGROUND_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + AOAI_BACKGROUND_MASK_FILENAME
                    );
            }
            if (_aoaLow == null)
            {
                _aoaLow = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + AOAI_AOA_LOW_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + AOAI_AOA_LOW_MASK_FILENAME
                    );
            }
            if (_aoaOn == null)
            {
                _aoaOn = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + AOAI_AOA_ON_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + AOAI_AOA_ON_RANGE_MASK_FILENAME
                    );
            }
            if (_aoaHigh == null)
            {
                _aoaHigh = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + AOAI_AOA_HIGH_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + AOAI_AOA_HIGH_MASK_FILENAME
                    );
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16AngleOfAttackIndexerInstrumentState InstrumentState { get; set; }

        #region Instrument State

        [Serializable]
        public class F16AngleOfAttackIndexerInstrumentState : InstrumentStateBase
        {
            public bool AoaAbove { get; set; }
            public bool AoaOn { get; set; }
            public bool AoaBelow { get; set; }
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
                var width = _background.Image.Width - (92);
                var height = _background.Image.Height - (101);
                width -= 117;
                destinationGraphics.ResetTransform(); //clear any existing transforms
                destinationGraphics.SetClip(destinationRectangle); //set the clipping region on the graphics object to our render rectangle's boundaries
                destinationGraphics.FillRectangle(Brushes.Black, destinationRectangle);
                destinationGraphics.ScaleTransform(destinationRectangle.Width/(float) width, destinationRectangle.Height/(float) height);
                //set the initial scale transformation 

                destinationGraphics.TranslateTransform(-46, -50);
                destinationGraphics.TranslateTransform(-55, 0);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = destinationGraphics.Save();

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                destinationGraphics.DrawImage(_background.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

                if (InstrumentState.AoaAbove)
                {
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                    destinationGraphics.DrawImage(_aoaHigh.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                }
                if (InstrumentState.AoaBelow)
                {
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                    destinationGraphics.DrawImage(_aoaLow.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                }
                if (InstrumentState.AoaOn)
                {
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                    destinationGraphics.DrawImage(_aoaOn.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                }

                //restore the canvas's transform and clip settings to what they were when we entered this method
                destinationGraphics.Restore(initialState);
            }
        }

    }
}