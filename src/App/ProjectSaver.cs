using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using lenticulis_gui.src.Containers;

namespace lenticulis_gui.src.App
{
    public static class ProjectSaver
    {
        /// <summary>
        /// Save project using previously stored project file name (the project was saved in the past)
        /// </summary>
        public static void saveProject()
        {
            saveProject(ProjectHolder.ProjectFileName);
        }

        /// <summary>
        /// Save project to specified file; this method overwrites file at supplied path, validation
        /// must be done before call.
        /// </summary>
        /// <param name="filename">Path to file (absolute or relative) to serve as project save file</param>
        public static void saveProject(String filename)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            XmlWriter xw = XmlWriter.Create(filename, settings);
            xw.WriteStartDocument();

            xw.WriteStartElement("project");
            {
                xw.WriteAttributeString("name", ProjectHolder.ProjectName);
                // TODO: move version somewhere globally
                xw.WriteAttributeString("lenticulis-version", "1.0");

                // write out project properties
                writeProperties(xw);

                // write out resources
                writeResources(xw);

                // write out objects
                writeObjects(xw);

                // write out layers
                writeLayers(xw);
            }
            xw.WriteEndElement();

            xw.WriteEndDocument();
            xw.Flush();
        }

        /// <summary>
        /// Writes project properties using supplied XmlWriter
        /// </summary>
        /// <param name="xw">XmlWriter instance in needed state</param>
        private static void writeProperties(XmlWriter xw)
        {
            xw.WriteStartElement("properties");
            {
                // count of frames
                writeProperty(xw, "frame-count", ProjectHolder.ImageCount.ToString());
                // count of layers
                writeProperty(xw, "layer-count", ProjectHolder.LayerCount.ToString());
            }
            xw.WriteEndElement();
        }

        /// <summary>
        /// Writes single property to XmlWriter with prepared state
        /// </summary>
        /// <param name="xw">XmlWriter in needed state</param>
        /// <param name="name">Property name</param>
        /// <param name="value">Property value</param>
        private static void writeProperty(XmlWriter xw, String name, String value)
        {
            xw.WriteStartElement("property");
            {
                xw.WriteAttributeString("name", name);
                xw.WriteAttributeString("value", value);
            }
            xw.WriteEndElement();
        }

        /// <summary>
        /// Writes resource element into prepared XmlWriter
        /// </summary>
        /// <param name="xw">XmlWriter in needed state</param>
        private static void writeResources(XmlWriter xw)
        {
            xw.WriteStartElement("resources");
            {
                // TODO
            }
            xw.WriteEndElement();
        }

        /// <summary>
        /// Writes single resource element to XmlWriter with prepared state
        /// </summary>
        /// <param name="xw">XmlWriter in needed state</param>
        /// <param name="id">resource ID</param>
        /// <param name="type">type of resource</param>
        /// <param name="path">path to resource (relative or absolute)</param>
        /// <param name="psdlayer">PSD layer identifier, or null for other formats</param>
        private static void writeResource(XmlWriter xw, String id, String type, String path, String psdlayer = null)
        {
            xw.WriteStartElement("resource");
            {
                xw.WriteAttributeString("id", id);
                xw.WriteAttributeString("type", type);
                xw.WriteAttributeString("path", path);
                if (psdlayer != null)
                    xw.WriteAttributeString("psd-layer", psdlayer);
            }
            xw.WriteEndElement();
        }

        /// <summary>
        /// Writes objects element into prepared XmlWriter
        /// </summary>
        /// <param name="xw">XmlWriter in needed state</param>
        private static void writeObjects(XmlWriter xw)
        {
            xw.WriteStartElement("objects");
            {
                for (int i = 0; i < ProjectHolder.layers.Count; i++)
                {
                    List<LayerObject> objects = ProjectHolder.layers[i].getLayerObjects();
                    for (int j = 0; j < objects.Count; j++)
                        writeObject(xw, objects[j].Id.ToString(), objects[j].ResourceId.ToString());
                }
            }
            xw.WriteEndElement();
        }

        /// <summary>
        /// Writes single resource element to prepared XmlWriter
        /// </summary>
        /// <param name="xw">XmlWriter in needed state</param>
        /// <param name="id">object ID</param>
        /// <param name="resourceId">resource ID object refers to</param>
        private static void writeObject(XmlWriter xw, String id, String resourceId)
        {
            xw.WriteStartElement("object");
            {
                xw.WriteAttributeString("id", id);
                xw.WriteAttributeString("resource-id", resourceId);
            }
            xw.WriteEndElement();
        }

        /// <summary>
        /// Writes layers element to prepared XmlWriter
        /// </summary>
        /// <param name="xw">XmlWriter in needed state</param>
        private static void writeLayers(XmlWriter xw)
        {
            xw.WriteStartElement("layers");
            {
                for (int i = 0; i < ProjectHolder.layers.Count; i++)
                    writeLayer(xw, ProjectHolder.layers[i]);
            }
            xw.WriteEndElement();
        }

        /// <summary>
        /// Writes layer element to prepared XmlWriter
        /// </summary>
        /// <param name="xw">XmlWriter in needed state</param>
        /// <param name="layer">layer to be saved</param>
        private static void writeLayer(XmlWriter xw, Layer layer)
        {
            xw.WriteStartElement("layer");
            xw.WriteAttributeString("id", layer.getId().ToString());
            {
                List<LayerObject> objects = layer.getLayerObjects();
                for (int i = 0; i < objects.Count; i++)
                    writeLayerObject(xw, objects[i]);
            }
            xw.WriteEndElement();
        }

        /// <summary>
        /// Writes layer object element to prepared XmlWriter
        /// </summary>
        /// <param name="xw">XmlWriter in needed state</param>
        /// <param name="obj">layer object to be saved</param>
        private static void writeLayerObject(XmlWriter xw, LayerObject obj)
        {
            xw.WriteStartElement("object");
            xw.WriteAttributeString("id", obj.Id.ToString());
            xw.WriteAttributeString("frame-start", obj.Column.ToString());
            xw.WriteAttributeString("frame-end", (obj.Column + obj.Length).ToString());
            xw.WriteAttributeString("visible", obj.Visible ? "1" : "0");
            // TODO: initial x, y, angle, scale-x,-y
            // TODO: transform elements inside this element
            xw.WriteEndElement();
        }
    }
}
