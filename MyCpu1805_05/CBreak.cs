using System;
using System.Collections.Generic;
using System.Text;

namespace MyCpu1805_05
{
    public class CBreak
    {


        public string Typ { get; set; }
        public int LineNum { get; set; }
        public int Adr { get; set; }
        public string Term1 { get; set; }

        public string Comp { get; set; }
        public string Term2 { get; set; }

        public string LogItems { get; set; }


        public string Condition
        {
            get { return Term1 + Comp + Term2; }

        }
        public string AdrHex
        {
            get { return Adr.ToString("X4"); }

        }

        public CBreak(string typ, int lineNum, int adr, string term1, string comp, string term2, string logItems)
        {
            Typ = typ;
            LineNum = lineNum;
            Adr = adr;
            Term1 = term1;
            Comp = comp;
            Term2 = term2;
            LogItems = (logItems.Replace("\n", "").Replace("\r", "")).Trim();
        }



        public static (string term1, string comp, string term2) CheckCondition(string term)
        {

            string sign = "== >= <= > <";

            string[] signs = sign.Split();

            string termLinks = "";
            string v = "";
            string termRechts = "";

            bool found = false;

            for (int i = 0; i < signs.Length; i++)
            {
                string[] s = term.Split(signs[i]);
                if (s.Length == 2)
                {
                    termLinks = s[0];
                    v = signs[i];
                    termRechts = s[1];
                    found = true;
                    break;
                }
            }

//          mögliche Einträge für Register:			        möglicher Eintrag: 
//          R1..RF  RP RX                                   Akku D

//          mögliche Einträge für Adressen:
//          '4stellig-Hex' zB: A0F7

//          Beispieleinträge für Register und Hexwert       D und Hexwert

//          R5 > A12F                                       D > A12F
//          R5 >= A12F                                      D >= A12F
//          R5 < A12F                                       D < A12F
//          R5 <= A12F                                      D <= A12F
//          R5 == A12F                                      D == A12F

//          Beispieleinträge für  Register und Register

//          R5 > R6
//          R5 >= R6
//          R5 < R6
//          R5 <= R6
//          R5 == R6


            if (!found)
                return ("", "", "");

            bool term1Ok = false;
            bool term2Ok = false;


            if (termLinks.Length == 2 && termLinks.StartsWith("R"))
            {
                if (termLinks[1] == 'P')        // RP
                    term1Ok = true;
                else if (termLinks[1] == 'X')   // RX
                    term1Ok = true;
                else if (CStat.IsHex(termLinks[1].ToString())) //Rn
                    term1Ok = true;
            }


            if (termLinks.Length == 4 && CStat.IsHex(termLinks))
                term1Ok = true;

            if (termLinks =="D")
                term1Ok = true;


            if (termRechts.Length == 2 && termRechts.StartsWith("R"))
            {
                if (termRechts[1] == 'P')        // RP
                    term2Ok = true;
                else if (termRechts[1] == 'X')   // RX
                    term2Ok = true;
                else if (CStat.IsHex(termRechts[1].ToString())) //Rn
                    term2Ok = true;
            }

            if (termRechts.Length == 4 && CStat.IsHex(termRechts))
                term2Ok = true;




            if (term1Ok && term2Ok)
                return (termLinks, v, termRechts);
            else
                return ("", "", "");

        }


    }
}
