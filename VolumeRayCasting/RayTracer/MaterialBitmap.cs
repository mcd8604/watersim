using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.Xna.Framework;

namespace RayTracer
{
    public class MaterialBitmap : Material
    {
        private Bitmap image;

        public MaterialBitmap(Bitmap image)
        {
            this.image = image;
        }

        public override Vector4 getDiffuseColor(float u, float v)
        {
            return getPixelColor(u, v);
        }

        public override Vector4 getAmbientColor(float u, float v)
        {
            return getPixelColor(u, v);
        }

        private Vector4 getPixelColor(float u, float v)
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
