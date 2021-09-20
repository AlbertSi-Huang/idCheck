using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace ExeReadPassport630
{
    public class CountryCodyDiction
    {
        static string[] _txtContents = null;
        public static string GetCountryName(string strCode)
        {
            string strRet = "中国";
            if(_txtContents == null)
            {
                _txtContents = File.ReadAllLines(@"PassportCountryDicTion.txt");
            }
            Trace.WriteLine("country Code = " + strCode);
            foreach(string str in _txtContents)
            {
                string[] ss = str.Split(',');
                if (ss.Length != 4) continue;
                if(ss[3].CompareTo(strCode) == 0 || ss[2].CompareTo(strCode) == 0)
                {
                    strRet = ss[1];
                    break;
                }
            }
            return strRet;
        }
    }
}
