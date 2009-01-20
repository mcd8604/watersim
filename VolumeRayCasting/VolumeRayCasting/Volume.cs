using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace VolumeRayCasting
{
    class Volume : Primitive
    {
        //public const float RANGE = 30;
        //private const float gridSize = RANGE / 32;
        private const int GRID_DIMENSION = 16;
        //private float cutOff = 2f;
        private Vector3 gridCellSize;
        private Vector3 gridCellSizeInv;

        private List<Water>[, ,] waterGrid;

        private float isoLevel;

        //private float pointVal;

        private Vector3[, ,] gridPoints;
        private float[, ,] gridValues;
        private BoundingBox[, ,] gridBoxes;

        private WaterBody waterBody;

        private VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[0];
        public VertexPositionNormalTexture[] Vertices
        {
            get { return vertices; }
        }

        private bool paused = false;
        public bool Paused
        {
            get { return paused; }
            set { paused = value; }
        }

        private BoundingBox boundingBox;

        public Volume(WaterBody waterBody)
        {
            this.waterBody = waterBody;
            initGrid();
        }

        private void initGrid()
        {
            boundingBox = new BoundingBox(waterBody.PositionMin, waterBody.PositionMax);

            isoLevel = waterBody.Radius / waterBody.Scale;

            gridCellSize = (waterBody.PositionMax - waterBody.PositionMin) / GRID_DIMENSION;
            gridCellSizeInv = new Vector3(1 / gridCellSize.X, 1 / gridCellSize.Y, 1 / gridCellSize.Z);

            gridPoints = new Vector3[GRID_DIMENSION, GRID_DIMENSION, GRID_DIMENSION];
            gridValues = new float[GRID_DIMENSION, GRID_DIMENSION, GRID_DIMENSION];
            gridBoxes = new BoundingBox[GRID_DIMENSION, GRID_DIMENSION, GRID_DIMENSION];
            waterGrid = new List<Water>[GRID_DIMENSION, GRID_DIMENSION, GRID_DIMENSION];

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
                //int minX = (int)((Math.Max(waterBody.PositionMin.X, w.Position.X - radius) - waterBody.PositionMin.X) * gridCellSizeInv.X);
                //int minY = (int)((Math.Max(waterBody.PositionMin.Y, w.Position.Y - radius) - waterBody.PositionMin.Y) * gridCellSizeInv.Y);
                //int minZ = (int)((Math.Max(waterBody.PositionMin.Z, w.Position.Z - radius) - waterBody.PositionMin.Z) * gridCellSizeInv.Z);

                //int maxX = (int)((Math.Min(waterBody.PositionMax.X, w.Position.X + radius) - waterBody.PositionMin.X) * gridCellSizeInv.X);
                //int maxY = (int)((Math.Min(waterBody.PositionMax.Y, w.Position.Y + radius) - waterBody.PositionMin.Y) * gridCellSizeInv.Y);
                //int maxZ = (int)((Math.Min(waterBody.PositionMax.Z, w.Position.Z + radius) - waterBody.PositionMin.Z) * gridCellSizeInv.Z);

                //for (int x = minX; x < maxX; ++x)
                //{
                //    for (int y = minY; y < maxY; ++y)
                //    {
                //        for (int z = minZ; z < maxZ; ++z)
                //        {
                //            waterGrid[x, y, z].Add(w);
                //        }
                //    }
                //}

                //Vector3 cellMin = gridPoints[x, y, z];
                //Vector3 cellMax = cellMin + gridCellSize;

                int x = (int)((w.Position.X - waterBody.PositionMin.X) * gridCellSizeInv.X);
                int y = (int)((w.Position.Y - waterBody.PositionMin.Y) * gridCellSizeInv.Y);
                int z = (int)((w.Position.Z - waterBody.PositionMin.Z) * gridCellSizeInv.Z);

                if (x >= 0 &&
                    y >= 0 &&
                    z >= 0 &&
                    x < waterGrid.GetLength(0) &&
                    y < waterGrid.GetLength(1) &&
                    z < waterGrid.GetLength(2))
                {
                    waterGrid[x, y, z].Add(w);
                }

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
                            value += 1 / distSq;
                        }

                        gridValues[x, y, z] = value;
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
        Stopwatch sw = new Stopwatch();
#endif

        internal void Update()
        {
            if (!paused)
            {
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

        public override Vector3 Center
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override float? Intersects(Ray ray)
        {
            float? macroDist = ray.Intersects(boundingBox);

            if(macroDist != null) 
            {
                // travel along the ray until we find a cell with a value

                float delta = gridCellSize.X ;
                float curDist = (float)macroDist;
                Vector3 microIntersection;
                int xIndex;
                int yIndex;
                int zIndex;
                ContainmentType containType;

                do
                {
                    microIntersection = ray.Position + (ray.Direction * curDist);
                    xIndex = (int)Math.Round(((microIntersection.X - waterBody.PositionMin.X) / gridCellSize.X));
                    yIndex = (int)Math.Round(((microIntersection.Y - waterBody.PositionMin.Y) / gridCellSize.Y));
                    zIndex = (int)Math.Round(((microIntersection.Z - waterBody.PositionMin.Z) / gridCellSize.Z));

                    if (xIndex < 0)
                        xIndex = 0;

                    if (yIndex < 0)
                        yIndex = 0;

                    if (zIndex < 0)
                        zIndex = 0;

                    if(xIndex > gridBoxes.GetLength(0) - 1)
                        xIndex = gridBoxes.GetLength(0) - 1;
                    
                    if(yIndex > gridBoxes.GetLength(1) - 1)
                        yIndex = gridBoxes.GetLength(1) - 1;
                    
                    if(zIndex > gridBoxes.GetLength(2) - 1)
                        zIndex = gridBoxes.GetLength(2) - 1;

                    if (gridValues[xIndex, yIndex, zIndex] > 0)
                        return ray.Intersects(gridBoxes[xIndex, yIndex, zIndex]);
                
                    curDist += delta;
                    containType = boundingBox.Contains(microIntersection);
                } while (containType == ContainmentType.Contains || containType == ContainmentType.Intersects);
            }
            return null;
        }

        public override Vector3 GetIntersectNormal(Vector3 intersectPoint)
        {
            return Vector3.Zero;
        }
    }
}
