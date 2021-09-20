using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace TS_IDCheck.log
{
    public class SimpleLog : TraceListener
    {
        public string FilePath
        {
            get;
            private set;
        }

        public SimpleLog(string filePath)
        {
            FilePath = filePath;
        }

        public override void Write(string message)
        {
            File.AppendAllText(FilePath, message);
        }

        public override void WriteLine(string message)
        {
            File.AppendAllText(FilePath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss    ") + message + Environment.NewLine);
        }

        public override void Write(object o, string category)
        {
            string message = string.Empty;
            if (!string.IsNullOrEmpty(category))
            {
                message = category + ":";
            }
            if (o is Exception)//如果参数对象o是与Exception类兼容,输出异常消息+堆栈,否则输出o.ToString()
            {
                var ex = (Exception)o;
                message += ex.Message + Environment.NewLine;
                message += ex.StackTrace;
            }
            else if (null != o)
            {
                message += o.ToString();
            }

            WriteLine(message);
        }
    }
}
