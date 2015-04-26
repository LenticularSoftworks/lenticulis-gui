using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using lenticulis_gui.src.App;
using lenticulis_gui.src.Containers;

namespace lenticulis_gui.src.SupportLib
{
    static class ImageProcessor
    {
        /// <summary>
        /// Exports given object matrix to image sequence to specified path, using supplied filename pattern and
        /// requested quality.
        /// 
        /// in object matrix, first coordinate is keyframe position, and second is layer position,
        /// therefore objects[3][6] means keyframe 3 in layer 6
        /// </summary>
        /// <param name="objects">objects matrix to be exported</param>
        /// <param name="path">path where to put output images</param>
        /// <param name="filenamePattern">output filename with %i instead of number</param>
        /// <param name="quality">requested output quality (not all formats would support it)</param>
        public static void exportObjectMatrix(LayerObject[][] objects, String path, String filenamePattern, byte quality)
        {
            // working variables
            float tmp_angle;
            uint tmp_x, tmp_y;
            int final_x, final_y;
            float progress;
            LayerObject current;
            Transformation trans;
            ImageHolder resource;

            String builtPathPattern = path + (path[path.Length - 1] == '\\' ? "" : "\\") + filenamePattern;

            for (int keyframe = 0; keyframe < objects.Length; keyframe++)
            {
                SupportLib.initializeCanvas((uint)ProjectHolder.Width, (uint)ProjectHolder.Height);

                for (int layer = objects[keyframe].Length - 1; layer >= 0; layer--)
                {
                    current = objects[keyframe][layer];
                    if (current == null)
                        continue;

                    if (!current.Visible)
                        continue;

                    SupportLib.loadImage(current.ResourceId);

                    resource = Storage.Instance.getImageHolder(current.ResourceId);

                    final_x = (int)current.InitialX;
                    final_y = (int)current.InitialY;

                    if (current.hasTransformations())
                    {
                        if (current.Length > 1)
                            progress = (float)(keyframe - current.Column) / (float)(current.Length - 1);
                        else
                            progress = 0.0f;

                        // at first, look for scaling transformation
                        trans = current.getTransformation(TransformType.Scale);
                        if (trans != null)
                        {
                            tmp_x = (uint)(resource.width * Interpolator.interpolateLinearValue(trans.Interpolation, progress, current.InitialScaleX, trans.TransformX));
                            tmp_y = (uint)(resource.height * Interpolator.interpolateLinearValue(trans.Interpolation, progress, current.InitialScaleY, trans.TransformY));
                            SupportLib.resizeImage(tmp_x, tmp_y);
                        }

                        // then for rotation
                        trans = current.getTransformation(TransformType.Rotate);
                        if (trans != null)
                        {
                            tmp_angle = Interpolator.interpolateLinearValue(trans.Interpolation, progress, current.InitialAngle, current.InitialAngle + trans.TransformAngle);
                            SupportLib.rotateImage(tmp_angle);
                        }

                        // and finally to translation, because image composition is done with coordinates to use
                        
                        trans = current.getTransformation(TransformType.Translation);
                        if (trans != null)
                        {
                            final_x = (int)Interpolator.interpolateLinearValue(trans.Interpolation, progress, current.InitialX, current.InitialX + trans.TransformX);
                            final_y = (int)Interpolator.interpolateLinearValue(trans.Interpolation, progress, current.InitialY, current.InitialY + trans.TransformY);
                        }
                    }

                    // place image onto canvas
                    SupportLib.compositeImage(final_y, final_x);

                    SupportLib.finalizeImage();
                }

                // export to formatted file
                String filename = builtPathPattern.Replace("%i", (keyframe + 1).ToString());
                SupportLib.exportCanvas(Utils.getCString(filename), quality);
            }
        }

        /// <summary>
        /// Fetches layers into matrix of layer objects
        /// </summary>
        /// <param name="layers">array of layers</param>
        /// <returns>prepared layer object matrix</returns>
        public static LayerObject[][] prepareObjectMatrix(Layer[] layers)
        {
            // prepare array
            LayerObject[][] returnArray = new LayerObject[ProjectHolder.ImageCount][];
            for (int i = 0; i < ProjectHolder.ImageCount; i++)
            {
                returnArray[i] = new LayerObject[ProjectHolder.LayerCount];
                for (int j = 0; j < ProjectHolder.LayerCount; j++)
                    returnArray[i][j] = null;
            }

            // go through every layer
            foreach (Layer layer in layers)
            {
                // and for every object within
                foreach (LayerObject lobj in layer.getLayerObjects())
                {
                    // and for every column in layer it occupies, put it into matrix to that position
                    for (int i = lobj.Column; i < lobj.Column + lobj.Length; i++)
                        returnArray[i][lobj.Layer] = lobj;
                }
            }

            return returnArray;
        }
    }
}
