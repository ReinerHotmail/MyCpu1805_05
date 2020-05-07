using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace MyCpu1805_05
{
    class COutput
    {
        public Label Num { get; set; }
        public TextBox Box { get; set; }

        public override string ToString()
        {
            return Num.Content + "  " + Box.Text;
        }
    }
}
