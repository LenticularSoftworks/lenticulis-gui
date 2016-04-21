using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using lenticulis_gui.src.SupportLib;
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
            private set
            {
                s_instance = value;
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

        /// <summary>
        /// Stores loaded image holder
        /// </summary>
        /// <param name="id">Image ID</param>
        /// <param name="image">Container instance</param>
        /// <returns></returns>
        public bool storeImageHolder(int id, ImageHolder image)
        {
            if (loadedImages.ContainsKey(id))
                return false;

            loadedImages.Add(id, image);
            return true;
        }

        /// <summary>
        /// Cleans up internal storages
        /// </summary>
        public void cleanUp()
        {
            ImageLoader.unloadAllImages();
            loadedImages.Clear();
            Storage.Instance = null;
        }

        /// <summary>
        /// Unload single image from storage by it's id
        /// </summary>
        /// <param name="id">resource id</param>
        public void unloadImage(int id)
        {
            if (loadedImages.ContainsKey(id))
            {
                ImageLoader.unloadImage(id);
                loadedImages.Remove(id);
            }     
        }
    }
}
