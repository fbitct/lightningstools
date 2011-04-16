using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Common.Imaging;

namespace LightningGauges
{
    internal class DEDPFLFont : IDisposable
    {
        private readonly Bitmap[] _charBitmaps = new Bitmap[256];
        private readonly Bitmap _font;
        private readonly Bitmap[] _invertCharBitmaps = new Bitmap[256];
        private bool _disposed;

        private DEDPFLFont()
        {
        }

        public DEDPFLFont(string fileName)
        {
            _font = (Bitmap) Util.LoadBitmapFromFile(fileName);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public Bitmap GetCharImage(byte someByte, bool invert)
        {
            if (someByte >= 32) someByte -= 32;

            Bitmap[] glyphCache = _charBitmaps;
            if (invert) glyphCache = _invertCharBitmaps;

            if (glyphCache[someByte] == null)
            {
                int glyphWidth = _font.Width/16;
                int glyphHeight = _font.Height/16;
                var thisCharBitmap = new Bitmap(glyphWidth, glyphHeight, PixelFormat.Format16bppRgb555);
                int leftX = ((someByte)%16)*glyphWidth;
                int topY = ((someByte)/16)*(glyphHeight);
                if (invert) topY += _font.Height/2;
                var toCut = new Rectangle(new Point(leftX, topY), new Size(glyphWidth, glyphHeight));
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
            byte thisCharByte = Encoding.ASCII.GetBytes(new[] {someChar})[0];
            return GetCharImage(thisCharByte, invert);
        }

        ~DEDPFLFont()
        {
            Dispose(false);
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