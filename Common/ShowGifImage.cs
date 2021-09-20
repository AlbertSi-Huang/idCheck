using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Common
{
    public class ShowGifImage : System.Windows.Controls.Image
    {
        private Bitmap gifBitmap;

        private BitmapSource bitmapSource;

        /// <summary>
        /// Delete local bitmap resource
        /// Reference: http://msdn.microsoft.com/en-us/library/dd183539(VS.85).aspx
        /// </summary>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeleteObject(IntPtr hObject);

        public ShowGifImage(string gifPath)
        {
            this.gifBitmap = new Bitmap(gifPath);
            this.bitmapSource = this.GetBitmapSource();
            this.Source = this.bitmapSource;
        }

        /// <summary>
        /// 从System.Drawing.Bitmap中获得用于显示的那一帧图像的BitmapSource
        /// </summary>
        /// <returns></returns>
        private BitmapSource GetBitmapSource()
        {
            IntPtr handle = IntPtr.Zero;

            try
            {
                handle = this.gifBitmap.GetHbitmap();
                this.bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    DeleteObject(handle);
                }
            }
            return this.bitmapSource;
        }

        /// <summary>
        /// Start animation
        /// </summary>
        public void StartAnimate()
        {
            ImageAnimator.Animate(this.gifBitmap, this.OnFrameChanged);
        }

        /// <summary>
        /// Stop animation
        /// </summary>
        public void StopAnimate()
        {
                ImageAnimator.StopAnimate(this.gifBitmap, this.OnFrameChanged);
        }

        /// <summary>
        /// Event handler for the frame changed
        /// </summary>
        private void OnFrameChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                
                ImageAnimator.UpdateFrames(); // 更新到下一帧
                if (this.bitmapSource != null)
                {
                    this.bitmapSource.Freeze();
                }

                //// Convert the bitmap to BitmapSource that can be display in WPF Visual Tree
                this.bitmapSource = this.GetBitmapSource();
                Source = this.bitmapSource;
                this.InvalidateVisual();
            }));
        }
    }
}
