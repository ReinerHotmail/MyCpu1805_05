using System;
using System.Collections.Generic;
using System.Text;

namespace MyCpu1805_05
{
    class CProg
    {
        public int LineNum { get; set; }
        public string ProgName { get; set; }

        public string Adr { get; set; }

        public CProg(int lineNum, string progName, string adr)
        {
            LineNum = lineNum;
            ProgName = progName;
            Adr = adr;

        }

        public override string ToString()
        {
            return ProgName.PadRight(12) + Adr;
        }
    }
}
