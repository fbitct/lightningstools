using F4Utils.Terrain.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrain
{
    internal interface IDistanceBetweenElevationPostsCalculator
    {
        float GetNumFeetBetweenElevationPosts(int lod, TheaterDotLxFileInfo[] theaterDotLxFiles, TheaterDotMapFileInfo theaterDotMapFileInfo);
    }
    class DistanceBetweenElevationPostsCalculator:IDistanceBetweenElevationPostsCalculator
    {
        public float GetNumFeetBetweenElevationPosts(int lod, TheaterDotLxFileInfo[] theaterDotLxFiles, TheaterDotMapFileInfo theaterDotMapFileInfo)
        {
            
            if (theaterDotLxFiles == null) return 0;
            var lodInfo = theaterDotLxFiles[0];
            var mapInfo = theaterDotMapFileInfo;
            var feetBetweenPosts = mapInfo.FeetBetweenL0Posts;
            for (var i = 1; i <= lodInfo.LoDLevel; i++)
            {
                feetBetweenPosts *= 2;
            }
            return feetBetweenPosts;
        }

    }
}
