using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Common;
using System.Drawing.Imaging;

namespace TS_IDCheck
{
    /// <summary>
    /// 流程状态
    /// </summary>
    public enum ESTATUSTIP
    {
        /// <summary>
        /// 重新捕捉人脸
        /// </summary>
        EST_Capture = 0,
        /// <summary>
        /// 获取身份证信息完成
        /// </summary>
        EST_CardComplete,
        /// <summary>
        /// 等待人脸比对
        /// </summary>
        EST_WaitDetect,
        /// <summary>
        /// 人脸比对完成
        /// </summary>
        EST_DetectComplete,
        /// <summary>
        /// 等待页面
        /// </summary>
        EST_Waiting,
        /// <summary>
        /// 获取证件（护照）信息失败提示
        /// </summary>
        EST_Prompt,
    }

    /// <summary>
    /// 给人脸画矩形框
    /// </summary>
    public class CaptureThread : BaseThread
    {
        /// <summary>
        /// 线程名称
        /// </summary>
        private string ThreadName = "【CaptureThread】";

        /// <summary>
        /// 获取人脸完成事件
        /// </summary>
        private static CaptureFaceCompleteDelegate onCaptureComplete;
        public event CaptureFaceCompleteDelegate captureCompleteEvent
        {
            add { onCaptureComplete += new CaptureFaceCompleteDelegate(value); }
            remove { onCaptureComplete -= new CaptureFaceCompleteDelegate(value); }
        }

        /// <summary>
        /// 给人脸画矩形框
        /// </summary>
        private static DrawLineDelegate onDrawLine;
        public event DrawLineDelegate drawLineEvent
        {
            add { onDrawLine += new DrawLineDelegate(value); }
            remove { onDrawLine -= new DrawLineDelegate(value); }
        }

        /// <summary>
        /// 流程状态改变
        /// </summary>
        private static StatueChangeDelegate onStatueChange;
        public event StatueChangeDelegate statueChangeEvent
        {
            add { onStatueChange += new StatueChangeDelegate(value); }
            remove { onStatueChange -= new StatueChangeDelegate(value); }
        }

        /// <summary>
        /// 人脸识别算法库
        /// </summary>
        GFaceRecognizer m_gface = GFaceRecognizer.GetInstance();

        /// <summary>
        /// 实例化
        /// </summary>
        public CaptureThread()
        {
            m_captures = new List<byte[]>();
        }

        /// <summary>
        /// 抓捕数据
        /// </summary>
        private List<byte[]> m_captures;

        /// <summary>
        /// 判断n是否大于0
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private int MakePositive(int n)
        {
            return n > 0 ? n : 0;
        }

        /// <summary>
        /// 摄像头句柄
        /// </summary>
        private camera_usb _cameraUsb = null;

        /// <summary>
        /// 获取摄像头句柄
        /// </summary>
        /// <param name="cam"></param>
        public void GetCamera(camera_usb cam)
        {
            _cameraUsb = cam;
        }

        /// <summary>
        /// 查询图片
        /// </summary>
        /// <returns></returns>
        public Bitmap QueryFrame()
        {
            try
            {
                //如果为初始流程
                if (currentStatus == ESTATUSTIP.EST_Capture)
                {
                    //如果没有获取到摄像头句柄
                    if (_cameraUsb != null)
                    {
                        int nRotateFlip = ConfigOperator.Single().RotateFlip;
                        Bitmap bm = _cameraUsb._FrameVedio.Bitmap;
                        RotateFlipType rft = RotateFlipType.Rotate270FlipNone;
                        if (nRotateFlip != 0)
                        {
                            switch (nRotateFlip)
                            {
                                case 1: rft = RotateFlipType.Rotate90FlipNone; break;
                                case 2: rft = RotateFlipType.RotateNoneFlipY; break;
                                default: break;
                            }
                            bm.RotateFlip(rft);
                        }

                        return bm;
                    }
                    else
                    {
                        Trace.WriteLine(ThreadName + "QueryFrame:_cameraUsb==null");
                    }
                }
                else
                {
                    Trace.WriteLine(ThreadName + "QueryFrame:currentStatus != ESTATUSTIP.EST_Capture");
                }
                return null;
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ThreadName + "QueryFrame异常:" + ex.Message);
                return null;
            }
        }


        /// <summary>
        /// 当前人脸矩形
        /// </summary>
        private RECT[] m_currRect;
        private byte[] m_rgb8;
        private byte[] m_rgb24;

        FacePointInfo[] m_currFacePoint;

        /// <summary>
        /// 当前流程状态
        /// </summary>
        private ESTATUSTIP currentStatus = ESTATUSTIP.EST_WaitDetect;

        /// <summary>
        /// 当前流程状态
        /// </summary>
        public ESTATUSTIP CurrentSataus
        {
            set { currentStatus = value; }
            get { return currentStatus; }
        }

        private int nNoFace = 0;

        public int NNoFace
        {
            set
            {
                if (nNoFace > 300)
                {
                    _nCaptureInterval = 1000;
                }
                else if (nNoFace > 10000)
                {
                    nNoFace = 301;
                    return;
                }
                else
                {
                    _nCaptureInterval = 200;
                }
                nNoFace = value;
            }
            get { return nNoFace; }
        }

        private int _nCaptureInterval = 200;

        /// <summary>
        /// 获取人脸图像线程
        /// </summary>
        public override void Run()
        {
            try
            {
                m_currFacePoint = new FacePointInfo[1024];
                m_currRect = new RECT[1024];
                DateTime lastCaptureFrameTime = new DateTime();
                Trace.WriteLine(ThreadName + "Run()启动...");
                while (IsAlive)
                {
                    //如果为等待人脸比对，则继续循环
                    if (currentStatus == ESTATUSTIP.EST_WaitDetect)
                    {
                        Thread.Sleep(100);
                        RECT rc = new RECT();
                        onDrawLine(rc, false);
                        //Thread.Sleep(2000);
                        continue;
                    }

                    //如果已经完成人脸比对，则恢复人脸可提取
                    if (CurrentSataus == ESTATUSTIP.EST_DetectComplete)
                    {
                        Trace.WriteLine(ThreadName + "已经完成人脸比对，则恢复人脸可提取");
                        CurrentSataus = ESTATUSTIP.EST_Capture;
                        Thread.Sleep(2000);
                        onStatueChange(ESTATUSTIP.EST_DetectComplete, ESTATUSTIP.EST_Capture);
                    }

                    #region 获取图像时间计数
                    TimeSpan ts = DateTime.Now.Subtract(lastCaptureFrameTime);
                    bool bBig = (ts.TotalMilliseconds > 0.0f);
                    if ((ts.TotalMilliseconds < _nCaptureInterval) && bBig)
                    {
                        //降频
                        Thread.Sleep(20);
                        continue;
                    }
                    Bitmap bmFrame = null;
                    try
                    {
                        Bitmap bmTmp = QueryFrame();
                        bmFrame = (Bitmap)(bmTmp != null ? bmTmp.Clone() : null);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                    if (bmFrame == null)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    lastCaptureFrameTime = DateTime.Now;
                    #endregion

                    //如果为等待人脸比对，则继续循环
                    if (currentStatus == ESTATUSTIP.EST_WaitDetect)
                    {
                        Thread.Sleep(100);
                        Trace.WriteLine(ThreadName + "currentStatus == ESTATUSTIP.EST_WaitDetect");
                        continue;
                    }

                    //如果需要剪裁照片
                    if (_bNeedCut)
                    {
                        //照片裁剪
                        bmFrame = ImageDealOper.CutImageByMem(bmFrame, _rtCut.X, _rtCut.Y, _rtCut.Width, _rtCut.Height);
                        
                    }

                    //提取图像
                    m_gface.GetRgb8And24Buf(bmFrame, out m_rgb8, out m_rgb24);
                    int w = bmFrame.Width;
                    int h = bmFrame.Height;

                    //确认是否为人脸
                    int nCount =  m_gface.Detect(0, m_rgb8, w, h, out m_currRect, out m_currFacePoint);
                    if (nCount < 1)
                    {
                        //未找到人脸
                        //Trace.WriteLine(ThreadName + "未找到人脸");
                        bmFrame.Dispose();
                        Thread.Sleep(100);
                        NNoFace++;
                        RECT rc = new RECT();
                        onDrawLine(rc, false);
                        continue;
                    }
                    else
                    {
                        //脸部太小 请靠近摄像头
                        if (m_currRect[0].right - m_currRect[0].left < 150)
                        {
                            RECT rc = new RECT();
                            if (onDrawLine != null)
                            {
                                onDrawLine(rc, false);
                            }
                        }
                        double disRL = m_currFacePoint[0].ptEyeRight.x - m_currFacePoint[0].ptEyeLeft.x;
                        double disHarfRL = (m_currFacePoint[0].ptEyeRight.x + m_currFacePoint[0].ptEyeLeft.x) / 2;
                        double disScale = Math.Abs(m_currFacePoint[0].ptNose.x - disHarfRL) / disRL;
                        if (disScale > 0.250)
                        {
                            RECT rc = new RECT();
                            if (onDrawLine != null)
                            {
                                onDrawLine(rc, false);
                            }
                            //Trace.WriteLine(ThreadName + "请正对摄像头");
                            continue;
                        }
                        if (onDrawLine != null)
                        {
                            onDrawLine(m_currRect[0], true);
                        }
                        NNoFace = 0;
                        onCaptureComplete(bmFrame, m_rgb24, w, h, m_currFacePoint, m_currRect, 0);
                        m_rgb24 = null;
                    }
                    Thread.Sleep(10);
                }
                Trace.WriteLine(ThreadName + "Run()停止");
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ThreadName + "获取人脸图像线程异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 是否需要裁剪
        /// </summary>
        private bool _bNeedCut = false;

        Rectangle _rtCut = new Rectangle();

        /// <summary>
        /// 设置剪裁照片尺寸
        /// </summary>
        /// <param name="bNeedCut"></param>
        /// <param name="rt"></param>
        public void SetCutImgInfo(bool bNeedCut, Rectangle rt)
        {
            try
            {
                _bNeedCut = bNeedCut;
                _rtCut = rt;
                _rtCut.Width = rt.Width / 4 * 4;
            }
            catch
            {
            }
        }

        #region 未使用函数

        //窗口宽和高
        private int m_nWidth = 0, m_nHeight = 0;
        public void SetWindowWH(int w, int h)
        {
            m_nWidth = w;
            m_nHeight = h;
        }

        private int FindMaxPhoto(int nCount)
        {
            int nMaxVPixels = 0;
            int nVpixels = 0;
            int nIndex = -1;
            for (int i = 0; i < nCount; ++i)
            {
                nVpixels = (MakePositive(m_currRect[0].right) - MakePositive(m_currRect[0].left)) * (MakePositive(m_currRect[0].bottom) - MakePositive(m_currRect[0].top));
                if (nVpixels > nMaxVPixels)
                {
                    nMaxVPixels = nVpixels;
                    nIndex = i;
                }
            }

            return nIndex;
        }

        private void ExpandFaceRect(int nIndex, int nExpandSize = 10)
        {
            if (nIndex < 0 || nIndex >= 1024)
                return;
            m_currRect[nIndex].top -= nExpandSize;
            m_currRect[nIndex].bottom += nExpandSize;
        }
        #endregion
    }
}