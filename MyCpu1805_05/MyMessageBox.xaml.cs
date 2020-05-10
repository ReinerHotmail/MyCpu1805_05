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
    public partial class MyMessageBox : Window
    {
        bool leftSide;

        public MyMessageBox(bool leftSide,string title, string message)
        {  
            InitializeComponent();
            this.LabelTitle.Content = title;
            this.TextBlock_Message.Text = message;
            string imageBig = "pack://siteoforigin:,,,/Resources/HelpBoxBreak01.png";
            ImageBig.Source = new BitmapImage(new Uri(imageBig));
            this.leftSide = leftSide;
        }

    

        private void WindowMyMessageBox_Loaded(object sender, RoutedEventArgs e)
        {
            double  screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth ;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight ;
            this.Top = 0;

            if (leftSide)
                this.Left = 0;
            else
                this.Left = screenWidth - this.ActualWidth;
             
       




        }

        private void WindowMyMessageBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
