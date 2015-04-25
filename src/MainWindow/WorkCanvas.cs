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
using System.Windows.Input;

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

        //drag n drop values
        bool captured = false;
        float y_image, x_canvas, x_image, y_canvas;

        private double canvasScaleCached = 1.0;
        public double CanvasScale
        {
            get
            {
                return canvasScaleCached;
            }
            set
            {
                canvasScaleCached = value;
                this.LayoutTransform = new ScaleTransform(canvasScaleCached, canvasScaleCached);
            }
        }

        public WorkCanvas(int imageID)
            : base()
        {
            this.imageID = imageID;

            this.Width = ProjectHolder.Width;
            this.Height = ProjectHolder.Height;
            this.Margin = new Thickness(10, 10, 10, 10);
            this.Background = new SolidColorBrush(Colors.White);

            this.LayoutTransform = new ScaleTransform(canvasScaleCached, canvasScaleCached);

            Paint();
        }

        /// <summary>
        /// Drag image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UIElement source = sender as UIElement;
            Mouse.Capture(source);
            captured = true;
            y_image = (float)Canvas.GetLeft(source);
            x_canvas = (float)e.GetPosition(this).X;
            x_image = (float)Canvas.GetTop(source);
            y_canvas = (float)e.GetPosition(this).Y;
        }

        /// <summary>
        /// Move image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (captured)
            {
                UIElement source = sender as UIElement;

                double x = e.GetPosition(this).X;
                double y = e.GetPosition(this).Y;
                y_image += (float)(x - x_canvas);
                Canvas.SetLeft(source, y_image);
                x_canvas = (float)x;
                x_image += (float)(y - y_canvas);
                Canvas.SetTop(source, x_image);
                y_canvas = (float)y;
            }
        }

        /// <summary>
        /// Drop image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            SetLayerObjectProperties((UIElement) sender);
            captured = false;
        }

        /// <summary>
        /// Set layer object properties after drop
        /// </summary>
        /// <param name="source"></param>
        private void SetLayerObjectProperties(UIElement source)
        {
            System.Windows.Controls.Image droppedImage = source as System.Windows.Controls.Image;

            LayerObject lo = GetLayerObjectByImage(droppedImage);

            if (imageID == lo.Column)
            {
                // if there's some transformation present, preserve destination location by moving its vector
                Transformation tr = lo.getTransformation(TransformType.Translation);
                if (tr != null && lo.Length > 1)
                {
                    tr.setVector(tr.TransformX - (x_image - lo.InitialX),
                                 tr.TransformY - (y_image - lo.InitialY));
                }

                lo.InitialX = x_image;
                lo.InitialY = y_image;
            }
            else
            {
                float progress = (float)(imageID - lo.Column) / (float)(lo.Length - 1);

                float transX = Interpolator.interpolateLinearValue(InterpolationType.Linear, progress, lo.InitialX, x_image) - lo.InitialX;
                float transY = Interpolator.interpolateLinearValue(InterpolationType.Linear, progress, lo.InitialY, y_image) - lo.InitialY;
                lo.setTransformation(new Transformation(TransformType.Translation, transX, transY, 0));
            }
        }

        /// <summary>
        /// Return layer object by Image
        /// </summary>
        /// <param name="droppedImage"></param>
        /// <returns></returns>
        private LayerObject GetLayerObjectByImage(System.Windows.Controls.Image droppedImage)
        {
            List<LayerObject> layerObjects = GetImages();
            int index = 0;

            for (int i = 0; i < this.Children.Count; i++)
            {
                if (this.Children[i].GetType() == typeof(System.Windows.Controls.Image))
                {
                    if (this.Children[i] == droppedImage)
                    {
                        return layerObjects[index];
                    }

                    else
                    {
                        index++;
                    }
                }
            }
            return null;
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
                ImageHolder imageHolder = Storage.Instance.getImageHolder(lo.ResourceId);
                ImageSource source = imageHolder.getImageForSize(ProjectHolder.Width, ProjectHolder.Height);

                System.Windows.Controls.Image image = SetImage(source, lo);

                image.Width = imageHolder.width;
                image.Height = imageHolder.height;
                image.Stretch = Stretch.Fill;

                this.Children.Add(image);
            }

            //add border of canvas
            CreateBorder();
        }

        /// <summary>
        /// Set image properties
        /// </summary>
        /// <param name="source"></param>
        /// <param name="lo"></param>
        /// <returns></returns>
        private System.Windows.Controls.Image SetImage(ImageSource source, LayerObject lo)
        {
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();

            //source 
            image.Source = source;

            //listeners
            image.MouseLeftButtonDown += Image_MouseLeftButtonDown;
            image.MouseMove += Image_MouseMove;
            image.MouseLeftButtonUp += Image_MouseLeftButtonUp;

            //transform
            InterpolationType interType = InterpolationType.Linear;
            float progress = 0.0f;

            if (lo.Length > 1)
                progress = (imageID - lo.Column) / (float)(lo.Length - 1);

            float angle = Interpolator.interpolateLinearValue(interType, progress, lo.InitialAngle, lo.InitialAngle + lo.getTransformation(TransformType.Rotate).TransformAngle);
            float positionX = Interpolator.interpolateLinearValue(interType, progress, lo.InitialX, lo.InitialX + lo.getTransformation(TransformType.Translation).TransformX);
            float positionY = Interpolator.interpolateLinearValue(interType, progress, lo.InitialY, lo.InitialY + lo.getTransformation(TransformType.Translation).TransformY);
            float scaleX = Interpolator.interpolateLinearValue(interType, progress, lo.InitialScaleX, lo.getTransformation(TransformType.Scale).TransformX);
            float scaleY = Interpolator.interpolateLinearValue(interType, progress, lo.InitialScaleY, lo.getTransformation(TransformType.Scale).TransformY);

            image.LayoutTransform = new RotateTransform(angle);
            image.LayoutTransform = new ScaleTransform(scaleX, scaleY);
            Canvas.SetTop(image, positionX);
            Canvas.SetLeft(image, positionY);

            return image;
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
        private void CreateBorder()
        {
            Line top = new Line()
            {
                X1 = 0,
                X2 = this.Width,
                Y1 = 0,
                Y2 = 0,
                StrokeDashArray = new DoubleCollection() { 2 },
                StrokeThickness = 1,
                Stroke = System.Windows.Media.Brushes.DarkOrange,
            };

            Line bottom = new Line()
            {
                X1 = 0,
                X2 = this.Width,
                Y1 = this.Height,
                Y2 = this.Height,
                StrokeDashArray = new DoubleCollection() { 2 },
                StrokeThickness = 1,
                Stroke = System.Windows.Media.Brushes.DarkOrange,
            };

            Line left = new Line()
            {
                X1 = 0,
                X2 = 0,
                Y1 = 0,
                Y2 = this.Height,
                StrokeDashArray = new DoubleCollection() { 2 },
                StrokeThickness = 1,
                Stroke = System.Windows.Media.Brushes.DarkOrange,
            };

            Line right = new Line()
            {
                X1 = this.Width,
                X2 = this.Width,
                Y1 = 0,
                Y2 = this.Height,
                StrokeDashArray = new DoubleCollection() { 2 },
                StrokeThickness = 1,
                Stroke = System.Windows.Media.Brushes.DarkOrange,
            };

            this.Children.Add(top);
            this.Children.Add(bottom);
            this.Children.Add(left);
            this.Children.Add(right);
        }
    }
}

