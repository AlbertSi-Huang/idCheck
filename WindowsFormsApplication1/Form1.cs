using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //string str = string.Empty;
            //MessageBox.Show(str.Length.ToString());
        }
        [DllImport("GetTicketLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern static void GetTicketInfo(string url, string signNum, StringBuilder strOut);
        StringBuilder outInfoData = new StringBuilder(1024);

        private bool IsCheckTicketSuccess(string url, string strNum)
        {
            bool bRet = false;
            try
            {
                string strUrl = url;//ConfigOperator.Single().GetTicketUrl;
                GetTicketInfo(strUrl, strNum, outInfoData);
                string strRet = outInfoData.ToString();
                if(strRet.IndexOf("error") != -1)
                {
                    this.richTextBox1.Text = strRet;
                    return false;
                }
                this.richTextBox1.Text = strRet;
                string[] ss = strRet.Split('|');
                if (ss != null && ss.Length > 0 && ss[0].CompareTo("0") == 0)
                {
                    //MessageBox.Show()
                }
                else
                {
                    string msg = ss[1];
                }
                return bRet;
            }
            catch (Exception ex)
            {
                //Trace.WriteLine("IsCheckTicketSuccess error: " + ex.Message);
                this.richTextBox1.Text = ex.Message;
                return bRet;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Text = "";
            IsCheckTicketSuccess(this.textBox1.Text, this.textBox2.Text);
        }
    }
}
