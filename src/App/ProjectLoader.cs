using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows;
using lenticulis_gui.src.Containers;
using lenticulis_gui.src.App;

namespace lenticulis_gui.src.App
{
    public class ProjectLoader
    {
        private static Dictionary<int, int> resourceRemap;
        private static Dictionary<int, int> objectResourceMap;

        /// <summary>
        /// Loads project from specified location
        /// Warning: this method will overwrite all data loaded in current program state
        /// </summary>
        /// <param name="filename">path to file to be loaded</param>
        public static void loadProject(String filename)
        {
            // create stream from file
            FileStream docIn = null;
            try
            {
                docIn = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            // There may be various exceptions, but generally they indicates some problem with filesystem (file not found, insufficient permissions, ..)
            catch (Exception)
            {
                MessageBox.Show("Soubor nebylo možné načíst. Zkontrolujte, zdali je dostupný, a zdali máte práva na jeho přečtení.", "Chyba při načítání souboru", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // try to parse document
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(docIn);
            }
            // this means the XML format was broken
            catch (XmlException)
            {
                MessageBox.Show("Soubor nebylo možné načíst, jelikož se nejedná o soubor platného formátu projektu Lenticulis.", "Chyba při načítání souboru", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // clean up all previously done stuff
            ProjectHolder.cleanUp();
            Storage.Instance.cleanUp();

            // and load project from file
            if (!proceedLoad(doc))
            {
                // something fails - just clean up and return; the error message was shown before

                resourceRemap = null;
                objectResourceMap = null;
                return;
            }

            resourceRemap = null;
            objectResourceMap = null;

            // set the rest of project holder properties
            ProjectHolder.ValidProject = true;
            ProjectHolder.ProjectFileName = filename;
        }

        /// <summary>
        /// Main parsing and loading method
        /// </summary>
        /// <param name="doc">Document to be parsed</param>
        /// <returns>succeeded?</returns>
        private static bool proceedLoad(XmlDocument doc)
        {
            XmlElement el = doc.DocumentElement;

            ProjectHolder.ProjectName = el.GetAttribute("name");

            // maybe somehow deal with version? No greater changes are planned, nor needed for further functionality
            String version = el.GetAttribute("lenticulis-version");

            // at first, parse project properties
            XmlNodeList nl = el.GetElementsByTagName("properties");
            if (!loadProperties(nl[0] as XmlElement))
                return false;

            // then resources used
            nl = el.GetElementsByTagName("resources");
            if (!loadResources(nl[0] as XmlElement))
                return false;

            // then objects (instances of resources)
            nl = el.GetElementsByTagName("objects");
            if (!loadObjects(nl[0] as XmlElement))
                return false;

            // and finally layers
            nl = el.GetElementsByTagName("layers");
            if (!loadLayers(nl[0] as XmlElement))
                return false;

            return true;
        }

        /// <summary>
        /// Loads properties from specified element
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static bool loadProperties(XmlElement element)
        {
            // go through all property elements
            XmlNodeList nl = element.GetElementsByTagName("property");

            int frames = -1, layers = -1;

            // we generally consider any exception as error with format
            try
            {
                foreach (XmlElement el in nl.OfType<XmlElement>())
                {
                    switch (el.GetAttribute("name"))
                    {
                        case "frame-count":
                            frames = int.Parse(el.GetAttribute("value"));
                            break;
                        case "layer-count":
                            layers = int.Parse(el.GetAttribute("value"));
                            break;
                        case "canvas-width":
                            ProjectHolder.Width = int.Parse(el.GetAttribute("value"));
                            break;
                        case "canvas-height":
                            ProjectHolder.Height = int.Parse(el.GetAttribute("value"));
                            break;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Soubor nebylo možné načíst, jelikož obsahuje chyby.", "Chyba při načítání souboru", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (frames <= 0 || layers <= 0)
            {
                MessageBox.Show("Soubor nebylo možné načíst, jelikož obsahuje chyby.", "Chyba při načítání souboru", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // set loaded project properties
            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;
            mw.SetProjectProperties(frames, layers);

            return true;
        }

        /// <summary>
        /// Loads resources from specified element
        /// </summary>
        /// <param name="element">element with resources</param>
        /// <returns>succeeded?</returns>
        private static bool loadResources(XmlElement element)
        {
            // initialize resource remap
            resourceRemap = new Dictionary<int, int>();

            // we need to remap resources due to possible gaps in assigning IDs

            XmlNodeList nl = element.GetElementsByTagName("resource");
            int id;
            ImageHolder ih;
            String path, type;
            int psdlayer;

            try
            {
                foreach (XmlElement el in nl.OfType<XmlElement>())
                {
                    id = int.Parse(el.GetAttribute("id"));

                    path = el.GetAttribute("path");
                    type = el.GetAttribute("type");

                    psdlayer = -1;
                    if (el.HasAttribute("psd-layer"))
                    {
                        try
                        {
                            psdlayer = int.Parse(el.GetAttribute("psd-layer"));
                        }
                        catch (Exception)
                        {
                            //
                        }
                    }

                    // verify, if it's supported format
                    if (!type.Equals("image"))
                    {
                        MessageBox.Show("Nepodporovaný typ zdroje pro soubor: " + path + " (" + type + ") - projekt byl nejspíše uložen ve vyšší verzi programu", "Nepodporovaný zdroj", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }

                    // load image
                    ih = ImageHolder.loadImage(path, false, psdlayer);
                    // if anything fails, show message about it
                    if (ih == null)
                    {
                        MessageBox.Show("Nelze nalézt obrázek v umístění: "+path, "Chybějící zdroj", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        // and add remap to dictionary
                        resourceRemap.Add(id, ih.id);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Soubor nebylo možné načíst, jelikož obsahuje chyby.", "Chyba při načítání souboru", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads objects (resource instances) from specified element
        /// </summary>
        /// <param name="element">element with objects</param>
        /// <returns>succeeded?</returns>
        private static bool loadObjects(XmlElement element)
        {
            // this is very similar to previous case - we store only ID-RESOURCE pair
            objectResourceMap = new Dictionary<int, int>();

            XmlNodeList nl = element.GetElementsByTagName("object");
            int id, resourceId;

            try
            {
                foreach (XmlElement el in nl.OfType<XmlElement>())
                {
                    id = int.Parse(el.GetAttribute("id"));
                    resourceId = int.Parse(el.GetAttribute("id"));

                    objectResourceMap.Add(id, resourceRemap[resourceId]);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Soubor nebylo možné načíst, jelikož obsahuje chyby.", "Chyba při načítání souboru", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads layers from specified element
        /// </summary>
        /// <param name="element">element with layers</param>
        /// <returns>succeeded?</returns>
        private static bool loadLayers(XmlElement element)
        {
            XmlNodeList nl = element.GetElementsByTagName("layer");

            foreach (XmlElement el in nl.OfType<XmlElement>())
            {
                if (!loadLayer(el))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Loads single layer from specified element
        /// </summary>
        /// <param name="element">element with layer data</param>
        /// <returns>succeeded?</returns>
        private static bool loadLayer(XmlElement element)
        {
            int layerId;
            try
            {
                layerId = int.Parse(element.GetAttribute("id"));
            }
            catch (Exception)
            {
                MessageBox.Show("Soubor nebylo možné načíst, jelikož obsahuje chyby.", "Chyba při načítání souboru", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            int objId, frameStart, frameEnd;
            float objX, objY, objAngle, objScaleX, objScaleY;

            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;

            XmlNodeList objects = element.GetElementsByTagName("object");

            foreach (XmlElement el in objects.OfType<XmlElement>())
            {
                // set defaults
                objX = 0; objY = 0; objAngle = 0; objScaleX = 1.0f; objScaleY = 1.0f;

                // mandatory attributes
                try
                {
                    objId = int.Parse(el.GetAttribute("id"));
                    frameStart = int.Parse(el.GetAttribute("frame-start"));
                    frameEnd = int.Parse(el.GetAttribute("frame-end"));
                }
                catch (Exception)
                {
                    MessageBox.Show("Soubor nebylo možné načíst, jelikož obsahuje chyby.", "Chyba při načítání souboru", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // optional parameters
                try
                {
                    if (el.HasAttribute("x"))
                        objX = float.Parse(el.GetAttribute("x"));
                    if (el.HasAttribute("y"))
                        objY = float.Parse(el.GetAttribute("y"));
                    if (el.HasAttribute("angle"))
                        objAngle = float.Parse(el.GetAttribute("angle"));
                    if (el.HasAttribute("scale-x"))
                        objScaleX = float.Parse(el.GetAttribute("scale-x"));
                    if (el.HasAttribute("scale-y"))
                        objScaleY = float.Parse(el.GetAttribute("scale-y"));
                }
                catch (Exception)
                {
                    MessageBox.Show("Soubor nebylo možné načíst, jelikož obsahuje chyby.", "Chyba při načítání souboru", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // load the object into timeline
                int resourceId = 0;
                ImageHolder hldr = Storage.Instance.getImageHolder(objectResourceMap[objId]);
                String[] expl = hldr.fileName.Split('.');
                String extension = expl[expl.Length-1];
                expl = hldr.fileName.Split('\\');
                String fileBareName = expl[expl.Length-1];
                String fileBarePath = hldr.fileName.Substring(0, hldr.fileName.Length - fileBareName.Length);

                // load resource and put it into internal structures
                String loadFileName = hldr.fileName;
                if (hldr.psdLayerIndex > -1)
                    loadFileName = loadFileName + "["+hldr.psdLayerIndex+"]";
                bool result = mw.LoadAndPutResource(loadFileName, extension, true, out resourceId);

                if (!result)
                {
                    MessageBox.Show("Soubor nebylo možné načíst, jelikož obsahuje chyby.", "Chyba při načítání souboru", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // add to last used
                mw.AddLastUsedItem(fileBarePath, fileBareName, extension);

                // create timeline object
                TimelineItem newItem = new TimelineItem(layerId, frameStart, frameEnd - frameStart, fileBareName);
                newItem.getLayerObject().ResourceId = resourceId;
                mw.AddTimelineItem(newItem);

                // assign initial state to layerobject
                LayerObject lobj = newItem.getLayerObject();
                lobj.InitialX = objX;
                lobj.InitialY = objY;
                lobj.InitialAngle = objAngle;
                lobj.InitialScaleX = objScaleX;
                lobj.InitialScaleY = objScaleY;

                XmlNodeList transformations = el.GetElementsByTagName("transform");
                foreach (XmlElement trel in transformations.OfType<XmlElement>())
                {
                    String type = trel.GetAttribute("type");
                    float vectorX = 0.0f, vectorY = 0.0f, angle = 0.0f;

                    TransformType ttype = TransformType.Translation;

                    switch (type)
                    {
                        case "translation":
                            ttype = TransformType.Translation;
                            vectorX = float.Parse(trel.GetAttribute("vector-x"));
                            vectorY = float.Parse(trel.GetAttribute("vector-y"));
                            break;
                        case "scale":
                            ttype = TransformType.Scale;
                            vectorX = float.Parse(trel.GetAttribute("vector-x"));
                            vectorY = float.Parse(trel.GetAttribute("vector-y"));
                            break;
                        case "rotation":
                            ttype = TransformType.Rotate;
                            angle = float.Parse(trel.GetAttribute("angle"));
                            break;
                    }

                    String interpolation = "linear";
                    if (trel.HasAttribute("interpolation"))
                        interpolation = trel.GetAttribute("interpolation");

                    InterpolationType itype = InterpolationType.Linear;

                    switch (interpolation)
                    {
                        case "linear":
                            itype = InterpolationType.Linear;
                            break;
                        case "quadratic":
                            itype = InterpolationType.Quadratic;
                            break;
                        case "cubic":
                            itype = InterpolationType.Cubic;
                            break;
                        case "goniometric":
                            itype = InterpolationType.Goniometric;
                            break;
                    }

                    Transformation trans = new Transformation(ttype, vectorX, vectorY, angle);
                    trans.Interpolation = itype;
                    lobj.setTransformation(trans);
                }
            }

            return true;
        }
    }
}
