using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Common.Win32.Paths
{
    public static class Util
    {
        public static string GetShortPathName(string path)
        {
            StringBuilder sbShortPath = new StringBuilder(NativeMethods.MAX_PATH);
            NativeMethods.GetShortPathName(path, sbShortPath, NativeMethods.MAX_PATH);
            return sbShortPath.ToString();
        }
        /// 
        /// Truncates a path to fit within a certain number of 
        /// characters by replacing path components with ellipses.
        /// 
        /// 
        /// 
        public static string Compact(string path, int maxlen)
        {
            StringBuilder buffer;
            bool success;

            if (path.Length > NativeMethods.MAX_PATH)
            {
                throw new ArgumentOutOfRangeException(
                    "path", path, "path length must be less than or equal toNativeMethods.MAX_PATH"
                );
            }

            if (maxlen > NativeMethods.MAX_PATH)
            {
                throw new ArgumentOutOfRangeException(
                    "maxlen", maxlen, "maxlen must be less than or equal toNativeMethods.MAX_PATH"
                );
            }

            buffer = new StringBuilder(NativeMethods.MAX_PATH, NativeMethods.MAX_PATH);
            success = NativeMethods.PathCompactPathEx(buffer, path, maxlen, 0);
            if (success)
            {
                return buffer.ToString();
            }
            return null;
        }

        /// 
        /// Determines if a file's registered content type matches 
        /// the specified content type. This function obtains the 
        /// content type for the specified file type and compares 
        /// that string with the pszContentType. The comparison is 
        /// not case sensitive. 
        /// 
        /// 
        /// 
        public static bool IsContentType(string path, string contenttype)
        {
            if (path.Length > NativeMethods.MAX_PATH)
            {
                throw new ArgumentOutOfRangeException(
                    "path", path, "path length must be less than or equal toNativeMethods.MAX_PATH"
                );
            }

            return NativeMethods.PathIsContentType(path, contenttype);
        }
        /// 
        /// Converts a path to all lowercase characters to give 
        /// the path a consistent appearance. This function only 
        /// operates on paths that are entirely uppercase. For 
        /// example: C:\WINDOWS will be converted to c:\windows, 
        /// but c:\Windows will not be changed.
        /// 
        /// 
        public static string MakePretty(string path)
        {
            StringBuilder buffer;

            if (path.Length > NativeMethods.MAX_PATH)
            {
                throw new ArgumentOutOfRangeException(
                    "path", path, "path length must be less than or equal toNativeMethods.MAX_PATH"
                );
            }

            buffer = new StringBuilder(NativeMethods.MAX_PATH, NativeMethods.MAX_PATH);
            buffer.Append(path);
            if (NativeMethods.PathMakePretty(buffer))
            {
                return buffer.ToString();
            }
            return null;
        }
        /// 
        /// Canonicalizes a path. This function allows the user 
        /// to specify what to remove from a path by inserting 
        /// special character sequences into the path. The ".." 
        /// sequence indicates to remove the path part from the 
        /// current position to the previous path part. The "." 
        /// sequence indicates to skip over the next path part 
        /// to the following path part. The root part of the path 
        /// cannot be removed. 
        /// 
        /// 
        public static string Canonicalize(string path)
        {
            StringBuilder buffer;

            if (path.Length > NativeMethods.MAX_PATH)
            {
                throw new ArgumentOutOfRangeException(
                    "path", path, "path length must be less than or equal toNativeMethods.MAX_PATH"
                );
            }

            buffer = new StringBuilder(NativeMethods.MAX_PATH, NativeMethods.MAX_PATH);
            if (NativeMethods.PathCanonicalize(buffer, path))
            {
                return buffer.ToString();
            }
            return null;
        }
        /// 
        /// Determines if a given file name has one of a list of 
        /// suffixes. This function does a case-sensitive comparison. 
        /// The suffix must match exactly. 
        /// 
        /// 
        /// 
        public static bool ContainsExtension(string path, string[] extensions)
        {
            string ext;

            if (path.Length > NativeMethods.MAX_PATH)
            {
                throw new ArgumentOutOfRangeException(
                    "path", path, "path length must be less than or equal toNativeMethods.MAX_PATH"
                );
            }

            ext = NativeMethods.PathFindSuffixArray(path, extensions, extensions.Length);
            if (ext == null)
            {
                return false;
            }
            return true;
        }
    }
}
