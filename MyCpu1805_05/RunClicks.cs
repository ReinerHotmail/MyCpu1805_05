using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MyCpu1805_05
{
    public partial class MainWindow
    {
        private void CheckBoxDebug_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxDebug.IsChecked == true)
            {
                RadioButtonCpuModeSingleStep.Visibility = Visibility.Visible;
                RadioButtonCpuModeFetchExecute.Visibility = Visibility.Visible;
            }
            else
            {
                CheckBoxBreakpoints.IsChecked = false;
                BreakpointsOn = false;
                BreakpointReached = false;
                ListViewBreakpoints.Background = Brushes.White;
                LabelBeakpoints.Background = Brushes.Transparent;
                LabelBeakpoints.Content = "";


                RadioButtonCpuModeRun.IsChecked = true;
                RadioButtonCpuModeSingleStep.Visibility = Visibility.Hidden;
                RadioButtonCpuModeFetchExecute.Visibility = Visibility.Hidden;
            }
        }







        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {


            if (LogFileLines() >= LogFileMaxLines)
            {
                MessageBoxResult result =
                MessageBox.Show("LogFile hat " + LogFileMaxLines + " Zeilen erreicht\nSoll das LogFile gelöscht werden?", "Achtung", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return;

                if (File.Exists(PathLog + "\\LogFile.txt"))
                    File.Delete(PathLog + "\\LogFile.txt");

            }


            if (CheckBoxDebug.IsChecked == true)
            {
                if (RadioButtonCpuModeRun.IsChecked == true)
                    CpuMode = Cycle.DebugLoop;
                if (RadioButtonCpuModeSingleStep.IsChecked == true)
                    CpuMode = Cycle.DebugStep;
                if (RadioButtonCpuModeFetchExecute.IsChecked == true)
                    CpuMode = Cycle.DebugFetchExecute;

            }
            else
            {
                CpuMode = Cycle.Run;
            }

            if (RcaFile.Long == null)
            {
                MessageBox.Show("Kein startbares Programm im Speicher");
                return;
            }

            StopWatchRunTime.Start();
            LabelRunTimeStart.Content = DateTime.Now.ToString("HH-mm-ss.fff tt");
            LogFileAddLine(LogLine.Start, "", "PXD", false);

            LabelBeakpoints.Background = Brushes.Transparent;
            LabelBeakpoints.Content = "";

            ExeStop = false;
            TimerExe.Start();
        }

        private int LogFileLines()
        {
            if (File.Exists(PathLog + "\\LogFile.txt"))
            {
                string[] logLines = File.ReadAllLines(PathLog + "\\LogFile.txt");
                int len = logLines.Length;
                return len;
            }
            return -1;
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            ExeStop = true;


        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            CpuReset();
        }



        private void CheckBoxBreakpoints_Click(object sender, RoutedEventArgs e)
        {

            if (CheckBoxBreakpoints.IsChecked == true)
            {
                if (CheckBoxDebug.IsChecked == false)
                    CheckBoxDebug.IsChecked = true;

                BreakpointsOn = true;
                ListViewBreakpoints.Background = Brushes.Orange;
            }
            else
            {
                BreakpointsOn = false;
                BreakpointReached = false;
                ListViewBreakpoints.Background = Brushes.White;
                LabelBeakpoints.Background = Brushes.Transparent;
                LabelBeakpoints.Content = "";

            }
        }



        private void CheckBoxIntr_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxIntr.IsChecked == true && IE)
                INTR = true;
            else
                INTR = false;

            ShowIntrBits();
        }

        private void CheckBoxIe_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxIe.IsChecked == true)
                IE = true;
            else
                IE = false;


            ShowIntrBits();
        }



        private void ShowIntrBits()
        {
            if (IE)
                CheckBoxIe.IsChecked = true;
            else
                CheckBoxIe.IsChecked = false;

            if (INTR)
                CheckBoxIntr.IsChecked = true;
            else
                CheckBoxIntr.IsChecked = false;
        }

        private void ButtonLogFileRuntime_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(PathLog + "\\LogFile.txt"))
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = PathLog + "\\LogFile.txt";
                process.Start();
            }




        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ListViewRegister_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CRegister regClicked = (CRegister)ListViewRegister.SelectedItem;

            if (regClicked == null)
                return;

            ColorLine_ListViewMem(Convert.ToInt32(regClicked.Val, 16));

            CEditLine lineFound = ListEditLines.Find(x => x.Adr == regClicked.Val);

            if (lineFound == null)
                return;

            ColorLine_Editor(lineFound);
            ColorLine_ListViewBreakpoints(lineFound.Num);
            ColorLine_ListViewJump(lineFound.Num);
            ColorLine_ListViewProg(lineFound.Num);

        }


        private void CheckBoxDmaIn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxDmaOut_Click(object sender, RoutedEventArgs e)
        {
            MonitorOnOff();
         

        }


        private void CheckBoxDmaOut_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!Monitor.DmaOutActive && CheckBoxDmaOut.IsChecked == true)
            {
                //LabelDmaOut.Background = Brushes.White;
                //CheckBoxDmaOut.IsChecked = false;
            }
        }
    }
}
