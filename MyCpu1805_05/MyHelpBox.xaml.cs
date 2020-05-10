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
    /// Interaktionslogik für MyMessageBox.xaml
    /// </summary>
    public partial class CHelpBox : Window
    {
        bool leftSide;

        public CHelpBox(bool leftSide,string title, string message)
        {  
            InitializeComponent();
            this.LabelTitle.Content = title;
            this.TextBlock_Message.Text = message;
            string imageBig = "pack://siteoforigin:,,,/Resources/HelpBoxBreak01.png";
            ImageBig.Source = new BitmapImage(new Uri(imageBig));
            this.leftSide = leftSide;
        }

    

    }
}
