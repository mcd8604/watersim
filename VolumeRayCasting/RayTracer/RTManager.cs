using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayTracer
{
    public class RTManager : DrawableGameComponent
    {
        private int width;
        private int height;

        private Matrix worldMatrix;
        private Matrix viewMatrix;
        private Matrix projectionMatrix;

        private int recursionDepth = 1;
        public int RecursionDepth
        {
            get { return recursionDepth; }
            set { recursionDepth = value; }
        }

        private Vector3 cameraPos;
        public Vector3 CameraPosition
        {
            get { return cameraPos; }
            set { cameraPos = value; }
        }
        private Vector3 cameraTarget;
        public Vector3 CameraTarget
        {
            get { return cameraTarget; }
            set { cameraTarget = value; }
        }

        private float nearDist;
        public float NearPlaneDistance
        {
            get { return nearDist; }
            set { nearDist = value; }
        }
        private float farDist;
        public float FarPlaneDistance
        {
            get { return farDist; }
            set { farDist = value; }
        }

        private Ray[,] rayTable;
        private Texture2D projection;
        private SpriteBatch spriteBatch;

        private Vector4 ambientLight = new Vector4(.2f, .2f, .2f, 1f);
        public Vector4 AmbientLight
        {
            get { return ambientLight; }
            set { ambientLight = value; }
        }

        private Vector4 backgroundColor = Vector4.Zero;//Color.CornflowerBlue.ToVector4();
        public Vector4 BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        private List<Light> lights = new List<Light>();
        public List<Light> Lights
        {
            get { return lights; }
            set { lights = value; }
        }

        private List<RayTraceable> worldObjects = new List<RayTraceable>();
        public List<RayTraceable> WorldObjects
        {
            get { return worldObjects; }
            set { worldObjects = value; }
        }

        public RTManager(Game game)
            : base(game) { }

        public override void Initialize()
        {
            worldMatrix = Matrix.Identity;
        
            base.Initialize();
        }

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

        public void UpdateCamera()
        {
            InitializeViewProjection();
        }

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

        public override void Draw(GameTime gameTime)
        {
            trace();

            spriteBatch.Begin();
            spriteBatch.Draw(projection, Vector2.Zero, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void trace()
        {
            Color[] colorData = new Color[width * height];

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    Ray ray = rayTable[x, y];
                    int pixelIndex = (y * width) + x;

                    if (x == 350 && y == 407)
                    { }

                    colorData[pixelIndex] = new Color(Illuminate(ray, 0));
                }
            }
            projection = new Texture2D(GraphicsDevice, width, height);
            projection.SetData<Color>(colorData);
        }

        private Vector4 Illuminate(Ray ray, int depth)
        {
            Vector3 intersectPoint;
            float? intersectDist;
            RayTraceable rt = getClosestIntersection(ray, out intersectDist);

            if (intersectDist != null)
            {
                intersectPoint = ray.Position + (ray.Direction * (float)intersectDist);
                Vector3 intersectNormal = rt.GetIntersectNormal(intersectPoint);
                Vector3 viewVector = Vector3.Normalize(ray.Position - intersectPoint);

                Vector4 totalLight = rt.calculateAmbient(ambientLight, ray, (float)intersectDist);
                //Vector4 totalLight = rt.GetLighting(ambientLight, ray, (float)intersectDist, lights[0], viewVector);

                ////feed this all into the volume, calc diffuse and specular all together for now
                Vector3 lightVector = Vector3.Normalize(lights[0].Position - intersectPoint);
                totalLight += rt.calculateDiffuse(intersectPoint, intersectNormal, lights[0], lightVector);
                totalLight += rt.calculateSpecular(intersectPoint, intersectNormal, lights[0], lightVector, viewVector);

                //totalLight += spawnShadowRay(ref intersectPoint, rt, ref intersectNormal, ref viewVector);

                //if (depth < recursionDepth)
                //{
                //    Vector3 incidentRay = Vector3.Normalize(intersectPoint - ray.Position);
                //    if (rt.Material1.ReflectionCoef > 0)
                //    {
                //        Vector3 dir = Vector3.Reflect(incidentRay, intersectNormal);
                //        Ray reflectionRay = new Ray(intersectPoint, dir);
                //        totalLight += rt.Material1.ReflectionCoef * Illuminate(reflectionRay, ++depth);
                //    }
                //    if (rt.Material1.TransmissionCoef > 0)
                //    {
                //        //double theta = Math.Acos(incidentRay.Length() / intersectNormal.Length());
                        
                //        Ray transRay = new Ray(intersectPoint, incidentRay);
                        
                //        totalLight += rt.Material1.TransmissionCoef * Illuminate(transRay, ++depth);
                //    }
                //}

                return totalLight;
            }
            else
            {
                return backgroundColor;
            }
        }

        private Vector4 spawnShadowRay(ref Vector3 intersectPoint, RayTraceable p, ref Vector3 intersectNormal, ref Vector3 viewVector)
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

                    foreach (RayTraceable rt in worldObjects)
                    {
                        if (rt != p)
                        {
                            float? curDist = rt.Intersects(shadowRay);
                            if (curDist != null && curDist < dist)
                            {
                                dist = (float)curDist;
                                shadowed = true;
                                break;
                            }
                        }
                    }

                    if (!shadowed)
                    {
                        diffuseTotal += p.calculateDiffuse(intersectPoint, intersectNormal, light, lightVector);
                        specularTotal += p.calculateSpecular(intersectPoint, intersectNormal, light, lightVector, viewVector);
                    }

                }
            }

            return Vector4.Multiply(diffuseTotal, p.Material1.DiffuseStrength) + 
                Vector4.Multiply(specularTotal, p.Material1.SpecularStrength);
        }

        /// <summary>
        /// Finds the closest intersected Primitive and sets the intersectPoint Vector3.
        /// </summary>
        /// <param name="ray">The ray to test Primitive intersections.</param>
        /// <param name="intersectPoint">The Vector3 to hold the intersection data.</param>
        /// <returns>The closest intersected Primitive, or null if no Primitive is intersected.</returns>
        //private RayTraceable getClosestIntersection(Ray ray, out Vector3 intersectPoint)
        //{
        //    float? dist = float.PositiveInfinity;
        //    float? curDist = null;
        //    RayTraceable intersected = null;

        //    foreach (RayTraceable rt in worldObjects)
        //    {
        //        curDist = rt.Intersects(ray);
        //        if (curDist < dist && curDist > 0.001f)
        //        {
        //            dist = curDist;
        //            intersected = rt;
        //        }
        //    }

        //    intersectPoint = ray.Position + Vector3.Multiply(ray.Direction, (float)dist);

        //    return intersected;
        //}

         private RayTraceable getClosestIntersection(Ray ray, out float? intersectDist)
        {
            intersectDist = null;
            float? dist = float.PositiveInfinity;
            RayTraceable intersected = null;

            foreach (RayTraceable rt in worldObjects)
            {
                intersectDist = rt.Intersects(ray);
                if (intersectDist < dist && intersectDist > 0.001f)
                {
                    dist = intersectDist;
                    intersected = rt;
                }
            }

            return intersected;
        }
    }
}
