using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using MahApps.Metro.Controls;
using lenticulis_gui.src.App;
using lenticulis_gui.src.Containers;
using lenticulis_gui.src.SupportLib;
using lenticulis_gui.src.Dialogs;
using System.Windows.Controls.Primitives;
using System.Diagnostics;

namespace lenticulis_gui
{
    public partial class MainWindow
    {
        #region 3D methods
        /// <summary>
        /// Gets layer dpeths from timeline and return them as array
        /// </summary>
        /// <param name="foreground">foreground</param>
        /// <param name="background">background</param>
        /// <returns>depths array</returns>
        private double[] GetDepthArray(double foreground, double background)
        {
            double[] depthArray = new double[ProjectHolder.LayerCount];
            double tmpDepth = Double.NegativeInfinity;

            for (int i = 0; i < ProjectHolder.LayerCount; i++)
            {
                TextBox tb = LayerDepth.Children[i] as TextBox;

                if (Double.TryParse(tb.Text, out tmpDepth))
                {
                    //must be between foreground and background
                    if (tmpDepth <= foreground && tmpDepth >= background)
                    {
                        depthArray[i] = tmpDepth;

                        Debug.WriteLine(tmpDepth);
                    }
                    else
                    {
                        Debug.WriteLine("depth out of bounds"); //TODO
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("depth parse err"); //TODO
                    return null;
                }
            }

            return depthArray;
        }
        #endregion 3D methods

        #region 3D listeners

        /// <summary>
        /// Action to generate shifts for 3D print
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Generate3D_Click(object sender, RoutedEventArgs e)
        {
            if (!ProjectHolder.ValidProject || timelineList.Count == 0)
                return;

            //Input values
            double viewDist = Convert.ToDouble(ViewDist3D.Text);
            double viewAngle = Convert.ToDouble(ViewAngle3D.Text);
            double foreground = Convert.ToDouble(Foreground3D.Text);
            double background = Convert.ToDouble(Background3D.Text);

            //get depths of layers
            double[] depthArray = GetDepthArray(foreground, background);

            if (depthArray == null)
                return;

            //set new positions
            Generator3D.Generate3D(viewDist, viewAngle, ProjectHolder.ImageCount, ProjectHolder.Width, ProjectHolder.Dpi, timelineList, depthArray);

            //repaint result
            RepaintCanvas();
        }

        /// <summary>
        /// Calculates frame spacing and enable 3D generate button if higher than 0.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewDist3D_Changed(object sender, TextChangedEventArgs e)
        {
            if (!ProjectHolder.ValidProject)
                return;

            if (ViewAngle3D.Text.Trim().Equals("") || ViewDist3D.Text.Trim().Equals(""))
            {
                FrameSpacing3D.Text = "";
                Generate3D.IsEnabled = false;

                return;
            }

            //initial
            int frameSpacing = 0;
            double distance = Double.NegativeInfinity;
            double angle = Double.NegativeInfinity;

            if (Double.TryParse(ViewAngle3D.Text, out angle) && Double.TryParse(ViewDist3D.Text, out distance))
            {
                //must be higher than 0
                if (distance > 0.0 && angle > 0.0)
                {
                    //show frame spacing
                    frameSpacing = Generator3D.CalculateZoneDistance(distance, angle, ProjectHolder.ImageCount);
                    FrameSpacing3D.Text = frameSpacing.ToString();
                }

                //enable / disable generate button
                if (frameSpacing >= 1)
                {
                    Generate3D.IsEnabled = true;
                }
                else
                {
                    Generate3D.IsEnabled = false;
                }
            }
        }

        #endregion 3D listeners

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
                layer = capturedTimelineItemContext.getLayerObject().Layer;
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
                layer = capturedTimelineItemContext.getLayerObject().Layer;
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
            if (ProjectHolder.layers[capturedTimelineItemContext.getLayerObject().Layer].getLayerObjects().Count > 1)
            {
                capturedTimelineItemContext = null;
                MessageBox.Show(LangProvider.getString("ITEM_CANNOT_BE_SPREAD_CONFLICT"), LangProvider.getString("ITEM_CANNOT_BE_SPREAD_CONFLICT_TITLE"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            capturedTimelineItemContext.SetPosition(capturedTimelineItemContext.getLayerObject().Layer, 0, ProjectHolder.ImageCount);
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
                if (capturedTimelineItem.getLayerObject().Length == 1)
                    capturedTimelineItem.getLayerObject().resetTransformations();
            }

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
