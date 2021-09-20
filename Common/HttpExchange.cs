using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public struct SParam
    {
        public int _nType;
        public string _strKey;
        public string _strValue;
    }

    public class HttpExchange
    {
        /// <summary>
        /// Post
        /// </summary>
        /// <param name="URL"></param>
        /// <param name="data"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool RequestPost(string URL, object data, out string msg,int nType )
        {
            msg = "";
            try
            {
                var requestjson = JsonConvert.SerializeObject(data);
                HttpContent httpcontent = new StringContent(requestjson);
                httpcontent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                
                HttpClientHandler handle = new HttpClientHandler();
                handle.AutomaticDecompression = System.Net.DecompressionMethods.GZip;
                HttpClient httpclient = new HttpClient(handle);
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
        /// Post
        /// </summary>
        /// <param name="URL"></param>
        /// <param name="data"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool RequestPost(string URL, object data, out string msg)
        {
            msg = "";
            try
            {
                var requestjson = JsonConvert.SerializeObject(data);
                HttpContent httpcontent = new StringContent(requestjson);
                httpcontent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                //httpcontent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

                HttpClientHandler handle = new HttpClientHandler();
                handle.AutomaticDecompression = System.Net.DecompressionMethods.GZip;
                HttpClient httpclient = new HttpClient(handle);
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
        
        public async void HttpPost(string URL, Dictionary<string, string> dic)
        {
            HttpClient httpClient = new HttpClient();
            //var requestjson = JsonConvert.SerializeObject(data);
            var content = new FormUrlEncodedContent(dic);
            HttpResponseMessage response = await httpClient.PostAsync(URL, content);
            response.EnsureSuccessStatusCode();
            string resultStr = await response.Content.ReadAsStringAsync();

        }

        /// <summary>  
        /// Http同步Post异步请求  
        /// </summary>  
        /// <param name="url">Url地址</param>  
        /// <param name="postStr">请求Url数据</param>  
        /// <param name="callBackUploadDataCompleted">回调事件</param>  
        /// <param name="encode"></param>  
        public void HttpPostAsync(string url, string postStr = "",
            UploadDataCompletedEventHandler callBackUploadDataCompleted = null, Encoding encode = null)
        {
            var webClient = new WebClient { Encoding = Encoding.UTF8 };

            if (encode != null)
                webClient.Encoding = encode;

            var sendData = Encoding.GetEncoding("GB2312").GetBytes(postStr);

            webClient.Headers.Add("Content-Type", "application/json");//"application/x-www-form-urlencoded");
            webClient.Headers.Add("ContentLength", sendData.Length.ToString(CultureInfo.InvariantCulture));

            if (callBackUploadDataCompleted != null)
                webClient.UploadDataCompleted += callBackUploadDataCompleted;

            webClient.UploadDataAsync(new Uri(url), "POST", sendData);
        }

        public bool RequestGet(string URL, out string outMsg)
        {
            bool bRet = false;
            outMsg = "";

            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "GET";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = "application/json";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    outMsg = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return bRet;
            }


            return bRet;
        }

        private byte[] GetPhotoToByte(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                Trace.WriteLine(imagePath + "文件不存在");
                return null;
            }

            FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8, FileOptions.Asynchronous); //new FileStream(imagePath, FileMode.Open);
            byte[] byteData = new byte[fs.Length];
            fs.Read(byteData, 0, byteData.Length);
            fs.Close();
            return byteData;
        }

        string _urlIp = string.Empty;

        public bool PostFileParam(string url, List<SParam> postParams, out string strOutMsg)
        {
            Dictionary<string, object> DicParam = new Dictionary<string, object>();
            for (int i = 0; i < postParams.Count; ++i)
            {
                if (postParams[i]._nType == 1)
                {
                    DicParam.Add(postParams[i]._strKey, postParams[i]._strValue);
                }
                else
                {
                    byte[] data = GetPhotoToByte(postParams[i]._strValue);
                    if (data == null) continue;

                    DicParam.Add(postParams[i]._strKey, new FormUpload.FileParameter(data, postParams[i]._strValue));
                }
            }
            //int nPos = url.IndexOf(':',7);
            //if(nPos == -1)
            //{
            //    strOutMsg = "";
            //    return false;
            //}
            //string strTmpIp = url.Substring(7, nPos - 7);
            //bool b = Ping(strTmpIp);
            //if (!b)
            //{
            //    strOutMsg = "";
            //    return false;
            //}
            try
            {
                HttpWebResponse webResponse = FormUpload.MultipartFormDataPost(url, "", DicParam);
                StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
                string fullResponse = responseReader.ReadToEnd();
                strOutMsg = fullResponse;
                webResponse.Close();
            }
            catch (Exception ex)
            {
                //Trace.WriteLine(ex.Message);
                strOutMsg = ex.Message;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 指定Post地址使用Get 方式获取全部字符串
        /// </summary>
        /// <param name="url">请求后台地址</param>
        /// <returns></returns>
        public bool PostKeyAndValue(string url, Dictionary<string, string> dic,out string outMsg)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            #region 添加Post 参数
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in dic)
            {
                if (i > 0)
                    builder.Append("&");
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }
            string str = builder.ToString();

            byte[] data = Encoding.UTF8.GetBytes(str);
            
            req.ContentLength = data.Length;
            try
            {
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                }
                #endregion
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream stream = resp.GetResponseStream();
                //获取响应内容
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
            }catch(Exception ex)
            {
                outMsg = ex.Message;
                return false;
            }
            
            outMsg = result;
            return true;
        }
        

        public bool Ping(string ip)
        {
            System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
            System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions();
            options.DontFragment = true;
            string data = "Test Data!";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 2000; // Timeout 时间，单位：毫秒
            try
            {
                System.Net.NetworkInformation.PingReply reply = p.Send(ip, timeout, buffer, options);
                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return false;
            }

        }


    }

    public static class FormUpload
    {
        private static readonly Encoding encoding = Encoding.UTF8;
        public static HttpWebResponse MultipartFormDataPost(string postUrl, string userAgent, Dictionary<string, object> postParameters)
        {
            string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
            string contentType = "multipart/form-data; boundary=" + formDataBoundary;  
            //string contentType = "application/x-www-form-urlencoded" + formDataBoundary;


            byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

            return PostForm(postUrl, userAgent, contentType, formData);
        }
        private static HttpWebResponse PostForm(string postUrl, string userAgent, string contentType, byte[] formData)
        {
            HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

            if (request == null)
            {
                throw new NullReferenceException("request is not a http request");
            }

            // Set up the request properties.  
            request.Method = "POST";
            request.ContentType = contentType;
            request.UserAgent = userAgent;
            request.CookieContainer = new CookieContainer();
            request.ContentLength = formData.Length;

            // You could add authentication here as well if needed:  
            // request.PreAuthenticate = true;  
            // request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;  
            // request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes("username" + ":" + "password")));  

            // Send the form data to the request.  
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }

            return request.GetResponse() as HttpWebResponse;
        }

        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            Stream formDataStream = new System.IO.MemoryStream();
            bool needsCLRF = false;

            foreach (var param in postParameters)
            {
                // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.  
                // Skip it on the first parameter, add it to subsequent parameters.  
                if (needsCLRF)
                    formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                needsCLRF = true;

                if (param.Value is FileParameter)
                {
                    FileParameter fileToUpload = (FileParameter)param.Value;

                    string strName = "files";//param.Key;
                    string strFileName = fileToUpload.FileName ?? param.Key;
                    // Add just the first part of this param, since we will write the file data directly to the Stream  
                    string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                        boundary,
                        strName,
                        //wfc 对接card平台 将file全部改为files
                        //"files",
                        strFileName,
                        fileToUpload.ContentType ?? "application/octet-stream");

                    formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                    // Write the file data directly to the Stream, rather than serializing it to a string.  

                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                }
                else
                {
                    string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                        boundary,
                        param.Key,
                        param.Value);
                    formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                }
            }

            // Add the end of the request.  Start with a newline  
            string footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

            // Dump the Stream into a byte[]  
            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();

            return formData;
        }

        public class FileParameter
        {
            public byte[] File { get; set; }
            public string FileName { get; set; }
            public string ContentType { get; set; }
            public FileParameter(byte[] file) : this(file, null) { }
            public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
            public FileParameter(byte[] file, string filename, string contenttype)
            {
                File = file;
                FileName = filename;
                ContentType = contenttype;
            }
        }
    }
}
