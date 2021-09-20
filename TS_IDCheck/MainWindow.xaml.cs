using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TCPLibrary.DeailMsg;
using System.IO;
using System.Windows.Threading;
using System.Timers;
using System.Drawing;
using System.Diagnostics;
using TS_IDCheck.log;
using Common;
using System.Windows.Interop;
using Emgu.CV;
using LibExchangePlate;

namespace TS_IDCheck
{
    delegate void ShowCurrentTimeD(string strInfo);
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string m_sText = string.Empty;
        private bool m_bOpened = false;

        /// <summary>
        /// 线程名称
        /// </summary>
        private string ThreadName = "【MainWindow】";

        /// <summary>
        /// 给人脸画矩形框线程
        /// </summary>
        private CaptureThread _captureThread;

        /// <summary>
        /// 算法线程
        /// </summary>
        private DetectThread _detectThread;

        /// <summary>
        /// 读卡器类型
        /// </summary>
        private string _cardReaderBrand = "1";

        /// <summary>
        /// C卡读卡器句柄
        /// </summary>
        private CReadIdCard m_readIdcard;

        /// <summary>
        /// 护照读卡器句柄
        /// </summary>
        private ReadPassport m_ReadPassport;

        private ReadPassport630 m_readPassport630;

        /// <summary>
        /// H卡读卡器句柄
        /// </summary>
        private HSReadIdCard m_HSReadIdCard;

        /// <summary>
        /// 闸机句柄
        /// </summary>
        private ComOpenDoor _OpenDoor;

        /// <summary>
        /// 人脸算法线程
        /// </summary>
        private InitThread _initThread;


        private ESTATUSTIP m_currentStatue = ESTATUSTIP.EST_Capture;
        private delegate void outputDelegate(ESTATUSTIP currStatue, Bitmap bm = null, string strCard = "");
        private delegate void drawFaceDelegate(RECT rc, bool bNeed);
        private delegate void connectManagerDelegate(bool bConn);
        private System.Timers.Timer m_timeReadCard;  //读卡到刷人脸 定时器6s
        private ShowGifImage gifImg;
        private ShowGifImage gifLogoImg;
        string strFilePath = Directory.GetCurrentDirectory();
        private SimpleLog _simLog;

        private BitmapImage completeSuccess = new BitmapImage();
        private BitmapImage completeFailed = new BitmapImage();
        private BitmapImage completeRegister = new BitmapImage();
        private BitmapImage faceLeftTop = new BitmapImage();
        private BitmapImage faceLeftBottom = new BitmapImage();
        private BitmapImage faceTopRight = new BitmapImage();
        private BitmapImage faceRightBottom = new BitmapImage();
        private BitmapImage rosterCheck = new BitmapImage();
        private BitmapImage ticketErr = new BitmapImage();
        private BitmapImage ticketNetErr = new BitmapImage();
        private BitmapImage ticketOn = new BitmapImage();
        private BitmapImage dateInvalid = new BitmapImage();
        private BitmapImage ticketCheck = new BitmapImage();

        private System.Timers.Timer m_timeMisread;//读取护照时出错（护照未正确放置等）

        /// <summary>
        /// 摄像头句柄
        /// </summary>
        private camera_usb _vedioPlay;

        private int m_detectRes = 2;
        byte[] _cardImgBytes = null;
        /// <summary>
        /// 界面显示刷新
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="bm"></param>
        /// <param name="strCard"></param>
        private void OnShowTip(ESTATUSTIP msg, Bitmap bm = null, string strCard = "")
        {
            switch (msg)
            {
                //请刷身份证
                case ESTATUSTIP.EST_Capture:
                    {
                        #region  请刷身份证
                        ImgSite.Visibility = Visibility.Collapsed;
                        ImgCard.Visibility = Visibility.Collapsed;
                        ImgDRes.Visibility = Visibility.Collapsed;
                        LinePartDetectH.Visibility = Visibility.Collapsed;
                        string strGifName = "";
                        if (bScreenHorizontal)
                        {
                            strGifName = @"\skin\inputCardH.gif";
                            if (ConfigOperator.Single().CardReaderBrand.CompareTo("1") == 0)
                                strGifName = @"\skin\inputIcCardH.gif";
                            if (ConfigOperator.Single().CardReaderBrand.CompareTo("1") == 0)
                                lab_txtTip.Content = "请刷IC卡";
                            ImgDetectRes.Source = imgDetectNRes;
                        }
                        else
                        {
                            strGifName = @"\skin\inputCardv.gif";
                            if (ConfigOperator.Single().CardReaderBrand.CompareTo("1") == 0)
                                strGifName = @"\skin\inputIcCardv.gif";
                            if (!bScreenHorizontal)
                            {
                                lab_txtTip.Content = "请刷证件";
                                if (ConfigOperator.Single().CardReaderBrand.CompareTo("1") == 0)
                                    lab_txtTip.Content = "请刷IC卡";
                                lab_txtTip.Visibility = Visibility.Visible;
                            }
                        }
                        gifImg.StopAnimate();
                        gifImg = new ShowGifImage(strFilePath + strGifName);
                        gifImg.StartAnimate();
                        ImgTip.Content = gifImg;
                        if(bScreenHorizontal)
                            ImgTip.Margin = new Thickness(40, 0, 0, 30);// full line add by shenyiqi
                        //if(m_currentStatue == ESTATUSTIP.EST_Prompt)
                        //{
                        //    Trace.WriteLine("提示正确放置证件" + strFilePath + @"\skin\inputCardH.gif");
                        //    lab_txtTip.Content = "提示正确放置证件";
                        //    lab_txtTip.Visibility = Visibility.Visible;
                        //}
                        #endregion
                    }
                    break;


                //请正对屏幕
                case ESTATUSTIP.EST_CardComplete:
                    {
                        #region 请正对屏幕
                        ImgSite.Visibility = Visibility.Collapsed;
                        ImgCard.Visibility = Visibility.Collapsed;
                        ImgDRes.Visibility = Visibility.Collapsed;
                        LinePartDetectH.Visibility = Visibility.Collapsed;
                        string strGifName = @"\skin\onScreen.gif";
                        gifImg.StopAnimate();
                        gifImg = new ShowGifImage(strFilePath + strGifName);
                        ImgTip.Content = gifImg;
                        //add by shenyiqi
                        gifImg.StartAnimate();

                        if (!bScreenHorizontal)
                        {
                            //竖屏
                            lab_txtTip.Content = "请正对屏幕";
                            lab_txtTip.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            //横屏
                            ImgTip.Margin = new Thickness(30, 0, 30, 30);
                            ImgDetectRes.Source = imgDetectNRes;
                        }
                        #endregion
                    }
                    break;

                //请正确放置证件
                case ESTATUSTIP.EST_Prompt:
                    {
                        ImgSite.Visibility = Visibility.Collapsed;
                        ImgCard.Visibility = Visibility.Collapsed;
                        ImgDRes.Visibility = Visibility.Collapsed;

                        gifImg.StopAnimate();
                        ImgTip.Visibility = Visibility.Visible;  //显示“请刷身份证”图片
                        if (bScreenHorizontal)
                        {
                            //横屏
                            gifImg = new ShowGifImage(strFilePath + @"\skin\inputCardRight.gif");
                            Trace.WriteLine("提示正确放置证件 " + strFilePath + @"\skin\inputCardH.gif");
                        }
                        else
                        {
                            //竖屏
                            gifImg = new ShowGifImage(strFilePath + @"\skin\inputCardv.gif");
                        }
                        ImgTip.Content = gifImg;
                        gifImg.StartAnimate();
                        lab_txtTip.Content = "请正确放置证件";
                        lab_txtTip.Visibility = Visibility.Visible;
                    }
                    break;
                //比对完成，显示比对结果
                case ESTATUSTIP.EST_DetectComplete:
                    {
                        #region 比对完成，显示比对结果
                        gifImg.StopAnimate();
                        gifImg.Visibility = Visibility.Collapsed;
                        lab_txtTip.Visibility = Visibility.Collapsed;

                        if (!bScreenHorizontal)
                        {
                            #region 竖屏
                            Thickness tk = new Thickness();
                            //ImgCard 身份证图片位置 大小
                            ImgCard.Width = 81;
                            ImgCard.Height = 108;
                            tk.Top = 8;
                            tk.Left = 150;
                            if (mainWindow.Width == 600 && mainWindow.Height == 800)
                            {
                                ImgCard.Margin = new Thickness(120, 8, 0, 0);
                            }
                            else if (mainWindow.Width == 768 && mainWindow.Height == 1024)
                            {
                                ImgCard.Margin = new Thickness(160, 8, 0, 0);
                            }
                            else
                            {
                                ImgCard.Margin = tk;
                            }
                            ImgCard.Stretch = Stretch.Fill;
                            //wfc
                            if (File.Exists(strCard))
                            {
                                using (BinaryReader binReader = new BinaryReader(File.Open(strCard, FileMode.Open, FileAccess.Read, FileShare.Read)))
                                {
                                    FileInfo fileInfo = new FileInfo(strCard);
                                    _cardImgBytes = binReader.ReadBytes((int)fileInfo.Length);
                                    binReader.Close();

                                    BitmapImage bitmap = new BitmapImage();
                                    bitmap.BeginInit();
                                    bitmap.StreamSource = new MemoryStream(_cardImgBytes);
                                    bitmap.EndInit();

                                    ImgCard.Source = bitmap;
                                    ImgCard.Visibility = Visibility.Visible;
                                }
                            }

                            //设置显示刷脸的对比图片 位置大小
                            ImgSite.Width = 80;
                            ImgSite.Height = 110;
                            if (mainWindow.Width == 600 && mainWindow.Height == 800)
                            {
                                ImgSite.Margin = new Thickness(ImgCard.Width + 130, 8, 0, 0);
                            }
                            else if (mainWindow.Width == 768 && mainWindow.Height == 1024)
                            {
                                ImgSite.Margin = new Thickness(ImgCard.Width + 170, 8, 0, 0);
                            }
                            else
                            {
                                ImgSite.Margin = new Thickness(ImgCard.Width + 160, 8, 0, 0);
                            }
                            ImgSite.Stretch = Stretch.Fill;
                            ImgSite.Source = GetBitmapSource(bm);
                            ImgSite.Visibility = Visibility.Visible;

                            //设置显示比对结果图片的位置 大小
                            ImgDRes.Width = 180;
                            ImgDRes.Height = 50;
                            if (mainWindow.Width == 600 && mainWindow.Height == 800)
                            {
                                ImgDRes.Margin = new Thickness(ImgCard.Width + 230, 50, 0, 0);
                            }
                            else if (mainWindow.Width == 768 && mainWindow.Height == 1024)
                            {
                                ImgDRes.Margin = new Thickness(ImgCard.Width + 350, 50, 0, 0);
                            }
                            else
                            {
                                ImgDRes.Margin = new Thickness(ImgCard.Width + 300, 0, 0, 0);
                            }
                            #endregion
                        }
                        else
                        {
                            #region 横屏
                            ImgDetectRes.Source = imgDetectRes;
                            Thickness tk = new Thickness();
                            int nLeft = 65; //add by shenyiqi
                            tk.Left = nLeft;
                            tk.Top = 118;if (_IsPc) tk.Top = mainWindow.Height / 3 ;
                            ImgCard.Margin = tk;
                            ImgCard.Width = 100;
                            ImgCard.Height = 126;
                            ImgCard.Stretch = Stretch.Fill;

                            ImgSite.Width = 100;
                            ImgSite.Height = 126;
                            tk.Top = 118; if (_IsPc) tk.Top = mainWindow.Height / 3 ;
                            tk.Left = nLeft + ImgSite.Width + 10;// + 25 add by shenyiqi
                            ImgSite.Margin = tk;
                            ImgSite.Stretch = Stretch.Fill;
                            ImgSite.Source = GetBitmapSource(bm);
                            ImgSite.Visibility = Visibility.Visible;
                            if (File.Exists(strCard))
                            {
                                using (BinaryReader binReader = new BinaryReader(File.Open(strCard, FileMode.Open, FileAccess.Read, FileShare.Read)))
                                {
                                    FileInfo fileInfo = new FileInfo(strCard);
                                    _cardImgBytes = binReader.ReadBytes((int)fileInfo.Length);
                                    binReader.Close();

                                    BitmapImage bitmap = new BitmapImage();
                                    bitmap.BeginInit();
                                    bitmap.StreamSource = new MemoryStream(_cardImgBytes);
                                    bitmap.EndInit();

                                    ImgCard.Source = bitmap;
                                    ImgCard.Visibility = Visibility.Visible;
                                }
                            }

                            ImgDRes.Width = 160;
                            ImgDRes.Height = 80;
                            //设置显示比对结果图片的位置
                            tk.Left = nLeft + 5;// 文字提示 add by shenyiqi
                            tk.Top = 118 + ImgCard.Height + 28;
                            ImgDRes.Margin = tk;

                            tk.Left = nLeft;
                            tk.Top = ImgCard.Height + 5;
                            //LinePartDetectH.Visibility = Visibility.Visible;
                            LinePartDetectH.Margin = tk;
                            #endregion
                        }
                        if (m_detectRes == 1)
                        {
                            //
                            ImgDRes.Source = completeSuccess;
                        }
                        else if (m_detectRes == 2)
                        {
                            //LinePartDetectH.Visibility = Visibility.Visible;
                            ImgDRes.Source = completeFailed;
                        }
                        else if (m_detectRes == 3)
                        {
                            //LinePartDetectH.Visibility = Visibility.Visible;
                            ImgDRes.Source = completeRegister;
                        }
                        else if (m_detectRes == 4)
                        {
                            //LinePartDetectH.Visibility = Visibility.Visible;
                            ImgDRes.Source = rosterCheck;
                        }
                        else if (m_detectRes == 6)
                        {
                            ImgDRes.Source = ticketErr;
                        }else if(m_detectRes == 7)
                        {
                            ImgDRes.Source = ticketNetErr;
                        }
                        else if (m_detectRes == 8)
                        {
                            ImgDRes.Source = ticketOn;
                        }
                        else if (m_detectRes == 9)
                        {
                            ImgDRes.Source = dateInvalid;
                        }
                        else if (m_detectRes == 10)
                        {
                            //已检票
                            ImgDRes.Source = ticketCheck;
                        }
                        ImgDRes.Visibility = Visibility.Visible;
                        #endregion
                    }
                    break;
            }
        }



        /// <summary>
        /// 连接客户端管理
        /// </summary>
        /// <param name="bConn">是否连接成功</param>
        private void OnConnectManager(bool bConn)
        {
            if (bConn)  //连接成功
            {
                ImgManager.Source = imgManagerOnLine;
            }
            else  //连接失败
            {
                ImgManager.Source = imgManagerOutLine;
            }
        }

        private int nLoop = 0;

        /// <summary>
        /// 抓到人脸时画框
        /// </summary>
        /// <param name="rc">人脸矩形</param>
        /// <param name="bNeed">是否有人脸</param>
        private void DrawFace(RECT rc, bool bNeed)
        {
            if (!bNeed)
            {
                if (nLoop > 1) return;
                nLoop++;
                if (rc.left == 0 && rc.right == 0)
                {
                    ImgFaceLeftTop.Visibility = Visibility.Collapsed;
                    ImgFaceTopRight.Visibility = Visibility.Collapsed;
                    ImgFaceLeftBottom.Visibility = Visibility.Collapsed;
                    ImgFaceRightBottom.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                nLoop = 0;

                Thickness tk1 = new Thickness();
                tk1.Left = rc.left * imgBox_vedio.Width / _oneW  + nLeftBox;
                tk1.Top = rc.top * imgBox_vedio.Height / _oneH - 40;
                int nLen = 42;
                if (!_bNeedCut)
                {
                    //Trace.WriteLine("nLen = 30;");
                    nLen = 30;
                }
                ImgFaceLeftTop.Width = nLen;
                ImgFaceLeftTop.Height = nLen;
                //if (bAutoScene && !_bNeedCut)
                //{
                //    tk1.Left += imgBox_vedio.Margin.Left;
                //    tk1.Top += imgBox_vedio.Margin.Top;
                //}

                //tk1.Top = (rc.top - _rtVedio.Top) * imgBox_vedio.Height / _oneH - 40;

                ImgFaceLeftTop.Margin = tk1;
                ImgFaceLeftTop.Source = faceLeftTop;

                Thickness tk2 = new Thickness();
                tk2.Left = rc.right * imgBox_vedio.Width / _oneW - 30 + nLeftBox;
                tk2.Top = rc.top * imgBox_vedio.Height / _oneH - 40;
                ImgFaceTopRight.Width = nLen;
                ImgFaceTopRight.Height = nLen;
                //if (bAutoScene && !_bNeedCut)
                //{
                //    tk2.Left += imgBox_vedio.Margin.Left;
                //    tk2.Top += imgBox_vedio.Margin.Top;
                //}

                //if (bAutoScene && _bNeedCut)
                //{
                //    tk2.Top = (rc.top - _rtVedio.Top) * imgBox_vedio.Height / _oneH - 40;
                //}

                ImgFaceTopRight.Margin = tk2;
                ImgFaceTopRight.Source = faceTopRight;

                Thickness tk3 = new Thickness();
                tk3.Left = rc.left * imgBox_vedio.Width / _oneW  + nLeftBox;
                tk3.Top = rc.bottom * imgBox_vedio.Height / _oneH - 60;
                ImgFaceLeftBottom.Width = nLen;
                ImgFaceLeftBottom.Height = nLen;
                //if (bAutoScene && !_bNeedCut)
                //{
                //    tk3.Left += imgBox_vedio.Margin.Left;
                //    tk3.Top += imgBox_vedio.Margin.Top;
                //}

                //if (bAutoScene && _bNeedCut)
                //{
                //    tk3.Top = (rc.bottom - _rtVedio.Top) * imgBox_vedio.Height / _oneH - 60;
                //}
                ImgFaceLeftBottom.Margin = tk3;
                ImgFaceLeftBottom.Source = faceLeftBottom;

                Thickness tk4 = new Thickness();
                tk4.Left = rc.right * imgBox_vedio.Width / _oneW - 30 + nLeftBox;
                tk4.Top = rc.bottom * imgBox_vedio.Height / _oneH - 60;
                ImgFaceRightBottom.Width = nLen;
                ImgFaceRightBottom.Height = nLen;
                //if (bAutoScene && !_bNeedCut)
                //{
                //    tk4.Left += imgBox_vedio.Margin.Left;
                //    tk4.Top += imgBox_vedio.Margin.Top;
                //}

                //if (bAutoScene && _bNeedCut)
                //{
                //    tk4.Top = (rc.bottom - _rtVedio.Top) * imgBox_vedio.Height / _oneH - 60;
                //}
                ImgFaceRightBottom.Margin = tk4;
                ImgFaceRightBottom.Source = faceRightBottom;

                if (tk1.Left + nLen > tk2.Left || tk1.Top + nLen > tk3.Top)
                {
                    Trace.WriteLine("人脸太小--------，不画框");
                    ImgFaceLeftTop.Visibility = Visibility.Collapsed;
                    ImgFaceTopRight.Visibility = Visibility.Collapsed;
                    ImgFaceLeftBottom.Visibility = Visibility.Collapsed;
                    ImgFaceRightBottom.Visibility = Visibility.Collapsed;
                    return;
                }

                ImgFaceLeftTop.Visibility = Visibility.Visible;
                ImgFaceTopRight.Visibility = Visibility.Visible;
                ImgFaceLeftBottom.Visibility = Visibility.Visible;
                ImgFaceRightBottom.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 事件
        /// 抓到人脸时画框
        /// </summary>
        /// <param name="rc"></param>
        /// <param name="bNeed"></param>
        private void OnDrawFace(RECT rc, bool bNeed)
        {
            try
            {
                this.m_Grid.Dispatcher.Invoke(new drawFaceDelegate(DrawFace), rc, bNeed);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 事件
        /// </summary>
        /// <param name="currStatue"></param>
        /// <param name="bm"></param>
        /// <param name="strCard"></param>
        private void OnShowTipChangeNew(ESTATUSTIP currStatue, Bitmap bm = null, string strCard = "")
        {
            try
            {
                this.ImgTip.Dispatcher.Invoke(new outputDelegate(OnShowTip), currStatue, bm, strCard);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ThreadName + "界面刷新事件异常:" + ex.Message);
            }
        }

        BitmapImage imgDetectRes = new BitmapImage();
        BitmapImage imgDetectNRes = new BitmapImage();
        BitmapImage imgBackGroundTop = new BitmapImage();
        BitmapImage imgBackGroundBottom = new BitmapImage();
        BitmapImage imgBackGroundRight = new BitmapImage();
        BitmapImage imgManagerOnLine = new BitmapImage();
        BitmapImage imgManagerOutLine = new BitmapImage();
        BitmapImage imgCameraOnLine = new BitmapImage();
        BitmapImage imgCameraOutLine = new BitmapImage();
        BitmapImage imgIDCardOnLine = new BitmapImage();
        BitmapImage imgIDCardOutLine = new BitmapImage();
        private BitmapImage imgLogo = new BitmapImage();
        bool bScreenHorizontal = true;
        bool _IsPc = false;

        int nLeftBox = 260;

        private void ImageLocation()
        {
            #region 定位UI
            m_runImg.Width = mainWindow.Width;
            m_runImg.Height = mainWindow.Height;
            Thickness tk = new Thickness();
            if (!bScreenHorizontal)  
            {
                #region 竖屏
                ImgBackGroundTop.Width = mainWindow.Width;
                ImgBackGroundTop.Height = 150;

                ImgBackGroundBottom.Width = mainWindow.Width;
                ImgBackGroundBottom.Height = 140;

                imgBox_vedio.Width = mainWindow.Width;
                imgBox_vedio.Height = mainWindow.Height;
                tk.Top = 0;
                imgBox_vedio.Margin = tk;

                _mainwindowWidth = imgBox_vedio.Width;
                _mainwindowHeight = imgBox_vedio.Height;

                //设置logo位置 大小
                ImgLogo.Width = 500;
                ImgLogo.Height = 140;
                ImgLogo.HorizontalAlignment = HorizontalAlignment.Center;//logo居中
                ImgLogo.Margin = new Thickness(0, 5, 0, 0);

                tk.Top = 10;
                tk.Left = ImgBackGroundTop.Width;

                //设置“请刷身份证”位置 大小
                this.ImgTip.Height = 140;
                this.ImgTip.Width = 280;
                if (mainWindow.Width == 600 && mainWindow.Height == 800)
                {
                    this.ImgTip.Margin = new Thickness(90, 0, 0, 0);
                }
                else if (mainWindow.Width == 768 && mainWindow.Height == 1024)
                {
                    this.ImgTip.Margin = new Thickness(-150,5, 0, 0);
                }
                else
                {
                    this.ImgTip.Margin = new Thickness(150, 0, 0, 0);
                }

                //提示语
                lab_txtTip.Width = 280;
                lab_txtTip.Height = 80;
                tk.Left = 320; tk.Top = 20;

                lab_txtTip.Margin = tk;
                lab_txtTip.Content = "请刷证件";

                lab_txtTip.Visibility = Visibility.Visible;

                gridDetectRes.Width = mainWindow.Width;
                gridDetectRes.Height = ImgBackGroundBottom.Height;

                ImgDetectRes.Width = gridDetectRes.Width;
                ImgDetectRes.Height = gridDetectRes.Height;
                ImgDetectRes.Stretch = Stretch.Fill;

                ImgSite.Visibility = Visibility.Collapsed;
                ImgCard.Visibility = Visibility.Collapsed;
                ImgDRes.Visibility = Visibility.Collapsed;
                LinePartDetect.X1 = (double)mainWindow.Width / 2;
                LinePartDetect.X2 = (double)mainWindow.Width / 2;
                LinePartDetect.Visibility = Visibility.Collapsed;

                tk.Left = 20;
                tk.Bottom = 0;
                tk.Right = 0;
                tk.Top = 12;
                GridDevice.Margin = tk;
                GridDevice.Width = 125;
                GridDevice.Height = 21;
                nLeftBox = 0;
                #endregion
            }
            else
            {
                if (_IsPc)
                {
                    ImgBackGroundRight.Height = mainWindow.Height;
                    ImgBackGroundRight.Width = 1024 / 3;
                    imgBox_vedio.Width = mainWindow.Width - ImgBackGroundRight.Width - nLeftBox;
                    imgBox_vedio.Height = mainWindow.Height;
                }
                else
                {
                    ImgBackGroundRight.Height = mainWindow.Height;
                    ImgBackGroundRight.Width = mainWindow.Width / 3;
                    nLeftBox = (int)(mainWindow.Width - ImgBackGroundRight.Width - 450);
                    //横屏显示
                    imgBox_vedio.Width = 450;//mainWindow.Width;
                    imgBox_vedio.Height = 600;//mainWindow.Height;
                }
                
                tk.Top = 0;
                tk.Left = nLeftBox;
                imgBox_vedio.Margin = tk;


                tk.Left = mainWindow.Width - ImgBackGroundRight.Width;
                ImgBackGroundRight.Margin = tk;

                _mainwindowWidth = imgBox_vedio.Width;
                _mainwindowHeight = imgBox_vedio.Height;
                ImgLogo.Width = 140;
                ImgLogo.Height = 100;
                //设置logo位置

                tk.Top = 30;//add by shenyiqi
                tk.Left = mainWindow.Width - ImgBackGroundRight.Width + ImgBackGroundRight.Width / 3 - 15;// add by shenyiqi
                tk.Right = 0;
                tk.Bottom = tk.Top;//add by shenyiqi
                ImgLogo.Margin = tk;
                gridDetectRes.Width = ImgBackGroundRight.Width;//add by shenyiqi
                gridDetectRes.Height = imgBox_vedio.Height - ImgLogo.Height - 30 - 30 - 20;//add by shenyiqi
                gridDetectRes.HorizontalAlignment = HorizontalAlignment.Right;
                gridDetectRes.VerticalAlignment = VerticalAlignment.Top;

                tk.Top = ImgLogo.Height + 30 + 10;//add by shenyiqi
                tk.Left = 10;
                tk.Right = 0;
                tk.Bottom = 0;
                gridDetectRes.Margin = tk;

                ImgTip.Height = gridDetectRes.Height / 3 * 2;
                ImgTip.Width = gridDetectRes.Width / 3 * 2;
                tk.Left = 30;
                tk.Top = 0;
                tk.Bottom = 30;
                tk.Right = 10;
                ImgTip.Margin = tk;

                tk.Left = 320; tk.Top = ImgTip.Height + 98;
                lab_txtTip.Margin = tk;

                ImgSite.Visibility = Visibility.Collapsed;
                ImgCard.Visibility = Visibility.Collapsed;
                ImgDRes.Visibility = Visibility.Collapsed;
                LinePartDetectH.Visibility = Visibility.Collapsed;

                tk.Left = 20; tk.Top = 0; tk.Right = 0; tk.Bottom = 12;
                GridDevice.Margin = tk;
                GridDevice.Width = 125;
                GridDevice.Height = 21;
                GridDevice.VerticalAlignment = VerticalAlignment.Bottom;
            }
            gridDetectRes.Visibility = Visibility.Hidden;
            #endregion
        }


        private void ImageLoading()
        {
            #region 图片加载
            if (bScreenHorizontal)
            {
                //横屏
                MainWindowHelper.LoadImageInit(imgBackGroundRight, strFilePath + @"\skin\backgroundRight.png");
                
                if (ConfigOperator.Single().CardReaderBrand.CompareTo("1") == 0)
                    gifImg = new ShowGifImage(strFilePath + @"\skin\inputIcCardH.gif");
                else
                    gifImg = new ShowGifImage(strFilePath + @"\skin\inputCardH.gif");
                gifImg.Width = this.ImgTip.Width;
                gifImg.Height = this.ImgTip.Height;
                this.ImgTip.Content = gifImg;

                this.gifImg.StartAnimate();

                lab_txtTip.Visibility = Visibility.Collapsed;

                MainWindowHelper.LoadImageInit(imgDetectRes, strFilePath + @"\skin\detectRes.png");
                MainWindowHelper.LoadImageInit(imgDetectNRes, strFilePath + @"\skin\detectNRes.png");
                
                ImgDetectRes.Source = imgDetectNRes;
            }
            else
            {
                //竖屏
                MainWindowHelper.LoadImageInit(imgBackGroundTop, strFilePath + @"\skin\backgroundtop.png");
                MainWindowHelper.LoadImageInit(imgBackGroundBottom, strFilePath + @"\skin\backgroundbottom.png");
                

                //刷身份证 gif
                if (ConfigOperator.Single().CardReaderBrand.CompareTo("1") == 0)
                    gifImg = new ShowGifImage(strFilePath + @"\skin\inputIcCardv.gif");
                else
                    gifImg = new ShowGifImage(strFilePath + @"\skin\inputCardv.gif");
                gifImg.Width = this.ImgTip.Width;
                gifImg.Height = this.ImgTip.Height;
                this.ImgTip.Content = gifImg;
                this.gifImg.StartAnimate();
            }
            MainWindowHelper.LoadImageInit(completeSuccess, strFilePath + @"\skin\success.png");
            MainWindowHelper.LoadImageInit(completeFailed, strFilePath + @"\skin\failed.png");
            MainWindowHelper.LoadImageInit(completeRegister, strFilePath + @"\skin\Register.png");
            MainWindowHelper.LoadImageInit(faceLeftTop, strFilePath + @"\skin\faceLeftTop.png");
            MainWindowHelper.LoadImageInit(faceLeftBottom, strFilePath + @"\skin\faceLeftBottom.png");
            MainWindowHelper.LoadImageInit(faceTopRight, strFilePath + @"\skin\faceTopRight.png");
            MainWindowHelper.LoadImageInit(faceRightBottom, strFilePath + @"\skin\faceRightBottm.png");
            MainWindowHelper.LoadImageInit(rosterCheck, strFilePath + @"\skin\check.png");
            MainWindowHelper.LoadImageInit(ticketErr, strFilePath + @"\skin\ticketErr.png");
            MainWindowHelper.LoadImageInit(ticketNetErr, strFilePath + @"\skin\ticketneterr.png");
            MainWindowHelper.LoadImageInit(ticketOn, strFilePath + @"\skin\ticketOn.png");
            MainWindowHelper.LoadImageInit(dateInvalid, strFilePath + @"\skin\cardInvalid.png");
            MainWindowHelper.LoadImageInit(ticketCheck, strFilePath + @"\skin\ticketCheck.png");
            MainWindowHelper.LoadImageInit(imgManagerOnLine, strFilePath + @"\skin\managerOnline.png");
            MainWindowHelper.LoadImageInit(imgManagerOutLine, strFilePath + @"\skin\managerOutLine.png");
            MainWindowHelper.LoadImageInit(imgCameraOnLine, strFilePath + @"\skin\cameraOnLine.png");
            MainWindowHelper.LoadImageInit(imgCameraOutLine, strFilePath + @"\skin\cameraOutLine.png");
            MainWindowHelper.LoadImageInit(imgIDCardOnLine, strFilePath + @"\skin\IdCardOnLine.png");
            MainWindowHelper.LoadImageInit(imgIDCardOutLine, strFilePath + @"\skin\IdCardOutLine.png");
            string strLogo = strFilePath + @"\skin\logoV.png";
            if (bScreenHorizontal) { strLogo = strFilePath + @"\skin\logoH.png"; }
            MainWindowHelper.LoadImageInit(imgLogo, strLogo);
            #endregion
        }
        private PlateExchangeAbstract[] _plateExchange = null;
        int _nPlateNum = 0;
        /// <summary>
        /// 界面加载
        /// </summary>
        public MainWindow()
        {
            try
            {
                CConsoleManager.Hide();
                m_currentStatue = 0;
                InitializeComponent();
                this.Closing += MainWindow_Closing;
                Trace.WriteLine(ThreadName + "注册MainWindow_Closing事件");
                double dWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                double dHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

                //侧屏显示分辨率
#if DEBUG
                //mainWindow.Width = 1200;
                //mainWindow.Height = 600;
                mainWindow.Width = dWidth;//add by shenyiqi;
                mainWindow.Height = dHeight;//add by shenyiqi;
                

#else
                mainWindow.Width = dWidth;
                mainWindow.Height = dHeight;
#endif
                //日志
                _simLog = new SimpleLog("mainWindow");
                _simLog.LogWriteBegin();
                Trace.Listeners.Add(_simLog);

                if (mainWindow.Height > mainWindow.Width)
                {
                    //竖屏
                    bScreenHorizontal = false;
                }else
                {
                    //横屏
                    if(mainWindow.Width > 1024 || mainWindow.Height > 1024)
                    {
                        _IsPc = true;
                    }else
                    {
                        mainWindow.Height = 600;
                    }
                }
                ConfigOperator.Single().InitConfig();
                ImageLocation();

                string strWaitPhoto = strFilePath + @"\skin\waiting.png";
                if (bScreenHorizontal)
                    strWaitPhoto = strFilePath + @"\skin\waitingH.png";
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(strWaitPhoto, UriKind.RelativeOrAbsolute);
                bi.EndInit();
                bi.Freeze();
                this.Background = new ImageBrush
                {
                    ImageSource = bi
                };
                labOSName.Visibility = Visibility.Hidden;
                ImageLoading();
                //读卡后未抓到人脸超时时间
                m_timeReadCard = new System.Timers.Timer(6000);
                m_timeReadCard.Elapsed += TimeReadCard_Elapsed;
                m_timeReadCard.AutoReset = true;
                m_timeReadCard.Enabled = false;

                //读取护照出错
                m_timeMisread = new System.Timers.Timer(5000);
                m_timeMisread.Elapsed += TimeMisread_Elapsed;
                m_timeMisread.AutoReset = false;
                m_timeMisread.Enabled = false;

                _initThread = new InitThread();
               Trace.WriteLine(ThreadName + "实例化InitThread线程完成");

                m_readIdcard = new CReadIdCard();
                m_HSReadIdCard = new HSReadIdCard();
                m_ReadPassport = new ReadPassport("1|2|11|12", true, "1|8");//护照
                m_readPassport630 = new ReadPassport630();

                _captureThread = new CaptureThread();
                Trace.WriteLine(ThreadName + "实例化CaptureThread线程完成");

                _detectThread = new DetectThread(ConfigOperator.Single().CardReaderBrand);//
                Trace.WriteLine(ThreadName + "实例化DetectThread线程完成");

                IntPtr hwnd = new WindowInteropHelper(this).Handle;

                Trace.WriteLine(ThreadName + "开始硬件初始化...");
                if (InitDevice() == false)
                {
                    Trace.WriteLine(ThreadName + "硬件初始化失败");
                    Close();
                    System.Environment.Exit(0);
                    return;
                }

                if(ConfigOperator.Single().ExchangePlateChoose != null && ConfigOperator.Single().ExchangePlateChoose.Length > 0)
                {
                    string[] strPlate = ConfigOperator.Single().ExchangePlateChoose.Split('|');
                    if(strPlate[0].Length > 0)
                    {
                        _nPlateNum = strPlate.Length;
                        _plateExchange = new PlateExchangeAbstract[_nPlateNum];
                        PlateExchangeFactory factory = new PlateExchangeFactory();
                        string strLogin = string.Empty;
                        for(int i = 0; i < _nPlateNum; ++i)
                        {
                            _plateExchange[i] = factory.CreateInstance(strPlate[i]);
                            if(_plateExchange[i] != null)
                            {
                                _plateExchange[i].broadcastExchangeInfo += MainWindowBroadcastExchangeInfo;
                                _plateExchange[i].Init("", "", "");
                                _plateExchange[i].LoginIn(strLogin);
                            }
                        }

                        SetAllPlate();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("系统启动错误:" + ex.Message);
            }
        }

        public void SetAllPlate()
        {
            if (ConfigOperator.Single().ExchangePlateChoose.Split('|').Length > 0)
            {
                if (_detectThread != null)
                    _detectThread.SetPlate(_plateExchange);
            }
        }

        private void SavePlateCardInfo(SCardInfo info)
        {
            if (_nPlateNum == 0) return;
            SCardInfo libCard = new SCardInfo();
            libCard._idNum = info._idNum;
            libCard._name = info._name;

            libCard._nation = info._nation;
            libCard._birthday = info._birthday;
            libCard._address = info._address;
            libCard._photo = info._photo;
            if (!File.Exists(info._photo))
            {
                libCard._photo = ConfigOperator.Single().StrSavePath + @"\RosterImg\" + info._idNum + @"_0.jpg";
            }
            if (!File.Exists(libCard._photo))
            {
                Trace.WriteLine("平台证件照为空：" + libCard._photo);
            }
            libCard._dateEnd = info._dateEnd;
            libCard._dateStart = info._dateStart;
            libCard._sex = info._sex;
            libCard._issure = info._issure;
            for (int i = 0; i < _nPlateNum; ++i)
            {
                _plateExchange[i].SaveCardInfo(libCard);
            }
            return;
        }

        private void MainWindowBroadcastExchangeInfo(PlateExchangeBakInfo info)
        {
            throw new NotImplementedException();
        }

        private void TimeMisread_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                //如果定时器到时，仍未正确放置证件，则重置。
                if (m_currentStatue == ESTATUSTIP.EST_Prompt)
                {
                    m_currentStatue = ESTATUSTIP.EST_Capture;
                    OnShowTipChangeNew(ESTATUSTIP.EST_Capture);
                }
                m_timeMisread.Enabled = false;

            }
            catch (Exception ex)
            {
                Trace.WriteLine("TimeMisread_Elapsed:" + ex.Message);
            }
        }

        /// <summary>
        /// 窗体关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                ComOpenDoor.getInstance().CloseCom();

                Trace.WriteLine(ThreadName + "开始执行窗体关闭...");
                //关闭读卡器
                if (_cardReaderBrand == "0")//身份证
                {
                    m_readIdcard.StopRun();
                }
                else if (_cardReaderBrand == "2")//护照
                {
                    m_ReadPassport.Close();
                }
                else
                {
                    m_HSReadIdCard.StopRun();
                }
                
                Trace.WriteLine(ThreadName + "窗体关闭执行完成");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ThreadName + "MainWindow_Closing异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 读卡后定时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeReadCard_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                //如果定时器到时，当前已经读取身份证，则重置整个流程
                if (m_currentStatue == ESTATUSTIP.EST_CardComplete)
                {
                    m_currentStatue = ESTATUSTIP.EST_Capture;
                }
                OnShowTipChangeNew(m_currentStatue);
                if (_cardReaderBrand == "0")
                {
                    m_readIdcard.IsNeedCapture = true;
                }
                else if (_cardReaderBrand == "1")
                {
                    m_HSReadIdCard.IsNeedCapture = true;
                }
                else if (_cardReaderBrand == "2")//护照
                {
                    m_ReadPassport.IsNeedCapture = true;
                }else if(_cardReaderBrand == "3")
                {
                    m_readPassport630.IsNeedCapture = true;
                }
                OnShowReadCardPhoto("", false);
                m_timeReadCard.Enabled = false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ThreadName + "TimeReadCard_Elapsed异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 人脸抓拍定时器
        /// </summary>
        DispatcherTimer timerVedio = new DispatcherTimer();

        /// <summary>
        /// 初始化硬件设备 包括配置、读卡器、存储文件、算法库、
        /// </summary>
        /// <returns></returns>
        private bool InitDevice()
        {
            try
            {
                //摄像头初始化
                _vedioPlay = new camera_usb();
                bool bStart = _vedioPlay.StartRun();
                if (!bStart)
                {
                    Common.AutoClosedMsgBox.Show("摄像头初始化失败，即将退出程序", "启动提示", 3000);
                }
                //启动人脸算法
                Trace.WriteLine(ThreadName + "_initThread.Start...");
                _initThread.Start();
                Trace.WriteLine(ThreadName + "_initThread.Start finish");
                _cardReaderBrand = ConfigOperator.Single().CardReaderBrand;
                Trace.WriteLine("_cardReaderBrand = " + _cardReaderBrand);
                #region licsense.dat文件加载，刘飞翔，20180504
                if (!InitLicsence())
                {
                    MessageBox.Show("license.dat初始化失败");
                    return false;
                }
                #endregion

                #region 初始化读卡器
                Trace.WriteLine(ThreadName + "初始化读卡器...");
                if (_cardReaderBrand == "0")
                {
                    //C卡
                    if (!m_readIdcard.OpenIDCard())
                    {
                        MessageBox.Show("读卡器初始化失败");
                        return false;
                    }
                    if (!m_readIdcard.InitFileData())
                    {
                        MessageBox.Show("文件数据初始化失败");
                        return false;
                    }
                    m_readIdcard.StartRun();
                }
                else if (_cardReaderBrand == "1")
                {
                    //H卡
                    if (!m_HSReadIdCard.OpenIDCard())
                    {
                        MessageBox.Show("读卡器初始化失败");
                        return false;
                    }

                    if (!m_HSReadIdCard.InitFileData())
                    {
                        MessageBox.Show("文件数据初始化失败");
                        return false;
                    }
                    m_HSReadIdCard.StartRun();
                }
                else if (_cardReaderBrand == "2")//护照
                {
                    if (!m_ReadPassport.Init())
                    {
                        MessageBox.Show("护照初始化失败");
                        return false;
                    }
                    if (!m_ReadPassport.InitFileData())
                    {
                        MessageBox.Show("文件数据初始化失败");
                        return false;
                    }
                    m_ReadPassport.StartRun();//开启读卡
                }else if(_cardReaderBrand.CompareTo("3") == 0)
                {
                    if (!m_readPassport630.Init())
                    {
                        MessageBox.Show("630护照初始化失败");
                        return false;
                    }
                    if (!m_readPassport630.InitFileData())
                    {
                        MessageBox.Show("文件数据初始化失败");
                        return false;
                    }
                    m_readPassport630.ReadCard();
                }

                
                //}
                Trace.WriteLine(ThreadName + "读卡器初始化完成");
                #endregion

                //初始化闸机
                _OpenDoor = ComOpenDoor.getInstance();
                _OpenDoor.InitOpenCom();

                //设置图像源
                ImgManager.Source = imgManagerOutLine;
                ImgIDCard.Source = imgIDCardOnLine;
                ImgCamera.Source = imgCameraOnLine;

                return bStart;
            }
            catch (Exception ex)
            {
                Trace.Write(ThreadName + "设备初始化异常:" + ex.Message);
                return false;
            }
        }

        private void MainWindowRestartgChangeEvent(int nInfo)
        {
            if (nInfo == 1)
            {

                //重启设备
                // By Default the Restart will take place after 30 Seconds  
                //if you want to change the Delay try this one  
                Process.Start("shutdown.exe", "-r -t 10");
            }
            else if (nInfo == 2)
            {
                //重启程序
                StopAllThread();
                Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
        }

        private void MainWindowConfigChangeEvent(MsgDownConfig mdc)
        {

            //MsgDownConfigBack mdcb = new MsgDownConfigBack();
            //mdcb.Serial = ConfigOperator.Single().StrSerialNum;
            //if (!ConfigOperator.Single().SaveManagerConfig(mdc))
            //{
            //    mdcb.StrReturn = "保存配置失败，请检查后重新下发";
            //}
            //else
            //{
            //    mdcb.StrReturn = "保存配置成功";
            //}
        }

        /// <summary>
        /// 主界面 开门
        /// </summary>
        public void MainWindowOpenDoor()
        {
            _OpenDoor.WriteCom();
        }

        /// <summary>
        /// 是否需要画线
        /// </summary>
        /// <param name="rc"></param>
        /// <param name="bNeed"></param>
        private void CaptureThreadDrawLineEvent(RECT rc, bool bNeed)
        {
            OnDrawFace(rc, bNeed);
        }

        /// <summary>
        /// 状态转变事件
        /// </summary>
        /// <param name="oldStatue"></param>
        /// <param name="newStatue"></param>
        private void CaptureThread_statueChangeEvent(ESTATUSTIP oldStatue, ESTATUSTIP newStatue)
        {
            OnShowTipChangeNew(newStatue);
            if (oldStatue == ESTATUSTIP.EST_DetectComplete)
            {
                if (_cardReaderBrand == "0")
                {
                    m_readIdcard.IsNeedCapture = true;
                }
                else if (_cardReaderBrand == "1")
                {
                    m_HSReadIdCard.IsNeedCapture = true;
                }
                else if (_cardReaderBrand == "2")//护照
                {
                    m_ReadPassport.IsNeedCapture = true;
                }else if(_cardReaderBrand.CompareTo("3") == 0)
                {
                    m_readPassport630.IsNeedCapture = true;
                }
            }
        }

        /// <summary>
        /// 比对完成提示事件
        /// </summary>
        /// <param name="detectRes"></param>
        /// <param name="bm"></param>
        /// <param name="strCardPhoto"></param>
        private void DetectThread_detectCompleteEvent(int detectRes, Bitmap bm, string strCardPhoto)
        {
            if (_cardReaderBrand == "0")//身份证
            {
                if (detectRes == 0)
                {
                    ///即未比对
                    m_readIdcard.IsNeedCapture = true;
                    m_currentStatue = ESTATUSTIP.EST_Capture;
                    _captureThread.CurrentSataus = ESTATUSTIP.EST_Capture;
                    OnShowTipChangeNew(ESTATUSTIP.EST_Capture);
                    return;
                }

                m_detectRes = detectRes;
                _captureThread.CurrentSataus = ESTATUSTIP.EST_DetectComplete;
                OnShowTipChangeNew(ESTATUSTIP.EST_DetectComplete, bm, strCardPhoto);
                Trace.WriteLine(ThreadName + "timeRecord:比对完成并触发继续读卡");
            }
            else if (_cardReaderBrand == "2" || _cardReaderBrand == "3")//护照
            {
                if (detectRes == 0)
                {
                    ///即未比对
                    m_ReadPassport.IsNeedCapture = true;
                    m_readPassport630.IsNeedCapture = true;
                    m_currentStatue = ESTATUSTIP.EST_Capture;
                    _captureThread.CurrentSataus = ESTATUSTIP.EST_Capture;
                    OnShowTipChangeNew(ESTATUSTIP.EST_Capture);
                    return;
                }

                m_detectRes = detectRes;
                _captureThread.CurrentSataus = ESTATUSTIP.EST_DetectComplete;
                OnShowTipChangeNew(ESTATUSTIP.EST_DetectComplete, bm, strCardPhoto);
                Trace.WriteLine(ThreadName + "timeRecord:比对完成并触发继续读卡");
            }
        }

        /// <summary>
        /// 获取人脸完成 需要关闭定时器
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="rgb24"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="facePoint"></param>
        /// <param name="rect"></param>
        /// <param name="nIndex"></param>
        private void CaptureThread_captureCompleteEvent(Bitmap bm, byte[] rgb24, int w, int h, FacePointInfo[] facePoint, RECT[] rect, int nIndex)
        {
            try
            {
                //如果已经获取到身份证信息
                if (m_currentStatue == ESTATUSTIP.EST_CardComplete)
                {
                    OnShowReadCardPhoto("", false);
                    m_timeReadCard.Enabled = false;  //关闭定时器

                    Bitmap shadow = DeepCopyBitmap(bm);
                    _detectThread.SetCurrentInfo(shadow, rgb24, w, h, facePoint, rect, nIndex);
                    _detectThread.RunNum++;
                    m_currentStatue = ESTATUSTIP.EST_WaitDetect;
                    _captureThread.CurrentSataus = ESTATUSTIP.EST_WaitDetect;
                    Trace.WriteLine(ThreadName + "timeRecord:抓脸完成并触发比对");
                    OnShowTipChangeNew(ESTATUSTIP.EST_WaitDetect);
                }
                else
                {
                    //Trace.WriteLine(ThreadName + "人脸抓拍完成，因为没有读取到身份证，不进行比对");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ThreadName + "CaptureThread_captureCompleteEvent事件异常:" + ex.Message);
            }
        }

        private void OnShowReadCardPhoto(string strPath,bool bShow = true)
        {
            this.ImgReadCard.Dispatcher.BeginInvoke(new ShowReadCardPhotoDelegate(ShowReadCardPhoto), strPath, bShow);
        }

        private void ShowReadCardPhoto(string strPath, bool bShow = true)
        {
            if (bShow)
            {
                if (!File.Exists(strPath)) return;
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(strPath, UriKind.RelativeOrAbsolute);
                bi.EndInit();
                bi.Freeze();
                ImgReadCard.Visibility = Visibility.Visible;
                ImgReadCard.Source = bi;
            }
            else
            {
                ImgReadCard.Visibility = Visibility.Collapsed;
            }
        }

        delegate void ShowReadCardPhotoDelegate(string strPath, bool bShow = true);
        public static Bitmap DeepCopyBitmap(Bitmap bitmap)
        {
            try
            {
                Bitmap dstBitmap = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
                return dstBitmap;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("DeepCopyBitmap error " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 读卡完成消息处理
        /// </summary>
        /// <param name="info">身份证信息结构体</param>
        /// <returns></returns>
        private void OnReadCardComplete(SCardInfo info)
        {
            try
            {
                m_timeReadCard.Enabled = true;

                //保存读取到的身份证信息
                Trace.WriteLine("设置证件信息到比对线程:"+info._idNum);
                _detectThread.SetCardData(info, 0);

                //改变全局状态，设置当前状态为读取身份证完成
                m_currentStatue = ESTATUSTIP.EST_CardComplete;


                //清除读取状态，允许再次读取身份证
                m_readIdcard.IsNeedCapture = false;
                m_HSReadIdCard.IsNeedCapture = false;
                m_ReadPassport.IsNeedCapture = false;//护照
                m_readPassport630.IsNeedCapture = false;

                //更新提示,界面刷新
                SCardInfo cInfo = (SCardInfo)info;
                OnShowReadCardPhoto(cInfo._photo);
                OnShowTipChangeNew(ESTATUSTIP.EST_CardComplete);
                Trace.WriteLine(ThreadName + "timeRecord:读卡完成并触发抓拍人脸");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ThreadName + "读卡完成消息处理异常:" + ex.Message);
            }
            SavePlateCardInfo(info);
        }

        /// <summary>
        /// 读卡完成消息处理
        /// </summary>
        /// <param name="info">护照信息结构体</param>
        /// <returns></returns>
        private void OnReadCardComplete(object info, int nType)
        {
            try
            {
                if (nType == 2)//护照读取
                {
                    PassportInfo psInfo = (PassportInfo)info; 
                    if (psInfo == null)//读取错误
                    {
                        //改变全局状态，设置当前状态为读取护照出错
                        m_currentStatue = ESTATUSTIP.EST_Prompt;
                        OnShowTipChangeNew(m_currentStatue);//请正确放置证件
                        //允许读取护照信息
                        if (_cardReaderBrand == "2")//护照
                        {
                            m_ReadPassport.IsNeedCapture = true;
                        }else if(_cardReaderBrand == "3")
                        {
                            m_readPassport630.IsNeedCapture = true;
                        }
                        m_timeMisread.Enabled = true;//启动护照读取错误提示后，重置请刷证件线程

                        return;
                    }
                    else if (
                        (psInfo.PersonalIdNo.ToString() == "" &&
                        psInfo.PassportNo.ToString() == "")
                             /*&& psInfo.Name.ToString() == ""
                             && psInfo.Sex.ToString() == ""
                             || psInfo.DateOfBirth.ToString() == ""*/)//读取错误
                    {
                        //改变全局状态，设置当前状态为读取护照出错
                        m_currentStatue = ESTATUSTIP.EST_Prompt;
                        OnShowTipChangeNew(m_currentStatue);//请正确放置证件
                        //允许读取护照信息
                        if (_cardReaderBrand == "2")//护照
                        {
                            m_ReadPassport.IsNeedCapture = true;
                        }else if(_cardReaderBrand == "3")
                        {
                            m_readPassport630.IsNeedCapture = true;
                        }
                        m_timeMisread.Enabled = true;//启动护照读取错误提示后，重置请刷证件线程
                        return;
                    }
                    m_timeMisread.Enabled = false;//正确放置证件提示中，如果读到证件信息，则结束重置线程。
                    m_timeReadCard.Enabled = true;
                    _detectThread.SetCardData(info, nType);

                    //改变全局状态，设置当前状态为读取护照完成
                    m_currentStatue = ESTATUSTIP.EST_CardComplete;

                    //保存读取到的护照信息
                    OnShowReadCardPhoto(psInfo.PhotoHead);

                    //清除读取状态，允许再次读取护照
                    m_ReadPassport.IsNeedCapture = false;//护照

                    //更新提示,界面刷新
                    OnShowTipChangeNew(ESTATUSTIP.EST_CardComplete);
                    Trace.WriteLine(ThreadName + "timeRecord:读卡完成并触发抓拍人脸护照");
                }
                else if(nType == 0)
                {
                    SCardInfo cInfo = (SCardInfo)info;
                    OnShowReadCardPhoto(cInfo._photo);
                    Trace.WriteLine("读取到身份证：" + cInfo._name+",照片位置："+cInfo._photo);
                    OnReadCardComplete((SCardInfo)info);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ThreadName + "读卡完成消息处理异常:" + ex.Message);
            }
        }

        private void mainWindow_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        System.Timers.Timer timeShowTime = new System.Timers.Timer(1000);

        private void ShowLeftInfo()
        {
            ImgReadCard.Width = 100;
            ImgReadCard.Height = 126;


            labOSName.Width = 200;
            labOSName.Height = 40;
            Thickness tk = new Thickness();
            if (bScreenHorizontal)
            {
                //横屏
                tk.Left = 30; tk.Top = 200;
                if (_IsPc) tk.Top = mainWindow.Height / 3;
                labOSName.Margin = tk;
                labOSName.Content = "人行通道管理系统";
                labOSName.FontSize = 20;
            }
            else
            {
                labOSName.Content = "";
                tk.Left = 10;tk.Top = 50;
            }
            
            labCuttentTime.Width = 210;
            labCuttentTime.Height = 40;
            if (bScreenHorizontal)
            {
                tk.Left = 15; tk.Top = 360;
                if (_IsPc) tk.Top = mainWindow.Height / 3 * 2;
            }else
            {
                tk.Left = 5;tk.Top = 100;
            }
            labCuttentTime.Margin = tk;
            tk.Top = 50;tk.Left = 30;
            ImgReadCard.Margin = tk;
            labCuttentTime.FontSize = 20;
            timeShowTime.Elapsed += TimeShowTimeElapsed;
            timeShowTime.AutoReset = true;
            labCuttentTime.Visibility = Visibility.Visible;
        }

        private void OnRealShowCurrentTime(string strInfo)
        {
            this.labCuttentTime.Content = strInfo;
        }
        private void OnCurrentTimeShow(string strInfo)
        {
            this.labCuttentTime.Dispatcher.Invoke(new ShowCurrentTimeD(OnRealShowCurrentTime), strInfo);
        }

        private void TimeShowTimeElapsed(object sender, ElapsedEventArgs e)
        {
            string strTimeInfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OnCurrentTimeShow(strTimeInfo);
            //ShowCurrentTimeD tCurrent = n => {  OnRealShowCurrentTime(strTimeInfo); };
        }

        /// <summary>
        /// 窗口加载函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string strWaitPhoto = strFilePath + @"\skin\waitingHClear.png";
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(strWaitPhoto, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            bi.Freeze();
            ShowLeftInfo();
#if DEBUG
#else

            System.Windows.Forms.Screen[] s0 = System.Windows.Forms.Screen.AllScreens;

            System.Windows.Forms.Screen s1 = System.Windows.Forms.Screen.PrimaryScreen;//.AllScreens;

            Rectangle r1 = s1.WorkingArea;
            this.Top = r1.Top;
            this.Left = r1.Left;
            this.Width = r1.Width;
            this.Height = r1.Height;
            for(int i = 0; i < s0.Length; ++i)
            {
                Trace.WriteLine(i + " width = " + s0[i].WorkingArea.Width + " height = " + s0[i].WorkingArea.Height);
            }
#endif
            try
            {
                Trace.WriteLine(ThreadName + "硬件初始化完成");
                Trace.WriteLine(ThreadName + "开始注册各种事件...");
                #region  各种事件注册

                //注册读卡完成事件
                if (_cardReaderBrand == "0")
                {
                    m_readIdcard.readCompleteEvent += this.OnReadCardComplete;
                }
                else if (_cardReaderBrand == "1")
                {
                    m_HSReadIdCard.readCompleteEvent += this.OnReadCardComplete;
                }
                else if (_cardReaderBrand == "2")//护照
                {
                    m_ReadPassport.readCompleteEvent += this.OnReadCardComplete;
                }else if(_cardReaderBrand.CompareTo("3") == 0)
                {
                    m_readPassport630.OnReadCardEvent += OnReadCardComplete;
                }
                _captureThread.statueChangeEvent += CaptureThread_statueChangeEvent;
                _captureThread.captureCompleteEvent += CaptureThread_captureCompleteEvent;
                _captureThread.drawLineEvent += CaptureThreadDrawLineEvent;
                _detectThread.detectCompleteEvent += DetectThread_detectCompleteEvent;
                _detectThread.SetOpenDoorOperator(_OpenDoor);

                //摄像头事件
                _vedioPlay.vedioPlayEvent += VedioPlayEvent;
                Trace.WriteLine(ThreadName + "事件注册完成");
                #endregion

                //开启对比线程
                Trace.WriteLine(ThreadName + "_detectThread.Start()...");
                _detectThread.Start();
                Thread.Sleep(3000);
                if (ConfigOperator.Single().nIsDoubleScreen == 1)
                {
                    OhterProcess myDbExe = new OhterProcess();
                    myDbExe.startExeFile(@".\doublescreen", null, 0);
                    Thread.Sleep(3000);
                    dbscclient.Single().BeginListen();
                    if (_cardReaderBrand == "0")//身份证
                    {
                        dbscclient.Single().readCompleteEvent += OnReadCardComplete;
                    }
                    if (_cardReaderBrand == "2")//护照
                    {
                        dbscclient.Single().readCardCompleteEvent += OnReadCardComplete;
                    }
                    Thread.Sleep(50);
                    dbscclient.Single().ClientSendMsg("hello");
                }

                
                SetAutoBootStatu(true);
                Trace.WriteLine(ThreadName + "侧屏软件开启成功");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ThreadName + "主窗体加载异常:" + ex.Message);
            }

            gridDetectRes.Visibility = Visibility.Visible;
            this.Background = new ImageBrush
            {
                ImageSource = bi
            };
            labOSName.Visibility = Visibility.Visible;
            timeShowTime.Start();
        }

        [System.Runtime.InteropServices.DllImport("./startupdll.dll", EntryPoint = "External_SetProgramStartup")]
        extern static void External_SetProgramStartup(System.Text.StringBuilder str, int secondsDelay, bool startup);
        /// <summary>  
        /// 在启动项中写入批处理文件 开机延长30秒后启动  
        /// </summary>  
        private bool SetAutoBootStatu(bool isAutoBoot)
        {
            System.Text.StringBuilder str = new System.Text.StringBuilder("启动核查机");
            External_SetProgramStartup(str, 30, isAutoBoot);
            return true;
        }

        /// <summary>
        /// 是否需要裁剪
        /// </summary>
        private bool _bNeedCut = false;

        System.Drawing.Rectangle _rtVedio = new System.Drawing.Rectangle();
        double _mainwindowWidth = 0, _mainwindowHeight = 0;
        private double _oneW, _oneH;

        /// <summary>
        /// 判断照片是否需要裁剪
        /// </summary>
        /// <param name="bm"></param>
        private void JudgeCutVedioImg(Bitmap bm)
        {
            double fScale = (double)bm.Width / (double)bm.Height;
            double fVedioScale = _mainwindowWidth / _mainwindowHeight;
            //照片较宽  需要裁剪原照片宽度像素
            if (fScale > fVedioScale)
            {
                _bNeedCut = true;
                int bmRealW = Convert.ToInt32(_mainwindowWidth) * bm.Height / Convert.ToInt32(_mainwindowHeight);

                int nCutW = (bm.Width - bmRealW) / 2;
                _rtVedio.X = nCutW;
                _rtVedio.Y = 0;
                _rtVedio.Width = bmRealW;
                _rtVedio.Height = bm.Height;
                _oneW = bmRealW;
                _oneH = bm.Height;
            }
            else if (fScale == fVedioScale)
            {
                _bNeedCut = false;
                _oneW = bm.Width;
                _oneH = bm.Height;
            }
            else if (fScale < fVedioScale)
            {
                _bNeedCut = true;

                int bmRealH = Convert.ToInt32(_mainwindowHeight) * bm.Width / Convert.ToInt32(_mainwindowWidth);
                int nCutH = (bm.Height - bmRealH) / 2;
                _rtVedio.X = 0;
                _rtVedio.Y = nCutH;
                _rtVedio.Height = bmRealH;
                _rtVedio.Width = bm.Width;

                _oneW = bm.Width;
                _oneH = bmRealH;
            }
        }
        private BitmapSource GetBitmapSource(Bitmap bmp)
        {
            IntPtr hBitmap = IntPtr.Zero;
            BitmapSource bmpsrc = null;
            if (bmp != null)
            {
                try
                {
                    //using(Graphics g = Graphics.FromImage(bmp))
                    //{
                    //    String str = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    //    Font font = new Font("宋体", 8);
                    //    SolidBrush sbrush = new SolidBrush(System.Drawing.Color.White);
                    //    g.DrawString(str, font, sbrush, new PointF(10, 120));
                    //    MemoryStream ms = new MemoryStream();
                    //    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    //}
                    hBitmap = bmp.GetHbitmap();
                    bmpsrc = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("视频图片转换错误..." + ex.Message);
                }
                finally
                {
                    if (hBitmap != IntPtr.Zero)
                    {
                        if (!DeleteObject(hBitmap))//记得要进行内存释放。否则会有内存不足的报错。
                        {
                            throw new System.ComponentModel.Win32Exception();
                        }
                    }
                    else
                    {
                        Trace.WriteLine("视频为空,请检查摄像头连接");
                    }
                }
            }
            return bmpsrc;
        }
        bool _bFirstT = true;

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr o);
        private void ShowVedios(Bitmap frame)
        {
            
            if (_bFirstT)
            {
                if (bScreenHorizontal)
                {
                    //横屏显示
                    ImgBackGroundRight.Source = imgBackGroundRight;
                    ImgBackGroundTop.Source = imgBackGroundTop;
                    ImgBackGroundBottom.Source = imgBackGroundBottom;
                    gifLogoImg = new ShowGifImage(strFilePath + @"\skin\logoH.png");
                    gifLogoImg.StartAnimate();

                    ImgLogo.Source = imgLogo;
                }
                else
                {
                    //竖屏显示
                    ImgBackGroundTop.Source = imgBackGroundTop;
                    ImgBackGroundBottom.Source = imgBackGroundBottom;
                    gifLogoImg = new ShowGifImage(strFilePath + @"\skin\logoV.png");
                    gifLogoImg.StartAnimate();
                    LinePartDetect.Visibility = Visibility.Visible;
                    ImgLogo.Source = imgLogo;
                }
                _bFirstT = false;
            }

            if (frame != null)
            {
                imgBox_vedio.Source = GetBitmapSource(frame);
            }
        }

        private void OnShowVedio(Bitmap frame)
        {
            this.imgBox_vedio.Dispatcher.BeginInvoke(new VedioPlayDelegate(ShowVedios), frame);
        }

        /// <summary>
        /// 如果为首次启动
        /// </summary>
        private bool _bFirst = true;
        bool _bStartCapture = true;

        /// <summary>
        /// 视频显示事件
        /// </summary>
        /// <param name="frame"></param>
        private void VedioPlayEvent(Mat frame)
        {
            int nRotateFlip = ConfigOperator.Single().RotateFlip;
            RotateFlipType rft = RotateFlipType.Rotate270FlipNone;
            if (nRotateFlip != 0)
            {
                switch (nRotateFlip)
                {
                    case 1: rft = RotateFlipType.Rotate90FlipNone; break;
                    case 2: rft = RotateFlipType.RotateNoneFlipY; break;
                    default: break;
                }
            }

            try
            {
                //如果是首次启动
                if (_bFirst)
                {
                    if (nRotateFlip != 0) {
                        Bitmap bmJudge = frame.Bitmap;
                        bmJudge.RotateFlip(rft);
                        //bmJudge.Save("1.jpg", ImageFormat.Jpeg);
                        JudgeCutVedioImg(bmJudge);
                    }else
                        JudgeCutVedioImg(frame.Bitmap);
                    Trace.WriteLine(ThreadName + "JudgeCutVedioImg finish");
                    _bFirst = false;
                }

                if (_bStartCapture == true && _initThread.NInited == 1)
                {
                    if (_captureThread != null)
                    {
                        _bStartCapture = false;
                        _captureThread.GetCamera(_vedioPlay);
                        _captureThread.SetCutImgInfo(_bNeedCut, _rtVedio);
                        Trace.WriteLine(ThreadName + "_captureThread.Start()...");
                        _captureThread.Start();
                        _captureThread.CurrentSataus = ESTATUSTIP.EST_Capture;
                        Trace.WriteLine(ThreadName + "_captureThread.Start()执行完成");
                    }
                    else
                    {
                        Trace.WriteLine("_captureThread == null");
                    }
                }
                Bitmap bm = frame.Bitmap;
                if (nRotateFlip != 0)
                {
                    bm.RotateFlip(rft);
                }
                if (_bNeedCut)
                {
                    bm = (Bitmap)bm.Clone(_rtVedio, bm.PixelFormat);
                }
                else
                {
                    bm = (Bitmap)bm.Clone();
                }
                
                OnShowVedio(bm);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 窗体关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Trace.WriteLine(ThreadName + "开始执行主窗体关闭线程...");
                if (m_bOpened)
                {
                    m_bOpened = false;
                }
                StopAllThread();

                Process.GetCurrentProcess().Kill();

                Trace.WriteLine(ThreadName + "主窗体关闭线程执行完成");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ThreadName + "主窗体关闭线程异常:" + ex.Message);
            }
        }

        private void mainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
        }

        private void mainWindow_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        /// <summary>
        /// 关闭所有开启线程
        /// </summary>
        private void StopAllThread()
        {
            try
            {
                Trace.WriteLine(ThreadName + "开始关闭所有线程...");
                _detectThread.Stop(0);
                _vedioPlay.StopRun();
                _initThread.Stop(0);
                _gface.Uninit();
                _captureThread.Stop(0);
                Trace.WriteLine(ThreadName + "关闭所有线程完成");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ThreadName + "关闭所有线程异常:" + ex.Message);
            }
        }

        private GFaceRecognizer _gface = GFaceRecognizer.GetInstance();
        private void mainWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.Key).ToString() == "q" || (e.Key).ToString() == "Q")
            {
                StopAllThread();
                ComOpenDoor.getInstance().CloseCom();
                Process.GetCurrentProcess().Kill();
            }
        }

        /// <summary>
        /// 根据配置文件中设置的设备类型加载不同的license.dat文件，刘飞翔，2018-05-04
        /// </summary>
        /// <returns></returns>
        private bool InitLicsence()
        {
            try
            {
                if (_cardReaderBrand == "2")//中安未来快证通设备
                {
                    Trace.WriteLine("加载中安未来快证通设备license.dat文件");
                    File.Delete(@"license.dat");
                    if (File.Exists(@"./license/Passport/license.dat"))
                    {
                        File.Copy(@"./license/Passport/license.dat", @"license.dat", true);
                        Trace.WriteLine("成功加载中安未来快证通设备license.dat文件");
                        return true;
                    }
                    else
                    {
                        Trace.WriteLine("中安未来快证通设备license.dat文件加载失败");
                        return false;
                    }

                }
                else
                {
                    Trace.WriteLine("加载身份证读卡器license.dat文件");
                    File.Delete(@"license.dat");
                    if (File.Exists(@"./license/IDCard/license.dat"))
                    {
                        File.Copy(@"./license/IDCard/license.dat", @"license.dat", true);
                        Trace.WriteLine("成功加载身份证读卡器license.dat文件");
                        return true;
                    }
                    else
                    {
                        Trace.WriteLine("身份证读卡器license.dat文件加载失败");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("license.dat文件加载失败" + ex);
                return false;
            }
        }
    }
}