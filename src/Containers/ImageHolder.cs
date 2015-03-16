using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using lenticulis_gui.src.SupportLib;

namespace lenticulis_gui.src.Containers
{
    /// <summary>
    /// Container class for holding information about one image (prototype)
    /// </summary>
    public class ImageHolder
    {
        /// <summary>
        /// Image ID
        /// </summary>
        public int id;

        /// <summary>
        /// Image file name
        /// </summary>
        public String fileName;

        /// <summary>
        /// Image format
        /// </summary>
        public String format;

        /// <summary>
        /// Image total width in pixels
        /// </summary>
        public uint width;

        /// <summary>
        /// Image total height in pixels
        /// </summary>
        public uint height;

        /// <summary>
        /// Image color space
        /// </summary>
        public int colorSpace;

        /// <summary>
        /// Formatted mipmap in Image class instance
        /// </summary>
        public Image mipMapData;

        /// <summary>
        /// Static factory method for building ImageHolder class
        /// </summary>
        /// <param name="path">Path of image to be loaded</param>
        /// <returns>build ImageHolder instance based on input path</returns>
        public static unsafe ImageHolder loadImage(String path)
        {
            ImageHolder h = new ImageHolder();

            // at first, load image (using librarian call)
            void* mipMapTarget;

            //try
            {
                h.id = ImageLoader.loadImage(path, out h.format, out h.colorSpace, out h.width, out h.height, out mipMapTarget);
            }
            //catch (BadImageFormatException ex)
            {
                // TODO: better exception handling, let user know about what's exactly wrong
                return null;
            }

            // then process mipmap into internal class
            int mmSize = SupportLib.SupportLib.getMipmapSize();

            int mmWidth = mmSize * 3/2;
            int mmHeight = mmSize;

            h.fileName = path;
            h.mipMapData = ImageLoader.resolveMipmap((uint*)mipMapTarget, mmWidth, mmHeight);

            return h;
        }
    }
}
