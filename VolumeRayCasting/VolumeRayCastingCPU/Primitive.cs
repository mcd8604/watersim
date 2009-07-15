using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace VolumeRayCasting
{
    abstract class Primitive
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

        public abstract Vector3 GetIntersectNormal(Vector3 intersectPoint);

        public virtual Vector4 calculateAmbient(Vector4 ambientLight, Vector3 intersection)
        {
            return material1.calculateAmbient(ambientLight, 0, 0);
        }

        public virtual Vector4 calculateDiffuse(Vector3 intersection, Vector3 normal, Light l, Vector3 lightVector)
        {
            return material1.calculateDiffuse(intersection, normal, l, lightVector, 0, 0);
        }

        public virtual Vector4 calculateSpecular(Vector3 intersection, Vector3 normal, Light l, Vector3 lightVector, Vector3 viewVector)
        {
            return material1.calculateSpecular(intersection, normal, l, lightVector, viewVector);
        }

    }
}
