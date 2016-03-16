using lenticulis_gui.src.App;
using lenticulis_gui.src.Containers;
using lenticulis_gui.src.Dialogs;
using lenticulis_gui.src.SupportLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace lenticulis_gui
{
    public partial class MainWindow
    {
        /// <summary>
        /// drag n drop captured items
        /// </summary>
        private TimelineItem capturedTimelineItem = null;
        private TimelineItem capturedTimelineItemContext = null;
        private WrapPanel capturedResizePanel = null;
        private TimelineItemHistory timelineHistory = null;

        /// <summary>
        /// drag n drop captured coords and dimensions
        /// </summary>
        private double capturedX;
        private double capturedY;
        private int capturedTimelineItemColumn;
        private int capturedTimelineItemLength;

        #region Timeline methods

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

        /// <summary>
        /// Updates image count in opened project
        /// </summary>
        /// <param name="newcount">new image count</param>
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

                for (int i = timelineList.Count - 1; i >= 0; i--)
                {
                    TimelineItem item = timelineList[i];
                    LayerObject lobj = item.GetLayerObject();
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
                    TimelineHeader.Children.Remove(TimelineHeader.Children[i]);
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
                return;

            for (int i = 0; i < count; i++)
            {
                //timeline row def.
                RowDefinition rowDef = new RowDefinition();
                rowDef.Height = new GridLength(rowHeight, GridUnitType.Pixel);

                //layer row def
                RowDefinition depthRowDef = new RowDefinition();
                depthRowDef.Height = new GridLength(rowHeight, GridUnitType.Pixel);

                //Add row definitions
                Timeline.RowDefinitions.Add(rowDef);
                LayerDepth.RowDefinitions.Add(depthRowDef);
                ProjectHolder.LayerCount++;

                //add textbox to layer depth column
                TextBox depthBox = new TextBox();
                depthBox.Text = "0";
                depthBox.TextChanged += DepthBox_TextChanged;
                Grid.SetRow(depthBox, Timeline.RowDefinitions.Count - 1);
                LayerDepth.Children.Add(depthBox);

                ShiftDepthBox();

                //Create and add horizontal border
                Border horizontalBorder = new Border() { BorderBrush = Brushes.Gray };
                horizontalBorder.BorderThickness = new Thickness() { Bottom = 1 };

                Grid.SetRow(horizontalBorder, Timeline.RowDefinitions.Count - 1);
                Grid.SetColumnSpan(horizontalBorder, ProjectHolder.ImageCount);

                Timeline.Children.Add(horizontalBorder);

                //increment id of existing layers
                IncrementLayerId();

                // create layer object and put it into layer list in project holder class
                Layer layer = new Layer(0);
                ProjectHolder.layers.Insert(0, layer);
            }

            SetTimelineVerticalLines();

            //Layer has been set. Refresh existing timeline items with new properties
            RefreshTimelineItemPosition();
        }

        /// <summary>
        /// Shifts layer depth text box contents 
        /// </summary>
        private void ShiftDepthBox()
        {
            for (int i = LayerDepth.Children.Count - 1; i > 0; i--)
            {
                string text = (LayerDepth.Children[i - 1] as TextBox).Text;
                TextBox tb = LayerDepth.Children[i] as TextBox;

                //same value as highest layer
                if (!text.Trim().Equals(""))
                    tb.Text = text;
                else
                    tb.Text = "0";
            }
        }

        /// <summary>
        /// Increments id in all existing layers.
        /// </summary>
        private void IncrementLayerId()
        {
            foreach (Layer l in ProjectHolder.layers)
            {
                l.incrementLayerId();
            }
        }

        /// <summary>
        /// Refresh timeline items position.
        /// </summary>
        private void RefreshTimelineItemPosition()
        {
            // return if timeline list is not set
            if (timelineList == null)
                return;

            foreach (TimelineItem item in timelineList)
            {
                LayerObject lo = item.GetLayerObject();

                //refresh position with new layer object properties
                item.SetPosition(lo.Layer, lo.Column, lo.Length);
            }
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
            for (int i = 0; i < ProjectHolder.LayerCount; i++)
            {
                Border horizontalBorder = new Border() { BorderBrush = Brushes.Gray };
                horizontalBorder.BorderThickness = new Thickness() { Bottom = 1 };

                Grid.SetRow(horizontalBorder, i);
                Grid.SetColumnSpan(horizontalBorder, ProjectHolder.ImageCount);

                Timeline.Children.Add(horizontalBorder);
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
                if (item.GetLayerObject().Layer == lastLayer)
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

            //remove from depth layer column
            LayerDepth.Children.RemoveAt(LayerDepth.RowDefinitions.Count - 1);
            LayerDepth.RowDefinitions.Remove(LayerDepth.RowDefinitions[LayerDepth.RowDefinitions.Count - 1]);

            //set ProjectHolder
            ProjectHolder.layers.RemoveAt(ProjectHolder.LayerCount - 1);
            ProjectHolder.LayerCount--;
        }

        /// <summary>
        /// Removes item from timeline, and properly disposes its references
        /// </summary>
        /// <param name="item"></param>
        private void RemoveTimelineItem(TimelineItem item)
        {
            timelineList.Remove(item);
            Timeline.Children.Remove(item);

            item.GetLayerObject().dispose();

            RepaintCanvas();
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
                    if (item.GetLayerObject().Layer + 1 > newcount)
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
            Timeline.Children.Clear();
            TimelineHeader.ColumnDefinitions.Clear();
            TimelineHeader.Children.Clear();
            LayerDepth.RowDefinitions.Clear();
            LayerDepth.Children.Clear();
            SliderPanel.Children.Clear();

            ProjectHolder.layers.Clear();
            ProjectHolder.LayerCount = 0;
        }

        /// <summary>
        /// Time line item shift
        /// </summary>
        /// <param name="columnWidth">column width</param>
        private void TimelineItemShift(double columnWidth)
        {
            Point mouse = Mouse.GetPosition(Timeline);

            //Position in grid calculated from mouse position and grid dimensions
            int timelineColumn = (int)((mouse.X - capturedX + columnWidth / 2) / columnWidth);
            int timelineRow = (int)((mouse.Y - capturedY + rowHeight / 2) / rowHeight);

            SetTimelineItemPosition(timelineRow, timelineColumn, capturedTimelineItem.GetLayerObject().Length);
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

            SetTimelineItemPosition(capturedTimelineItem.GetLayerObject().Layer, column, length);
        }

        /// <summary>
        /// Set timeline item position in grid
        /// </summary>
        /// <param name="row">row number</param>
        /// <param name="column">column number</param>
        /// <param name="length">image length (column span)</param>
        private void SetTimelineItemPosition(int row, int column, int length)
        {
            bool overlap = TimelineItemOverlap(column, row, length);
            int endColumn = column + length - 1;

            //if the whole element is in grid and doesn't overlaps another item
            if (column >= 0 && endColumn < ProjectHolder.ImageCount && capturedTimelineItem.GetLayerObject().Layer >= 0 && capturedTimelineItem.GetLayerObject().Layer < ProjectHolder.LayerCount && !overlap)
            {
                capturedTimelineItem.SetPosition(row, column, length);
            }
        }

        /// <summary>
        /// Returns true if timeline item overlaps another
        /// </summary>
        /// <param name="timelineColumn">column number</param>
        /// <param name="timelineRow">row (layer) number</param>
        /// <param name="timelineLength">length (column span)</param>
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
        /// Adds new item to timeline
        /// </summary>
        /// <param name="newItem">item to be added</param>
        public void AddTimelineItem(TimelineItem newItem)
        {
            timelineList.Add(newItem);
            newItem.MouseLeftButtonDown += TimelineItem_MouseLeftButtonDown;
            newItem.MouseRightButtonUp += TimelineItem_MouseRightButtonUp;
            newItem.leftResizePanel.MouseLeftButtonDown += TimelineResize_MouseLeftButtonDown;
            newItem.rightResizePanel.MouseLeftButtonDown += TimelineResize_MouseLeftButtonDown;
            newItem.deleteMenuItem.Click += TimelineDelete_Click;
            newItem.spreadMenuItem.Click += TimelineSpreadItem_Click;
            newItem.transformMenuItem.Click += TimelineTransformItem_Click;
            newItem.layerUp.Click += LayerUp_Click;
            newItem.layerDown.Click += LayerDown_Click;

            // add into timeline
            Timeline.Children.Add(newItem);

            //repaint canvas
            canvasList[newItem.GetLayerObject().Column].Paint();
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

        #endregion Timeline methods

        #region Timeline listeners
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
                if (item.GetLayerObject().Layer == lastLayer)
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
        /// Mouse drag n drop action listener
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimelineItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                TimelineItem_DoubleClick(sender, e);
            }
            else
            {
                capturedTimelineItem = (TimelineItem)sender;

                //create history action
                timelineHistory = capturedTimelineItem.GetTimeLineItemAction();

                Point mouse = Mouse.GetPosition((UIElement)sender);

                capturedX = mouse.X;
                capturedY = mouse.Y;
            }
        }

        /// <summary>
        /// Set captured item for timeline item context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimelineItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            capturedTimelineItemContext = (TimelineItem)sender;
        }

        /// <summary>
        /// Shows current layer object on canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimelineItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            TimelineItem item = (TimelineItem)sender;

            int lower = item.GetLayerObject().Column;
            int upper = lower + item.GetLayerObject().Length - 1;

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

            capturedTimelineItemColumn = capturedTimelineItem.GetLayerObject().Column;
            capturedTimelineItemLength = capturedTimelineItem.GetLayerObject().Length;
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
                bool result = LoadAndPutResource(browserItem.Path + (browserItem.Path[browserItem.Path.Length - 1] == '\\' ? "" : "\\") + browserItem.Name, browserItem.Extension, false, out resourceId);

                if (result)
                {
                    AddLastUsedItem(browserItem.Path, browserItem.Name, browserItem.Extension);

                    //new item into column and row with length 1 and zero coordinates. Real position is set after mouse up event
                    TimelineItem newItem = new TimelineItem(row, column, 1, browserItem.ToString());
                    newItem.GetLayerObject().ResourceId = resourceId;

                    AddTimelineItem(newItem);
                }
            }
            else
            {
                MessageBox.Show(LangProvider.getString("ITEM_OVERLAPS_MSG"), LangProvider.getString("ITEM_OVERLAPS"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Event for layer move down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayerDown_Click(object sender, RoutedEventArgs e)
        {
            int layer;

            //switch between click to empty column or timeline item
            if (capturedTimelineItemContext == null)
            {
                layer = layerContext;
            }
            else
            {
                layer = capturedTimelineItemContext.GetLayerObject().Layer;
            }

            //if it is last layer
            if (layer == ProjectHolder.LayerCount - 1)
            {
                return;
            }

            //change layer id (position)
            ProjectHolder.layers[layer].incrementLayerId();
            ProjectHolder.layers[layer + 1].decrementLayerId();

            Layer tmp = ProjectHolder.layers[layer + 1];
            ProjectHolder.layers.RemoveAt(layer + 1);
            ProjectHolder.layers.Insert(layer, tmp);

            RefreshTimelineItemPosition();
            RepaintCanvas();

            capturedTimelineItemContext = null;
        }

        /// <summary>
        /// Event for layer move up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayerUp_Click(object sender, RoutedEventArgs e)
        {
            int layer;

            //switch between click to empty column or timeline item
            if (capturedTimelineItemContext == null)
            {
                layer = layerContext;
            }
            else
            {
                layer = capturedTimelineItemContext.GetLayerObject().Layer;
            }

            //if it is first layer
            if (layer == 0)
            {
                return;
            }

            //change layer id (position)
            ProjectHolder.layers[layer].decrementLayerId();
            ProjectHolder.layers[layer - 1].incrementLayerId();

            Layer tmp = ProjectHolder.layers[layer];
            ProjectHolder.layers.RemoveAt(layer);
            ProjectHolder.layers.Insert(layer - 1, tmp);

            RefreshTimelineItemPosition();
            RepaintCanvas();

            capturedTimelineItemContext = null;
        }

        /// <summary>
        /// Sets position and length to spread item across the layer, if possible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimelineSpreadItem_Click(object sender, RoutedEventArgs e)
        {
            // if there's more than one object in layer, do not allow this
            if (ProjectHolder.layers[capturedTimelineItemContext.GetLayerObject().Layer].getLayerObjects().Count > 1)
            {
                capturedTimelineItemContext = null;
                MessageBox.Show(LangProvider.getString("ITEM_CANNOT_BE_SPREAD_CONFLICT"), LangProvider.getString("ITEM_CANNOT_BE_SPREAD_CONFLICT_TITLE"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            capturedTimelineItemContext.SetPosition(capturedTimelineItemContext.GetLayerObject().Layer, 0, ProjectHolder.ImageCount);
            capturedTimelineItemContext = null;
        }

        /// <summary>
        /// Sets position and length to spread item across the layer, if possible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimelineTransformItem_Click(object sender, RoutedEventArgs e)
        {
            TransformationsWindow twin = new TransformationsWindow(capturedTimelineItemContext);
            twin.ShowDialog();
            capturedTimelineItemContext = null;
        }

        /// <summary>
        /// Remove timeline item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimelineDelete_Click(object sender, RoutedEventArgs e)
        {
            //remove from list and from timeline
            RemoveTimelineItem(capturedTimelineItemContext);

            capturedTimelineItemContext = null;
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
                if (capturedTimelineItem.GetLayerObject().Length == 1)
                    capturedTimelineItem.GetLayerObject().resetTransformations();
            }

            //store for udno action
            timelineHistory.StoreAction();
            historyList.AddHistoryItem(timelineHistory);

            timelineHistory = null;
            capturedTimelineItem = null;
            capturedResizePanel = null;
        }

        /// <summary>
        /// Get layer by mouse event when right click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timeline_MouseRightButtonUp(object sender, MouseEventArgs e)
        {
            Point mouse = Mouse.GetPosition(Timeline);

            //Position in grid calculated from mouse position and grid dimensions
            layerContext = (int)(mouse.Y / rowHeight);
        }

        #endregion Timeline listeners
    }
}
