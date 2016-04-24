using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChannelsMixer.Utils
{
    public static class BitmapWorker
    {
        public static BitmapSource GetGrayscale(BitmapSource source)
        {
            var bytes = ToBytes(source);
            
            for (int blue = 0, green = 1, red = 2, alpha = 3; alpha < bytes.Length; blue += 4, green += 4, red += 4, alpha += 4)
            {
                var average = (byte) ((bytes[blue] + bytes[green] + bytes[red]) / 3);
                bytes[blue] = average;
                bytes[red] = average;
                bytes[green] = average;
            }

            return source.CloneWithAnotherBytes(bytes);
        }

        public static BitmapSource GetGrayscaleWithAlpha(BitmapSource source)
        {
            var bytes = ToBytes(source);

            for (int blue = 0, green = 1, red = 2, alpha = 3; alpha < bytes.Length; blue += 4, green += 4, red += 4, alpha += 4)
            {
                var average = (byte)((bytes[blue] + bytes[green] + bytes[red]) / 3 * bytes[alpha] / 255);
                bytes[blue] = average;
                bytes[red] = average;
                bytes[green] = average;
                bytes[alpha] = 255;
            }

            return source.CloneWithAnotherBytes(bytes);
        }

        public static BitmapSource GetRedChannel(BitmapSource source)
        {
            var bytes = ToBytes(source);
            
            for (int blue = 0, green = 1, red = 2, alpha = 3; alpha < bytes.Length; blue += 4, green += 4, red += 4, alpha += 4)
            {
                bytes[blue] = bytes[red];
                bytes[green] = bytes[red];
            }

            return source.CloneWithAnotherBytes(bytes);
        }

        public static BitmapSource GetGreenChannel(BitmapSource source)
        {
            var bytes = ToBytes(source);
            
            for (int blue = 0, green = 1, red = 2, alpha = 3; alpha < bytes.Length; blue += 4, green += 4, red += 4, alpha += 4)
            {
                bytes[blue] = bytes[green];
                bytes[red] = bytes[green];
            }

            return source.CloneWithAnotherBytes(bytes);
        }

        public static BitmapSource GetBlueChannel(BitmapSource source)
        {
            var bytes = ToBytes(source);
            
            for (int blue = 0, green = 1, red = 2, alpha = 3; alpha < bytes.Length; blue += 4, green += 4, red += 4, alpha += 4)
            {
                bytes[green] = bytes[blue];
                bytes[red] = bytes[blue];
            }

            return source.CloneWithAnotherBytes(bytes);
        }

        public static BitmapSource GetAlphaChannel(BitmapSource source)
        {
            var bytes = ToBytes(source);
            
            for (int blue = 0, green = 1, red = 2, alpha = 3; alpha < bytes.Length; blue += 4, green += 4, red += 4, alpha += 4)
            {
                bytes[green] = bytes[alpha];
                bytes[red] = bytes[alpha];
                bytes[blue] = bytes[alpha];
                bytes[alpha] = 255;
            }

            return source.CloneWithAnotherBytes(bytes);
        }

        public static BitmapSource GetBitmapSourceWithReplacedRedChannel(BitmapSource source, BitmapSource additingSource)
        {
            var sourceBytes = ToBytes(source);
            byte[] additingBytes;

            if (Math.Abs(source.Width - additingSource.Width) > 1 || Math.Abs(source.Height - additingSource.Height) > 1)
                additingBytes = ToBytes(CloneWithDifferentSize(additingSource, source.PixelWidth, source.PixelHeight));
            else
                additingBytes = ToBytes(additingSource);

            for (int blue = 0, green = 1, red = 2, alpha = 3; alpha < sourceBytes.Length && alpha < additingBytes.Length; blue += 4, green += 4, red += 4, alpha += 4)
            {
                var average = CalculateAverageByte(additingBytes[blue],
                    additingBytes[green], additingBytes[red], additingBytes[alpha]);
                sourceBytes[red] = average;
            }

            return source.CloneWithAnotherBytes(sourceBytes);
        }

        public static BitmapSource GetBitmapSourceWithReplacedGreenChannel(BitmapSource source, BitmapSource additingSource)
        {
            var sourceBytes = ToBytes(source);
            byte[] additingBytes;

            if (Math.Abs(source.Width - additingSource.Width) > 1 || Math.Abs(source.Height - additingSource.Height) > 1)
                additingBytes = ToBytes(CloneWithDifferentSize(additingSource, source.PixelWidth, source.PixelHeight));
            else
                additingBytes = ToBytes(additingSource);

            for (int blue = 0, green = 1, red = 2, alpha = 3; alpha < sourceBytes.Length && alpha < additingBytes.Length; blue += 4, green += 4, red += 4, alpha += 4)
            {
                var average = CalculateAverageByte(additingBytes[blue],
                    additingBytes[green], additingBytes[red], additingBytes[alpha]);
                sourceBytes[green] = average;
            }

            return source.CloneWithAnotherBytes(sourceBytes);
        }

        public static BitmapSource GetBitmapSourceWithReplacedBlueChannel(BitmapSource source, BitmapSource additingSource)
        {
            var sourceBytes = ToBytes(source);
            byte[] additingBytes;

            if (Math.Abs(source.Width - additingSource.Width) > 1 || Math.Abs(source.Height - additingSource.Height) > 1)
                additingBytes = ToBytes(CloneWithDifferentSize(additingSource, source.PixelWidth, source.PixelHeight));
            else
                additingBytes = ToBytes(additingSource);

            for (int blue = 0, green = 1, red = 2, alpha = 3; alpha < sourceBytes.Length && alpha < additingBytes.Length; blue += 4, green += 4, red += 4, alpha += 4)
            {
                var average = CalculateAverageByte(additingBytes[blue],
                    additingBytes[green], additingBytes[red], additingBytes[alpha]);
                sourceBytes[blue] = average;
            }

            return source.CloneWithAnotherBytes(sourceBytes);
        }

        public static BitmapSource GetBitmapSourceWithReplacedAlphaChannel(BitmapSource source, BitmapSource additingSource)
        {
            var sourceBytes = ToBytes(source);
            byte[] additingBytes;

            if (Math.Abs(source.Width - additingSource.Width) > 1 || Math.Abs(source.Height - additingSource.Height) > 1)
                additingBytes = ToBytes(CloneWithDifferentSize(additingSource, source.PixelWidth, source.PixelHeight));
            else
                additingBytes = ToBytes(additingSource);

            for (int blue = 0, green = 1, red = 2, alpha = 3; alpha < sourceBytes.Length && alpha < additingBytes.Length; blue += 4, green += 4, red += 4, alpha += 4)
            {
                var average = CalculateAverageByte(additingBytes[blue],
                    additingBytes[green], additingBytes[red], additingBytes[alpha]);
                sourceBytes[alpha] = average;
            }

            return source.CloneWithAnotherBytes(sourceBytes);
        }


        // ------------------------------ PRIVATE ------------------------------ //

        private static BitmapSource CloneWithDifferentSize(BitmapSource source, double width, double height)
        {
            var rect = new Rect(0, 0, width, height);

            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(source, rect));

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawDrawing(group);

            var resizedImage = new RenderTargetBitmap(
                (int)width, (int)height, 96, 96, PixelFormats.Default);
            resizedImage.Render(drawingVisual);

            return resizedImage;
        }

        private static byte CalculateAverageByte(byte red, byte green, byte blue, byte alpha)
            => (byte) ((blue + green + red) / 3 * alpha / 255);

        public static byte[] ToBytes(BitmapSource source)
        {
            int strideRow = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;
            var pixels = new byte[source.PixelHeight * strideRow];
            source.CopyPixels(pixels, strideRow, 0);
            return pixels;
        }

        private static BitmapSource CloneWithAnotherBytes(this BitmapSource sourceOptions, byte[] bytes)
        {
            var strideRow = (sourceOptions.PixelWidth * sourceOptions.Format.BitsPerPixel + 7) / 8;

            var lenghtRequired = strideRow*sourceOptions.PixelHeight;
            if (lenghtRequired != bytes.Length)
                throw new ArgumentException($"Bytes array lenght must be {lenghtRequired}, not {bytes.Length}");

            return BitmapSource.Create(
                sourceOptions.PixelWidth, sourceOptions.PixelHeight, sourceOptions.DpiX, sourceOptions.DpiY,
                sourceOptions.Format, sourceOptions.Palette, bytes, strideRow);
        }

    }
}