using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public float Scale
        {
            get { return SimScale; }
        }

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

        public float Radius
        {
            get { return SmoothRadius; }
        }

		float R2;
		float Poly6Kern;
		float SpikyKern;
		float LapKern;

        Vector3 Min = new Vector3(-15f, -15f, -15f);
        Vector3 Max = new Vector3(15f, 15f, 15f);

        public Vector3 PositionMin
        {
            get { return Min; }
        }
        public Vector3 PositionMax
        {
            get { return Max; }
        }

		Vector3 InitMin = new Vector3(0f, -10f, 0f);
        Vector3 InitMax = new Vector3(8f, 15f, 8f);

		//Vector3 Min = new Vector3(-25f, 0f, -25f);
		//Vector3 Max = new Vector3(25f, 100f, 25f);

		//Vector3 InitMin = new Vector3(-20f, 0f, -20f);
		//Vector3 InitMax = new Vector3(-5f, 30f, 5f);


		internal Water[] water;

		internal List<Water>[, ,] watergrid;

		Vector3 GridMin;
		Vector3 GridMax;
		Vector3 GridSize;
        Vector3 GridResolution = Vector3.Zero;

		public bool UseGrid = true;

		public Stopwatch timer;

		public WaterBody()
		{
			CalcKernels();
			Spawn();
			if (UseGrid)
			{
				SetupGrid();
			}
			timer = new Stopwatch();
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

		private void SetupGrid()
		{
			float cellsize = SmoothRadius * 2;
			double res = SimScale / cellsize;
			double size = cellsize / SimScale;

			GridMin = Min;
			GridMin.X -= 1f; GridMin.Y -= 1f; GridMin.Z -= 1f;
			GridMax = Max;
			GridMax.X += 1f; GridMax.Y += 1f; GridMax.Z += 1f;

			GridSize = GridMax - GridMin;

			GridResolution.X = (int)Math.Ceiling(GridSize.X * res);
			GridResolution.Y = (int)Math.Ceiling(GridSize.Y * res);
			GridResolution.Z = (int)Math.Ceiling(GridSize.Z * res);

			GridSize.X = (int)Math.Ceiling(GridResolution.X * size);
			GridSize.Y = (int)Math.Ceiling(GridResolution.Y * size);
			GridSize.Z = (int)Math.Ceiling(GridResolution.Z * size);

            //simCellSize = GridSize / GridResolution;

			watergrid = new List<Water>[(int)GridSize.X, (int)GridSize.Y, (int)GridSize.Z];
			for (int x = 0; x < (int)GridSize.X; ++x)
			{
				for (int y = 0; y < (int)GridSize.Y; ++y)
				{
					for (int z = 0; z < (int)GridSize.Z; ++z)
					{
						watergrid[x,y,z] = new List<Water>();
					}
				}
			}
		}

		private void GridParticles()
		{
			foreach (List<Water> list in watergrid)
			{
				list.Clear();
			}

			Vector3 delta = GridResolution / GridSize;
			foreach (Water item in water)
			{
                int x = (int)((item.Position.X - GridMin.X) * delta.X);
				int y = (int)((item.Position.Y - GridMin.Y) * delta.Y);
				int z = (int)((item.Position.Z - GridMin.Z) * delta.Z);
                if( x >= 0 && y >= 0 && z >= 0 &&
                    x < watergrid.GetLength(0) && y < watergrid.GetLength(1) && z < watergrid.GetLength(2)) {
				    watergrid[x, y, z].Add(item);
                }
			}
		}

		public void Update()
		{
			timer.Start();

			if (UseGrid)
			{
				GridParticles();
				findNeighborsGrid();
			}
			else
			{
				findNeighbors();
			}

			doPressure();
			doForces();
			
			doStuff();


			timer.Stop();
			Console.WriteLine( "Update: " + timer.Elapsed.TotalSeconds);
			timer.Reset();
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

		private void findNeighborsGrid()
		{
			for (int x = 0; x < (int)GridSize.X; ++x)
			{
				for (int y = 0; y < (int)GridSize.Y; ++y)
				{
					for (int z = 0; z < (int)GridSize.Z; ++z)
					{
						foreach (Water a in watergrid[x,y,z])
						{
							a.Neighbors.Clear();
							foreach (Water b in watergrid[x, y, z])
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
							if (x > 0)
							{
								foreach (Water b in watergrid[x - 1, y, z])
								{
									if (a == b)
									{
										continue;
									}
									float dx = (a.Position.X - b.Position.X) * SimScale;
									float dy = (a.Position.Y - b.Position.Y) * SimScale;
									float dz = (a.Position.Z - b.Position.Z) * SimScale;
									float dsq = (dx * dx + dy * dy + dz * dz);
									if (R2 > dsq)
									{
										a.Neighbors.Add(b);
									}
								}
								if (y > 0)
								{
									foreach (Water b in watergrid[x - 1, y - 1, z])
									{
										if (a == b)
										{
											continue;
										}
										float dx = (a.Position.X - b.Position.X) * SimScale;
										float dy = (a.Position.Y - b.Position.Y) * SimScale;
										float dz = (a.Position.Z - b.Position.Z) * SimScale;
										float dsq = (dx * dx + dy * dy + dz * dz);
										if (R2 > dsq)
										{
											a.Neighbors.Add(b);
										}
									}
									if (z > 0)
									{
										foreach (Water b in watergrid[x - 1, y - 1, z - 1])
										{
											if (a == b)
											{
												continue;
											}
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
									if (z < GridSize.Z - 1)
									{
										foreach (Water b in watergrid[x - 1, y - 1, z + 1])
										{
											if (a == b)
											{
												continue;
											}
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
								if (y < GridSize.Y - 1)
								{
									foreach (Water b in watergrid[x - 1, y + 1, z])
									{
										if (a == b)
										{
											continue;
										}
										float dx = (a.Position.X - b.Position.X) * SimScale;
										float dy = (a.Position.Y - b.Position.Y) * SimScale;
										float dz = (a.Position.Z - b.Position.Z) * SimScale;
										float dsq = (dx * dx + dy * dy + dz * dz);
										if (R2 > dsq)
										{
											a.Neighbors.Add(b);
										}
									}
									if (z > 0)
									{
										foreach (Water b in watergrid[x - 1, y + 1, z - 1])
										{
											if (a == b)
											{
												continue;
											}
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
									if (z < GridSize.Z - 1)
									{
										foreach (Water b in watergrid[x - 1, y + 1, z + 1])
										{
											if (a == b)
											{
												continue;
											}
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
							}
							if (x < GridSize.X - 1)
							{
								foreach (Water b in watergrid[x + 1, y, z])
								{
									if (a == b)
									{
										continue;
									}
									float dx = (a.Position.X - b.Position.X) * SimScale;
									float dy = (a.Position.Y - b.Position.Y) * SimScale;
									float dz = (a.Position.Z - b.Position.Z) * SimScale;
									float dsq = (dx * dx + dy * dy + dz * dz);
									if (R2 > dsq)
									{
										a.Neighbors.Add(b);
									}
								}
								if (y > 0)
								{
									foreach (Water b in watergrid[x + 1, y - 1, z])
									{
										if (a == b)
										{
											continue;
										}
										float dx = (a.Position.X - b.Position.X) * SimScale;
										float dy = (a.Position.Y - b.Position.Y) * SimScale;
										float dz = (a.Position.Z - b.Position.Z) * SimScale;
										float dsq = (dx * dx + dy * dy + dz * dz);
										if (R2 > dsq)
										{
											a.Neighbors.Add(b);
										}
									}
									if (z > 0)
									{
										foreach (Water b in watergrid[x + 1, y - 1, z - 1])
										{
											if (a == b)
											{
												continue;
											}
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
									if (z < GridSize.Z - 1)
									{
										foreach (Water b in watergrid[x + 1, y - 1, z + 1])
										{
											if (a == b)
											{
												continue;
											}
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
								if (y < GridSize.Y - 1)
								{
									foreach (Water b in watergrid[x + 1, y + 1, z])
									{
										if (a == b)
										{
											continue;
										}
										float dx = (a.Position.X - b.Position.X) * SimScale;
										float dy = (a.Position.Y - b.Position.Y) * SimScale;
										float dz = (a.Position.Z - b.Position.Z) * SimScale;
										float dsq = (dx * dx + dy * dy + dz * dz);
										if (R2 > dsq)
										{
											a.Neighbors.Add(b);
										}
									}
									if (z > 0)
									{
										foreach (Water b in watergrid[x + 1, y + 1, z - 1])
										{
											if (a == b)
											{
												continue;
											}
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
									if (z < GridSize.Z - 1)
									{
										foreach (Water b in watergrid[x + 1, y + 1, z + 1])
										{
											if (a == b)
											{
												continue;
											}
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
							}

							if (y > 0)
							{
								foreach (Water b in watergrid[x, y - 1, z])
								{
									if (a == b)
									{
										continue;
									}
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
							if (y < GridSize.Y - 1)
							{
								foreach (Water b in watergrid[x, y + 1, z])
								{
									if (a == b)
									{
										continue;
									}
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

							if (z > 0)
							{
								foreach (Water b in watergrid[x, y, z - 1])
								{
									if (a == b)
									{
										continue;
									}
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
							if (z < GridSize.Z - 1)
							{
								foreach (Water b in watergrid[x, y, z + 1])
								{
									if (a == b)
									{
										continue;
									}
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
