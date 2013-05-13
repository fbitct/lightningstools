using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public interface IF16NosewheelSteeringIndexer : IInstrumentRenderer
    {
        F16NosewheelSteeringIndexer.F16NosewheelSteeringIndexerInstrumentState InstrumentState { get; set; }
    }

    public class F16NosewheelSteeringIndexer : InstrumentRendererBase, IF16NosewheelSteeringIndexer
    {
        #region Image Location Constants

        private const string NWSI_BACKGROUND_IMAGE_FILENAME = "index2.bmp";
        private const string NWSI_BACKGROUND_MASK_FILENAME = "index2_mask.bmp";
        private const string NWSI_DISC_IMAGE_FILENAME = "ind2disc.bmp";
        private const string NWSI_DISC_MASK_FILENAME = "ind2disc_mask.bmp";
        private const string NWSI_NWS_IMAGE_FILENAME = "ind2nws.bmp";
        private const string NWSI_NWS_MASK_FILENAME = "ind2nws_mask.bmp";
        private const string NWSI_RDY_IMAGE_FILENAME = "ind2ready.bmp";
        private const string NWSI_RDY_MASK_FILENAME = "ind2ready_mask.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static readonly object _imagesLock = new object();
        private static ImageMaskPair _background;
        private static ImageMaskPair _disc;
        private static ImageMaskPair _nws;
        private static ImageMaskPair _rdy;
        private static bool _imagesLoaded;

        #endregion

        public F16NosewheelSteeringIndexer()
        {
            InstrumentState = new F16NosewheelSteeringIndexerInstrumentState();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + NWSI_BACKGROUND_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + NWSI_BACKGROUND_MASK_FILENAME
                    );
                _background.Use1BitAlpha = true;
            }
            if (_disc == null)
            {
                _disc = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + NWSI_DISC_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + NWSI_DISC_MASK_FILENAME
                    );
            }
            if (_nws == null)
            {
                _nws = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + NWSI_NWS_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + NWSI_NWS_MASK_FILENAME
                    );
            }
            if (_rdy == null)
            {
                _rdy = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + NWSI_RDY_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + NWSI_RDY_MASK_FILENAME
                    );
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16NosewheelSteeringIndexerInstrumentState InstrumentState { get; set; }

        #region Instrument State

        [Serializable]
        public class F16NosewheelSteeringIndexerInstrumentState : InstrumentStateBase
        {
            public bool AR_NWS { get; set; }
            public bool RDY { get; set; }
            public bool DISC { get; set; }
        }

        #endregion

        public override void Render(Graphics g, Rectangle bounds)
        {
            if (!_imagesLoaded)
            {
                LoadImageResources();
            }
            lock (_imagesLock)
            {
                //store the canvas's transform and clip settings so we can restore them later
                var initialState = g.Save();

                //set up the canvas scale and clipping region
                var width = _background.Image.Width - (92);
                var height = _background.Image.Height - (99);
                width -= 117;
                height -= 2;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                //set the initial scale transformation 

                g.TranslateTransform(-46, -46);
                g.TranslateTransform(-50, -2);

                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = g.Save();

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                if (InstrumentState.DISC)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.DrawImage(_disc.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }
                if (InstrumentState.RDY)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.DrawImage(_rdy.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }
                if (InstrumentState.AR_NWS)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.DrawImage(_nws.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }
   }
}