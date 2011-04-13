using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MFDExtractor.Networking
{
    internal class ImageObjectStore:ObjectStore
    {
        private string _compressionType = null;
        private string _imageFormat = null;
        private ImageObjectStore() { }
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
                Image existingImage = GetRawObject(instrumentName) as Image;
                StoreRawObject(instrumentName, image);
                Common.Util.DisposeObject(existingImage);
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
                Image asImage = toSerialize as Image;
                Common.Imaging.Util.ConvertPixelFormat(ref asImage, PixelFormat.Format16bppRgb565);
                return Common.Imaging.Util.BytesFromBitmap(asImage as Image, _compressionType, _imageFormat);
            }
            else
            {
                return base.SerializeObject(toSerialize);
            }
        }
    }
}
