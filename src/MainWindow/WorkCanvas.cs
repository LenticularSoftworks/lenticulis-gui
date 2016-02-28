using lenticulis_gui.src.App;
using lenticulis_gui.src.Containers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        /// <summary>
        /// Drag and drop indicator
        /// </summary>
        private bool captured = false;

        /// <summary>
        /// Working attributes, for transformations
        /// </summary>
        private float x_image, x_canvas, y_image, y_canvas, initialAngle, alpha = 0;
        private double scaleX = 1.0, scaleY = 1.0;
        private double scaleStartX, scaleStartY;

        /// <summary>
        /// Captured element of transformation in progress
        /// </summary>
        private UIElement capturedElement = null;

        private BoundingBox bounding;

        /// <summary>
        /// Cached canvas scale (for zoom in/out)
        /// </summary>
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

        /// <summary>
        /// The only one constructor
        /// </summary>
        /// <param name="imageID">Id of image on this canvas</param>
        public WorkCanvas(int imageID)
            : base()
        {
            this.imageID = imageID;

            this.Width = ProjectHolder.Width;
            this.Height = ProjectHolder.Height;
            this.Margin = new Thickness(10, 10, 10, 10);
            this.Background = new SolidColorBrush(Colors.White);
            this.bounding = new BoundingBox(this);

            // zoom in/out cached scale transform
            this.LayoutTransform = new ScaleTransform(canvasScaleCached, canvasScaleCached);

            // draw everything we need to have there- contents of current keyframe
            Paint();
        }

        /// <summary>
        /// Starts dragging event for transformation, if there's a object under mouse cursor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // there may not be source we are looking for
            UIElement source = sender as UIElement;
            if (source == null)
                return;

            capturedElement = source;
            Mouse.Capture(source);
            captured = true;

            // cache image position, and position relative to image
            x_image = (float)Canvas.GetLeft(source);
            x_canvas = (float)e.GetPosition(this).X;
            y_image = (float)Canvas.GetTop(source);
            y_canvas = (float)e.GetPosition(this).Y;

            // retrieves layer object from clicked item
            LayerObject obj = GetLayerObjectByImage((Image)source);

            //create bounding box
            bounding.PaintBox((Image)sender);

            float progress = 0.0f;
            // if the image is longer than 1 frame, and column is not the initial one, set proper progress
            if (obj.Length > 1 && imageID != obj.Column)
                progress = 1.0f / ((float)(imageID - obj.Column) / (float)(obj.Length - 1));

            // for scaling, we save scale on start of transformation
            if (MainWindow.SelectedTool == TransformType.Scale)
            {
                scaleStartX = Interpolator.interpolateLinearValue(obj.TransformInterpolationTypes[TransformType.Scale], progress, obj.InitialScaleX, obj.InitialScaleX + obj.getTransformation(TransformType.Scale).TransformX);
                scaleStartY = Interpolator.interpolateLinearValue(obj.TransformInterpolationTypes[TransformType.Scale], progress, obj.InitialScaleY, obj.InitialScaleY + obj.getTransformation(TransformType.Scale).TransformY);
            }
            // for rotation, we save angle on stat of transformation, and determine absolute angle on start (so we can deal with relative angle later)
            else if (MainWindow.SelectedTool == TransformType.Rotate)
            {
                System.Windows.Controls.Image img = sender as System.Windows.Controls.Image;

                float imageCenterX = (float)img.ActualWidth / 2.0f;
                float imageCenterY = (float)img.ActualHeight / 2.0f;

                float dx = x_canvas - imageCenterX;
                float dy = y_canvas - imageCenterY;

                // get initial angle, and enhance it with interpolated transformation value
                initialAngle = (float)Math.Atan2(dy, dx) - (float)(Interpolator.interpolateLinearValue(obj.TransformInterpolationTypes[TransformType.Rotate], progress, 0, obj.getTransformation(TransformType.Rotate).TransformAngle)*Math.PI/180.0);
            }
        }

        /// <summary>
        /// Mouse move event - just dispatcher, depending on what tool is selected, the appropriate
        /// method is then called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            // use captured element as event sender
            sender = capturedElement;

            // if there's a captured element, proceed
            if (captured)
            {
                switch (MainWindow.SelectedTool)
                {
                    case TransformType.Translation: Image_MouseMoveTranslation(sender, e); break;
                    case TransformType.Scale: Image_MouseMoveScale(sender, e); break;
                    case TransformType.Rotate: Image_MouseMoveRotate(sender, e); break;
                }

                //repaint bounding box
                bounding.PaintBox((Image)sender);
            }
        }

        /// <summary>
        /// Mouse move handler, when translation tool is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseMoveTranslation(object sender, MouseEventArgs e)
        {
            System.Windows.Controls.Image img = sender as System.Windows.Controls.Image;

            // current position of image
            double x = e.GetPosition(this).X;
            double y = e.GetPosition(this).Y;

            // calculate position relative to canvas x/y
            x_image += (float)(x - x_canvas);
            x_canvas = (float)x;
            y_image += (float)(y - y_canvas);
            y_canvas = (float)y;

            // transform image
            img = SetTransformations(GetLayerObjectByImage(img), img, null, false);

            Canvas.SetLeft(img, x_image);
            Canvas.SetTop(img, y_image);
        }

        /// <summary>
        /// Mouse move handler, when scaling tool is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseMoveScale(object sender, MouseEventArgs e)
        {
            System.Windows.Controls.Image img = sender as System.Windows.Controls.Image;

            System.Windows.Point mouse = mouse = Mouse.GetPosition(this);

            // scale using new values
            // also we multiply the new scale using the scale, which has been there on transform start
            // it's due to "continuing" in scaling from current scale value, not jumping back to scale 1.0
            scaleX = scaleStartX * (mouse.X - x_image) / (x_canvas - x_image);
            scaleY = scaleStartY * (mouse.Y - y_image) / (y_canvas - y_image);

            // there's no such thing as valid negative scale (at least for now)
            if (scaleX < 0.0 || scaleY < 0.0)
                return;

            // transform image on canvas
            ScaleTransform transform = new ScaleTransform(scaleX, scaleY);
            img = SetTransformations(GetLayerObjectByImage(img), img, transform, false);
        }

        /// <summary>
        /// Mouse move handler, when rotation tool is selected
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void Image_MouseMoveRotate(object source, MouseEventArgs e)
        {
            System.Windows.Controls.Image img = source as System.Windows.Controls.Image;

            // current center (so we can calculate the rotation angle using it)
            float imageCenterX = (float)img.RenderSize.Width / 2.0f;
            float imageCenterY = (float)img.RenderSize.Height / 2.0f;

            float x = (float)e.GetPosition(this).X;
            float y = (float)e.GetPosition(this).Y;

            // current angle
            float dx = x - imageCenterX;
            float dy = y - imageCenterY;
            float new_angle = (float)Math.Atan2(dy, dx);

            // alpha is final angle
            alpha = new_angle - initialAngle;

            // convert to degrees
            alpha *= 180 / (float)Math.PI;

            // apply new tranfrormation
            RotateTransform rotateTransform = new RotateTransform(alpha + GetLayerObjectByImage(img).InitialAngle)
            {
                CenterX = imageCenterX,
                CenterY = imageCenterY,
            };

            SetTransformations(GetLayerObjectByImage(img), img, rotateTransform, true);
        }

        /// <summary>
        /// Mouse up - finish transformation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);

            System.Windows.Controls.Image img = capturedElement as System.Windows.Controls.Image;

            x_image = (float)Canvas.GetLeft(img);
            y_image = (float)Canvas.GetTop(img);

            // finish transformation by setting transformations properly interpolated/extrapolated to layerobject itself
            SetLayerObjectProperties(capturedElement);

            // and restore initial attribute values
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
            // retrieve layer object from image on canvas
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
                // on rotation, just add angle, it's already relative angle, so it is sufficient to add it
                // to initial value (or in case of existing transformation, subtract from transform value)
                else if (MainWindow.SelectedTool == TransformType.Rotate)
                {
                    tr = lo.getTransformation(TransformType.Rotate);
                    // if there's a transformation present, update relative angle
                    if (tr != null && lo.Length > 1)
                        tr.setAngle(tr.TransformAngle - (alpha));

                    lo.InitialAngle += alpha;
                }
                // on scaling, preserve final scale when modifying first frame
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
                        // inter/extrapolate value of both directions, and store newly calculated vector into transformation object
                        float transX = Interpolator.interpolateLinearValue(lo.TransformInterpolationTypes[TransformType.Translation], progress, lo.InitialX, y_image) - lo.InitialX;
                        float transY = Interpolator.interpolateLinearValue(lo.TransformInterpolationTypes[TransformType.Translation], progress, lo.InitialY, x_image) - lo.InitialY;
                        tr = new Transformation(TransformType.Translation, transX, transY, 0);
                        break;
                    case TransformType.Rotate:
                        // inter/extrapolate value of new relative angle
                        float angle = Interpolator.interpolateLinearValue(lo.TransformInterpolationTypes[TransformType.Rotate], progress, lo.InitialAngle, lo.InitialAngle + alpha) - lo.InitialAngle;
                        tr = new Transformation(TransformType.Rotate, 0, 0, angle);
                        break;
                    case TransformType.Scale:
                        // inter/extrapolate value of both directions; it's also relative scale, so 0 means "no change"
                        float scX = Interpolator.interpolateLinearValue(lo.TransformInterpolationTypes[TransformType.Scale], progress, lo.InitialScaleX, (float)scaleX) - lo.InitialScaleX;
                        float scY = Interpolator.interpolateLinearValue(lo.TransformInterpolationTypes[TransformType.Scale], progress, lo.InitialScaleY, (float)scaleY) - lo.InitialScaleY;
                        tr = new Transformation(TransformType.Scale, scX, scY, 0);
                        break;
                }

                // if there's a transformation created, propagate it to layerobject
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
        /// <param name="droppedImage">Image</param>
        /// <returns>Instance of LayerObject</returns>
        private LayerObject GetLayerObjectByImage(System.Windows.Controls.Image droppedImage)
        {
            List<LayerObject> layerObjects = GetImages();
            int index = 0;

            // go through all images on canvas
            for (int i = 0; i < this.Children.Count; i++)
            {
                if (this.Children[i].GetType() == typeof(System.Windows.Controls.Image))
                {
                    // if we got the image we are looking for, return layer object on that index
                    if (this.Children[i] == droppedImage)
                        return layerObjects[index];
                    else
                        index++; // otherwise increase index
                }
            }

            return null;
        }

        /// <summary>
        /// Paint imagec on canvas
        /// </summary>
        public void Paint()
        {
            // remove all child elements
            this.Children.Clear();

            // list of images sorted by layer
            List<LayerObject> images = GetImages();

            // now we draw all images belonging here
            foreach (LayerObject lo in images)
            {
                // get image holder for current image
                ImageHolder imageHolder = Storage.Instance.getImageHolder(lo.ResourceId);
                // and retrieve image source for specified size
                ImageSource source = imageHolder.getImageForSize(ProjectHolder.Width, ProjectHolder.Height);

                // create new image instance
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();

                // set its properties
                image.Source = source;
                image.Width = imageHolder.width;
                image.Height = imageHolder.height;
                image.Stretch = Stretch.Fill;

                // attach listeners
                image.MouseLeftButtonDown += Image_MouseLeftButtonDown;
                image.MouseMove += Image_MouseMove;
                image.MouseLeftButtonUp += Image_MouseLeftButtonUp;
                image.MouseMove += ImageCursor_MouseMove;

                // set transformations
                image = SetTransformations(lo, image, null, true);

                this.Children.Add(image);
            }

            // add border around canvas
            CreateBorder();
        }

        /// <summary>
        /// Set transformations to image
        /// </summary>
        /// <param name="lo">layer object to be modified</param>
        /// <param name="image">canvas image</param>
        /// <param name="addedTransform">the transform we are adding right now</param>
        /// <param name="setPosition">when false, does not recalculate position</param>
        /// <returns></returns>
        private System.Windows.Controls.Image SetTransformations(LayerObject lo, System.Windows.Controls.Image image, Transform addedTransform, bool setPosition)
        {
            Transformation trans;

            // determine progress
            float progress = 0.0f;
            // if it's not longer than one image, the progress is always 0
            if (lo.Length > 1)
                progress = (imageID - lo.Column) / (float)(lo.Length - 1);

            // interpolate rotation value
            trans = lo.getTransformation(TransformType.Rotate);
            float angle = Interpolator.interpolateLinearValue(trans.Interpolation, progress, lo.InitialAngle, lo.InitialAngle + trans.TransformAngle);

            // retrieve translation value
            trans = lo.getTransformation(TransformType.Translation);
            float positionX = Interpolator.interpolateLinearValue(trans.Interpolation, progress, lo.InitialX, lo.InitialX + trans.TransformX);
            float positionY = Interpolator.interpolateLinearValue(trans.Interpolation, progress, lo.InitialY, lo.InitialY + trans.TransformY);

            // retrieve scaled value
            trans = lo.getTransformation(TransformType.Scale);
            float scaleX = Interpolator.interpolateLinearValue(trans.Interpolation, progress, lo.InitialScaleX, lo.InitialScaleX + trans.TransformX);
            float scaleY = Interpolator.interpolateLinearValue(trans.Interpolation, progress, lo.InitialScaleY, lo.InitialScaleY + trans.TransformY);

            TransformGroup transform = new TransformGroup();
            ImageHolder holder = Storage.Instance.getImageHolder(lo.ResourceId);
            // if is added transformation
            if (addedTransform != null)
            {
                // order of transformations
                if (addedTransform.GetType() == typeof(RotateTransform))
                {
                    // refreshes scaling transformation
                    transform.Children.Add(new ScaleTransform(scaleX, scaleY));

                    // and appends new rotate transform
                    RotateTransform rt = addedTransform as RotateTransform;
                    rt.CenterX = holder.width * scaleX / 2.0;
                    rt.CenterY = holder.height * scaleY / 2.0;
                    transform.Children.Add(rt);
                }
                if (addedTransform.GetType() == typeof(ScaleTransform))
                {
                    // appends new scale transform
                    ScaleTransform st = addedTransform as ScaleTransform;
                    transform.Children.Add(st);

                    // and refreshes rotation transform
                    RotateTransform rt = new RotateTransform(angle);
                    rt.CenterX = holder.width * st.ScaleX / 2.0;
                    rt.CenterY = holder.height * st.ScaleY / 2.0;
                    transform.Children.Add(rt);
                }
            }
            else
            {
                // refreshes scaling transform
                transform.Children.Add(new ScaleTransform(scaleX, scaleY));

                // and refreshes rotate transform
                RotateTransform rt = new RotateTransform(angle);
                rt.CenterX = holder.width * scaleX / 2.0;
                rt.CenterY = holder.height * scaleY / 2.0;
                transform.Children.Add(rt);
            }

            // append transform group
            image.RenderTransform = transform;

            // and set position if needed
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

            // fill layer objects in current canvas
            foreach (TimelineItem item in mw.timelineList)
            {
                if (item.IsInColumn(this.imageID) && item.getLayerObject().Visible)
                    layerObjects.Add(item.getLayerObject());
            }

            // sort layer objects by order of visibility - lower layers goes first, top layers comes last
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
            // create top border
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

            // bottom border
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

            // left border
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

            // right border
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
            // do not change cursor during transformation
            if (captured)
                return;

            // gets image on position
            System.Windows.Controls.Image img = sender as System.Windows.Controls.Image;
            // retrieves mouse position
            System.Windows.Point mouse = Mouse.GetPosition(img);

            // and set cursor according to selected tool
            switch (MainWindow.SelectedTool)
            {
                case TransformType.Translation:
                    img.Cursor = Cursors.SizeAll;
                    break;
                case TransformType.Scale:
                    img.Cursor = Cursors.SizeNWSE;
                    break;
                case TransformType.Rotate:
                    img.Cursor = Cursors.Hand;
                    break;
            }
        }
    }
}
