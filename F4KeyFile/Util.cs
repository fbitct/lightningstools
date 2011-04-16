using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace F4KeyFile
{
    internal static class Util
    {
        internal static List<string> Tokenize(string input)
        {
            var r = new Regex(@"[\s]+");
            string[] tokens = r.Split(input);
            var tokenList = new List<string>();
            foreach (string token in tokens)
            {
                if (token.Trim().Length == 0)
                    continue;
                tokenList.Add(token);
            }
            return tokenList;
        }
    }
}