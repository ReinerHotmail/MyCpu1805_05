using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyCpu1805_05
{

    public partial class MainWindow
    {
        enum Cycle { Run, DebugLoop, DebugStep, DebugFetchExecute }

        enum CycleColor { Green, Gold, Red, Orange } // Run,Stop,Reset

        Cycle CpuMode = Cycle.Run;
        CycleColor CpuLed = CycleColor.Red;




        byte D = 0x00;
        byte T = 0x00;
        byte B = 0x00;
        bool DF = false;
        bool Q = false;
        bool EF1 = false;
        bool EF2 = false;
        bool EF3 = false;
        bool EF4 = false;
        int[] INP = new int[8];

        int[] OUT = new int[8];
        byte I = 0x0;
        byte N = 0x0;
        byte P = 0x0;
        byte X = 0x0;
        bool IE = false;
        bool INTR = false;

        ushort[] R = new ushort[16];
        List<CRegister> ListRegister = new List<CRegister>();
        readonly int LoopSteps = 5_000_000;

        byte[] M = new byte[0x10000];



        List<CMemView> ListMem = new List<CMemView>();
        List<CBreak> ListBreakpoints = new List<CBreak>();


        Stopwatch StopWatchRunTime = new Stopwatch();

        enum OpCode
        {
            IDL, LDN1, LDN2, LDN3, LDN4, LDN5, LDN6, LDN7, LDN8, LDN9, LDNA, LDNB, LDNC, LDND, LDNE, LDNF,
            INC0, INC1, INC2, INC3, INC4, INC5, INC6, INC7, INC8, INC9, INCA, INCB, INCC, INCD, INCE, INCF,
            DEC0, DEC1, DEC2, DEC3, DEC4, DEC5, DEC6, DEC7, DEC8, DEC9, DECA, DECB, DECC, DECD, DECE, DECF,
            BR, BQ, BZ, BDF, B1, B2, B3, B4, NBR, BNQ, BNZ, BNF, BN1, BN2, BN3, BN4,
            LDA0, LDA1, LDA2, LDA3, LDA4, LDA5, LDA6, LDA7, LDA8, LDA9, LDAA, LDAB, LDAC, LDAD, LDAE, LDAF,
            STR0, STR1, STR2, STR3, STR4, STR5, STR6, STR7, STR8, STR9, STRA, STRB, STRC, STRD, STRE, STRF,
            IRX, OUT1, OUT2, OUT3, OUT4, OUT5, OUT6, OUT7, Code68xx, INP1, INP2, INP3, INP4, INP5, INP6, INP7,
            RET, DIS, LDXA, STXD, ADC, SDB, SHRC, SMB, SAV, MARK, REQ, SEQ, ADCI, SDBI, SHLC, SMBI,
            GLO0, GLO1, GLO2, GLO3, GLO4, GLO5, GLO6, GLO7, GLO8, GLO9, GLOA, GLOB, GLOC, GLOD, GLOE, GLOF,
            GHI0, GHI1, GHI2, GHI3, GHI4, GHI5, GHI6, GHI7, GHI8, GHI9, GHIA, GHIB, GHIC, GHID, GHIE, GHIF,
            PLO0, PLO1, PLO2, PLO3, PLO4, PLO5, PLO6, PLO7, PLO8, PLO9, PLOA, PLOB, PLOC, PLOD, PLOE, PLOF,
            PHI0, PHI1, PHI2, PHI3, PHI4, PHI5, PHI6, PHI7, PHI8, PHI9, PHIA, PHIB, PHIC, PHID, PHIE, PHIF,
            LBR, LBQ, LBZ, LBDF, NOP, LSNQ, LSNZ, LSNF, NLBR, LBNQ, LBNZ, LBNF, LSIE, LSQ, LSZ, LSDF,
            SEP0, SEP1, SEP2, SEP3, SEP4, SEP5, SEP6, SEP7, SEP8, SEP9, SEPA, SEPB, SEPC, SEPD, SEPE, SEPF,
            SEX0, SEX1, SEX2, SEX3, SEX4, SEX5, SEX6, SEX7, SEX8, SEX9, SEXA, SEXB, SEXC, SEXD, SEXE, SEXF,
            LDX, OR, AND, XOR, ADD, SD, SHR, SM, LDI, ORI, ANI, XRI, ADI, SDI, SHL, SMI, BPZ, BGE, SKP, BM, BL, RSHR, RSHL, LSKP
        };


        private void CpuReset()
        {
            CpuLed = CycleColor.Red;
            ExeStop = true;

            for (int i = 0; i < 16; i++)
                R[i] = 0;

            D = 0x00;
            T = 0x00;
            B = 0x00;
            DF = false;
            Q = false;
            IE = true;

            I = 0x0;
            N = 0x0;
            P = 0x0;
            X = 0x0;

            LabelRunTime.Content = "";
            LabelRunTimeStart.Content = "";
            LabelRunTimeStop.Content = "";

            LabelBeakpoints.Background = Brushes.Transparent;
            LabelBeakpoints.Content = "";

            ShowDebugStatus(false);

            ShowCpuExeData();
        }


        /// <summary>
        /// Start DoExeLoop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerExe_Tick(object sender, EventArgs e)
        {
           

            DoExeLoop(ExeStop);

        }

        /// <summary>
        /// Loop Run RCA-Opcodes - LoopEnd starts TimerTick for new Start
        /// </summary>
        /// <param name="exeStop"></param>
        private void DoExeLoop(bool exeStop)
        {
            TimerExe.Stop();
            CpuLed = CycleColor.Green;
            OpCode opCode = OpCode.IDL;

            switch (CpuMode)
            {
                case Cycle.Run:

                    for (int i = 0; i < LoopSteps; i++)
                    {

                        opCode = (OpCode)M[R[P]];
                        R[P]++;
                        ExecuteOpCode(opCode);
                        if (INTR)
                            InterruptStart();
                    }

                    break;
                case Cycle.DebugLoop:
                    for (int i = 0; i < LoopSteps; i++)
                    {
                        opCode = (OpCode)M[R[P]];
                        R[P]++;

                        // I und N für Anzeige
                        int opc = (int)opCode;
                        I = (byte)((opc & 0xF0) >> 4);
                        N = (byte)(opc & 0x0F);

                        ExecuteOpCode(opCode);

                        if (INTR)
                            InterruptStart();

                        if (BreakpointsOn)
                        {
                            CBreak found = ListBreakpoints.Find(x => x.Adr == R[P]);

                            if (found != null)
                            {
                                if (OnBreakCondition(found))
                                {
                                    if (found.Typ == "B")
                                    {
                                        i = LoopSteps - 1;
                                        BreakpointReached = true;
                                    }
                                    else if (found.Typ == "L")
                                    {
                                        LogFileAddLine(LogLine.Run, "Adr " + found.AdrHex, found.LogItems, false);
                                    }
                                    i += 1000; //LogFile kostet viel Zeit, deshalb LOOP verkürzen
                                }
                            }
                        }
                    }
                    break;
                case Cycle.DebugStep:

                    opCode = (OpCode)M[R[P]];
                    R[P]++;


                    // I und N für Anzeige
                    int o = (int)opCode;
                    I = (byte)((o & 0xF0) >> 4);
                    N = (byte)(o & 0x0F);

                    ExecuteOpCode(opCode);
                    if (INTR && IE)
                        InterruptStart();

                    break;
                case Cycle.DebugFetchExecute:
                    break;
                default:
                    break;
            }


            if (!exeStop && !(M[R[P]] == 0x00))
            {
                if (CpuMode != Cycle.Run)
                {
                    #region DEBUG / Breakpoints
                    ShowDebugStatus(false);

                    if (BreakpointReached)
                    {
                        BreakpointReached = false;
                        CpuLed = CycleColor.Orange;
                        LabelBeakpoints.Content = "Breakpoint !!";
                        LabelBeakpoints.Background = Brushes.Orange;


                    }
                    else
                    {
                        if (CpuMode == Cycle.Run || CpuMode == Cycle.DebugLoop)
                            TimerExe.Start();
                    }
                    #endregion
                }
                else
                {
                    TimerExe.Start();
                }
            }
            else
            {
                #region bei ext.STOP oder M[R[P]] == 0x00


                if (CpuLed != CycleColor.Red)  // bei RESET kein Gold
                    CpuLed = CycleColor.Gold;




                StopWatchRunTime.Stop();
                TimeSpan ts = StopWatchRunTime.Elapsed;

                string elapsedTime = String.Format("{0:00}-{1:00}-{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                LabelRunTime.Content = elapsedTime;
                LabelRunTimeStop.Content = DateTime.Now.ToString("HH-mm-ss.fff tt");

                LogFileAddLine(LogLine.End, "Runtime " + elapsedTime, "PXD", false);

                StopWatchRunTime.Reset();

                ShowDebugStatus(false);

                #endregion
            }

            ShowCpuExeData();
        }

        private bool OnBreakCondition(CBreak found)
        {
            if (found.Comp == "B" || found.Comp == "L")
                return true;

            int iLeft = 0;
            int iRight = 0;

            if (found.Term1.Length == 2)
            {
                if (found.Term1[1] == 'P')
                    iLeft = R[P];
                else if (found.Term1[1] == 'X')
                    iLeft = R[X];
                else
                    iLeft = R[Convert.ToInt32(found.Term1[1].ToString(), 16)];
            }
            else
            {
                iLeft = Convert.ToInt32(found.Term1, 16);
            }

            if (found.Term2.Length == 2)
            {
                if (found.Term2[1] == 'P')
                    iRight = R[P];
                else if (found.Term2[1] == 'X')
                    iRight = R[X];
                else
                    iRight = R[Convert.ToInt32(found.Term2[1].ToString(), 16)];
            }
            else
            {
                iRight = Convert.ToInt32(found.Term2, 16);
            }


            if (found.Comp == ">")
            {
                if (iLeft > iRight)
                    return true;
            }
            else if (found.Comp == ">=")
            {
                if (iLeft >= iRight)
                    return true;
            }
            else if (found.Comp == "==")
            {
                if (iLeft == iRight)
                    return true;
            }
            else if (found.Comp == "<")
            {
                if (iLeft < iRight)
                    return true;
            }
            else if (found.Comp == "<=")
            {
                if (iLeft <= iRight)
                    return true;
            }
            return false;
        }

        private void ShowCpuExeData()
        {
            switch (CpuLed)
            {
                case CycleColor.Green:
                    LabelLedRun.Background = Brushes.Green;
                    break;
                case CycleColor.Gold:
                    LabelLedRun.Background = Brushes.Gold;
                    break;
                case CycleColor.Red:
                    LabelLedRun.Background = Brushes.Red;
                    break;
                case CycleColor.Orange:
                    LabelLedRun.Background = Brushes.Orange;
                    break;
                default:
                    break;
            }


            GetOutputs();

            if (Q)
                LabelQ.Background = Brushes.Red;
            else
                LabelQ.Background = Brushes.White;

            ListEf[1].Val = EF1;
            ListEf[2].Val = EF2;
            ListEf[3].Val = EF3;
            ListEf[4].Val = EF4;


            ShowIntrBits();

            if (Monitor.On)
                MyMonitor.SetText();
        }

        private void InterruptStart()
        {
            //X,P -+ T; 1 -+ P; 2 -+ X; 0 -+ IE 
            T = (byte)(X << 4 | P);
            P = 1;
            X = 2;

            IE = false;
            INTR = false;

            if (CpuMode != Cycle.Run)
            {

                ShowDebugStatus(true);

            }


        }


        private void ShowDebugStatus(bool intrActiv)
        {
            if (InputActiv && !intrActiv)  // sonst wird der Focus von den Eingabefeldern weggenommen
                return;



            ShowCpu();

            ShowRegister();

            ShowMem(R[P]);


            ColorLine_ListViewMem(R[P]);

            CEditLine lineFound = ListEditLines.Find(x => x.Adr == R[P].ToString("X4"));

            if (lineFound == null)
                return;


            ShowTextBoxLineInfo(lineFound.Num);

            ColorLine_ListViewProg(lineFound.Num);

            ColorLine_ListViewJump(lineFound.Num);

            ColorLine_ListViewBreakpoints(lineFound.Num);

            ListViewOpCodeFilterExtern = "";

            int listViewOpCodeIdx = ColorLine_ListViewOpCode(lineFound.Num);

            ShowTextBoxOpCodeInfo(listViewOpCodeIdx);

            ColorLine_ListViewMem(Convert.ToInt32(lineFound.Adr, 16));

            ColorLine_Editor(lineFound);



        }

        private void ShowCpu()
        {
            LabelD.Content = D.ToString("X2");

            string dBin = Convert.ToString(D, 2).PadLeft(8, '0');

            LabelD.ToolTip = "Hex: " + D.ToString("X2") + Environment.NewLine +
                              "Bin: " + dBin[..4] + " " + dBin[4..] + Environment.NewLine +
                              "Dec: " + D.ToString();

            if (DF)
                LabelDF.Content = "1";
            else
                LabelDF.Content = "0";

            LabelT.Content = T.ToString("X2");

            string tBin = Convert.ToString(T, 2).PadLeft(8, '0');

            LabelT.ToolTip = "Hex: " + T.ToString("X2") + Environment.NewLine +
                              "Bin: " + tBin[..4] + " " + tBin[4..] + Environment.NewLine +
                              "Dec: " + T.ToString();

            LabelB.Content = B.ToString("X2");

            string bBin = Convert.ToString(B, 2).PadLeft(8, '0');

            LabelB.ToolTip = "Hex: " + B.ToString("X2") + Environment.NewLine +
                              "Bin: " + bBin[..4] + " " + bBin[4..] + Environment.NewLine +
                              "Dec: " + B.ToString();

            LabelI.Content = I.ToString("X1").ToString();

            LabelN.Content = N.ToString("X1").ToString();

            LabelP.Content = P.ToString("X1").ToString();
            LabelX.Content = X.ToString("X1").ToString();

        }

        /// <summary>
        /// switch to execute OpCode(para)
        /// </summary>
        /// <param name="o"></param>
        private void ExecuteOpCode(OpCode o)
        {
            int d = 0;
            int übertrag = 0;
            int erg = 0;
            int einerkomplement;
            int zweierkomplement;


            //if (o==OpCode.Code68xx)
            //{
            //    Execute68(M[R[P]]);
            //    return;
            //}



            switch (o)
            {
                case OpCode.IDL:
                    #region 00: WAIT FOR DMA OR INTERRUPT;  M(R(0)) -> BUS
                    R[P]--;
                    //ToDo  Monitor DMA-Aus
                    Monitor.DmaOutActive = false;
                    #endregion
                    break;
                case OpCode.LDN1:
                    #region 01: M(R(1)) -> D
                    D = M[R[1]];
                    #endregion
                    break;
                case OpCode.LDN2:
                    #region 02: M(R(2)) -> D
                    D = M[R[2]];
                    #endregion
                    break;
                case OpCode.LDN3:
                    #region 03: M(R(3)) -> D
                    D = M[R[3]];
                    #endregion
                    break;
                case OpCode.LDN4:
                    #region 04: M(R(4)) -> D
                    D = M[R[4]];
                    #endregion
                    break;
                case OpCode.LDN5:
                    #region 05: M(R(5)) -> D
                    D = M[R[5]];
                    #endregion
                    break;
                case OpCode.LDN6:
                    #region 06: M(R(6)) -> D
                    D = M[R[6]];
                    #endregion
                    break;
                case OpCode.LDN7:
                    #region 07: M(R(7)) -> D
                    D = M[R[7]];
                    #endregion
                    break;
                case OpCode.LDN8:
                    #region 08: M(R(8)) -> D
                    D = M[R[8]];
                    #endregion
                    break;
                case OpCode.LDN9:
                    #region 09: M(R(9)) -> D
                    D = M[R[9]];
                    #endregion
                    break;
                case OpCode.LDNA:
                    #region 0A: M(R(10)) -> D
                    D = M[R[10]];
                    #endregion
                    break;
                case OpCode.LDNB:
                    #region 0B: M(R(11)) -> D
                    D = M[R[11]];
                    #endregion
                    break;
                case OpCode.LDNC:
                    #region 0C: M(R(12)) -> D
                    D = M[R[12]];
                    #endregion
                    break;
                case OpCode.LDND:
                    #region 0D: M(R(13)) -> D
                    D = M[R[13]];
                    #endregion
                    break;
                case OpCode.LDNE:
                    #region 0E: M(R(14)) -> D
                    D = M[R[14]];
                    #endregion
                    break;
                case OpCode.LDNF:
                    #region 0F: M(R(15)) -> D
                    D = M[R[15]];
                    #endregion
                    break;
                case OpCode.INC0:
                    #region  10: R(0) + 1 -> R(0)
                    R[0]++;
                    //if (R[0] > 0xffff)
                    //    R[0] = 0x00;
                    #endregion
                    break;
                case OpCode.INC1:
                    #region  11: R(1) + 1 -> R(1)
                    R[1]++;

                    // R[1] = R[1] & 0xFFFF;

                    //if (R[1] > 0xFFFF)
                    //{
                    //    R[1] = 0;
                    //}
                    #endregion
                    break;
                case OpCode.INC2:
                    #region  12: R(2) + 1 -> R(2)
                    R[2]++;
                    //if (R[2] > 0xffff)
                    //    R[2] = 0x00;
                    #endregion
                    break;
                case OpCode.INC3:
                    #region  13: R(3) + 1 -> R(3)
                    R[3]++;
                    //if (R[3] > 0xffff)
                    //    R[3] = 0x00;
                    #endregion
                    break;
                case OpCode.INC4:
                    #region 14:  R(4) + 1 -> R(4)
                    R[4]++;
                    //if (R[4] > 0xffff)
                    //    R[4] = 0x00;
                    #endregion
                    break;
                case OpCode.INC5:
                    #region 15:  R(5) + 1 -> R(5)
                    R[5]++;
                    //if (R[5] > 0xffff)
                    //    R[5] = 0x00;
                    #endregion
                    break;
                case OpCode.INC6:
                    #region  16: R(6) + 1 -> R(6)
                    R[6]++;
                    //if (R[6] > 0xffff)
                    //    R[6] = 0x00;
                    #endregion
                    break;
                case OpCode.INC7:
                    #region 17:  R(7) + 1 -> R(7)
                    R[7]++;
                    //if (R[7] > 0xffff)
                    //    R[7] = 0x00;
                    #endregion
                    break;
                case OpCode.INC8:
                    #region  18: R(8) + 1 -> R(8)
                    R[8]++;
                    //if (R[8] > 0xffff)
                    //    R[8] = 0x00;
                    #endregion
                    break;
                case OpCode.INC9:
                    #region  19: R(9) + 1 -> R(9)
                    R[9]++;
                    //if (R[9] > 0xffff)
                    //    R[9] = 0x00;
                    #endregion
                    break;
                case OpCode.INCA:
                    #region  1A: R(10) + 1 -> R(10)
                    R[10]++;
                    //if (R[10] > 0xffff)
                    //    R[10] = 0x00;
                    #endregion
                    break;
                case OpCode.INCB:
                    #region  1B: R(11) + 1 -> R(11)
                    R[11]++;
                    //if (R[11] > 0xffff)
                    //    R[11] = 0x00;
                    #endregion
                    break;
                case OpCode.INCC:
                    #region  1C: R(12) + 1 -> R(12)
                    R[12]++;
                    //if (R[12] > 0xffff)
                    //    R[12] = 0x00;
                    #endregion
                    break;
                case OpCode.INCD:
                    #region  1D: R(13) + 1 -> R(13)
                    R[13]++;
                    //if (R[13] > 0xffff)
                    //    R[13] = 0x00;
                    #endregion
                    break;
                case OpCode.INCE:
                    #region  1E: R(14) + 1 -> R(14)
                    R[14]++;
                    //if (R[14] > 0xffff)
                    //    R[14] = 0x00;
                    #endregion
                    break;
                case OpCode.INCF:
                    #region  1F: R(15) + 1 -> R(15)
                    R[15]++;
                    #endregion
                    //if (R[15] > 0xffff)
                    //    R[15] = 0x00;
                    break;
                case OpCode.DEC0:
                    #region  20: R(0) - 1 -> R(0)
                    R[0]--;
                    //if (R[0] < 0x0)
                    //    R[0] = 0xFFFF;
                    #endregion
                    break;
                case OpCode.DEC1:
                    #region  21: R(1) - 1 -> R(1)
                    R[1]--;
                    //if (R[1] < 0x0)
                    //    R[1] = 0xFFFF;
                    #endregion
                    break;
                case OpCode.DEC2:
                    #region  22: R(2) - 1 -> R(2)
                    R[2]--;
                    //if (R[2] < 0x0)
                    //    R[2] = 0xFFFF;
                    #endregion
                    break;
                case OpCode.DEC3:
                    #region  23: R(3) - 1 -> R(3)
                    R[3]--;
                    //if (R[3] < 0x0)
                    //    R[3] = 0xFFFF;
                    #endregion
                    break;
                case OpCode.DEC4:
                    #region  24: R(4) - 1 -> R(4)
                    R[4]--;
                    //if (R[4] < 0x0)
                    //    R[4] = 0xFFFF;
                    #endregion
                    break;
                case OpCode.DEC5:
                    #region  25_: R(5) - 1 -> R(5)
                    R[5]--;
                    //if (R[5] < 0x0)
                    //    R[5] = 0xFFFF;
                    #endregion
                    break;
                case OpCode.DEC6:
                    #region  26: R(6) - 1 -> R(6)
                    R[6]--;
                    //if (R[6] < 0x0)
                    //    R[6] = 0xFFFF;
                    #endregion
                    break;
                case OpCode.DEC7:
                    #region  27: R(7) - 1 -> R(7)
                    R[7]--;
                    //if (R[7] < 0x0)
                    //    R[7] = 0xFFFF;
                    #endregion
                    break;
                case OpCode.DEC8:
                    #region  28: R(8) - 1 -> R(8)
                    R[8]--;
                    //if (R[8] < 0x0)
                    //    R[8] = 0xFFFF;
                    #endregion
                    break;
                case OpCode.DEC9:
                    #region  29: R(9) - 1 -> R(9)
                    R[9]--;
                    //if (R[9] < 0x0)
                    //    R[9] = 0xFFFF;
                    #endregion
                    break;
                case OpCode.DECA:
                    #region  2A: R(10) - 1 -> R(10)
                    R[10]--;
                    //if (R[10] < 0x0)
                    //    R[10] = 0xFFFF;
                    #endregion
                    break;
                case OpCode.DECB:
                    #region  2B: R(11) - 1 -> R(11)
                    R[11]--;
                    //if (R[11] < 0x0)
                    //    R[11] = 0xFFFF;
                    #endregion
                    break;
                case OpCode.DECC:
                    #region  2C: R(12) - 1 -> R(12)
                    R[12]--;
                    //if (R[12] < 0x0)
                    //    R[12] = 0xFFFF;
                    #endregion
                    break;
                case OpCode.DECD:
                    #region  2D: R(13) - 1 -> R(13)
                    R[13]--;
                    //if (R[13] < 0x0)
                    //    R[13] = 0xFFFF;
                    #endregion
                    break;
                case OpCode.DECE:
                    #region  2E: R(14) - 1 -> R(14)
                    R[14]--;
                    //if (R[14] < 0x0)
                    //    R[14] = 0xFFFF;
                    #endregion
                    break;
                case OpCode.DECF:
                    #region  2F: R(15) - 1 -> R(15)
                    R[15]--;
                    //if (R[15] < 0x0)
                    //    R[15] = 0xFFFF;
                    #endregion
                    break;
                case OpCode.BR:
                    #region  30: M(R(P)) -> R(P).0
                    int low = M[R[P]];
                    int high = R[P] & 0xFF00;
                    R[P] = (ushort)(high | low);
                    #endregion
                    break;
                case OpCode.BQ:
                    #region  31: IF Q = 1, M(R(P)) -> R(P).0, ELSE R(P) + 1 -> R(P)
                    if (Q)
                    {
                        low = M[R[P]];
                        high = R[P] & 0xff00;

                        R[P] = (ushort)(high | low);
                    }
                    else
                    {
                        R[P]++;
                    }
                    #endregion
                    break;
                case OpCode.BZ:
                    #region  32: IF D = 0, M(R(P)) -> R(P).0, ELSE R(P) + 1 -> R(P)
                    if (D == 0)
                    {
                        low = M[R[P]];
                        high = R[P] & 0xff00;

                        R[P] = (ushort)(high | low);
                    }
                    else
                    {
                        R[P]++;
                    }
                    #endregion
                    break;
                case OpCode.BDF:
                    #region  33: IF DF = 1, M(R(P)) -> R(P).0, ELSE R(P) + 1 -> R(P)
                    if (DF)
                    {
                        low = M[R[P]];
                        high = R[P] & 0xff00;

                        R[P] = (ushort)(high | low);
                    }
                    else
                    {
                        R[P]++;
                    }
                    #endregion
                    break;
                case OpCode.B1:
                    #region  34: IF EF1 = 1, M(R(P)) -> R(P).0, ELSE R(P) + 1 -> R(P)

                    if (EF1)
                    {
                        low = M[R[P]];
                        high = R[P] & 0xff00;

                        R[P] = (ushort)(high | low);
                    }
                    else
                    {
                        R[P]++;
                    }
                    #endregion
                    break;
                case OpCode.B2:
                    #region  35: IF EF2 = 1, M(R(P)) -> R(P).0, ELSE R(P) + 1 -> R(P)

                    if (EF2)
                    {
                        low = M[R[P]];
                        high = R[P] & 0xff00;

                        R[P] = (ushort)(high | low);
                    }
                    else
                    {
                        R[P]++;
                    }
                    #endregion
                    break;
                case OpCode.B3:
                    #region  36: IF EF3 = 1, M(R(P)) -> R(P).0, ELSE R(P) + 1 -> R(P)

                    if (EF3)
                    {
                        low = M[R[P]];
                        high = R[P] & 0xff00;

                        R[P] = (ushort)(high | low);
                    }
                    else
                    {
                        R[P]++;
                    }
                    #endregion
                    break;
                case OpCode.B4:
                    #region  37: IF EF4 = 1, M(R(P)) -> R(P).0, ELSE R(P) + 1 -> R(P)

                    if (EF4)
                    {
                        low = M[R[P]];
                        high = R[P] & 0xff00;

                        R[P] = (ushort)(high | low);
                    }
                    else
                    {
                        R[P]++;
                    }
                    #endregion
                    break;
                case OpCode.NBR:
                    #region  38: R(P) + 1 -> R(P) (Note 2)
                    R[P]++;
                    #endregion
                    break;
                case OpCode.BNQ:
                    #region  39: IF Q = 0, M(R(P)) -> R(P).0, ELSE R(P) + 1 -> R(P)
                    if (!Q)
                    {
                        low = M[R[P]];
                        high = R[P] & 0xff00;

                        R[P] = (ushort)(high | low);
                    }
                    else
                    {
                        R[P]++;
                    }
                    #endregion
                    break;
                case OpCode.BNZ:
                    #region  3A: IF D not 0 M(R(P)) -> R(P).0, ELSE R(P) + 1 -> R(P)
                    if (D != 0)
                    {
                        low = M[R[P]];
                        high = R[P] & 0xff00;

                        R[P] = (ushort)(high | low);
                    }
                    else
                    {
                        R[P]++;
                    }
                    #endregion
                    break;
                case OpCode.BNF:
                    #region  3B: IF DF = 0, M(R(P)) -> R(P).0, ELSE R(P) + 1 -> R(P)
                    if (!DF)
                    {
                        low = M[R[P]];
                        high = R[P] & 0xff00;

                        R[P] = (ushort)(high | low);
                    }
                    else
                    {
                        R[P]++;
                    }
                    #endregion
                    break;
                case OpCode.BN1:
                    #region  3C: IF EF1 = 0, M(R(P)) -> R(P).0, ELSE R(P) + 1 -> R(P)
                    for (int i = 1; i < 5; i++)
                    {
                        if (ListINP[i].QueueInput.Count > 0)
                        {
                            if (ListINP[i].Setting == 1)
                            {
                                EF1 = true;
                                break;

                            }
                        }
                    }
                    if (!EF1)
                    {
                        low = M[R[P]];
                        high = R[P] & 0xff00;

                        R[P] = (ushort)(high | low);
                    }
                    else
                    {
                        R[P]++;
                    }
                    #endregion
                    break;
                case OpCode.BN2:
                    #region  3D: IF EF2 = 0, M(R(P)) -> R(P).0, ELSE R(P) + 1 -> R(P)
                    for (int i = 1; i < 5; i++)
                    {
                        if (ListINP[i].QueueInput.Count > 0)
                        {
                            if (ListINP[i].Setting == 2)
                            {
                                EF2 = true;
                                break;

                            }
                        }
                    }
                    if (!EF2)
                    {
                        low = M[R[P]];
                        high = R[P] & 0xff00;

                        R[P] = (ushort)(high | low);
                    }
                    else
                    {
                        R[P]++;
                    }
                    #endregion
                    break;
                case OpCode.BN3:
                    #region  3E: IF EF3 = 0, M(R(P)) -> R(P).0, ELSE R(P) + 1 -> R(P)
                    for (int i = 1; i < 5; i++)
                    {
                        if (ListINP[i].QueueInput.Count > 0)
                        {
                            if (ListINP[i].Setting == 3)
                            {
                                EF3 = true;
                                break;

                            }
                        }
                    }
                    if (!EF3)
                    {
                        low = M[R[P]];
                        high = R[P] & 0xff00;

                        R[P] = (ushort)(high | low);
                    }
                    else
                    {
                        R[P]++;
                    }
                    #endregion
                    break;
                case OpCode.BN4:
                    #region  3F: IF EF4 = 0, M(R(P)) -> R(P).0, ELSE R(P) + 1 -> R(P)
                    for (int i = 1; i < 5; i++)
                    {
                        if (ListINP[i].QueueInput.Count > 0)
                        {
                            if (ListINP[i].Setting == 4)
                            {
                                EF4 = true;
                                break;

                            }
                        }
                    }
                    if (!EF4)
                    {
                        low = M[R[P]];
                        high = R[P] & 0xff00;

                        R[P] = (ushort)(high | low);
                    }
                    else
                    {
                        R[P]++;
                    }
                    #endregion
                    break;
                case OpCode.LDA0:
                    #region   40: M(R(0)) -> D ;  R(0) + 1 -> R(0)
                    D = M[R[0]];
                    R[0]++;
                    #endregion
                    break;
                case OpCode.LDA1:
                    #region   41: M(R(1)) -> D ;  R(1) + 1 -> R(1)
                    D = M[R[1]];
                    R[1]++;
                    //if (R[1] > 0xFFFF)
                    //    R[1] = 0;
                    #endregion
                    break;
                case OpCode.LDA2:
                    #region   42: M(R(2)) -> D ;  R(2) + 1 -> R(2)
                    D = M[R[2]];
                    R[2]++;
                    //if (R[2] > 0xFFFF)
                    //    R[2] = 0;
                    #endregion
                    break;
                case OpCode.LDA3:
                    #region   43: M(R(3)) -> D ;  R(3) + 1 -> R(3)
                    D = M[R[3]];
                    R[3]++;
                    //if (R[3] > 0xFFFF)
                    //    R[3] = 0;
                    #endregion
                    break;
                case OpCode.LDA4:
                    #region   44: M(R(4)) -> D ;  R(4) + 1 -> R(4)
                    D = M[R[4]];
                    R[4]++;
                    //if (R[4] > 0xFFFF)
                    //    R[4] = 0;
                    #endregion
                    break;
                case OpCode.LDA5:
                    #region   45: M(R(5)) -> D ;  R(5) + 1 -> R(5)
                    D = M[R[5]];
                    R[5]++;
                    //if (R[5] > 0xFFFF)
                    //    R[5] = 0;
                    #endregion
                    break;
                case OpCode.LDA6:
                    #region   46: M(R(6)) -> D ;  R(6) + 1 -> R(6)
                    D = M[R[6]];
                    R[6]++;
                    //if (R[6] > 0xFFFF)
                    //    R[6] = 0;
                    #endregion
                    break;
                case OpCode.LDA7:
                    #region   47: M(R(7)) -> D ;  R(7) + 1 -> R(7)
                    D = M[R[7]];
                    R[7]++;
                    //if (R[7] > 0xFFFF)
                    //    R[7] = 0;
                    #endregion
                    break;
                case OpCode.LDA8:
                    #region   48: M(R(8)) -> D ;  R(8) + 1 -> R(8)
                    D = M[R[8]];
                    R[8]++;
                    //if (R[8] > 0xFFFF)
                    //    R[8] = 0;
                    #endregion
                    break;
                case OpCode.LDA9:
                    #region   49: M(R(9)) -> D ;  R(9) + 1 -> R(9)
                    D = M[R[9]];
                    R[9]++;
                    //if (R[9] > 0xFFFF)
                    //    R[9] = 0;
                    #endregion
                    break;
                case OpCode.LDAA:
                    #region   4A: M(R(10)) -> D ;  R(10) + 1 -> R(10)
                    D = M[R[10]];
                    R[10]++;
                    //if (R[10] > 0xFFFF)
                    //    R[10] = 0;
                    #endregion
                    break;
                case OpCode.LDAB:
                    #region   4B: M(R(11)) -> D ;  R(11) + 1 -> R(11)
                    D = M[R[11]];
                    R[11]++;
                    //if (R[11] > 0xFFFF)
                    //    R[11] = 0;
                    #endregion
                    break;
                case OpCode.LDAC:
                    #region   4C: M(R(12)) -> D ;  R(12) + 1 -> R(12)
                    D = M[R[12]];
                    R[12]++;
                    //if (R[12] > 0xFFFF)
                    //    R[12] = 0;
                    #endregion
                    break;
                case OpCode.LDAD:
                    #region   4D: M(R(13)) -> D ;  R(13) + 1 -> R(13)
                    D = M[R[13]];
                    R[13]++;
                    //if (R[13] > 0xFFFF)
                    //    R[13] = 0;
                    #endregion
                    break;
                case OpCode.LDAE:
                    #region   4E: M(R(14)) -> D ;  R(14) + 1 -> R(14)
                    D = M[R[14]];
                    R[14]++;
                    //if (R[14] > 0xFFFF)
                    //    R[14] = 0;
                    #endregion
                    break;
                case OpCode.LDAF:
                    #region  4F: M(R(15)) -> D ;  R(5) + 1 -> R(15)
                    D = M[R[15]];
                    R[15]++;
                    //if (R[15] > 0xFFFF)
                    //    R[15] = 0;
                    #endregion
                    break;
                case OpCode.STR0:
                    #region   50: D -> M(R(0))
                    M[R[0]] = D;
                    #endregion
                    break;
                case OpCode.STR1:
                    #region   51: D -> M(R(1))
                    M[R[1]] = D;
                    #endregion
                    break;
                case OpCode.STR2:
                    #region   52: D -> M(R(2))
                    M[R[2]] = D;
                    #endregion
                    break;
                case OpCode.STR3:
                    #region   53: D -> M(R(3))
                    M[R[3]] = D;
                    #endregion
                    break;
                case OpCode.STR4:
                    #region   54: D -> M(R(4))
                    M[R[4]] = D;
                    #endregion
                    break;
                case OpCode.STR5:
                    #region   55: D -> M(R(5))
                    M[R[5]] = D;
                    #endregion
                    break;
                case OpCode.STR6:
                    #region   56: D -> M(R(6))
                    M[R[6]] = D;
                    #endregion
                    break;
                case OpCode.STR7:
                    #region   57: D -> M(R(7))
                    M[R[7]] = D;
                    #endregion
                    break;
                case OpCode.STR8:
                    #region   58: D -> M(R(8))
                    M[R[8]] = D;
                    #endregion
                    break;
                case OpCode.STR9:
                    #region   59: D -> M(R(9))
                    M[R[9]] = D;
                    #endregion
                    break;
                case OpCode.STRA:
                    #region   5A: D -> M(R(10))
                    M[R[10]] = D;
                    #endregion
                    break;
                case OpCode.STRB:
                    #region   5B: D -> M(R(11))
                    M[R[11]] = D;
                    #endregion
                    break;
                case OpCode.STRC:
                    #region   5C: D -> M(R(12))
                    M[R[12]] = D;
                    #endregion
                    break;
                case OpCode.STRD:
                    #region   5D: D -> M(R(13))
                    M[R[13]] = D;
                    #endregion
                    break;
                case OpCode.STRE:
                    #region   5E: D -> M(R(14))
                    M[R[14]] = D;
                    #endregion
                    break;
                case OpCode.STRF:
                    #region   5F: D -> M(R(15))
                    M[R[15]] = D;
                    #endregion
                    break;
                case OpCode.IRX:
                    #region   60: R(X) + 1 -> R(X)
                    R[X]++;
                    //if (R[X] > 0xffff)
                    //    R[X] = 0x00;
                    #endregion
                    break;
                case OpCode.OUT1:
                    #region   61: M(R(X)) -> BUS ; R(X) + 1 -> R(X) ;N LINES = 1
                    OUT[1] = M[R[X]];
                    R[X]++;
                    OutputFromCpu(1);
                    #endregion
                    break;
                case OpCode.OUT2:
                    #region   62: M(R(X)) -> BUS ; R(X) + 1 -> R(X) ;N LINES = 2
                    OUT[2] = M[R[X]];
                    R[X]++;
                    OutputFromCpu(2);
                    #endregion
                    break;
                case OpCode.OUT3:
                    #region   63: M(R(X)) -> BUS ; R(X) + 1 -> R(X) ;N LINES = 3
                    OUT[3] = M[R[X]];
                    R[X]++;
                    OutputFromCpu(3);
                    #endregion
                    break;
                case OpCode.OUT4:
                    #region   64: M(R(X)) -> BUS ; R(X) + 1 -> R(X) ;N LINES = 4
                    OUT[4] = M[R[X]];
                    R[X]++;
                    OutputFromCpu(4);
                    #endregion
                    break;
                case OpCode.OUT5:
                    #region   65: M(R(X)) -> BUS ; R(X) + 1 -> R(X) ;N LINES = 5
                    OUT[5] = M[R[X]];
                    R[X]++;
                    OutputFromCpu(5);
                    #endregion
                    break;
                case OpCode.OUT6:
                    #region   66: M(R(X)) -> BUS ; R(X) + 1 -> R(X) ;N LINES = 6
                    OUT[6] = M[R[X]];
                    R[X]++;
                    OutputFromCpu(6);
                    #endregion
                    break;
                case OpCode.OUT7:
                    #region   67: M(R(X)) -> BUS ; R(X) + 1 -> R(X) ;N LINES = 7
                    OUT[6] = M[R[X]];
                    R[X]++;
                    OutputFromCpu(7);
                    #endregion
                    break;
                case OpCode.Code68xx:
                    ExecuteOpCode68(M[R[P]]);
                    break;
                case OpCode.INP1:
                    #region   69: BUS -> M(R(X)) ; BUS -> D ; N LINES = 1

                    InputToCpu(1);

                    M[R[X]] = (byte)INP[1];
                    D = M[R[X]];

                    if (Monitor.On)
                    {
                        MyMonitor.DmaPointer = 0;
                        MyMonitor.DmaOutOnOff(true);
                    }


                    #endregion
                    break;
                case OpCode.INP2:
                    #region   6A: BUS -> M(R(X)) ; BUS -> D ; N LINES = 2

                    InputToCpu(2);

                    M[R[X]] = (byte)INP[2];
                    D = M[R[X]];

                    #endregion
                    break;
                case OpCode.INP3:
                    #region   6B: BUS -> M(R(X)) ; BUS -> D ; N LINES = 3

                    InputToCpu(3);

                    M[R[X]] = (byte)INP[3];
                    D = M[R[X]];

                    #endregion
                    break;
                case OpCode.INP4:
                    #region   6C: BUS -> M(R(X)) ; BUS -> D ; N LINES = 4

                    InputToCpu(4);

                    M[R[X]] = (byte)INP[4];
                    D = M[R[X]];

                    #endregion
                    break;
                case OpCode.INP5:
                    #region   6D: BUS -> M(R(X)) ; BUS -> D ; N LINES = 5

                    InputToCpu(5);

                    M[R[X]] = (byte)INP[5];
                    D = M[R[X]];

                    #endregion
                    break;
                case OpCode.INP6:
                    #region   6E: BUS -> M(R(X)) ; BUS -> D ; N LINES = 6

                    InputToCpu(6);

                    M[R[X]] = (byte)INP[6];
                    D = M[R[X]];

                    #endregion
                    break;
                case OpCode.INP7:
                    #region   6F: BUS -> M(R(X)) ; BUS -> D ; N LINES = 7

                    InputToCpu(7);

                    M[R[X]] = (byte)INP[7];
                    D = M[R[X]];

                    #endregion
                    break;
                case OpCode.RET:
                    #region   70: M(R(X)) -> (X, P) ;  R(X) + 1 -> R(X), 1 -> IE

                    P = (byte)(M[R[X]] & 0x0F);
                    int x = M[R[X]] & 0xF0;
                    R[X]++;
                    X = (byte)(x >> 4);


                    IE = true;
                    #endregion
                    break;
                case OpCode.DIS:
                    #region   71: M(R(X)) -> (X, P) ;  R(X) + 1 -> R(X), 0 -> IE

                    P = (byte)(M[R[X]] & 0x0F);
                    x = M[R[X]] & 0xF0;
                    R[X]++;
                    X = (byte)(x >> 4);


                    IE = false;
                    #endregion
                    break;
                case OpCode.LDXA:
                    #region   72: M(R(X)) -> D ;  R(X) + 1 -> R(X)
                    D = M[R[X]];
                    R[X]++;
                    #endregion
                    break;
                case OpCode.STXD:
                    #region   73: D -> M(R(X)) ; R(X) - 1 -> R(X)
                    M[R[X]] = D;
                    R[X]--;
                    #endregion
                    break;
                case OpCode.ADC:
                    #region   74: M(R(X)) + D + DF -> DF, D
                    if (DF)
                        übertrag = 1;
                    else
                        übertrag = 0;

                    erg = M[R[X]] + D + übertrag;
                    D = (byte)(erg);

                    if (erg > 0xff)
                        DF = true;
                    else
                        DF = false;
                    #endregion
                    break;
                case OpCode.SDB:
                    #region  75:  M(R(X)) - D - (NOT DF) -> DF, D
                    if (DF)            //DF = false, wenn vorher Übertrag
                        übertrag = 0;
                    else
                        übertrag = 1;

                    einerkomplement = (D + übertrag) ^ (byte)0b1111_1111;
                    zweierkomplement = einerkomplement + 1;

                    erg = M[R[X]] + zweierkomplement;

                    if (erg > 0xff)
                        DF = true;
                    else
                        DF = false;

                    D = (byte)erg;
                    #endregion
                    break;
                case OpCode.SHRC:
                    #region   76: SHIFT D RIGHT, LSB(D) -> DF, DF -> MSB(D) (Note 2)

                    erg = D & 0b0000_0001;

                    d = D >> 1;

                    if (DF)
                        D = (byte)(d | 0b1000_0000);
                    else
                        D = (byte)(d);


                    if (erg > 0)
                        DF = true;
                    else
                        DF = false;

                    #endregion
                    break;
                case OpCode.SMB:
                    #region   77: D-M(R(X))-(NOT DF) -> DF, D

                    if (DF)            //DF = false, wenn vorher Übertrag
                        übertrag = 0;
                    else
                        übertrag = 1;


                    einerkomplement = (M[R[X]] + übertrag) ^ (byte)0b1111_1111;
                    zweierkomplement = einerkomplement + 1;

                    erg = D + zweierkomplement;

                    if (erg > 0xff)
                        DF = true;
                    else
                        DF = false;

                    D = (byte)erg;

                    #endregion
                    break;
                case OpCode.SAV:
                    #region   78: T->M(R(X))

                    M[R[X]] = T;

                    #endregion
                    break;
                case OpCode.MARK:
                    #region   79: (X, P)-> M(R(2)), THEN P-> X ; R(2) - 1-> R(2)

                    M[R[2]] = (byte)(X << 4 | P);
                    X = P;
                    R[2]--;

                    #endregion
                    break;
                case OpCode.REQ:
                    #region 7A:  0 -> Q
                    Q = false;
                    #endregion
                    break;
                case OpCode.SEQ:
                    #region 7B:  1 -> Q
                    Q = true;
                    #endregion
                    break;
                case OpCode.ADCI:
                    #region 7C  M(R(P)) + D + DF -> DF, D ; R(P) + 1 -> R(P)
                    if (DF)
                        übertrag = 1;
                    else
                        übertrag = 0;

                    erg = M[R[P]] + D + übertrag;
                    D = (byte)(erg);

                    if (erg > 0xff)
                        DF = true;
                    else
                        DF = false;
                    R[P]++;
                    #endregion
                    break;
                case OpCode.SDBI:
                    #region  7D:  M(R(P)) - D - (Not DF) -> DF, D  ; R(P) + 1 -> R(P)
                    if (DF)            //DF = false, wenn vorher Übertrag
                        übertrag = 0;
                    else
                        übertrag = 1;

                    einerkomplement = (D + übertrag) ^ (byte)0b1111_1111;
                    zweierkomplement = einerkomplement + 1;

                    erg = M[R[P]] + zweierkomplement;

                    if (erg > 0xff)
                        DF = true;
                    else
                        DF = false;

                    D = (byte)erg;

                    R[P]++;
                    #endregion
                    break;
                case OpCode.SHLC:
                    #region   7E: SHIFT D LEFT, MSB(D) -> DF, DF -> LSB(D) (Note 2)

                    erg = D & 0b1000_0000;

                    d = D << 1;

                    if (DF)
                        D = (byte)(d | 0b0000_0001);
                    else
                        D = (byte)(d);

                    if (erg > 0)
                        DF = true;
                    else
                        DF = false;

                    #endregion
                    break;
                case OpCode.SMBI:
                    #region  7F:  D-M(R(P))-(NOT DF) -> DF, D  ;  R(P) + 1 -> R(P)

                    if (DF)            //DF = false, wenn vorher Übertrag
                        übertrag = 0;
                    else
                        übertrag = 1;

                    einerkomplement = (M[R[P]] + übertrag) ^ (byte)0b1111_1111;
                    zweierkomplement = einerkomplement + 1;

                    erg = D + zweierkomplement;

                    if (erg > 0xff)
                        DF = true;
                    else
                        DF = false;

                    D = (byte)erg;

                    R[P]++;

                    #endregion
                    break;
                case OpCode.GLO0:
                    #region 80   R(0).0 -> D
                    D = (byte)(R[0] & 0x00ff);
                    #endregion
                    break;
                case OpCode.GLO1:
                    #region 81   R(1).0 -> D
                    D = (byte)(R[1] & 0x00ff);
                    #endregion
                    break;
                case OpCode.GLO2:
                    #region 82   R(2).0 -> D
                    D = (byte)(R[2] & 0x00ff);
                    #endregion
                    break;
                case OpCode.GLO3:
                    #region 83   R(3).0 -> D
                    D = (byte)(R[3] & 0x00ff);
                    #endregion
                    break;
                case OpCode.GLO4:
                    #region 84   R(4).0 -> D
                    D = (byte)(R[4] & 0x00ff);
                    #endregion
                    break;
                case OpCode.GLO5:
                    #region 85   R(5).0 -> D
                    D = (byte)(R[5] & 0x00ff);
                    #endregion
                    break;
                case OpCode.GLO6:
                    #region 86   R(6).0 -> D
                    D = (byte)(R[6] & 0x00ff);
                    #endregion
                    break;
                case OpCode.GLO7:
                    #region 87   R(7).0 -> D
                    D = (byte)(R[7] & 0x00ff);
                    #endregion
                    break;
                case OpCode.GLO8:
                    #region 88   R(8).0 -> D
                    D = (byte)(R[8] & 0x00ff);
                    #endregion
                    break;
                case OpCode.GLO9:
                    #region 89   R(9).0 -> D
                    D = (byte)(R[9] & 0x00ff);
                    #endregion
                    break;
                case OpCode.GLOA:
                    #region 8A   R(10).0 -> D
                    D = (byte)(R[10] & 0x00ff);
                    #endregion
                    break;
                case OpCode.GLOB:
                    #region 8B   R(11).0 -> D
                    D = (byte)(R[11] & 0x00ff);
                    #endregion
                    break;
                case OpCode.GLOC:
                    #region 8C   R(12).0 -> D
                    D = (byte)(R[12] & 0x00ff);
                    #endregion
                    break;
                case OpCode.GLOD:
                    #region 8D   R(13).0 -> D
                    D = (byte)(R[13] & 0x00ff);
                    #endregion
                    break;
                case OpCode.GLOE:
                    #region 8E   R(14).0 -> D
                    D = (byte)(R[14] & 0x00ff);
                    #endregion
                    break;
                case OpCode.GLOF:
                    #region 8F  R(15).0 -> D
                    D = (byte)(R[15] & 0x00ff);
                    #endregion
                    break;
                case OpCode.GHI0:
                    #region 90   R(0).1 -> D
                    D = (byte)(R[0] >> 8);
                    #endregion
                    break;
                case OpCode.GHI1:
                    #region 91   R(1).1 -> D
                    D = (byte)(R[1] >> 8);
                    #endregion
                    break;
                case OpCode.GHI2:
                    #region 92   R(2).1 -> D
                    D = (byte)(R[2] >> 8);
                    #endregion
                    break;
                case OpCode.GHI3:
                    #region 93   R(3).1 -> D
                    D = (byte)(R[3] >> 8);
                    #endregion
                    break;
                case OpCode.GHI4:
                    #region 94   R(4).1 -> D
                    D = (byte)(R[4] >> 8);
                    #endregion
                    break;
                case OpCode.GHI5:
                    #region 95   R(5).1 -> D
                    D = (byte)(R[5] >> 8);
                    #endregion
                    break;
                case OpCode.GHI6:
                    #region 96   R(6).1 -> D
                    D = (byte)(R[6] >> 8);
                    #endregion
                    break;
                case OpCode.GHI7:
                    #region 97   R(7).1 -> D
                    D = (byte)(R[7] >> 8);
                    #endregion
                    break;
                case OpCode.GHI8:
                    #region 98   R(8).1 -> D
                    D = (byte)(R[8] >> 8);
                    #endregion
                    break;
                case OpCode.GHI9:
                    #region 99   R(9).1 -> D
                    D = (byte)(R[9] >> 8);
                    #endregion
                    break;
                case OpCode.GHIA:
                    #region 9A   R(A).1 -> D
                    D = (byte)(R[10] >> 8);
                    #endregion
                    break;
                case OpCode.GHIB:
                    #region 9B   R(B).1 -> D
                    D = (byte)(R[11] >> 8);
                    #endregion
                    break;
                case OpCode.GHIC:
                    #region 9C   R(C).1 -> D
                    D = (byte)(R[12] >> 8);
                    #endregion
                    break;
                case OpCode.GHID:
                    #region 9D   R(D).1 -> D
                    D = (byte)(R[13] >> 8);
                    #endregion
                    break;
                case OpCode.GHIE:
                    #region 9E   R(E).1 -> D
                    D = (byte)(R[14] >> 8);
                    #endregion
                    break;
                case OpCode.GHIF:
                    #region 9F   R(F).1 -> D
                    D = (byte)(R[15] >> 8);
                    #endregion
                    break;
                case OpCode.PLO0:
                    #region A0  D -> R(0).0
                    erg = R[0] & 0xff00;
                    R[0] = (ushort)(erg | D);
                    #endregion
                    break;
                case OpCode.PLO1:
                    #region A1  D -> R(1).0
                    erg = R[1] & 0xff00;
                    R[1] = (ushort)(erg | D);
                    #endregion
                    break;
                case OpCode.PLO2:
                    #region A2  D -> R(2).0
                    erg = R[2] & 0xff00;
                    R[2] = (ushort)(erg | D);
                    #endregion
                    break;
                case OpCode.PLO3:
                    #region A3  D -> R(3).0
                    erg = R[3] & 0xff00;
                    R[3] = (ushort)(erg | D);
                    #endregion
                    break;
                case OpCode.PLO4:
                    #region A4  D -> R(4).0
                    erg = R[4] & 0xff00;
                    R[4] = (ushort)(erg | D);
                    //erg = R[4] & 0xff00;
                    //R[4] = erg | D;
                    #endregion
                    break;
                case OpCode.PLO5:
                    #region A5  D -> R(5).0
                    erg = R[5] & 0xff00;
                    R[5] = (ushort)(erg | D);
                    //erg = R[5] & 0xff00;
                    //R[5] = erg | D;
                    #endregion
                    break;
                case OpCode.PLO6:
                    #region A6  D -> R(6).0
                    erg = R[6] & 0xff00;
                    R[6] = (ushort)(erg | D);
                    //erg = R[6] & 0xff00;
                    //R[6] = erg | D;
                    #endregion
                    break;
                case OpCode.PLO7:
                    #region A7  D -> R(7).0
                    erg = R[7] & 0xff00;
                    R[7] = (ushort)(erg | D);
                    //erg = R[7] & 0xff00;
                    //R[7] = erg | D;
                    #endregion
                    break;
                case OpCode.PLO8:
                    #region A8  D -> R(8).0
                    erg = R[8] & 0xff00;
                    R[8] = (ushort)(erg | D);
                    //erg = R[8] & 0xff00;
                    //R[8] = erg | D;
                    #endregion
                    break;
                case OpCode.PLO9:
                    erg = R[9] & 0xff00;
                    R[9] = (ushort)(erg | D);
                    #region A9  D -> R(9).0
                    //erg = R[9] & 0xff00;
                    //R[9] = erg | D;
                    #endregion
                    break;
                case OpCode.PLOA:
                    #region AA  D -> R(A).0
                    erg = R[10] & 0xff00;
                    R[10] = (ushort)(erg | D);
                    //erg = R[10] & 0xff00;
                    //R[10] = erg | D;
                    #endregion
                    break;
                case OpCode.PLOB:
                    #region AB  D -> R(B).0
                    erg = R[11] & 0xff00;
                    R[11] = (ushort)(erg | D);
                    //erg = R[11] & 0xff00;
                    //R[11] = erg | D;
                    #endregion
                    break;
                case OpCode.PLOC:
                    #region AB D -> R(B).0
                    erg = R[12] & 0xff00;
                    R[12] = (ushort)(erg | D);
                    //erg = R[12] & 0xff00;
                    //R[12] = erg | D;
                    #endregion
                    break;
                case OpCode.PLOD:
                    #region AD  D -> R(D).0
                    erg = R[13] & 0xff00;
                    R[13] = (ushort)(erg | D);
                    //erg = R[13] & 0xff00;
                    //R[13] = erg | D;
                    #endregion
                    break;
                case OpCode.PLOE:
                    #region AE  D -> R(E).0
                    erg = R[14] & 0xff00;
                    R[14] = (ushort)(erg | D);
                    //erg = R[14] & 0xff00;
                    //R[14] = erg | D;
                    #endregion
                    break;
                case OpCode.PLOF:
                    #region AF  D -> R(F).0
                    erg = R[15] & 0xff00;
                    R[15] = (ushort)(erg | D);
                    //erg = R[15] & 0xff00;
                    //R[15] = erg | D;
                    #endregion
                    break;
                case OpCode.PHI0:
                    #region B0  D -> R(0).1
                    erg = R[0] & 0x00ff;
                    R[0] = (ushort)((D << 8) | erg);
                    //erg = R[0] & 0x00ff;
                    //R[0] = (D << 8) | erg;
                    #endregion
                    break;
                case OpCode.PHI1:
                    #region B1  D -> R(1).1
                    erg = R[1] & 0x00ff;
                    R[1] = (ushort)((D << 8) | erg);
                    //erg = R[1] & 0x00ff;
                    //R[1] = (D << 8) | erg;
                    #endregion
                    break;
                case OpCode.PHI2:
                    #region B2  D -> R(2).1
                    erg = R[2] & 0x00ff;
                    R[2] = (ushort)((D << 8) | erg);
                    //erg = R[2] & 0x00ff;
                    //R[2] = (D << 8) | erg;
                    #endregion
                    break;
                case OpCode.PHI3:
                    #region B3  D -> R(3).1
                    erg = R[3] & 0x00ff;
                    R[3] = (ushort)((D << 8) | erg);

                    //erg = R[3] & 0x00ff;
                    //R[3] = (D << 8) | erg;
                    #endregion
                    break;
                case OpCode.PHI4:
                    #region B4  D -> R(4).1
                    erg = R[4] & 0x00ff;
                    R[4] = (ushort)((D << 8) | erg);
                    //erg = R[4] & 0x00ff;
                    //R[4] = (D << 8) | erg;
                    #endregion
                    break;
                case OpCode.PHI5:
                    #region B5  D -> R(5).1
                    erg = R[5] & 0x00ff;
                    R[5] = (ushort)((D << 8) | erg);
                    //erg = R[5] & 0x00ff;
                    //R[5] = (D << 8) | erg;
                    #endregion
                    break;
                case OpCode.PHI6:
                    #region B06  D -> R(6).1
                    erg = R[6] & 0x00ff;
                    R[6] = (ushort)((D << 8) | erg);
                    //erg = R[6] & 0x00ff;
                    //R[6] = (D << 8) | erg;
                    #endregion
                    break;
                case OpCode.PHI7:
                    #region B7 D -> R(7).1
                    erg = R[7] & 0x00ff;
                    R[7] = (ushort)((D << 8) | erg);
                    //erg = R[7] & 0x00ff;
                    //R[7] = (D << 8) | erg;
                    #endregion
                    break;
                case OpCode.PHI8:
                    #region B8  D -> R(8).1
                    erg = R[8] & 0x00ff;
                    R[8] = (ushort)((D << 8) | erg);
                    //erg = R[8] & 0x00ff;
                    //R[8] = (D << 8) | erg;
                    #endregion
                    break;
                case OpCode.PHI9:
                    #region B9  D -> R(9).1
                    erg = R[9] & 0x00ff;
                    R[9] = (ushort)((D << 8) | erg);
                    //erg = R[9] & 0x00ff;
                    //R[9] = (D << 8) | erg;
                    #endregion
                    break;
                case OpCode.PHIA:
                    #region BA  D -> R(A).1
                    erg = R[10] & 0x00ff;
                    R[10] = (ushort)((D << 8) | erg);
                    //erg = R[10] & 0x00ff;
                    //R[10] = (D << 8) | erg;
                    #endregion
                    break;
                case OpCode.PHIB:
                    #region BB  D -> R(B).1
                    erg = R[11] & 0x00ff;
                    R[11] = (ushort)((D << 8) | erg);
                    //erg = R[11] & 0x00ff;
                    //R[11] = (D << 8) | erg;
                    #endregion
                    break;
                case OpCode.PHIC:
                    #region BC  D -> R(C).1
                    erg = R[12] & 0x00ff;
                    R[12] = (ushort)((D << 8) | erg);
                    //erg = R[12] & 0x00ff;
                    //R[12] = (D << 8) | erg;
                    #endregion
                    break;
                case OpCode.PHID:
                    #region BD  D -> R(D).1
                    erg = R[13] & 0x00ff;
                    R[13] = (ushort)((D << 8) | erg);
                    //erg = R[13] & 0x00ff;
                    //R[13] = (D << 8) | erg;
                    #endregion
                    break;
                case OpCode.PHIE:
                    #region BE  D -> R(E).1
                    erg = R[14] & 0x00ff;
                    R[14] = (ushort)((D << 8) | erg);
                    //erg = R[14] & 0x00ff;
                    //R[14] = (D << 8) | erg;
                    #endregion
                    break;
                case OpCode.PHIF:
                    #region BF D -> R(F).1
                    erg = R[15] & 0x00ff;
                    R[15] = (ushort)((D << 8) | erg);
                    //erg = R[15] & 0x00ff;
                    //R[15] = (D << 8) | erg;
                    #endregion
                    break;
                case OpCode.LBR:
                    #region C0    M(R(P)) -> R(P). 1, M(R(P) + 1) -> R(P).0
                    erg = M[R[P]] << 8;
                    d = M[R[P] + 1];
                    R[P] = (ushort)(erg | d);
                    #endregion
                    break;
                case OpCode.LBQ:
                    #region C1    IF Q = 1, M(R(P)) -> R(P).1, M(R(P) + 1) -> R(P).0, ELSE R(P) + 2 -> R(P)
                    if (Q)
                    {
                        erg = M[R[P]] << 8;
                        d = M[R[P] + 1];
                        R[P] = (ushort)(erg | d);
                    }
                    else
                    {
                        R[P] += 2;
                    }
                    #endregion
                    break;
                case OpCode.LBZ:
                    #region C2    IF D=0, M(R(P)) -> R(P).1, M(R(P) + 1) -> R(P).0, ELSE R(P) + 2 -> R(P)
                    if (D == 0)
                    {
                        erg = M[R[P]] << 8;
                        d = M[R[P] + 1];
                        R[P] = (ushort)(erg | d);
                    }
                    else
                    {
                        R[P] += 2;
                    }
                    #endregion
                    break;
                case OpCode.LBDF:
                    #region C3    IF DF=1, M(R(P)) -> R(P).1, M(R(P) + 1) -> R(P).0, ELSE R(P) + 2 -> R(P)
                    if (DF)
                    {
                        erg = M[R[P]] << 8;
                        d = M[R[P] + 1];
                        R[P] = (ushort)(erg | d);
                    }
                    else
                    {
                        R[P] += 2;
                    }
                    #endregion
                    break;
                case OpCode.NOP:
                    #region  C4
                    #endregion
                    break;
                case OpCode.LSNQ:
                    #region  C5    IF Q = 0, R(P) + 2 -> R(P), ELSE CONTINUE
                    if (!Q)
                        R[P] += 2;
                    #endregion
                    break;
                case OpCode.LSNZ:
                    #region  C6    IF D Not 0, R(P) + 2 -> R(P), ELSE CONTINUE
                    if (D != 0)
                        R[P] += 2;
                    #endregion
                    break;
                case OpCode.LSNF:
                    #region  C7    IF DF = 0, R(P) + 2 -> R(P), ELSE CONTINUE
                    if (!DF)
                        R[P] += 2;
                    #endregion
                    break;
                case OpCode.NLBR:
                    #region  C8    R(P) + 2 -> R(P) (Note 2)
                    R[P] += 2;
                    #endregion
                    break;
                case OpCode.LBNQ:
                    #region  C9    IF Q = 0, M(R(P)) -> R(P).1, M(R(P) + 1) -> R(P).0 EISE R(P) + 2 -> R(P)
                    if (!Q)
                    {
                        erg = M[R[P]] << 8;
                        d = M[R[P] + 1];
                        R[P] = (ushort)(erg | d);
                    }
                    else
                    {
                        R[P] += 2;
                    }
                    #endregion
                    break;
                case OpCode.LBNZ:
                    #region  CA    IF D Not 0, M(R(P))-> R(P).1, M(R(P) + 1)-> R(P).0, ELSE R(P) + 2 -> R(P)
                    if (D != 0)
                    {
                        erg = M[R[P]] << 8;
                        d = M[R[P] + 1];
                        R[P] = (ushort)(erg | d);
                    }
                    else
                    {
                        R[P] += 2;
                    }
                    #endregion
                    break;
                case OpCode.LBNF:
                    #region  CB    IF DF = 0, M(R(P))-> R(P).1, M(R(P) + 1)-> R(P).0, ELSE R(P) + 2 -> R(P)
                    if (!DF)
                    {
                        erg = M[R[P]] << 8;
                        d = M[R[P] + 1];
                        R[P] = (ushort)(erg | d);
                    }
                    else
                    {
                        R[P] += 2;
                    }
                    #endregion
                    break;
                case OpCode.LSIE:
                    #region  CC    IF IE = 1, R(P) + 2 -> R(P), ELSE CONTINUE
                    if (IE)
                        R[P] += 2;
                    #endregion
                    break;
                case OpCode.LSQ:
                    #region  CD    IF Q = 1, R(P) + 2 -> R(P), ELSE CONTINUE
                    if (Q)
                        R[P] += 2;
                    #endregion
                    break;
                case OpCode.LSZ:
                    #region  CE    IF D = 0, R(P) + 2 -> R(P), ELSE CONTINUE
                    if (D == 0)
                        R[P] += 2;
                    #endregion
                    break;
                case OpCode.LSDF:
                    #region  CF    IF DF = 1, R(P) + 2 -> R(P), ELSE CONTINUE
                    if (DF)
                        R[P] += 2;
                    #endregion
                    break;
                case OpCode.SEP0:
                    #region  D0    0 -> P
                    P = 0;
                    #endregion
                    break;
                case OpCode.SEP1:
                    #region  D1    1 -> P
                    P = 1;
                    #endregion
                    break;
                case OpCode.SEP2:
                    #region  D2    2 -> P
                    P = 2;
                    #endregion
                    break;
                case OpCode.SEP3:
                    #region  D3    3-> P
                    P = 3;
                    #endregion
                    break;
                case OpCode.SEP4:
                    #region  D4    4 -> P
                    P = 4;
                    #endregion
                    break;
                case OpCode.SEP5:
                    #region  D5    5-> P
                    P = 5;
                    #endregion
                    break;
                case OpCode.SEP6:
                    #region  D6    6 -> P
                    P = 6;
                    #endregion
                    break;
                case OpCode.SEP7:
                    #region  D7    7 -> P
                    P = 7;
                    #endregion
                    break;
                case OpCode.SEP8:
                    #region  D8    8 -> P
                    P = 8;
                    #endregion
                    break;
                case OpCode.SEP9:
                    #region  D9    9 -> P
                    P = 9;
                    #endregion
                    break;
                case OpCode.SEPA:
                    #region  DA    10 -> P
                    P = 10;
                    #endregion
                    break;
                case OpCode.SEPB:
                    #region  DB    11 -> P
                    P = 11;
                    #endregion
                    break;
                case OpCode.SEPC:
                    #region  DC    12 -> P
                    P = 12;
                    #endregion
                    break;
                case OpCode.SEPD:
                    #region  DD    13 -> P
                    P = 13;
                    #endregion
                    break;
                case OpCode.SEPE:
                    #region  DE    14 -> P
                    P = 14;
                    #endregion
                    break;
                case OpCode.SEPF:
                    #region  DF    15 -> P
                    P = 15;
                    #endregion
                    break;
                case OpCode.SEX0:
                    #region  E0    0 -> X
                    X = 0;
                    #endregion
                    break;
                case OpCode.SEX1:
                    #region  E1    1 -> X
                    X = 1;
                    #endregion
                    break;
                case OpCode.SEX2:
                    #region  E2    2 -> X
                    X = 2;
                    #endregion
                    break;
                case OpCode.SEX3:
                    #region  E3    3 -> X
                    X = 3;
                    #endregion
                    break;
                case OpCode.SEX4:
                    #region  E4    4 -> X
                    X = 4;
                    #endregion
                    break;
                case OpCode.SEX5:
                    #region  E5   5 -> X
                    X = 5;
                    #endregion
                    break;
                case OpCode.SEX6:
                    #region  E6    6-> X
                    X = 6;
                    #endregion
                    break;
                case OpCode.SEX7:
                    #region  E7    7 -> X
                    X = 7;
                    #endregion
                    break;
                case OpCode.SEX8:
                    #region  E8    8 -> X
                    X = 8;
                    #endregion
                    break;
                case OpCode.SEX9:
                    #region  E9    9 -> X
                    X = 9;
                    #endregion
                    break;
                case OpCode.SEXA:
                    #region  EA    10 -> X
                    X = 10;
                    #endregion
                    break;
                case OpCode.SEXB:
                    #region  EB   11 -> X
                    X = 11;
                    #endregion
                    break;
                case OpCode.SEXC:
                    #region  EB    12 -> X
                    X = 12;
                    #endregion
                    break;
                case OpCode.SEXD:
                    #region  ED    13 -> X
                    X = 13;
                    #endregion
                    break;
                case OpCode.SEXE:
                    #region  EE    14 -> X
                    X = 14;
                    #endregion
                    break;
                case OpCode.SEXF:
                    #region  EF    15 -> X
                    X = 15;
                    #endregion
                    break;
                case OpCode.LDX:
                    #region  F0    M(R(X)) -> D
                    D = M[R[X]];
                    #endregion
                    break;
                case OpCode.OR:
                    #region  F1    M(R(X)) OR D -> D
                    D = (byte)(M[R[X]] | D);
                    #endregion
                    break;
                case OpCode.AND:
                    #region  F2    M(R(X)) AND D -> D
                    D = (byte)(M[R[X]] & D);
                    #endregion
                    break;
                case OpCode.XOR:
                    #region  F3    M(R(X)) XOR D -> D
                    D = (byte)(M[R[X]] ^ D);
                    #endregion
                    break;
                case OpCode.ADD:
                    #region  F4    M(R(X)) + D -> DF, D

                    erg = M[R[X]] + D;
                    D = (byte)(erg);

                    if (erg > 0xff)
                        DF = true;
                    else
                        DF = false;
                    #endregion
                    break;
                case OpCode.SD:
                    #region  F5:  M(R(X)) - D  -> DF, D

                    einerkomplement = (D) ^ (byte)0b1111_1111;
                    zweierkomplement = einerkomplement + 1;

                    erg = M[R[X]] + zweierkomplement;

                    if (erg > 0xff)
                        DF = true;
                    else
                        DF = false;

                    D = (byte)erg;
                    #endregion
                    break;
                case OpCode.SHR:
                    #region  F6:   SHIFT D RIGHT, LSB(D) -> DF, 0 -> MSB(D)

                    if (((int)D & 0x1) == 0x1)
                        DF = true;
                    else
                        DF = false;
                    d = D >> 1;
                    D = (byte)d;
                    #endregion
                    break;
                case OpCode.SM:
                    #region  F7:  D-M(R(X)) -> DF, D

                    einerkomplement = (M[R[X]]) ^ (byte)0b1111_1111;
                    zweierkomplement = einerkomplement + 1;

                    erg = D + zweierkomplement;

                    if (erg > 0xff)
                        DF = true;
                    else
                        DF = false;

                    D = (byte)erg;
                    #endregion
                    break;
                case OpCode.LDI:
                    #region  F8:   M(R(P)) -> D ;  R(P) + 1 -> R(P)

                    D = M[R[P]];
                    R[P]++;

                    #endregion
                    break;
                case OpCode.ORI:
                    #region  F9:   M(R(P)) OR D -> D   R(P) + 1 -> R(P)

                    D = (byte)(M[R[P]] | D);
                    R[P]++;

                    #endregion
                    break;
                case OpCode.ANI:
                    #region  FA:   M(R(P)) AND D -> D   R(P) + 1 -> R(P)

                    D = (byte)(M[R[P]] & D);
                    R[P]++;

                    #endregion
                    break;
                case OpCode.XRI:
                    #region  FB:   M(R(P)) XOR D -> D    R(P) + 1 -> R(P)

                    D = (byte)(M[R[P]] ^ D);
                    R[P]++;

                    #endregion
                    break;
                case OpCode.ADI:
                    #region FC  M(R(P)) + D + DF -> DF, D ; R(P) + 1 -> R(P)     M(R(P)) + D -> DF, D   R(P) + 1 -> R(P)

                    erg = M[R[P]] + D;
                    D = (byte)(erg);

                    if (erg > 0xff)
                        DF = true;
                    else
                        DF = false;
                    R[P]++;
                    #endregion
                    break;
                case OpCode.SDI:
                    #region  FD:  M(R(P)) - D -> DF, D   R(P) + 1 -> R(P)

                    einerkomplement = (D) ^ (byte)0b1111_1111;
                    zweierkomplement = einerkomplement + 1;

                    erg = M[R[P]] + zweierkomplement;

                    if (erg > 0xff)
                        DF = true;
                    else
                        DF = false;

                    D = (byte)erg;

                    R[P]++;

                    #endregion
                    break;
                case OpCode.SHL:
                    #region  FE: SHIFT D LEFT, MSB(D) -> DF, 0 -> LSB(D)
                    if (D > 0b0111_1111)
                        DF = true;
                    else
                        DF = true;
                    d = D << 1;
                    D = (byte)d;
                    #endregion
                    break;
                case OpCode.SMI:
                    #region  FF: D-M(R(P)) -> DF, D   R(P) + 1 -> R(P)

                    einerkomplement = M[R[P]] ^ (byte)0b1111_1111;
                    zweierkomplement = einerkomplement + 1;

                    erg = D + zweierkomplement;

                    if (erg > 0xff)
                        DF = true;
                    else
                        DF = false;

                    D = (byte)erg;

                    R[P]++;

                    #endregion
                    break;
                case OpCode.BPZ:
                    break;
                case OpCode.BGE:
                    break;
                case OpCode.SKP:
                    break;
                case OpCode.BM:
                    break;
                case OpCode.BL:
                    break;
                case OpCode.RSHR:
                    break;
                case OpCode.RSHL:
                    break;
                case OpCode.LSKP:
                    break;
                default:
                    break;
            }

            DoDmaOut();


        }

        private void DoDmaOut()
        {
            if (Monitor.DmaOutActive)
            {
                int maxDmaByte = Monitor.ROW * Monitor.COL;


                for (int i = 0; i < 8; i++)
                {
                    if (M[R[0]] >= 0x10 && M[R[0]] <= 127)
                        MyMonitor.DmaValues[MyMonitor.DmaPointer] = (byte)M[R[0]];
                    else
                        MyMonitor.DmaValues[MyMonitor.DmaPointer] = 32;

                    MyMonitor.DmaPointer += 1;
                    R[0]++;
                }

                if ((MyMonitor.DmaPointer) >= maxDmaByte)
                {
                    MyMonitor.DmaPointer = 0;
                }
            }
        }

        // Execute68 : ein Befehl komplett ausführen
        private void ExecuteOpCode68(int Opcode68xx)
        {
            int low = 0;
            int high = 0;

            switch (Opcode68xx)
            {
                #region 0-f
                case 0x0:
                    break;
                case 0x1:
                    break;
                case 0x2:
                    break;
                case 0x3:
                    break;
                case 0x4:
                    break;
                case 0x5:
                    break;
                case 0x6:
                    break;
                case 0x7:
                    break;
                case 0x8:
                    break;
                case 0x9:
                    break;
                case 0xA:
                    break;
                case 0xB:
                    break;
                case 0xC:
                    break;
                case 0xD:
                    break;
                case 0xE:
                    break;
                case 0xF:
                    break;
                #endregion
                #region 10-1f
                case 0x10:
                    break;
                case 0x11:
                    break;
                case 0x12:
                    break;
                case 0x13:
                    break;
                case 0x14:
                    break;
                case 0x15:
                    break;
                case 0x16:
                    break;
                case 0x17:
                    break;
                case 0x18:
                    break;
                case 0x19:
                    break;
                case 0x1A:
                    break;
                case 0x1B:
                    break;
                case 0x1C:
                    break;
                case 0x1D:
                    break;
                case 0x1E:
                    break;
                case 0x1F:
                    break;
                #endregion
                #region 20-2f  DBNZ: Decrement REG N and long branch if not equal 0
                case 0x20:
                case 0x21:
                case 0x22:
                case 0x23:
                case 0x24:
                case 0x25:
                case 0x26:
                case 0x27:
                case 0x28:
                case 0x29:
                case 0x2A:
                case 0x2B:
                case 0x2C:
                case 0x2D:
                case 0x2E:
                case 0x2F:

                    //        DBNZ
                    // R(N) - 1 -> R(N); 
                    // IF R(N) NOT 0, M(R(P)) -> R(P).1, M(R(P) + 1) -> R(P).0, ELSE R(P) + 2 -> R(P) 
                    // 
                    //--------------- Detail -------------
                    //RN-1 -> RN
                    //MRP -> B; RP + 1 -> RP
                    //TAKEN: B -> RP.1, MRP -> RP.0 NOT TAKEN: RP + 1 -> RP

                    //RN-1 -> RN
                    int reg = M[R[P]] & 0xF;
                    R[reg]--;
                    R[P]++;
                    //MRP -> B; RP + 1 -> RP
                    B = (byte)(M[R[P]]);
                    R[P]++;
                    //TAKEN: B -> RP.1, MRP -> RP.0 NOT TAKEN: RP + 1 -> RP

                    if (R[reg] == 0)
                    {
                        R[P]++;
                    }
                    else
                    {
                        low = M[R[P]];
                        high = B << 8;
                        R[P] = (ushort)(high | low);
                    }
                    break;
                #endregion
                #region 30-3f
                case 0x30:
                    break;
                case 0x31:
                    break;
                case 0x32:
                    break;
                case 0x33:
                    break;
                case 0x34:
                    break;
                case 0x35:
                    break;
                case 0x36:
                    break;
                case 0x37:
                    break;
                case 0x38:
                    break;
                case 0x39:
                    break;
                case 0x3A:
                    break;
                case 0x3B:
                    break;
                case 0x3C:
                    break;
                case 0x3D:
                    break;
                case 0x3E:
                    break;
                case 0x3F:
                    break;
                #endregion
                #region 40-4f
                case 0x40:
                    break;
                case 0x41:
                    break;
                case 0x42:
                    break;
                case 0x43:
                    break;
                case 0x44:
                    break;
                case 0x45:
                    break;
                case 0x46:
                    break;
                case 0x47:
                    break;
                case 0x48:
                    break;
                case 0x49:
                    break;
                case 0x4A:
                    break;
                case 0x4B:
                    break;
                case 0x4C:
                    break;
                case 0x4D:
                    break;
                case 0x4E:
                    break;
                case 0x4F:
                    break;
                #endregion
                #region 50-5f
                case 0x50:
                    break;
                case 0x51:
                    break;
                case 0x52:
                    break;
                case 0x53:
                    break;
                case 0x54:
                    break;
                case 0x55:
                    break;
                case 0x56:
                    break;
                case 0x57:
                    break;
                case 0x58:
                    break;
                case 0x59:
                    break;
                case 0x5A:
                    break;
                case 0x5B:
                    break;
                case 0x5C:
                    break;
                case 0x5D:
                    break;
                case 0x5E:
                    break;
                case 0x5F:
                    break;
                #endregion
                #region 60-6f  RLXA : Register load via X and advance
                case 0x60:
                case 0x61:
                case 0x62:
                case 0x63:
                case 0x64:
                case 0x65:
                case 0x66:
                case 0x67:
                case 0x68:
                case 0x69:
                case 0x6A:
                case 0x6B:
                case 0x6C:
                case 0x6D:
                case 0x6E:
                case 0x6F:

                    //        RLXA
                    // M(R(X)) -> R(N).1; 
                    // M(R(X) + 1) -> R(N).0;
                    // R(X)) + 2 -> R(X) 
                    //--------------- Detail -------------
                    // MRX->B, RX + 1->RX
                    // B->T; MRX->B; RX + 1->RX
                    // B, T->RN.0, RN.1
                    reg = M[R[P]] & 0xF;
                    R[P]++;
                    // MRX->B, RX + 1->RX 
                    B = (byte)(M[R[X]]);
                    R[X]++;
                    //B->T; MRX->B; RX + 1->RX
                    T = B;
                    B = (byte)(M[R[X]]);
                    R[X]++;

                    // B, T->RN.0, RN.1

                    low = B;
                    high = T << 8;
                    R[reg] = (ushort)(high | low);

                    break;
                #endregion
                #region 70-7f
                case 0x70:
                    break;
                case 0x71:
                    break;
                case 0x72:
                    break;
                case 0x73:
                    break;
                case 0x74:
                    break;
                case 0x75:
                    break;
                case 0x76:
                    break;
                case 0x77:
                    break;
                case 0x78:
                    break;
                case 0x79:
                    break;
                case 0x7A:
                    break;
                case 0x7B:
                    break;
                case 0x7C:
                    break;
                case 0x7D:
                    break;
                case 0x7E:
                    break;
                case 0x7F:
                    break;
                #endregion
                #region 80-8f  SCALL: Standard Call
                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                case 0x84:
                case 0x85:
                case 0x86:
                case 0x87:
                case 0x88:
                case 0x89:
                case 0x8A:
                case 0x8B:
                case 0x8C:
                case 0x8D:
                case 0x8E:
                case 0x8F:

                    //    RN.0, RN.1->T, B
                    //    T->MRX RX - 1->RX
                    //    B->MRX RX - 1->RX
                    //    RP.0, RP.1->T, B
                    //    B, T->RN.1, RN.0
                    //    MRN->B; RN + 1->RN
                    //    B->T; MRN->B; RN + 1->RN
                    //    B, T->RP.0, RP.1

                    //    RN.0, RN.1->T, B

                    // SCAL reg  : 688N
                    reg = M[R[P]] & 0xF;
                    T = (byte)(R[reg]);
                    B = (byte)(R[reg] >> 8);
                    R[P]++;


                    //    T->MRX RX - 1->RX
                    M[R[X]] = T;
                    R[X]--;


                    //    B->MRX RX - 1->RX
                    M[R[X]] = B;
                    R[X]--;


                    //    RP.0, RP.1->T, B
                    T = (byte)(R[P]);
                    B = (byte)(R[P] >> 8);

                    //    B, T->RN.1, RN.0
                    R[reg] = T;

                    R[reg] = (ushort)((int)(R[reg]) | (B << 8));

                    //    MRN->B; RN + 1->RN
                    B = M[R[reg]];
                    R[reg]++;

                    //    B->T; MRN->B; RN + 1->RN
                    T = B;
                    B = M[R[reg]];
                    R[reg]++;

                    //    B, T->RP.0, RP.1
                    R[P] = B;
                    //R[P] = R[P] | T << 8;
                    R[P] = (ushort)((int)(R[P]) | (T << 8));
                    break;
                #endregion
                #region 90-9f  SRET: Standard Return
                case 0x90:
                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x96:
                case 0x97:
                case 0x98:
                case 0x99:
                case 0x9A:
                case 0x9B:
                case 0x9C:
                case 0x9D:
                case 0x9E:
                case 0x9F:

                    //R(N)->R(P)
                    //M(R(X) + 1)->R(N).1
                    //M(R(X) + 2)->R(N).0
                    //R(X) + 2->R(X)
                    //---------------- Details ---------------
                    //RN.0, RN.1->T, B
                    //RX + 1->RX
                    //B, T->RP.1, RP.0

                    //MRX->B; RX + 1->RX
                    //B->T; MRX->B
                    //B, T->RN.0, RN.1
                    //----------------------------------
                    //RN.0, RN.1->T, B
                    reg = M[R[P]] & 0xF;
                    T = (byte)(R[reg]);
                    B = (byte)(R[reg] >> 8);
                    R[P]++;


                    //RX + 1->RX
                    R[X]++;


                    //B, T->RP.1, RP.0
                    high = B << 8;
                    R[P] = (ushort)(high | T);

                    //MRX->B; RX + 1->RX 
                    B = M[R[X]];
                    R[X]++;

                    //B->T; MRX->B
                    T = B;
                    B = M[R[X]];

                    //    MRN->B; RN + 1->RN
                    //B, T->RN.0, RN.1
                    high = T << 8;
                    R[reg] = (ushort)(high | B);

                    break;
                #endregion
                #region a0-af  RSXD: Register store via X and decrement
                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa3:
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xa7:
                case 0xa8:
                case 0xa9:
                case 0xaA:
                case 0xaB:
                case 0xaC:
                case 0xaD:
                case 0xaE:
                case 0xaF:

                    //        RSXD
                    // R(N).0->M(R(X));
                    // R(N).1->M(R)(X) - 1);
                    // R(X) - 2->R(X)
                    //--------------- Detail -------------
                    //RN.0, RN.1->T, B
                    //T->MRX; RX - 1->RX
                    //B->MRX; RX - 1->RX
                    reg = M[R[P]] & 0xF;

                    // RN.0, RN.1->T, B

                    low = R[reg] & 0xff;
                    high = R[reg] >> 8;
                    T = (byte)low;
                    B = (byte)high;
                    R[P]++;


                    //T->MRX; RX - 1->RX
                    M[R[X]] = T;
                    R[X]--;

                    // B->MRX; RX - 1->RX
                    M[R[X]] = B;
                    R[X]--;

                    break;
                #endregion
                #region b0-bf
                case 0xb0:
                    break;
                case 0xb1:
                    break;
                case 0xb2:
                    break;
                case 0xb3:
                    break;
                case 0xb4:
                    break;
                case 0xb5:
                    break;
                case 0xb6:
                    break;
                case 0xb7:
                    break;
                case 0xb8:
                    break;
                case 0xb9:
                    break;
                case 0xbA:
                    break;
                case 0xbB:
                    break;
                case 0xbC:
                    break;
                case 0xbD:
                    break;
                case 0xbE:
                    break;
                case 0xbF:
                    break;
                #endregion
                #region c0-cf  RLDI  :Register Load Immediately
                case 0xc0:
                case 0xc1:
                case 0xc2:
                case 0xc3:
                case 0xc4:
                case 0xc5:
                case 0xc6:
                case 0xc7:
                case 0xc8:
                case 0xc9:
                case 0xcA:
                case 0xcB:
                case 0xcC:
                case 0xcD:
                case 0xcE:
                case 0xcF:

                    //        RLDI
                    // M(R(P)) -> R(N).1; 
                    // M(R(P)) + 1 -> R(N).0; 
                    // R(P) + 2 -> R(P 
                    //--------------- Detail -------------
                    //MRP -> B; RP + 1 -> RP
                    //B -> T; MRP -> B; RP + 1 -> RP
                    //B, T -> RN.0, RN.1;RP + 1-> RP
                    //----------------------------
                    reg = M[R[P]] & 0xF;
                    R[P]++;

                    // MRP -> B; RP + 1 -> RP 
                    B = (byte)(M[R[P]]);
                    R[P]++;


                    //B -> T; MRP -> B; RP + 1 -> RP
                    T = B;
                    B = (byte)(M[R[P]]);
                    //R[P]++;  falsch

                    //B, T -> RN.0, RN.1;RP + 1-> RP

                    low = B;
                    high = T << 8;
                    R[reg] = (ushort)(high | low);
                    R[P]++;

                    break;
                #endregion
                #region d0-df
                case 0xd0:
                    break;
                case 0xd1:
                    break;
                case 0xd2:
                    break;
                case 0xd3:
                    break;
                case 0xd4:
                    break;
                case 0xd5:
                    break;
                case 0xd6:
                    break;
                case 0xd7:
                    break;
                case 0xd8:
                    break;
                case 0xd9:
                    break;
                case 0xdA:
                    break;
                case 0xdB:
                    break;
                case 0xdC:
                    break;
                case 0xdD:
                    break;
                case 0xdE:
                    break;
                case 0xdF:
                    break;
                #endregion
                #region e0-ef
                case 0xe0:
                    break;
                case 0xe1:
                    break;
                case 0xe2:
                    break;
                case 0xe3:
                    break;
                case 0xe4:
                    break;
                case 0xe5:
                    break;
                case 0xe6:
                    break;
                case 0xe7:
                    break;
                case 0xe8:
                    break;
                case 0xe9:
                    break;
                case 0xeA:
                    break;
                case 0xeB:
                    break;
                case 0xeC:
                    break;
                case 0xeD:
                    break;
                case 0xeE:
                    break;
                case 0xeF:
                    break;
                #endregion
                #region f0-ff
                case 0xf0:
                    break;
                case 0xf1:
                    break;
                case 0xf2:
                    break;
                case 0xf3:
                    break;
                case 0xf4:
                    break;
                case 0xf5:
                    break;
                case 0xf6:
                    break;
                case 0xf7:
                    break;
                case 0xf8:
                    break;
                case 0xf9:
                    break;
                case 0xfA:
                    break;
                case 0xfB:
                    break;
                case 0xfC:
                    break;
                case 0xfD:
                    break;
                case 0xfE:
                    break;
                case 0xfF:
                    break;
                #endregion
                default:
                    break;
            }


        }

        public void ShowRegister()
        {
            for (int i = 0; i < 16; i++)
            {

                ListRegister[i].Val = R[i].ToString();

                if (P == i)
                    ListRegister[i].P = "P";
                else
                    ListRegister[i].P = "";

                if (X == i)
                    ListRegister[i].X = "X";
                else
                    ListRegister[i].X = "";

            }

        }


        private void MonitorOnOff()
        {

            if (CheckBoxDmaOut.IsChecked == true)
            {
                MyMonitor = null;
                MyMonitor = new Monitor();

                MyMonitor.Show();
                Monitor.On = true;
                LabelDmaOut.Background = Brushes.LightGreen;
            }
            else
            {
                MyMonitor.Close();
                MyMonitor = null;
                Monitor.On = false;
                LabelDmaOut.Background = Brushes.White;
            }
        }


 






    }
}
