using System;
using F4SharedMem;

namespace F4Utils.SimSupport
{
    public interface IDEDAlowReader
    {
        bool CheckDED_ALOW(FlightData fromFalcon, out int newAlow);
    }
    public class DEDAlowReader:IDEDAlowReader
    {
        public bool CheckDED_ALOW(FlightData fromFalcon, out int newAlow)
        {
            var alowString = fromFalcon.DEDLines[1];
            var alowInverseString = fromFalcon.Invert[1];
            var anyCharsHighlighted = false;
            if (alowString.Contains("CARA ALOW"))
            {
                var newAlowString = "";
                for (var i = 0; i < alowString.Length; i++)
                {
                    var someChar = alowString[i];
                    var inverseChar = alowInverseString[i];
                    int tryParse;
                    if (Int32.TryParse(new String(someChar, 1), out tryParse))
                    {
                        if (inverseChar != ' ')
                        {
                            anyCharsHighlighted = true;
                            break;
                        }
                        newAlowString += someChar;
                    }
                }
                if (anyCharsHighlighted)
                {
                    newAlow = -1;
                    return false;
                }
                var success = Int32.TryParse(newAlowString, out newAlow);
                if (!success)
                {
                    newAlow = -1;
                }
                return success;
            }
            newAlow = -1;
            return false;
        }
    }
}
