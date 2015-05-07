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

        //context menu items
        public MenuItem delete, spreadMenuItem, transformMenuItem;

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
            this.Background = new LinearGradientBrush(Color.FromRgb(0x1E, 0x90, 0xFF), Color.FromRgb(0x87, 0xCD, 0xFA), 90.0);
            this.Margin = new Thickness(0, 0, 0, 1);

            //Add label
            string labelContent = this.Text;
            if (this.Text.Length > 15)
            {
                labelContent = this.Text.Substring(0, 14) + "...";
            }

            System.Windows.Controls.Label label = new System.Windows.Controls.Label() { Content = labelContent };
            label.Margin = new Thickness(8, 2, 0, 0);
            this.Children.Add(label);

            //Alignment
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;

            AddResizePanel();
            AddVisibilityButton();
            AddContextMenu();
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
            btn.Click += new RoutedEventHandler(delegate(object o, RoutedEventArgs args)
            {
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

                RepaintChange();
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
            rightResizePanel.Background = Brushes.SteelBlue;
            rightResizePanel.Cursor = System.Windows.Input.Cursors.SizeWE;

            leftResizePanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            leftResizePanel.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            leftResizePanel.Width = sizeChangePanelWidth;
            leftResizePanel.Background = Brushes.SteelBlue;
            leftResizePanel.Cursor = System.Windows.Input.Cursors.SizeWE;

            this.Children.Add(rightResizePanel);
            this.Children.Add(leftResizePanel);
        }

        /// <summary>
        /// Add context menu to item
        /// </summary>
        private void AddContextMenu()
        {
            ContextMenu cMenu = new ContextMenu();

            //set public menu item
            delete = new MenuItem()
            {
                Header = LangProvider.getString("REMOVE_TIMELINE_ITEM"),
                Icon = new Image()
                {
                    Source = Utils.iconResourceToImageSource("Erase")
                }
            };

            spreadMenuItem = new MenuItem()
            {
                Header = LangProvider.getString("SPREAD_TIMELINE_ITEM")
            };

            transformMenuItem = new MenuItem()
            {
                Header = LangProvider.getString("TRANSFORMATIONS_TIMELINE_ITEM")
            };

            cMenu.Items.Add(delete);
            cMenu.Items.Add(spreadMenuItem);
            cMenu.Items.Add(transformMenuItem);

            this.ContextMenu = cMenu;
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

                SetGridSettings();

                RepaintChange();
            }
        }

        /// <summary>
        /// Repaint canvases after change
        /// </summary>
        private void RepaintChange()
        {
            //main window object
            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;
            if (mw == null)
                return;

            //repaint canvas
            mw.RepaintCanvas();
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
        /// True if timelien item is in column
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool IsInColumn(int column)
        {
            if (column >= this.dataObject.Column && column < (this.dataObject.Column + this.dataObject.Length))
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

