using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Common
{

    public static class TimeAbout
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTime
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMiliseconds;
        }

        #region 引用api设置系统时间
        [DllImport("Kernel32.dll")]
        public static extern bool SetLocalTime(ref SystemTime sysTime);

        public static bool SetLocalTimeByStr(string timestr)
        {
            bool flag = false;
            SystemTime sysTime = new SystemTime();
            DateTime dt = Convert.ToDateTime(timestr);
            sysTime.wYear = Convert.ToUInt16(dt.Year);
            sysTime.wMonth = Convert.ToUInt16(dt.Month);
            sysTime.wDay = Convert.ToUInt16(dt.Day);
            sysTime.wHour = Convert.ToUInt16(dt.Hour);
            sysTime.wMinute = Convert.ToUInt16(dt.Minute);
            sysTime.wSecond = Convert.ToUInt16(dt.Second);
            try
            {
                flag = SetLocalTime(ref sysTime);
            }
            catch (Exception e)
            {
                Console.WriteLine("SetSystemDateTime函数执行异常" + e.Message);
            }
            return flag;
        }

        #endregion
        
        public static double SpanTick(DateTime dtLast)
        {
            TimeSpan ts = DateTime.Now.Subtract(dtLast);
            if (ts.Days > 1 || ts.Days < -1)
            {
                return 0;
            }
            return Convert.ToDouble(ts.TotalMilliseconds);
        }

        public static string GetNationCode(string strNation)
        {
            if (strNation.CompareTo("汉") == 0 || strNation.CompareTo("汉族") == 0)
            {
                return "01";
            }
            else if (strNation.CompareTo("蒙古") == 0)
            {
                return "02";
            }
            else if (strNation.CompareTo("回") == 0)
            {
                return "03";
            }
            else if (strNation.CompareTo("藏") == 0)
            {
                return "04";
            }
            else if (strNation.CompareTo("维吾尔") == 0 || strNation.IndexOf("维") != -1)
            {
                return "05";
            }
            else if (strNation.CompareTo("苗") == 0)
            {
                return "06";
            }
            else if (strNation.CompareTo("彝") == 0)
            {
                return "07";
            }
            else if (strNation.CompareTo("壮") == 0)
            {
                return "08";
            }
            else if (strNation.CompareTo("布依") == 0)
            {
                return "09";
            }
            else if (strNation.CompareTo("朝鲜") == 0)
            {
                return "10";
            }
            else if (strNation.CompareTo("满") == 0)
            {
                return "11";
            }
            else if (strNation.CompareTo("侗") == 0)
            {
                return "12";
            }
            else if (strNation.CompareTo("瑶") == 0)
            {
                return "13";
            }
            else if (strNation.CompareTo("白") == 0)
            {
                return "14";
            }
            else if (strNation.CompareTo("土家") == 0)
            {
                return "15";
            }
            else if (strNation.CompareTo("哈尼") == 0)
            {
                return "16";
            }
            else if (strNation.CompareTo("哈萨克") == 0)
            {
                return "17";
            }
            else if (strNation.CompareTo("傣") == 0)
            {
                return "18";
            }
            else if (strNation.CompareTo("黎") == 0)
            {
                return "19";
            }
            else if (strNation.CompareTo("傈僳") == 0)
            {
                return "20";
            }
            else if (strNation.CompareTo("佤") == 0)
            {
                return "21";
            }
            else if (strNation.CompareTo("畲") == 0)
            {
                return "22";
            }
            else if (strNation.CompareTo("高山") == 0)
            {
                return "23";
            }
            else if (strNation.CompareTo("拉祜") == 0)
            {
                return "24";
            }
            else if (strNation.CompareTo("水") == 0)
            {
                return "25";
            }
            else if (strNation.CompareTo("东乡") == 0)
            {
                return "26";
            }
            else if (strNation.CompareTo("纳西") == 0)
            {
                return "27";
            }
            else if (strNation.CompareTo("景颇") == 0)
            {
                return "28";
            }
            else if (strNation.CompareTo("柯尔克孜") == 0)
            {
                return "29";
            }
            else if (strNation.CompareTo("土") == 0)
            {
                return "30";
            }
            else if (strNation.CompareTo("达斡尔") == 0)
            {
                return "31";
            }
            else if (strNation.CompareTo("仫佬") == 0)
            {
                return "32";
            }
            else if (strNation.CompareTo("羌") == 0)
            {
                return "33";
            }
            else if (strNation.CompareTo("布朗") == 0)
            {
                return "34";
            }
            else if (strNation.CompareTo("撒拉") == 0)
            {
                return "35";
            }
            else if (strNation.CompareTo("毛南") == 0)
            {
                return "36";
            }
            else if (strNation.CompareTo("仡佬") == 0)
            {
                return "37";
            }
            else if (strNation.CompareTo("锡伯") == 0)
            {
                return "38";
            }
            else if (strNation.CompareTo("阿昌") == 0)
            {
                return "39";
            }
            else if (strNation.CompareTo("普米") == 0)
            {
                return "40";
            }
            else if (strNation.CompareTo("塔吉克") == 0)
            {
                return "41";
            }
            else if (strNation.CompareTo("怒") == 0)
            {
                return "42";
            }
            else if (strNation.CompareTo("乌孜别克") == 0)
            {
                return "43";
            }
            else if (strNation.CompareTo("俄罗斯") == 0)
            {
                return "44";
            }
            else if (strNation.CompareTo("鄂温克") == 0)
            {
                return "45";
            }
            else if (strNation.CompareTo("德昂") == 0)
            {
                return "46";
            }
            else if (strNation.CompareTo("保安") == 0)
            {
                return "47";
            }
            else if (strNation.CompareTo("裕固") == 0)
            {
                return "48";
            }
            else if (strNation.CompareTo("京") == 0)
            {
                return "49";
            }
            else if (strNation.CompareTo("塔塔尔") == 0)
            {
                return "50";
            }
            else if (strNation.CompareTo("独龙") == 0)
            {
                return "51";
            }
            else if (strNation.CompareTo("鄂伦春") == 0)
            {
                return "52";
            }
            else if (strNation.CompareTo("赫哲") == 0)
            {
                return "53";
            }
            else if (strNation.CompareTo("门巴") == 0)
            {
                return "54";
            }
            else if (strNation.CompareTo("珞巴") == 0)
            {
                return "55";
            }
            else if (strNation.CompareTo("基诺") == 0)
            {
                return "56";
            }
            else if (strNation.CompareTo("其他") == 0)
            {
                return "58";
            }
            else if (strNation.CompareTo("外国人入籍") == 0)
            {
                return "57";
            }
            return "58";
        }
    }
}