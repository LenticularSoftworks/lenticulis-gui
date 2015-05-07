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
using MahApps.Metro.Controls;
using lenticulis_gui.src.App;
using lenticulis_gui.src.Containers;
using lenticulis_gui.src.SupportLib;
using lenticulis_gui.src.Dialogs;
using System.Windows.Controls.Primitives;

namespace lenticulis_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
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

        public static TransformType SelectedTool = TransformType.Translation;

        public MainWindow()
        {
            InitializeComponent();

            GetDrives();

            // Get available languages
            Dictionary<String, String> availableLangs = LangProvider.GetAvailableLangs();
            // and fetch them to menu items as available options
            LangChooserItem.Items.Clear();
            foreach (KeyValuePair<String, String> kvp in availableLangs)
            {
                // create new menu item
                MenuItem mi = new MenuItem();
                mi.Header = kvp.Value;
                // assign click event to change all we want
                mi.Click += new RoutedEventHandler(delegate(object o, RoutedEventArgs args)
                {
                    // change language
                    LangProvider.UseLanguage(kvp.Key);
                    // and update all bindings
                    LangDataSource.UpdateDataSources();
                });
                LangChooserItem.Items.Add(mi);
            }
        }

        /// <summary>
        /// Set image and layer count
        /// </summary>
        /// <param name="imageCount"></param>
        /// <param name="layerCount"></param>
        public void SetProjectProperties(int imageCount, int layerCount)
        {
            Timeline.Children.Clear();
            SetImageCount(imageCount);
            AddTimelineHeader();
            AddTimelineLayer(layerCount);
            timelineList = new List<TimelineItem>();

            RefreshCanvasList();
        }

        /// <summary>
        /// Refreshes all stored canvases according to actual properties
        /// </summary>
        public void RefreshCanvasList(bool resetSlider = true)
        {
            canvasList = new List<WorkCanvas>();
            SetWorkCanvasList();
            if (resetSlider)
            {
                ShowSingleCanvas(0);
                SetSingleSlider(0);
            }
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

            GridSplitter gs = new GridSplitter();
            gs.BorderBrush = Brushes.DodgerBlue;
            gs.BorderThickness = new Thickness(1.0, 0, 0, 0);
            gs.Width = 1.0;

            if (leftCanvas != null && rightCanvas != null)
            {
                CanvasPanel.ColumnDefinitions.Clear();
                CanvasPanel.Children.Clear();

                CanvasPanel.ColumnDefinitions.Add(new ColumnDefinition());
                ColumnDefinition gs_coldef = new ColumnDefinition();
                gs_coldef.Width = new GridLength(1.0);
                CanvasPanel.ColumnDefinitions.Add(gs_coldef);
                CanvasPanel.ColumnDefinitions.Add(new ColumnDefinition());

                Grid.SetColumn(leftCanvas, 0);
                Grid.SetColumn(gs, 1);
                Grid.SetColumn(rightCanvas, 2);

                CanvasPanel.Children.Add(leftCanvas);
                CanvasPanel.Children.Add(gs);
                CanvasPanel.Children.Add(rightCanvas);
            }
        }

        /// <summary>
        /// Adds range slider under the canvas
        /// </summary>
        /// <param name="firstImageID"></param>
        /// <param name="secondImageID"></param>
        private void SetRangeSlider(int firstImageID, int secondImageID)
        {
            RangeSlider slider = new RangeSlider();

            slider.Margin = new Thickness() { Top = 5, Left = 5 };
            slider.TickFrequency = 1;
            slider.TickPlacement = TickPlacement.Both;
            slider.IsSnapToTickEnabled = true;
            slider.Minimum = 0;
            slider.Maximum = ProjectHolder.ImageCount - 1;
            slider.MinRangeWidth = 0;
            slider.LowerValue = firstImageID;
            slider.UpperValue = secondImageID;
            slider.RangeSelectionChanged += DoubleSlider_ValueChanged;

            SliderPanel.Children.Clear();
            SliderPanel.Children.Add(slider);
            SliderPanel.Margin = new Thickness() { Left = 43 + (Timeline.ActualWidth / Timeline.ColumnDefinitions.Count) / 2, Right = (Timeline.ActualWidth / Timeline.ColumnDefinitions.Count) / 2 };
        }

        /// <summary>
        /// Adds slider under the canvas
        /// </summary>
        /// <param name="imageID"></param>
        private void SetSingleSlider(int imageID)
        {
            Slider slider = new Slider();

            slider.Margin = new Thickness() { Top = 5, Left = 5 };
            slider.TickFrequency = 1;
            slider.TickPlacement = TickPlacement.Both;
            slider.IsSnapToTickEnabled = true;
            slider.Minimum = 0;
            slider.Maximum = ProjectHolder.ImageCount - 1;
            slider.Value = imageID;
            slider.ValueChanged += SingleSlider_ValueChanged;

            SliderPanel.Children.Clear();
            SliderPanel.Children.Add(slider);
            SliderPanel.Margin = new Thickness() { Left = 43 + (Timeline.ActualWidth / Timeline.ColumnDefinitions.Count) / 2, Right = (Timeline.ActualWidth / Timeline.ColumnDefinitions.Count) / 2 };
        }

        /// <summary>
        /// Slider value change event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SingleSlider_ValueChanged(object sender, RoutedEventArgs e)
        {
            ShowSingleCanvas((int)((Slider)sender).Value);
        }

        /// <summary>
        /// Range slider value change event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoubleSlider_ValueChanged(object sender, RoutedEventArgs e)
        {
            RangeSlider slider = sender as RangeSlider;

            ShowDoubleCanvas((int)slider.LowerValue, (int)slider.UpperValue);
        }

        /// <summary>
        /// Get cavnas by image ID and return with scrollbar
        /// </summary>
        /// <param name="imageID"></param>
        /// <returns></returns>
        public ScrollViewer GetCanvas(int imageID)
        {
            if (imageID < 0 || imageID >= ProjectHolder.ImageCount)
            {
                return null;
            }

            ScrollViewer scViewer = new ScrollViewer();
            scViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            WorkCanvas canvas = canvasList[imageID];
            scViewer.Content = canvas;

            canvas.Paint();

            return scViewer;
        }

        /// <summary>
        /// Retrieves canvas currently drawn on canvas panel
        /// </summary>
        /// <returns>current canvas element</returns>
        public ScrollViewer GetCurrentCanvas()
        {
            if (CanvasPanel.Children.Count == 0)
                return null;

            return CanvasPanel.Children[0] as ScrollViewer;
        }

        /// <summary>
        /// Repaint canvas when changed
        /// </summary>
        public void RepaintCanvas()
        {
            for (int i = 0; i < canvasList.Count; i++)
            {
                canvasList[i].Paint();
            }
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
        public bool LoadAndPutResource(String path, String extension, bool callback, out int resourceId)
        {
            resourceId = 0;

            if (!Utils.IsAcceptedImageExtension(extension))
                return false;

            // Create new loading window
            LoadingWindow lw = new LoadingWindow("image");
            // show it
            lw.Show();
            // and disable this window to disallow all operations
            this.IsEnabled = false;
            // TODO for far future: use asynchronnous loading thread, to be able to cancel loading

            // load image...

            int psdLayerIndex = -1;
            if (!callback && extension.ToLower().Equals(".psd"))
            {
                List<String> layers = ImageLoader.getLayerInfo(path);

                LayerSelectWindow lsw = new LayerSelectWindow(layers);
                lsw.ShowDialog();

                psdLayerIndex = lsw.selectedLayer;
            }

            ImageHolder ih = ImageHolder.loadImage(path, true, psdLayerIndex);

            // after image was loaded, enable main window
            this.IsEnabled = true;
            // and close loading window
            lw.Close();

            if (ih == null)
                return false;

            resourceId = ih.id;

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

            SliderPanel.Margin = new Thickness() { Left = 43 + (Timeline.ActualWidth / Timeline.ColumnDefinitions.Count) / 2, Right = (Timeline.ActualWidth / Timeline.ColumnDefinitions.Count) / 2 };
        }

        public void UpdateImageCount(int newcount)
        {
            int current = ProjectHolder.ImageCount;

            // adding specific count of frames
            if (newcount > current)
            {
                for (int i = 0; i < newcount - current; i++)
                {
                    ColumnDefinition colDef = new ColumnDefinition();
                    colDef.MinWidth = columnMinWidth;
                    Timeline.ColumnDefinitions.Add(colDef);

                    colDef = new ColumnDefinition();
                    colDef.MinWidth = columnMinWidth;
                    TimelineHeader.ColumnDefinitions.Add(colDef);

                    System.Windows.Controls.Label label = new System.Windows.Controls.Label()
                    {
                        Content = "#" + (current + i + 1),
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    Grid.SetColumn(label, current + i);

                    TimelineHeader.Children.Add(label);
                }
            }
            // removing some frames
            else if (newcount < current)
            {
                bool removeConfirmed = false;

                foreach (TimelineItem item in timelineList)
                {
                    LayerObject lobj = item.getLayerObject();
                    // if some item is in deleted images
                    if (lobj != null)
                    {
                        if (lobj.Column + lobj.Length - 1 >= newcount)
                        {
                            if (!removeConfirmed)
                            {
                                MessageBoxResult mres = MessageBox.Show(LangProvider.getString("PROP_ERR_IMAGES_IN_DELETED_FRAMES"), LangProvider.getString("PROP_ERR_IMAGES_IN_DELETED_FRAMES_TITLE"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                                if (mres == MessageBoxResult.Yes)
                                    removeConfirmed = true;
                                else
                                    return;
                            }

                            RemoveTimelineItem(item);
                        }
                    }
                }

                // delete N last elements
                for (int i = current - 1; i > newcount - 1; i--)
                {
                    Timeline.ColumnDefinitions.Remove(Timeline.ColumnDefinitions[i]);
                    TimelineHeader.ColumnDefinitions.Remove(TimelineHeader.ColumnDefinitions[i]);
                    TimelineHeader.Children.Remove(Timeline.Children[i]);
                }
            }

            ProjectHolder.ImageCount = newcount;

            SetTimelineVerticalLines();
            SetTimelineHorizontalLines();
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
                Border horizontalBorder = new Border() { BorderBrush = Brushes.Gray };
                horizontalBorder.BorderThickness = new Thickness() { Bottom = 1 };

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
            for (int i = Timeline.Children.Count - 1; i >= 0; i--)
            {
                Border el = Timeline.Children[i] as Border;
                // all vertical lines
                if (el != null && el.BorderThickness.Right > 0)
                    Timeline.Children.RemoveAt(i);
            }

            //Create and add vertical border
            for (int i = 0; i < ProjectHolder.ImageCount; i++)
            {
                Border verticalBorder = new Border() { BorderBrush = Brushes.Gray };
                verticalBorder.BorderThickness = new Thickness() { Right = 1 };

                Grid.SetColumn(verticalBorder, i);
                Grid.SetRowSpan(verticalBorder, ProjectHolder.LayerCount);

                Timeline.Children.Add(verticalBorder);
            }
        }

        /// <summary>
        /// Add vertical lines
        /// </summary>
        private void SetTimelineHorizontalLines()
        {
            for (int i = Timeline.Children.Count - 1; i >= 0; i--)
            {
                Border el = Timeline.Children[i] as Border;
                // all vertical lines
                if (el != null && el.BorderThickness.Bottom > 0)
                    Timeline.Children.RemoveAt(i);
            }

            //Create and add vertical border
            for (int i = 0; i < ProjectHolder.ImageCount; i++)
            {
                Border horizontalBorder = new Border() { BorderBrush = Brushes.Gray };
                horizontalBorder.BorderThickness = new Thickness() { Bottom = 1 };

                Grid.SetRow(horizontalBorder, i);
                Grid.SetColumnSpan(horizontalBorder, ProjectHolder.ImageCount);

                Timeline.Children.Add(horizontalBorder);
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
                    messageBoxResult = MessageBox.Show(LangProvider.getString("DEL_LAYER_CONFIRM_TEXT"), LangProvider.getString("DEL_LAYER_CONFIRM_TITLE"), MessageBoxButton.YesNo);

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
        /// Updates layer count
        /// </summary>
        /// <param name="newcount">new layer count</param>
        public void UpdateLayerCount(int newcount)
        {
            if (newcount == ProjectHolder.LayerCount)
                return;

            if (newcount > ProjectHolder.LayerCount)
            {
                AddTimelineLayer(newcount - ProjectHolder.LayerCount);
            }
            else
            {
                MessageBoxResult messageBoxResult = MessageBoxResult.Yes;

                foreach (TimelineItem item in timelineList)
                {
                    //if some item is in last layer
                    if (item.getLayerObject().Layer + 1 > newcount)
                    {
                        messageBoxResult = MessageBox.Show(LangProvider.getString("DEL_LAYER_CONFIRM_TEXT"), LangProvider.getString("DEL_LAYER_CONFIRM_TITLE"), MessageBoxButton.YesNo);

                        break;
                    }
                }

                // remove last layers
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    while (ProjectHolder.LayerCount > newcount)
                        RemoveLastLayer();
                }
            }
        }

        /// <summary>
        /// Clears timeline
        /// </summary>
        public void ClearTimeline()
        {
            Timeline.RowDefinitions.Clear();
            Timeline.ColumnDefinitions.Clear();
            TimelineHeader.ColumnDefinitions.Clear();
            TimelineHeader.Children.Clear();

            ProjectHolder.layers.Clear();
            ProjectHolder.LayerCount = 0;
        }

        /// <summary>
        /// Mouse drag n drop action listener
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimelineItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                TimelineItem_DoubleClick(sender, e);
            }
            else
            {
                capturedTimelineItem = (TimelineItem)sender;

                Point mouse = Mouse.GetPosition((UIElement)sender);

                capturedX = mouse.X;
                capturedY = mouse.Y;
            }
        }

        /// <summary>
        /// Shows current layer object on canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimelineItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            TimelineItem item = (TimelineItem)sender;

            int lower = item.getLayerObject().Column;
            int upper = lower + item.getLayerObject().Length - 1;

            if (lower != upper)
            {
                DoubleCanvas.IsChecked = true;
                SetRangeSlider(lower, upper);
                ShowDoubleCanvas(lower, upper);
            }
            else
            {
                SingleCanvas.IsChecked = true;
                SetSingleSlider(lower);
                ShowSingleCanvas(lower);
            }
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
                MessageBox.Show(LangProvider.getString("UNSUPPORTED_TYPE_MSG"), LangProvider.getString("UNSUPPORTED_TYPE"), MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            Point mouse = e.GetPosition(Timeline);

            if (Timeline.ColumnDefinitions.Count == 0)
            {
                MessageBox.Show(LangProvider.getString("NO_PROJECT_MSG"), LangProvider.getString("NO_PROJECT"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //actual column width
            double columnWidth = Timeline.ColumnDefinitions[0].ActualWidth;

            int column = (int)(mouse.X / columnWidth);
            int row = (int)(mouse.Y / rowHeight);

            if (!TimelineItemOverlap(column, row, 1))
            {
                int resourceId = 0;
                // load resource and put it into internal structures
                bool result = LoadAndPutResource(browserItem.Path + (browserItem.Path[browserItem.Path.Length-1] == '\\' ? "" : "\\") + browserItem.Name, browserItem.Extension, false, out resourceId);

                if (result)
                {
                    AddLastUsedItem(browserItem.Path, browserItem.Name, browserItem.Extension);

                    //new item into column and row with length 1 and zero coordinates. Real position is set after mouse up event
                    TimelineItem newItem = new TimelineItem(row, column, 1, browserItem.ToString());
                    newItem.getLayerObject().ResourceId = resourceId;

                    AddTimelineItem(newItem);
                }
            }
            else
            {
                MessageBox.Show(LangProvider.getString("ITEM_OVERLAPS_MSG"), LangProvider.getString("ITEM_OVERLAPS"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Adds new item to timeline
        /// </summary>
        /// <param name="newItem">item to be added</param>
        public void AddTimelineItem(TimelineItem newItem)
        {
            timelineList.Add(newItem);
            newItem.MouseDown += TimelineItem_MouseDown;
            newItem.leftResizePanel.MouseLeftButtonDown += TimelineResize_MouseLeftButtonDown;
            newItem.rightResizePanel.MouseLeftButtonDown += TimelineResize_MouseLeftButtonDown;
            newItem.delete.Click += TimelineDelete_Click;
            newItem.spreadMenuItem.Click += TimelineSpreadItem_Click;
            newItem.transformMenuItem.Click += TimelineTransformItem_Click;

            // add into timeline
            Timeline.Children.Add(newItem);

            //repaint canvas
            canvasList[newItem.getLayerObject().Column].Paint();
        }

        /// <summary>
        /// Adds specified resource to last used items list, if it's not already there
        /// </summary>
        /// <param name="path">path to file</param>
        /// <param name="name">filename</param>
        /// <param name="extension">file extension</param>
        public void AddLastUsedItem(String path, String name, String extension)
        {
            String fullpath = path + ((path[path.Length-1] == '\\') ? "" : "\\") + name;

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
        /// Sets position and length to spread item across the layer, if possible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimelineSpreadItem_Click(object sender, RoutedEventArgs e)
        {
            // if there's more than one object in layer, do not allow this
            if (ProjectHolder.layers[capturedTimelineItem.getLayerObject().Layer].getLayerObjects().Count > 1)
            {
                capturedTimelineItem = null;
                MessageBox.Show(LangProvider.getString("ITEM_CANNOT_BE_SPREAD_CONFLICT"), LangProvider.getString("ITEM_CANNOT_BE_SPREAD_CONFLICT_TITLE"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            capturedTimelineItem.SetPosition(capturedTimelineItem.getLayerObject().Layer, 0, ProjectHolder.ImageCount);
            capturedTimelineItem = null;
        }

        /// <summary>
        /// Sets position and length to spread item across the layer, if possible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimelineTransformItem_Click(object sender, RoutedEventArgs e)
        {
            TransformationsWindow twin = new TransformationsWindow(capturedTimelineItem);
            twin.ShowDialog();
            capturedTimelineItem = null;
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

            RepaintCanvas();
        }

        /// <summary>
        /// Timeline mouse button up listener
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timeline_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (capturedTimelineItem != null && capturedResizePanel != null)
            {
                if (capturedTimelineItem.getLayerObject().Length == 1)
                    capturedTimelineItem.getLayerObject().resetTransformations();
            }

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
            SaveRoutine();
        }

        /// <summary>
        /// Save routine - asks for file, if needed
        /// </summary>
        /// <returns></returns>
        private bool SaveRoutine(bool saveAs = false)
        {
            if (!ProjectHolder.ValidProject)
                return false;

            if (saveAs || ProjectHolder.ProjectFileName == null || ProjectHolder.ProjectFileName == "")
            {
                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.FileName = "projekt";
                dialog.DefaultExt = ".lcp";
                dialog.Filter = LangProvider.getString("LENTICULIS_PROJECT_FILE")+" (.lcp)|*.lcp";

                Nullable<bool> res = dialog.ShowDialog();
                if (res == true)
                {
                    // save project to newly selected file target
                    ProjectSaver.saveProject(dialog.FileName);
                    return true;
                }
                return false;
            }
            else
            {
                // use previously stored filename
                ProjectSaver.saveProject();
                return true;
            }
        }

        /// <summary>
        /// Clicked on project loading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectHolder.ValidProject)
            {
                MessageBoxResult res = MessageBox.Show(LangProvider.getString("UNSAVED_WORK_CONFIRM_SAVE"), LangProvider.getString("UNSAVED_WORK"), MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                switch (res)
                {
                    // When clicked "Yes", offer saving, and if saving succeeds, proceed to load; otherwise do nothing
                    case MessageBoxResult.Yes:
                        if (!SaveRoutine())
                            return;
                        break;
                    // When clicked "No", discard any changes
                    case MessageBoxResult.No:
                        break;
                    // When clicked "Cancel", just do nothing
                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = LangProvider.getString("LENTICULIS_PROJECT_FILE")+" (.lcp)|*.lcp";

            Nullable<bool> dres = dialog.ShowDialog();
            if (dres == true)
            {
                // load project from selected file
                // Create new loading window
                LoadingWindow lw = new LoadingWindow("project");
                // show it
                lw.Show();
                // and disable this window to disallow all operations
                this.IsEnabled = false;

                ProjectLoader.loadProject(dialog.FileName);

                // after image was loaded, enable main window
                this.IsEnabled = true;
                // and close loading window
                lw.Close();
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
            if (ProjectHolder.ValidProject)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(LangProvider.getString("NEW_PROJECT_CONFIRM_TEXT"), LangProvider.getString("NEW_PROJECT_CONFIRM_TITLE"), MessageBoxButton.YesNo);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    SaveRoutine();
                    return;
                }
            }

            ProjectHolder.ValidProject = false;
            ProjectPropertiesWindow ppw = new ProjectPropertiesWindow();
            ppw.ShowDialog();
        }

        /// <summary>
        /// Opens dialog with project properties
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProjectPropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectHolder.ValidProject)
            {
                ProjectPropertiesWindow ppw = new ProjectPropertiesWindow();
                ppw.ShowDialog();
            }
        }

        /// <summary>
        /// Change canvas view event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoubleCanvas_Checked(object sender, RoutedEventArgs e)
        {
            // no project loaded / created
            if (!ProjectHolder.ValidProject)
                return;

            SetRangeSlider(0, ProjectHolder.ImageCount - 1);
            ShowDoubleCanvas(0, ProjectHolder.ImageCount - 1);
        }

        /// <summary>
        /// Change canvas view event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SingleCanvas_Checked(object sender, RoutedEventArgs e)
        {
            // no project loaded / created
            if (!ProjectHolder.ValidProject)
                return;

            SetSingleSlider(0);
            ShowSingleCanvas(0);
        }

        /// <summary>
        /// Change tool to translation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Translation_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.SelectedTool = TransformType.Translation;
        }

        /// <summary>
        /// Change tool to scale
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scale_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.SelectedTool = TransformType.Scale;
        }

        /// <summary>
        /// Change tool to rotate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rotate_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.SelectedTool = TransformType.Rotate;
        }

        /// <summary>
        /// Clicked on zoom in button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomInButton_Clicked(object sender, RoutedEventArgs e)
        {
            // no project loaded / created
            if (!ProjectHolder.ValidProject)
                return;

            foreach (WorkCanvas wc in canvasList)
            {
                if (wc.CanvasScale < 50.0)
                    wc.CanvasScale *= 1.2;
            }
        }

        /// <summary>
        /// Clicked on zoom out button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomOutButton_Clicked(object sender, RoutedEventArgs e)
        {
            // no project loaded / created
            if (!ProjectHolder.ValidProject)
                return;

            foreach (WorkCanvas wc in canvasList)
            {
                if (wc.CanvasScale > 0.1)
                    wc.CanvasScale /= 1.2;
            }
        }

        /// <summary>
        /// On resize event - window itself
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MetroWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // adjust slider size to match the timeline
            SliderPanel.Margin = new Thickness() { Left = 43 + (Timeline.ActualWidth / Timeline.ColumnDefinitions.Count) / 2, Right = (Timeline.ActualWidth / Timeline.ColumnDefinitions.Count) / 2 };
        }

        /// <summary>
        /// Clicked on export button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportButton_Clicked(object sender, RoutedEventArgs e)
        {
            // no project loaded / created
            if (!ProjectHolder.ValidProject)
                return;

            ExportWindow ew = new ExportWindow();
            ew.ShowDialog();
        }

        /// <summary>
        /// Clicked on "About" menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowAboutWindow_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aw = new AboutWindow();
            aw.ShowDialog();
        }

        /// <summary>
        /// Clicked on GitHub menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenGithub_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/LenticularSoftworks");
        }

        /// <summary>
        /// Clicked on licencing menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenLicence_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.gnu.org/copyleft/gpl.html");
        }

        /// <summary>
        /// Clicked on close menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseProgram_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Clicked "Undo" button (menu item or toolbar)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            //
        }

        /// <summary>
        /// Clicked "Redo" button (menu item or toolbar)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            //
        }

    }
}
