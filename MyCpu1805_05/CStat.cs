using System;
using System.Collections.Generic;
using System.Text;

namespace MyCpu1805_05
{
    public class CStat
    {
        public static bool IsHex(string testString)
        {
            // For C-style hex notation (0xFF) you can use @"\A\b(0[xX])?[0-9a-fA-F]+\b\Z"
            return System.Text.RegularExpressions.Regex.IsMatch(testString, @"\A\b[0-9a-fA-F]+\b\Z");
        }



    







    }
}
