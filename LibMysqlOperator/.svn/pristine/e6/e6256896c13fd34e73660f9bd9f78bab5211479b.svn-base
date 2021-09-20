using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows;

namespace MySqlLibrary
{
    internal class DbManager
    {
        //連接用的字符串  
        private string connStr;
        public string ConnStr
        {
            get { return this.connStr; }
            set { this.connStr = value; }
        }

        private DbManager() { }

        //DbManager單實例  
        private static DbManager _instance = null;
        public static DbManager Ins
        {
            get { if (_instance == null) { _instance = new DbManager(); } return _instance; }
        }

       


        /// <summary>  
        /// 需要獲得多個結果集的時候用該方法，返回DataSet對象。  
        /// </summary>  
        /// <param name="sql語句"></param>  
        /// <returns></returns>  

        public DataSet ExecuteDataSet(string sql, params MySqlParameter[] paras)
        {
            using (MySqlConnection con = new MySqlConnection(ConnStr))
            {
                //數據適配器  
                MySqlDataAdapter sqlda = new MySqlDataAdapter(sql, con);
                sqlda.SelectCommand.Parameters.AddRange(paras);
                DataSet ds = new DataSet();
                sqlda.Fill(ds);
                return ds;
                //不需要打開和關閉鏈接.  
            }
        }

        /// <summary>  
        /// 獲得單個結果集時使用該方法，返回DataTable對象。  
        /// </summary>  
        /// <param name="sql"></param>  
        /// <returns></returns>  
        public DataTable ExcuteDataTable(string sql, params MySqlParameter[] paras)
        {
            using (MySqlConnection con = new MySqlConnection(ConnStr))
            {
                MySqlDataAdapter sqlda = new MySqlDataAdapter(sql, con);
                sqlda.SelectCommand.Parameters.AddRange(paras);
                DataTable dt = new DataTable();
                sqlda.Fill(dt);
                return dt;
            }
        }


        /// <summary>     
        /// 執行一條計算查詢結果語句，返回查詢結果（object）。     
        /// </summary>     
        /// <param name="SQLString">計算查詢結果語句</param>     
        /// <returns>查詢結果（object）</returns>     
        public object ExecuteScalar(string SQLString, params MySqlParameter[] paras)
        {
            using (MySqlConnection connection = new MySqlConnection(ConnStr))
            {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
                {
                    connection.Open();
                    cmd.Parameters.AddRange(paras);
                    object obj = cmd.ExecuteScalar();
                    if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
            }
        }

        /// <summary>  
        /// 執行Update,Delete,Insert操作  
        /// </summary>  
        /// <param name="sql"></param>  
        /// <returns></returns>  
        public int ExecuteNonquery(string sql, params MySqlParameter[] paras)
        {
            using (MySqlConnection con = new MySqlConnection(ConnStr))
            {
                MySqlCommand cmd = new MySqlCommand(sql, con);
                cmd.Parameters.AddRange(paras);
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>  
        /// 調用存儲過程 無返回值  
        /// </summary>  
        /// <param name="procname">存儲過程名</param>  
        /// <param name="paras">sql語句中的參數數組</param>  
        /// <returns></returns>  
        public int ExecuteProcNonQuery(string procname, params MySqlParameter[] paras)
        {
            using (MySqlConnection con = new MySqlConnection(ConnStr))
            {
                MySqlCommand cmd = new MySqlCommand(procname, con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(paras);
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>  
        /// 存儲過程 返回Datatable  
        /// </summary>  
        /// <param name="procname"></param>  
        /// <param name="paras"></param>  
        /// <returns></returns>  
        public DataTable ExecuteProcQuery(string procname, params MySqlParameter[] paras)
        {
            using (MySqlConnection con = new MySqlConnection(ConnStr))
            {
                MySqlCommand cmd = new MySqlCommand(procname, con);
                cmd.CommandType = CommandType.StoredProcedure;
                MySqlDataAdapter sqlda = new MySqlDataAdapter(procname, con);
                sqlda.SelectCommand.Parameters.AddRange(paras);
                DataTable dt = new DataTable();
                sqlda.Fill(dt);
                return dt;
            }
        }

        /// <summary>  
        /// 多語句的事物管理  
        /// </summary>  
        /// <param name="cmds">命令數組</param>  
        /// <returns></returns>  
        public bool ExcuteCommandByTran(params MySqlCommand[] cmds)
        {
            using (MySqlConnection con = new MySqlConnection(ConnStr))
            {
                con.Open();
                MySqlTransaction tran = con.BeginTransaction();
                foreach (MySqlCommand cmd in cmds)
                {
                    cmd.Connection = con;
                    cmd.Transaction = tran;
                    cmd.ExecuteNonQuery();
                }

                try
                {
                    //提交
                    tran.Commit();
                    return true;
                }
                catch
                {
                    //出错回滚  
                    tran.Rollback();
                    return false;
                }
            }
        }

        ///分頁  
        public DataTable ExcuteDataWithPage(string sql, ref int totalCount, params MySqlParameter[] paras)
        {
            using (MySqlConnection con = new MySqlConnection(ConnStr))
            {
                MySqlDataAdapter dap = new MySqlDataAdapter(sql, con);
                DataTable dt = new DataTable();
                dap.SelectCommand.Parameters.AddRange(paras);
                dap.Fill(dt);
                MySqlParameter ttc = dap.SelectCommand.Parameters["@totalCount"];
                if (ttc != null)
                {
                    totalCount = Convert.ToInt32(ttc.Value);
                }
                return dt;
            }
        }
    }
}
