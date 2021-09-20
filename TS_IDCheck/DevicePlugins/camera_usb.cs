using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using System.Diagnostics;

namespace TS_IDCheck
{
    /// <summary>
    /// 摄像头
    /// </summary>
    public class camera_usb
    {
        /// <summary>
        /// 视频播放句柄
        /// </summary>
        private VideoCapture _captureVedio = null;
        private Emgu.CV.Mat _frame0;

        private static VedioPlayDelegateFrame onVedioPlay;
        public event VedioPlayDelegateFrame vedioPlayEvent
        {
            add { onVedioPlay += new VedioPlayDelegateFrame(value); }
            remove { onVedioPlay -= new VedioPlayDelegateFrame(value); }
        }

        /// <summary>
        /// 设置视频句柄
        /// </summary>
        public Mat _FrameVedio
        {
            set
            {
                _frame0 = value;
                if(onVedioPlay != null)
                    onVedioPlay(_frame0);
            }
            get
            {
                return _frame0;
            }
        }

        /// <summary>
        /// 实例化
        /// </summary>
        public camera_usb()
        {
            try
            {
            }
            catch
            {
            }
        }

        /// <summary>
        /// 启动摄像头
        /// </summary>
        /// <returns></returns>
        public bool StartRun()
        {
            try
            {
                Trace.WriteLine("开始启动摄像头...");

                //初始化摄像头
                if (InitCamera() != true)
                {
                    return false;
                }

                //打开摄像头
                OpenCamera();
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("启动摄像头异常:" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 初始化摄像头
        /// </summary>
        /// <returns></returns>
        private bool InitCamera()
        {
            try
            {
                Trace.WriteLine("初始化摄像头...");
                _captureVedio = new VideoCapture();
                if(_captureVedio.Width > 0)
                {
                    //_captureVedio.FlipHorizontal = true;
                    _captureVedio.ImageGrabbed += CaptureVedioImageGrabbed;
                    Trace.WriteLine("初始化摄像头成功");
                    _frame0 = new Mat();
                    return true;
                }
                else
                {
                    Trace.WriteLine("初始化摄像头失败");
                    _frame0 = null;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("初始化摄像头异常:" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 事件
        /// 获取视频
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CaptureVedioImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                if (_captureVedio != null && _captureVedio.Ptr != IntPtr.Zero)
                {

                    lock (_frame0)
                    {
                        _captureVedio.Retrieve(_frame0, 0);
                        _FrameVedio = _frame0;
                    }
                }
                else
                {
                    Trace.WriteLine("获取视频失败");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("获取视频异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 停止摄像头数据采集
        /// </summary>
        public void StopRun()
        {
            try
            {
                //关闭摄像头
                CloseCamera();
                if (_captureVedio != null)
                {
                    if (_captureVedio.Width > 0)
                    {
                        _captureVedio.ImageGrabbed -= CaptureVedioImageGrabbed;
                        Trace.WriteLine("摄像头数据采集事件注销完成");
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("停止摄像头数据采集:" + ex.Message);
            }
        }

        /// <summary>
        /// 打开摄像头
        /// </summary>
        private void OpenCamera()
        {
            try
            {
                if (_captureVedio != null)
                {
                    _captureVedio.Start();
                    Trace.WriteLine("打开摄像头完成");
                }
                else
                {
                    Trace.WriteLine("摄像头已经打开，不需要重复打开s");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("打开摄像头异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 关闭摄像头
        /// </summary>
        private void CloseCamera()
        {
            try
            {
                if (_captureVedio != null)
                {
                    _captureVedio.Stop();
                    Trace.WriteLine("关闭摄像头完成");
                }
                else
                {
                    Trace.WriteLine("摄像头没有打开，不用关闭");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("关闭摄像头异常:" + ex.Message);
            }
        }
    }
}
