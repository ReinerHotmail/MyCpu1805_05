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

            string t1 = "";
            string v = "";
            string t2 = "";

            bool found = false;

            for (int i = 0; i < signs.Length; i++)
            {
                string[] s = term.Split(signs[i]);
                if (s.Length == 2)
                {
                    t1 = s[0];
                    v = signs[i];
                    t2 = s[1];
                    found = true;
                    break;
                }
            }

            //if (found.Term1[1] == 'P')
            //    iLeft = R[P];
            //else if (found.Term1[1] == 'X')
            //    iLeft = R[X];
            //else if (CStat.IsHex(found.Term1[1].ToString()))
            //    iLeft = R[Convert.ToInt32(found.Term1[1].ToString())];



            if (!found)
                return ("", "", "");

            bool t1Ok = false;
            bool t2Ok = false;


            if (t1.Length == 2 && t1.StartsWith("R"))
            {
                if (t1[1] == 'P')
                    t1Ok = true;
                else if (t1[1] == 'X')
                    t1Ok = true;
                else if (CStat.IsHex(t1[1].ToString()))
                    t1Ok = true;
            }


            if (t1.Length == 4 && CStat.IsHex(t1))
                t1Ok = true;


            if (t2.Length == 2 && t2.StartsWith("R"))
            {
                if (t2[1] == 'P')
                    t2Ok = true;
                else if (t2[1] == 'X')
                    t2Ok = true;
                else if (CStat.IsHex(t2[1].ToString()))
                    t2Ok = true;
            }

            if (t2.Length == 4 && CStat.IsHex(t2))
                t2Ok = true;


            if (t1Ok && t2Ok)
                return (t1, v, t2);
            else
                return ("", "", "");

        }


    }
}
