using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using lenticulis_gui.src.App;
using System.Windows;
using System.Drawing;
using System.Windows.Media;

namespace lenticulis_gui
{
    class WorkCanvas : Canvas
    {
        public int imageID { get; set; }

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
                this.RenderTransform = new ScaleTransform(canvasScaleCached, canvasScaleCached);
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

            this.RenderTransform = new ScaleTransform(canvasScaleCached, canvasScaleCached);

            //for testing purposes only
            this.Children.Add(new Label()
            {
                Content = imageID + "( + 1)",
                FontSize = 25
            });
        }
    }
}

