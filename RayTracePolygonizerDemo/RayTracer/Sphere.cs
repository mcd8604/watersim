using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayTracer
{
    public class Sphere : RayTraceable
    {
        protected BoundingSphere boundingSphere;
        public BoundingSphere MyBoundingSphere
        {
            get { return boundingSphere; }
            set { boundingSphere = value; }
        }

        public override Vector3 Center
        {
            get
            {
                return boundingSphere.Center;
            }
            set
            {
                boundingSphere.Center = value;
            }
        }

        public float Radius
        {
            get
            {
                return boundingSphere.Radius;
            }
            set
            {
                boundingSphere.Radius = value;
            }
        }

        public Sphere(float radius)
        {
            this.boundingSphere = new BoundingSphere(Vector3.Zero, radius);
        }

        public Sphere(Vector3 center, float radius)
        {
            this.boundingSphere = new BoundingSphere(center, radius);
        }

        //public override float? Intersects(Ray ray)
        //{
        //    //Compute A, B and C coefficients
        //    float a = Vector3.Dot(ray.Direction, ray.Direction);
        //    float b = 2 * Vector3.Dot(ray.Direction, ray.Position);
        //    float c = Vector3.Dot(ray.Position, ray.Position) - (boundingSphere.Radius * boundingSphere.Radius);

        //    //Find discriminant
        //    float disc = b * b - 4 * a * c;

        //    // if discriminant is negative there are no real roots, so return 
        //    // false as ray misses sphere
        //    if (disc < 0)
        //        return null;

        //    // compute q as described above
        //    float distSqrt = (float)Math.Sqrt(disc);
        //    float q;
        //    if (b < 0)
        //        q = (0-b - distSqrt) / 2.0f;
        //    else
        //        q = (0-b + distSqrt) / 2.0f;

        //    // compute t0 and t1
        //    float t0 = q / a;
        //    float t1 = c / q;

        //    // make sure t0 is smaller than t1
        //    if (t0 > t1)
        //    {
        //        // if t0 is bigger than t1 swap them around
        //        float temp = t0;
        //        t0 = t1;
        //        t1 = temp;
        //    }

        //    // if t1 is less than zero, the object is in the ray's negative direction
        //    // and consequently the ray misses the sphere
        //    if (t1 < 0)
        //        return null;

        //    // if t0 is less than zero, the intersection point is at t1
        //    if (t0 < 0)
        //    {
        //        return t1;
        //    }
        //    // else the intersection point is at t0
        //    else
        //    {
        //        return t0;
        //    }
        //}

        public override float? Intersects(Ray ray)
        {
            //ContainmentType containType = boundingSphere.Contains(ray.Position);

            //float? val = ray.Intersects(boundingSphere);
            //if (val == 0)
            //{
            //    // the ray is directly on the sphere, offset it 

            //    Vector3 normal = Vector3.Normalize(boundingSphere.Center - ray.Position);
            //    float dot = Vector3.Dot(normal, ray.Position + ray.Direction);
            //    //(0 - dot) * boundingSphere.Radius

            //    Ray newRay = new Ray(ray.Position + (ray.Direction * dist), ray.Direction);

            //}
            //else if (containType == ContainmentType.Contains && val != 0)
            //{

            //}
            //return val;

            float? rayVal = ray.Intersects(boundingSphere);

            // Quadratic formula

            double diffX = ray.Position.X - boundingSphere.Center.X;
            double diffY = ray.Position.Y - boundingSphere.Center.Y;
            double diffZ = ray.Position.Z - boundingSphere.Center.Z;

            double B = 2 * ((ray.Direction.X * diffX) + (ray.Direction.Y * diffY) + (ray.Direction.Z * diffZ));
            double C = (diffX * diffX) + (diffY * diffY) + (diffZ * diffZ) - (boundingSphere.Radius * boundingSphere.Radius);
            if (C < .001)
            {
                C = 0;
            }

            double square = (B * B) - (4 * C);

            // no real root, no intersection
            if (square < 0)
                return null;

            // one root, ray is tangent to sphere's surface
            if (square == 0)
                return (float)(0 - B) / 2;

            // two roots, ray goes through sphere
            double root = Math.Sqrt(square);
            double dist1 = ((0 - B) - root) / 2;
            double dist2 = ((0 - B) + root) / 2;

            if (dist1 <= 0)
            {
                if (dist2 <= 0)
                    return null;

                if (rayVal != dist2)
                { }

                return (float)dist2;
            }

            if (rayVal != dist1)
            { }


            return (float)dist1;
        }

        public override Vector3 GetIntersectNormal(Vector3 intersectPoint)
        {
            return Vector3.Normalize(boundingSphere.Center - intersectPoint);
        }
    }
}
