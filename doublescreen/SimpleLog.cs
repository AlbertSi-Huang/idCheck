using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Data;
using System.Windows;

namespace doublescreen
{
    public class SimpleLog : TraceListener
    {
        public string FilePath
        {
            get;
            set;
        }
        System.Timers.Timer _timeCheckDate = null;
        string _strFilePath = string.Empty;
        string _strTodayDate = string.Empty;
        public SimpleLog(string fileName)
        {
            string strFilePath = Assembly.GetExecutingAssembly().Location;
            int nPos = strFilePath.LastIndexOf('\\');
            strFilePath = strFilePath.Substring(0, nPos);
            DateTime dt = DateTime.Now;
            string strDay = dt.ToString("yyyyMMdd");
            string strFileFullPath = strFilePath + @"\log\" + fileName + "_" + strDay + ".log";
            _strFilePath = strFilePath + @"\log\" + fileName;
            _strTodayDate = strDay;
            if (!Directory.Exists(strFilePath + @"\log\"))
            {
                Directory.CreateDirectory(strFilePath + @"\log\");
            }
            _timeCheckDate = new System.Timers.Timer(60 * 1000 * 30);//半个小时检查一次
            _timeCheckDate.Elapsed += TimeCheckDateElapsed;
            _timeCheckDate.AutoReset = true;
            _timeCheckDate.Enabled = true;

            FilePath = strFileFullPath;
        }

        private void TimeCheckDateElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            DateTime dt = DateTime.Now;
            string strDay = dt.ToString("yyyyMMdd");
            if (strDay.CompareTo(_strTodayDate) != 0)
            {
                string fileFullName = _strFilePath + "_" + strDay + ".log";
                FilePath = fileFullName;
                _strTodayDate = strDay;
            }
        }

        public override void Write(string message)
        {
            File.AppendAllText(FilePath, message);
        }

        public void LogWriteBegin()
        {
            WriteLine("************************************************************");
            WriteLine("*                                                          *");
            WriteLine("*                                                          *");
            WriteLine("*                    application begin                     *");
            WriteLine("*                                                          *");
            WriteLine("*                                                          *");
            WriteLine("************************************************************");
        }

        public override void WriteLine(string message)
        {
            DateTime dt = DateTime.Now;
            StringBuilder sb = new StringBuilder();
            sb.Append(dt.ToString("yyyy-MM-dd HH:mm:ss")).AppendFormat(".{0:D3}    ", dt.Millisecond);
            sb.Append(message).Append(Environment.NewLine);
            File.AppendAllText(FilePath, sb.ToString());
        }

        public void WriteLine(StringBuilder message)
        {
            DateTime dt = DateTime.Now;
            StringBuilder sb = new StringBuilder();
            sb.Append(dt.ToString("yyyy-MM-dd HH:mm:ss")).AppendFormat(".{0:D3}    ", dt.Millisecond);
            sb.Append(message).Append(Environment.NewLine);
            File.AppendAllText(FilePath, sb.ToString());
        }

        public override void Write(object o, string category)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(category))
            {
                sb.Append(category).Append(":");
            }
            if (o is Exception)//如果参数对象o是与Exception类兼容,输出异常消息+堆栈,否则输出o.ToString()
            {
                var ex = (Exception)o;
                sb.Append(ex.Message).Append(Environment.NewLine);
                sb.Append(ex.StackTrace);
            }
            else if (null != o)
            {
                sb.Append(o.ToString());
            }

            WriteLine(sb);
        }
    }
}
