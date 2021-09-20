using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TS_IDCheck
{
    public class MainWindowHelper
    {
        public static void LoadImageInit(BitmapImage bi ,string strPath) {
            bi.BeginInit();
            bi.UriSource = new Uri(strPath, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            bi.Freeze();
        }


    //    public static void ImageLocation(double mainWidth,double mainHeight,Image m_runImg,Image ImgBackGroundRight,
    //        Image imgBox_vedio)
    //    {
    //        #region 定位UI
    //        m_runImg.Width = mainWidth;
    //        m_runImg.Height = mainHeight;
    //        Thickness tk = new Thickness();
    //        if (!bScreenHorizontal)  
    //        {
    //            #region 竖屏
    //            ImgBackGroundTop.Width = mainWidth;
    //            ImgBackGroundTop.Height = 150;

    //            ImgBackGroundBottom.Width = mainWidth;
    //            ImgBackGroundBottom.Height = 140;

    //            imgBox_vedio.Width = mainWidth;
    //            imgBox_vedio.Height = mainHeight;
    //            tk.Top = 0;
    //            imgBox_vedio.Margin = tk;

    //            _mainwindowWidth = imgBox_vedio.Width;
    //            _mainwindowHeight = imgBox_vedio.Height;

    //            //设置logo位置 大小
    //            ImgLogo.Width = 500;
    //            ImgLogo.Height = 140;
    //            ImgLogo.HorizontalAlignment = HorizontalAlignment.Center;//logo居中
    //            ImgLogo.Margin = new Thickness(0, 5, 0, 0);

    //    tk.Top = 10;
    //            tk.Left = ImgBackGroundTop.Width;

    //            //设置“请刷身份证”位置 大小
    //            ImgTip.Height = 140;
    //            ImgTip.Width = 280;
    //            if (mainWidth == 600 && mainHeight == 800)
    //            {
    //                ImgTip.Margin = new Thickness(90, 0, 0, 0);
    //}
    //            else if (mainWidth == 768 && mainHeight == 1024)
    //            {
    //                ImgTip.Margin = new Thickness(-150, 5, 0, 0);
    //            }
    //            else
    //            {
    //                ImgTip.Margin = new Thickness(150, 0, 0, 0);
    //            }

    ////提示语
    //lab_txtTip.Width = 280;
    //            lab_txtTip.Height = 80;
    //            tk.Left = 320; tk.Top = 20;

    //            lab_txtTip.Margin = tk;
    //            lab_txtTip.Content = "请刷证件";

    //            lab_txtTip.Visibility = Visibility.Visible;

    //            gridDetectRes.Width = mainWidth;
    //            gridDetectRes.Height = ImgBackGroundBottom.Height;

    //            ImgDetectRes.Width = gridDetectRes.Width;
    //            ImgDetectRes.Height = gridDetectRes.Height;
    //            ImgDetectRes.Stretch = Stretch.Fill;

    //            ImgSite.Visibility = Visibility.Collapsed;
    //            ImgCard.Visibility = Visibility.Collapsed;
    //            ImgDRes.Visibility = Visibility.Collapsed;
    //            LinePartDetect.X1 = (double)mainWidth / 2;
    //            LinePartDetect.X2 = (double)mainWidth / 2;
    //            LinePartDetect.Visibility = Visibility.Collapsed;

    //            tk.Left = 20;
    //            tk.Bottom = 0;
    //            tk.Right = 0;
    //            tk.Top = 12;
    //            GridDevice.Margin = tk;
    //            GridDevice.Width = 125;
    //            GridDevice.Height = 21;
    //            nLeftBox = 0;
    //            #endregion
    //        }
    //        else
    //        {

    //            ImgBackGroundRight.Height = mainHeight;
    //            ImgBackGroundRight.Width = mainWidth / 3;
    //            nLeftBox = (int)(mainWidth - ImgBackGroundRight.Width - 450);
    //            //横屏显示
    //            imgBox_vedio.Width = 450;//mainWidth;
    //            imgBox_vedio.Height = 600;//mainHeight;
    //            tk.Top = 0;
    //            tk.Left = nLeftBox;
    //            imgBox_vedio.Margin = tk;


    //            tk.Left = mainWidth - ImgBackGroundRight.Width;
    //            ImgBackGroundRight.Margin = tk;

    //            _mainwindowWidth = imgBox_vedio.Width;
    //            _mainwindowHeight = imgBox_vedio.Height;
    //            ImgLogo.Width = 140;
    //            ImgLogo.Height = 100;
    //            //设置logo位置

    //            tk.Top = 30;//add by shenyiqi
    //            tk.Left = mainWidth - ImgBackGroundRight.Width + ImgBackGroundRight.Width / 3 - 15;// add by shenyiqi
    //            tk.Right = 0;
    //            tk.Bottom = tk.Top;//add by shenyiqi
    //            ImgLogo.Margin = tk;
    //            gridDetectRes.Width = ImgBackGroundRight.Width;//add by shenyiqi
    //            gridDetectRes.Height = imgBox_vedio.Height - ImgLogo.Height - 30 - 30 - 20;//add by shenyiqi
    //            gridDetectRes.HorizontalAlignment = HorizontalAlignment.Right;
    //            gridDetectRes.VerticalAlignment = VerticalAlignment.Top;

    //            tk.Top = ImgLogo.Height + 30 + 10;//add by shenyiqi
    //            tk.Left = 10;
    //            tk.Right = 0;
    //            tk.Bottom = 0;
    //            gridDetectRes.Margin = tk;

    //            ImgTip.Height = gridDetectRes.Height / 3 * 2;
    //            ImgTip.Width = gridDetectRes.Width / 3 * 2;
    //            tk.Left = 30;
    //            tk.Top = 0;
    //            tk.Bottom = 30;
    //            tk.Right = 10;
    //            ImgTip.Margin = tk;

    //            tk.Left = 320; tk.Top = ImgTip.Height + 98;
    //            lab_txtTip.Margin = tk;

    //            ImgSite.Visibility = Visibility.Collapsed;
    //            ImgCard.Visibility = Visibility.Collapsed;
    //            ImgDRes.Visibility = Visibility.Collapsed;
    //            LinePartDetectH.Visibility = Visibility.Collapsed;

    //            tk.Left = 20; tk.Top = 0; tk.Right = 0; tk.Bottom = 12;
    //            GridDevice.Margin = tk;
    //            GridDevice.Width = 125;
    //            GridDevice.Height = 21;
    //            GridDevice.VerticalAlignment = VerticalAlignment.Bottom;
    //        }
    //        gridDetectRes.Visibility = Visibility.Hidden;
    //        #endregion
    //    }

    }
}
