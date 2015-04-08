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
using lenticulis_gui.src.App;
using lenticulis_gui.src.Containers;

namespace lenticulis_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //List of acceptable file extensions
        public static string[] Extensions = { ".jpg", ".png", ".gif" };

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

            //test image and layer count
            int imageCount = 15;
            int layerCount = 5;

            SetImageCount(imageCount);
            AddTimelineHeader();
            AddTimelineLayer(layerCount);

            //test items
            timelineList = new List<TimelineItem>();
            timelineList.Add(new TimelineItem(0, 0, 1, "Obr1.jpg"));
            timelineList.Add(new TimelineItem(1, 1, 3, "Obr2.png"));
            timelineList.Add(new TimelineItem(2, 5, 1, "Obr3.gif"));
            SetPanels();
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
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
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
        /// Add resize panels to timeline item
        /// </summary>
        private void SetPanels()
        {
            foreach (TimelineItem item in timelineList)
            {
                item.MouseLeftButtonDown += TimelineItem_MouseLeftButtonDown;

                item.leftResizePanel.MouseLeftButtonDown += TimelineResize_MouseLeftButtonDown;
                item.rightResizePanel.MouseLeftButtonDown += TimelineResize_MouseLeftButtonDown;

                Timeline.Children.Add(item);
            }
        }

        /// <summary>
        /// Mouse drag n drop action listener
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimelineItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
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
        private void TimelineResize_MouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
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
        private void Timeline_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (capturedTimelineItem != null)
            {
                Point mouse = Mouse.GetPosition(Timeline);

                double columnWidth = Timeline.ColumnDefinitions[0].ActualWidth; //get actual column width

                if (capturedResizePanel == null)
                {
                    //Timeline item shift
                    TimelineItemShift(mouse, columnWidth);
                }
                else
                {
                    //Timeline item resize
                    TimelineItemResize(sender, mouse, columnWidth);
                }
            }
        }

        /// <summary>
        /// Time line item shift
        /// </summary>
        /// <param name="mouse"></param>
        /// <param name="columnWidth"></param>
        private void TimelineItemShift(Point mouse, double columnWidth)
        {
            int timelineColumn = (int)((mouse.X - capturedX + columnWidth / 2) / columnWidth);
            int timelineRow = (int)((mouse.Y - capturedY + rowHeight / 2) / rowHeight);

            setTimelineItemPosition(timelineRow, timelineColumn, capturedTimelineItem.getLayerObject().Length);
        }

        /// <summary>
        /// Timeline item resize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouse"></param>
        /// <param name="columnWidth"></param>
        private void TimelineItemResize(object sender, Point mouse, double columnWidth)
        {
            int length;
            int column;
            int currentColumn = (int)(mouse.X / columnWidth);

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

            setTimelineItemPosition(capturedTimelineItem.getLayerObject().Layer, column, length);
        }

        /// <summary>
        /// Set timeline item position in grid
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="length"></param>
        private void setTimelineItemPosition(int row, int column, int length)
        {
            bool overlap = TimelineItemOverlap(column, row, length);
            int endColumn = column + capturedTimelineItem.getLayerObject().Length - 1;

            //if the whole element is in grid and doesn't overlaps another item
            if (column >= 0 && endColumn < ProjectHolder.ImageCount && capturedTimelineItem.getLayerObject().Layer >= 0 && capturedTimelineItem.getLayerObject().Layer < ProjectHolder.LayerCount && !overlap)
            {
                capturedTimelineItem.SetPosition(row, column, length);
            }
        }

        /// <summary>
        /// True if timeline overlaps another
        /// </summary>
        /// <param name="timelineColumn"></param>
        /// <param name="timelineRow"></param>
        /// <param name="timelineLength"></param>
        /// <returns></returns>
        private bool TimelineItemOverlap(int timelineColumn, int timelineRow, int timelineLength)
        {
            foreach (TimelineItem item in timelineList)
            {
                if (item == capturedTimelineItem)
                {
                    continue;
                }

                for (int i = 0; i < timelineLength; i++)
                {
                    if (item.IsInPosition(timelineRow, timelineColumn + i))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Timeline mouse button up listener. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timeline_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (capturedTimelineItem != null)
            {
                capturedTimelineItem = null;
            }

            if (capturedResizePanel != null)
            {
                capturedResizePanel = null;
            }
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
    }
}
