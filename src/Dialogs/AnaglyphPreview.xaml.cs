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
            Image anaglyph = GetAnaglyphImage(leftCanvas, rightCanvas, grayScale);
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
        /// Create instance of Image class with anaglyph image created from canvases
        /// </summary>
        /// <param name="leftCanvas">Left Canvas</param>
        /// <param name="rightCanvas">Right canvas</param>
        /// <param name="grayScale">Color if false, else grayscale</param>
        /// <returns>Anaglyph Image</returns>
        private Image GetAnaglyphImage(Canvas leftCanvas, Canvas rightCanvas, bool grayScale)
        {
            System.Drawing.Bitmap bmp = RenderFilteredBitmap(leftCanvas, rightCanvas, grayScale);

            //convert to image source
            BitmapImage bitmapSource = new BitmapImage();
            MemoryStream stream = new MemoryStream();

            bmp.Save(stream, ImageFormat.Bmp);
            stream.Position = 0;
            bitmapSource.BeginInit();
            bitmapSource.StreamSource = stream;
            bitmapSource.EndInit();

            //create and return image
            return new Image() { Source = bitmapSource };
        }

        /// <summary>
        /// Renders anaglyph image as bitmap.
        /// </summary>
        /// <param name="leftCanvas">Left canvas</param>
        /// <param name="rightCanvas">Right canvas</param>
        /// <param name="grayScale">Color if false, else grayscale</param>
        /// <returns>Bitmap anaglyph image</returns>
        private System.Drawing.Bitmap RenderFilteredBitmap(Canvas leftCanvas, Canvas rightCanvas, bool grayScale)
        {
            //left and right image as bitmap
            System.Drawing.Bitmap bmpLeft = RenderBitmapImage(leftCanvas);
            System.Drawing.Bitmap bmpRight = RenderBitmapImage(rightCanvas);

            if(grayScale) 
            {
                ConvertToGrayScale(ref bmpLeft);
                ConvertToGrayScale(ref bmpRight);
            }

            //anaglyph image
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ProjectHolder.Width, ProjectHolder.Height);

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //get pixel of right and left image on same position
                    System.Drawing.Color leftPx = bmpLeft.GetPixel(i, j);
                    System.Drawing.Color rightPx = bmpRight.GetPixel(i, j);

                    //set pixel to anaglyph image with left red channel and right green and blue channels
                    bmp.SetPixel(i, j, System.Drawing.Color.FromArgb(leftPx.R, rightPx.G, rightPx.B));
                }
            }

            return bmp;
        }

        /// <summary>
        /// Convert referenced color bitmap to gray scaly bitmap
        /// </summary>
        /// <param name="bitmap">Color bitmap</param>
        private void ConvertToGrayScale(ref System.Drawing.Bitmap bitmap) 
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    System.Drawing.Color px = bitmap.GetPixel(i, j);
                    
                    //set all channels gray value as avrage value
                    int gray = (int)((px.R + px.G + px.B) / 3.0);
                    bitmap.SetPixel(i, j, System.Drawing.Color.FromArgb(gray, gray, gray));
                }
            }
        }

        /// <summary>
        /// Render canvas as bitmap and returns as Image instance
        /// </summary>
        /// <param name="canvas">Current canvas</param>
        /// <returns>Rendered bitmap image</returns>
        private System.Drawing.Bitmap RenderBitmapImage(Canvas canvas)
        {
            //create graphics instance from specified handle to a window
            System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);

            Size measureSize = new Size(ProjectHolder.Width, ProjectHolder.Height);
            canvas.Measure(measureSize);
            canvas.Arrange(new Rect(measureSize));

            //render bitmap
            RenderTargetBitmap bmp = new RenderTargetBitmap(ProjectHolder.Width, ProjectHolder.Height, g.DpiX, g.DpiY, PixelFormats.Pbgra32);
            bmp.Render(canvas);

            //convert to bitmap
            MemoryStream stream = new MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(stream);

            return new System.Drawing.Bitmap(stream);
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
