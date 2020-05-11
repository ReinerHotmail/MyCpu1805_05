using System;
using System.Collections.Generic;
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
        private void LoadRcaFile(bool hex)
        {
            #region File->ListEditLines mit OpenFileDialog

            string[] lines;


            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            if (hex)
                openFileDialog.Filter = "Text-Files  | *.txt";
            else
                openFileDialog.Filter = "RCA-Files  | *.rca";


            if (openFileDialog.ShowDialog() != true)
                return;

            string nameLong = openFileDialog.FileName;

            RcaFile.Long = nameLong;
            RcaFile.Path = nameLong[0..nameLong.LastIndexOf("\\")];
            RcaFile.Name = nameLong[(nameLong.LastIndexOf('\\') + 1)..];

            PutSetting("PathRca", RcaFile.Path);

            ShowFileName(nameLong);
            lines = File.ReadAllLines(nameLong, Encoding.UTF8);



            ListEditLines.Clear();

            for (int i = 0; i < lines.Length; i++)
            {
                ListEditLines.Add(new CEditLine(i, lines[i].Trim()));
            }
            #endregion

            if (!hex)
                CompileFirstSteps();
            else
                HexToRca();
        }

        private void HexToRca()
        {
            string longLine = "";

            #region alle NICHT-Hex raus
            foreach (CEditLine edLine in ListEditLines)
            {
                string line = "";
                foreach (char item in edLine.Original)
                {
                    if (CStat.IsHex(item.ToString()))
                    {
                        longLine += item.ToString();
                    }
                }

            }
            #endregion



            ListEditLines.Clear();
            int lineNum = 0;
            int k = 0;


            try
            {

                for (k = 0; k < longLine.Length; k += 2)
                {
                    string byt = longLine[k..(k + 2)].ToUpper();

                    COpCode op = ListOpCodes.Find(x => x.Hex == byt);

                    if (op != null)
                    {
                        ListEditLines.Add(new CEditLine(lineNum, "Line" + lineNum + "  Column" + k));
                        ListEditLines[lineNum].OpCodeLeft = op.Short;
                        ListEditLines[lineNum].Byte1 = op.Hex;
                        switch (op.Bytes)
                        {
                            case 1:
                                if (op.Syntax.Contains(" "))
                                {
                                    if (op.Short == "INP")
                                    {
                                        int num = Convert.ToInt32(op.Hex[1].ToString(), 16);
                                        num -= 8;
                                        ListEditLines[lineNum].OpCodeRight = num.ToString("X1");
                                    }
                                    else
                                    {
                                        ListEditLines[lineNum].OpCodeRight = op.Hex[1].ToString();
                                    }

                                }

                                break;
                            case 2:
                                byt = longLine[(k + 2)..(k + 4)].ToUpper();
                                ListEditLines[lineNum].OpCodeRight = "x" + byt;
                                ListEditLines[lineNum].Byte2 = byt;
                                k += 2;
                                break;
                            case 3:
                                byt = longLine[(k + 2)..(k + 6)].ToUpper();
                                ListEditLines[lineNum].OpCodeRight = "x" + byt;
                                ListEditLines[lineNum].Byte2 = byt;
                                k += 4;
                                break;
                            default:
                                break;
                        }
                        lineNum += 1;
                    }

                }
            }
            catch (Exception)
            {

                MessageBox.Show("Fehler in Spalte " + k + "\n" + longLine[(k - 3)..]);
            }

            ShowEditLines();

        }

        private void ShowFileName(string file)
        {
            string s = file;

            if (file.Length > 56)
                s = file[0..36] + "  . . .  " + file[^20..];

            LabelEditorProgName.Content = s;


        }


        private bool ShowEditLines()
        {
            bool noErr = true;

            (int maxLeft, int maxOpLeft, int maxOpRight) = GetDimLine();


            int maxLines = ListEditLines.Count;

            foreach (CEditLine item in ListEditLines.Reverse<CEditLine>())
            {
                if (item.Original.Length < 1)
                    maxLines += -1;
                else
                    break;
            }

            RichTextBoxEditor.Document.Blocks.Clear();

            bool b = false;
            CEditLine line = null;

            for (int i = 0; i < maxLines; i++)
            {
                line = ListEditLines[i];

                Run run = new Run();
                Paragraph p = new Paragraph(run);
                line.Para = p;   // zum späteren Einfärben
                p.Tag = line.Num;
                p.MouseEnter += EditorItem_MouseEnter;
                p.MouseLeave += EditorItem_MouseLeave;
                p.PreviewMouseLeftButtonDown += EditorItem_MouseLeftButtonUp;


                RichTextBoxEditor.Document.Blocks.Add(p);

                if (line.ErrLine != "")
                {
                    noErr = false;
                    run.Text = line.Original;
                    p.Background = Brushes.Red;
                    continue;

                }

                if (line.OpCodeLeft.Length > 0)
                {
                    string cRight = " ";

                    if (line.commentRight.Length > 0)
                        cRight = "/";

                    string dp = " ";
                    if (line.LeftJump.Length > 0)
                        dp = ":";

                    run.Text = line.LeftJump.PadRight(maxLeft) + dp + line.OpCodeLeft.PadRight(maxOpLeft) + " " + line.OpCodeRight.PadRight(maxOpRight) + cRight + line.commentRight;
                }
                else if (line.OpCodeRight.Length > 0)
                {
                    string cRight = " ";

                    if (line.commentRight.Length > 0)
                        cRight = "/";

                    string dp = " ";
                    if (line.LeftJump.Length > 0)
                        dp = ":";

                    run.Text = line.LeftJump.PadRight(maxLeft) + dp + line.OpCodeRight.PadRight(maxOpRight) + cRight + line.commentRight;
                }
                else if (line.LineProgram != "")
                    run.Text = line.LineProgram;
                else if (line.LineReplace != "")
                    run.Text = line.LineReplace;
                else if (line.LineComment != "")
                    run.Text = line.LineComment;


                if (line.ProgColor)
                    line.Para.Background = Brushes.Moccasin;
                else
                    line.Para.Background = Brushes.Tan;
            }


            return noErr;
        }

        private void ShowTextBoxLineInfo(int lineNum)
        {
            string s = "Line".PadRight(15) + ListEditLines[lineNum].Num + Environment.NewLine;
            s += "Adr".PadRight(15) + ListEditLines[lineNum].Adr + Environment.NewLine;
            s += "ByteLen".PadRight(15) + ListEditLines[lineNum].ByteLen + Environment.NewLine;
            s += "Byte1".PadRight(15) + ListEditLines[lineNum].Byte1 + Environment.NewLine;
            s += "Byte2".PadRight(15) + ListEditLines[lineNum].Byte2 + Environment.NewLine;
            s += "ErrLine".PadRight(15) + ListEditLines[lineNum].ErrLine + Environment.NewLine;
            s += "OpCodeLeft".PadRight(15) + ListEditLines[lineNum].OpCodeLeft + Environment.NewLine;
            s += "OpCodeRight".PadRight(15) + ListEditLines[lineNum].OpCodeRight + Environment.NewLine;
            s += "Original".PadRight(15) + ListEditLines[lineNum].Original + Environment.NewLine;
            s += "LineComment".PadRight(15) + ListEditLines[lineNum].LineComment + Environment.NewLine;
            s += "LineProgram".PadRight(15) + ListEditLines[lineNum].LineProgram + Environment.NewLine;
            s += "ProgColor".PadRight(15) + ListEditLines[lineNum].ProgColor + Environment.NewLine;
            s += "LineReplace".PadRight(15) + ListEditLines[lineNum].LineReplace + Environment.NewLine;
            s += "LeftJump".PadRight(15) + ListEditLines[lineNum].LeftJump + Environment.NewLine;
            s += "commentRight".PadRight(15) + ListEditLines[lineNum].commentRight + Environment.NewLine;
            TextBoxLineInfo.Text = s;
        }


        private void ShowTextBoxOpCodeInfo(int listViewOpCodeIdx)
        {
            if (listViewOpCodeIdx < 0)
                return;

            COpCode opCode = (COpCode)ListViewOpCode.Items[listViewOpCodeIdx];


            TextBoxOpCodeInfo.Text = opCode.Short + "      " + opCode.Long;

            string[] info = opCode.Info.Split(';');
            ListBoxOpCodeInfo.Items.Clear();

            foreach (string inf in info)
            {
                ListBoxOpCodeInfo.Items.Add(inf.Trim());
            }

        }



        private void DoStar(int bytes, CEditLine line, COpCode foundOpCode, string opCodeTerm2)
        {
            // bytes = 1:  
            // BR *Loop
            // LDI *Prog2.Start.0 / 1Byte AdrLow
            // LDI *Prog2.Start.1 / 1Byte AdrHigh
            // LDI *Prog2.Start - Fehler 2Byte bei LDI nicht möglich  

            // bytes = 2:  
            // LBR *Prog2.Start 

            if (!opCodeTerm2.StartsWith("*"))
            {
                line.ErrLine = "'Syntax *   ist falsch  -  richtig zB  LDI *Prog2.Start.0  oder BR *Loop   oder BR *-2";
                line.ByteLen = -1;
                return;
            }
            string[] opCodeTerm2SplitP = opCodeTerm2.Split('.');

            bool c6805 = false;

            if (foundOpCode.Hex[..2] == "68")
                c6805 = true;

            if (bytes == 2)
            {
                // BR *Loop
                // LDI *Prog2.Start.0 / 1Byte AdrLow
                // LDI *Prog2.Start.1 / 1Byte AdrHigh
                if (!c6805 && opCodeTerm2SplitP.Length != 1 && opCodeTerm2SplitP.Length != 3)
                {
                    line.ErrLine = "'Syntax *   ist falsch  -  richtig zB  LDI *Prog2.Start.0  oder BR *Loop   oder BR *-2";
                    line.ByteLen = -1;
                    return;
                }
                if (c6805 && opCodeTerm2SplitP.Length != 2)
                {
                    line.ErrLine = "'Syntax *   ist falsch  -  richtig zB  DBNZ *Prog2.Start";
                    line.ByteLen = -1;
                    return;
                }
                if (opCodeTerm2SplitP.Length == 1)
                {
                    // BR *Loop
                    // BR *-0  *+0  bis *-9  *+9
                    line.OpCodeRight = opCodeTerm2SplitP[0];
                    line.ByteLen = 2;
                    line.Byte1 = foundOpCode.Hex.ToUpper();
                    return;
                }

                if (!c6805 && opCodeTerm2SplitP.Length == 3)
                {
                    // LDI *Prog2.Start.0 / 1Byte AdrLow
                    // LDI *Prog2.Start.1 / 1Byte AdrHigh
                    // DBNZ *Prog2.Start
                    bool end0 = opCodeTerm2SplitP[2].EndsWith("0");
                    bool end1 = opCodeTerm2SplitP[2].EndsWith("1");

                    if (!end0 & !end1)
                    {
                        line.ErrLine = "'Syntax *   ist falsch  -  richtig zB LDI *Prog2.Start.0";
                        line.ByteLen = -1;
                        return;
                    }
                    else
                    {

                        line.OpCodeRight = opCodeTerm2SplitP[0] + "." + opCodeTerm2SplitP[1] + "." + opCodeTerm2SplitP[2];
                        line.ByteLen = 2;
                        line.Byte1 = foundOpCode.Hex.ToUpper();
                        return;

                    }
                }
                if (c6805 && opCodeTerm2SplitP.Length == 2)
                {
                    // LDI *Prog2.Start.0 / 1Byte AdrLow
                    // LDI *Prog2.Start.1 / 1Byte AdrHigh
                    // DBNZ *Prog2.Start

                    line.OpCodeRight = opCodeTerm2SplitP[0] + "." + opCodeTerm2SplitP[1];
                    line.ByteLen = 6;
                    //line.Byte1 =vorher beschrieben
                    return;

                }
            }
            else if (bytes == 3)
            {
                // LBR *Prog2.Start 
                if (opCodeTerm2SplitP.Length != 2)
                {
                    line.ErrLine = "'Syntax *   ist falsch  -  richtig zB  LBR *Prog2.Start";
                    line.ByteLen = -1;
                    return;
                }

                // LBR *Prog2.Start 
                line.OpCodeRight = opCodeTerm2;
                line.ByteLen = 3;
                line.Byte1 = foundOpCode.Hex.ToUpper();
                return;

            }
        }

        private void AutoSizeLists()
        {
            #region Spaltenbreite AutoSize in Listen-Prog / Jump
            GridViewColumn0ListViewProg.Width = GridViewColumn0ListViewProg.ActualWidth;
            GridViewColumn0ListViewProg.Width = Double.NaN;

            GridViewColumn0ListViewJump.Width = GridViewColumn0ListViewJump.ActualWidth;
            GridViewColumn0ListViewJump.Width = Double.NaN;
            GridViewColumn1ListViewJump.Width = GridViewColumn1ListViewJump.ActualWidth;
            GridViewColumn1ListViewJump.Width = Double.NaN;
            GridViewColumn2ListViewJump.Width = GridViewColumn2ListViewJump.ActualWidth;
            GridViewColumn2ListViewJump.Width = Double.NaN;

            GridViewColumn0ListViewReplace.Width = GridViewColumn0ListViewReplace.ActualWidth;
            GridViewColumn0ListViewReplace.Width = Double.NaN;
            GridViewColumn1ListViewReplace.Width = GridViewColumn1ListViewReplace.ActualWidth;
            GridViewColumn1ListViewReplace.Width = Double.NaN;
            #endregion
        }

        private void CompileFirstSteps()
        {
            DocPanelEditor.Background = Brushes.LightGray;
            ButtonLoadMem.IsEnabled = false;

            if (Compile1())
                Compile2();

            ListViewProg.Items.Clear();
            foreach (CProg item in ListProg)
            {
                ListViewProg.Items.Add(item);
            }

            ListViewJump.Items.Clear();
            foreach (CJump item in ListJump)
            {
                ListViewJump.Items.Add(item);
            }

            ListViewReplace.Items.Clear();
            foreach (CReplace item in ListReplace)
            {
                ListViewReplace.Items.Add(item);
            }

            if (ShowEditLines())
            {
                DocPanelEditor.Background = Brushes.LightGreen;
                ButtonLoadMem.IsEnabled = true;
            }

        }

        private bool Compile1()
        {
            bool compileOk = true;

            ListProg.Clear();
            ListJump.Clear();
            ListReplace.Clear();

            bool progColor = false;

            // ListOpCodes transfer in listOpCodeTemp, da elegante Suche in ObservableCollection nicht funktioniert
            COpCode[] opCodeArr = new COpCode[ListOpCodes.Count];
            ListOpCodes.CopyTo(opCodeArr, 0);
            List<COpCode> listOpCodeTemp = opCodeArr.ToList();


            CEditLine line;

            for (int i = 0; i < ListEditLines.Count; i++)
            {
                line = ListEditLines[i];

                // Fehler bei voriger Zeile ?
                if (i > 0)
                    if (ListEditLines[i - 1].ErrLine != "")
                        compileOk = false;


                line.ProgColor = progColor;

                if (line.Original.StartsWith("/"))
                {
                    line.LineComment = line.Original;
                    line.ByteLen = 0;
                    continue;
                }

                if (line.Original.StartsWith("#"))
                {
                    // entfernen mehrerer Leerzeichen
                    line.Original = System.Text.RegularExpressions.Regex.Replace(line.Original, "[ ]+", " ");

                    if (line.Original.ToUpper().StartsWith("#PROG "))
                    {
                        line.Original = "#Prog " + line.Original[6..];

                        // #Prog asdfg x1234
                        string[] progSplit = line.Original.Split(' ');
                        if (progSplit.Length != 3)
                        {
                            line.ErrLine = "Syntax #Prog ist falsch  richtig zB #Prog MyTest x1A0F";
                            continue;
                        }
                        if (!CStat.IsHex(progSplit[2][1..]))
                        {
                            line.ErrLine = "Hex-Wert ist falsch -   richtig zB #Prog MyTest x1A0F";
                            continue;
                        }
                        if (progSplit[2].Length != 5)
                        {
                            line.ErrLine = "Hex-Wert ist falsch -   richtig zB #Prog MyTest x1A0F";
                            continue;
                        }

                        foreach (CProg prog in ListProg)
                        {
                            if (prog.ProgName == progSplit[1])
                            {
                                line.ErrLine = "Programmname doppelt";
                                break;
                            }
                        }

                        ListProg.Add(new CProg(line.Num, progSplit[1], progSplit[2][1..]));

                        line.LineProgram = line.Original;
                        line.ByteLen = 0;

                        progColor = !progColor; // Programmfarbe wechseln
                        line.ProgColor = progColor;

                        continue;
                    }
                    else if (line.Original.ToUpper().StartsWith("#REPLACE "))
                    {
                        line.Original = "#Replace " + line.Original[9..];

                        // #Replace Stack = 2
                        string[] replaceSplit = line.Original.Split(' ');

                        if (replaceSplit.Length != 4)
                        {
                            line.ErrLine = "Syntax #Replace ist falsch  richtig zB #Replace Stack = 2 oder MyVal = xF4";
                            continue;
                        }


                        if (replaceSplit[1].ToUpper().StartsWith("X"))
                        {
                            line.ErrLine = "Syntax #Replace ist falsch  Name darf nicht mit 'X' beginnen";
                            continue;
                        }

                        // ToDo Replace neu  xDF  muss gehen

                        string hexVal = "";

                        if (replaceSplit[3].ToUpper().StartsWith("X"))
                        {
                            hexVal = replaceSplit[3].ToUpper()[1..];
                        }
                        else
                        {
                            hexVal = replaceSplit[3].ToUpper();

                            if (hexVal.Length != 1)
                            {
                                line.ErrLine = "Parameter ist falsch -   richtig zB #Replace Stack = 2 oder MyVal = xF4";
                                continue;
                            }
                        }

                        if (!CStat.IsHex(hexVal))
                        {
                            line.ErrLine = "Parameter ist falsch -   nur HEX-Werte erlaubt";
                            continue;
                        }



                        CReplace foundName = ListReplace.Find(x => x.Name == replaceSplit[3]);
                        CReplace foundNickName = ListReplace.Find(x => x.NickName == replaceSplit[1]);
                        if (foundName == null && foundNickName == null)
                        {
                            ListReplace.Add(new CReplace(replaceSplit[1], replaceSplit[3]));
                        }
                        else
                        {
                            line.ErrLine = "Replace - Name/Nickname -  schon vergeben";
                            continue;
                        }


                        line.LineReplace = line.Original;
                        line.ByteLen = 0;
                        continue;
                    }
                    else
                    {
                        line.ErrLine = "'Syntax #Prog' oder 'Syntax #Replace' ist falsch";
                        continue;
                    }

                }

                // Mark1: CHRs "klk. . :/ ... ghg" /Comment
                if (line.Original.ToUpper().Contains("CHRS"))
                {
                    int posDp = line.Original.IndexOf(":");
                    int posChrs = line.Original.ToUpper().IndexOf("CHRS");
                    int posHk1 = line.Original.IndexOf("\"");
                    int posHk2 = line.Original.LastIndexOf("\"")+1;
                    int posSl = line.Original.IndexOf("/");

                    // JumpMark?
                    if (posDp > 0 && posDp < posChrs)
                        line.LeftJump = line.Original[..posDp].Trim();

                    // korrekte CHRS-Zeile
                    if (posChrs < posHk1 && posHk1 < posHk2)
                    {
                        line.OpCodeLeft = "CHRS";
                        line.OpCodeRight = line.Original[posHk1..posHk2];
                        line.ByteLen = line.OpCodeRight.Length-2;
                        line.Byte2 = line.OpCodeRight;
                        // CommentRight?
                        if (posSl > posHk1)
                        {
                            line.commentRight = line.Original[posSl..];
                        }
                     
                    }
                    else
                    {
                        line.ErrLine = "Syntax CHRS falsch - richtig zB CHRS \"abcd\" ";
                        continue;
                    }

                }
                if (line.Original.ToUpper().Contains("CHRX"))
                {
                    int chrxPos = line.Original.ToUpper().IndexOf("CHRX");

                    string val = line.Original.ToUpper()[(chrxPos + 5)..].Trim();

                    int posSlash = val.IndexOf("/");
                    string cmdRight = "";

                    if (posSlash > 0)
                    {
                       cmdRight = val[posSlash..];
                       val = val[0..posSlash].Trim();
                       
                    }
                    

        

                    if (val.Length % 2 != 0)
                    {
                        line.ErrLine = "Hex-Zahlen müssen geradzahlige Länge haben";
                        continue;
                    }
                    for (int n = 0; n < val.Length; n++)
                    {
                        if (!CStat.IsHex(val[n].ToString()))
                        {
                            line.ErrLine = "Hex-Wert falsch -  nur 0..99 A..F erlaubt";
                            continue;
                        }
                    }

                    line.OpCodeLeft = "CHRX";
                    line.OpCodeRight = val.ToUpper();
                    line.ByteLen = line.OpCodeRight.Length / 2;
                    line.Byte2 = val;
                    line.commentRight = cmdRight;


                }



                string opCodeTerm = line.Original;

                if (line.Original.Contains(':'))
                {
                    // leftMark : Br *Loop
                    int posDp = line.Original.IndexOf(':');
                    int posSl = line.Original.IndexOf('/');

                    if (posDp > 0 && (posDp < posSl | posSl == -1))
                    {
                        line.LeftJump = line.Original[0..posDp].Trim();

                        if (line.LeftJump.ToUpper() == "START")
                            line.LeftJump = "Start";

                        if (line.LeftJump.Contains(" "))
                        {
                            line.ErrLine = "Sprungziel darf kein Leerzeichen enthalten";
                            continue;
                        }

                        if (ListProg.Count > 0)
                        {
                            foreach (CJump jump in ListJump)
                            {
                                if (jump.ProgName == ListProg[^1].ProgName && jump.JumpName == line.LeftJump)
                                {
                                    line.ErrLine = "Sprungziel ist doppelt";
                                    break;
                                }
                            }
                            ListJump.Add(new CJump(line.Num, ListProg[^1].ProgName, line.LeftJump));
                        }
                        else
                        {
                            // Jump kann keinem Programm zugeordnet werden
                            ListJump.Add(new CJump(line.Num, "?", line.LeftJump));
                        }


                        opCodeTerm = line.Original[(posDp + 1)..].Trim();

                        if (opCodeTerm.ToUpper().StartsWith("CHR"))
                            continue;
                    }
                }

                if (opCodeTerm.Contains('/'))
                {
                    // BR *Loop / Kommentar
                    int posSl = opCodeTerm.IndexOf('/');

                    if (posSl > 0)
                    {
                        line.commentRight = opCodeTerm[(opCodeTerm.LastIndexOf('/') + 1)..];
                        opCodeTerm = opCodeTerm[0..opCodeTerm.LastIndexOf('/')].Trim();
                    }
                    else
                    {
                        line.ErrLine = "Syntax KommentarRechts falsch  - richtig zB 'PHI F / Kommentar' ";
                        continue;
                    }
                }


                // doppelte Leerzeichen entfernen
                opCodeTerm = System.Text.RegularExpressions.Regex.Replace(opCodeTerm, "[ ]+", " ");

                if (opCodeTerm.StartsWith('*'))
                {
                    // *Prog2.Start.1 / 1Byte AdrHigh
                    // *Prog2.Start.0 / 1Byte AdrLow
                    // *Prog2.Start / 2Byte AdrHigh und AdrLow

                    string[] opCodeTermSplit = opCodeTerm.Split('.');

                    if (opCodeTermSplit.Length != 2 && opCodeTermSplit.Length != 3)
                    {
                        line.ErrLine = "'Syntax *   ist falsch  -  richtig zB *Prog2.Start.0";
                        continue;
                    }

                    if (opCodeTermSplit.Length == 2)
                    {
                        line.OpCodeRight = opCodeTermSplit[0] + "." + opCodeTermSplit[1];
                        line.ByteLen = 2;
                    }

                    if (opCodeTermSplit.Length == 3)
                    {
                        bool end0 = opCodeTermSplit[2].EndsWith("0");
                        bool end1 = opCodeTermSplit[2].EndsWith("1");

                        if (!end0 & !end1)
                        {
                            line.ErrLine = "'Syntax *   ist falsch  -  richtig zB *Prog2.Start.0";
                            continue;
                        }
                        else
                        {

                            line.OpCodeRight = opCodeTermSplit[0] + "." + opCodeTermSplit[1] + "." + opCodeTermSplit[2];
                            line.ByteLen = 1;

                        }
                    }

                    continue;
                }





                string[] opCodeTermSplit2 = opCodeTerm.Split();
                int opCodeTermSplit2Len = opCodeTermSplit2.Length;

                if (opCodeTermSplit2Len > 0)
                {
                    opCodeTermSplit2[0] = opCodeTermSplit2[0].ToUpper();
                    COpCode foundOpCode = listOpCodeTemp.Find(x => x.Short == opCodeTermSplit2[0]);
                    if (foundOpCode != null && foundOpCode.Hex.StartsWith("68"))
                    {
                        if (foundOpCode.Short == "")  //nicht realisierte Befehle
                        {
                            line.ByteLen = 0;
                            continue;
                        }
                        if (opCodeTermSplit2Len == 1)
                        {
                            line.ErrLine = "ungültiger 6805-Befehl";
                            return false;
                        }
                        else if (opCodeTermSplit2Len == 2)
                        {
                            // DBNZ 3
                            // DBNZ Stack
                            if (foundOpCode.Register >= 0)
                            {
                                string regTemp = "";


                                for (int k = 0; k < ListReplace.Count; k++)
                                {
                                    if (opCodeTermSplit2[1] == ListReplace[k].NickName)
                                    {
                                        regTemp = ListReplace[k].Name;
                                        break;
                                    }
                                }
                                if (regTemp == "")
                                    regTemp = opCodeTermSplit2[1];


                                if (!CStat.IsHex(regTemp))
                                {
                                    line.ErrLine = "RegisterNr (hex) fehlt";
                                    line.ByteLen = -1;
                                    continue;
                                }
                                if (regTemp.Length != 1)
                                {
                                    line.ErrLine = "RegisterNr (hex) fehlt";
                                    line.ByteLen = -1;
                                    continue;
                                }
                                line.OpCodeLeft = opCodeTermSplit2[0];
                                line.OpCodeRight = opCodeTermSplit2[1];
                                line.Byte1 = foundOpCode.Hex[..3] + regTemp.ToUpper();
                                line.ByteLen = 2;
                                continue;
                            }
                            else
                            {
                                line.ErrLine = "??????????";
                                line.OpCodeRight = "";
                                line.Byte1 = foundOpCode.Hex;
                                continue;
                            }

                        }
                        else if (opCodeTermSplit2Len == 3)
                        {
                            // DBNZ 3    *Prog1.Start
                            // DBNZ Stack   *Prog1.Start

                            if (foundOpCode.Register >= 0)
                            {
                                string regTemp = "";


                                for (int k = 0; k < ListReplace.Count; k++)
                                {
                                    if (opCodeTermSplit2[1] == ListReplace[k].NickName)
                                    {
                                        regTemp = ListReplace[k].Name;
                                        break;
                                    }
                                }
                                if (regTemp == "")
                                    regTemp = opCodeTermSplit2[1];


                                if (!CStat.IsHex(regTemp))
                                {
                                    line.ErrLine = "RegisterNr (hex) fehlt";
                                    line.ByteLen = -1;
                                    continue;
                                }
                                if (regTemp.Length != 1)
                                {
                                    line.ErrLine = "RegisterNr (hex) fehlt";
                                    line.ByteLen = -1;
                                    continue;
                                }
                                line.OpCodeRight = opCodeTermSplit2[0] + " " + opCodeTermSplit2[1];
                                line.Byte1 = foundOpCode.Hex[..3] + regTemp.ToUpper();
                                line.OpCodeLeft = opCodeTermSplit2[0];
                                DoStar(2, line, foundOpCode, opCodeTermSplit2[2]);
                                line.ByteLen = 6;
                                continue;
                            }
                            else
                            {
                                line.ErrLine = "??????????";
                                line.OpCodeRight = "";
                                line.Byte1 = foundOpCode.Hex;
                                continue;
                            }
                        }

                    }
                    if (foundOpCode != null)
                    {

                        line.OpCodeLeft = opCodeTermSplit2[0];
                        if (!foundOpCode.Short.StartsWith("CHR"))
                            line.ByteLen = foundOpCode.Bytes;
                        else
                            continue;


                    }
                    else
                    {
                        line.ErrLine = opCodeTermSplit2[0] + "    ist kein Befehl";
                        continue;
                    }

                    if (line.ByteLen == 1)
                    {

                        if (foundOpCode.Register >= 0)
                        {
                            string regTemp = "";

                            if (opCodeTermSplit2.Length != 2)
                            {
                                line.ErrLine = "RegisterNr (hex) fehlt";
                                line.ByteLen = -1;
                                continue;
                            }
                            for (int k = 0; k < ListReplace.Count; k++)
                            {
                                if (opCodeTermSplit2[1] == ListReplace[k].NickName)
                                {
                                    regTemp = ListReplace[k].Name;
                                    break;
                                }
                            }
                            if (regTemp == "")
                                regTemp = opCodeTermSplit2[1];


                            if (!CStat.IsHex(regTemp))
                            {
                                line.ErrLine = "RegisterNr (hex) fehlt";
                                line.ByteLen = -1;
                                continue;
                            }
                            if (regTemp.Length != 1)
                            {
                                line.ErrLine = "RegisterNr (hex) fehlt";
                                line.ByteLen = -1;
                                continue;
                            }


                            line.OpCodeRight = opCodeTermSplit2[1];

                            string strReg = regTemp.ToUpper();
                            int outIn = 0;

                            if (foundOpCode.Short == "INP" || foundOpCode.Short == "OUT")
                            {
                                try
                                {
                                    outIn = Convert.ToInt32(strReg, 16);

                                    if (outIn <= 0 || outIn > 8)
                                    {
                                        line.ErrLine = "bei INP/OUT nur 1-7 erlaubt";
                                        line.ByteLen = -1;
                                        continue;
                                    }

                                }
                                catch (Exception)
                                {
                                    line.ErrLine = "bei INP/OUT nur 1-7 erlaubt";
                                    line.ByteLen = -1;
                                    continue;
                                }

                            }


                            if (foundOpCode.Short == "INP")
                                strReg = (outIn + 8).ToString("X1");
                            else if (foundOpCode.Short == "OUT")
                                strReg = (outIn).ToString("X1");


                            line.Byte1 = foundOpCode.Hex[0] + strReg;
                            continue;
                        }
                        else
                        {
                            line.OpCodeRight = "";
                            line.Byte1 = foundOpCode.Hex;
                            continue;
                        }

                    }

                    if (line.ByteLen == 2)
                    {
                        if (opCodeTermSplit2.Length != 2)
                        {
                            line.ErrLine = "Befehl nicht vollständig";
                            line.ByteLen = -1;
                            continue;
                        }


                        string opCodeTerm2 = opCodeTermSplit2[1];
                        string opCodeTerm2Temp = opCodeTermSplit2[1];

                        // ToDo Replace muss hier rein
                        for (int k = 0; k < ListReplace.Count; k++)
                        {
                            if (opCodeTerm2 == ListReplace[k].NickName)
                            {
                                opCodeTerm2Temp = ListReplace[k].Name;
                                break;
                            }
                        }

                        if (opCodeTerm2Temp.ToUpper().StartsWith("X") && opCodeTerm2Temp.Length == 3)
                        {
                            if (!CStat.IsHex(opCodeTerm2Temp[1..]))
                            {
                                line.ErrLine = " 2.Parameter falsch";
                                line.ByteLen = -1;
                                continue;
                            }

                            if (opCodeTerm2.ToUpper().StartsWith("X"))
                                line.OpCodeRight = "x" + opCodeTerm2[1..].ToUpper();
                            else
                                line.OpCodeRight = opCodeTerm2;

                            line.Byte1 = foundOpCode.Hex.ToUpper();
                            line.Byte2 = opCodeTerm2Temp[1..].ToUpper();
                            continue;
                        }

                        // BR *Loop
                        // LDI* Prog2.Start.0 / 1Byte AdrLow
                        // LDI* Prog2.Start.1 / 1Byte AdrHigh
                        // LDI * Prog2.Start - Fehler 2Byte bei LDI nicht möglich

                        DoStar(2, line, foundOpCode, opCodeTerm2);
                        continue;
                    }


                    if (line.ByteLen == 3)
                    {

                        // LBR x1234
                        // LBR* Prog2.Loop / 2Byte AdrHigh und AdrLow

                        if (opCodeTermSplit2.Length != 2)
                        {
                            line.ErrLine = "Befehl nicht vollständig";
                            line.ByteLen = -1;
                            continue;
                        }


                        string opCodeTerm2 = opCodeTermSplit2[1];
                        string opCodeTerm2Temp = opCodeTermSplit2[1];

                        // ToDo Replace muss hier rein
                        for (int k = 0; k < ListReplace.Count; k++)
                        {
                            if (opCodeTerm2 == ListReplace[k].NickName)
                            {
                                opCodeTerm2Temp = ListReplace[k].Name;
                                break;
                            }
                        }

                        if (opCodeTerm2Temp.ToUpper().StartsWith("X") && opCodeTerm2Temp.Length == 5)
                        {
                            if (!CStat.IsHex(opCodeTerm2Temp[1..]))
                            {
                                line.ErrLine = " 2.Parameter falsch";
                                line.ByteLen = -1;
                                continue;
                            }

                            if (opCodeTerm2.ToUpper().StartsWith("X"))
                                line.OpCodeRight = "x" + opCodeTerm2[1..].ToUpper();
                            else
                                line.OpCodeRight = opCodeTerm2;

                            line.Byte1 = foundOpCode.Hex.ToUpper();
                            line.Byte2 = opCodeTerm2[1..].ToUpper();
                            continue;
                        }

                        // LBR* Prog2.Loop / 2Byte AdrHigh und AdrLow
                        DoStar(3, line, foundOpCode, opCodeTerm2);
                        continue;


                    }
                 
                }

            }

            AutoSizeLists();

            return compileOk;
        }

        private bool Compile2()
        {


            #region  Programmaufbau prüfen - Prog Name x1234   und  - Start
            bool firstProg = false;
            bool firstStart = false;

            foreach (CEditLine line in ListEditLines)
            {
                if (line.LineProgram != "")
                {
                    if (firstProg && firstStart)
                    {
                        //alten Check löschen
                        firstProg = false;
                        firstStart = false;
                    }
                    if (firstStart)
                    {
                        line.ErrLine = "Programmaufbaufehler - 1.Programmdefinitionszeile 2.Startmarke in nächster Zeile";
                        return false;
                    }

                    firstProg = true;
                    continue;
                }

                if (line.LeftJump == "Start")
                {
                    firstStart = true;

                }

                if (line.ByteLen > 0 && (!firstProg || !firstStart))
                {
                    line.ErrLine = "Programmaufbaufehler - 1.Programmdefinitionszeile 2.Startmarke in nächster Zeile";
                    return false;
                }
            }
            #endregion




            int adr = 0;
            string progNow = "";

            foreach (CEditLine line in ListEditLines)
            {
                #region  Adressen aus Programmliste übernehmen

                if (line.LineProgram != "")
                {
                    progNow = line.LineProgram[6..line.LineProgram.LastIndexOf(" ")];
                    int idx = ListProg.IndexOf(ListProg.Find(x => x.ProgName == progNow));

                    int adrInLine = Convert.ToInt32(ListProg[idx].Adr, 16);

                    if (adrInLine == 0)
                    {
                        ListProg[idx].Adr = adr.ToString("X4");
                    }
                    else
                    {
                        if (adrInLine >= adr)
                        {
                            adr = adrInLine;
                        }
                        else
                        {
                            line.ErrLine = "Programmadresse korrigieren";
                            return false;
                        }
                    }
                }

                #endregion


                #region Adressen zur Sprungmarke 
                if (line.LeftJump != "")
                {
                    int idx = ListJump.IndexOf(ListJump.Find(x => x.ProgName == progNow && x.JumpName == line.LeftJump));
                    ListJump[idx].Adr = adr.ToString("X4");
                }
                #endregion



                if (line.ByteLen > 0)
                {
                    line.Adr = adr.ToString("X4");
                    adr += line.ByteLen;
                }


            }



            foreach (CJump item in ListJump)
            {
                if (item.Adr == null)
                {
                    MessageBox.Show("Adressermittlung geht nicht");
                    return false;
                }
            }


            foreach (CEditLine line in ListEditLines)
            {
                if (line.LineProgram != "")
                {
                    progNow = line.LineProgram[6..line.LineProgram.LastIndexOf(" ")];
                }


                //Points == 0 -------------------
                // bytes = 1:  
                // BR *Loop
                // BR *+0 bis *+9   oder  *-0 bis *-9  (Sprung um Befehlszeilen)

                //Points == 1 -------------------
                // bytes = 2:  
                // LBR *Prog2.Start 

                //Points == 2 -------------------
                // bytes = 1:  
                // LDI *Prog2.Start.0 / 1Byte AdrLow
                // LDI *Prog2.Start.1 / 1Byte AdrHigh
                // LDI *Prog2.Start - Fehler 2Byte bei LDI nicht möglich  


                if (line.OpCodeRight.StartsWith("*"))
                {
                    string[] opCodeRightSplitP = line.OpCodeRight.Split(".");
                    int points = opCodeRightSplitP.Length - 1;

                    if (points == 0)
                    {
                        // BR *+0 *-0  bis *+9 *-9
                        if (line.OpCodeRight[1..].StartsWith("+"))
                        {
                            if (line.OpCodeRight[1..].Length != 2)
                            {
                                line.ErrLine = "Sprung zur Zeile kann nur von 0 bis 9 programmiert werden zB: 'BR *+3'";
                                return false;
                            }
                            if (!Char.IsNumber(line.OpCodeRight[2]))
                            {
                                line.ErrLine = "Sprung zur Zeile kann nur von 0 bis 9 programmiert werden zB: 'BR *+3'";
                                return false;
                            }

                            int jumpLines = Convert.ToInt32(line.OpCodeRight[2].ToString());
                            bool foundPlus = false;

                            for (int i = line.Num; i < ListEditLines.Count; i++)
                            {
                                if (ListEditLines[i].ByteLen > 0)
                                {
                                    if (jumpLines == 0)
                                    {
                                        line.Byte2 = ListEditLines[i].Adr[2..];
                                        foundPlus = true;
                                        break;
                                    }
                                    else
                                    {
                                        jumpLines -= 1;
                                    }
                                }

                            }

                            if (!foundPlus)
                            {
                                line.ErrLine = "Ein Sprung um " + line.OpCodeRight[2] + "-Zeilen ist nicht möglich";
                                return false;
                            }
                            else
                            {
                                continue;
                            }


                        }
                        if (line.OpCodeRight[1..].StartsWith("-"))
                        {
                            if (line.OpCodeRight[1..].Length != 2)
                            {
                                line.ErrLine = "Sprung zur Zeile kann nur von 0 bis 9 programmiert werden zB: 'BR *-3'";
                                return false;
                            }
                            if (!Char.IsNumber(line.OpCodeRight[2]))
                            {
                                line.ErrLine = "Sprung zur Zeile kann nur von 0 bis 9 programmiert werden zB: 'BR *-3'";
                                return false;
                            }

                            int jumpLines = Convert.ToInt32(line.OpCodeRight[2].ToString());
                            bool foundMinus = false;

                            for (int i = line.Num; i >= 0; i--)
                            {
                                if (ListEditLines[i].ByteLen > 0)
                                {
                                    if (jumpLines == 0)
                                    {
                                        line.Byte2 = ListEditLines[i].Adr[2..];
                                        foundMinus = true;
                                        break;
                                    }
                                    else
                                    {
                                        jumpLines -= 1;
                                    }
                                }

                            }

                            if (!foundMinus)
                            {
                                line.ErrLine = "Ein Sprung um " + line.OpCodeRight[2] + "-Zeilen ist nicht möglich";
                                return false;
                            }
                            else
                            {
                                continue;
                            }


                        }
                        // BR* Loop
                        CJump jumpFound = ListJump.Find(x => x.ProgName == progNow && x.JumpName == line.OpCodeRight[1..]);
                        if (jumpFound != null && jumpFound.Adr != null)
                        {
                            // line.Adr = jumpFound.Adr[2..];
                            line.Byte2 = jumpFound.Adr[2..];
                        }
                        else
                        {
                            line.ErrLine = "Sprungadresse nicht gefunden";
                            return false;
                        }
                    }

                    if (points == 1)
                    {
                        // LBR *Prg1.Loop
                        CJump jumpFound = ListJump.Find(x => x.ProgName == opCodeRightSplitP[0][1..] && x.JumpName == opCodeRightSplitP[1]);
                        if (jumpFound != null && jumpFound.Adr != null)
                        {
                            //line.Adr = jumpFound.Adr;
                            line.Byte2 = jumpFound.Adr;
                        }
                        else
                        {
                            line.ErrLine = "Sprungadresse nicht gefunden";
                            return false;
                        }
                    }
                    if (points == 2)
                    {
                        // LDI *Prog2.Start.0 / 1Byte AdrLow
                        // LDI *Prog2.Start.1 / 1Byte AdrHigh
                        CJump jumpFound = ListJump.Find(x => x.ProgName == opCodeRightSplitP[0][1..] && x.JumpName == opCodeRightSplitP[1]);
                        if (jumpFound != null && jumpFound.Adr != null)
                        {
                            if (opCodeRightSplitP[2] == ("0"))
                            {
                                // line.Adr = jumpFound.Adr[2..];
                                line.Byte2 = jumpFound.Adr[2..];
                            }
                            else
                            {
                                // line.Adr = jumpFound.Adr[..2];
                                line.Byte2 = jumpFound.Adr[..2];
                            }

                        }
                        else
                        {
                            line.ErrLine = "Sprungadresse nicht gefunden";
                            return false;
                        }
                    }
                }


            }



            return true;
        }



        private void IniMem()
        {
            // M[] erzeugen
            for (int i = 0; i < 0x10000; i++)
                M[i] = 0;






            // ListViewMemNew Items erzeugen, falls nicht vorhanden
            if (ListMem.Count == 0)
            {
                for (int i = 0; i < 0x100; i++)
                {
                    ListMem.Add(new CMemView() { Adr = i.ToString("X4"), Data = "00", Break = " ", AdrBrush = Brushes.White });

                }
            }

            // ListViewItem.Content löschen
            //for (int i = 0; i < 0x100; i++)
            //    ListMem[i].Data = "00";



            if (ListViewMemCpu.Items.Count == 0)
            {
                for (int i = 0; i < 16; i++)
                {
                    ListViewItem lvi = new ListViewItem();
                    ListViewMemCpu.Items.Add(lvi);
                }
            }



            ShowMem(0);

            ShowMemCpu();
        }

        //TODO ShowMemView
        private void ShowMemCpu()
        {
            int ad = 0;

            if (ListProg.Count > 0)
                ad = Convert.ToInt32(ListProg[0].Adr, 16);


            for (int i = 0; i < 3; i++)
            {

                CMemView memView = new CMemView() { Adr = (ad + i).ToString("X4"), Data = M[ad + i].ToString("X2") };

                ((ListViewItem)ListViewMemCpu.Items[i]).Content = memView;

            }
            for (int i = 3; i < 5; i++)
            {

                CMemView memView = new CMemView() { Adr = " .. ", Data = ".." };

                ((ListViewItem)ListViewMemCpu.Items[i]).Content = memView;

            }

            int adr = 0xFFFF - 2;

            for (int i = 5; i < 8; i++)
            {

                CMemView memView = new CMemView() { Adr = adr.ToString("X4"), Data = M[adr].ToString("X2") };

                ((ListViewItem)ListViewMemCpu.Items[i]).Content = memView;
                adr++;

            }
        }

        private void ShowMem(int memAdr)
        {


            int adr = memAdr - 8 >= 0 ? memAdr - 8 : 0;


            for (int i = 0; i < 0x100; i++)
            {
                if (adr > 0xFFFF)
                    break;

                ListMem[i].Adr = adr.ToString("X4");
                ListMem[i].AdrBrush = Brushes.White;

                ListMem[i].Data = M[adr].ToString("X2");
                ListMem[i].DataBrush = Brushes.White;

                CBreak found = ListBreakpoints.Find(x => x.Adr == adr);
                if (found != null)
                {
                    if (found.Comp == "B" || found.Comp == "L")
                        ListMem[i].Break = found.Typ;
                    else
                        ListMem[i].Break = found.Typ + " " + found.Term1 + found.Comp + found.Term2;

                    ListMem[i].AdrBrush = Brushes.Orange;
                }
                else
                {
                    ListMem[i].Break = " ";
                    ListMem[i].AdrBrush = Brushes.White;
                }


                //ListViewItem lvi2 = ListViewMem.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                //lvi2.Background = ListMem[i].ViewBrush;
                adr++;
            }


            ListViewMem.Items.Refresh();
        }

        private void ColorLine_Editor(CEditLine lineFound)
        {

            if (EditLineSave.line != -1)
                ListEditLines[EditLineSave.line].Para.Background = EditLineSave.Color;

            EditLineSave.line = lineFound.Num;
            EditLineSave.Color = ListEditLines[lineFound.Num].Para.Background;
            ListEditLines[lineFound.Num].Para.Background = Brushes.LightBlue;
            ListEditLines[lineFound.Num].Para.BringIntoView();
        }

        private void ColorLine_ListViewMem(int memAdr)
        {



            int adr = memAdr - 8 >= 0 ? memAdr - 8 : 0;


            for (int i = 0; i < 0x100; i++)
            {
                if (adr > 0xFFFF)
                    break;

                ListMem[i].Adr = adr.ToString("X4");
                ListMem[i].AdrBrush = Brushes.White;

                ListMem[i].Data = M[adr].ToString("X2");
                ListMem[i].DataBrush = Brushes.White;

                CBreak found = ListBreakpoints.Find(x => x.Adr == adr);
                if (found != null)
                {
                    if (found.Comp == "B" || found.Comp == "L")
                        ListMem[i].Break = found.Typ;
                    else
                        ListMem[i].Break = found.Typ + " " + found.Term1 + found.Comp + found.Term2;

                    ListMem[i].AdrBrush = Brushes.Orange;
                }
                else
                {
                    ListMem[i].Break = " ";
                    ListMem[i].AdrBrush = Brushes.White;
                }

                if (Convert.ToInt32(ListMem[i].Adr, 16) == memAdr)
                {
                    ListMem[i].DataBrush = Brushes.LightBlue;

                    if (ListMem[i].AdrBrush == Brushes.White)
                        ListMem[i].AdrBrush = Brushes.LightBlue;
                }

                //ListViewItem lvi2 = ListViewMem.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                //lvi2.Background = ListMem[i].ViewBrush;
                adr++;
            }

            ListViewMem.Items.Refresh();
            return;



        }

        private int ColorLine_ListViewOpCode(int editLineNum)
        {
            string codeLeft = ListEditLines[editLineNum].OpCodeLeft;

            for (int i = 0; i < ListViewOpCode.Items.Count; i++)
            {
                if (((COpCode)ListViewOpCode.Items[i]).Short == codeLeft)
                {
                    ListViewOpCode.Focus();

                    ListViewItem lvi = ListViewOpCode.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                    if (lvi != null)
                    {
                        if (ListViewOpCodeLviSave != null)
                            ListViewOpCodeLviSave.Background = ListViewOpCodeBrushSave;

                        ListViewOpCodeBrushSave = lvi.Background;
                        ListViewOpCodeLviSave = lvi;

                        lvi.Background = Brushes.LightBlue;
                    }

                    ListViewOpCode.ScrollIntoView(ListViewOpCode.Items[i]);
                    ListViewOpCode.SelectedIndex = i;

                    return ListViewOpCode.SelectedIndex;
                }
            }
            return -1;
        }

        private void ColorLine_ListViewProg(int editLineNum)
        {
            ListViewItem lvi = null;


            for (int i = 0; i < ListProg.Count; i++)
            {
                lvi = ListViewProg.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;

                if (lvi != null)
                {
                    lvi = ListViewProg.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                    lvi.Background = Brushes.White;
                }

            }

            for (int i = ListProg.Count - 1; i >= 0; i--)
            {
                lvi = ListViewProg.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;

                if (lvi != null && ListProg[i].LineNum <= editLineNum)
                {
                    lvi.Background = Brushes.LightBlue;
                    break;
                }
            }
        }

        private void ColorLine_ListViewJump(int editLineNum)
        {
            ListViewItem lvi = null;


            for (int i = 0; i < ListJump.Count; i++)
            {
                lvi = ListViewJump.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;

                if (lvi != null)
                {
                    lvi = ListViewJump.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                    lvi.Background = Brushes.White;
                }

            }

            for (int i = ListJump.Count - 1; i >= 0; i--)
            {
                lvi = ListViewJump.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;

                if (lvi != null && ListJump[i].LineNum == editLineNum)
                {
                    lvi.Background = Brushes.LightBlue;
                    break;
                }
            }
        }

        private void ColorLine_ListViewBreakpoints(int editLineNum)
        {
            ListViewItem lvi = null;


            for (int i = 0; i < ListBreakpoints.Count; i++)
            {
                lvi = ListViewBreakpoints.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;

                if (lvi != null)
                {
                    lvi = ListViewBreakpoints.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                    lvi.Background = Brushes.White;
                }

            }

            for (int i = ListBreakpoints.Count - 1; i >= 0; i--)
            {
                lvi = ListViewBreakpoints.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;

                if (lvi != null && ListBreakpoints[i].LineNum == editLineNum)
                {
                    lvi.Background = Brushes.LightBlue;
                    break;
                }
            }
        }
        private (int maxLeft, int maxOpLeft, int maxOpRight) GetDimLine()
        {
            int maxLeft = 8;
            int maxOpLeft = 4;
            int maxOpRight = 4;

            foreach (CEditLine line in ListEditLines)
            {
                maxLeft = maxLeft > line.LeftJump.Length ? maxLeft : line.LeftJump.Length;
                maxOpLeft = maxOpLeft > line.OpCodeLeft.Length ? maxOpLeft : line.OpCodeLeft.Length;
                maxOpRight = maxOpRight > line.OpCodeRight.Length ? maxOpRight : line.OpCodeRight.Length;
            }

            return (maxLeft, maxOpLeft, maxOpRight);
        }




        private void SetBreakpoint(string chrTyp, CMemView memSelect)
        {
            if (memSelect == null)
                return;

            CBreak breakInList = ListBreakpoints.Find(x => x.Adr == Convert.ToInt32(memSelect.Adr, 16));

            CEditLine lineFound = ListEditLines.Find(x => x.Adr == memSelect.Adr);

            if (lineFound == null)
                return;

            if (breakInList != null && breakInList.Condition != "")
            {
                #region Breakpoint löschen
                MessageBoxResult result = MessageBox.Show(breakInList.Term1 + breakInList.Comp + breakInList.Term2, "Breakpoint löschen ? '" + memSelect.Adr + "'", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                    return;

                ListBreakpoints.Remove(breakInList);

                ListViewBreakpoints.Items.Refresh();

                CMemView mv = (CMemView)ListViewMem.Items[0];
                ShowMem(Convert.ToInt32(mv.Adr, 16) + 8);
                #endregion
            }
            else
            {
                string strTyp = "Break";

                if (chrTyp == "L")
                    strTyp = "Log";





                #region neuen Break-/LogPoint hinzufügen
                //string text = "Eingabe der " + strTyp + "point-Bedingung:\n\n" +
                //              chrTyp + "\t" + strTyp + "point ohne Bedingung\n" +
                //              ">\tgrösser\n" +
                //              ">=\tgrösser/gleich\n" +
                //              "<\tkleiner\n" +
                //              "<=\tkleiner/gleich\n" +
                //              "==\tgleich\n\n" +
                //              "Beispiele:\n\n" +
                //              "R5>A12F\t\tRegister und Hexwert\n" +
                //              "R5>=A12F\n" +
                //              "R5<A12F\n" +
                //              "R5<=A12F\n" +
                //              "R5==A12F\n\n" +
                //              "R5>R6\t\tRegister und Register\n" +
                //              "R5>=R6\n" +
                //              "R5<R6\n" +
                //              "R5<=R6\n" +
                //              "R5==R6\n\n" +
                //              "möglich: R1..RF  RP  RX  und '4stellig-Hex' zB: A0F7"
                //              ;

                MessageBoxResult result = MessageBox.Show("", strTyp + "point  setzen bei Adresse '" + memSelect.Adr + "'", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                {
                    TextBoxBreak.Text = "";
                    return;
                }
                // Übernahme der Eingabewerte -  siehe TextBoxBreak_KeyUp
                TextBoxBreak.Visibility = Visibility.Visible;
                TextBoxBreak2.Visibility = Visibility.Visible;
                ButtonBreakpointYes.Visibility = Visibility.Visible;
                ButtonBreakpointNo.Visibility = Visibility.Visible;
                TextBoxBreak.Focus();
                TextBoxBreak.Text = chrTyp;
                BreakLogTyp = chrTyp;
                TextBoxBreak.CaretIndex = 1;




                #region HelpBox
                string text0 = "BREAKPOINT:    Programm wird bei Erreichen der Adresse, je nach Bedingung angehalten\n" +
                               "LOGPOINT:        Bei Erreichen der Adresse, wird das Logfile mit den ausgewählten \n  " +
                               "                           CPU-Daten beschrieben\n\n";

                string text = "1. Break-/Logpointbedingung eintragen (Standard = B/L - keine Bedingung)\n" +
                              "2. Logfileeinträge filtern (Standard = alle)\n" +
                              "3. Break- oder Logbedingung abschliessen mit  'OK-Button'\n";

                OpenHelpBox(false, "Eingabemöglichkeiten", text0+text, "HelpBoxBreak01.png");

                #endregion





                #endregion

            }
        }




    }
}
