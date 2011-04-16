using System.Drawing;
using Common.SimSupport;

namespace MFDExtractor.UI
{
    public class CanvasRenderer : IInstrumentRenderer
    {
        public Image Image { get; set; }

        #region IInstrumentRenderer Members

        public void Render(Graphics g, Rectangle bounds)
        {
            if (Image != null)
            {
                g.DrawImage(Image, bounds);
            }
        }

        #endregion
    }
}