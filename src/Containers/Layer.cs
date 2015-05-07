using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.Containers
{
    /// <summary>
    /// Container class for Layer - contains elements and wraps several atributes
    /// </summary>
    public class Layer
    {
        /// <summary>
        /// Layer ID - typically the position (0, 1, ..)
        /// </summary>
        private int id;

        /// <summary>
        /// List of objects placed within this layer
        /// </summary>
        private List<LayerObject> objects = new List<LayerObject>();

        /// <summary>
        /// Constructor retaining the ID (or position, if you want)
        /// </summary>
        /// <param name="id"></param>
        public Layer(int id)
        {
            this.id = id;
        }

        /// <summary>
        /// Retrieves layer ID (position)
        /// </summary>
        /// <returns>ID of this layer</returns>
        public int getId()
        {
            return id;
        }

        /// <summary>
        /// Adds layer object to this layer
        /// </summary>
        /// <param name="obj">Object to be inserted into this layer</param>
        public void addLayerObject(LayerObject obj)
        {
            objects.Add(obj);
        }

        /// <summary>
        /// Increment layer id (position) when adding new layer at first position
        /// </summary>
        public void incrementLayerId()
        {
            this.id++;
        }

        /// <summary>
        /// Decrement layer id (position) when layer move
        /// </summary>
        public void decrementLayerId()
        {
            this.id--;
        }

        /// <summary>
        /// Removes layer object from layer, regardless of its presence
        /// </summary>
        /// <param name="obj">Object to be removed from this layer</param>
        public void removeLayerObject(LayerObject obj)
        {
            objects.Remove(obj);
        }

        /// <summary>
        /// Retrieves layer objects placet within this layer - typically needed for saving project to file
        /// </summary>
        /// <returns>List of layer objects within this layer</returns>
        public List<LayerObject> getLayerObjects()
        {
            return objects;
        }
    }
}
