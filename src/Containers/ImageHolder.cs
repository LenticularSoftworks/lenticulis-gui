using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Media;
using lenticulis_gui.src.App;
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
        /// Index of PSD layer
        /// </summary>
        public int psdLayerIndex;

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
        /// <param name="reportError">when true, the messagebox will apear on error</param>
        /// <param name="psdLayerIdentifier">if we are about to load specific layer from PSD, this is >= 0</param>
        /// <returns>build ImageHolder instance based on input path</returns>
        public static unsafe ImageHolder loadImage(String path, bool reportError = true, int psdLayerIdentifier = -1)
        {
            String origPath = path;
            if (psdLayerIdentifier > -1)
                path = path + "["+psdLayerIdentifier+"]";

            // image already loaded, return the loaded one
            ImageHolder tmp = Storage.Instance.getImageHolder(path);
            if (tmp != null)
                return tmp;

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
                System.Windows.MessageBox.Show(LangProvider.getString("IML_UNEXPECTED_ERROR") + ex.Message, LangProvider.getString("IMAGE_LOAD_ERROR"), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }

            // negative return value means error
            if (tmpId < 0)
            {
                if (reportError)
                {
                    switch (tmpId)
                    {
                        case ImageLoader.LOADER_ERROR_IMAGE_NOT_FOUND:
                            System.Windows.MessageBox.Show(LangProvider.getString("IML_IMAGE_NOT_FOUND"), LangProvider.getString("IMAGE_LOAD_ERROR"), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            break;
                        case ImageLoader.LOADER_ERROR_IMAGE_CORRUPTED:
                            System.Windows.MessageBox.Show(LangProvider.getString("IML_IMAGE_CORRUPTED"), LangProvider.getString("IMAGE_LOAD_ERROR"), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            break;
                        case ImageLoader.LOADER_ERROR_IMAGE_DEPTH_UNSUPPORTED:
                            System.Windows.MessageBox.Show(LangProvider.getString("IML_UNSUPPORTED_DEPTH"), LangProvider.getString("IMAGE_LOAD_ERROR"), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            break;
                        case ImageLoader.LOADER_ERROR_IMAGE_FORMAT_UNSUPPORTED:
                            System.Windows.MessageBox.Show(LangProvider.getString("IML_UNSUPPORTED_FORMAT"), LangProvider.getString("IMAGE_LOAD_ERROR"), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            break;
                    }
                }
                return null;
            }

            h.id = tmpId;

            // then process mipmap into internal class
            int mmSize = SupportLib.SupportLib.getMipmapSize();

            int mmWidth = mmSize * 3/2;
            int mmHeight = mmSize;

            h.psdLayerIndex = psdLayerIdentifier;
            h.fileName = origPath;
            h.mipMapData = ImageLoader.resolveMipmap((uint*)mipMapTarget, mmWidth, mmHeight);
            h.imageThumbnails = ImageLoader.parseMipmap(h);

            // store image for possible later reuse
            Storage.Instance.storeImageHolder(h.id, h);

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
