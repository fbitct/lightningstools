using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using F4Utils.Terrain.Structs;

namespace F4Utils.Terrain
{
    public static class Util
    {
        

        public static TextureDotBinFileInfo ReadTextureDotBinFile(string textureDotBinFilePath)
        {
            if (String.IsNullOrEmpty(textureDotBinFilePath)) throw new ArgumentNullException("textureBinFilePath");
            var fileInfo = new FileInfo(textureDotBinFilePath);
            if (!fileInfo.Exists) throw new FileNotFoundException(textureDotBinFilePath);

            var bytesToRead = (int) fileInfo.Length;
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
                        var thisAreaRecord = new TextureBinAreaRecord {type = BitConverter.ToInt32(bytesRead, curByte)};
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
                        var thisPathRecord = new TextureBinPathRecord {type = BitConverter.ToInt32(bytesRead, curByte)};
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

        public static FarTilesDotPalFileInfo ReadFarTilesDotPalFile(string farTilesDotPalFilePath)
        {
            if (String.IsNullOrEmpty(farTilesDotPalFilePath)) throw new ArgumentNullException("farTilesPalletePath");
            var fileInfo = new FileInfo(farTilesDotPalFilePath);
            if (!fileInfo.Exists) throw new FileNotFoundException(farTilesDotPalFilePath);

            var bytesToRead = (int) fileInfo.Length;
            var bytesRead = new byte[bytesToRead];

            using (var stream = File.OpenRead(farTilesDotPalFilePath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(bytesRead, 0, bytesToRead);
                stream.Close();
            }
            var pallete = new Color[256];
            var curByte = 0;
            for (var i = 0; i < pallete.Length; i++)
            {
                var thisPalleteEntry = BitConverter.ToUInt32(bytesRead, curByte);

                var cR = (byte) (thisPalleteEntry & 0xFF);
                var cG = (byte) ((thisPalleteEntry >> 8) & 0xFF);
                var cB = (byte) ((thisPalleteEntry >> 16) & 0xFF);
                //byte cA = (byte)((thisPalleteEntry >> 24) & 0xFF);
                const byte cA = 0xFF;
                var thisColor = Color.FromArgb(cA, cR, cG, cB);
                pallete[i] = thisColor;

                curByte += 4;
            }
            uint texCount = 0;
            uint tilesAtLod = 0;
            do
            {
                texCount += tilesAtLod;
                tilesAtLod = BitConverter.ToUInt32(bytesRead, curByte);
                curByte += 4;
            } while (curByte < bytesToRead);
            var toReturn = new FarTilesDotPalFileInfo {numTextures = texCount, pallete = pallete};
            return toReturn;
        }

        public static TheaterDotLxFileInfo LoadTheaterDotLxFile(uint lodLevel, string theaterDotMapFilePath)
        {
            if (String.IsNullOrEmpty(theaterDotMapFilePath)) throw new ArgumentNullException("theaterMapFilePath");
            var lFileInfo =
                new FileInfo(Path.GetDirectoryName(theaterDotMapFilePath) + Path.DirectorySeparatorChar + "theater.L" +
                             lodLevel);
            var oFileInfo =
                new FileInfo(Path.GetDirectoryName(theaterDotMapFilePath) + Path.DirectorySeparatorChar + "theater.O" +
                             lodLevel);

            var toReturn = new TheaterDotLxFileInfo
                               {
                                   MinElevation = UInt16.MaxValue,
                                   MaxElevation = 0,
                                   LoDLevel = lodLevel
                               };

            var bytesToRead = oFileInfo.Length;
            var bytesRead = new byte[bytesToRead];
            using (var stream = File.OpenRead(oFileInfo.FullName))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(bytesRead, 0, (int) bytesToRead);
                stream.Close();
            }
            var isFourByte = true;

            var oFileRecords = new List<TheaterDotOxFileRecord>();
            var curByte = 0;
            while (curByte < bytesToRead)
            {
                var thisDword = BitConverter.ToUInt32(bytesRead, curByte);
                if (thisDword%2304 != 0)
                {
                    //not a 4-byte file
                    isFourByte = false;
                    break;
                }
                var record = new TheaterDotOxFileRecord {LRecordStartingOffset = thisDword};
                oFileRecords.Add(record);
                curByte += 4;
            }

            curByte = 0;
            if (!isFourByte)
            {
                oFileRecords.Clear();
                while (curByte < bytesToRead)
                {
                    UInt32 thisWord = BitConverter.ToUInt16(bytesRead, curByte);
                    var record = new TheaterDotOxFileRecord {LRecordStartingOffset = thisWord};
                    oFileRecords.Add(record);
                    curByte += 2;
                }
            }

            curByte = 0;
            bytesToRead = lFileInfo.Length;
            bytesRead = new byte[bytesToRead];
            using (var stream = File.OpenRead(lFileInfo.FullName))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(bytesRead, 0, (int) bytesToRead);
                stream.Close();
            }
            uint maxTextureOffset = 0;
            var minTextureOffset = uint.MaxValue;
            var lFileRecords = new List<TheaterDotLxFileRecord>();
            if (isFourByte)
            {
                toReturn.LRecordSizeBytes = 9;
                while (curByte < bytesToRead)
                {
                    var record = new TheaterDotLxFileRecord
                                     {
                                         TextureId = BitConverter.ToUInt32(bytesRead, curByte)
                                     };
                    if (record.TextureId > maxTextureOffset) maxTextureOffset = record.TextureId;
                    if (record.TextureId < minTextureOffset) minTextureOffset = record.TextureId;
                    record.Elevation = BitConverter.ToUInt16(bytesRead, curByte + 4);
                    if (record.Elevation < toReturn.MinElevation) toReturn.MinElevation = record.Elevation;
                    if (record.Elevation > toReturn.MaxElevation) toReturn.MaxElevation = record.Elevation;
                    record.Pallete = bytesRead[curByte + 6];
                    record.X1 = bytesRead[curByte + 7];
                    record.X2 = bytesRead[curByte + 8];
                    lFileRecords.Add(record);
                    curByte += 9;
                }
            }
            else
            {
                toReturn.LRecordSizeBytes = 7;
                while (curByte < bytesToRead)
                {
                    var record = new TheaterDotLxFileRecord
                                     {
                                         TextureId = BitConverter.ToUInt16(bytesRead, curByte)
                                     };
                    if (record.TextureId > maxTextureOffset) maxTextureOffset = record.TextureId;
                    if (record.TextureId < minTextureOffset) minTextureOffset = record.TextureId;
                    record.Elevation = BitConverter.ToUInt16(bytesRead, curByte + 2);
                    if (record.Elevation < toReturn.MinElevation) toReturn.MinElevation = record.Elevation;
                    if (record.Elevation > toReturn.MaxElevation) toReturn.MaxElevation = record.Elevation;
                    record.Pallete = bytesRead[curByte + 4];
                    record.X1 = bytesRead[curByte + 5];
                    record.X2 = bytesRead[curByte + 6];
                    lFileRecords.Add(record);
                    curByte += 7;
                }
            }

            toReturn.minTexOffset = minTextureOffset;
            toReturn.maxTexOffset = maxTextureOffset;
            toReturn.O = oFileRecords.ToArray();
            toReturn.L = lFileRecords.ToArray();
            oFileRecords.Clear();
            lFileRecords.Clear();
            GC.Collect();
            return toReturn;
        }

        public static List<string> Tokenize(string thisLine)
        {
            throw new NotImplementedException();
        }
    }
}