using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ExeReadPassport630
{
    public class SystemConfigIni
    {
        private string _fileName;

        public SystemConfigIni(string fileName)
        {
            _fileName = fileName;
        }

        public string GetConfigItem(string lpAppName, string lpKeyName, string strDefault)
        {
            StringBuilder sb = new StringBuilder(256);
            GetPrivateProfileString(lpAppName, lpKeyName, strDefault, sb, 256, _fileName);
            return sb.ToString();
        }

        public bool SetConfigItem(string lpAppName, string lpKeyName, string strValue)
        {
            bool bSetRet = false;
            bSetRet = WritePrivateProfileString(lpAppName, lpKeyName, strValue, _fileName);
            return bSetRet;
        }

        public int GetConfigItem(string lpAppName, string lpKeyName, int nDefalue)
        {
            int nRet = -1;
            nRet = GetPrivateProfileInt(lpAppName, lpKeyName, nDefalue, _fileName);
            return nRet;
        }
        public string InitItem(string lpAppName, string lpKeyName, string strDefalue)
        {
            string strTmp = GetConfigItem(lpAppName, lpKeyName, strDefalue);
            SetConfigItem(lpAppName, lpKeyName, strTmp);
            return strTmp;
        }

        public int InitItem(string lpAppName, string lpKeyName, int nDefalue)
        {
            int nTmp = GetConfigItem(lpAppName, lpKeyName, nDefalue);
            SetConfigItem(lpAppName, lpKeyName, nTmp.ToString());
            return nTmp;
        }

        #region 引用系统dll

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileInt(
         string lpAppName,// 指向包含 Section 名称的字符串地址
              string lpKeyName,// 指向包含 Key 名称的字符串地址
              int nDefault,// 如果 Key 值没有找到，则返回缺省的值是多少
              string lpFileName
         );

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(
         string lpAppName,// 指向包含 Section 名称的字符串地址
              string lpKeyName,// 指向包含 Key 名称的字符串地址
              string lpDefault,// 如果 Key 值没有找到，则返回缺省的字符串的地址
              StringBuilder lpReturnedString,// 返回字符串的缓冲区地址
              int nSize,// 缓冲区的长度
              string lpFileName
         );

        [DllImport("kernel32")]
        public static extern bool WritePrivateProfileString(
         string lpAppName,// 指向包含 Section 名称的字符串地址
              string lpKeyName,// 指向包含 Key 名称的字符串地址
              string lpString,// 要写的字符串地址
              string lpFileName
         );

        #endregion 
    }
}
