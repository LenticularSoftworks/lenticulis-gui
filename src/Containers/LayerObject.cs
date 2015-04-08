using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using lenticulis_gui.src.App;

namespace lenticulis_gui.src.Containers
{
    public class LayerObject
    {
        private static int lobj_guid_high = 1;

        private Layer parentLayer = null;

        public int Id { get; set; }
        public int ResourceId { get; set; }
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
                if (value >= ProjectHolder.layers.Count)
                    return;

                if (parentLayer != null)
                    parentLayer.removeLayerObject(this);

                parentLayer = ProjectHolder.layers[value];

                parentLayer.addLayerObject(this);
            }
        }
        public int Column { get; set; }
        public int Length { get; set; }
        public bool Visible { get; set; }

        public LayerObject()
        {
            Id = lobj_guid_high++;
        }
    }
}
