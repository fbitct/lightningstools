using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Common.Imaging;
using Common.SimSupport;
using MFDExtractor.UI;
using log4net;
using System.Collections.Generic;

namespace MFDExtractor
{
    internal interface IInstrumentRenderHelper
    {
        void Render(
            IInstrumentRenderer renderer, 
            InstrumentForm targetForm, 
            RotateFlipType rotation, 
            bool monochrome, 
            bool highlightBorder,
            bool nightVisionMode);
    }

    class InstrumentRenderHelper : IInstrumentRenderHelper
    {
        private readonly ILog _log;
        private readonly Dictionary<Form, Bitmap> _renderSurfaces = new Dictionary<Form, Bitmap>();
        public InstrumentRenderHelper(ILog log = null)
        {
            _log = log ?? LogManager.GetLogger(GetType());
        }
        public void Render(
            IInstrumentRenderer renderer, 
            InstrumentForm targetForm, 
            RotateFlipType rotation, 
            bool monochrome, 
            bool highlightBorder,
            bool nightVisionMode)
        {
            Bitmap renderSurface = null;
            try
            {
                renderSurface = ObtainRenderSurface(targetForm);
                using (var destinationGraphics = Graphics.FromImage(renderSurface))
                {
                    try
                    {
                        renderer.Render(destinationGraphics, new Rectangle(0, 0, renderSurface.Width, renderSurface.Height));
                        targetForm.LastRenderedOn = DateTime.Now;
                        if (highlightBorder)
                        {
                            HighlightBorder(targetForm, destinationGraphics, renderSurface);
                        }
                    }
                    catch (ThreadAbortException)
                    {
                    }
                    catch (ThreadInterruptedException)
                    {
                    }
                    catch (Exception ex)
                    {
                        var rendererName  =renderer !=null ? renderer.GetType().FullName:"unknown";
                        _log.Error(string.Format("An error occurred while rendering {0}", rendererName), ex);
                    }
                }
                if (rotation != RotateFlipType.RotateNoneFlipNone)
                {
                    renderSurface.RotateFlip(rotation);
                }
                using (var graphics = targetForm.CreateGraphics())
                {
                    if (nightVisionMode)
                    {
                        RenderWithNightVisionEffect(targetForm, graphics, renderSurface);
                    }
                    else if (monochrome)
                    {
                        RenderWithMonochromeEffect(targetForm, graphics, renderSurface);
                    }
                    else
                    {
                        RenderWithStandardEffect(graphics, renderSurface);
                    }
                }
            }
            catch (ExternalException)
            {
                //GDI+ error message we don't care about
            }
            catch (ObjectDisposedException)
            {
                //GDI+ error message thrown on operations on disposed images -- can happen when one thread disposes while shutting-down thread tries to render
            }
            catch (ArgumentException)
            {
                //GDI+ error message we don't care about
            }
            catch (OutOfMemoryException)
            {
                //bullshit OOM messages from GDI+
            }
            catch (InvalidOperationException)
            {
                //GDI+ error message we don't care about
            }
            finally
            {
                //Common.Util.DisposeObject(renderSurface);
            }
        }

        private static void RenderWithStandardEffect(Graphics graphics, Bitmap image)
        {
            graphics.DrawImageUnscaled(image, 0, 0, image.Width, image.Height);
        }

        private static void RenderWithMonochromeEffect(Control targetForm, Graphics graphics, Bitmap image)
        {
            var monochromeImageAttribs = new ImageAttributes();
            var greyscaleColorMatrix = Util.GreyscaleColorMatrix;
            monochromeImageAttribs.SetColorMatrix(greyscaleColorMatrix, ColorMatrixFlag.Default);
            graphics.DrawImage(image, targetForm.ClientRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, monochromeImageAttribs);
        }

        private static void RenderWithNightVisionEffect(Control targetForm, Graphics graphics, Bitmap image)
        {
            graphics.DrawImage(image, targetForm.ClientRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel,NvisImageAttribs);
        }

        private static ImageAttributes _nvisImageAttributes;
        private static ImageAttributes NvisImageAttribs
        {
            get
            {
                if(_nvisImageAttributes !=null) return _nvisImageAttributes;
                var nvisColorMatrix = Util.GetNVISColorMatrix(255, 255);
                _nvisImageAttributes = new ImageAttributes();
                _nvisImageAttributes.SetColorMatrix(nvisColorMatrix, ColorMatrixFlag.Default);
                return _nvisImageAttributes;
            }
        }

        private Bitmap ObtainRenderSurface(InstrumentForm targetForm)
        {
            if (!_renderSurfaces.ContainsKey(targetForm) || _renderSurfaces[targetForm].Width != targetForm.Width || _renderSurfaces[targetForm].Height != targetForm.Height)
            {
                _renderSurfaces[targetForm] =
                       (targetForm.Rotation.ToString().Contains("90") || targetForm.Rotation.ToString().Contains("270")
                       ? new Bitmap(targetForm.ClientRectangle.Height, targetForm.ClientRectangle.Width, PixelFormat.Format32bppPArgb)
                       : new Bitmap(targetForm.ClientRectangle.Width, targetForm.ClientRectangle.Height, PixelFormat.Format32bppPArgb));
            }
            return _renderSurfaces[targetForm];
        }

        private static void HighlightBorder(InstrumentForm targetForm, Graphics graphics, Bitmap image)
        {
            var scopeGreenColor = Color.FromArgb(255, 63, 250, 63);
            var scopeGreenPen = new Pen(scopeGreenColor) {Width = 5};
            graphics.DrawRectangle(scopeGreenPen, new Rectangle(new Point(0, 0), image.Size));
            targetForm.RenderImmediately = true;
        }
    }
}
