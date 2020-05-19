using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaktionslogik für MyMessageBox.xaml
    /// </summary>
    public partial class CHelpBox : Window
    {
        bool leftSide;

        public CHelpBox(bool leftSide, string title, string message, int page)
        {
            InitializeComponent();
            this.LabelTitle.Content = title;
            this.TextBlock_Message.Text = message;
            //string imageBig = "pack://siteoforigin:,,,/Resources/" + "HelpBoxBreak01.png";
            //ImageBig.Source = new BitmapImage(new Uri(imageBig));
            this.leftSide = leftSide;


            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

            this.Width = screenWidth * 0.4;
            this.Height = screenHeight;

            this.Top = 0;


            if (leftSide)
                this.Left = 0;
            else
                this.Left = screenWidth - this.Width;

            //string fileName = @"C:\Users\rsche\Desktop\Cpu1802.xps";

            string resPath = System.IO.Path.GetFullPath("Resources");
            string fileName = resPath + "\\Cpu1802.xps";

            System.Windows.Xps.Packaging.XpsDocument doc = new System.Windows.Xps.Packaging.XpsDocument(fileName, FileAccess.Read);
            this.DocumentViewerHelp.Document = doc.GetFixedDocumentSequence();


            this.DocumentViewerHelp.GoToPage(page);



        }

        private void TextBoxPage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(TextBoxPage.Text, out int page))
                DocumentViewerHelp.GoToPage(page);
        }

        private void RadioButtonLeft_Click(object sender, RoutedEventArgs e)
        {

            this.Left = 0;
            this.Top = 0;

        }

        private void RadioButtonRight_Click(object sender, RoutedEventArgs e)
        {

            this.Left = System.Windows.SystemParameters.PrimaryScreenWidth - this.Width;
            this.Top = 0;
        }

        private void ButtonFocus_Click(object sender, RoutedEventArgs e)
        {
            this.DocumentViewerHelp.Find();
        }

        private void ButtonUp_Click(object sender, RoutedEventArgs e)
        {
            this.DocumentViewerHelp.PreviousPage();
        }

        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            this.DocumentViewerHelp.NextPage();
        }
    }
}
