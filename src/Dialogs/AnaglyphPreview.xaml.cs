using lenticulis_gui.src.App;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace lenticulis_gui.src.Dialogs
{
    /// <summary>
    /// Interaction logic for AnaglyphPreview.xaml
    /// </summary>
    public partial class AnaglyphPreview : Window
    {
        /// <summary>
        /// Size of rendered image
        /// </summary>
        private Size imageSize;

        /// <summary>
        /// Canvas idnent relaive to screen
        /// </summary>
        private const int canvasIndent = 90;

        /// <summary>
        /// Creates modal window with anaglyph preview
        /// </summary>
        /// <param name="leftCanvas">Left image canvas</param>
        /// <param name="rightCanvas">Right image canvas</param>
        /// <param name="grayScale">Show gray scale anaglyph if true, else color</param>
        public AnaglyphPreview(Canvas leftCanvas, Canvas rightCanvas, bool grayScale)
        {
            InitializeComponent();

            LoadingWindow lw = new LoadingWindow("anaglyph");
            lw.Show();

            //canvas width to screen resolution 
            AnaglyphCanvas.Width = SystemParameters.PrimaryScreenWidth - canvasIndent;
            AnaglyphCanvas.Height = SystemParameters.PrimaryScreenHeight - canvasIndent;
            Canvas.SetTop(AnaglyphCanvas, SystemParameters.PrimaryScreenHeight / 2.0 - AnaglyphCanvas.Height / 2.0);
            Canvas.SetLeft(AnaglyphCanvas, SystemParameters.PrimaryScreenWidth / 2.0 - AnaglyphCanvas.Width / 2.0);

            //set size
            imageSize = CalculateImageSize();

            //add to preview window
            Image anaglyph = Anaglyph.RenderAnaglyphImage(leftCanvas, rightCanvas, grayScale);
            anaglyph.Width = imageSize.Width;
            anaglyph.Height = imageSize.Height;
            Canvas.SetTop(anaglyph, AnaglyphCanvas.Height / 2.0 - imageSize.Height / 2.0);
            Canvas.SetLeft(anaglyph, AnaglyphCanvas.Width / 2.0 - imageSize.Width / 2.0);

            AnaglyphCanvas.Children.Add(anaglyph);

            lw.Close();
            //show as modal window
            this.ShowDialog();
        }

        /// <summary>
        /// Calculates size of new rednered image by window size.
        /// </summary>
        /// <returns>Size of image for rendering</returns>
        private Size CalculateImageSize()
        {
            double width, height;

            //if original width and height is less than anaglyph window
            if (ProjectHolder.Width < AnaglyphCanvas.Width && ProjectHolder.Height < AnaglyphCanvas.Height)
                return new Size(ProjectHolder.Width, ProjectHolder.Height);

            //aspect ratio
            double ratio = ProjectHolder.Width / (double)ProjectHolder.Height;

            //width of parent canvas
            width = AnaglyphCanvas.Width;
            height = AnaglyphCanvas.Height;

            if (ratio >= 1)
            {
                height = width / ratio;
                if (height > AnaglyphCanvas.Height)
                {
                    width /= height / AnaglyphCanvas.Height;
                    height = AnaglyphCanvas.Height;
                }
            }
            else
                width = height * ratio;

            return new Size(width, height);
        }

        /// <summary>
        /// Close window key down action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }
    }
}
