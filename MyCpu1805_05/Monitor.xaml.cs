using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyCpu1805_05
{
    /// <summary>
    /// Interaktionslogik für Monitor.xaml
    /// </summary>
    public partial class Monitor : Window
    {
        public static int ROW = 24;
        public static int COL = 48;
        public static bool Active = false;
        public static bool On = false;

        Label[,] Fields = new Label[ROW, COL];
        int FieldNumNow = 0;


        #region PIXL
        //Rectangle[,] Pixl = new Rectangle[64, 128];

        //int RowCount = 0;

        //public int PixlRowNow = 0;

        //public int PixlColNow = 0;
        #endregion
        public Monitor()
        {
            InitializeComponent();
        }

        private void WindowMonitor_Loaded(object sender, RoutedEventArgs e)
        {

            #region PIXL
            //double monitorWidth = CanvasMonitor.ActualWidth;
            //double monitorHeight = CanvasMonitor.ActualHeight;



            //Rectangle rec = null;

            //for (int row = 0; row < 64; row++)
            //{
            //    for (int col = 0; col < 128; col++)
            //    {
            //        rec = new Rectangle() { Width = (monitorWidth / 128), Height = (monitorHeight / 64), Fill = Brushes.Black };
            //        Pixl[row, col] = rec;
            //        CanvasMonitor.Children.Add(rec);
            //        Canvas.SetLeft(rec, rec.Width * col);
            //        Canvas.SetTop(rec, rec.Height * row);
            //    }
            //}
            #endregion

            double monitorWidth = GridMonitor.ActualWidth;
            double monitorHeight = GridMonitor.ActualHeight;



            Label field = null;

            for (int row = 0; row < ROW; row++)
            {
                for (int col = 0; col < COL; col++)
                {
                    field = new Label();
                    field.Background = Brushes.Black;
                    field.FontSize = 14;
                    field.FontFamily = new FontFamily("CourierNew");
                    field.FontWeight = FontWeights.Bold;
                    field.Foreground = Brushes.White;
                    field.Margin = new Thickness(0);
                    field.Padding = new Thickness(0);
                    //field.Content = col.ToString()[0];
                    Fields[row, col] = field;
                    GridMonitor.Children.Add(field);
                    Grid.SetColumn(field, col);
                    Grid.SetRow(field, row);

                }
            }

        }


        public void SetText()
        {
            int i = 0;


            for (int row = 0; row < ROW; row++)
            {
                for (int col = 0; col < COL; col++)
                {
                    Fields[row, col].Content = CStat.DmaValues[i++];
                }
            }


        }

        private void ScrollMonitor()
        {
            for (int r = 1; r < ROW; r++)
            {
                for (int c = 0; c < COL; c++)
                {
                    Fields[r - 1, c].Content = Fields[r, c].Content;
                }
            }
            for (int c = 0; c < COL; c++)
            {
                //Fields[31, c].Content = "";
                Fields[(ROW - 1), c].Content = "";
            }
        }

        public void MonitorOnOff(bool on)
        {
            if (on)
            {
                LabelMonitorOnOff.Background = Brushes.LightGreen;
            }
            else
            {
                LabelMonitorOnOff.Background = Brushes.LightPink;
            }

        }


        #region PIXL
        ////#DAAADF	 0   
        ////#D9DD8F	 1   
        ////#9EDB8F	 2
        ////#9EDE9F	 3
        ////#EAA8EF	 4
        ////#8B9E9F	 5
        ////#CB9ADF	 6
        ////#8EDBBF    7
        ////#DADADF	 8
        ////#DACEDF	 9

        ///// <summary>
        ///// WriteTerm on Monitor at row/col
        ///// </summary>
        ///// <param name = "rowPixlStart" > 0..10 </ param >
        ///// < param name="colPixlStart">0 .. 42 </param>
        ///// <param name = "term" ></ param >
        ////public void WriteTerm(int rowPixlStart, int colPixlStart, string term)
        ////{


        ////    if (rowPixlStart >= 0)
        ////    {
        ////        PixlRowNow = rowPixlStart;
        ////    }





        ////    if (colPixlStart >= 0)
        ////    {
        ////        PixlColNow = (colPixlStart * 4) % 128;
        ////    }


        ////    zB 1101 1001 1101 1101 1000 1111 - ohne Space
        ////     1101
        ////     1001
        ////     1101
        ////     1101
        ////     1000
        ////     1111




        ////    #region  um 6 Lines hochscrollen, wenn Monitor voll
        ////    if (PixlRowNow >= 59)
        ////    {
        ////        for (int line = 0; line < 6; line++)
        ////        {
        ////            for (int row = 0; row < 63; row++)
        ////            {
        ////                for (int col = 0; col < 128; col++)
        ////                {

        ////                    Pixl[row, col].Fill = Pixl[row + 1, col].Fill;


        ////                }
        ////            }
        ////        }
        ////        PixlRowNow -= 6;
        ////    }
        ////    #endregion

        ////    int iChar = Convert.ToInt32(term.ToString(), 16);
        ////    string bChar = Convert.ToString(iChar, 2);

        ////    int maxLoop = (bChar.Length / 8);
        ////    int i = 0;
        ////    int loop = 0;



        ////    for (loop = 0; loop < maxLoop; loop++) // len = 8/16/24.. -> lines 2/4/8...  maxLoop = 1/2/3
        ////    {

        ////        for (int row = 0; row < 2; row++)
        ////        {
        ////            for (int col = 0; col < 4; col++)
        ////            {
        ////                char c = bChar[i];

        ////                if (c == '1')
        ////                    Pixl[PixlRowNow + row, PixlColNow + col].Fill = Brushes.Black;
        ////                else
        ////                    Pixl[PixlRowNow + row, PixlColNow + col].Fill = Brushes.White;

        ////                i++;
        ////            }
        ////        }

        ////        PixlRowNow += 2;
        ////        RowCount += 2;

        ////        if (RowCount % 6 == 0)
        ////        {
        ////            RowCount = 0;
        ////            PixlColNow += 4;

        ////            if (PixlColNow < 127)
        ////                PixlRowNow -= 6;

        ////            if (PixlColNow >= 127)
        ////                PixlColNow = 0;
        ////        }

        ////    }




        ////}

        #endregion

        private void WindowMonitor_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Monitor.Active = false;
            Monitor.On = false;
        }

        private void WindowMonitor_Activated(object sender, EventArgs e)
        {
            Monitor.Active = true;
        }
    }



}
