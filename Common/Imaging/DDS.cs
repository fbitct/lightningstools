using System.Drawing;
using System.IO;
using FreeImageAPI;

namespace Common.Imaging
{
    public static class DDS
    {
        public static Bitmap Load(string filePath)
        {
            return
                FreeImage.GetBitmap(FreeImage.Load(FREE_IMAGE_FORMAT.FIF_DDS, filePath, FREE_IMAGE_LOAD_FLAGS.DEFAULT));
        }

        public static Bitmap GetBitmapFromDDSFileBytes(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return GetBitmapFromDDSFileStream(ms);
            }

        }

        public static Bitmap GetBitmapFromDDSFileStream(Stream s)
        {
            var format = FREE_IMAGE_FORMAT.FIF_DDS;
            return FreeImage.GetBitmap(FreeImage.LoadFromStream(s, FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref format));
        }

     
    }
}