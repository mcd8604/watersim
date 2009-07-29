using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using WaterLib;

namespace WaterLib
{
    public class Volume
    {
        //public const float RANGE = 30;
        //private const float gridSize = RANGE / 32;
        public const int GRID_DIMENSION = 64;

        //private float cutOff = 2f;
        private Vector3 gridCellSize;
        public Vector3 GridCellSize
        {
            get { return gridCellSize; }
        }
        private Vector3 gridCellSizeInv;

        private float isoLevel;
        public float IsoLevel
        {
            get { return isoLevel; }
        }

        //private float pointVal;

        private WaterBody waterBody;
        public Vector3 PositionMax
        {
            get { return waterBody.PositionMax; }
        }
        public Vector3 PositionMin
        {
            get { return waterBody.PositionMin; }
        }

        private List<Water>[, ,] waterGrid;

        private Vector3[, ,] gridPoints;
        public Vector3[, ,] GridPoints
        {
            get { return gridPoints; }
        }

        private Vector3[, ,] gradient;
        public Vector3[, ,] Gradient
        {
            get { return gradient; }
        }

        private float[, ,] gridValues;
        public float[, ,] GridValues
        {
            get { return gridValues; }
        }

        private BoundingBox[, ,] gridBoxes;

        private bool paused = false;
        public bool Paused
        {
            get { return paused; }
            set { paused = value; }
        }

        private BoundingBox boundingBox;
        public BoundingBox VolumeBB
        {
            get { return boundingBox; }
        }

        public Volume()
        {
            this.waterBody = new WaterBody();
            initGrid();
        }

        //Vector3 center;
        //float totalVal;

        private void initGrid()
        {
            boundingBox = new BoundingBox(waterBody.PositionMin, waterBody.PositionMax);

            isoLevel = waterBody.Radius / waterBody.Scale;

            gridCellSize = (waterBody.PositionMax - waterBody.PositionMin) / GRID_DIMENSION;
            gridCellSizeInv = new Vector3(1 / gridCellSize.X, 1 / gridCellSize.Y, 1 / gridCellSize.Z);

            gridPoints = new Vector3[GRID_DIMENSION, GRID_DIMENSION, GRID_DIMENSION];
            gradient = new Vector3[GRID_DIMENSION, GRID_DIMENSION, GRID_DIMENSION];
            gridValues = new float[GRID_DIMENSION, GRID_DIMENSION, GRID_DIMENSION];
            gridBoxes = new BoundingBox[GRID_DIMENSION, GRID_DIMENSION, GRID_DIMENSION];
            waterGrid = new List<Water>[GRID_DIMENSION, GRID_DIMENSION, GRID_DIMENSION];

            //totalVal = (float)Math.Pow(GRID_DIMENSION, 3);
            //center = new Vector3(GRID_DIMENSION);

            for (int x = 0; x < GRID_DIMENSION; ++x)
            {
                for (int y = 0; y < GRID_DIMENSION; ++y)
                {
                    for (int z = 0; z < GRID_DIMENSION; ++z)
                    {
                        gridPoints[x, y, z] = new Vector3(
                            waterBody.PositionMin.X + x * gridCellSize.X,
                            waterBody.PositionMin.Y + y * gridCellSize.Y,
                            waterBody.PositionMin.Z + z * gridCellSize.Z);
                        gridBoxes[x, y, z] = new BoundingBox(gridPoints[x, y, z], gridPoints[x, y, z] + gridCellSize);

                        // Init static values
                        //gridValues[x, y, z] = 1 / Vector3.Distance(new Vector3(x, y, z), center);

                        waterGrid[x, y, z] = new List<Water>();
                    }
                }
            }
        }

        private void updateGrid()
        {
            // Clear cells

            for (int x = 0; x < GRID_DIMENSION; ++x)
            {
                for (int y = 0; y < GRID_DIMENSION; ++y)
                {
                    for (int z = 0; z < GRID_DIMENSION; ++z)
                    {
                        waterGrid[x, y, z].Clear();
                    }
                }
            }

            // Put water in cells

            float radius = waterBody.Radius / waterBody.Scale;

            foreach (Water w in waterBody.water)
            {
                int minX = (int)((Math.Max(waterBody.PositionMin.X, w.Position.X - radius) - waterBody.PositionMin.X) * gridCellSizeInv.X);
                int minY = (int)((Math.Max(waterBody.PositionMin.Y, w.Position.Y - radius) - waterBody.PositionMin.Y) * gridCellSizeInv.Y);
                int minZ = (int)((Math.Max(waterBody.PositionMin.Z, w.Position.Z - radius) - waterBody.PositionMin.Z) * gridCellSizeInv.Z);

                int maxX = (int)((Math.Min(waterBody.PositionMax.X, w.Position.X + radius) - waterBody.PositionMin.X) * gridCellSizeInv.X);
                int maxY = (int)((Math.Min(waterBody.PositionMax.Y, w.Position.Y + radius) - waterBody.PositionMin.Y) * gridCellSizeInv.Y);
                int maxZ = (int)((Math.Min(waterBody.PositionMax.Z, w.Position.Z + radius) - waterBody.PositionMin.Z) * gridCellSizeInv.Z);

                for (int x = minX; x < maxX; ++x)
                {
                    for (int y = minY; y < maxY; ++y)
                    {
                        for (int z = minZ; z < maxZ; ++z)
                        {
                            waterGrid[x, y, z].Add(w);
                        }
                    }
                }

                //Vector3 cellMin = gridPoints[x, y, z];
                //Vector3 cellMax = cellMin + gridCellSize;

                //int x = (int)((w.Position.X - waterBody.PositionMin.X) * gridCellSizeInv.X);
                //int y = (int)((w.Position.Y - waterBody.PositionMin.Y) * gridCellSizeInv.Y);
                //int z = (int)((w.Position.Z - waterBody.PositionMin.Z) * gridCellSizeInv.Z);

                //if (x >= 0 &&
                //    y >= 0 &&
                //    z >= 0 &&
                //    x < waterGrid.GetLength(0) &&
                //    y < waterGrid.GetLength(1) &&
                //    z < waterGrid.GetLength(2))
                //{
                //    waterGrid[x, y, z].Add(w);
                //}

                //if (w.Position.X >= cellMin.X &&
                //    w.Position.Y >= cellMin.Y &&
                //    w.Position.Z >= cellMin.Z &&

                //    w.Position.X < cellMax.X &&
                //    w.Position.Y < cellMax.Y &&
                //    w.Position.Z < cellMax.Z)
                //{
                //    waterGrid[x, y, z].Add(w);
                //}
            }

            // Calculate grid values

            for (int x = 0; x < GRID_DIMENSION; ++x)
            {
                for (int y = 0; y < GRID_DIMENSION; ++y)
                {
                    for (int z = 0; z < GRID_DIMENSION; ++z)
                    {
                        float value = 0f;



                        foreach (Water w in waterGrid[x, y, z])
                        {
                            float distSq = Vector3.DistanceSquared(gridPoints[x, y, z] + (gridCellSize / 2), w.Position);

                            //float dist = Vector3.DistanceSquared(gridPoints[x, y, z] + (gridCellSize / 2), w.Position);
                            //if(dist <= cutOff)
                            //value += w.density;// / distSq;
                            value += 1 / distSq;
                            //if (value > 1)
                            //{
                            //    value = 1;
                            //    break;
                            //}
                        }

                        gridValues[x, y, z] = value;
                    }
                }
            }

            calculateGradient();

        }

        private void calculateGradient()
        {
            for (int x = 0; x < GRID_DIMENSION; ++x)
            {
                for (int y = 0; y < GRID_DIMENSION; ++y)
                {
                    for (int z = 0; z < GRID_DIMENSION; ++z)
                    {
                        //get average of all vectors from current point to each neighboring point 

                        Vector3 avg = Vector3.Zero;

                        int minX = Math.Max(0, x - 1);
                        int minY = Math.Max(0, y - 1);
                        int minZ = Math.Max(0, z - 1);
                        int maxX = Math.Min(GRID_DIMENSION - 1, x + 1);
                        int maxY = Math.Min(GRID_DIMENSION - 1, y + 1);
                        int maxZ = Math.Min(GRID_DIMENSION - 1, z + 1);

                        for (int nx = minX; nx <= maxX; ++nx)
                        {
                            for (int ny = minY; ny <= maxY; ++ny)
                            {
                                for (int nz = minZ; nz <= maxZ; ++nz)
                                {
                                    if (x != nx || y != ny || z != nz)
                                    {
                                        avg += (gridPoints[x, y, z] - gridPoints[nx, ny, nz]) * (gridValues[x, y, z] - gridValues[nx, ny, nz]);
                                    }
                                }
                            }
                        }

                        if (avg != Vector3.Zero)
                            gradient[x, y, z] = Vector3.Normalize(avg);
                    }
                }
            }
        }

        
#if DEBUG
        private double gridTime;
        public double GridTime
        {
            get { return gridTime; }
        }
        public double WaterTime
        {
            get { return waterBody.UpdateTime; }
        }
        Stopwatch sw = new Stopwatch();
#endif

        public void Update()
        {
            if (!paused)
            {
                waterBody.Update();
#if DEBUG
                sw.Start();
#endif
                updateGrid();                
#if DEBUG
                sw.Stop();
                gridTime = sw.Elapsed.TotalSeconds;
                sw.Reset();
#endif
            }
        }
    }
}
