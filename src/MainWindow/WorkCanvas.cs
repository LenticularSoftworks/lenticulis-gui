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
using System.Windows.Controls.Primitives;

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
        private double scaleStartX, scaleStartY;
        private UIElement capturedElement = null;

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
            capturedElement = source;
            Mouse.Capture(source);
            captured = true;
            x_image = (float)Canvas.GetLeft(source);
            x_canvas = (float)e.GetPosition(this).X;
            y_image = (float)Canvas.GetTop(source);
            y_canvas = (float)e.GetPosition(this).Y;

            LayerObject obj = GetLayerObjectByImage(source as System.Windows.Controls.Image);
            float progress = 0.0f;
            // if the image is longer than 1 frame, and column is not the initial one, set proper progress
            if (obj.Length > 1 && imageID != obj.Column)
                progress = 1.0f / ((float)(imageID - obj.Column) / (float)(obj.Length - 1));

            if (MainWindow.SelectedTool == TransformType.Scale)
            {
                scaleStartX = Interpolator.interpolateLinearValue(obj.TransformInterpolationTypes[TransformType.Scale], progress, obj.InitialScaleX, obj.InitialScaleX + obj.getTransformation(TransformType.Scale).TransformX);
                scaleStartY = Interpolator.interpolateLinearValue(obj.TransformInterpolationTypes[TransformType.Scale], progress, obj.InitialScaleY, obj.InitialScaleY + obj.getTransformation(TransformType.Scale).TransformY);
            }
            else if (MainWindow.SelectedTool == TransformType.Rotate)
            {
                System.Windows.Controls.Image img = sender as System.Windows.Controls.Image;

                float imageCenterX = (float)img.ActualWidth / 2.0f;
                float imageCenterY = (float)img.ActualHeight / 2.0f;

                float dx = x_canvas - imageCenterX;
                float dy = y_canvas - imageCenterY;

                //get initial angle
                initialAngle = (float)Math.Atan2(dy, dx) - (float)(Interpolator.interpolateLinearValue(obj.TransformInterpolationTypes[TransformType.Rotate], progress, 0, obj.getTransformation(TransformType.Rotate).TransformAngle)*Math.PI/180.0);
            }
        }

        /// <summary>
        /// Move image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            sender = capturedElement;

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
            System.Windows.Controls.Image img = sender as System.Windows.Controls.Image;

            double x = e.GetPosition(this).X;
            double y = e.GetPosition(this).Y;

            x_image += (float)(x - x_canvas);
            x_canvas = (float)x;
            y_image += (float)(y - y_canvas);
            y_canvas = (float)y;

            img = SetTransformations(GetLayerObjectByImage(img), img, null, false);

            Canvas.SetLeft(img, x_image);
            Canvas.SetTop(img, y_image);
        }

        /// <summary>
        /// Scale mouse move
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseMoveScale(object sender, MouseEventArgs e)
        {
            System.Windows.Controls.Image img = sender as System.Windows.Controls.Image;

            System.Windows.Point mouse = mouse = Mouse.GetPosition(this);

            //scale
            scaleX = scaleStartX * (mouse.X - x_image) / (x_canvas - x_image);
            scaleY = scaleStartY * (mouse.Y - y_image) / (y_canvas - y_image);

            if (scaleX < 0.0 || scaleY < 0.0)
                return;

            ScaleTransform transform = new ScaleTransform(scaleX, scaleY);
            img = SetTransformations(GetLayerObjectByImage(img), img, transform, false);
        }

        /// <summary>
        /// Rotate mouse move
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void Image_MouseMoveRotate(object source, MouseEventArgs e)
        {
            System.Windows.Controls.Image img = source as System.Windows.Controls.Image;

            float imageCenterX = (float)img.RenderSize.Width / 2.0f;
            float imageCenterY = (float)img.RenderSize.Height / 2.0f;

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
                CenterY = imageCenterY,
            };

            SetTransformations(GetLayerObjectByImage(img), img, rotateTransform, true);
        }

        /// <summary>
        /// Drop image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);

            System.Windows.Controls.Image img = capturedElement as System.Windows.Controls.Image;

            x_image = (float)Canvas.GetLeft(img);
            y_image = (float)Canvas.GetTop(img);

            SetLayerObjectProperties(capturedElement);

            alpha = 0;
            scaleX = 1.0;
            scaleY = 1.0;
            captured = false;
            capturedElement = null;
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
                if (MainWindow.SelectedTool == TransformType.Translation)
                {
                    tr = lo.getTransformation(TransformType.Translation);
                    if (tr != null && lo.Length > 1)
                    {
                        tr.setVector(tr.TransformX - (y_image - lo.InitialX),
                                     tr.TransformY - (x_image - lo.InitialY));
                    }

                    lo.InitialX = y_image;
                    lo.InitialY = x_image;
                }
                else if (MainWindow.SelectedTool == TransformType.Rotate)
                {
                    tr = lo.getTransformation(TransformType.Rotate);
                    if (tr != null && lo.Length > 1)
                        tr.setAngle(tr.TransformAngle - (alpha));

                    lo.InitialAngle += alpha;
                }
                else if (MainWindow.SelectedTool == TransformType.Scale)
                {
                    tr = lo.getTransformation(TransformType.Scale);
                    // apply back logic only when any scale transformation was set
                    if (tr != null && lo.Length > 1 && (Math.Abs(tr.TransformX) > 0.001 || Math.Abs(tr.TransformY) > 0.001))
                    {
                        tr.setVector(tr.TransformX - ((float)scaleX - lo.InitialScaleX),
                                     tr.TransformY - ((float)scaleY - lo.InitialScaleY));
                    }

                    lo.InitialScaleX = (float)scaleX;
                    lo.InitialScaleY = (float)scaleY;

                    lo.InitialX = y_image;
                    lo.InitialY = x_image;
                }
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
                        float angle = Interpolator.interpolateLinearValue(lo.TransformInterpolationTypes[TransformType.Rotate], progress, lo.InitialAngle, lo.InitialAngle + alpha) - lo.InitialAngle;
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

                System.Windows.Controls.Image image = new System.Windows.Controls.Image();

                image.Source = source;
                image.Width = imageHolder.width;
                image.Height = imageHolder.height;
                image.Stretch = Stretch.Fill;

                //listeners
                image.MouseLeftButtonDown += Image_MouseLeftButtonDown;
                image.MouseMove += Image_MouseMove;
                image.MouseLeftButtonUp += Image_MouseLeftButtonUp;
                image.MouseMove += ImageCursor_MouseMove;

                //transform
                image = SetTransformations(lo, image, null, true);

                this.Children.Add(image);
            }

            //add border of canvas
            CreateBorder();
        }

        /// <summary>
        /// Set transformations to image
        /// </summary>
        /// <param name="lo"></param>
        /// <param name="image"></param>
        /// <param name="addedTransform"></param>
        /// <returns></returns>
        private System.Windows.Controls.Image SetTransformations(LayerObject lo, System.Windows.Controls.Image image, Transform addedTransform, bool setPosition)
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
            float scaleX = Interpolator.interpolateLinearValue(trans.Interpolation, progress, lo.InitialScaleX, lo.InitialScaleX + trans.TransformX);
            float scaleY = Interpolator.interpolateLinearValue(trans.Interpolation, progress, lo.InitialScaleY, lo.InitialScaleY + trans.TransformY);

            TransformGroup transform = new TransformGroup();
            ImageHolder holder = Storage.Instance.getImageHolder(lo.ResourceId);
            //if is added transformation
            if (addedTransform != null)
            {
                //order of transformation
                if (addedTransform.GetType() == typeof(RotateTransform))
                {
                    transform.Children.Add(new ScaleTransform(scaleX, scaleY));

                    RotateTransform rt = addedTransform as RotateTransform;
                    rt.CenterX = holder.width * scaleX / 2.0;
                    rt.CenterY = holder.height * scaleY / 2.0;
                    transform.Children.Add(rt);
                }
                if (addedTransform.GetType() == typeof(ScaleTransform))
                {
                    ScaleTransform st = addedTransform as ScaleTransform;
                    transform.Children.Add(st);

                    RotateTransform rt = new RotateTransform(angle);
                    rt.CenterX = holder.width * st.ScaleX / 2.0;
                    rt.CenterY = holder.height * st.ScaleY / 2.0;
                    transform.Children.Add(rt);
                }
            }
            else
            {
                transform.Children.Add(new ScaleTransform(scaleX, scaleY));

                RotateTransform rt = new RotateTransform(angle);
                rt.CenterX = holder.width * scaleX / 2.0;
                rt.CenterY = holder.height * scaleY / 2.0;
                transform.Children.Add(rt);
            }

            image.RenderTransform = transform;

            if (setPosition)
            {
                Canvas.SetTop(image, positionX);
                Canvas.SetLeft(image, positionY);
            }

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
            if (captured)
            {
                return;
            }

            System.Windows.Controls.Image img = sender as System.Windows.Controls.Image;
            System.Windows.Point mouse = Mouse.GetPosition(img);

            if (MainWindow.SelectedTool == TransformType.Translation)
            {
                img.Cursor = Cursors.SizeAll;
            }

            if (MainWindow.SelectedTool == TransformType.Scale)
            {
                img.Cursor = Cursors.SizeNWSE;
            }

            if (MainWindow.SelectedTool == TransformType.Rotate)
            {
                img.Cursor = Cursors.Hand;
            }
        }
    }
}
