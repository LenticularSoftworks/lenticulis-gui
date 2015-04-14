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
using lenticulis_gui.src.App;
using lenticulis_gui.src.Containers;
using lenticulis_gui.src.SupportLib;

namespace lenticulis_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //timeline column dimensions
        private const int rowHeight = 30;
        private const int columnMinWidth = 150;

        //timeline item list
        public List<TimelineItem> timelineList;

        //drag n drop captured items
        private TimelineItem capturedTimelineItem = null;
        private WrapPanel capturedResizePanel = null;

        //drag n drop captured coords and dimensions
        private double capturedX;
        private double capturedY;
        private int capturedTimelineItemColumn;
        private int capturedTimelineItemLength;

        public MainWindow()
        {
            InitializeComponent();

            GetDrives();

            SetImageCount(5); //start image count for testing
            AddTimelineHeader();
            AddTimelineLayer(1); //start layer count 1

            timelineList = new List<TimelineItem>();
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
                    MessageBox.Show(ex.Message, "Nelze otevřít", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else if (BItem.Dir && BItem.Path == "root")
            {
                GetDrives();
            }
            else if (!BItem.Dir)
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
            }
        }

        /// <summary>
        /// Select item from last used tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LastUsed_DoubleClick(object sender, EventArgs e)
        {
            BrowserItem BItem = (BrowserItem)(BrowserList.SelectedItem);

            LoadAndPutResource(BItem.Path + "\\" + BItem.Name, BItem.Extension);
        }

        /// <summary>
        /// Method for loading and putting element on canvas / to timeline to implicit position
        /// </summary>
        /// <param name="path">Path to image file</param>
        /// <param name="extension">Extension (often obtained via file browser)</param>
        /// <returns>True if everything succeeded</returns>
        private bool LoadAndPutResource(String path, String extension)
        {
            if (!Utils.IsAcceptedImageExtension(extension))
                return false;

            // TODO: main loading routine

            // return true if succeeded - may be used to put currently loaded resource to "Last used" tab
            return true;
        }

        /// <summary>
        /// Add timeline header
        /// </summary>
        private void AddTimelineHeader()
        {
            for (int i = 0; i < ProjectHolder.ImageCount; i++)
            {
                System.Windows.Controls.Label label = new System.Windows.Controls.Label()
                {
                    Content = "#" + (i + 1),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Grid.SetColumn(label, i);

                TimelineHeader.Children.Add(label);
            }
        }

        /// <summary>
        /// Set image count
        /// </summary>
        /// <param name="count">image count</param>
        private void SetImageCount(int count)
        {
            for (int i = 0; i < count; i++)
            {
                //Add Timeline column definition
                ColumnDefinition colDef = new ColumnDefinition();
                colDef.MinWidth = columnMinWidth;

                Timeline.ColumnDefinitions.Add(colDef);

                //Add TimelineHeader column definition
                colDef = new ColumnDefinition();
                colDef.MinWidth = columnMinWidth;

                TimelineHeader.ColumnDefinitions.Add(colDef);
            }

            ProjectHolder.ImageCount = count;
        }

        /// <summary>
        /// Add timeline layer
        /// </summary>
        /// <param name="count">layer count</param>
        private void AddTimelineLayer(int count)
        {
            for (int i = 0; i < count; i++)
            {
                RowDefinition rowDef = new RowDefinition();
                rowDef.Height = new GridLength(rowHeight, GridUnitType.Pixel);

                //Add row definition
                Timeline.RowDefinitions.Add(rowDef);
                ProjectHolder.LayerCount++;

                //Create and add horizontal border
                Border horizontalBorder = new Border() { BorderBrush = Brushes.Black };
                horizontalBorder.BorderThickness = new Thickness() { Bottom = 0.5 };

                Grid.SetRow(horizontalBorder, Timeline.RowDefinitions.Count - 1);
                Grid.SetColumnSpan(horizontalBorder, ProjectHolder.ImageCount);

                Timeline.Children.Add(horizontalBorder);

                // create layer object and put it into layer list in project holder class
                Layer layer = new Layer(ProjectHolder.LayerCount - 1);
                ProjectHolder.layers.Add(layer);
            }

            SetTimelineVerticalLines();
        }

        /// <summary>
        /// Add vertical lines
        /// </summary>
        private void SetTimelineVerticalLines()
        {
            //Create and add vertical border
            for (int i = 0; i < ProjectHolder.ImageCount; i++)
            {
                Border verticalBorder = new Border() { BorderBrush = Brushes.Gray };
                verticalBorder.BorderThickness = new Thickness() { Right = 0.5 };

                Grid.SetColumn(verticalBorder, i);
                Grid.SetRowSpan(verticalBorder, ProjectHolder.LayerCount);

                Timeline.Children.Add(verticalBorder);
            }
        }

        /// <summary>
        /// Add layer action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddLayer_Click(object sender, RoutedEventArgs e)
        {
            AddTimelineLayer(1);
        }

        /// <summary>
        /// Remove last layer action listener - if there's images shows yes/no dialog first
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveLayer_Click(object sender, RoutedEventArgs e)
        {
            //it has to be at least one layer
            if (ProjectHolder.LayerCount == 1)
            {
                return;
            }

            int lastLayer = ProjectHolder.LayerCount - 1;
            MessageBoxResult messageBoxResult = MessageBoxResult.Yes;

            foreach (TimelineItem item in timelineList)
            {
                //if some item is in last layer
                if (item.getLayerObject().Layer == lastLayer)
                {
                    messageBoxResult = MessageBox.Show("Ve vrstvě se nachází obrázky. Opravdu chcete vrstvu smazat?", "Smazat vrstvu", MessageBoxButton.YesNo);

                    break;
                }
            }

            //remove last layer
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                RemoveLastLayer();
            }
        }

        /// <summary>
        /// Remove last layer in timeline and project holder and its images
        /// </summary>
        private void RemoveLastLayer() 
        {
            int lastLayer = ProjectHolder.LayerCount - 1;
            //list of deleting items
            List<TimelineItem> deleteItems = new List<TimelineItem>();

            //fill deleteItems list
            foreach (TimelineItem item in timelineList)
            {
                if (item.getLayerObject().Layer == lastLayer)
                {
                    deleteItems.Add(item);
                }
            }

            //remove items in timelineList
            foreach (TimelineItem item in deleteItems)
            {
                timelineList.Remove(item);
                Timeline.Children.Remove(item);
            }

            //remove from Timeline
            Timeline.RowDefinitions.Remove(Timeline.RowDefinitions[Timeline.RowDefinitions.Count - 1]);
            
            //set ProjectHolder
            ProjectHolder.layers.RemoveAt(ProjectHolder.LayerCount - 1);
            ProjectHolder.LayerCount--;
        }

        /// <summary>
        /// Mouse drag n drop action listener
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimelineItem_MouseDown(object sender, MouseEventArgs e)
        {
            capturedTimelineItem = (TimelineItem)sender;

            Point mouse = Mouse.GetPosition((UIElement)sender);

            capturedX = mouse.X;
            capturedY = mouse.Y;
        }

        /// <summary>
        /// Add resize panel action listener
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimelineResize_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            capturedTimelineItem = (TimelineItem)((WrapPanel)sender).Parent;
            capturedResizePanel = (WrapPanel)sender;

            capturedTimelineItemColumn = capturedTimelineItem.getLayerObject().Column;
            capturedTimelineItemLength = capturedTimelineItem.getLayerObject().Length;
        }

        /// <summary>
        /// Timeline item move listener
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timeline_MouseMove(object sender, MouseEventArgs e)
        {
            double columnWidth = Timeline.ColumnDefinitions[0].ActualWidth; //get actual column width

            if (capturedTimelineItem != null)
            {
                if (capturedResizePanel == null)
                {
                    //Timeline item shift
                    TimelineItemShift(columnWidth);
                }
                else
                {
                    //Timeline item resize
                    TimelineItemResize(sender, columnWidth);
                }
            }
        }

        /// <summary>
        /// Time line item shift
        /// </summary>
        /// <param name="columnWidth"></param>
        private void TimelineItemShift(double columnWidth)
        {
            Point mouse = Mouse.GetPosition(Timeline);

            //Position in grid calculated from mouse position and grid dimensions
            int timelineColumn = (int)((mouse.X - capturedX + columnWidth / 2) / columnWidth);
            int timelineRow = (int)((mouse.Y - capturedY + rowHeight / 2) / rowHeight);

            SetTimelineItemPosition(timelineRow, timelineColumn, capturedTimelineItem.getLayerObject().Length);
        }

        /// <summary>
        /// Timeline item resize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="columnWidth"></param>
        private void TimelineItemResize(object sender, double columnWidth)
        {
            Point mouse = Mouse.GetPosition(Timeline);

            int length;
            int column;
            int currentColumn = (int)(mouse.X / columnWidth);

            //if left panel is dragged else right panel is dragged
            if (capturedResizePanel.HorizontalAlignment.ToString() == "Left")
            {
                column = currentColumn;
                length = capturedTimelineItemLength - column + capturedTimelineItemColumn;
            }
            else
            {
                column = capturedTimelineItemColumn;
                length = currentColumn - column + 1; //index start at 0 - length + 1
            }

            SetTimelineItemPosition(capturedTimelineItem.getLayerObject().Layer, column, length);
        }

        /// <summary>
        /// Set timeline item position in grid
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="length"></param>
        private void SetTimelineItemPosition(int row, int column, int length)
        {
            bool overlap = TimelineItemOverlap(column, row, length);
            int endColumn = column + length - 1;

            //if the whole element is in grid and doesn't overlaps another item
            if (column >= 0 && endColumn < ProjectHolder.ImageCount && capturedTimelineItem.getLayerObject().Layer >= 0 && capturedTimelineItem.getLayerObject().Layer < ProjectHolder.LayerCount && !overlap)
            {
                capturedTimelineItem.SetPosition(row, column, length);
            }
        }

        /// <summary>
        /// Returns true if timeline item overlaps another
        /// </summary>
        /// <param name="timelineColumn"></param>
        /// <param name="timelineRow"></param>
        /// <param name="timelineLength"></param>
        /// <returns></returns>
        private bool TimelineItemOverlap(int timelineColumn, int timelineRow, int timelineLength)
        {
            foreach (TimelineItem item in timelineList)
            {
                //if its the same item
                if (item == capturedTimelineItem)
                {
                    continue;
                }

                for (int i = 0; i < timelineLength; i++)
                {
                    //overlaps antoher item
                    if (item.IsInPosition(timelineRow, timelineColumn + i))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Browser listener - Double click opens folder, click starts drag n drop action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                //Open folder
                Browser_DoubleClick(sender, e);
            }
            else
            {
                //drag browser item
                Browser_Click(sender, e);
            }
        }

        /// <summary>
        /// Browser drag handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_Click(object sender, MouseButtonEventArgs e)
        {
            ListBox parent = (ListBox)sender;
            //dragged data as browser item
            BrowserItem browserItem = (BrowserItem)GetObjectDataFromPoint(parent, e.GetPosition(parent));

            // dragged item has to be instance of browserItem
            if (browserItem == null)
                return;

            //drag drop event
            DragDrop.DoDragDrop(parent, browserItem, System.Windows.DragDropEffects.Move);
        }

        /// <summary>
        /// Browser drop handler - Creates new timeline item and adds to timeline
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timeline_DropHandler(object sender, DragEventArgs e)
        {
            //dropped data as browser item
            BrowserItem browserItem = (BrowserItem)e.Data.GetData(typeof(BrowserItem));

            //if is not image in acceptable format
            if (!browserItem.Image)
            {
                MessageBox.Show("Tento formát nelze načíst do projektu", "Nepodporovaný formát", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            Point mouse = e.GetPosition(Timeline);

            //actual column width
            double columnWidth = Timeline.ColumnDefinitions[0].ActualWidth;

            int column = (int)(mouse.X / columnWidth);
            int row = (int)(mouse.Y / rowHeight);

            if (!TimelineItemOverlap(column, row, 1))
            {
                //new item into column and row with length 1 and zero coordinates. Real position is set after mouse up event
                TimelineItem newItem = new TimelineItem(row, column, 1, browserItem.ToString());

                //add into list of items and set mouse listeners
                timelineList.Add(newItem);
                newItem.MouseDown += TimelineItem_MouseDown;
                newItem.leftResizePanel.MouseLeftButtonDown += TimelineResize_MouseLeftButtonDown;
                newItem.rightResizePanel.MouseLeftButtonDown += TimelineResize_MouseLeftButtonDown;
                newItem.delete.Click += TimelineDelete_Click;

                //add into timeline
                Timeline.Children.Add(newItem);
            }
        }

        /// <summary>
        /// Remove timeline item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimelineDelete_Click(object sender, RoutedEventArgs e)
        {
            //remove from list and from timeline
            timelineList.Remove(capturedTimelineItem);
            Timeline.Children.Remove(capturedTimelineItem);

            capturedTimelineItem = null;
        }

        /// <summary>
        /// Timeline mouse button up listener
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timeline_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            capturedTimelineItem = null;
            capturedResizePanel = null;
        }

        /// <summary>
        /// Saving button click event hook - invoke save dialog and proceed saving if confirmed and filled correctly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectHolder.ProjectFileName == null || ProjectHolder.ProjectFileName == "")
            {
                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.FileName = "projekt";
                dialog.DefaultExt = ".lcp";
                dialog.Filter = "Lenticulis projekt (.lcp)|*.lcp";

                Nullable<bool> res = dialog.ShowDialog();
                if (res == true)
                {
                    // save project to newly selected file target
                    ProjectSaver.saveProject(dialog.FileName);
                }
            }
            else
            {
                // use previously stored filename
                ProjectSaver.saveProject();
            }
        }

        /// <summary>
        /// Gets the object for the element selected in the listbox
        /// </summary>
        /// <param name="source"></param>
        /// <param name="point"></param>
        /// <returns></returns>
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
    }
}
