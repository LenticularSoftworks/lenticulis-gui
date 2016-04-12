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
        /// Working attributes, for transformations
        /// </summary>
        private float canvasX, canvasY, imageX, imageY, initialAngle, alpha = 0;
        private double scaleX = 1.0, scaleY = 1.0;
        private double scaleStartX, scaleStartY;
        private double initWidth, initHeight;
        private double centerX, centerY;
        private double initialCapAngle = 0.0;

        private ScaleTransform scale = null;
        private RotateTransform rotate = null;
        private TransformGroup transformGroup = null;

        /// <summary>
        /// save history only if true (mousemove method was called)
        /// </summary>
        private bool saveHistory = false;

        /// <summary>
        /// scale type
        /// </summary>
        private ScaleType scaleType;

        /// <summary>
        /// Scale type for center scale
        /// </summary>
        private ScaleType centerScaleType;

        /// <summary>
        /// Captured element of transformation in progress
        /// </summary>
        private Image capturedImage = null;

        /// <summary>
        /// Bounding box
        /// </summary>
        private BoundingBox bounding;

        /// <summary>
        /// Mouse move on / off flag
        /// </summary>
        private bool mouseMoveOn = false;

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
                bounding.SetThickness();
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

            this.MouseMove += Image_MouseMove;
            this.MouseLeftButtonUp += Image_MouseLeftButtonUp;

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
        public void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // there may not be source we are looking for
            if (sender == null)
                return;

            capturedImage = sender as Image;

            // cache image position, and position relative to image
            imageX = (float)Canvas.GetLeft(capturedImage);
            imageY = (float)Canvas.GetTop(capturedImage);
            canvasX = (float)e.GetPosition(this).X;
            canvasY = (float)e.GetPosition(this).Y;

            transformGroup = capturedImage.RenderTransform as TransformGroup;
            scale = transformGroup.Children[0] as ScaleTransform;
            rotate = transformGroup.Children[1] as RotateTransform;

            initWidth = capturedImage.Width * scale.ScaleX;
            initHeight = capturedImage.Height * scale.ScaleY;
            scaleStartX = scale.ScaleX;
            scaleStartY = scale.ScaleY;

            //create bounding box and store initial real size
            bounding.PaintBox(capturedImage);
            initWidth = capturedImage.Width * scaleStartX;
            initHeight = capturedImage.Height * scaleStartY;

            //set center coordinates
            centerX = imageX + initWidth / 2.0;
            centerY = imageY + initHeight / 2.0;

            // for scaling, we save scale on start of transformation
            SetScaleProperties(capturedImage);
            // for rotation, we save angle on stat of transformation, and determine absolute angle on start (so we can deal with relative angle later)
            SetRotateProperties(capturedImage);
        }

        /// <summary>
        /// Set rotate properties when mouse button down
        /// </summary>
        /// <param name="source"></param>
        private void SetRotateProperties(Image source)
        {
            // retrieves layer object from clicked item
            LayerObject obj = GetLayerObjectByImage(source);

            float progress = 0.0f;
            // if the image is longer than 1 frame, and column is not the initial one, set proper progress
            if (obj.Length > 1 && imageID != obj.Column)
                progress = 1.0f / ((float)(imageID - obj.Column) / (float)(obj.Length - 1));

            // get initial angle, and enhance it with interpolated transformation value
            initialAngle = (float)rotate.Angle;
            alpha = initialAngle;

            // captured angle
            double dx = centerX - canvasX;
            double dy = centerY - canvasY;
            initialCapAngle = Math.Atan2(dx, dy);
        }

        /// <summary>
        /// Set scale properties and direction type when mouse button down
        /// </summary>
        private void SetScaleProperties(Image source)
        {
            //split image to areas 3x3
            double tmpWidth = source.ActualWidth / 3.0;
            double tmpHeight = source.ActualHeight / 3.0;

            Point mouse = Mouse.GetPosition(source);

            //set scale type
            if (mouse.Y < tmpHeight)
            {
                if (mouse.X < tmpWidth)
                    scaleType = ScaleType.BottomRight;
                else if (mouse.X > tmpWidth * 2)
                    scaleType = ScaleType.BottomLeft;
                else
                    scaleType = ScaleType.Bottom;
            }
            else if (mouse.Y > tmpHeight * 2)
            {
                if (mouse.X < tmpWidth)
                    scaleType = ScaleType.TopRight;
                else if (mouse.X > tmpWidth * 2)
                    scaleType = ScaleType.TopLeft;
                else
                    scaleType = ScaleType.Top;
            }
            else
            {
                if (mouse.X < tmpWidth)
                    scaleType = ScaleType.Right;
                else
                    scaleType = ScaleType.Left;
            }

            tmpWidth = capturedImage.ActualWidth / 2.0;
            tmpHeight = capturedImage.ActualHeight / 2.0;

            //center scale type
            if (mouse.X < tmpWidth && mouse.Y < tmpHeight)
                centerScaleType = ScaleType.BottomRight;
            else if (mouse.X < tmpWidth && mouse.Y > tmpHeight)
                centerScaleType = ScaleType.TopRight;
            else if (mouse.X > tmpWidth && mouse.Y > tmpHeight)
                centerScaleType = ScaleType.TopLeft;
            else if (mouse.X > tmpWidth && mouse.Y < tmpHeight)
                centerScaleType = ScaleType.BottomLeft;
        }

        /// <summary>
        /// Mouse move event - just dispatcher, depending on what tool is selected, the appropriate
        /// method is then called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Image_MouseMove(object sender, MouseEventArgs e)
        {
            sender = capturedImage;

            // if there's a captured element, proceed
            if (capturedImage != null)
            {
                //prevents transformations proceeding and history saving on first click
                if (!mouseMoveOn)
                {
                    mouseMoveOn = true;
                    return;
                }

                saveHistory = true;

                switch (MainWindow.SelectedTool)
                {
                    case TransformType.Translation: Image_MouseMoveTranslation(sender, e); break;
                    case TransformType.Scale: Image_MouseMoveScale(sender, e); break;
                    case TransformType.Rotate: Image_MouseMoveRotate(sender, e); break;
                }

                bounding.Paint();
            }
        }

        /// <summary>
        /// Mouse move handler, when translation tool is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseMoveTranslation(object sender, MouseEventArgs e)
        {
            Image img = capturedImage;

            // current position of image
            double x = e.GetPosition(this).X;
            double y = e.GetPosition(this).Y;

            // calculate position relative to canvas x/y
            imageX += (float)(x - canvasX);
            canvasX = (float)x;
            imageY += (float)(y - canvasY);
            canvasY = (float)y;

            Canvas.SetLeft(img, imageX);
            Canvas.SetTop(img, imageY);
        }

        /// <summary>
        /// Mouse move handler, when rotation tool is selected
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void Image_MouseMoveRotate(object source, MouseEventArgs e)
        {
            double x = e.GetPosition(this).X;
            double y = e.GetPosition(this).Y;

            // current angle
            double dx = centerX - x;
            double dy = centerY - y;
            double new_angle = Math.Atan2(dx, dy);

            // alpha is final angle
            alpha = (float)(ConvertToRadians(initialAngle) + initialCapAngle - new_angle);

            // convert to degrees
            alpha *= 180 / (float)Math.PI;

            rotate.CenterX = initWidth / 2.0;
            rotate.CenterY = initHeight / 2.0;
            rotate.Angle = alpha;
        }

        /// <summary>
        /// Mouse move handler, when scaling tool is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseMoveScale(object sender, MouseEventArgs e)
        {
            Point mouse = Mouse.GetPosition(this);

            if (Keyboard.IsKeyDown(Key.LeftAlt))
            {
                Image_CenterScale(mouse);
                return;
            }

            var dx = mouse.X - canvasX;
            var dy = mouse.Y - canvasY;

            double width = dx * Math.Cos(ConvertToRadians(-alpha)) + dy * Math.Cos(ConvertToRadians(90.0 - alpha));
            double height = dx * Math.Sin(ConvertToRadians(-alpha)) + dy * Math.Sin(ConvertToRadians(90.0 - alpha));

            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                //no possibility to scale and keep ratio in this directions
                if (scaleType == ScaleType.Top || scaleType == ScaleType.Bottom || scaleType == ScaleType.Left || scaleType == ScaleType.Right)
                    return;

                KeepRatio(ref width, ref height);
            }

            //select specific scale method
            switch (scaleType)
            {
                case ScaleType.BottomRight: Image_BottomRightScale(mouse, width, height); break;
                case ScaleType.BottomLeft:
                    Image_BottomScale(mouse, width, height);
                    Image_LeftScale(mouse, width, height);
                    break;
                case ScaleType.Bottom: Image_BottomScale(mouse, width, height); break;
                case ScaleType.TopRight:
                    Image_TopScale(mouse, width, height);
                    Image_RightScale(mouse, width, height); break;
                case ScaleType.TopLeft:
                    Image_TopScale(mouse, width, height);
                    Image_LeftScale(mouse, width, height); break;
                case ScaleType.Top: Image_TopScale(mouse, width, height); break;
                case ScaleType.Right: Image_RightScale(mouse, width, height); break;
                case ScaleType.Left: Image_LeftScale(mouse, width, height); break;
                default: Image_TopScale(mouse, width, height);
                    Image_LeftScale(mouse, width, height); break;
            }
        }

        #region Scale methods

        /// <summary>
        /// Keeps aspect ratio when scaling
        /// </summary>
        /// <param name="xScale">x - scale vale</param>
        /// <param name="yScale">y - scale value</param>
        private void KeepRatio(ref double width, ref double height)
        {
            double ratio = initWidth / initHeight;
            if (initWidth > initHeight)
                height = width / ratio;
            else
                width = height * ratio;

            //invert values when TopRight or BottomLeft scaling
            if (centerScaleType == ScaleType.TopRight || centerScaleType == ScaleType.BottomLeft)
            {
                if (initWidth > initHeight)
                    height *= -1;
                else
                    width *= -1;
            }
        }

        private void Image_TopScale(Point mouse, double width, double height)
        {
            scaleY = (capturedImage.Height * scaleStartY + height) / capturedImage.Height;

            if (scaleY > 0.0)
                scale.ScaleY = scaleY;
        }

        private void Image_LeftScale(Point mouse, double width, double height)
        {
            scaleX = (capturedImage.Width * scaleStartX + width) / capturedImage.Width;

            if (scaleX > 0.0)
                scale.ScaleX = scaleX;
        }

        private void Image_BottomScale(Point mouse, double width, double height)
        {
            scaleY = (capturedImage.Height * scaleStartY - height) / capturedImage.Height;

            if (scaleY <= 0.0)
                return;

            double diffX = height * Math.Sin(ConvertToRadians(-initialAngle));
            double diffY = height * Math.Cos(ConvertToRadians(-initialAngle));

            Canvas.SetLeft(capturedImage, imageX + diffX);
            Canvas.SetTop(capturedImage, imageY + diffY);

            scale.ScaleY = scaleY;
        }

        private void Image_RightScale(Point mouse, double width, double height)
        {
            scaleX = (capturedImage.Width * scaleStartX - width) / capturedImage.Width;

            if (scaleX <= 0.0)
                return;

            double diffX = width * Math.Cos(ConvertToRadians(initialAngle));
            double diffY = width * Math.Sin(ConvertToRadians(initialAngle));

            Canvas.SetLeft(capturedImage, imageX + diffX);
            Canvas.SetTop(capturedImage, imageY + diffY);

            scale.ScaleX = scaleX;
        }

        private void Image_BottomRightScale(Point mouse, double width, double height)
        {
            scaleX = (capturedImage.Width * scaleStartX - width) / capturedImage.Width;
            scaleY = (capturedImage.Height * scaleStartY - height) / capturedImage.Height;

            if (scaleX <= 0.0 || scaleY <= 0.0)
                return;

            double diffX = height * Math.Sin(ConvertToRadians(-initialAngle)) + width * Math.Cos(ConvertToRadians(initialAngle));
            double diffY = height * Math.Cos(ConvertToRadians(-initialAngle)) + width * Math.Sin(ConvertToRadians(initialAngle));

            Canvas.SetLeft(capturedImage, imageX + diffX);
            Canvas.SetTop(capturedImage, imageY + diffY);

            scale.ScaleX = scaleX;
            scale.ScaleY = scaleY;
        }

        private void Image_CenterScale(Point mouse)
        {
            if (centerScaleType == ScaleType.BottomRight || centerScaleType == ScaleType.TopLeft)
                Image_CenterScaleBottomRight(mouse);

            if (centerScaleType == ScaleType.TopRight || centerScaleType == ScaleType.BottomLeft)
                Image_CenterScaleTopRight(mouse);
        }

        private void Image_CenterScaleBottomRight(Point mouse)
        {
            var dx = mouse.X - canvasX;
            var dy = mouse.Y - canvasY;

            if (scaleType == ScaleType.TopLeft)
            {
                dx *= -1;
                dy *= -1;
            }

            double width = dx * Math.Cos(ConvertToRadians(-alpha)) + dy * Math.Cos(ConvertToRadians(90.0 - alpha));
            double height = dx * Math.Sin(ConvertToRadians(-alpha)) + dy * Math.Sin(ConvertToRadians(90.0 - alpha));

            if (Keyboard.IsKeyDown(Key.LeftShift))
                KeepRatio(ref width, ref height);

            scaleX = (capturedImage.Width * scaleStartX - width * 2) / capturedImage.Width;
            scaleY = (capturedImage.Height * scaleStartY - height * 2) / capturedImage.Height;

            if (scaleX <= 0.0 || scaleY <= 0.0)
                return;

            double diffX = height * Math.Sin(ConvertToRadians(-initialAngle)) + width * Math.Cos(ConvertToRadians(initialAngle));
            double diffY = height * Math.Cos(ConvertToRadians(-initialAngle)) + width * Math.Sin(ConvertToRadians(initialAngle));

            Canvas.SetLeft(capturedImage, imageX + diffX);
            Canvas.SetTop(capturedImage, imageY + diffY);

            scale.ScaleX = scaleX;
            scale.ScaleY = scaleY;
        }

        private void Image_CenterScaleTopRight(Point mouse)
        {
            var dx = 2 * (mouse.X - canvasX);
            var dy = 2 * (mouse.Y - canvasY);

            if (scaleType == ScaleType.BottomLeft)
            {
                dx *= -1;
                dy *= -1;
            }

            double width = dx * Math.Cos(ConvertToRadians(-initialAngle)) + dy * Math.Cos(ConvertToRadians(90.0 - initialAngle));
            double height = dx * Math.Sin(ConvertToRadians(-initialAngle)) + dy * Math.Sin(ConvertToRadians(90.0 - initialAngle));

            if (Keyboard.IsKeyDown(Key.LeftShift))
                KeepRatio(ref width, ref height);

            //top
            scaleY = (capturedImage.Height * scaleStartY + height) / capturedImage.Height;
            //right
            scaleX = (capturedImage.Width * scaleStartX - width) / capturedImage.Width;

            if (scaleX <= 0.0 || scaleY <= 0.0)
                return;

            scale.ScaleX = scaleX;
            scale.ScaleY = scaleY;

            double diffX = width * Math.Cos(ConvertToRadians(initialAngle)) - height * Math.Sin(ConvertToRadians(-initialAngle));
            double diffY = width * Math.Sin(ConvertToRadians(initialAngle)) - height * Math.Cos(ConvertToRadians(-initialAngle));

            Canvas.SetLeft(capturedImage, imageX + diffX / 2.0);
            Canvas.SetTop(capturedImage, imageY + diffY / 2.0);
        }

        #endregion Scale methods

        /// <summary>
        /// Mouse up - finish transformation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);

            //if captured is not image in canvas return
            if (capturedImage == null)
                return;

            scale.CenterX = 0;
            scale.CenterY = 0;
            scaleX = scale.ScaleX;
            scaleY = scale.ScaleY;

            if (MainWindow.SelectedTool == TransformType.Scale)
            {
                Rect bounds = capturedImage.TransformToVisual(this).TransformBounds(new Rect(capturedImage.RenderSize));
                var centX = bounds.Left + bounds.Width / 2.0;
                var centY = bounds.Top + bounds.Height / 2.0;

                imageX = (float)(centX - capturedImage.Width * scaleX / 2.0);
                imageY = (float)(centY - capturedImage.Height * scaleY / 2.0);

                Canvas.SetLeft(capturedImage, imageX);
                Canvas.SetTop(capturedImage, imageY);

                transformGroup.Children[1] = new RotateTransform()
                {
                    CenterX = capturedImage.Width * scaleX / 2.0,
                    CenterY = capturedImage.Height * scaleY / 2.0,
                    Angle = alpha
                };

            }

            // finish transformation by setting transformations properly interpolated/extrapolated to layerobject itself
            SetLayerObjectProperties(capturedImage);

            saveHistory = false;
            mouseMoveOn = false;

            // and restore initial attribute values
            alpha = 0;
            scaleX = 1.0;
            scaleY = 1.0;
            capturedImage = null;
        }

        /// <summary>
        /// Set layer object properties after drop
        /// </summary>
        /// <param name="source"></param>
        private void SetLayerObjectProperties(UIElement source)
        {
            // retrieve layer object from image on canvas
            Image droppedImage = source as Image;
            LayerObject lo = GetLayerObjectByImage(droppedImage);

            //store history item for undo
            LayerObjectHistory historyItem = lo.GetHistoryItem();

            //layer object started at canvas
            if (imageID == lo.Column)
                SetStartFrameProperties(lo);
            else
                SetOtherFrameProperties(lo);

            //store history item for redo
            if (saveHistory)
            {
                historyItem.StoreRedo();
                ProjectHolder.HistoryList.AddHistoryItem(historyItem);
            }
        }

        /// <summary>
        /// Set layer ovject properties if current canvas is object's starting
        /// </summary>
        /// <param name="layerObject">layer object</param>
        private void SetStartFrameProperties(LayerObject layerObject)
        {
            Transformation transformation;

            // if there's some transformation present, preserve destination location by moving its vector
            if (MainWindow.SelectedTool == TransformType.Translation)
            {
                transformation = layerObject.getTransformation(TransformType.Translation);
                if (transformation != null && layerObject.Length > 1 && transformation.TransformX != 0 && transformation.TransformY != 0)
                {
                    transformation.setVector(transformation.TransformX - (imageY - layerObject.InitialX),
                                 transformation.TransformY - (imageX - layerObject.InitialY));
                }

                layerObject.InitialX = imageX;
                layerObject.InitialY = imageY;
            }
            // on rotation, just add angle, it's already relative angle, so it is sufficient to add it
            // to initial value (or in case of existing transformation, subtract from transform value)
            else if (MainWindow.SelectedTool == TransformType.Rotate)
            {
                transformation = layerObject.getTransformation(TransformType.Rotate);
                // if there's a transformation present, update relative angle
                if (transformation != null && layerObject.Length > 1 && transformation.TransformAngle != 0.0)
                    transformation.setAngle(transformation.TransformAngle - (alpha));

                layerObject.InitialAngle = alpha;
            }
            // on scaling, preserve final scale when modifying first frame
            else if (MainWindow.SelectedTool == TransformType.Scale)
            {
                transformation = layerObject.getTransformation(TransformType.Scale);
                // apply back logic only when any scale transformation was set
                if (transformation != null && layerObject.Length > 1 && (Math.Abs(transformation.TransformX) > 0.001 || Math.Abs(transformation.TransformY) > 0.001))
                {
                    transformation.setVector(transformation.TransformX - ((float)scaleX - layerObject.InitialScaleX),
                                 transformation.TransformY - ((float)scaleY - layerObject.InitialScaleY));
                }

                if (scaleX > 0.0 && scaleY > 0.0)
                {
                    layerObject.InitialScaleX = (float)scaleX;
                    layerObject.InitialScaleY = (float)scaleY;
                }

                layerObject.InitialX = imageX;
                layerObject.InitialY = imageY;
            }
        }

        /// <summary>
        /// Set layer ovject properties if current canvas isn't object's starting
        /// </summary>
        /// <param name="layerObject">layer object</param>
        private void SetOtherFrameProperties(LayerObject layerObject)
        {
            Transformation transformation = null;
            Transformation trAdded = null;

            // use reciproc value to be able to eighter interpolate and extrapolate
            float progress = 1.0f / ((float)(imageID - layerObject.Column) / (float)(layerObject.Length - 1));
            InterpolationType interpolation;

            switch (MainWindow.SelectedTool)
            {
                case TransformType.Translation:
                    // inter/extrapolate value of both directions, and store newly calculated vector into transformation object
                    interpolation = layerObject.getTransformation(TransformType.Translation).Interpolation;
                    Transformation trans3D = layerObject.getTransformation(TransformType.Translation3D); // subtract 3d translation vector
                    float transX = Interpolator.interpolateLinearValue(interpolation, progress, layerObject.InitialX, imageX) - layerObject.InitialX;
                    float transY = Interpolator.interpolateLinearValue(interpolation, progress, layerObject.InitialY, imageY) - layerObject.InitialY;
                    transformation = new Transformation(TransformType.Translation, transX - trans3D.TransformX, transY, 0);
                    break;
                case TransformType.Rotate:
                    // inter/extrapolate value of new relative angle
                    interpolation = layerObject.getTransformation(TransformType.Rotate).Interpolation;
                    float angle = Interpolator.interpolateLinearValue(interpolation, progress, layerObject.InitialAngle, alpha) - layerObject.InitialAngle;
                    transformation = new Transformation(TransformType.Rotate, 0, 0, -angle);
                    break;
                case TransformType.Scale:
                    // inter/extrapolate value of both directions; it's also relative scale, so 0 means "no change"
                    interpolation = layerObject.getTransformation(TransformType.Scale).Interpolation;
                    float scX = Interpolator.interpolateLinearValue(interpolation, progress, layerObject.InitialScaleX, (float)scaleX) - layerObject.InitialScaleX;
                    float scY = Interpolator.interpolateLinearValue(interpolation, progress, layerObject.InitialScaleY, (float)scaleY) - layerObject.InitialScaleY;
                    transformation = new Transformation(TransformType.Scale, scX, scY, 0);

                    //Add transform translation because of image coordinates change
                    float tranScaleX = Interpolator.interpolateLinearValue(InterpolationType.Linear, progress, layerObject.InitialX, imageX) - layerObject.InitialX;
                    float tranScaleY = Interpolator.interpolateLinearValue(InterpolationType.Linear, progress, layerObject.InitialY, imageY) - layerObject.InitialY;
                    trAdded = new Transformation(TransformType.Translation, tranScaleX, tranScaleY, 0);
                    trAdded.Interpolation = InterpolationType.Linear;
                    break;
            }

            // if there's a transformation created, propagate it to layerobject
            if (trAdded != null)
                layerObject.setTransformation(trAdded);

            if (transformation != null)
            {
                transformation.Interpolation = layerObject.getTransformation(MainWindow.SelectedTool).Interpolation;
                layerObject.setTransformation(transformation);
            }
        }

        /// <summary>
        /// Return layer object by Image
        /// </summary>
        /// <param name="droppedImage">Image</param>
        /// <returns>Instance of LayerObject</returns>
        private LayerObject GetLayerObjectByImage(Image droppedImage)
        {
            List<LayerObject> layerObjects = GetImages();
            int index = 0;

            // go through all images on canvas
            for (int i = 0; i < this.Children.Count; i++)
            {
                if (this.Children[i].GetType() == typeof(Image))
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
        /// Paint images on work canvas
        /// </summary>
        public void Paint()
        {
            // remove all child elements
            this.Children.Clear();

            // list of images sorted by layer
            List<LayerObject> images = GetImages();

            //add layer object
            AddLayerObjects(this, true);

            //re-add bounding - always on top
            this.Children.Remove(bounding);
            bounding = new BoundingBox(this);

            // add border around canvas
            CreateBorder();
        }


        /// <summary>
        /// Create and returns new canvas for bitmap rendering.
        /// </summary>
        /// <returns>Canvas</returns>
        public Canvas GetCanvas()
        {
            //create new canvas
            Canvas canvas = new Canvas();
            canvas.Width = ProjectHolder.Width;
            canvas.Height = ProjectHolder.Height;

            //Add layer objects
            AddLayerObjects(canvas, false);

            //set clip
            canvas.ClipToBounds = true;

            return canvas;
        }

        /// <summary>
        /// Add layer object to canvas and attach listeners if needed.
        /// </summary>
        /// <param name="canvas">Canvas</param>
        /// <param name="listeners">Attach listeners if true</param>
        private void AddLayerObjects(Canvas canvas, bool listeners)
        {
            // list of images sorted by layer
            List<LayerObject> images = GetImages();

            foreach (LayerObject lo in images)
            {
                // get image holder for current image
                ImageHolder imageHolder = Storage.Instance.getImageHolder(lo.ResourceId);
                // and retrieve image source for specified size
                ImageSource source = imageHolder.getImageForSize(ProjectHolder.Width, ProjectHolder.Height);

                //TODO loading project error

                // create new image instance
                Image image = new Image();

                // set its properties
                image.Source = source;
                image.Width = imageHolder.width;
                image.Height = imageHolder.height;
                image.Stretch = Stretch.Fill;

                // attach listener
                if (listeners)
                    image.MouseLeftButtonDown += Image_MouseLeftButtonDown;

                // set transformations
                image = SetTransformations(lo, image);
                canvas.Children.Add(image);
            }
        }

        /// <summary>
        /// Set transformations to image
        /// </summary>
        /// <param name="lo">layer object to be modified</param>
        /// <param name="image">canvas image</param>
        /// <returns></returns>
        private Image SetTransformations(LayerObject lo, Image image)
        {
            Transformation trans;

            // determine progress
            float progress = 0.0f;
            // if it's not longer than one image, the progress is always 0
            if (lo.Length > 1)
                progress = (imageID - lo.Column) / (float)(lo.Length - 1);

            // interpolate rotation value
            trans = lo.getTransformation(TransformType.Rotate);
            float angle = Interpolator.interpolateLinearValue(trans.Interpolation, progress, lo.InitialAngle, lo.InitialAngle - trans.TransformAngle);

            // retrieve translation value
            trans = lo.getTransformation(TransformType.Translation);
            float transX = Interpolator.interpolateLinearValue(trans.Interpolation, progress, lo.InitialX, lo.InitialX + trans.TransformX);

            // 3D translation value
            Transformation trans3D = lo.getTransformation(TransformType.Translation3D);
            float trans3DX = Interpolator.interpolateLinearValue(trans3D.Interpolation, progress, lo.InitialX, lo.InitialX + trans3D.TransformX);

            float positionX = transX + trans3DX - lo.InitialX; // merge transformation set by canvas operation and set by 3D generator 
            float positionY = Interpolator.interpolateLinearValue(trans.Interpolation, progress, lo.InitialY, lo.InitialY + trans.TransformY);

            // retrieve scaled value
            trans = lo.getTransformation(TransformType.Scale);
            float scaleX = Interpolator.interpolateLinearValue(trans.Interpolation, progress, lo.InitialScaleX, lo.InitialScaleX + trans.TransformX);
            float scaleY = Interpolator.interpolateLinearValue(trans.Interpolation, progress, lo.InitialScaleY, lo.InitialScaleY + trans.TransformY);

            //if scale values is negative, set them to 0
            if (scaleX <= 0.0f)
                scaleX = 0.0f;
            if (scaleY <= 0.0f)
                scaleY = 0.0f;

            TransformGroup transform = new TransformGroup();
            ImageHolder holder = Storage.Instance.getImageHolder(lo.ResourceId);

            // refreshes scaling transform
            transform.Children.Add(new ScaleTransform(scaleX, scaleY));

            // and refreshes rotate transform
            RotateTransform rt = new RotateTransform(angle);
            rt.CenterX = image.Width * scaleX / 2.0;
            rt.CenterY = image.Height * scaleY / 2.0;
            transform.Children.Add(rt);

            // append transform group
            image.RenderTransform = transform;

            // and set position if needed
            Canvas.SetTop(image, positionY);
            Canvas.SetLeft(image, positionX);

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
                if (item.IsInColumn(this.imageID) && item.GetLayerObject().Visible)
                    layerObjects.Add(item.GetLayerObject());
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
        /// Degrees to Radians Conversion
        /// </summary>
        /// <param name="angle">Angle - degrees</param>
        /// <returns>Angle - radians</returns>
        public double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        /// <summary>
        /// Removes box from canvas
        /// </summary>
        public void HideBox()
        {
            if (bounding != null)
                bounding.HideBox();
        }
    }
}