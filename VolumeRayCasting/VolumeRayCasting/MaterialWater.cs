using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace VolumeRayCasting
{
    class MaterialWater : Material
    {

        public override Microsoft.Xna.Framework.Vector4 getAmbientColor(float u, float v)
        {
            return Color.Blue.ToVector4();
        }
    }
}
