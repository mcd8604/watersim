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

        public override float? Intersects(Ray ray)
        {
            return ray.Intersects(boundingSphere);
        }

        public override Vector3 GetIntersectNormal(Ray ray, float dist)
        {
            Vector3 intersectPt = (ray.Position + (ray.Direction * dist));
            return Vector3.Normalize(boundingSphere.Center - intersectPt);
        }
    }
}
