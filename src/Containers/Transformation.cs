using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using lenticulis_gui.src.App;

namespace lenticulis_gui.src.Containers
{
    public class Transformation
    {
        /// <summary>
        /// Transformation type, value from enumerator
        /// </summary>
        public TransformType Type { get; private set; }

        /// <summary>
        /// X coordinate of transformation vector
        /// </summary>
        public float TransformX { get; private set; }

        /// <summary>
        /// Y coordinate of transformation vector
        /// </summary>
        public float TransformY { get; private set; }

        /// <summary>
        /// Angle of transformation (just rotation)
        /// </summary>
        public float TransformAngle { get; private set; }

        /// <summary>
        /// Full constructor, there's no use for it yet
        /// </summary>
        /// <param name="type">Transformation type</param>
        /// <param name="x">X coordinate of vector</param>
        /// <param name="y">Y coordinate of vector</param>
        /// <param name="angle">transform angle</param>
        public Transformation(TransformType type, float x, float y, float angle)
        {
            Type = type;
            TransformX = x;
            TransformY = y;
            TransformAngle = angle;
        }

        /// <summary>
        /// Constructor specifying transform vector
        /// </summary>
        /// <param name="type">Transformation type</param>
        /// <param name="x">X coordinate of vector</param>
        /// <param name="y">Y coordinate of vector</param>
        public Transformation(TransformType type, float x, float y)
        {
            Type = type;
            TransformX = x;
            TransformY = y;
        }

        /// <summary>
        /// Constructor specifying transform angle
        /// </summary>
        /// <param name="type">Transformation type</param>
        /// <param name="angle">transform angle</param>
        public Transformation(TransformType type, float angle)
        {
            Type = type;
            TransformAngle = angle;
        }

        /// <summary>
        /// Plain constructor, only specifying type
        /// </summary>
        /// <param name="type">Transformation type</param>
        public Transformation(TransformType type)
        {
            Type = type;
        }

        /// <summary>
        /// Sets the vector of this transformation
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public void setVector(float x, float y)
        {
            TransformX = x;
            TransformY = y;
        }

        /// <summary>
        /// Sets the angle of transformation; may be oriented angle
        /// </summary>
        /// <param name="angle">transform angle</param>
        public void setAngle(float angle)
        {
            TransformAngle = angle;
        }
    }
}
