
// Checkpoint 6 extra - transmit shadow rays
// Flag - continue shadow rays through transparent materials
#undef TRANSMIT_SHADOW

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayTracer
{
    /// <summary>
    /// Tone Reproduction Operators
    /// </summary>
    public enum TROp
    {
        None,
        Ward,
        Reinhard
    }

    /// <summary>
    /// Manages the process of ray tracing.
    /// </summary>
    public class RTManager : DrawableGameComponent
    {
        /// <summary>
        /// Width of the projected scene.
        /// </summary>
        private int width;
        /// <summary>
        /// Height of the projected scene.
        /// </summary>
        private int height;

        // Matrices
        private Matrix worldMatrix = Matrix.Identity;
        private Matrix viewMatrix;
        private Matrix projectionMatrix;

        private int recursionDepth = 1;
        /// <summary>
        /// The recursion depth of the ray tracer.
        /// </summary>
        public int RecursionDepth
        {
            get { return recursionDepth; }
            set { recursionDepth = value; }
        }

        private Vector3 cameraPos;
        /// <summary>
        /// The current camera position.
        /// </summary>
        /// <remarks>Call UpdateCamera() after setting this value post-instansiation.</remarks>
        public Vector3 CameraPosition
        {
            get { return cameraPos; }
            set { cameraPos = value; }
        }
        private Vector3 cameraTarget;
        /// <summary>
        /// The current camera target.
        /// </summary>
        /// <remarks>Call UpdateCamera() after setting this value post-instansiation.</remarks>
        public Vector3 CameraTarget
        {
            get { return cameraTarget; }
            set { cameraTarget = value; }
        }

        private float nearDist;
        /// <summary>
        /// The near plane distance of the projection matrix.
        /// </summary>
        public float NearPlaneDistance
        {
            get { return nearDist; }
            set { nearDist = value; }
        }
        private float farDist;
        /// <summary>
        /// The far plane distance of the projection matrix.
        /// </summary>
        public float FarPlaneDistance
        {
            get { return farDist; }
            set { farDist = value; }
        }

        private Ray[,] rayTable;
        private Texture2D projection;
        private SpriteBatch spriteBatch;

        private Vector4 ambientLight = new Vector4(.2f, .2f, .2f, 1f);
        /// <summary>
        /// The color of ambient light in the world (R, G, B, A).
        /// </summary>
        public Vector4 AmbientLight
        {
            get { return ambientLight; }
            set { ambientLight = value; }
        }

        private Vector4 backgroundColor = Vector4.Zero;
        /// <summary>
        /// The background color of the world (R, G, B, A).
        /// </summary>
        public Vector4 BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        private float lMax = 100f;
        /// <summary>
        /// The max luminance value of the scene. Default is 100.
        /// </summary>
        public float LMax
        {
            get { return lMax; }
            set { lMax = value; }
        }

        private float lDMax = 100f;
        /// <summary>
        /// The max luminance value of the display device. Default is 100.
        /// </summary>
        public float LDMax
        {
            get { return lDMax; }
            set 
            { 
                lDMax = value;
                numerator = 1.219 + Math.Pow(lDMax / 2, 0.4);
            }
        }

        private TROp trOp = TROp.None;
        /// <summary>
        /// Tone reproduction operator to apply.
        /// </summary>
        /// <remarks>Tone reproduction is not applied if TROp.None is set.</remarks>
        public TROp ToneReproductionOperator
        {
            get { return trOp; }
            set { trOp = value; }
        }

        /// <summary>
        /// Used in scale factor for tone reproduction
        /// </summary>
        private static double numerator = 1.219 + Math.Pow(50, 0.4);

        private List<Light> lights = new List<Light>();
        /// <summary>
        /// The list of point lights in the world.
        /// </summary>
        public List<Light> Lights
        {
            get { return lights; }
            set { lights = value; }
        }

        private List<RayTraceable> worldObjects = new List<RayTraceable>();
        /// <summary>
        /// The list of ray traceable objects in the world.
        /// </summary>
        public List<RayTraceable> WorldObjects
        {
            get { return worldObjects; }
            set { worldObjects = value; }
        }

        /// <summary>
        /// Creates a new RTManager.
        /// </summary>
        /// <param name="game"></param>
        public RTManager(Game game)
            : base(game) { }

        protected override void LoadContent()
        {
            width = GraphicsDevice.PresentationParameters.BackBufferWidth;
            height = GraphicsDevice.PresentationParameters.BackBufferHeight;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            InitializeViewProjection();

            base.LoadContent();
        }

        private void InitializeViewProjection()
        {
            viewMatrix = Matrix.CreateLookAt(cameraPos, cameraTarget, Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, nearDist, farDist);

            populateRayTable();
            projection = new Texture2D(GraphicsDevice, width, height);
        }

        /// <summary>
        /// Updates view and projection matrices.
        /// </summary>
        public void UpdateCamera()
        {
            InitializeViewProjection();
        }

        /// <summary>
        /// Creates a two dimensional array of projection rays - one per pixel.
        /// </summary>
        private void populateRayTable()
        {
            rayTable = new Ray[width, height];

            Ray ray = new Ray(cameraPos, Vector3.Zero);
            Vector3 viewPlaneVec = Vector3.Zero;

            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    viewPlaneVec = GraphicsDevice.Viewport.Unproject(new Vector3(x, y, 0), projectionMatrix, viewMatrix, worldMatrix);
                    ray.Direction = Vector3.Normalize(viewPlaneVec - cameraPos);
                    rayTable[x, y] = ray;
                }
            }
        }

        /// <summary>
        /// Performs ray tracing, rasterizes and draws a projection texture.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            trace();

            spriteBatch.Begin();
            spriteBatch.Draw(projection, Vector2.Zero, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Traces each ray into the world, applies tone reproduction, then creates a projection texture.
        /// </summary>
        private void trace()
        {
            Vector4[] colorData = new Vector4[width * height];

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    Ray ray = rayTable[x, y];
                    int pixelIndex = (y * width) + x;

                    colorData[pixelIndex] = Illuminate(ray, 0);
                }
            }

            if(trOp != TROp.None) 
                applyToneReproduction(colorData);

            Color[] colors = new Color[colorData.Length];
            int i = 0;
            foreach (Vector4 v in colorData)
                colors[i++] = new Color(v);
            projection = new Texture2D(GraphicsDevice, width, height);
            projection.SetData<Color>(colors);
        }

        private void applyToneReproduction(Vector4[] colorData)
        {
            // calculate luminance
            float[] absLuminance = new float[colorData.Length];

            for (int i = 0; i < colorData.Length; ++i)
            {
                colorData[i].X *= lMax;
                colorData[i].Y *= lMax;
                colorData[i].Z *= lMax;
                absLuminance[i] = (0.27f * colorData[i].X) + (0.67f * colorData[i].Y) + (0.06f * colorData[i].Z);
            }

            // log-avg luminance            
            double logAvg = getLogAvgLuminance(absLuminance);

            if(trOp == TROp.Ward)
                WardOp(colorData, logAvg);
            else if(trOp == TROp.Reinhard)
                ReinhardOp(colorData, (float)logAvg);

            // apply device model
            for (int i = 0; i < colorData.Length; ++i)
            {
                colorData[i].X /= lDMax;
                colorData[i].Y /= lDMax;
                colorData[i].Z /= lDMax;
            }
        }

        // Luminance Zone 5
        private const float a = 0.18f;

        /// <summary>
        /// Applies Reinhard tone reproduction operator
        /// </summary>
        /// <param name="colorData"></param>
        /// <param name="logAvg"></param>
        private void ReinhardOp(Vector4[] colorData, float logAvg)
        {
            for (int i = 0; i < colorData.Length; ++i)
            {
                // scaled luminance
                colorData[i].X *= (a / logAvg);
                colorData[i].Y *= (a / logAvg);
                colorData[i].Z *= (a / logAvg);

                // reflected luminance
                colorData[i].X /= (1 + colorData[i].X);
                colorData[i].Y /= (1 + colorData[i].Y);
                colorData[i].Z /= (1 + colorData[i].Z);

                // simulate illumination
                colorData[i].X *= lDMax;
                colorData[i].Y *= lDMax;
                colorData[i].Z *= lDMax;
            }
        }

        /// <summary>
        /// Applies Ward tone reproduction operator
        /// </summary>
        /// <param name="colorData"></param>
        /// <param name="logAvg"></param>
        private void WardOp(Vector4[] colorData, double logAvg)
        {
            // scale factor
            float sf = (float)(Math.Pow(numerator / (1.219 + Math.Pow(logAvg, 0.4)), 2.5));

            for (int i = 0; i < colorData.Length; ++i)
            {
                colorData[i].X *= sf;
                colorData[i].Y *= sf;
                colorData[i].Z *= sf;
            }
        }

        /// <summary>
        /// Gets the log average luminance for an array of absolute luminances
        /// </summary>
        /// <param name="absLuminance">array of absolute luminances</param>
        /// <returns></returns>
        private static double getLogAvgLuminance(float[] absLuminance)
        {
            double E = 0f;

            foreach (float L in absLuminance)
                E += Math.Log(0.00000001 + L);

            double logAvg = Math.Exp(E / absLuminance.Length);
            return logAvg;
        }

        /// <summary>
        /// Calculates the illumination (un)projected to a ray in the world.
        /// </summary>
        /// <param name="ray">The ray to calculate illumination.</param>
        /// <param name="depth">Current recursion depth</param>
        /// <returns></returns>
        private Vector4 Illuminate(Ray ray, int depth)
        {
            Vector3 intersectPoint;
            float? dist = null;
            RayTraceable rt = getClosestIntersection(ray, out intersectPoint, out dist);

            if (rt != null)
            {
                if (!(rt is Quad))
                { }
                Vector3 intersectNormal = rt.GetIntersectNormal(intersectPoint);
                
                
                //Vector3 viewVector = Vector3.Normalize(ray.Position - intersectPoint);

                Vector4 totalLight = rt.calculateAmbient(ambientLight, ray, (float)dist);

                totalLight += spawnShadowRay(ref intersectPoint, rt, ref intersectNormal, ref ray.Direction, depth);

                if (depth < recursionDepth)
                {
                    Vector3 incidentVector = Vector3.Normalize(intersectPoint - ray.Position);
                    
                    // Material is reflective
                    if (rt.Material1.Reflectivity > 0)
                    {
                        Vector3 dir = Vector3.Reflect(incidentVector, intersectNormal);
                        Ray reflectionRay = new Ray(intersectPoint, dir);
                        totalLight += rt.Material1.Reflectivity * Illuminate(reflectionRay, depth + 1);
                    }

                    // Material is transparent
                    if (rt.Material1.Transparency > 0)
                    {
                        spawnTransmissionRay(depth, ref intersectPoint, rt, ref intersectNormal, ref totalLight, ref incidentVector);
                    }
                }

                return totalLight;
            }
            else
            {
                return backgroundColor;
            }
        }

        /// <summary>
        /// Spawns a recursive, transmitted (refracted) ray.
        /// </summary>
        /// <param name="depth">Current recursion depth</param>
        /// <param name="intersectPoint">Origin of the ray</param>
        /// <param name="intersectedObject">World object that was intersected</param>
        /// <param name="intersectNormal">Normal of the world object at the intersection point</param>
        /// <param name="totalLight">Total light to contribute to.</param>
        /// <param name="incidentVector">Ray direction incident to intersection.</param>
        private void spawnTransmissionRay(int depth, ref Vector3 intersectPoint, RayTraceable intersectedObject, ref Vector3 intersectNormal, ref Vector4 totalLight, ref Vector3 incidentVector)
        {
            float n;

            // Parity check

            if (depth % 2 == 0)
            {
                // assuming outside to inside
                n = intersectedObject.Material1.RefractionIndex;
            }
            else
            {
                // assuming inside to outside
                n = 1 / intersectedObject.Material1.RefractionIndex;
                intersectNormal = Vector3.Negate(intersectNormal);
            }

            float dot = Vector3.Dot(incidentVector, intersectNormal);
            float discriminant = 1 + ((n * n) * ((dot * dot) - 1));

            if (discriminant < 0)
            {
                // simulate total internal reflection
                Vector3 dir = Vector3.Reflect(incidentVector, intersectNormal);
                Ray reflectionRay = new Ray(intersectPoint, dir);
                totalLight += intersectedObject.Material1.RefractionIndex * Illuminate(reflectionRay, depth + 1);
            }
            else
            {
                float sqrt = (float)Math.Sqrt(discriminant);
                Vector3 dir = (n * incidentVector) + ((n * dot - sqrt) * intersectNormal);
                Ray transRay = new Ray(intersectPoint, dir);
                totalLight += intersectedObject.Material1.Transparency * Illuminate(transRay, depth + 1);
            }
        }

        /// <summary>
        /// Spawns a shadow ray.
        /// </summary>
        /// <param name="intersectPoint">Origin of the ray</param>
        /// <param name="intersectedObject">World object that was intersected</param>
        /// <param name="intersectNormal">Normal of the world object at the intersection point</param>
        /// <param name="viewVector">Camera view vector.</param>
        /// <param name="depth">current recursion depth.</param>
        /// <returns></returns>
        private Vector4 spawnShadowRay(ref Vector3 intersectPoint, RayTraceable intersectedObject, ref Vector3 intersectNormal, ref Vector3 viewVector, int depth)
        {
            Vector4 diffuseTotal = Vector4.Zero;
            Vector4 specularTotal = Vector4.Zero;

            foreach (Light light in lights)
            {
                // Spawn a shadow ray from the intersection point to the light source
                Vector3 lightVector = Vector3.Normalize(light.Position - intersectPoint);

                // but only if the intersection is facing the light source
                float facing = Vector3.Dot(intersectNormal, lightVector);
                if (facing < 0)
                {
                    Ray shadowRay = new Ray(intersectPoint, lightVector);

                    // Check if the shadow ray reaches the light before hitting any other object
                    float dist = Vector3.Distance(intersectPoint, light.Position);
                    bool shadowed = false;

                    Vector4 shadowLight = Vector4.Zero;

                    foreach (RayTraceable rt in worldObjects)
                    {
                        if (rt != intersectedObject)
                        {
                            float? curDist = rt.Intersects(shadowRay);
                            if (curDist != null && curDist < dist)
                            {
                                dist = (float)curDist;
                                shadowed = true;

#if !TRANSMIT_SHADOW
                                break;
#else
                                if (rt.Material1.Transparency > 0)
                                {
                                    Vector3 incidentVector = Vector3.Normalize(intersectPoint - shadowRay.Position);
                                    Vector3 shadowIntersect = shadowRay.Position + (shadowRay.Direction * (float)curDist);
                                    Vector3 shadowNormal = rt.GetIntersectNormal(shadowIntersect);

                                    spawnTransmissionRay(depth, ref shadowIntersect, rt, ref shadowNormal, ref shadowLight, ref incidentVector);
                                    shadowLight *= rt.Material1.Transparency;
                                }
                                else
                                {
                                    shadowLight = Vector4.Zero;
                                    break;
                                }
#endif
                            }
                        }
                    }

                    if (shadowed)
                    {
                        diffuseTotal += intersectedObject.calculateDiffuse(intersectPoint, intersectNormal, light, lightVector) * shadowLight;
                        specularTotal += intersectedObject.calculateSpecular(intersectPoint, intersectNormal, light, lightVector, viewVector) * shadowLight;
                    }
                    else
                    {
                        diffuseTotal += intersectedObject.calculateDiffuse(intersectPoint, intersectNormal, light, lightVector);
                        specularTotal += intersectedObject.calculateSpecular(intersectPoint, intersectNormal, light, lightVector, viewVector);
                    }

                }
            }

            return Vector4.Multiply(diffuseTotal, intersectedObject.Material1.DiffuseStrength) + 
                Vector4.Multiply(specularTotal, intersectedObject.Material1.SpecularStrength);
        }

        /// <summary>
        /// Finds the closest intersected RayTraceable and sets the intersectPoint Vector3.
        /// </summary>
        /// <param name="ray">The ray to test RayTraceable intersections.</param>
        /// <param name="intersectPoint">The Vector3 to hold the intersection data.</param>
        /// <returns>The closest intersected RayTraceable, or null if no RayTraceable is intersected.</returns>
        private RayTraceable getClosestIntersection(Ray ray, out Vector3 intersectPoint, out float? intersectDist)
        {
            float? dist = float.PositiveInfinity;
            float? curDist = intersectDist = null;
            RayTraceable intersected = null;

            foreach (RayTraceable rt in worldObjects)
            {
                curDist = rt.Intersects(ray);
                if (curDist < dist)
                {
                    dist = curDist;
                    intersectDist = dist;
                    intersected = rt;
                }
            }

            intersectPoint = ray.Position + Vector3.Multiply(ray.Direction, (float)dist);

            return intersected;
        }
    }
}
