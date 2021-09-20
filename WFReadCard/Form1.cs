using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WFReadCard
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            int nRet = SDReadIdCard.SDT_OpenPort(1001);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            byte[] CardPUCIIN = new byte[255];
            byte[] pucManaMsg = new byte[255];
            byte[] pucCHMsg = new byte[255];
            byte[] pucPHMsg = new byte[3024];
            UInt32 puiCHMsgLen = 0;
            UInt32 puiPHMsgLen = 0;
            int st = 0;
            //读卡操作
            st = SDReadIdCard.SDT_StartFindIDCard(1, CardPUCIIN, 1);
            if (st != 0x9f) return;
            st = SDReadIdCard.SDT_SelectIDCard(1, pucManaMsg, 1);
            if (st != 0x90) return;
            st = SDReadIdCard.SDT_ReadBaseMsg(1, pucCHMsg, ref puiCHMsgLen, pucPHMsg, ref puiPHMsgLen, 1);
            if (st != 0x90) return;
            //显示结果
            textBox1.Text = System.Text.ASCIIEncoding.Unicode.GetString(pucCHMsg);
        }
    }
}
