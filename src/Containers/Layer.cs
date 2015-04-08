using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.Containers
{
    public class Layer
    {
        private int id;
        private List<LayerObject> objects = new List<LayerObject>();

        public Layer(int id)
        {
            this.id = id;
        }

        public int getId()
        {
            return id;
        }

        public void addLayerObject(LayerObject obj)
        {
            objects.Add(obj);
        }

        public void removeLayerObject(LayerObject obj)
        {
            objects.Remove(obj);
        }

        public List<LayerObject> getLayerObjects()
        {
            return objects;
        }
    }
}
