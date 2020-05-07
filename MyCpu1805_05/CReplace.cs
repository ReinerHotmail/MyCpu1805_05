using System;
using System.Collections.Generic;
using System.Text;

namespace MyCpu1805_05
{
    class CReplace
    {

        public string NickName { get; set; }
        public string Name { get; set; }

        public CReplace(string nickName, string name)
        {
            NickName = nickName;
            Name = name;


        }
        public override string ToString()
        {
            return NickName + "=" + Name;
        }
    }
}
