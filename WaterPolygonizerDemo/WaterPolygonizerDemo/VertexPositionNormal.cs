using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace WaterPolygonizerDemo
{
    struct VertexPositionNormal
    {
        private Vector3 position;
        private Vector3 normal;

        public VertexPositionNormal(Vector3 position, Vector3 normal)
        {
            this.position = position;
            this.normal = normal;
        }

        public static VertexElement[] VertexElements = 
        {
            new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
            new VertexElement(0, sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0)
        };

        public static int SizeInBytes = sizeof(float) * 6;
    }
}
