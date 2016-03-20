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

namespace lenticulis_gui.src.App
{
    /// <summary>
    /// Class provides rendering anaglyph bitmap image from 2 canvases
    /// </summary>
    public static class Anaglyph
    {
        /// <summary>
        /// Create instance of Image class with anaglyph image created from canvases
        /// </summary>
        /// <param name="leftCanvas">Left Canvas</param>
        /// <param name="rightCanvas">Right canvas</param>
        /// <param name="grayScale">Color if false, else grayscale</param>
        /// <returns>Anaglyph Image</returns>
        public static Image RenderAnaglyphImage(Canvas leftCanvas, Canvas rightCanvas, bool grayScale)
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
        private static System.Drawing.Bitmap RenderFilteredBitmap(Canvas leftCanvas, Canvas rightCanvas, bool grayScale)
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
        private static void ConvertToGrayScale(ref System.Drawing.Bitmap bitmap) 
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
        private static System.Drawing.Bitmap RenderBitmapImage(Canvas canvas)
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
    }
}
