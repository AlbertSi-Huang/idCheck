using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TS_IDCheck.http
{
    public class CPHttpClient
    {
        private string _strHttpUrl;
        private string _strHttpServerIp;
        private string _strHttpServerPort;

        public CPHttpClient(string ip,string port)
        {
            this._strHttpServerIp = ip;
            this._strHttpServerPort = port;
            this._strHttpUrl = "http://" + _strHttpServerIp + ":" + _strHttpServerPort + "/";
            
            this._strHttpUrl += @"verificationInterface/dataQuery/";
        }

        private static readonly string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

        private  bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受     
        }

        public HttpWebResponse CreatePostHttpResponse(string url, IDictionary<string, string> parameters, Encoding charset)
        {
            HttpWebRequest request = null;
            //HTTPSQ请求  
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            request = WebRequest.Create(url) as HttpWebRequest;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = DefaultUserAgent;
            //如果需要POST数据     
            if (!(parameters == null || parameters.Count == 0))
            {
                StringBuilder buffer = new StringBuilder();
                int i = 0;
                foreach (string key in parameters.Keys)
                {
                    if (i > 0)
                    {
                        buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}={1}", key, parameters[key]);
                    }
                    i++;
                }
                byte[] data = charset.GetBytes(buffer.ToString());
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            return request.GetResponse() as HttpWebResponse;
        }

        /// <summary>
        /// 查找常驻人员接口
        /// </summary>
        /// <param name="idNum"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public  string SearchResidentPopleInfo(string idNum,string name = "")
        {
            string strRet = "";
            try
            {
                var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.None };
                string uuidN = Guid.NewGuid().ToString("N");
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    Encoding encoding = Encoding.GetEncoding("utf-8");
                    IDictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("method", "queryPersonInfo");
                    parameters.Add("cardNo", idNum);
                    parameters.Add("personName", name);
                    parameters.Add("invokeKey",uuidN);
                    HttpWebResponse response = CreatePostHttpResponse(_strHttpUrl, parameters, encoding);
                    using(Stream sm = response.GetResponseStream())
                    {
                        StreamReader sr = new StreamReader(sm);
                        strRet = sr.ReadToEnd();
                        sr.Close();
                    }
                    
                }  
            }
            catch(Exception ex)
            {
                strRet = ex.Message;
                return strRet;
            }
            
            return strRet;
        }


    }
}
