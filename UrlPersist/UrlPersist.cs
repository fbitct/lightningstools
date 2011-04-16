using System.IO;
using ADODB;
using CDO;
using Stream = System.IO.Stream;

namespace UrlPersist
{
    public static class UrlPersist
    {
        private const int BUFFER_SIZE = 64*1024;

        public static bool SaveToFile(string url, string fileName)
        {
            bool toReturn = false;
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                toReturn = SaveToStream(url, fs);
                fs.Flush();
                fs.Close();
            }
            return toReturn;
        }

        public static bool SaveToStream(string url, Stream outputStream)
        {
            bool result = false;
            Message msg = new MessageClass();
            ADODB.Stream stm = null;
            try
            {
                msg.MimeFormatted = true;
                msg.CreateMHTMLBody(url, CdoMHTMLFlags.cdoSuppressNone, "", "");
                stm = msg.GetStream();
                var sw = new StreamWriter(outputStream);
                sw.AutoFlush = true;
                while (!stm.EOS)
                {
                    sw.Write(stm.ReadText(BUFFER_SIZE));
                }
                result = true;
            }
            finally
            {
                msg = null;
                if (stm != null && IsStreamOpen(stm))
                {
                    stm.Close();
                }
                stm = null;
            }
            return result;
        }

        private static bool IsStreamOpen(ADODB.Stream stm)
        {
            return ((stm.State & ObjectStateEnum.adStateOpen) == ObjectStateEnum.adStateOpen);
        }
    }
}