using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using F4Utils.Process;
using F4Utils.Terrain.Structs;

namespace F4Utils.Terrain
{
    public interface ITerrainDBFactory
    {
        TerrainDB Create(bool loadAllLods);
    }
    public class TerrainDBFactory : ITerrainDBFactory
    {
        public TerrainDB Create(bool loadAllLods)
        {
            var falconExePath = GetFalconExePath();
            if (falconExePath == null) return null;
            return new TerrainDB(falconExePath, loadAllLods);
        }

        private string GetFalconExePath()
        {
            //TODO: check these against other theaters, for correct way to read theater installation locations
            var exePath = Util.GetFalconExePath();
            if (exePath == null) return null;
            var f4BasePathFI = new FileInfo(exePath);
            return f4BasePathFI.DirectoryName + Path.DirectorySeparatorChar;
        }
    }
}
