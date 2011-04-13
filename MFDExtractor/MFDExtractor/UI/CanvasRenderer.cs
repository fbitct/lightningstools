using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.SimSupport;
using System.Drawing;

namespace MFDExtractor.UI
{
    public class CanvasRenderer:IInstrumentRenderer
    {
        public Image Image { get; set; }
        public void Render(System.Drawing.Graphics g, System.Drawing.Rectangle bounds)
        {
            if (this.Image != null)
            {
                g.DrawImage(this.Image, bounds);
            }
        }
    }
}
