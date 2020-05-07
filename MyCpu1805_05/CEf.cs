using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace MyCpu1805_05
{
    class CEf
    {
        private bool val;

        public bool Val
        {
            get { return val; }
            set
            {
                val = value;
                if (val)
                    LabelVal.Content = "1";
                else
                    LabelVal.Content = "0";
            }
        }

        public CheckBox Box { get; set; }
        public Label LabelNum { get; set; }

        public Label LabelVal { get; set; }




    }
}
