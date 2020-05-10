using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MyCpu1805_05
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        List<CJump> ListJump = new List<CJump>();
        List<CProg> ListProg = new List<CProg>();
        List<CReplace> ListReplace = new List<CReplace>();
        List<CEditLine> ListEditLines = new List<CEditLine>();

        (int line, Brush Color) EditLineSave = (-1, null);


        List<COpCode> ListOpCodes = new List<COpCode>();

        Queue<string> QueueLogFile = new Queue<string>();  // 'Info mit Datum' ';' 'PXD0123456789ABCDEF'



        //    Stand 20200414 8:34     string RcaFileLoaded = "";

        Brush ListViewOpCodeBrushSave = null;
        ListViewItem ListViewOpCodeLviSave = null;

        DispatcherTimer TimerColorListViewOpCode = new DispatcherTimer();
        DispatcherTimer TimerExe = new DispatcherTimer();
        bool ExeStop = true;



        bool BreakpointsOn = false;
        bool BreakpointReached = false;
        string BreakLogTyp = "B";
        int LogFileMaxLines = 1000;
        int LogFileLineCounter = 0;
        public MainWindow()
        {
            InitializeComponent();
            Ini();
            IniInput();
            IniEfs();
            IniOutput();
            MyMonitor = new Monitor(); //wegen statischer Variablen
        }

        public void Ini()
        {



            IniMem();

            ListViewMem.ItemsSource = ListMem;
            ListViewBreakpoints.ItemsSource = ListBreakpoints;


            ReadOpCodeTextFiles();

            IniRegister();

            LabelLineInfoTitle.Content = "Line-Info";  //Breakpoint/Logpoint
            StackPanelBreakLog.Visibility = Visibility.Hidden;
            TextBoxLineInfo.Visibility = Visibility.Visible;


            #region Spaltenbreite AutoSize in Listen-Prog / Jump
            GridViewColumn0ListViewOpCode.Width = GridViewColumn0ListViewOpCode.ActualWidth;
            GridViewColumn0ListViewOpCode.Width = Double.NaN;
            GridViewColumn1ListViewOpCode.Width = GridViewColumn1ListViewOpCode.ActualWidth;
            GridViewColumn1ListViewOpCode.Width = Double.NaN;
            GridViewColumn2ListViewOpCode.Width = GridViewColumn2ListViewOpCode.ActualWidth;
            GridViewColumn2ListViewOpCode.Width = Double.NaN;
            GridViewColumn3ListViewOpCode.Width = GridViewColumn3ListViewOpCode.ActualWidth;
            GridViewColumn3ListViewOpCode.Width = Double.NaN;
            GridViewColumn4ListViewOpCode.Width = GridViewColumn4ListViewOpCode.ActualWidth;
            GridViewColumn4ListViewOpCode.Width = Double.NaN;
            GridViewColumn5ListViewOpCode.Width = GridViewColumn5ListViewOpCode.ActualWidth;
            GridViewColumn5ListViewOpCode.Width = Double.NaN;
            GridViewColumn6ListViewOpCode.Width = GridViewColumn6ListViewOpCode.ActualWidth;
            GridViewColumn6ListViewOpCode.Width = Double.NaN;
            GridViewColumn7ListViewOpCode.Width = GridViewColumn7ListViewOpCode.ActualWidth;
            GridViewColumn7ListViewOpCode.Width = Double.NaN;
            GridViewColumn8ListViewOpCode.Width = GridViewColumn8ListViewOpCode.ActualWidth;
            GridViewColumn8ListViewOpCode.Width = Double.NaN;
            #endregion

            TimerColorListViewOpCode.Interval = TimeSpan.FromMilliseconds(1);
            TimerColorListViewOpCode.Tick += TimerColorListViewOpCode_Tick;
            ViewOpCodeColor = false;


            CheckBoxDebug.IsChecked = false;
            RadioButtonCpuModeSingleStep.Visibility = Visibility.Hidden;
            RadioButtonCpuModeFetchExecute.Visibility = Visibility.Hidden;

            TextBoxBreak.Visibility = Visibility.Collapsed;
            TextBoxBreak2.Visibility = Visibility.Collapsed;
            ButtonBreakpointYes.Visibility = Visibility.Collapsed;
            ButtonBreakpointNo.Visibility = Visibility.Collapsed;

            TimerExe.Interval = TimeSpan.FromTicks(1);
            TimerExe.Tick += TimerExe_Tick;




        }

        private void IniRegister()
        {
            ListRegister.Clear();

            for (int i = 0; i < 16; i++)
            {
                R[i] = 0;
                CRegister r = new CRegister { Nr = i.ToString("X1"), Val = R[i].ToString(), P = "" };
                ListRegister.Add(r);
            }


            ListViewRegister.ItemsSource = ListRegister;

            ShowRegister();
        }



        private void ReadOpCodeTextFiles()
        {
            string[] lines = File.ReadAllLines("OpCodes1802.txt");
            //Nr;OpCodeLong; OpCode;OpCodeHex;OpCodeSyntax;OpCodeDuble;OpCodeBytes;OpCodeSignNextByte;OpCodeInfo
            //1 |LOAD VIA N |LDN   |01       |LDN 1       |0          |1          |                  |M(R(1)) -> D 
            //public int Num { get; set; }
            //public string Long { get; set; }
            //public string Short { get; set; }
            //public string Hex { get; set; }
            //public string Syntax { get; set; }
            //public int Register { get; set; }
            //public int Double { get; set; }
            //public int Bytes { get; set; }
            //public string SignNextByte { get; set; }
            //public string Info { get; set; }

            foreach (string line in lines)
            {
                string[] items = line.Split('|');

                COpCode code = new COpCode();
                code.Num = Convert.ToInt32(items[0].Trim());
                code.Long = items[1].Trim();
                code.Short = items[2].Trim();
                code.Hex = items[3].Trim();
                code.Syntax = items[4].Trim();
                code.Register = -1;
                if (code.Syntax.Split().Length == 2)
                {
                    string reg = code.Syntax.Split()[1].Trim();
                    code.Register = Convert.ToInt32(reg, 16);
                }

                code.Double = Convert.ToInt32(items[5].Trim());
                code.Bytes = Convert.ToInt32(items[6].Trim());
                code.SignNextByte = items[7].Trim();
                code.Info = items[8].Trim();
                if (items[9] != "")
                    code.Singles = Convert.ToInt32(items[9].Trim());
                if (items[10] != "")
                    code.OrderByFunction = items[10].Trim().PadLeft(3, '0');
                else
                    code.OrderByFunction = "999";

                ListOpCodes.Add(code);
            }

            lines = File.ReadAllLines("OpCodes1805.txt");

            foreach (string line in lines)
            {
                string[] items = line.Split('|');

                COpCode code = new COpCode();
                code.Num = Convert.ToInt32(items[0].Trim());
                code.Long = items[1].Trim();
                code.Short = items[2].Trim();
                code.Hex = items[3].Trim();
                code.Syntax = items[4].Trim();
                code.Register = -1;
                if (code.Syntax.Split().Length == 2)
                {
                    string reg = code.Syntax.Split()[1].Trim();
                    code.Register = Convert.ToInt32(reg, 16);
                }

                if (items[5] != "")
                    code.Double = Convert.ToInt32(items[5].Trim());
                else
                    code.Double = 0;

                if (items[6] != "")
                    code.Bytes = Convert.ToInt32(items[6].Trim());
                else
                    code.Bytes = 0;

                code.SignNextByte = items[7].Trim();
                code.Info = items[8].Trim();

                if (items[9] != "")
                    code.Singles = Convert.ToInt32(items[9].Trim());
                if (items[10] != "")
                    code.OrderByFunction = items[10].Trim().PadLeft(3, '0');
                else
                    code.OrderByFunction = "999";

                ListOpCodes.Add(code);
            }


            ListViewOpCode.ItemsSource = ListOpCodes;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewOpCode.ItemsSource);
            view.Filter = ListViewOpCodeFilter;



        }

        string ListViewOpCodeFilterExtern = "";
        private bool ListViewOpCodeFilter(object item)
        {
            if (String.IsNullOrEmpty(ListViewOpCodeFilterExtern))
                return true;
            else
                return ((item as COpCode).Short.IndexOf(ListViewOpCodeFilterExtern, StringComparison.OrdinalIgnoreCase) >= 0);
        }






        private void WindowCpu1805Main_Loaded(object sender, RoutedEventArgs e)
        {

            IniMyFolder();
            CpuReset();

            if (RcaFile.Long != null)
                ShowFileName(RcaFile.Long);
        }

        private void ListViewOpCode_Loaded(object sender, RoutedEventArgs e)
        {
            ListViewOpCodeFilterExtern = "";
            ListViewOpCodeSortStandard();
        }

        private void ButtonListViewOpCodeStandard_Click(object sender, RoutedEventArgs e)
        {
            ListViewOpCodeFilterExtern = "";
            ListViewOpCodeSortStandard();
        }

        private void WindowCpu1805Main_Closing(object sender, CancelEventArgs e)
        {
            MyMonitor.Close();

            if(HelpBox!=null)
                HelpBox.Close();


        }
    }
}
