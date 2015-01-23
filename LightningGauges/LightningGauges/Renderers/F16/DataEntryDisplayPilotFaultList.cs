using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers.F16
{
    public interface IDataEntryDisplayPilotFaultList : IInstrumentRenderer
    {
        DataEntryDisplayPilotFaultList.DataEntryDisplayPilotFaultListInstrumentState InstrumentState { get; set; }
    }

    public class DataEntryDisplayPilotFaultList : InstrumentRendererBase, IDataEntryDisplayPilotFaultList
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

        public DataEntryDisplayPilotFaultList()
        {
            InstrumentState = new DataEntryDisplayPilotFaultListInstrumentState();
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

        public DataEntryDisplayPilotFaultListInstrumentState InstrumentState { get; set; }

        #region Instrument State

        [Serializable]
        public class DataEntryDisplayPilotFaultListInstrumentState : InstrumentStateBase
        {
            public DataEntryDisplayPilotFaultListInstrumentState()
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
                var width = 239;
                var height = 87;
                destinationGraphics.ResetTransform(); //clear any existing transforms
                destinationGraphics.SetClip(destinationRectangle); //set the clipping region on the graphics object to our render rectangle's boundaries
                destinationGraphics.FillRectangle(Brushes.Black, destinationRectangle);
                destinationGraphics.ScaleTransform(destinationRectangle.Width/(float) width, destinationRectangle.Height/(float) height);
                //set the initial scale transformation 
                destinationGraphics.TranslateTransform(-8, -84);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = destinationGraphics.Save();

                //draw the background image
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                destinationGraphics.DrawImageFast(_background.MaskedImage,
                            new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height),
                            new Rectangle(0, 0, _background.MaskedImage.Width, _background.MaskedImage.Height),
                            GraphicsUnit.Pixel);
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);

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

                        GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                        destinationGraphics.TranslateTransform(15, 91);
                        destinationGraphics.DrawImageFast(textImage, new Rectangle(0, 0, width - 16, height - 14),
                                    new Rectangle(0, 0, textImage.Width, textImage.Height), GraphicsUnit.Pixel);
                        GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                    }
                }
                //restore the canvas's transform and clip settings to what they were when we entered this method
                destinationGraphics.Restore(initialState);
            }
        }

        private void DrawDEDPFLLineOfText(Graphics g, Point location, byte[] line, byte[] invertLine)
        {
            lock (_imagesLock)
            {
                if (line == null) return;
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
                    if (thisByte == 0x5E) thisByte = 0x0A; //18-08-12 Added by Falcas, sets the correct deg symbol
                    var thisByteInvert = invertLine !=null && invertLine.Length > i ? invertLine[i] : (byte) 0;
                    var inverted = thisByteInvert > 0 && thisByteInvert != 32;
                    var thisCharImage = _font.GetCharImage(thisByte, inverted);

                    float xPos = location.X + (i*thisCharImage.Width);
                    float yPos = location.Y;
                    g.DrawImageUnscaled(thisCharImage, new Point((int) xPos, (int) yPos));
                }
            }
        }

        }
}