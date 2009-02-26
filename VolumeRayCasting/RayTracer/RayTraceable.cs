using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayTracer
{
    /// <summary>
    /// Represents an object that can be ray traced.
    /// </summary>
    public abstract class RayTraceable
    {
        protected float mU;
        /// <summary>
        /// Maximum U texture coordinate
        /// </summary>
        public float MaxU
        {
            get { return mU; }
            set { mU = value; }
        }

        protected float mV;
        /// <summary>
        /// Maximum V texture coordinate
        /// </summary>
        public float MaxV
        {
            get { return mV; }
            set { mV = value; }
        }

        protected Material material1;
        /// <summary>
        /// Material of the object
        /// </summary>
        public Material Material1
        {
            get { return material1; }
            set { material1 = value; }
        }

        /// <summary>
        /// Tests a ray for intersection against the object.
        /// </summary>
        /// <param name="ray">The ray</param>
        /// <returns>The distance of the closest positive intersection, or null if no intersection exists.</returns>
        public abstract float? Intersects(Ray ray);

        /// <summary>
        /// Gets the normal of the object at the specified point.
        /// </summary>
        /// <param name="intersectPoint">Point to find normal.</param>
        /// <returns>The normal of the object at the specified point.</returns>
        public abstract Vector3 GetIntersectNormal(Vector3 intersectPoint);

        /// <summary>
        /// Gets the U texture coordinate of the object at the specified world coordinates.
        /// </summary>
        /// <param name="worldCoords">The world coordinates.</param>
        /// <returns>Gets the U texture coordinate.</returns>
        protected virtual float getU(Vector3 worldCoords)
        {
            return 0;
        }

        /// <summary>
        /// Gets the V texture coordinate of the object at the specified world coordinates.
        /// </summary>
        /// <param name="worldCoords">The world coordinates.</param>
        /// <returns>Gets the V texture coordinate.</returns>
        protected virtual float getV(Vector3 worldCoords)
        {
            return 0;
        }

        /// <summary>
        /// Returns the ambient color of the object at the specified world coordinates.
        /// </summary>
        /// <param name="worldAmbient">World ambient color.</param>
        /// 
        /// <returns>The ambient color of the object at the specified world coordinates.</returns>
        public virtual Vector4 calculateAmbient(Vector4 worldAmbient, Ray ray, float dist)
        {
            Vector4 ambientLight = worldAmbient;

            if (material1 != null)
            {
                if (material1 is IMaterialTexture)
                {
                    Vector3 worldCoords = ray.Position + (ray.Direction * dist);
                    ambientLight *= ((IMaterialTexture)material1).GetColor(getU(worldCoords), getV(worldCoords)) * material1.AmbientStrength;
                }
                else
                    ambientLight *= material1.getAmbientColor() * material1.AmbientStrength;
            }

            return ambientLight;
        }

        /// <summary>
        /// Returns the diffuse color of the object at the specified world coordinates for a given light source.
        /// </summary>
        /// <param name="worldCoords">The world coordinates.</param>
        /// <param name="normal">Normal of the intersection.</param>
        /// <param name="l">Light source.</param>
        /// <param name="lightVector">Vector to light source.</param>
        /// <returns>The diffuse color of the object at the specified world coordinates for a given light source.</returns>
        public virtual Vector4 calculateDiffuse(Vector3 worldCoords, Vector3 normal, Light l, Vector3 lightVector)
        {
            Vector4 diffuseLight = l.LightColor;

            if (material1 != null)
            {
                if (material1 is IMaterialTexture)
                    diffuseLight *= ((IMaterialTexture)material1).GetColor(getU(worldCoords), getV(worldCoords));
                else
                    diffuseLight *= material1.getDiffuseColor();

                diffuseLight *= material1.DiffuseStrength * Math.Abs(Vector3.Dot(lightVector, normal));
            }

            return diffuseLight;
        }

        /// <summary>
        /// Returns the specular color of the object at the specified world coordinates for a given light source.
        /// </summary>
        /// <param name="intersection">Point to find diffuse color.</param>
        /// <param name="normal">Normal of the intersection.</param>
        /// <param name="l">Light source.</param>
        /// <param name="lightVector">Vector to light source.</param>
        /// <param name="viewVector">Vector of the camera view.</param>
        /// <returns>The specular color of the object at the specified point for a given light source.</returns>
        public Vector4 calculateSpecular(Vector3 worldCoords, Vector3 normal, Light l, Vector3 lightVector, Vector3 viewVector)
        {
            Vector4 specularLight = l.LightColor;

            if (material1 != null)
            {
                if (material1 is IMaterialTexture)
                    specularLight *= ((IMaterialTexture)material1).GetColor(getU(worldCoords), getV(worldCoords));
                else
                    specularLight *= material1.getSpecularColor();

                Vector3 reflectedVector = Vector3.Reflect(lightVector, normal);
                double dot = (double)Vector3.Dot(reflectedVector, viewVector);

                if (dot >= 0)
                    return Vector4.Zero;

                specularLight *= material1.SpecularStrength * Math.Abs(Vector3.Dot(lightVector, normal) * (float)Math.Pow(dot, material1.Exponent));
            }

            return specularLight;
        }
    }
}
