using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayTracer
{
    /// <summary>
    /// A rectangular plane defined by four vectors.
    /// </summary>
    public class Quad : RayTraceable
    {
        private Plane plane;

        protected BoundingBox boundingBox;

        public Quad(Vector3 pt1, Vector3 pt2, Vector3 pt3, Vector3 pt4)
        {
            plane = new Plane(pt1, pt2, pt3);
            List<Vector3> points = new List<Vector3>();
            points.Add(pt1);
            points.Add(pt2);
            points.Add(pt3);
            points.Add(pt4);

            boundingBox = BoundingBox.CreateFromPoints(points);
        }

        protected override float getU(Vector3 intersection)
        {
            return (intersection.X - boundingBox.Min.X) / (boundingBox.Max.X - boundingBox.Min.X) * MaxU;
        }

        protected override float getV(Vector3 intersection)
        {
            return (intersection.Z - boundingBox.Min.Z) / (boundingBox.Max.Z - boundingBox.Min.Z) * MaxV;
        }

        public override float? Intersects(Ray ray)
        {
            if (ray.Intersects(plane) != null)
                return ray.Intersects(boundingBox);

            return null;
        }

        public override Vector3 GetIntersectNormal(Vector3 intersectPoint)
        {
            return plane.Normal;
        }
    }
}
