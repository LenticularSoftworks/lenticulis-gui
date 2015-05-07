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
using MahApps.Metro.Controls;
using lenticulis_gui.src.App;
using lenticulis_gui.Properties;

namespace lenticulis_gui.src.Dialogs
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : MetroWindow
    {
        public AboutWindow()
        {
            InitializeComponent();

            Title = LangProvider.getString("ABOUT_WINDOW_TITLE");

            // we retrieve version programatically instead of using layout macro
            VersionLabel.Content = LangProvider.getString("VERSION_TXT") + " " + lenticulis_gui.Properties.Resources.LENTICULIS_VERSION;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            // only starts default action when clicking on webpage link
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }
}
