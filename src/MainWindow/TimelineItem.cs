using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace lenticulis_gui
{
    /// <summary>
    /// Timeline item
    /// </summary>
    public class TimelineItem : Grid
    {
        public int Layer { get; set; }
        public int Column { get; set; }
        public int Length { get; set; }
        public string Text { get; set; }
        public bool Visible { get; set; }

        //resize panels
        public WrapPanel rightResizePanel;
        public WrapPanel leftResizePanel;

        //size of resize panel
        private const int sizeChangePanelWidth = 5;

        public TimelineItem(int layer, int column, int length, string text)
            : base()
        {
            SetPosition(layer, column, length);
            this.Text = text;
            this.Visible = true;

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
            Grid.SetColumn(this, this.Column);
            Grid.SetRow(this, this.Layer);
            Grid.SetColumnSpan(this, this.Length);
        }

        /// <summary>
        /// Add visibility button
        /// </summary>
        private void AddVisibilityButton()
        {
            Button btn = new Button()
            {
                Width = 20,
                Height = 20,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 2 * sizeChangePanelWidth, 0),
                Content = new Image()
                {
                    //Source = new BitmapImage(new Uri("pack://application:,,,/lenticulis-gui;component/res/icon/Eye.ico"))
                }
            };

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
                this.Layer = layer;
                this.Column = column;
                this.Length = length;
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
            if (this.Layer == row && column >= this.Column && column < (this.Column + this.Length))
            {
                return true;
            }
            else return false;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}

