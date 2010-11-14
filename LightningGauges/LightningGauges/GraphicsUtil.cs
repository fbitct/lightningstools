using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace LightningGauges
{
    class GraphicsUtil
    {
        public static void SaveGraphicsState(Graphics g, ref GraphicsState state)
        {
            state = g.Save();
        }
        public static void RestoreGraphicsState(Graphics g, ref GraphicsState state)
        {
            g.Restore(state);
            state = g.Save();
        }

    }
}
