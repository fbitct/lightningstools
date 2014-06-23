﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrain
{
    [Serializable]
    internal class LodTextureKey
    {
        public uint Lod;
        public uint chunkXIndex;
        public uint chunkYIndex;
        public uint textureId;

        public override string ToString()
        {
            return (Common.Serialization.Util.ToRawBytes(this));
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