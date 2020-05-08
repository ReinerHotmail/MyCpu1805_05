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
        IInputElement iinputElement;
        public MyMessageBox(string title, string message)
        {  
            InitializeComponent();
            this.LabelTitle.Content = title;
            this.TextBlock_Message.Text = message;
            string imageBig = "pack://siteoforigin:,,,/Resources/MyImage.jpg";
            ImageBig.Source = new BitmapImage(new Uri(imageBig));
        }

    

        private void WindowMyMessageBox_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
