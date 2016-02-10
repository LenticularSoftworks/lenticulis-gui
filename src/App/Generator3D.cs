using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using lenticulis_gui.src.Containers;
using System.Diagnostics;
using System.Windows;

namespace lenticulis_gui.src.App
{
    public static class Generator3D
    {
        /// <summary>
        /// Step of image count which are seen by left and right eye
        /// </summary>
        private static int viewZoneDistance;

        /// <summary>
        /// DPI
        /// </summary>
        private static double dpi;

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
        /// <param name="inputDpi">DPI</param>
        /// <param name="timelineList">List of objects in project</param>
        /// <param name="depthArray">Array of depths of layers</param>
        public static void Generate3D(double viewDistance, double viewAngle, int imageCount, int width, double inputDpi, List<TimelineItem> timelineList, double[] depthArray)
        {
            //set DPI
            dpi = inputDpi;
            //image step for left and right eye
            viewZoneDistance = CalculateZoneDistance(viewDistance, viewAngle, imageCount);

            //shift each object in project
            foreach (TimelineItem item in timelineList) 
            {
                LayerObject lo = item.getLayerObject();

                //swapped coordinates - initialY from left side
                float positionX = lo.InitialY;

                int layer = item.getLayerObject().Layer;

                //new positions
                float newLeft = CalcSingleEyeImage(positionX, width, viewDistance, depthArray[layer], true) + positionX;
                float newRight = CalcSingleEyeImage(positionX, width, viewDistance, depthArray[layer], false) + positionX;

                //transform in image (start column + viewZoneDistance - 1)
                int rightEyeImage = lo.Column + viewZoneDistance - 1;

                //disparity is less than size of visibility zone
                if (rightEyeImage <= lo.Column)
                {
                    //TODO - temp message
                    MessageBox.Show("rightEyeImage: " + rightEyeImage, "rightEyeImage <= lo.Column", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                //set new position for left eye
                item.getLayerObject().InitialY = newLeft;

                //set new position for frame seen by right and interpolate other images
                //distance of one object seen by left and right eye should be same in all frames
                float progress = 1.0f / ((float)(rightEyeImage) / (float)(lo.Length - 1));
                float transY = Interpolator.interpolateLinearValue(InterpolationType.Linear, progress, lo.InitialY, newRight) - lo.InitialY;
                
                Transformation tr = new Transformation(TransformType.Translation, 0.0f, transY, 0);
                tr.Interpolation = lo.TransformInterpolationTypes[TransformType.Translation];
                lo.setTransformation(tr);

                //Debug.WriteLine("-- Image --\nLayer (depth): " + layer + " (" + depthArray[layer] + ")\nInitX: " + positionX + "\nLeft (image): " +
                    //newLeft + " (0)\nRight (image): " + newRight + " (" + viewZoneDistance + ")");
            }
        }

        /// <summary>
        /// Calculate shift from initial coordinate for single eye image.
        /// </summary>
        /// <param name="initX">initial coordinate X [px]</param>
        /// <param name="width">width of image [px]</param>
        /// <param name="viewDistance">view distance [in]</param>
        /// <param name="objectDistance">object distance from focal pane [in] (+ is closer to observer)</param>
        /// <param name="left">if true returns shift for left eye, lse for right eye</param>
        /// <returns>Pixel shift fro minitial position</returns>
        private static int CalcSingleEyeImage(double initX, int width, double viewDistance, double objectDistance, bool left)
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

        /// <summary>
        /// Step of image count which are seen by left and right eye
        /// </summary>
        /// <param name="viewDistance">View distance</param>
        /// <param name="viewAngle">View angle</param>
        /// <param name="imageCount">Image count</param>
        /// <returns>Image step</returns>
        private static int CalculateZoneDistance(double viewDistance, double viewAngle, int imageCount)
        {
            //length all images view zone in eye level
            double viewLevelLength = viewDistance * Math.Tan((Math.PI / 180) * (viewAngle / 2.0)) * 2;

            //length of single image view zone
            double singleViewZone = viewLevelLength / (double)imageCount;

            return (int)Math.Floor(eyeDistance / singleViewZone);
        }
    }
}
