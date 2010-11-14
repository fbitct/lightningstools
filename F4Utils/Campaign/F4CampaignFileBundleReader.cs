using System;
using System.Text;
using System.IO;
namespace F4Utils.Campaign
{
    public struct EmbeddedFileInfo
    {
        public string FileName;
        public uint FileOffset;
        public uint FileSizeBytes;
    }

    public class F4CampaignFileBundleReader
    {
        protected EmbeddedFileInfo[] _embeddedFileDirectory;
        protected byte[] _rawBytes;

        public F4CampaignFileBundleReader()
            : base()
        {
        }
        public F4CampaignFileBundleReader(string campaignFileBundleFileName)
            : this()
        {
            Load(campaignFileBundleFileName);
        }
        public void Load(string campaignFileBundleFileName)
        {
            FileInfo fi = new FileInfo(campaignFileBundleFileName);
            if (!fi.Exists) throw new FileNotFoundException(campaignFileBundleFileName);
            _rawBytes = new byte[fi.Length];
            using (FileStream fs = new FileStream(campaignFileBundleFileName, FileMode.Open))
            {
                fs.Seek(0, SeekOrigin.Begin);
                fs.Read(_rawBytes, 0, (int)fi.Length);
            }
            uint directoryStartOffset = BitConverter.ToUInt32(_rawBytes, 0);
            uint numEmbeddedFiles = BitConverter.ToUInt32(_rawBytes, (int)directoryStartOffset);
            _embeddedFileDirectory = new EmbeddedFileInfo[numEmbeddedFiles];
            int curLoc = (int)directoryStartOffset + 4;
            for (int i = 0; i < numEmbeddedFiles; i++)
            {
                EmbeddedFileInfo thisFileResourceInfo = new EmbeddedFileInfo();
                byte thisFileNameLength = (byte)(_rawBytes[curLoc] & 0xFF);
                curLoc++;
                string thisFileName = Encoding.ASCII.GetString(_rawBytes, curLoc, thisFileNameLength);
                thisFileResourceInfo.FileName = thisFileName;
                curLoc += thisFileNameLength;
                thisFileResourceInfo.FileOffset = BitConverter.ToUInt32(_rawBytes, curLoc);
                curLoc += 4;
                thisFileResourceInfo.FileSizeBytes = BitConverter.ToUInt32(_rawBytes, curLoc);
                curLoc += 4;
                _embeddedFileDirectory[i] = thisFileResourceInfo;
            }
        }
        public EmbeddedFileInfo[] GetEmbeddedFileDirectory()
        {
            if (_embeddedFileDirectory == null || _rawBytes == null || _rawBytes.Length == 0) throw new InvalidOperationException("Campaign bundle file not loaded yet.");
            return _embeddedFileDirectory;
        }
        public byte[] GetEmbeddedFileContents(string embeddedFileName)
        {
            if (_embeddedFileDirectory == null || _rawBytes == null || _rawBytes.Length == 0) throw new InvalidOperationException("Campaign bundle file not loaded yet.");
            for (int i = 0; i < _embeddedFileDirectory.Length; i++)
            {
                EmbeddedFileInfo thisFile = _embeddedFileDirectory[i];
                if (thisFile.FileName.ToLowerInvariant() == embeddedFileName.ToLowerInvariant())
                {
                    byte[] toReturn = new byte[thisFile.FileSizeBytes];
                    Array.Copy(_rawBytes, thisFile.FileOffset, toReturn, 0, thisFile.FileSizeBytes);
                    return toReturn;
                }
            }
            throw new FileNotFoundException(embeddedFileName);
        }

    }
}