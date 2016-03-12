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
            int final_x, final_y, final_width, final_height, w_delta, h_delta;
            float progress;
            LayerObject current;
            Transformation trans;
            ImageHolder resource;

            // file path pattern successfully built from supplied params
            String builtPathPattern = path + (path[path.Length - 1] == '\\' ? "" : "\\") + filenamePattern;

            // we are exporting frame by frame
            for (int keyframe = 0; keyframe < objects.Length; keyframe++)
            {
                // this creates new image canvas, and prepares everything for drawing
                SupportLib.initializeCanvas((uint)ProjectHolder.Width, (uint)ProjectHolder.Height);

                // now we go through all objects in all layers in that keyframe
                // the order is important - last placed image is on top of everything
                for (int layer = objects[keyframe].Length - 1; layer >= 0; layer--)
                {
                    current = objects[keyframe][layer];
                    if (current == null)
                        continue;

                    // do not export layers, that are hidden
                    if (!current.Visible)
                        continue;

                    // this tells support library imageprocessor to work with image of specified ID
                    SupportLib.loadImage(current.ResourceId);

                    // we also retrieve instance from our container, where we store i.e. dimensions, etc.
                    resource = Storage.Instance.getImageHolder(current.ResourceId);

                    // final X, Y - we will mess a bit with it later
                    final_x = (int)current.InitialX;
                    final_y = (int)current.InitialY;

                    final_width = (int)resource.width;
                    final_height = (int)resource.height;

                    // temp variables, that are set when rotating image - more is written a bit further
                    w_delta = 0;
                    h_delta = 0;

                    // the next block is applicable only when there are some transformations
                    if (current.hasTransformations())
                    {
                        // calculate progress on current frame
                        if (current.Length > 1)
                            progress = (float)(keyframe - current.Column) / (float)(current.Length - 1);
                        else
                            progress = 0.0f;

                        // at first, look for scaling transformation
                        trans = current.getTransformation(TransformType.Scale);
                        if (trans != null)
                        {
                            // resize image, if needed, according to progress and transform vector setting
                            tmp_x = (uint)(resource.width * Interpolator.interpolateLinearValue(trans.Interpolation, progress, current.InitialScaleX, current.InitialScaleX + trans.TransformX));
                            tmp_y = (uint)(resource.height * Interpolator.interpolateLinearValue(trans.Interpolation, progress, current.InitialScaleY, current.InitialScaleY + trans.TransformY));
                            SupportLib.resizeImage(tmp_x, tmp_y);

                            // store final_width/_height so we can then compute the center of bounding box
                            final_width = (int)tmp_x;
                            final_height = (int)tmp_y;
                        }

                        // then for rotation
                        trans = current.getTransformation(TransformType.Rotate);
                        if (trans != null)
                        {
                            tmp_angle = Interpolator.interpolateLinearValue(trans.Interpolation, progress, current.InitialAngle, current.InitialAngle + trans.TransformAngle);
                            SupportLib.rotateImage(tmp_angle);

                            // we have to increase final_width/_height to match the dimensions of bounding box
                            // so we can then place image preciselly on canvas
                            double w_now = final_width;
                            double h_now = final_height;
                            double angle_rad = ((double)tmp_angle) * Math.PI / 180.0;
                            final_width = (int)Math.Ceiling(w_now * Math.Cos(angle_rad) + h_now * Math.Sin(angle_rad));
                            final_height = (int)Math.Ceiling(w_now * Math.Sin(angle_rad) + h_now * Math.Cos(angle_rad));

                            // the bounding box also moves a bit from original position
                            w_delta = (int)((final_width - w_now) / 2);
                            h_delta = (int)((final_height - h_now) / 2);
                        }

                        // and finally to translation, because image composition is done with coordinates to use
                        trans = current.getTransformation(TransformType.Translation);
                        Transformation trans3D = current.getTransformation(TransformType.Translation3D);
                        if (trans != null)
                        {
                            // we just store coordinates, then we will work a bit with this value, so save it for later use
                            final_x = (int)Interpolator.interpolateLinearValue(trans.Interpolation, progress, current.InitialX, current.InitialX + trans.TransformX);
                            final_y = (int)Interpolator.interpolateLinearValue(trans.Interpolation, progress, current.InitialY, current.InitialY + trans.TransformY);

                            if (trans3D != null)
                            {
                                int final3D_x = (int)Interpolator.interpolateLinearValue(trans3D.Interpolation, progress, current.InitialX, current.InitialX + trans3D.TransformX);
                                final_x = (int)(final_x + final3D_x - current.InitialX);
                            }
                        }
                    }

                    // we place image using its center, so final_x should be increased by half the width, and final_y by half the height
                    // also we subtract w_delta and h_delta - these are values got as moved coordinates of bounding box during rotation (around center)
                    final_x += final_width / 2 - w_delta;
                    final_y += final_height / 2 - h_delta;

                    // place image onto canvas
                    SupportLib.compositeImage(final_x, final_y);

                    // this properly frees original image
                    SupportLib.finalizeImage();
                }

                // export to file, using formatted file path
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
