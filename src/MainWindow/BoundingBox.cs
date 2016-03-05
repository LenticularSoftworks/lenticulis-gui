using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Diagnostics;

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
        private Canvas canvas;

        /// <summary>
        /// Squares in border corners
        /// </summary>
        private Rectangle topLeft, topRight, bottomLeft, bottomRight;

        /// <summary>
        /// Square scale
        /// </summary>
        private double squareScale = 1.0;

        /// <summary>
        /// Initial square size
        /// </summary>
        private const int initSquareSize = 10;

        /// <summary>
        /// Class constructor creates border (bounding box) and adds to canvas.
        /// </summary>
        /// <param name="canvas">Canvas</param>
        public BoundingBox(Canvas canvas)
        {
            this.canvas = canvas;
            this.BorderBrush = CreateBrush();

            InitializeSquares();

            canvas.Children.Add(this);
        }

        /// <summary>
        /// Paints bounding box arround image
        /// </summary>
        /// <param name="img">Image</param>
        public void PaintBox(Image img)
        {
            Rect bounds = img.TransformToVisual(canvas).TransformBounds(new Rect(img.RenderSize));

            this.Width = bounds.Width;
            this.Height = bounds.Height;

            Canvas.SetTop(this, bounds.Top);
            Canvas.SetLeft(this, bounds.Left);

            SetThickness();

            //set border always on top
            canvas.Children.Remove(this);
            canvas.Children.Add(this);

            PaintSquares();
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
            topRight.LayoutTransform = transform;
            bottomLeft.LayoutTransform = transform;
            bottomRight.LayoutTransform = transform;
            PaintSquares();
        }

        /// <summary>
        /// Paint squares
        /// </summary>
        private void PaintSquares()
        {
            RemoveSquares();

            double borderTop = Canvas.GetTop(this);
            double borderLeft = Canvas.GetLeft(this);

            Canvas.SetTop(topLeft, borderTop);
            Canvas.SetLeft(topLeft, borderLeft);

            Canvas.SetTop(topRight, borderTop);
            Canvas.SetLeft(topRight, borderLeft + this.Width - squareScale * initSquareSize);

            Canvas.SetTop(bottomLeft, borderTop + this.Height - squareScale * initSquareSize);
            Canvas.SetLeft(bottomLeft, borderLeft);

            Canvas.SetTop(bottomRight, borderTop + this.Height - squareScale * initSquareSize);
            Canvas.SetLeft(bottomRight, borderLeft + this.Width - squareScale * initSquareSize);

            AddSquares();
        }

        /// <summary>
        /// Init squares
        /// </summary>
        private void InitializeSquares()
        {
            DoubleCollection dash = new DoubleCollection() { 2 };

            topLeft = new Rectangle()
            {
                Width = initSquareSize,
                Height = initSquareSize,
                Stroke = Brushes.Black,
                StrokeDashArray = dash,
                Fill = Brushes.Yellow,
                Opacity = 0.5,
                IsHitTestVisible = false
            };

            topRight = new Rectangle()
            {
                Width = initSquareSize,
                Height = initSquareSize,
                Stroke = Brushes.Black,
                StrokeDashArray = dash,
                Fill = Brushes.Yellow,
                Opacity = 0.5,
                IsHitTestVisible = false
            };

            bottomLeft = new Rectangle()
            {
                Width = initSquareSize,
                Height = initSquareSize,
                Stroke = Brushes.Black,
                StrokeDashArray = dash,
                Fill = Brushes.Yellow,
                Opacity = 0.5,
                IsHitTestVisible = false
            };

            bottomRight = new Rectangle()
            {
                Width = initSquareSize,
                Height = initSquareSize,
                Stroke = Brushes.Black,
                StrokeDashArray = dash,
                Fill = Brushes.Yellow,
                Opacity = 0.5,
                IsHitTestVisible = false
            };

            AddSquares();
        }

        /// <summary>
        /// Add squares to canvas
        /// </summary>
        private void AddSquares()
        {
            canvas.Children.Add(topLeft);
            canvas.Children.Add(topRight);
            canvas.Children.Add(bottomLeft);
            canvas.Children.Add(bottomRight);
        }

        /// <summary>
        /// Remove squares from canvas
        /// </summary>
        private void RemoveSquares()
        {
            canvas.Children.Remove(topLeft);
            canvas.Children.Remove(topRight);
            canvas.Children.Remove(bottomLeft);
            canvas.Children.Remove(bottomRight);
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
