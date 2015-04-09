using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using lenticulis_gui.src.App;
using lenticulis_gui.src.Containers;

namespace lenticulis_gui
{
    /// <summary>
    /// Timeline item
    /// </summary>
    public class TimelineItem : Grid
    {
        public string Text { get; set; }

        //resize panels
        public WrapPanel rightResizePanel;
        public WrapPanel leftResizePanel;

        //size of resize panel
        private const int sizeChangePanelWidth = 5;

        // data storage class
        private LayerObject dataObject;

        public TimelineItem(int layer, int column, int length, string text)
            : base()
        {
            dataObject = new LayerObject();

            SetPosition(layer, column, length);
            this.Text = text;
            this.dataObject.Visible = true;

            //Color
            this.Background = Brushes.MediumTurquoise;

            //Add label
            System.Windows.Controls.Label label = new System.Windows.Controls.Label() { Content = this.Text };
            this.Children.Add(label);

            //Alignment
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;

            AddResizePanel();
            AddVisibilityButton();
        }

        /// <summary>
        /// Set grid settings
        /// </summary>
        private void SetGridSettings()
        {
            Grid.SetColumn(this, this.dataObject.Column);
            Grid.SetRow(this, this.dataObject.Layer);
            Grid.SetColumnSpan(this, this.dataObject.Length);
        }

        /// <summary>
        /// Add visibility button
        /// </summary>
        private void AddVisibilityButton()
        {
            ToggleButton btn = new ToggleButton()
            {
                Width = 20,
                Height = 20,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 2 * sizeChangePanelWidth, 0),
                Content = new Image()
                {
                    Source = Utils.iconResourceToImageSource("Eye")
                }
            };

            // hook click event - toggle visibility state
            btn.Click += new RoutedEventHandler(delegate(object o, RoutedEventArgs args) {
                ToggleButton source = (ToggleButton)o;
                Image content = (Image)source.Content;
                LayerObject lobj = ((TimelineItem)source.Parent).getLayerObject();

                if (lobj.Visible)
                {
                    lobj.Visible = false;
                    content.Source = Utils.iconResourceToImageSource("EyeStrikeOut");
                }
                else
                {
                    lobj.Visible = true;
                    content.Source = Utils.iconResourceToImageSource("Eye");
                }
            });

            this.Children.Add(btn);
        }

        /// <summary>
        /// Add resize panels to timelien item
        /// </summary>
        private void AddResizePanel()
        {
            rightResizePanel = new WrapPanel();
            leftResizePanel = new WrapPanel();

            rightResizePanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            rightResizePanel.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            rightResizePanel.Width = sizeChangePanelWidth;
            rightResizePanel.Background = Brushes.DodgerBlue;
            rightResizePanel.Cursor = System.Windows.Input.Cursors.SizeWE;

            leftResizePanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            leftResizePanel.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            leftResizePanel.Width = sizeChangePanelWidth;
            leftResizePanel.Background = Brushes.DodgerBlue;
            leftResizePanel.Cursor = System.Windows.Input.Cursors.SizeWE;

            this.Children.Add(rightResizePanel);
            this.Children.Add(leftResizePanel);
        }

        /// <summary>
        /// Set position of timeline item in grid
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="column"></param>
        /// <param name="length"></param>
        public void SetPosition(int layer, int column, int length)
        {
            if (column >= 0 && layer >= 0 && length > 0)
            {
                this.dataObject.Layer = layer;
                this.dataObject.Column = column;
                this.dataObject.Length = length;
            }

            SetGridSettings();
        }

        /// <summary>
        /// True if timelien item is in position [row, column]
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool IsInPosition(int row, int column)
        {
            if (this.dataObject.Layer == row && column >= this.dataObject.Column && column < (this.dataObject.Column + this.dataObject.Length))
            {
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Retrieves data holder object
        /// </summary>
        /// <returns>data holder object</returns>
        public LayerObject getLayerObject()
        {
            return dataObject;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}

