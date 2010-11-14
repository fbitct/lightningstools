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
    public class F16DataEntryDisplayPilotFaultList : InstrumentRendererBase, IDisposable
    {
        #region Image Location Constants
        private static string IMAGES_FOLDER_NAME = new DirectoryInfo (System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)).FullName + Path.DirectorySeparatorChar + "images";
        private const string DED_PFL_BACKGROUND_IMAGE_FILENAME = "ded.bmp";
        private const string DED_PFL_BACKGROUND_MASK_FILENAME = "ded.bmp";
        private const string DED_PFL_FONT_IMAGE_FILENAME = "normal.bmp";
        #endregion

        #region Instance variables
        private static ImageMaskPair _background;
        private static DEDPFLFont _font;
        private static object _imagesLock = new object();
        private static bool _imagesLoaded = false;
        private bool _disposed = false;
        #endregion

        public F16DataEntryDisplayPilotFaultList()
            : base()
        {
            this.InstrumentState = new F16DataEntryDisplayPilotFaultListInstrumentState();
        }
        #region Initialization Code
        private void LoadImageResources()
        {
            lock (_imagesLock)
            {
                if (_background == null)
                {
                    _background = ImageMaskPair.CreateFromFiles(
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + DED_PFL_BACKGROUND_IMAGE_FILENAME,
                        IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + DED_PFL_BACKGROUND_MASK_FILENAME
                        );
                }
                if (_font == null)
                {
                    _font = new DEDPFLFont(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + DED_PFL_FONT_IMAGE_FILENAME);
                }
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
                int width = 239;
                int height = 87;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform((float)bounds.Width / (float)width, (float)bounds.Height / (float)height); //set the initial scale transformation 
                g.TranslateTransform(-8, -84);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                GraphicsState basicState = g.Save();

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background.MaskedImage, new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height), new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height), GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                if (this.InstrumentState.PowerOn)
                {
                    //draw the text
                    using (Bitmap textImage = new Bitmap(400, 80))
                    using (Graphics d = Graphics.FromImage(textImage))
                    {
                        Point location = new Point(0, 0);
                        DrawDEDPFLLineOfText(d, location, this.InstrumentState.Line1, this.InstrumentState.Line1Invert);
                        location.Offset(0, 16);
                        DrawDEDPFLLineOfText(d, location, this.InstrumentState.Line2, this.InstrumentState.Line2Invert);
                        location.Offset(0, 16);
                        DrawDEDPFLLineOfText(d, location, this.InstrumentState.Line3, this.InstrumentState.Line3Invert);
                        location.Offset(0, 16);
                        DrawDEDPFLLineOfText(d, location, this.InstrumentState.Line4, this.InstrumentState.Line4Invert);
                        location.Offset(0, 16);
                        DrawDEDPFLLineOfText(d, location, this.InstrumentState.Line5, this.InstrumentState.Line5Invert);

                        GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                        g.TranslateTransform(15, 91);
                        g.DrawImage(textImage, new Rectangle(0, 0, width - 16, height - 14), new Rectangle(0, 0, textImage.Width, textImage.Height), GraphicsUnit.Pixel);
                        GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    }
                }
                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }

        private void DrawDEDPFLLineOfText(Graphics g, Point location, byte[] line, byte[] invertLine)
        {
            lock (_imagesLock)
            {
                if (line != null)
                {
                    if (line.Length < 25)
                    {
                        byte[] lineReplacement = new byte[25];
                        Array.Copy(line, 0, lineReplacement, 0, line.Length);
                        line = lineReplacement;
                    }
                    for (int i = 0; i < line.Length; i++)
                    {
                        byte thisByte = line[i];
                        if (thisByte == 0x04) thisByte = 0x02;
                        byte thisByteInvert = invertLine.Length > i ? invertLine[i] : (byte)0;
                        bool inverted = false;
                        if (thisByteInvert > 0 && thisByteInvert !=32)
                        {
                            inverted = true;
                        }
                        Bitmap thisCharImage = _font.GetCharImage(thisByte, inverted);

                        float xPos = location.X + (i * thisCharImage.Width);
                        float yPos = location.Y;
                        //g.DrawImage(thisCharImage, new Point((int)xPos, (int)yPos));
                        g.DrawImageUnscaled(thisCharImage, new Point((int)xPos, (int)yPos));
                    }
                }
            }
        }
        public F16DataEntryDisplayPilotFaultListInstrumentState InstrumentState
        {
            get;
            set;
        }
        #region Instrument State
        [Serializable]
        public class F16DataEntryDisplayPilotFaultListInstrumentState : InstrumentStateBase
        {
            public F16DataEntryDisplayPilotFaultListInstrumentState():base()
            {
                this.PowerOn = true;
            }
            public byte[] Line1 { get; set; }
            public byte[] Line2 { get; set; }
            public byte[] Line3 { get; set; }
            public byte[] Line4 { get; set; }
            public byte[] Line5 { get; set; }
            public byte[] Line1Invert { get; set; }
            public byte[] Line2Invert { get; set; }
            public byte[] Line3Invert { get; set; }
            public byte[] Line4Invert { get; set; }
            public byte[] Line5Invert { get; set; }
            public bool PowerOn { get; set; }
        }
        #endregion
        ~F16DataEntryDisplayPilotFaultList()
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
                    //Common.Util.DisposeObject(_font);
                }
                _disposed = true;
            }

        }
    }
}
