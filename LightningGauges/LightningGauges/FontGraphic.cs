using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace LightningGauges
{
    class FontGraphic : IDisposable
    {
        private Bitmap _font;
        private Bitmap[] _charBitmaps = new Bitmap[256];
        private bool _disposed = false;
        private FontGraphic() : base() { }
        public FontGraphic(string fileName)
        {
            _font = (Bitmap)Common.Imaging.Util.LoadBitmapFromFile(fileName);
        }
        public Bitmap GetCharImage(byte someByte)
        {
            someByte -= 32;
            if (_charBitmaps[someByte] == null)
            {
                int glyphWidth = _font.Width / 16;
                int glyphHeight = _font.Height / 16;
                Bitmap thisCharBitmap = new Bitmap(glyphWidth, glyphHeight, PixelFormat.Format16bppRgb565);
                int leftX = (((int)someByte) % 16) * glyphWidth;
                int topY = (int)(((int)someByte) / 16) * glyphHeight;
                Rectangle toCut = new Rectangle(new Point(leftX, topY), new Size(glyphWidth, glyphHeight));
                using (Graphics g = Graphics.FromImage(thisCharBitmap))
                {
                    g.FillRectangle(Brushes.Black, new Rectangle(0, 0, glyphWidth, glyphHeight));
                    g.DrawImage(_font, new Rectangle(0, 0, glyphWidth, glyphHeight), toCut, GraphicsUnit.Pixel);
                }
                thisCharBitmap.MakeTransparent(Color.Black);
                _charBitmaps[someByte] = thisCharBitmap;
            }
            return _charBitmaps[someByte];
        }
        public Bitmap GetCharImage(char someChar)
        {
            byte thisCharByte = Encoding.ASCII.GetBytes(new char[] { someChar })[0];
            return GetCharImage(thisCharByte);
        }
        ~FontGraphic()
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
                }
                _disposed = true;
            }
        }
    }
}
