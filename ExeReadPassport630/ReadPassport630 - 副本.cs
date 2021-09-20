using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExeReadPassport630
{
    class ReadPassport630
    {
        #region DLL引用
        [DllImport("kernel32")]
        public static extern int LoadLibrary(string strDllName);

        const string dllPath = @"IDCard.dll";//
        const string idCardDll = @"sdtapi.dll";//device\Passport_630\
        const string idApiDll = @"zysdtapi.dll";//device\Passport_630\
        const string photoDll = @"WltRS.dll"; //device\Passport_630\

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int InitIDCard(char[] cArrUserID, int nType, char[] cArrDirectory);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetRecogResult(int nIndex, char[] cArrBuffer, ref int nBufferLen);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogIDCard();

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetFieldName(int nIndex, char[] cArrBuffer, ref int nBufferLen);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int AcquireImage(int nCardType);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int SaveImage(char[] cArrFileName);
        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int SaveHeadImage(char[] cArrFileName);


        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetCurrentDevice(char[] cArrDeviceName, int nLength);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern void GetVersionInfo(char[] cArrVersion, int nLength);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern bool CheckDeviceOnline();

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern bool SetAcquireImageType(int nLightType, int nImageType);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern bool SetUserDefinedImageSize(int nWidth, int nHeight);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern bool SetAcquireImageResolution(int nResolutionX, int nResolutionY);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int SetIDCardID(int nMainID, int[] nSubID, int nSubIdCount);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int AddIDCardID(int nMainID, int[] nSubID, int nSubIdCount);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogIDCardEX(int nMainID, int nSubID);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetButtonDownType();

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetGrabSignalType();

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int SetSpecialAttribute(int nType, int nSet);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern void FreeIDCard();
        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetDeviceSN(char[] cArrSn, int nLength);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetBusinessCardResult(int nID, int nIndex, char[] cArrBuffer, ref int nBufferLen);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogBusinessCard(int nType);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetBusinessCardFieldName(int nID, char[] cArrBuffer, ref int nBufferLen);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetBusinessCardResultCount(int nID);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int LoadImageToMemory(string path, int nType);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int ClassifyIDCard(ref int nCardType);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogChipCard(int nDGGroup, bool bRecogVIZ, int nSaveImageType);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogGeneralMRZCard(bool bRecogVIZ, int nSaveImageType);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogCommonCard(int nSaveImageType);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int SaveImageEx(char[] lpFileName, int nType);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetDataGroupContent(int nDGIndex, bool bRawData, byte[] lpBuffer, ref int len);

        [DllImport(idCardDll, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int SDT_OpenPort(int iPort);
        [DllImport(idCardDll, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int SDT_ClosePort(int iPort);

        [DllImport(idCardDll, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int SDT_StartFindIDCard(int iPort, ref byte pRAPDU, int iIfOpen);

        [DllImport(idCardDll, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int SDT_SelectIDCard(int iPort, ref byte pRAPDU, int iIfOpen);

        [DllImport(idCardDll, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int SDT_ReadBaseMsg(int iPort, ref byte pucCHMsg, ref int puiCHMsgLen, ref byte pucPHMsg, ref int puiPHMsgLen, int iIfOpen);

        [DllImport(idCardDll, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int SDT_ReadNewAppMsg(int iPort, ref byte pucAppMsg, ref int puiAppMsgLen, int iIfOpen);

        [DllImport(@"WltRS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetBmp(string filename, int nType);

        #endregion
        private string _strUserId = string.Empty;
        private bool _bIsDllLoaded = false;
        private const int _nType = 13;//   证件类型

        /// <summary>
        /// 循环读卡线程
        /// </summary>
        private Thread m_ThreadHandle = null;


        private int nDGGroup = 0;//表示要读取的DG，对于普通设备调用传0
        private bool bRecogVIZ = true;//true表示识别视读区相关类容，false则相反
        private int nSaveImageType = 0;//保存图片类型：1表示保存白光全图,2表示保存红外全图,4表示保存紫外全图,8表示保存版面头像,16 表示保存芯片的头像

        private string GetUserId()
        {
            string strFilePath = Assembly.GetExecutingAssembly().Location;
            int nPos = strFilePath.LastIndexOf('\\');
            strFilePath = strFilePath.Substring(0, nPos);

            string sUserID = string.Empty;

            string sPath = strFilePath + @".\UserId.txt";
            using (StreamReader sr = new StreamReader(sPath, Encoding.Default))
            {
                String line = sr.ReadLine();//读取第一行
                sUserID = line.Substring(line.IndexOf('=') + 1, line.Length - line.IndexOf('=') - 1);
            }

            return sUserID;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>返回0正确，小于0初始化错误，大于0授权文件错误</returns>
        private int InitPtr()
        {
            m_ThreadHandle = new Thread(OnRun);
            m_ThreadHandle.IsBackground = true;

            if (_bIsDllLoaded)
            {
                return -1;
            }
            string strExePath = Directory.GetCurrentDirectory();
            string strDllPath = strExePath + "\\" + dllPath;
            int nRet = LoadLibrary(strDllPath);
            if (nRet == 0)
            {
                Trace.WriteLine("630护照类型加载 dllpath 失败");
                return -2;
            }
            strDllPath = strExePath + "\\" + idCardDll;
            nRet = LoadLibrary(strDllPath);
            if (nRet == 0)
            {
                Trace.WriteLine("630护照类型加载 idCardDll 失败");
                return -3;
            }
            strDllPath = strExePath + "\\" + idApiDll;
            nRet = LoadLibrary(strDllPath);
            if (nRet == 0)
            {
                Trace.WriteLine("630护照类型加载 zysdtapi 失败");
                return -4;
            }
            strDllPath = strExePath + "\\" + photoDll;
            nRet = LoadLibrary(photoDll);
            if (nRet == 0)
            {
                Trace.WriteLine("630护照类型加载 photoDll 失败");
                return -5;
            }
            string sDG = "1|2|11|12";
            string sSaveImage = "1|8";
            string[] sArray = sDG.Split('|');
            foreach (string i in sArray)
            {
                this.nDGGroup |= (1 << (int.Parse(i) - 1));
            }
            string[] sArrayImage = sSaveImage.Split('|');
            foreach (string i in sArray)
            {
                this.nSaveImageType |= int.Parse(i);
            }
            this.bRecogVIZ = true ;

            this._strUserId = GetUserId();

            nRet = InitIDCard(this._strUserId.ToCharArray(), 1, null);
            return nRet;
        }

        private FileOperator fileOper;
        public bool Init()
        {
            int nRet = InitPtr();
            if (nRet != 0)
            {
                Trace.WriteLine("630护照初始化错误：" + nRet);
                return false;
            }
            fileOper = new FileOperator();
            SetSpecialAttribute(1, 1);

            bool bRet = false;
            bool bFlagOpenIdCard = false;
            for (int iPort = 1001; iPort < 1017; iPort = iPort + 1)
            {
                nRet = SDT_OpenPort(iPort);
                if (nRet == 0x90)
                {
                    m_nOpenPort = iPort;
                    bFlagOpenIdCard = true;
                    break;
                }
            }

            if (bFlagOpenIdCard)
            {
                m_ThreadHandle.Start();
                bRet = true;
            }

            return bRet;
        }

        byte[] pucCHMsg = new byte[512];
        byte[] pucPHMsg = new byte[1024];
        int puiCHMsgLen = 512;
        int puiPHMsgLen = 1024;
        private static PassportInfo cPassport = null;
        private int m_nOpenPort = 0;//身份证识别端口打开
        public int iType = -2;
        private const int nType = 13;//证件类型，13为护照

        void OnRun()
        {
            bool bIsReadPassport = false;
            bool bIsReadIDCard = false;
            Thread.Sleep(3000);
            PassportInfo passportInfo = new PassportInfo();

            DateTime dtLastTick = DateTime.Now;
            int MAX_CH_NUM = 128;
            char[] cArrFieldValue = new char[MAX_CH_NUM];
            char[] cArrFieldName = new char[MAX_CH_NUM];
            while (m_ThreadHandle.IsAlive)
            {
                
                TimeSpan ts = DateTime.Now.Subtract(dtLastTick);
                if(ts.TotalMilliseconds < 330)
                {
                    Thread.Sleep(100);
                    continue;
                }


                int nRet = 0;
                nRet = GetGrabSignalType();
                bIsReadPassport = false;
                if (nRet == 1)
                {
                    int[] nSubID = new int[1];
                    nSubID[0] = 0;
                    int nRet1 = SetIDCardID(nType, nSubID, 1);//返回0代表成功，其他失败，设置识别护照
                    SetIDCardID(2, nSubID, 1);//身份证照片页
                    AddIDCardID(3, nSubID, 1);//身份证签发机关页
                    AddIDCardID(13, nSubID, 1);//护照

                    if (nRet1 != 0)
                    {
                        Trace.WriteLine("证件类别设置失败");
                        cPassport = null;
                        Thread.Sleep(50);
                        continue;
                    }

                    int ncardType = 0;
                    nRet = ClassifyIDCard(ref ncardType);
                    string filePath = Directory.GetCurrentDirectory();

                    if (nRet < 0)
                    {
                        Trace.WriteLine("证件分类获取失败");
                        cPassport = null;
                        iType = -1;
                    }
                    if (ncardType == 1)//芯片卡
                    {
                        nRet = RecogChipCard(nDGGroup, bRecogVIZ, nSaveImageType);
                        iType = 2;
                    }

                    if (ncardType == 2)//普通MRZ证件
                    {
                        nRet = RecogGeneralMRZCard(bRecogVIZ, nSaveImageType);
                        iType = 2;//当前识别为护照
                    }
                    if (ncardType == 3)//不含有MRZ的普通证件（身份证识别）
                    {
                        nRet = RecogCommonCard(nSaveImageType);
                        //Thread.Sleep(50);
                        //continue;
                    }

                    if (nRet < 0)//识别失败
                    {
                        Trace.WriteLine("证件识别失败");
                        cPassport = null;
                        iType = -1;
                        //continue;
                    }

                    if (nRet > 0)
                    {
                        iType = 2;
                        bIsReadPassport = true;
                        bIsReadIDCard = false;
                        cPassport = new PassportInfo();
                        cPassport.CardType = "2";
                        #region 识别成功,取得识别结果
                        
                        
                        if (cPassport != null)
                        {
                            for (int i = 0; ; i++)
                            {
                                nRet = GetRecogResult(i, cArrFieldValue, ref MAX_CH_NUM);
                                if (nRet == 3)
                                {
                                    break;
                                }
                                
                                string sFieldValue = new string(cArrFieldValue);
                                sFieldValue = sFieldValue.Substring(0, sFieldValue.IndexOf('\0'));
                                #region cPassport对象赋值
                                switch (i)
                                {
                                    case 0:
                                        cPassport.Type = sFieldValue;
                                        continue;
                                    case 1:
                                        cPassport.MRZ = sFieldValue;
                                        continue;
                                    case 2:
                                        cPassport.Name = sFieldValue;
                                        continue;
                                    case 3:
                                        cPassport.EnglishName = sFieldValue;
                                        continue;
                                    case 4:
                                        cPassport.Sex = sFieldValue;
                                        continue;
                                    case 5:
                                        cPassport.DateOfBirth = sFieldValue;
                                        continue;
                                    case 6:
                                        cPassport.DateOfExpiry = sFieldValue;
                                        continue;
                                    case 7:
                                        cPassport.CountryCode = sFieldValue;
                                        continue;
                                    case 8:
                                        cPassport.EnglishFamilyName = sFieldValue;
                                        continue;
                                    case 9:
                                        cPassport.EnglishGivienName = sFieldValue;
                                        continue;
                                    case 10:
                                        cPassport.MRZ1 = sFieldValue;
                                        continue;
                                    case 11:
                                        cPassport.MRZ2 = sFieldValue;
                                        continue;
                                    case 12:
                                        cPassport.Nationality = sFieldValue;
                                        continue;
                                    case 13:
                                        cPassport.PassportNo = sFieldValue;
                                        continue;
                                    case 14:
                                        cPassport.PlaceOfBirth = sFieldValue;
                                        continue;
                                    case 15:
                                        cPassport.PlaceOfIssue = sFieldValue;
                                        continue;
                                    case 16:
                                        cPassport.DateOfIssue = sFieldValue;
                                        continue;
                                    case 17:
                                        cPassport.RFIDMRZ = sFieldValue;
                                        continue;
                                    case 18:
                                        cPassport.OCRMRZ = sFieldValue;
                                        continue;
                                    case 19:
                                        cPassport.PlaceOfBirthPinyin = sFieldValue;
                                        continue;
                                    case 20:
                                        cPassport.PlaceOfIssuePinyin = sFieldValue;
                                        continue;
                                    case 21:
                                        cPassport.PersonalIdNo = sFieldValue;
                                        continue;
                                    case 22:
                                        cPassport.NamePinyinOCR = sFieldValue;
                                        continue;
                                    case 23:
                                        cPassport.SexOCR = sFieldValue;
                                        continue;
                                    case 24:
                                        cPassport.NationalityOCR = sFieldValue;
                                        continue;
                                    case 25:
                                        cPassport.PersonalIdNoOCR = sFieldValue;
                                        continue;
                                    case 26:
                                        cPassport.PlaceOfBirthOCR = sFieldValue;
                                        continue;
                                    case 27:
                                        cPassport.DateOfExpiryOCR = sFieldValue;
                                        continue;
                                    case 28:
                                        cPassport.AuthorityOCR = sFieldValue;
                                        continue;
                                    case 29:
                                        cPassport.FamilyName = sFieldValue;
                                        continue;
                                    case 30:
                                        cPassport.GivienName = sFieldValue;
                                        continue;
                                    default:
                                        break;
                                }
                                #endregion
                            }
                        }
                        #endregion
                    }
                }
                #region 身份证信息读取
                if (!bIsReadPassport && (iType != -1))
                {
                    byte[] pRAPDU = new byte[30];
                    byte[] pucAppMsg = new byte[320];
                    int len = 320;
                    nRet = SDT_ReadNewAppMsg(m_nOpenPort, ref pucAppMsg[0], ref len, 0);
                    if (nRet == 0x91 || nRet == 0x90)//此卡已读过
                    {
                        Trace.WriteLine("此卡已读过");
                        cPassport = null;
                        iType = -2;
                        Thread.Sleep(50);
                        dtLastTick = DateTime.Now;
                        continue;
                    }
                    else
                    {
                        iType = 0;
                    }
                    nRet = SDT_StartFindIDCard(m_nOpenPort, ref pRAPDU[0], 0);//寻找卡失败
                    if (nRet != 0x9F)
                    {
                        //Trace.WriteLine("寻找卡失败");
                        cPassport = null;
                        iType = -2;
                        Thread.Sleep(50);
                        continue;
                    }
                    else
                    {
                        iType = 0;
                    }
                    if (SDT_SelectIDCard(m_nOpenPort, ref pRAPDU[0], 0) != 0x90)//选卡失败
                    {
                        Trace.WriteLine("选卡失败");
                        cPassport = null;
                        iType = -2;
                        dtLastTick = DateTime.Now;
                        Thread.Sleep(50);
                        continue;
                    }
                    else
                    {
                        iType = 0;
                    }
                    nRet = SDT_ReadBaseMsg(m_nOpenPort, ref pucCHMsg[0], ref puiCHMsgLen, ref pucPHMsg[0], ref puiPHMsgLen, 0);//读取数据到数组失败
                    if (nRet != 0x90)
                    {
                        Trace.WriteLine("读取数据到数组失败");
                        cPassport = null;
                        iType = -2;
                    }
                    else
                    {
                        iType = 0;
                    }
                    if (iType == 0)
                    {
                        bIsReadIDCard = true;
                        bIsReadPassport = false;
                        cPassport = new PassportInfo();
                        #region 识别成功，获取信息
                        cPassport.CardType = "0";
                        cPassport.Name = GetName();
                        cPassport.Sex = GetSex();
                        cPassport.Nation = GetNation();
                        cPassport.DateOfBirth = GetBirthday();
                        cPassport.PlaceOfBirth = GetAddress();
                        cPassport.PersonalIdNo = GetIDCode();
                        cPassport.AuthorityOCR = GetAuthority();
                        cPassport.DateOfIssue = GetIssueDay();
                        cPassport.DateOfExpiry = GetExpityDay();
                        #endregion
                    }
                    #endregion

                }
                #region 保存图片
                if (cPassport != null)
                {
                    SaveImages();
                }
                #endregion

                #region 读取证件信息后触发事件，时间不为空，读到身份证信息或者护照信息或者读取护照信息时护照放置不正确
                
                #endregion

                #region 保存护照信息到本地硬盘
                if (cPassport != null)
                {
                    SaveReadPassPort();
                }
                #endregion

                bIsReadPassport = false;
                bIsReadIDCard = false;
                iType = -2;
                dtLastTick = DateTime.Now;
            }
        }

        private bool SaveImages()
        {
            if (iType == 2)//读取护照信息
            {
                //if (!m_bInited)
                //{
                //    return false;
                //}
                try
                {
                    string filePath = Directory.GetCurrentDirectory();

                    String JpegImagePath = ConfigOperator.Single().StrSavePath;
                    if (Directory.Exists(JpegImagePath) == false)//如果不存在就创建file文件夹
                    {
                        Directory.CreateDirectory(JpegImagePath);
                    }
                    Base64Crypt crypt = new Base64Crypt();

                    string strEncNum = crypt.Encode(cPassport.PassportNo + cPassport.DateOfIssue);
                    String jpegImage = JpegImagePath + @"\cardImg\" + strEncNum + "_zp.jpg";
                    cPassport.PhotoHead = jpegImage;
                    if (File.Exists(jpegImage))
                    {
                        return true;
                    }

                    string photoPath = filePath + "\\zp.wlt";
                    int j = SaveHeadImage(jpegImage.ToCharArray());//保存护照中头像信息
                    string sPathPhoto = filePath + "\\zp.bmp";
                    int i = SaveHeadImage(sPathPhoto.ToCharArray());
                    if (File.Exists(filePath + "\\zp.bmp"))
                    {
                        File.Copy(filePath + "\\zp.bmp", filePath + "\\zp_temp.bmp", true);

                        using (Bitmap bmp = new Bitmap(filePath + "\\zp_temp.bmp"))
                        {
                            bmp.Save(jpegImage, ImageFormat.Jpeg);
                            bmp.Dispose();
                        }
                    }
                    //m_isPassportRead = false;
                    cPassport.PhotoHead = jpegImage;

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                    throw ex;
                }
            }
            if (iType == 0)//读取身份证信息
            {
                //if (!m_bInited && !m_bNIDapi)
                //{
                //    return false;
                //}
                try
                {
                    string filePath = Directory.GetCurrentDirectory();

                    String JpegImagePath = ConfigOperator.Single().StrSavePath;
                    if (Directory.Exists(JpegImagePath) == false)//如果不存在就创建file文件夹
                    {
                        Directory.CreateDirectory(JpegImagePath);
                    }
                    Base64Crypt crypt = new Base64Crypt();

                    string strEncNum = crypt.Encode(cPassport.PassportNo + cPassport.DateOfIssue);
                    String jpegImage = JpegImagePath + @"\cardImg\" + strEncNum + "_zp.jpg";
                    cPassport.PhotoHead = jpegImage;
                    if (File.Exists(jpegImage))
                    {
                        return true;
                    }

                    string photoPath = filePath + "\\zp.wlt";

                    string path = @"device\Passport";
                    string savepath = @"device\Passport" + "\\head.wlt";

                    FileStream fs;
                    fs = new FileStream(savepath, FileMode.Create, FileAccess.ReadWrite);
                    fs.Write(pucPHMsg, 0, pucPHMsg.Length);
                    fs.Close();

                    int b = GetBmp(savepath, 2);//保存身份证中头像信息
                    path += "\\head.bmp";

                    File.Copy(path, filePath + "\\zp.bmp", true);//复制头像信息到当前目录下

                    File.Copy(filePath + "\\zp.bmp", filePath + "\\zp_temp.bmp", true);

                    using (Bitmap bmp = new Bitmap(filePath + "\\zp_temp.bmp"))
                    {
                        bmp.Save(jpegImage, ImageFormat.Jpeg);
                        bmp.Dispose();
                    }

                    //m_isPassportRead = false;
                    cPassport.PhotoHead = jpegImage;

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                    throw ex;
                }
            }

            return false;

        }
        //保存读取的证件信息组成的字符串
        private List<string> strCardNumLists = new List<string>();

        /// <summary>
        /// 保存护照信息到本地硬盘
        /// </summary>
        private void SaveReadPassPort()
        {
            //if (m_bInited)
            //{
            try
            {
                foreach (string s in strCardNumLists)
                {
                    string[] ss = s.Split(',');

                    if (ss[0].CompareTo(cPassport.PassportNo) == 0 && ss[7].CompareTo(cPassport.DateOfIssue) == 0)
                    {
                        return;
                    }
                }
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                watch.Start();  //开始监视代码运行时间

                //save to file
                fileOper.SaveCardInfo(cPassport, "2");
                //save to memory
                PushCardInfo();

                watch.Stop();  //停止监视
                TimeSpan timespan = watch.Elapsed;  //获取当前实例测量得出的总时间
            }
            catch (Exception ex)
            {
                throw ex;
            }
            // }
        }
        private void PushCardInfo()
        {
            //if (m_bInited)
            //{
            try
            {
                string strInfo = cPassport.PassportNo;
                strInfo += ",";
                strInfo += cPassport.Name;
                strInfo += ",";
                strInfo += cPassport.Sex;
                strInfo += ",";
                strInfo += cPassport.Nation;
                strInfo += ",";
                strInfo += cPassport.DateOfBirth;
                strInfo += ",";
                strInfo += cPassport.PlaceOfBirth;
                strInfo += ",";
                strInfo += cPassport.PlaceOfIssue;
                strInfo += ",";
                strInfo += cPassport.DateOfIssue;
                strInfo += ",";
                strInfo += cPassport.DateOfExpiry;
                strInfo += ",";
                strInfo += cPassport.PhotoHead;
                strCardNumLists.Add(strInfo);
            }
            catch (Exception ex)
            {
                Trace.Write("填充护照信息异常:" + ex.Message);
            }
            //}
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
