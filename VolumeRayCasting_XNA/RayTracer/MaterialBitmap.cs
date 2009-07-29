using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.Xna.Framework;

namespace RayTracer
{
    /// <summary>
    /// A material that uses a Bitmap image to map texture coordinates.
    /// </summary>
    public class MaterialBitmap : Material, IMaterialTexture
    {
        private Bitmap image;

        /// <summary>
        /// Creates a new MaterialBitmap for a given image.
        /// </summary>
        /// <param name="image">The image.</param>
        public MaterialBitmap(Bitmap image)
        {
            this.image = image;
        }

        public Vector4 GetColor(float u, float v)
        {
            u = u % 1f;
            v = v % 1f;
            int x = (int)(u * image.Width);
            int y = (int)(v * image.Height);
            Color pixel = image.GetPixel(x, y);
            return new Vector4(pixel.R / 255f, pixel.G / 255f, pixel.B / 255f, pixel.A / 255f);
        }
    }
}
