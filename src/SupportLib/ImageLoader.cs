using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using lenticulis_gui.src.App;

namespace lenticulis_gui.src.SupportLib
{
    static class ImageLoader
    {
        /// <summary>
        /// Encapsulates librarian call for loading image
        /// </summary>
        /// <param name="filename">Path of image to be loaded</param>
        /// <param name="format">Image format (output param)</param>
        /// <param name="colorSpace">Image color format (output param)</param>
        /// <param name="width">Image width in pixels (output param)</param>
        /// <param name="height">Image height in pixels (output param)</param>
        /// <param name="mipmapData">pointer to mipmap data (output param)</param>
        /// <returns>ID of loaded image</returns>
        public static unsafe int loadImage(String filename, out String format, out int colorSpace, out uint width, out uint height, out void* mipmapData)
        {
            StringBuilder formatTarget = new StringBuilder(4092);
            IntPtr colorSpaceTarget;
            UIntPtr widthTarget, heightTarget;
            int id = SupportLib.registerImageP(Utils.getCString(filename), formatTarget, out colorSpaceTarget, out widthTarget, out heightTarget, out mipmapData);

            format = formatTarget.ToString();
            colorSpace = colorSpaceTarget.ToInt32();
            width = widthTarget.ToUInt32();
            height = heightTarget.ToUInt32();

            return id;
        }

        /// <summary>
        /// Resolves raw mipmap data to Image class instance
        /// </summary>
        /// <param name="data">Pointer to mipmap data</param>
        /// <param name="width">Mipmap total width in pixels</param>
        /// <param name="height">Mipmap total height in pixels</param>
        /// <returns>Image instance built from raw data</returns>
        public static unsafe Image resolveMipmap(uint* data, int width, int height)
        {
            int i = 0;

            // create bitmap, we will manipulate with pixels
            Bitmap bmp = new Bitmap(width, height);
            // lock whole region for us
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            // init pointer
            int* tmp_ptr = (int*)bmpData.Scan0;

            uint b;
            while (i < width * height)
            {
                b = data[i];
                tmp_ptr[i++] = Utils.convertColor(b);
            }
            // this propagates all changes done to Bitmap object
            bmp.UnlockBits(bmpData);

            return (Image)bmp;
        }
    }
}
