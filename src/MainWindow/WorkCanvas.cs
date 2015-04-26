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
        private bool captured = false;
        private float x_image, x_canvas, y_image, y_canvas, initialAngle, alpha = 0;
        private double scaleX = 1.0, scaleY = 1.0;
        private System.Windows.Point centerPoint;
        private bool topLeft;
        private bool bottomRight;
        private bool topRight;
        private bool bottomLeft;
        private const int dragTolerance = 10;

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
            x_image = (float)Canvas.GetLeft(source);
            x_canvas = (float)e.GetPosition(this).X;
            y_image = (float)Canvas.GetTop(source);
            y_canvas = (float)e.GetPosition(this).Y;

            if (MainWindow.SelectedTool == TransformType.Rotate)
            {
                System.Windows.Controls.Image img = sender as System.Windows.Controls.Image;

                float imageCenterX = (float)img.Width / 2.0f;
                float imageCenterY = (float)img.Height / 2.0f;

                float dx = x_canvas - imageCenterX;
                float dy = y_canvas - imageCenterY;

                //get initial angle
                initialAngle = (float)Math.Atan2(dy, dx);
            }

            if (MainWindow.SelectedTool == TransformType.Scale)
            {
                System.Windows.Controls.Image img = sender as System.Windows.Controls.Image;
                System.Windows.Point mouse = e.GetPosition(img);

                //set drag position on image
                SetDragImagePosition(mouse, img.Width, img.Height);

                centerPoint = GetScaleCenterPoint(img.Width, img.Height);
            }
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
                switch (MainWindow.SelectedTool)
                {
                    case TransformType.Translation: Image_MouseMoveTranslation(sender, e); break;
                    case TransformType.Scale: Image_MouseMoveScale(sender, e); break;
                    case TransformType.Rotate: Image_MouseMoveRotate(sender, e); break;
                }
            }
        }

        /// <summary>
        /// Translation Mouse move
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseMoveTranslation(object sender, MouseEventArgs e)
        {
            UIElement source = sender as UIElement;

            double x = e.GetPosition(this).X;
            double y = e.GetPosition(this).Y;

            x_image += (float)(x - x_canvas);

            Canvas.SetLeft(source, x_image);

            x_canvas = (float)x;
            y_image += (float)(y - y_canvas);

            Canvas.SetTop(source, y_image);

            y_canvas = (float)y;
        }

        /// <summary>
        /// Scale mouse move
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseMoveScale(object sender, MouseEventArgs e)
        {
            UIElement source = sender as UIElement;
            System.Windows.Controls.Image img = source as System.Windows.Controls.Image;

            System.Windows.Point mouse = mouse = Mouse.GetPosition(this);

            //scale
            scaleX = Math.Abs(centerPoint.X - mouse.X) / Math.Abs(centerPoint.X - x_canvas);
            scaleY = Math.Abs(centerPoint.Y - mouse.Y) / Math.Abs(centerPoint.Y - y_canvas);

            ScaleTransform transform = new ScaleTransform(scaleX, scaleY, centerPoint.X, centerPoint.Y);

            img = SetTransformations(GetLayerObjectByImage(img), img, transform);

            //set image coordinates
            x_image = (float)Canvas.GetLeft(img);
            y_image = (float)Canvas.GetTop(img);
        }

        /// <summary>
        /// Returns center point for scale transform
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private System.Windows.Point GetScaleCenterPoint(double width, double height)
        {
            System.Windows.Point centerPoint = new System.Windows.Point();

            if (topLeft)
            {
                centerPoint.X = width;
                centerPoint.Y = height;
            }
            else if (bottomLeft)
            {
                centerPoint.X = width;
                centerPoint.Y = 0;
            }
            else if (bottomRight)
            {
                centerPoint.X = 0;
                centerPoint.Y = 0;
            }
            else
            {
                centerPoint.X = 0;
                centerPoint.Y = height;
            }

            return centerPoint;
        }

        /// <summary>
        /// Rotate mouse move
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseMoveRotate(object sender, MouseEventArgs e)
        {
            UIElement source = sender as UIElement;
            System.Windows.Controls.Image img = source as System.Windows.Controls.Image;

            float imageCenterX = (float)img.ActualWidth / 2.0f;
            float imageCenterY = (float)img.ActualHeight / 2.0f;

            float x = (float)e.GetPosition(this).X;
            float y = (float)e.GetPosition(this).Y;

            //current angle
            float dx = x - imageCenterX;
            float dy = y - imageCenterY;
            float new_angle = (float)Math.Atan2(dy, dx);

            //alpha is final angle
            alpha = new_angle - initialAngle;

            //to degrees
            alpha *= 180 / (float)Math.PI;

            RotateTransform rotateTransform = new RotateTransform(alpha + GetLayerObjectByImage(img).InitialAngle)
            {
                CenterX = imageCenterX,
                CenterY= imageCenterY,
            };

            SetTransformations(GetLayerObjectByImage(img), img, rotateTransform);
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
            alpha = 0;
            scaleX = 1.0;
            scaleY = 1.0;
            captured = false;
        }

        /// <summary>
        /// Set layer object properties after drop
        /// </summary>
        /// <param name="source"></param>
        private void SetLayerObjectProperties(UIElement source)
        {
            System.Windows.Controls.Image droppedImage = source as System.Windows.Controls.Image;

            Transformation tr = null;
            LayerObject lo = GetLayerObjectByImage(droppedImage);

            if (imageID == lo.Column)
            {
                // if there's some transformation present, preserve destination location by moving its vector
                tr = lo.getTransformation(TransformType.Translation);
                if (tr != null && lo.Length > 1)
                {
                    tr.setVector(tr.TransformX - (y_image - lo.InitialX),
                                 tr.TransformY - (x_image - lo.InitialY));
                }

                lo.InitialX = y_image;
                lo.InitialY = x_image;
                lo.InitialAngle = alpha;
                lo.InitialScaleX = (float)scaleX;
                lo.InitialScaleY = (float)scaleY;
            }
            else
            {
                // use reciproc value to be able to eighter interpolate and extrapolate
                float progress = 1.0f / ((float)(imageID - lo.Column) / (float)(lo.Length - 1));

                switch (MainWindow.SelectedTool)
                {
                    case TransformType.Translation:
                        float transX = Interpolator.interpolateLinearValue(lo.TransformInterpolationTypes[TransformType.Translation], progress, lo.InitialX, y_image) - lo.InitialX;
                        float transY = Interpolator.interpolateLinearValue(lo.TransformInterpolationTypes[TransformType.Translation], progress, lo.InitialY, x_image) - lo.InitialY;
                        tr = new Transformation(TransformType.Translation, transX, transY, 0);
                        break;
                    case TransformType.Rotate:
                        float angle = Interpolator.interpolateLinearValue(lo.TransformInterpolationTypes[TransformType.Rotate], progress, lo.InitialAngle, alpha) - lo.InitialAngle;
                        tr = new Transformation(TransformType.Rotate, 0, 0, angle);
                        break;
                    case TransformType.Scale:
                        float scX = Interpolator.interpolateLinearValue(lo.TransformInterpolationTypes[TransformType.Scale], progress, lo.InitialScaleX, (float)scaleX) - lo.InitialScaleX;
                        float scY = Interpolator.interpolateLinearValue(lo.TransformInterpolationTypes[TransformType.Scale], progress, lo.InitialScaleY, (float)scaleY) - lo.InitialScaleY;
                        tr = new Transformation(TransformType.Scale, scX, scY, 0);
                        break;
                }

                if (tr != null)
                {
                    tr.Interpolation = lo.TransformInterpolationTypes[MainWindow.SelectedTool];
                    lo.setTransformation(tr);
                }
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
                image.MouseMove += ImageCursor_MouseMove;

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

            //transform
            image = SetTransformations(lo, image, null);

            //source 
            image.Source = source;

            //listeners
            image.MouseLeftButtonDown += Image_MouseLeftButtonDown;
            image.MouseMove += Image_MouseMove;
            image.MouseLeftButtonUp += Image_MouseLeftButtonUp;

            return image;
        }

        /// <summary>
        /// Set transformations to image
        /// </summary>
        /// <param name="lo"></param>
        /// <param name="image"></param>
        /// <param name="addedTransform"></param>
        /// <returns></returns>
        private System.Windows.Controls.Image SetTransformations(LayerObject lo, System.Windows.Controls.Image image, Transform addedTransform)
        {
            Transformation trans;
            float progress = 0.0f;

            if (lo.Length > 1)
                progress = (imageID - lo.Column) / (float)(lo.Length - 1);

            trans = lo.getTransformation(TransformType.Rotate);
            float angle = Interpolator.interpolateLinearValue(trans.Interpolation, progress, lo.InitialAngle, lo.InitialAngle + trans.TransformAngle);

            trans = lo.getTransformation(TransformType.Translation);
            float positionX = Interpolator.interpolateLinearValue(trans.Interpolation, progress, lo.InitialX, lo.InitialX + trans.TransformX);
            float positionY = Interpolator.interpolateLinearValue(trans.Interpolation, progress, lo.InitialY, lo.InitialY + trans.TransformY);

            trans = lo.getTransformation(TransformType.Scale);
            float scaleX = Interpolator.interpolateLinearValue(trans.Interpolation, progress, lo.InitialScaleX, trans.TransformX);
            float scaleY = Interpolator.interpolateLinearValue(trans.Interpolation, progress, lo.InitialScaleY, trans.TransformY);

            TransformGroup transform = new TransformGroup();
            
            //if is added transformation
            if (addedTransform != null)
            {
                //order of transformation
                if (addedTransform.GetType() == typeof(RotateTransform))
                {
                    transform.Children.Add(addedTransform);
                    transform.Children.Add(new ScaleTransform(scaleX, scaleY));
                }
                if (addedTransform.GetType() == typeof(ScaleTransform))
                {
                    transform.Children.Add(new RotateTransform(angle));
                    transform.Children.Add(addedTransform);
                }
            }
            else
            {
                transform.Children.Add(new RotateTransform(angle));
                transform.Children.Add(new ScaleTransform(scaleX, scaleY));
            }

            image.RenderTransform = transform;

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

        /// <summary>
        /// Sets cursors by selected tool
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageCursor_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Controls.Image img = sender as System.Windows.Controls.Image;
            System.Windows.Point mouse = Mouse.GetPosition(img);

            SetDragImagePosition(mouse, img.Width, img.Height);

            if (MainWindow.SelectedTool == TransformType.Translation)
            {
                img.Cursor = Cursors.SizeAll;
            }

            if (MainWindow.SelectedTool == TransformType.Scale)
            {
                if (topLeft || bottomRight)
                {
                    img.Cursor = Cursors.SizeNWSE;
                }
                else if (bottomLeft || topRight)
                {
                    img.Cursor = Cursors.SizeNESW;
                }
                else
                {
                    img.Cursor = Cursors.Arrow;
                }
            }

            if (MainWindow.SelectedTool == TransformType.Rotate)
            {
                img.Cursor = Cursors.Hand;
            }
        }

        /// <summary>
        /// Set bool variables of mouse position in image
        /// </summary>
        /// <param name="mouse"></param>
        /// <param name="imageWidth"></param>
        /// <param name="imageHeight"></param>
        private void SetDragImagePosition(System.Windows.Point mouse, double imageWidth, double imageHeight)
        {
            topLeft = mouse.X < dragTolerance && mouse.X > -dragTolerance && mouse.Y < dragTolerance && mouse.Y > -dragTolerance;
            bottomRight = mouse.X < dragTolerance + imageWidth && mouse.X > -dragTolerance + imageWidth && mouse.Y < dragTolerance + imageHeight && mouse.Y > -dragTolerance + imageHeight;
            topRight = mouse.X < dragTolerance + imageWidth && mouse.X > -dragTolerance + imageWidth && mouse.Y < dragTolerance && mouse.Y > -dragTolerance;
            bottomLeft = mouse.X < dragTolerance && mouse.X > -dragTolerance && mouse.Y < dragTolerance + imageHeight && mouse.Y > -dragTolerance + imageHeight;
        }
    }
}

