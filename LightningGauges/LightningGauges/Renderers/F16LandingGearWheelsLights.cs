using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public class F16LandingGearWheelsLights : InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants

        private const string GEAR_BACKGROUND_IMAGE_FILENAME = "gear.bmp";
        private const string GEAR_BACKGROUND_MASK_FILENAME = "gear_mask.bmp";

        private const string GEAR_LEFT_GEAR_LIGHT_IMAGE_FILENAME = "leftgr.bmp";
        private const string GEAR_LEFT_GEAR_LIGHT_MASK_FILENAME = "leftgr_mask.bmp";
        private const string GEAR_RIGHT_GEAR_LIGHT_IMAGE_FILENAME = "rightgr.bmp";
        private const string GEAR_RIGHT_GEAR_LIGHT_MASK_FILENAME = "rightgr_mask.bmp";
        private const string GEAR_NOSE_GEAR_LIGHT_IMAGE_FILENAME = "nosegr.bmp";
        private const string GEAR_NOSE_GEAR_LIGHT_MASK_FILENAME = "nosegr_mask.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static readonly object _imagesLock = new object();
        private static ImageMaskPair _background;
        private static ImageMaskPair _noseGearLight;
        private static ImageMaskPair _leftGearLight;
        private static ImageMaskPair _rightGearLight;
        private static bool _imagesLoaded;
        private bool _disposed;

        #endregion

        public F16LandingGearWheelsLights()
        {
            InstrumentState = new F16LandingGearWheelsLightsInstrumentState();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + GEAR_BACKGROUND_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + GEAR_BACKGROUND_MASK_FILENAME
                    );
                _background.Use1BitAlpha = true;
            }
            if (_leftGearLight == null)
            {
                _leftGearLight = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + GEAR_LEFT_GEAR_LIGHT_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + GEAR_LEFT_GEAR_LIGHT_MASK_FILENAME
                    );
            }

            if (_rightGearLight == null)
            {
                _rightGearLight = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + GEAR_RIGHT_GEAR_LIGHT_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + GEAR_RIGHT_GEAR_LIGHT_MASK_FILENAME
                    );
            }

            if (_noseGearLight == null)
            {
                _noseGearLight = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + GEAR_NOSE_GEAR_LIGHT_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + GEAR_NOSE_GEAR_LIGHT_MASK_FILENAME
                    );
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16LandingGearWheelsLightsInstrumentState InstrumentState { get; set; }

        #region Instrument State

        [Serializable]
        public class F16LandingGearWheelsLightsInstrumentState : InstrumentStateBase
        {
            public bool LeftGearDown { get; set; }
            public bool RightGearDown { get; set; }
            public bool NoseGearDown { get; set; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
                var width = 142;
                var height = 168;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                //set the initial scale transformation 
                g.TranslateTransform(-56, -29);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = g.Save();

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the left gear light
                if (InstrumentState.LeftGearDown)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.DrawImage(_leftGearLight.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the right gear light
                if (InstrumentState.RightGearDown)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.DrawImage(_rightGearLight.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the nose gear light
                if (InstrumentState.NoseGearDown)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.DrawImage(_noseGearLight.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }

        ~F16LandingGearWheelsLights()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //Common.Util.DisposeObject(_background);
                    //Common.Util.DisposeObject(_noseGearLight);
                    //Common.Util.DisposeObject(_leftGearLight);
                    //Common.Util.DisposeObject(_rightGearLight);
                }
                _disposed = true;
            }
        }
    }
}