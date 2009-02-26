using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayTracer
{
    public abstract class RayTraceable
    {
        protected float mU;
        public float MaxU
        {
            get { return mU; }
            set { mU = value; }
        }

        protected float mV;
        public float MaxV
        {
            get { return mV; }
            set { mV = value; }
        }

        protected Material material1;
        public Material Material1
        {
            get { return material1; }
            set { material1 = value; }
        }

        public abstract Vector3 Center { get; set; }

        public abstract float? Intersects(Ray ray);

        public abstract Vector3 GetIntersectNormal(Ray ray, float dist);

        public virtual Vector4 calculateAmbient(Vector4 ambientLight, Ray ray, float dist)
        {
            return material1.calculateAmbient(ambientLight, 0, 0);
        }

        public virtual Vector4 calculateDiffuse(Vector3 intersection, Vector3 normal, Light l, Vector3 lightVector)
        {
            return material1.calculateDiffuse(intersection, normal, l, lightVector, 0, 0);
        }

        public Vector4 calculateSpecular(Vector3 intersection, Vector3 normal, Light l, Vector3 lightVector, Vector3 viewVector)
        {
            return material1.calculateSpecular(intersection, normal, l, lightVector, viewVector);
        }

        public virtual Vector4 GetLighting(Vector4 ambientLight, Ray ray, float dist, Light l, Vector3 viewVector)
        {
            return Vector4.Zero;
        }

    }
}
