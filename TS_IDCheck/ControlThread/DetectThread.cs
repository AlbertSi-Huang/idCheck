using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Drawing;
using System.IO;
using Common;
using System.Drawing.Imaging;
using System.Diagnostics;
using Newtonsoft.Json;
using LibExchangePlate;
using System.Runtime.InteropServices;

namespace TS_IDCheck
{
    public class SDetectRecord
    {
        public SCardInfo _card { set; get; }
        public PassportInfo _passportCard { set; get; }
        public string _cardNum { set; get; }
        public string _createTime { set; get; }
        public string _detectScore { set; get; }
        public int _detectResult { set; get; }
        public string _siteImage { set; get; }
        public int _updateState { set; get; }
        public string ticketInfo { set; get; }

        public SDetectRecord()
        {
            _card = new SCardInfo();
            _passportCard = new PassportInfo();
            _cardNum = "";
            _createTime = "";
            _detectScore = "";
            _detectResult = -1;
            _siteImage = "";
            _updateState = -1;
            ticketInfo = "";

        }
    }


    /// <summary>
    /// 人脸比对
    /// </summary>
    public class DetectThread : BaseThread
    {
        /// <summary>
        /// 线程名称
        /// </summary>
        private string ThreadName = "【DetectThread】";

        
        GFaceRecognizer m_gface = GFaceRecognizer.GetInstance();
        private SCardInfo m_cardData;
        private PassportInfo m_PassportData;
        private byte[] m_rgb24;
        private RECT[] m_rect;
        private int m_width, m_height;
        private FacePointInfo[] m_facePoint;
        private int m_nIndex = 0;
        private byte[] m_fea;
        Bitmap m_bm;
        private string strSavePath;
        private ComOpenDoor _openDoorOperator;
        private string _strFilePath = Directory.GetCurrentDirectory();
        
        private PlaySoundThread playSoundThread = new PlaySoundThread();

        /// <summary>
        /// 
        /// </summary>
        DetectThreadHelper _detectInline = new DetectThreadHelper();
        /// <summary>
        /// 特征值获取完成事件
        /// </summary>
        private static DetectCompleteDelegate onDetectComplete;
        /// <summary>
        /// 人脸比对完成事件
        /// </summary>
        public event DetectCompleteDelegate detectCompleteEvent
        {
            add { onDetectComplete += new DetectCompleteDelegate(value); }
            remove { onDetectComplete -= new DetectCompleteDelegate(value); }
        }

        private SaveRecordFile _srfile = new SaveRecordFile();

        public DetectThread()
        {
            m_cardData = new SCardInfo();
            strSavePath = ConfigOperator.Single().StrSavePath + @"\siteImg\";
            _srfile.CreateDirAndFile();
        }

        /// <summary>
        /// 构造重载
        /// </summary>
        /// <param name="nType">卡类型</param>
        public DetectThread(string nType)
        {
            m_cardData = new SCardInfo();
            m_PassportData = new PassportInfo();
            strSavePath = ConfigOperator.Single().StrSavePath + @"\siteImg\";
            _srfile.CreateDirAndFile();

        }

        /// <summary>
        /// 0 身份证 1 ic卡 2护照
        /// </summary>
        int _nCardType = 0;

        /// <summary>
        /// 保存护照信息
        /// </summary>
        /// <param name="data"></param>
        public void SetCardData(object data,int nType)
        {
            _nCardType = nType;
            m_cardData._idNum = "";

            m_cardData.ClearData();
            m_PassportData.Clear();
            if (nType==2)//护照
            {
                m_PassportData.copyData((PassportInfo)data);
                Trace.WriteLine("护照信息：" + m_PassportData.MRZ+",photo="+m_PassportData.PhotoHead);
            }else if(nType == 0)
            {
                m_cardData.CopyData((SCardInfo)data);
                Trace.WriteLine("身份证信息：" + m_cardData._idNum);
            }
            
        }

        object lockObj = new object();

        public void SetCurrentInfo(Bitmap bm, byte[] rbg24, int w, int h, FacePointInfo[] facePoint, RECT[] rect, int nIndex)
        {
            lock (lockObj)
            {
                m_bm = bm;
                m_rgb24 = null;
                m_rgb24 = rbg24;
                m_width = w;
                m_height = h;
                m_facePoint = facePoint;
                m_rect = rect;
                m_nIndex = nIndex;
            }
            
        }

        private int runNum = 0;

        /// <summary>
        /// 
        /// </summary>
        public int RunNum
        {
            set { this.runNum = value; }
            get { return runNum; }
        }

        /// <summary>
        /// 提取人脸特征值
        /// </summary>
        /// <param name="detectRes"></param>
        /// <returns></returns>
        private bool ModeDetect(out float detectRes)
        {
            detectRes = 0.0f;
            try
            {
                System.Drawing.Bitmap bmCard = null;
                if (_nCardType == 0)//身份证
                {
                    bmCard = m_gface.ReadImageFile(m_cardData._photo);
                }
                if (_nCardType == 2)//护照
                {
                    bmCard = m_gface.ReadImageFile(m_PassportData.PhotoHead);
                }
                if(bmCard == null)
                {
                    Trace.WriteLine("证件照为空,返回0");
                    return false;
                }
                byte[] rgb8;
                byte[] rgb24;
                byte[] fea;
                Trace.WriteLine("获取证件照特征值");
                m_gface.GetRgb8And24Buf(bmCard, out rgb8, out rgb24);

                RECT[] rect = new RECT[1024];
                FacePointInfo[] facePoint = new FacePointInfo[1024];
                int nCardCount =  m_gface.Detect(0, rgb8, bmCard.Width, bmCard.Height, out rect, out facePoint);
                if (nCardCount < 1)
                {
                    //Console.WriteLine("身份证照片未找到头像");
                    detectRes = 0.0f;
                    return false;
                }

                Trace.WriteLine(ThreadName + "timeRecode:---------算法比对提取特征值开始---------");
                m_gface.GetFea(0, rgb24, bmCard.Width, bmCard.Height, facePoint[0], out fea);
                m_gface.GetFea(0, m_rgb24, m_width, m_height, m_facePoint[m_nIndex], out m_fea);
                Trace.WriteLine(ThreadName + "timeRecode:---------算法比对提取特征值结束---------");
                detectRes = m_gface.Compare(0, fea, m_fea);

                return true;
            }
            catch (Exception ex)
            {
                Trace.Write(ThreadName + "算法比对提取特征值线程异常:" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 开门操作对象赋值
        /// </summary>
        /// <param name="comOpenDoor"></param>
        public void SetOpenDoorOperator(ComOpenDoor comOpenDoor)
        {
            _openDoorOperator = comOpenDoor;
        }

        static string _jobId = null;
        private string _jobIdT = string.Empty;

        

        /// <summary>
        /// 查找黑名单是否正确返回
        /// </summary>
        bool _bGetBlackInfo = false;
        int _nRetGetTicketSuccess = 0;
        ShowBlackWindows _blackTip = new ShowBlackWindows();
        
        
        /// <summary>
        /// 人脸比对线程
        /// </summary>
        public override void Run()
        {
            _sqliteOper.DataBaseInit();
            _historyRecord.SetSqlite(_sqliteOper);
            _historyRecord.Start();
            Trace.WriteLine(ThreadName + "Run()启动");
            playSoundThread.Start();
            _blackTip.Start();
            while (IsAlive)
            {
                if (runNum <= 0)
                {
                    Thread.Sleep(100);
                    continue;
                }
                int nManType = 0; //0 陌生人 1 黑名单

                #region 黑名单查找
                Trace.WriteLine("timeRecode:---------黑名单查找---------"+ _nCardType+","+ m_cardData._idNum);
                _jobId = "";

                string strQueryNum = m_cardData._idNum;
                if(_nCardType == 2)
                {
                    if (m_cardData == null || m_cardData._idNum == null || m_cardData._idNum.Length != 18)
                    {
                        if (m_PassportData != null && m_PassportData.PassportNo != null && m_PassportData.PassportNo.Length != 0)
                        {
                            strQueryNum = m_PassportData.PassportNo;
                        }
                        else if (m_PassportData != null && m_PassportData.MRZ != null && m_PassportData.MRZ.Length != 0)
                        {
                            strQueryNum = m_PassportData.MRZ;
                        }
                    }
                }
                
                if (strQueryNum.Length == 0)
                {
                    Trace.WriteLine("msg.cardNo = " + m_cardData._idNum);
                    Trace.WriteLine("m_PassportData.PassportNo = " + m_PassportData.PassportNo + " m_PassportData.MRZ = " + m_PassportData.MRZ);
                    onDetectComplete(0, null, "");
                    playSoundThread.SetPlay(true, 5);
                    RunNum--;
                    Thread.Sleep(10);
                    continue;
                }
                Trace.WriteLine("准备查找黑名单 " + strQueryNum);
                string strQueryBack = _detectInline.QueryBlack(strQueryNum);
                if (strQueryBack != null && (strQueryBack.IndexOf("-93") != -1 ))
                {
                    onDetectComplete(0, null, "");
                    playSoundThread.SetPlay(true, 5);
                    RunNum--;
                    Thread.Sleep(10);
                    continue;
                }

                if(ConfigOperator.Single().StrictGrade == 2)
                {
                    if(strQueryBack.IndexOf("-100") != -1 || strQueryBack.Length == 0)
                    {
                        onDetectComplete(0, null, "");
                        playSoundThread.SetPlay(true, 5);
                        RunNum--;
                        Thread.Sleep(10);
                        continue;
                    }
                }
                Trace.WriteLine("黑名单查找错误返回:" + strQueryBack);
                if ((strQueryBack != null) && (strQueryBack.IndexOf("isBlackList") != -1))
                {
                    ThirdBlackQurryReturn mdc = JsonConvert.DeserializeObject<ThirdBlackQurryReturn>(strQueryBack);
                    nManType = mdc.isBlackList;
                    Trace.WriteLine("jobId get");
                    Trace.WriteLine(mdc.jobId);
                    _jobId = mdc.jobId;
                    _bGetBlackInfo = true;
                    Trace.WriteLine("黑名单返回反序列化后的结果：" + nManType);
                }
                else
                {
                    _bGetBlackInfo = false;
                }
                if (ConfigOperator.Single().PlateChoose == 4)
                {
                    ThirdBlackQurryReturn tbqr = _detectInline.QueryArmPlateBlack(strQueryNum);
                    nManType = tbqr.isBlackList;
                    _jobIdT = tbqr.jobId;

                    if (tbqr != null && (tbqr.status.CompareTo("-93") == 0))
                    {
                        onDetectComplete(0, null, "");
                        playSoundThread.SetPlay(true, 5);
                        RunNum--;
                        Thread.Sleep(10);
                        continue;
                    }

                    if (ConfigOperator.Single().StrictGrade == 2)
                    {
                        if (tbqr.status.CompareTo("-100") == 0 || strQueryBack.Length == 0)
                        {
                            onDetectComplete(0, null, "");
                            playSoundThread.SetPlay(true, 5);
                            RunNum--;
                            Thread.Sleep(10);
                            continue;
                        }
                    }

                }

#if DEBUG
                //nManType = 1;
#endif
                if(nManType == 1)
                {
                    Trace.WriteLine(strQueryNum + " 是黑名单 ");
                    _blackTip.SetShow(true);
                }
#endregion
                Trace.WriteLine(ThreadName + "timeRecode:---------正在进入人脸比对---------");

                bool bLocalMan = false;
                string strLocalCard = "";
                if (_nCardType == 0)//身份证
                {
                    strLocalCard = m_cardData._idNum.Substring(0, ConfigOperator.Single().LocalCard.Length);
                }
                if (_nCardType == 2)//护照
                {
                    strLocalCard = m_PassportData.PassportNo.Substring(0, ConfigOperator.Single().LocalCard.Length);
                }

                bLocalMan = (strLocalCard.CompareTo(ConfigOperator.Single().LocalCard) == 0);

                float ff = 0;
                //int nDetectRes = 2; 
                EDetectResult nDetectRes = EDetectResult.EDR_FAILED;
                lock (lockObj)
                {
                    ModeDetect(out ff);
                }
                //记录比对分数
                Trace.WriteLine(ThreadName + "GFace7 比对完成 比对分数为：" + ff);
                
                Trace.WriteLine(ThreadName + "timeRecode:---------人脸比对完成---------");
                string strGetTickMsg = "", strInfoMsg = "";
                _nRetGetTicketSuccess =  _detectInline.IsCheckTicketSuccess(_nCardType == 0 ? m_cardData._idNum : (_nCardType == 2 ? m_PassportData.PassportNo : "123456"), 
                    out strGetTickMsg,out strInfoMsg);
                string dateInvalid = m_cardData._dateEnd;
                if (dateInvalid.Length == 0)
                {
                    dateInvalid = m_PassportData.DateOfExpiry;
                    if(dateInvalid == null || dateInvalid.Length == 0)
                    {
                        dateInvalid = m_PassportData.DateOfExpiryOCR;
                    }

                }

                if (dateInvalid.Length == 8)
                {
                    string strTmp = dateInvalid;
                    dateInvalid = strTmp.Substring(0, 4) + "-" + strTmp.Substring(4, 2) + "-" + strTmp.Substring(6, 2);
                }
                
                nDetectRes = _detectInline.GetDetectResult(ff, nManType,_nRetGetTicketSuccess, dateInvalid);

                playSoundThread.SetPlay(true, (int)nDetectRes);
                
                DateTime dtDetectComplete = DateTime.Now;
                try
                {
                    //现场图片保存
                    int nLeft = 0, nTop = 0, nGetX = 0, nGetY = 0;
                    int nWidth = m_rect[m_nIndex].right - m_rect[m_nIndex].left;
                    int nHeight = m_rect[m_nIndex].bottom - m_rect[m_nIndex].top;
                    bool bWidthBig = false;
                    //这个计算比较蛋疼  需要考虑各种边界问题 暂时用以下计算顶着用 hsx20171015
                    if (nWidth > 350)
                    {
                        nLeft = m_rect[m_nIndex].left + (m_rect[m_nIndex].right - m_rect[m_nIndex].left - 350) / 2;
                        nLeft = nLeft > 0 ? nLeft : 0;
                        nGetX = 350;
                        nGetX = nLeft + nGetX < m_bm.Width ? nGetX : m_bm.Width - nLeft;
                        nTop = m_rect[m_nIndex].top - 60 > 0 ? m_rect[m_nIndex].top - 60 : 0;
                        nGetY = nGetX / 3 * 4;
                        nGetY = m_bm.Height > nTop + nGetY ? nGetY : m_bm.Height - nTop;
                        bWidthBig = true;
                    }
                    else if (nWidth < 350)
                    {
                        nLeft = m_rect[m_nIndex].left - 20 > 0 ? m_rect[m_nIndex].left - 20 : 0;
                        nTop = m_rect[m_nIndex].top - 60 > 0 ? m_rect[m_nIndex].top - 60 : 0;
                        nGetX = nLeft + nWidth + 40 > m_bm.Width ? m_bm.Width - nLeft : nWidth + 40;
                        nHeight = nGetX / 3 * 4;
                        nGetY = nTop + nHeight > m_bm.Height ? m_bm.Height - nTop : nHeight;
                    }
                    nLeft = nLeft > 0 ? nLeft : 0;
                    nTop = nTop > 0 ? nTop : 0;
                    nGetX = m_bm.Width - nLeft > nGetX ? nGetX : m_bm.Width - nLeft;
                    nGetY = m_bm.Height - nTop > nGetY ? nGetY : m_bm.Height - nTop;
                    Trace.WriteLine(ThreadName + "timeRecord 剪切保存照片");
                    Bitmap cutBm = null;
                    lock (lockObj)
                    {
                        if (nGetX > 0 && nGetY > 0)
                        {
                            cutBm = ImageDealOper.CutImageByMem(m_bm, nLeft, nTop, nGetX, nGetY);

                            if (bWidthBig)
                            {
                                cutBm = (Bitmap)ImageDealOper.GetThumb((Image)cutBm, 351, 468);
                            }
                        }
                    }
                    string strName = dtDetectComplete.ToString("yyyyMMddHHmmss");
                    string strImgFormat = ".jpg";

                    //比对完成事件
                    Trace.WriteLine(ThreadName + "触发 比对完成事件");

                    string strCardPhoto = m_cardData._photo;
                    if (_nCardType == 2)//护照
                    {
                        strCardPhoto = m_PassportData.PhotoHead;
                    }
                    onDetectComplete((int)nDetectRes, cutBm, strCardPhoto);
                    
                    //比对成功，开门
                    if (nDetectRes == EDetectResult.EDR_SUCCESS || nDetectRes == EDetectResult.EDR_TICKETCHECKED)
                    {
                        try
                        {
                            _openDoorOperator.WriteCom();
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.Message);
                        }
                    }
                    if (cutBm != null)
                        cutBm.Save(strSavePath + strName + strImgFormat, ImageFormat.Jpeg);
                    if (_nCardType == 0)//身份证
                    {
                        SDetectRecord sdr = new SDetectRecord();
                        // 保存至文件 
                        sdr._cardNum = m_cardData._idNum;
                        sdr._createTime = dtDetectComplete.ToString("yyyy-MM-dd HH:mm:ss");
                        sdr._detectResult = (int)nDetectRes;
                        sdr._siteImage = strSavePath + strName + strImgFormat;
                        int iff = (int)(ff + 0.5);
                        sdr._detectScore = iff.ToString();// ff.ToString();
                        sdr._updateState = 2;
                        sdr._card = m_cardData;
                        PassportInfo pInfo = new PassportInfo();
                        pInfo.CardType = "0";
                        pInfo.Name = m_cardData._name;
                        pInfo.Sex = m_cardData._sex;
                        pInfo.Nation = m_cardData._nation;
                        pInfo.DateOfBirth = m_cardData._birthday;
                        pInfo.PlaceOfBirth = m_cardData._address;
                        pInfo.PassportNo = m_cardData._idNum;
                        pInfo.AuthorityOCR = m_cardData._issure;
                        pInfo.DateOfIssue = m_cardData._dateStart;
                        pInfo.DateOfExpiry = m_cardData._dateEnd;
                        sdr._passportCard = new PassportInfo();
                        sdr._passportCard = pInfo;
                        sdr.ticketInfo = strInfoMsg;

                        //上传比对结果 
                        //  if (dbscclient.Single().IsConnected())
                        m_bm.Save("uploadScene.jpg");
#region 保存文件
                        Trace.WriteLine("保存文件");
                        if (cutBm != null)
                            _srfile.SaveFile(m_bm, cutBm, m_gface.ReadImageFile(m_cardData._photo), sdr, nManType);
#endregion
                    }
                    else if (_nCardType == 2)//护照
                    {
                        SDetectRecord sdr = new SDetectRecord();
                        // 保存至文件 
                        sdr._cardNum = m_PassportData.PassportNo;
                        if (sdr._cardNum.Length < 1) sdr._cardNum = m_PassportData.PersonalIdNo;
                        sdr._createTime = dtDetectComplete.ToString("yyyy-MM-dd HH:mm:ss");
                        sdr._detectResult = (int)nDetectRes;
                        sdr._siteImage = strSavePath + strName + strImgFormat;
                        int iff = (int)(ff + 0.5);
                        sdr._detectScore = iff.ToString();
                        sdr._updateState = 2;
                        sdr._passportCard = m_PassportData;
                        sdr.ticketInfo = strInfoMsg;
                        //上传比对结果 
                        m_bm.Save("uploadScene.jpg");
#region 保存文件
                        Trace.WriteLine("保存文件");
                        _srfile.SaveFile(m_bm, cutBm, m_gface.ReadImageFile(m_PassportData.PhotoHead), sdr, nManType);
#endregion
                    }

                    //保存
                    SavePlateRecord(ff, nDetectRes.ToString(), 0, strSavePath + strName + strImgFormat, dtDetectComplete.ToString("yyyy-MM-dd HH:mm:ss"));
                    
                    //上传比对结果，启动异步线程
                    UpLoadInfo sendhandle = new UpLoadInfo();
                    sendhandle.dtDetectComplete = dtDetectComplete;
                    sendhandle.ff = ff;
                    sendhandle.nDetectRes = (nDetectRes == EDetectResult.EDR_SUCCESS ? 1 : 2);
                    sendhandle.faceCompareReslut = (nDetectRes == EDetectResult.EDR_SUCCESS ? 1 : 2).ToString();//ff > fConfigScore ? "1" : "2";
                    if (nManType == 1)
                    {
                        sendhandle.nDetectRes = 2;
                    }
                    sendhandle._srfile = _srfile;

                    Thread th = new Thread(Thread_UpLoadFile);
                    th.Start((object)sendhandle);
                    if (cutBm != null)
                        cutBm.Dispose();
                    RunNum--;
                    Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    RunNum--;
                    Thread.Sleep(10);
                    Trace.WriteLine(ThreadName + "人脸比对线程异常:" + ex.Message);
                }

            }
            playSoundThread.Stop(0);
            Trace.WriteLine(ThreadName + "Run()停止");
        }

        /// <summary>
        /// 上传文件信息类
        /// </summary>
        private class UpLoadInfo
        {
            /// <summary>
            /// 比对时间
            /// </summary>
            public DateTime dtDetectComplete { get; set; }

            /// <summary>
            /// 比对分值
            /// </summary>
            public float ff { get; set; }

            /// <summary>
            /// 比对结果
            /// </summary>
            public int nDetectRes { get; set; }

            public string faceCompareReslut { set; get; }

            /// <summary>
            /// 文件句柄
            /// </summary>
            public SaveRecordFile _srfile { get; set; }
        }
        private PlateExchangeAbstract[] _plateExchange = null;
        public void SetPlate(PlateExchangeAbstract[] plate)
        {
            _plateExchange = plate;
        }

        /// <summary>
        /// 异步上传图片信息
        /// </summary>
        /// <param name="temp"></param>
        private void Thread_UpLoadFile(object temp)
        {
            string strSavePath = ConfigOperator.Single().StrSavePath;

            try
            {
                Trace.WriteLine(ThreadName + "开始异步上传图片信息...");
                UpLoadInfo handle = (UpLoadInfo)temp;

                string uploadCardFileName = string.Empty;
                string uploadSceneFileName = string.Empty;
                string uploadSiteFileName = string.Empty;
                string yy = string.Empty;

                uploadCardFileName = handle._srfile.GetCurCardPath();
                if(File.Exists(uploadCardFileName))
                    yy = SanTranProto.Single().ThirdUpLoadFile(uploadCardFileName);
                if ((yy != null) && (yy.IndexOf("FilePath") != -1))
                {
                    FileUploadReturn kk = JsonConvert.DeserializeObject<FileUploadReturn>(yy);
                    uploadCardFileName = kk.FilePath;
                    //bUploadCardImg = true;
                    Trace.WriteLine(ThreadName + "Card上传成功");
                }else
                {
                    string strUid = Guid.NewGuid().ToString("N");
                    File.Copy(uploadCardFileName, strSavePath + "\\history\\" + strUid + ".jpg");
                    uploadCardFileName = strSavePath + "\\history\\" + strUid + ".jpg";
                }
                yy = string.Empty;
                uploadSceneFileName = _srfile.GetCurScenePath();
                if(File.Exists(uploadSceneFileName))
                    yy = SanTranProto.Single().ThirdUpLoadFile(uploadSceneFileName);
                if ((yy != null) && (yy.IndexOf("FilePath") != -1))
                {
                    FileUploadReturn kk = JsonConvert.DeserializeObject<FileUploadReturn>(yy);
                    uploadSceneFileName = kk.FilePath;
                    //bUploadSceneImg = true;
                    Trace.WriteLine(ThreadName + "Scene上传成功");
                }else
                {
                    string strUid = Guid.NewGuid().ToString("N");
                    File.Copy(uploadSceneFileName, strSavePath + "\\history\\" + strUid + ".jpg");
                    uploadSceneFileName = strSavePath + "\\history\\" + strUid + ".jpg";
                }
                yy = string.Empty;
                uploadSiteFileName = _srfile.GetCurSitePath();
                if (File.Exists(uploadSiteFileName))
                    yy = SanTranProto.Single().ThirdUpLoadFile(uploadSiteFileName);
                if ((yy != null) && (yy.IndexOf("FilePath") != -1))
                {
                    FileUploadReturn kk = JsonConvert.DeserializeObject<FileUploadReturn>(yy);
                    uploadSiteFileName = kk.FilePath;
                    Trace.WriteLine(ThreadName + "Site上传成功");

                    if(ConfigOperator.Single().nIsDoubleScreen == 1)
                    {
                        string mystr = ((int)(dbscclient.SendToScreenType.SendUrlCommitSucc)).ToString();
                        if (!_bGetBlackInfo)
                        {
                            mystr = ((int)dbscclient.SendToScreenType.SendUrlCommitSuccNoNet).ToString();
                        }
                        dbscclient.Single().ClientSendMsg(mystr + ",3");
                    }
                }
                else
                {
                    if(ConfigOperator.Single().nIsDoubleScreen == 1)
                    {
                        string mystr = ((int)(dbscclient.SendToScreenType.SendUrlCommitFail)).ToString();
                        if (!_bGetBlackInfo)
                        {
                            mystr = ((int)dbscclient.SendToScreenType.SendUrlCommitFailNoNet).ToString();
                        }
                        dbscclient.Single().ClientSendMsg(mystr + ",3");
                    }
                    string strUid = Guid.NewGuid().ToString("N");
                    File.Copy(uploadSiteFileName, strSavePath + "\\history\\" + strUid + ".jpg");
                    uploadSiteFileName = strSavePath + "\\history\\" + strUid + ".jpg";
                }

                Trace.WriteLine(ThreadName + "上传比对结果");
                PersonInfoUpload pupload = new PersonInfoUpload();
                string sTken = ConfigOperator.Single().StrThreeToken;
                
                pupload.token = sTken;
                pupload.deviceCode = ConfigOperator.Single().StrThreeDeviceCode;
                Trace.WriteLine("上传JobID" + _jobId);
                pupload.blackQueryJobId = _jobId;
                pupload.verifyResult = handle.nDetectRes;
                Trace.WriteLine("上传的比对结果："+pupload.verifyResult.ToString());
                pupload.collectionType = 1;
                pupload.passNo = "0";

                if (_nCardType == 0)
                {
                    #region 身份证
                    pupload.name = m_cardData._name;
                    Trace.WriteLine("Name:");
                    Trace.WriteLine(pupload.name);
                    pupload.cardNo = m_cardData._idNum;
                    string strBir = m_cardData._birthday;
                    strBir = strBir.Replace("年", "");
                    strBir = strBir.Replace("月", "");
                    strBir = strBir.Replace("日", "");
                    pupload.birthday = strBir;
                    pupload.addr = m_cardData._address;
                    pupload.genderCode = m_cardData._sex.CompareTo("男") == 0 ? 1 : 2;
                    pupload.cardType = 1;
                    pupload.inOrOut = Convert.ToInt32(ConfigOperator.Single().NInOut);
                    pupload.nationality = "中国";
                    pupload.nationCode = Common.TS_Helper.GetNationCode(m_cardData._nation);
                    pupload.cardStartTime = m_cardData._dateStart;
                    pupload.cardEndTime = m_cardData._dateEnd;
                    pupload.issuingUnit = m_cardData._issure;
                    pupload.faceCompareScore = float.Parse(handle.ff.ToString("f2"));

                    pupload.cardImgPath = uploadCardFileName;///.Substring(3, uploadCardFileName.Length - 3);
                    pupload.sceneImgPath = uploadSceneFileName;//.Substring(3, uploadSceneFileName.Length - 3);
                    pupload.faceImgPath = uploadSiteFileName;//.Substring(3, uploadSiteFileName.Length - 3);

                    pupload.passTime = handle.dtDetectComplete.ToString("yyyy-MM-dd HH:mm:ss");
                    pupload.verifyResult = handle.nDetectRes;
                    pupload.faceCompareResult = handle.faceCompareReslut;
                    pupload.faceQualityScore = "0.0";

                    string strRet = SanTranProto.Single().ThirdUpLoad(pupload);

                    Trace.WriteLine("上传返回数据：" + strRet);
                    if (UploadSuccess(strRet))
                    {
                        //上传成功
                    }
                    else
                    {
                        SCardInfo cardInfo = new SCardInfo();
                        bool bReadCard = _sqliteOper.DatabaseReadCardInfo(m_cardData._idNum, ref cardInfo);
                        if (!bReadCard)
                        {
                            cardInfo._idNum = m_cardData._idNum;
                            cardInfo._birthday = strBir;
                            cardInfo._name = m_cardData._name;
                            cardInfo._sex = pupload.genderCode.ToString();
                            cardInfo._address = pupload.addr;
                            cardInfo._dateStart = m_cardData._dateStart;
                            cardInfo._dateEnd = m_cardData._dateEnd;
                            cardInfo._nation = pupload.nationCode;
                            cardInfo._issure = m_cardData._issure;
                            string[] ss = { m_cardData._idNum , m_cardData._name , pupload.genderCode.ToString() ,
                            strBir,pupload.nationCode,m_cardData._issure,m_cardData._address,m_cardData._dateStart,m_cardData._dateEnd};
                            _sqliteOper.DatabaseInsert("idCardInfo", ss);
                        }
                        
                        string[] ssRecord =
                        {
                            m_cardData._idNum,pupload.passTime,_jobId,pupload.faceCompareScore.ToString(),pupload.faceCompareResult.ToString(),
                            uploadCardFileName,uploadSceneFileName,uploadSiteFileName,pupload.verifyResult.ToString(),"0",""
                        };
                        _sqliteOper.DatabaseInsert("idCardRecord", ssRecord);
                    }
                    #endregion
                }

                if (_nCardType == 2)//护照
                {
                    pupload.name = m_PassportData.Name;
                    Trace.WriteLine("Name:" + pupload.name);

                    pupload.cardNo = m_PassportData.PassportNo;
                    if(pupload.cardNo.Length < 1)
                    {
                        pupload.cardNo = m_PassportData.PersonalIdNo;
                    }
                    string strBir = m_PassportData.DateOfBirth;
                    strBir = strBir.Replace("年", "");
                    strBir = strBir.Replace("月", "");
                    strBir = strBir.Replace("日", "");
                    pupload.birthday = strBir;
                    pupload.addr = m_PassportData.PlaceOfBirth;
                    pupload.genderCode = m_PassportData.Sex.CompareTo("男") == 0 ? 1 : 2;
                    pupload.cardType = 2;
                    pupload.inOrOut = Convert.ToInt32(ConfigOperator.Single().NInOut);
                    pupload.extendInfo3 = m_PassportData.Nation;
                    pupload.nationality = m_PassportData.Nationality;
                    pupload.nationCode = Common.TS_Helper.GetNationCode(m_PassportData.Nation);//民族代码
                    pupload.cardStartTime = m_PassportData.DateOfIssue;
                    pupload.cardEndTime = m_PassportData.DateOfExpiry;
                    pupload.issuingUnit = m_PassportData.AuthorityOCR;
                    pupload.faceCompareScore = float.Parse(handle.ff.ToString("f2"));

                    pupload.cardImgPath = uploadCardFileName;///.Substring(3, uploadCardFileName.Length - 3);
                    pupload.sceneImgPath = uploadSceneFileName;//.Substring(3, uploadSceneFileName.Length - 3);
                    pupload.faceImgPath = uploadSiteFileName;//.Substring(3, uploadSiteFileName.Length - 3);

                    pupload.passTime = handle.dtDetectComplete.ToString("yyyy-MM-dd HH:mm:ss");
                    pupload.verifyResult = handle.nDetectRes;
                    pupload.faceCompareResult = handle.faceCompareReslut;
                    pupload.faceQualityScore = "0.0";
                    string strRet = SanTranProto.Single().ThirdUpLoad(pupload);
                }
                
                Trace.WriteLine(ThreadName + "上传比对结果完成");

                if(ConfigOperator.Single().PlateChoose == 4)
                {
                    string uploadCardFileNameLocal = handle._srfile.GetCurCardPath();
                    if(File.Exists(uploadCardFileNameLocal))
                        yy = SanTranProto.Single().ThirdUpLoadFileT(uploadCardFileNameLocal);
                    if ((yy != null) && (yy.IndexOf("FilePath") !=-1))
                    {
                        FileUploadReturn kk = JsonConvert.DeserializeObject<FileUploadReturn>(yy);
                        uploadCardFileName = kk.FilePath;
                        Trace.WriteLine(ThreadName + "arm Card上传成功");
                    }
                    else
                    {
                        uploadCardFileName = "";
                    }
                    yy = string.Empty;
                    string uploadSceneFileNameLocal = handle._srfile.GetCurScenePath();
                    if(File.Exists(uploadSceneFileNameLocal))
                        yy = SanTranProto.Single().ThirdUpLoadFileT(uploadSceneFileNameLocal);
                    if ((yy != null) && (yy.IndexOf("FilePath") != -1))
                    {
                        FileUploadReturn kk = JsonConvert.DeserializeObject<FileUploadReturn>(yy);
                        uploadSceneFileName = kk.FilePath;
                        Trace.WriteLine(ThreadName + "arm Scene上传成功");
                    }else
                    {
                        uploadSceneFileName = "";
                    }
                    yy = string.Empty;
                    string uploadSiteFileNameLocal = handle._srfile.GetCurSitePath();
                    if(File.Exists(uploadSiteFileNameLocal))
                       yy = SanTranProto.Single().ThirdUpLoadFileT(uploadSiteFileNameLocal);
                    if ((yy != null) && (yy.IndexOf("FilePath") != -1))
                    {
                        FileUploadReturn kk = JsonConvert.DeserializeObject<FileUploadReturn>(yy);
                        uploadSiteFileName = kk.FilePath;
                        Trace.WriteLine(ThreadName + "arm Site上传成功");
                    }else
                    {
                        uploadSiteFileName = "";
                    }
                    
                    Trace.WriteLine("上传JobID" + _jobIdT);
                    pupload.blackQueryJobId = _jobIdT;
                    pupload.cardImgPath = uploadCardFileName;///.Substring(3, uploadCardFileName.Length - 3);
                    pupload.sceneImgPath = uploadSceneFileName;//.Substring(3, uploadSceneFileName.Length - 3);
                    pupload.faceImgPath = uploadSiteFileName;
                    string strRet = SanTranProto.Single().ThirdUpLoad(pupload,true);
                    Trace.WriteLine("兵团上传返回：" + strRet);



                    if (UploadSuccess(strRet))
                    {
                        //上传成功
                    }
                    else
                    {
                        SCardInfo cardInfo = new SCardInfo();
                        bool bReadCard = _sqliteOper.DatabaseReadCardInfo(m_cardData._idNum, ref cardInfo);
                        if (!bReadCard)
                        {
                            cardInfo._idNum = m_cardData._idNum;
                            string strBir = m_cardData._birthday;
                            cardInfo._birthday = strBir;
                            cardInfo._name = m_cardData._name;
                            cardInfo._sex = pupload.genderCode.ToString();
                            cardInfo._address = pupload.addr;
                            cardInfo._dateStart = m_cardData._dateStart;
                            cardInfo._dateEnd = m_cardData._dateEnd;
                            cardInfo._nation = pupload.nationCode;
                            cardInfo._issure = m_cardData._issure;
                            string[] ss = { m_cardData._idNum , m_cardData._name , pupload.genderCode.ToString() ,
                            strBir,pupload.nationCode,m_cardData._issure,m_cardData._address,m_cardData._dateStart,m_cardData._dateEnd};
                            _sqliteOper.DatabaseInsert("idCardInfo", ss);
                        }

                        string[] ssRecord =
                        {
                            m_cardData._idNum,pupload.passTime,_jobIdT,pupload.faceCompareScore.ToString(),pupload.faceCompareResult.ToString(),
                            uploadCardFileName,uploadSceneFileName,uploadSiteFileName,pupload.verifyResult.ToString(),"0",""
                        };
                        _sqliteOper.DatabaseInsert("idCardRecord", ssRecord);
                    }
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ThreadName + "Thread_UpLoadFile线程异常:" + ex.Message);
            }
        }

        private void SavePlateRecord(float fScore,string strDetectRes,int personType,string siteFullName,string passTime)
        {
            SPersonRecord personInfo = new SPersonRecord();
            personInfo._idNum = m_cardData._idNum;
            personInfo._detectScore = fScore.ToString();
            personInfo._detectType = "0";
            personInfo._detectResult = strDetectRes;
            personInfo._personType = personType.ToString();
            personInfo._imgSite = siteFullName;
            personInfo._passTime = passTime;
            if(_plateExchange != null && _plateExchange.Length > 0)
            {
                for (int i = 0; i < _plateExchange.Length; ++i)
                {
                    try
                    {
                        int iRet = _plateExchange[i].SavePersonRecord(personInfo);
                        if(iRet != 0)
                        {
                            Trace.WriteLine("平台记录保存失败：" + m_cardData._name + " 错误码："+ iRet);
                        }
                    }catch(Exception ex)
                    {
                        Trace.WriteLine("平台保存记录错误" + ex.Message);
                    }
                }
            }
        }

        private SqliteOper _sqliteOper = new SqliteOper();
        UploadHistroyRecord _historyRecord = new UploadHistroyRecord();

        private bool UploadSuccess(string strMsg)
        {
            if (strMsg == null) return false;
            int nPos = strMsg.IndexOf("status");
            if (nPos != -1)
            {
                string strStatus = strMsg.Substring(nPos + 9, 1);
                if(strStatus.CompareTo("0") == 0)
                {
                    return true;
                }
            }
            return false;
        }


        FileOperator _fo = new FileOperator();
        private List<SDetectRecord> _recordLists = new List<SDetectRecord>();
        
    }

}