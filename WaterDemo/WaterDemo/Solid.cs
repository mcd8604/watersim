using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace WaterDemo
{
	class Solid
	{
		internal Vector3 Position = Vector3.Zero;
		internal Vector3 Velocity = Vector3.Zero;

		internal Vector3 VelocityEval = Vector3.Zero;

		internal Vector3 Acceleration = Vector3.Zero;

		internal float radius = 2;

		internal BoundingSphere BoundingSphere
		{
			get
			{
				return new BoundingSphere(Position, radius);
			}
		}

		public Solid(Vector3 startPosition)
		{
			StartPosition = startPosition;
			LastPosition = startPosition;
			Position = startPosition;
		}

		internal Vector3 StartPosition = Vector3.Zero;
		internal Vector3 LastPosition = Vector3.Zero;
	}
}
