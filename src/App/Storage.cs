using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using lenticulis_gui.src.Containers;

namespace lenticulis_gui.src.App
{
    /// <summary>
    /// Singleton class for storing all loaded data
    /// </summary>
    public class Storage
    {
        /// <summary>
        /// The only one singleton instance
        /// </summary>
        private static Storage s_instance;

        /// <summary>
        /// Static instance property
        /// </summary>
        public static Storage Instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = new Storage();
                return s_instance;
            }
        }

        /// <summary>
        /// List of loaded images
        /// </summary>
        private Dictionary<int, ImageHolder> loadedImages = new Dictionary<int,ImageHolder>();

        /// <summary>
        /// Retrieves ImageHolder instance of image with specified ID
        /// </summary>
        /// <param name="id">ID of image</param>
        /// <returns>ImageHolder instance or null if not found</returns>
        public ImageHolder getImageHolder(int id)
        {
            if (loadedImages.ContainsKey(id))
                return loadedImages[id];
            else
                return null;
        }

        /// <summary>
        /// Retrieves ImageHolder instance of image with specified filename
        /// </summary>
        /// <param name="filename">Image filename</param>
        /// <returns>ImageHolder instance or null if not found</returns>
        public ImageHolder getImageHolder(String filename)
        {
            foreach (KeyValuePair<int, ImageHolder> imgholder in loadedImages)
            {
                if (imgholder.Value.fileName.Equals(filename))
                    return imgholder.Value;
            }

            return null;
        }
    }
}
