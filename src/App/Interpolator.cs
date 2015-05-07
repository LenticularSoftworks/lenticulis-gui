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
        ///
        /// Uses built-in formula for calculating value depending on progress supplied
        /// This method also allows extrapolation, if progress is greater than 1 (or lower than 0)
        /// </summary>
        /// <param name="type">type of interpolation</param>
        /// <param name="progress">function progress (range [0;1] for interpolation, anything else for extrapolation)</param>
        /// <param name="inputStart">value at the beginning (at progress = 0)</param>
        /// <param name="inputEnd">value at the end (at progress = 1)</param>
        /// <returns>interpolated single value</returns>
        public static float interpolateLinearValue(InterpolationType type, float progress, float inputStart, float inputEnd)
        {
            // depending on interpolation type, we use different formulas
            switch (type)
            {
                case InterpolationType.Linear:
                    // y = x0 + a * delta
                    return inputStart + progress * (inputEnd - inputStart);
                case InterpolationType.Quadratic:
                    // y = x0 + a^2 * delta
                    return inputStart + progress * progress * (inputEnd - inputStart);
                case InterpolationType.Cubic:
                    // y = x0 + (4 * a^3 + 0.5)*delta
                    return inputStart + (float)(4 * Math.Pow((double)progress - 0.5, 3) + 0.5f) * (inputEnd - inputStart);
                case InterpolationType.Goniometric:
                    // y = x0 + sin(a * PI/2) * delta
                    return inputStart + (float)(Math.Sin(progress*Math.PI/2.0)) * (inputEnd - inputStart);
            }

            // this would mean invalid interpolation type, which should never happen at all
            return inputStart;
        }
    }
}
