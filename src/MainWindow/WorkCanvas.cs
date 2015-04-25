using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using lenticulis_gui.src.App;
using System.Windows;
using System.Drawing;
using System.Windows.Media;
using lenticulis_gui.src.Containers;
using System.Windows.Shapes;

namespace lenticulis_gui
{
    /// <summary>
    /// Class represents single Canvas
    /// </summary>
    class WorkCanvas : Canvas
    {
        /// <summary>
        /// Number of image represented by canvas
        /// </summary>
        public int imageID { get; set; }

        private double canvasScaleCached = 1.0;

        public WorkCanvas(int imageID)
            : base()
        {
            this.imageID = imageID;

            this.Width = ProjectHolder.Width;
            this.Height = ProjectHolder.Height;
            this.Margin = new Thickness(10, 10, 10, 10);
            this.Background = new SolidColorBrush(Colors.White);

            this.RenderTransform = new ScaleTransform(canvasScaleCached, canvasScaleCached);

            Paint();
        }

        public double CanvasScale
        {
            get
            {
                return canvasScaleCached;
            }
            set
            {
                canvasScaleCached = value;
                this.RenderTransform = new ScaleTransform(canvasScaleCached, canvasScaleCached);
            }
        }

        /// <summary>
        /// Paint imagec on canvas
        /// </summary>
        public void Paint()
        {
            this.Children.Clear();

            //list of images sorted by layer
            List<LayerObject> images = GetImages();

            foreach (LayerObject lo in images)
            {
                ImageHolder imageHolder = Storage.Instance.getImageHolder(lo.Id);
                ImageSource source = imageHolder.getImageForSize(ProjectHolder.Width, ProjectHolder.Height);

                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                image.Source = source;

                Canvas.SetTop(image, lo.InitialX);
                Canvas.SetLeft(image, lo.InitialY);

                this.Children.Add(image);
            }

            //add border of canvas
            createBorder();
        }


        /// <summary>
        /// Returns visible images on canvas
        /// </summary>
        /// <returns></returns>
        private List<LayerObject> GetImages()
        {
            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;
            if (mw == null)
                return null;

            List<LayerObject> layerObjects = new List<LayerObject>();

            //fill layer objects in current canvas
            foreach (TimelineItem item in mw.timelineList)
            {
                if (item.IsInColumn(this.imageID) && item.getLayerObject().Visible)
                {
                    layerObjects.Add(item.getLayerObject());
                }
            }

            //sort layer objects
            layerObjects.Sort(delegate(LayerObject x, LayerObject y)
            {
                return y.Layer.CompareTo(x.Layer);
            });

            return layerObjects;
        }

        /// <summary>
        /// Add border on canvas
        /// </summary>
        private void createBorder()
        {
            System.Windows.Shapes.Rectangle border = new System.Windows.Shapes.Rectangle()
            {
                Fill = System.Windows.Media.Brushes.Transparent,
                Stroke = System.Windows.Media.Brushes.DarkOrange,
                StrokeDashArray = new DoubleCollection() { 2 },
                Width = this.Width,
                Height = this.Height,
            };

            this.Children.Add(border);
        }
    }
}

