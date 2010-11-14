using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Common.SimSupport;
using System.IO;
using System.Drawing.Drawing2D;
using Common.Imaging;

namespace LightningGauges.Renderers
{
    public class F16AngleOfAttackIndexer : InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants
        private static string IMAGES_FOLDER_NAME = new DirectoryInfo (System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)).FullName + Path.DirectorySeparatorChar + "images";
        private const string AOAI_BACKGROUND_IMAGE_FILENAME = "index.bmp";
        private const string AOAI_BACKGROUND_MASK_FILENAME = "index_mask.bmp";
        private const string AOAI_AOA_LOW_IMAGE_FILENAME = "aoadn.bmp";
        private const string AOAI_AOA_LOW_MASK_FILENAME = "aoadn_mask.bmp";
        private const string AOAI_AOA_ON_IMAGE_FILENAME = "aoaon.bmp";
        private const string AOAI_AOA_ON_RANGE_MASK_FILENAME = "aoaon_mask.bmp";
        private const string AOAI_AOA_HIGH_IMAGE_FILENAME = "aoaup.bmp";
        private const string AOAI_AOA_HIGH_MASK_FILENAME = "aoaup_mask.bmp";

        #endregion

        #region Instance variables
        private static object _imagesLock = new object();
        private static ImageMaskPair _background;
        private static ImageMaskPair _aoaLow;
        private static ImageMaskPair _aoaOn;
        private static ImageMaskPair _aoaHigh;
        private static bool _imagesLoaded = false;
        private bool _disposed = false;
        #endregion

        public F16AngleOfAttackIndexer()
            : base()
        {
            this.InstrumentState = new F16AngleOfAttackIndexerInstrumentState();
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
                int width = _background.Image.Width - (92);
                int height = _background.Image.Height - (101);
                width -= 117;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform((float)bounds.Width / (float)width, (float)bounds.Height / (float)height); //set the initial scale transformation 

                g.TranslateTransform(-46, -50);
                g.TranslateTransform(-55, 0);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                GraphicsState basicState = g.Save();

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                if (this.InstrumentState.AoaAbove)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.DrawImage(_aoaHigh.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }
                if (this.InstrumentState.AoaBelow)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.DrawImage(_aoaLow.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }
                if (this.InstrumentState.AoaOn)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.DrawImage(_aoaOn.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }
        public F16AngleOfAttackIndexerInstrumentState InstrumentState
        {
            get;
            set;
        }
        #region Instrument State
        [Serializable]
        public class F16AngleOfAttackIndexerInstrumentState : InstrumentStateBase
        {
            public F16AngleOfAttackIndexerInstrumentState():base()
            {
            }
            public bool AoaAbove 
            {
                get;
                set;
            }
            public bool AoaOn 
            {
                get;
                set;
            }
            public bool AoaBelow 
            {
                get;
                set;
            }
        }
        #endregion
        ~F16AngleOfAttackIndexer()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //Common.Util.DisposeObject(_background);
                    //Common.Util.DisposeObject(_aoaLow);
                    //Common.Util.DisposeObject(_aoaOn);
                    //Common.Util.DisposeObject(_aoaHigh);
               }
                _disposed = true;
            }

        }
    }
}
