using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TS_IDCheck
{
    class ShowBlackWindows : BaseThread
    {
        private bool _isShow = false;
        
        public void SetShow(bool isShow)
        {
            _isShow = isShow;
        }
        
        public override void Run()
        {
            Thread.Sleep(2000);
            FormBlackTip _blackWindow = new FormBlackTip();
            Trace.WriteLine("_blackWindow Run begin");
            while (IsAlive)
            {
                if (!_isShow)
                {
                    Thread.Sleep(50);
                    continue;
                }
                Trace.WriteLine("终端显示黑名单提示");
                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Interval = 3000;
                timer.Elapsed += (a, b) =>
                {
                    _blackWindow.CloseBlackTipWindow();
                    timer.Stop();
                };
                timer.Start();
                _blackWindow.TopMost = true;
                _blackWindow.ShowDialog();
                _isShow = false;
                continue;
            }
        }
    }
}
