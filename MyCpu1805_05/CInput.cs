using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace MyCpu1805_05
{
    class CInput
    {
        public Label Num { get; set; }
        public TextBox Box { get; set; }

        public Label LastInput { get; set; }


        public Queue<int> QueueInput = new Queue<int>();

        public int Setting { get; set; }

        public override string ToString()
        {
            return Num.Content + "  " + Box.Text + "  " + LastInput.Content;
        }


    }
}
