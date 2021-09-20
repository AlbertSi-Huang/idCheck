using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/***********************
 * 功能：用户ID
 * 作者：刘飞翔
 * 时间：2018-4-25
 * 
 * 读取txt文件中的用户ID
 * *********************/

namespace LibReadCard
{
    public sealed class PassportUserID
    {
        #region 字段定义
        private string sUserID = "";
        private string sPath = "UserId.txt";
        #endregion

        /// <summary>
        /// 缺省构造
        /// </summary>
        private PassportUserID()
        {
        }

        private static PassportUserID cPassPortUserID = new PassportUserID();

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <returns></returns>
        public static PassportUserID CreateInstance()
        {
            if (cPassPortUserID == null)
            {
                cPassPortUserID = new PassportUserID();
            }
            return cPassPortUserID;
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <returns></returns>
        public static PassportUserID CreateInstance(string sPath)
        {
            if (cPassPortUserID == null)
            {
                cPassPortUserID = new PassportUserID();
            }
            cPassPortUserID.sPath = sPath;
            return cPassPortUserID;
        }

        /// <summary>
        /// 获取UserID
        /// </summary>
        /// <returns></returns>
        public static string GetUserID()
        {
            if (cPassPortUserID != null)
            {
                cPassPortUserID.SetUserID();
                return cPassPortUserID.sUserID;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 根据文件设置UserID
        /// </summary>
        private void SetUserID()
        {
            if (cPassPortUserID != null)
            {
                using (StreamReader sr = new StreamReader(cPassPortUserID.sPath, Encoding.Default))
                {
                    String line = sr.ReadLine();//读取第一行
                    cPassPortUserID.sUserID = line.Substring(line.IndexOf('=') + 1, line.Length - line.IndexOf('=')-1);
                }
            }
        }
    }
}
