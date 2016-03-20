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
        /// Instance of selected image
        /// </summary>
        private Image image;

        /// <summary>
        /// Rectangular area for mouse hit test
        /// </summary>
        private Rectangle mouseHitRectangle;

        /// <summary>
        /// Drag started flag
        /// </summary>
        private bool buttonDown;

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
            //TODO temp color - for debugging
            mouseHitRectangle.Fill = new SolidColorBrush(Colors.Red);
            mouseHitRectangle.Opacity = 0.5;
            mouseHitRectangle.Stretch = Stretch.Fill;

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
        private void Paint()
        {
            Rect bounds = image.TransformToVisual(canvas).TransformBounds(new Rect(image.RenderSize));

            this.Width = bounds.Width;
            this.Height = bounds.Height;

            mouseHitRectangle.Width = this.Width;
            mouseHitRectangle.Height = this.Height;

            Canvas.SetTop(mouseHitRectangle, bounds.Top);
            Canvas.SetLeft(mouseHitRectangle, bounds.Left);

            Canvas.SetTop(this, bounds.Top);
            Canvas.SetLeft(this, bounds.Left);

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

            //do not update squares if they weren't painted yet
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
            topRight.LayoutTransform = transform;
            bottomLeft.LayoutTransform = transform;
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

            Canvas.SetTop(topLeft, borderTop);
            Canvas.SetLeft(topLeft, borderLeft);

            Canvas.SetTop(topRight, borderTop);
            Canvas.SetLeft(topRight, borderLeft + this.Width - squareScale * initSquareSize);

            Canvas.SetTop(bottomLeft, borderTop + this.Height - squareScale * initSquareSize);
            Canvas.SetLeft(bottomLeft, borderLeft);

            Canvas.SetTop(bottomRight, borderTop + this.Height - squareScale * initSquareSize);
            Canvas.SetLeft(bottomRight, borderLeft + this.Width - squareScale * initSquareSize);
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
        /// Mouse down event for hit test rectangle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseHitRectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(mouseHitRectangle);

            canvas.Image_MouseLeftButtonDown(image, e);
            buttonDown = true;
        }

        /// <summary>
        /// Mouse move event for hit test rectangle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseHitRectangle_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!buttonDown)
                return;

            canvas.Image_MouseMove(image, e);

            Paint();
        }

        /// <summary>
        /// Mouse up event for hit test rectangle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseHitRectangle_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            buttonDown = false;
            canvas.Image_MouseLeftButtonUp(image, e);
        }

        /// <summary>
        /// Sets cursors by selected tool
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageCursor_MouseMove(object sender, MouseEventArgs e)
        {
            // retrieves mouse position
            Point mouse = Mouse.GetPosition(mouseHitRectangle);

            // and set cursor according to selected tool
            switch (MainWindow.SelectedTool)
            {
                case TransformType.Translation:
                    mouseHitRectangle.Cursor = Cursors.SizeAll;
                    break;
                case TransformType.Scale:
                    SetScaleCursor(mouseHitRectangle, mouse);
                    break;
                case TransformType.Rotate:
                    mouseHitRectangle.Cursor = Cursors.Hand;
                    break;
            }
        }

        /// <summary>
        /// Sets cusror for scale by cursor position in image
        /// </summary>
        /// <param name="rect">image</param>
        /// <param name="mouse">cursor point</param>
        private void SetScaleCursor(Rectangle rect, Point mouse)
        {
            double halfWidth = rect.ActualWidth / 2.0;
            double halfHeight = rect.ActualHeight / 2.0;

            //left top / right bottom = NWSE
            if ((mouse.X < halfWidth && mouse.Y < halfHeight) || (mouse.X > halfWidth && mouse.Y > halfHeight))
                rect.Cursor = Cursors.SizeNWSE;
            else
                rect.Cursor = Cursors.SizeNESW;
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
        }

        /// <summary>
        /// Set listeners to mouse hit test rectangle
        /// </summary>
        private void SetListeners()
        {
            mouseHitRectangle.IsHitTestVisible = true;
            mouseHitRectangle.MouseLeftButtonDown += mouseHitRectangle_MouseLeftButtonDown;
            mouseHitRectangle.MouseMove += mouseHitRectangle_MouseMove;
            mouseHitRectangle.MouseMove += ImageCursor_MouseMove;
            mouseHitRectangle.MouseLeftButtonUp += mouseHitRectangle_MouseLeftButtonUp;
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
