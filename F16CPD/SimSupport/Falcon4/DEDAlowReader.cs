using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4
{
    internal interface IDEDAlowReader
    {
        bool CheckDED_ALOW(F4SharedMem.FlightData fromFalcon, out int newAlow);
    }
    class DEDAlowReader:IDEDAlowReader
    {
        public bool CheckDED_ALOW(F4SharedMem.FlightData fromFalcon, out int newAlow)
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
