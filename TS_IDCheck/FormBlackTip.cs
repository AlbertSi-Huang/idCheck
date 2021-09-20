using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TS_IDCheck
{
    public partial class FormBlackTip : Form
    {
        System.Timers.Timer _timeShow = null;
        public FormBlackTip()
        {
            InitializeComponent();
            

            _timeShow = new System.Timers.Timer(500);
            _timeShow.AutoReset = true;
            _timeShow.Elapsed += TimeShowElapsed;
        }

        delegate void CloseThisWindow();

        void OnCloseThisWindow()
        {
            _timeShow.Stop();
            this.Close();
        }

        void ShowBackGroupColor()
        {
            if(this.BackColor == Color.Red)
            {
                this.BackColor = Color.Green;
            }else if(this.BackColor == Color.Green)
            {
                this.BackColor = Color.Red;
            }
        }
        private void TimeShowElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Invoke(new CloseThisWindow(ShowBackGroupColor));
        }

        public void CloseBlackTipWindow()
        {
            this.Invoke(new CloseThisWindow(OnCloseThisWindow));
        }
        private void FormBlackTip_Load(object sender, EventArgs e)
        {
            this.Width = 1024;
            this.Height = 600;
            this.labTip.Width = 1024;
            this.labTip.Height = 600;
            //红绿闪烁
            //_timeShow.Start();
        }

        public void ShowSelfDialog()
        {
            //this.Show();
            //this.Visible = true;
            //_timeShow.Start();
        }

    }
}
