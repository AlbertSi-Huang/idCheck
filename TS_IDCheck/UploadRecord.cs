using Common;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace TS_IDCheck
{
    /// <summary>
    /// 离线上传
    /// </summary>
    public class UploadHistroyRecord : BaseThread
    {
        private int _saveDay = 0;
        private SqliteOper _sqlite;
        public UploadHistroyRecord()
        {
            _saveDay = Convert.ToInt32(ConfigOperator.Single().StrSaveDays);
        }

        public void SetSqlite(SqliteOper sqlite)
        {
            _sqlite = sqlite;
        }

        private bool CheckTimeOutRecord()
        {
            string strLine = _fo.ReadDetectRecord();
            if(strLine == "")
            {
                return false;
            }
            string[] ss = strLine.Split(',');
            DateTime dtRecord = Convert.ToDateTime(ss[0]);
            DateTime dtTimeOut = DateTime.Now.AddDays(-_saveDay);

            int iRet = DateTime.Compare(dtRecord, dtTimeOut);
            if(iRet < 0)
            {
                //超过保存时间需要删除
                return true;
            }
            return false;
        }

        FileOperator _fo = new FileOperator(); 

        private bool SetInit()
        {
            CUploadRecordData recordData = new CUploadRecordData();
            bool bRet = _sqlite.DatabaseReadRecordInfo(ref recordData, "1");
            if (bRet)
            {
                _sqlite.DatabaseUpdateRecord(recordData._idNum, recordData._passtime, "0");
            }
            return bRet;
        }

        public override void Run()
        {
            Thread.Sleep(3000);

            while (SetInit())
            {
                Thread.Sleep(10);
            }
            CUploadRecordData recordData = new CUploadRecordData();
            SCardInfo cardInfo = new SCardInfo();
            PersonInfoUpload uploadInfo = new PersonInfoUpload();
            uploadInfo.token = ConfigOperator.Single().StrThreeToken;
            uploadInfo.deviceCode = ConfigOperator.Single().StrThreeDeviceCode;
            uploadInfo.collectionType = 1;
            uploadInfo.nationality = "中国";
            int nPlateChoose = ConfigOperator.Single().PlateChoose;

            //string strBlackUrl = "";
            ////string strIp = "";
            //if (nPlateChoose == 1 || nPlateChoose == 4)
            //{
            //    //公安网
            //    strBlackUrl = "http://10.20.50.110:9092/verificationInterface/passlog/blackList";
            //    //strIp = "10.20.50.110";
            //}
            //else if (nPlateChoose == 2)
            //{
            //    //视频网
            //    strBlackUrl = "http://21.0.0.125:9092/verificationInterface/passlog/blackList";
            //    strIp = "21.0.0.125";
            //}
            //else if (nPlateChoose == 3)
            //{
            //    //互联网
            //    strBlackUrl = "http://124.117.209.133:29092/verificationInterface/passlog/blackList";
            //    strIp = "124.117.209.133";
            //}
            ThirdBlackQurryMsg msg = new ThirdBlackQurryMsg();
            msg.deviceCode = ConfigOperator.Single().StrThreeDeviceCode;
            msg.token = ConfigOperator.Single().StrThreeToken;
            int nLoop = 0;
            try
            {
                while (IsAlive)
                {
                    recordData.Clear();
                    bool bHaveRecord = _sqlite.DatabaseReadRecordInfo(ref recordData);
                    if (!bHaveRecord)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    cardInfo.ClearData();
                    bool bHaveCard = _sqlite.DatabaseReadCardInfo(recordData._idNum, ref cardInfo);
                    if (!bHaveCard)
                    {
                        Trace.WriteLine("没有找到身份证:" + recordData._idNum);
                    }

                    if (bHaveCard && bHaveRecord)
                    {
                        //string strMsg = "";
                        ////if (recordData._blackJobId.Length == 0) {
                        ////    msg.cardNo = cardInfo._idNum;
                        ////    strMsg = SanTranProto.Single().ThirdQueryBlack(msg, strBlackUrl);
                        ////}
                        //if ((strMsg != null) && (strMsg.IndexOf("isBlackList") != -1))
                        //{
                        //    ThirdBlackQurryReturn mdc = JsonConvert.DeserializeObject<ThirdBlackQurryReturn>(strMsg);
                            
                        //    //Trace.WriteLine("离线上传 jobId get" + cardInfo._idNum);
                        //    //Trace.WriteLine(mdc.jobId);
                        //    recordData._blackJobId = mdc.jobId;
                        //}
                        uploadInfo.blackQueryJobId = recordData._blackJobId;
                        //if(uploadInfo.blackQueryJobId.Length == 0)
                        //{
                            uploadInfo.blackQueryJobId = Guid.NewGuid().ToString("N");
                        //}
                        uploadInfo.name = cardInfo._name;
                        uploadInfo.nationCode = cardInfo._nation;
                        uploadInfo.birthday = cardInfo._birthday;
                        uploadInfo.addr = cardInfo._address;
                        uploadInfo.genderCode = Convert.ToInt32(cardInfo._sex);
                        uploadInfo.nationCode = cardInfo._nation;
                        uploadInfo.cardNo = cardInfo._idNum;
                        uploadInfo.cardStartTime = cardInfo._dateStart;
                        uploadInfo.cardEndTime = cardInfo._dateEnd;
                        uploadInfo.issuingUnit = cardInfo._issure;

                        uploadInfo.faceCompareScore = Convert.ToDouble(recordData._compareScore);
                        uploadInfo.faceCompareResult = recordData._compareResult;
                        uploadInfo.passTime = recordData._passtime;
                        uploadInfo.verifyResult = Convert.ToInt32(recordData._verifyResult);
                        uploadInfo.faceQualityScore = "0.0";
                        string strUploadFile = string.Empty;
                        //身份证照上传
                        if (File.Exists(recordData._cardImg))
                        {
                            strUploadFile = SanTranProto.Single().ThirdUpLoadFile(recordData._cardImg);
                        }
                        else
                        {
                            uploadInfo.cardImgPath = recordData._cardImg;
                        }
                        //bool bUploadCardImg = false, bUploadSceneImg = false, bUploadSiteImg = false;
                        if (strUploadFile != null && strUploadFile.IndexOf("FilePath") != -1)
                        {
                            FileUploadReturn fur = JsonConvert.DeserializeObject<FileUploadReturn>(strUploadFile);
                            uploadInfo.cardImgPath = fur.FilePath;
                            File.Delete(recordData._cardImg);
                            //bUploadCardImg = true;
                        }

                        //全景照
                        strUploadFile = string.Empty;
                        if (File.Exists(recordData._sceneImg))
                        {
                            strUploadFile = SanTranProto.Single().ThirdUpLoadFile(recordData._sceneImg);
                        }
                        else
                        {
                            uploadInfo.sceneImgPath = recordData._sceneImg;
                        }
                        if (strUploadFile != null && strUploadFile.IndexOf("FilePath") != -1)
                        {
                            FileUploadReturn fur = JsonConvert.DeserializeObject<FileUploadReturn>(strUploadFile);
                            uploadInfo.sceneImgPath = fur.FilePath;
                            File.Delete(recordData._sceneImg);
                            //bUploadSceneImg = true;
                        }

                        //人脸照
                        strUploadFile = string.Empty;
                        if (File.Exists(recordData._faceImg))
                        {
                            strUploadFile = SanTranProto.Single().ThirdUpLoadFile(recordData._faceImg);
                        }
                        else
                        {
                            uploadInfo.faceImgPath = recordData._faceImg;
                        }
                        if (strUploadFile != null && strUploadFile.IndexOf("FilePath") != -1)
                        {
                            FileUploadReturn fur = JsonConvert.DeserializeObject<FileUploadReturn>(strUploadFile);
                            uploadInfo.faceImgPath = fur.FilePath;
                            File.Delete(recordData._faceImg);
                            //bUploadSiteImg = true;
                        }

                        uploadInfo.cardType = 1;

                        string strHttpRet = SanTranProto.Single().ThirdUpLoad(uploadInfo);
                        //Trace.WriteLine("历史记录上传返回：" + strHttpRet);
                        bool bUpload = UploadSuccess(strHttpRet);
                        if (bUpload)
                        {
                            //上传成功
                            nLoop = 0;
                            _sqlite.DatabaseDeleteRecord(cardInfo._idNum, recordData._passtime);
                        }
                        else
                        {
                            //上传失败 修改上传标志
                            nLoop++;
                            if(nLoop == 100)
                            {
                                _sqlite.DatabaseUpdateRecord(cardInfo._idNum, recordData._passtime, "1");
                                nLoop = 0;
                            } 
                        }
                    }

                    Thread.Sleep(1000);
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine("上传历史线程记录退出 ：" + ex.Message);
            }
            finally
            {
                Trace.WriteLine("上传历史线程记录退出,未知原因");
            }
            
        }


        private bool UploadSuccess(string strMsg)
        {
            if (strMsg == null) return false;
            int nPos = strMsg.IndexOf("status");
            if (nPos != -1)
            {
                string strStatus = strMsg.Substring(nPos + 9, 1);
                if (strStatus.CompareTo("0") == 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
