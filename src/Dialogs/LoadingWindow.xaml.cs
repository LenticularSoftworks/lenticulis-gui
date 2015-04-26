using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using lenticulis_gui.src.App;
using MahApps.Metro.Controls;

namespace lenticulis_gui
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        /// <summary>
        /// Following stuff is there only to disable cross in window handle
        /// </summary>

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private String windowType;

        public LoadingWindow(String type = "image")
        {
            InitializeComponent();

            windowType = type;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // disable cross on window
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);

            // depending on type, use appropriate image and texts
            switch (windowType)
            {
                case "image":
                    this.Title = LangProvider.getString("LD_LOADING_IMAGE_TITLE");
                    NiceImage.Source = Utils.iconResourceToImageSource("Image");
                    WhatsGoingOnLabel.Content = LangProvider.getString("LD_LOADING_IMAGE_TXT");
                    break;
                case "project":
                    this.Title = LangProvider.getString("LD_LOADING_PROJECT_TITLE");
                    NiceImage.Source = Utils.iconResourceToImageSource("Folder");
                    WhatsGoingOnLabel.Content = LangProvider.getString("LD_LOADING_PROJECT_TXT");
                    break;
                case "export":
                    this.Title = LangProvider.getString("LD_LOADING_EXPORT_TITLE");
                    NiceImage.Source = Utils.iconResourceToImageSource("Export");
                    WhatsGoingOnLabel.Content = LangProvider.getString("LD_LOADING_EXPORT_TXT");
                    break;
                default:
                    this.Title = LangProvider.getString("LD_LOADING_GENERAL_TITLE");
                    NiceImage.Source = Utils.iconResourceToImageSource("Refresh");
                    WhatsGoingOnLabel.Content = LangProvider.getString("LD_LOADING_GENERAL_TXT");
                    break;
            }
        }
    }
}
