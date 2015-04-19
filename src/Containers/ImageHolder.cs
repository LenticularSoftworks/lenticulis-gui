using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Media;
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
        /// Parsed mipmap into array of ImageSource instances to be used in frontend (on canvas)
        /// </summary>
        private ImageSource[] imageThumbnails;

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

            int tmpId = -1;

            try
            {
                tmpId = ImageLoader.loadImage(path, out h.format, out h.colorSpace, out h.width, out h.height, out mipMapTarget);
            }
            catch (Exception ex)
            {
                // This should never happen - we don't throw exceptions there
                System.Windows.MessageBox.Show("Byla vyvolána neočekávaná výjimka: "+ex.Message, "Došlo k chybě", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }

            if (tmpId < 0)
            {
                switch (tmpId)
                {
                    case ImageLoader.LOADER_ERROR_IMAGE_NOT_FOUND:
                        System.Windows.MessageBox.Show("Obrázek nebyl nalezen", "Došlo k chybě", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        break;
                    case ImageLoader.LOADER_ERROR_IMAGE_CORRUPTED:
                        System.Windows.MessageBox.Show("Soubor obrázku je pravděpodobně poškozen", "Došlo k chybě", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        break;
                    case ImageLoader.LOADER_ERROR_IMAGE_DEPTH_UNSUPPORTED:
                        System.Windows.MessageBox.Show("Nepodporovaná barevná hloubka obrázku", "Došlo k chybě", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        break;
                    case ImageLoader.LOADER_ERROR_IMAGE_FORMAT_UNSUPPORTED:
                        System.Windows.MessageBox.Show("Nepodporovaný formát obrázku", "Došlo k chybě", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        break;
                }
                return null;
            }

            h.id = tmpId;

            // then process mipmap into internal class
            int mmSize = SupportLib.SupportLib.getMipmapSize();

            int mmWidth = mmSize * 3/2;
            int mmHeight = mmSize;

            h.fileName = path;
            h.mipMapData = ImageLoader.resolveMipmap((uint*)mipMapTarget, mmWidth, mmHeight);
            h.imageThumbnails = ImageLoader.parseMipmap(h);

            return h;
        }

        /// <summary>
        /// Retrieves image thumbnail for desired dimensions
        /// </summary>
        /// <param name="width">width of visible image</param>
        /// <param name="height">height of visible image</param>
        /// <returns>the nearest larger image thumbnail available</returns>
        public ImageSource getImageForSize(int width, int height)
        {
            // not yet loaded, or invalid image
            if (imageThumbnails == null || imageThumbnails.Length == 0)
                return null;

            // iterate from second element, and store first as "last visited" element
            ImageSource last = imageThumbnails[0];
            ImageSource current;
            for (int i = 1; i < imageThumbnails.Length; i++)
            {
                // just save current
                current = imageThumbnails[i];

                // if the current is smaller than needed image, return the last visited
                if (current.Width < width && current.Height < height)
                    break;

                // otherwise set current as last visited and iterate again
                last = current;
            }

            return last;
        }
    }
}
