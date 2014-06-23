using Common.Imaging;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F4Utils.Terrain
{
    internal interface INearTileTextureLoader
    {
        Bitmap LoadNearTileTexture(string textureBaseFolderPath, string tileName, ref ZipFile textureZipFile, ref Dictionary<string, ZipEntry> textureDotZipFileEntries);
    }
    class NearTileTextureLoader:INearTileTextureLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NearTileTextureLoader));

        public Bitmap LoadNearTileTexture(string textureBaseFolderPath, string tileName, ref ZipFile textureZipFile, ref Dictionary<string, ZipEntry> textureDotZipFileEntries)
        {
            Bitmap toReturn;
            var tileFullPath = Path.Combine(textureBaseFolderPath, tileName);

            var tileInfo = new FileInfo(tileFullPath);
            if (string.Equals(tileInfo.Extension, ".PCX", StringComparison.InvariantCultureIgnoreCase))
            {
                tileFullPath = Path.Combine(Path.Combine(textureBaseFolderPath, "texture"),
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


            if (textureZipFile == null)
            {
                textureZipFile = new ZipFile(textureBaseFolderPath + Path.DirectorySeparatorChar + "texture.zip");
            }
            if (textureDotZipFileEntries == null || textureDotZipFileEntries.Count == 0)
            {
                textureDotZipFileEntries = Common.Compression.Zip.Util.GetZipFileEntries(textureZipFile);
            }
            if (!textureDotZipFileEntries.ContainsKey(tileName.ToLowerInvariant())) return null;
            var thisEntry = textureDotZipFileEntries[tileName.ToLowerInvariant()];
            using (var zipStream = textureZipFile.GetInputStream(thisEntry))
            {
                var rawBytes = new byte[zipStream.Length];
                zipStream.Read(rawBytes, 0, rawBytes.Length);
                toReturn = PCX.LoadFromBytes(rawBytes);
            }
            return toReturn;
        }

    }
}
