using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class TS_Helper
    {
        public static string GetNationCode(string strNation)
        {
            if(strNation == null || strNation.CompareTo("") == 0)
            {
                return "57";
            }
            string strCode = "";
            if(strNation.CompareTo("汉") == 0 || strNation.CompareTo("汉族") == 0)
            {
                strCode = "01";
            }
            else if (strNation.CompareTo("蒙古") == 0 || strNation.CompareTo("蒙古族") == 0)
            {
                return "02";
            }
            else if (strNation.CompareTo("回") == 0 || strNation.CompareTo("回族") == 0)
            {
                return "03";
            }
            else if (strNation.CompareTo("藏") == 0 || strNation.CompareTo("藏族") == 0)
            {
                return "04";
            }
            else if (strNation.CompareTo("维吾尔") == 0 || strNation.CompareTo("维吾尔族") == 0)
            {
                return "05";
            }
            else if (strNation.CompareTo("苗") == 0 || strNation.CompareTo("苗族") == 0)
            {
                return "06";
            }
            else if (strNation.CompareTo("彝") == 0 || strNation.CompareTo("彝族") == 0)
            {
                return "07";
            }
            else if (strNation.CompareTo("壮") == 0 || strNation.CompareTo("壮族") == 0)
            {
                return "08";
            }
            else if (strNation.CompareTo("布依") == 0 || strNation.CompareTo("布依族") == 0)
            {
                return "09";
            }
            else if (strNation.CompareTo("朝鲜") == 0 || strNation.CompareTo("朝鲜族") == 0)
            {
                return "10";
            }
            else if (strNation.CompareTo("满") == 0 || strNation.CompareTo("满族") == 0)
            {
                return "11";
            }
            else if (strNation.CompareTo("侗") == 0 || strNation.CompareTo("侗族") == 0)
            {
                return "12";
            }
            else if (strNation.CompareTo("瑶") == 0 || strNation.CompareTo("瑶族") == 0)
            {
                return "13";
            }
            else if (strNation.CompareTo("白") == 0 || strNation.CompareTo("白族") == 0)
            {
                return "14";
            }
            else if (strNation.CompareTo("土家") == 0 || strNation.CompareTo("土家族") == 0)
            {
                return "15";
            }
            else if (strNation.CompareTo("哈尼") == 0 || strNation.CompareTo("哈尼族") == 0)
            {
                return "16";
            }
            else if (strNation.CompareTo("哈萨克") == 0 || strNation.CompareTo("哈萨克族") == 0)
            {
                return "17";
            }
            else if (strNation.CompareTo("傣") == 0 || strNation.CompareTo("傣族") == 0)
            {
                return "18";
            }
            else if (strNation.CompareTo("黎") == 0 || strNation.CompareTo("黎族") == 0)
            {
                return "19";
            }
            else if (strNation.CompareTo("傈僳") == 0 || strNation.CompareTo("傈傈族") == 0)
            {
                return "20";
            }
            else if (strNation.CompareTo("佤") == 0 || strNation.CompareTo("佤族") == 0)
            {
                return "21";
            }
            else if (strNation.CompareTo("畲") == 0 || strNation.CompareTo("畲族") == 0)
            {
                return "22";
            }
            else if (strNation.CompareTo("高山") == 0 || strNation.CompareTo("高山族") == 0)
            {
                return "23";
            }
            else if (strNation.CompareTo("拉祜") == 0 || strNation.CompareTo("拉祜族") == 0)
            {
                return "24";
            }
            else if (strNation.CompareTo("水") == 0 || strNation.CompareTo("水族") == 0)
            {
                return "25";
            }
            else if (strNation.CompareTo("东乡") == 0 || strNation.CompareTo("东乡族") == 0)
            {
                return "26";
            }
            else if (strNation.CompareTo("纳西") == 0 || strNation.CompareTo("纳西族") == 0)
            {
                return "27";
            }
            else if (strNation.CompareTo("景颇") == 0 || strNation.CompareTo("景颇族") == 0)
            {
                return "28";
            }
            else if (strNation.CompareTo("柯尔克孜") == 0 || strNation.CompareTo("柯尔克孜族") == 0)
            {
                return "29";
            }
            else if (strNation.CompareTo("土") == 0 || strNation.CompareTo("土族") == 0)
            {
                return "30";
            }
            else if (strNation.CompareTo("达斡尔") == 0 || strNation.CompareTo("达斡尔族") == 0)
            {
                return "31";
            }
            else if (strNation.CompareTo("仫佬") == 0 || strNation.CompareTo("仫佬族") == 0)
            {
                return "32";
            }
            else if (strNation.CompareTo("羌") == 0 || strNation.CompareTo("羌族") == 0)
            {
                return "33";
            }
            else if (strNation.CompareTo("布朗") == 0 || strNation.CompareTo("布朗族") == 0)
            {
                return "34";
            }
            else if (strNation.CompareTo("毛南") == 0 || strNation.CompareTo("毛南族") == 0)
            {
                return "35";
            }
            else if (strNation.CompareTo("仡佬") == 0 || strNation.CompareTo("仡佬族") == 0)
            {
                return "36";
            }
            else if (strNation.CompareTo("柯尔克孜") == 0 || strNation.CompareTo("柯尔克族") == 0)
            {
                return "37";
            }
            else if (strNation.CompareTo("锡伯") == 0 || strNation.CompareTo("锡伯族") == 0)
            {
                return "38";
            }
            else if (strNation.CompareTo("阿昌") == 0 || strNation.CompareTo("阿昌族") == 0)
            {
                return "39";
            }
            else if (strNation.CompareTo("普米") == 0 || strNation.CompareTo("普米族") == 0)
            {
                return "40";
            }
            else if (strNation.CompareTo("塔吉克") == 0 || strNation.CompareTo("塔吉克族") == 0)
            {
                return "41";
            }
            else if (strNation.CompareTo("怒") == 0 || strNation.CompareTo("怒族") == 0)
            {
                return "42";
            }
            else if (strNation.CompareTo("乌孜别克") == 0 || strNation.CompareTo("乌孜别克族") == 0)
            {
                return "43";
            }
            else if (strNation.CompareTo("俄罗斯") == 0 || strNation.CompareTo("俄罗斯族") == 0)
            {
                return "44";
            }
            else if (strNation.CompareTo("鄂温克") == 0 || strNation.CompareTo("鄂温克族") == 0)
            {
                return "45";
            }
            else if (strNation.CompareTo("德昂") == 0 || strNation.CompareTo("德昂族") == 0)
            {
                return "46";
            }
            else if (strNation.CompareTo("保安") == 0 || strNation.CompareTo("保安族") == 0)
            {
                return "47";
            }
            else if (strNation.CompareTo("裕固") == 0 || strNation.CompareTo("裕固族") == 0)
            {
                return "48";
            }
            else if (strNation.CompareTo("京") == 0 || strNation.CompareTo("京族") == 0)
            {
                return "49";
            }
            else if (strNation.CompareTo("塔塔尔") == 0 || strNation.CompareTo("塔塔尔族") == 0)
            {
                return "50";
            }
            else if (strNation.CompareTo("独龙") == 0 || strNation.CompareTo("独龙族") == 0)
            {
                return "51";
            }
            else if (strNation.CompareTo("鄂伦春") == 0 || strNation.CompareTo("鄂伦春族") == 0)
            {
                return "52";
            }
            else if (strNation.CompareTo("赫哲") == 0 || strNation.CompareTo("赫哲族") == 0)
            {
                return "53";
            }
            else if (strNation.CompareTo("门巴") == 0 || strNation.CompareTo("门巴族") == 0)
            {
                return "54";
            }
            else if (strNation.CompareTo("珞巴") == 0 || strNation.CompareTo("珞巴族") == 0)
            {
                return "55";
            }
            else if (strNation.CompareTo("基诺") == 0 || strNation.CompareTo("基诺族") == 0)
            {
                return "56";
            }
            else if (strNation.CompareTo("其他") == 0)
            {
                return "57";
            }
            else if (strNation.CompareTo("外国人入籍") == 0)
            {
                return "58";
            }

            return strCode;
        }
    }
}
