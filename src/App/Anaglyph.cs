using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace lenticulis_gui.src.App
{
    /// <summary>
    /// Class provides rendering anaglyph bitmap image from 2 canvases
    /// </summary>
    public static class Anaglyph
    {
        /// <summary>
        /// Red channel color matrix
        /// </summary>
        private static ColorMatrix redChannelMatrix = new ColorMatrix(new float[][] 
                                                {
                                                    new float[]{1, 0, 0, 0, 0},
                                                    new float[]{0, 0, 0, 0, 0},
                                                    new float[]{0, 0, 0, 0, 0},
                                                    new float[]{0, 0, 0, 1, 0},
                                                    new float[]{0, 0, 0, 0, 1}
                                                });

        /// <summary>
        /// Green - Blue channel color matrix
        /// </summary>
        private static ColorMatrix cyanChannelMatrix = new ColorMatrix(new float[][] 
                                                {
                                                    new float[]{0, 0, 0, 0, 0},
                                                    new float[]{0, 1, 0, 0, 0},
                                                    new float[]{0, 0, 1, 0, 0},
                                                    new float[]{0, 0, 0, 1, 0},
                                                    new float[]{0, 0, 0, 0, 1}
                                                });

        /// <summary>
        /// Gray scale color matrix
        /// </summary>
        private static ColorMatrix grayScaleMatrix = new ColorMatrix(new float[][] 
                                                {
                                                    new float[] {.34f, .34f, .34f, 0, 0},
                                                    new float[] {.34f, .34f, .34f, 0, 0},
                                                    new float[] {.34f, .34f, .34f, 0, 0},
                                                    new float[] {0, 0, 0, 1, 0},
                                                    new float[] {0, 0, 0, 0, 1}
                                                });

        /// <summary>
        /// Create instance of Image class with anaglyph image created from canvases
        /// </summary>
        /// <param name="leftCanvas">Left Canvas</param>
        /// <param name="rightCanvas">Right canvas</param>
        /// <param name="grayScale">Color if false, else grayscale</param>
        /// <returns>Anaglyph Image</returns>
        public static System.Windows.Controls.Image GetAnaglyphImage(Canvas leftCanvas, Canvas rightCanvas, bool grayScale)
        {
            //render bitmap from canvases
            Bitmap bmpLeft = RenderBitmapImage(leftCanvas);
            Bitmap bmpRight = RenderBitmapImage(rightCanvas);

            //apply gray scale matrix if grayScale is true
            if (grayScale)
            {
                ApplyColorFilter(bmpLeft, grayScaleMatrix);
                ApplyColorFilter(bmpRight, grayScaleMatrix);
            }

            //apply red channel filter for left and green-blue for right
            ApplyColorFilter(bmpLeft, redChannelMatrix);
            ApplyColorFilter(bmpRight, cyanChannelMatrix);

            //create anaglyph by addition of right pixel channels to left
            AnaglyphFromBitmaps(bmpLeft, bmpRight);

            //convert to image source
            System.Windows.Media.Imaging.BitmapImage bitmapSource = new System.Windows.Media.Imaging.BitmapImage();
            MemoryStream stream = new MemoryStream();

            bmpLeft.Save(stream, ImageFormat.Bmp);
            stream.Position = 0;
            bitmapSource.BeginInit();
            bitmapSource.StreamSource = stream;
            bitmapSource.EndInit();

            //create and return image
            return new System.Windows.Controls.Image() { Source = bitmapSource };
        }

        /// <summary>
        /// Merge left (red) and right (cyan) bitmap images to left bitmap by 
        /// pixel channel additions. Method contains fast pixel acces through unsafe modifier and lockBits.
        /// </summary>
        /// <param name="left">left image (red)</param>
        /// <param name="right">right image (green + blue)</param>
        private static void AnaglyphFromBitmaps(Bitmap left, Bitmap right)
        {
            //unsafe - using pointers for fast access
            unsafe
            {
                //lock bitamps
                BitmapData leftData = left.LockBits(new Rectangle(0, 0, ProjectHolder.Width, ProjectHolder.Height),
                   ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                BitmapData rightData = right.LockBits(new Rectangle(0, 0, ProjectHolder.Width, ProjectHolder.Height),
                   ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                //row pointers
                byte* pixelRowLeft;
                byte* pixelRowRight;

                //color channel position - each pixel: 3 bytes (R, G, B)
                int colorPos;

                for (int y = 0; y < leftData.Height; y++)
                {
                    //actual row positions
                    //Scan0 returns first bitmap pixel
                    pixelRowLeft = (byte*)leftData.Scan0 + (y * leftData.Stride);
                    pixelRowRight = (byte*)rightData.Scan0 + (y * rightData.Stride);

                    for (int x = 0; x < leftData.Width; x++)
                    {
                        colorPos = x * 3; //pixel channel step

                        pixelRowLeft[colorPos] += pixelRowRight[colorPos]; //R
                        pixelRowLeft[colorPos + 1] += pixelRowRight[colorPos + 1]; //G
                        pixelRowLeft[colorPos + 2] += pixelRowRight[colorPos + 2]; //B
                    }
                }

                left.UnlockBits(leftData);
                right.UnlockBits(rightData);
            }
        }

        /// <summary>
        /// Apply color matrix to bitmap image
        /// </summary>
        /// <param name="inputCanvasBmp">Bitmap image</param>
        /// <param name="colorMatrix">Color matrix</param>
        private static void ApplyColorFilter(Bitmap bmp, ColorMatrix colorMatrix)
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                //set color matrix to atributes
                ImageAttributes imgAtr = new ImageAttributes();
                imgAtr.SetColorMatrix(colorMatrix);

                // draw image with applied filter 
                g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, ProjectHolder.Width, ProjectHolder.Height, GraphicsUnit.Pixel, imgAtr);
            }
        }

        /// <summary>
        /// Render canvas as bitmap
        /// </summary>
        /// <param name="canvas">Current canvas</param>
        /// <returns>Rendered bitmap image</returns>
        private static Bitmap RenderBitmapImage(Canvas canvas)
        {
            //create graphics instance from specified handle to a window
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);

            System.Windows.Size measureSize = new System.Windows.Size(ProjectHolder.Width, ProjectHolder.Height);
            canvas.Measure(measureSize);
            canvas.Arrange(new Rect(measureSize));

            //render bitmap
            System.Windows.Media.Imaging.RenderTargetBitmap bmp = new System.Windows.Media.Imaging.RenderTargetBitmap(ProjectHolder.Width, ProjectHolder.Height, g.DpiX, g.DpiY, System.Windows.Media.PixelFormats.Pbgra32);
            bmp.Render(canvas);

            //convert to bitmap
            MemoryStream stream = new MemoryStream();
            System.Windows.Media.Imaging.BitmapEncoder encoder = new System.Windows.Media.Imaging.BmpBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmp));
            encoder.Save(stream);

            return new Bitmap(stream);
        }
    }
}
