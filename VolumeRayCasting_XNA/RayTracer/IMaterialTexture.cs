using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayTracer
{
    /// <summary>
    /// A material that maps texture coordinates to colors.
    /// </summary>
    public interface IMaterialTexture
    {
        /// <summary>
        /// Returns the color of the material at the given texture coordinates.
        /// </summary>
        /// <returns>The color of the material.</returns>
        /// <param name="u">The U texture coordinate.</param>
        /// <param name="v">The V texture coordinate.</param>
        /// <returns>The color of the material at the given texture coordinates.</returns>
        Vector4 GetColor(float u, float v);
    }
}