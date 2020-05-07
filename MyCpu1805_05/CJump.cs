using System;
using System.Collections.Generic;
using System.Text;

namespace MyCpu1805_05
{
    class CJump
    {
        public int LineNum { get; set; }
        public string ProgName { get; set; }
        public string JumpName { get; set; }
        public string Adr { get; set; }

        public CJump(int lineNum, string progName, string jumpName)
        {
            LineNum = lineNum;
            ProgName = progName;
            JumpName = jumpName;

        }
        public override string ToString()
        {
            return (ProgName + "." + JumpName).PadRight(22) + " " + Adr;
        }
    }
}
