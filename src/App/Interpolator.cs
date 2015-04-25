using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.App
{
    public static class Interpolator
    {
        /// <summary>
        /// Performs interpolation of single value
        /// </summary>
        /// <param name="type">type of interpolation</param>
        /// <param name="progress">function progress (range [0;1])</param>
        /// <param name="inputStart">value at the beginning</param>
        /// <param name="inputEnd">value at the end</param>
        /// <returns>interpolated single value</returns>
        public static float interpolateLinearValue(InterpolationType type, float progress, float inputStart, float inputEnd)
        {
            switch (type)
            {
                case InterpolationType.Linear:
                    return inputStart + progress * (inputEnd - inputStart);
                case InterpolationType.Quadratic:
                    return inputStart + progress * progress * (inputEnd - inputStart);
                case InterpolationType.Cubic:
                    return inputStart + (float)(4 * Math.Pow((double)progress - 0.5, 3) + 0.5f) * (inputEnd - inputStart);
                case InterpolationType.Goniometric:
                    return inputStart + (float)(Math.Sin(progress*Math.PI/2.0)) * (inputEnd - inputStart);
            }

            return inputStart;
        }
    }
}
