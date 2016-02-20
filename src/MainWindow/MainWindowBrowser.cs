using lenticulis_gui.src.App;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace lenticulis_gui
{
    public partial class MainWindow
    {
        #region Browser methods
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
            AddressBlock.Text = LangProvider.getString("MY_COMPUTER");
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
        /// Select item from last used tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LastUsed_DoubleClick(object sender, EventArgs e)
        {
            // no action for now?
        }

        /// <summary>
        /// Adds specified resource to last used items list, if it's not already there
        /// </summary>
        /// <param name="path">path to file</param>
        /// <param name="name">filename</param>
        /// <param name="extension">file extension</param>
        public void AddLastUsedItem(String path, String name, String extension)
        {
            String fullpath = path + ((path[path.Length - 1] == '\\') ? "" : "\\") + name;

            // check for presence in last used list
            bool found = false;
            foreach (BrowserItem bi in LastUsedList.Items)
            {
                if (fullpath.Equals(bi.Path + ((bi.Path[bi.Path.Length - 1] == '\\') ? "" : "\\") + bi.Name))
                {
                    found = true;
                    break;
                }
            }

            // if not yet there, add it
            if (!found)
                LastUsedList.Items.Add(new BrowserItem(name, path, extension, false));
        }

        /// <summary>
        /// Gets the object for the element selected in the listbox
        /// </summary>
        /// <param name="source"></param>
        /// <param name="point"></param>
        /// <returns>object data of selected element in listbox</returns>
        private static object GetObjectDataFromPoint(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                //get the object from the element
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    // try to get the object value for the corresponding element
                    data = source.ItemContainerGenerator.ItemFromContainer(element);

                    //get the parent and we will iterate again
                    if (data == DependencyProperty.UnsetValue)
                    {
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    }

                    //if we reach the actual listbox then we must break to avoid an infinite loop
                    if (element == source)
                    {
                        return null;
                    }
                }

                //return the data that we fetched only if it is not Unset value
                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }

            return null;
        }

        #endregion Browser methods

        #region Browser listeners

        /// <summary>
        /// Browser listener - Double click opens folder, click starts drag n drop action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBox parent = (ListBox)sender;
            //dragged data as browser item
            BrowserItem browserItem = (BrowserItem)GetObjectDataFromPoint(parent, e.GetPosition(parent));

            if (browserItem == null)
                return;

            if (e.ClickCount == 2)
            {
                //Open folder
                Browser_DoubleClick(browserItem);
            }
            else
            {
                //drag browser item
                Browser_Click(browserItem, parent);
            }
        }

        /// <summary>
        /// Browser drag handler
        /// </summary>
        /// <param name="browserItem">dragged browser item</param>
        /// <param name="parent">parent component</param>
        private void Browser_Click(BrowserItem browserItem, ListBox parent)
        {
            // dragged item has to be instance of browserItem
            if (browserItem == null)
                return;

            //drag drop event
            DragDrop.DoDragDrop(parent, browserItem, System.Windows.DragDropEffects.Move);
        }

        /// <summary>
        /// Open folder by selected item in browser
        /// </summary>
        /// <param name="browserItem">selected browser item</param>
        private void Browser_DoubleClick(BrowserItem browserItem)
        {
            if (browserItem.Dir && browserItem.Path != "root")
            {
                try
                {
                    ActualFolder(browserItem.Path);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, LangProvider.getString("COULD_NOT_OPEN"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    GetDrives();
                }
            }
            else if (browserItem.Dir && browserItem.Path == "root")
            {
                GetDrives();
            }
            // disable this action for now - we will allow putting things to project only by dragging them onto timeline
            /*else if (!BItem.Dir)
            {
                bool result = LoadAndPutResource(BItem.Path + "\\" + BItem.Name, BItem.Extension);

                // positive result means the image was successfully loaded and put into canvas + timeline
                if (result)
                {
                    // check for presence in last used list
                    bool found = false;
                    foreach (BrowserItem bi in LastUsedList.Items)
                    {
                        if (bi.Path.Equals(BItem.Path))
                        {
                            found = true;
                            break;
                        }
                    }

                    // if not yet there, add it
                    if (!found)
                        LastUsedList.Items.Add(new BrowserItem(BItem.Name, BItem.Path, BItem.Extension, false));
                }
            }*/
        }

        #endregion Browser listeners
    }
}
