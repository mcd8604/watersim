using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VolumeRayCastingCPU
{
	public class Water
	{
		internal Vector3 Position = Vector3.Zero;
		internal Vector3 Velocity = Vector3.Zero;

		internal Vector3 VelocityEval = Vector3.Zero;

		internal Vector3 Acceleration = Vector3.Zero;
		internal Vector3 Force = Vector3.Zero;

		internal float pressure;
		internal float density;

		internal Color color = Color.White;

		internal List<Water> Neighbors = new List<Water>();

		public Water(Vector3 startPosition)
		{
			Position = startPosition;
		}
	}
}
