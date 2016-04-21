using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using lenticulis_gui.src.App;

namespace lenticulis_gui
{
    /// <summary>
    /// Class represents bounding box arround image
    /// </summary>
    public class BoundingBox : Border
    {
        /// <summary>
        /// Canvas associated with this bounding box
        /// </summary>
        private WorkCanvas canvas;

        /// <summary>
        /// Scale diraction sqaures
        /// </summary>
        private Rectangle topLeft, topRight, bottomLeft, bottomRight, top, bottom, left, right;

        /// <summary>
        /// Square scale
        /// </summary>
        private double squareScale = 1.0;

        /// <summary>
        /// Initial square size
        /// </summary>
        private const int initSquareSize = 7;

        /// <summary>
        /// Instance of selected image
        /// </summary>
        private Image image;

        /// <summary>
        /// Rectangular area for mouse hit test
        /// </summary>
        private Rectangle mouseHitRectangle;

        /// <summary>
        /// List of resize cursors
        /// </summary>
        private List<Cursor> cursors;

        /// <summary>
        /// Class constructor creates border (bounding box) and adds to canvas.
        /// </summary>
        /// <param name="canvas">Canvas</param>
        public BoundingBox(Canvas canvas)
        {
            this.canvas = (WorkCanvas)canvas;
            this.BorderBrush = CreateBrush();
            this.canvas.Children.Add(this);
            this.mouseHitRectangle = new Rectangle();

            mouseHitRectangle.Fill = new SolidColorBrush(Colors.Red);
            mouseHitRectangle.Opacity = 0.5;
            mouseHitRectangle.Stretch = Stretch.Fill;
            mouseHitRectangle.IsHitTestVisible = false;

            cursors = new List<Cursor>()
            {
                Cursors.SizeNWSE,
                Cursors.SizeNS,
                Cursors.SizeNESW,
                Cursors.SizeWE,
            };

            //listeners to mouse hit layer
            SetListeners();

            InitializeSquares();
        }

        /// <summary>
        /// Paints bounding box arround image
        /// </summary>
        /// <param name="img">Image</param>
        public void PaintBox(Image img)
        {
            this.image = img;
            Paint();

            //if not conatins add mouse hit rectangle
            if (!canvas.Children.Contains(mouseHitRectangle))
                canvas.Children.Add(mouseHitRectangle);

            //paint squares
            RemoveSquares();
            RepaintSquarePositions();
            AddSquares();

            SetThickness();
        }

        /// <summary>
        /// Repaint bounding box
        /// </summary>
        public void Paint()
        {
            TransformGroup transGroup = image.RenderTransform as TransformGroup;
            ScaleTransform scale = transGroup.Children[0] as ScaleTransform;
            RotateTransform rotate = transGroup.Children[1] as RotateTransform;

            //Set size by actual scale to box and hit rectangle
            this.Width = image.Width * scale.ScaleX;
            this.Height = image.Height * scale.ScaleY;
            mouseHitRectangle.Width = this.Width;
            mouseHitRectangle.Height = this.Height;

            //apply only rotate scale resizec bounding box thickness
            this.RenderTransform = rotate;
            mouseHitRectangle.RenderTransform = rotate;

            //place to canvas with new coordinates
            Canvas.SetTop(mouseHitRectangle, Canvas.GetTop(image));
            Canvas.SetLeft(mouseHitRectangle, Canvas.GetLeft(image));

            Canvas.SetTop(this, Canvas.GetTop(image));
            Canvas.SetLeft(this, Canvas.GetLeft(image));

            RepaintSquarePositions();
        }

        /// <summary>
        /// Adjust the border thickness to canvas width
        /// </summary>
        /// <returns>Thickness</returns>
        public void SetThickness()
        {
            int thickness = 1;
            double value = 1 / ((WorkCanvas)canvas).CanvasScale;

            if (value > 1)
                thickness = (int)Math.Round(value);

            this.BorderThickness = new Thickness(thickness);

            //do not update squares if they weren't painted
            if (canvas.Children.Contains(topLeft))
                UpdateSquares();
        }

        /// <summary>
        /// Set square size by canvas scale
        /// </summary>
        private void UpdateSquares()
        {
            double size = (1 / ((WorkCanvas)canvas).CanvasScale) * initSquareSize;
            squareScale = size / initSquareSize;

            ScaleTransform transform = new ScaleTransform(squareScale, squareScale);

            topLeft.LayoutTransform = transform;
            top.LayoutTransform = transform;
            topRight.LayoutTransform = transform;
            left.LayoutTransform = transform;
            right.LayoutTransform = transform;
            bottomLeft.LayoutTransform = transform;
            bottom.LayoutTransform = transform;
            bottomRight.LayoutTransform = transform;
            RepaintSquarePositions();
        }

        /// <summary>
        /// Renew positions of squares
        /// </summary>
        private void RepaintSquarePositions()
        {
            //get bounds size and center of rotation
            Rect bounds = image.TransformToVisual(canvas).TransformBounds(new Rect(image.RenderSize));
            RotateTransform rotate = ((TransformGroup)image.RenderTransform).Children[1] as RotateTransform;
            double centerX = bounds.Left + bounds.Width / 2.0;
            double centerY = bounds.Top + bounds.Height / 2.0;

            //center to rectangle corner distance
            double radius = Math.Sqrt((this.Width / 2.0) * (this.Width / 2.0) + (this.Height / 2.0) * (this.Height / 2.0));
            //diagonal angle
            double initAngle = Math.Atan(this.Height / this.Width);

            //corners coordinates
            double topLeftX = centerX - radius * Math.Cos(initAngle + canvas.ConvertToRadians(rotate.Angle));
            double topLeftY = centerY - radius * Math.Sin(initAngle + canvas.ConvertToRadians(rotate.Angle));
            double bottomRightX = centerX + radius * Math.Cos(initAngle + canvas.ConvertToRadians(rotate.Angle));
            double bottomRightY = centerY + radius * Math.Sin(initAngle + canvas.ConvertToRadians(rotate.Angle));
            double bottomLeftX = centerX - radius * Math.Cos(initAngle + canvas.ConvertToRadians(-rotate.Angle));
            double bottomLeftY = centerY + radius * Math.Sin(initAngle + canvas.ConvertToRadians(-rotate.Angle));
            double topRightX = centerX + radius * Math.Cos(initAngle + canvas.ConvertToRadians(-rotate.Angle));
            double topRightY = centerY - radius * Math.Sin(initAngle + canvas.ConvertToRadians(-rotate.Angle));

            //top left
            Canvas.SetTop(topLeft, topLeftY);
            Canvas.SetLeft(topLeft, topLeftX);
            topLeft.RenderTransform = new RotateTransform() { Angle = rotate.Angle };
            //bottom right
            Canvas.SetTop(bottomRight, bottomRightY - squareScale * initSquareSize);
            Canvas.SetLeft(bottomRight, bottomRightX - squareScale * initSquareSize);
            bottomRight.RenderTransform = new RotateTransform() { Angle = rotate.Angle, CenterX = squareScale * initSquareSize, CenterY = squareScale * initSquareSize };
            //top
            Canvas.SetTop(top, topLeftY + (topRightY - topLeftY) / 2.0);
            Canvas.SetLeft(top, topLeftX + (topRightX - topLeftX) / 2.0);
            top.RenderTransform = new RotateTransform() { Angle = rotate.Angle };
            //top right
            Canvas.SetTop(topRight, topRightY);
            Canvas.SetLeft(topRight, topRightX - squareScale * initSquareSize);
            topRight.RenderTransform = new RotateTransform() { Angle = rotate.Angle, CenterX = squareScale * initSquareSize };
            //left
            Canvas.SetTop(left, bottomLeftY - (bottomLeftY - topLeftY) / 2.0);
            Canvas.SetLeft(left, bottomLeftX - (bottomLeftX - topLeftX) / 2.0);
            left.RenderTransform = new RotateTransform() { Angle = rotate.Angle };
            //right
            Canvas.SetTop(right, bottomRightY - (bottomRightY - topRightY) / 2.0);
            Canvas.SetLeft(right, bottomRightX - (bottomRightX - topRightX) / 2.0 - squareScale * initSquareSize);
            right.RenderTransform = new RotateTransform() { Angle = rotate.Angle, CenterX = squareScale * initSquareSize };
            //bottom left
            Canvas.SetTop(bottomLeft, bottomLeftY - squareScale * initSquareSize);
            Canvas.SetLeft(bottomLeft, bottomLeftX);
            bottomLeft.RenderTransform = new RotateTransform() { Angle = rotate.Angle, CenterY = squareScale * initSquareSize };
            //bottom
            Canvas.SetTop(bottom, bottomLeftY - (bottomLeftY - bottomRightY) / 2.0 - squareScale * initSquareSize);
            Canvas.SetLeft(bottom, bottomLeftX - (bottomLeftX - bottomRightX) / 2.0);
            bottom.RenderTransform = new RotateTransform() { Angle = rotate.Angle, CenterY = squareScale * initSquareSize };
        }

        /// <summary>
        /// Add squares to canvas
        /// </summary>
        private void AddSquares()
        {
            canvas.Children.Add(topLeft);
            canvas.Children.Add(top);
            canvas.Children.Add(topRight);
            canvas.Children.Add(left);
            canvas.Children.Add(right);
            canvas.Children.Add(bottomLeft);
            canvas.Children.Add(bottom);
            canvas.Children.Add(bottomRight);
        }

        /// <summary>
        /// Remove squares from canvas
        /// </summary>
        private void RemoveSquares()
        {
            canvas.Children.Remove(topLeft);
            canvas.Children.Remove(top);
            canvas.Children.Remove(topRight);
            canvas.Children.Remove(left);
            canvas.Children.Remove(right);
            canvas.Children.Remove(bottomLeft);
            canvas.Children.Remove(bottom);
            canvas.Children.Remove(bottomRight);
        }

        /// <summary>
        /// Init squares
        /// </summary>
        private void InitializeSquares()
        {
            topLeft = CreateSquare();
            top = CreateSquare();
            topRight = CreateSquare();
            left = CreateSquare();
            right = CreateSquare();
            bottomLeft = CreateSquare();
            bottom = CreateSquare();
            bottomRight = CreateSquare();
        }

        /// <summary>
        /// Creates Rectangle instance as drag square
        /// </summary>
        /// <returns></returns>
        private Rectangle CreateSquare()
        {
            return new Rectangle()
            {
                Width = initSquareSize,
                Height = initSquareSize,
                Fill = Brushes.Yellow,
                Opacity = 0.5,
                IsHitTestVisible = false
            };
        }

        /// <summary>
        /// Mouse down event for hit test rectangle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseHitRectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(mouseHitRectangle);
            canvas.Image_MouseLeftButtonDown(image, e);
        }

        /// <summary>
        /// Mouse up event for hit test rectangle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseHitRectangle_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            canvas.Image_MouseLeftButtonUp(image, e);
        }

        /// <summary>
        /// Removes box by right button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseHitRectangle_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            HideBox();
        }

        /// <summary>
        /// Removes box
        /// </summary>
        public void HideBox()
        {
            canvas.Children.Remove(mouseHitRectangle);
            RemoveSquares();
            this.Width = 0;
            this.Height = 0;
        }

        /// <summary>
        /// Sets cursors by selected tool
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageCursor_MouseMove(object sender, MouseEventArgs e)
        {
            Rectangle rect = sender as Rectangle;

            // retrieves mouse position
            Point mouse = Mouse.GetPosition(rect);

            // and set cursor according to selected tool
            switch (MainWindow.SelectedTool)
            {
                case TransformType.Translation:
                    rect.Cursor = Cursors.SizeAll;
                    break;
                case TransformType.Scale:
                    SetScaleCursor(rect, mouse);
                    break;
                case TransformType.Rotate:
                    rect.Cursor = Cursors.Hand;
                    break;
            }
        }

        /// <summary>
        /// Sets cusror for scale by cursor position in image
        /// </summary>
        /// <param name="rect">mouse hit layer</param>
        /// <param name="mouse">cursor point</param>
        private void SetScaleCursor(Rectangle rect, Point mouse)
        {
            double tmpWidth = rect.ActualWidth / 3.0;
            double tmpHeight = rect.ActualHeight / 3.0;

            if (mouse.Y < tmpHeight * 2 && mouse.Y > tmpHeight && (mouse.X < tmpWidth || mouse.X > tmpWidth * 2))
                rect.Cursor = RotateMouseScaleCursor(Cursors.SizeWE);
            else if (mouse.X < tmpWidth * 2 && mouse.X > tmpWidth && (mouse.Y < tmpHeight || mouse.Y > tmpHeight * 2))
                rect.Cursor = RotateMouseScaleCursor(Cursors.SizeNS);
            else if ((mouse.X > tmpWidth * 2 && tmpHeight * 2 > mouse.Y) || (mouse.Y > tmpHeight * 2 && mouse.X < tmpWidth * 2))
                rect.Cursor = RotateMouseScaleCursor(Cursors.SizeNESW);
            else if ((mouse.X < tmpWidth && tmpHeight > mouse.Y) || (mouse.Y > tmpHeight * 2 && mouse.X > tmpWidth * 2))
                rect.Cursor = RotateMouseScaleCursor(Cursors.SizeNWSE);
            else
                rect.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Return rotated scale cursor
        /// </summary>
        /// <param name="cursor">Default cursor with zero angle</param>
        /// <returns>Rotated cursor</returns>
        private Cursor RotateMouseScaleCursor(Cursor cursor)
        {
            int index = cursors.IndexOf(cursor);

            RotateTransform rotate = ((TransformGroup)image.RenderTransform).Children[1] as RotateTransform;
            double angle = rotate.Angle % 360;
            
            if (rotate.Angle < 0)
                angle = 360 + angle;

            //move index by rotate +- 22.5 degrees
            if ((angle >= 22.5 && angle < 67.5) || (angle >= 202.5 && angle < 247.5))
                index += 1;
            else if ((angle >= 67.5 && angle < 112.5) || (angle >= 247.5 && angle < 292.5))
                index += 2;
            else if ((angle >= 112.5 && angle < 157.5) || (angle >= 292.5 && angle < 337.5))
                index += 3;
            
            index = index % cursors.Count;

            return cursors[index];
        }

        /// <summary>
        /// Set listeners to mouse hit test rectangle
        /// </summary>
        private void SetListeners()
        {
            mouseHitRectangle.IsHitTestVisible = true;
            mouseHitRectangle.MouseLeftButtonDown += mouseHitRectangle_MouseLeftButtonDown;
            mouseHitRectangle.MouseLeftButtonUp += mouseHitRectangle_MouseLeftButtonUp;
            mouseHitRectangle.MouseRightButtonUp += mouseHitRectangle_MouseRightButtonUp;
            mouseHitRectangle.MouseMove += ImageCursor_MouseMove;
        }

        /// <summary>
        /// Creates drawing tile brush to make dashed border
        /// </summary>
        /// <returns>Tile drawing brush</returns>
        private DrawingBrush CreateBrush()
        {
            DrawingBrush brush = new DrawingBrush();

            //yellow background
            GeometryDrawing whiteGeometry = new GeometryDrawing()
            {
                Brush = Brushes.Yellow,
                Geometry = new RectangleGeometry(new Rect(0, 0, 1, 1))
            };

            //first black row
            GeometryDrawing blackGeometry = new GeometryDrawing()
            {
                Brush = Brushes.Black,
                Geometry = new RectangleGeometry(new Rect(0, 0, 0.5, 0.5))
            };

            //second shifted black row
            GeometryDrawing blackGeometryShift = new GeometryDrawing()
            {
                Brush = Brushes.Black,
                Geometry = new RectangleGeometry(new Rect(0.5, 0.5, 0.5, 0.5))
            };

            DrawingGroup group = new DrawingGroup();
            group.Children.Add(whiteGeometry);
            group.Children.Add(blackGeometry);
            group.Children.Add(blackGeometryShift);

            brush.Viewport = new Rect(0, 0, 0.1, 0.1);
            brush.TileMode = TileMode.Tile;
            brush.Drawing = group;

            return brush;
        }
    }
}
