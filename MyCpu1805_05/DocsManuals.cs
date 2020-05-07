using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace MyCpu1805_05
{
    public partial class MainWindow
    {
        private void ButtonDocCpd1802_Click(object sender, RoutedEventArgs e)
        {

            string resPath = Path.GetFullPath("Resources");
            string pdfFile = resPath + "\\cdp1802.pdf";

            if (File.Exists(pdfFile))
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = pdfFile;
                process.Start();
            }


        }

        private void ButtonDocCpd1806_Click(object sender, RoutedEventArgs e)
        {
            string resPath = Path.GetFullPath("Resources");
            string pdfFile = resPath + "\\cdp1806.pdf";

            if (File.Exists(pdfFile))
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = pdfFile;
                process.Start();
            }
        }

        private void ButtonDocDataSheets_Click(object sender, RoutedEventArgs e)
        {

            Process process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = "http://www.cosmacelf.com/publications/data-sheets";
            process.Start();

        }

        private void ButtonDocShortCourse_Click(object sender, RoutedEventArgs e)
        {

            string resPath = Path.GetFullPath("Resources");
            string pdfFile = resPath + "\\A Short Course In Programming by Tom Pittman.pdf";

            if (File.Exists(pdfFile))
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = pdfFile;
                process.Start();
            }
        }

        private void ButtonDocAscii_Click(object sender, RoutedEventArgs e)
        {

            string resPath = Path.GetFullPath("Resources");
            string pdfFile = resPath + "\\1802_ASCII-Tabelle.pdf";

            if (File.Exists(pdfFile))
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = pdfFile;
                process.Start();
            }
        }
    }
}
