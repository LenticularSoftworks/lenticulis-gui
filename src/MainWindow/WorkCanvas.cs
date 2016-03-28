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

        /// <summary>
        /// save history only if true (mousemove method was called)
        /// </summary>
        private bool saveHistory = false;

        //scale type
        private ScaleType scaleType;

        /// <summary>
        /// Captured element of transformation in progress
        /// </summary>
        private Image capturedImage = null;

        /// <summary>
        /// Bounding box
        /// </summary>
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

            //create bounding box and store initial real size
            bounding.PaintBox(capturedImage);
            initWidth = bounding.ActualWidth;
            initHeight = bounding.ActualHeight;

            //set center coordinates
            centerX = imageX + initWidth / 2.0;
            centerY = imageY + initHeight / 2.0;

            // for scaling, we save scale on start of transformation
            if (MainWindow.SelectedTool == TransformType.Scale)
                SetScaleProperties(capturedImage);
            // for rotation, we save angle on stat of transformation, and determine absolute angle on start (so we can deal with relative angle later)
            else if (MainWindow.SelectedTool == TransformType.Rotate)
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

            float imageCenterX = (float)source.ActualWidth / 2.0f;
            float imageCenterY = (float)source.ActualHeight / 2.0f;

            float dx = canvasX - imageCenterX;
            float dy = canvasY - imageCenterY;

            // get initial angle, and enhance it with interpolated transformation value
            initialAngle = (float)Math.Atan2(dy, dx) - (float)(Interpolator.interpolateLinearValue(obj.getTransformation(TransformType.Rotate).Interpolation, progress, 0, obj.getTransformation(TransformType.Rotate).TransformAngle) * Math.PI / 180.0);
        }

        /// <summary>
        /// Set scale properties and direction type when mouse button down
        /// </summary>
        private void SetScaleProperties(Image source)
        {
            scaleStartX = initWidth / source.Width;
            scaleStartY = initHeight / source.Height;

            //split image to areas 3x3
            double tmpWidth = initWidth / 3.0;
            double tmpHeight = initHeight / 3.0;

            Point mouse = Mouse.GetPosition(bounding);

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
        }

        /// <summary>
        /// Mouse move event - just dispatcher, depending on what tool is selected, the appropriate
        /// method is then called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Image_MouseMove(object sender, MouseEventArgs e)
        {
            // use captured element as event sender
            sender = capturedImage;
            saveHistory = true;

            // if there's a captured element, proceed
            if (capturedImage != null)
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
        /// Mouse move handler, when translation tool is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseMoveTranslation(object sender, MouseEventArgs e)
        {
            Image img = sender as Image;

            // current position of image
            double x = e.GetPosition(this).X;
            double y = e.GetPosition(this).Y;

            // calculate position relative to canvas x/y
            imageX += (float)(x - canvasX);
            canvasX = (float)x;
            imageY += (float)(y - canvasY);
            canvasY = (float)y;

            // transform image
            img = SetTransformations(GetLayerObjectByImage(img), img, null, false);

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
            Image img = source as Image;

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
        /// Mouse move handler, when scaling tool is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseMoveScale(object sender, MouseEventArgs e)
        {
            Image img = sender as Image;
            Point mouse = Mouse.GetPosition(this);
            ScaleTransform transform = new ScaleTransform();

            //select specific scale method
            if (Keyboard.IsKeyDown(Key.LeftAlt))
                transform = Image_CenterScale(img, mouse);
            else
            {
                switch (scaleType)
                {
                    case ScaleType.BottomRight: transform = Image_BottomRightCornerScale(img, mouse, true, true); break;
                    case ScaleType.BottomLeft: transform = Image_BottomLeftCornerScale(img, mouse); break;
                    case ScaleType.Bottom: transform = Image_BottomRightCornerScale(img, mouse, false, true); break;
                    case ScaleType.TopRight: transform = Image_TopRightCornerScale(img, mouse); break;
                    case ScaleType.TopLeft: transform = Image_TopLeftCornerScale(img, mouse, true, true); break;
                    case ScaleType.Top: transform = Image_TopLeftCornerScale(img, mouse, false, true); break;
                    case ScaleType.Right: transform = Image_BottomRightCornerScale(img, mouse, true, false); break;
                    case ScaleType.Left: transform = Image_TopLeftCornerScale(img, mouse, true, false); break;
                    default: transform = Image_TopLeftCornerScale(img, mouse, true, true); break;
                }
            }

            if (transform != null)
                img = SetTransformations(GetLayerObjectByImage(img), img, transform, false);
        }

        #region Scale methods

        /// <summary>
        /// Keeps aspect ratio when scaling
        /// </summary>
        /// <param name="xScale">x - scale vale</param>
        /// <param name="yScale">y - scale value</param>
        private void keepRatio(ref double xScale, ref double yScale)
        {
            if (xScale > yScale)
                yScale = xScale * scaleStartY / scaleStartX;
            else
                xScale = yScale * scaleStartX / scaleStartY;
        }

        /// <summary>
        /// Scale to top left corner
        /// </summary>
        /// <param name="img">image</param>
        /// <param name="mouse">mouse</param>
        /// <param name="left">Scale to left</param>
        /// <param name="top">scale to top</param>
        /// <returns>scale transform</returns>
        private ScaleTransform Image_TopLeftCornerScale(Image img, Point mouse, bool left, bool top)
        {
            scaleX = scaleStartX;
            scaleY = scaleStartY;

            if (left)
                scaleX = scaleStartX * (mouse.X - imageX) / initWidth;
            if (top)
                scaleY = scaleStartY * (mouse.Y - imageY) / initHeight;

            //if left shift down - scale with keep aspect ratio
            if (Keyboard.IsKeyDown(Key.LeftShift))
                keepRatio(ref scaleX, ref scaleY);

            if (scaleX > 0.0 && scaleY > 0.0)
                return new ScaleTransform(scaleX, scaleY);
            else
                return null;
        }

        /// <summary>
        /// Scale image to its center
        /// </summary>
        /// <param name="img">Image</param>
        /// <param name="mouse">mouse point</param>
        /// <returns>scale transform</returns>
        private ScaleTransform Image_CenterScale(Image img, Point mouse)
        {
            scaleX = scaleStartX * Math.Abs(mouse.X - centerX) / initWidth * 2.0;
            scaleY = scaleStartY * Math.Abs(mouse.Y - centerY) / initHeight * 2.0;

            if (Keyboard.IsKeyDown(Key.LeftShift))
                keepRatio(ref scaleX, ref scaleY);

            if (scaleX > 0.0 && scaleY > 0.0)
            {
                Canvas.SetTop(img, centerY - (img.Height * scaleY) / 2.0);
                Canvas.SetLeft(img, centerX - (img.Width * scaleX) / 2.0);

                return new ScaleTransform(scaleX, scaleY);
            }
            else
                return null;
        }

        /// <summary>
        /// Scale to bottom right corner
        /// </summary>
        /// <param name="img">image</param>
        /// <param name="mouse">mouse</param>
        /// <param name="bottom">scale to bottom</param>
        /// <param name="right">scale to right</param>
        /// <returns>scale transform</returns>
        private ScaleTransform Image_BottomRightCornerScale(Image img, Point mouse, bool right, bool bottom)
        {
            //Scale initial values
            scaleX = scaleStartX;
            scaleY = scaleStartY;

            //calc new scale if needed
            if (right)
                scaleX = scaleStartX * (initWidth + imageX - mouse.X) / initWidth;
            if (bottom)
                scaleY = scaleStartY * (initHeight + imageY - mouse.Y) / initHeight;

            if (Keyboard.IsKeyDown(Key.LeftShift))
                keepRatio(ref scaleX, ref scaleY);

            if (scaleX > 0.0 && scaleY > 0.0)
            {
                Canvas.SetTop(img, imageY + initHeight - scaleY * img.Height);
                Canvas.SetLeft(img, imageX + initWidth - scaleX * img.Width);

                return new ScaleTransform(scaleX, scaleY);
            }
            else
                return null;
        }

        /// <summary>
        /// Scale to bottom left corner
        /// </summary>
        /// <param name="img">image</param>
        /// <param name="mouse">mouse</param>
        /// <returns>scale transform</returns>
        private ScaleTransform Image_BottomLeftCornerScale(Image img, Point mouse)
        {
            scaleX = scaleStartX * (mouse.X - imageX) / initWidth;
            scaleY = scaleStartY * (initHeight + imageY - mouse.Y) / initHeight;

            if (Keyboard.IsKeyDown(Key.LeftShift))
                keepRatio(ref scaleX, ref scaleY);

            if (scaleX > 0.0 && scaleY > 0.0)
            {
                Canvas.SetTop(img, imageY + initHeight - scaleY * img.Height);
                return new ScaleTransform(scaleX, scaleY);
            }
            else
                return null;
        }

        /// <summary>
        /// Scale to top right corner
        /// </summary>
        /// <param name="img">image</param>
        /// <param name="mouse">mouse</param>
        /// <returns>scale transform</returns>
        private ScaleTransform Image_TopRightCornerScale(Image img, Point mouse)
        {
            scaleX = scaleStartX * (initWidth + imageX - mouse.X) / initWidth;
            scaleY = scaleStartY * (mouse.Y - imageY) / initHeight;

            if (Keyboard.IsKeyDown(Key.LeftShift))
                keepRatio(ref scaleX, ref scaleY);

            if (scaleX > 0.0 && scaleY > 0.0)
            {
                Canvas.SetLeft(img, imageX + initWidth - scaleX * img.Width);
                return new ScaleTransform(scaleX, scaleY);
            }
            else
                return null;
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

            imageX = (float)Canvas.GetLeft(capturedImage);
            imageY = (float)Canvas.GetTop(capturedImage);

            // finish transformation by setting transformations properly interpolated/extrapolated to layerobject itself
            SetLayerObjectProperties(capturedImage);

            saveHistory = false;

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
                historyItem.StoreAction();
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

                layerObject.InitialAngle += alpha;
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
                    float angle = Interpolator.interpolateLinearValue(interpolation, progress, layerObject.InitialAngle, layerObject.InitialAngle + alpha) - layerObject.InitialAngle;
                    transformation = new Transformation(TransformType.Rotate, 0, 0, angle);
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
                ImageSource source = imageHolder.getImageForSize(ProjectHolder.Width, ProjectHolder.Height); //TODO loading project

                // create new image instance
                Image image = new Image();

                // set its properties
                image.Source = source;
                image.Width = imageHolder.width;
                image.Height = imageHolder.height;
                image.Stretch = Stretch.Fill;

                if (listeners)
                {
                    // attach listeners
                    image.MouseLeftButtonDown += Image_MouseLeftButtonDown;
                    image.MouseLeftButtonUp += Image_MouseLeftButtonUp;
                }

                // set transformations
                image = SetTransformations(lo, image, null, true);
                canvas.Children.Add(image);
            }
        }

        /// <summary>
        /// Set transformations to image
        /// </summary>
        /// <param name="lo">layer object to be modified</param>
        /// <param name="image">canvas image</param>
        /// <param name="addedTransform">the transform we are adding right now</param>
        /// <param name="setPosition">when false, does not recalculate position</param>
        /// <returns></returns>
        private Image SetTransformations(LayerObject lo, Image image, Transform addedTransform, bool setPosition)
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
                Canvas.SetTop(image, positionY);
                Canvas.SetLeft(image, positionX);
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
    }
}