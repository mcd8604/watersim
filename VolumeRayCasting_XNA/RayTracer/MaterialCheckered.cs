using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayTracer
{
    /// <summary>
    /// A red and yellow checkerboard material.
    /// </summary>
    public class MaterialCheckered : Material, IMaterialTexture
    {

        private Vector4 red = Color.Red.ToVector4();
        private Vector4 yellow = Color.Yellow.ToVector4();

        public Vector4 GetColor(float u, float v)
        {
            if (u % 1 < 0.5)
            {
                if (v % 1 < 0.5)
                {
                    //red
                    return ambientStrength * red;
                }
                else
                {
                    //yellow
                    return ambientStrength * yellow;
                }
            }
            else
            {
                if (v % 1 < 0.5)
                {
                    //yellow
                    return ambientStrength * yellow;
                }
                else
                {
                    //red
                    return ambientStrength * red;
                }
            }
        }
    }
}
