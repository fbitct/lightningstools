using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace F16CPD.SimSupport.Falcon4.MovingMap
{
    internal interface IMapLoadingMessageRenderer
    {
        void RenderLoadingMessage(Graphics g, Rectangle renderRectangle, float mapRenderProgressPercentage);
    }

    internal class MapLoadingMessageRenderer : IMapLoadingMessageRenderer
    {

        public void RenderLoadingMessage(Graphics g, Rectangle renderRectangle, float mapRenderProgressPercentage)
        {
            var gTransform = g.Transform;
            g.ResetTransform();
            var toDisplay = String.Format("LOADING: {0}%", mapRenderProgressPercentage);
            var greenBrush = Brushes.Green;
            var path = new GraphicsPath();
            var sf = new StringFormat(StringFormatFlags.NoWrap)
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            var f = new Font(FontFamily.GenericMonospace, 20, FontStyle.Bold);
            var textSize = g.MeasureString(toDisplay, f, 1, sf);
            var leftX = (((renderRectangle.Width - ((int) textSize.Width))/2));
            var topY = (((renderRectangle.Height - ((int) textSize.Height))/2));
            var target = new Rectangle(leftX, topY, (int) textSize.Width, (int) textSize.Height);
            path.AddString(toDisplay, f.FontFamily, (int) f.Style, f.Size, target.Location, sf);
            g.FillPath(greenBrush, path);
            g.Transform = gTransform;
        }
    }
}