using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace doublescreen
{
    public partial class FormMsgShow : Form
    {
        public FormMsgShow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 设置提示字符串
        /// </summary>
        /// <param name="msg"></param>
        public void SetMsg(string msg)
        {
            try
            {
                lbMsg.Text = msg;
            }
            catch
            {
            }
        }

        private void FormMsgShow_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.Screen[] s1 = System.Windows.Forms.Screen.AllScreens;
            if (s1.Length == 1)
            {
                this.Location = new Point(0, 0);
            }
            else if (s1.Length == 2)
            {
                this.Location = new Point(1024, 0);

                System.Drawing.Rectangle r1 = s1[1].WorkingArea;
                this.Top = r1.Top;
                this.Left = 1024 + r1.Left;
                this.Width = r1.Width/4;
                this.Height = r1.Height/4;
            }
        }


    }
}
