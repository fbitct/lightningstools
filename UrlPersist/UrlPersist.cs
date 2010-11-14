using System.IO;


namespace UrlPersist
{
    public static class UrlPersist
    {
        private const int BUFFER_SIZE = 64*1024;

        public static bool SaveToFile(string url, string fileName)
        {
            bool toReturn = false;
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
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
            CDO.Message msg = new CDO.MessageClass();
            ADODB.Stream stm = null;
            try
            {
                msg.MimeFormatted = true;
                msg.CreateMHTMLBody(url, CDO.CdoMHTMLFlags.cdoSuppressNone, "", "");
                stm = msg.GetStream();
                StreamWriter sw = new StreamWriter(outputStream);
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
            return ((stm.State & ADODB.ObjectStateEnum.adStateOpen) == ADODB.ObjectStateEnum.adStateOpen);
        }

    }
}