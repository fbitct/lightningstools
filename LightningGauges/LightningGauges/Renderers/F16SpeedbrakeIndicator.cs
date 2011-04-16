using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public class F16SpeedbrakeIndicator : InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants

        private const string SB_BACKGROUND_IMAGE_FILENAME = "speedbrake.bmp";
        private const string SB_BACKGROUND_MASK_FILENAME = "speedbrake_mask.bmp";
        private const string SB_CLOSED_IMAGE_FILENAME = "sbclosed2.bmp";
        private const string SB_POWER_OFF_IMAGE_FILENAME = "sbclosed.bmp";
        private const string SB_OPEN_IMAGE_FILENAME = "sbopen.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static readonly object _imagesLock = new object();
        private static ImageMaskPair _background;
        private static Bitmap _closed;
        private static Bitmap _powerLoss;
        private static Bitmap _open;
        private static bool _imagesLoaded;
        private bool _disposed;

        #endregion

        public F16SpeedbrakeIndicator()
        {
            InstrumentState = new F16SpeedbrakeIndicatorInstrumentState();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            if (_background == null)
            {
                _background = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + SB_BACKGROUND_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + SB_BACKGROUND_MASK_FILENAME
                    );
            }
            if (_closed == null)
            {
                _closed =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + SB_CLOSED_IMAGE_FILENAME);
            }
            if (_powerLoss == null)
            {
                _powerLoss =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                            SB_POWER_OFF_IMAGE_FILENAME);
            }
            if (_open == null)
            {
                _open =
                    (Bitmap)
                    Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + SB_OPEN_IMAGE_FILENAME);
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16SpeedbrakeIndicatorInstrumentState InstrumentState { get; set; }

        #region Instrument State

        [Serializable]
        public class F16SpeedbrakeIndicatorInstrumentState : InstrumentStateBase
        {
            private float _percentOpen;

            public F16SpeedbrakeIndicatorInstrumentState()
            {
                PercentOpen = 0;
                PowerLoss = false;
            }

            public float PercentOpen
            {
                get { return _percentOpen; }
                set
                {
                    float pct = value;
                    if (pct < 0) pct = 0;
                    if (pct > 100) pct = 100;
                    _percentOpen = pct;
                }
            }

            public bool PowerLoss { get; set; }
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
                GraphicsState initialState = g.Save();

                //set up the canvas scale and clipping region
                int width = _background.MaskedImage.Width - 110;
                int height = _background.MaskedImage.Height - 110 - 4;
                width -= 59;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                    //set the initial scale transformation 
                g.TranslateTransform(-55, -55);
                g.TranslateTransform(-29, 0);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                GraphicsState basicState = g.Save();

                float percentOpen = InstrumentState.PercentOpen;
                if (!InstrumentState.PowerLoss)
                {
                    if (percentOpen < 2.0f)
                    {
                        GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                        g.DrawImage(_closed,
                                    new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height),
                                    new Rectangle(0, 0, _closed.Width, _closed.Height), GraphicsUnit.Pixel);
                        GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    }
                    else
                    {
                        GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                        g.DrawImage(_open,
                                    new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height),
                                    new Rectangle(0, 0, _open.Width, _open.Height), GraphicsUnit.Pixel);
                        GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    }
                }
                else
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.DrawImage(_powerLoss,
                                new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height),
                                new Rectangle(0, 0, _powerLoss.Width, _powerLoss.Height), GraphicsUnit.Pixel);
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background.MaskedImage,
                            new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height),
                            new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height),
                            GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);


                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }

        ~F16SpeedbrakeIndicator()
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
                    //Common.Util.DisposeObject(_closed);
                    //Common.Util.DisposeObject(_powerLoss);
                    //Common.Util.DisposeObject(_open);              
                }
                _disposed = true;
            }
        }
    }
}