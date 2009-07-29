using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using RayTracer;
using WaterLib;

namespace VolumeRayCastingCPU
{
    class VolumeRayTraceable : RayTraceable
    {
        Volume volume;

        public Vector3 PositionMin
        {
            get { return volume.PositionMin; }
        }

        public Vector3 PositionMax
        {
            get { return volume.PositionMax; }
        }

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

        public VolumeRayTraceable()
        {
            volume = new Volume();
        }
        
#if DEBUG
        public double GridTime
        {
            get { return volume.GridTime; }
        }
#endif

        internal void Update()
        {
            volume.Update();
        }

        public override float? Intersects(Ray ray)
        {
            float? macroDist = ray.Intersects(volume.VolumeBB);
            if(macroDist == null)
                return null;

            float distDelta = volume.GridCellSize.X;
            float curDist = (float)macroDist;
            Vector3 microIntersection = Vector3.Zero;
            int xIndex;
            int yIndex;
            int zIndex;
            ContainmentType containType;

            getMicroIntersection(ref ray, curDist, ref microIntersection);
            containType = volume.VolumeBB.Contains(microIntersection);

            while (containType == ContainmentType.Contains || containType == ContainmentType.Intersects)
            {
                xIndex = (int)((microIntersection.X - volume.PositionMin.X) / volume.GridCellSize.X);
                yIndex = (int)((microIntersection.Y - volume.PositionMin.Y) / volume.GridCellSize.Y);
                zIndex = (int)((microIntersection.Z - volume.PositionMin.Z) / volume.GridCellSize.Z);

                if (xIndex > volume.GridPoints.GetLength(0) - 2)
                    xIndex = volume.GridPoints.GetLength(0) - 2;

                if (yIndex > volume.GridPoints.GetLength(1) - 2)
                    yIndex = volume.GridPoints.GetLength(1) - 2;

                if (zIndex > volume.GridPoints.GetLength(2) - 2)
                    zIndex = volume.GridPoints.GetLength(2) - 2;

                // trilinear interpolation

                Vector3 c000 = volume.GridPoints[xIndex, yIndex, zIndex];
                Vector3 c111 = volume.GridPoints[xIndex + 1, yIndex + 1, zIndex + 1];
                Vector3 delta = c111 - c000;
                Vector3 deltaMin = microIntersection - c000;
                Vector3 deltaMax = c111 - microIntersection;

                float d000 = volume.GridValues[xIndex, yIndex, zIndex];
                float d001 = volume.GridValues[xIndex, yIndex, zIndex + 1];
                float d010 = volume.GridValues[xIndex, yIndex + 1, zIndex];
                float d011 = volume.GridValues[xIndex, yIndex + 1, zIndex + 1];
                float d100 = volume.GridValues[xIndex + 1, yIndex, zIndex];
                float d101 = volume.GridValues[xIndex + 1, yIndex, zIndex + 1];
                float d110 = volume.GridValues[xIndex + 1, yIndex + 1, zIndex];
                float d111 = volume.GridValues[xIndex + 1, yIndex + 1, zIndex + 1];

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

                // sampled density > 1
                // TODO: iso-value
                if ((deltaZMax * d0 + deltaZMin * d1) > volume.IsoLevel)
                {
                    return curDist;
                }

                curDist += distDelta;

                getMicroIntersection(ref ray, curDist, ref microIntersection);
                containType = volume.VolumeBB.Contains(microIntersection);
            }

            return null;
        }

        public override Vector3 GetIntersectNormal(Vector3 intersection)
        {
            // Get the grid index of the intersection point

            int xIndex = (int)((intersection.X - volume.PositionMin.X) / volume.GridCellSize.X);
            int yIndex = (int)((intersection.Y - volume.PositionMin.Y) / volume.GridCellSize.Y);
            int zIndex = (int)((intersection.Z - volume.PositionMin.Z) / volume.GridCellSize.Z);

            if (xIndex > volume.GridPoints.GetLength(0) - 2)
                xIndex = volume.GridPoints.GetLength(0) - 2;

            if (yIndex > volume.GridPoints.GetLength(1) - 2)
                yIndex = volume.GridPoints.GetLength(1) - 2;

            if (zIndex > volume.GridPoints.GetLength(2) - 2)
                zIndex = volume.GridPoints.GetLength(2) - 2;

            // Interpolate the volume.Gradient vector and negate it to get the normal
            // TODO: store negated volume.Gradient (normal) instead

            Vector3 delta = (intersection - volume.GridPoints[xIndex, yIndex, zIndex]) / (volume.GridPoints[xIndex + 1, yIndex + 1, zIndex + 1] - volume.GridPoints[xIndex, yIndex, zIndex]);

            Vector3 n00 = Vector3.Lerp(volume.Gradient[xIndex, yIndex, zIndex], volume.Gradient[xIndex + 1, yIndex, zIndex], delta.X);
            Vector3 n01 = Vector3.Lerp(volume.Gradient[xIndex, yIndex, zIndex + 1], volume.Gradient[xIndex + 1, yIndex, zIndex + 1], delta.X);
            Vector3 n10 = Vector3.Lerp(volume.Gradient[xIndex, yIndex + 1, zIndex], volume.Gradient[xIndex + 1, yIndex + 1, zIndex], delta.X);
            Vector3 n11 = Vector3.Lerp(volume.Gradient[xIndex, yIndex + 1, zIndex + 1], volume.Gradient[xIndex + 1, yIndex + 1, zIndex + 1], delta.X);

            Vector3 n0 = Vector3.Lerp(n00, n10, delta.Y);
            Vector3 n1 = Vector3.Lerp(n01, n11, delta.Y);

            Vector3 n = Vector3.Lerp(n0, n1, delta.Z);

            if (n == Vector3.Zero)
                return n;

            return Vector3.Normalize(n);
        }

        //public override Vector4 GetLighting(Vector4 ambientLight, Ray ray, float dist, Light l, Vector3 viewVector)
        //{
        //    // sum the density
            
        //    float density = 0f;
        //    float distDelta = volume.GridCellSize.X;
        //    float curDist = (float)dist;
        //    Vector3 microIntersection = Vector3.Zero;
        //    int xIndex;
        //    int yIndex;
        //    int zIndex;
        //    ContainmentType containType;

        //    int count = 0;

        //    getMicroIntersection(ref ray, curDist, ref microIntersection);
        //    containType = volume.VolumeBB.Contains(microIntersection);

        //    Vector3 surfaceIntersection = Vector3.Zero;
        //    Vector3 intersectNormal = Vector3.Zero;

        //    bool surfaceFound = false;

        //    while (containType == ContainmentType.Contains || containType == ContainmentType.Intersects)
        //    {
        //        xIndex = (int)((microIntersection.X - waterBody.PositionMin.X) / volume.GridCellSize.X);
        //        yIndex = (int)((microIntersection.Y - waterBody.PositionMin.Y) / volume.GridCellSize.Y);
        //        zIndex = (int)((microIntersection.Z - waterBody.PositionMin.Z) / volume.GridCellSize.Z);

        //        if (xIndex > volume.GridPoints.GetLength(0) - 2)
        //            xIndex = volume.GridPoints.GetLength(0) - 2;

        //        if (yIndex > volume.GridPoints.GetLength(1) - 2)
        //            yIndex = volume.GridPoints.GetLength(1) - 2;

        //        if (zIndex > volume.GridPoints.GetLength(2) - 2)
        //            zIndex = volume.GridPoints.GetLength(2) - 2;

        //        // trilinear interpolation

        //        Vector3 c000 = volume.GridPoints[xIndex, yIndex, zIndex];
        //        Vector3 c111 = volume.GridPoints[xIndex + 1, yIndex + 1, zIndex + 1];
        //        Vector3 delta = c111 - c000;
        //        Vector3 deltaMin = microIntersection - c000;
        //        Vector3 deltaMax = c111 - microIntersection;

        //        float d000 = volume.GridValues[xIndex, yIndex, zIndex];
        //        float d001 = volume.GridValues[xIndex, yIndex, zIndex + 1];
        //        float d010 = volume.GridValues[xIndex, yIndex + 1, zIndex];
        //        float d011 = volume.GridValues[xIndex, yIndex + 1, zIndex + 1];
        //        float d100 = volume.GridValues[xIndex + 1, yIndex, zIndex + 1];
        //        float d101 = volume.GridValues[xIndex + 1, yIndex, zIndex + 1];
        //        float d110 = volume.GridValues[xIndex + 1, yIndex + 1, zIndex];
        //        float d111 = volume.GridValues[xIndex + 1, yIndex + 1, zIndex + 1];

        //        // perform linear interpolation between:
        //        //   C000 and C100 to find C00,
        //        //   C001 and C101 to find C01,
        //        //   C011 and C111 to find C11,
        //        //   C010 and C110 to find C10.       

        //        float deltaXMin = (deltaMin.X / delta.X);
        //        float deltaXMax = (deltaMax.X / delta.X);

        //        float d00 = deltaXMax * d000 + deltaXMin * d100;
        //        float d01 = deltaXMax * d001 + deltaXMin * d101;
        //        float d11 = deltaXMax * d011 + deltaXMin * d111;
        //        float d10 = deltaXMax * d010 + deltaXMin * d110;

        //        // perform linear interpolation between:
        //        //      C00 and C10 to find C0,
        //        //      C01 and C11 to find C1. 

        //        float deltaYMin = (deltaMin.Y / delta.Y);
        //        float deltaYMax = (deltaMax.Y / delta.Y);

        //        float d0 = deltaYMax * d00 + deltaYMin * d10;
        //        float d1 = deltaYMax * d01 + deltaYMin * d11;

        //        // perform linear interpolation between: 
        //        //      C0 and C1 to find c

        //        float deltaZMin = (deltaMin.Z / delta.Z);
        //        float deltaZMax = (deltaMax.Z / delta.Z);

        //        density += deltaZMax * d0 + deltaZMin * d1;

        //        if (!surfaceFound && density > 1)
        //        {
        //            surfaceIntersection = ray.Position + (ray.Direction * curDist);
        //            intersectNormal = GetIntersectNormal(ray.Position + (ray.Direction * curDist));
        //            surfaceFound = true;
        //        }

        //        curDist += distDelta;

        //        ++count;

        //        getMicroIntersection(ref ray, curDist, ref microIntersection);
        //        containType = volume.VolumeBB.Contains(microIntersection);
        //    }

        //    //density /= (curDist - dist) / distDelta;

        //    density /= count;

        //    Vector4 baseColor = Vector4.Zero;//base.calculateAmbient(ambientLight, ray, dist) * density;

        //    //if (density > 1) density = 1;

        //    if (surfaceFound)
        //    {
        //        Vector3 lightVector = Vector3.Normalize(l.Position - surfaceIntersection);
        //        baseColor += calculateDiffuse(surfaceIntersection, intersectNormal, l, lightVector);
        //        baseColor += calculateSpecular(surfaceIntersection, intersectNormal, l, lightVector, viewVector);
        //    }

        //    return baseColor;// *waterBody.Scale;
        //}

        public override Vector4 calculateAmbient(Vector4 worldAmbient, Ray ray, float dist)
        {
            return base.calculateAmbient(worldAmbient, ray, dist);
        }

        private void getMicroIntersection(ref Ray ray, float curDist, ref Vector3 microIntersection)
        {
            microIntersection = ray.Position + (ray.Direction * curDist);

            // round off floating point error

            if (microIntersection.X > volume.VolumeBB.Max.X && microIntersection.X - volume.VolumeBB.Max.X < 0.001)
                microIntersection.X = volume.VolumeBB.Max.X;

            if (microIntersection.Y > volume.VolumeBB.Max.Y && microIntersection.Y - volume.VolumeBB.Max.Y < 0.001)
                microIntersection.Y = volume.VolumeBB.Max.Y;

            if (microIntersection.Z > volume.VolumeBB.Max.Z && microIntersection.Z - volume.VolumeBB.Max.Z < 0.001)
                microIntersection.Z = volume.VolumeBB.Max.Z;
        }
    }
}
