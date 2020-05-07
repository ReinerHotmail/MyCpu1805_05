using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MyCpu1805_05
{
    public partial class MainWindow
    {




        //private void ListViewOpCodesSortStandard()
        //{
        //    ListViewOpCode.Items.SortDescriptions.Clear();
        //    ListViewOpCode.Items.SortDescriptions.Add(new SortDescription("OrderByFunction", ListSortDirection.Ascending));


        //    TimerViewOpCode.Start();

        //}


        private void ColorListViewOpCode()
        {
            bool brush1 = false;
            bool num144Reached = false;

            for (int i = 0; i < ListViewOpCode.Items.Count; i++)
            {
                ListViewItem lvi = ListViewOpCode.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                if (lvi != null)
                {
                    if (ViewOpCodeColor)
                    {
                        lvi.Background = Brushes.White;
                        continue;
                    }


                    if (((COpCode)lvi.Content).Syntax == "_")
                    {
                        brush1 = !brush1;
                        lvi.FontWeight = FontWeights.Bold;
                    }

                    if (((COpCode)lvi.Content).OrderByFunction == "144" || num144Reached)
                    {
                        num144Reached = true;
                        lvi.Foreground = Brushes.Gray;
                        lvi.Background = Brushes.White;
                    }
                    else
                    {
                        if (brush1)
                        {
                            lvi.Background = Brushes.Moccasin;
                        }
                        else
                        {
                            lvi.Background = Brushes.Khaki;
                        }
                    }
                }
            }
            ViewOpCodeColor = false;
        }

        bool ViewOpCodeColor = false;


        private void TimerColorListViewOpCode_Tick(object sender, EventArgs e)
        {

            TimerColorListViewOpCode.Stop();
            ColorListViewOpCode();
        }







    }
}
