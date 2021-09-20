#define USE_BINGTUAN_XIEYI
using System;
using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;
using Common;
using System.Windows;
using System.Net.Http;
using Newtonsoft.Json;
//using TaiShou.ConfigLoad;

namespace TS_IDCheck
{
    #region Module类定义
    public class ThirdQuerryByIdMsg
    {
        public string method { get; set; }
        public string cardNo { get; set; }
        public string personName { get; set; }
        public string invokeKey { get; set; }
    }
    public class personInfo
    {
        public string personName { get; set; }
        public string cardNo { get; set; }
        public string genderCode { get; set; }
        public string nationCode { get; set; }
        public string addr { get; set; }
        public string photo { get; set; }
    }
    public class ThirdQuerryByIdReturnMsg
    {
        public string message { get; set; }
        public string status { get; set; }
        public personInfo personInfo { get; set; }

    }
    public class responseBody
    {

        public string deviceCode { get; set; }

        public string ip { get; set; }

        public string license { get; set; }

        public double latitude { get; set; }

        public double longitude { get; set; }
    }
    /// <summary>  
    /// 设备注册实体 
    /// </summary>  
    public class ThirdRegistMsg
    {
        public string responseName { get; set; }
        public responseBody responseBody { get; set; }
    }
    
    public class reguestBody
    {

        public string validity { get; set; }
        public string token { get; set; }
    }


    public class ThirdBlackQurryMsg
    {
        public string cardNo { get; set; }
        public string deviceCode { get; set; }
        public string token { get; set; }
    }

    public class ThirdBlackQurryReturn
    {
        public string message { get; set; }
        public string status { get; set; }
        public string jobId { get; set; }
        public int isBlackList { get; set; }
        public string extendInfo1 { get; set; }
        public string extendInfo2 { get; set; }
        public string extendInfo3 { get; set; }
    }
    public class PersonInfoUpload
    {
        //基本信息
        public string token { get; set; }
        public string deviceCode { get; set; }
        public string blackQueryJobId { get; set; }
        public int collectionType { get; set; }
        public string passNo { get; set; }
        public int inOrOut { get; set; }
        public int verifyResult { get; set; }
        public string passTime { get; set; }
        

        //身份证信息
        public string name { get; set; }
        public int genderCode { get; set; }
        public string nationCode { get; set; }
        public string nationality { get; set; }
        public string birthday { get; set; }
        public string addr { get; set; }
        public string cardNo { get; set; }
        public string cardStartTime { get; set; }
        public string cardEndTime { get; set; }
        public int cardType { get; set; }
        public string issuingUnit { get; set; }

        //图片信息
        public string cardImgPath { get; set; }
        public string sceneImgPath { get; set; }
        public string faceImgPath { get; set; }
        public double faceCompareScore { get; set; }
        
        /// <summary>
        /// 人脸比对结果
        /// </summary>
        public string faceCompareResult { set; get; }

        public string faceQualityScore { set; get; }

        public string extendInfo1 { set; get; }
        public string extendInfo2 { set; get; }
        public string extendInfo3 { set; get; }
    }
    
    public struct KeepLiveResponse
    {
        public string resName;
        //public responseBody informInfo;
    }

    //public struct responseBody
    //{
    //    public string deviceCode;
    //    public string token;
    //}

    public class FileUploadReturn
    {
        //基本信息
        public string status { get; set; }
        public string FilePath { get; set; }

    }
    #endregion

    /// <summary>
    /// 三所http接口访问
    /// </summary>
    public class SanTranProto
    {
        private static SanTranProto instance = null;
        private static reguestBody _tokenInfo = new reguestBody();
        private reguestBody _tokenInfoT = new reguestBody();
        public static SanTranProto Single()
        {
            if (instance == null)
            {
                System.Net.ServicePointManager.DefaultConnectionLimit = 50;
                instance = new SanTranProto();
            }
            return instance;
        }
        

        /// <summary>
        /// Post方法
        /// </summary>
        /// <param name="mystrpath"></param>
        /// <param name="myQuery"></param>
        /// <returns></returns>
        private string GetContent(string mystrpath, string myQuery)
        {
            byte[] payload;
            string str_path = mystrpath;

            StringBuilder sb = new StringBuilder();
            HttpWebRequest request;
            HttpWebResponse response;
            request = (HttpWebRequest)(WebRequest.Create(str_path));
            request.Method = "POST";
            request.ContentType = "application/json;charset=utf-8";
            request.Timeout = 1500;
            payload = System.Text.Encoding.UTF8.GetBytes(myQuery);
            request.ContentLength = payload.Length;
            request.KeepAlive = false;
            request.Proxy = null;

            try
            {
                Stream newStream = request.GetRequestStream();
                newStream.Write(payload, 0, payload.Length);
                newStream.Close();
                response = (HttpWebResponse)(request.GetResponse());
                Stream streamResponse = response.GetResponseStream();
                StreamReader streamRead = new StreamReader(streamResponse);
                Char[] readBuffer = new Char[256];
                int count = streamRead.Read(readBuffer, 0, 256);
                
                while (count > 0)
                {
                    sb.Append(readBuffer, 0, count);
                    count = streamRead.Read(readBuffer, 0, 256);
                }
                streamRead.Close();
                streamResponse.Close();
                response.Close();
                if(sb.Length <100)
                {
                    string hh;
                    hh = sb.ToString();
                }
                else
                {
                    Trace.WriteLine("get Picture...");
                }
               
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Connect failed!" + ex.Message);
                //无法连接服务器
                Trace.WriteLine("无法连接三所平台服务器");
                return null;
            }
        }

        private static readonly HttpClient _httpClient = new HttpClient();
        public bool RequestPostNew(string URL,object data,out string msg)
        {
            msg = "";
            try
            {
                _httpClient.SendAsync(new HttpRequestMessage
                {
                    Method = new HttpMethod("HEAD"),
                    RequestUri = new Uri(URL)
                })
                .Result.EnsureSuccessStatusCode();
                _httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");

                var requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                HttpContent httpcontent = new StringContent(requestjson);
                httpcontent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = _httpClient.PostAsync(URL, httpcontent).Result;
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    msg = response.Content.ReadAsStringAsync().Result;
                }
                //WriteLog("平台返回:");
                //WriteLog(msg);
                return true;
            }catch(Exception ex)
            {
                msg = ex.Message;
                Trace.WriteLine(msg);
                return false;
            }
        }

        public bool SendHttpRequest(string strUrl,object objData,out string msg)
        {
            msg = "";
            var requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(objData);

            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(strUrl);
                myRequest.Method = "POST";
                byte[] buf = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(requestjson);
                myRequest.ContentLength = buf.Length;
                myRequest.Timeout = 2500;
                //指定为json否则会出错
                myRequest.ContentType = "application/json";
                myRequest.MaximumAutomaticRedirections = 1;
                myRequest.AllowAutoRedirect = true;

                using (Stream newStream = myRequest.GetRequestStream())
                {
                    newStream.Write(buf, 0, buf.Length);
                }

                HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
                using (StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8))
                {
                    msg = reader.ReadToEnd();
                }
                myResponse.Close();
                return true;
            }
            catch(Exception ex)
            {
                msg = ex.Message;
                Trace.WriteLine(msg);
                return false;
            }

        }

        public bool RequestPost(string URL, object data, out string msg)
        {
            msg = "";
            try
            {
                var requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                HttpContent httpcontent = new StringContent(requestjson);
                httpcontent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                //httpcontent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

                HttpClientHandler handle = new HttpClientHandler();
                
                handle.AutomaticDecompression = System.Net.DecompressionMethods.GZip;
                HttpClient httpclient = new HttpClient(handle);
                httpclient.DefaultRequestHeaders.Connection.Add("keep-alive");
                
                //TimeSpan ts = new TimeSpan(2000);
                //httpclient.Timeout = ts;
                //httpclient      //getHttpConnectionManager().getParams().setSoTimeout(2000);
                HttpResponseMessage response = httpclient.PostAsync(URL, httpcontent).Result;

                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    msg = response.Content.ReadAsStringAsync().Result;
                }
                //WriteLog("平台返回:");
                //WriteLog(msg);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                Trace.WriteLine(msg);
                return false;
            }
        }

        /// <summary>
        /// Post方法
        /// </summary>
        /// <param name="mystrpath"></param>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private string fileUpload(string mystrpath, string filepath)
        {

            string str_path = mystrpath;
            String boundary = System.Guid.NewGuid().ToString("N"); ;
            String Enter = "\r\n";
            StringBuilder sb = new StringBuilder();
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            
            String part1 = "--" + boundary + Enter + "Content-Type: application/octet-stream" + Enter + "Content-Disposition: form-data; filename=\"" + filepath + "\"; name=\"file\"" + Enter + Enter;
            // part 2
            String part2 = Enter + "--" + boundary + Enter + "Content-Type: text/plain" + Enter + "Content-Disposition: form-data" + Enter + Enter + Enter + "--" + boundary + "--";
            int bufferLength = 4096;
            byte[] buffer = new byte[bufferLength];
            long offset = 0;
            byte[] PostHead = System.Text.Encoding.UTF8.GetBytes(part1);
            byte[] PostTail = System.Text.Encoding.UTF8.GetBytes(part2);
            request = (HttpWebRequest)(WebRequest.Create(str_path));
            request.Method = "POST";
            request.ContentType = "multipart/form-data;boundary=" + boundary;
            request.AllowWriteStreamBuffering = false;
            request.Timeout = 5000;
            request.SendChunked = true;
            Stream streamResponse = null;
            StreamReader streamRead = null;
            FileStream fs = null; BinaryReader r = null;
            try
            {
                fs = new FileStream(filepath, FileMode.Open, 
                    FileAccess.Read, FileShare.ReadWrite);
                r = new BinaryReader(fs);
                fs.Close();
                int size = r.Read(buffer, 0, bufferLength);
                Stream newStream = request.GetRequestStream();

                newStream.Write(PostHead, 0, PostHead.Length);

                while (size > 0)
                {
                    newStream.Write(buffer, 0, size);
                    offset += size;
                    size = r.Read(buffer, 0, bufferLength);
                }
                newStream.Write(PostTail, 0, PostTail.Length);

                newStream.Close();
                response = (HttpWebResponse)(request.GetResponse());
                streamResponse = response.GetResponseStream();
                streamRead = new StreamReader(streamResponse);
                Char[] readBuffer = new Char[256];
                int count = streamRead.Read(readBuffer, 0, 256);

                while (count > 0)
                {
                    sb.Append(readBuffer, 0, count);
                    count = streamRead.Read(readBuffer, 0, 256);
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("无法连接三所平台服务器 Connect failed!" + ex.Message);
                return null;
            }
            finally
            {
                if(streamRead != null)
                    streamRead.Close();
                if(streamResponse != null)
                streamResponse.Close();
                if (response != null)
                    response.Close();
                if (r != null)
                    r.Close();
                //if (fs != null)
                //    fs.Close();
            }
        }

        /// <summary>
        /// 三所http接口
        /// 通过身份证查询照片
        /// </summary>
        /// <param name="mymsg"></param>
        /// <returns></returns>
        public string ThirdQueryById(ThirdQuerryByIdMsg mymsg,string url)
        {
            if(ConfigOperator.Single().PlateChoose == 0)
            {
                return null;
            }

            try
            {
                string ss = null;
                string returnstr = null;
                Trace.WriteLine("开始查询照片");
                if (_tokenInfo.token != null)
                {
                    string mystr = url;
                        //string.Format("http://{0}/verificationInterface/dataQuery/queryPersonInfo", 
                        //ConfigLoad.URL_InertNet);

                    mymsg.invokeKey = "F05949BEEA3BCE3F211ACA5569D3F6D3";
                    ss = Newtonsoft.Json.JsonConvert.SerializeObject(mymsg);
                    RequestPost(mystr, mymsg, out returnstr);
                    //returnstr = GetContent(mystr, ss);
                    Trace.WriteLine("得到照片信息查询返回");
                    ss = returnstr;
                }
                return returnstr;
            }
            catch
            {
                return null;
            }
        }
        

        private string GetUserIp()
        {
            string name = Dns.GetHostName();
            string info = string.Empty;
            IPAddress[] ipadrList = Dns.GetHostAddresses(name);
            foreach (IPAddress ipa in ipadrList)
            {
                if (ipa.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    info = ipa.ToString();
                    break;
                }
            }
            return info;
        }

        private bool DealRegistBack(string strRegBack,bool bTwo = false)
        {
            Trace.WriteLine("begin DealRegistBack");
            if (strRegBack != null && strRegBack.IndexOf("validity") > 0)
            {
                int nPos = strRegBack.IndexOf("status");
                nPos += 9;
                string strStatus = strRegBack.Substring(nPos, 1);
                if(strStatus.CompareTo("0") == 0)
                {
                    //注册成功
                    nPos = strRegBack.IndexOf("validity");
                    nPos += 11;
                    string strVal = strRegBack.Substring(nPos, 19);
                    
                    nPos = strRegBack.IndexOf("token\"");
                    nPos += 8;
                    string strToken = strRegBack.Substring(nPos, 36);

                    ConfigOperator.Single().SetRegistInfo(1, strToken, strVal);
                    return true;
                }
                Trace.WriteLine(strRegBack);
                return false;
            }
            //MessageBox.Show("平台返回：" + strRegBack);
            return false;
        }

        public bool ThirdQueryRegistT()
        {
            try
            {
                ThirdRegistMsg mymsg = new ThirdRegistMsg();
                mymsg.responseName = "Register";
                responseBody resBody = new responseBody();
                resBody.deviceCode = ConfigOperator.Single().StrThreeDeviceCode;
                resBody.ip = GetUserIp();
                resBody.license = ConfigOperator.Single().StrThreeLicense;
                resBody.latitude = Convert.ToDouble(ConfigOperator.Single().StrThreeLatitude);
                resBody.longitude = Convert.ToDouble(ConfigOperator.Single().StrThreeLongitude);
                mymsg.responseBody = resBody;
                Trace.WriteLine(resBody.deviceCode);
                Trace.WriteLine(resBody.ip);
                Trace.WriteLine(resBody.license);
                Trace.WriteLine(resBody.latitude.ToString());
                Trace.WriteLine(resBody.longitude.ToString());
                string ss = Newtonsoft.Json.JsonConvert.SerializeObject(mymsg);
                Trace.WriteLine(ss);
                string strUrl = "";
                int nPlateChoose = ConfigOperator.Single().PlateChoose;
                strUrl = "http://10.84.2.174:9094/DeviceManageService/ACSServer.html";

                string InvokeKey = string.Empty;//GetContent(strUrl, ss);
                RequestPost(strUrl, mymsg, out InvokeKey);

                Trace.WriteLine("兵团网注册返回：" + InvokeKey);

                return DealRegistBack(InvokeKey, true);
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// 三所http接口
        /// 注册
        /// </summary>
        /// <returns></returns>
        public bool ThirdQueryRegist(bool bTwo = false)
        {
            if (ConfigOperator.Single().PlateChoose == 0)
                return true;

            try
            {
                ThirdRegistMsg mymsg = new ThirdRegistMsg();
                mymsg.responseName = "Register";
                responseBody resBody = new responseBody();
                resBody.deviceCode = ConfigOperator.Single().StrThreeDeviceCode;
                resBody.ip = GetUserIp();
                resBody.license = ConfigOperator.Single().StrThreeLicense;
                resBody.latitude = Convert.ToDouble(ConfigOperator.Single().StrThreeLatitude);
                resBody.longitude = Convert.ToDouble(ConfigOperator.Single().StrThreeLongitude);
                mymsg.responseBody = resBody;
                Trace.WriteLine(resBody.deviceCode);
                Trace.WriteLine(resBody.ip);
                Trace.WriteLine(resBody.license);
                Trace.WriteLine(resBody.latitude.ToString());
                Trace.WriteLine(resBody.longitude.ToString());
                string ss = Newtonsoft.Json.JsonConvert.SerializeObject(mymsg);
                Trace.WriteLine(ss);
                string strUrl = "";
                int nPlateChoose = ConfigOperator.Single().PlateChoose;
                if (nPlateChoose == 1 || nPlateChoose == 4)
                {
                    strUrl = "http://10.20.50.110:9094/DeviceManageService/ACSServer.html";
                }
                else if (nPlateChoose == 2)
                {
                    strUrl = "http://21.0.0.125:9094/DeviceManageService/ACSServer.html";
                }
                else if (nPlateChoose == 3)
                {
                    strUrl = "http://124.117.209.133:29094/DeviceManageService/ACSServer.html";
                    return true;
                }
                
                string InvokeKey = string.Empty;
                RequestPost(strUrl, ss, out InvokeKey);
                Trace.WriteLine("公安网注册返回：" + InvokeKey);
                
                return DealRegistBack(InvokeKey);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 三所http接口
        /// 黑名单清单获取
        /// </summary>
        /// <param name="mymsg"></param>
        /// <returns></returns>
        public string ThirdQueryBlack(ThirdBlackQurryMsg mymsg,string url)
        {
            if(ConfigOperator.Single().PlateChoose == 0)
            {
                return "{\"message\":\"成功\",\"status\":\"0\",\"jobId\":\"0100007F60BF3C6D0160E3B5470F6374\",\"isBlackList\":0}";
            }

            try
            {
                string strJson = string.Empty;
                string strHttpBack = string.Empty;

                strJson = Newtonsoft.Json.JsonConvert.SerializeObject(mymsg);
                Trace.WriteLine(strJson);
                //strHttpBack = GetContent(url, strJson);
                bool bRet = SendHttpRequest(url, mymsg, out strHttpBack); //RequestPost(url, mymsg, out strHttpBack);
                if (!bRet)
                {
                    //返回超时
                    return "{\"message\":\"失败：请求超时\",\"status\":\"-100\",\"jobId\":\"0100007F60BF3C6D0160E3B5470F6374\",\"isBlackList\":0}";
                }
                Trace.WriteLine(strHttpBack);
                return strHttpBack;
            }
            catch(Exception ex)
            {
                Trace.WriteLine("黑名单查询错误" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 三所http接口
        /// 上传选择信息
        /// </summary>
        /// <param name="mymsg"></param>
        /// <returns></returns>
        public string ThirdUpLoad(PersonInfoUpload mymsg,bool bTwo = false)
        {
            int nPlateChoose = ConfigOperator.Single().PlateChoose;

            if (nPlateChoose == 0)
            {
                return "{\"message\":\"调用成功\",\"status\":\"0\"}";
            }

            try
            {
                string ss = null;
                string tt = null;

                string strUrl = "";
                if (nPlateChoose == 1 || nPlateChoose == 4)
                {
                    strUrl = ConfigOperator.Single().UploadPersonUrlPolice; //"http://10.20.50.110:9092/verificationInterface/passlog/personLog";
                }
                else if (nPlateChoose == 2)
                {
                    strUrl = ConfigOperator.Single().UploadPersonUrlVedio;//"http://21.0.0.125:9092/verificationInterface/passlog/personLog";
                }
                else if (nPlateChoose == 3)
                {
                    strUrl = ConfigOperator.Single().UploadPersonUrlNet; //"http://124.117.209.133:29092/verificationInterface/passlog/personLog";
                }
                if (bTwo)
                    strUrl = ConfigOperator.Single().UploadPersonUrlArmy;// "http://10.84.2.174:9092/verificationInterface/passlog/personLog";
                ss = Newtonsoft.Json.JsonConvert.SerializeObject(mymsg);
                
                RequestPost(strUrl, mymsg, out tt);

                return tt;
            }
            catch
            {
                return null;
            }
        }

        public string ThirdUpLoadFileT(string fileName)
        {
            try
            {
                string strUrl = ConfigOperator.Single().UploadFileUrlArmy; //"http://10.84.2.174:9095/FileStorageService/FileUpload";
                if (fileName == "")
                {
                    return null;
                }
                Trace.WriteLine("arm start file Upload");
                string tt = fileUpload(strUrl, fileName);
                Trace.WriteLine(tt);
                return tt;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 三所http接口
        /// 上传图片信息
        /// </summary>
        /// <param name="myFilePath"></param>
        /// <returns></returns>
        public string ThirdUpLoadFile(string  myFilePath)
        {
            if(ConfigOperator.Single().PlateChoose == 0)
            {
                return "{\"status\":0,\"FilePath\":\"groupa/180111/22/f494cb55dbbd430abd10261e80ade9df.jpg\"}";
            }

            try
            {
                string strUrl = "";
                int nPlateChoose = ConfigOperator.Single().PlateChoose;
                if (nPlateChoose == 1 || nPlateChoose == 4)
                {
                    strUrl = ConfigOperator.Single().UploadFileUrlPolice;//"http://10.20.50.110:9095/FileStorageService/FileUpload";
                }
                else if (nPlateChoose == 2)
                {
                    strUrl = ConfigOperator.Single().UploadFileUrlVedio;// "http://21.0.0.125:9095/FileStorageService/FileUpload";
                }
                else if (nPlateChoose == 3)
                {
                    strUrl = ConfigOperator.Single().UploadFileUrlNet;// "http://124.117.209.133:29095/FileStorageService/FileUpload";
                }
                if (myFilePath == "")
                {
                    return null;
                }
                Trace.WriteLine("start file Upload");
                string tt = fileUpload(strUrl, myFilePath);
                Trace.WriteLine(tt);
                return tt;
            }
            catch
            {
                return null;
            }
        }
    }

}
