using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WaterPolygonizerDemo
{
	[Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormal
    {
        public Vector3 Position;
	    public Vector3 Normal;

		public const int SizeInBytes = 24;

		public static readonly VertexElement[] VertexElements = 
        {
            new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
            new VertexElement(0, 12, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0)
        };

        public VertexPositionNormal(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }
    }
}