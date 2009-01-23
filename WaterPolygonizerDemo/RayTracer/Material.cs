using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayTracer
{
    public class Material
    {
        #region Phong parameters

        protected float ambientStrength;
        public float AmbientStrength
        {
            get { return ambientStrength; }
            set { ambientStrength = value; }
        }

        protected float diffuseStrength;
        public float DiffuseStrength
        {
            get { return diffuseStrength; }
            set { diffuseStrength = value; }
        }

        protected float specularStrength;
        public float SpecularStrength
        {
            get { return specularStrength; }
            set { specularStrength = value; }
        }

        protected double exponent;
        public double Exponent
        {
            get { return exponent; }
            set { exponent = value; }
        }

        #endregion

        protected Vector4 ambientColor = Vector4.Zero;

        public virtual Vector4 getAmbientColor()
        {
            return ambientColor;
        }

        public virtual Vector4 getAmbientColor(float u, float v)
        {
            return ambientColor;
        }

        public virtual void setAmbientColor(Vector4 color)
        {
            ambientColor = color;
        }        

        protected Vector4 diffuseColor = Vector4.Zero;

        public virtual Vector4 getDiffuseColor()
        {
            return diffuseColor;
        }

        public virtual Vector4 getDiffuseColor(float u, float v)
        {
            return diffuseColor;
        }

        public virtual void setDiffuseColor(Vector4 color)
        {
            diffuseColor = color;
        }   

        protected Vector4 specularColor = Vector4.One;

        public virtual Vector4 getSpecularColor()
        {
            return specularColor;
        }

        public virtual void setSpecularColor(Vector4 color)
        {
            specularColor = color;
        } 

        public Vector4 calculateAmbient(Vector4 ambientLight, float u, float v)
        {
            return ambientStrength * getAmbientColor(u, v) * ambientLight;
        }

        public Vector4 calculateDiffuse(Vector3 intersection, Vector3 normal, Light l, Vector3 lightVector, float u, float v)
        {
            Vector4 diffuse = l.LightColor * getDiffuseColor(u, v);

            float diffuseAmount = Math.Abs(Vector3.Dot(lightVector, normal));

            return Vector4.Multiply(diffuse, diffuseAmount);
        }

        public Vector4 calculateSpecular(Vector3 intersection, Vector3 normal, Light l, Vector3 lightVector, Vector3 viewVector)
        {
            Vector4 specular = l.LightColor * getSpecularColor();

            Vector3 reflectedVector = Vector3.Reflect(lightVector, normal);

            double dot = (double)Vector3.Dot(reflectedVector, viewVector);

            if (dot >= 0)
                return Vector4.Zero;

            float specularAmount = (float)Math.Pow(dot, exponent);

            return Vector4.Multiply(specular, specularAmount);
        }
    }
}
