using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyCpu1805_05
{
    class CMemView
    {
        public string Adr { get; set; }

        public string Data { get; set; }

        public string Break { get; set; }

        public Brush AdrBrush { get; set; }

        public Brush DataBrush { get; set; }

        public override string ToString()
        {
            return Adr + "   " + Data;
        }

    }
}
