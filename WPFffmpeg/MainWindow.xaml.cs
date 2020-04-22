
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFffmpeg
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        tstRtmp rtmp = new tstRtmp();
        Thread thPlayer;
        //WriteableBitmap WB;
        public MainWindow()
        {
            InitializeComponent();
            //LiveImg.Source = rtmp.Wb;
            //WB = rtmp.Wb;
            thPlayer = new Thread(DeCoding);
            thPlayer.IsBackground = true;
            thPlayer.Start();
        }

        /// <summary>
        /// 播放线程执行方法
        /// </summary>
        private unsafe void DeCoding()
        {
            try
            {
                Console.WriteLine("DeCoding run...");
                //Bitmap oldBmp = null;


                // 更新图片显示
                tstRtmp.ShowBitmap show = (width, height, stride, data) =>
                {
                    //    //    this.Invoke(new MethodInvoker(() =>
                    //    //    {
                    //    //        this.pic.Image = bmp;
                    //    //        if (oldBmp != null)
                    //    //        {
                    //    //            oldBmp.Dispose();
                    //    //        }
                    //    //        oldBmp = bmp;
                    //    //    }));
                    WriteableBitmap Wb = null;
                    Int32Rect rec = Int32Rect.Empty;
                    this.Dispatcher.Invoke(new Action(delegate {
                        //this.LiveImg.Source = bmp;

                        //Solution 1: Official Way
                        //IntPtr ptr = bmp.GetHbitmap();
                        //BitmapSource Bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        //        ptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        ////release resource
                        //DeleteObject(ptr);
                        //this.LiveImg.Source = Bs;

                        //Solution 2: Memory Way
                        //BitmapImage Bi = BitmapToBitmapImage(bmp);
                        //LiveLU.Source = Bi;

                        //Solution 3: WriteableBitmap
                        //Low Frame Rate
                        if (Wb == null)
                        {
                            Wb = new WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null);
                            rec = new Int32Rect(0, 0, width, height);
                        }
                        Wb.Lock();
                        Wb.AddDirtyRect(rec);
                        Wb.WritePixels(rec, data, width * height * 4, stride);
                        //Debug.WriteLine(frameNumber);
                        Wb.Unlock();
                        this.LiveImg.Source = Wb;


                    }));

                };
                rtmp.Start(show, "rtmp://127.0.0.1/live");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine("DeCoding exit");
                rtmp.Stop();

                thPlayer = null;
                //this.Invoke(new MethodInvoker(() =>
                //{
                //    btnStart.Text = "开始播放";
                //    btnStart.Enabled = true;
                //}));
            }
        }

        //Solution 1 Ori
        //[System.Runtime.InteropServices.DllImport("gdi32.dll")]
        //public static extern bool DeleteObject(IntPtr hObject);

        //Solution 2 Ori
        //public BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        //{
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        //        stream.Position = 0;
        //        BitmapImage result = new BitmapImage();
        //        result.BeginInit();
        //        result.CacheOption = BitmapCacheOption.OnLoad;
        //        result.StreamSource = stream;
        //        result.EndInit();
        //        result.Freeze();
        //        return result;
        //    }
        //}

        // D3D Solution
        private void RenderD3D(IntPtr surface, D3DImage d3dImage)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (d3dImage.IsFrontBufferAvailable && surface != IntPtr.Zero)
                {
                    var showRect = new Int32Rect(0, 0, d3dImage.PixelWidth, d3dImage.PixelHeight);
                    d3dImage.Lock();
                    d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface);

                    d3dImage.AddDirtyRect(showRect);
                    d3dImage.Unlock();
                }

            }));

        }

    }
}
