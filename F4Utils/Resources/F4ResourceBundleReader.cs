using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
namespace F4Utils.Resources
{

    public enum F4ResourceType : uint
    {
        Unknown = 0,
        ImageResource = 100,
        SoundResource = 101,
        FlatResource = 102,
    }
    public class F4ResourceBundleReader
    {

        [Flags]
        protected internal enum F4ResourceFlags : uint
        {
            EightBit = 0x00000001,
            SixteenBit = 0x00000002,
            UseColorKey = 0x40000000,
        }
        protected internal class F4ResourceRawDataPackage
        {
            public uint Version;
            public uint Size;
            public byte[] Data;
        }
        protected internal class F4ResourceHeader
        {
            public uint Type;
            public string ID = null;
        }
        protected internal class F4FlatResourceHeader : F4ResourceHeader
        {
            public uint Offset;
            public uint Size;
        }
        protected internal class F4ImageResourceHeader : F4ResourceHeader
        {
            public uint Flags;
            public ushort CenterX;
            public ushort CenterY;
            public ushort Width;
            public ushort Height;
            public uint ImageOffset;
            public uint PaletteSize;
            public uint PaletteOffset;
        }
        protected internal class F4SoundResourceHeader : F4ResourceHeader
        {
            public uint Flags;
            public ushort Channels;
            public ushort SoundType;
            public uint Offset;
            public uint HeaderSize;
        }
        protected internal class F4ResourceBundleIndex
        {
            public uint Size;
            public uint NumResources;
            public uint ResourceIndexVersion;
            public F4ResourceHeader[] ResourceHeaders;
            public F4ResourceRawDataPackage ResourceData;
        }
        
        private F4ResourceBundleIndex _resourceIndex = null;
        public virtual void Load(string resourceBundleIndexPath)
        {
            FileInfo resourceIndexFileInfo = new FileInfo(resourceBundleIndexPath);
            if (resourceIndexFileInfo.Exists)
            {
                byte[] bytes = new byte[resourceIndexFileInfo.Length];
                using (FileStream fs = new FileStream(resourceBundleIndexPath, FileMode.Open))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.Read(bytes, 0, (int)resourceIndexFileInfo.Length);
                }
                _resourceIndex = new F4ResourceBundleIndex();
                int curByte = 0;
                _resourceIndex.Size= BitConverter.ToUInt32(bytes, curByte);
                curByte += 4;
                _resourceIndex.ResourceIndexVersion = BitConverter.ToUInt32(bytes, curByte);
                curByte += 4;
                uint size = _resourceIndex.Size;
                List<F4ResourceHeader> headers = new List<F4ResourceHeader>();

                while (size >0)
                {
                    _resourceIndex.NumResources++;
                    uint resourceType = BitConverter.ToUInt32(bytes, curByte);
                    curByte += 4;
                    byte[] resourceId = new byte[32];
                    for (int j = 0; j < 32; j++)
                    {
                        resourceId[j] = bytes[curByte];
                        curByte++;
                    }
                    string resourceName = Encoding.ASCII.GetString(resourceId);
                    int nullLoc = resourceName.IndexOf('\0');
                    if (nullLoc > 0)
                    {
                        resourceName = resourceName.Substring(0, nullLoc);
                    }
                    else
                    {
                        resourceName = null;
                    }
                    if (resourceType == (uint)(F4ResourceType.ImageResource))
                    {
                        F4ImageResourceHeader thisResourceHeader = new F4ImageResourceHeader();
                        thisResourceHeader.Type = resourceType;
                        thisResourceHeader.ID = resourceName;
                        thisResourceHeader.Flags = BitConverter.ToUInt32(bytes, curByte);
                        curByte += 4;
                        thisResourceHeader.CenterX = BitConverter.ToUInt16(bytes, curByte);
                        curByte += 2;
                        thisResourceHeader.CenterY = BitConverter.ToUInt16(bytes, curByte);
                        curByte += 2;
                        thisResourceHeader.Width = BitConverter.ToUInt16(bytes, curByte);
                        curByte += 2;
                        thisResourceHeader.Height = BitConverter.ToUInt16(bytes, curByte);
                        curByte += 2;
                        thisResourceHeader.ImageOffset = BitConverter.ToUInt32(bytes, curByte);
                        curByte += 4;
                        thisResourceHeader.PaletteSize = BitConverter.ToUInt32(bytes, curByte);
                        curByte += 4;
                        thisResourceHeader.PaletteOffset = BitConverter.ToUInt32(bytes, curByte);
                        curByte += 4;
                        headers.Add(thisResourceHeader);
                        size -= 60;
                    }
                    else if (resourceType == (uint)(F4ResourceType.SoundResource))
                    {
                        F4SoundResourceHeader thisResourceHeader = new F4SoundResourceHeader();
                        thisResourceHeader.Type = resourceType;
                        thisResourceHeader.ID = resourceName;
                        thisResourceHeader.Flags = BitConverter.ToUInt32(bytes, curByte);
                        curByte += 4;
                        thisResourceHeader.Channels = BitConverter.ToUInt16(bytes, curByte);
                        curByte += 2;
                        thisResourceHeader.SoundType = BitConverter.ToUInt16(bytes, curByte);
                        curByte += 2;
                        thisResourceHeader.Offset = BitConverter.ToUInt32(bytes, curByte);
                        curByte += 4;
                        thisResourceHeader.HeaderSize = BitConverter.ToUInt32(bytes, curByte);
                        curByte += 4;
                        headers.Add(thisResourceHeader);
                        size -= 52;
                    }
                    else if (resourceType == (uint)(F4ResourceType.FlatResource))
                    {
                        F4FlatResourceHeader thisResourceHeader = new F4FlatResourceHeader();
                        thisResourceHeader.Type = resourceType;
                        thisResourceHeader.ID = resourceName;
                        thisResourceHeader.Offset= BitConverter.ToUInt32(bytes, curByte);
                        curByte += 4;
                        thisResourceHeader.Size= BitConverter.ToUInt32(bytes, curByte);
                        curByte += 4;
                        headers.Add(thisResourceHeader);
                        size -= 44;
                    }
                }
                _resourceIndex.ResourceHeaders = headers.ToArray();

                FileInfo resourceDataFileInfo = new FileInfo(
                    Path.GetDirectoryName(resourceIndexFileInfo.FullName) + Path.DirectorySeparatorChar + 
                    Path.GetFileNameWithoutExtension(resourceIndexFileInfo.FullName) + ".rsc");
                if (resourceDataFileInfo.Exists)
                {
                    bytes = new byte[resourceDataFileInfo.Length];

                    using (FileStream fs = new FileStream(resourceDataFileInfo.FullName, FileMode.Open))
                    {
                        fs.Seek(0, SeekOrigin.Begin);
                        fs.Read(bytes, 0, (int)resourceDataFileInfo.Length);
                    }
                    F4ResourceRawDataPackage rawDataPackage = new F4ResourceRawDataPackage();
                    curByte = 0;
                    rawDataPackage.Size = BitConverter.ToUInt32(bytes, curByte);
                    curByte += 4;
                    rawDataPackage.Version = BitConverter.ToUInt32(bytes, curByte);
                    curByte += 4;
                    rawDataPackage.Data = new byte[rawDataPackage.Size];
                    int numBytesToCopy = (int)Math.Min(rawDataPackage.Data.Length, bytes.Length - curByte);
                    Array.Copy(bytes, curByte, rawDataPackage.Data, 0, numBytesToCopy);
                    curByte += numBytesToCopy;
                    _resourceIndex.ResourceData = rawDataPackage;
                }
                else
                {
                    throw new FileNotFoundException(resourceDataFileInfo.FullName);
                }

            }
            else
            {
                throw new FileNotFoundException(resourceBundleIndexPath);
            }
        }
        public int NumResources
        {
            get
            {
                if (_resourceIndex == null)
                {
                    return -1;
                }
                else
                {
                    return (int)_resourceIndex.NumResources;
                }
            }
        }
        public virtual F4ResourceType GetResourceType(int resourceNum)
        {
            if (_resourceIndex == null)
            {
                return F4ResourceType.Unknown;
            }
            else
            {
                return (F4ResourceType)_resourceIndex.ResourceHeaders[resourceNum].Type;
            }
        }
        public virtual string  GetResourceID(int resourceNum)
        {
            if (_resourceIndex == null)
            {
                return null;
            }
            else
            {
                return _resourceIndex.ResourceHeaders[resourceNum].ID;
            }
        }
        
        public virtual byte[] GetSoundResource(string resourceId)
        {
            F4SoundResourceHeader resourceHeader = GetResourceHeader(resourceId) as F4SoundResourceHeader;
            return GetSoundResource(resourceHeader);
        }
        public virtual byte[] GetSoundResource(int resourceNum)
        {
            if (_resourceIndex == null || _resourceIndex.ResourceHeaders == null || resourceNum >= _resourceIndex.ResourceHeaders.Length)
            {
                return null;
            }
            F4SoundResourceHeader resourceHeader = _resourceIndex.ResourceHeaders[resourceNum] as F4SoundResourceHeader;
            return GetSoundResource(resourceHeader);
        }
        protected virtual byte[] GetSoundResource(F4SoundResourceHeader resourceHeader)
        {
            if (resourceHeader == null) return null;
            int curByte = (int)resourceHeader.Offset;
            curByte += 4;
            uint dataSize = BitConverter.ToUInt32(_resourceIndex.ResourceData.Data, curByte);
            curByte += 4;
            byte[] toReturn = new byte[dataSize+8];
            Array.Copy(_resourceIndex.ResourceData.Data, curByte-8, toReturn, 0, dataSize+8);
            return toReturn;
        }
        public virtual byte[] GetFlatResource(int resourceNum)
        {
            if (_resourceIndex == null || _resourceIndex.ResourceHeaders == null || resourceNum >= _resourceIndex.ResourceHeaders.Length)
            {
                return null;
            }
            F4FlatResourceHeader resourceHeader = _resourceIndex.ResourceHeaders[resourceNum] as F4FlatResourceHeader;
            return GetFlatResource(resourceHeader);
        }
        public virtual byte[] GetFlatResource(string resourceId)
        {
            F4FlatResourceHeader resourceHeader = GetResourceHeader(resourceId) as F4FlatResourceHeader;
            return GetFlatResource(resourceHeader);
        }
        protected virtual byte[] GetFlatResource(F4FlatResourceHeader resourceHeader)
        {
            if (resourceHeader == null) return null;
            byte[] bytes = new byte[resourceHeader.Size];
            for (int i = 0; i < resourceHeader.Size; i++)
            {
                bytes[i] = _resourceIndex.ResourceData.Data[resourceHeader.Offset + i];
            }
            return bytes;
        }
        public virtual Bitmap GetImageResource(string resourceId)
        {
            F4ImageResourceHeader imageHeader = GetResourceHeader(resourceId) as F4ImageResourceHeader;
            return GetImageResource(imageHeader);
        }
        public virtual Bitmap GetImageResource(int resourceNum)
        {
            if (_resourceIndex == null || _resourceIndex.ResourceHeaders == null || resourceNum >= _resourceIndex.ResourceHeaders.Length)
            {
                return null;
            }
            F4ImageResourceHeader imageHeader = _resourceIndex.ResourceHeaders[resourceNum] as F4ImageResourceHeader;
            return GetImageResource(imageHeader);
        }
        protected virtual Bitmap GetImageResource(F4ImageResourceHeader imageHeader)
        {
            if (imageHeader == null) return null;
            Bitmap toReturn = new Bitmap(imageHeader.Width, imageHeader.Height);
            ushort[] palette = new ushort[imageHeader.PaletteSize];
            if ((imageHeader.Flags & (uint)F4ResourceFlags.EightBit) == (uint)F4ResourceFlags.EightBit)
            {
                for (int i = 0; i < palette.Length; i++)
                {
                    palette[i] = BitConverter.ToUInt16(_resourceIndex.ResourceData.Data, (int)imageHeader.PaletteOffset + (i * 2));
                }
            }
            int curByte = 0;
            for (int y = 0; y < imageHeader.Height; y++)
            {
                for (int x = 0; x < imageHeader.Width; x++)
                {
                    int A = 0;
                    int R = 0;
                    int G = 0;
                    int B = 0;
                    if ((imageHeader.Flags & (uint)F4ResourceFlags.EightBit)==(uint)F4ResourceFlags.EightBit)
                    {
                        byte thisPixelPaletteIndex = _resourceIndex.ResourceData.Data[imageHeader.ImageOffset + curByte];
                        ushort thisPixelPaletteEntry = palette[thisPixelPaletteIndex];
                        A = 255;
                        R = ((thisPixelPaletteEntry & 0x7C00) >> 10) << 3;
                        G = ((thisPixelPaletteEntry & 0x3E0) >> 5) << 3;
                        B = (thisPixelPaletteEntry & 0x1F) << 3;
                        curByte++;
                    }
                    else if ((imageHeader.Flags & (uint)F4ResourceFlags.SixteenBit) == (uint)F4ResourceFlags.SixteenBit)
                    {
                        ushort thisPixelPaletteEntry = BitConverter.ToUInt16(_resourceIndex.ResourceData.Data, (int)(imageHeader.ImageOffset + curByte));
                        A = 255;
                        R = ((thisPixelPaletteEntry & 0x7C00) >> 10) << 3;
                        G = ((thisPixelPaletteEntry & 0x3E0) >> 5) << 3;
                        B = (thisPixelPaletteEntry & 0x1F) << 3;
                        curByte+=2;
                    }
                    toReturn.SetPixel(x, y, Color.FromArgb(A, R, G, B));
                }
            }
            return toReturn;
        }
        protected virtual F4ResourceHeader GetResourceHeader(string resourceId) 
        {
            if (_resourceIndex == null || _resourceIndex.ResourceHeaders == null || resourceId == null)
            {
                return null;
            }
            for (int i = 0; i < _resourceIndex.ResourceHeaders.Length; i++)
            {
                F4ResourceHeader thisResourceHeader = _resourceIndex.ResourceHeaders[i];
                string thisResourceId = thisResourceHeader.ID;
                if (thisResourceId.ToLowerInvariant() == resourceId.ToLowerInvariant())
                {
                    return thisResourceHeader;
                }
            }
            return null;
        }
    }
}
