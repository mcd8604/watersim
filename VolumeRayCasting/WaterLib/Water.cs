using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WaterLib
{
	public class Water
	{
		public Vector3 Position = Vector3.Zero;
        public Vector3 Velocity = Vector3.Zero;

        public Vector3 VelocityEval = Vector3.Zero;

        public Vector3 Acceleration = Vector3.Zero;
        public Vector3 Force = Vector3.Zero;

        public float pressure;
        public float density;

        public Color color = Color.White;

        public List<Water> Neighbors = new List<Water>();

		public Water(Vector3 startPosition)
		{
			Position = startPosition;
		}
	}
}
