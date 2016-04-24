using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChannelsMixer.Properties;

namespace ChannelsMixer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }

        private void LoadSettings()
        {
            // Load window options
            this.WindowState = Settings.Default.MainWindowState;
            {
                // Load window position
                var location = Settings.Default.MainWindowLocation;
                if (location.X == 0 && location.Y == 0)
                {
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
                else
                {
                    this.Left = location.X;
                    this.Top = location.Y;
                }
            }
            {
                // Load window size
                var size = Settings.Default.MainWindowSize;
                this.Width = size.Width;
                this.Height = size.Height;
            }
        }

        private void SaveSettings()
        {
            // Save window options
            Settings.Default.MainWindowState = this.WindowState;
            {   // Load window position
                var location = this.PointToScreen(new Point());
                Settings.Default.MainWindowLocation = location;
            }
            {   // Load window size
                var size = new Size(this.Width, this.Height);
                Settings.Default.MainWindowSize = size;
            }

            Settings.Default.Save();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.LoadSettings();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            this.SaveSettings();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void MainWindow_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                if (this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;
                else if (this.WindowState == WindowState.Normal)
                    this.WindowState = WindowState.Maximized;
        }
    }
}
