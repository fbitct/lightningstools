using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace F4Utils.Campaign.Save
{
    public class PakFile
    {
        public Bitmap PakMap { get; private set; }
        public byte[] MapData { get; private set; }
        public PakFile(string fileName)
        {
            LoadPakFile(fileName);
        }
        private void LoadPakFile(string fileName)
        {
            //reads PAK file
            using (var stream = new FileStream(fileName, FileMode.Open))
            {
                MapData = new byte[stream.Length];
                stream.Read(MapData, 0, (int)stream.Length);
            }
            var dims = (int)Math.Sqrt(MapData.Length);
            System.Drawing.Bitmap mapImage = new System.Drawing.Bitmap(width:dims, height:dims, format: PixelFormat.Format8bppIndexed);
            var bitmapData = mapImage.LockBits(new System.Drawing.Rectangle(0, 0, dims, dims), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            Marshal.Copy(MapData, 0, bitmapData.Scan0, MapData.Length);
            mapImage.UnlockBits(bitmapData);
            PakMap = mapImage;
        }
    }
}
