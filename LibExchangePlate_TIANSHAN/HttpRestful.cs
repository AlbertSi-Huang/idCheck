using System;
using System.IO;
using System.Net;
using System.Text;
// 添加https
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
// end添加https


namespace LibExchangePlate
{
    public enum HttpVerbNew
    {
        GET,            //method  常用的就这几样，可以添加其他的   get：获取    post：修改    put：写入    delete：删除
        POST,
        PUT,
        DELETE
    }


    public class ContentType//根据Postman整理，可以添加
    {
        public string Text = "text/plain";
        public string JSON = "application/json";
        public string Javascript = "application/javascript";
        public string XML = "application/xml";
        public string TextXML = "text/xml";
        public string HTML = "text/html";
    }


    public class RestApiClient
    {
        public string EndPoint { get; set; }    //请求的url地址  
        public HttpVerbNew Method { get; set; }    //请求的方法
        public string ContentType { get; set; } //格式类型
        public string PostData { get; set; }    //传送的数据


        public RestApiClient()
        {
            EndPoint = "";
            Method = HttpVerbNew.POST;
            ContentType = "application/json";
            PostData = "";
        }
        public RestApiClient(string endpoint, string contentType)
        {
            EndPoint = endpoint;
            Method = HttpVerbNew.GET;
            ContentType = contentType;
            PostData = "";
        }
        public RestApiClient(string endpoint, HttpVerbNew method, string contentType)
        {
            EndPoint = endpoint;
            Method = method;
            ContentType = contentType;
            PostData = "";
        }


        public RestApiClient(string endpoint, HttpVerbNew method, string contentType, string postData)
        {
            EndPoint = endpoint;
            Method = method;
            ContentType = contentType;
            PostData = postData;
        }


        // 添加https
        private static readonly string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";


        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受     
        }
        // end添加https


        public string MakeRequest()
        {
            return MakeRequest("");
        }


        public string MakeRequest(string parameters)
        {
            var request = (HttpWebRequest)WebRequest.Create(EndPoint + parameters);


            // 添加https
            if (EndPoint.Substring(0, 8) == "https://")
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            }
            // end添加https


            request.Method = Method.ToString();
            request.ContentLength = 0;
            request.ContentType = ContentType;


            if (!string.IsNullOrEmpty(PostData) && Method == HttpVerbNew.POST)//如果传送的数据不为空，并且方法是post
            {
                var encoding = new UTF8Encoding();
                var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(PostData);//编码方式按自己需求进行更改，我在项目中使用的是UTF-8
                request.ContentLength = bytes.Length;


                using (var writeStream = request.GetRequestStream())
                {
                    writeStream.Write(bytes, 0, bytes.Length);
                }
            }


            if (!string.IsNullOrEmpty(PostData) && Method == HttpVerbNew.PUT)//如果传送的数据不为空，并且方法是put
            {
                var encoding = new UTF8Encoding();
                var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(PostData);//编码方式按自己需求进行更改，我在项目中使用的是UTF-8
                request.ContentLength = bytes.Length;


                using (var writeStream = request.GetRequestStream())
                {
                    writeStream.Write(bytes, 0, bytes.Length);
                }
            }
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;


                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }


                // grab the response
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                        using (var reader = new StreamReader(responseStream))
                        {
                            responseValue = reader.ReadToEnd();
                        }
                }


                return responseValue;
            }
        }


        public bool CheckUrl(string parameters)
        {
            bool bResult = true;


            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(EndPoint + parameters);
            myRequest.Method = Method.ToString();             //设置提交方式可以为＂ｇｅｔ＂，＂ｈｅａｄ＂等
            myRequest.Timeout = 10000;　             //设置网页响应时间长度
            myRequest.AllowAutoRedirect = false;//是否允许自动重定向
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
            bResult = (myResponse.StatusCode == HttpStatusCode.OK);//返回响应的状态


            return bResult;
        }
    }
}

