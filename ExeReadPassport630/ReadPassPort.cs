using Common;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ExeReadPassport630
{
    public class ReadPassPort
    {
        /// <summary>
        /// 循环读卡线程
        /// </summary>
        private Thread m_ThreadHandle = null;
        private int nDGGroup = 0;//表示要读取的DG，对于普通设备调用传0
        private bool bRecogVIZ = true;//true表示识别视读区相关类容，false则相反
        private int nSaveImageType = 0;//保存图片类型：1表示保存白光全图,2表示保存红外全图,4表示保存紫外全图,8表示保存版面头像,16 表示保存芯片的头像
        private int m_nOpenPort = 0;
        private string GetUserId()
        {
            string strFilePath = Assembly.GetExecutingAssembly().Location;
            int nPos = strFilePath.LastIndexOf('\\');
            strFilePath = strFilePath.Substring(0, nPos);

            string sUserID = string.Empty;

            string sPath = strFilePath + @"\UserId.txt";
            Trace.WriteLine("useridPath = " + sPath);
            using (StreamReader sr = new StreamReader(sPath, Encoding.Default))
            {
                String line = sr.ReadLine();//读取第一行
                sUserID = line.Substring(line.IndexOf('=') + 1, line.Length - line.IndexOf('=') - 1);
            }

            return sUserID;
        }

        void CloseReadIDCard()
        {
            PassportSDK.SDT_ClosePort(0);
        }

        private bool OpenReadIDCard()
        {
            //return true;
            bool bRet = false;
            int[] nSuid = { 0 };
            PassportSDK.SetIDCardID(1, nSuid, 1);
            for (int iPort = 1001; iPort < 1017; iPort = iPort + 1)
            {
                int nRet = PassportSDK.SDT_OpenPort(iPort);
                if (nRet == 0x90)
                {
                    m_nOpenPort = iPort;
                    bRet = true;
                    break;
                }else
                {
                    Trace.WriteLine("启动护照身份证错误：端口 " + iPort + " 返回：" + nRet);
                }
            }
            return bRet;
        }
        byte[] pucCHMsg = new byte[512];
        byte[] pucPHMsg = new byte[1024];
        byte[] pucPHMsg1 = new byte[1024 * 128];
        int pucPHMsgLen1 = 1024 * 128;
        int puiCHMsgLen = 512;
        int puiPHMsgLen = 1024;

        int ReadIDCard(out SCardInfo info)
        {
            //info = new SCardInfo();
            //return 2;
            byte[] pRAPDU = new byte[30];
            byte[] pucAppMsg = new byte[320];
            int len = 320;
            info = new SCardInfo();
            int nRet = PassportSDK.SDT_ReadNewAppMsg(m_nOpenPort, ref pucAppMsg[0], ref len, 0);
            if (nRet == 0x91 || nRet == 0x90)
            {
                //("此卡已读过！");
                return 1;
            }

            nRet = PassportSDK.SDT_StartFindIDCard(m_nOpenPort, ref pRAPDU[0], 0);
            if (nRet != 0x9F)
            {
                //MessageBox.Show("寻找卡失败");
                return 2;
            }

            if (PassportSDK.SDT_SelectIDCard(m_nOpenPort, ref pRAPDU[0], 0) != 0x90)
            {
                //MessageBox.Show("选卡失败");
                return 3;
            }

            nRet = PassportSDK.SDT_ReadBaseMsg(m_nOpenPort, ref pucCHMsg[0], ref puiCHMsgLen, ref pucPHMsg[0], ref puiPHMsgLen, 0);
            if (nRet != 0x90)
            {
                //MessageBox.Show("读取数据到数组失败");
                return 4;
            }
           // = new SCardInfo();
            //strReadMsg = GetName().Trim();
            info._name = GetName().Trim();
            Trace.WriteLine("读到身份证信息：" + info._name);
            //strReadMsg += "," + GetSex().Trim();
            info._sex = GetSex().Trim();
            //strReadMsg += "," + GetNation().Trim();
            info._nation = GetNation().Trim();
            //strReadMsg += "," + GetBirthday().Trim();
            info._birthday = GetBirthday().Trim();
            //strReadMsg += "," + GetAddress().Trim();
            info._address = GetAddress().Trim();
            //strReadMsg +=  "," + GetIDCode().Trim();
            info._idNum = GetIDCode().Trim();
            //strReadMsg += "," + GetAuthority().Trim();
            info._issure = GetAuthority().Trim();
            //strReadMsg += "," + GetIssueDay().Trim() + "," + GetExpityDay().Trim();
            info._dateStart = GetIssueDay().Trim();
            info._dateEnd = GetExpityDay().Trim();

            //保存图像
            string path = Directory.GetCurrentDirectory();
            try
            {
                File.Delete(path + "/zp.bmp");
                File.Delete(path + "/zp_temp.tmp");
                File.Delete(path + "/zp.wlt");
            }catch(Exception ex)
            { }
            string strFilePtr = info._idNum + info._dateEnd;
            //path += "\\device\\passport_630";
            info._photo = SavePhoto(path, strFilePtr, 2);

            
            return 0;
        }
        public static Image BytesToImage(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            Image image = System.Drawing.Image.FromStream(ms);
            return image;
        }
        string SavePhoto(string retFileName, string strFilePtr, int nType,bool bXinPian = false)
        {
            //判断照片是否存在
            String JpegImagePath = ConfigOperator.Single().StrSavePath;
            if (Directory.Exists(JpegImagePath) == false)//如果不存在就创建file文件夹
            {
                Directory.CreateDirectory(JpegImagePath);
            }
            string strTmp = JpegImagePath + @"\cardImg\";
            if (!Directory.Exists(strTmp))
            {
                Directory.CreateDirectory(strTmp);
            }

            Base64Crypt crypt = new Base64Crypt();

            string strEncNum = crypt.Encode(strFilePtr);
            string jpegImage = JpegImagePath + @"\cardImg\" + strEncNum + "_zp.jpg";

            if (File.Exists(jpegImage))
            {
                return jpegImage;
            }

            string savepath = retFileName + "\\zp.wlt";
            try
            {
                if (bXinPian)
                {
                    //PassportSDK.SaveImageEx()
                }
                else
                {
                    FileStream fs;
                    fs = new FileStream(savepath, FileMode.Create, FileAccess.ReadWrite);
                    fs.Write(pucPHMsg, 0, pucPHMsg.Length);
                    fs.Close();
                }
                
            }catch(Exception ex)
            {
                Trace.WriteLine(savepath + " open error " + ex.Message);
            }
            
            if (nType == 2)
            {
                PassportSDK.GetBmp(savepath, 2);
            }else if(nType == 1)
            {
                string str = retFileName + "\\zp.bmp";
                Trace.WriteLine("保存护照照片 " + str);
                if (bXinPian)
                {
                    PassportSDK.SaveImageEx(str.ToCharArray(), 16);
                }else
                    PassportSDK.SaveHeadImage((str).ToCharArray());
            }
            if(File.Exists(retFileName + "\\zp.bmp"))
            {
                File.Copy(retFileName + "\\zp.bmp", retFileName + "\\zp_temp.bmp",true);
                using (Bitmap bmp = new Bitmap(retFileName + "\\zp_temp.bmp"))
                {
                    bmp.Save(jpegImage, ImageFormat.Jpeg);
                    bmp.Dispose();
                }
            }

            return jpegImage;
        }
        PassportInfo _cPassport = new PassportInfo();
        int RealReadPassport()
        {
            int ncardType = 0;
            int nRet = PassportSDK.GetGrabSignalType();
            
            if (nRet != 1)
            {
                //Trace.WriteLine("GetGrabSignalType 返回 采集信号未触发 ");
                return 1;
            }
            int[] nSubID = new int[1];
            nSubID[0] = 0;
            int nRet1 = PassportSDK.SetIDCardID(13, nSubID, 1);//返回0代表成功，其他失败，设置识别护照
            PassportSDK.SetIDCardID(2, nSubID, 1);//身份证照片页
            PassportSDK.AddIDCardID(3, nSubID, 1);//身份证签发机关页
            PassportSDK.AddIDCardID(9, nSubID, 1);//港澳通行证
            PassportSDK.AddIDCardID(13, nSubID, 1);//护照
            nRet = PassportSDK.ClassifyIDCard(ref ncardType);
            if(nRet < 0)
            {
                Trace.WriteLine("ClassifyIDCard 失败 " + nRet);
                return 2;
            }
            if(ncardType > 0)
            {
                Trace.WriteLine("ncardType = " + ncardType);

                string strDg1 = "";
                int nRetPasspart = 0;
                if(ncardType == 2)
                {
                    nRetPasspart = PassportSDK.RecogGeneralMRZCard(bRecogVIZ, nSaveImageType);
                }else if(ncardType == 1)
                {
                    //表示 证件类型是电子芯片类
                    Trace.WriteLine(nDGGroup + "----" + bRecogVIZ.ToString() + "----" + nSaveImageType);
                    
                    //1表示保存白光 图
                    //2表示保存红外 图
                    //4表示保存紫外 图
                    //8表示保存版面头像
                    //16 表示保存芯片头像
                    int nSaveImgType = 0x10;       
                    // 
                    int nDGValue = 6150;
                    byte[] bt = new byte[1024];
                    nRetPasspart = PassportSDK.RecogChipCard(nDGValue, bRecogVIZ, nSaveImgType);
                    if (nRetPasspart > 0)
                    {
                        int nBtLen = 1024;
                        PassportSDK.GetDataGroupContent(1, false, bt, ref nBtLen);
                        strDg1 = System.Text.Encoding.UTF8.GetString(bt);
                        Trace.WriteLine("strDg1 = " + strDg1);
                        //PassportSDK.GetDataGroupContent(2, false, pucPHMsg1, ref pucPHMsgLen1);
                        
                        //PassportSDK.GetDataGroupContent(11, false, bt, ref nBtLen);
                        //strDg1 = System.Text.Encoding.UTF8.GetString(bt);
                        //Trace.WriteLine("strDg11 = " + strDg1);
                        //PassportSDK.GetDataGroupContent(12, false, bt, ref nBtLen);
                        //strDg1 = System.Text.Encoding.UTF8.GetString(bt);
                        //Trace.WriteLine("strDg12 = " + strDg1);
                    }
                }
                if (nRetPasspart > 0)
                {
                    //int nIsReal = PassportSDK.CheckUVDull(false, 0);
                    //if (nIsReal != 0) return 4;
                    //读到护照信息
                    int MAX_CH_NUM = 128;
                    char[] cArrFieldValue = new char[MAX_CH_NUM];
                    char[] cArrFieldName = new char[MAX_CH_NUM];
                    for (int i = 0; ; i++)
                    {
                        nRet = PassportSDK.GetRecogResult(i, cArrFieldValue, ref MAX_CH_NUM);
                        if (nRet == 3)
                        {
                            break;
                        }
                        string sFieldValue = new string(cArrFieldValue);
                        PassportSDK.GetFieldName(i, cArrFieldName, ref MAX_CH_NUM);
                        sFieldValue = sFieldValue.Substring(0, sFieldValue.IndexOf('\0'));
                        #region cPassport对象赋值
                        switch (i)
                        {
                            case 0:
                                _cPassport.Type = sFieldValue;
                                continue;
                            case 1:
                                _cPassport.MRZ = sFieldValue;
                                continue;
                            case 2:
                                _cPassport.Name = sFieldValue;
                                continue;
                            case 3:
                                _cPassport.EnglishName = sFieldValue;
                                continue;
                            case 4:
                                _cPassport.Sex = sFieldValue;
                                continue;
                            case 5:
                                _cPassport.DateOfBirth = sFieldValue;
                                continue;
                            case 6:
                                _cPassport.DateOfExpiry = sFieldValue;
                                continue;
                            case 7:
                                _cPassport.CountryCode = sFieldValue;
                                continue;
                            case 8:
                                _cPassport.EnglishFamilyName = sFieldValue;
                                continue;
                            case 9:
                                _cPassport.EnglishGivienName = sFieldValue;
                                continue;
                            case 10:
                                _cPassport.MRZ1 = sFieldValue;
                                continue;
                            case 11:
                                _cPassport.MRZ2 = sFieldValue;
                                continue;
                            case 12:
                                _cPassport.Nationality = sFieldValue;
                                continue;
                            case 13:
                                _cPassport.PassportNo = sFieldValue;
                                continue;
                            case 14:
                                _cPassport.PlaceOfBirth = sFieldValue;
                                continue;
                            case 15:
                                _cPassport.PlaceOfIssue = sFieldValue;
                                continue;
                            case 16:
                                _cPassport.DateOfIssue = sFieldValue;
                                continue;
                            case 17:
                                _cPassport.RFIDMRZ = sFieldValue;
                                continue;
                            case 18:
                                _cPassport.OCRMRZ = sFieldValue;
                                continue;
                            case 19:
                                _cPassport.PlaceOfBirthPinyin = sFieldValue;
                                continue;
                            case 20:
                                _cPassport.PlaceOfIssuePinyin = sFieldValue;
                                continue;
                            case 21:
                                _cPassport.PersonalIdNo = sFieldValue;
                                continue;
                            case 22:
                                _cPassport.NamePinyinOCR = sFieldValue;
                                continue;
                            case 23:
                                _cPassport.SexOCR = sFieldValue;
                                continue;
                            case 24:
                                _cPassport.NationalityOCR = sFieldValue;
                                continue;
                            case 25:
                                _cPassport.PersonalIdNoOCR = sFieldValue;
                                continue;
                            case 26:
                                _cPassport.PlaceOfBirthOCR = sFieldValue;
                                continue;
                            case 27:
                                _cPassport.DateOfExpiryOCR = sFieldValue;
                                continue;
                            case 28:
                                _cPassport.AuthorityOCR = sFieldValue;
                                continue;
                            case 29:
                                _cPassport.FamilyName = sFieldValue;
                                continue;
                            case 30:
                                _cPassport.GivienName = sFieldValue;
                                continue;
                            default:
                                break;
                        }
                        #endregion
                    }
                    if(ncardType == 1)
                    {
                        if (strDg1.Length > 5)
                        {
                            string strCountryCode = strDg1.Substring(2, 3);
                            _cPassport.Nation = CountryCodyDiction.GetCountryName(strCountryCode);
                        }
                    }else
                    {
                        if (_cPassport.MRZ1 == null || _cPassport.MRZ1.Length > 5)
                        {
                            Trace.WriteLine("NationalityOCR = " + _cPassport.MRZ1);
                            string strContryCode = _cPassport.MRZ1.Substring(2, 3);
                            _cPassport.Nation = CountryCodyDiction.GetCountryName(strContryCode);
                        }
                    }
                    if (_cPassport.DateOfBirth == null || _cPassport.DateOfBirth.Length == 0)
                    {
                        _cPassport.DateOfBirth = _cPassport.PlaceOfBirthOCR;
                    }
                    string path = Directory.GetCurrentDirectory();
                    Trace.WriteLine("currentDir = " + path);
                    string strFilePtr = _cPassport.PassportNo + _cPassport.MRZ;
                    if(ncardType == 1)
                    {
                        _cPassport.PhotoHead = SavePhoto(path, strFilePtr, 1,true);
                    }else
                       _cPassport.PhotoHead = SavePhoto(path, strFilePtr, 1);
                    return 0;
                }else
                {
                    Trace.WriteLine("RecogGeneralMRZCard 返回未识别 " + nRetPasspart);
                }
            }
            
            return 1;
        }

        void OnRun()
        {
            DateTime dtLastTick = DateTime.Now;

            Thread.Sleep(5000);
            Trace.WriteLine("开始读卡和读护照......");
            while (m_ThreadHandle.IsAlive)
            {
                TimeSpan ts = DateTime.Now.Subtract(dtLastTick);
                if(ts.TotalMilliseconds < 330)
                {
                    Thread.Sleep(50);
                    continue;
                }

                //string str
                bool bReadIDCard = false;       //本次循环是否读到身份证信息
                bool bReadPassport = false;
                SCardInfo info = new SCardInfo();
                int nRetReadCard = ReadIDCard(out info);
                if(nRetReadCard == 0)
                {
                    //读到身份证信息
                    bReadIDCard = true;
                    string strContext = info._name + "," + info._sex + "," + info._birthday + "," + info._nation + "," +
                        info._address + "," + info._idNum + "," + info._issure + "," + info._dateStart + "," + info._dateEnd + "," + info._photo;
                    SaveReadTxt(strContext, 1);
                }
                //如果读到身份证信息  不再读取护照信息
                if (!bReadIDCard)
                {
                    //_cPassport.PassportNo = "";
                    int nRet = RealReadPassport();

                    if(nRet == 0)
                    {
                        //读到护照信息
                        bReadPassport = true;
                        string strContext = (_cPassport.PassportNo.Length == 0?_cPassport.MRZ:_cPassport.PassportNo) + "~" +
                            _cPassport.Nation + "~" +
                            _cPassport.Sex + "~" +
                            _cPassport.MRZ + "~" +
                            _cPassport.Name + "~" +
                            _cPassport.DateOfBirth + "~" +
                            _cPassport.EnglishName + "~" +
                            _cPassport.EnglishFamilyName + "~" +
                            _cPassport.EnglishGivienName + "~" +
                            _cPassport.MRZ1 + "~" +
                            _cPassport.MRZ2 + "~" +
                            _cPassport.Nationality + "~" +
                            _cPassport.PlaceOfBirth + "~" +
                            _cPassport.PlaceOfIssue + "~" +
                            _cPassport.DateOfIssue + "~" +
                            _cPassport.RFIDMRZ + "~" +
                            _cPassport.OCRMRZ + "~" +
                            _cPassport.PlaceOfBirthPinyin + "~" +
                            _cPassport.PlaceOfIssuePinyin + "~" +
                            _cPassport.PersonalIdNo + "~" +
                            _cPassport.NamePinyinOCR + "~" +
                            _cPassport.SexOCR + "~" +
                            _cPassport.NationalityOCR + "~" +
                            _cPassport.PersonalIdNoOCR + "~" +
                            _cPassport.PlaceOfBirthOCR + "~" +
                            _cPassport.DateOfExpiryOCR + "~" +
                            _cPassport.AuthorityOCR + "~" +
                             _cPassport.FamilyName + "~" +
                             _cPassport.GivienName + "~" +
                             _cPassport.Photo + "~" +
                             _cPassport.PhotoHead;

                        string[] ss = strContext.Split('~');
                        int nLen = ss.Length;
                        SaveReadTxt(strContext, 2);
                    }
                }
                
                if(bReadIDCard || bReadPassport)
                {
                    Thread.Sleep(300);
                }

                dtLastTick = DateTime.Now;
                Thread.Sleep(50);
                continue;
            }
        }

        int InitPtr()
        {
            m_ThreadHandle = new Thread(OnRun);
            m_ThreadHandle.IsBackground = true;
            string strExePath = Directory.GetCurrentDirectory();
            //if(!File.Exists(strExePath + "\\WltRS.dll"))
                strExePath += "\\device\\passport_630";
            Trace.WriteLine("dllPath = " + strExePath);
            int nIdCard = PassportSDK.LoadLibrary(strExePath +  "\\IDCard.dll");
            int nSdtapi = PassportSDK.LoadLibrary(strExePath + "\\sdtapi.dll");
            int nWltRS = PassportSDK.LoadLibrary(strExePath + "\\WltRS.dll");
            if(nIdCard == 0 || nSdtapi == 0 || nWltRS == 0)
            {
                Trace.WriteLine("InitPtr " + nIdCard + "," + nSdtapi + "," + nWltRS);
                return 1;
            }

            string sDG = "1|2|11|12";
            string sSaveImage = "1|2|8";
            string[] sArray = sDG.Split('|');
            foreach (string i in sArray)
            {
                this.nDGGroup |= (1 << (int.Parse(i) - 1));
            }
            string[] sArrayImage = sSaveImage.Split('|');
            foreach (string i in sArrayImage)
            {
                this.nSaveImageType |= int.Parse(i);
            }
            this.bRecogVIZ = true;
            string _strUserId = GetUserId();

            int nRet = PassportSDK.InitIDCard(_strUserId.ToCharArray(), 1, null);
            Trace.WriteLine("check userid = " + nRet);
            return nRet;
        }

        public bool InitPassport()
        {
            bool bRet = false;
            int nRet = InitPtr();   //加载动态库和初始化核心库
            if (nRet != 0) return false;
            
            Trace.WriteLine("begin openPort");
            bRet = OpenReadIDCard();

            PassportSDK.SetSpecialAttribute(1, 1);
            int[] nSubID = new int[1];
            nSubID[0] = 0;
            int nAddRet = PassportSDK.SetIDCardID(13, nSubID, 1);//身份证照片页
            Trace.WriteLine("nAddRet = " + nAddRet);
            PassportSDK.AddIDCardID(2, nSubID, 1);
            PassportSDK.AddIDCardID(3, nSubID, 1);//身份证签发机关页
            PassportSDK.AddIDCardID(9, nSubID, 1);
            PassportSDK.AddIDCardID(13, nSubID, 1);//护照
            if (bRet) m_ThreadHandle.Start();
            return bRet;
        }


        private void SaveReadTxt(string strContext,int nType)
        {
            string strFileName = "readcard.txt";
            if (nType == 2)
            {
                //读到护照
                strFileName = "readpassport.txt";
                Trace.WriteLine("读取到护照信息****** "+ strContext);
            }
            try
            {
                if (File.Exists(strFileName))
                {
                    File.Delete(strFileName);
                }
                File.WriteAllText(strFileName, strContext);
            }catch(Exception ex)
            {

            }
        }

        #region 身份证信息获取
        private string GetName()
        {
            if (puiCHMsgLen == 0)
            {
                return "";
            }
            string str = System.Text.Encoding.Unicode.GetString(pucCHMsg, 0, 30);
            return str;

        }
        private string GetSex()
        {
            if (puiCHMsgLen == 0)
            {
                return " ";
            }

            byte sex = pucCHMsg[30];

            if (sex == '1')
            {
                return "男";
            }
            else
                return "女";

        }

        private string GetNation()
        {
            if (puiCHMsgLen == 0)
            {
                return " ";
            }

            string str = System.Text.Encoding.Unicode.GetString(pucCHMsg, 32, 4);
            switch (str)
            {
                case "01": return "汉";
                case "02": return "蒙古";
                case "03": return "回";
                case "04": return "藏";
                case "05": return "维吾尔";
                case "06": return "苗";
                case "07": return "彝";
                case "08": return "壮";
                case "09": return "布依";
                case "10": return "朝鲜";
                case "11": return "满";
                case "12": return "侗";
                case "13": return "瑶";
                case "14": return "白";
                case "15": return "土家";
                case "16": return "哈尼";
                case "17": return "哈萨克";
                case "18": return " 傣";
                case "19": return " 黎";
                case "20": return " 傈僳";
                case "21": return " 佤";
                case "22": return " 畲";
                case "23": return " 高山";
                case "24": return " 拉祜";
                case "25": return " 水";
                case "26": return " 东乡";
                case "27": return " 纳西";
                case "28": return " 景颇";
                case "29": return " 柯尔克孜";
                case "30": return " 土";
                case "31": return " 达斡尔";
                case "32": return " 仫佬";
                case "33": return "羌";
                case "34": return "布朗";
                case "35": return "撒拉";
                case "36": return "毛南";
                case "37": return "仡佬";
                case "38": return "锡伯";
                case "39": return "阿昌";
                case "40": return "普米";
                case "41": return "塔吉克";
                case "42": return "怒";
                case "43": return "乌孜别克";
                case "44": return "俄罗斯";
                case "45": return "鄂温克";
                case "46": return "德昂";
                case "47": return "保安";
                case "48": return "裕固";
                case "49": return "京";
                case "50": return "塔塔尔";
                case "51": return "独龙";
                case "52": return "鄂伦春";
                case "53": return "赫哲";
                case "54": return "门巴";
                case "55": return "珞巴";
                case "56": return "基诺";
                case "97": return "其他";
                case "98": return "外国血统中国籍人士";
                default: return "";
            }

        }
        private string GetBirthday()
        {
            if (puiCHMsgLen == 0)
            {
                return " ";
            }
            string str = System.Text.Encoding.Unicode.GetString(pucCHMsg, 36, 16);
            return str;
        }
        private string GetAddress()
        {
            if (puiCHMsgLen == 0)
                return " ";

            string str = System.Text.Encoding.Unicode.GetString(pucCHMsg, 52, 70);
            return str;
        }
        private string GetAuthority()
        {
            if (puiCHMsgLen == 0)
                return " ";
            string str = System.Text.Encoding.Unicode.GetString(pucCHMsg, 158, 30);
            return str;
        }
        private string GetIDCode()
        {
            if (puiCHMsgLen == 0)
                return "";

            string str = System.Text.Encoding.Unicode.GetString(pucCHMsg, 122, 36);
            return str;
        }
        private string GetIssueDay()
        {
            if (puiCHMsgLen == 0)
                return "";


            string str = System.Text.Encoding.Unicode.GetString(pucCHMsg, 188, 16);
            return str;
        }
        private string GetExpityDay()
        {
            if (puiCHMsgLen == 0)
                return "";

            string str = System.Text.Encoding.Unicode.GetString(pucCHMsg, 204, 16);
            return str;
        }
        #endregion

    }
}
