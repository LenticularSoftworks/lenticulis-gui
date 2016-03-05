using lenticulis_gui.src.App;
using lenticulis_gui.src.Dialogs;
using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace lenticulis_gui.src.Dialogs
{
    /// <summary>
    /// Interaction logic for AnaglyphPreview.xaml
    /// </summary>
    public partial class AnaglyphPreview : MetroWindow
    {
        /// <summary>
        /// Size of rendered image
        /// </summary>
        private Size imageSize;

        /// <summary>
        /// Creates modal window with anaglyph preview
        /// </summary>
        /// <param name="leftCanvas">Left image canvas</param>
        /// <param name="rightCanvas">Right image canvas</param>
        public AnaglyphPreview(Canvas leftCanvas, Canvas rightCanvas)
        {
            InitializeComponent();

            LoadingWindow lw = new LoadingWindow("anaglyph");
            lw.Show();

            //set size
            imageSize = CalculateImageSize(leftCanvas);
            //add to preview window
            AnaglyphCanvas.Children.Add(GetFilteredImage(leftCanvas, rightCanvas));


            lw.Close();
            //show as modal window
            this.ShowDialog();
        }

        /// <summary>
        /// Create instance of Image class with anaglyph image created from canvases
        /// </summary>
        /// <param name="leftCanvas">Left Canvas</param>
        /// <param name="rightCanvas">Right canvas</param>
        /// <returns>Anaglyph Image</returns>
        private Image GetFilteredImage(Canvas leftCanvas, Canvas rightCanvas)
        {
            System.Drawing.Bitmap bmp = RenderFilteredBitmap(leftCanvas, rightCanvas);

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
        /// <returns>Bitmap anaglyph image</returns>
        private System.Drawing.Bitmap RenderFilteredBitmap(Canvas leftCanvas, Canvas rightCanvas)
        {
            //left and right image as bitmap
            System.Drawing.Bitmap bmpLeft = RenderBitmapImage(leftCanvas);
            System.Drawing.Bitmap bmpRight = RenderBitmapImage(rightCanvas);

            //anaglyph image
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap((int)imageSize.Width, (int)imageSize.Height);

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
        /// Render canvas as bitmap and returns as Image instance
        /// </summary>
        /// <param name="canvas">Current canvas</param>
        /// <returns>Rendered bitmap image</returns>
        private System.Drawing.Bitmap RenderBitmapImage(Canvas canvas)
        {
            //create graphics instance from specified handle to a window
            System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);

            canvas.Measure(imageSize);
            canvas.Arrange(new Rect(imageSize));

            //render bitmap
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)imageSize.Width, (int)imageSize.Height, g.DpiX, g.DpiY, PixelFormats.Pbgra32);
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
        /// <param name="canvas">Current canvas</param>
        /// <returns>Size of image for rendering</returns>
        private Size CalculateImageSize(Canvas canvas)
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

            if (canvas.Width > canvas.Height)
                height = width / ratio;
            else
                width = height * ratio;

            return new Size(width, height);
        }
    }
}
