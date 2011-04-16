using System.Drawing;
using System.Drawing.Imaging;
using Common;

namespace MFDExtractor.Networking
{
    internal class ImageObjectStore : ObjectStore
    {
        private readonly string _compressionType;
        private readonly string _imageFormat;

        private ImageObjectStore()
        {
        }

        public ImageObjectStore(string compressionType, string imageFormat)
            : this()
        {
            _compressionType = compressionType;
            _imageFormat = imageFormat;
        }

        public void StoreImage(string instrumentName, Image image)
        {
            object imageLock = GetLockForObject(instrumentName);
            lock (imageLock)
            {
                var existingImage = GetRawObject(instrumentName) as Image;
                StoreRawObject(instrumentName, image);
                Util.DisposeObject(existingImage);
            }
        }

        public override byte[] SerializeObject(object toSerialize)
        {
            if (toSerialize == null)
            {
                return null;
            }
            else if (toSerialize is Image)
            {
                var asImage = toSerialize as Image;
                Common.Imaging.Util.ConvertPixelFormat(ref asImage, PixelFormat.Format16bppRgb565);
                return Common.Imaging.Util.BytesFromBitmap(asImage, _compressionType, _imageFormat);
            }
            else
            {
                return base.SerializeObject(toSerialize);
            }
        }
    }
}