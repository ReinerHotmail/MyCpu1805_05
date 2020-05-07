using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MyCpu1805_05
{
    public partial class MainWindow
    {
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        #region Editor Clicks and Mouse
        private void ButtonBreakpointYes_Click(object sender, RoutedEventArgs e)
        {
            CMemView memSelect = (CMemView)ListViewMem.SelectedItem;

            if (memSelect == null)
                return;

            SetNewBreakpoint(null, 1, memSelect);
        }

        private void ButtonBreakpointNo_Click(object sender, RoutedEventArgs e)
        {
            CMemView memSelect = (CMemView)ListViewMem.SelectedItem;

            if (memSelect == null)
                return;

            SetNewBreakpoint(null, 2, memSelect);
        }

        private void ButtonLoadClipboard_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonSaveProgr_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "RCA file (*.rca)|*.rca";
            saveFileDialog.FileName = RcaFile.Name;

            if (saveFileDialog.ShowDialog() == true)
            {
                string nameLong = saveFileDialog.FileName;

                ShowFileName(nameLong);

                RcaFile.Long = nameLong;
                RcaFile.Path = nameLong[0..nameLong.LastIndexOf("\\")];
                RcaFile.Name = nameLong[(nameLong.LastIndexOf('\\') + 1)..];


                TextRange textRange = new TextRange(RichTextBoxEditor.Document.ContentStart, RichTextBoxEditor.Document.ContentEnd);
                File.WriteAllText(saveFileDialog.FileName, textRange.Text);

                PutSetting("PathRca", RcaFile.Path);


            }

        }

        private void ButtonLoadProgr_Click(object sender, RoutedEventArgs e)
        {
            LoadRcaFile(false);

        }
        private void ButtonLoadHex_Click(object sender, RoutedEventArgs e)
        {
            LoadRcaFile(true);
        }
        private void ButtonFileNew_Click(object sender, RoutedEventArgs e)
        {
            string fileName = "Prog01";

            if (TextBoxNewFileName.Text != "")
                fileName = TextBoxNewFileName.Text;

            RichTextBoxEditor.AppendText("#Prog " + fileName + " x0000\nStart:");
            RichTextBoxEditor.Focus();
            RichTextBoxEditor.CaretPosition = RichTextBoxEditor.Document.ContentEnd;


        }

        private void ButtonEditorFind_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonEditorFindNext_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonEditorClear_Click(object sender, RoutedEventArgs e)
        {
            RichTextBoxEditor.Document.Blocks.Clear();
        }

        private void ButtonCompile_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(RichTextBoxEditor.Document.ContentStart, RichTextBoxEditor.Document.ContentEnd);
            string s = textRange.Text;
            //  string[] lines = s.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            string[] lines = s.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);

            ListEditLines.Clear();

            for (int i = 0; i < lines.Length; i++)
            {
                ListEditLines.Add(new CEditLine(i, lines[i].Trim()));
            }

            CompileFirstSteps();

        }

        private void ButtonLoadMem_Click(object sender, RoutedEventArgs e)
        {
            if (DocPanelEditor.Background != Brushes.LightGreen)
                return;


            if (RcaFile.Name == null)
            {
                MessageBox.Show("Programm muss zuerst gespeichert werde (Save)");
                return;
            }

            ListBreakpoints.Clear();
            ListViewBreakpoints.Items.Refresh();

            int adrFirst = CompileLastStep();

            ShowMem(adrFirst);
            ShowMemCpu();

            DocPanelMemNew.Background = Brushes.LightGreen;
            DocPanelMemCpu.Background = Brushes.LightGreen;
        }

        private int CompileLastStep()
        {
            int adrFirst = -1;

            for (int i = 0; i < ListEditLines.Count; i++)
            {
                string allBytes = ListEditLines[i].Byte1 + ListEditLines[i].Byte2;

                if (ListEditLines[i].Adr.Length > 0 && allBytes.Length > 0)
                {
                    int adr = Convert.ToInt32(ListEditLines[i].Adr, 16);



                    if (allBytes.StartsWith("\""))
                    {
                        allBytes = allBytes[1..^1];
                        int len = allBytes.Length;


                        for (int b = 0; b < len; b++)
                        {



                            M[adr] = (byte)allBytes[0];


                            if (adrFirst < 0)
                                adrFirst = adr;

                            if (allBytes.Length > 1)
                            {
                                allBytes = allBytes[1..];
                                adr++;
                            }

                        }

                    }
                    else
                    {


                        int len = allBytes.Length / 2;

                        for (int b = 0; b < len; b++)
                        {


                            M[adr] = (byte)Convert.ToInt32(allBytes[0..2], 16);

                            if (adrFirst < 0)
                                adrFirst = adr;

                            if (allBytes.Length > 1)
                            {
                                allBytes = allBytes[2..];
                                adr++;
                            }

                        }
                    }
                }
            }

            return adrFirst;
        }



        private void TextBoxNewFileName_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void ButtonEditorDirectory_Click(object sender, RoutedEventArgs e)
        {
            //IniMyFolder(true);
            //ShowFileName(RcaFile.Long);

        }


        private void RichTextBoxEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.Changes.Count == 0)
                return;

            DocPanelEditor.Background = Brushes.LightGray;
            ButtonLoadMem.IsEnabled = false;
        }

        private void RichTextBoxEditor_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void RichTextBoxEditor_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
        private void RichTextBoxEditor_KeyUp(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Return)
            {
                ListViewOpCodeFilterExtern = "";
                ListViewOpCodeSortStandard();
                return;
            }

            string currentLine = new TextRange(RichTextBoxEditor.CaretPosition.GetLineStartPosition(0),
                RichTextBoxEditor.CaretPosition.GetLineStartPosition(1) ?? RichTextBoxEditor.CaretPosition.DocumentEnd).Text;

            string[] currentLineSplit = currentLine.Trim().Split(":");
            string lookCode = "";


            if (currentLineSplit.Length == 1)
                lookCode = currentLineSplit[0];
            else if (currentLineSplit.Length == 2)
                lookCode = currentLineSplit[1];

            if (lookCode.Length > 4)
                return;


            string[] strFilter = lookCode.ToUpper().Split();

            if (strFilter.Length > 0 && strFilter[0] != "")
            {
                ListViewOpCodeFilterExtern = strFilter[0].Trim();
                ListViewOpCodeSortStandard();
            }




            //foreach (var item in ListOpCodes )
            //{
            //    if (item.Short.Length > 4)
            //        continue;
            //    if(item.Short.StartsWith(lookCodeUp4.Substring(0,item.Short.Length)))
            //    {
            //        OpCodeViewFilterExtern=
            //        break;

            //    }
            //}


        }

        private void RichTextBoxEditor_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void Editor_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //var textToSync = (sender == RichTextBoxEditor) ? RichTextBoxErr : RichTextBoxEditor;

            //textToSync.ScrollToVerticalOffset(e.VerticalOffset);
            // textToSync.ScrollToHorizontalOffset(e.HorizontalOffset);
        }

        private void TexBoxEditorFind_KeyUp(object sender, KeyEventArgs e)
        {

        }




        private void EditorItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Paragraph x = (Paragraph)sender;
            int num = (int)x.Tag;

            ShowTextBoxLineInfo(num);

            ColorLine_ListViewProg(num);

            ColorLine_ListViewJump(num);

            ColorLine_ListViewBreakpoints(num);

            int listViewOpCodeIdx = ColorLine_ListViewOpCode(num);

            ShowTextBoxOpCodeInfo(listViewOpCodeIdx);

            if (ListEditLines[num].Adr.Length > 0)
            {
                int showAdr = Convert.ToInt32(ListEditLines[num].Adr, 16);
                ColorLine_ListViewMem(showAdr);
            }
            else
            {
                int showAdr = 0;
                ShowMem(showAdr);
            }
        }

        private void EditorItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (FileNameSaved == "")
                return;

            LabelEditorProgName.Content = FileNameSaved;
            LabelEditorProgName.Background = Brushes.White;
            FileNameSaved = "";

        }

        string FileNameSaved = "";
        private void EditorItem_MouseEnter(object sender, MouseEventArgs e)
        {
            Paragraph x = (Paragraph)sender;
            int num = (int)x.Tag;



            if (ListEditLines[num].ErrLine != "")
            {
                FileNameSaved = (string)LabelEditorProgName.Content;
                LabelEditorProgName.Content = ListEditLines[num].ErrLine;
                LabelEditorProgName.Background = Brushes.Violet;
            }

        }




        private void ListViewMem_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CMemView memSelect = (CMemView)ListViewMem.SelectedItem;

            if (memSelect == null)
                return;

            CBreak breakInList = ListBreakpoints.Find(x => x.Adr == Convert.ToInt32(memSelect.Adr, 16));

            CEditLine lineFound = ListEditLines.Find(x => x.Adr == memSelect.Adr);

            if (lineFound == null)
                return;

            if (breakInList != null && breakInList.Condition != "")
            {
                #region Breakpoint löschen
                MessageBoxResult result = MessageBox.Show(breakInList.Term1 + breakInList.Comp + breakInList.Term2, "Break-/Logpoint löschen ? '" + memSelect.Adr + "'", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    ListBreakpoints.Remove(breakInList);

                    ListViewBreakpoints.Items.Refresh();

                    CMemView mv = (CMemView)ListViewMem.Items[0];
                    ShowMem(Convert.ToInt32(mv.Adr, 16) + 8);
                }
                return;

                #endregion
            }

            LabelLineInfoTitle.Content = "Breakpoint/Logpoint";  //Line-Info
            StackPanelBreakLog.Visibility = Visibility.Visible;
            TextBoxLineInfo.Visibility = Visibility.Hidden;


            return;


            MessageBoxResult result2 =
            MessageBox.Show("Breakpoint setzen\t\tYes\nLogpoint setzen\t\tNo\nAbbruch\t\t\tCancel", "Breakpoint oder Logpoint", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result2 == MessageBoxResult.Yes)
            {

                SetBreakpoint("B", memSelect);
                TextBoxBreak.Focus();
            }

            if (result2 == MessageBoxResult.No)
            {

                SetBreakpoint("L", memSelect);
                TextBoxBreak.Focus();
            }

        }



        private void ButtonBreakLogOk_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonBreakpoint.IsChecked == true)
            {
                CMemView memSelect = (CMemView)ListViewMem.SelectedItem;
                SetBreakpoint("B", memSelect);
                TextBoxBreak.Focus();
                LabelLineInfoTitle.Content = "Line-Info";  //Breakpoint/Logpoint
                StackPanelBreakLog.Visibility = Visibility.Hidden;
                TextBoxLineInfo.Visibility = Visibility.Visible;
            }

            if (RadioButtonLogpoint.IsChecked == true)
            {
                CMemView memSelect = (CMemView)ListViewMem.SelectedItem;
                SetBreakpoint("L", memSelect);
                TextBoxBreak.Focus();
                LabelLineInfoTitle.Content = "Line-Info";  //Breakpoint/Logpoint
                StackPanelBreakLog.Visibility = Visibility.Hidden;
                TextBoxLineInfo.Visibility = Visibility.Visible;
            }
        }

        private void ButtonBreakLogCancel_Click(object sender, RoutedEventArgs e)
        {
            LabelLineInfoTitle.Content = "Line-Info";  //Breakpoint/Logpoint
            StackPanelBreakLog.Visibility = Visibility.Hidden;
            TextBoxLineInfo.Visibility = Visibility.Visible;
        }




        private void TextBoxBreak_KeyUp(object sender, KeyEventArgs e)
        {
            CMemView memSelect = (CMemView)ListViewMem.SelectedItem;

            if (memSelect == null)
                return;

            SetNewBreakpoint(e, 0, memSelect);
        }




        //ToDo ListViewMem_PreviewMouseLeftButtonUp
        private void ListViewMem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {


            CMemView lvi = (CMemView)ListViewMem.SelectedItem;

            if (lvi == null)
                return;


            CEditLine lineFound = ListEditLines.Find(x => x.Adr == lvi.Adr);

            if (lineFound == null)
                return;

            ShowTextBoxLineInfo(lineFound.Num);

            ColorLine_ListViewProg(lineFound.Num);

            ColorLine_ListViewJump(lineFound.Num);

            ColorLine_ListViewBreakpoints(lineFound.Num);

            int listViewOpCodeIdx = ColorLine_ListViewOpCode(lineFound.Num);

            ShowTextBoxOpCodeInfo(listViewOpCodeIdx);

            ColorLine_ListViewMem(Convert.ToInt32(lvi.Adr, 16));

            ColorLine_Editor(lineFound);

        }



        private void ListViewOpCodeColumnHeader_Click(object sender, RoutedEventArgs e)
        {

            GridViewColumnHeader column = (GridViewColumnHeader)sender;

            string sortBy = column.Tag.ToString();

            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                ListViewOpCode.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            ListViewOpCode.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));

            TimerColorListViewOpCode.Start();
        }
        private void ListViewOpCodeSortStandard()
        {
            GridViewColumnHeader column = ListViewOpCodeColumnHeaderStandard;



            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                ListViewOpCode.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            //if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
            //    newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            ListViewOpCode.Items.SortDescriptions.Add(new SortDescription("OrderByFunction", newDir));

            TimerColorListViewOpCode.Start();

        }




        private void ListViewOpCode_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int i = ListViewOpCode.SelectedIndex;

            if (i < 0)
                return;


            ListViewItem lvi = ListViewOpCode.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;


            if (lvi != null)
            {
                if (ListViewOpCodeLviSave != null)
                    ListViewOpCodeLviSave.Background = ListViewOpCodeBrushSave;

                ListViewOpCodeBrushSave = lvi.Background;
                ListViewOpCodeLviSave = lvi;

                lvi.Background = Brushes.LightBlue;
            }

            ShowTextBoxOpCodeInfo(i);
        }

        private void ListViewProg_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CProg selProg = (CProg)ListViewProg.SelectedItem;

            if (selProg == null)
                return;

            ColorLine_ListViewProg(selProg.LineNum);
            ColorLine_Editor(ListEditLines[selProg.LineNum]);
            ColorLine_ListViewMem(Convert.ToInt32(selProg.Adr, 16));

            //ColorLine_ListViewMem(Convert.ToInt32(selProg.Adr, 16));


        }



        private void ListViewJump_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CJump selJump = (CJump)ListViewJump.SelectedItem;

            if (selJump == null)
                return;

            ColorLine_ListViewJump(selJump.LineNum);
            ColorLine_ListViewBreakpoints(selJump.LineNum);
            ColorLine_Editor(ListEditLines[selJump.LineNum]);
            ColorLine_ListViewMem(Convert.ToInt32(selJump.Adr, 16));

            // ColorLine_ListViewMem(Convert.ToInt32(selJump.Adr, 16));
        }



        private void ListViewBreakpoints_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CBreak selBreak = (CBreak)ListViewBreakpoints.SelectedItem;

            if (selBreak == null)
                return;

            ColorLine_ListViewJump(selBreak.LineNum);
            ColorLine_ListViewBreakpoints(selBreak.LineNum);
            ColorLine_Editor(ListEditLines[selBreak.LineNum]);
            ColorLine_ListViewMem(selBreak.Adr);
        }




        private void ListViewReplaceName_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void ListViewReplaceCmd_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        /// <summary>
        /// Breakpoint-Condition aus TextBox übernehmen
        /// </summary>
        /// <param name="e">Taste ENTER oder ESC</param>
        /// <param name="button">1:OK-Button 2:Cancel-Button</param>
        /// <param name="memSelect">MemSelect</param>
        private void SetNewBreakpoint(KeyEventArgs e, int button, CMemView memSelect)
        {
            bool ok = false;
            bool cancel = false;

            if (button == 1)
                ok = true;

            if (button == 2)
                cancel = true;


            if (button == 0)
            {
                if (e.Key == Key.Escape)
                    cancel = true;

                if (e.Key == Key.Return)
                    ok = true;
            }

            if (cancel)
            {
                TextBoxBreak.Visibility = Visibility.Collapsed;
                TextBoxBreak2.Visibility = Visibility.Collapsed;
                ButtonBreakpointYes.Visibility = Visibility.Collapsed;
                ButtonBreakpointNo.Visibility = Visibility.Collapsed;
                return;
            }


            if (!ok)
                return;

            CEditLine lineFound = ListEditLines.Find(x => x.Adr == memSelect.Adr);
            string textBoxText = TextBoxBreak.Text.Trim();


            if (BreakLogTyp == "B")
            {
                if (textBoxText == "B")
                {
                    ListBreakpoints.Add(new CBreak(BreakLogTyp, lineFound.Num, Convert.ToInt32(memSelect.Adr, 16), "", "B", "", TextBoxBreak2.Text.Trim()));
                }

                else
                {
                    (string term1, string comp, string term2) = CBreak.CheckCondition(textBoxText);
                    if (term1 != "")
                    {
                        ListBreakpoints.Add(new CBreak(BreakLogTyp, lineFound.Num, Convert.ToInt32(memSelect.Adr, 16), term1, comp, term2, TextBoxBreak2.Text.Trim()));
                    }
                    else
                    {
                        MessageBox.Show("Breakpoint Condition fehlerhaft");
                        return;
                    }
                }

            }
            else
            {
                if (textBoxText == "L")
                {
                    ListBreakpoints.Add(new CBreak(BreakLogTyp, lineFound.Num, Convert.ToInt32(memSelect.Adr, 16), "", "L", "", TextBoxBreak2.Text.Trim()));
                }

                else
                {
                    (string term1, string comp, string term2) = CBreak.CheckCondition(textBoxText);
                    if (term1 != "")
                    {
                        ListBreakpoints.Add(new CBreak(BreakLogTyp, lineFound.Num, Convert.ToInt32(memSelect.Adr, 16), term1, comp, term2, TextBoxBreak2.Text.Trim()));
                    }
                    else
                    {
                        MessageBox.Show("Logpoint Condition fehlerhaft");
                        return;
                    }
                }
            }



            //ein Eintrag in Liste:             'Info mit Datum' ';' 'PXD0123456789ABCDEF'

            TextBoxBreak.Visibility = Visibility.Collapsed;
            TextBoxBreak2.Visibility = Visibility.Collapsed;
            ButtonBreakpointYes.Visibility = Visibility.Collapsed;
            ButtonBreakpointNo.Visibility = Visibility.Collapsed;


            ListViewBreakpoints.Items.Refresh();


            CMemView mv = (CMemView)ListViewMem.Items[0];

            ShowMem(Convert.ToInt32(mv.Adr, 16) + 8);

        }


        #endregion
    }
}
