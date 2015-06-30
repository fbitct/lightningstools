using F4Utils.Campaign.F4Structs;
using System;
using System.IO;
using System.Text;

namespace F4Utils.Campaign.Save
{
    public class PriFile
    {
        public byte[,] DefaultObjtypePriority=new byte[(int)AirTacticTypeEnum.TAT_CAS,TeamConstants.MAX_TGTTYPE];		// AI's suggested settings
        public byte[,] DefaultUnittypePriority = new byte[(int)AirTacticTypeEnum.TAT_CAS, TeamConstants.MAX_UNITTYPE];		
        public byte[,] DefaultMissionPriority = new byte[(int)AirTacticTypeEnum.TAT_CAS, (int)MissionTypeEnum.AMIS_OTHER];		

        public PriFile(string fileName)
        {
            LoadPriFile(fileName);
        }

        private void LoadPriFile(string fileName)
        {
            int n=0;
            int t = 0;
            //reads PRI file
            using (var stream = new FileStream(fileName, FileMode.Open))
            using (var reader = new StreamReader(stream))
            {

                n = ParseInt(GetNextToken(reader));		// # of objective target priorities
                while (n >= 0)
                {
                    DefaultObjtypePriority[t,n] = (byte)ParseInt(GetNextToken(reader));
                    n = ParseInt(GetNextToken(reader));
                }
                n = ParseInt(GetNextToken(reader));		// # of unit target priorities
                while (n >= 0)
                {
                    DefaultUnittypePriority[t, n] = (byte)ParseInt(GetNextToken(reader));
                    n = ParseInt(GetNextToken(reader));
                }
                n = ParseInt(GetNextToken(reader));		// # of mission priorities
                while (n >= 0)
                {
                    DefaultMissionPriority[t, n] = (byte)ParseInt(GetNextToken(reader));
                    n = ParseInt(GetNextToken(reader));
                }

            }
        }
        private int ParseInt(string a)
        {
            int toReturn = 0;
            Int32.TryParse(a.Trim(), out toReturn);
            return toReturn;
        }
        private string GetNextToken(StreamReader reader)
        {
            string line=string.Empty;
            var isComment = false;
            while (isComment)
            {
                try
                {
                    line = ReadString(reader,160);
                    isComment = (line.StartsWith(";") || line.StartsWith("#"));
                }
                catch (IOException) { }
            }
            return line;
            
        }
        private string ReadString(StreamReader reader, int maxLength)
        {
            var readBuffer = new char[1];
            var sb = new StringBuilder(maxLength);
            bool hasReadNonWhitespaceChar = false;
            for (var i=0;i<maxLength;i++) 
            {
                var read = reader.Read(readBuffer, 0,1);
                if (read > 0)
                {
                    if (!char.IsWhiteSpace(readBuffer[0]))
                    {
                        sb.Append(readBuffer, 0, 1);
                    }
                    else if (hasReadNonWhitespaceChar)
                    {
                        break;
                    }
                }
            }
            return sb.ToString();
        }


    }
}
