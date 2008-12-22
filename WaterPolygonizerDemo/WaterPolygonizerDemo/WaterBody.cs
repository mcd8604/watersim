using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WaterPolygonizerDemo
{
	class WaterBody
	{
		Random Rand = new Random();

		float DT = 0.004f;
		Vector3 Gravity = new Vector3(0, -9.8f, 0);

		float SimScale = 0.004f;
		float Viscosity = 0.2f;
		float RestDensity = 600f;
		float ParticleMass = 0.00020543f;
		float ParticleRadius = 0.004f;
		float ParticleDistance = 0.0059f;
		float SmoothRadius = 0.01f;
		float InteriorStiffness = 1f;
		float ExteriorStiffness = 10000f;
		float ExteriorDampening = 256f;
		float SpeedLimit = 200f;

		float R2;
		float Poly6Kern;
		float SpikyKern;
		float LapKern;

		Vector3 Min = new Vector3(5f, 5f, 5f);
        Vector3 Max = new Vector3(45f, 80f, 45f);

		Vector3 InitMin = new Vector3(18f, 20f, 18f);
        Vector3 InitMax = new Vector3(20f, 50f, 20f);

		//Vector3 Min = new Vector3(-25f, 0f, -25f);
		//Vector3 Max = new Vector3(25f, 100f, 25f);

		//Vector3 InitMin = new Vector3(-20f, 0f, -20f);
		//Vector3 InitMax = new Vector3(-5f, 30f, 5f);


		internal Water[] water;

		public WaterBody()
		{
			CalcKernels();
			Spawn();
		}

		private void CalcKernels()
		{
			ParticleDistance = (float)Math.Pow(ParticleMass / RestDensity, 1 / 3f);
			R2 = SmoothRadius * SmoothRadius;
			Poly6Kern = 315f / (64f * MathHelper.Pi * (float)Math.Pow(SmoothRadius, 9));
			SpikyKern = -45f / (MathHelper.Pi * (float)Math.Pow(SmoothRadius, 6));
			LapKern = 45f / (MathHelper.Pi * (float)Math.Pow(SmoothRadius, 6));
		}

		private void Spawn()
		{
			float delta = ParticleDistance * 0.87f / SimScale;

			List<Water> list = new List<Water>();

			for (float z = InitMin.Z; z <= InitMax.Z; z += delta)
			{
				for (float y = InitMin.Y; y <= InitMax.Y; y += delta)
				{
					for (float x = InitMin.X; x <= InitMax.X; x += delta)
					{
						Water p = new Water(new Vector3(x, y, z));
						p.Position.X += -0.05f + ((float)Rand.NextDouble() * 0.1f);
						p.Position.Y += -0.05f + ((float)Rand.NextDouble() * 0.1f);
						p.Position.Z += -0.05f + ((float)Rand.NextDouble() * 0.1f);
						list.Add(p);
					}
				}
			}

			water = list.ToArray();
		}

		public void Update()
		{
			findNeighbors();
			doPressure();
			doForces();
			doStuff();
		}

		private void findNeighbors()
		{
			foreach (Water a in water)
			{
				a.Neighbors.Clear();
				foreach (Water b in water)
				{
					if (a == b) { continue; }
					float dx = (a.Position.X - b.Position.X) * SimScale;
					float dy = (a.Position.Y - b.Position.Y) * SimScale;
					float dz = (a.Position.Z - b.Position.Z) * SimScale;
					float dsq = (dx * dx + dy * dy + dz * dz);
					if (R2 > dsq)
					{
						a.Neighbors.Add(b);
					}
				}
			}
		}

		private void doPressure()
		{
			foreach (Water a in water)
			{
				float sum = 0;
				foreach (Water b in a.Neighbors)
				{
					float dx = (a.Position.X - b.Position.X) * SimScale;
					float dy = (a.Position.Y - b.Position.Y) * SimScale;
					float dz = (a.Position.Z - b.Position.Z) * SimScale;
					float dsq = (dx * dx + dy * dy + dz * dz);

					float diff = R2 - dsq;
					sum += diff * diff * diff;
				}

				a.density = sum * ParticleMass * Poly6Kern;
				a.pressure = (a.density - RestDensity) * InteriorStiffness;
				float v = Math.Max(0.1f, 0.5f + (a.pressure / 1500.0f));
				a.color = new Color(new Vector3(1f - v, 1f - v, 1.0f));
				a.density = 1f / a.density;
			}
		}

		private void doForces()
		{
			foreach (Water a in water)
			{
				Vector3 force = Vector3.Zero;
				foreach (Water b in a.Neighbors)
				{
					float dx = (a.Position.X - b.Position.X) * SimScale;
					float dy = (a.Position.Y - b.Position.Y) * SimScale;
					float dz = (a.Position.Z - b.Position.Z) * SimScale;
					float dsq = (dx * dx + dy * dy + dz * dz);

					float r = (float)Math.Sqrt(dsq);
					float c = SmoothRadius - r;
					float pt = -0.5f * c * SpikyKern * (a.pressure + b.pressure) / r;
					float vt = LapKern * Viscosity;
					Vector3 currentForce;
					currentForce.X = pt * dx + vt * (b.VelocityEval.X - a.VelocityEval.X);
					currentForce.Y = pt * dy + vt * (b.VelocityEval.Y - a.VelocityEval.Y);
					currentForce.Z = pt * dz + vt * (b.VelocityEval.Z - a.VelocityEval.Z);
					currentForce *= c * a.density * b.density;
					force += currentForce;

				}
				a.Force = force;
			}

		}

		private void doStuff()
		{

			float LimitSq = SpeedLimit * SpeedLimit;
			float Epsilon = 0.00001f;

			foreach (Water a in water)
			{
				Vector3 acceleration = a.Force * ParticleMass;

				float speed = acceleration.X * acceleration.X + acceleration.Y * acceleration.Y + acceleration.Z * acceleration.Z;

				if (speed > LimitSq)
				{
					acceleration *= (SpeedLimit / (float)Math.Sqrt(speed));
				}



				float diff = 2 * ParticleRadius - (a.Position.X - Min.X) * SimScale;
				if (diff > Epsilon)
				{
					float adjustment = ExteriorStiffness * diff - ExteriorDampening * Vector3.Dot(a.VelocityEval, Vector3.Right);
					acceleration.X += adjustment;
				}

				diff = 2 * ParticleRadius - (Max.X - a.Position.X) * SimScale;
				if (diff > Epsilon)
				{
					float adjustment = ExteriorStiffness * diff - ExteriorDampening * Vector3.Dot(a.VelocityEval, Vector3.Left);
					acceleration.X -= adjustment;
				}


				diff = 2 * ParticleRadius - (a.Position.Y - Min.Y) * SimScale;
				if (diff > Epsilon)
				{
					float adjustment = ExteriorStiffness * diff - ExteriorDampening * Vector3.Dot(a.VelocityEval, Vector3.Up);
					acceleration.Y += adjustment;
				}

				diff = 2 * ParticleRadius - (Max.Y - a.Position.Y) * SimScale;
				if (diff > Epsilon)
				{
					float adjustment = ExteriorStiffness * diff - ExteriorDampening * Vector3.Dot(a.VelocityEval, Vector3.Down);
					acceleration.Y -= adjustment;
				}


				diff = 2 * ParticleRadius - (a.Position.Z - Min.Z) * SimScale;
				if (diff > Epsilon)
				{
					float adjustment = ExteriorStiffness * diff - ExteriorDampening * Vector3.Dot(a.VelocityEval, Vector3.Backward);
					acceleration.Z += adjustment;
				}

				diff = 2 * ParticleRadius - (Max.Z - a.Position.Z) * SimScale;
				if (diff > Epsilon)
				{
					float adjustment = ExteriorStiffness * diff - ExteriorDampening * Vector3.Dot(a.VelocityEval, Vector3.Forward);
					acceleration.Z -= adjustment;
				}


				Vector3 nextVelocity = a.Velocity + ((acceleration + Gravity) * DT);

				a.VelocityEval = (a.Velocity + nextVelocity) * 0.5f;
				a.Velocity = nextVelocity;

				a.Position += nextVelocity * (DT / SimScale);

			}

		}
	}
}
