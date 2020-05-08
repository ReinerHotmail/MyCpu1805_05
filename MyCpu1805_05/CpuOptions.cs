using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace MyCpu1805_05
{
    public partial class MainWindow
    {
        enum LogLine
        {
            Start, Run, Stop, End
        }

        Monitor MyMonitor;


        private void ButtonOptionMonitor_Click(object sender, RoutedEventArgs e)
        {

        }
        public IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                int depObjCount = VisualTreeHelper.GetChildrenCount(depObj);
                for (int i = 0; i < depObjCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    if (child is GroupBox)
                    {
                        GroupBox gb = child as GroupBox;
                        Object gpchild = gb.Content;
                        if (gpchild is T)
                        {
                            yield return (T)child;
                            child = gpchild as T;
                        }
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        //ToDo ButtonTest
        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            CreateMyMessageBox(this,"TTTTTTT","dies ist eine message");

        }

        private void CreateMyMessageBox(IInputElement iinputElement,String title,string message)
        {
            Point location = System.Windows.Input.Mouse.GetPosition(iinputElement);
            Point point = PointToScreen(location);


            MyMessageBox myBox = new MyMessageBox(point,title,message);

            // Manually alter window height and width
            //this.SizeToContent = SizeToContent.Manual;

            // Automatically resize width relative to content
            //this.SizeToContent = SizeToContent.Width;

            // Automatically resize height relative to content
            //this.SizeToContent = SizeToContent.Height;

            // Automatically resize height and width relative to content
            myBox.SizeToContent = SizeToContent.WidthAndHeight;




          

            //var location = myBox.PointToScreen(new Point(0, 0));



    

            myBox.ShowDialog();



      
        }

        int TestR = 0;
        int TestC = 0;
        bool TestBool1 = false;
        bool TestBool = false;
        private void ButtonTest2_Click(object sender, RoutedEventArgs e)
        {
            return;

 

            #region PIXL
            //#DAAADF	 0   
            //#D9DD8F	 1   
            //#9EDB8F	 2
            //#9EDE9F	 3
            //#EAA8EF	 4
            //#8B9E9F	 5
            //#CB9ADF	 6
            //#8EDBBF    7
            //#DADADF	 8
            //#DACEDF	 9

            //MyMonitor.WriteTerm(-1, -1, "DAAADF");
            //MyMonitor.WriteTerm(-1, -1, "D9DD8F");
            //MyMonitor.WriteTerm(-1, -1, "9EDB8F");
            //MyMonitor.WriteTerm(-1, -1, "9EDE9F");
            //MyMonitor.WriteTerm(-1, -1, "EAA8EF");
            //MyMonitor.WriteTerm(-1, -1, "8B9E9F");
            //MyMonitor.WriteTerm(-1, -1, "CB9ADF");
            //MyMonitor.WriteTerm(-1, -1, "8EDBBF");
            //MyMonitor.WriteTerm(-1, -1, "DADADF");
            //MyMonitor.WriteTerm(-1, -1, "DACEDF");

            //if (!TestBool)
            //{
            //    TestBool = true;
            //    for (int i = 0; i < 320; i++)
            //    {
            //        MyMonitor.WriteTerm(TestR, TestC, "DADA");
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < 20; i++)
            //    {
            //        MyMonitor.WriteTerm(TestR, TestC, "F10B");
            //    }
            //}

            //TestC++;

            //if (MyMonitor.PixlColNow >= 120)
            //{
            //    TestR= MyMonitor.PixlRowNow+1;
            //    TestC = 0;
            //}

            //MyMonitor.WriteTerm(TestR, TestC, "D9DD8F");
            //TestC++;

            //if (MyMonitor.PixlColNow>=127)
            //{
            //    TestR+=6;
            //    TestC = 0;
            //}
            //var a = MyMonitor.PixlRowNow;
            //var b = MyMonitor.PixlColNow;
            #endregion


            int iMax = Monitor.ROW * Monitor.COL;

            for (int i = 0; i < iMax; i++)
            {
                string iStr = i.ToString().PadLeft(3, '0');

                if (!TestBool)
                {
                    MyMonitor.DmaValues[i] = (byte)(iStr[1]);

                }
                else
                {
                    MyMonitor.DmaValues[i] = (byte)(iStr[2]);

                }
            }

            MyMonitor.SetText();

            TestBool = !TestBool;




        }
    }
}
