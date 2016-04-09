using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Diagnostics;
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
        /// Squares in border corners
        /// </summary>
        private Rectangle topLeft, topRight, bottomLeft, bottomRight, top, bottom, left, right;

        /// <summary>
        /// Square scale
        /// </summary>
        private double squareScale = 1.0;

        /// <summary>
        /// Initial square size
        /// </summary>
        private const int initSquareSize = 10;

        /// <summary>
        /// Instance of selected image
        /// </summary>
        private Image image;

        /// <summary>
        /// Rectangular area for mouse hit test
        /// </summary>
        private Rectangle mouseHitRectangle;

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

            //Set size to box and hit rectangle
            this.Width = image.Width;
            this.Height = image.Height;
            mouseHitRectangle.Width = this.Width;
            mouseHitRectangle.Height = this.Height;

            this.RenderTransform = transGroup;
            mouseHitRectangle.RenderTransform = transGroup;

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
            double borderTop = Canvas.GetTop(this);
            double borderLeft = Canvas.GetLeft(this);

            //top left
            Canvas.SetTop(topLeft, borderTop);
            Canvas.SetLeft(topLeft, borderLeft);
            //top
            Canvas.SetTop(top, borderTop);
            Canvas.SetLeft(top, borderLeft + this.Width / 2.0 - squareScale * initSquareSize);
            //top right
            Canvas.SetTop(topRight, borderTop);
            Canvas.SetLeft(topRight, borderLeft + this.Width - squareScale * initSquareSize);
            //left
            Canvas.SetTop(left, borderTop + this.Height / 2.0 - squareScale * initSquareSize);
            Canvas.SetLeft(left, borderLeft);
            //right
            Canvas.SetTop(right, borderTop + this.Height / 2.0 - squareScale * initSquareSize);
            Canvas.SetLeft(right, borderLeft + this.Width - squareScale * initSquareSize);
            //bottom left
            Canvas.SetTop(bottomLeft, borderTop + this.Height - squareScale * initSquareSize);
            Canvas.SetLeft(bottomLeft, borderLeft);
            //bottom
            Canvas.SetTop(bottom, borderTop + this.Height - squareScale * initSquareSize);
            Canvas.SetLeft(bottom, borderLeft + this.Width / 2.0 - squareScale * initSquareSize);
            //bottom right
            Canvas.SetTop(bottomRight, borderTop + this.Height - squareScale * initSquareSize);
            Canvas.SetLeft(bottomRight, borderLeft + this.Width - squareScale * initSquareSize);
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
            DoubleCollection dash = new DoubleCollection() { 2 };

            return new Rectangle()
            {
                Width = initSquareSize,
                Height = initSquareSize,
                Stroke = Brushes.Black,
                StrokeDashArray = dash,
                Fill = Brushes.Yellow,
                Opacity = 0.5,
                IsHitTestVisible = false
            };
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
