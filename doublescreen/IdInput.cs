using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace doublescreen
{
    /// <summary>
    /// 查询窗体
    /// </summary>
    public partial class IdInput : Form
    {

        SocketService myIdSocService = null;
        Button[] btns = new Button[11];
        //    public event fmainwindow.SubFormExitHandle MyFormExit;

        //   public event InputFormRecvHandler InputFormUpdata;
        public delegate void IdInputRefreshhandle(string ss);
      //  public static bool thisFormClosed = false;
        public IdInput(SocketService mySocService)
        {
            InitializeComponent();
            btns[0] = btn0;
            btns[1] = btn1;
            btns[2] = btn2;
            btns[3] = btn3;
            btns[4] = btn4;
            btns[5] = btn5;
            btns[6] = btn6;
            btns[7] = btn7;
            btns[8] = btn8;
            btns[9] = btn9;
            btns[10] = btn10;
            for (Int32 i=0;i<11;i++)
            {
                btns[i].Click += new System.EventHandler(this.numBtnClick);
            }
            myIdSocService = mySocService;

        }

        private delegate void InvokeCallback(string msg);

        private void  RecUpdateInputFrom(string ss)
        {
            try
            {
                if (picIdFromNet.InvokeRequired)
                {
                    InvokeCallback mi = new InvokeCallback(RecUpdateInputFrom);
                    this.Invoke(mi, new object[] { ss });
                    //picIdFromNet.Invoke(new InvokeCallback(RecUpdateInputFrom), new object[] { ss });//回调 else//当前线程是创建线程（界面线程）   
                }
                else
                {
                    MTWFile.Single().WriteLine("会崩溃？？？？？？");
                    if (ss == "Qerror")
                    {
                        MTWFile.Single().WriteLine("收到照片错误返回");
                        MessageBoxEx.Show("查询失败，请检查网络");
                    }
                    else if (ss == "GetPic")
                    {
                        MTWFile.Single().WriteLine("收到照片显示");
                        picShow.Single().PicFromNetUpData(picIdFromNet);
                    }
                    else
                    {
                        MTWFile.Single().WriteLine("异常");
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBoxEx.Show("查询失败，程序异常:" + ex.Message);
            }
    }

        private void IdInput_Load(object sender, EventArgs e)
        {
            textBox1.KeyPress += TextBox1_KeyPress;
            SocketService.Single().IdInputRefresh += new IdInputRefreshhandle(RecUpdateInputFrom);
            this.StartPosition = FormStartPosition.CenterParent;

            System.Windows.Forms.Screen[] s1 = System.Windows.Forms.Screen.AllScreens;
            if (s1.Length == 1)
            {
                this.Location = new Point(0, 0);
            }
            else if (s1.Length == 2)
            {
                //System.Drawing.Rectangle r1 = s1[1].WorkingArea;
                //this.Top = r1.Top ;
                //this.Left = r1.Left;
                ////this.Width = r1.Width / 4;
                ////this.Height = r1.Height / 4;
                this.Location = new Point(1024, 0);
            }
        }

        /// <summary>
        /// 身份证输入框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                e.Handled = true;
                bool checkend = false;
                switch (e.KeyChar)
                {
                    case '0':
                        checkend = true;
                        break;
                    case '1':
                        checkend = true;
                        break;
                    case '2':
                        checkend = true;
                        break;
                    case '3':
                        checkend = true;
                        break;
                    case '4':
                        checkend = true;
                        break;
                    case '5':
                        checkend = true;
                        break;
                    case '6':
                        checkend = true;
                        break;
                    case '7':
                        checkend = true;
                        break;
                    case '8':
                        checkend = true;
                        break;
                    case '9':
                        checkend = true;
                        break;
                    case 'x':
                        checkend = true;
                        break;
                    case 'X':
                        checkend = true;
                        break;
                    default:
                        checkend = false;
                        break;
                }
                if (checkend == false)
                {
                    e.Handled = false;
                }
            }
            catch
            {
            }
        }

        public void  numBtnClick(object sender, EventArgs e)
        {
            this.textBox1.Text += ((Button)sender).Text;
        }
        
        private void btnClose_Click(object sender, EventArgs e)
        {
            string ss = string.Empty;
            SocketService.Single().IdInputRefresh -= new IdInputRefreshhandle(RecUpdateInputFrom);
            this.Dispose();
            
        }

        private void btnErase_Click(object sender, EventArgs e)
        {
            string ss = this.textBox1.Text.Trim();
            if (ss.Length > 0)
            {
                this.textBox1.Text = ss.Substring(0, ss.Length - 1);
            }
        }

        private void btnComp_Click(object sender, EventArgs e)
        {
            SocketService.Single().ServerSendMsg(SocketService.dbScreenSendType.SendCompare, "Com");
            MTWFile.Single().WriteLine(DateTime.Now.ToString() + "Send Compare");
            SocketService.Single().IdInputRefresh -= new IdInputRefreshhandle(RecUpdateInputFrom);
            this.Dispose();
        }

        /// <summary>
        /// 输入数据检查
        /// </summary>
        /// <param name="InPut"></param>
        /// <returns></returns>
        private bool InPutStringCheck(string InPut)
        {
            try
            {
                if (InPut.Length != 18)
                {
                    Thread th = new Thread(Thread_ShowNoteMsg);
                    th.Start("输入的身份证长度不等于18位");
                    return false;
                }
                for (int i = 0; i < InPut.Length; i++)
                {
                    bool checkend = false;
                    switch (InPut[i])
                    {
                        case '0':
                            checkend = true;
                            break;
                        case '1':
                            checkend = true;
                            break;
                        case '2':
                            checkend = true;
                            break;
                        case '3':
                            checkend = true;
                            break;
                        case '4':
                            checkend = true;
                            break;
                        case '5':
                            checkend = true;
                            break;
                        case '6':
                            checkend = true;
                            break;
                        case '7':
                            checkend = true;
                            break;
                        case '8':
                            checkend = true;
                            break;
                        case '9':
                            checkend = true;
                            break;
                        case 'x':
                            checkend = true;
                            break;
                        case 'X':
                            checkend = true;
                            break;
                        default:
                            checkend = false;
                            break;
                    }
                    if (checkend != true)
                    {
                        Thread th = new Thread(Thread_ShowNoteMsg);
                        th.Start(string.Format("包含非法字符{0}", InPut[i]));
                        return false;
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                MessageBoxEx.Show(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 显示提示消息
        /// </summary>
        /// <param name="temp"></param>
        private void Thread_ShowNoteMsg(object temp)
        {
            FormMsgShow handle = new FormMsgShow();
            try
            {
                string msg = Convert.ToString(temp);
                handle.SetMsg(msg);
                handle.Show();
                for (int i = 0; i < 20; i++)
                {
                    Thread.Sleep(100);
                }
                handle.Hide();
                handle.Dispose();
            }
            catch
            {
                if (handle != null)
                {
                    handle.Dispose();
                }
            }
        }

        /// <summary>
        /// 查询按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQueryInPutForm_Click(object sender, EventArgs e)
        {
            try
            {
                string mystr = textBox1.Text.Trim();
                if (InPutStringCheck(mystr) != true)
                {
                    return;
                }
                SocketService.Single().ServerSendMsg(SocketService.dbScreenSendType.SendQuerry, mystr);
                MTWFile.Single().WriteLine(DateTime.Now.ToString() + "Send querry");
            }
            catch(Exception ex)
            {
                MTWFile.Single().WriteLine(DateTime.Now.ToString() + "查询异常:" + ex.Message);
            }
        }
    }
}
