using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TargaImage;

namespace ChannelsMixer.Utils
{
    public class FileWorker
    {
        public static BitmapSource OpenImage(FileInfo fileInfo)
        {
            switch (fileInfo.Extension)
            {
                case ".tga":
                    var targa = new TgaImage(fileInfo.FullName);
                    return targa.BitmapSourceImage;
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
                        //var targa = new TargaImage(source.CopyPixels());
                        return;
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
