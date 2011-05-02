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
        public static TheaterDotMapFileInfo ReadTheaterDotMapFile(string theaterDotMapFilePath)
        {
            if (String.IsNullOrEmpty(theaterDotMapFilePath)) throw new ArgumentNullException("theaterMapFilePath");
            var fileInfo = new FileInfo(theaterDotMapFilePath);
            if (!fileInfo.Exists) throw new FileNotFoundException(theaterDotMapFilePath);

            var mapInfo = new TheaterDotMapFileInfo {Pallete = new Color[256], GreenPallete = new Color[256]};
            var bytesToRead = (int) fileInfo.Length;
            var bytesRead = new byte[bytesToRead];

            using (var stream = File.OpenRead(theaterDotMapFilePath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(bytesRead, 0, bytesToRead);
                stream.Close();
            }
            mapInfo.FeetBetweenL0Posts = BitConverter.ToSingle(bytesRead, 0);
            mapInfo.MEAMapWidth = BitConverter.ToUInt32(bytesRead, 4);
            mapInfo.MEAMapHeight = BitConverter.ToUInt32(bytesRead, 8);
            mapInfo.FeetToMeaCellConversionFactor = BitConverter.ToSingle(bytesRead, 12);
            mapInfo.NumLODs = BitConverter.ToUInt32(bytesRead, 16);
            mapInfo.LastNearTiledLOD = BitConverter.ToUInt32(bytesRead, 20);
            mapInfo.LastFarTiledLOD = BitConverter.ToUInt32(bytesRead, 24);

            var pallete = new UInt32[256];
            for (var i = 0; i < pallete.Length; i++)
            {
                pallete[i] = BitConverter.ToUInt32(bytesRead, 28 + (i*4));
            }
            for (var i = 0; i < mapInfo.Pallete.Length; i++)
            {
                var cR = (byte) (pallete[i] & 0xFF);
                var cG = (byte) ((pallete[i] >> 8) & 0xFF);
                var cB = (byte) ((pallete[i] >> 16) & 0xFF);
                var cA = (byte) ((pallete[i] >> 24) & 0xFF);
                var thisColor = Color.FromArgb(cA, cR, cG, cB);
                mapInfo.Pallete[i] = thisColor;

                const byte gR = 0x00;
                var gG = (byte) ((thisColor.R*0.25f) + (thisColor.G*0.5f) + (thisColor.B*0.25f));
                const byte gB = 0x00;
                var gA = (byte) ((pallete[i] >> 24) & 0xFF);
                var greenColor = Color.FromArgb(gA, gR, gG, gB);
                mapInfo.GreenPallete[i] = greenColor;
            }
            mapInfo.LODMapWidths = new UInt32[mapInfo.NumLODs];
            mapInfo.LODMapHeights = new UInt32[mapInfo.NumLODs];
            for (var i = 0; i < mapInfo.NumLODs; i++)
            {
                mapInfo.LODMapWidths[i] = BitConverter.ToUInt32(bytesRead, 1052 + (i*8));
                mapInfo.LODMapHeights[i] = BitConverter.ToUInt32(bytesRead, 1056 + (i*8));
            }
            mapInfo.flags = 0;
            mapInfo.baseLong = 119.1148778F;
            mapInfo.baseLat = 33.775918333F;
            if (bytesRead.Length >= (1056 + ((mapInfo.NumLODs - 1)*8) + 4))
            {
                mapInfo.flags = BitConverter.ToUInt32(bytesRead, (int) (1056 + ((mapInfo.NumLODs - 1)*8) + 4));
            }
            if (bytesRead.Length >= (1056 + ((mapInfo.NumLODs - 1)*8) + 8))
            {
                mapInfo.baseLong = BitConverter.ToSingle(bytesRead, (int) (1056 + ((mapInfo.NumLODs - 1)*8) + 8));
            }
            if (bytesRead.Length >= (1056 + ((mapInfo.NumLODs - 1)*8) + 12))
            {
                mapInfo.baseLat = BitConverter.ToSingle(bytesRead, (int) (1056 + ((mapInfo.NumLODs - 1)*8) + 12));
            }
            return mapInfo;
        }

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