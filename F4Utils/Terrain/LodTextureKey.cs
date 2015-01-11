using System;
using Common.Serialization;

namespace F4Utils.Terrain
{
    [Serializable]
    public class LodTextureKey
    {
        public uint Lod;
        public uint chunkXIndex;
        public uint chunkYIndex;
        public uint textureId;

        public override string ToString()
        {
            return (Util.ToRawBytes(this));
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (GetType() != obj.GetType()) return false;
            if (ToString() != obj.ToString()) return false;
            return true;
        }
        public LodTextureKey()
        {
        }

        public LodTextureKey(uint lod, uint textureId, uint chunkXIndex, uint chunkYIndex)
            : this()
        {
            Lod = lod;
            this.textureId = textureId;
            this.chunkXIndex = chunkXIndex;
            this.chunkYIndex = chunkYIndex;
        }
    }


}
