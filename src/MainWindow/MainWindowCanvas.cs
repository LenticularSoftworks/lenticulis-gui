using lenticulis_gui.src.App;
using lenticulis_gui.src.Containers;
using lenticulis_gui.src.Dialogs;
using lenticulis_gui.src.SupportLib;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace lenticulis_gui
{
    public partial class MainWindow
    {
        #region Canvas methods

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
        /// <param name="imageID">image number</param>
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
        /// <param name="firstImageID">number of first (start) image</param>
        /// <param name="secondImageID">number of second(end) image</param>
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
        /// Get cavnas by image ID and return with scrollbar
        /// </summary>
        /// <param name="imageID">number of image</param>
        /// <returns>single scrollable canvas</returns>
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

        #endregion Canvas methods

        #region Slider methods
        /// <summary>
        /// Adds range slider under the canvas
        /// </summary>
        /// <param name="firstImageID">number of first (start) image</param>
        /// <param name="secondImageID">number of second(end) image</param>
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
        /// <param name="imageID">number of image</param>
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

        #endregion Slider methods
    }
}
