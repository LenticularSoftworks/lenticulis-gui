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

        // Section for layer object properties

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

        // Section for canvas object properties

        /// <summary>
        /// Initial X position of element on canvas
        /// </summary>
        public float InitialX { get; set; }
        /// <summary>
        /// Initial Y position of element on canvas
        /// </summary>
        public float InitialY { get; set; }
        /// <summary>
        /// Initial angle of element on canvas
        /// </summary>
        public float InitialAngle { get; set; }
        /// <summary>
        /// Initial X scale of element on canvas
        /// </summary>
        public float InitialScaleX { get; set; }
        /// <summary>
        /// Initial Y scale of element on canvas
        /// </summary>
        public float InitialScaleY { get; set; }

        /// <summary>
        /// Dictionary of transformations done on this object during its lifetime
        /// </summary>
        private Dictionary<TransformType, Transformation> Transformations;

        /// <summary>
        /// Constructor - only sets ID of this object by generating a new one from static member,
        /// inits initial state of position, rotation and scale, and creates transformation dictionary
        /// </summary>
        public LayerObject()
        {
            Id = lobj_guid_high++;

            resetInitialState();

            initTransformationDict();
        }

        /// <summary>
        /// Destroys any references to this object in layer, etc.
        /// </summary>
        public void dispose()
        {
            if (parentLayer != null)
            {
                parentLayer.removeLayerObject(this);
                parentLayer = null;
            }
        }

        /// <summary>
        /// Init transformation dictionary; prefill with nulls
        /// </summary>
        private void initTransformationDict()
        {
            Transformations = new Dictionary<TransformType, Transformation>();

            var values = Enum.GetValues(typeof(TransformType));
            foreach (TransformType tr in values)
                Transformations.Add(tr, null);
        }

        /// <summary>
        /// Resets object to initial state (position, rotation and scale)
        /// </summary>
        private void resetInitialState()
        {
            InitialX = 0;
            InitialY = 0;
            InitialAngle = 0;
            InitialScaleX = 1.0f;
            InitialScaleY = 1.0f;
        }

        /// <summary>
        /// Returns true, if there is any transformation present
        /// </summary>
        /// <returns>Is there any transformation?</returns>
        public bool hasTransformations()
        {
            foreach (KeyValuePair<TransformType, Transformation> kvp in Transformations)
            {
                if (kvp.Value != null)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves transformation using its type
        /// </summary>
        /// <param name="type">Type of transformation</param>
        /// <returns>Transformation instance or null</returns>
        public Transformation getTransformation(TransformType type)
        {
            return Transformations[type];
        }

        /// <summary>
        /// Sets transformation to its slot
        /// </summary>
        /// <param name="transformation">Transformation to be applied</param>
        public void setTransformation(Transformation transformation)
        {
            Transformations[transformation.Type] = transformation;
        }
    }
}
