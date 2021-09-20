using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows;

namespace MySqlLibrary
{
    public delegate void ExceptionEventHandler(StackFrame stackFrame, string databaseName, string tableName, StringBuilder sbSql, IEnumerable<MySqlParameter> paras, Exception ex);
    public delegate void LogEventHandler(StackFrame stackFrame, string databaseName, string tableName, StringBuilder sbSql, IEnumerable<MySqlParameter> paras);
    public static class DbHandle
    {
        public static event ExceptionEventHandler ExceptionEvent;
        public static event LogEventHandler LogEvent;

        // 定義一個標識確保線程同步
        private static readonly object locker = new object();

        public static bool Init(string databaseName, string dataSource, string userId, string password)
        {
            lock (locker)
            {
                //Trace.TraceInformation("新日志 " + databaseName + dataSource + userId + password);
                //DbManager.Ins.ConnStr = "Data Source='localhost';User Id='root';Password='123456';charset='utf8mb4';pooling=true";
                string connStr = $"Data Source='{dataSource}';";
                connStr += $"User Id='{userId}';";
                connStr += $"Password='{password}';";
                connStr += "charset = 'utf8mb4'; pooling = true; SslMode=none";
                DbManager.Ins.ConnStr = connStr;
                MySqlParameter[] paras =
                {
                    new MySqlParameter(),
                    new MySqlParameter(),
                };

                StringBuilder sbSql = new StringBuilder();
                sbSql.Append("create database if not exists  ").Append(databaseName).Append(" character set=utf8mb4");
                //string strSql = "create database if not exists  " + databaseName + " character set=utf8mb4";
                try
                {
                    DbManager.Ins.ExecuteNonquery(sbSql.ToString(), paras);
                }
                catch (Exception ex)
                {
                    DbHandle.ExceptionProcess(new StackFrame(true), databaseName, null, sbSql, paras, ex);
                    return false;
                }
            }

            //Trace.TraceInformation("InitDatabase end......");
            return true;
        }

        public static void ExceptionProcess(StackFrame stackFrame,string databaseName, string tableName, StringBuilder sbSql, IEnumerable<MySqlParameter> paras, Exception ex)
        {
            if(ExceptionEvent != null)
                ExceptionEvent(stackFrame, databaseName, tableName, sbSql, paras, ex);
        }

        public static void LogOutput(StackFrame stackFrame, string databaseName, string tableName, StringBuilder sbSql, IEnumerable<MySqlParameter> paras)
        {
            if (LogEvent != null)
                LogEvent(stackFrame, databaseName, tableName, sbSql, paras);
        }

        public static object ToDbType(object value)
        {
            if (value != null)
                return value;
            else
                return DBNull.Value;
        }
    }
}
