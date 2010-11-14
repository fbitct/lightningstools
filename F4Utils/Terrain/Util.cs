using System;
using System.Collections.Generic;

using System.Text;
using F4Utils.Terrain.Structs;
using System.IO;
using System.Drawing;

namespace F4Utils.Terrain
{
    public static class Util
    {
        public static TheaterDotMapFileInfo ReadTheaterDotMapFile(string theaterDotMapFilePath)
        {
            if (String.IsNullOrEmpty(theaterDotMapFilePath)) throw new ArgumentNullException("theaterMapFilePath");
            FileInfo fileInfo = new FileInfo(theaterDotMapFilePath);
            if (!fileInfo.Exists) throw new FileNotFoundException(theaterDotMapFilePath);

            TheaterDotMapFileInfo mapInfo = new TheaterDotMapFileInfo();
            mapInfo.Pallete = new Color[256];
            mapInfo.GreenPallete = new Color[256];
            int bytesToRead = (int)fileInfo.Length;
            byte[] bytesRead = new byte[bytesToRead];

            using (FileStream stream = File.OpenRead(theaterDotMapFilePath))
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

            UInt32[] pallete = new UInt32[256];
            for (int i = 0; i < pallete.Length; i++)
            {
                pallete[i] = BitConverter.ToUInt32(bytesRead, 28 + (i * 4));
            }
            for (int i = 0; i < mapInfo.Pallete.Length; i++)
            {

                byte cR = (byte)(pallete[i] & 0xFF);
                byte cG = (byte)((pallete[i] >> 8) & 0xFF);
                byte cB = (byte)((pallete[i] >> 16) & 0xFF);
                byte cA = (byte)((pallete[i] >> 24) & 0xFF);
                Color thisColor = Color.FromArgb(cA, cR, cG, cB);
                mapInfo.Pallete[i] = thisColor;

                byte gR = (byte)0x00;
                byte gG = (byte)(((float)thisColor.R * 0.25f) + ((float)thisColor.G * 0.5f) + ((float)thisColor.B * 0.25f));
                byte gB = (byte)0x00;
                byte gA = (byte)((pallete[i] >> 24) & 0xFF);
                Color greenColor = Color.FromArgb(gA, gR, gG, gB);
                mapInfo.GreenPallete[i] = greenColor;
            }
            mapInfo.LODMapWidths = new UInt32[mapInfo.NumLODs];
            mapInfo.LODMapHeights = new UInt32[mapInfo.NumLODs];
            for (int i = 0; i < mapInfo.NumLODs; i++)
            {
                mapInfo.LODMapWidths[i] = BitConverter.ToUInt32(bytesRead, 1052 + (i * 8));
                mapInfo.LODMapHeights[i] = BitConverter.ToUInt32(bytesRead, 1056 + (i * 8));
            }
            mapInfo.flags = 0;
            mapInfo.baseLong = 119.1148778F;
            mapInfo.baseLat = 33.775918333F;
            if (bytesRead.Length >= (1056 + ((mapInfo.NumLODs - 1) * 8) + 4))
            {
                mapInfo.flags = BitConverter.ToUInt32(bytesRead, (int)(1056 + ((mapInfo.NumLODs - 1) * 8) + 4));
            }
            if (bytesRead.Length >= (1056 + ((mapInfo.NumLODs - 1) * 8) + 8))
            {
                mapInfo.baseLong = BitConverter.ToSingle(bytesRead, (int)(1056 + ((mapInfo.NumLODs - 1) * 8) + 8));
            }
            if (bytesRead.Length >= (1056 + ((mapInfo.NumLODs - 1) * 8) + 12))
            {
                mapInfo.baseLat = BitConverter.ToSingle(bytesRead, (int)(1056 + ((mapInfo.NumLODs - 1) * 8) + 12));
            }
            return mapInfo;
        }
        public static TextureDotBinFileInfo ReadTextureDotBinFile(string textureDotBinFilePath)
        {
            if (String.IsNullOrEmpty(textureDotBinFilePath)) throw new ArgumentNullException("textureBinFilePath");
            FileInfo fileInfo = new FileInfo(textureDotBinFilePath);
            if (!fileInfo.Exists) throw new FileNotFoundException(textureDotBinFilePath);

            int bytesToRead = (int)fileInfo.Length;
            byte[] bytesRead = new byte[bytesToRead];

            using (FileStream stream = File.OpenRead(textureDotBinFilePath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(bytesRead, 0, bytesToRead);
                stream.Close();
            }
            TextureDotBinFileInfo textureBinFileInfo = new TextureDotBinFileInfo();
            int curByte = 0;

            textureBinFileInfo.numSets = BitConverter.ToUInt32(bytesRead, curByte);
            textureBinFileInfo.setRecords = new TextureBinSetRecord[textureBinFileInfo.numSets];
            curByte += 4;
            textureBinFileInfo.totalTiles = BitConverter.ToUInt32(bytesRead, curByte);
            curByte += 4;

            for (int h = 0; h < textureBinFileInfo.numSets; h++)
            {
                TextureBinSetRecord thisSetRecord = new TextureBinSetRecord();

                thisSetRecord.numTiles = BitConverter.ToUInt32(bytesRead, curByte);
                curByte += 4;
                thisSetRecord.terrainType = bytesRead[curByte];
                curByte++;

                thisSetRecord.tileRecords = new TextureBinTileRecord[thisSetRecord.numTiles];
                for (int i = 0; i < thisSetRecord.numTiles; i++)
                {
                    TextureBinTileRecord tileRecord = new TextureBinTileRecord();
                    tileRecord.tileName = Encoding.ASCII.GetString(bytesRead, curByte, 20);
                    int nullLoc = tileRecord.tileName.IndexOf('\0');
                    if (nullLoc > 0) tileRecord.tileName = tileRecord.tileName.Substring(0, nullLoc);
                    curByte += 20;
                    tileRecord.numAreas = BitConverter.ToUInt32(bytesRead, curByte);
                    tileRecord.areaRecords = new TextureBinAreaRecord[tileRecord.numAreas];
                    curByte += 4;
                    tileRecord.numPaths = BitConverter.ToUInt32(bytesRead, curByte);
                    tileRecord.pathRecords = new TextureBinPathRecord[tileRecord.numPaths];
                    curByte += 4;
                    for (int j = 0; j < tileRecord.numAreas; j++)
                    {
                        TextureBinAreaRecord thisAreaRecord = new TextureBinAreaRecord();
                        thisAreaRecord.type = BitConverter.ToInt32(bytesRead, curByte);
                        curByte += 4;
                        thisAreaRecord.size = BitConverter.ToSingle(bytesRead, curByte);
                        curByte += 4;
                        thisAreaRecord.x = BitConverter.ToSingle(bytesRead, curByte);
                        curByte += 4;
                        thisAreaRecord.y = BitConverter.ToSingle(bytesRead, curByte);
                        curByte += 4;
                        tileRecord.areaRecords[j] = thisAreaRecord;
                    }
                    for (int k = 0; k < tileRecord.numPaths; k++)
                    {
                        TextureBinPathRecord thisPathRecord = new TextureBinPathRecord();
                        thisPathRecord.type = BitConverter.ToInt32(bytesRead, curByte);
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
            FileInfo fileInfo = new FileInfo(farTilesDotPalFilePath);
            if (!fileInfo.Exists) throw new FileNotFoundException(farTilesDotPalFilePath);

            int bytesToRead = (int)fileInfo.Length;
            byte[] bytesRead = new byte[bytesToRead];

            using (FileStream stream = File.OpenRead(farTilesDotPalFilePath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(bytesRead, 0, bytesToRead);
                stream.Close();
            }
            Color[] pallete = new Color[256];
            int curByte = 0;
            for (int i = 0; i < pallete.Length; i++)
            {
                uint thisPalleteEntry = BitConverter.ToUInt32(bytesRead, curByte);

                byte cR = (byte)(thisPalleteEntry & 0xFF);
                byte cG = (byte)((thisPalleteEntry >> 8) & 0xFF);
                byte cB = (byte)((thisPalleteEntry >> 16) & 0xFF);
                //byte cA = (byte)((thisPalleteEntry >> 24) & 0xFF);
                byte cA = (byte)0xFF;
                Color thisColor = Color.FromArgb(cA, cR, cG, cB);
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
            FarTilesDotPalFileInfo toReturn = new FarTilesDotPalFileInfo();
            toReturn.numTextures = texCount;
            toReturn.pallete = pallete;
            return toReturn;
        }
        public static TheaterDotLxFileInfo LoadTheaterDotLxFile(uint lodLevel, string theaterDotMapFilePath)
        {
            if (String.IsNullOrEmpty(theaterDotMapFilePath)) throw new ArgumentNullException("theaterMapFilePath");
            FileInfo lFileInfo = new FileInfo(Path.GetDirectoryName(theaterDotMapFilePath) + Path.DirectorySeparatorChar + "theater.L" + lodLevel);
            FileInfo oFileInfo = new FileInfo(Path.GetDirectoryName(theaterDotMapFilePath) + Path.DirectorySeparatorChar + "theater.O" + lodLevel);

            TheaterDotLxFileInfo toReturn = new TheaterDotLxFileInfo();
            toReturn.MinElevation = UInt16.MaxValue;
            toReturn.MaxElevation = 0;

            toReturn.LoDLevel = (uint)lodLevel;
            long bytesToRead = oFileInfo.Length;
            byte[] bytesRead = new byte[bytesToRead];
            using (FileStream stream = File.OpenRead(oFileInfo.FullName))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(bytesRead, 0, (int)bytesToRead);
                stream.Close();
            }
            bool isFourByte = true;

            List<TheaterDotOxFileRecord> oFileRecords = new List<TheaterDotOxFileRecord>();
            int curByte = 0;
            while (curByte < bytesToRead)
            {
                UInt32 thisDword = BitConverter.ToUInt32(bytesRead, curByte);
                if (thisDword % 2304 != 0)
                {
                    //not a 4-byte file
                    isFourByte = false;
                    break;
                }
                else
                {
                    TheaterDotOxFileRecord record = new TheaterDotOxFileRecord();
                    record.LRecordStartingOffset = thisDword;
                    oFileRecords.Add(record);
                }
                curByte += 4;
            }

            curByte = 0;
            if (!isFourByte)
            {
                oFileRecords.Clear();
                while (curByte < bytesToRead)
                {
                    UInt32 thisWord = BitConverter.ToUInt16(bytesRead, curByte);
                    TheaterDotOxFileRecord record = new TheaterDotOxFileRecord();
                    record.LRecordStartingOffset = thisWord;
                    oFileRecords.Add(record);
                    curByte += 2;
                }
            }

            curByte = 0;
            bytesToRead = lFileInfo.Length;
            bytesRead = new byte[bytesToRead];
            using (FileStream stream = File.OpenRead(lFileInfo.FullName))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(bytesRead, 0, (int)bytesToRead);
                stream.Close();
            }
            uint maxTextureOffset = 0;
            uint minTextureOffset = uint.MaxValue;
            List<TheaterDotLxFileRecord> lFileRecords = new List<TheaterDotLxFileRecord>();
            if (isFourByte)
            {
                toReturn.LRecordSizeBytes = 9;
                while (curByte < bytesToRead)
                {
                    TheaterDotLxFileRecord record = new TheaterDotLxFileRecord();
                    record.TextureId = BitConverter.ToUInt32(bytesRead, curByte);
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
                    TheaterDotLxFileRecord record = new TheaterDotLxFileRecord();
                    record.TextureId = BitConverter.ToUInt16(bytesRead, curByte);
                    if (record.TextureId > maxTextureOffset) maxTextureOffset = record.TextureId;
                    if (record.TextureId < minTextureOffset) minTextureOffset = record.TextureId;
                    record.Elevation = BitConverter.ToUInt16(bytesRead, curByte + 2); ;
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
            System.GC.Collect();
            return toReturn;

        }
    }
}
