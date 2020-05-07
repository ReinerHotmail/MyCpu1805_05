using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MyCpu1805_05
{
    public partial class MainWindow
    {
        List<COutput> ListOUT = new List<COutput>();

        private void IniOutput()
        {
            //  in Liste, damit sie per Loop adressiert werden können
            ListOUT.Add(new COutput() { Num = LabelOutput1, Box = TextBoxOutput1 }); // damit index0 gefdüllt ist
            ListOUT.Add(new COutput() { Num = LabelOutput1, Box = TextBoxOutput1 });
            ListOUT.Add(new COutput() { Num = LabelOutput2, Box = TextBoxOutput2 });
            ListOUT.Add(new COutput() { Num = LabelOutput3, Box = TextBoxOutput3 });
            ListOUT.Add(new COutput() { Num = LabelOutput4, Box = TextBoxOutput4 });
            ListOUT.Add(new COutput() { Num = LabelOutput5, Box = TextBoxOutput5 });
            ListOUT.Add(new COutput() { Num = LabelOutput6, Box = TextBoxOutput6 });
            ListOUT.Add(new COutput() { Num = LabelOutput7, Box = TextBoxOutput7 });

        }

        private void RadioButtonOutputHex_Click(object sender, RoutedEventArgs e)
        {
            GetOutputs();
        }

        private void RadioButtonOutputBin_Click(object sender, RoutedEventArgs e)
        {
            GetOutputs();
        }

        private void RadioButtonOutputChar_Click(object sender, RoutedEventArgs e)
        {
            GetOutputs();
        }
        private void GetOutputs()
        {

            if (RadioButtonOutputChar.IsChecked == true)
            {
                for (int i = 1; i < 8; i++)
                {
                    if (OUT[i] != -1)
                    {
                        ListOUT[i].Box.Text = ((char)OUT[i]).ToString();
                    }

                }
            }
            else if (RadioButtonOutputHex.IsChecked == true)
            {
                for (int i = 1; i < 8; i++)
                {
                    if (OUT[i] != -1)
                    {
                        ListOUT[i].Box.Text = ((int)OUT[i]).ToString("X1");
                    }

                }
            }
            else if (RadioButtonOutputBin.IsChecked == true)
            {
                for (int i = 1; i < 8; i++)
                {
                    if (OUT[i] != -1)
                    {
                        ListOUT[i].Box.Text = Convert.ToString((int)OUT[i], 2);
                    }

                }
            }

        }

        /// <summary>
        /// Aufruf in CPU-OpCode OUT, und damit Abarbeiten mit jedem Cpu-Step
        /// </summary>
        /// <param name="outNum"></param>
        public void OutputFromCpu(int outNum)
        {
            for (int i = 1; i < 5; i++)
            {
                if (ListINP[i].QueueInput.Count > 0)
                {
                    if (ListINP[i].Setting == outNum && OUT[outNum] == 1)
                    {
                        switch (outNum)
                        {
                            case 1:
                                EF1 = false;
                                break;
                            case 2:
                                EF3 = false;
                                break;
                            case 3:
                                EF3 = false;
                                break;
                            case 4:
                                EF4 = false;
                                break;
                            default:
                                break;
                        }

                        ListINP[i].QueueInput.Dequeue();
                    }
                }
            }


        }



    }
}
