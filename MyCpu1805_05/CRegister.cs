using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MyCpu1805_05
{
    public class CRegister : INotifyPropertyChanged
    {
        public string Nr { get; set; }

        private bool p;
        public string P
        {
            get
            {
                if (p)
                    return "P";
                else
                    return "";

            }
            set
            {
                if (value != "")
                    p = true;
                else
                    p = false;

                OnPropertyChanged(new PropertyChangedEventArgs("P"));
            }
        }

        private bool x;
        public string X
        {
            get
            {
                if (x)
                    return "X";
                else
                    return "";

            }
            set
            {
                if (value != "")
                    x = true;
                else
                    x = false;

                OnPropertyChanged(new PropertyChangedEventArgs("X"));
            }
        }

        private int val;
        public string Val
        {
            get { return val.ToString("X4"); }
            set { val = Convert.ToInt32(value); HexBinDec = val.ToString(); OnPropertyChanged(new PropertyChangedEventArgs("Val")); }
        }


        private int hexBinDec;
        public string HexBinDec
        {
            get
            {
                int v = hexBinDec;
                string binHighLow = Convert.ToString(v, 2).PadLeft(16, '0');

                string[] h = new string[4];
                for (int i = 0; i < 4; i++)
                {
                    h[i] = binHighLow.Substring(i * 4, 4);
                }

                return "Hex: " + v.ToString("X4") + Environment.NewLine +
                       "Bin: " + h[0] + " " + h[1] + " " + h[2] + " " + h[3] + Environment.NewLine +
                       "Dec: " + hexBinDec;
            }
            set { hexBinDec = Convert.ToInt32(value); OnPropertyChanged(new PropertyChangedEventArgs("HexBinDec")); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }


    }
}
