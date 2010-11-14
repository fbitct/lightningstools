using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections;

namespace Common.Compression.Zip
{
    public static class Util
    {
        public static Dictionary<string, ZipEntry> GetZipFileEntries(ZipFile zipFile)
        {
            Dictionary<string, ZipEntry> toReturn = new Dictionary<string, ZipEntry>();
            IEnumerator zipEntries = zipFile.GetEnumerator();
            while (zipEntries.MoveNext()) 
            {
                ZipEntry thisEntry = (ZipEntry)zipEntries.Current;
                toReturn.Add(thisEntry.Name, thisEntry);
            }
            return toReturn;
        }
    }
}
