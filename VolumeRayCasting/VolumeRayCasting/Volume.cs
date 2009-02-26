using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using RayTracer;

namespace VolumeRayCasting
{
    class Volume : RayTraceable
    {
        //public const float RANGE = 30;
        //private const float gridSize = RANGE / 32;
        private const int GRID_DIMENSION = 64;
        //private float cutOff = 2f;
        private Vector3 gridCellSize;
        private Vector3 gridCellSizeInv;

        private float isoLevel;

        //private float pointVal;

        private WaterBody waterBody;

        private List<Water>[, ,] waterGrid;
        private Vector3[, ,] gridPoints;
        private Vector3[, ,] gradient;
        private float[, ,] gridValues;
        private BoundingBox[, ,] gridBoxes;

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

        Vector3 center;
        float totalVal;

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

            totalVal = (float)Math.Pow(GRID_DIMENSION, 3);

            center = new Vector3(GRID_DIMENSION);

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
                            if (value > 1)
                            {
                                value = 1;
                                break;
                            }
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
                        //(min 6, max 26)

                        Vector3 avg = Vector3.Zero;

                        int numVectors = 0;

                        int minX = Math.Max(0, x - 1);
                        int minY = Math.Max(0, y - 1);
                        int minZ = Math.Max(0, z - 1);
                        int maxX = Math.Min(GRID_DIMENSION - 1, x + 1);
                        int maxY = Math.Min(GRID_DIMENSION - 1, y + 1);
                        int maxZ = Math.Min(GRID_DIMENSION - 1, z + 1);

                        for (int nx = minX; nx < maxX; ++nx)
                        {
                            for (int ny = minY; ny < maxY; ++ny)
                            {
                                for (int nz = minZ; nz < maxZ; ++nz)
                                {
                                    if (x != nx || y != ny || z != nz)
                                    {
                                        avg += (gridPoints[x, y, z] - gridPoints[nx, ny, nz]) * (gridValues[x, y, z] - gridValues[nx, ny, nz]);
                                        ++numVectors;
                                    }
                                }
                            }
                        }

                        if (numVectors != 0 && avg != Vector3.Zero)
                            gradient[x, y, z] = avg / numVectors;
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
            return ray.Intersects(boundingBox);

            //float? macroDist = ray.Intersects(boundingBox);

            //if (macroDist != null)
            //{
            //    // travel along the ray until we find a cell with a value

            //    float delta = gridCellSize.X / 4;
            //    float curDist = (float)macroDist;
            //    Vector3 microIntersection;
            //    int xIndex;
            //    int yIndex;
            //    int zIndex;
            //    ContainmentType containType;

            //    do
            //    {
            //        microIntersection = ray.Position + (ray.Direction * curDist);
            //        xIndex = (int)((microIntersection.X - waterBody.PositionMin.X) / gridCellSize.X);
            //        yIndex = (int)((microIntersection.Y - waterBody.PositionMin.Y) / gridCellSize.Y);
            //        zIndex = (int)((microIntersection.Z - waterBody.PositionMin.Z) / gridCellSize.Z);

            //        if (xIndex < 0)
            //            xIndex = 0;

            //        if (yIndex < 0)
            //            yIndex = 0;

            //        if (zIndex < 0)
            //            zIndex = 0;

            //        if (xIndex > gridBoxes.GetLength(0) - 1)
            //            xIndex = gridBoxes.GetLength(0) - 1;

            //        if (yIndex > gridBoxes.GetLength(1) - 1)
            //            yIndex = gridBoxes.GetLength(1) - 1;

            //        if (zIndex > gridBoxes.GetLength(2) - 1)
            //            zIndex = gridBoxes.GetLength(2) - 1;

            //        if (gridValues[xIndex, yIndex, zIndex] > 0)
            //            return ray.Intersects(gridBoxes[xIndex, yIndex, zIndex]);

            //        curDist += delta;
            //        containType = boundingBox.Contains(microIntersection);
            //    } while (containType == ContainmentType.Contains || containType == ContainmentType.Intersects);
            //}
            //return null;
        }

        public override Vector3 GetIntersectNormal(Ray ray, float dist)
        {
            Vector3 intersection = Vector3.Zero;
            getMicroIntersection(ref ray, dist, ref intersection);

            int xIndex = (int)((intersection.X - waterBody.PositionMin.X) / gridCellSize.X);
            int yIndex = (int)((intersection.Y - waterBody.PositionMin.Y) / gridCellSize.Y);
            int zIndex = (int)((intersection.Z - waterBody.PositionMin.Z) / gridCellSize.Z);

            if (xIndex > gridPoints.GetLength(0) - 2)
                xIndex = gridPoints.GetLength(0) - 2;

            if (yIndex > gridPoints.GetLength(1) - 2)
                yIndex = gridPoints.GetLength(1) - 2;

            if (zIndex > gridPoints.GetLength(2) - 2)
                zIndex = gridPoints.GetLength(2) - 2;

            return Vector3.Negate(gradient[xIndex, yIndex, zIndex]);

            //return Vector3.Zero;
        }


        public override Vector4  GetLighting(Vector4 ambientLight, Ray ray, float dist, Light l, Vector3 viewVector)
        {
            // sum the density
            
            float density = 0f;
            float distDelta = gridCellSize.X;
            float curDist = (float)dist;
            Vector3 microIntersection = Vector3.Zero;
            int xIndex;
            int yIndex;
            int zIndex;
            ContainmentType containType;

            int count = 0;

            getMicroIntersection(ref ray, curDist, ref microIntersection);
            containType = boundingBox.Contains(microIntersection);

            Vector3 surfaceIntersection = Vector3.Zero;
            Vector3 intersectNormal = Vector3.Zero;

            bool surfaceFound = false;

            while (containType == ContainmentType.Contains || containType == ContainmentType.Intersects)
            {
                xIndex = (int)((microIntersection.X - waterBody.PositionMin.X) / gridCellSize.X);
                yIndex = (int)((microIntersection.Y - waterBody.PositionMin.Y) / gridCellSize.Y);
                zIndex = (int)((microIntersection.Z - waterBody.PositionMin.Z) / gridCellSize.Z);

                if (xIndex > gridPoints.GetLength(0) - 2)
                    xIndex = gridPoints.GetLength(0) - 2;

                if (yIndex > gridPoints.GetLength(1) - 2)
                    yIndex = gridPoints.GetLength(1) - 2;

                if (zIndex > gridPoints.GetLength(2) - 2)
                    zIndex = gridPoints.GetLength(2) - 2;

                // trilinear interpolation

                Vector3 c000 = gridPoints[xIndex, yIndex, zIndex];
                Vector3 c111 = gridPoints[xIndex + 1, yIndex + 1, zIndex + 1];
                Vector3 delta = c111 - c000;
                Vector3 deltaMin = microIntersection - c000;
                Vector3 deltaMax = c111 - microIntersection;

                float d000 = gridValues[xIndex, yIndex, zIndex];
                float d001 = gridValues[xIndex, yIndex, zIndex + 1];
                float d010 = gridValues[xIndex, yIndex + 1, zIndex];
                float d011 = gridValues[xIndex, yIndex + 1, zIndex + 1];
                float d100 = gridValues[xIndex + 1, yIndex, zIndex + 1];
                float d101 = gridValues[xIndex + 1, yIndex, zIndex + 1];
                float d110 = gridValues[xIndex + 1, yIndex + 1, zIndex];
                float d111 = gridValues[xIndex + 1, yIndex + 1, zIndex + 1];

                // perform linear interpolation between:
                //   C000 and C100 to find C00,
                //   C001 and C101 to find C01,
                //   C011 and C111 to find C11,
                //   C010 and C110 to find C10.       

                float deltaXMin = (deltaMin.X / delta.X);
                float deltaXMax = (deltaMax.X / delta.X);

                float d00 = deltaXMax * d000 + deltaXMin * d100;
                float d01 = deltaXMax * d001 + deltaXMin * d101;
                float d11 = deltaXMax * d011 + deltaXMin * d111;
                float d10 = deltaXMax * d010 + deltaXMin * d110;

                // perform linear interpolation between:
                //      C00 and C10 to find C0,
                //      C01 and C11 to find C1. 

                float deltaYMin = (deltaMin.Y / delta.Y);
                float deltaYMax = (deltaMax.Y / delta.Y);

                float d0 = deltaYMax * d00 + deltaYMin * d10;
                float d1 = deltaYMax * d01 + deltaYMin * d11;

                // perform linear interpolation between: 
                //      C0 and C1 to find c

                float deltaZMin = (deltaMin.Z / delta.Z);
                float deltaZMax = (deltaMax.Z / delta.Z);

                density += deltaZMax * d0 + deltaZMin * d1;

                if (!surfaceFound && density > 1)
                {
                    surfaceIntersection = ray.Position + (ray.Direction * curDist);

                    // interp normal 

                    Vector3 n000 = gradient[xIndex, yIndex, zIndex];
                    Vector3 n001 = gradient[xIndex, yIndex, zIndex + 1];
                    Vector3 n010 = gradient[xIndex, yIndex + 1, zIndex];
                    Vector3 n011 = gradient[xIndex, yIndex + 1, zIndex + 1];
                    Vector3 n100 = gradient[xIndex + 1, yIndex, zIndex + 1];
                    Vector3 n101 = gradient[xIndex + 1, yIndex, zIndex + 1];
                    Vector3 n110 = gradient[xIndex + 1, yIndex + 1, zIndex];
                    Vector3 n111 = gradient[xIndex + 1, yIndex + 1, zIndex + 1];

                    Vector3 n00 = Vector3.Lerp(n000, n100, deltaXMin);//deltaXMax * n000 + deltaXMin * n100;
                    Vector3 n01 = Vector3.Lerp(n001, n101, deltaXMin);//deltaXMax * n001 + deltaXMin * n101;
                    Vector3 n11 = Vector3.Lerp(n011, n111, deltaXMin);//deltaXMax * n011 + deltaXMin * n111;
                    Vector3 n10 = Vector3.Lerp(n010, n110, deltaXMin);//deltaXMax * n010 + deltaXMin * n110;

                    //Vector3 n0 = deltaYMax * n00 + deltaYMin * n10;
                    //Vector3 n1 = deltaYMax * n01 + deltaYMin * n11;
                    Vector3 n0 = Vector3.Lerp(n00, n10, deltaYMin);
                    Vector3 n1 = Vector3.Lerp(n01, n11, deltaYMin);

                    //intersectNormal = deltaZMax * n0 + deltaZMin * n1;
                    intersectNormal = Vector3.Normalize(Vector3.Negate(Vector3.Lerp(n0, n1, deltaZMin)));

                    surfaceFound = true;
                }

                curDist += distDelta;

                ++count;

                getMicroIntersection(ref ray, curDist, ref microIntersection);
                containType = boundingBox.Contains(microIntersection);
            }

            //density /= (curDist - dist) / distDelta;

            density /= count;

            Vector4 baseColor = base.calculateAmbient(ambientLight, ray, dist) * density;

            //if (density > 1) density = 1;

            if (surfaceFound)
            {
                Vector3 lightVector = Vector3.Normalize(l.Position - surfaceIntersection);
                baseColor += calculateDiffuse(surfaceIntersection, intersectNormal, l, lightVector);
                baseColor += calculateSpecular(surfaceIntersection, intersectNormal, l, lightVector, viewVector);
            }

            return baseColor;// *waterBody.Scale;
        }

        private void getMicroIntersection(ref Ray ray, float curDist, ref Vector3 microIntersection)
        {
            microIntersection = ray.Position + (ray.Direction * curDist);

            // round off floating point error

            if (microIntersection.X > boundingBox.Max.X && microIntersection.X - boundingBox.Max.X < 0.001)
                microIntersection.X = boundingBox.Max.X;

            if (microIntersection.Y > boundingBox.Max.Y && microIntersection.Y - boundingBox.Max.Y < 0.001)
                microIntersection.Y = boundingBox.Max.Y;

            if (microIntersection.Z > boundingBox.Max.Z && microIntersection.Z - boundingBox.Max.Z < 0.001)
                microIntersection.Z = boundingBox.Max.Z;
        }
    }
}
