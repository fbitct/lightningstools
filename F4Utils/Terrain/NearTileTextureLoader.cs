using System;
using System.Drawing;
using System.IO;
using Common.Imaging;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using Util = Common.Compression.Zip.Util;

namespace F4Utils.Terrain
{
    public interface INearTileTextureLoader
    {
        Bitmap LoadNearTileTexture(string tileName, TerrainDB terrainDB);
    }
    public class NearTileTextureLoader:INearTileTextureLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NearTileTextureLoader));

        public Bitmap LoadNearTileTexture(string tileName, TerrainDB terrainDB)
        {
            Bitmap toReturn;
            var tileFullPath = Path.Combine(terrainDB.CurrentTheaterTextureBaseFolderPath, tileName);

            var tileInfo = new FileInfo(tileFullPath);
            if (string.Equals(tileInfo.Extension, ".PCX", StringComparison.InvariantCultureIgnoreCase))
            {
                tileFullPath = Path.Combine(Path.Combine(terrainDB.CurrentTheaterTextureBaseFolderPath, "texture"),
                                            Path.GetFileNameWithoutExtension(tileInfo.Name) + ".DDS");
                tileInfo = new FileInfo(tileFullPath);
            }
            if (tileInfo.Exists)
            {
                try
                {
                    toReturn = DDS.Load(tileFullPath);
                    return toReturn;
                }
                catch (Exception e)
                {
                    Log.Debug(e.Message, e);
                }
            }


            if (terrainDB.TextureZipFile == null)
            {
                terrainDB.TextureZipFile = new ZipFile(terrainDB.CurrentTheaterTextureBaseFolderPath + Path.DirectorySeparatorChar + "texture.zip");
            }
            if (terrainDB.TextureDotZipFileEntries == null || terrainDB.TextureDotZipFileEntries.Count == 0)
            {
                terrainDB.TextureDotZipFileEntries = Util.GetZipFileEntries(terrainDB.TextureZipFile);
            }
            if (!terrainDB.TextureDotZipFileEntries.ContainsKey(tileName.ToLowerInvariant())) return null;
            var thisEntry = terrainDB.TextureDotZipFileEntries[tileName.ToLowerInvariant()];
            using (var zipStream = terrainDB.TextureZipFile.GetInputStream(thisEntry))
            {
                var rawBytes = new byte[zipStream.Length];
                zipStream.Read(rawBytes, 0, rawBytes.Length);
                toReturn = PCX.LoadFromBytes(rawBytes);
            }
            return toReturn;
        }

    }
}
