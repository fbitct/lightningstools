using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace LightningGauges
{
    class DEDPFLFont:IDisposable
    {
        private Bitmap _font;
        private Bitmap[] _charBitmaps = new Bitmap[256];
        private Bitmap[] _invertCharBitmaps = new Bitmap[256];
        private bool _disposed = false;
        private DEDPFLFont() : base() { }
        public DEDPFLFont(string fileName)
        {
            _font = (Bitmap)Common.Imaging.Util.LoadBitmapFromFile(fileName);
        }
        public Bitmap GetCharImage(byte someByte, bool invert)
        {
            if (someByte >= 32) someByte -= 32;

            Bitmap[] glyphCache = _charBitmaps;
            if (invert) glyphCache = _invertCharBitmaps;

            if (glyphCache[someByte] == null)
            {
                int glyphWidth = _font.Width / 16;
                int glyphHeight = _font.Height / 16;
                Bitmap thisCharBitmap = new Bitmap(glyphWidth, glyphHeight, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
                int leftX = (((int)someByte) % 16) * glyphWidth;
                int topY = (int)(((int)someByte) / 16) * (int)((float)glyphHeight);
                if (invert) topY += _font.Height / 2;
                Rectangle toCut = new Rectangle(new Point(leftX, topY), new Size(glyphWidth, glyphHeight));
                using (Graphics g = Graphics.FromImage(thisCharBitmap))
                {
                    g.FillRectangle(Brushes.Black, new Rectangle(0, 0, glyphWidth, glyphHeight));
                    g.DrawImage(_font, new Rectangle(0, 0, glyphWidth, glyphHeight), toCut, GraphicsUnit.Pixel);
                }
                glyphCache[someByte] = thisCharBitmap;
            }
            return glyphCache[someByte];
        }
        public Bitmap GetCharImage(char someChar, bool invert)
        {
            byte thisCharByte = Encoding.ASCII.GetBytes(new char[] { someChar })[0];
            return GetCharImage(thisCharByte, invert);
        }
        ~DEDPFLFont()
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
                    Common.Util.DisposeObject(_font);
                    if (_charBitmaps != null)
                    {
                        for (int i = 0; i < _charBitmaps.Length; i++)
                        {
                            Common.Util.DisposeObject(_charBitmaps[i]);
                        }
                    }
                    if (_invertCharBitmaps != null)
                    {
                        for (int i = 0; i < _invertCharBitmaps.Length; i++)
                        {
                            Common.Util.DisposeObject(_invertCharBitmaps[i]);
                        }
                    }
                }
                _disposed = true;
            }

        }
    }
}
