using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WaterDemo
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

		internal Color color = new Color(new Vector4(.0f, .5f, .8f, 1f));

		internal List<Water> Neighbors = new List<Water>();

		public Water(Vector3 startPosition)
		{
			ControlPosition = startPosition + new Vector3(0, Vector3.Distance(startPosition, Vector3.Zero), 0);
			LastPosition = startPosition;
			Position = startPosition;
		}

		internal Vector3 ControlPosition = Vector3.Zero;
		internal Vector3 LastPosition = Vector3.Zero;
	}
}
