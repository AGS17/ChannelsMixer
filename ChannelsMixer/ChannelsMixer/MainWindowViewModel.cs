using System;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChannelsMixer.Core;
using ChannelsMixer.Utils;
using ChannelsMixer.Utils.Commands;
using FreeImageAPI;
using Microsoft.Win32;

namespace ChannelsMixer
{
    public class MainWindowViewModel : BaseViewModel
    {
        public BitmapSource Viewport
        {
            get
            {
                if (this.Bitmap == null || this.Bitmap.PixelWidth == 0 || this.Bitmap.PixelHeight == 0)
                    return null;

                switch (this.ActiveChannel)
                {
                    case Channel.Default:
                        return this.Bitmap;
                    case Channel.Grayscale:
                        return BitmapWorker.GetGrayscale(this.Bitmap);
                    case Channel.GrayscaleWithAlpha:
                        return BitmapWorker.GetGrayscaleWithAlpha(this.Bitmap);
                    case Channel.RedChannel:
                        return BitmapWorker.GetRedChannel(this.Bitmap);
                    case Channel.GreenChannel:
                        return BitmapWorker.GetGreenChannel(this.Bitmap);
                    case Channel.BlueChannel:
                        return BitmapWorker.GetBlueChannel(this.Bitmap);
                    case Channel.AlphaChannel:
                        return BitmapWorker.GetAlphaChannel(this.Bitmap);
                    default:
                        throw new ArgumentException($"{this.ActiveChannel} did not provide", nameof(this.ActiveChannel));
                }
            }
        }

        private BitmapSource bitmap;
        private BitmapSource Bitmap
        {
            get
            {
                return this.bitmap;
            }
            set
            {
                this.bitmap = value;
                this.OnPropertyChanged(nameof(this.Viewport));
                this.OnPropertyChanged(nameof(this.IsImageExist));
            }
        }

        private Channel activeChannel;
        public Channel ActiveChannel
        {
            get { return this.activeChannel; }
            set
            {
                this.activeChannel = value;
                this.OnPropertyChanged(nameof(this.ActiveChannel));
                this.OnPropertyChanged(nameof(this.Viewport));
            }
        }

        public MainWindowViewModel()
        {
            this.OpenImageCommand = new SimpleCommand(this.OpenImageAction);
            this.SaveImageCommand = new SimpleCommand(this.SaveImageAction);
            this.ReplaceRedChannelCommand = new SimpleCommand(this.ReplaceRedChannelAction);
            this.ReplaceGreenChannelCommand = new SimpleCommand(this.ReplaceGreenChannelAction);
            this.ReplaceBlueChannelCommand = new SimpleCommand(this.ReplaceBlueChannelAction);
            this.ReplaceAlphaChannelCommand = new SimpleCommand(this.ReplaceAlphaChannelAction);
        }

        public bool IsImageExist => this.Bitmap != null;

        private string[] SupportedFileExtensions => new [] { "*.tga", "*.jpg", "*.jpeg", "*.png", "*.tiff", "*.gif", "*.bmp" };
        private string GetSupportedExtensionsOpenDialogFilter()
        {
            var stringBuilder = new StringBuilder();
            foreach (var extension in this.SupportedFileExtensions)
                stringBuilder.Append($"{extension}, ");
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append("|");
            foreach (var extension in this.SupportedFileExtensions)
                stringBuilder.Append($"{extension};");
            return stringBuilder.ToString();
        }
        private string GetSupportedExtensionsSaveDialogFilter()
        {
            var stringBuilder = new StringBuilder();
            foreach (var extension in this.SupportedFileExtensions)
                stringBuilder.Append($"{extension}|{extension}|");
            return stringBuilder.ToString(0, stringBuilder.Length - 1);
        }

        public ICommand OpenImageCommand { get; set; }
        private void OpenImageAction()
        {
            var openDialog = new OpenFileDialog { Filter = this.GetSupportedExtensionsOpenDialogFilter() };
            var dialogResult = openDialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                var fileInfo = new FileInfo(openDialog.FileName);
                if (fileInfo.Exists)
                {
                    this.Bitmap = FileWorker.OpenImage(fileInfo);
                    this.OnPropertyChanged(nameof(this.Viewport));
                }
            }
        }

        public ICommand SaveImageCommand { get; set; }
        private void SaveImageAction()
        {
            var saveDialog = new SaveFileDialog { Filter = this.GetSupportedExtensionsSaveDialogFilter() };
            var dialogResult = saveDialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                var fileInfo = new FileInfo(saveDialog.FileName);
                FileWorker.SaveImage(fileInfo, this.Viewport);
            }
        }

        public ICommand ReplaceRedChannelCommand { get; set; }
        private void ReplaceRedChannelAction()
        {
            var openDialog = new OpenFileDialog { Filter = this.GetSupportedExtensionsOpenDialogFilter() };
            var dialogResult = openDialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                var fileInfo = new FileInfo(openDialog.FileName);
                if (fileInfo.Exists)
                {
                    this.Bitmap = BitmapWorker.GetBitmapSourceWithReplacedRedChannel(
                        this.Bitmap, FileWorker.OpenImage(fileInfo));
                    this.OnPropertyChanged(nameof(this.Viewport));
                }
            }
        }

        public ICommand ReplaceGreenChannelCommand { get; set; }
        private void ReplaceGreenChannelAction()
        {
            var openDialog = new OpenFileDialog { Filter = this.GetSupportedExtensionsOpenDialogFilter() };
            var dialogResult = openDialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                var fileInfo = new FileInfo(openDialog.FileName);
                if (fileInfo.Exists)
                {
                    this.Bitmap = BitmapWorker.GetBitmapSourceWithReplacedGreenChannel(
                        this.Bitmap, FileWorker.OpenImage(fileInfo));
                    this.OnPropertyChanged(nameof(this.Viewport));
                }
            }
        }

        public ICommand ReplaceBlueChannelCommand { get; set; }
        private void ReplaceBlueChannelAction()
        {
            var openDialog = new OpenFileDialog { Filter = this.GetSupportedExtensionsOpenDialogFilter() };
            var dialogResult = openDialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                var fileInfo = new FileInfo(openDialog.FileName);
                if (fileInfo.Exists)
                {
                    this.Bitmap = BitmapWorker.GetBitmapSourceWithReplacedBlueChannel(
                        this.Bitmap, FileWorker.OpenImage(fileInfo));
                    this.OnPropertyChanged(nameof(this.Viewport));
                }
            }
        }

        public ICommand ReplaceAlphaChannelCommand { get; set; }
        private void ReplaceAlphaChannelAction()
        {
            var openDialog = new OpenFileDialog { Filter = this.GetSupportedExtensionsOpenDialogFilter() };
            var dialogResult = openDialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                var fileInfo = new FileInfo(openDialog.FileName);
                if (fileInfo.Exists)
                {
                    this.Bitmap = BitmapWorker.GetBitmapSourceWithReplacedAlphaChannel(
                        this.Bitmap, FileWorker.OpenImage(fileInfo));
                    this.OnPropertyChanged(nameof(this.Viewport));
                }
            }
        }
    }
}