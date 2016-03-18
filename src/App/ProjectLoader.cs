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
        /// <summary>
        /// Remapping dictionary (resources numbering may change during load)
        /// </summary>
        private static Dictionary<int, int> resourceRemap;

        /// <summary>
        /// maps object id to resource ids
        /// </summary>
        private static Dictionary<int, int> objectResourceMap;

        /// <summary>
        /// Loaded file URI
        /// </summary>
        private static Uri LoadFilePath;

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
                MessageBox.Show(LangProvider.getString("PLE_FILE_NOT_FOUND"), LangProvider.getString("PROJECT_LOAD_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LoadFilePath = new Uri(filename);

            // try to parse document
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(docIn);
            }
            // this means the XML format was broken
            catch (XmlException)
            {
                MessageBox.Show(LangProvider.getString("PLE_FILE_FORMAT"), LangProvider.getString("PROJECT_LOAD_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // clean up all previously done stuff
            ProjectHolder.CleanUp();
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
            if (nl.Count == 0 || !loadProperties(nl[0] as XmlElement))
                return false;

            // then resources used
            nl = el.GetElementsByTagName("resources");
            if (nl.Count == 0 || !loadResources(nl[0] as XmlElement))
                return false;

            // then objects (instances of resources)
            nl = el.GetElementsByTagName("objects");
            if (nl.Count == 0 || !loadObjects(nl[0] as XmlElement))
                return false;

            // and finally layers
            nl = el.GetElementsByTagName("layers");
            if (nl.Count == 0 || !loadLayers(nl[0] as XmlElement))
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
                MessageBox.Show(LangProvider.getString("PLE_FILE_ERRORS"), LangProvider.getString("PROJECT_LOAD_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // frames and layers has to be valid natural numbers (1 and more)
            if (frames <= 0 || layers <= 0)
            {
                MessageBox.Show(LangProvider.getString("PLE_FILE_ERRORS"), LangProvider.getString("PROJECT_LOAD_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
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
                    // PSD layer doesn't have to be included - when it's not, it means we will load
                    // whole PSD file, not just one layer
                    if (el.HasAttribute("psd-layer"))
                    {
                        try
                        {
                            psdlayer = int.Parse(el.GetAttribute("psd-layer"));
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(LangProvider.getString("PLE_FILE_ERRORS"), LangProvider.getString("PROJECT_LOAD_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                    }

                    // verify, if it's supported format
                    if (!type.Equals("image"))
                    {
                        MessageBox.Show(String.Format(LangProvider.getString("PLE_UNSUPPORTED_TYPE"), path, type), LangProvider.getString("PROJECT_LOAD_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }

                    // if the file does not exist, try to resolve relative path
                    if (!File.Exists(path))
                    {
                        Uri baseDir = new Uri(LoadFilePath, ".");
                        String absPath = Path.Combine(baseDir.AbsolutePath, path);

                        path = Path.GetFullPath((new Uri(absPath)).LocalPath);
                    }

                    // load image
                    ih = ImageHolder.loadImage(path, false, psdlayer);
                    // if anything fails, show message about it
                    if (ih == null)
                    {
                        MessageBox.Show(LangProvider.getString("PLE_RESOURCE_NOT_FOUND") + path, LangProvider.getString("PROJECT_LOAD_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(LangProvider.getString("PLE_FILE_ERRORS"), LangProvider.getString("PROJECT_LOAD_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
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
                // each object must have ID and resourceID defined
                foreach (XmlElement el in nl.OfType<XmlElement>())
                {
                    id = int.Parse(el.GetAttribute("id"));
                    resourceId = int.Parse(el.GetAttribute("id"));

                    // store object in remapping dictionary to be able to resolve it later
                    objectResourceMap.Add(id, resourceRemap[resourceId]);
                }
            }
            catch (Exception)
            {
                MessageBox.Show(LangProvider.getString("PLE_FILE_ERRORS"), LangProvider.getString("PROJECT_LOAD_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
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

            // just goes through all layers and load them one by one
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
            // each layer has to have its layer ID ("position" in layer list)
            try
            {
                layerId = int.Parse(element.GetAttribute("id"));
            }
            catch (Exception)
            {
                MessageBox.Show(LangProvider.getString("PLE_FILE_ERRORS"), LangProvider.getString("PROJECT_LOAD_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // and here comes some fun
            int objId, frameStart, frameEnd;
            float objX, objY, objAngle, objScaleX, objScaleY;

            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;

            XmlNodeList objects = element.GetElementsByTagName("object");

            // go through all "object" elements in parent layer element, and parse their attributes
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
                    MessageBox.Show(LangProvider.getString("PLE_FILE_ERRORS"), LangProvider.getString("PROJECT_LOAD_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // optional parameters
                try
                {
                    // initial X and Y position
                    if (el.HasAttribute("x"))
                        objX = float.Parse(el.GetAttribute("x"));
                    if (el.HasAttribute("y"))
                        objY = float.Parse(el.GetAttribute("y"));
                    // initial angle
                    if (el.HasAttribute("angle"))
                        objAngle = float.Parse(el.GetAttribute("angle"));
                    // initial X and Y scale
                    if (el.HasAttribute("scale-x"))
                        objScaleX = float.Parse(el.GetAttribute("scale-x"));
                    if (el.HasAttribute("scale-y"))
                        objScaleY = float.Parse(el.GetAttribute("scale-y"));
                }
                catch (Exception)
                {
                    MessageBox.Show(LangProvider.getString("PLE_FILE_ERRORS"), LangProvider.getString("PROJECT_LOAD_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // load the object into timeline
                int resourceId = 0;

                // retrieves image holder of previously loaded and prepared resource
                ImageHolder hldr = Storage.Instance.getImageHolder(objectResourceMap[objId]);

                // resolve extension, filename, and bare path
                String[] expl = hldr.fileName.Split('.');
                String extension = expl[expl.Length-1];
                expl = hldr.fileName.Split('\\');
                String fileBareName = expl[expl.Length-1];
                String fileBarePath = hldr.fileName.Substring(0, hldr.fileName.Length - fileBareName.Length);

                // load resource and put it into internal structures
                String loadFileName = hldr.fileName;
                if (hldr.psdLayerIndex > -1)
                    loadFileName = loadFileName + "["+hldr.psdLayerIndex+"]";
                // puts resource on canvas, the timeline object would be created later
                bool result = mw.LoadAndPutResource(loadFileName, extension, true, out resourceId);

                // loading errors results in false response, so it indicates, that previously used file is now inaccessible
                if (!result)
                {
                    MessageBox.Show(LangProvider.getString("PLE_FILE_ERRORS"), LangProvider.getString("PROJECT_LOAD_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // add to last used
                mw.AddLastUsedItem(fileBarePath, fileBareName, extension);

                // create timeline object
                TimelineItem newItem = new TimelineItem(layerId, frameStart, frameEnd - frameStart, fileBareName);
                newItem.GetLayerObject().ResourceId = resourceId;
                mw.AddTimelineItem(newItem, true, false);

                // assign initial state to layerobject
                LayerObject lobj = newItem.GetLayerObject();
                lobj.InitialX = objX;
                lobj.InitialY = objY;
                lobj.InitialAngle = objAngle;
                lobj.InitialScaleX = objScaleX;
                lobj.InitialScaleY = objScaleY;

                // load transformations, if there are any (they are not mandatory, but often they are present)
                XmlNodeList transformations = el.GetElementsByTagName("transform");
                foreach (XmlElement trel in transformations.OfType<XmlElement>())
                {
                    // type is mandatory attribute
                    String type = trel.GetAttribute("type");
                    // initial values
                    float vectorX = 0.0f, vectorY = 0.0f, angle = 0.0f;

                    TransformType ttype = TransformType.Translation;

                    // each transform type has its own set of mandatory parameters
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
                        case "rotate":
                            ttype = TransformType.Rotate;
                            angle = float.Parse(trel.GetAttribute("angle"));
                            break;
                    }

                    // default interpolation type is linear, may be something else (not mandatory)
                    String interpolation = "linear";
                    if (trel.HasAttribute("interpolation"))
                        interpolation = trel.GetAttribute("interpolation");

                    InterpolationType itype = InterpolationType.Linear;

                    // convert string-represented interpolation types to enum values
                    // fallback to linear, when invalid interpolation type defined
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

                    // and finally create transformation, and store it within the object
                    Transformation trans = new Transformation(ttype, vectorX, vectorY, angle);
                    trans.Interpolation = itype;
                    lobj.setTransformation(trans);
                }
            }

            return true;
        }
    }
}
