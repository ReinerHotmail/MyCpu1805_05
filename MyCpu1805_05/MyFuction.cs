using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace MyCpu1805_05
{
    public partial class MainWindow
    {





        /// <summary>
        /// eine Zeile zum LogFile hinzufügen
        /// </summary>
        /// <param name="LineTyp">enum Logline</param>
        /// <param name="runTimeAtEndOrText">bei LoLine.End: Laufzeit - sonst allg.Text</param>
        /// <param name="newLogFile">löschen und erzeugen neues LogFile</param>
        private void LogFileAddLine(LogLine LineTyp, string runTimeAtEndOrText, string logItems, bool newLogFile)
        {

            if (newLogFile || !File.Exists(RcaFile.Path + "\\LogFile.txt"))
            {
                if (File.Exists(RcaFile.Path + "\\LogFile.txt"))
                    File.Delete(RcaFile.Path + "\\LogFile.txt");

                var logFile = File.Create(RcaFile.Path + "\\LogFile.txt");
                logFile.Close();
            }



            string dt = "";


            string progName = "";
            if (LineTyp != LogLine.Run && RcaFile.Long.Length > 4)
                progName = RcaFile.Name;

            switch (LineTyp)
            {
                case LogLine.Start:

                    QueueLogFile.Clear();

                    #region mit jedem START, LogFile lesen und in Queue füllen
                    string[] buffer = File.ReadAllLines(RcaFile.Path + "\\LogFile.txt");
                    int len = buffer.Length;
                    if (len > LogFileMaxLines)
                        len = LogFileMaxLines;

                    for (int i = 0; i < len; i += 2)
                    {
                        if (buffer[i].Length > 0)
                        {
                            //ein Eintrag:             'Info mit Datum' ';' 'PXD0123456789ABCDEF'
                            QueueLogFile.Enqueue(buffer[i] + ";" + buffer[i + 1]);
                        }

                    }
                    #endregion
                    dt = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff tt");
                    QueueLogFile.Enqueue(LogFileLineCounter.ToString("D4") + "  " + dt + " - START     " + progName + ";     " + "-");// logItems);
                    break;
                case LogLine.Run:
                    #region mit jedem RUN (Breakpoints oder Logpoints), die programmierten CPU-Daten ermitteln

                    string outline = "P=" + P.ToString("X1") + "   X=" + X.ToString("X1");

                    for (int i = 0; i < logItems.Length; i++)
                    {
                        switch (logItems[i])
                        {
                            case 'P':
                                outline += "  " + "R[P]=" + R[P].ToString("X4");
                                break;
                            case 'X':
                                outline += "  " + "R[X]=" + R[X].ToString("X4");
                                break;
                            case 'd':
                                outline += "  " + "D=" + D.ToString("X1");
                                break;
                            case 'Q':
                                string q = "0";
                                if (Q)
                                    q = "1";
                                outline += "  " + "Q=" + q;
                                break;
                            case '0':
                                outline += "  " + "R[0]=" + R[0].ToString("X4");
                                break;
                            case '1':
                                outline += "  " + "R[1]=" + R[1].ToString("X4");
                                break;
                            case '2':
                                outline += "  " + "R[2]=" + R[2].ToString("X4");
                                break;
                            case '3':
                                outline += "  " + "R[3]=" + R[3].ToString("X4");
                                break;
                            case '4':
                                outline += "  " + "R[4]=" + R[4].ToString("X4");
                                break;
                            case '5':
                                outline += "  " + "R[5]=" + R[5].ToString("X4");
                                break;
                            case '6':
                                outline += "  " + "R[6]=" + R[6].ToString("X4");
                                break;
                            case '7':
                                outline += "  " + "R[7]=" + R[7].ToString("X4");
                                break;
                            case '8':
                                outline += "  " + "R[8]=" + R[8].ToString("X4");
                                break;
                            case '9':
                                outline += "  " + "R[9]=" + R[9].ToString("X4");
                                break;
                            case 'A':
                                outline += "  " + "R[A]=" + R[10].ToString("X4");
                                break;
                            case 'B':
                                outline += "  " + "R[B]=" + R[11].ToString("X4");
                                break;
                            case 'C':
                                outline += "  " + "R[C]=" + R[12].ToString("X4");
                                break;
                            case 'D':
                                outline += "  " + "R[D]=" + R[13].ToString("X4");
                                break;
                            case 'E':
                                outline += "  " + "R[E]=" + R[14].ToString("X4");
                                break;
                            case 'F':
                                outline += "  " + "R[F]=" + R[15].ToString("X4");
                                break;
                            case 'I':
                                outline += "  " + "INP0=" + INP[0].ToString("X2") + " INP1=" + INP[1].ToString("X2") + " INP2=" + INP[2].ToString("X2") + " INP3=" + INP[3].ToString("X2") + " INP4=" + INP[4].ToString("X2") + " INP5=" + INP[5].ToString("X2") + " INP6=" + INP[6].ToString("X2") + " INP7=" + INP[7].ToString("X2");
                                break;
                            case 'O':
                                outline += "  " + "OUT0=" + OUT[0].ToString("X2") + " OUT1=" + OUT[1].ToString("X2") + " OUT2=" + OUT[2].ToString("X2") + " OUT3=" + OUT[3].ToString("X2") + " OUT4=" + OUT[4].ToString("X2") + " OUT5=" + OUT[5].ToString("X2") + " OUT6=" + OUT[6].ToString("X2") + " OUT7=" + OUT[7].ToString("X2");
                                break;
                            case 'e':
                                string ef1 = "0";
                                if (EF1)
                                    ef1 = "1";
                                string ef2 = "0";
                                if (EF2)
                                    ef2 = "1";
                                string ef3 = "0";
                                if (EF3)
                                    ef3 = "1";
                                string ef4 = "0";
                                if (EF4)
                                    ef4 = "1";
                                outline += "  " + "EF1=" + ef1 + " EF2=" + ef2 + " EF3=" + ef3 + " EF4=" + ef4;
                                break;
                            default:
                                break;
                        }
                    }



                    #endregion
                    dt = DateTime.Now.ToString("              mm:ss.fff tt");
                    QueueLogFile.Enqueue(LogFileLineCounter.ToString("D4") + "  " + dt + " - " + runTimeAtEndOrText + ";     " + outline);
                    break;
                case LogLine.Stop:
                    dt = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff tt");
                    QueueLogFile.Enqueue(LogFileLineCounter.ToString("D4") + "  " + dt + " - STOP     " + progName + ";     " + "-");// logItems);
                    break;
                case LogLine.End:
                    dt = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff tt");
                    QueueLogFile.Enqueue(LogFileLineCounter.ToString("D4") + "  " + dt + " - END  " + runTimeAtEndOrText + ";     " + "-" + Environment.NewLine);
                    #region mit jedem END, Queue in LogFile schreiben
                    int max = QueueLogFile.Count;
                    string[] bufferLine = new string[max * 2];
                    int bufNum = 0;

                    for (int i = 0; i < max; i++)
                    {
                        var queueItem = QueueLogFile.Dequeue();
                        string[] two = queueItem.Split(';');
                        bufferLine[bufNum] = two[0];
                        bufferLine[bufNum + 1] = two[1];
                        bufNum += 2;
                    }
                    File.WriteAllLines(RcaFile.Path + "\\LogFile.txt", bufferLine);
                    #endregion
                    break;
                case LogLine.Break:
                    #region mit jedem Break, Queue in LogFile schreiben
                    max = QueueLogFile.Count;
                    bufferLine = new string[max * 2 + 2];
                    bufNum = 0;

                    for (int i = 0; i < max; i++)
                    {
                        var queueItem = QueueLogFile.Dequeue();
                        string[] two = queueItem.Split(';');
                        bufferLine[bufNum] = two[0];
                        bufferLine[bufNum + 1] = two[1];
             
                        bufNum += 2;

                        if (i == max - 1)//Break-Eintrag
                        {
                            bufferLine[bufNum + 1] = "    -- BREAK  --";
                            bufferLine[bufNum] = "  ";
                            bufNum += 2;
                        }

                    }
                    File.WriteAllLines(RcaFile.Path + "\\LogFile.txt", bufferLine);


                    #endregion
                    break;
                default:
                    break;
            }

            if (QueueLogFile.Count > LogFileMaxLines)
                QueueLogFile.Dequeue();


            LogFileLineCounter++;


            if (LogFileLineCounter > 9999)
                LogFileLineCounter = 0;


        }

    }
}
