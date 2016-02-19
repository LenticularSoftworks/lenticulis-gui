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
                Grid.SetRow(depthBox, Timeline.RowDefinitions.Count - 1);
                LayerDepth.Children.Add(depthBox);

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
            {
                return;
            }

            foreach (TimelineItem item in timelineList)
            {
                LayerObject lo = item.getLayerObject();

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

            //remove from depth layer column
            LayerDepth.RowDefinitions.Remove(LayerDepth.RowDefinitions[Timeline.RowDefinitions.Count - 1]);

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

            item.getLayerObject().dispose();

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
        /// Time line item shift
        /// </summary>
        /// <param name="columnWidth">column width</param>
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
        /// <param name="row">row number</param>
        /// <param name="column">column number</param>
        /// <param name="length">image length (column span)</param>
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
            canvasList[newItem.getLayerObject().Column].Paint();
        }

        #endregion Timeline methods
    }
}
