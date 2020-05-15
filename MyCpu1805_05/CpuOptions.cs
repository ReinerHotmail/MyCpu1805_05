using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
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
            Start, Run, Stop, End, Break
        }

        Monitor MyMonitor;



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

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);


        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {






            return;


   












            string target = "http://www.microsoft.com";    //Use no more than one assignment when you test this code. 
                                                           //string target = "ftp://ftp.microsoft.com";
                                                           //string target = "C:\\Program Files\\Microsoft Visual Studio\\INSTALL.HTM"; 


            //Process process = new Process();
            //process.StartInfo.UseShellExecute = true;
            //process.StartInfo.FileName = RcaFile.Path + "\\LogFile.txt";
            //process.Start();
            Process _childp = new Process();
            _childp.StartInfo.UseShellExecute = true;
            _childp.StartInfo.FileName = @"D:\MyDat\HelpNDoc\Output\html\Willkommen.html";


            try
            {
                _childp.Start();
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }

            return;






















            //string text0 = "BREAKPOINT:    Programm wird bei Erreichen der Adresse, je nach Bedingung angehalten\n" +
            //                 "LOGPOINT:        Bei Erreichen der Adresse, wird das Logfile mit den ausgewählten \n  " +
            //                 "                           CPU-Daten beschrieben\n\n";

            //string text = "1. Break-/Logpointbedingung eintragen (Standard = B/L - keine Bedingung)\n" +
            //              "2. Logfileeinträge filtern (Standard = alle)\n" +
            //              "3. Break- oder Logbedingung abschliessen mit  'OK-Button'\n";

            //OpenHelpBox(false, "Eingabemöglichkeiten", text0 + text, "HelpBoxBreak01.png");

        }
     

        private void OpenHelpBox(bool leftSide, String title, string message, string imagePath)
        {
            //if (HelpBox != null)
            //    CloseHelpBox();

            //HelpBox = new CHelpBox(leftSide, title, message, imagePath);

            // Manually alter window height and width
            //this.SizeToContent = SizeToContent.Manual;

            // Automatically resize width relative to content
            //this.SizeToContent = SizeToContent.Width;

            // Automatically resize height relative to content
            //this.SizeToContent = SizeToContent.Height;

            //// Automatically resize height and width relative to content
            ////HelpBox.SizeToContent = SizeToContent.WidthAndHeight;



            //HelpBox.Show();

            //double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            //double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

            //HelpBox.Width = screenWidth / 3.0;
            //HelpBox.Height = screenHeight;

            //HelpBox.Top = 0;

            //if (leftSide)
            //    HelpBox.Left = 0;
            //else
            //    HelpBox.Left = screenWidth - screenWidth / 3.0;


        }

        private void CloseHelpBox()
        {
            if (HelpBoxNew == null)
                return;

            HelpBoxNew.Close();
            HelpBoxNew = null;
        }



        int TestR = 0;
        int TestC = 0;
        bool TestBool1 = false;
        bool TestBool = false;
        private void ButtonTest2_Click(object sender, RoutedEventArgs e)
        {
            if (!TestBool1)
            {
                HelpBoxNew = new CHelpBox(false, "Titel-Neu", "", 0);
                TestBool1 = true;
            }
            else
            {
                HelpBoxNew = new CHelpBox(true, "Titel-Neu", "", 4);
            }



            HelpBoxNew.Show();
  

            return;



            //HelpBox = new CHelpBox(false, "Title", "message", "imagePath");

            // Manually alter window height and width
            //this.SizeToContent = SizeToContent.Manual;

            // Automatically resize width relative to content
            //this.SizeToContent = SizeToContent.Width;

            // Automatically resize height relative to content
            //this.SizeToContent = SizeToContent.Height;

            // Automatically resize height and width relative to content
            //HelpBox.SizeToContent = SizeToContent.WidthAndHeight;



            //HelpBox.Show();

            //double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            //double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

            //HelpBox.Width = screenWidth *0.4;
            //HelpBox.Height = screenHeight;

            //HelpBox.Top = 0;

            ////if (leftSide)
            ////    HelpBox.Left = 0;
            ////else
            //    HelpBox.Left = screenWidth - HelpBox.Width;




            //string fileName = @"C:\Users\rsche\Desktop\Cpu1802.xps";
            ////fileName = fileName.Remove(0, 6);
            //System.Windows.Xps.Packaging.XpsDocument doc = new System.Windows.Xps.Packaging.XpsDocument(fileName, FileAccess.Read);
            //HelpBox.DocumentViewerHelp.Document = doc.GetFixedDocumentSequence();
            //HelpBox.DocumentViewerHelp.GoToPage(4);
















            // CloseHelpBox();




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
