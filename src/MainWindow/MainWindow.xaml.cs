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
using lenticulis_gui.src.Dialogs;

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

        //canvas list
        private List<WorkCanvas> canvasList;

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

            timelineList = new List<TimelineItem>();
        }

        /// <summary>
        /// Set image and layer count
        /// </summary>
        /// <param name="imageCount"></param>
        /// <param name="layerCount"></param>
        public void SetProjectProperties(int imageCount, int layerCount)
        {
            SetImageCount(imageCount);
            AddTimelineHeader();
            AddTimelineLayer(layerCount);

            canvasList = new List<WorkCanvas>();
            SetWorkCanvasList();
            ShowSingleCanvas(0);
        }

        /// <summary>
        /// Create Canvas for each image
        /// </summary>
        private void SetWorkCanvasList()
        {
            canvasList.Clear();

            for (int i = 0; i < ProjectHolder.ImageCount; i++)
            {
                canvasList.Add(new WorkCanvas(i));
            }
        }

        /// <summary>
        /// Show single canvas
        /// </summary>
        /// <param name="imageID"></param>
        private void ShowSingleCanvas(int imageID)
        {
            ScrollViewer canvas = GetCanvas(imageID);

            if (canvas != null)
            {
                CanvasPanel.ColumnDefinitions.Clear();
                CanvasPanel.Children.Clear();

                CanvasPanel.Children.Add(canvas);
            }
        }

        /// <summary>
        /// Splits canvas in two 
        /// </summary>
        /// <param name="firstImageID"></param>
        /// <param name="secondImageID"></param>
        private void ShowDoubleCanvas(int firstImageID, int secondImageID)
        {
            ScrollViewer leftCanvas = GetCanvas(firstImageID);
            ScrollViewer rightCanvas = GetCanvas(secondImageID);

            if (leftCanvas != null && rightCanvas != null)
            {
                CanvasPanel.ColumnDefinitions.Clear();
                CanvasPanel.Children.Clear();

                CanvasPanel.ColumnDefinitions.Add(new ColumnDefinition());
                CanvasPanel.ColumnDefinitions.Add(new ColumnDefinition());

                Grid.SetColumn(leftCanvas, 0);
                Grid.SetColumn(rightCanvas, 1);

                CanvasPanel.Children.Add(leftCanvas);
                CanvasPanel.Children.Add(rightCanvas);
            }
        }

        /// <summary>
        /// Get cavnas by image ID and return with scrollbar
        /// </summary>
        /// <param name="imageID"></param>
        /// <returns></returns>
        private ScrollViewer GetCanvas(int imageID)
        {
            if (imageID < 0 || imageID >= ProjectHolder.ImageCount)
            {
                return null;
            }

            ScrollViewer scViewer = new ScrollViewer();
            scViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            WorkCanvas canvas = canvasList[imageID];

            //TODO: redraw canvas canvas.Draw();

            scViewer.Content = canvas;

            return scViewer;
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
        /// <param name="browserItem"></param>
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
                    MessageBox.Show(ex.Message, "Nelze otevřít", MessageBoxButton.OK, MessageBoxImage.Warning);
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
        /// Method for loading and putting element on canvas / to timeline to implicit position
        /// </summary>
        /// <param name="path">Path to image file</param>
        /// <param name="extension">Extension (often obtained via file browser)</param>
        /// <returns>True if everything succeeded</returns>
        private bool LoadAndPutResource(String path, String extension, int layer, int frame)
        {
            if (!Utils.IsAcceptedImageExtension(extension))
                return false;

            // Create new loading window
            LoadingWindow lw = new LoadingWindow();
            // show it
            lw.Show();
            // and disable this window to disallow all operations
            this.IsEnabled = false;
            // TODO for far future: use asynchronnous loading thread, to be able to cancel loading

            // load image...
            ImageHolder ih = ImageHolder.loadImage(path);

            // after image was loaded, enable main window
            this.IsEnabled = true;
            // and close loading window
            lw.Close();

            if (ih == null)
                return false;

            // TODO: main canvas logic - put item onto canvas here

            // The following lines of code are only to show, how to manage image displaying on WPF canvas

            // now the image is ready for putting onto canvas
            // retrieve desired image as ImageSource, using dimensions we want to use (initially it would be just
            // image original dimensions, i think, so use ih.width and ih.height)

            //ImageSource isrc = ih.getImageForSize(100, 80);

            // Assign ImageSource as source of Image control on canvas
            // These are only important attributes needed to be set - source, width and height, and stretch set to fill

            //MyCanvasImage.Source = isrc;
            //MyCanvasImage.Width = 100;
            //MyCanvasImage.Height = 80;
            //MyCanvasImage.Stretch = System.Windows.Media.Stretch.Fill;

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

        public void UpdateImageCount(int newcount)
        {
            int current = ProjectHolder.ImageCount;

            // adding specific count of frames
            if (newcount > current)
            {
                for (int i = 0; i < newcount-current; i++)
                {
                    ColumnDefinition colDef = new ColumnDefinition();
                    colDef.MinWidth = columnMinWidth;
                    Timeline.ColumnDefinitions.Add(colDef);

                    colDef = new ColumnDefinition();
                    colDef.MinWidth = columnMinWidth;
                    TimelineHeader.ColumnDefinitions.Add(colDef);
                }
            }
            // removing some frames
            else if (newcount < current)
            {
                // delete N last elements
                for (int i = current - 1; i > newcount - 1; i++)
                {
                    Timeline.ColumnDefinitions.Remove(Timeline.ColumnDefinitions[i]);
                    TimelineHeader.ColumnDefinitions.Remove(TimelineHeader.ColumnDefinitions[i]);
                }

                // TODO: check for existing layer objects in there columns !
            }

            ProjectHolder.ImageCount = newcount;
        }

        /// <summary>
        /// Add timeline layer
        /// </summary>
        /// <param name="count">layer count</param>
        private void AddTimelineLayer(int count)
        {
            if (ProjectHolder.ImageCount == 0)
            {
                return;
            }

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
            if (ProjectHolder.LayerCount == 1 || ProjectHolder.ImageCount == 0)
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
                RemoveTimelineItem(item);
            }

            //remove from Timeline
            Timeline.RowDefinitions.Remove(Timeline.RowDefinitions[Timeline.RowDefinitions.Count - 1]);
            
            //set ProjectHolder
            ProjectHolder.layers.RemoveAt(ProjectHolder.LayerCount - 1);
            ProjectHolder.LayerCount--;
        }

        /// <summary>
        /// Clears timeline
        /// </summary>
        public void ClearTimeline()
        {
            TimelineItem[] toDelete = timelineList.ToArray();

            // Remove each timeline item
            for (int i = 0; i < toDelete.Length; i++)
                RemoveTimelineItem(toDelete[i]);

            // Remove all rows except the first one
            for (int i = Timeline.RowDefinitions.Count - 1; i > 0; i--)
                Timeline.RowDefinitions.Remove(Timeline.RowDefinitions[i]);

            ProjectHolder.layers.Clear();
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
            if (Timeline.ColumnDefinitions.Count == 0)
            {
                return;
            }

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
            ListBox parent = (ListBox)sender;
            //dragged data as browser item
            BrowserItem browserItem = (BrowserItem)GetObjectDataFromPoint(parent, e.GetPosition(parent));

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
        /// <param name="browserItem"></param>
        /// <param name="parent"></param>
        private void Browser_Click(BrowserItem browserItem, ListBox parent)
        {
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
                // load resource and put it into internal structures
                bool result = LoadAndPutResource(browserItem.Path + "\\" + browserItem.Name, browserItem.Extension, row, column);

                if (result)
                {
                    // check for presence in last used list
                    bool found = false;
                    foreach (BrowserItem bi in LastUsedList.Items)
                    {
                        if (bi.Path.Equals(browserItem.Path))
                        {
                            found = true;
                            break;
                        }
                    }

                    // if not yet there, add it
                    if (!found)
                        LastUsedList.Items.Add(new BrowserItem(browserItem.Name, browserItem.Path, browserItem.Extension, false));

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
        }

        /// <summary>
        /// Remove timeline item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimelineDelete_Click(object sender, RoutedEventArgs e)
        {
            //remove from list and from timeline
            RemoveTimelineItem(capturedTimelineItem);

            capturedTimelineItem = null;
        }

        /// <summary>
        /// Removes item from timeline, and properly disposes its references
        /// </summary>
        /// <param name="item"></param>
        private void RemoveTimelineItem(TimelineItem item)
        {
            timelineList.Remove(item);
            Timeline.Children.Remove(item);

            item.getLayerObject().dispose();
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

        /// <summary>
        /// Creates new project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewProjectButton_Click(object sender, RoutedEventArgs e)
        {
            ProjectPropertiesWindow ppw = new ProjectPropertiesWindow();
            ppw.ShowDialog();
        }

        /// <summary>
        /// Change canvas view event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoubleCanvas_Checked(object sender, RoutedEventArgs e)
        {
            ShowDoubleCanvas(0, ProjectHolder.ImageCount - 1);
        }

        /// <summary>
        /// Change canvas view event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SingleCanvas_Checked(object sender, RoutedEventArgs e)
        {
            ShowSingleCanvas(0);
        }

        
    }
}
