using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MyCpu1805_05
{
    public partial class MainWindow
    {

        List<CInput> ListINP = new List<CInput>();



        private void IniInput()
        {
            // alle Objekte in Liste, damit sie per Loop adressiert werden können

            // Eintrag 0  -  nicht genutzt
            Label num = LabelInput1;
            TextBox box = TextBoxInput1;
            Label lastInput = LabelINP1;

            CInput input = new CInput() { Num = num, Box = box, LastInput = lastInput, Setting = 0 };
            ListINP.Add(input);

            num = LabelInput1;
            box = TextBoxInput1;
            lastInput = LabelINP1;

            input = new CInput() { Num = num, Box = box, LastInput = lastInput, Setting = 0 };
            ListINP.Add(input);

            num = LabelInput2;
            box = TextBoxInput2;
            lastInput = LabelINP2;

            input = new CInput() { Num = num, Box = box, LastInput = lastInput, Setting = 0 };
            ListINP.Add(input);

            num = LabelInput3;
            box = TextBoxInput3;
            lastInput = LabelINP3;

            input = new CInput() { Num = num, Box = box, LastInput = lastInput, Setting = 0 };
            ListINP.Add(input);

            num = LabelInput4;
            box = TextBoxInput4;
            lastInput = LabelINP4;

            input = new CInput() { Num = num, Box = box, LastInput = lastInput, Setting = 0 };
            ListINP.Add(input);

            num = LabelInput5;
            box = TextBoxInput5;
            lastInput = LabelINP5;

            input = new CInput() { Num = num, Box = box, LastInput = lastInput, Setting = 0 };
            ListINP.Add(input);

            num = LabelInput6;
            box = TextBoxInput6;
            lastInput = LabelINP6;

            input = new CInput() { Num = num, Box = box, LastInput = lastInput, Setting = 0 };
            ListINP.Add(input);

            num = LabelInput7;
            box = TextBoxInput7;
            lastInput = LabelINP7;

            input = new CInput() { Num = num, Box = box, LastInput = lastInput, Setting = 0 };
            ListINP.Add(input);

            StackPanelInputConfig.Visibility = Visibility.Hidden;


            waitForIniInput = false;
        }


        private void TextBoxInputN_KeyUp(object sender, KeyEventArgs e)
        {
            bool newTerm = false;

            if (RadioButtonInputChar.IsChecked == true)
                newTerm = true;
            else if (RadioButtonInputHex.IsChecked == true && e.Key == Key.Return)
                newTerm = true;
            else if (RadioButtonInputBin.IsChecked == true && e.Key == Key.Return)
                newTerm = true;

            if (!newTerm)
                return;

            TextBox tb = (TextBox)sender;
            int num = Convert.ToInt32((string)tb.Tag);

            GetInput(num);

        }



        private void GetInput(int num)
        {
            int i = num;

            if (ListINP[i].Box.Text != "")
            {
                if (RadioButtonInputHex.IsChecked == true)
                {
                    if (CStat.IsHex(ListINP[i].Box.Text))
                    {
                        int result = Convert.ToInt32(ListINP[i].Box.Text, 16);

                        if (result < 256)
                        {
                            ListINP[i].QueueInput.Enqueue(result);
                            ListINP[i].LastInput.Content = result.ToString("X2");
                            ListINP[i].Box.Text = "";
                            ListINP[i].Num.Background = Brushes.LightGreen;
                        }
                        else
                        {
                            ListINP[i].Num.Background = Brushes.Red;
                        }
                    }
                    else
                    {
                        ListINP[i].Num.Background = Brushes.Red;
                    }
                }
                else if (RadioButtonInputBin.IsChecked == true)
                {
                    try
                    {
                        int result = Convert.ToInt32(ListINP[i].Box.Text, 2);

                        if (result < 256)
                        {
                            ListINP[i].QueueInput.Enqueue(result);
                            ListINP[i].LastInput.Content = result.ToString("X2");
                            ListINP[i].Box.Text = "";
                            ListINP[i].Num.Background = Brushes.LightGreen;
                        }
                        else
                        {
                            ListINP[i].Num.Background = Brushes.Red;
                        }
                    }
                    catch
                    {
                        ListINP[i].Num.Background = Brushes.Red;
                    }
                }
                else if (RadioButtonInputChar.IsChecked == true)
                {
                    for (int l = 0; l < ListINP[i].Box.Text.Length; l++)
                    {
                        char c = ListINP[i].Box.Text[l];

                        ListINP[i].QueueInput.Enqueue((int)c);
                        ListINP[i].LastInput.Content = ((int)c).ToString("X2");

                    }
                    ListINP[i].Box.Text = "";
                    ListINP[i].Num.Background = Brushes.LightGreen;

                }
            }

        }

        /// <summary>
        /// Aufruf in CPU-OpCode INP, und damit Abarbeiten mit jedem Cpu-Step
        /// </summary>
        /// <param name="inpNum">INP-Nummer</param>
        private void InputToCpu(int inpNum)
        {
            if (ListINP[inpNum].QueueInput.Count > 0)
            {
                INP[inpNum] = ListINP[inpNum].QueueInput.Peek();

                // Wenn Setting !=0, QueueInput.Dequeue mit OUTn
                if (ListINP[inpNum].Setting == 0)
                    INP[inpNum] = ListINP[inpNum].QueueInput.Dequeue();
            }
        }



        private void ButtonInputConfig_Click(object sender, RoutedEventArgs e)
        {
            InputNumIsChanged = false;

            if (StackPanelInputConfig.Visibility == Visibility.Hidden)
                StackPanelInputConfig.Visibility = Visibility.Visible;
            else
                StackPanelInputConfig.Visibility = Visibility.Hidden;

        }

        bool waitForIniInput = true;
        bool InputNumIsChanged = false;
        private void ComboBoxInputNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (waitForIniInput)
                return;
            int selInputNum = ((ComboBox)sender).SelectedIndex;
            InputNumIsChanged = true;
            ComboBoxInputSetting.SelectedIndex = ListINP[selInputNum].Setting;
            InputNumIsChanged = false;

        }

        private void ComboBoxInputSetting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (waitForIniInput || InputNumIsChanged)
                return;
            int selInputNum = ComboBoxInputNum.SelectedIndex;
            int selInputSetting = ((ComboBox)sender).SelectedIndex;

            for (int i = 1; i < 8; i++)
            {
                ListINP[i].Num.Background = Brushes.White;

                ListEf[ListINP[i].Setting].Box.Content = "";
                ListEf[ListINP[i].Setting].Box.IsEnabled = false;
                ListEf[ListINP[i].Setting].LabelNum.Background = Brushes.White;
                ListOUT[ListINP[i].Setting].Num.Background = Brushes.White;
            }

            ListINP[selInputNum].Setting = selInputSetting;

            for (int i = 1; i < 8; i++)
            {


                if (ListINP[i].Setting == 1 || ListINP[i].Setting == 2 || ListINP[i].Setting == 3 || ListINP[i].Setting == 4)
                {
                    ListINP[i].Num.Background = Brushes.YellowGreen;
                    ListEf[ListINP[i].Setting].Box.Content = "send with INP " + ListINP[i];
                    ListEf[ListINP[i].Setting].Box.IsEnabled = false;
                    ListEf[ListINP[i].Setting].LabelNum.Background = Brushes.YellowGreen;
                    ListOUT[ListINP[i].Setting].Num.Background = Brushes.YellowGreen;
                }
            }
            if (!InputNumIsChanged)
                StackPanelInputConfig.Visibility = Visibility.Hidden;

            InputNumIsChanged = false;
        }

        bool InputActiv = false;
        private void StackPanelInputs_MouseEnter(object sender, MouseEventArgs e)
        {
            InputActiv = true;
        }

        private void StackPanelInputs_MouseLeave(object sender, MouseEventArgs e)
        {
            InputActiv = false;
        }
    }
}
