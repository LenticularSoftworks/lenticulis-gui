using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using lenticulis_gui.src.App;

namespace lenticulis_gui.src.Containers
{
    /// <summary>
    /// Container class for object placed into layer
    /// </summary>
    public class LayerObject
    {
        /// <summary>
        /// Next ID to be assigned to LayerObject instance - unique within project
        /// </summary>
        private static int lobj_guid_high = 1;

        /// <summary>
        /// Parent layer of this object
        /// </summary>
        private Layer parentLayer = null;

        /// <summary>
        /// Object ID (generated as unique value from lobj_guid_high)
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Resource ID this objects is instance of
        /// </summary>
        public int ResourceId { get; set; }
        /// <summary>
        /// Layer ID of this object - needs reassignment to another layer even by reference after changing
        /// this property
        /// </summary>
        public int Layer
        {
            get
            {
                if (parentLayer != null)
                    return parentLayer.getId();
                return 0;
            }
            set
            {
                // this would mean invalid layer - do not set anything
                if (value >= ProjectHolder.layers.Count)
                    return;

                // if this object belongs to some layer, remove it from here
                if (parentLayer != null)
                    parentLayer.removeLayerObject(this);

                // set new layer to this object
                parentLayer = ProjectHolder.layers[value];

                // and add self to its list
                parentLayer.addLayerObject(this);
            }
        }
        /// <summary>
        /// Starting frame of this layer object
        /// </summary>
        public int Column { get; set; }
        /// <summary>
        /// Number of frames this objects occupies
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// Visibility flag
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Constructor - only sets ID of this object by generating a new 
        /// </summary>
        public LayerObject()
        {
            Id = lobj_guid_high++;
        }
    }
}
