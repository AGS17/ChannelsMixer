using System;
using System.Data;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChannelsMixer.Utils
{
    public class FileWorker
    {
        public static BitmapSource OpenImage(FileInfo fileInfo)
        {
            //using (var fs = fileInfo.OpenRead())
            //{
            //    BitmapDecoder decoder = null;

            //    switch (fileInfo.Extension)
            //    {
            //        case ".tga":
            //            break;
            //        case ".jpg":
            //        case ".jpeg":
            //            decoder = new JpegBitmapDecoder(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            //            break;
            //        case ".png":
            //            decoder = new PngBitmapDecoder(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            //            break;
            //        case ".tiff":
            //            decoder = new TiffBitmapDecoder(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            //            break;
            //        case ".gif":
            //            decoder = new GifBitmapDecoder(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            //            break;
            //        case ".bmp":
            //            decoder = new BmpBitmapDecoder(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            //            break;
            //        default:
            //            throw new NotSupportedException($"File extension {fileInfo.Extension} not supported");
            //    }

            //    BitmapSource bitmap = null;

            //    if (decoder?.Frames.Count > 0)
            //        bitmap = decoder.Frames[0];

            //}

            switch (fileInfo.Extension)
            {
                case ".tga":
                    return Ultima.Package.Tga.FromFile(fileInfo.FullName).GetImageAsBitmapSource();
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".tiff":
                case ".gif":
                case ".bmp":
                    var bitmapImage = new BitmapImage(new Uri(fileInfo.FullName, UriKind.Relative));
                    return new FormatConvertedBitmap(bitmapImage, PixelFormats.Bgra32, bitmapImage.Palette, .5);
                default:
                    throw new NotSupportedException($"File extension {fileInfo.Extension} not supported");
            }
        }

        public static void SaveImage(FileInfo fileInfo, BitmapSource source)
        {
            using (var fs = fileInfo.OpenWrite())
            {
                BitmapEncoder encoder = null;

                switch (fileInfo.Extension)
                {
                    case ".tga":
                        break;
                    case ".jpg":
                    case ".jpeg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    case ".png":
                        encoder = new PngBitmapEncoder();
                        break;
                    case ".tiff":
                        encoder = new TiffBitmapEncoder();
                        break;
                    case ".gif":
                        encoder = new GifBitmapEncoder();
                        break;
                    case ".bmp":
                        encoder = new BmpBitmapEncoder();
                        break;
                    default:
                        throw new NotSupportedException($"File extension {fileInfo.Extension} not supported");
                }

                encoder?.Frames.Add(BitmapFrame.Create(source));
                encoder?.Save(fs);
            }
        }

    }
}
