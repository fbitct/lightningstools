using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public interface IF16DataEntryDisplayPilotFaultList : IInstrumentRenderer
    {
        F16DataEntryDisplayPilotFaultList.F16DataEntryDisplayPilotFaultListInstrumentState InstrumentState { get; set; }
    }

    public class F16DataEntryDisplayPilotFaultList : InstrumentRendererBase, IF16DataEntryDisplayPilotFaultList
    {
        #region Image Location Constants

        private const string DED_PFL_BACKGROUND_IMAGE_FILENAME = "ded.bmp";
        private const string DED_PFL_BACKGROUND_MASK_FILENAME = "ded.bmp";
        private const string DED_PFL_FONT_IMAGE_FILENAME = "normal.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static ImageMaskPair _background;
        private static DEDPFLFont _font;
        private static readonly object _imagesLock = new object();
        private static bool _imagesLoaded;

        #endregion

        public F16DataEntryDisplayPilotFaultList()
        {
            InstrumentState = new F16DataEntryDisplayPilotFaultListInstrumentState();
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
                    _font =
                        new DEDPFLFont(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + DED_PFL_FONT_IMAGE_FILENAME);
                }
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16DataEntryDisplayPilotFaultListInstrumentState InstrumentState { get; set; }

        #region Instrument State

        [Serializable]
        public class F16DataEntryDisplayPilotFaultListInstrumentState : InstrumentStateBase
        {
            public F16DataEntryDisplayPilotFaultListInstrumentState()
            {
                PowerOn = true;
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
                var width = 239;
                var height = 87;
                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                //set the initial scale transformation 
                g.TranslateTransform(-8, -84);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = g.Save();

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.DrawImage(_background.MaskedImage,
                            new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height),
                            new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height),
                            GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                if (InstrumentState.PowerOn)
                {
                    //draw the text
                    using (var textImage = new Bitmap(400, 80))
                    using (var d = Graphics.FromImage(textImage))
                    {
                        var location = new Point(0, 0);
                        DrawDEDPFLLineOfText(d, location, InstrumentState.Line1, InstrumentState.Line1Invert);
                        location.Offset(0, 16);
                        DrawDEDPFLLineOfText(d, location, InstrumentState.Line2, InstrumentState.Line2Invert);
                        location.Offset(0, 16);
                        DrawDEDPFLLineOfText(d, location, InstrumentState.Line3, InstrumentState.Line3Invert);
                        location.Offset(0, 16);
                        DrawDEDPFLLineOfText(d, location, InstrumentState.Line4, InstrumentState.Line4Invert);
                        location.Offset(0, 16);
                        DrawDEDPFLLineOfText(d, location, InstrumentState.Line5, InstrumentState.Line5Invert);

                        GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                        g.TranslateTransform(15, 91);
                        g.DrawImage(textImage, new Rectangle(0, 0, width - 16, height - 14),
                                    new Rectangle(0, 0, textImage.Width, textImage.Height), GraphicsUnit.Pixel);
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
                        var lineReplacement = new byte[25];
                        Array.Copy(line, 0, lineReplacement, 0, line.Length);
                        line = lineReplacement;
                    }
                    for (var i = 0; i < line.Length; i++)
                    {
                        var thisByte = line[i];
                        if (thisByte == 0x04) thisByte = 0x02;
                        var thisByteInvert = invertLine.Length > i ? invertLine[i] : (byte) 0;
                        var inverted = false;
                        if (thisByteInvert > 0 && thisByteInvert != 32)
                        {
                            inverted = true;
                        }
                        var thisCharImage = _font.GetCharImage(thisByte, inverted);

                        float xPos = location.X + (i*thisCharImage.Width);
                        float yPos = location.Y;
                        //g.DrawImage(thisCharImage, new Point((int)xPos, (int)yPos));
                        g.DrawImageUnscaled(thisCharImage, new Point((int) xPos, (int) yPos));
                    }
                }
            }
        }

    }
}