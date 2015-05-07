using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using lenticulis_gui.src.Containers;

namespace lenticulis_gui.src.App
{
    public static class ProjectSaver
    {
        /// <summary>
        /// Delta for comparing floating point values
        /// </summary>
        private const float FLOAT_DELTA = 0.00001f;

        /// <summary>
        /// Flag (simple lock) for saving being in progress
        /// </summary>
        private static bool SavingInProgress = false;

        /// <summary>
        /// Filename to be used
        /// </summary>
        private static Uri SavingFilePath;

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
            if (SavingInProgress)
                return;
            SavingFilePath = new Uri(filename);

            SavingInProgress = true;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            XmlWriter xw = XmlWriter.Create(filename, settings);
            xw.WriteStartDocument();

            xw.WriteStartElement("project");
            {
                xw.WriteAttributeString("name", ProjectHolder.ProjectName);
                xw.WriteAttributeString("lenticulis-version", lenticulis_gui.Properties.Resources.LENTICULIS_VERSION);

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

            xw.Close();

            SavingInProgress = false;
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
                // width of canvas
                writeProperty(xw, "canvas-width", ProjectHolder.Width.ToString());
                // height of canvas
                writeProperty(xw, "canvas-height", ProjectHolder.Height.ToString());
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
                // we need to retrieve links to used resources from layer objects
                for (int i = 0; i < ProjectHolder.layers.Count; i++)
                {
                    List<LayerObject> objects = ProjectHolder.layers[i].getLayerObjects();
                    for (int j = 0; j < objects.Count; j++)
                    {
                        LayerObject obj = objects[j];
                        ImageHolder ih = Storage.Instance.getImageHolder(obj.ResourceId);
                        if (ih == null)
                            continue;

                        String pathToUse = ih.fileName;

                        Uri dir = new Uri(SavingFilePath, ".");
                        Uri target = new Uri(ih.fileName);
                        String relPath = Utils.getRelativePath(target.AbsolutePath, dir.AbsolutePath);
                        // use relative path only when containing less than 3 "folder ups"
                        if (relPath != null && relPath.Split(new String[] { ".." }, StringSplitOptions.RemoveEmptyEntries).Length < 3)
                            pathToUse = relPath;

                        writeResource(xw, ih.id.ToString(), "image", relPath, ih.psdLayerIndex);
                    }
                }
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
        private static void writeResource(XmlWriter xw, String id, String type, String path, int psdlayer = -1)
        {
            // write resource, with its id, type and path to it
            xw.WriteStartElement("resource");
            {
                xw.WriteAttributeString("id", id);
                xw.WriteAttributeString("type", type);
                xw.WriteAttributeString("path", path);
                // psd layer may be specified (-1 = all or 'not specified', both means the same when dealing with PSD)
                if (psdlayer > -1)
                    xw.WriteAttributeString("psd-layer", psdlayer.ToString());
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
            // store layer element, and write Id attribute (mandatory)
            xw.WriteStartElement("layer");
            xw.WriteAttributeString("id", layer.getId().ToString());
            {
                // then write all layer objects
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

            // write optional fields

            // X and Y position, if not zero
            if (obj.InitialX != 0.0)
                xw.WriteAttributeString("x", obj.InitialX.ToString());
            if (obj.InitialY != 0.0)
                xw.WriteAttributeString("y", obj.InitialY.ToString());

            // initial angle, if not zero
            if (obj.InitialAngle != 0.0)
                xw.WriteAttributeString("angle", obj.InitialAngle.ToString());

            // initial scale if not 1.0 (close to one)
            if (Math.Abs(obj.InitialScaleX - 1.0) > FLOAT_DELTA)
                xw.WriteAttributeString("scale-x", obj.InitialScaleX.ToString());
            if (Math.Abs(obj.InitialScaleY - 1.0) > FLOAT_DELTA)
                xw.WriteAttributeString("scale-y", obj.InitialScaleY.ToString());

            // if the object has some transformation, write it
            if (obj.hasTransformations())
            {
                // go through all possible types
                var values = Enum.GetValues(typeof(TransformType));
                foreach (TransformType tr in values)
                {
                    // and if present
                    Transformation trans = obj.getTransformation(tr);
                    if (trans != null)
                    {
                        // write its element
                        xw.WriteStartElement("transform");
                        {
                            // type is mandatory
                            xw.WriteAttributeString("type", tr.ToString().ToLower());

                            // other fields are mandatory depending on type
                            switch (tr)
                            {
                                // mandatory transform vector
                                case TransformType.Translation:
                                case TransformType.Scale:
                                    xw.WriteAttributeString("vector-x", trans.TransformX.ToString());
                                    xw.WriteAttributeString("vector-y", trans.TransformY.ToString());
                                    break;
                                // mandatory rotation angle
                                case TransformType.Rotate:
                                    xw.WriteAttributeString("angle", trans.TransformAngle.ToString());
                                    break;
                            }

                            // interpolation field is not mandatory, but we will include it anyways
                            xw.WriteAttributeString("interpolation", trans.Interpolation.ToString().ToLower());
                        }
                        xw.WriteEndElement();
                    }
                }
            }

            xw.WriteEndElement();
        }
    }
}
