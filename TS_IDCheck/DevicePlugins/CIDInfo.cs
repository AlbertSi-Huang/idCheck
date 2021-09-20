using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS_IDCheck
{
    public class CIDInfo
    {
        //身份信息类，以下方法可以获取string类型的相应用户信息
        
        private int len = 0;
        private int index = 0;

        public string GetGBKStr(byte[] srData)
        {
            //获取GBK格式的string
            string str = Encoding.Unicode.GetString(srData);
            string newstr = str.Replace("\0", "");
            newstr = newstr.Trim();//去掉空格
            return newstr;
        }

        public string GetName(byte[] srData)
        {
            byte[] desData = new byte[30];
            len = 30;
            Array.Copy(srData, index, desData, 0, len);
            string str = GetGBKStr(desData);
            return str;
        }

        public string GetSex(byte[] srData)
        {
            byte[] desData = new byte[2];
            len = 2;
            index = 30;
            Array.Copy(srData, index, desData, 0, len);
            string str = GetGBKStr(desData);
            string[] sex = new string[2];
            sex[0] = "男";
            sex[1] = "女";
            int intType = Int32.Parse(str);

            return sex[intType - 1];
        }

        public string GetFolk(byte[] srData)
        {
            byte[] desData = new byte[4];
            len = 4;
            index = 32;
            Array.Copy(srData, index, desData, 0, len);
            string str = GetGBKStr(desData);
            int intType = Int32.Parse(str);
            string[] FOLK = new string[57];
            FOLK[0] = "";
            FOLK[1] = "汉";			//注意 民族编码有误 请查国标，这里只是例子
            FOLK[2] = "蒙古";
            FOLK[3] = "回";//注意 民族编码有误 请查国标，这里只是例子
            FOLK[4] = "藏";//注意 民族编码有误 请查国标，这里只是例子
            FOLK[5] = "维吾尔";
            FOLK[6] = "苗";
            FOLK[7] = "彝";
            FOLK[8] = "壮";
            FOLK[9] = "满";
            FOLK[10] = "侗";
            FOLK[11] = "瑶";
            FOLK[12] = "白";
            FOLK[13] = "土家";//注意 民族编码有误 请查国标，这里只是例子
            FOLK[14] = "哈尼";
            FOLK[15] = "哈萨克";
            FOLK[16] = "傣";//注意 民族编码有误 请查国标，这里只是例子
            FOLK[17] = "佤";
            FOLK[18] = "畲";
            FOLK[19] = "高山";//注意 民族编码有误 请查国标，这里只是例子
            FOLK[20] = "拉祜";
            FOLK[21] = "水";
            FOLK[22] = "东乡";//注意 民族编码有误 请查国标，这里只是例子
            FOLK[23] = "纳西";
            FOLK[24] = "景颇";
            FOLK[25] = "达斡尔";
            FOLK[26] = "仫佬";
            FOLK[27] = "羌";
            FOLK[28] = "布朗";
            FOLK[29] = "撒拉";
            FOLK[30] = "毛南";
            FOLK[31] = "仡佬";
            FOLK[32] = "锡伯";
            FOLK[33] = "塔吉克";
            FOLK[34] = "怒";
            FOLK[35] = "乌孜别克";
            FOLK[36] = "俄罗斯";
            FOLK[37] = "鄂温克";
            FOLK[38] = "德昂";
            FOLK[39] = "保安";
            FOLK[40] = "裕固";
            FOLK[41] = "独龙";
            FOLK[42] = "鄂伦春";
            FOLK[43] = "赫哲";
            FOLK[44] = "门巴";
            FOLK[45] = "珞巴";
            FOLK[46] = "基诺";
            FOLK[47] = "朝鲜";
            FOLK[48] = "傈僳";
            FOLK[49] = "普米";
            FOLK[50] = "塔塔尔";
            FOLK[51] = "布依";
            FOLK[52] = "黎";
            FOLK[53] = "柯尔克孜";
            FOLK[54] = "阿昌";
            FOLK[55] = "京";
            FOLK[56] = "土";
            return FOLK[intType];
        }

        public string GetBirth(byte[] srData)
        {
            byte[] desData = new byte[16];
            len = 16;
            index = 36;
            Array.Copy(srData, index, desData, 0, len);
            string str = GetGBKStr(desData);
            return str;
        }

        public string GetAddr(byte[] srData)
        {
            byte[] desData = new byte[70];
            len = 70;
            index = 52;
            Array.Copy(srData, index, desData, 0, len);
            string str = GetGBKStr(desData);
            return str;
        }

        public string GetIDNum(byte[] srData)
        {
            byte[] desData = new byte[36];
            len = 36;
            index = 122;
            Array.Copy(srData, index, desData, 0, len);
            string str = GetGBKStr(desData);
            return str;
        }

        public string GetDep(byte[] srData)
        {
            byte[] desData = new byte[30];
            len = 30;
            index = 158;
            Array.Copy(srData, index, desData, 0, len);
            string str = GetGBKStr(desData);
            return str;
        }

        public string GetBegin(byte[] srData)
        {
            //身份证有效期限的起始日期
            byte[] desData = new byte[16];
            len = 16;
            index = 188;
            Array.Copy(srData, index, desData, 0, len);
            string str = GetGBKStr(desData);
            return str;
        }

        public string GetEnd(byte[] srData)
        {
            //身份证有效期限的截止日期
            byte[] desData = new byte[16];
            len = 16;
            index = 204;
            Array.Copy(srData, index, desData, 0, len);
            string str = GetGBKStr(desData);
            return str;
        }
    }
}
