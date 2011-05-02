using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace F4KeyFile
{
    internal static class Util
    {
        internal static List<string> Tokenize(string input)
        {
            var r = new Regex(@"[\s]+");
            var tokens = r.Split(input);
            return tokens.Where(token => token.Trim().Length != 0).ToList();
        }
    }
}