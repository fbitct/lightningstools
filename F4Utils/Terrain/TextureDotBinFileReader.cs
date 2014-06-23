using F4Utils.Terrain.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F4Utils.Terrain
{
    internal interface ITextureDotBinFileReader
    {
        TextureDotBinFileInfo ReadTextureDotBinFile(string textureDotBinFilePath);
    }
    class TextureDotBinFileReader:ITextureDotBinFileReader
    {
        public TextureDotBinFileInfo ReadTextureDotBinFile(string textureDotBinFilePath)
        {
            if (String.IsNullOrEmpty(textureDotBinFilePath)) throw new ArgumentNullException("textureBinFilePath");
            var fileInfo = new FileInfo(textureDotBinFilePath);
            if (!fileInfo.Exists) throw new FileNotFoundException(textureDotBinFilePath);

            var bytesToRead = (int)fileInfo.Length;
            var bytesRead = new byte[bytesToRead];

            using (var stream = File.OpenRead(textureDotBinFilePath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(bytesRead, 0, bytesToRead);
                stream.Close();
            }
            var textureBinFileInfo = new TextureDotBinFileInfo();
            var curByte = 0;

            textureBinFileInfo.numSets = BitConverter.ToUInt32(bytesRead, curByte);
            textureBinFileInfo.setRecords = new TextureBinSetRecord[textureBinFileInfo.numSets];
            curByte += 4;
            textureBinFileInfo.totalTiles = BitConverter.ToUInt32(bytesRead, curByte);
            curByte += 4;

            for (var h = 0; h < textureBinFileInfo.numSets; h++)
            {
                var thisSetRecord = new TextureBinSetRecord
                {
                    numTiles = BitConverter.ToUInt32(bytesRead, curByte)
                };

                curByte += 4;
                thisSetRecord.terrainType = bytesRead[curByte];
                curByte++;

                thisSetRecord.tileRecords = new TextureBinTileRecord[thisSetRecord.numTiles];
                for (var i = 0; i < thisSetRecord.numTiles; i++)
                {
                    var tileRecord = new TextureBinTileRecord
                    {
                        tileName = Encoding.ASCII.GetString(bytesRead, curByte, 20)
                    };
                    var nullLoc = tileRecord.tileName.IndexOf('\0');
                    if (nullLoc > 0) tileRecord.tileName = tileRecord.tileName.Substring(0, nullLoc);
                    curByte += 20;
                    tileRecord.numAreas = BitConverter.ToUInt32(bytesRead, curByte);
                    tileRecord.areaRecords = new TextureBinAreaRecord[tileRecord.numAreas];
                    curByte += 4;
                    tileRecord.numPaths = BitConverter.ToUInt32(bytesRead, curByte);
                    tileRecord.pathRecords = new TextureBinPathRecord[tileRecord.numPaths];
                    curByte += 4;
                    for (var j = 0; j < tileRecord.numAreas; j++)
                    {
                        var thisAreaRecord = new TextureBinAreaRecord { type = BitConverter.ToInt32(bytesRead, curByte) };
                        curByte += 4;
                        thisAreaRecord.size = BitConverter.ToSingle(bytesRead, curByte);
                        curByte += 4;
                        thisAreaRecord.x = BitConverter.ToSingle(bytesRead, curByte);
                        curByte += 4;
                        thisAreaRecord.y = BitConverter.ToSingle(bytesRead, curByte);
                        curByte += 4;
                        tileRecord.areaRecords[j] = thisAreaRecord;
                    }
                    for (var k = 0; k < tileRecord.numPaths; k++)
                    {
                        var thisPathRecord = new TextureBinPathRecord { type = BitConverter.ToInt32(bytesRead, curByte) };
                        curByte += 4;
                        thisPathRecord.size = BitConverter.ToSingle(bytesRead, curByte);
                        curByte += 4;
                        thisPathRecord.x1 = BitConverter.ToSingle(bytesRead, curByte);
                        curByte += 4;
                        thisPathRecord.y1 = BitConverter.ToSingle(bytesRead, curByte);
                        curByte += 4;
                        thisPathRecord.x2 = BitConverter.ToSingle(bytesRead, curByte);
                        curByte += 4;
                        thisPathRecord.y2 = BitConverter.ToSingle(bytesRead, curByte);
                        curByte += 4;
                        tileRecord.pathRecords[k] = thisPathRecord;
                    }
                    thisSetRecord.tileRecords[i] = tileRecord;
                }
                textureBinFileInfo.setRecords[h] = thisSetRecord;
            }
            return textureBinFileInfo;
        }

    }
}
