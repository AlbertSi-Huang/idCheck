using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TesMessage2
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new Form1());
            }
            catch (Exception ex )
            {

                MessageBox.Show(ex.Message);
            }
           
        }
    }
}
