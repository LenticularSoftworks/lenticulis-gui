using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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
        /// Class constructor creates border (bounding box) and adds to canvas.
        /// </summary>
        /// <param name="canvas">Canvas</param>
        public BoundingBox(Canvas canvas)
        {
            this.canvas = canvas;
            this.BorderThickness = SetThickness();
            this.BorderBrush = CreateBrush();

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
        }

        /// <summary>
        /// Adjust the border thickness to canvas width
        /// </summary>
        /// <returns>Thickness</returns>
        private Thickness SetThickness()
        {
            int value = 1;
            double canvasWidth = canvas.Width * ((WorkCanvas)canvas).CanvasScale;

            //value = 1 for width <= 1000, value = 2 for width <= 2000 ...
            value = (int)Math.Ceiling(canvasWidth / 1000);

            return new Thickness(value);
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
