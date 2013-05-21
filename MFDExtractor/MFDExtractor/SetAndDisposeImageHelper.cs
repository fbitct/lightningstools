using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Common.Networking;
using MFDExtractor.UI;

namespace MFDExtractor
{
    internal interface ISetAndDisposeImageHelper
    {
        void SetAndDisposeImage(ExtractorState extractorState, Image image, Action<Image> serveImageFunc, RotateFlipType rotateFlipType, InstrumentForm instrumentForm, bool monochrome);
    }

    class SetAndDisposeImageHelper : ISetAndDisposeImageHelper
    {
        public void SetAndDisposeImage(
            ExtractorState extractorState, 
            Image image, 
            Action<Image> serveImageFunc, 
            RotateFlipType rotateFlipType, 
            InstrumentForm instrumentForm, 
            bool monochrome)
        {
            if (image == null) return;
            if (extractorState.NetworkMode == NetworkMode.Server)
            {
                serveImageFunc(image);
            }
            if (instrumentForm != null)
            {
                if (rotateFlipType != RotateFlipType.RotateNoneFlipNone)
                {
                    image.RotateFlip(rotateFlipType);
                }
                if (monochrome)
                {
                    DrawImageToControlMonochrome(image, instrumentForm);
                }
                else
                {
                    DrawImageToControlInColor(image, instrumentForm);
                }
            }
            Common.Util.DisposeObject(image);
        }

        private static void DrawImageToControlInColor(Image image, Control targetForm)
        {
            using (var graphics = targetForm.CreateGraphics())
            {
                graphics.DrawImage(image, targetForm.ClientRectangle);
            }
        }

        private static void DrawImageToControlMonochrome(Image image, Control targetForm)
        {
            using (var graphics = targetForm.CreateGraphics())
            {
                var imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(Common.Imaging.Util.GreyscaleColorMatrix);
                using (var compatible = Common.Imaging.Util.CopyBitmap(image))
                {
                    graphics.DrawImage(compatible, targetForm.ClientRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
                }
            }
        }
    }
}
