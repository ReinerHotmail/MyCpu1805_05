using System;
using System.Collections.Generic;
using System.Text;

namespace MyCpu1805_05
{
    class COpCode
    {
        //Nr;OpCodeLong; OpCode;OpCodeHex;OpCodeSyntax;OpCodeDuble;OpCodeBytes;OpCodeSignNextByte;OpCodeInfo
        //1 |LOAD VIA N |LDN   |01       |LDN 1       |0          |1          |                  |M(R(1)) -> D 
        public int Num { get; set; }
        public string Long { get; set; }
        public string Short { get; set; }
        public string Hex { get; set; }
        public string Syntax { get; set; }
        public int Register { get; set; }
        public int Double { get; set; }
        public int Bytes { get; set; }
        public string SignNextByte { get; set; }
        public string Info { get; set; }
        public int Singles { get; set; }

        public string OrderByFunction { get; set; }





        public override string ToString()
        {
            return Syntax + " - " + Hex;
        }
    }
}
