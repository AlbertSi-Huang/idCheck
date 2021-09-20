using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Common
{
    /*------------------------------调用代码----------------------------
      * AutoClosedMsgBox.Show("嘘...\r\n窗口5秒后关闭...", "提示", 5000);
      * AutoClosedMsgBox.Show("嘘...\r\n窗口5秒后关闭...", "提示", 5000, 0);
      * AutoClosedMsgBox.Show("嘘...\r\n窗口5秒后关闭...", "提示", 5000, MsgBoxStyle.OK);
      */
    public class AutoClosedMsgBox
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool EndDialog(IntPtr hDlg, int nResult);

        [DllImport("user32.dll")]
        static extern int MessageBoxTimeout(IntPtr hwnd, string txt, string caption,
            int wtype, int wlange, int dwtimeout);

        const int WM_CLOSE = 0x10;
        /// <summary>
        /// 弹出自动关闭的MessageBox窗口，只有“确定”按钮
        /// </summary>
        /// <param name="text">弹出窗口的显示内容</param>
        /// <param name="caption">弹出窗口的标题</param>
        /// <param name="milliseconds">窗口持续显示时间(毫秒)</param>
        /// <returns>固定返回DialogResult.OK</returns>
        public static DialogResult Show(string text, string caption, int milliseconds)
        {
            System.Windows.Forms.Timer timer = new Timer();
            timer.Interval = milliseconds;
            timer.Tick += (a, b) =>
            {
                IntPtr ptr = FindWindow(null, caption);
                if (ptr != IntPtr.Zero) EndDialog(ptr, 0);
                timer.Stop();
            };
            timer.Start();
            MessageBox.Show(text, caption);
            return DialogResult.OK;
        }

        /// <summary>
        /// 弹出自动关闭的MessageBox窗口，有多种显示方式
        /// </summary>
        /// <param name="txt">弹出窗口的显示内容</param>
        /// <param name="caption">弹出窗口的标题</param>
        /// <param name="style">窗口样式(枚举)</param>
        /// <param name="dwtimeout">窗口持续显示时间(毫秒)</param>
        /// <returns>0-无显示 1-确定 2-取消 3-终止 4-重试 5-忽略 6-是 7-否 10-重试 11-继续 32000-系统关闭</returns>
        public static int Show(string text, string caption, int milliseconds, MsgBoxStyle style)
        {
            return MessageBoxTimeout(IntPtr.Zero, text, caption, (int)style, 0, milliseconds);
        }

        public static int Show(string text, string caption, int milliseconds, int style)
        {
            return MessageBoxTimeout(IntPtr.Zero, text, caption, style, 0, milliseconds);
        }
    }
    public enum MsgBoxStyle
    {
        OK = 0, OKCancel = 1, AbortRetryIgnore = 2, YesNoCancel = 3, YesNo = 4,
        RetryCancel = 5, CancelRetryContinue = 6,

        //红叉 + ...
        RedCritical_OK = 16, RedCritical_OKCancel = 17, RedCritical_AbortRetryIgnore = 18,
        RedCritical_YesNoCancel = 19, RedCritical_YesNo = 20,
        RedCritical_RetryCancel = 21, RedCritical_CancelRetryContinue = 22,

        //蓝问号 + ...
        BlueQuestion_OK = 32, BlueQuestion_OKCancel = 33, BlueQuestion_AbortRetryIgnore = 34,
        BlueQuestion_YesNoCancel = 35, BlueQuestion_YesNo = 36,
        BlueQuestion_RetryCancel = 37, BlueQuestion_CancelRetryContinue = 38,

        //黄叹号 + ...
        YellowAlert_OK = 48, YellowAlert_OKCancel = 49, YellowAlert_AbortRetryIgnore = 50,
        YellowAlert_YesNoCancel = 51, YellowAlert_YesNo = 52,
        YellowAlert_RetryCancel = 53, YellowAlert_CancelRetryContinue = 54,

        //蓝叹号 + ...
        BlueInfo_OK = 64, BlueInfo_OKCancel = 65, BlueInfo_AbortRetryIgnore = 66,
        BlueInfo_YesNoCancel = 67, BlueInfo_YesNo = 68,
        BlueInfo_RetryCancel = 69, BlueInfo_CancelRetryContinue = 70,
    }
}
