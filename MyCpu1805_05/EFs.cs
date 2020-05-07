using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MyCpu1805_05
{
    public partial class MainWindow
    {

        List<CEf> ListEf = new List<CEf>();

        private void IniEfs()
        {
            // alle Objekte in Liste, damit sie per Loop adressiert werden können

            // Eintrag 0  -  nicht genutzt
            CheckBox box = CheckBoxEF1;
            Label labelNum = LabelEF1;
            Label labelVal = LabelEF1Value;
            CEf ef = new CEf() { Box = box, LabelNum = labelNum, LabelVal = labelVal };
            ListEf.Add(ef);

            box = CheckBoxEF1;
            labelNum = LabelEF1;
            labelVal = LabelEF1Value;
            ef = new CEf() { Box = box, LabelNum = labelNum, LabelVal = labelVal };
            ListEf.Add(ef);

            box = CheckBoxEF2;
            labelNum = LabelEF2;
            labelVal = LabelEF2Value;
            ef = new CEf() { Box = box, LabelNum = labelNum, LabelVal = labelVal };
            ListEf.Add(ef);

            box = CheckBoxEF3;
            labelNum = LabelEF3;
            labelVal = LabelEF3Value;
            ef = new CEf() { Box = box, LabelNum = labelNum, LabelVal = labelVal };
            ListEf.Add(ef);

            box = CheckBoxEF4;
            labelNum = LabelEF4;
            labelVal = LabelEF4Value;
            ef = new CEf() { Box = box, LabelNum = labelNum, LabelVal = labelVal };
            ListEf.Add(ef);

        }

        private void Ef_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxEF1.IsChecked == true)
            {
                EF1 = true;
                ListEf[1].Val = true;
            }
            else
            {
                EF1 = false;
                ListEf[1].Val = false;
            }


            if (CheckBoxEF2.IsChecked == true)
            {
                EF2 = true;
                ListEf[2].Val = true;
            }
            else
            {
                EF2 = false;
                ListEf[2].Val = false;
            }
            if (CheckBoxEF3.IsChecked == true)
            {
                EF3 = true;
                ListEf[3].Val = true;
            }
            else
            {
                EF3 = false;
                ListEf[3].Val = false;
            }
            if (CheckBoxEF4.IsChecked == true)
            {
                EF4 = true;
                ListEf[4].Val = true;
            }
            else
            {
                EF4 = false;
                ListEf[4].Val = false;
            }
        }


    }
}
