using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayTracer
{
    public class MaterialCheckered : Material
    {

        private Vector4 red = Color.Red.ToVector4();
        private Vector4 yellow = Color.Yellow.ToVector4();

        public override Vector4 getAmbientColor(float u, float v)
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

        public override Vector4 getDiffuseColor(float u, float v)
        {
            if (u % 1 < 0.5)
            {
                if (v % 1 < 0.5)
                {
                    //red
                    return diffuseStrength * red;
                }
                else
                {
                    //yellow
                    return diffuseStrength * yellow;
                }
            }
            else
            {
                if (v % 1 < 0.5)
                {
                    //yellow
                    return diffuseStrength * yellow;
                }
                else
                {
                    //red
                    return diffuseStrength * red;
                }
            }
        }
    }
}
