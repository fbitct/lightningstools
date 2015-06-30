using F4Utils.Campaign.F4Structs;
using System.IO;

namespace F4Utils.Campaign.Save
{
    public class FmapFile
    {
        public int XCELLS { get; private set; }
        public int YCELLS { get; private set; }
        public FmapWeatherCell[,] CellArray { get; private set; }

        public FmapFile(string fileName)
        {
            LoadFmapFile(fileName);
        }

        private void LoadFmapFile(string fileName)
        {
            //reads FMAP file
            using (var stream = new FileStream(fileName, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                XCELLS = reader.ReadInt32();
                YCELLS = reader.ReadInt32();
                CellArray = new FmapWeatherCell[XCELLS,YCELLS];

                for (int i = (XCELLS-1); i>-1 ;i--)
			    {
					    for (int j =0 ; j < YCELLS ;j++)
					    {
						    CellArray[i,j].BasicCondition =reader.ReadInt32();
					    }
			    }

			    for (int i = (XCELLS-1); i>-1 ;i--)
			    {
					    for (int j =0 ; j < YCELLS ;j++)
					    {
                            CellArray[i, j].Pressure = reader.ReadSingle();
					    }
			    }

			    for (int i = (XCELLS-1); i>-1 ;i--)
			    {
					    for (int j =0 ; j < YCELLS ;j++)
					    {
                            CellArray[i, j].Temperature = reader.ReadSingle();
					    }
			    }

			    for (int i = (XCELLS-1); i>-1 ;i--)
			    {
					    for (int j =0 ; j < YCELLS ;j++)
					    {
                            CellArray[i, j].WindSpeed = reader.ReadSingle();
					    }
			    }
			    for (int i = (XCELLS-1); i>-1 ;i--)
			    {
					    for (int j =0 ; j < YCELLS ;j++)
					    {
                            CellArray[i, j].WindHeading = reader.ReadSingle();
					    }
			    }
            }
        }
    }
}
