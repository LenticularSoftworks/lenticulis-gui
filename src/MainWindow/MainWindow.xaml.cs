using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;

namespace lenticulis_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //List of acceptable file extensions
        public static string[] Extensions = { ".jpg", ".png", ".gif" };

        public MainWindow()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();

            GetDrives();
        }

        /// <summary>
        /// Write list of drives into file browser.
        /// </summary>
        private void GetDrives()
        {
            List<BrowserItem> items = new List<BrowserItem>();
            DriveInfo[] drives = DriveInfo.GetDrives();

            for (int i = 0; i < drives.Length; i++)
            {
                items.Add(new BrowserItem(drives[i].Name, drives[i].Name, "drive", true));
            }

            BrowserList.ItemsSource = items;
            AddressBlock.Text = "Tento počítač";
        }

        /// <summary>
        /// Load directory contents.
        /// </summary>
        /// <param name="path">Directory path</param>
        private void ActualFolder(String path)
        {
            //List of files
            List<BrowserItem> items = new List<BrowserItem>();

            DirectoryInfo dir = new DirectoryInfo(path);
            DirectoryInfo[] directories = dir.GetDirectories().Where(file => (file.Attributes & FileAttributes.Hidden) == 0).ToArray();
            FileInfo[] files = dir.GetFiles();

            //Add path to parent
            if (dir.Parent != null)
            {
                items.Add(new BrowserItem("..", dir.Parent.FullName, "parent", true));
            }
            else if (dir != dir.Root)
            {
                items.Add(new BrowserItem("..", "root", "parent", true));
            }

            //Add directories
            for (int i = 0; i < directories.Length; i++)
            {
                items.Add(new BrowserItem(directories[i].Name, directories[i].FullName, "dir", true));
            }

            //Add files
            for (int i = 0; i < files.Length; i++)
            {
                items.Add(new BrowserItem(files[i].Name, files[i].Directory.ToString(), files[i].Extension, false));
            }

            AddressBlock.Text = dir.FullName;
            BrowserList.ItemsSource = items;
        }

        /// <summary>
        /// Open folder by selected item in browser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_DoubleClick(object sender, EventArgs e)
        {
            BrowserItem BItem = (BrowserItem)(BrowserList.SelectedItem);

            if (BItem.Dir && BItem.Path != "root")
            {
                try
                {
                    ActualFolder(BItem.Path);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message, "Nelze otevřít", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if (BItem.Dir && BItem.Path == "root")
            {
                GetDrives();
            }
        }
    }
}
