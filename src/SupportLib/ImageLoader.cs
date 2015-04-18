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
using lenticulis_gui.src.Containers;
using lenticulis_gui.src.SupportLib;

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
            void* mmdata;
            int id = SupportLib.registerImageP(Utils.getCString(filename), formatTarget, out colorSpaceTarget, out widthTarget, out heightTarget, out mmdata);

            format = formatTarget.ToString();
            colorSpace = colorSpaceTarget.ToInt32();
            width = widthTarget.ToUInt32();
            height = heightTarget.ToUInt32();

            mipmapData = mmdata;

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

            return bmp as Image;
        }

        /// <summary>
        /// Parse mipmap from supplied imageholder into ImageSource array reusable later
        /// </summary>
        /// <param name="holder">ImageHolder holding image properties and mipmap data</param>
        /// <returns>array of ImageSource instances holding parsed mipmap images</returns>
        public static System.Windows.Media.ImageSource[] parseMipmap(ImageHolder holder)
        {
            // get mipmap size to calculate the rest
            uint mmSize = (uint) SupportLib.getMipmapSize();

            List<System.Windows.Media.ImageSource> retList = new List<System.Windows.Media.ImageSource>();

            // retrieve image as bitmap instance and extract data pointer
            Bitmap bmp = holder.mipMapData as Bitmap;
            IntPtr hBitmap = bmp.GetHbitmap();

            // size and position variables
            uint size_x, size_y;
            uint pos_x, pos_y;

            // the formula will change depending on image dimensions
            if (holder.width > holder.height)
            {
                size_x = mmSize;
                size_y = (uint) (mmSize * (float)holder.height / (float)holder.width);
            }
            else
            {
                size_x = (uint)(mmSize * (float)holder.width / (float)holder.height);
                size_y = mmSize;
            }

            // we always start at position 0;0
            pos_x = 0;
            pos_y = 0;

            // at first pass, the recursive formula changes
            bool first = true;

            // while there's something to parse
            while (size_x > 0 && size_y > 0)
            {
                // create imagesource from current viewport
                System.Windows.Media.ImageSource part = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, new System.Windows.Int32Rect((int)pos_x, (int)pos_y, (int)size_x, (int)size_y), System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                // position change - note that on first pass, we change only X, and on every other pass we change only Y
                pos_x += first ? size_x : 0;
                pos_y += first ? 0 : size_y;
                first = false;

                // size of next mipmap element is half of current
                size_x /= 2;
                size_y /= 2;

                retList.Add(part);
            }

            return retList.ToArray();
        }
    }
}
