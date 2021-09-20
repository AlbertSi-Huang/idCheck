using System;
using System.Collections.Generic;
using MySqlLibrary;
using Common;
using System.Data;
using System.Threading;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.Serialization.Json;
using System.IO;

namespace LibExchangePlate
{
    #region 数据结构
    public struct SKeepLiveState
    {
        public float DevState { set; get; }
        public float DevStorage { set; get; } 
    }

    public struct SKeepLiveData
    {
        public string DevUpLoadId { set; get; }
        public string DevSN { set; get; }
        public SKeepLiveState DevState { set; get; }
        public string CustomID { set; get; }

        public string Status { set; get; }
        public string Message { set; get; }
    }

    public struct SBackInfo
    {
        public string status;
        public string message;
        public string type;
        public string tag;
    }

    public struct SUploadData
    {
        public string DevImport { set; get; }
        public string DevType { set; get; }
        public string DevID { set; get; }
        public string DevName { set; get; }
        public string DevAddress { set; get; }
        public string DevImportName { set; get; }
        public string videoCode { set; get; }
        public string videoName { set; get; }
        public string DetectType { set; get; }
        public string DetectTypeName { set; get; }
        public string PersonType{ set; get; }
        public string DetectTime { set; get; }       //2017-06-01 12:43:22
        public string DetectScore { set; get; }
        public string DetectResult { set; get; }
        public string IDNumber { set; get; }
        public string IdentificationNumber { set; get; }
        public string SignCount { set; get; }
        public string CertificateNO { set; get; }


        public string PlateDevice { set; get; }      //关联的车牌设备 DevUploadId
        public string ICNumber { set; get; }
        public string IDPlate { set; get; }
        public string PlateColor { set; get; }
        public string CarColor { set; get; }         //黄色
        public string ImagePlate { set; get; }
        public string ImageFullPlate { set; get; }
        public string Name { set; get; }
        public string Sex { set; get; }
        public string SexCode { set; get; }
        public string Nation { set; get; }
        public string NationCode { set; get; }
        public string Birthday { set; get; }
        public string Address { set; get; }
        public string Telephone { set; get; }
        public string ResidentialAddress { set; get; }

        public string Issuer { set; get; }
        public string ValidDateBegin { set; get; }
        public string ValidDateEnd { set; get; }
        public string ImageCard { set; get; }
        public string ImageFace { set; get; }
        public string IsVisitoredIdNumber { set; get; }
        public string IsVisitoredName { set; get; }
        public string IsVisitoredMainContentCode { set; get; }
        public string IsVisitoredMainContent { set; get; }
        public string DevUpLoadID { set; get; }          //上报的数据的唯一设备编码
        public string CustomID { set; get; }
        public string CusTomPwd { set; get; }
        public string RightFingure1 { set; get; }
        public string RightFingure2 { set; get; }
        public string RightFingure3 { set; get; }
        public string RightFingure4 { set; get; }
        public string RightFingure5 { set; get; }

        public string LeftFingure1 { set; get; }
        public string LeftFingure2{ set; get; }
    public string LeftFingure3 { set; get; }
    public string LeftFingure4 { set; get; }
        public string LeftFingure5 { set; get; }
        public string RightEye { set; get; }
        public string LeftEye { set; get; }
        public string BloodType { set; get; }
    }

    //[DataContract]
    public struct STianShanUploadInfo
    {
        public string version;
        public bool encryption;        //是否加密 
        public string app_key;
        public string data;
    }

    #endregion
    
    public class PlateTianShan : PlateExchangeAbstract
    {
        private static readonly string databaseName = "TS_UploadPlate";
        TableHandle<EUploadPlate> uploadTable = null;
        public static readonly TableColumnParam<EUploadPlate>[] uploadCol = {
            new TableColumnParam<EUploadPlate>(EUploadPlate.passTime,"varchar(32)",false,EKeyType.PRI),
            new TableColumnParam<EUploadPlate>(EUploadPlate.idCardNumber,"varchar(32)"),
            new TableColumnParam<EUploadPlate>(EUploadPlate.carNumber,"varchar(16)"),
            new TableColumnParam<EUploadPlate>(EUploadPlate.reserved0,"varchar(4)"),
        };

        public PlateTianShan()
        {
            Name = "TIANSHAN";
        }

        private bool CreateuploadTable()
        {
            Trace.WriteLine("开始创建 TianShan_UploadPlate 平台");
            uploadTable = new TableHandle<EUploadPlate>("TianShan_uploadPlate", databaseName, uploadCol);
            if (!uploadTable.CreateTable()) return false;
            Trace.WriteLine("开始创建 TianShan_UploadPlate 平台结束");
            return true;
        }
        // 定义一个标识确保线程同步
        private static readonly object locker = new object();
        Thread uploadThread = null;
        Thread keepLiveThread = null;
        HttpExchange _httpOper = new HttpExchange();
        
        /// <summary>
        /// 进行DES加密。
        /// </summary>
        /// <param name="pToEncrypt">要加密的字符串。</param>
        /// <param name="sKey">密钥，且必须为8位。</param>
        /// <returns>以Base64格式返回的加密字符串。</returns>
        private string Encrypt(string pToEncrypt, string sKey)
        {
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                byte[] inputByteArray = Encoding.UTF8.GetBytes(pToEncrypt);
                
                des.Key = Encoding.UTF8.GetBytes(sKey);
                des.IV = Encoding.UTF8.GetBytes(sKey);
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                }

                StringBuilder strRet = new StringBuilder();
                foreach(byte b in ms.ToArray())
                {
                    strRet.AppendFormat("{0:X2}", b);
                }
                
                //byte[] b = ms.ToArray();
                //string str = BitConverter.ToString(b, 0, b.Length).Replace("-", string.Empty);
                ms.Close();
                return strRet.ToString();
            }
        }

        // <summary>
        // 进行DES解密。
        // </summary>
        // <param name="pToDecrypt">要解密的以Base64</param>
        // <param name="sKey">密钥，且必须为8位。</param>
        // <returns>已解密的字符串。</returns>
        private string Decrypt(string pToDecrypt, string sKey)
        {
            byte[] inputByteArray = Convert.FromBase64String(pToDecrypt);
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
                des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                }
                string str = Encoding.UTF8.GetString(ms.ToArray());
                ms.Close();
                return str;
            }
        }
        #region web server方式
        //private void GetExchangeSoap()
        //{
        //    try
        //    {
        //        string str = GetEndPointAddressByName("savePersonRecord");     //根据此名称，调用上面的方法，获取webservice的地址
        //        if (str != null)
        //            webInterface = new zxkjDataExchangeSoapClient(new BasicHttpBinding(), new EndpointAddress(str));   //根据地址去实例化接口
        //        else
        //            return;
        //        if (webInterface == null)
        //        {
        //            //RecordLog<string>("Recive:[SendPolicyInfo.OBWSC]", "服务调用失败");
        //            return;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.WriteLine("tianshan upload:" + ex.Message);
        //        return;
        //    }
        //}

        //private string GetEndPointAddressByName(string name)
        //{
        //    return "http://" + ConfigTianShan.Single().WebsiteAddress + "/zxkjDataExchange.asmx/";
        //}
        #endregion


        private void Run()
        {
            Thread.Sleep(3000);
            DataRow dr = null;
            DataRow icDr = null;

            string strUrl = "http://" + ConfigTianShan.Single().WebsiteAddress + "/zxkjDataExchange/uploadperson";//"/zxkjDataExchange.asmx/savePersonRecord";
            
            DataRow drPersonRecord = null;
            DataRow drPersonInfo = null;
            string strbackMsg = string.Empty;

            STianShanUploadInfo uploadInfo = new STianShanUploadInfo();
            uploadInfo.version = ConfigTianShan.Single().Version;
            uploadInfo.encryption = ConfigTianShan.Single().Encryption.CompareTo("1") == 0 ? true:false;
            uploadInfo.app_key = uploadInfo.encryption ? ConfigTianShan.Single().Appkey : "";
            SUploadData data = new SUploadData();
            data.PlateDevice = ConfigTianShan.Single().PlateDevice;
            data.DevUpLoadID = ConfigTianShan.Single().SerialNum;
            data.DevAddress = ConfigTianShan.Single().DevAddress;
            data.DevID = ConfigTianShan.Single().DevId;
            data.DevImport = ConfigTianShan.Single().DevImport;
            data.DevImportName = ConfigTianShan.Single().DevImport.CompareTo("1") == 0 ? "入口" : "出口";//ConfigTianShan.Single().DevName;
            data.DevType = ConfigTianShan.Single().DevType;
            data.CustomID = ConfigTianShan.Single().CustomID;
            data.CusTomPwd = ConfigTianShan.Single().CustomPwd;
            data.DevName = ConfigTianShan.Single().DevName;
            data.videoCode = "未采集";
            data.videoName = "未采集";
            data.IdentificationNumber = "未采集";
            data.SignCount = "未采集";
            data.CertificateNO = "未采集";
            data.Telephone = "未采集";
            data.ResidentialAddress = "未采集";
            data.IsVisitoredMainContent = "未采集";
            data.RightFingure1 = "未采集";
            data.RightFingure2 = "未采集";
            data.RightFingure3 = "未采集";
            data.RightFingure4 = "未采集";
            data.RightFingure5 = "未采集";
            data.LeftFingure1 = "未采集";
            data.LeftFingure2 = "未采集";
            data.LeftFingure3 = "未采集";
            data.LeftFingure4 = "未采集";
            data.LeftFingure5 = "未采集";
            data.LeftEye = "未采集";
            data.RightEye = "未采集";
            data.BloodType = "未采集";
            data.IsVisitoredMainContentCode = "2";
            data.DetectTypeName = "人证";
            int nSleepTick = 1;
            while (uploadThread.IsAlive)
            {
                lock (locker)
                {
                    dr = uploadTable.GetOneData();
                }
                if (dr == null || dr[EUploadPlate.idCardNumber.ToString()].ToString().Length == 0)
                {
                    Thread.Sleep(100);
                    continue;
                }

                string upId = dr[EUploadPlate.idCardNumber.ToString()].ToString();
                string upPassTime = dr[EUploadPlate.passTime.ToString()].ToString();
                string upType = dr[EUploadPlate.reserved0.ToString()].ToString();

                //if (upType.CompareTo("0") == 0)
                //{
                    //过人记录
                    drPersonRecord = DatabaseHandle.Single.FaceDetectTable.GetOneData(new List<QueryCondition<FaceDetectColumn>>
                    { new QueryCondition<FaceDetectColumn>(FaceDetectColumn.idCardNumber,upId),new QueryCondition<FaceDetectColumn>(FaceDetectColumn.compareTime,upPassTime) });

                    //Trace.WriteLine("查找身份证信息");
                    //身份证信息
                    drPersonInfo = DatabaseHandle.Single.IdCardTable.GetOneData(new List<QueryCondition<IdCardColumn>> {
                    new QueryCondition<IdCardColumn>(IdCardColumn.idCardNumber,upId)});
                    //Trace.WriteLine("查找身份证信息结束*****");
                    if (drPersonInfo == null)
                    {
                        Trace.WriteLine("人员信息为空：" + upId);
                    }
                    if (drPersonRecord == null)
                    {
                        Trace.WriteLine("记录信息为空：" + upPassTime);
                    }
                if (drPersonRecord != null && drPersonInfo != null)
                {
                    SPersonRecord record = new SPersonRecord();
                    record._detectResult = drPersonRecord[FaceDetectColumn.detectResult.ToString()].ToString();
                    record._detectScore = drPersonRecord[FaceDetectColumn.detectScore.ToString()].ToString();
                    record._detectType = drPersonRecord[FaceDetectColumn.detectMode.ToString()].ToString();
                    record._imgSite = drPersonRecord[FaceDetectColumn.imgSite.ToString()].ToString();
                    record._imgScene = drPersonRecord[FaceDetectColumn.imgScene.ToString()].ToString();
                    record._personType = drPersonRecord[FaceDetectColumn.personType.ToString()].ToString();

                    //DateTime dtPassTime = (DateTime);
                    record._passTime = drPersonRecord[FaceDetectColumn.compareTime.ToString()].ToString();
                    SCardInfo card = new SCardInfo();
                    card._idNum = upId;
                    card._name = drPersonInfo[IdCardColumn.personName.ToString()].ToString();
                    card._birthday = drPersonInfo[IdCardColumn.birthday.ToString()].ToString();
                    card._dateEnd = drPersonInfo[IdCardColumn.dateEnd.ToString()].ToString();
                    card._dateStart = drPersonInfo[IdCardColumn.dateStart.ToString()].ToString();
                    card._issure = drPersonInfo[IdCardColumn.issue.ToString()].ToString();
                    card._nation = drPersonInfo[IdCardColumn.ethnic.ToString()].ToString();
                    card._sex = drPersonInfo[IdCardColumn.gender.ToString()].ToString();
                    card._photo = drPersonInfo[IdCardColumn.idCardPhoto.ToString()].ToString();
                    card._address = drPersonInfo[IdCardColumn.address.ToString()].ToString();
                    data.DetectType = record._detectType.CompareTo("0") == 0?"2":"3";
                    data.PersonType = Exchange(record._personType);//record._personType;
                    data.DetectTime = record._passTime;
                    data.DetectResult = ExchangeDetectResult(record._detectResult); //record._detectResult;
                    data.DetectScore = record._detectScore;

                    data.IDNumber = card._idNum;
                    data.Name = card._name;
                    bool bSex = (Convert.ToInt32(card._idNum.Substring(16, 1)) % 2 == 0);
                    data.Sex = bSex ? "女" : "男";
                    data.SexCode = bSex ? "2" : "1";
                    data.Nation = card._nation;
                    data.NationCode = TS_Helper.GetNationCode(card._nation);

                    data.Birthday = card._idNum.Substring(6, 4) + "-" + card._idNum.Substring(10, 2) + "-" + card._idNum.Substring(12, 2);
                    data.Address = card._address;
                    data.Issuer = card._issure;
                    string sStrart = "";
                    if (card._dateStart != null && card._dateStart.Length == 8)
                    {
                        sStrart = card._dateStart.Substring(0, 4) + "-" + card._dateStart.Substring(4, 2) + "-" + card._dateStart.Substring(6, 2);
                    }
                    data.ValidDateBegin = sStrart;
                    string sEnd = card._dateEnd;
                    if (card._dateEnd != null && card._dateEnd.Length == 8)
                        sEnd = card._dateEnd.Substring(0, 4) + "-" + card._dateEnd.Substring(4, 2) + "-" + card._dateEnd.Substring(6, 2);
                    data.ValidDateEnd = sEnd;
                    if (card._photo.Length > 1)
                        data.ImageCard = Common.ImageDealOper.ImgToBase64String(card._photo);
                    if (record._imgSite.Length > 1)
                        data.ImageFace = Common.ImageDealOper.ImgToBase64String(record._imgSite);
                    if (record._personType.CompareTo("1") == 0)
                    {
                        icDr = null;
                        icDr = DatabaseHandle.Single.IcCardTable.GetOneData(new List<QueryCondition<icCardColum>> {
                                new QueryCondition<icCardColum>(icCardColum.idCardNumber,upId)});

                        if (icDr != null)
                            data.ICNumber = icDr[icCardColum.icCardNumber.ToString()].ToString();
                        else
                            data.ICNumber = "";
                    }
                    else
                    {
                        DatabaseHandle.Single.IcCardTable.DeleteData(new List<QueryCondition<icCardColum>>
                            {
                                new QueryCondition<icCardColum>(icCardColum.idCardNumber,upId)
                            });
                        data.ICNumber = "";
                    }

                    //汽车信息
                    data.IDPlate = "";
                    data.PlateColor = "";
                    data.IsVisitoredIdNumber = "";
                    data.IsVisitoredName = "";
                    data.ImageFullPlate = "";
                    data.ImagePlate = "";
                    data.CarColor = "";

                    string strEncData = Encrypt(DeseralizerObjectToString<SUploadData>(data),
                       ConfigTianShan.Single().EncryptKey);

                    uploadInfo.data = uploadInfo.encryption? strEncData: DeseralizerObjectToString<SUploadData>(data);
                    string strJson = JsonConvert.SerializeObject(uploadInfo);
                    try
                    {
                        //Dictionary<string, string> dic = new Dictionary<string, string>();
                        //dic.Add("json", strJson);
                        //bool b = _httpOper.PostKeyAndValue(strUrl, dic, out strbackMsg);
                        bool b = _httpOper.RequestPost(strUrl, uploadInfo, out strbackMsg);
                        Trace.WriteLine("天山平台上传返回：" + strbackMsg);
                    }
                    catch (Exception ex)
                    {
                        nSleepTick++;
                        Thread.Sleep(100 * nSleepTick);
                        Trace.WriteLine("webInterface.savePersonRecord" + ex.Message);
                    }

                    string strSuccess = DealBackString(strbackMsg);

                    if (strSuccess!= null && strSuccess.CompareTo("0") == 0)
                    {
                        //上传成功
                        Trace.WriteLine(card._name + " 过往信息上传成功");

                        if (data.PersonType.CompareTo("2") == 0)
                        {
                            Trace.WriteLine(data.Name + " 黑名单上传成功");
                        }
                        try
                        {
                            DateTime dt;
                            DateTime.TryParse(record._passTime, out dt);
                            string strTmp = dt.ToString("yyyy-MM-dd HH:mm:ss");
                            lock (locker)
                            {
                                uploadTable.DeleteData(new List<QueryCondition<EUploadPlate>> {
                                    new QueryCondition<EUploadPlate>(EUploadPlate.idCardNumber,upId),
                                    new QueryCondition<EUploadPlate>(EUploadPlate.passTime,strTmp)
                                        });
                            }
                        }
                        catch (Exception ex)
                        {
                            nSleepTick++;
                            Thread.Sleep(100 * nSleepTick);
                            Trace.WriteLine("天山平台删除错误：" + ex.Message);
                        }
                        nSleepTick = 1;
                        nSleepTick++;
                        Thread.Sleep(200 * nSleepTick);
                    }
                    else
                    {
                        //上传失败
                        nSleepTick++;
                        Thread.Sleep(100 * nSleepTick);
                    }
                    strbackMsg = "";
                }

                Thread.Sleep(30);
            }
        }
        
        public string DeseralizerObjectToString<T>(T obj)
        {
            DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(typeof(T));
            MemoryStream msObj = new MemoryStream();
            deseralizer.WriteObject(msObj, obj);
            msObj.Position = 0;
            StreamReader sr = new StreamReader(msObj);
            string json = sr.ReadToEnd();

            return json;
        }
        private string DealBackString(string strBak)
        {
            int nPosBegin = strBak.IndexOf('{');
            int nPosEnd = strBak.IndexOf('}');
            string strRet = "";
            if (nPosBegin != -1 && nPosEnd != -1)
            {
                string strJson = strBak.Substring(nPosBegin, nPosEnd - (nPosBegin ) + 1);
                try
                {
                    SBackInfo info = (SBackInfo)JsonConvert.DeserializeObject(strJson, typeof(SBackInfo));
                    return info.status;
                }
                catch(Exception ex)
                {
                    return ex.Message;
                }
            }
            return strRet;
        }
        
        PlateExchangeBakInfo _broadcastInfo = new PlateExchangeBakInfo();

        public override bool Init(string userName, string pwd, string localHost)
        {
            bool bRet = base.Init(userName, pwd, localHost);
            if (!bRet)
            {
                return false;
            }

            ConfigTianShan.Single().InitConfig();
            uploadThread = new Thread(Run);
            uploadThread.IsBackground = true;
            uploadThread.Start();

            keepLiveThread = new Thread(OnKeepLive);
            keepLiveThread.IsBackground = true;
            keepLiveThread.Start();

            return CreateuploadTable();
        }
        bool _bFirst = true;

        private void OnKeepLive()
        {
            Thread.Sleep(5000);
            string strUrl = "http://" + ConfigTianShan.Single().WebsiteAddress + "/zxkjDataExchange/heartbeat";
            int nTimeTick = ConfigTianShan.Single().KeepHeartTick;
            string strDevId = ConfigTianShan.Single().DevId;
            //"65010501600300101";
            string strDevSN = ConfigTianShan.Single().DevSN;
            string strCustomID = ConfigTianShan.Single().CustomID;

            STianShanUploadInfo updataInfo = new STianShanUploadInfo();
            updataInfo.version = ConfigTianShan.Single().Version;
            updataInfo.app_key = ConfigTianShan.Single().Appkey;
            updataInfo.encryption = false;

            SKeepLiveData  keepLiveData = new SKeepLiveData();
            keepLiveData.DevUpLoadId = strDevId;
            keepLiveData.DevSN = strDevSN;
            keepLiveData.CustomID = strCustomID;
            SKeepLiveState liveState = new SKeepLiveState();
            liveState.DevState = 0.0f;
            liveState.DevStorage = 0.0f;
            keepLiveData.DevState = new SKeepLiveState();
            keepLiveData.DevState = liveState;
            keepLiveData.Status = "0";
            keepLiveData.Message = "无";
            updataInfo.data = JsonConvert.SerializeObject(keepLiveData);

            DateTime _lastTikc = DateTime.Now;
            while (keepLiveThread.IsAlive)
            {
                DateTime dtNow = DateTime.Now;
                TimeSpan ts = dtNow.Subtract(_lastTikc);
                if(ts.TotalMilliseconds < nTimeTick * 1000)
                {
                    Thread.Sleep(100);
                    continue;
                }
                string strMsg = string.Empty;
                bool bRet = _httpOper.RequestPost(strUrl, updataInfo, out strMsg);

                if(_bFirst && bRet)
                {
                    int nPos = strMsg.IndexOf("message");
                    if(nPos != -1)
                    {
                        string strTime = strMsg.Substring(nPos + 10, 19);
                        try
                        {
                            TimeAbout.SetLocalTimeByStr(strTime);
                        }
                        catch(Exception ex)
                        {
                            Trace.WriteLine("TianShan Set Time Err " + ex.Message);
                        }
                        _bFirst = false;
                    }

                }
                _lastTikc = DateTime.Now;
            }
        }

        public override bool LoginIn(string reserv)
        {
            return true;
        }

        public override int QueryBlack(string cardId, int nType = 0)
        {
            return 0;
        }
        
        public override int SaveCarRecord(SCarRecord info)
        {
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="persionType"></param>
        /// <returns>0 白名单 1陌生人 2黑名单</returns>
        private string Exchange(string persionType)
        {
            string strRet = "1";
            if (persionType.CompareTo("2") == 0)
            {
                strRet = "2";
            }
            else if (persionType.CompareTo("1") == 0)
            {
                strRet = "0";
            }
            return strRet;
        }

        private string ExchangeDetectResult(string detectResult)
        {
            string strRet = "0";
            if (detectResult.CompareTo("2") == 0 || detectResult.CompareTo("4") == 0 || detectResult.CompareTo("5") == 0 || detectResult.CompareTo("6") == 0)
            {
                strRet = "1";
            }
            return strRet;
        }

        public override int SavePersonRecord(SPersonRecord info)
        {
            int nRet = base.SavePersonRecord(info);
            if (nRet == -1) return nRet;

            DataRow drUpload = uploadTable.NewDataTable().NewRow();
            drUpload[EUploadPlate.carNumber.ToString()] = "";
            drUpload[EUploadPlate.idCardNumber.ToString()] = info._idNum;
            drUpload[EUploadPlate.passTime.ToString()] = info._passTime;
            drUpload[EUploadPlate.reserved0.ToString()] = "0";     //过人记录
            Trace.WriteLine("天山区过人记录保存*****");
            lock (locker)
            {
                nRet = uploadTable.InsertData(drUpload);
                if (nRet == 0)
                {
                    _broadcastInfo._funcName = "SavePersonRecord";
                    _broadcastInfo._msg = "天山区过人记录保存失败";
                    _broadcastInfo._nLevel = EWainLevel.EWL_debug;
                    OnBroadcastExchangeInfo(_broadcastInfo);
                    return -2;
                }
            }

            return 0;
        }
        
    }
}
