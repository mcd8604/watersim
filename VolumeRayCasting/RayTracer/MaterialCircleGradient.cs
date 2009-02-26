using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayTracer
{
    public class MaterialCircleGradient : Material
    {
        Vector2 center = new Vector2(0.5f, 0.5f);

        private readonly float scale;
        private readonly Vector4 color1 = Color.Red.ToVector4();
        private readonly Vector4 color2 = Color.Yellow.ToVector4();

        public MaterialCircleGradient(float scale, Vector4 color1, Vector4 color2)
        {
            this.scale = scale;
            this.color1 = color1;
            this.color2 = color2;
        }

        private Vector4 getColor(float u, float v)
        {
            u = u % 1f;
            v = v % 1f;
            float dist = Vector2.Distance(new Vector2(u, v), center);

            float sect = (dist % scale);
            float per = sect / scale;

            if (sect <= scale / 2)
            {
                return (per * color1) + ((1 - per) * color2);
            }
            else
            {
                return (per * color2) + ((1 - per) * color1);
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
