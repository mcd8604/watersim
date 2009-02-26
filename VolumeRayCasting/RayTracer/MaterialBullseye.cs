using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayTracer
{
    public class MaterialBullseye : Material
    {
        Vector2 center = new Vector2(0.5f, 0.5f);

        private Vector4 red = Color.Red.ToVector4();
        private Vector4 yellow = Color.Yellow.ToVector4();

        private Vector4 getColor(float u, float v)
        {
            u = u % 1f;
            v = v % 1f;
            float dist = Vector2.Distance(new Vector2(u, v), center);
            if (dist % 0.2 < 0.1)
            {
                return red;
            }
            else
            {
                return yellow;
            }
        }

        public override Vector4 getAmbientColor(float u, float v)
        {
            return ambientStrength * getColor(u, v); ;
        }

        public override Vector4 getDiffuseColor(float u, float v)
        {
            return diffuseStrength * getColor(u, v); ; 
        }
    }
}
