using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;

namespace MyCpu1805_05
{
    class CEditLine
    {
        public CEditLine(int num, string original)
        {
            Num = num;
            Adr = "";
            //CodeLine = false;
            Original = original;
            LineComment = "";
            LineProgram = "";
            ProgColor = false;
            LineReplace = "";

            LeftJump = "";
            OpCodeLeft = "";
            OpCodeRight = "";
            commentRight = "";
            ErrLine = "";
            ByteLen = -1;
            Byte1 = "";
            Byte2 = "";

        }

        public int Num { get; set; }

        public string Adr { get; set; }

        //public bool CodeLine { get; set; }
        public string Original { get; set; }
        public string LineComment { get; set; }

        public string LineProgram { get; set; }

        public bool ProgColor { get; set; }

        public string LineReplace { get; set; }


        public string LeftJump { get; set; }

        public string OpCodeLeft { get; set; }

        public string OpCodeRight { get; set; }

        public string commentRight { get; set; }

        public string ErrLine { get; set; }

        public Paragraph Para { get; set; }

        public int ByteLen { get; set; }
        public string Byte1 { get; set; }
        public string Byte2 { get; set; }


        public override string ToString()
        {
            return Original;
        }


    }
}
