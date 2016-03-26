using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.Containers
{
    /// <summary>
    /// Container class for Layer - contains elements and wraps several atributes
    /// </summary>
    public class Layer : IHistoryStorable<LayerHistory>
    {
        /// <summary>
        /// Layer ID - typically the position (0, 1, ..)
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Depth of layer in inches
        /// </summary>
        public double Depth { get; set; }

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
            this.Id = id;
            this.Depth = 0.0;
        }

        /// <summary>
        /// Adds layer object to this layer
        /// </summary>
        /// <param name="obj">Object to be inserted into this layer</param>
        public void AddLayerObject(LayerObject obj)
        {
            objects.Add(obj);
        }

        /// <summary>
        /// Increment layer id (position) when adding new layer at first position
        /// </summary>
        public void IncrementLayerId()
        {
            this.Id++;
        }

        /// <summary>
        /// Decrement layer id (position) when layer move
        /// </summary>
        public void DecrementLayerId()
        {
            this.Id--;
        }

        /// <summary>
        /// Removes layer object from layer, regardless of its presence
        /// </summary>
        /// <param name="obj">Object to be removed from this layer</param>
        public void RemoveLayerObject(LayerObject obj)
        {
            objects.Remove(obj);
        }

        /// <summary>
        /// Retrieves layer objects placet within this layer - typically needed for saving project to file
        /// </summary>
        /// <returns>List of layer objects within this layer</returns>
        public List<LayerObject> GetLayerObjects()
        {
            return objects;
        }

        /// <summary>
        /// Return history item
        /// </summary>
        /// <returns>Layer history item</returns>
        public LayerHistory GetHistoryItem()
        {
            return new LayerHistory()
            {
                LayerId = this.Id,
                UpLayer = false,
                DownLayer = false,
                AddLayer = false,
                RemoveLayer = false
            };
        }
    }
}
