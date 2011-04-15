using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Common.Strings
{
    public static class Util
    {
        public static List<string> Tokenize(string input)
        {
            Regex r = new Regex(@"[\s]+");
            string[] tokens = r.Split(input);
            List<string> tokenList = new List<string>();
            foreach (string token in tokens)
            {
                if (token.Trim().Length == 0)
                    continue;
                if (token.EndsWith(";"))
                {
                    string thisToken = token;
                    int tokensReplaced = 0;
                    while (thisToken.EndsWith(";"))
                    {
                        thisToken = thisToken.Substring(0, thisToken.Length - 1);
                        tokensReplaced++;
                    }
                    tokenList.Add(thisToken);
                    for (int i = 0; i < tokensReplaced; i++)
                    {
                        tokenList.Add(";");
                    }
                }
                else if (token.StartsWith("//"))
                {
                    string thisToken = token;
                    int tokensReplaced = 0;
                    while (thisToken.StartsWith("//"))
                    {
                        thisToken = thisToken.Substring(2, thisToken.Length - 2);
                        tokensReplaced++;
                    }
                    for (int i = 0; i < tokensReplaced; i++)
                    {
                        tokenList.Add("//");
                    }
                    tokenList.Add(thisToken);
                }
                else
                {
                    tokenList.Add(token);
                }
            }
            return tokenList;
        }
        public static byte[] GetBytesInDefaultEncoding(string aString) 
        {
            if (aString != null) return Encoding.Default.GetBytes(aString);
            return new byte[0];
        }
    }
}
