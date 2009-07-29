using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayTracer
{
    /// <summary>
    /// A sphere with a center position and radius.
    /// </summary>
    public class Sphere : RayTraceable
    {
        protected BoundingSphere boundingSphere;

        /// <summary>
        /// The radius of the sphere.
        /// </summary>
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

        /// <summary>
        /// Creates a Sphere with the specified radius.
        /// </summary>
        /// <param name="radius">Radius of the sphere.</param>
        public Sphere(float radius)
        {
            this.boundingSphere = new BoundingSphere(Vector3.Zero, radius);
        }

        /// <summary>
        /// Creates a Sphere with the specified center position and radius.
        /// </summary>
        /// <param name="center">Center position of the sphere.</param>
        /// <param name="radius">Radius of the sphere.</param>
        public Sphere(Vector3 center, float radius)
        {
            this.boundingSphere = new BoundingSphere(center, radius);
        }

        /// <summary>
        /// Tests a ray for intersection against the sphere.
        /// </summary>
        /// <param name="ray">The ray</param>
        /// <returns>The distance of the closest positive intersection, or null if no intersection exists.</returns>
        public override float? Intersects(Ray ray)
        {
            // float? rayVal = ray.Intersects(boundingSphere);

            // Quadratic formula

            double diffX = ray.Position.X - boundingSphere.Center.X;
            double diffY = ray.Position.Y - boundingSphere.Center.Y;
            double diffZ = ray.Position.Z - boundingSphere.Center.Z;

            double B = 2 * ((ray.Direction.X * diffX) + (ray.Direction.Y * diffY) + (ray.Direction.Z * diffZ));
            double C = (diffX * diffX) + (diffY * diffY) + (diffZ * diffZ) - (boundingSphere.Radius * boundingSphere.Radius);
            
            // Round off 
            if (C < .001)
                C = 0;

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

                return (float)dist2;
            }

            return (float)dist1;
        }

        /// <summary>
        /// Gets the normal of the sphere at the specified point.
        /// </summary>
        /// <param name="intersectPoint">Point to find normal.</param>
        /// <returns>The normal of the sphere at the specified point.</returns>
        public override Vector3 GetIntersectNormal(Vector3 intersectPoint)
        {
            return Vector3.Normalize(boundingSphere.Center - intersectPoint);
        }
    }
}
