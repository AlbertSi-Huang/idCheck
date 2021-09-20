using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using Common;
using System.Drawing.Imaging;
using TS_IDCheck.Info;

/***********************
 * 功能：护照读取
 * 作者：刘飞翔
 * 时间：2018-4-25
 * 
 * 
 * *********************/

namespace TS_IDCheck
{

    public class ReadPassport : IReadCard
    {
        #region 字段定义
        private string m_sIpUserID = "";//用户ID，由文通提供
        private bool m_bIsIDCardLoaded = false;//IDCard载入成功与否标志
        private const int nType = 13;//证件类型，13为护照

        private int nDGGroup=0;//表示要读取的DG，对于普通设备调用传0
        private bool bRecogVIZ = true;//true表示识别视读区相关类容，false则相反
        private int nSaveImageType = 0;//保存图片类型：1表示保存白光全图,2表示保存红外全图,4表示保存紫外全图,8表示保存版面头像,16 表示保存芯片的头像
        private static PassportInfo cPassport = null;//护照读取对象
        //int i = 1 | 2 | 3;

        public int iType = -2;//证件类别：-2-重新读取,-1-读取护照信息时出错，0-读到身份证，2-读到护照
        
        /// <summary>
        /// 循环读卡线程
        /// </summary>
        private Thread m_ThreadHandle = null;

        /// <summary>
        /// 线程运行状态
        /// </summary>
        private bool m_ThreadRunStatus = false;

        /// <summary>
        /// 确认是否需要再次读护照
        /// </summary>
        private bool m_isNeedCapture = true;
        /// <summary>
        /// 是否打开机器
        /// </summary>
        private bool m_isOpened = false;

        private bool m_isPassportRead = false;//护照读取成功

        private String strRunPath = Application.StartupPath + "\\images";//图片保存路径文件

        private bool m_bInited = false;//护照初始化成功

        private int m_nOpenPort = 0;//身份证识别端口打开
        bool m_bNIDapi = false;//身份证识别打开成功
        byte[] pucCHMsg = new byte[512];
        byte[] pucPHMsg = new byte[1024];
        int puiCHMsgLen = 512;
        int puiPHMsgLen = 1024;

        public bool bIsReadPassport = false;//是否读取护照信息 
        public bool bIsReadIDCard = false;

        private FileOperator fileOper;

        private static ReadCardCompleteDelegate OnReadComplete;
        public event ReadCardCompleteDelegate readCompleteEvent
        {
            add { OnReadComplete += new ReadCardCompleteDelegate(value); }
            remove { OnReadComplete -= new ReadCardCompleteDelegate(value); }
        }

        public bool isNeedCapture
        {
            get { return m_isNeedCapture; }
            set { m_isNeedCapture = value; }
        }

        #endregion

        #region DLL引用
        [DllImport("kernel32")]
        public static extern int LoadLibrary(string strDllName);

        const string dllPath = @"device\Passport\IDCard.dll";
        const string idCardDll = @"device\Passport\sdtapi.dll";

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

        [DllImport(@"device\Passport\WltRS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetBmp(string filename, int nType);

        #endregion

        #region 构造函数
        
        /// <summary>
        /// 构造重载
        /// </summary>
        /// <param name="nDG"></param>
        /// <param name="bVIZ"></param>
        /// <param name="nSaveImage"></param>
        public ReadPassport(string sDG, bool bVIZ, string sSaveImage)
        {
            try
            {
                m_isNeedCapture = true;
                m_ThreadRunStatus = false;
                //fileOper = new FileOperator();
                m_ThreadHandle = new Thread(AutoClassAndRecognize);
                m_ThreadHandle.IsBackground = true;
                m_isPassportRead = false;
                fileOper = new FileOperator();

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
                this.bRecogVIZ = bVIZ;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("实例化 ReadPassport异常:" + ex.Message);
            }
        }

        

        #endregion

        #region 字段设置与读取
        ///// <summary>
        ///// 设置要读取的DG
        ///// </summary>
        ///// <param name="nDG">nDG的取值组合：例如：1|2|3|4</param>
        //public void SetDGGroup(string sDG)
        //{
        //    try
        //    {
        //        string[] sArray = sDG.Split('|');
        //        foreach (string i in sArray)
        //        {
        //            this.nDGGroup |= (1 << (int.Parse(i) - 1));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        ///// <summary>
        ///// 设置要读取的RecogVIZ
        ///// </summary>
        ///// <param name="bVIZ"></param>
        //public void SetRecogVIZ(bool bVIZ)
        //{
        //    this.bRecogVIZ = bVIZ;
        //}
        ///// <summary>
        ///// 设置要读取的SaveImageType
        ///// </summary>
        ///// <param name="sSaveImage">取值组合：例如：1|2|3|4</param>
        //public void SetSaveImageType(string sSaveImage)
        //{
        //    try
        //    {
        //        string[] sArray = sSaveImage.Split('|');
        //        foreach (string i in sArray)
        //        {
        //            this.nSaveImageType |= int.Parse(i);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        ///// <summary>
        ///// 设置用户ID
        ///// </summary>
        ///// <param name="sIpUserID"></param>
        //public  void SetUserID(string sIpUserID)
        //{
        //    this.m_sIpUserID = sIpUserID;
        //}

        ///// <summary>
        ///// 获取用户ID
        ///// </summary>
        ///// <returns></returns>
        //public  string GetUserID()
        //{
        //    return this.m_sIpUserID;
        //}

        ///// <summary>
        ///// 获取当前载入状态（是否成功）
        ///// </summary>
        ///// <returns></returns>
        //public bool GetIsIDCardLoaded()
        //{
        //    return this.m_bIsIDCardLoaded;
        //}
#endregion
        public PassportInfo GetPassportInfo()
        {
            return cPassport;
        }
        
        /// <summary>
        /// 是否打开
        /// </summary>
        public bool IsOpened
        {
            set { m_isOpened = value; }
            get { return m_isOpened; }
        }

        /// <summary>
        /// 是否需要读卡
        /// </summary>
        public  bool IsNeedCapture
        {
            get { return m_isNeedCapture; }
            set { m_isNeedCapture = value; }
        }

        /// <summary>
        /// 读卡器名称
        /// </summary>
        public  string Name { set; get; }

        private static ReadCardDelegate readCardComplete;
        /// <summary>
        /// 完成读卡事件
        /// </summary>
        public event ReadCardDelegate OnReadCardEvent
        {
            add { readCardComplete += new ReadCardDelegate(value); }
            remove { readCardComplete -= new ReadCardDelegate(value); }
        }

        #region 读卡器初始化
        /// <summary>
        /// 读卡器初始化
        /// </summary>
        /// <returns></returns>
        public bool Init()
        {
            InitPassPort();
            return this.m_bIsIDCardLoaded;
        }

        /// <summary>
        /// 初始化护照读卡器
        /// </summary>
        /// <returns></returns>
        public string InitPassPort()
        {
            try
            {
                string Result = "";
                //检查Dll是否已经载入
                if (m_bIsIDCardLoaded)
                {
                    Result = "初始化成功";
                    return Result;
                }

                #region 载入DLL

                //护照识别
                int nRet;
                nRet = LoadLibrary(@".\device\Passport\IDCard.dll");
                if (nRet == 0)
                {
                    Trace.WriteLine("IDCard.Dll加载失败");
                    Result = "IDCard.Dll加载失败";
                    return Result;
                }

                //身份证识别
                nRet = LoadLibrary(@"device\Passport\WltRS.dll");
                if (nRet == 0)
                {
                    Trace.WriteLine("WltRS.dll加载失败");
                    Result = "sdtapi.dll加载失败";
                    return Result;
                }
                nRet = LoadLibrary(@"device\Passport\zysdtapi.dll");
                if (nRet == 0)
                {
                    Trace.WriteLine("zysdtapi.dll加载失败");
                    Result = "sdtapi.dll加载失败";
                    return Result;
                }
                nRet = LoadLibrary(idCardDll);
                if (nRet == 0)
                {
                    Trace.WriteLine("sdtapi.dll加载失败");
                    Result = "sdtapi.dll加载失败";
                    return Result;
                }
                #endregion

                #region 身份证读取设置，获取端口号
                for (int iPort = 1001; iPort < 1017; iPort = iPort + 1)
                {
                    nRet = SDT_OpenPort(iPort);
                    if (nRet == 0x90)
                    {
                        m_nOpenPort = iPort;
                        m_bNIDapi = true;
                        break;
                    }
                }
                #endregion

                //Load engine

                //获取用户ID
                if (this.m_sIpUserID.ToString() == "")
                {
                    InitUserID();
                }

                nRet = InitIDCard(this.m_sIpUserID.ToCharArray(), 1, null);
                if (nRet != 0)
                {
                    if (nRet == 1)
                    {
                        Result = "用户ID错误";
                    }
                    if (nRet == 2)
                    {
                        Result = "设备初始化失败";
                    }
                    if (nRet == 3)
                    {
                        Result = "初始化核心失败";
                    }
                    if (nRet == 4)
                    {
                        Result = "未找到授权文件";
                    }

                    return Result;
                }

                SetSpecialAttribute(1, 1);//护照读取设置
                

                m_bIsIDCardLoaded = true;
                m_isPassportRead = false;
                m_bInited = true;
                Result = "初始化成功";

                return Result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        /// <summary>
        /// 循环读卡
        /// </summary>
        public void ReadCard()
        {
            //int nRet;
            if (!m_bInited)
            {
                Trace.WriteLine("读卡器没有初始化成功，退出");
                return;
            }
            //成功连接
            if (m_bIsIDCardLoaded)
            {
                try
                {
                    #region 定时器自动读取护照信息
                    //StartRun();//开始线程循环读取
                    this.m_ThreadHandle.Start();
                    #endregion
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 关闭读卡
        /// </summary>
        public void Close()
        {
            if (m_bInited)
            {
                m_bInited = false;
                try
                {
                    if (m_bIsIDCardLoaded)
                    {
                        StopRun();//停止线程
                        FreeIDCard();//关闭护照读卡
                        SDT_ClosePort(0);//关闭身份证读卡
                        m_bIsIDCardLoaded = false;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 初始化用户id
        /// </summary>
        private void InitUserID()
        {
            PassportUserID.CreateInstance();
            this.m_sIpUserID = PassportUserID.GetUserID();
        }

        /// <summary>
        /// 读卡
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoClassAndRecognize()
        {
            try
            {
                m_ThreadRunStatus = true;
                while (true)
                { 
                    //如果不需要采集 则直接循环
                    if (!m_isNeedCapture)
                    {
                        Thread.Sleep(50);
                        continue;
                    }
                    if (m_ThreadRunStatus == false)
                    {
                        Trace.WriteLine("循环读卡线程退出");
                        return;
                    }
                    m_isPassportRead = true;
                    cPassport = null;
                    int nRet = 0;
                    #region 护照信息读取
                    nRet = GetGrabSignalType();//判断是否采集信号触发,开始读取护照信息：0-没有，1-有
                    if (nRet == 1)
                    {
                        #region 设置当前要识别的证件类型，并将之前已经设置的证件类型清除掉

                        int[] nSubID = new int[1];
                        nSubID[0] = 0;
                        int nRet1 = SetIDCardID(nType, nSubID, 1);//返回0代表成功，其他失败，设置识别护照
                        SetIDCardID(2, nSubID, 1);//身份证照片页
                        AddIDCardID(3, nSubID, 1);//身份证签发机关页
                        AddIDCardID(13, nSubID, 1);//护照

                        #endregion
                        if (nRet1 != 0)
                        {
                            Trace.WriteLine("证件类别设置失败");
                            cPassport = null;
                            Thread.Sleep(50);
                            continue;
                            //Thread.Sleep(100);
                            //continue;
                        }
                        #region 证件分类：对即将识别的证件自动分类
                        int ncardType = 0;
                        nRet = ClassifyIDCard(ref ncardType);
                        string filePath = Directory.GetCurrentDirectory();

                        if (nRet < 0)
                        {
                            Trace.WriteLine("证件分类获取失败");
                            cPassport = null;
                            iType = -1;
                        }
                        #endregion

                        #region 证件识别
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
                            //nRet = RecogCommonCard(nSaveImageType);
                            Thread.Sleep(50);
                            continue;
                        }
                        if (nRet < 0)//识别失败
                        {
                            Trace.WriteLine("证件识别失败");
                            cPassport = null;
                            iType = -1;
                            //continue;
                        }

                        #endregion

                        if (nRet > 0)
                        {
                            iType = 2;
                            bIsReadPassport = true;
                            bIsReadIDCard = false;
                            cPassport = new PassportInfo();
                            cPassport.CardType = "2";
                            #region 识别成功,取得识别结果
                            int MAX_CH_NUM = 128;
                            char[] cArrFieldValue = new char[MAX_CH_NUM];
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
                    #endregion


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
                    if (OnReadComplete != null && ((iType==0)||(iType==2)||(iType==-1)))
                    {
                        OnReadComplete(cPassport, 2);
                    }
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

                    Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                m_ThreadRunStatus = false;
                Trace.WriteLine(ex);
                throw ex;

            }
        }
        
        /// <summary>
        /// 保存图片
        /// </summary>
        private bool SaveImages()
        {
            if (iType == 2)//读取护照信息
            {
                if (!m_bInited)
                {
                    return false;
                }
                try
                {
                    if (!m_bIsIDCardLoaded)
                    {
                        return false;
                    }
                    if (!m_isPassportRead)
                    {
                        return false;
                    }
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
                        m_isPassportRead = false;
                        return true;
                    }

                    string photoPath = filePath + "\\zp.wlt";
                    int j = SaveHeadImage(jpegImage.ToCharArray());//保存护照中头像信息
                    string sPathPhoto = filePath + "\\zp.bmp";
                    int i = SaveHeadImage(sPathPhoto.ToCharArray());
                    File.Copy(filePath + "\\zp.bmp", filePath + "\\zp_temp.bmp", true);

                    using (Bitmap bmp = new Bitmap(filePath + "\\zp_temp.bmp"))
                    {
                        bmp.Save(jpegImage, ImageFormat.Jpeg);
                        bmp.Dispose();
                    }

                    m_isPassportRead = false;
                    cPassport.PhotoHead = jpegImage;

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                    throw ex;
                }
            }
            if(iType == 0)//读取身份证信息
            {
                if (!m_bInited && !m_bNIDapi)
                {
                    return false;
                }
                try
                {
                    if (!m_bIsIDCardLoaded)
                    {
                        return false;
                    }
                    if (!m_isPassportRead)
                    {
                        return false;
                    }
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
                        m_isPassportRead = false;
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
                    
                    File.Copy(path, filePath + "\\zp.bmp",true);//复制头像信息到当前目录下

                    File.Copy(filePath + "\\zp.bmp", filePath + "\\zp_temp.bmp", true);

                    using (Bitmap bmp = new Bitmap(filePath + "\\zp_temp.bmp"))
                    {
                        bmp.Save(jpegImage, ImageFormat.Jpeg);
                        bmp.Dispose();
                    }

                    m_isPassportRead = false;
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
            if (m_bInited)
            {
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
            }
        }

        /// <summary>
        /// 文件数据初始化
        /// </summary>
        /// <returns></returns>
        public bool InitFileData()
        {
            try
            {
                Trace.WriteLine("文件数据初始化...");
                bool bRet = false;
                if (!fileOper.BInited)
                {
                    bRet = fileOper.InitFileOper();
                    if (!bRet)
                        return bRet;
                }
                GetPassportLists();
                Trace.WriteLine("文件数据初始化完成");
                return bRet;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("文件数据初始化异常:" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 启动读卡
        /// </summary>
        public void StartRun()
        {
            try
            {
                m_ThreadHandle.Start();
            }
            catch
            {
            }
        }

        /// <summary>
        /// 停止运行
        /// </summary>
        private void StopRun()
        {
            try
            {
                Trace.WriteLine("开始停止读卡...");
                m_ThreadRunStatus = false;
                Thread.Sleep(500);
                if (!m_ThreadHandle.Join(10))
                {
                    m_ThreadHandle.Abort();
                }
                Close();
                Trace.WriteLine("停止读卡完成");
            }
            catch (Exception ex)
            {
                Trace.WriteLine("停止运行异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 填充证件信息到string中
        /// </summary>
        /// <param name="info"></param>
        private void PushCardInfo()
        {
            if (m_bInited)
            {
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
            }
        }

        /// <summary>
        /// 获取护照信息列表
        /// </summary>
        private void GetPassportLists()
        {
            try
            {
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                watch.Start();  //开始监视代码运行时间
                if (fileOper.BInited)
                {
                    fileOper.ReadPassportInfo(out strCardNumLists);
                }
                watch.Stop();  //停止监视
                TimeSpan timespan = watch.Elapsed;  //获取当前实例测量得出的总时间
            }
            catch (Exception ex)
            {
                Trace.WriteLine("GetCardLists异常:" + ex.Message);
            }
        }

        ///// <summary>
        ///// 返回当前护照对象信息
        ///// </summary>
        ///// <returns></returns>
        //public PassportInfo GetPassportRead()
        //{
        //    return cPassport;
        //}

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
