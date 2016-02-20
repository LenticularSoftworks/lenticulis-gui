using lenticulis_gui.src.Containers;
using System;
using System.Collections.Generic;

namespace lenticulis_gui.src.App
{
    /// <summary>
    /// Class contains static methods used for calculates and sets disparity of
    /// canvas objects to create 3D
    /// </summary>
    public static class Generator3D
    {
        /// <summary>
        /// Step of image count which are seen by left and right eye
        /// </summary>
        private static int viewZoneDistance;

        /// <summary>
        /// Average eye distance in inches
        /// </summary>
        private const double eyeDistance = 2.5f;

        /// <summary>
        /// milimetres to inches conversion
        /// </summary>
        private const double mmToInch = 25.4f;

        /// <summary>
        /// Calculates disparity and set image positions to current frames
        /// </summary>
        /// <param name="viewDistance">View distance</param>
        /// <param name="viewAngle">View angle in degrees</param>
        /// <param name="imageCount">Number of frames</param>
        /// <param name="width">Width of image</param>
        /// <param name="dpi">DPI</param>
        /// <param name="timelineList">List of objects in project</param>
        /// <param name="depthArray">Array of depths of layers</param>
        public static void Generate3D(double viewDistance, double viewAngle, int imageCount, int width, double dpi, List<TimelineItem> timelineList, double[] depthArray)
        {
            //image step for left and right eye
            viewZoneDistance = CalculateZoneDistance(viewDistance, viewAngle, imageCount);

            //shift each object in project
            foreach (TimelineItem item in timelineList) 
            {
                LayerObject lo = item.getLayerObject();

                int layer = lo.Layer;
                double depth = depthArray[layer];

                //calculates and set disparity of object
                SetDisparity(width, dpi, viewDistance, depth, lo);
            }
        }

        /// <summary>
        /// Step of image count which are seen by left and right eye
        /// </summary>
        /// <param name="viewDistance">View distance</param>
        /// <param name="viewAngle">View angle</param>
        /// <param name="imageCount">Image count</param>
        /// <returns>Image step</returns>
        public static int CalculateZoneDistance(double viewDistance, double viewAngle, int imageCount)
        {
            //length all images view zone in eye level
            double viewLevelLength = viewDistance * Math.Tan((Math.PI / 180) * (viewAngle / 2.0)) * 2;

            //length of single image view zone
            double singleViewZone = viewLevelLength / (double)imageCount;

            return (int)Math.Floor(eyeDistance / singleViewZone);
        }

        /// <summary>
        /// Calculates disparity between left and right image of single object and set to frames.
        /// </summary>
        /// <param name="width">Width of image</param>
        /// <param name="dpi">DPI</param>
        /// <param name="viewDistance">View distance</param>
        /// <param name="depth">Depth of object</param>
        /// <param name="lo">Layer object instance</param>
        private static void SetDisparity(int width, double dpi, double viewDistance, double depth, LayerObject lo)
        {
            //transform in image (start column + viewZoneDistance - 1)
            int rightEyeImage = lo.Column + viewZoneDistance - 1;

            //disparity is less than size of visibility zone
            if (rightEyeImage <= lo.Column)
                return;

            //new positions, swapped coordinates - initialY from left side
            float tempLeft = CalcSingleEyeImage(lo.InitialY, width, dpi, viewDistance, depth, true) + lo.InitialY;
            float tempRight = CalcSingleEyeImage(lo.InitialY, width, dpi, viewDistance, depth, false) + lo.InitialY;

            //disparity
            float disparity = tempRight - tempLeft;

            //position for right eye
            float newRight = lo.InitialY + disparity;

            //set new position for frame seen by right and interpolate other images
            //distance of one object seen by left and right eye should be same in all frames
            float progress = 1.0f / ((float)(rightEyeImage) / (float)(lo.Length - 1));
            float transY = Interpolator.interpolateLinearValue(InterpolationType.Linear, progress, lo.InitialY, newRight) - lo.InitialY;

            Transformation tr = new Transformation(TransformType.Translation, 0.0f, transY, 0);
            tr.Interpolation = lo.TransformInterpolationTypes[TransformType.Translation];
            lo.setTransformation(tr);
        }

        /// <summary>
        /// Calculate shift from initial coordinate for single eye image.
        /// </summary>
        /// <param name="initX">initial coordinate X [px]</param>
        /// <param name="width">width of image [px]</param>
        /// <param name="dpi">DPI</param>
        /// <param name="viewDistance">view distance [in]</param>
        /// <param name="objectDistance">object distance from focal pane [in] (+ is closer to observer)</param>
        /// <param name="left">if true returns shift for left eye, lse for right eye</param>
        /// <returns>Pixel shift from initial position</returns>
        private static int CalcSingleEyeImage(double initX, int width, double dpi, double viewDistance, double objectDistance, bool left)
        {
            //convert to inches
            double initXInch = initX / dpi;
            double widthInch = width / dpi;
            //half length between eyes
            double halfEyeDist = eyeDistance / 2.0f;

            //addition for left eye, substraction for right
            if (!left)
                halfEyeDist = -1 * halfEyeDist;

            //distance between eye and initial position in same level
            double eyeToImage = initXInch - (widthInch / 2.0f) + halfEyeDist;

            //shift in inches
            double resultInch = (eyeToImage * objectDistance) / (viewDistance - objectDistance);

            return (int)Math.Round(resultInch * dpi);
        }
    }
}
