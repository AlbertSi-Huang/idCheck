using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
//using System.Net.Sockets;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace doublescreen
{



    /// <summary>  
    /// 身份证查询实体  
    /// </summary>  
    public class ThirdQuerryByIdMsg
    {
        public string method { get; set; }
        public string carNo { get; set; }
        public string personName { get; set; }
        public string invokeKey { get; set; }
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
    public class ThirdRegistReturnMsg
    {
        public string message { get; set; }
        public string status { get; set; }
        public string requestName { get; set; }
        public string holdRequests { get; set; }
        public ThirdRegistReturnBody ThirdRegistReturnBody { get; set; }

    }
    public class ThirdRegistReturnBody
    {
        public string token { get; set; }
        public string validity { get; set; }
    }


    public class ThirdBlackQurryMsg
    {
        public string cardNo { get; set; }
        public string deviceCode { get; set; }
        public string passTime { get; set; }
        public int collectionType { get; set; }
        public string extendInfo1 { get; set; }
        public string extendInfo2 { get; set; }
        public string extendInfo3 { get; set; }
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
        public string collectionType { get; set; }
        public string passNo { get; set; }
        public string inOrOut { get; set; }
        public string verifyResult { get; set; }
        public string passTime { get; set; }

        //身份证信息
        public string name { get; set; }
        public string genderCode { get; set; }
        public string nationCode { get; set; }
        public string nationality { get; set; }
        public string birthday { get; set; }
        public string addr { get; set; }
        public string cardNo { get; set; }
        public string cardStartTime { get; set; }
        public string cardEndTime { get; set; }
        public string cardType { get; set; }
        public string issuingUnit { get; set; }
        public string hasFingerFea { get; set; }
        public string fingerFeature0 { get; set; }
        public string fingerFeature1 { get; set; }

        //图片信息
        public string cardImgPath { get; set; }
        public string sceneImgPath { get; set; }
        public string faceImgPath { get; set; }
        public string fingerImgPath { get; set; }
        public string faceCompareResult { get; set; }
        public string faceQualityScore { get; set; }
        public string faceCompareScore { get; set; }

        //指纹信息
        public string fingerCompareResult { get; set; }
        public string fingerCompareFeatureId { get; set; }
        public string fingerQualityScore { get; set; }
        public string fingerCompareScore { get; set; }

        //酒店信息
        public string checkInTime { get; set; }
        public string leaveTime { get; set; }
        public string roomNum { get; set; }

        //票务信息
        public string hasTicket { get; set; }
        public string stationNo { get; set; }
        public string ticketNo { get; set; }
        public string ticketType { get; set; }
        public string trainNo { get; set; }
        public string seat { get; set; }
        public string goTime { get; set; }
        public string goAddr { get; set; }
        public string arriveTime { get; set; }
        public string arriveAddr { get; set; }

        //预留信息
        public string extendInfo1 { get; set; }
        public string extendInfo2 { get; set; }
        public string extendInfo3 { get; set; }
    }
    class  SanTranProto
    {
        public static string myInvokeKey{ get; set; }
        ThirdBlackQurryMsg myTranMsgQuerry = new ThirdBlackQurryMsg()
        {
            cardNo = "412721198911123875",
            deviceCode = "CV33000277159299000235",
            passTime = "2017-09-29 13:45:38",
            collectionType = 1,
            extendInfo1 = "xxx",
            extendInfo2 = "xxx",
            extendInfo3 = "xxx"
        };
        private static SanTranProto instance = null;
        public static SanTranProto Single()
        {
            if (instance == null)
            {
                instance = new SanTranProto();
            }
            return instance;
        }
  
        public string prepareJson()
        {
            string ss;
            /*         
                   DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(ThirdTranMsg));
                   MemoryStream msObj = new MemoryStream();
                       //将序列化之后的Json格式数据写入流中
                   js.WriteObject(msObj, myTranMsg);
                   msObj.Position = 0;
                       //从0这个位置开始读取流中的数据
                   StreamReader sr = new StreamReader(msObj, Encoding.UTF8);
                   string json = sr.ReadToEnd();
                   sr.Close();
                   msObj.Close();
              //     return json;
                     //  Console.WriteLine(json);
              */
            myTranMsgQuerry.collectionType = 3;
            ss = Newtonsoft.Json.JsonConvert.SerializeObject(myTranMsgQuerry);

            return ss;
        }
        private string GetContent(string mystrpath, string myQuery)
        {


            byte[] payload;
            //    string str_path = "http://192.168.2.143:8009/" + mystrpath;
            string str_path = mystrpath;
             //"verificationInterface/person/personTypeQuery";
             //or url    
             // string str_path = "http://sports.sina.com.cn/mynba/"; 
             //or url
             // string yy = "12345678980";
            StringBuilder sb = new StringBuilder();
            HttpWebRequest request;
            HttpWebResponse response;
            request = (HttpWebRequest)(WebRequest.Create(str_path));
            request.Method = "POST";
            request.ContentType = "application/json";
            payload = System.Text.Encoding.UTF8.GetBytes(myQuery);
            request.ContentLength = payload.Length;
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

            return sb.ToString();
        }
        private string GetContent()
        {
            byte[] payload = { 1, 2, 3, 4, 5, 6, 7, 8 };
            string str_path = "http://192.168.2.143:8009/verificationInterface/person/personTypeQuery";
            //or url                                                                                                
            // string str_path = "http://sports.sina.com.cn/mynba/"; //or url   
            // string yy = "12345678980";
            StringBuilder sb = new StringBuilder();
            HttpWebRequest request;
            HttpWebResponse response;
            request = (HttpWebRequest)(WebRequest.Create(str_path));
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = payload.Length;
            Stream newStream = request.GetRequestStream();
            newStream.Write(payload, 0, payload.Length);
            newStream.Close();



            //  payload = System.Text.Encoding.UTF8.GetBytes(yy);



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

            return sb.ToString();
        }
        public string  ThirdQuerryById(ThirdQuerryByIdMsg mymsg)
        {
            string mystr = "http://124.117.209.133:29092/verificationInterface/dataQuery/queryPersonInfo";
            string ss = Newtonsoft.Json.JsonConvert.SerializeObject(mymsg);
            GetContent(mystr,ss);
            return ss;
        }
        public string ThirdQuerryRegist(ThirdRegistMsg mymsg)
        {
            string mystr = "http://10.20.50.110:9094/DeviceManageService/ ACSServer.html";
            string ss = Newtonsoft.Json.JsonConvert.SerializeObject(mymsg);
            GetContent(mystr, ss);
            return ss;
        }

        public string ThirdQuerryBlack(ThirdBlackQurryMsg mymsg)
        {
            string mystr = "http://10.84.2.174:9092/passlog/blackList";
            string ss = Newtonsoft.Json.JsonConvert.SerializeObject(mymsg);
            GetContent(mystr, ss);
            return ss;
        }
        public string ThirdUpLoad(ThirdBlackQurryMsg mymsg)
        {
            string mystr = "http://10.84.2.174:9095/FileStorageService/FileUpload";
            string ss = Newtonsoft.Json.JsonConvert.SerializeObject(mymsg);
            GetContent(mystr, ss);
            return ss;
        }
        public string test()
        {
            return null;
            /*   照片查询
            ThirdQuerryByIdMsg mymsgtmp = new ThirdQuerryByIdMsg();
            mymsgtmp.method = "querryPersonInfo";
            mymsgtmp.carNo = "230828198909243514";
            mymsgtmp.personName = "刘子龙";
            mymsgtmp.invokeKey = "F05949BEEA3BCE3F211ACA5569D3F6D3";
           return  ThirdQuerryById(mymsgtmp);
           */
        }
    } 
}
