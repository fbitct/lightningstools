using F4Utils.Terrain;
using F4Utils.Terrain.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrain
{
    internal interface ITheaterMapBuilder
    {
        Bitmap GetTheaterMap(uint lod, TheaterDotLxFileInfo[] theaterDotLxFiles, TheaterDotMapFileInfo theaterDotMapFileInfo);
    }
    class TheaterMapBuilder:ITheaterMapBuilder
    {
        public unsafe Bitmap GetTheaterMap(uint lod, TheaterDotLxFileInfo[] theaterDotLxFiles, TheaterDotMapFileInfo theaterDotMapFileInfo)
        {
            var lodInfo = theaterDotLxFiles[lod];
            var mapInfo = theaterDotMapFileInfo;
            const int postsAcross = Constants.NUM_ELEVATION_POSTS_ACROSS_SINGLE_LOD_SEGMENT;
            var bmp = new Bitmap((int)mapInfo.LODMapWidths[lodInfo.LoDLevel] * postsAcross,
                                 (int)mapInfo.LODMapHeights[lodInfo.LoDLevel] * postsAcross,
                                 PixelFormat.Format8bppIndexed);
            TheaterDotOxFileRecord block;
            TheaterDotLxFileRecord lRecord;
            var palette = bmp.Palette;
            for (var i = 0; i < 256; i++)
            {
                palette.Entries[i] = mapInfo.Pallete[i];
            }
            bmp.Palette = palette;

            var bmpLock = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly,
                                       bmp.PixelFormat);
            var scan0 = bmpLock.Scan0;
            var startPtr = scan0.ToPointer();
            var height = bmp.Height;
            var width = bmp.Width;
            for (var blockRow = 0; blockRow < ((int)mapInfo.LODMapHeights[lodInfo.LoDLevel]); blockRow++)
            {
                for (var blockCol = 0; blockCol < (mapInfo.LODMapWidths[lodInfo.LoDLevel]); blockCol++)
                {
                    var oIndex = (int)(blockRow * mapInfo.LODMapHeights[lodInfo.LoDLevel]) + blockCol;
                    block = lodInfo.O[oIndex];
                    for (var postRow = 0; postRow < postsAcross; postRow++)
                    {
                        for (var postCol = 0; postCol < postsAcross; postCol++)
                        {
                            var lIndex =
                                (int)
                                (((block.LRecordStartingOffset / (lodInfo.LRecordSizeBytes * postsAcross * postsAcross)) *
                                  postsAcross * postsAcross) + ((postRow * postsAcross) + postCol));
                            lRecord = lodInfo.L[lIndex];
                            var xCoord = (blockCol * postsAcross) + postCol;
                            var yCoord = height - 1 - (blockRow * postsAcross) - postRow;
                            *((byte*)startPtr + (yCoord * width) + xCoord) = lRecord.Pallete;
                        }
                    }
                }
            }
            bmp.UnlockBits(bmpLock);
            return bmp;
        }
    }
}
