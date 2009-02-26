using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayTracer
{
    /// <summary>
    /// Defines properties of material to support the Phong illumination model.
    /// </summary>
    public class Material
    {
        protected float kR;
        /// <summary>
        /// Reflectivity of the material
        /// </summary>
        public float Reflectivity
        {
            get { return kR; }
            set { kR = value; }
        }

        protected float kT;
        /// <summary>
        /// Transparency of material [0, 1]
        /// </summary>
        public float Transparency
        {
            get { return kT; }
            set { kT = value; }
        }

        protected float n;
        /// <summary>
        /// Index of refraction for transparency
        /// </summary>
        public float RefractionIndex
        {
            get { return n; }
            set { n = value; }
        }

        #region Phong parameters

        protected float ambientStrength;
        /// <summary>
        /// Ambient light strength [0, 1]
        /// </summary>
        public float AmbientStrength
        {
            get { return ambientStrength; }
            set { ambientStrength = value; }
        }

        protected float diffuseStrength;
        /// <summary>
        /// Diffuse light strength [0, 1]
        /// </summary>
        public float DiffuseStrength
        {
            get { return diffuseStrength; }
            set { diffuseStrength = value; }
        }

        protected float specularStrength;
        /// <summary>
        /// Specular light strength [0, 1]
        /// </summary>
        public float SpecularStrength
        {
            get { return specularStrength; }
            set { specularStrength = value; }
        }

        protected double exponent;
        /// <summary>
        /// Exponent of specular lighting
        /// </summary>
        public double Exponent
        {
            get { return exponent; }
            set { exponent = value; }
        }

        #endregion

        protected Vector4 ambientColor = Vector4.Zero;

        /// <summary>
        /// Returns the ambient color of the material.
        /// </summary>
        /// <returns>The ambient color of the material.</returns>
        public virtual Vector4 getAmbientColor()
        {
            return ambientColor;
        }

        /// <summary>
        /// Sets the ambient color of the material.
        /// </summary>
        /// <param name="color">Ambient color</param>
        public virtual void setAmbientColor(Vector4 color)
        {
            ambientColor = color;
        }        

        protected Vector4 diffuseColor = Vector4.Zero;

        /// <summary>
        /// Returns the diffuse color of the material.
        /// </summary>
        /// <returns>The diffuse color of the material.</returns>
        public virtual Vector4 getDiffuseColor()
        {
            return diffuseColor;
        }

        /// <summary>
        /// Sets the diffuse color of the material.
        /// </summary>
        /// <param name="color">Diffuse color</param>
        public virtual void setDiffuseColor(Vector4 color)
        {
            diffuseColor = color;
        }   

        protected Vector4 specularColor = Vector4.One;

        /// <summary>
        /// Returns the specular color of the material.
        /// </summary>
        /// <returns>The specular color of the material.</returns>
        public virtual Vector4 getSpecularColor()
        {
            return specularColor;
        }

        /// <summary>
        /// Sets the specular color of the material.
        /// </summary>
        /// <param name="color">Specular color</param>
        public virtual void setSpecularColor(Vector4 color)
        {
            specularColor = color;
        }
    }
}
