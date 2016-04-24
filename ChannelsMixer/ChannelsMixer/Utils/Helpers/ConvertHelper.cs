using System;
using System.Drawing;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChannelsMixer.Utils
{
    public class ConvertHelper
    {
        //public static BitmapSource ToBitmapSource(byte[] bytes)
        //{
        //    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        //                  source.GetHbitmap(),
        //                  IntPtr.Zero,
        //                  Int32Rect.Empty,
        //                  BitmapSizeOptions.FromEmptyOptions());
        //}

        public static BitmapSource ToBitmapSource(Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          source.GetHbitmap(),
                          IntPtr.Zero,
                          Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
        }
        public static Bitmap ToBitmap(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

    }
}
